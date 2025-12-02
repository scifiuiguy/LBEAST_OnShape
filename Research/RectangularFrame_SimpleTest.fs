// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// SIMPLE TEST - Just creates one square tube to debug the extrude issue
// If this works, we can build up from here

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");

annotation { "Feature Type Name" : "Simple Frame Test" }
export const simpleFrameTest = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Tube Width", "Default unit" : "in", "Default" : "1 in" }
        isLength(definition.tubeWidth, LENGTH_BOUNDS);
        
        annotation { "Name" : "Tube Wall Thickness", "Default unit" : "in", "Default" : "0.0625 in" }
        isLength(definition.tubeWallThickness, LENGTH_BOUNDS);
        
        annotation { "Name" : "Height", "Default unit" : "in", "Default" : "24 in" }
        isLength(definition.height, LENGTH_BOUNDS);
    }
    {
        const innerWidth = definition.tubeWidth - 2 * definition.tubeWallThickness;
        const halfTube = definition.tubeWidth / 2;
        const halfInner = innerWidth / 2;
        
        // Create sketch on XY plane at origin
        const sketchPlane = plane(vector(0, 0, 0) * inch, vector(0, 0, 1) * inch);
        const sketchId = id + "sketch";
        const sketch = newSketchOnPlane(context, sketchId, {
            "sketchPlane" : sketchPlane
        });
        
        // Create outer rectangle
        skRectangle(sketch, "outerRect", {
            "firstCorner" : vector(-halfTube, -halfTube),
            "secondCorner" : vector(halfTube, halfTube)
        });
        skSolve(sketch);
        
        // Extrude outer rectangle as solid
        const outerRegions = qSketchRegion(sketchId);
        opExtrude(context, id + "outer", {
            "entities" : outerRegions,
            "direction" : vector(0, 0, 1),
            "endBound" : BoundingType.BLIND,
            "endDepth" : definition.height
        });
        
        // Create inner rectangle sketch on same plane
        const innerSketchId = id + "innerSketch";
        const innerSketch = newSketchOnPlane(context, innerSketchId, {
            "sketchPlane" : sketchPlane
        });
        
        // Create inner rectangle
        skRectangle(innerSketch, "innerRect", {
            "firstCorner" : vector(-halfInner, -halfInner),
            "secondCorner" : vector(halfInner, halfInner)
        });
        skSolve(innerSketch);
        
        // Extrude inner rectangle as solid (full height)
        const innerRegions = qSketchRegion(innerSketchId);
        opExtrude(context, id + "inner", {
            "entities" : innerRegions,
            "direction" : vector(0, 0, 1),
            "endBound" : BoundingType.BLIND,
            "endDepth" : definition.height
        });
        
        // Subtract inner from outer to create hollow tube
        opBoolean(context, id + "subtract", {
            "tools" : qCreatedBy(id + "inner", EntityType.BODY),
            "operationType" : BooleanOperationType.SUBTRACTION,
            "targets" : qCreatedBy(id + "outer", EntityType.BODY)
        });
    });

