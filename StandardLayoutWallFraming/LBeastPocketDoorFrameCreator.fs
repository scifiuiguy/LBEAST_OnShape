// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// LBeast Pocket Door Frame Creator FeatureScript
// 
// Variation of the Wall Frame Creator for pocket door frame systems.
// Layout: Footer 1 -> Center Segment 1 above it -> Footer 2 (2*segmentLength on Y) -> Center Segment 2 above Footer 2

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");
import(path : "onshape/std/debug.fs", version : "2384.0");
import(path : "onshape/std/moveFace.fs", version : "2384.0");
import(path : "onshape/std/manipulator.fs", version : "2384.0");
import(path : "1a352bc5f15cd57be34e8ae2", version : "000000000000000000000000"); // LBEASTWallUtil
// TODO: Update with actual Element ID for LBEASTWallComponents when Feature Studio is created
import(path : "a0140dc52674d230c34d7da0", version : "000000000000000000000000"); // LBEASTWallComponents

annotation { "Feature Type Name" : "LBeast Pocket Door Frame Creator" }
export const lbeastPocketDoorFrameCreator = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Tube Width" }
        isLength(definition.tubeWidth, { (inch) : [0.1, 1, 10] } as LengthBoundSpec);
        
        annotation { "Name" : "Tube Wall Thickness" }
        isLength(definition.tubeWallThickness, { (inch) : [0.01, 0.0625, 1] } as LengthBoundSpec);
        
        annotation { "Name" : "Frame Depth (Y)" }
        isLength(definition.frameDepth, { (inch) : [1, 12, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "Footer Frame Height (Z)" }
        isLength(definition.footerFrameHeight, { (inch) : [1, 24, 200] } as LengthBoundSpec);
        
        // Segment parameters
        annotation { "Name" : "Segment Height" }
        isLength(definition.segmentHeight, { (inch) : [1, 46, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "Segment Length" }
        isLength(definition.segmentLength, { (inch) : [1, 47, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "End X Position" }
        isLength(definition.endX, { (inch) : [0, 0.5, 100] } as LengthBoundSpec);
        
        // Facing direction
        annotation { "Name" : "Facing Direction (degrees)" }
        isAngle(definition.facingDirection, { (degree) : [0, 0, 360] } as AngleBoundSpec);
    }
    {
        // Set explicit defaults for tube measurements
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
        
        // Frame depth defaults
        if (definition.frameDepth == undefined || definition.frameDepth == 0 * inch)
        {
            definition.frameDepth = 12 * inch;
        }
        else if (definition.frameDepth == 1 * inch)
        {
            definition.frameDepth = 12 * inch;
        }
        
        if (definition.footerFrameHeight == undefined || definition.footerFrameHeight == 0 * inch)
        {
            definition.footerFrameHeight = 24 * inch;
        }
        else if (definition.footerFrameHeight == 1 * inch)
        {
            definition.footerFrameHeight = 24 * inch;
        }
        
        // Segment parameter defaults
        if (definition.segmentHeight == undefined || definition.segmentHeight == 0 * inch)
        {
            definition.segmentHeight = 46 * inch;
        }
        else if (definition.segmentHeight == 1 * inch)
        {
            definition.segmentHeight = 46 * inch;
        }
        
        // Clamp segmentHeight to maximum of 44 inches
        const maxSegmentHeight = 44 * inch;
        if (definition.segmentHeight > maxSegmentHeight)
        {
            definition.segmentHeight = maxSegmentHeight;
            println("Segment height clamped to maximum of 44 inches");
        }
        
        if (definition.segmentLength == undefined || definition.segmentLength == 0 * inch)
        {
            definition.segmentLength = 46 * inch;
        }
        else if (definition.segmentLength == 1 * inch)
        {
            definition.segmentLength = 46 * inch;
        }
        
        if (definition.endX == undefined || definition.endX == 0 * inch)
        {
            definition.endX = 0.5 * inch;
        }
        else if (definition.endX == 1 * inch)
        {
            definition.endX = 0.5 * inch;
        }
        
        // Calculate footerFrameWidth from segmentLength + 1 * tubeWidth
        // This keeps footer and segment widths in sync since they're stacked 1-to-1
        definition.footerFrameWidth = definition.segmentLength + 1 * definition.tubeWidth;
        
        // Facing direction defaults
        if (definition.facingDirection == undefined)
        {
            definition.facingDirection = 0 * degree;
        }
        definition.facingDirection = normalizeFacingDirection(definition.facingDirection);
        
        const innerWidth = definition.tubeWidth - 2 * definition.tubeWallThickness;
        const halfTube = definition.tubeWidth / 2;
        const halfInner = innerWidth / 2;
        const zero = 0 * inch;
        
        // Create first footer (same as Wall Frame Creator - at Z=0, X=0, Y=0)
        const footer1Id = id + "footer1";
        const footer1Offset = vector(zero, zero, zero);
        createBothBroadSideFaces(context, footer1Id,
            definition.tubeWidth, definition.tubeWallThickness, innerWidth, halfTube, halfInner,
            definition.footerFrameWidth, definition.frameDepth, definition.footerFrameHeight,
            footer1Offset);
        
        // Create corner segment 1 above footer 1
        // Position: X = 0 (directly above footer), Y = -0.5*tubeWidth, Z = footerFrameHeight - 0.5*tubeWidth
        const cornerSegment1Id = id + "cornerSegment1";
        const cornerSegment1Offset = vector(zero, -0.5 * definition.tubeWidth, definition.footerFrameHeight - 0.5 * definition.tubeWidth);
        createCornerSegmentBodies(context, cornerSegment1Id,
            definition.tubeWidth, definition.tubeWallThickness,
            definition.frameDepth, definition.segmentHeight, definition.segmentLength,
            definition.endX, cornerSegment1Offset, definition.facingDirection, 0, false);
        
        // Create second footer - move two (segmentLength + tubeWidth) over on X
        // Position: X = 2*(segmentLength + tubeWidth), Y = 0, Z = 0
        const footer2Id = id + "footer2";
        const footer2Offset = vector(2 * (definition.segmentLength + definition.tubeWidth), zero, zero);
        createBothBroadSideFaces(context, footer2Id,
            definition.tubeWidth, definition.tubeWallThickness, innerWidth, halfTube, halfInner,
            definition.footerFrameWidth, definition.frameDepth, definition.footerFrameHeight,
            footer2Offset);
        
        // Create corner segment 2 - first create at same initial position as segment 1, then rotate 180 degrees (yaw), then translate
        const cornerSegment2Id = id + "cornerSegment2";
        const cornerSegment2InitialOffset = vector(zero, -0.5 * definition.tubeWidth, definition.footerFrameHeight - 0.5 * definition.tubeWidth);
        createCornerSegmentBodies(context, cornerSegment2Id,
            definition.tubeWidth, definition.tubeWallThickness,
            definition.frameDepth, definition.segmentHeight, definition.segmentLength,
            definition.endX, cornerSegment2InitialOffset, definition.facingDirection, 0, false);
        
        // Rotate 180 degrees around Z axis (yaw) before translation
        const cornerSegment2RotationAxis = line(cornerSegment2InitialOffset, vector(0, 0, 1)); // Z axis through the initial position
        const cornerSegment2RotationTransform = rotationAround(cornerSegment2RotationAxis, 180 * degree);
        opTransform(context, cornerSegment2Id + "yawRotation", {
            "bodies" : queryAllBodies(cornerSegment2Id),
            "transform" : cornerSegment2RotationTransform
        });
        
        // Translate: 3*(segmentLength + tubeWidth) on X, frameDepth on Y
        const cornerSegment2FinalOffset = vector(3 * (definition.segmentLength + definition.tubeWidth), definition.frameDepth, zero);
        const cornerSegment2TranslationTransform = transform(cornerSegment2FinalOffset);
        opTransform(context, cornerSegment2Id + "translation", {
            "bodies" : queryAllBodies(cornerSegment2Id),
            "transform" : cornerSegment2TranslationTransform
        });
        
        // Create center segment 3 at ground level
        // Position: X = segmentLength + tubeWidth (moved over by segmentLength + tubeWidth on X), Y = -0.5*tubeWidth, Z = -0.5*tubeWidth (moved down by 0.5*tubeWidth)
        const centerSegment3Id = id + "centerSegment3";
        const centerSegment3Offset = vector(definition.segmentLength + definition.tubeWidth, -0.5 * definition.tubeWidth, -0.5 * definition.tubeWidth);
        createCenterSegmentBodies(context, centerSegment3Id,
            definition.tubeWidth, definition.tubeWallThickness,
            definition.frameDepth, definition.segmentHeight, definition.segmentLength,
            definition.endX, centerSegment3Offset, definition.facingDirection, 0, false);
        
        // Create third footer - move over by segmentLength + tubeWidth on X, move up by segmentHeight * 2 + 2 * tubeWidth on Z (moved down by 1 * tubeWidth)
        // Position: X = segmentLength + tubeWidth, Y = 0, Z = segmentHeight * 2 + 2 * tubeWidth
        const footer3Id = id + "footer3";
        const footer3Offset = vector(definition.segmentLength + definition.tubeWidth, zero, 2 * definition.segmentHeight + 2 * definition.tubeWidth);
        createBothBroadSideFaces(context, footer3Id,
            definition.tubeWidth, definition.tubeWallThickness, innerWidth, halfTube, halfInner,
            definition.footerFrameWidth, definition.frameDepth, definition.footerFrameHeight,
            footer3Offset);
        
        // ========================================
        // PocketDoorBraceFoot Category
        // ========================================
        // Create horizontal tubes (PocketDoorBraceFoot)
        // First tube: extends 21.5 inches along X, starts at 73.5 inches from origin on X
        const horizontalTubeStartX = 73.5 * inch;
        const horizontalTubeLength = 21.5 * inch;
        const horizontalTubeStart = vector(horizontalTubeStartX, zero, zero);
        const horizontalTubeEnd = vector(horizontalTubeStartX + horizontalTubeLength, zero, zero);
        const horizontalTube1Id = id + "horizontalTube1";
        // PocketDoorBraceFoot: horizontalTube1
        createTube(context, horizontalTube1Id,
            horizontalTubeStart,
            horizontalTubeEnd,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Duplicate the first tube three times with Y offsets
        // First duplicate: move 4 inches along Y
        const horizontalTube2Id = id + "horizontalTube2";
        const horizontalTube2Start = vector(horizontalTubeStartX, 4 * inch, zero);
        const horizontalTube2End = vector(horizontalTubeStartX + horizontalTubeLength, 4 * inch, zero);
        // PocketDoorBraceFoot: horizontalTube2
        createTube(context, horizontalTube2Id,
            horizontalTube2Start,
            horizontalTube2End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Second duplicate: move 7 inches along Y
        const horizontalTube3Id = id + "horizontalTube3";
        const horizontalTube3Start = vector(horizontalTubeStartX, 7 * inch, zero);
        const horizontalTube3End = vector(horizontalTubeStartX + horizontalTubeLength, 7 * inch, zero);
        // PocketDoorBraceFoot: horizontalTube3
        createTube(context, horizontalTube3Id,
            horizontalTube3Start,
            horizontalTube3End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Third duplicate: move 11 inches along Y
        const horizontalTube4Id = id + "horizontalTube4";
        const horizontalTube4Start = vector(horizontalTubeStartX, 11 * inch, zero);
        const horizontalTube4End = vector(horizontalTubeStartX + horizontalTubeLength, 11 * inch, zero);
        // PocketDoorBraceFoot: horizontalTube4
        createTube(context, horizontalTube4Id,
            horizontalTube4Start,
            horizontalTube4End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Create orthogonal tube (along Y axis, perpendicular to X-axis tubes) (PocketDoorBraceFoot)
        // 3 inches long, translated 94 inches + 0.5*tubeWidth along X and 1 inch - 0.5*tubeWidth along Y
        const orthogonalTubeId = id + "orthogonalTube";
        const orthogonalTubeLength = 3 * inch;
        const orthogonalTubeStart = vector(94 * inch + 0.5 * definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth, zero);
        const orthogonalTubeEnd = vector(94 * inch + 0.5 * definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + orthogonalTubeLength, zero);
        // PocketDoorBraceFoot: orthogonalTube
        createTube(context, orthogonalTubeId,
            orthogonalTubeStart,
            orthogonalTubeEnd,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Duplicate orthogonal tube, move 7 inches along Y
        const orthogonalTube2Id = id + "orthogonalTube2";
        const orthogonalTube2Start = vector(94 * inch + 0.5 * definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + 7 * inch, zero);
        const orthogonalTube2End = vector(94 * inch + 0.5 * definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + 7 * inch + orthogonalTubeLength, zero);
        // PocketDoorBraceFoot: orthogonalTube2
        createTube(context, orthogonalTube2Id,
            orthogonalTube2Start,
            orthogonalTube2End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Duplicate both orthogonal tubes, move -20.5 inches along X
        // First duplicate (of original tube)
        const orthogonalTube3Id = id + "orthogonalTube3";
        const orthogonalTube3Start = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch, 1 * inch - 0.5 * definition.tubeWidth, zero);
        const orthogonalTube3End = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch, 1 * inch - 0.5 * definition.tubeWidth + orthogonalTubeLength, zero);
        // PocketDoorBraceFoot: orthogonalTube3
        createTube(context, orthogonalTube3Id,
            orthogonalTube3Start,
            orthogonalTube3End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Second duplicate (of tube moved 7 inches on Y)
        const orthogonalTube4Id = id + "orthogonalTube4";
        const orthogonalTube4Start = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch, 1 * inch - 0.5 * definition.tubeWidth + 7 * inch, zero);
        const orthogonalTube4End = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch, 1 * inch - 0.5 * definition.tubeWidth + 7 * inch + orthogonalTubeLength, zero);
        // PocketDoorBraceFoot: orthogonalTube4
        createTube(context, orthogonalTube4Id,
            orthogonalTube4Start,
            orthogonalTube4End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // ========================================
        // PocketDoorVerticalBrace Category
        // ========================================
        // Vertical tubes for pocket door brace system
        
        // First vertical tube: 89 inches long, positioned at X = 72.5 inches + 0.5*tubeWidth, starting Z = -0.5*2*tubeWidth + 0.5 inches (adjusted up)
        const verticalTube1Id = id + "verticalTube1";
        const verticalTube1Length = 89 * inch;
        const verticalTube1StartZ = -0.5 * 2 * definition.tubeWidth + 0.5 * inch; // Move down by 0.5 * 2 * tubeWidth = 1 * tubeWidth, then adjust up by 0.5 inches
        const verticalTube1Start = vector(72.5 * inch + 0.5 * definition.tubeWidth, zero, verticalTube1StartZ);
        const verticalTube1End = vector(72.5 * inch + 0.5 * definition.tubeWidth, zero, verticalTube1StartZ + verticalTube1Length);
        // PocketDoorVerticalBrace: verticalTube1
        createTube(context, verticalTube1Id,
            verticalTube1Start,
            verticalTube1End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Duplicate vertical tube three times with Y offsets
        // First duplicate: move 4 inches along Y
        const verticalTube2Id = id + "verticalTube2";
        const verticalTube2Start = vector(72.5 * inch + 0.5 * definition.tubeWidth, 4 * inch, verticalTube1StartZ);
        const verticalTube2End = vector(72.5 * inch + 0.5 * definition.tubeWidth, 4 * inch, verticalTube1StartZ + verticalTube1Length);
        // PocketDoorVerticalBrace: verticalTube2
        createTube(context, verticalTube2Id,
            verticalTube2Start,
            verticalTube2End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Second duplicate: move 7 inches along Y
        const verticalTube3Id = id + "verticalTube3";
        const verticalTube3Start = vector(72.5 * inch + 0.5 * definition.tubeWidth, 7 * inch, verticalTube1StartZ);
        const verticalTube3End = vector(72.5 * inch + 0.5 * definition.tubeWidth, 7 * inch, verticalTube1StartZ + verticalTube1Length);
        // PocketDoorVerticalBrace: verticalTube3
        createTube(context, verticalTube3Id,
            verticalTube3Start,
            verticalTube3End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Third duplicate: move 11 inches along Y
        const verticalTube4Id = id + "verticalTube4";
        const verticalTube4Start = vector(72.5 * inch + 0.5 * definition.tubeWidth, 11 * inch, verticalTube1StartZ);
        const verticalTube4End = vector(72.5 * inch + 0.5 * definition.tubeWidth, 11 * inch, verticalTube1StartZ + verticalTube1Length);
        // PocketDoorVerticalBrace: verticalTube4
        createTube(context, verticalTube4Id,
            verticalTube4Start,
            verticalTube4End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Duplicate last two tubes from PocketDoorBraceFoot category, move -1 * tubeWidth along X
        // First duplicate (of orthogonalTube3)
        const orthogonalTube5Id = id + "orthogonalTube5";
        const orthogonalTube5Start = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth, zero);
        const orthogonalTube5End = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + orthogonalTubeLength, zero);
        // PocketDoorVerticalBrace: orthogonalTube5
        createTube(context, orthogonalTube5Id,
            orthogonalTube5Start,
            orthogonalTube5End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Second duplicate (of orthogonalTube4)
        const orthogonalTube6Id = id + "orthogonalTube6";
        const orthogonalTube6Start = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + 7 * inch, zero);
        const orthogonalTube6End = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + 7 * inch + orthogonalTubeLength, zero);
        // PocketDoorVerticalBrace: orthogonalTube6
        createTube(context, orthogonalTube6Id,
            orthogonalTube6Start,
            orthogonalTube6End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Duplicate orthogonalTube5 and orthogonalTube6, move up 88 inches on Z
        // First duplicate (of orthogonalTube5)
        const orthogonalTube7Id = id + "orthogonalTube7";
        const orthogonalTube7Start = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth, 88 * inch);
        const orthogonalTube7End = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + orthogonalTubeLength, 88 * inch);
        // PocketDoorVerticalBrace: orthogonalTube7
        createTube(context, orthogonalTube7Id,
            orthogonalTube7Start,
            orthogonalTube7End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Second duplicate (of orthogonalTube6)
        const orthogonalTube8Id = id + "orthogonalTube8";
        const orthogonalTube8Start = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + 7 * inch, 88 * inch);
        const orthogonalTube8End = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + 7 * inch + orthogonalTubeLength, 88 * inch);
        // PocketDoorVerticalBrace: orthogonalTube8
        createTube(context, orthogonalTube8Id,
            orthogonalTube8Start,
            orthogonalTube8End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // ========================================
        // Door Frame Category
        // ========================================
        // Door frame vertical tubes
        
        // First vertical tube: 88 inches long, positioned at X = 69.5 inches, Y = 5 inches, starting Z = -0.52*tubeWidth (adjusted down)
        const doorFrameTube1Id = id + "doorFrameTube1";
        const doorFrameTube1Length = 88 * inch;
        const doorFrameTube1StartZ = -0.52 * definition.tubeWidth; // Move down by 0.52 * tubeWidth
        const doorFrameTube1Start = vector(69.5 * inch, 5 * inch, doorFrameTube1StartZ);
        const doorFrameTube1End = vector(69.5 * inch, 5 * inch, doorFrameTube1StartZ + doorFrameTube1Length);
        // Door Frame: doorFrameTube1
        createTube(context, doorFrameTube1Id,
            doorFrameTube1Start,
            doorFrameTube1End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Duplicate door frame tube, move 1 inch along Y
        const doorFrameTube2Id = id + "doorFrameTube2";
        const doorFrameTube2Start = vector(69.5 * inch, 5 * inch + 1 * inch, doorFrameTube1StartZ);
        const doorFrameTube2End = vector(69.5 * inch, 5 * inch + 1 * inch, doorFrameTube1StartZ + doorFrameTube1Length);
        // Door Frame: doorFrameTube2
        createTube(context, doorFrameTube2Id,
            doorFrameTube2Start,
            doorFrameTube2End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Duplicate both door frame tubes, move 24 inches along X
        // First duplicate (of doorFrameTube1)
        const doorFrameTube3Id = id + "doorFrameTube3";
        const doorFrameTube3Start = vector(69.5 * inch + 24 * inch, 5 * inch, doorFrameTube1StartZ);
        const doorFrameTube3End = vector(69.5 * inch + 24 * inch, 5 * inch, doorFrameTube1StartZ + doorFrameTube1Length);
        // Door Frame: doorFrameTube3
        createTube(context, doorFrameTube3Id,
            doorFrameTube3Start,
            doorFrameTube3End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Second duplicate (of doorFrameTube2)
        const doorFrameTube4Id = id + "doorFrameTube4";
        const doorFrameTube4Start = vector(69.5 * inch + 24 * inch, 5 * inch + 1 * inch, doorFrameTube1StartZ);
        const doorFrameTube4End = vector(69.5 * inch + 24 * inch, 5 * inch + 1 * inch, doorFrameTube1StartZ + doorFrameTube1Length);
        // Door Frame: doorFrameTube4
        createTube(context, doorFrameTube4Id,
            doorFrameTube4Start,
            doorFrameTube4End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Horizontal tube: 23 inches long along X, positioned at X = 70 inches, Y = 5 inches
        const doorFrameHorizontalTube1Id = id + "doorFrameHorizontalTube1";
        const doorFrameHorizontalTube1Length = 23 * inch;
        const doorFrameHorizontalTube1Start = vector(70 * inch, 5 * inch, zero);
        const doorFrameHorizontalTube1End = vector(70 * inch + doorFrameHorizontalTube1Length, 5 * inch, zero);
        // Door Frame: doorFrameHorizontalTube1
        createTube(context, doorFrameHorizontalTube1Id,
            doorFrameHorizontalTube1Start,
            doorFrameHorizontalTube1End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Duplicate horizontal tube, move 1 inch along Y
        const doorFrameHorizontalTube2Id = id + "doorFrameHorizontalTube2";
        const doorFrameHorizontalTube2Start = vector(70 * inch, 5 * inch + 1 * inch, zero);
        const doorFrameHorizontalTube2End = vector(70 * inch + doorFrameHorizontalTube1Length, 5 * inch + 1 * inch, zero);
        // Door Frame: doorFrameHorizontalTube2
        createTube(context, doorFrameHorizontalTube2Id,
            doorFrameHorizontalTube2Start,
            doorFrameHorizontalTube2End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Duplicate both horizontal tubes, move up 87 inches on Z
        // First duplicate (of doorFrameHorizontalTube1)
        const doorFrameHorizontalTube3Id = id + "doorFrameHorizontalTube3";
        const doorFrameHorizontalTube3Start = vector(70 * inch, 5 * inch, 87 * inch);
        const doorFrameHorizontalTube3End = vector(70 * inch + doorFrameHorizontalTube1Length, 5 * inch, 87 * inch);
        // Door Frame: doorFrameHorizontalTube3
        createTube(context, doorFrameHorizontalTube3Id,
            doorFrameHorizontalTube3Start,
            doorFrameHorizontalTube3End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Second duplicate (of doorFrameHorizontalTube2)
        const doorFrameHorizontalTube4Id = id + "doorFrameHorizontalTube4";
        const doorFrameHorizontalTube4Start = vector(70 * inch, 5 * inch + 1 * inch, 87 * inch);
        const doorFrameHorizontalTube4End = vector(70 * inch + doorFrameHorizontalTube1Length, 5 * inch + 1 * inch, 87 * inch);
        // Door Frame: doorFrameHorizontalTube4
        createTube(context, doorFrameHorizontalTube4Id,
            doorFrameHorizontalTube4Start,
            doorFrameHorizontalTube4End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // ========================================
        // Door Suspender Group Category
        // ========================================
        // Door suspender components
        
        // Create horizontal orthogonal tubes similar to orthogonalTube7 and orthogonalTube8
        // Positioned 1 * tubeWidth higher on Z and 1 inch longer
        const doorSuspenderTube1Length = orthogonalTubeLength + 1 * inch; // 3 + 1 = 4 inches
        const doorSuspenderTube1Z = 88 * inch + definition.tubeWidth; // 1 * tubeWidth higher than orthogonalTube7/8
        const doorSuspenderTube1Id = id + "doorSuspenderTube1";
        const doorSuspenderTube1Start = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth, doorSuspenderTube1Z);
        const doorSuspenderTube1End = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + doorSuspenderTube1Length, doorSuspenderTube1Z);
        // Door Suspender Group: doorSuspenderTube1
        createTube(context, doorSuspenderTube1Id,
            doorSuspenderTube1Start,
            doorSuspenderTube1End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Second tube (similar to orthogonalTube8, moved -1 * tubeWidth along Y)
        const doorSuspenderTube2Id = id + "doorSuspenderTube2";
        const doorSuspenderTube2Start = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + 7 * inch - definition.tubeWidth, doorSuspenderTube1Z);
        const doorSuspenderTube2End = vector(94 * inch + 0.5 * definition.tubeWidth - 20.5 * inch - definition.tubeWidth, 1 * inch - 0.5 * definition.tubeWidth + 7 * inch - definition.tubeWidth + doorSuspenderTube1Length, doorSuspenderTube1Z);
        // Door Suspender Group: doorSuspenderTube2
        createTube(context, doorSuspenderTube2Id,
            doorSuspenderTube2Start,
            doorSuspenderTube2End,
            halfTube, halfInner, definition.tubeWidth, definition.tubeWallThickness);
        
        // Create rectangular tube: 2" wide x 1" tall x 24" long, positioned at X = 72.5, Y = 4, Z = 89
        // Horizontal along Y axis
        const rectTubeId = id + "doorSuspenderRectTube";
        const rectTubeWidth = 2 * inch;
        const rectTubeHeight = 1 * inch;
        const rectTubeLength = 22.375 * inch;
        const halfRectWidth = rectTubeWidth / 2;
        const halfRectHeight = rectTubeHeight / 2;
        const innerRectWidth = rectTubeWidth - 2 * definition.tubeWallThickness;
        const innerRectHeight = rectTubeHeight - 2 * definition.tubeWallThickness;
        const halfInnerRectWidth = innerRectWidth / 2;
        const halfInnerRectHeight = innerRectHeight / 2;
        
        const rectTubeStart = vector(72.5 * inch + 0.5 * inch, 4 * inch + 0.5 * definition.tubeWidth, 89 * inch + 0.5 * definition.tubeWidth);
        const rectTubeEnd = vector(72.5 * inch + 0.5 * inch, 4 * inch + 0.5 * definition.tubeWidth + rectTubeLength, 89 * inch + 0.5 * definition.tubeWidth);
        const rectTubeDirection = normalize(rectTubeEnd - rectTubeStart); // Direction along Y
        
        // Create outer rectangle sketch (in XZ plane, normal = Y)
        const rectTubeOuterSketchId = rectTubeId + "outerSketch";
        const rectTubeOuterSketch = newSketchOnPlane(context, rectTubeOuterSketchId, {
            "sketchPlane" : plane(rectTubeStart, rectTubeDirection)
        });
        skRectangle(rectTubeOuterSketch, "outerRect", {
            "firstCorner" : vector(-halfRectWidth, -halfRectHeight),
            "secondCorner" : vector(halfRectWidth, halfRectHeight)
        });
        skSolve(rectTubeOuterSketch);
        
        const rectTubeOuterRegions = qSketchRegion(rectTubeOuterSketchId);
        opExtrude(context, rectTubeId + "outer", {
            "entities" : rectTubeOuterRegions,
            "direction" : rectTubeDirection,
            "endBound" : BoundingType.BLIND,
            "endDepth" : rectTubeLength,
            "operationType" : NewBodyOperationType.NEW
        });
        
        // Create inner rectangle sketch for hollow tube
        const rectTubeInnerSketchId = rectTubeId + "innerSketch";
        const rectTubeInnerSketch = newSketchOnPlane(context, rectTubeInnerSketchId, {
            "sketchPlane" : plane(rectTubeStart, rectTubeDirection)
        });
        skRectangle(rectTubeInnerSketch, "innerRect", {
            "firstCorner" : vector(-halfInnerRectWidth, -halfInnerRectHeight),
            "secondCorner" : vector(halfInnerRectWidth, halfInnerRectHeight)
        });
        skSolve(rectTubeInnerSketch);
        
        const rectTubeInnerRegions = qSketchRegion(rectTubeInnerSketchId);
        opExtrude(context, rectTubeId + "inner", {
            "entities" : rectTubeInnerRegions,
            "direction" : rectTubeDirection,
            "endBound" : BoundingType.BLIND,
            "endDepth" : rectTubeLength,
            "operationType" : NewBodyOperationType.NEW
        });
        
        // Subtract inner from outer to create hollow rectangular tube
        opBoolean(context, rectTubeId + "subtract", {
            "tools" : qBodyType(qCreatedBy(rectTubeId + "inner", EntityType.BODY), BodyType.SOLID),
            "operationType" : BooleanOperationType.SUBTRACTION,
            "targets" : qBodyType(qCreatedBy(rectTubeId + "outer", EntityType.BODY), BodyType.SOLID)
        });
        // Door Suspender Group: doorSuspenderRectTube
        
        // Find the four broad edges and rotate around index 0 (90 degrees counterclockwise)
        const rectTubeBody = qBodyType(qCreatedBy(rectTubeId + "outer", EntityType.BODY), BodyType.SOLID);
        const allEdges = qOwnedByBody(rectTubeBody, EntityType.EDGE);
        const edgeArray = evaluateQuery(context, allEdges);
        
        // Find the 4 long edges (along Y direction, length = rectTubeLength)
        var longEdges = [];
        for (var e = 0; e < size(edgeArray); e += 1)
        {
            const edge = edgeArray[e];
            const edgeBox = evBox3d(context, {
                "topology" : edge
            });
            const edgeVector = edgeBox.maxCorner - edgeBox.minCorner;
            const edgeLength = norm(edgeVector);
            
            // Check if this edge is approximately the length of the tube (long edge)
            if (abs(edgeLength - rectTubeLength) < 0.1 * inch)
            {
                longEdges = append(longEdges, edge);
            }
        }
        
        // Rotate the rectangular tube around index 0 edge (90 degrees counterclockwise)
        if (size(longEdges) > 0)
        {
            const rotationEdge = longEdges[0]; // Index 0 (was RED)
            
            // Get the vertices of the edge to create the rotation axis
            const edgeVertices = evaluateQuery(context, qAdjacent(rotationEdge, AdjacencyType.VERTEX, EntityType.VERTEX));
            if (size(edgeVertices) >= 2)
            {
                const vertex0Point = evVertexPoint(context, {
                    "vertex" : edgeVertices[0]
                });
                const vertex1Point = evVertexPoint(context, {
                    "vertex" : edgeVertices[1]
                });
                
                // Create the rotation axis line from the edge endpoints
                const edgeDirection = normalize(vertex1Point - vertex0Point);
                const rotationAxis = line(vertex0Point, edgeDirection);
                
                // Rotate 90 degrees counterclockwise around the axis (using -90 to reverse direction)
                const rotationTransform = rotationAround(rotationAxis, -90 * degree);
                opTransform(context, rectTubeId + "rotation", {
                    "bodies" : rectTubeBody,
                    "transform" : rotationTransform
                });
            }
        }
        
        // Find the four outer edges closest to zero on Y
        // Re-query edges after rotation
        const rectTubeBodyAfterRotation = qBodyType(qCreatedBy(rectTubeId + "outer", EntityType.BODY), BodyType.SOLID);
        const allEdgesAfterRotation = qOwnedByBody(rectTubeBodyAfterRotation, EntityType.EDGE);
        const edgeArrayAfterRotation = evaluateQuery(context, allEdgesAfterRotation);
        
        // Find the minimum Y position among all edges
        var minY = 1000 * inch;
        for (var e = 0; e < size(edgeArrayAfterRotation); e += 1)
        {
            const edge = edgeArrayAfterRotation[e];
            const edgeBox = evBox3d(context, {
                "topology" : edge
            });
            if (edgeBox.minCorner[1] < minY)
            {
                minY = edgeBox.minCorner[1];
            }
        }
        
        // Find edges that are at or near the minimum Y (closest to zero)
        var edgesAtMinY = [];
        for (var e = 0; e < size(edgeArrayAfterRotation); e += 1)
        {
            const edge = edgeArrayAfterRotation[e];
            const edgeBox = evBox3d(context, {
                "topology" : edge
            });
            const edgeMidpointY = (edgeBox.minCorner[1] + edgeBox.maxCorner[1]) / 2;
            
            // Check if edge is at minimum Y (within tolerance)
            if (abs(edgeMidpointY - minY) < 0.1 * inch)
            {
                edgesAtMinY = append(edgesAtMinY, edge);
            }
        }
        
        // Rotate the rectangular tube around index 0 edge (90 degrees)
        if (size(edgesAtMinY) > 0)
        {
            const rotationEdge2 = edgesAtMinY[0]; // Index 0 (was RED)
            
            // Get the vertices of the edge to create the rotation axis
            const edgeVertices2 = evaluateQuery(context, qAdjacent(rotationEdge2, AdjacencyType.VERTEX, EntityType.VERTEX));
            if (size(edgeVertices2) >= 2)
            {
                const vertex0Point2 = evVertexPoint(context, {
                    "vertex" : edgeVertices2[0]
                });
                const vertex1Point2 = evVertexPoint(context, {
                    "vertex" : edgeVertices2[1]
                });
                
                // Create the rotation axis line from the edge endpoints
                const edgeDirection2 = normalize(vertex1Point2 - vertex0Point2);
                const rotationAxis2 = line(vertex0Point2, edgeDirection2);
                
                // Rotate -90 degrees around the axis
                const rotationTransform2 = rotationAround(rotationAxis2, -90 * degree);
                opTransform(context, rectTubeId + "rotation2", {
                    "bodies" : rectTubeBodyAfterRotation,
                    "transform" : rotationTransform2
                });
            }
        }
        
        // Create rectangular prism cutting tool: 25" on X, 0.5" on Y, 1" on Z
        // Positioned at the same location as the rectangular tube
        const cuttingToolId = id + "cuttingTool";
        const cuttingToolWidthX = 25 * inch;
        const cuttingToolDepthY = 0.5 * inch;
        const cuttingToolHeightZ = 1 * inch;
        const halfCuttingX = cuttingToolWidthX / 2;
        const halfCuttingY = cuttingToolDepthY / 2;
        const halfCuttingZ = cuttingToolHeightZ / 2;
        
        // Position cutting tool: same as rectangular tube start, then move -1 on X, +1 on Y, -1 on Z
        const cuttingToolCenter = rectTubeStart + vector(-1 * inch, 1 * inch, -1 * inch);
        
        // Create sketch for the cutting tool (in YZ plane, normal = X)
        const cuttingToolSketchId = cuttingToolId + "sketch";
        const cuttingToolSketch = newSketchOnPlane(context, cuttingToolSketchId, {
            "sketchPlane" : plane(cuttingToolCenter, vector(1, 0, 0))
        });
        skRectangle(cuttingToolSketch, "cuttingRect", {
            "firstCorner" : vector(-halfCuttingY, -halfCuttingZ),
            "secondCorner" : vector(halfCuttingY, halfCuttingZ)
        });
        skSolve(cuttingToolSketch);
        
        // Extrude along X direction
        const cuttingToolRegions = qSketchRegion(cuttingToolSketchId);
        const cuttingToolDirection = vector(1, 0, 0); // Extrude along X
        opExtrude(context, cuttingToolId, {
            "entities" : cuttingToolRegions,
            "direction" : cuttingToolDirection,
            "endBound" : BoundingType.BLIND,
            "endDepth" : cuttingToolWidthX,
            "operationType" : NewBodyOperationType.NEW
        });
        
        // Perform boolean subtraction: cutting tool subtracts from rectangular tube
        // The rectangular tube body after rotations is rectTubeBodyAfterRotation
        const cuttingToolBody = qBodyType(qCreatedBy(cuttingToolId, EntityType.BODY), BodyType.SOLID);
        opBoolean(context, id + "rectTubeCut", {
            "tools" : cuttingToolBody,
            "targets" : rectTubeBodyAfterRotation,
            "operationType" : BooleanOperationType.SUBTRACTION,
            "keepTools" : false // Delete the cutting tool after the operation
        });
        
        // Delete sketch bodies for rectangular tube and cutting tool
        try
        {
            opDeleteBodies(context, id + "deleteRectTubeOuterSketch", {
                "entities" : qCreatedBy(rectTubeOuterSketchId, EntityType.BODY)
            });
        }
        catch
        {
            // It's okay if there are no sketch bodies to delete
        }
        
        try
        {
            opDeleteBodies(context, id + "deleteRectTubeInnerSketch", {
                "entities" : qCreatedBy(rectTubeInnerSketchId, EntityType.BODY)
            });
        }
        catch
        {
            // It's okay if there are no sketch bodies to delete
        }
        
        try
        {
            opDeleteBodies(context, id + "deleteCuttingToolSketch", {
                "entities" : qCreatedBy(cuttingToolSketchId, EntityType.BODY)
            });
        }
        catch
        {
            // It's okay if there are no sketch bodies to delete
        }
        
        // Create flat bar: 0.125" thick on X, 4" long on Y, 1" tall on Z
        // Position: X = 94.875", Y = 3.5", Z = 88.5"
        const flatBarId = id + "flatBar";
        const flatBarThicknessX = 0.125 * inch;
        const flatBarLengthY = 4 * inch;
        const flatBarHeightZ = 1 * inch;
        const halfFlatBarY = flatBarLengthY / 2;
        const halfFlatBarZ = flatBarHeightZ / 2;
        
        const flatBarPosition = vector(94.875 * inch, 5.5 * inch, 89.0 * inch);
        
        // Create sketch for the flat bar (in YZ plane, normal = X)
        const flatBarSketchId = flatBarId + "sketch";
        const flatBarSketch = newSketchOnPlane(context, flatBarSketchId, {
            "sketchPlane" : plane(flatBarPosition, vector(1, 0, 0))
        });
        skRectangle(flatBarSketch, "flatBarRect", {
            "firstCorner" : vector(-halfFlatBarY, -halfFlatBarZ),
            "secondCorner" : vector(halfFlatBarY, halfFlatBarZ)
        });
        skSolve(flatBarSketch);
        
        // Extrude along X direction (0.125" thick)
        const flatBarRegions = qSketchRegion(flatBarSketchId);
        const flatBarDirection = vector(1, 0, 0); // Extrude along X
        opExtrude(context, flatBarId, {
            "entities" : flatBarRegions,
            "direction" : flatBarDirection,
            "endBound" : BoundingType.BLIND,
            "endDepth" : flatBarThicknessX,
            "operationType" : NewBodyOperationType.NEW
        });
        
        // Delete sketch body for flat bar
        try
        {
            opDeleteBodies(context, id + "deleteFlatBarSketch", {
                "entities" : qCreatedBy(flatBarSketchId, EntityType.BODY)
            });
        }
        catch
        {
            // It's okay if there are no sketch bodies to delete
        }
        
    }, {
        editingLogic : function(context is Context, definition is map) returns map
        {
            // Override 1 inch values (OnShape's fallback) with correct defaults when editing
            if (definition.tubeWallThickness == 1 * inch)
            {
                definition.tubeWallThickness = 0.0625 * inch;
            }
            
            if (definition.frameDepth == 1 * inch)
            {
                definition.frameDepth = 12 * inch;
            }
            
            if (definition.footerFrameHeight == 1 * inch)
            {
                definition.footerFrameHeight = 24 * inch;
            }
            
            if (definition.segmentHeight == 1 * inch)
            {
                definition.segmentHeight = 46 * inch;
            }
            
            if (definition.segmentLength == 1 * inch)
            {
                definition.segmentLength = 46 * inch;
            }
            
            if (definition.endX == 1 * inch)
            {
                definition.endX = 0.5 * inch;
            }
            
            // Recalculate footerFrameWidth from segmentLength
            if (definition.segmentLength != undefined && definition.tubeWidth != undefined)
            {
                definition.footerFrameWidth = definition.segmentLength + 1 * definition.tubeWidth;
            }
            
            return definition;
        }
    });

