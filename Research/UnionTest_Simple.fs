// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// Simple Union Test - Just union two basic primitives

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");
import(path : "onshape/std/debug.fs", version : "2384.0");

annotation { "Feature Type Name" : "Union Test Simple" }
export const unionTestSimple = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const zero = 0 * inch;
        const startPoint = vector(zero, zero, zero);
        
        // Create first cube - make it bigger and ensure solid volume
        const cube1SketchId = id + "cube1Sketch";
        const cube1Sketch = newSketchOnPlane(context, cube1SketchId, {
            "sketchPlane" : plane(startPoint, vector(0, 0, 1))
        });
        skRectangle(cube1Sketch, "rect1", {
            "firstCorner" : vector(-1 * inch, -1 * inch),
            "secondCorner" : vector(1 * inch, 1 * inch)
        });
        skSolve(cube1Sketch);
        
        const cube1Regions = qSketchRegion(cube1SketchId);
        opExtrude(context, id + "cube1", {
            "entities" : cube1Regions,
            "direction" : vector(0, 0, 1),
            "endBound" : BoundingType.BLIND,
            "endDepth" : 2 * inch,  // Make taller for better overlap
            "operationType" : NewBodyOperationType.NEW  // Create as new separate body
        });
        
        // Create second cube - ensure significant overlap (not just touching)
        // Start at origin, so it overlaps with first cube
        const cube2SketchId = id + "cube2Sketch";
        const cube2Sketch = newSketchOnPlane(context, cube2SketchId, {
            "sketchPlane" : plane(startPoint, vector(0, 0, 1))
        });
        skRectangle(cube2Sketch, "rect2", {
            "firstCorner" : vector(-0.5 * inch, -0.5 * inch),
            "secondCorner" : vector(1.5 * inch, 1.5 * inch)  // Different size, overlapping
        });
        skSolve(cube2Sketch);
        
        const cube2Regions = qSketchRegion(cube2SketchId);
        opExtrude(context, id + "cube2", {
            "entities" : cube2Regions,
            "direction" : vector(0, 0, 1),
            "endBound" : BoundingType.BLIND,
            "endDepth" : 2 * inch,  // Same height
            "operationType" : NewBodyOperationType.NEW  // Create as new separate body
        });
        
        // Query bodies - use qUnion to combine all bodies for union operation
        const cube1Body = qBodyType(qCreatedBy(id + "cube1", EntityType.BODY), BodyType.SOLID);
        const cube2Body = qBodyType(qCreatedBy(id + "cube2", EntityType.BODY), BodyType.SOLID);
        
        // Debug to see what we're querying
        addDebugEntities(context, cube1Body, DebugColor.RED);
        addDebugEntities(context, cube2Body, DebugColor.BLUE);
        
        // Try union - put all bodies in tools parameter using qUnion
        opBoolean(context, id + "union", {
            "tools" : qUnion([cube1Body, cube2Body]),
            "operationType" : BooleanOperationType.UNION
        });
    });

// Simple function to union two primitives - exported for testing
export function unionTwoPrimitives(context is Context, id is Id, 
    body1Query is Query, body2Query is Query)
{
    opBoolean(context, id + "union", {
        "tools" : qUnion([body1Query, body2Query]),
        "operationType" : BooleanOperationType.UNION
    });
}

// Simple feature to test unioning two square tubes
annotation { "Feature Type Name" : "Union Two Primitives Test" }
export const unionTwoPrimitivesTest = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        const zero = 0 * inch;
        const tubeWidth = 1 * inch;
        const wallThickness = 1/16 * inch;
        const halfTube = tubeWidth / 2;
        const innerWidth = tubeWidth - 2 * wallThickness;
        const halfInner = innerWidth / 2;
        const tubeLength = 2 * inch;
        
        // Create first square tube
        const tube1Start = vector(zero, zero, zero);
        const tube1End = vector(zero, zero, tubeLength);
        const tube1Direction = normalize(tube1End - tube1Start);
        
        // Outer rectangle for tube 1
        const tube1OuterSketchId = id + "tube1OuterSketch";
        const tube1OuterSketch = newSketchOnPlane(context, tube1OuterSketchId, {
            "sketchPlane" : plane(tube1Start, tube1Direction)
        });
        skRectangle(tube1OuterSketch, "outerRect1", {
            "firstCorner" : vector(-halfTube, -halfTube),
            "secondCorner" : vector(halfTube, halfTube)
        });
        skSolve(tube1OuterSketch);
        
        const tube1OuterRegions = qSketchRegion(tube1OuterSketchId);
        opExtrude(context, id + "tube1Outer", {
            "entities" : tube1OuterRegions,
            "direction" : tube1Direction,
            "endBound" : BoundingType.BLIND,
            "endDepth" : tubeLength,
            "operationType" : NewBodyOperationType.NEW
        });
        
        // Inner rectangle for tube 1
        const tube1InnerSketchId = id + "tube1InnerSketch";
        const tube1InnerSketch = newSketchOnPlane(context, tube1InnerSketchId, {
            "sketchPlane" : plane(tube1Start, tube1Direction)
        });
        skRectangle(tube1InnerSketch, "innerRect1", {
            "firstCorner" : vector(-halfInner, -halfInner),
            "secondCorner" : vector(halfInner, halfInner)
        });
        skSolve(tube1InnerSketch);
        
        const tube1InnerRegions = qSketchRegion(tube1InnerSketchId);
        opExtrude(context, id + "tube1Inner", {
            "entities" : tube1InnerRegions,
            "direction" : tube1Direction,
            "endBound" : BoundingType.BLIND,
            "endDepth" : tubeLength,
            "operationType" : NewBodyOperationType.NEW
        });
        
        // Subtract inner from outer to create hollow tube 1
        opBoolean(context, id + "tube1Subtract", {
            "tools" : qBodyType(qCreatedBy(id + "tube1Inner", EntityType.BODY), BodyType.SOLID),
            "operationType" : BooleanOperationType.SUBTRACTION,
            "targets" : qBodyType(qCreatedBy(id + "tube1Outer", EntityType.BODY), BodyType.SOLID)
        });
        
        // Create second square tube - stacked directly on top of first (same X, Y, higher Z)
        // Start slightly below the top to create overlap
        const overlapDistance = 0.1 * inch; // 0.1" overlap
        const tube2Start = vector(zero, zero, tubeLength - overlapDistance); // Start slightly below top of first tube
        const tube2End = vector(zero, zero, tubeLength + tubeLength - overlapDistance); // Extend upward
        const tube2Direction = normalize(tube2End - tube2Start);
        
        // Outer rectangle for tube 2
        const tube2OuterSketchId = id + "tube2OuterSketch";
        const tube2OuterSketch = newSketchOnPlane(context, tube2OuterSketchId, {
            "sketchPlane" : plane(tube2Start, tube2Direction)
        });
        skRectangle(tube2OuterSketch, "outerRect2", {
            "firstCorner" : vector(-halfTube, -halfTube),
            "secondCorner" : vector(halfTube, halfTube)
        });
        skSolve(tube2OuterSketch);
        
        const tube2OuterRegions = qSketchRegion(tube2OuterSketchId);
        opExtrude(context, id + "tube2Outer", {
            "entities" : tube2OuterRegions,
            "direction" : tube2Direction,
            "endBound" : BoundingType.BLIND,
            "endDepth" : tubeLength,
            "operationType" : NewBodyOperationType.NEW
        });
        
        // Inner rectangle for tube 2
        const tube2InnerSketchId = id + "tube2InnerSketch";
        const tube2InnerSketch = newSketchOnPlane(context, tube2InnerSketchId, {
            "sketchPlane" : plane(tube2Start, tube2Direction)
        });
        skRectangle(tube2InnerSketch, "innerRect2", {
            "firstCorner" : vector(-halfInner, -halfInner),
            "secondCorner" : vector(halfInner, halfInner)
        });
        skSolve(tube2InnerSketch);
        
        const tube2InnerRegions = qSketchRegion(tube2InnerSketchId);
        opExtrude(context, id + "tube2Inner", {
            "entities" : tube2InnerRegions,
            "direction" : tube2Direction,
            "endBound" : BoundingType.BLIND,
            "endDepth" : tubeLength,
            "operationType" : NewBodyOperationType.NEW
        });
        
        // Subtract inner from outer to create hollow tube 2
        opBoolean(context, id + "tube2Subtract", {
            "tools" : qBodyType(qCreatedBy(id + "tube2Inner", EntityType.BODY), BodyType.SOLID),
            "operationType" : BooleanOperationType.SUBTRACTION,
            "targets" : qBodyType(qCreatedBy(id + "tube2Outer", EntityType.BODY), BodyType.SOLID)
        });
        
        // Query both tube bodies (after subtraction, query the outer body)
        const tube1Body = qBodyType(qCreatedBy(id + "tube1Outer", EntityType.BODY), BodyType.SOLID);
        const tube2Body = qBodyType(qCreatedBy(id + "tube2Outer", EntityType.BODY), BodyType.SOLID);
        
        // Union them using the exact pattern
        opBoolean(context, id + "union", {
            "tools" : qUnion([tube1Body, tube2Body]),
            "operationType" : BooleanOperationType.UNION
        });
    });

