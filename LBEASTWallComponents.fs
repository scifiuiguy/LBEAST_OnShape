// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// LBEAST Wall Components FeatureScript
// 
// Component builder functions for wall frame creation.

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");
import(path : "onshape/std/debug.fs", version : "2384.0");
import(path : "onshape/std/moveFace.fs", version : "2384.0");
import(path : "onshape/std/manipulator.fs", version : "2384.0");
import(path : "1a352bc5f15cd57be34e8ae2", version : "d709f83721fcbc8242cc7caf"); // LBEASTWallUtil


// -------------------------
// Composite creation
// -------------------------

export function createComposite(context is Context, baseId is Id,
    tubeWidth is ValueWithUnits, tubeWallThickness is ValueWithUnits,
    frameDepth is ValueWithUnits, segmentHeight is ValueWithUnits, segmentLength is ValueWithUnits,
    endX is ValueWithUnits, offset is Vector, facingDirection is ValueWithUnits) returns map
{
    const innerWidth = tubeWidth - 2 * tubeWallThickness;
    const halfTube = tubeWidth / 2;
    const halfInner = innerWidth / 2;
    const zero = 0 * inch;
    const localOrigin = vector(zero, zero, zero); // Build at origin
    
    // Maintain indexed array of all created objects
    var createdObjects = {};
    createdObjects["flatBars"] = []; // Array of all flat bars with their metadata
    
    // Create first upper frame piece at origin
    // segmentHeight is used as the depth (Y dimension) of the joiner before rotation
    // After rotation, frameDepth becomes the height (Z)
    const piece1FlatBars = createUpperFramePiece(context, baseId + "piece1",
        tubeWidth, tubeWallThickness, innerWidth, halfTube, halfInner,
        frameDepth, segmentHeight,
        endX, localOrigin);
    
    // Store piece1 flat bars with metadata
    createdObjects["flatBars"] = append(createdObjects["flatBars"], {
        "pieceId" : "piece1",
        "zOffset" : 0 * inch,
        "backFlatBar" : piece1FlatBars["backFlatBar"],
        "frontFlatBar" : piece1FlatBars["frontFlatBar"]
    });
    
    // Create second upper frame piece, translated upward by segment height (height of one piece)
    const secondPieceOffset = localOrigin + vector(zero, zero, segmentHeight);
    const piece2FlatBars = createUpperFramePiece(context, baseId + "piece2",
        tubeWidth, tubeWallThickness, innerWidth, halfTube, halfInner,
        frameDepth, segmentHeight,
        endX, secondPieceOffset);
    
    // Store piece2 flat bars with metadata
    createdObjects["flatBars"] = append(createdObjects["flatBars"], {
        "pieceId" : "piece2",
        "zOffset" : segmentHeight,
        "backFlatBar" : piece2FlatBars["backFlatBar"],
        "frontFlatBar" : piece2FlatBars["frontFlatBar"]
    });
    
    // Create a horizontal rectangular tube on top of the frame stack
    // Dimensions: tubeWidth x 2 wide, tubeWidth tall
    // Position it at the top of the second frame piece
    // Top pieces should be at segmentHeight * 2 + tubeWidth
    const topOfSecondPieceZ = localOrigin[2] + segmentHeight * 2 + tubeWidth;
    const rectTubeWidth = tubeWidth * 2;
    const rectTubeHeight = tubeWidth;
    const rectTubeLength = frameDepth; // Match the frame depth
    const halfRectWidth = rectTubeWidth / 2;
    const halfRectHeight = rectTubeHeight / 2;
    const innerRectWidth = rectTubeWidth - 2 * tubeWallThickness;
    const innerRectHeight = rectTubeHeight - 2 * tubeWallThickness;
    const halfInnerRectWidth = innerRectWidth / 2;
    const halfInnerRectHeight = innerRectHeight / 2;
    
    // Create rectangular tube horizontally along Y axis (front to back)
    // Build at origin - no Y offset
    const rectTubeCenterY = zero - 0.5 * tubeWidth;
    const rectTubeZ = topOfSecondPieceZ + 0.5 * tubeWidth;
    const rectTubeStart = localOrigin + vector(endX, rectTubeCenterY, rectTubeZ);
    const rectTubeEnd = localOrigin + vector(endX, rectTubeCenterY + rectTubeLength, rectTubeZ);
    const rectTubeDirection = normalize(rectTubeEnd - rectTubeStart);
    
    // Create outer rectangle sketch (tubeWidth x tubeWidth*2 cross-section)
    const rectTubeOuterSketchId = baseId + "rectTubeOuterSketch";
    const rectTubeOuterSketch = newSketchOnPlane(context, rectTubeOuterSketchId, {
        "sketchPlane" : plane(rectTubeStart, rectTubeDirection)
    });
    skRectangle(rectTubeOuterSketch, "outerRect", {
        "firstCorner" : vector(-halfRectWidth, -halfRectHeight),
        "secondCorner" : vector(halfRectWidth, halfRectHeight)
    });
    skSolve(rectTubeOuterSketch);
    
    const rectTubeOuterRegions = qSketchRegion(rectTubeOuterSketchId);
    opExtrude(context, baseId + "rectTubeOuter", {
        "entities" : rectTubeOuterRegions,
        "direction" : rectTubeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : rectTubeLength,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Create inner rectangle sketch for hollow tube
    const rectTubeInnerSketchId = baseId + "rectTubeInnerSketch";
    const rectTubeInnerSketch = newSketchOnPlane(context, rectTubeInnerSketchId, {
        "sketchPlane" : plane(rectTubeStart, rectTubeDirection)
    });
    skRectangle(rectTubeInnerSketch, "innerRect", {
        "firstCorner" : vector(-halfInnerRectWidth, -halfInnerRectHeight),
        "secondCorner" : vector(halfInnerRectWidth, halfInnerRectHeight)
    });
    skSolve(rectTubeInnerSketch);
    
    const rectTubeInnerRegions = qSketchRegion(rectTubeInnerSketchId);
    opExtrude(context, baseId + "rectTubeInner", {
        "entities" : rectTubeInnerRegions,
        "direction" : rectTubeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : rectTubeLength,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Subtract inner from outer to create hollow rectangular tube
    opBoolean(context, baseId + "rectTubeSubtract", {
        "tools" : qBodyType(qCreatedBy(baseId + "rectTubeInner", EntityType.BODY), BodyType.SOLID),
        "operationType" : BooleanOperationType.SUBTRACTION,
        "targets" : qBodyType(qCreatedBy(baseId + "rectTubeOuter", EntityType.BODY), BodyType.SOLID)
    });
    
    // Apply offset to rectangular tube: +0.5 on Y, -0.5 on Z
    const rectTubeQuery = qBodyType(qCreatedBy(baseId + "rectTubeOuter", EntityType.BODY), BodyType.SOLID);
    const rectTubeOffsetTransform = transform(vector(zero, 0.5 * inch, -0.5 * inch));
    opTransform(context, baseId + "rectTubeOffset", {
        "bodies" : rectTubeQuery,
        "transform" : rectTubeOffsetTransform
    });
    
    // Create a square tube oriented horizontally along X axis (orthogonal to rectangular tube)
    // Length is segmentLength, top face aligned with top face of rectangular tube
    // One end butts up to the broad side upper corner of the rectangular tube
    const rectTubeTopZ = rectTubeZ + halfRectHeight; // Top face of rectangular tube
    const squareTubeTopZ = rectTubeTopZ; // Align top faces
    const squareTubeCenterZ = squareTubeTopZ - halfTube; // Center Z of square tube
    const squareTubeLength = segmentLength;
    
    // Broad side upper corner of rectangular tube:
    // - X edge: endX + halfRectWidth = endX + tubeWidth (outer edge)
    // - Y: rectTubeCenterY (start end)
    // - Z: rectTubeTopZ (top face)
    // Apply offsets: -0.5*tubeWidth on X, +0.5*tubeWidth on Y, +0.5*tubeWidth on Z
    const cornerX = endX + halfRectWidth - 0.5 * tubeWidth;
    const cornerY = rectTubeCenterY + 0.5 * tubeWidth;
    const cornerZ = squareTubeCenterZ + 0.5 * tubeWidth;
    
    // Square tube runs along X axis, starting at the corner
    const squareTubeStart = localOrigin + vector(cornerX, cornerY, cornerZ);
    const squareTubeEnd = localOrigin + vector(cornerX + squareTubeLength, cornerY, cornerZ);
    createTube(context, baseId + "squareTube",
        squareTubeStart,
        squareTubeEnd,
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    // Duplicate the square tube and translate it along Y by frameDepth - tubeWidth
    const duplicateYOffset = frameDepth - tubeWidth;
    const duplicateTubeStart = localOrigin + vector(cornerX, cornerY + duplicateYOffset, cornerZ);
    const duplicateTubeEnd = localOrigin + vector(cornerX + squareTubeLength, cornerY + duplicateYOffset, cornerZ);
    createTube(context, baseId + "squareTubeDuplicate",
        duplicateTubeStart,
        duplicateTubeEnd,
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    // Apply offset to the two square tubes: +0.5 on Y, -0.5 on Z
    const squareTubeQuery = qBodyType(qCreatedBy(baseId + "squareTube" + "outer", EntityType.BODY), BodyType.SOLID);
    const squareTubeDuplicateQuery = qBodyType(qCreatedBy(baseId + "squareTubeDuplicate" + "outer", EntityType.BODY), BodyType.SOLID);
    const squareTubeOffsetTransform = transform(vector(zero, 0.5 * inch, -0.5 * inch));
    opTransform(context, baseId + "squareTubeOffset", {
        "bodies" : qUnion([squareTubeQuery, squareTubeDuplicateQuery]),
        "transform" : squareTubeOffsetTransform
    });
    
    // Delete the bottom flat bar (frontFlatBar) from the second frame piece
    // This is the third flat bar (the lower one of the duplicated pair)
    // It will be replaced by the parallel tube
    const secondPieceFrontFlatBar = qBodyType(qCreatedBy(baseId + "piece2" + "joiner" + "frontFlatBar", EntityType.BODY), BodyType.SOLID);
    opDeleteBodies(context, baseId + "deleteSecondFrontFlatBar", {
        "entities" : secondPieceFrontFlatBar
    });
    
    // Extend the two tubes from the second frame piece downward by 0.125" to fill the gap
    const extendDistance = 0.125 * inch;
    const flatBarThicknessValue = 0.125 * inch;
    const tubeTranslation = flatBarThicknessValue;
    const tubeStartY = halfTube + tubeTranslation;
    
    // Second piece bottom is at segmentHeight; add tubeWidth to align extensions flush
    const secondPieceBottomZ = localOrigin[2] + segmentHeight + tubeWidth - tubeStartY + 0.25 * inch;
    
    // Top tube Y position after rotation: frameDepth - tubeWidth
    // Build at origin - no Y offset
    const topTubeY = frameDepth - tubeWidth;
    // Bottom tube Y position after rotation: 0
    const bottomTubeY = zero;
    
    // Create extensions at the calculated positions
    // Extend slightly upward to ensure overlap with the tubes for union
    const overlapDistance = 0.01 * inch; // Small overlap to ensure union works
    const topTubeExtensionStart = localOrigin + vector(endX, topTubeY, secondPieceBottomZ + overlapDistance);
    const topTubeExtensionEnd = topTubeExtensionStart + vector(zero, zero, -extendDistance - overlapDistance);
    createTube(context, baseId + "extendTopTubeDownward",
        topTubeExtensionStart,
        topTubeExtensionEnd,
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    const bottomTubeExtensionStart = localOrigin + vector(endX, bottomTubeY, secondPieceBottomZ + overlapDistance);
    const bottomTubeExtensionEnd = bottomTubeExtensionStart + vector(zero, zero, -extendDistance - overlapDistance);
    createTube(context, baseId + "extendBottomTubeDownward",
        bottomTubeExtensionStart,
        bottomTubeExtensionEnd,
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    // Union the extensions with the original tubes
    const secondPieceTopTubeQuery = qBodyType(qCreatedBy(baseId + "piece2" + "joiner" + "topY" + "outer", EntityType.BODY), BodyType.SOLID);
    const secondPieceBottomTubeQuery = qBodyType(qCreatedBy(baseId + "piece2" + "joiner" + "bottomY" + "outer", EntityType.BODY), BodyType.SOLID);
    const topTubeExtensionQuery = qBodyType(qCreatedBy(baseId + "extendTopTubeDownward" + "outer", EntityType.BODY), BodyType.SOLID);
    const bottomTubeExtensionQuery = qBodyType(qCreatedBy(baseId + "extendBottomTubeDownward" + "outer", EntityType.BODY), BodyType.SOLID);
    
    // Apply offset to extensions: +0.5 on Y, -0.5 on Z (before swapping)
    const extensionOffsetTransform = transform(vector(zero, 0.5 * inch, -0.5 * inch));
    opTransform(context, baseId + "extensionOffset", {
        "bodies" : qUnion([topTubeExtensionQuery, bottomTubeExtensionQuery]),
        "transform" : extensionOffsetTransform
    });
    
    // Swap extensions to match correct tubes
    const secondPieceTopTube = secondPieceTopTubeQuery;
    const secondPieceBottomTube = secondPieceBottomTubeQuery;
    const topTubeExtension = bottomTubeExtensionQuery; // Swap: use bottom extension for top tube
    const bottomTubeExtension = topTubeExtensionQuery; // Swap: use top extension for bottom tube
    
    // Union operations
    const topTubeCount = size(evaluateQuery(context, secondPieceTopTube));
    const bottomTubeCount = size(evaluateQuery(context, secondPieceBottomTube));
    const topExtensionCount = size(evaluateQuery(context, topTubeExtension));
    const bottomExtensionCount = size(evaluateQuery(context, bottomTubeExtension));
    
    if (topTubeCount > 0 && topExtensionCount > 0)
    {
        opBoolean(context, baseId + "unionTopTube", {
            "tools" : qUnion([secondPieceTopTube, topTubeExtension]),
            "operationType" : BooleanOperationType.UNION
        });
    }
    
    if (bottomTubeCount > 0 && bottomExtensionCount > 0)
    {
        opBoolean(context, baseId + "unionBottomTube", {
            "tools" : qUnion([secondPieceBottomTube, bottomTubeExtension]),
            "operationType" : BooleanOperationType.UNION
        });
    }
    
    // Create a square tube parallel to the flat bar, matching its length
    // After rotation, flat bars are horizontal along Y axis (front to back)
    // The flat bar length is frameDepth (horizontal Y dimension after rotation)
    // Shorten by tubeWidth x 2 and move along its length by tubeWidth
    const flatBarLength = frameDepth;
    const tubeX = 0.5 * tubeWidth;
    const tubeY = zero; // Build at origin - no Y offset
    // First parallel tube (purple) should be at segmentHeight
    const tubeZ = segmentHeight + tubeWidth;
    const shortenedLength = flatBarLength - (tubeWidth * 2);
    const tubeStart = localOrigin + vector(tubeX, tubeY + tubeWidth, tubeZ); // Move along Y by tubeWidth
    const tubeEnd = localOrigin + vector(tubeX, tubeY + tubeWidth + shortenedLength, tubeZ);
    createTube(context, baseId + "parallelTube",
        tubeStart,
        tubeEnd,
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    // Apply offset to parallel tube: -0.5 on Z
    const parallelTubeQuery = qBodyType(qCreatedBy(baseId + "parallelTube" + "outer", EntityType.BODY), BodyType.SOLID);
    const parallelTubeOffsetTransform = transform(vector(zero, zero, -0.5 * inch));
    opTransform(context, baseId + "parallelTubeOffset", {
        "bodies" : parallelTubeQuery,
        "transform" : parallelTubeOffsetTransform
    });
    
    // Duplicate the parallel tube and move it up another segmentHeight
    // Position at segmentHeight * 2: Z should be segmentHeight * 2 + tubeWidth * 2
    // X offset: segmentLength (broad-side length of the segment)
    const parallelTubeDuplicateZ = segmentHeight * 2 + tubeWidth * 2;
    const parallelTubeDuplicateStart = localOrigin + vector(tubeX + segmentLength, tubeY + tubeWidth, parallelTubeDuplicateZ);
    const parallelTubeDuplicateEnd = localOrigin + vector(tubeX + segmentLength, tubeY + tubeWidth + shortenedLength, parallelTubeDuplicateZ);
    createTube(context, baseId + "parallelTubeDuplicate",
        parallelTubeDuplicateStart,
        parallelTubeDuplicateEnd,
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    // Apply offset to parallel tube duplicate: -0.5 on Z
    const parallelTubeDuplicateQuery = qBodyType(qCreatedBy(baseId + "parallelTubeDuplicate" + "outer", EntityType.BODY), BodyType.SOLID);
    const parallelTubeDuplicateOffsetTransform = transform(vector(zero, zero, -0.5 * inch));
    opTransform(context, baseId + "parallelTubeDuplicateOffset", {
        "bodies" : parallelTubeDuplicateQuery,
        "transform" : parallelTubeDuplicateOffsetTransform
    });
    
    // Query all bodies created by this composite
    const allCompositeBodies = qBodyType(qCreatedBy(baseId, EntityType.BODY), BodyType.SOLID);
    
    // Calculate transform: rotate around Z axis by facingDirection, then translate by offset
    var finalTransform = transform(offset); // Default to translation only
    if (abs(facingDirection) < 1e-6 * degree)
    {
        // No rotation needed
        finalTransform = transform(offset);
    }
    else
    {
        // Create rotation around Z axis through origin
        const zAxisLine = line(localOrigin, vector(0, 0, 1)); // Z axis
        const rotationTransform = rotationAround(zAxisLine, facingDirection);
        const translationTransform = transform(offset);
        // Apply rotation first, then translation
        finalTransform = translationTransform * rotationTransform;
    }
    
    // Apply transform to all composite bodies
    opTransform(context, baseId + "compositeTransform", {
        "bodies" : allCompositeBodies,
        "transform" : finalTransform
    });
    
    // Return the indexed object references
    return createdObjects;
}

// -------------------------
// Upper frame piece creation
// -------------------------

export function createUpperFramePiece(context is Context, baseId is Id,
    tubeWidth is ValueWithUnits, tubeWallThickness is ValueWithUnits, innerWidth is ValueWithUnits,
    halfTube is ValueWithUnits, halfInner is ValueWithUnits,
    frameDepth is ValueWithUnits, segmentHeight is ValueWithUnits,
    endX is ValueWithUnits, baseOffset is Vector) returns map
{
    const zero = 0 * inch;
    
    // Create the joiner structure at the base position
    // Note: After 90° rotation around X, Y and Z swap:
    // - frameDepth (Y) becomes the vertical height (Z) after rotation
    // - segmentHeight (Z) becomes the horizontal depth (Y) after rotation
    // So we swap the parameters: pass segmentHeight as depth and frameDepth as height
    // isFooterJoiner = false for upper frame pieces
    const joinerFlatBars = createEndFaceJoiner(context, baseId + "joiner",
        tubeWidth, tubeWallThickness, innerWidth,
        halfTube, halfInner,
        segmentHeight, frameDepth, // Swapped: segmentHeight becomes depth (Y), frameDepth becomes height (Z)
        endX, baseOffset, false); // false = not a footer joiner
    
    // Query the joiner body (all bodies created by the joiner)
    const joinerBodies = qBodyType(qCreatedBy(baseId + "joiner", EntityType.BODY), BodyType.SOLID);
    
    // Calculate rotation center (at the base position where joiner was created)
    const rotationCenter = baseOffset + vector(endX, zero, zero);
    
    // Create rotation: 90 degrees around X axis
    const xAxisLine = line(rotationCenter, vector(1, 0, 0)); // Line along X axis
    const rotationTransform = rotationAround(xAxisLine, 90 * degree);
    
    // After rotation, Z becomes Y, so we need to translate by +frameDepth along Y to correct for the offset
    // The rotation introduces a -frameDepth offset on Y, so we add +frameDepth to correct it back to origin
    // Also need to add back the 0.5*tubeWidth offset that was used during creation (endX = halfTube)
    // Apply translation AFTER rotation so it's in the rotated coordinate system
    const translationVector = vector(zero, frameDepth - halfTube, -halfTube);
    const translationTransform = transform(translationVector);
    
    // Combine transforms: rotation first, then translation (translation in rotated coordinate system)
    const combinedTransform = translationTransform * rotationTransform;
    
    // Apply combined transform to frame piece
    opTransform(context, baseId + "framePieceTransform", {
        "bodies" : joinerBodies,
        "transform" : combinedTransform
    });
    
    // The flat bars are already transformed by createEndFaceJoiner, and now they're transformed again
    // by framePieceTransform. The queries in joinerFlatBars still work because they reference the
    // bodies after the first transform, and framePieceTransform applies to all joiner bodies.
    // Return the flat bar references
    return joinerFlatBars;
}

// -------------------------
// End face joiner creation
// -------------------------

export function createEndFaceJoiner(context is Context, baseId is Id,
    tubeWidth is ValueWithUnits, tubeWallThickness is ValueWithUnits, innerWidth is ValueWithUnits,
    halfTube is ValueWithUnits, halfInner is ValueWithUnits,
    frameDepth is ValueWithUnits, frameHeight is ValueWithUnits,
    endX is ValueWithUnits, baseOffset is Vector, isFooterJoiner is boolean) returns map
{
    const zero = 0 * inch;
    
    // Calculate horizontal Y-direction tube positions for the end face
    // Each end face joiner has 2 horizontal tubes (Y-direction) that span the depth (front to back)
    // They connect the front and back broad side faces
    // Tubes are shortened by 0.25" (total thickness of both flat bars) and translated by 0.125" to sit between them
    
    const flatBarThicknessValue = 0.125 * inch; // Each flat bar is 1/8" thick
    const totalFlatBarThickness = 2 * flatBarThicknessValue; // Both flat bars = 0.25"
    // Shorten tubes by 2*tubeWidth to match upper segment depth (indices 0-3) - only for footer joiners
    var tubeLength = frameDepth - totalFlatBarThickness;
    if (isFooterJoiner)
    {
        tubeLength = tubeLength - 2 * tubeWidth; // Shorten by 2*tubeWidth for footer joiners only
    }
    const tubeTranslation = flatBarThicknessValue; // Translate by 0.125" (half of one flat bar thickness)
    const tubeStartY = halfTube + tubeTranslation; // Start position translated by 0.125"
    
    // Top tube: at Z = frameHeight - tubeWidth (matching top horizontal tube position)
    const topZ = frameHeight - tubeWidth;
    createTube(context, baseId + "topY",
        baseOffset + vector(endX, tubeStartY, topZ),
        baseOffset + vector(endX, tubeStartY + tubeLength, topZ),
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    // Bottom tube: at Z = 0 (matching bottom horizontal tube position)
    const bottomZ = zero;
    createTube(context, baseId + "bottomY",
        baseOffset + vector(endX, tubeStartY, bottomZ),
        baseOffset + vector(endX, tubeStartY + tubeLength, bottomZ),
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    // Create vertical flat bar at the end of the joiner
    // Flat bar: 0.125" thick, same width as tube, spans full height
    // Note: flatBarThicknessValue is already declared above
    const halfFlatBar = flatBarThicknessValue / 2;
    
    // Create both flat bars: one at back end, one at front end retreating by 1/8"
    // For footer joiners, adjust both flat bars back (negative Y) by 2*tubeWidth to align with new depth
    var backFlatBarY = halfTube + frameDepth; // Position at back end of joiner tubes
    const frontEndOffset = 1/8 * inch; // 1/8" = 0.125"
    var frontFlatBarY = halfTube + frontEndOffset; // Original position (no adjustment - just remove any incorrect additions)
    if (isFooterJoiner)
    {
        // Move back flat bar back (negative Y) by 2*tubeWidth to align with shortened tubes and adjusted back face
        backFlatBarY = backFlatBarY - 2 * tubeWidth;
        // Front flat bar stays at original position (halfTube + frontEndOffset) - no adjustment needed
    }
    
    // Create first flat bar at back end
    const backFlatBarPosition = baseOffset + vector(endX, backFlatBarY, zero);
    const backFlatBarSketchPlane = plane(backFlatBarPosition, vector(1, 0, 0)); // Normal points in X direction
    
    const backFlatBarSketchId = baseId + "backFlatBarSketch";
    const backFlatBarSketch = newSketchOnPlane(context, backFlatBarSketchId, {
        "sketchPlane" : backFlatBarSketchPlane
    });
    skRectangle(backFlatBarSketch, "backFlatBarRect", {
        "firstCorner" : vector(-halfTube, zero),
        "secondCorner" : vector(halfTube, frameHeight)
    });
    skSolve(backFlatBarSketch);
    
    const backFlatBarRegions = qSketchRegion(backFlatBarSketchId);
    const backFlatBarExtrudeStart = backFlatBarPosition;
    const backFlatBarExtrudeEnd = backFlatBarPosition + vector(-flatBarThicknessValue, zero, zero);
    const backFlatBarExtrudeDirection = normalize(backFlatBarExtrudeEnd - backFlatBarExtrudeStart);
    
    opExtrude(context, baseId + "backFlatBar", {
        "entities" : backFlatBarRegions,
        "direction" : backFlatBarExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : flatBarThicknessValue,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Create second flat bar at front end
    const frontFlatBarPosition = baseOffset + vector(endX, frontFlatBarY, zero);
    const frontFlatBarSketchPlane = plane(frontFlatBarPosition, vector(1, 0, 0)); // Normal points in X direction
    
    const frontFlatBarSketchId = baseId + "frontFlatBarSketch";
    const frontFlatBarSketch = newSketchOnPlane(context, frontFlatBarSketchId, {
        "sketchPlane" : frontFlatBarSketchPlane
    });
    skRectangle(frontFlatBarSketch, "frontFlatBarRect", {
        "firstCorner" : vector(-halfTube, zero),
        "secondCorner" : vector(halfTube, frameHeight)
    });
    skSolve(frontFlatBarSketch);
    
    const frontFlatBarRegions = qSketchRegion(frontFlatBarSketchId);
    const frontFlatBarExtrudeStart = frontFlatBarPosition;
    const frontFlatBarExtrudeEnd = frontFlatBarPosition + vector(-flatBarThicknessValue, zero, zero);
    const frontFlatBarExtrudeDirection = normalize(frontFlatBarExtrudeEnd - frontFlatBarExtrudeStart);
    
    opExtrude(context, baseId + "frontFlatBar", {
        "entities" : frontFlatBarRegions,
        "direction" : frontFlatBarExtrudeDirection,
        "endBound" : BoundingType.BLIND,
        "endDepth" : flatBarThicknessValue,
        "operationType" : NewBodyOperationType.NEW
    });
    
    // Query both flat bar bodies
    const backFlatBarBody = qBodyType(qCreatedBy(baseId + "backFlatBar", EntityType.BODY), BodyType.SOLID);
    const frontFlatBarBody = qBodyType(qCreatedBy(baseId + "frontFlatBar", EntityType.BODY), BodyType.SOLID);
    
    // Apply rotation: 90 degrees around Z axis (yaw) and translate down along Z by half a tube width
    // Create a line along Z axis through the rotation center
    const rotationCenter = backFlatBarPosition;
    const zAxisLine = line(rotationCenter, vector(0, 0, 1)); // Line along Z axis
    const rotationTransform = rotationAround(zAxisLine, 90 * degree);
    
    // Create translation: translate down along Z by half a tube width
    const translationVector = vector(zero, zero, -halfTube);
    const translationTransform = transform(translationVector);
    
    // Combine transforms: rotation first, then translation
    const combinedTransform = rotationTransform * translationTransform;
    
    // Apply combined transform to back flat bar
    opTransform(context, baseId + "backFlatBarTransform", {
        "bodies" : backFlatBarBody,
        "transform" : combinedTransform
    });
    
    // Apply same transform to front flat bar (rotation center is at front position)
    const frontRotationCenter = frontFlatBarPosition;
    const frontZAxisLine = line(frontRotationCenter, vector(0, 0, 1));
    const frontRotationTransform = rotationAround(frontZAxisLine, 90 * degree);
    const frontCombinedTransform = frontRotationTransform * translationTransform;
    
    opTransform(context, baseId + "frontFlatBarTransform", {
        "bodies" : frontFlatBarBody,
        "transform" : frontCombinedTransform
    });
    
    // Get references to the transformed flat bar bodies
    // Query the bodies after transformation to get their final state
    const backFlatBarBodyAfterTransform = qBodyType(qCreatedBy(baseId + "backFlatBarTransform", EntityType.BODY), BodyType.SOLID);
    const frontFlatBarBodyAfterTransform = qBodyType(qCreatedBy(baseId + "frontFlatBarTransform", EntityType.BODY), BodyType.SOLID);
    
    // Clean up sketches
    try
    {
        opDeleteBodies(context, baseId + "deleteBackFlatBarSketch", {
            "entities" : qCreatedBy(backFlatBarSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    try
    {
        opDeleteBodies(context, baseId + "deleteFrontFlatBarSketch", {
            "entities" : qCreatedBy(frontFlatBarSketchId, EntityType.BODY)
        });
    }
    catch
    {
        // Sketch may not be deletable - this is okay
    }
    
    // Return references to the flat bars after transformation
    return {
        "backFlatBar" : backFlatBarBodyAfterTransform,
        "frontFlatBar" : frontFlatBarBodyAfterTransform
    };
}

// -------------------------
// Broad side face creation
// -------------------------

export function createBroadSideFace(context is Context, baseId is Id,
    tubeWidth is ValueWithUnits, tubeWallThickness is ValueWithUnits, innerWidth is ValueWithUnits,
    halfTube is ValueWithUnits, halfInner is ValueWithUnits,
    frameWidth is ValueWithUnits, frameHeight is ValueWithUnits,
    baseOffset is Vector)
{
    const zero = 0 * inch;
    
    // Bottom horizontal tube (along X axis at Y=0, Z=0)
    const bottomTubeId = baseId + "bottomX";
    createTube(context, bottomTubeId,
        baseOffset + vector(zero, zero, zero),
        baseOffset + vector(frameWidth, zero, zero),
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    // Top horizontal tube (along X axis at Y=0, elevated to topZ)
    const topZ = frameHeight - tubeWidth;
    const topTubeId = baseId + "topBottomX";
    createTube(context, topTubeId,
        baseOffset + vector(zero, zero, topZ),
        baseOffset + vector(frameWidth, zero, topZ),
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    // Calculate post positions
    const postStartZ = tubeWidth - halfTube; // Start above bottom frame, adjusted down
    const postEndZ = frameHeight - tubeWidth - halfTube; // End at top frame, adjusted down
    const postOffsetX = halfTube; // Offset to align with corner
    const postOffsetY = zero; // Front face is at Y=0
    
    // Left vertical post (front-left corner)
    createTube(context, baseId + "post1",
        baseOffset + vector(postOffsetX, postOffsetY, postStartZ),
        baseOffset + vector(postOffsetX, postOffsetY, postEndZ),
        halfTube, halfInner, tubeWidth, tubeWallThickness);
    
    // Right vertical post (front-right corner)
    const post2OffsetX = postOffsetX + frameWidth - tubeWidth;
    createTube(context, baseId + "post2",
        baseOffset + vector(post2OffsetX, postOffsetY, postStartZ),
        baseOffset + vector(post2OffsetX, postOffsetY, postEndZ),
        halfTube, halfInner, tubeWidth, tubeWallThickness);
}

// -------------------------
// Corner segment creation
// -------------------------

export function createCornerSegmentBodies(context is Context, baseId is Id,
    tubeWidth is ValueWithUnits, tubeWallThickness is ValueWithUnits,
    frameDepth is ValueWithUnits, segmentHeight is ValueWithUnits, segmentLength is ValueWithUnits,
    endX is ValueWithUnits, offset is Vector, facingDirection is ValueWithUnits, rotationNormalized is number, addManipulator is boolean)
{
    // Call the composite creation function WITHOUT rotation - we'll apply rotation to ALL bodies at the end
    const createdObjects = createComposite(context, baseId,
        tubeWidth, tubeWallThickness,
        frameDepth, segmentHeight, segmentLength,
        endX, offset, 0 * degree);
    
    // Create facingDirection rotation transform
    var facingDirectionRotationTransform = identityTransform();
    if (abs(facingDirection) > 1e-6 * degree)
    {
        const localOrigin = vector(0 * inch, 0 * inch, 0 * inch);
        const zAxisLine = line(localOrigin, vector(0, 0, 1)); // Z axis through origin
        facingDirectionRotationTransform = rotationAround(zAxisLine, facingDirection);
    }
    
    // Apply facing direction rotation to ALL bodies BEFORE finding axes
    if (abs(facingDirection) > 1e-6 * degree)
    {
        opTransform(context, baseId + "allBodiesRotation", {
            "bodies" : queryAllBodies(baseId),
            "transform" : facingDirectionRotationTransform
        });
        
        println("Applied facing direction rotation to ALL bodies around world origin");
    }
    
    // Re-evaluate all bodies AFTER facing direction rotation
    const allBodiesArray = evaluateQuery(context, queryAllBodies(baseId));
    
    println("Total bodies found (after facing direction rotation): " ~ size(allBodiesArray));
    
    // Identify which index is the rectangular tube (for excluding from second rotation)
    const rectTubeQuery = qBodyType(qCreatedBy(baseId + "rectTubeOuter", EntityType.BODY), BodyType.SOLID);
    const rectTubeArray = evaluateQuery(context, rectTubeQuery);
    var rectTubeIndex = -1;
    if (size(rectTubeArray) > 0)
    {
        const rectTubeBody = rectTubeArray[0];
        // Find which index in allBodiesArray matches this body
        for (var i = 0; i < size(allBodiesArray); i += 1)
        {
            if (allBodiesArray[i] == rectTubeBody)
            {
                rectTubeIndex = i;
                println("Rectangular tube found at index: " ~ rectTubeIndex);
                break;
            }
        }
    }
    
    // Create definition map for rotation helper function (kept for manipulator placement)
    var segmentDefinition = {
        "rotationNormalized" : rotationNormalized,
        "facingDirection" : facingDirection,
        "segmentLength" : segmentLength,
        "tubeWidth" : tubeWidth
    };
    
    // Apply rotation groups 1 and 2 using builder directly
    // First: map 0-0.5 to 0-180 degrees for indices 0-3
    const rotationNormalized12 = segmentDefinition.rotationNormalized == undefined ? 0 : segmentDefinition.rotationNormalized;
    
    const targetBodyIndex = 2;
    const secondTargetBodyIndex = 6;
    
    var rotationAxisLine;
    var rotationAxisFound = false;
    var secondRotationAxisLine;
    var secondRotationAxisFound = false;
    
    // Find first rotation axis (index 2)
    if (size(allBodiesArray) > targetBodyIndex)
    {
        const targetBody = allBodiesArray[targetBodyIndex];
        const axisResult = findRotationAxisFromBody(context, targetBody, targetBodyIndex, true, true); // true = use larger X (broad edge), true = use larger Y (the OTHER long edge = inner for groups 1&2)
        if (axisResult.found && axisResult.edge != undefined)
        {
            rotationAxisLine = axisResult.axisLine;
            rotationAxisFound = true;
            
            // Highlight the edge in red for visualization
            // addDebugEntities(context, axisResult.edge, DebugColor.RED); // Disabled per user request
            
            // Add manipulator if requested (only for center wall segment)
            if (addManipulator)
            {
                const targetBodyBox = evBox3d(context, {
                    "topology" : targetBody
                });
                
                // Position manipulator at 1/2 segmentLength along X, adjusted for facingDirection
                const manipulatorBaseX = segmentDefinition.segmentLength;
                const manipulatorBaseY = 0 * inch;
                const manipulatorBaseZ = targetBodyBox.maxCorner[2] + segmentDefinition.tubeWidth * 0.5;
                const manipulatorBasePosition = vector(manipulatorBaseX / 2, manipulatorBaseY, manipulatorBaseZ);
                
                // Apply facingDirection rotation to the manipulator position
                const manipulatorBase = facingDirectionRotationTransform * manipulatorBasePosition;
                
                const manipulatorDirection = vector(0, 0, 1);
                const manipulatorRange = segmentDefinition.tubeWidth * 10;
                const clampedRotationNormalized = clamp(rotationNormalized12, 0, 2);
                const manipulatorOffset = clampedRotationNormalized * manipulatorRange / 2;
                println("Setting manipulator - rotationNormalized: " ~ clampedRotationNormalized ~ ", offset: " ~ manipulatorOffset ~ ", range: " ~ manipulatorRange);
                
                addManipulators(context, baseId, {
                    "rotationManipulator" : linearManipulator({
                        "base" : manipulatorBase,
                        "direction" : manipulatorDirection,
                        "offset" : manipulatorOffset,
                        "minOffset" : 0 * inch,
                        "maxOffset" : manipulatorRange
                    })
                });
            }
        }
    }
    
    // Find second rotation axis (index 6)
    if (size(allBodiesArray) > secondTargetBodyIndex)
    {
        const secondTargetBody = allBodiesArray[secondTargetBodyIndex];
        const axisResult2 = findRotationAxisFromBody(context, secondTargetBody, secondTargetBodyIndex, true, true); // true = use larger X (broad edge), true = use larger Y (the OTHER long edge = inner for groups 1&2)
        if (axisResult2.found && axisResult2.edge != undefined)
        {
            secondRotationAxisLine = axisResult2.axisLine;
            secondRotationAxisFound = true;
            
            // Highlight the edge in red for visualization
            // addDebugEntities(context, axisResult2.edge, DebugColor.RED); // Disabled per user request
        }
    }
    
    // Use builder pattern for first rotation (group 1)
    if (rotationAxisFound && size(allBodiesArray) >= 4)
    {
        var group1 = newRotationGroupBuilder("G1");
        group1 = withNormalizedRange(group1, 0, 0.5);
        group1 = withMaxAngle(group1, 180 * degree);
        group1 = withClockwise(group1, true);
        group1 = withBodyIndices(group1, [0, 1, 2, 3]);
        group1 = withAxisLine(group1, rotationAxisLine);
        group1 = withOpSuffix(group1, "First");
        
        applyRotationGroupBuilder(context, baseId, allBodiesArray, rotationNormalized12, group1);
    }
    
    // Use builder pattern for second rotation (group 2)
    if (secondRotationAxisFound && size(allBodiesArray) >= 8)
    {
        // Build indices 0-7 plus 10, excluding rectTubeIndex
        var indices2 = [];
        for (var i = 0; i < 8; i += 1)
        {
            if (rectTubeIndex < 0 || i != rectTubeIndex)
            {
                indices2 = append(indices2, i);
            }
        }
        if (size(allBodiesArray) > 10 && (rectTubeIndex < 0 || 10 != rectTubeIndex))
        {
            indices2 = append(indices2, 10);
        }
        
        var group2 = newRotationGroupBuilder("G2");
        group2 = withNormalizedRange(group2, 0.5, 1.0);
        group2 = withMaxAngle(group2, 90 * degree);
        group2 = withClockwise(group2, true);
        group2 = withBodyIndices(group2, indices2);
        group2 = withAxisLine(group2, secondRotationAxisLine);
        group2 = withOpSuffix(group2, "Second");
        
        applyRotationGroupBuilder(context, baseId, allBodiesArray, rotationNormalized12, group2);
    }
}

// -------------------------
// Center segment creation
// -------------------------

export function createCenterSegmentBodies(context is Context, baseId is Id,
    tubeWidth is ValueWithUnits, tubeWallThickness is ValueWithUnits,
    frameDepth is ValueWithUnits, segmentHeight is ValueWithUnits, segmentLength is ValueWithUnits,
    endX is ValueWithUnits, offset is Vector, facingDirection is ValueWithUnits, rotationNormalized is number, addManipulator is boolean)
{
    // Call the composite creation function WITHOUT rotation - we'll apply rotation to ALL bodies at the end
    const createdObjects = createComposite(context, baseId,
        tubeWidth, tubeWallThickness,
        frameDepth, segmentHeight, segmentLength,
        endX, offset, 0 * degree);
    
    // Delete the second purple cross member (parallelTubeDuplicate)
    const secondPurpleCrossMember = qBodyType(qCreatedBy(baseId + "parallelTubeDuplicate" + "outer", EntityType.BODY), BodyType.SOLID);
    opDeleteBodies(context, baseId + "deleteSecondPurpleCrossMember", {
        "entities" : secondPurpleCrossMember
    });
    
    // Duplicate all bodies except the top square tubes, second purple cross member, and rectangular tube,
    // translating the duplicates along X by segmentLength to form the center segment span.
    const originalBodies = qBodyType(qCreatedBy(baseId, EntityType.BODY), BodyType.SOLID);
    const excludeQueries = qUnion([
        qBodyType(qCreatedBy(baseId + "parallelTubeDuplicate" + "outer", EntityType.BODY), BodyType.SOLID),
        qBodyType(qCreatedBy(baseId + "squareTube" + "outer", EntityType.BODY), BodyType.SOLID),
        qBodyType(qCreatedBy(baseId + "squareTubeDuplicate" + "outer", EntityType.BODY), BodyType.SOLID),
        qBodyType(qCreatedBy(baseId + "rectTubeOuter", EntityType.BODY), BodyType.SOLID)
    ]);
    
    const bodiesToDuplicate = qSubtraction(originalBodies, excludeQueries);
    
    // Pattern the bodies along X by segmentLength
    opPattern(context, baseId + "centerSpanCopy", {
        "entities" : bodiesToDuplicate,
        "transforms" : [transform(vector(segmentLength, 0 * inch, 0 * inch))],
        "instanceNames" : ["copy1"]
    });
    
    // Duplicate the 2x tall rectangular tube and translate it across X by segmentLength
    const rectTubeQuery = qBodyType(qCreatedBy(baseId + "rectTubeOuter", EntityType.BODY), BodyType.SOLID);
    opPattern(context, baseId + "duplicateRectTube", {
        "entities" : rectTubeQuery,
        "transforms" : [transform(vector(segmentLength, 0 * inch, 0 * inch))],
        "instanceNames" : ["copy1"]
    });
    
    // Duplicate the second rectangular tube and translate it down by 2x tubeWidth
    const duplicateRectTubeQuery = qBodyType(qCreatedBy(baseId + "duplicateRectTube", EntityType.BODY), BodyType.SOLID);
    opPattern(context, baseId + "duplicateSecondRectTube", {
        "entities" : duplicateRectTubeQuery,
        "transforms" : [transform(vector(0 * inch, 0 * inch, -2 * tubeWidth))],
        "instanceNames" : ["copy1"]
    });
    
    // Get all bodies for dimensional adjustments (same as center wall segment)
    var allBodiesArrayForAdjustments = evaluateQuery(context, queryAllBodies(baseId));
    println("Total bodies found (before adjustments): " ~ size(allBodiesArrayForAdjustments));
    
    // Dimensional adjustments: Move top flat bar down, trim tubes, move groups down
    // Find the top flat bar (highest body in pattern) and move it down by tubeWidth
    const patternBodiesQuery = qBodyType(qCreatedBy(baseId + "centerSpanCopy", EntityType.BODY), BodyType.SOLID);
    const patternBodyArray = evaluateQuery(context, patternBodiesQuery);
    const arraySize = size(patternBodyArray);
    if (arraySize > 0)
    {
        // Find the body with the highest Z position
        var topMostBody = patternBodyArray[0];
        const firstBox = evBox3d(context, {
            "topology" : topMostBody
        });
        var maxZ = firstBox.maxCorner[2];
        
        for (var i = 1; i < arraySize; i += 1)
        {
            const currentBody = patternBodyArray[i];
            const currentBox = evBox3d(context, {
                "topology" : currentBody
            });
            const currentMaxZ = currentBox.maxCorner[2];
            if (currentMaxZ > maxZ)
            {
                maxZ = currentMaxZ;
                topMostBody = currentBody;
            }
        }
        
        // Move the flat bar down by tubeWidth
        opTransform(context, baseId + "moveFlatBarDown", {
            "bodies" : topMostBody,
            "transform" : transform(vector(0 * inch, 0 * inch, -tubeWidth))
        });
        
        // Find the two square tubes directly below the flat bar in the duplicated area
        const flatBarBox = evBox3d(context, {
            "topology" : topMostBody
        });
        const flatBarBottomZ = flatBarBox.minCorner[2];
        
        // Find vertical square tubes in pattern bodies
        var allVerticalSquareTubes = [];
        for (var j = 0; j < arraySize; j += 1)
        {
            const body = patternBodyArray[j];
            if (body == topMostBody)
            {
                continue;
            }
            
            const bodyBox = evBox3d(context, {
                "topology" : body
            });
            
            const xSize = bodyBox.maxCorner[0] - bodyBox.minCorner[0];
            const ySize = bodyBox.maxCorner[1] - bodyBox.minCorner[1];
            const zSize = bodyBox.maxCorner[2] - bodyBox.minCorner[2];
            
            const isSquareCrossSection = (abs(xSize - tubeWidth) < 0.1 * inch) && 
                                         (abs(ySize - tubeWidth) < 0.1 * inch);
            const isVertical = zSize > tubeWidth * 2;
            
            if (isSquareCrossSection && isVertical)
            {
                allVerticalSquareTubes = append(allVerticalSquareTubes, body);
            }
        }
        
        // Find the upper set (two with highest Z positions)
        var upperSquareTubes = [];
        if (size(allVerticalSquareTubes) >= 2)
        {
            var highestZ = -1e10 * inch;
            var secondHighestZ = -1e10 * inch;
            var highestTube;
            var secondHighestTube;
            
            for (var k = 0; k < size(allVerticalSquareTubes); k += 1)
            {
                const tube = allVerticalSquareTubes[k];
                const tubeBox = evBox3d(context, {
                    "topology" : tube
                });
                const tubeTopZ = tubeBox.maxCorner[2];
                
                if (tubeTopZ > highestZ)
                {
                    secondHighestZ = highestZ;
                    secondHighestTube = highestTube;
                    highestZ = tubeTopZ;
                    highestTube = tube;
                }
                else if (tubeTopZ > secondHighestZ)
                {
                    secondHighestZ = tubeTopZ;
                    secondHighestTube = tube;
                }
            }
            
            if (highestTube != undefined)
            {
                upperSquareTubes = append(upperSquareTubes, highestTube);
            }
            if (secondHighestTube != undefined)
            {
                upperSquareTubes = append(upperSquareTubes, secondHighestTube);
            }
        }
        
        // Trim 1x tubeWidth off the top of each upper square tube
        for (var m = 0; m < size(upperSquareTubes); m += 1)
        {
            const tube = upperSquareTubes[m];
            const tubeBox = evBox3d(context, {
                "topology" : tube
            });
            const tubeTopZ = tubeBox.maxCorner[2];
            
            const patternFaces = qCreatedBy(baseId + "centerSpanCopy", EntityType.FACE);
            const patternFaceArray = evaluateQuery(context, patternFaces);
            
            const upVector = vector(0, 0, 1);
            var topFace;
            var maxDot = -1;
            
            for (var n = 0; n < size(patternFaceArray); n += 1)
            {
                const face = patternFaceArray[n];
                const faceBox = evBox3d(context, {
                    "topology" : face
                });
                
                if (abs(faceBox.maxCorner[2] - tubeTopZ) < 0.001 * inch)
                {
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
            }
            
            if (topFace != undefined)
            {
                const topFaceQuery = qUnion([qEntityFilter(topFace, EntityType.FACE)]);
                const moveTransform = transform(vector(0 * inch, 0 * inch, -tubeWidth));
                opMoveFace(context, baseId + "trimGroup3Tube" + toString(m), {
                    "moveFaces" : topFaceQuery,
                    "transform" : moveTransform
                });
                println("Shortened group 3 vertical tube at index " ~ m ~ " by 1x tubeWidth");
            }
        }
    }
    
    // Move index 13 (top flat bar in group 3) down by 1x tubeWidth
    if (size(allBodiesArrayForAdjustments) > 13)
    {
        const index13Body = qBodyType(qEntityFilter(allBodiesArrayForAdjustments[13], EntityType.BODY), BodyType.SOLID);
        const index13MoveTransform = transform(vector(0 * inch, 0 * inch, -tubeWidth));
        opTransform(context, baseId + "moveIndex13Down", {
            "bodies" : index13Body,
            "transform" : index13MoveTransform
        });
        println("Moved index 13 down by 1x tubeWidth");
    }
    
    // Move group 4 (indices 15-18) down by 1x tubeWidth
    const group4Indices = [15, 16, 17, 18];
    var group4BodiesToMove = [];
    for (var g4 = 0; g4 < size(group4Indices); g4 += 1)
    {
        const bodyIndex = group4Indices[g4];
        if (bodyIndex < size(allBodiesArrayForAdjustments))
        {
            group4BodiesToMove = append(group4BodiesToMove, 
                qBodyType(qEntityFilter(allBodiesArrayForAdjustments[bodyIndex], EntityType.BODY), BodyType.SOLID));
        }
    }
    
    if (size(group4BodiesToMove) > 0)
    {
        const group4MoveTransform = transform(vector(0 * inch, 0 * inch, -tubeWidth));
        opTransform(context, baseId + "moveGroup4Down", {
            "bodies" : qUnion(group4BodiesToMove),
            "transform" : group4MoveTransform
        });
        println("Moved group 4 (indices 15-18) down by 1x tubeWidth");
    }
    
    // CRITICAL: Shorten the two horizontal tubes at the top BEFORE rotations
    // The two horizontal tubes at the top of the center segment are at:
    // - startIndex + 1 (first target body)
    // - startIndex + 2 (second target body)
    // 
    // startIndex: The starting array index calculated as 3 positions before the middle
    // of the array (floor(bodiesArraySize / 2) - 3), clamped to minimum 0.
    // This identification was established through color-coding and user confirmation.
    const bodiesArraySizeBeforeRotations = size(allBodiesArrayForAdjustments);
    var startIndexBeforeRotations = floor(bodiesArraySizeBeforeRotations / 2) - 3;
    if (startIndexBeforeRotations < 0)
    {
        startIndexBeforeRotations = 0;
    }
    
    // Shorten the two horizontal tubes at the top using face index 9 (confirmed by user)
    const horizontalIndicesToShorten = [startIndexBeforeRotations + 1, startIndexBeforeRotations + 2];
    const targetFaceIndex = 9; // Confirmed: face index 9 is the end face to shorten
    
    for (var idx = 0; idx < size(horizontalIndicesToShorten); idx += 1)
    {
        const bodyIndex = horizontalIndicesToShorten[idx];
        if (size(allBodiesArrayForAdjustments) > bodyIndex)
        {
            const body = allBodiesArrayForAdjustments[bodyIndex];
            
            // Get all faces on this body - use exact same pattern as vertical tubes
            const allFaces = qOwnedByBody(qBodyType(qEntityFilter(body, EntityType.BODY), BodyType.SOLID), EntityType.FACE);
            const faceArray = evaluateQuery(context, allFaces);
            
            // Get face index 9 (the end face to shorten)
            if (size(faceArray) > targetFaceIndex)
            {
                const endFace = faceArray[targetFaceIndex];
                const endFaceQuery = qUnion([qEntityFilter(endFace, EntityType.FACE)]);
                
                // Get the face plane to determine the move direction
                const facePlane = evPlane(context, {
                    "face" : endFace
                });
                const faceNormal = facePlane.normal;
                
                // Move the face inward (opposite to its normal) by 1x tubeWidth
                // Multiply each component separately to ensure proper units
                const moveDirection = vector(-faceNormal[0] * tubeWidth, -faceNormal[1] * tubeWidth, -faceNormal[2] * tubeWidth);
                const moveTransform = transform(moveDirection);
                
                opMoveFace(context, baseId + "trimHorizontalIndex" + toString(bodyIndex), {
                    "moveFaces" : endFaceQuery,
                    "transform" : moveTransform
                });
                println("Shortened horizontal body at index " ~ bodyIndex ~ " by 1x tubeWidth using face index " ~ targetFaceIndex);
            }
            else
            {
                println("WARNING: Face index " ~ targetFaceIndex ~ " does not exist for body at index " ~ bodyIndex ~ " (face array size: " ~ size(faceArray) ~ ")");
            }
        }
        else
        {
            println("WARNING: Body index " ~ bodyIndex ~ " does not exist (array size: " ~ size(allBodiesArrayForAdjustments) ~ ")");
        }
    }
    
    // CRITICAL: Shorten indices 11 and 12 (vertical tubes in rotation group 3) at the top BEFORE rotations
    // These indices were identified through user confirmation
    const verticalIndicesToShorten = [11, 12];
    const upVector = vector(0, 0, 1);
    
    for (var idx = 0; idx < size(verticalIndicesToShorten); idx += 1)
    {
        const bodyIndex = verticalIndicesToShorten[idx];
        if (size(allBodiesArrayForAdjustments) > bodyIndex)
        {
            const body = allBodiesArrayForAdjustments[bodyIndex];
            const bodyBox = evBox3d(context, {
                "topology" : body
            });
            const bodyTopZ = bodyBox.maxCorner[2];
            
            // Get all faces on this body
            const allFaces = qOwnedByBody(qBodyType(qEntityFilter(body, EntityType.BODY), BodyType.SOLID), EntityType.FACE);
            const faceArray = evaluateQuery(context, allFaces);
            
            // Find the top face using dot product
            var topFace;
            var maxDot = -1;
            
            for (var n = 0; n < size(faceArray); n += 1)
            {
                const face = faceArray[n];
                const faceBox = evBox3d(context, {
                    "topology" : face
                });
                
                // Check if this face is at the top Z coordinate
                if (abs(faceBox.maxCorner[2] - bodyTopZ) < 0.001 * inch)
                {
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
            }
            
            // Move the top face down by 1x tubeWidth
            if (topFace != undefined)
            {
                const topFaceQuery = qUnion([qEntityFilter(topFace, EntityType.FACE)]);
                const moveTransform = transform(vector(0 * inch, 0 * inch, -tubeWidth));
                opMoveFace(context, baseId + "trimIndex" + toString(bodyIndex), {
                    "moveFaces" : topFaceQuery,
                    "transform" : moveTransform
                });
                println("Shortened body at index " ~ bodyIndex ~ " by 1x tubeWidth at the top");
            }
            else
            {
                println("WARNING: Could not find top face for body at index " ~ bodyIndex);
            }
        }
        else
        {
            println("WARNING: Body index " ~ bodyIndex ~ " does not exist (array size: " ~ size(allBodiesArrayForAdjustments) ~ ")");
        }
    }
    
    println("Total bodies found (after adjustments, before facing direction rotation): " ~ size(allBodiesArrayForAdjustments));
    
    // Create facingDirection rotation transform
    var facingDirectionRotationTransform = identityTransform();
    if (abs(facingDirection) > 1e-6 * degree)
    {
        const localOrigin = vector(0 * inch, 0 * inch, 0 * inch);
        const zAxisLine = line(localOrigin, vector(0, 0, 1)); // Z axis through origin
        facingDirectionRotationTransform = rotationAround(zAxisLine, facingDirection);
    }
    
    // Apply facing direction rotation to ALL bodies BEFORE finding axes
    if (abs(facingDirection) > 1e-6 * degree)
    {
        opTransform(context, baseId + "allBodiesRotation", {
            "bodies" : queryAllBodies(baseId),
            "transform" : facingDirectionRotationTransform
        });
        
        println("Applied facing direction rotation to ALL bodies around world origin");
    }
    
    // Re-evaluate all bodies AFTER facing direction rotation
    const allBodiesArray = evaluateQuery(context, queryAllBodies(baseId));
    
    println("Total bodies found (after facing direction rotation): " ~ size(allBodiesArray));
    
    // Identify which index is the rectangular tube
    const rectTubeQuery2 = qBodyType(qCreatedBy(baseId + "rectTubeOuter", EntityType.BODY), BodyType.SOLID);
    const rectTubeArray = evaluateQuery(context, rectTubeQuery2);
    var rectTubeIndex = -1;
    if (size(rectTubeArray) > 0)
    {
        const rectTubeBody = rectTubeArray[0];
        // Find which index in allBodiesArrayForAdjustments matches this body
        for (var i = 0; i < size(allBodiesArrayForAdjustments); i += 1)
        {
            if (allBodiesArrayForAdjustments[i] == rectTubeBody)
            {
                rectTubeIndex = i;
                println("Rectangular tube found at index: " ~ rectTubeIndex);
                break;
            }
        }
    }
    
    // Create definition map for rotation helper function
    var segmentDefinition = {
        "rotationNormalized" : rotationNormalized,
        "facingDirection" : facingDirection,
        "segmentLength" : segmentLength,
        "tubeWidth" : tubeWidth
    };
    
    // Apply rotation groups 1 and 2 using builder directly
    const rotationNormalized12 = segmentDefinition.rotationNormalized == undefined ? 0 : segmentDefinition.rotationNormalized;
    
    const targetBodyIndex = 2;
    const secondTargetBodyIndex = 6;
    
    var rotationAxisLine;
    var rotationAxisFound = false;
    var secondRotationAxisLine;
    var secondRotationAxisFound = false;
    
    // Find first rotation axis (index 2)
    if (size(allBodiesArray) > targetBodyIndex)
    {
        const targetBody = allBodiesArray[targetBodyIndex];
        const axisResult = findRotationAxisFromBody(context, targetBody, targetBodyIndex, true, true); // true = use larger X (broad edge), true = use larger Y (the OTHER long edge = inner for groups 1&2)
        if (axisResult.found && axisResult.edge != undefined)
        {
            rotationAxisLine = axisResult.axisLine;
            rotationAxisFound = true;
            
            // Highlight the edge in red for visualization
            // addDebugEntities(context, axisResult.edge, DebugColor.RED); // Disabled per user request
            
            // Add manipulator if requested (only for center wall segment)
            if (addManipulator)
            {
                const targetBodyBox = evBox3d(context, {
                    "topology" : targetBody
                });
                
                // Position manipulator at 1/2 segmentLength along X, adjusted for facingDirection
                const manipulatorBaseX = segmentDefinition.segmentLength;
                const manipulatorBaseY = 0 * inch;
                const manipulatorBaseZ = targetBodyBox.maxCorner[2] + segmentDefinition.tubeWidth * 0.5;
                const manipulatorBasePosition = vector(manipulatorBaseX / 2, manipulatorBaseY, manipulatorBaseZ);
                
                // Apply facingDirection rotation to the manipulator position
                const manipulatorBase = facingDirectionRotationTransform * manipulatorBasePosition;
                
                const manipulatorDirection = vector(0, 0, 1);
                const manipulatorRange = segmentDefinition.tubeWidth * 10;
                const clampedRotationNormalized = clamp(rotationNormalized12, 0, 2);
                const manipulatorOffset = clampedRotationNormalized * manipulatorRange / 2;
                println("Setting manipulator - rotationNormalized: " ~ clampedRotationNormalized ~ ", offset: " ~ manipulatorOffset ~ ", range: " ~ manipulatorRange);
                
                addManipulators(context, baseId, {
                    "rotationManipulator" : linearManipulator({
                        "base" : manipulatorBase,
                        "direction" : manipulatorDirection,
                        "offset" : manipulatorOffset,
                        "minOffset" : 0 * inch,
                        "maxOffset" : manipulatorRange
                    })
                });
            }
        }
    }
    
    // Find second rotation axis (index 6)
    if (size(allBodiesArray) > secondTargetBodyIndex)
    {
        const secondTargetBody = allBodiesArray[secondTargetBodyIndex];
        const axisResult2 = findRotationAxisFromBody(context, secondTargetBody, secondTargetBodyIndex, true, true); // true = use larger X (broad edge), true = use larger Y (the OTHER long edge = inner for groups 1&2)
        if (axisResult2.found && axisResult2.edge != undefined)
        {
            secondRotationAxisLine = axisResult2.axisLine;
            secondRotationAxisFound = true;
            
            // Highlight the edge in red for visualization
            // addDebugEntities(context, axisResult2.edge, DebugColor.RED); // Disabled per user request
        }
    }
    
    // Use builder pattern for first rotation (group 1)
    if (rotationAxisFound && size(allBodiesArray) >= 4)
    {
        var group1 = newRotationGroupBuilder("G1");
        group1 = withNormalizedRange(group1, 0, 0.5);
        group1 = withMaxAngle(group1, 180 * degree);
        group1 = withClockwise(group1, true);
        group1 = withBodyIndices(group1, [0, 1, 2, 3]);
        group1 = withAxisLine(group1, rotationAxisLine);
        group1 = withOpSuffix(group1, "First");
        
        applyRotationGroupBuilder(context, baseId, allBodiesArray, rotationNormalized12, group1);
    }
    
    // Use builder pattern for second rotation (group 2)
    if (secondRotationAxisFound && size(allBodiesArray) >= 8)
    {
        // Build indices 0-7 plus 10, excluding rectTubeIndex
        var indices2 = [];
        for (var i = 0; i < 8; i += 1)
        {
            if (rectTubeIndex < 0 || i != rectTubeIndex)
            {
                indices2 = append(indices2, i);
            }
        }
        if (size(allBodiesArray) > 10 && (rectTubeIndex < 0 || 10 != rectTubeIndex))
        {
            indices2 = append(indices2, 10);
        }
        
        var group2 = newRotationGroupBuilder("G2");
        group2 = withNormalizedRange(group2, 0.5, 1.0);
        group2 = withMaxAngle(group2, 90 * degree);
        group2 = withClockwise(group2, true);
        group2 = withBodyIndices(group2, indices2);
        group2 = withAxisLine(group2, secondRotationAxisLine);
        group2 = withOpSuffix(group2, "Second");
        
        applyRotationGroupBuilder(context, baseId, allBodiesArray, rotationNormalized12, group2);
    }
    
    // Apply rotation groups 3 and 4 for center segments using builder pattern
    // Group 3: range 1.0-1.5, max 180°, counter-clockwise (false), indices 11-14
    // Group 4: range 1.5-2.0, max 90°, counter-clockwise (false), indices 11-18 (includes group 3's indices)
    // NOTE: We find the axis from the duplicated bodies themselves (which are horizontal flat bars)
    // The duplicated body at index 11 corresponds to original index 0, index 12 to original 1, etc.
    // So index 13 corresponds to original index 2, and index 17 corresponds to original index 6
    const rotationNormalized34 = segmentDefinition.rotationNormalized == undefined ? 0 : segmentDefinition.rotationNormalized;
    
    // Find the duplicated horizontal flat bars that correspond to original indices 2 and 6
    // Original index 2 -> duplicated index 13 (if 8 bodies were duplicated starting at 11)
    // Original index 6 -> duplicated index 17
    const thirdTargetBodyIndex = 13; // Duplicated version of original index 2
    const fourthTargetBodyIndex = 17; // Duplicated version of original index 6
    
    var thirdRotationAxisLine;
    var thirdRotationAxisFound = false;
    var fourthRotationAxisLine;
    var fourthRotationAxisFound = false;
    
    // Find third rotation axis from duplicated horizontal flat bar at index 13 (mirror-side edge)
    if (size(allBodiesArray) > thirdTargetBodyIndex)
    {
        const thirdTargetBody = allBodiesArray[thirdTargetBodyIndex];
        println("Attempting to find rotation axis for group 3 (using duplicated horizontal flat bar at index " ~ thirdTargetBodyIndex ~ ")");
        const axisResult3 = findRotationAxisFromBody(context, thirdTargetBody, thirdTargetBodyIndex, true, false); // true = use larger X (broad edge), false = use smaller Y (mirror side)
        if (axisResult3.found && axisResult3.edge != undefined)
        {
            thirdRotationAxisLine = axisResult3.axisLine;
            thirdRotationAxisFound = true;
            println("Found rotation axis for group 3");
            
            // Highlight the edge in red for visualization
            // addDebugEntities(context, axisResult3.edge, DebugColor.RED); // Disabled per user request
        }
        else
        {
            println("ERROR: Could not find rotation axis for group 3 (body index " ~ thirdTargetBodyIndex ~ ") - found: " ~ axisResult3.found ~ ", edge defined: " ~ (axisResult3.edge != undefined));
        }
    }
    else
    {
        println("ERROR: allBodiesArray size (" ~ size(allBodiesArray) ~ ") is not > " ~ thirdTargetBodyIndex);
    }
    
    // Find fourth rotation axis from duplicated horizontal flat bar at index 17 (mirror-side edge)
    if (size(allBodiesArray) > fourthTargetBodyIndex)
    {
        const fourthTargetBody = allBodiesArray[fourthTargetBodyIndex];
        println("Attempting to find rotation axis for group 4 (using duplicated horizontal flat bar at index " ~ fourthTargetBodyIndex ~ ")");
        const axisResult4 = findRotationAxisFromBody(context, fourthTargetBody, fourthTargetBodyIndex, true, false); // true = use larger X (broad edge), false = use smaller Y (mirror side)
        if (axisResult4.found && axisResult4.edge != undefined)
        {
            fourthRotationAxisLine = axisResult4.axisLine;
            fourthRotationAxisFound = true;
            println("Found rotation axis for group 4");
            
            // Highlight the edge in red for visualization
            // addDebugEntities(context, axisResult4.edge, DebugColor.RED); // Disabled per user request
        }
        else
        {
            println("ERROR: Could not find rotation axis for group 4 (body index " ~ fourthTargetBodyIndex ~ ")");
        }
    }
    else
    {
        println("ERROR: allBodiesArray size (" ~ size(allBodiesArray) ~ ") is not > " ~ fourthTargetBodyIndex);
    }
    
    // Use builder pattern for third rotation (group 3)
    // NOTE: Array only has 12 bodies (0-11), so we can only use index 11 for group 3
    // Group 3 should rotate the duplicated bodies from the center span copy
    // After duplications, indices 11-14 should exist, but if array only has 12, we need to adjust
    println("DEBUG Group 3: thirdRotationAxisFound=" ~ thirdRotationAxisFound ~ ", array size=" ~ size(allBodiesArray) ~ ", need >= 15");
    if (thirdRotationAxisFound && size(allBodiesArray) > 11)
    {
        // Only use indices that actually exist - if array has 12 bodies, only index 11 exists
        var group3Indices = [];
        if (size(allBodiesArray) > 11) group3Indices = append(group3Indices, 11);
        if (size(allBodiesArray) > 12) group3Indices = append(group3Indices, 12);
        if (size(allBodiesArray) > 13) group3Indices = append(group3Indices, 13);
        if (size(allBodiesArray) > 14) group3Indices = append(group3Indices, 14);
        
        println("Applying rotation group 3 with rotationNormalized34 = " ~ rotationNormalized34 ~ ", indices: " ~ toString(group3Indices));
        var group3 = newRotationGroupBuilder("G3");
        group3 = withNormalizedRange(group3, 1.0, 1.5);
        group3 = withMaxAngle(group3, 180 * degree);
        group3 = withClockwise(group3, false); // Counter-clockwise
        group3 = withBodyIndices(group3, group3Indices);
        group3 = withAxisLine(group3, thirdRotationAxisLine);
        group3 = withOpSuffix(group3, "Third");
        
        applyRotationGroupBuilder(context, baseId, allBodiesArray, rotationNormalized34, group3);
        println("Applied rotation group 3 to " ~ size(group3Indices) ~ " bodies");
    }
    else
    {
        println("Skipping rotation group 3 - thirdRotationAxisFound: " ~ thirdRotationAxisFound ~ ", array size: " ~ size(allBodiesArray));
    }
    
    // Use builder pattern for fourth rotation (group 4)
    // Group 4 includes group 3's indices (11-14) plus its own indices (15-18)
    // When group 3 is done rotating, those bodies continue with group 4
    println("DEBUG Group 4: fourthRotationAxisFound=" ~ fourthRotationAxisFound ~ ", array size=" ~ size(allBodiesArray) ~ ", need >= 19");
    if (fourthRotationAxisFound && size(allBodiesArray) > 15)
    {
        // Include group 3's indices (11-14) plus group 4's indices (15-18)
        var group4Indices = [];
        // Add group 3 indices
        if (size(allBodiesArray) > 11) group4Indices = append(group4Indices, 11);
        if (size(allBodiesArray) > 12) group4Indices = append(group4Indices, 12);
        if (size(allBodiesArray) > 13) group4Indices = append(group4Indices, 13);
        if (size(allBodiesArray) > 14) group4Indices = append(group4Indices, 14);
        // Add group 4 indices
        if (size(allBodiesArray) > 15) group4Indices = append(group4Indices, 15);
        if (size(allBodiesArray) > 16) group4Indices = append(group4Indices, 16);
        if (size(allBodiesArray) > 17) group4Indices = append(group4Indices, 17);
        if (size(allBodiesArray) > 18) group4Indices = append(group4Indices, 18);
        
        println("Applying rotation group 4 with rotationNormalized34 = " ~ rotationNormalized34 ~ ", indices: " ~ toString(group4Indices));
        var group4 = newRotationGroupBuilder("G4");
        group4 = withNormalizedRange(group4, 1.5, 2.0);
        group4 = withMaxAngle(group4, 90 * degree); // Changed from 180 to 90 degrees total
        group4 = withClockwise(group4, false); // Counter-clockwise
        group4 = withBodyIndices(group4, group4Indices);
        group4 = withAxisLine(group4, fourthRotationAxisLine);
        group4 = withOpSuffix(group4, "Fourth");
        
        applyRotationGroupBuilder(context, baseId, allBodiesArray, rotationNormalized34, group4);
        println("Applied rotation group 4 to " ~ size(group4Indices) ~ " bodies");
    }
    else
    {
        println("Skipping rotation group 4 - fourthRotationAxisFound: " ~ fourthRotationAxisFound ~ ", array size: " ~ size(allBodiesArray));
    }
    
    // Re-evaluate bodies after all rotations to get current state
    const allBodiesArrayAfterRotations = evaluateQuery(context, queryAllBodies(baseId));
    }

// -------------------------
// Both broad side faces creation
// -------------------------

export function createBothBroadSideFaces(context is Context, baseId is Id,
    tubeWidth is ValueWithUnits, tubeWallThickness is ValueWithUnits, innerWidth is ValueWithUnits,
    halfTube is ValueWithUnits, halfInner is ValueWithUnits,
    frameWidth is ValueWithUnits, frameDepth is ValueWithUnits, frameHeight is ValueWithUnits,
    baseOffset is Vector)
{
    const zero = 0 * inch;
    
    // Front face: at Y=0 (or baseOffset's Y coordinate)
    createBroadSideFace(context, baseId + "front",
        tubeWidth, tubeWallThickness, innerWidth,
        halfTube, halfInner,
        frameWidth, frameHeight,
        baseOffset);
    
    // Back face: offset by frameDepth - tubeWidth in Y direction (subtract 2*tubeWidth from original)
    // The back face is at Y = frameDepth - tubeWidth (adjusted to match upper segment depth)
    const backFaceOffset = baseOffset + vector(zero, frameDepth - tubeWidth, zero);
    createBroadSideFace(context, baseId + "back",
        tubeWidth, tubeWallThickness, innerWidth,
        halfTube, halfInner,
        frameWidth, frameHeight,
        backFaceOffset);
    
    // Create end face joiner for left end (at X=halfTube, matching front-left post position)
    const leftEndX = halfTube;
    createEndFaceJoiner(context, baseId + "leftEndJoiner",
        tubeWidth, tubeWallThickness, innerWidth,
        halfTube, halfInner,
        frameDepth, frameHeight,
        leftEndX, baseOffset, true); // true = footer joiner
    
    // Create end face joiner for right end (at X=frameWidth - halfTube, matching front-right post position)
    const rightEndX = frameWidth - halfTube;
    createEndFaceJoiner(context, baseId + "rightEndJoiner",
        tubeWidth, tubeWallThickness, innerWidth,
        halfTube, halfInner,
        frameDepth, frameHeight,
        rightEndX, baseOffset, true); // true = footer joiner
    
}


