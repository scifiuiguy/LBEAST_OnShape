# Agent Guidelines for LBEAST OnShape FeatureScript Development

This document contains troubleshooting guides, best practices, and development guidelines specifically for AI agents working with OnShape FeatureScript code.

## Table of Contents

1. [Troubleshooting Common Issues](#troubleshooting-common-issues)
2. [FeatureScript API Patterns and Best Practices](#featurescript-api-patterns-and-best-practices)
3. [Debugging Best Practices](#debugging-best-practices)
4. [Code Quality: Preventing Duplication](#code-quality-preventing-duplication)
5. [CRITICAL: File Change Reporting](#critical-file-change-reporting)

---

## Troubleshooting Common Issues

### "Error regenerating" message

1. Click the **`{!}`** button in the upper-right corner of OnShape to open the FeatureScript Console
2. Read the error message - it will tell you exactly what's wrong (e.g., "undefined variable", "type mismatch", etc.)
3. Common issues:
   - Missing units on vectors (should have `* inch` for length values)
   - Incorrect function names or API changes
   - Invalid parameter values (negative numbers, zero, etc.)

### Script errors

- Make sure you copied the entire script, including the FeatureScript version header

### No frame appears

- Check that all parameters have valid values (positive numbers)

### Feature not found

- Ensure you're in a Part Studio (not an Assembly or Drawing)

### API errors

- OnShape's FeatureScript API may have changed - check the [OnShape FeatureScript Documentation](https://cad.onshape.com/FsDoc/index.html) for the latest API version

### CRITICAL: API Parameter Names Must Be Used Verbatim

**When an API Reference shows a "map" parameter with enum values, those are STATIC STRING LITERALS that must be used EXACTLY as specified in the documentation.**

**Why This Matters:**
- FeatureScript API functions use string keys in parameter maps (e.g., `{"moveFaces": query, "transform": transformObj}`)
- These parameter names are NOT variables or suggestions - they are REQUIRED exact string literals
- Using incorrect parameter names (e.g., `"offset"` instead of `"transform"`) will result in `INVALID_INPUT` errors
- The API will NOT accept similar or logical alternatives - only the exact strings documented

**How to Find Correct Parameter Names:**
1. **ALWAYS check the official OnShape API Reference**: https://cad.onshape.com/FsDoc/library.html
2. Navigate to the specific function (e.g., `moveFace` under "Onshape features")
3. Look at the parameter map definition - the keys shown are the EXACT strings you must use
4. Do NOT guess or infer parameter names based on similar functions or logical naming

**Example - opMoveFace:**
- ❌ **WRONG**: `{"moveFaces": query, "offset": vector(...)}` - "offset" is not a valid parameter
- ✅ **CORRECT**: `{"moveFaces": query, "transform": transform(vector(...))}` - "transform" is the exact parameter name

**Common Mistakes:**
- Assuming parameter names based on function purpose (e.g., using "offset" for move operations)
- Using camelCase variations of documented names
- Guessing parameter names without checking documentation
- Using parameter names from similar functions in other languages/APIs

**Required Workflow:**
1. Before using any FeatureScript function, check the official API documentation
2. Copy parameter names EXACTLY as shown in the documentation
3. Verify parameter types match (e.g., `transform()` requires a Transform object, not a vector)
4. If you get `INVALID_INPUT` errors, first verify parameter names match the documentation exactly

**Reference:** The OnShape Standard Library documentation is available at: https://cad.onshape.com/FsDoc/library.html

### CRITICAL: How to Move Faces with opMoveFace

**This is a complete guide for moving faces in OnShape FeatureScript. Follow these steps exactly to avoid hours of debugging.**

#### Step 1: Find the Target Face

**Method: Use Dot Product Comparison to Up Vector**

1. Query all faces of the body:
   ```javascript
   const allFaces = qOwnedByBody(bodyQuery, EntityType.FACE);
   const faceArray = evaluateQuery(context, allFaces);
   ```

2. Find the top-most upward-facing face using dot product:
   ```javascript
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
   ```

**Why This Works:**
- The face with the highest dot product between its normal and the up vector `(0, 0, 1)` is the top-most upward-facing face
- This method works reliably regardless of body orientation or creation method

#### Step 2: Create a Query from the Face Entity

**Method: Use qUnion with qEntityFilter**

```javascript
const topFaceQuery = qUnion([qEntityFilter(topFace, EntityType.FACE)]);
```

**Why This Works:**
- `qEntityFilter(topFace, EntityType.FACE)` creates a query from the face entity
- `qUnion([...])` wraps it in a union query that `opMoveFace` can accept
- This is the ONLY reliable method we found that works with `opMoveFace`

**Alternative Methods That DON'T Work:**
- ❌ `qNthElement(allFaces, index)` - Returns `INVALID_INPUT`
- ❌ `qContainsPoint` with `qIntersection` - Returns `DIRECT_EDIT_MOVE_FACE_SELECT` or `INVALID_INPUT`
- ❌ Passing face entity directly - Returns `INVALID_INPUT`
- ❌ `qParallelPlanes` - May select multiple faces, causing errors

#### Step 3: Create the Transform Object

**CRITICAL: All vector components must be length values**

```javascript
const moveTransform = transform(vector(0 * inch, 0 * inch, -moveDistance));
```

**Why This Matters:**
- `transform()` requires a 3D length vector
- All components must have units (e.g., `0 * inch`, not just `0`)
- Negative Z moves down, positive Z moves up

#### Step 4: Call opMoveFace with Correct Parameters

**EXACT Parameter Names (from API documentation):**

```javascript
opMoveFace(context, id + "moveFace", {
    "moveFaces" : topFaceQuery,  // NOT "faces" - must be "moveFaces"
    "transform" : moveTransform  // NOT "offset" - must be "transform"
});
```

**Parameter Details:**
- `"moveFaces"`: Query that selects the face(s) to move (must resolve to exactly one face)
- `"transform"`: Transform object created with `transform(vector(...))`
- **DO NOT** use `"moveType"` or `"offset"` - these are not valid parameters

#### Complete Working Example

```javascript
// 1. Query faces
const allFaces = qOwnedByBody(bodyQuery, EntityType.FACE);
const faceArray = evaluateQuery(context, allFaces);

// 2. Find top face using dot product
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

// 3. Create query from face entity
if (topFace != undefined)
{
    const topFaceQuery = qUnion([qEntityFilter(topFace, EntityType.FACE)]);
    
    // 4. Create transform (all components must be lengths)
    const moveDistance = -tubeWidth; // Negative moves down
    const moveTransform = transform(vector(0 * inch, 0 * inch, moveDistance));
    
    // 5. Move the face
    opMoveFace(context, id + "moveFace", {
        "moveFaces" : topFaceQuery,
        "transform" : moveTransform
    });
}
```

#### Common Errors and Solutions

| Error | Cause | Solution |
|-------|-------|----------|
| `INVALID_INPUT` | Wrong parameter name (e.g., `"offset"` instead of `"transform"`) | Check API docs, use exact parameter names |
| `INVALID_INPUT` | Query doesn't resolve correctly | Use `qUnion([qEntityFilter(face, EntityType.FACE)])` |
| `DIRECT_EDIT_MOVE_FACE_SELECT` | Query selects multiple faces or invalid query | Ensure query resolves to exactly one face |
| `Precondition of transform failed` | Vector components not all lengths | Use `vector(0 * inch, 0 * inch, distance)` not `vector(0, 0, distance)` |

#### Debugging Tips

1. **Verify face selection**: Use `addDebugEntities(context, topFace, DebugColor.RED)` to highlight the face you found
2. **Verify query**: Use `evaluateQuery(context, topFaceQuery)` and check it resolves to exactly one face
3. **Check query match**: Compare the query result to the original face entity to ensure they match

#### Key Takeaways

1. **Always use dot product** to find the top face - it's the most reliable method
2. **Always use `qUnion([qEntityFilter(face, EntityType.FACE)])`** to create the query - it's the only method that works
3. **Always use `"moveFaces"` and `"transform"`** as parameter names - check API docs, don't guess
4. **Always use length units** in transform vectors - `vector(0 * inch, 0 * inch, distance)`
5. **Always verify the query resolves** before calling `opMoveFace` - use `evaluateQuery` to check

### Compiler errors

- **"Error in initializer function arguments"**: Usually means a `var` declaration is uninitialized. FeatureScript requires all `var` declarations to be initialized with a value.
- **"missing TOP_SEMI at 'function'"**: Usually indicates a missing semicolon or closing brace before a function definition. Check for unclosed blocks or syntax errors in preceding code.
- **"Function mod with 2 argument(s) not found"**: FeatureScript doesn't have a `mod()` function. Use a custom helper function or alternative approach.
- **"Invalid enum access"**: Check that the enum value exists (e.g., `DebugColor.PURPLE` doesn't exist, use `DebugColor.MAGENTA` instead).
- **"Call addDebugEntities(...) does not match"**: Check the function signature - `addDebugEntities` takes `(context, query, DebugColor)` where `DebugColor` is an enum, not a map.

### Common Errors and Solutions

1. **"Precondition of skRectangle failed" or "Precondition of skCircle failed"**
   - Solution: Ensure sketch coordinates have units: `vector(0 * inch, 0 * inch)`

2. **"EXTRUDE_FAILED"**
   - Solution: Use `qSketchRegion(sketchId)` directly, not `qCreatedBy` with `EntityType.FACE`
   - Ensure direction vector is unitless (normalized)

3. **"Extrude direction not working / always extrudes in wrong direction"**
   - CRITICAL: OnShape IGNORES literal direction vectors like `vector(0, 0, -1)`
   - Solution: ALWAYS calculate direction from two points:
     ```javascript
     const delta = endPoint - startPoint;
     const direction = normalize(delta);
     ```
   - This is the ONLY reliable way to control extrude direction
   - `oppositeDirection` parameter does NOT work reliably
   - Sketch plane normal inversion does NOT control extrude direction

4. **"BOOLEAN_INPUTS_NOT_SOLID"**
   - Solution: Use `qBodyType(query, BodyType.SOLID)` to filter for solid bodies only

5. **"@opBoolean: BOOLEAN_BAD_INPUT" (for UNION operations)**
   - CRITICAL: OnShape auto-merges overlapping bodies during `opExtrude`
   - Solution: Add `"operationType" : NewBodyOperationType.NEW` to ALL `opExtrude` calls that create bodies you want to union later
   - For UNION operations, use `qUnion` in tools parameter (no targets):
     ```javascript
     opBoolean(context, id + "union", {
         "tools" : qUnion([body1, body2]),
         "operationType" : BooleanOperationType.UNION
     });
     ```
   - For SUBTRACTION operations, use tools/targets pattern:
     ```javascript
     opBoolean(context, id + "subtract", {
         "tools" : toolBody,
         "operationType" : BooleanOperationType.SUBTRACTION,
         "targets" : targetBody
     });
     ```

6. **"Function opBox with 3 args not found"**
   - Solution: Don't use `opBox` - use sketch + extrude pattern instead

7. **"Precondition of newSketch failed"**
   - Solution: Use `newSketchOnPlane` when you have a Plane object

8. **Parameters defaulting to 1 inch instead of desired values**
   - Solution: Add explicit initialization in function body AND editingLogic

9. **"Invalid enum access: EntityType.SKETCH"**
   - Solution: `EntityType.SKETCH` doesn't exist - use `EntityType.BODY` for queries

---

## Debugging Best Practices

### CRITICAL: Spatial Debugging Workflow for Bodies and Faces

**Problem:** AI agents struggle significantly with spatial/geometric reasoning. When you need to identify a specific body among many bodies, or a specific face among many faces on a body, you cannot rely on spatial logic alone.

**Solution:** Use systematic color-coding with user confirmation to identify targets, then document the results clearly.

#### Workflow for Identifying Specific Bodies

**When to Use:** You need to target one or more specific bodies from a collection (e.g., an array of bodies after rotations).

**Steps:**
1. **Add color-coding code** that colors 6 bodies at a time using the 6 available OnShape debug colors:
   ```javascript
   const colors = [DebugColor.RED, DebugColor.GREEN, DebugColor.BLUE, DebugColor.YELLOW, DebugColor.MAGENTA, DebugColor.CYAN];
   const startIndex = floor(arraySize / 2) - 3; // Position around likely target area
   if (startIndex < 0) startIndex = 0;
   
   for (var i = 0; i < 6 && (startIndex + i) < arraySize; i += 1)
   {
       const body = bodyArray[startIndex + i];
       addDebugEntities(context, body, colors[i]);
   }
   ```

2. **Provide the index-to-color map in CHAT** (NOT in console with `println`):
   ```
   Index (startIndex): RED
   Index (startIndex + 1): GREEN
   Index (startIndex + 2): BLUE
   Index (startIndex + 3): YELLOW
   Index (startIndex + 4): MAGENTA
   Index (startIndex + 5): CYAN
   ```

3. **Wait for user confirmation** - User will reply with the correct color(s) or index(es)

4. **Remove color-coding code** and document the identified indices with clear comments:
   ```javascript
   // CRITICAL: The two horizontal tubes at the top of the center segment are at:
   // - startIndex + 1 (first target body) - identified as GREEN through color-coding
   // - startIndex + 2 (second target body) - identified as BLUE through color-coding
   // 
   // WARNING: Do not change these indices or remove this documentation without user confirmation.
   // This identification was established through iterative color-coding and user verification.
   ```

#### Workflow for Identifying Specific Faces

**When to Use:** You need to target a specific face on a body (e.g., the end face of a tube for `opMoveFace`).

**Steps:**
1. **Get all faces on the target body:**
   ```javascript
   const allFaces = qOwnedByBody(qBodyType(qEntityFilter(body, EntityType.BODY), BodyType.SOLID), EntityType.FACE);
   const faceArray = evaluateQuery(context, allFaces);
   ```

2. **Color-code the first 6 faces** (or a specific group):
   ```javascript
   const colors = [DebugColor.RED, DebugColor.GREEN, DebugColor.BLUE, DebugColor.YELLOW, DebugColor.MAGENTA, DebugColor.CYAN];
   const faceGroupStart = 0; // Start with first 6, or adjust as needed
   
   for (var i = 0; i < 6 && (faceGroupStart + i) < size(faceArray); i += 1)
   {
       const face = faceArray[faceGroupStart + i];
       addDebugEntities(context, face, colors[i]);
   }
   ```

3. **Provide the face index-to-color map in CHAT** (NOT in console):
   ```
   Face 0: RED
   Face 1: GREEN
   Face 2: BLUE
   Face 3: YELLOW
   Face 4: MAGENTA
   Face 5: CYAN
   ```

4. **Wait for user confirmation** - User will reply with:
   - The correct color (e.g., "YELLOW")
   - The correct face index (e.g., "face 9")
   - Or "not in this group" to move to the next 6 faces

5. **If not found in current group:**
   - Remove current color-coding
   - Color-code the next group of 6 faces
   - Provide new map in chat
   - Repeat until target is identified

6. **Once identified, remove color-coding** and document the face index:
   ```javascript
   // CRITICAL: Face index 9 is the end face to shorten on horizontal tubes
   // - Identified as YELLOW through iterative face color-coding
   // - Confirmed by user through visual inspection
   // 
   // WARNING: Do not change this face index or remove this documentation without user confirmation.
   ```

#### Critical Rules for Agents

1. **ALWAYS provide color maps in CHAT, never in console** - Use `println` for debugging, but provide the map directly in your response to the user
2. **ALWAYS document identified indices** - Add clear comments explaining which indices were identified and how
3. **ALWAYS add warnings** - Include comments warning future changes not to destroy the index identification comments
4. **ALWAYS remove color-coding after identification** - Clean up debug code once the target is confirmed
5. **NEVER guess spatial relationships** - If you're uncertain, use color-coding rather than assuming
6. **NEVER remove index documentation comments** - These are critical for preventing future mistakes

#### Why This Workflow Exists

AI agents excel at theoretical and logical operations but struggle significantly with spatial/geometric reasoning. This workflow:
- Leverages human spatial perception for identification
- Uses systematic color-coding to narrow down candidates
- Documents results clearly to prevent future mistakes
- Establishes a repeatable process for similar problems

**Remember:** When in doubt about spatial identification, use color-coding. It's faster and more reliable than guessing.

### Color-Coded Queries for Boolean Operations

**CRITICAL:** Always use color-coded debug highlighting when working with boolean operations (union, subtraction, etc.). It is very easy to accidentally mix one object's query with another, especially when objects have been transformed or when working with multiple similar parts.

**Why This Matters:**
- Boolean operations require precise query matching between tools and targets
- After transformations (rotation, translation), object IDs may not match visual positions
- Multiple similar objects (e.g., top/bottom tubes, left/right extensions) can easily be confused
- Without visual debugging, you may spend significant time troubleshooting why unions fail

**How to Use:**
1. Import the debug module: `import(path : "onshape/std/debug.fs", version : "2384.0");`
2. Query each body you plan to use in the boolean operation
3. Use `addDebugEntities(context, query, DebugColor.COLOR)` to highlight each body with a unique color
4. Verify in the 3D viewer that:
   - The correct bodies are being queried
   - Bodies that should overlap are actually overlapping
   - Bodies are on the correct sides/positions relative to each other
5. Only proceed with the boolean operation once you've confirmed the queries are correct

**Example:**
```javascript
const body1 = qBodyType(qCreatedBy(id + "body1", EntityType.BODY), BodyType.SOLID);
const body2 = qBodyType(qCreatedBy(id + "body2", EntityType.BODY), BodyType.SOLID);

// Debug: Highlight bodies with different colors
addDebugEntities(context, body1, DebugColor.RED);
addDebugEntities(context, body2, DebugColor.BLUE);

// Verify they overlap correctly before unioning
opBoolean(context, id + "union", {
    "tools" : qUnion([body1, body2]),
    "operationType" : BooleanOperationType.UNION
});
```

**Common Pitfalls:**
- Querying base IDs instead of "outer" bodies after subtraction operations
- Mixing up top/bottom or left/right queries after rotations
- Querying transformed bodies incorrectly
- Assuming object names match their visual positions after transformations

**Lesson Learned:** In `LBEASTWallFrameCreator`, tube extensions were being unioned with tubes on opposite sides of the frame because queries were swapped. Color-coded debugging immediately revealed the mismatch and saved significant debugging time.

---

## FeatureScript API Patterns and Best Practices

### Basic Structure

- **FeatureScript version:** 2384 (current)
- **Required imports:**
  - `onshape/std/geometry.fs`
  - `onshape/std/sketch.fs`
  - `onshape/std/transform.fs` (for transforms/duplications)

- **Feature definition pattern:**
  ```javascript
  annotation { "Feature Type Name" : "Your Feature Name" }
  export const yourFeature = defineFeature(function(context is Context, id is Id, definition is map)
      precondition { ... }
      { ... }
      { editingLogic : function(context is Context, definition is map) returns map { ... } }
  );
  ```

### Parameter Definition

- **Length parameters** use `LengthBoundSpec` with `[min, default, max]` format:
  ```javascript
  annotation { "Name" : "Parameter Name" }
  isLength(definition.paramName, { (inch) : [min, default, max] } as LengthBoundSpec);
  ```

- **Integer parameters** use `IntegerBoundSpec`:
  ```javascript
  annotation { "Name" : "Parameter Name" }
  isInteger(definition.paramName, { (unitless) : [min, default, max] } as IntegerBoundSpec);
  ```

- **Boolean parameters:**
  ```javascript
  annotation { "Name" : "Parameter Name" }
  definition.paramName is boolean;
  ```

- **CRITICAL: OnShape defaults to 1 inch for length parameters if not properly set.**
  Always override 1 inch values in the function body and editingLogic:
  ```javascript
  if (definition.paramName == undefined || definition.paramName == 0 * inch)
  {
      definition.paramName = desiredDefault;
  }
  else if (definition.paramName == 1 * inch)
  {
      definition.paramName = desiredDefault; // Override OnShape's fallback
  }
  ```

- `LengthBoundSpec` in precondition sets GUI defaults, but you still need explicit initialization in the function body to handle OnShape's 1 inch fallback.

### Creating Hollow Tubes (Square Steel Tube Pattern)

**Pattern:** Extrude outer solid, extrude inner solid (full length), then boolean subtract.

1. **Calculate dimensions:**
   ```javascript
   const innerWidth = tubeWidth - 2 * wallThickness;
   const halfTube = tubeWidth / 2;
   const halfInner = innerWidth / 2;
   const zero = 0 * inch;
   ```

2. **Determine sketch plane based on tube direction:**
   - For axis-aligned tubes, use world coordinate planes
   - X-axis tube: sketch in YZ plane (normal = X)
   - Y-axis tube: sketch in XZ plane (normal = Y)
   - Z-axis tube: sketch in XY plane (normal = Z)
   
   ```javascript
   var sketchPlane;
   const absX = abs(direction[0]);
   const absY = abs(direction[1]);
   const absZ = abs(direction[2]);
   
   if (absX > absY && absX > absZ)
   {
       sketchPlane = plane(startPoint, vector(1, 0, 0)); // YZ plane
   }
   else if (absY > absZ)
   {
       sketchPlane = plane(startPoint, vector(0, 1, 0)); // XZ plane
   }
   else
   {
       sketchPlane = plane(startPoint, vector(0, 0, 1)); // XY plane
   }
   ```

3. **Create outer rectangle sketch:**
   ```javascript
   const outerSketchId = id + "outerSketch";
   const outerSketch = newSketchOnPlane(context, outerSketchId, {
       "sketchPlane" : sketchPlane
   });
   skRectangle(outerSketch, "outerRect", {
       "firstCorner" : vector(-halfTube, -halfTube),
       "secondCorner" : vector(halfTube, halfTube)
   });
   skSolve(outerSketch);
   ```

4. **Extrude outer rectangle:**
   ```javascript
   const outerRegions = qSketchRegion(outerSketchId);
   opExtrude(context, id + "outer", {
       "entities" : outerRegions,
       "direction" : direction, // Normalized unit vector
       "endBound" : BoundingType.BLIND,
       "endDepth" : length // Length with units
   });
   ```

5. **Create inner rectangle sketch (same plane):**
   ```javascript
   const innerSketchId = id + "innerSketch";
   const innerSketch = newSketchOnPlane(context, innerSketchId, {
       "sketchPlane" : sketchPlane
   });
   skRectangle(innerSketch, "innerRect", {
       "firstCorner" : vector(-halfInner, -halfInner),
       "secondCorner" : vector(halfInner, halfInner)
   });
   skSolve(innerSketch);
   ```

6. **Extrude inner rectangle (full length):**
   ```javascript
   const innerRegions = qSketchRegion(innerSketchId);
   opExtrude(context, id + "inner", {
       "entities" : innerRegions,
       "direction" : direction,
       "endBound" : BoundingType.BLIND,
       "endDepth" : length
   });
   ```

7. **Boolean subtract inner from outer:**
   ```javascript
   opBoolean(context, id + "subtract", {
       "tools" : qCreatedBy(id + "inner", EntityType.BODY),
       "operationType" : BooleanOperationType.SUBTRACTION,
       "targets" : qCreatedBy(id + "outer", EntityType.BODY)
   });
   ```

### Sketch Coordinates

- **CRITICAL: Sketch coordinates MUST have units, even though they're 2D points!**
- ✅ **Correct:** `vector(0 * inch, 0 * inch)` for sketch center
- ❌ **Incorrect:** `vector(0, 0)` - will fail `is2dPoint()` precondition
- **Example for skCircle:**
  ```javascript
  const circleCenter2D = vector(0 * inch, 0 * inch);
  skCircle(sketch, "circle1", {
      "center" : circleCenter2D,
      "radius" : radius
  });
  ```

### Sketch Plane Creation

- Use `newSketchOnPlane` (NOT `newSketch`) when you have a Plane object
- `newSketch` expects a Query, `newSketchOnPlane` expects a Plane
- **Pattern:**
  ```javascript
  const sketchPlane = plane(point, normal);
  const sketch = newSketchOnPlane(context, sketchId, {
      "sketchPlane" : sketchPlane
  });
  ```

### opExtrude Direction Control - CRITICAL Pattern

- **CRITICAL: OnShape's opExtrude IGNORES literal direction vectors!**
  - Using `vector(0, 0, -1)` directly will NOT extrude downward
  - The `oppositeDirection` parameter also does NOT work reliably
  - Sketch plane normal inversion does NOT control extrude direction

- **CORRECT PATTERN: Calculate direction from two points (start and end)**
  This is the ONLY reliable way to control extrude direction:
  
  ```javascript
  const startPoint = sketchCenter;
  const endPoint = sketchCenter + vector(0 * inch, 0 * inch, -depth);
  const delta = endPoint - startPoint;
  const direction = normalize(delta); // This WILL respect the direction!
  
  opExtrude(context, id + "extrude", {
      "entities" : sketchRegions,
      "direction" : direction, // Calculated from points - this works!
      "endBound" : BoundingType.BLIND,
      "endDepth" : depth
  });
  ```

- **Why this works:** OnShape treats direction vectors calculated from points differently than literal vectors. Always calculate direction from start/end points.

- **Pattern used in all working scripts (createTube, etc.):**
  ```javascript
  const delta = endPoint - startPoint;
  const direction = normalize(delta);
  // Then use direction in opExtrude
  ```

### Querying Bodies for Boolean Operations

- **CRITICAL: Use qBodyType to filter for solid bodies only**
- Boolean operations require solid bodies, not surfaces or other entity types
- **Pattern:**
  ```javascript
  const targetBodies = qBodyType(qCreatedBy(id + "feature", EntityType.BODY), BodyType.SOLID);
  const toolBodies = qBodyType(qCreatedBy(id + "tool", EntityType.BODY), BodyType.SOLID);
  opBoolean(context, id + "boolean", {
      "tools" : toolBodies,
      "operationType" : BooleanOperationType.SUBTRACTION,
      "targets" : targetBodies
  });
  ```

### Sketch Cleanup

- Sketches can be deleted after use to reduce feature tree clutter
- **Pattern (with error handling):**
  ```javascript
  try
  {
      opDeleteBodies(context, id + "deleteSketch", {
          "entities" : qCreatedBy(sketchId, EntityType.BODY)
      });
  }
  catch
  {
      // Sketches may not be deletable - this is okay, they're just in feature history
  }
  ```
- **Note:** OnShape may keep sketches in feature history regardless, but attempting deletion is good practice for cleaner feature trees.

### Vector Arithmetic

- All vector components must have units when creating vectors with units
- **Example:** `vector(offsetX, offsetY, offsetZ)` where all are `ValueWithUnits`
- **Zero vector:** `const zero = 0 * inch;`
- **Vector addition:** `baseOffset + vector(x, y, z)` works correctly
- **Direction vectors for extrude should be unitless (normalized):**
  ```javascript
  const direction = normalize(delta); // Unitless unit vector
  ```

### Coordinate System

- OnShape uses right-handed coordinate system
- Z is typically "up" in most contexts
- When creating frames:
  - X = width (horizontal)
  - Y = depth (horizontal, perpendicular to width)
  - Z = height (vertical)

### Procedural Duplication

- Use nested loops for multi-axis duplication:
  ```javascript
  for (var i = 0; i < totalOnX; i += 1)
  {
      for (var j = 0; j < totalOnY; j += 1)
      {
          for (var k = 0; k < totalOnZ; k += 1)
          {
              const offsetX = i * frameWidth;
              const offsetY = j * frameDepth;
              const offsetZ = k * frameHeight;
              const baseOffset = vector(offsetX, offsetY, offsetZ);
              const instanceId = id + "x" + i + "y" + j + "z" + k;
              // Create geometry with baseOffset applied
          }
      }
  }
  ```

### Debugging Patterns

- Add boolean debug parameters to conditionally skip operations:
  ```javascript
  if (!definition.debugCuts)
  {
      opBoolean(...); // Skip boolean to see tool geometry
  }
  ```
- This allows inspection of intermediate geometry before boolean operations

### Best Practices

1. Always use descriptive ID suffixes: `id + "outer"`, `id + "inner"`, `id + "subtract"`
2. Calculate all dimensions upfront: `halfTube`, `halfInner`, `innerWidth`, etc.
3. Use helper functions for repeated patterns (e.g., `createTube`)
4. Apply `baseOffset` to all geometry positions when duplicating
5. Use unique instance IDs when duplicating: `id + "x" + i + "y" + j + "z" + k`
6. Clean up sketches after use (with try/catch for safety)
7. Always filter boolean inputs with `qBodyType` for solid bodies
8. Use `LengthBoundSpec` in precondition for GUI defaults
9. Override 1 inch fallback values in both function body and editingLogic
10. For bodies that will be unioned later, ALWAYS use `"operationType" : NewBodyOperationType.NEW` in `opExtrude` to prevent auto-merging
11. For UNION operations, use `qUnion([body1, body2])` in tools parameter (no targets)
12. For SUBTRACTION operations, use tools/targets pattern

---

## Code Quality: Preventing Duplication

### CRITICAL: Never Duplicate Imported Functions

**RULE:** Do NOT duplicate functions that are already imported from other FeatureScript files unless you have explicit permission from the user.

**Why This Matters:**
- Functions are moved to separate files (like `LBEASTWallComponents.fs` and `LBEASTWallUtil.fs`) specifically to avoid duplication
- Duplicating imported functions defeats the purpose of modularization
- It creates maintenance nightmares when the same function exists in multiple places
- It causes confusion about which version is the "correct" one
- It leads to files growing unnecessarily large (e.g., 2600+ lines when they should be much smaller)

**When Duplication Might Be Acceptable:**
- Only if you have a **really good reason** to migrate an imported function inline
- **ALWAYS ask the user for permission first** before duplicating imported functions
- Even then, consider if the function should be refactored instead

**How to Check:**
- Before adding any function, check if it's already imported
- Look at the `import` statements at the top of the file
- Check the imported files to see what functions they export
- If a function exists in an imported file, use the imported version

**Example of What NOT to Do:**
```javascript
// BAD: Duplicating createComposite when it's already imported from WallComponents
import(path : "1a352bc5f15cd57be34e8ae2", version : "d709f83721fcbc8242cc7caf"); // LBEASTWallComponents

// ... later in the file ...
function createComposite(...) {  // DON'T DO THIS - it's already imported!
    // 300+ lines of duplicate code
}
```

**Example of What TO Do:**
```javascript
// GOOD: Import and use the function from WallComponents
import(path : "1a352bc5f15cd57be34e8ae2", version : "d709f83721fcbc8242cc7caf"); // LBEASTWallComponents

// ... later in the file ...
// Just call the imported function - no duplication needed
const result = createComposite(context, id, ...);
```

**Remember:** If a function is imported, it's imported for a reason. Don't duplicate it.

### CRITICAL: Check for Accidental Code Duplication After Every Change

**Why This Matters:**
- AI agents frequently create duplicate code blocks when making edits, especially when:
  - Copying and modifying existing code
  - Creating variations of features
  - Refactoring functions
  - Adding new parameters or logic
- Duplicate code causes:
  - Compiler errors ("Multiple visible overloads with identical signature")
  - Confusion about which version is correct
  - Maintenance nightmares
  - Unexpected behavior when the wrong version executes

**Required Workflow After Every Code Change:**

1. **Before completing any edit, search for duplicates:**
   ```bash
   # Search for duplicate function definitions
   grep -n "function functionName" path/to/file.fs
   
   # Search for duplicate variable declarations
   grep -n "const variableName\|var variableName" path/to/file.fs
   
   # Search for duplicate feature definitions
   grep -n "export const featureName" path/to/file.fs
   ```

2. **Check for these common duplication patterns:**
   - **Duplicate function definitions**: Same function name defined multiple times
   - **Duplicate variable declarations**: Same variable declared in the same scope
   - **Duplicate feature definitions**: Same feature exported multiple times
   - **Duplicate parameter handling**: Same parameter default/validation logic repeated
   - **Duplicate normalization code**: Same normalization/validation logic repeated (e.g., `facingDirection` normalization appearing twice)

3. **Use grep/ripgrep to verify:**
   - Search for function names you just created or modified
   - Search for variable names that might have been duplicated
   - Look for patterns like "if (definition.paramName == undefined)" appearing multiple times

4. **Common duplication scenarios:**
   - When creating a new feature based on an existing one (e.g., `createCenterWallSegment` based on `createCornerWallSegment`)
   - When adding new parameters to existing features
   - When refactoring code into helper functions
   - When copying code blocks to modify them

5. **If duplicates are found:**
   - Remove the duplicate immediately
   - Verify the remaining version is correct
   - Test that the code still compiles
   - Ensure functionality is preserved

**Example of Duplication to Watch For:**
```javascript
// BAD: Duplicate normalization code
if (definition.facingDirection == undefined)
{
    definition.facingDirection = 0 * degree;
}
definition.facingDirection = normalizeFacingDirection(definition.facingDirection);
if (definition.facingDirection == undefined)  // DUPLICATE!
{
    definition.facingDirection = 0 * degree;
}
definition.facingDirection = normalizeFacingDirection(definition.facingDirection);  // DUPLICATE!

// GOOD: Single normalization
if (definition.facingDirection == undefined)
{
    definition.facingDirection = 0 * degree;
}
definition.facingDirection = normalizeFacingDirection(definition.facingDirection);
```

**Tools to Use:**
- `grep` or `ripgrep` to search for duplicate patterns
- Code linters (if available) to catch duplicate declarations
- Manual review of the file after each significant change

**Remember:** Duplication is easy to introduce and hard to spot. Always verify after every edit, especially when:
- Creating new features based on existing ones
- Copying code blocks
- Refactoring functions
- Adding new parameters or logic

---

## CRITICAL: File Change Reporting

### ALWAYS Report File Count After OnShape Script Changes

**CRITICAL WORKFLOW REQUIREMENT:** When making changes to any OnShape FeatureScript file that has at least one `import` statement, you MUST inform the user at the end of your response about how many files were changed.

**Why This Matters:**
- OnShape requires manual re-import of each changed file
- The user needs to know exactly which files to re-import
- Files with imports may have dependencies that also need updating
- This prevents confusion and ensures all changes are properly applied

**Required Format:**
At the end of your response, after completing any OnShape script changes, include a statement like:
```
**Files Changed:** [X] file(s) - [list file names]
```

**Example:**
```
**Files Changed:** 1 file - LBEASTWallFrameCreator.fs
```

or

```
**Files Changed:** 2 files - LBEASTWallComponents.fs, LBEASTWallUtil.fs
```

**When to Report:**
- After ANY edit to a `.fs` file that contains `import` statements
- Even if you only changed one file, report it
- If you changed multiple files, list all of them
- Include the count and the file names

**What Counts as a Change:**
- Any modification to a `.fs` file (additions, deletions, modifications)
- Even small changes require re-import
- File creation counts as a change
- File deletion should also be reported

**Remember:** The user manually imports each file to OnShape, so they need to know exactly what changed. Always report file changes at the end of your response.


