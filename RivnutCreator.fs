// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// Rivnut Creator FeatureScript
// 
// Creates a rivnut primitive with flange, body, and bore.
// Exports createRivnut function for use in other scripts.

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");
import(path : "onshape/std/debug.fs", version : "2384.0");
import(path : "onshape/std/vector.fs", version : "2384.0");

/**
 * Creates a rivnut primitive at a specified position and orientation.
 * 
 * @param context {Context} - FeatureScript context
 * @param id {Id} - Base ID for all features created by this function
 * @param position {Vector} - Position where rivnut should be placed (on surface)
 * @param normal {Vector} - Surface normal (rivnut will extrude in opposite direction)
 * @param rivnutBodyDiameter {ValueWithUnits} - Optional, defaults to 25/64" (1/4"-20 rivnut)
 * @param startHeight {ValueWithUnits} - Optional, defaults to 1/16" (offset above position)
 * 
 * @returns {Query} - Query for the created rivnut body
 */
export function createRivnut(context is Context, id is Id, position is Vector, normal is Vector, rivnutBodyDiameter is ValueWithUnits, startHeight is ValueWithUnits) returns Query
{
    // Set defaults if not provided
    if (rivnutBodyDiameter == undefined || rivnutBodyDiameter == 0 * inch)
    {
        rivnutBodyDiameter = 25/64 * inch; // Standard 1/4"-20 rivnut body diameter
    }
    
    if (startHeight == undefined || startHeight == 0 * inch)
    {
        startHeight = 1/16 * inch; // Default 1/16" above surface
    }
    
    // Normalize the normal vector
    const normalizedNormal = normalize(normal);
    
    // Rivnut dimensions
    const flangeDiameter = 0.625 * inch; // Flange diameter
    const flangeThickness = 1/16 * inch; // Flange extrudes down 1/16"
    const bodyLength = 3/8 * inch; // Body extrudes down 3/8"
    const boreDiameter = 0.25 * inch; // 1/4" bore diameter
    const totalRivnutDepth = flangeThickness + bodyLength; // 1/16 + 3/8 = 7/16"
    const boreDepth = totalRivnutDepth + 1/16 * inch; // Add extra to ensure it goes all the way through
    
    // Calculate start point: position + (normal * startHeight)
    // The normal points "up" from the surface, so we add it to position
    const startPoint = position + (normalizedNormal * startHeight);
    
    // Create sketch plane at start point with normal pointing opposite to surface normal (down)
    // The rivnut extrudes in the opposite direction of the surface normal
    const downDirection = -normalizedNormal;
    const rivnutSketchPlane = plane(startPoint, downDirection);
    
    // Create flange cylinder
    const flangeSketchId = id + "flangeSketch";
    const flangeSketch = newSketchOnPlane(context, flangeSketchId, {
        "sketchPlane" : rivnutSketchPlane
    });
    const flangeRadius = flangeDiameter / 2;
    skCircle(flangeSketch, "flangeCircle", {
        "center" : vector(0 * inch, 0 * inch),
        "radius" : flangeRadius
    });
    skSolve(flangeSketch);
    
    const flangeRegions = qSketchRegion(flangeSketchId);
    const flangeExtrudeStart = startPoint;
    const flangeExtrudeEnd = startPoint + (downDirection * flangeThickness);
    const flangeExtrudeDirection = normalize(flangeExtrudeEnd - flangeExtrudeStart);
    
    const flangeExtrudeId = id + "flange";
    opExtrude(context, flangeExtrudeId, {
        "entities" : flangeRegions,
        "direction" : flangeExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : flangeThickness,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Query flange body
    const flangeBody = qBodyType(qCreatedBy(flangeExtrudeId, EntityType.BODY), BodyType.SOLID);
    
    // Create body sketch at the same start point as the flange
    const bodySketchId = id + "bodySketch";
    const bodySketch = newSketchOnPlane(context, bodySketchId, {
        "sketchPlane" : rivnutSketchPlane
    });
    const bodyRadius = rivnutBodyDiameter / 2;
    skCircle(bodySketch, "bodyCircle", {
        "center" : vector(0 * inch, 0 * inch),
        "radius" : bodyRadius
    });
    skSolve(bodySketch);
    
    const bodyRegions = qSketchRegion(bodySketchId);
    const bodyExtrudeStart = startPoint;
    const bodyExtrudeEnd = startPoint + (downDirection * bodyLength);
    const bodyExtrudeDirection = normalize(bodyExtrudeEnd - bodyExtrudeStart);
    
    // Extrude body
    const bodyExtrudeId = id + "body";
    opExtrude(context, bodyExtrudeId, {
        "entities" : bodyRegions,
        "direction" : bodyExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : bodyLength,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Query body
    const bodyBody = qBodyType(qCreatedBy(bodyExtrudeId, EntityType.BODY), BodyType.SOLID);
    
    // Union using qUnion in tools parameter
    opBoolean(context, id + "join", {
        "tools" : qUnion([flangeBody, bodyBody]),
        "operationType" : BooleanOperationType.UNION
    });
    
    // After union, query the merged rivnut body
    const rivnutBody = qBodyType(qCreatedBy(flangeExtrudeId, EntityType.BODY), BodyType.SOLID);
    
    // Create bore cylinder
    const boreSketchId = id + "boreSketch";
    const boreSketch = newSketchOnPlane(context, boreSketchId, {
        "sketchPlane" : rivnutSketchPlane
    });
    const boreRadius = boreDiameter / 2;
    skCircle(boreSketch, "boreCircle", {
        "center" : vector(0 * inch, 0 * inch),
        "radius" : boreRadius
    });
    skSolve(boreSketch);
    
    const boreRegions = qSketchRegion(boreSketchId);
    const boreExtrudeStart = startPoint;
    const boreExtrudeEnd = startPoint + (downDirection * boreDepth);
    const boreExtrudeDirection = normalize(boreExtrudeEnd - boreExtrudeStart);
    
    opExtrude(context, id + "bore", {
        "entities" : boreRegions,
        "direction" : boreExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : boreDepth,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Boolean subtract bore from composite rivnut
    const boreBody = qBodyType(qCreatedBy(id + "bore", EntityType.BODY), BodyType.SOLID);
    opBoolean(context, id + "boreSubtract", {
        "tools" : boreBody,
        "operationType" : BooleanOperationType.SUBTRACTION,
        "targets" : rivnutBody
    });
    
    // Clean up sketches after rivnut is complete
    try
    {
        opDeleteBodies(context, id + "deleteFlangeSketch", {
            "entities" : qCreatedBy(flangeSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    try
    {
        opDeleteBodies(context, id + "deleteBodySketch", {
            "entities" : qCreatedBy(bodySketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    try
    {
        opDeleteBodies(context, id + "deleteBoreSketch", {
            "entities" : qCreatedBy(boreSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    // Return the final rivnut body
    return rivnutBody;
}

annotation { "Feature Type Name" : "Rivnut Creator" }
export const rivnutCreator = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Rivnut Body Diameter" }
        isLength(definition.rivnutBodyDiameter, { (inch) : [0.2, 0.390625, 1] } as LengthBoundSpec); // 25/64" = 0.390625"
        
        annotation { "Name" : "Start Height Above Origin" }
        isLength(definition.startHeight, { (inch) : [0, 0.0625, 10] } as LengthBoundSpec); // Default 1/16" = 0.0625"
    }
    {
        // Set explicit defaults
        if (definition.rivnutBodyDiameter == undefined || definition.rivnutBodyDiameter == 0 * inch)
        {
            definition.rivnutBodyDiameter = 25/64 * inch; // Standard 1/4"-20 rivnut body diameter
        }
        else if (definition.rivnutBodyDiameter == 1 * inch)
        {
            definition.rivnutBodyDiameter = 25/64 * inch;
        }
        
        if (definition.startHeight == undefined || definition.startHeight == 0 * inch)
        {
            definition.startHeight = 1/16 * inch; // Default 1/16" above origin
        }
        else if (definition.startHeight == 1 * inch)
        {
            definition.startHeight = 1/16 * inch;
        }
        
        // Use the exported createRivnut function
        // Position at origin, normal pointing up (Z+)
        const zero = 0 * inch;
        const position = vector(zero, zero, zero);
        const normal = vector(0, 0, 1); // Surface normal pointing up
        
        createRivnut(context, id, position, normal, definition.rivnutBodyDiameter, definition.startHeight);
    }, {
        editingLogic : function(context is Context, definition is map) returns map
        {
            // Override 1 inch values (OnShape's fallback) with correct defaults when editing
            if (definition.rivnutBodyDiameter == 1 * inch)
            {
                definition.rivnutBodyDiameter = 25/64 * inch;
            }
            
            if (definition.startHeight == 1 * inch)
            {
                definition.startHeight = 1/16 * inch;
            }
            
            return definition;
        }
    });

