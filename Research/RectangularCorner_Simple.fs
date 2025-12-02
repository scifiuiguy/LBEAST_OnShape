// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// SIMPLE CORNER TEST - Creates two tubes at 90 degrees to each other
// Helps debug extrude issues with beams at angles

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");

annotation { "Feature Type Name" : "Rectangular Corner Simple" }
export const rectangularCornerSimple = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Tube Width" }
        isLength(definition.tubeWidth, { (inch) : [0.1, 1, 10] } as LengthBoundSpec);
        
        annotation { "Name" : "Tube Wall Thickness" }
        isLength(definition.tubeWallThickness, { (inch) : [0.01, 0.0625, 1] } as LengthBoundSpec);
        
        annotation { "Name" : "Frame Width (X)" }
        isLength(definition.frameWidth, { (inch) : [1, 72, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "Frame Depth (Y)" }
        isLength(definition.frameDepth, { (inch) : [1, 12, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "Frame Height (Z)" }
        isLength(definition.frameHeight, { (inch) : [1, 24, 200] } as LengthBoundSpec);
    }
    {
        // Set explicit defaults - only override if value is exactly 1 inch (OnShape's fallback default)
        // This preserves user-set values while fixing OnShape's default issue
        
        // Tube width: 1 inch is the correct default, so only check for undefined/zero
        if (definition.tubeWidth == undefined || definition.tubeWidth == 0 * inch)
        {
            definition.tubeWidth = 1 * inch;
        }
        
        // Wall thickness: Override 1 inch (wrong default) to 0.0625 inch
        // But preserve any other user-set value
        if (definition.tubeWallThickness == undefined || definition.tubeWallThickness == 0 * inch)
        {
            definition.tubeWallThickness = 0.0625 * inch;
        }
        else if (definition.tubeWallThickness == 1 * inch)
        {
            // This is OnShape's fallback default, override it
            definition.tubeWallThickness = 0.0625 * inch;
        }
        
        // Frame width: Override 1 inch (wrong default) to 72 inch
        if (definition.frameWidth == undefined || definition.frameWidth == 0 * inch)
        {
            definition.frameWidth = 72 * inch;
        }
        else if (definition.frameWidth == 1 * inch)
        {
            // This is OnShape's fallback default, override it
            definition.frameWidth = 72 * inch;
        }
        
        // Frame depth: Override 1 inch (wrong default) to 12 inch
        if (definition.frameDepth == undefined || definition.frameDepth == 0 * inch)
        {
            definition.frameDepth = 12 * inch;
        }
        else if (definition.frameDepth == 1 * inch)
        {
            // This is OnShape's fallback default, override it
            definition.frameDepth = 12 * inch;
        }
        
        // Frame height: Override 1 inch (wrong default) to 24 inch
        if (definition.frameHeight == undefined || definition.frameHeight == 0 * inch)
        {
            definition.frameHeight = 24 * inch;
        }
        else if (definition.frameHeight == 1 * inch)
        {
            // This is OnShape's fallback default, override it
            definition.frameHeight = 24 * inch;
        }
        
        const innerWidth = definition.tubeWidth - 2 * definition.tubeWallThickness;
        const halfTube = definition.tubeWidth / 2;
        const halfInner = innerWidth / 2;
        const zero = 0 * inch;
        
        // Create square frame with 4 tubes
        // Bottom X tube (along X axis at Y=0)
        const bottomTubeId = id + "bottomX";
        createTube(context, bottomTubeId, 
            vector(zero, zero, zero),
            vector(definition.frameWidth, zero, zero),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Top X tube (along X axis at Y=depth, offset by halfTube + halfTube = tubeWidth in Y)
        const topTubeId = id + "topX";
        const topY = definition.frameDepth + definition.tubeWidth;
        createTube(context, topTubeId,
            vector(zero, topY, zero),
            vector(definition.frameWidth, topY, zero),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Left Y tube (along Y axis at X=0, offset by halfTube in X and Y)
        const leftTubeId = id + "leftY";
        const leftOffsetX = halfTube;
        const leftOffsetY = halfTube;
        createTube(context, leftTubeId,
            vector(leftOffsetX, leftOffsetY, zero),
            vector(leftOffsetX, leftOffsetY + definition.frameDepth, zero),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Right Y tube (along Y axis at X=width, offset by halfTube - tubeWidth = -halfTube in X)
        const rightTubeId = id + "rightY";
        const rightOffsetX = definition.frameWidth - halfTube;
        const rightOffsetY = halfTube;
        createTube(context, rightTubeId,
            vector(rightOffsetX, rightOffsetY, zero),
            vector(rightOffsetX, rightOffsetY + definition.frameDepth, zero),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Create top frame by duplicating all four tubes and translating up by frameHeight
        // Offset down by one tube width so vertical posts connect properly
        const topZ = definition.frameHeight - definition.tubeWidth;
        
        // Bottom X tube (top frame)
        createTube(context, id + "topBottomX",
            vector(zero, zero, topZ),
            vector(definition.frameWidth, zero, topZ),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Top X tube (top frame)
        createTube(context, id + "topTopX",
            vector(zero, topY, topZ),
            vector(definition.frameWidth, topY, topZ),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Left Y tube (top frame)
        createTube(context, id + "topLeftY",
            vector(leftOffsetX, leftOffsetY, topZ),
            vector(leftOffsetX, leftOffsetY + definition.frameDepth, topZ),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Right Y tube (top frame)
        createTube(context, id + "topRightY",
            vector(rightOffsetX, rightOffsetY, topZ),
            vector(rightOffsetX, rightOffsetY + definition.frameDepth, topZ),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Create vertical corner post
        // Post connects bottom frame to top frame
        // Length is frameHeight minus two tube widths (one for bottom frame, one for top frame)
        // Offset by halfTube in X to align with corner, subtract halfTube from Y and Z
        const postStartZ = definition.tubeWidth - halfTube; // Start above bottom frame, adjusted down
        const postEndZ = definition.frameHeight - definition.tubeWidth - halfTube; // End at top frame, adjusted down
        const postLength = definition.frameHeight - 2 * definition.tubeWidth;
        const postOffsetX = halfTube; // Offset to align with corner
        const postOffsetY = halfTube - halfTube; // Offset to align with corner, adjusted
        
        // Post 1: Corner at origin (front-left)
        createTube(context, id + "post1",
            vector(postOffsetX, postOffsetY, postStartZ),
            vector(postOffsetX, postOffsetY, postEndZ),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Post 2: Corner offset on X by frameWidth - tubeWidth (front-right)
        const post2OffsetX = postOffsetX + definition.frameWidth - definition.tubeWidth;
        createTube(context, id + "post2",
            vector(post2OffsetX, postOffsetY, postStartZ),
            vector(post2OffsetX, postOffsetY, postEndZ),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Post 3: Corner offset on Y by frameDepth + tubeWidth (back-left)
        const post3OffsetY = postOffsetY + definition.frameDepth + definition.tubeWidth;
        createTube(context, id + "post3",
            vector(postOffsetX, post3OffsetY, postStartZ),
            vector(postOffsetX, post3OffsetY, postEndZ),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Post 4: Corner offset on both X and Y (back-right)
        createTube(context, id + "post4",
            vector(post2OffsetX, post3OffsetY, postStartZ),
            vector(post2OffsetX, post3OffsetY, postEndZ),
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
    }, {
        editingLogic : function(context is Context, definition is map) returns map
        {
            // Override 1 inch values (OnShape's fallback) with correct defaults when editing
            if (definition.tubeWallThickness == 1 * inch)
            {
                definition.tubeWallThickness = 0.0625 * inch;
            }
            
            if (definition.frameWidth == 1 * inch)
            {
                definition.frameWidth = 72 * inch;
            }
            
            if (definition.frameDepth == 1 * inch)
            {
                definition.frameDepth = 12 * inch;
            }
            
            if (definition.frameHeight == 1 * inch)
            {
                definition.frameHeight = 24 * inch;
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

