// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// Bolt and Capture Washer FeatureScript
// 

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");

/**
 * Creates a hex bolt primitive at a specified position and orientation.
 * 
 * @param context {Context} - FeatureScript context
 * @param id {Id} - Base ID for all features created by this function
 * @param position {Vector} - Position where bolt should be placed (typically on surface)
 * @param normal {Vector} - Surface normal (bolt will extrude in opposite direction)
 * @param shankLength {ValueWithUnits} - Optional, defaults to 1" (shank length, excluding head - hardware store standard)
 * @param headHeight {ValueWithUnits} - Optional, defaults to 3/16" (standard for 1/4-20)
 * @param headWidth {ValueWithUnits} - Optional, defaults to 7/16" across flats (standard for 1/4-20)
 * @param shankDiameter {ValueWithUnits} - Optional, defaults to 1/4" (0.25")
 * 
 * @returns {Query} - Query for the created bolt body
 */
export function createBolt(context is Context, id is Id, position is Vector, normal is Vector, shankLength is ValueWithUnits, headHeight is ValueWithUnits, headWidth is ValueWithUnits, shankDiameter is ValueWithUnits) returns Query
{
    // Set defaults if not provided
    if (shankLength == undefined || shankLength == 0 * inch)
    {
        shankLength = 1 * inch; // Default 1" shank length (hardware store standard)
    }
    
    if (headHeight == undefined || headHeight == 0 * inch)
    {
        headHeight = 3/16 * inch; // Standard 1/4-20 hex head height
    }
    
    if (headWidth == undefined || headWidth == 0 * inch)
    {
        headWidth = 7/16 * inch; // Standard 1/4-20 hex head width (across flats)
    }
    
    if (shankDiameter == undefined || shankDiameter == 0 * inch)
    {
        shankDiameter = 0.25 * inch; // 1/4" shank diameter
    }
    
    // Normalize the normal vector
    const normalizedNormal = normalize(normal);
    
    // Start point is at the position (bolt head sits on surface)
    const startPoint = position;
    
    // Create sketch plane at start point with normal pointing opposite to surface normal (down)
    // The bolt extrudes in the opposite direction of the surface normal
    const downDirection = -normalizedNormal;
    const boltSketchPlane = plane(startPoint, downDirection);
    
    // Create hex head sketch
    const headSketchId = id + "headSketch";
    const headSketch = newSketchOnPlane(context, headSketchId, {
        "sketchPlane" : boltSketchPlane
    });
    
    // Draw hexagon for head (6-sided polygon, headWidth is across flats)
    // For a hexagon, the radius from center to flat is headWidth/2
    // The radius from center to point is (headWidth/2) / cos(30°)
    const hexRadius = (headWidth / 2) / cos(30 * degree);
    skRegularPolygon(headSketch, "hexHead", {
        "center" : vector(0 * inch, 0 * inch),
        "firstVertex" : vector(hexRadius, 0 * inch),
        "sides" : 6
    });
    skSolve(headSketch);
    
    const headRegions = qSketchRegion(headSketchId);
    const headExtrudeStart = startPoint;
    const headExtrudeEnd = startPoint + (downDirection * headHeight);
    const headExtrudeDirection = normalize(headExtrudeEnd - headExtrudeStart);
    
    const headExtrudeId = id + "head";
    opExtrude(context, headExtrudeId, {
        "entities" : headRegions,
        "direction" : headExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : headHeight,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Query head body
    const headBody = qBodyType(qCreatedBy(headExtrudeId, EntityType.BODY), BodyType.SOLID);
    
    // Create shank sketch at the bottom of the head
    const shankStartPoint = startPoint + (downDirection * headHeight);
    const shankSketchId = id + "shankSketch";
    const shankSketch = newSketchOnPlane(context, shankSketchId, {
        "sketchPlane" : plane(shankStartPoint, downDirection)
    });
    
    const shankRadius = shankDiameter / 2;
    skCircle(shankSketch, "shankCircle", {
        "center" : vector(0 * inch, 0 * inch),
        "radius" : shankRadius
    });
    skSolve(shankSketch);
    
    const shankRegions = qSketchRegion(shankSketchId);
    const shankExtrudeStart = shankStartPoint;
    const shankExtrudeEnd = shankStartPoint + (downDirection * shankLength);
    const shankExtrudeDirection = normalize(shankExtrudeEnd - shankExtrudeStart);
    
    const shankExtrudeId = id + "shank";
    opExtrude(context, shankExtrudeId, {
        "entities" : shankRegions,
        "direction" : shankExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : shankLength,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Query shank body
    const shankBody = qBodyType(qCreatedBy(shankExtrudeId, EntityType.BODY), BodyType.SOLID);
    
    // Union using qUnion in tools parameter
    opBoolean(context, id + "join", {
        "tools" : qUnion([headBody, shankBody]),
        "operationType" : BooleanOperationType.UNION
    });
    
    // After union, query the merged bolt body
    const boltBody = qBodyType(qCreatedBy(headExtrudeId, EntityType.BODY), BodyType.SOLID);
    
    // Clean up sketches after bolt is complete
    try
    {
        opDeleteBodies(context, id + "deleteHeadSketch", {
            "entities" : qCreatedBy(headSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    try
    {
        opDeleteBodies(context, id + "deleteShankSketch", {
            "entities" : qCreatedBy(shankSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    // Return the final bolt body
    return boltBody;
}

/**
 * Creates a hex nut primitive at a specified position and orientation.
 * 
 * @param context {Context} - FeatureScript context
 * @param id {Id} - Base ID for all features created by this function
 * @param position {Vector} - Position where nut should be placed (typically on surface)
 * @param normal {Vector} - Surface normal (nut will extrude in opposite direction)
 * @param nutHeight {ValueWithUnits} - Optional, defaults to 3/16" (standard for 1/4-20)
 * @param nutWidth {ValueWithUnits} - Optional, defaults to 7/16" across flats (standard for 1/4-20)
 * @param boreDiameter {ValueWithUnits} - Optional, defaults to 1/4" (0.25")
 * 
 * @returns {Query} - Query for the created nut body
 */
export function createNut(context is Context, id is Id, position is Vector, normal is Vector, nutHeight is ValueWithUnits, nutWidth is ValueWithUnits, boreDiameter is ValueWithUnits) returns Query
{
    // Set defaults if not provided
    if (nutHeight == undefined || nutHeight == 0 * inch)
    {
        nutHeight = 3/16 * inch; // Standard 1/4-20 hex nut height
    }
    
    if (nutWidth == undefined || nutWidth == 0 * inch)
    {
        nutWidth = 7/16 * inch; // Standard 1/4-20 hex nut width (across flats)
    }
    
    if (boreDiameter == undefined || boreDiameter == 0 * inch)
    {
        boreDiameter = 0.25 * inch; // 1/4" bore diameter
    }
    
    // Normalize the normal vector
    const normalizedNormal = normalize(normal);
    
    // Start point is at the position (nut sits on surface)
    const startPoint = position;
    
    // Create sketch plane at start point with normal pointing opposite to surface normal (down)
    const downDirection = -normalizedNormal;
    const nutSketchPlane = plane(startPoint, downDirection);
    
    // Create hex nut sketch (solid hexagon only, no hole yet)
    const nutSketchId = id + "nutSketch";
    const nutSketch = newSketchOnPlane(context, nutSketchId, {
        "sketchPlane" : nutSketchPlane
    });
    
    // Draw hexagon for nut (6-sided polygon, nutWidth is across flats)
    // For a hexagon, the radius from center to flat is nutWidth/2
    // The radius from center to point is (nutWidth/2) / cos(30°)
    const hexRadius = (nutWidth / 2) / cos(30 * degree);
    skRegularPolygon(nutSketch, "hexNut", {
        "center" : vector(0 * inch, 0 * inch),
        "firstVertex" : vector(hexRadius, 0 * inch),
        "sides" : 6
    });
    
    skSolve(nutSketch);
    
    const nutRegions = qSketchRegion(nutSketchId);
    const nutExtrudeStart = startPoint;
    const nutExtrudeEnd = startPoint + (downDirection * nutHeight);
    const nutExtrudeDirection = normalize(nutExtrudeEnd - nutExtrudeStart);
    
    const nutExtrudeId = id + "nut";
    opExtrude(context, nutExtrudeId, {
        "entities" : nutRegions,
        "direction" : nutExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : nutHeight,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Query nut body
    const nutBody = qBodyType(qCreatedBy(nutExtrudeId, EntityType.BODY), BodyType.SOLID);
    
    // Now create and subtract the center bore hole using boolean operations
    const boreHoleSketchId = id + "boreHoleSketch";
    const boreHoleSketch = newSketchOnPlane(context, boreHoleSketchId, {
        "sketchPlane" : nutSketchPlane
    });
    
    skCircle(boreHoleSketch, "boreCircle", {
        "center" : vector(0 * inch, 0 * inch),
        "radius" : boreDiameter / 2
    });
    skSolve(boreHoleSketch);
    
    const boreHoleRegions = qSketchRegion(boreHoleSketchId);
    opExtrude(context, id + "boreHole", {
        "entities" : boreHoleRegions,
        "direction" : nutExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : nutHeight * 2, // Make sure it goes all the way through
        "operationType" : NewBodyOperationType.NEW
    });
    
    const boreHoleBody = qBodyType(qCreatedBy(id + "boreHole", EntityType.BODY), BodyType.SOLID);
    opBoolean(context, id + "boreHoleSubtract", {
        "tools" : boreHoleBody,
        "operationType" : BooleanOperationType.SUBTRACTION,
        "targets" : nutBody
    });
    
    // Clean up sketches after nut is complete
    try
    {
        opDeleteBodies(context, id + "deleteNutSketch", {
            "entities" : qCreatedBy(nutSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    try
    {
        opDeleteBodies(context, id + "deleteBoreHoleSketch", {
            "entities" : qCreatedBy(boreHoleSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    // Return the final nut body
    return nutBody;
}

/**
 * Creates a capture washer system (outer and inner washers) at a specified position and orientation.
 * 
 * @param context {Context} - FeatureScript context
 * @param id {Id} - Base ID for all features created by this function
 * @param position {Vector} - Position where washer should be placed (typically on surface)
 * @param normal {Vector} - Surface normal (washer will extrude in opposite direction)
 * @param outerWasherDiameter {ValueWithUnits} - Optional, defaults to 1" (outer washer diameter)
 * @param outerWasherThickness {ValueWithUnits} - Optional, defaults to 1/16" (outer washer thickness)
 * @param innerWasherDiameter {ValueWithUnits} - Optional, defaults to 0.75" (inner washer diameter)
 * @param innerWasherThickness {ValueWithUnits} - Optional, defaults to 1/16" (inner washer thickness)
 * @param boltHoleDiameter {ValueWithUnits} - Optional, defaults to 1/4" (center bolt hole diameter)
 * @param plugWeldHoleDiameter {ValueWithUnits} - Optional, defaults to 1/8" (flanking plug-weld holes)
 * @param holeSpacing {ValueWithUnits} - Optional, defaults to 5/16" (distance from center hole to plug-weld holes, away from center)
 * 
 * @returns {Query} - Query for both washer bodies (outer and inner washers remain separate for visualization)
 */
export function createCaptureWasher(context is Context, id is Id, position is Vector, normal is Vector, outerWasherDiameter is ValueWithUnits, outerWasherThickness is ValueWithUnits, innerWasherDiameter is ValueWithUnits, innerWasherThickness is ValueWithUnits, boltHoleDiameter is ValueWithUnits, plugWeldHoleDiameter is ValueWithUnits, holeSpacing is ValueWithUnits) returns Query
{
    // Set defaults if not provided
    if (outerWasherDiameter == undefined || outerWasherDiameter == 0 * inch)
    {
        outerWasherDiameter = 1 * inch; // Default 1" outer washer
    }
    
    if (outerWasherThickness == undefined || outerWasherThickness == 0 * inch)
    {
        outerWasherThickness = 1/16 * inch; // Default 1/16" thickness
    }
    
    if (innerWasherDiameter == undefined || innerWasherDiameter == 0 * inch)
    {
        innerWasherDiameter = 0.75 * inch; // Default 0.75" inner washer
    }
    
    if (innerWasherThickness == undefined || innerWasherThickness == 0 * inch)
    {
        innerWasherThickness = 1/16 * inch; // Default 1/16" thickness
    }
    
    if (boltHoleDiameter == undefined || boltHoleDiameter == 0 * inch)
    {
        boltHoleDiameter = 0.25 * inch; // Default 1/4" bolt hole
    }
    
    if (plugWeldHoleDiameter == undefined || plugWeldHoleDiameter == 0 * inch)
    {
        plugWeldHoleDiameter = 1/8 * inch; // Default 1/8" plug-weld holes
    }
    
    // Calculate hole spacing - plug-weld holes are positioned away from center
    // They used to be 4/16" (1/4") from center, now moved 1/16" further out = 5/16" from center
    if (holeSpacing == undefined || holeSpacing == 0 * inch)
    {
        holeSpacing = 5/16 * inch; // Plug-weld holes are 5/16" from center (3/16" from edge on 1" washer)
    }
    
    // Normalize the normal vector
    const normalizedNormal = normalize(normal);
    
    // Create sketch plane at position with normal pointing opposite to surface normal (down)
    const downDirection = -normalizedNormal;
    const washerSketchPlane = plane(position, downDirection);
    
    // === CREATE OUTER WASHER ===
    // First, create solid washer (no holes yet)
    const outerWasherSketchId = id + "outerWasherSketch";
    const outerWasherSketch = newSketchOnPlane(context, outerWasherSketchId, {
        "sketchPlane" : washerSketchPlane
    });
    
    // Draw outer washer circle only
    const outerWasherRadius = outerWasherDiameter / 2;
    skCircle(outerWasherSketch, "outerWasherCircle", {
        "center" : vector(0 * inch, 0 * inch),
        "radius" : outerWasherRadius
    });
    
    skSolve(outerWasherSketch);
    
    const outerWasherRegions = qSketchRegion(outerWasherSketchId);
    const outerWasherExtrudeStart = position;
    const outerWasherExtrudeEnd = position + (downDirection * outerWasherThickness);
    const outerWasherExtrudeDirection = normalize(outerWasherExtrudeEnd - outerWasherExtrudeStart);
    
    const outerWasherExtrudeId = id + "outerWasher";
    opExtrude(context, outerWasherExtrudeId, {
        "entities" : outerWasherRegions,
        "direction" : outerWasherExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : outerWasherThickness,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Query outer washer body
    const outerWasherBody = qBodyType(qCreatedBy(outerWasherExtrudeId, EntityType.BODY), BodyType.SOLID);
    
    // Now create and subtract the three holes using boolean operations
    // Center bolt hole
    const centerHoleSketchId = id + "centerHoleSketch";
    const centerHoleSketch = newSketchOnPlane(context, centerHoleSketchId, {
        "sketchPlane" : washerSketchPlane
    });
    skCircle(centerHoleSketch, "centerHoleCircle", {
        "center" : vector(0 * inch, 0 * inch),
        "radius" : boltHoleDiameter / 2
    });
    skSolve(centerHoleSketch);
    
    const centerHoleRegions = qSketchRegion(centerHoleSketchId);
    opExtrude(context, id + "centerHole", {
        "entities" : centerHoleRegions,
        "direction" : outerWasherExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : outerWasherThickness * 2, // Make sure it goes all the way through
        "operationType" : NewBodyOperationType.NEW
    });
    
    const centerHoleBody = qBodyType(qCreatedBy(id + "centerHole", EntityType.BODY), BodyType.SOLID);
    opBoolean(context, id + "centerHoleSubtract", {
        "tools" : centerHoleBody,
        "operationType" : BooleanOperationType.SUBTRACTION,
        "targets" : outerWasherBody
    });
    
    // Left plug-weld hole
    const leftHoleSketchId = id + "leftHoleSketch";
    const leftHoleSketch = newSketchOnPlane(context, leftHoleSketchId, {
        "sketchPlane" : washerSketchPlane
    });
    skCircle(leftHoleSketch, "leftHoleCircle", {
        "center" : vector(-holeSpacing, 0 * inch),
        "radius" : plugWeldHoleDiameter / 2
    });
    skSolve(leftHoleSketch);
    
    const leftHoleRegions = qSketchRegion(leftHoleSketchId);
    opExtrude(context, id + "leftHole", {
        "entities" : leftHoleRegions,
        "direction" : outerWasherExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : outerWasherThickness * 2,
        "operationType" : NewBodyOperationType.NEW
    });
    
    const leftHoleBody = qBodyType(qCreatedBy(id + "leftHole", EntityType.BODY), BodyType.SOLID);
    opBoolean(context, id + "leftHoleSubtract", {
        "tools" : leftHoleBody,
        "operationType" : BooleanOperationType.SUBTRACTION,
        "targets" : outerWasherBody
    });
    
    // Right plug-weld hole
    const rightHoleSketchId = id + "rightHoleSketch";
    const rightHoleSketch = newSketchOnPlane(context, rightHoleSketchId, {
        "sketchPlane" : washerSketchPlane
    });
    skCircle(rightHoleSketch, "rightHoleCircle", {
        "center" : vector(holeSpacing, 0 * inch),
        "radius" : plugWeldHoleDiameter / 2
    });
    skSolve(rightHoleSketch);
    
    const rightHoleRegions = qSketchRegion(rightHoleSketchId);
    opExtrude(context, id + "rightHole", {
        "entities" : rightHoleRegions,
        "direction" : outerWasherExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : outerWasherThickness * 2,
        "operationType" : NewBodyOperationType.NEW
    });
    
    const rightHoleBody = qBodyType(qCreatedBy(id + "rightHole", EntityType.BODY), BodyType.SOLID);
    opBoolean(context, id + "rightHoleSubtract", {
        "tools" : rightHoleBody,
        "operationType" : BooleanOperationType.SUBTRACTION,
        "targets" : outerWasherBody
    });
    
    // === CREATE INNER WASHER ===
    // Inner washer sits on top of outer washer
    const innerWasherStartPoint = position + (downDirection * outerWasherThickness);
    
    // First, create solid inner washer (no hole yet)
    const innerWasherSketchId = id + "innerWasherSketch";
    const innerWasherSketch = newSketchOnPlane(context, innerWasherSketchId, {
        "sketchPlane" : plane(innerWasherStartPoint, downDirection)
    });
    
    // Draw inner washer circle only
    const innerWasherRadius = innerWasherDiameter / 2;
    skCircle(innerWasherSketch, "innerWasherCircle", {
        "center" : vector(0 * inch, 0 * inch),
        "radius" : innerWasherRadius
    });
    
    skSolve(innerWasherSketch);
    
    const innerWasherRegions = qSketchRegion(innerWasherSketchId);
    const innerWasherExtrudeStart = innerWasherStartPoint;
    const innerWasherExtrudeEnd = innerWasherStartPoint + (downDirection * innerWasherThickness);
    const innerWasherExtrudeDirection = normalize(innerWasherExtrudeEnd - innerWasherExtrudeStart);
    
    const innerWasherExtrudeId = id + "innerWasher";
    opExtrude(context, innerWasherExtrudeId, {
        "entities" : innerWasherRegions,
        "direction" : innerWasherExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : innerWasherThickness,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Query inner washer body
    const innerWasherBody = qBodyType(qCreatedBy(innerWasherExtrudeId, EntityType.BODY), BodyType.SOLID);
    
    // Now create and subtract the hex nut clearance hole
    // 1/4-20 hex nut is 7/16" across flats, clearance should be slightly larger (1/2" = 0.5")
    const hexNutClearance = 0.5 * inch; // 1/2" across flats for clearance
    const hexNutRadius = (hexNutClearance / 2) / cos(30 * degree); // Radius to point
    
    const hexNutHoleSketchId = id + "hexNutHoleSketch";
    const hexNutHoleSketch = newSketchOnPlane(context, hexNutHoleSketchId, {
        "sketchPlane" : plane(innerWasherStartPoint, downDirection)
    });
    
    // Rotate hexagon 30 degrees so flat faces align with X-axis (where plug-weld holes are)
    const hexRotationAngle = 30 * degree;
    const hexFirstVertexX = hexNutRadius * cos(hexRotationAngle);
    const hexFirstVertexY = hexNutRadius * sin(hexRotationAngle);
    
    skRegularPolygon(hexNutHoleSketch, "hexNutClearance", {
        "center" : vector(0 * inch, 0 * inch),
        "firstVertex" : vector(hexFirstVertexX, hexFirstVertexY),
        "sides" : 6
    });
    skSolve(hexNutHoleSketch);
    
    const hexNutHoleRegions = qSketchRegion(hexNutHoleSketchId);
    opExtrude(context, id + "hexNutHole", {
        "entities" : hexNutHoleRegions,
        "direction" : innerWasherExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : innerWasherThickness * 2, // Make sure it goes all the way through
        "operationType" : NewBodyOperationType.NEW
    });
    
    const hexNutHoleBody = qBodyType(qCreatedBy(id + "hexNutHole", EntityType.BODY), BodyType.SOLID);
    opBoolean(context, id + "hexNutHoleSubtract", {
        "tools" : hexNutHoleBody,
        "operationType" : BooleanOperationType.SUBTRACTION,
        "targets" : innerWasherBody
    });
    
    // Keep washers separate (not unioned) so they can be pulled apart to show how pieces work
    // Return a query that includes both bodies (for reference), but they remain separate in the model
    const captureWasherBody = qUnion([outerWasherBody, innerWasherBody]);
    
    // Clean up sketches
    try
    {
        opDeleteBodies(context, id + "deleteOuterWasherSketch", {
            "entities" : qCreatedBy(outerWasherSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    try
    {
        opDeleteBodies(context, id + "deleteCenterHoleSketch", {
            "entities" : qCreatedBy(centerHoleSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    try
    {
        opDeleteBodies(context, id + "deleteLeftHoleSketch", {
            "entities" : qCreatedBy(leftHoleSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    try
    {
        opDeleteBodies(context, id + "deleteRightHoleSketch", {
            "entities" : qCreatedBy(rightHoleSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    try
    {
        opDeleteBodies(context, id + "deleteInnerWasherSketch", {
            "entities" : qCreatedBy(innerWasherSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    try
    {
        opDeleteBodies(context, id + "deleteHexNutHoleSketch", {
            "entities" : qCreatedBy(hexNutHoleSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    // Return the final capture washer body
    return captureWasherBody;
}

/**
 * Creates a complete bolt/capture washer/nut assembly at a specified position and orientation.
 * 
 * This function creates the full assembly:
 * - Hex bolt (1.25" shank, 3/16" head)
 * - Capture washer system (outer + inner washers)
 * - Two lock nuts (stacked)
 * 
 * The assembly is positioned so that the outer washer sits at the provided position
 * (typically on top of a tube surface). The inner washer slots into a hole in the tube,
 * and the nuts are positioned below.
 * 
 * @param context {Context} - FeatureScript context
 * @param id {Id} - Base ID for all features created by this function
 * @param position {Vector} - Position where outer washer should sit (on tube surface)
 * @param normal {Vector} - Surface normal (assembly extrudes in opposite direction)
 * @param boltShankLength {ValueWithUnits} - Optional, defaults to 1.25" (hardware store standard)
 * 
 * @returns {Query} - Query for all assembly bodies
 */
export function createBoltCaptureWasherAssembly(context is Context, id is Id, position is Vector, normal is Vector, boltShankLength is ValueWithUnits) returns Query
{
    // Set defaults
    if (boltShankLength == undefined || boltShankLength == 0 * inch)
    {
        boltShankLength = 1.25 * inch; // Default 1.25" shank length
    }
    
    // Normalize the normal vector
    const normalizedNormal = normalize(normal);
    
    // Translate everything up by 1/8" relative to origin
    // In our reference frame:
    // - Washers currently at origin, after 1/8" translation: outer washer top at +1/8"
    // - Bolt currently at +3/16", after 1/8" translation: at +5/16"
    // - Nuts currently at -0.25", after 1/8" translation: at -1/8"
    // 
    // When caller provides position P (tube surface), we want outer washer top at P
    // So we create washers at P (createCaptureWasher puts top surface at position)
    // And adjust bolt/nuts positions by +1/8" relative to their original offsets from washers
    
    const washerPosition = position; // Outer washer top will be at this position
    
    // Create bolt - originally 3/16" above washers, now 3/16" + 1/8" = 5/16" above washers
    const boltPosition = washerPosition + (normalizedNormal * (5/16 * inch));
    createBolt(context, id + "bolt", boltPosition, normal, boltShankLength, 0 * inch, 0 * inch, 0 * inch);
    
    // Create capture washer - top surface will be at provided position
    createCaptureWasher(context, id + "washer", washerPosition, normal, 0 * inch, 0 * inch, 0 * inch, 0 * inch, 0 * inch, 0 * inch, 0 * inch);
    
    // Create two nuts - originally 0.25" below washers, now 0.25" - 1/8" = 1/8" below washers
    const nut1Position = washerPosition - (normalizedNormal * (1/8 * inch));
    createNut(context, id + "nut1", nut1Position, normal, 0 * inch, 0 * inch, 0 * inch);
    
    // Second nut - positioned at bottom of first nut (3/16" below first nut)
    const nut2Position = nut1Position - (normalizedNormal * (3/16 * inch));
    createNut(context, id + "nut2", nut2Position, normal, 0 * inch, 0 * inch, 0 * inch);
    
    // Return query for all assembly bodies
    const boltBody = qBodyType(qCreatedBy(id + "bolt", EntityType.BODY), BodyType.SOLID);
    const washerBody = qCreatedBy(id + "washer", EntityType.BODY);
    const nut1Body = qBodyType(qCreatedBy(id + "nut1", EntityType.BODY), BodyType.SOLID);
    const nut2Body = qBodyType(qCreatedBy(id + "nut2", EntityType.BODY), BodyType.SOLID);
    
    return qUnion([boltBody, washerBody, nut1Body, nut2Body]);
}

annotation { "Feature Type Name" : "Bolt and Capture Washer" }
export const boltAndCaptureWasher = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        // Test createBolt and createCaptureWasher functions at startup
        const zero = 0 * inch;
        const normal = vector(0, 0, 1); // Surface normal pointing up
        
        // Create bolt 3/16" above origin (head sits above washers)
        const boltPosition = vector(zero, zero, 3/16 * inch);
        // Create bolt with 1.25" shank length (hardware store standard - excludes head)
        // Total length will be 1.25" shank + 3/16" head = 1.4375"
        // Pass 0 * inch for optional parameters to use defaults: 3/16" head height, 7/16" head width, 1/4" shank diameter
        createBolt(context, id + "bolt", boltPosition, normal, 1.25 * inch, 0 * inch, 0 * inch, 0 * inch);
        
        // Create capture washer at origin
        const washerPosition = vector(zero, zero, zero);
        // Pass 0 * inch for all optional parameters to use defaults
        createCaptureWasher(context, id + "washer", washerPosition, normal, 0 * inch, 0 * inch, 0 * inch, 0 * inch, 0 * inch, 0 * inch, 0 * inch);
        
        // Create two nuts 0.25" below the washers (below origin on Z)
        const nut1Position = vector(zero, zero, -0.25 * inch);
        // First nut - pass 0 * inch for optional parameters to use defaults: 3/16" height, 7/16" width, 1/4" bore
        createNut(context, id + "nut1", nut1Position, normal, 0 * inch, 0 * inch, 0 * inch);
        
        // Second nut - positioned at bottom of first nut (first nut extrudes down 3/16", so second nut starts there)
        const nut2Position = nut1Position + vector(zero, zero, -3/16 * inch);
        createNut(context, id + "nut2", nut2Position, normal, 0 * inch, 0 * inch, 0 * inch);
    });

