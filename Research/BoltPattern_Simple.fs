// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// Bolt Pattern Simple FeatureScript
// 
// Creates two steel tubes with a gap between them for testing bolt hole patterns.
// Bottom tube at origin, top tube translated up by two tube widths.

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");
// Import RivnutCreator: 
// IMPORTANT: RivnutCreator must be in a SEPARATE Feature Studio to use imports.
// Replace "db365b4948ca4490401db336" with the Tab ID (Element ID) from RivnutCreator's Feature Studio URL.
// To find it: Open RivnutCreator Feature Studio, look at the URL - the string after /e/ is the Tab ID.
// If both files are in the same Feature Studio, remove this import and call createRivnut directly.
// For same workspace imports, OnShape uses all zeros to represent current version
import(path : "db365b4948ca4490401db336", version : "000000000000000000000000");

annotation { "Feature Type Name" : "Bolt Pattern Simple" }
export const boltPatternSimple = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Tube Width" }
        isLength(definition.tubeWidth, { (inch) : [0.1, 1, 10] } as LengthBoundSpec);
        
        annotation { "Name" : "Tube Wall Thickness" }
        isLength(definition.tubeWallThickness, { (inch) : [0.01, 0.0625, 1] } as LengthBoundSpec);
        
        annotation { "Name" : "Tube Length" }
        isLength(definition.tubeLength, { (inch) : [1, 12, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "End Bolt Offset" }
        isLength(definition.endBoltOffset, { (inch) : [0.1, 1, 10] } as LengthBoundSpec);
        
        annotation { "Name" : "Bolt Diameter" }
        isLength(definition.boltDiameter, { (inch) : [0.1, 0.25, 2] } as LengthBoundSpec);
        
        annotation { "Name" : "Rivnut Body Diameter" }
        isLength(definition.rivnutBodyDiameter, { (inch) : [0.2, 0.390625, 1] } as LengthBoundSpec); // 25/64" = 0.390625"
        
        annotation { "Name" : "Debug Cuts" }
        definition.debugCuts is boolean;
    }
    {
        // Set explicit defaults
        if (definition.tubeWidth == undefined || definition.tubeWidth == 0 * inch)
        {
            definition.tubeWidth = 1 * inch;
        }
        
        if (definition.tubeWallThickness == undefined || definition.tubeWallThickness == 0 * inch)
        {
            definition.tubeWallThickness = 0.0625 * inch;
        }
        else if (definition.tubeWallThickness == 1 * inch)
        {
            definition.tubeWallThickness = 0.0625 * inch;
        }
        
        if (definition.tubeLength == undefined || definition.tubeLength == 0 * inch)
        {
            definition.tubeLength = 12 * inch;
        }
        else if (definition.tubeLength == 1 * inch)
        {
            definition.tubeLength = 12 * inch;
        }
        
        if (definition.endBoltOffset == undefined || definition.endBoltOffset == 0 * inch)
        {
            definition.endBoltOffset = 1 * inch;
        }
        else if (definition.endBoltOffset == 1 * inch)
        {
            // Keep 1 inch as default
        }
        
        if (definition.boltDiameter == undefined || definition.boltDiameter == 0 * inch)
        {
            definition.boltDiameter = 0.25 * inch;
        }
        else if (definition.boltDiameter == 1 * inch)
        {
            definition.boltDiameter = 0.25 * inch;
        }
        
        if (definition.rivnutBodyDiameter == undefined || definition.rivnutBodyDiameter == 0 * inch)
        {
            definition.rivnutBodyDiameter = 25/64 * inch; // Standard 1/4"-20 rivnut body diameter
        }
        else if (definition.rivnutBodyDiameter == 1 * inch)
        {
            definition.rivnutBodyDiameter = 25/64 * inch;
        }
        
        // Default debugCuts to false
        if (definition.debugCuts == undefined)
        {
            definition.debugCuts = false;
        }
        
        const innerWidth = definition.tubeWidth - 2 * definition.tubeWallThickness;
        const halfTube = definition.tubeWidth / 2;
        const halfInner = innerWidth / 2;
        const zero = 0 * inch;
        
        // Create bottom tube (horizontal along X axis at origin)
        const bottomTubeStart = vector(zero, zero, zero);
        const bottomTubeEnd = vector(definition.tubeLength, zero, zero);
        createTube(context, id + "bottom",
            bottomTubeStart,
            bottomTubeEnd,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Create top tube (duplicate translated up by two tube widths)
        const gap = 2 * definition.tubeWidth;
        const topTubeStart = vector(zero, zero, gap);
        const topTubeEnd = vector(definition.tubeLength, zero, gap);
        createTube(context, id + "top",
            topTubeStart,
            topTubeEnd,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Create bolt hole cylinder in top tube
        // The top tube's centerline is at Z = gap, so the top surface is at gap + halfTube
        const topTubeTopSurfaceZ = gap + halfTube; // Top surface of top tube
        const cylinderExtrudeDepth = definition.tubeWidth / 2; // Extrude down by half tube width
        const cylinderCenterX = definition.endBoltOffset; // Offset from end along tube
        const cylinderCenter = vector(cylinderCenterX, zero, topTubeTopSurfaceZ);
        
        // Create sketch plane at top surface with upward-pointing normal (not inverted)
        // We'll use oppositeDirection to flip the extrude direction
        const topFaceNormal = vector(0, 0, -1); // Normal points down
        // Create a plane at cylinderCenter using upward-pointing normal
        const cylinderSketchPlane = plane(cylinderCenter, topFaceNormal);
        const cylinderSketchId = id + "boltHoleSketch";
        // Create sketch on the plane at cylinderCenter
        const cylinderSketch = newSketchOnPlane(context, cylinderSketchId, {
            "sketchPlane" : cylinderSketchPlane
        });
        
        // Draw circle for bolt hole
        // In OnShape sketches, coordinates are 2D points but must have units
        const boltRadius = definition.boltDiameter / 2;
        const circleCenter2D = vector(0 * inch, 0 * inch); // 2D point with units (at sketch origin)
        skCircle(cylinderSketch, "boltCircle", {
            "center" : circleCenter2D,
            "radius" : boltRadius
        });
        skSolve(cylinderSketch);
        
        // Debug: Check the actual sketch plane normal
        const cylinderRegions = qSketchRegion(cylinderSketchId);
        const actualSketchPlane = evOwnerSketchPlane(context, {
            "entity" : cylinderRegions
        });
        
        // Debug logging
        println("DEBUG: topFaceNormal we set = " ~ topFaceNormal);
        println("DEBUG: cylinderSketchPlane.normal = " ~ cylinderSketchPlane.normal);
        println("DEBUG: actualSketchPlane.normal = " ~ actualSketchPlane.normal);
        println("DEBUG: actualSketchPlane.normal[0] = " ~ actualSketchPlane.normal[0]);
        println("DEBUG: actualSketchPlane.normal[1] = " ~ actualSketchPlane.normal[1]);
        println("DEBUG: actualSketchPlane.normal[2] = " ~ actualSketchPlane.normal[2]);
        
        // Calculate direction from two points (like we do in createTube)
        // This is the pattern that works in all our other scripts
        const extrudeStartPoint = cylinderCenter; // Start at sketch plane
        const extrudeEndPoint = cylinderCenter + vector(0 * inch, 0 * inch, -cylinderExtrudeDepth); // End below
        const extrudeDelta = extrudeEndPoint - extrudeStartPoint;
        const extrudeDirection = normalize(extrudeDelta); // Normalized direction vector
        println("DEBUG: extrudeStartPoint = " ~ extrudeStartPoint);
        println("DEBUG: extrudeEndPoint = " ~ extrudeEndPoint);
        println("DEBUG: extrudeDelta = " ~ extrudeDelta);
        println("DEBUG: extrudeDirection = " ~ extrudeDirection);
        println("DEBUG: extrudeDirection[2] = " ~ extrudeDirection[2]);
        opExtrude(context, id + "boltHoleCylinder", {
            "entities" : cylinderRegions,
            "direction" : extrudeDirection, // Calculated from points (should be downward)
            "endBound" : BoundingType.BLIND,
            "endDepth" : cylinderExtrudeDepth
        });
        
        // Boolean subtract cylinder from top tube only (unless debugCuts is enabled)
        if (!definition.debugCuts)
        {
            // Query for all solid bodies created by the top tube feature
            const topTubeBodies = qBodyType(qCreatedBy(id + "top", EntityType.BODY), BodyType.SOLID);
            opBoolean(context, id + "boltHoleSubtract", {
                "tools" : qBodyType(qCreatedBy(id + "boltHoleCylinder", EntityType.BODY), BodyType.SOLID),
                "operationType" : BooleanOperationType.SUBTRACTION,
                "targets" : topTubeBodies
            });
        }
        
        // Create rivnut hole in bottom tube (aligned with top tube hole)
        // Bottom tube's centerline is at Z=0, so top surface is at halfTube
        const bottomTubeTopSurfaceZ = halfTube; // Top surface of bottom tube
        const rivnutHoleDepth = definition.tubeWidth / 2; // Extrude down by half tube width
        const rivnutCenterX = definition.endBoltOffset; // Same X offset as top hole
        const rivnutCenter = vector(rivnutCenterX, zero, bottomTubeTopSurfaceZ);
        
        // Create sketch plane at bottom tube top surface
        const rivnutSketchPlane = plane(rivnutCenter, vector(0, 0, -1)); // Normal points down
        const rivnutSketchId = id + "rivnutHoleSketch";
        const rivnutSketch = newSketchOnPlane(context, rivnutSketchId, {
            "sketchPlane" : rivnutSketchPlane
        });
        
        // Draw circle for rivnut body hole
        const rivnutRadius = definition.rivnutBodyDiameter / 2;
        const rivnutCircleCenter2D = vector(0 * inch, 0 * inch); // 2D point at sketch origin
        skCircle(rivnutSketch, "rivnutCircle", {
            "center" : rivnutCircleCenter2D,
            "radius" : rivnutRadius
        });
        skSolve(rivnutSketch);
        
        // Calculate extrude direction from two points
        const rivnutRegions = qSketchRegion(rivnutSketchId);
        const rivnutExtrudeStartPoint = rivnutCenter;
        const rivnutExtrudeEndPoint = rivnutCenter + vector(0 * inch, 0 * inch, -rivnutHoleDepth);
        const rivnutExtrudeDelta = rivnutExtrudeEndPoint - rivnutExtrudeStartPoint;
        const rivnutExtrudeDirection = normalize(rivnutExtrudeDelta);
        
        opExtrude(context, id + "rivnutHoleCylinder", {
            "entities" : rivnutRegions,
            "direction" : rivnutExtrudeDirection,
            "endBound" : BoundingType.BLIND,
            "endDepth" : rivnutHoleDepth
        });
        
        // Boolean subtract rivnut hole from bottom tube only (unless debugCuts is enabled)
        if (!definition.debugCuts)
        {
            const bottomTubeBodies = qBodyType(qCreatedBy(id + "bottom", EntityType.BODY), BodyType.SOLID);
            opBoolean(context, id + "rivnutHoleSubtract", {
                "tools" : qBodyType(qCreatedBy(id + "rivnutHoleCylinder", EntityType.BODY), BodyType.SOLID),
                "operationType" : BooleanOperationType.SUBTRACTION,
                "targets" : bottomTubeBodies
            });
        }
        
        // Create rivnut primitive at the rivnut hole location (2nd hole)
        // Position is on the top surface of the bottom tube, normal points up
        // The rivnut will be placed 1/16" above the surface and extrude down into the hole
        const rivnutPosition = rivnutCenter; // Top surface of bottom tube at rivnut hole location
        const rivnutNormal = vector(0, 0, 1); // Surface normal pointing up
        createRivnut(context, id + "rivnut", rivnutPosition, rivnutNormal, definition.rivnutBodyDiameter, 1/16 * inch);
        
        // Create passthrough clearance hole in bottom face of top tube
        // This is the bottom half of a stepped through-hole (top half is the 1/4" bolt hole from the top)
        // The 0.75" clearance hole extrudes upward by halfTube to meet the bolt hole in the middle
        // Top tube's centerline is at Z=gap, so bottom surface is at gap - halfTube
        const topTubeBottomSurfaceZ = gap - halfTube; // Bottom surface of top tube
        const passthroughHoleDiameter = 0.75 * inch; // Clearance for rivnut flange (up to 0.625")
        const passthroughHoleDepth = halfTube; // Only half the tube width - meets the bolt hole in the middle
        const passthroughCenterX = definition.endBoltOffset; // Same X offset as other holes
        const passthroughCenter = vector(passthroughCenterX, zero, topTubeBottomSurfaceZ);
        
        // Create sketch plane at top tube bottom surface
        const passthroughSketchPlane = plane(passthroughCenter, vector(0, 0, 1)); // Normal points up
        const passthroughSketchId = id + "passthroughHoleSketch";
        const passthroughSketch = newSketchOnPlane(context, passthroughSketchId, {
            "sketchPlane" : passthroughSketchPlane
        });
        
        // Draw circle for passthrough clearance hole
        const passthroughRadius = passthroughHoleDiameter / 2;
        const passthroughCircleCenter2D = vector(0 * inch, 0 * inch); // 2D point at sketch origin
        skCircle(passthroughSketch, "passthroughCircle", {
            "center" : passthroughCircleCenter2D,
            "radius" : passthroughRadius
        });
        skSolve(passthroughSketch);
        
        // Calculate extrude direction from two points (upward into the tube)
        const passthroughRegions = qSketchRegion(passthroughSketchId);
        const passthroughExtrudeStartPoint = passthroughCenter;
        const passthroughExtrudeEndPoint = passthroughCenter + vector(0 * inch, 0 * inch, passthroughHoleDepth);
        const passthroughExtrudeDelta = passthroughExtrudeEndPoint - passthroughExtrudeStartPoint;
        const passthroughExtrudeDirection = normalize(passthroughExtrudeDelta);
        
        opExtrude(context, id + "passthroughHoleCylinder", {
            "entities" : passthroughRegions,
            "direction" : passthroughExtrudeDirection,
            "endBound" : BoundingType.BLIND,
            "endDepth" : passthroughHoleDepth
        });
        
        // Boolean subtract passthrough hole from top tube only (unless debugCuts is enabled)
        if (!definition.debugCuts)
        {
            const topTubeBodiesForPassthrough = qBodyType(qCreatedBy(id + "top", EntityType.BODY), BodyType.SOLID);
            opBoolean(context, id + "passthroughHoleSubtract", {
                "tools" : qBodyType(qCreatedBy(id + "passthroughHoleCylinder", EntityType.BODY), BodyType.SOLID),
                "operationType" : BooleanOperationType.SUBTRACTION,
                "targets" : topTubeBodiesForPassthrough
            });
        }
        
        // Clean up sketches
        try
        {
            opDeleteBodies(context, id + "deleteBoltSketch", {
                "entities" : qCreatedBy(cylinderSketchId, EntityType.BODY)
            });
        }
        catch
        {
            // Sketch may not be deletable - this is okay
        }
        
        try
        {
            opDeleteBodies(context, id + "deleteRivnutSketch", {
                "entities" : qCreatedBy(rivnutSketchId, EntityType.BODY)
            });
        }
        catch
        {
            // Sketch may not be deletable - this is okay
        }
        
        try
        {
            opDeleteBodies(context, id + "deletePassthroughSketch", {
                "entities" : qCreatedBy(passthroughSketchId, EntityType.BODY)
            });
        }
        catch
        {
            // Sketch may not be deletable - this is okay
        }
    }, {
        editingLogic : function(context is Context, definition is map) returns map
        {
            // Override 1 inch values (OnShape's fallback) with correct defaults when editing
            if (definition.tubeWallThickness == 1 * inch)
            {
                definition.tubeWallThickness = 0.0625 * inch;
            }
            
            if (definition.tubeLength == 1 * inch)
            {
                definition.tubeLength = 12 * inch;
            }
            
            if (definition.boltDiameter == 1 * inch)
            {
                definition.boltDiameter = 0.25 * inch;
            }
            
            if (definition.rivnutBodyDiameter == 1 * inch)
            {
                definition.rivnutBodyDiameter = 25/64 * inch;
            }
            
            return definition;
        }
    });

// Helper function to create a single tube
function createTube(context is Context, id is Id, startPoint is Vector, endPoint is Vector,
    halfTube is ValueWithUnits, halfInner is ValueWithUnits, tubeWidth is ValueWithUnits, wallThickness is ValueWithUnits)
{
    const delta = endPoint - startPoint;
    const direction = normalize(delta);
    const length = norm(delta);
    
    // Determine sketch plane based on direction
    // For axis-aligned directions, use world coordinate planes
    // Normalized direction components are unitless, so we can compare directly
    var sketchPlane;
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
}

