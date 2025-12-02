// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// Manipulator Test - Tests interactive manipulators for rotating a cube

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");
import(path : "onshape/std/debug.fs", version : "2384.0");
import(path : "onshape/std/manipulator.fs", version : "2384.0");

annotation { "Feature Type Name" : "Manipulator Test",
             "Manipulator Change Function" : "manipulatorTestOnChange" }
export const manipulatorTest = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Cube Width" }
        isLength(definition.cubeWidth, { (inch) : [1, 2, 10] } as LengthBoundSpec);
        
        annotation { "Name" : "Rotation Normalized" }
        isReal(definition.rotationNormalized, { (unitless) : [0, 0, 1] } as RealBoundSpec);
    }
    {
        const zero = 0 * inch;
        const cubeWidth = definition.cubeWidth == undefined || definition.cubeWidth == 1 * inch ? 2 * inch : definition.cubeWidth;
        const halfWidth = cubeWidth / 2;
        
        // Create a simple cube
        const cubeSketchId = id + "cubeSketch";
        const cubeSketch = newSketchOnPlane(context, cubeSketchId, {
            "sketchPlane" : plane(vector(zero, zero, zero), vector(0, 0, 1))
        });
        skRectangle(cubeSketch, "cubeRect", {
            "firstCorner" : vector(-halfWidth, -halfWidth),
            "secondCorner" : vector(halfWidth, halfWidth)
        });
        skSolve(cubeSketch);
        
        const cubeRegions = qSketchRegion(cubeSketchId);
        opExtrude(context, id + "cube", {
            "entities" : cubeRegions,
            "direction" : vector(0, 0, 1),
            "endBound" : BoundingType.BLIND,
            "endDepth" : cubeWidth,
            "operationType" : NewBodyOperationType.NEW
        });
        
        // Query the cube body
        const cubeBody = qBodyType(qCreatedBy(id + "cube", EntityType.BODY), BodyType.SOLID);
        
        // Use a vertical edge at the corner of the cube as rotation axis
        // The cube is centered at origin, so one corner is at (-halfWidth, -halfWidth, 0)
        // We'll rotate around the vertical edge at that corner
        const rotationAxisLine = line(vector(-halfWidth, -halfWidth, zero), vector(0, 0, 1));
        
        // Map normalized value (0-1) to rotation angle (0-180 degrees)
        const rotationNormalized = definition.rotationNormalized == undefined ? 0 : definition.rotationNormalized;
        const rotationAngle = rotationNormalized * 180 * degree;
        
        // Apply rotation to the cube
        if (abs(rotationAngle) > 1e-6 * degree)
        {
            const rotationTransform = rotationAround(rotationAxisLine, rotationAngle);
            opTransform(context, id + "rotateCube", {
                "bodies" : cubeBody,
                "transform" : rotationTransform
            });
        }
        
        // Add a linear manipulator for up/down dragging
        // The manipulator will be positioned vertically above the cube
        const cubeBox = evBox3d(context, {
            "topology" : cubeBody
        });
        const manipulatorBase = vector(0 * inch, 0 * inch, cubeBox.maxCorner[2] + cubeWidth * 0.5);
        const manipulatorDirection = vector(0, 0, 1); // Up direction (unitless - direction vector)
        
        // Map rotation angle (0-180 degrees) to manipulator offset (0 to manipulatorRange)
        // Use a smaller range so the manipulator doesn't need to be dragged so far
        const manipulatorRange = cubeWidth * 0.5; // Reduced range for easier manipulation
        const manipulatorOffset = rotationNormalized * manipulatorRange;
        
        addManipulators(context, id, {
            "rotationManipulator" : linearManipulator({
                "base" : manipulatorBase,
                "direction" : manipulatorDirection,
                "offset" : manipulatorOffset,
                "minOffset" : 0 * inch,
                "maxOffset" : manipulatorRange
            })
        });
    });

export function manipulatorTestOnChange(context is Context, definition is map, newManipulators is map) returns map
{
    println("onManipulatorChange called!");
    
    // Get the new offset from the manipulator
    if (newManipulators["rotationManipulator"] == undefined)
    {
        println("ERROR: rotationManipulator not found in newManipulators");
        return definition;
    }
    
    // Get the manipulator range (we'll calculate it the same way)
    const cubeWidth = definition.cubeWidth == undefined || definition.cubeWidth == 1 * inch ? 2 * inch : definition.cubeWidth;
    const manipulatorRange = cubeWidth * 0.5; // Match the reduced range
    
    // Clamp the offset to the manipulator's physical range to prevent dragging beyond limits
    var newOffset = newManipulators["rotationManipulator"].offset;
    newOffset = clamp(newOffset, 0 * inch, manipulatorRange);
    println("New offset (clamped): " ~ toString(newOffset));
    
    // Map manipulator offset (0 to manipulatorRange) back to normalized value (0-1)
    const normalizedValue = newOffset / manipulatorRange;
    println("Normalized value: " ~ toString(normalizedValue));
    
    // Clamp to 0-1 range
    definition.rotationNormalized = clamp(normalizedValue, 0, 1);
    println("Updated rotationNormalized: " ~ toString(definition.rotationNormalized));
    
    return definition;
}

