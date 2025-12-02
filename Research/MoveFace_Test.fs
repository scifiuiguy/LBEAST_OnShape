// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// Move Face Test - Proves we can select and move the top face of a cube

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");
import(path : "onshape/std/debug.fs", version : "2384.0");
import(path : "onshape/std/moveFace.fs", version : "2384.0");

annotation { "Feature Type Name" : "Move Face Test" }
export const moveFaceTest = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Cube Width" }
        isLength(definition.cubeWidth, { (inch) : [1, 2, 10] } as LengthBoundSpec);
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
        
        // Get bounding box to find top Z coordinate
        const cubeBox = evBox3d(context, {
            "topology" : cubeBody
        });
        const topZ = cubeBox.maxCorner[2];
        
        // Create a plane at the top of the cube (parallel to XY plane)
        const topPlane = plane(vector(0, 0, topZ), vector(0, 0, 1));
        
        // Query faces parallel to the top plane
        const topFaces = qParallelPlanes(qOwnedByBody(cubeBody, EntityType.FACE), topPlane);
        
        // Move the top face down by 50% of cube width
        const moveDistance = -cubeWidth * 0.5; // Move down by 50% of width
        
        opMoveFace(context, id + "moveTopFace", {
            "moveFaces" : topFaces,
            "moveType" : MoveFaceType.TRANSLATE,
            "offset" : vector(0, 0, moveDistance)
        });
    });


