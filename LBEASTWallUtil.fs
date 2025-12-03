// LBEAST Wall Utility Functions
// Core reusable helpers extracted from LBEASTWallFrameCreator.fs

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");

// Helper to normalize a facing direction angle to 0-360 degrees
export function normalizeFacingDirection(angle is ValueWithUnits) returns ValueWithUnits
{
    var angleDegrees = angle / degree;
    angleDegrees = angleDegrees - floor(angleDegrees / 360) * 360;
    if (angleDegrees < 0)
    {
        angleDegrees += 360;
    }
    return angleDegrees * degree;
}

// Helper function to query all bodies created by a base id
// Works for both corner and center wall segments
// Center segments may have additional bodies from duplication operations
export function queryAllBodies(id is Id) returns Query
{
    // Base bodies exist for both corner and center segments
    var bodyQueries = [
        qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID)
    ];
    
    // Center span copy only exists for center wall segments
    // Query will return empty if it doesn't exist, which is fine
    bodyQueries = append(bodyQueries, qBodyType(qCreatedBy(id + "centerSpanCopy", EntityType.BODY), BodyType.SOLID));
    
    // Duplicate tubes only exist for center wall segments
    bodyQueries = append(bodyQueries, qBodyType(qCreatedBy(id + "duplicateRectTube", EntityType.BODY), BodyType.SOLID));
    bodyQueries = append(bodyQueries, qBodyType(qCreatedBy(id + "duplicateSecondRectTube", EntityType.BODY), BodyType.SOLID));
    
    return qUnion(bodyQueries);
}

// Helper function to create a single hollow square tube between two points
export function createTube(context is Context, id is Id, startPoint is Vector, endPoint is Vector,
    halfTube is ValueWithUnits, halfInner is ValueWithUnits, tubeWidth is ValueWithUnits, wallThickness is ValueWithUnits)
{
    const delta = endPoint - startPoint;
    const direction = normalize(delta);
    const length = norm(delta);
    
    // Determine sketch plane based on direction
    // For axis-aligned directions, use world coordinate planes
    // Normalized direction components are unitless, so we can compare directly
    var sketchPlane = plane(startPoint, vector(0, 0, 1)); // Default to XY plane
    const absX = abs(direction[0]);
    const absY = abs(direction[1]);
    const absZ = abs(direction[2]);
    
    if (absX > absY && absX > absZ)
    {
        // Direction is mostly along X axis, sketch in YZ plane (normal = X)
        sketchPlane = plane(startPoint, vector(1, 0, 0));
    }
    else if (absY > absZ)
    {
        // Direction is mostly along Y axis, sketch in XZ plane (normal = Y)
        sketchPlane = plane(startPoint, vector(0, 1, 0));
    }
    else
    {
        // Direction is mostly along Z axis, sketch in XY plane (normal = Z)
        sketchPlane = plane(startPoint, vector(0, 0, 1));
    }
    
    // Create outer rectangle sketch
    const outerSketchId = id + "outerSketch";
    const outerSketch = newSketchOnPlane(context, outerSketchId, {
        "sketchPlane" : sketchPlane
    });
    skRectangle(outerSketch, "outerRect", {
        "firstCorner" : vector(-halfTube, -halfTube),
        "secondCorner" : vector(halfTube, halfTube)
    });
    skSolve(outerSketch);
    
    // Extrude outer rectangle
    const outerRegions = qSketchRegion(outerSketchId);
    opExtrude(context, id + "outer", {
        "entities" : outerRegions,
        "direction" : direction,
        "endBound" : BoundingType.BLIND,
        "endDepth" : length
    });
    
    // Create inner rectangle sketch
    const innerSketchId = id + "innerSketch";
    const innerSketch = newSketchOnPlane(context, innerSketchId, {
        "sketchPlane" : sketchPlane
    });
    skRectangle(innerSketch, "innerRect", {
        "firstCorner" : vector(-halfInner, -halfInner),
        "secondCorner" : vector(halfInner, halfInner)
    });
    skSolve(innerSketch);
    
    // Extrude inner rectangle
    const innerRegions = qSketchRegion(innerSketchId);
    opExtrude(context, id + "inner", {
        "entities" : innerRegions,
        "direction" : direction,
        "endBound" : BoundingType.BLIND,
        "endDepth" : length
    });
    
    // Subtract inner from outer to create hollow tube
    opBoolean(context, id + "subtract", {
        "tools" : qCreatedBy(id + "inner", EntityType.BODY),
        "operationType" : BooleanOperationType.SUBTRACTION,
        "targets" : qCreatedBy(id + "outer", EntityType.BODY)
    });

    // Attempt to delete sketch bodies so they don't clutter the model
    // Note: The sketch *features* remain in history; this only removes sketch-created bodies if any
    try
    {
        opDeleteBodies(context, id + "deleteOuterSketchBodies", {
            "entities" : qCreatedBy(outerSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // It's okay if there are no sketch bodies to delete
    }

    try
    {
        opDeleteBodies(context, id + "deleteInnerSketchBodies", {
            "entities" : qCreatedBy(innerSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // It's okay if there are no sketch bodies to delete
    }
}

// -------------------------
// Rotation group builder
// -------------------------
//
// Builder pattern for configuring rotation of a group of bodies around a given axis,
// using a normalized control parameter (e.g. rotationNormalized in [0,2]).
//
// Usage example (conceptual):
//
//   var groupConfig = newRotationGroupBuilder("First");
//   groupConfig = withNormalizedRange(groupConfig, 0, 0.5);
//   groupConfig = withMaxAngle(groupConfig, 180 * degree);
//   groupConfig = withClockwise(groupConfig, true);
//   groupConfig = withBodyIndices(groupConfig, [0, 1, 2, 3]);
//   groupConfig = withAxisLine(groupConfig, someAxisLine);
//   applyRotationGroupBuilder(context, id, allBodiesArray, rotationNormalized, groupConfig);
//
// This allows all rotation groups (1–4) to share the same core rotation logic.

export function newRotationGroupBuilder(groupName is string) returns map
{
    return {
        "groupName" : groupName,
        "startNorm" : 0.0,
        "endNorm" : 1.0,
        "maxAngle" : 0 * degree,
        "clockwise" : true,
        "bodyIndices" : [],
        "axisLine" : undefined,
        "opSuffix" : groupName
    };
}

export function withNormalizedRange(config is map, startNorm is number, endNorm is number) returns map
{
    var c = config;
    c["startNorm"] = startNorm;
    c["endNorm"] = endNorm;
    return c;
}

export function withMaxAngle(config is map, maxAngle is ValueWithUnits) returns map
{
    var c = config;
    c["maxAngle"] = maxAngle;
    return c;
}

export function withClockwise(config is map, clockwise is boolean) returns map
{
    var c = config;
    c["clockwise"] = clockwise;
    return c;
}

export function withBodyIndices(config is map, indices is array) returns map
{
    var c = config;
    c["bodyIndices"] = indices;
    return c;
}

export function withAxisLine(config is map, axisLine is Line) returns map
{
    var c = config;
    c["axisLine"] = axisLine;
    return c;
}

export function withOpSuffix(config is map, opSuffix is string) returns map
{
    var c = config;
    c["opSuffix"] = opSuffix;
    return c;
}

// Apply a configured rotation group to a set of bodies.
// - rotationNormalized: global control value (e.g. 0–2)
// - config: built via the builder helpers above
export function applyRotationGroupBuilder(context is Context, id is Id,
    allBodiesArray is array, rotationNormalized is number, config is map)
{
    if (config.axisLine == undefined)
    {
        println("ERROR: RotationGroupBuilder axisLine is undefined for group " ~ config.groupName);
        return;
    }
    if (size(config.bodyIndices) == 0)
    {
        println("ERROR: RotationGroupBuilder bodyIndices is empty for group " ~ config.groupName);
        return;
    }

    // Clamp normalized value into this group's active range
    var startNorm = config.startNorm;
    var endNorm = config.endNorm;
    if (endNorm <= startNorm)
    {
        println("ERROR: RotationGroupBuilder endNorm must be > startNorm for group " ~ config.groupName);
        return;
    }

    var clampedNorm = rotationNormalized;
    if (clampedNorm < startNorm)
    {
        clampedNorm = startNorm;
    }
    else if (clampedNorm > endNorm)
    {
        clampedNorm = endNorm;
    }

    // Map clampedNorm from [startNorm, endNorm] -> [0,1]
    const localNorm = (clampedNorm - startNorm) / (endNorm - startNorm);

    // Compute angle from localNorm and maxAngle
    var angle = localNorm * config.maxAngle;
    if (!config.clockwise)
    {
        angle = -angle;
    }

    const rotationTransform = rotationAround(config.axisLine, angle);

    // Build body query list from indices
    var bodiesToRotateList = [];
    for (var i = 0; i < size(config.bodyIndices); i += 1)
    {
        const idx = config.bodyIndices[i];
        if (idx >= 0 && idx < size(allBodiesArray))
        {
            bodiesToRotateList = append(bodiesToRotateList,
                qBodyType(allBodiesArray[idx], BodyType.SOLID));
        }
    }

    if (size(bodiesToRotateList) == 0)
    {
        println("WARNING: RotationGroupBuilder found no valid bodies to rotate for group " ~ config.groupName);
        return;
    }

    const opId = config.opSuffix == undefined ? "rotateGroup" : "rotateGroup" ~ config.opSuffix;

    opTransform(context, id + opId, {
        "bodies" : qUnion(bodiesToRotateList),
        "transform" : rotationTransform
    });
}

// -------------------------
// Rotation axis finding
// -------------------------

// Helper function to find rotation axis from a body's top face inner long edge
// Returns a map with "axisLine" (Line), "found" (boolean), and "edge" (Query)
// useLargerY can be undefined to use X-based selection instead of Y-based
export function findRotationAxisFromBody(context is Context, body is Query, bodyIndex is number, useLargerX is boolean, useLargerY is boolean) returns map
{
    // Find the top face of the body
    const allFaces = qOwnedByBody(qBodyType(qEntityFilter(body, EntityType.BODY), BodyType.SOLID), EntityType.FACE);
    const faceArray = evaluateQuery(context, allFaces);
    
    const upVector = vector(0, 0, 1);
    var topFace;
    var maxDot = -1;
    for (var n = 0; n < size(faceArray); n += 1)
    {
        const face = faceArray[n];
        const facePlane = evPlane(context, {
            "face" : face
        });
        const faceNormal = facePlane.normal;
        const dotProduct = dot(faceNormal, upVector);
        
        if (dotProduct > maxDot)
        {
            maxDot = dotProduct;
            topFace = face;
        }
    }
    
    if (topFace == undefined)
    {
        println("ERROR: Could not find top face of body at index " ~ bodyIndex);
        return { "axisLine" : undefined, "found" : false, "edge" : undefined };
    }
    
    // Get all edges of the body, then filter to find edges on the top face
    const allBodyEdges = qOwnedByBody(qBodyType(qEntityFilter(body, EntityType.BODY), BodyType.SOLID), EntityType.EDGE);
    const allEdgeArray = evaluateQuery(context, allBodyEdges);
    
    // Get the top face plane to check which edges are on it
    const topFacePlane = evPlane(context, {
        "face" : topFace
    });
    
    // Filter edges that are on the top face (check if edge midpoint is on the face plane)
    var topFaceEdges = [];
    for (var e = 0; e < size(allEdgeArray); e += 1)
    {
        const edge = allEdgeArray[e];
        const edgeBox = evBox3d(context, {
            "topology" : edge
        });
        const edgeMidpoint = (edgeBox.minCorner + edgeBox.maxCorner) / 2;
        
        // Check if edge midpoint is on the top face plane (within tolerance)
        const distanceToPlane = abs(dot(topFacePlane.normal, edgeMidpoint - topFacePlane.origin));
        if (distanceToPlane < 1e-6 * meter)
        {
            topFaceEdges = append(topFaceEdges, edge);
        }
    }
    
    if (size(topFaceEdges) < 2)
    {
        println("ERROR: Could not find at least 2 edges on top face of body at index " ~ bodyIndex);
        return { "axisLine" : undefined, "found" : false, "edge" : undefined };
    }
    
    // Find the longest edges (there should be 2 long edges and 2 short edges)
    var edgesWithLength = [];
    for (var e = 0; e < size(topFaceEdges); e += 1)
    {
        const edge = topFaceEdges[e];
        const edgeBox = evBox3d(context, {
            "topology" : edge
        });
        // Calculate edge length from bounding box dimensions
        const edgeVector = edgeBox.maxCorner - edgeBox.minCorner;
        const edgeLength = norm(edgeVector);
        edgesWithLength = append(edgesWithLength, {
            "edge" : edge,
            "length" : edgeLength
        });
    }
    
    // Sort by length (descending - longest first)
    edgesWithLength = sort(edgesWithLength, function(a, b) { return b.length.value - a.length.value; });
    
    // Debug: print edge lengths
    println("DEBUG body " ~ bodyIndex ~ ": Found " ~ size(edgesWithLength) ~ " edges on top face");
    for (var d = 0; d < size(edgesWithLength) && d < 4; d += 1)
    {
        println("  Edge " ~ d ~ " length: " ~ edgesWithLength[d].length);
    }
    
    // The two longest edges are the long edges (broad edges, not stubby)
    const longestEdge = edgesWithLength[0].edge;
    const secondLongestEdge = edgesWithLength[1].edge;
    
    // Verify these are actually the long edges (should be much longer than edges 2 and 3 if they exist)
    if (size(edgesWithLength) >= 4)
    {
        const thirdEdgeLength = edgesWithLength[2].length;
        const fourthEdgeLength = edgesWithLength[3].length;
        const avgShortEdgeLength = (thirdEdgeLength + fourthEdgeLength) / 2;
        const avgLongEdgeLength = (edgesWithLength[0].length + edgesWithLength[1].length) / 2;
        println("DEBUG body " ~ bodyIndex ~ ": Long edges avg length: " ~ avgLongEdgeLength ~ ", Short edges avg length: " ~ avgShortEdgeLength);
        
        // If the "longest" edges aren't significantly longer, we might have the wrong edges
        if (avgLongEdgeLength < avgShortEdgeLength * 1.5)
        {
            println("WARNING body " ~ bodyIndex ~ ": Long edges don't seem much longer than short edges - might be selecting wrong edges!");
        }
    }
    
    // Get the center points of both long edges to determine which is "inner"
    const longestEdgeBox = evBox3d(context, {
        "topology" : longestEdge
    });
    const secondLongestEdgeBox = evBox3d(context, {
        "topology" : secondLongestEdge
    });
    
    const longestEdgeCenterX = (longestEdgeBox.minCorner[0] + longestEdgeBox.maxCorner[0]) / 2;
    const secondLongestEdgeCenterX = (secondLongestEdgeBox.minCorner[0] + secondLongestEdgeBox.maxCorner[0]) / 2;
    const longestEdgeCenterY = (longestEdgeBox.minCorner[1] + longestEdgeBox.maxCorner[1]) / 2;
    const secondLongestEdgeCenterY = (secondLongestEdgeBox.minCorner[1] + secondLongestEdgeBox.maxCorner[1]) / 2;
    
    println("DEBUG body " ~ bodyIndex ~ ": Longest edge center Y=" ~ longestEdgeCenterY ~ ", Second longest edge center Y=" ~ secondLongestEdgeCenterY);
    println("DEBUG body " ~ bodyIndex ~ ": useLargerX=" ~ useLargerX ~ ", useLargerY=" ~ useLargerY);
    
    // Select "inner" edge based on useLargerX or useLargerY parameter
    // There are 4 edges on top of the flat bar: 2 long (broad) and 2 stubby
    // We want one of the 2 long edges - which one depends on the group:
    // useLargerX: must be true to ensure we get a broad edge (not stubby)
    // useLargerY: selects which of the two long edges:
    //   - useLargerY=true: select edge with larger Y (inner edge for groups 1&2)
    //   - useLargerY=false: select edge with smaller Y (inner edge for groups 3&4)
    var innerLongEdge;
    if (useLargerY && useLargerX)
    {
        // Groups 1&2: select the OTHER long edge (invert the comparison to get the opposite of groups 3&4)
        // Groups 3&4 use: longestEdgeCenterY < secondLongestEdgeCenterY ? longestEdge : secondLongestEdge
        // So groups 1&2 need the opposite: if that would select longestEdge, we want secondLongestEdge, and vice versa
        innerLongEdge = longestEdgeCenterY < secondLongestEdgeCenterY ? secondLongestEdge : longestEdge;
        println("DEBUG body " ~ bodyIndex ~ ": Selected edge (inverted selection for groups 1&2 inner edge)");
    }
    else if (!useLargerY && useLargerX)
    {
        // Groups 3&4: select edge with smaller Y (inner edge for groups 3&4)
        innerLongEdge = longestEdgeCenterY < secondLongestEdgeCenterY ? longestEdge : secondLongestEdge;
        println("DEBUG body " ~ bodyIndex ~ ": Selected edge with smaller Y (inner edge for groups 3&4)");
    }
    else if (useLargerX)
    {
        // The "inner" edge is the one with the LARGER X coordinate
        innerLongEdge = longestEdgeCenterX > secondLongestEdgeCenterX ? longestEdge : secondLongestEdge;
        println("DEBUG body " ~ bodyIndex ~ ": Selected edge with larger X");
    }
    else
    {
        // Groups 1&2: The "inner" edge is the one with the SMALLER X coordinate (the OTHER long edge)
        innerLongEdge = longestEdgeCenterX < secondLongestEdgeCenterX ? longestEdge : secondLongestEdge;
        println("DEBUG body " ~ bodyIndex ~ ": Selected edge with smaller X (inner edge for groups 1&2)");
    }
    
    // Verify the selected edge is actually long (broad edge, not stubby)
    const selectedEdgeBox = evBox3d(context, {
        "topology" : innerLongEdge
    });
    const selectedEdgeVector = selectedEdgeBox.maxCorner - selectedEdgeBox.minCorner;
    const selectedEdgeLength = norm(selectedEdgeVector);
    println("DEBUG body " ~ bodyIndex ~ ": Selected edge length: " ~ selectedEdgeLength);
    
    // Get the rotation axis from the edge
    // Get the actual vertices of the edge to construct an accurate axis line
    const edgeVertices = evaluateQuery(context, qAdjacent(innerLongEdge, AdjacencyType.VERTEX, EntityType.VERTEX));
    
    var axisLine;
    if (size(edgeVertices) >= 2)
    {
        // Get the actual vertex points (these are the real endpoints of the edge)
        const vertex0Point = evVertexPoint(context, {
            "vertex" : edgeVertices[0]
        });
        const vertex1Point = evVertexPoint(context, {
            "vertex" : edgeVertices[1]
        });
        
        // Create the axis line from the actual edge endpoints
        const edgeDirection = normalize(vertex1Point - vertex0Point);
        axisLine = line(vertex0Point, edgeDirection);
    }
    else
    {
        // Fallback to bounding box method if we can't get vertices
        const innerLongEdgeBox = evBox3d(context, {
            "topology" : innerLongEdge
        });
        const edgeStart = innerLongEdgeBox.minCorner;
        const edgeEnd = innerLongEdgeBox.maxCorner;
        const edgeDirection = normalize(edgeEnd - edgeStart);
        axisLine = line(edgeStart, edgeDirection);
        println("WARNING: Could not get edge vertices for body " ~ bodyIndex ~ ", using bounding box fallback");
    }
    
    return { "axisLine" : axisLine, "found" : true, "edge" : innerLongEdge };
}

// -------------------------
// Manipulator change handlers
// -------------------------

// Shared manipulator change logic for rotation manipulators
// Handles conversion from manipulator offset to normalized rotation value (0-2 range)
export function handleRotationManipulatorChange(context is Context, definition is map, newManipulators is map) returns map
{
    // Get the new offset from the manipulator
    if (newManipulators["rotationManipulator"] == undefined)
    {
        println("ERROR: rotationManipulator not found in newManipulators");
        return definition;
    }
    
    // Get the manipulator range (we'll calculate it the same way)
    const tubeWidth = definition.tubeWidth == undefined || definition.tubeWidth == 1 * inch ? 1 * inch : definition.tubeWidth;
    const manipulatorRange = tubeWidth * 10; // Match the range - 10x for 0-2 normalized (5x per unit)
    
    // Get the raw offset from the manipulator
    var newOffset = newManipulators["rotationManipulator"].offset;
    println("Raw manipulator offset: " ~ toString(newOffset));
    println("Manipulator range: " ~ toString(manipulatorRange));
    
    // Handle offset that exceeds the range - if it's at or beyond max, keep it at 2.0
    // If it's below 0, keep it at 0.0
    var normalizedValue;
    if (newOffset >= manipulatorRange)
    {
        // At or beyond maximum - set to 2.0
        normalizedValue = 2.0;
        println("Offset at or beyond maximum, setting normalized to 2.0");
    }
    else if (newOffset <= 0 * inch)
    {
        // At or below minimum - set to 0.0
        normalizedValue = 0.0;
        println("Offset at or below minimum, setting normalized to 0.0");
    }
    else
    {
        // Within range - map normally (0 to manipulatorRange maps to 0 to 2)
        normalizedValue = (newOffset / manipulatorRange) * 2;
        println("Normalized value: " ~ toString(normalizedValue));
    }
    
    // Ensure the value is in valid range (should already be, but double-check)
    definition.rotationNormalized = clamp(normalizedValue, 0, 2);
    println("Updated rotationNormalized: " ~ toString(definition.rotationNormalized));
    
    return definition;
}

