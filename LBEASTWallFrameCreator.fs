// Copyright (c) 2025 AJ Campbell. Licensed under the MIT License.
//
// LBEAST Wall Frame Creator FeatureScript
// 
// Creates a wall frame system with:
// - Footer frame set (QTY2, 72" wide each = 12' total, 24" tall)
// - Wall segment frame set (QTY3, 48" wide each = 12' total, 96" tall)
// Both sets use the same tube measurements and are aligned to create a 12' wide wall system.

FeatureScript 2384;
import(path : "onshape/std/geometry.fs", version : "2384.0");
import(path : "onshape/std/sketch.fs", version : "2384.0");
import(path : "onshape/std/transform.fs", version : "2384.0");
import(path : "onshape/std/debug.fs", version : "2384.0");
import(path : "onshape/std/moveFace.fs", version : "2384.0");
import(path : "onshape/std/manipulator.fs", version : "2384.0");
import(path : "1a352bc5f15cd57be34e8ae2", version : "2d512763c2cdc696a8563fb5");

annotation { "Feature Type Name" : "LBEAST Wall Frame Creator",
             "Manipulator Change Function" : "lbeastWallFrameCreatorOnChange" }
export const lbeastWallFrameCreator = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Tube Width" }
        isLength(definition.tubeWidth, { (inch) : [0.1, 1, 10] } as LengthBoundSpec);
        
        annotation { "Name" : "Tube Wall Thickness" }
        isLength(definition.tubeWallThickness, { (inch) : [0.01, 0.0625, 1] } as LengthBoundSpec);
        
        annotation { "Name" : "Footer Frame Width (X)" }
        isLength(definition.footerFrameWidth, { (inch) : [1, 48, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "Frame Depth (Y)" }
        isLength(definition.frameDepth, { (inch) : [1, 12, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "Footer Frame Height (Z)" }
        isLength(definition.footerFrameHeight, { (inch) : [1, 24, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "Footer Total on X" }
        isInteger(definition.footerTotalOnX, { (unitless) : [1, 3, 20] } as IntegerBoundSpec);
        
        annotation { "Name" : "Wall Frame Width (X)" }
        isLength(definition.wallFrameWidth, { (inch) : [1, 48, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "Wall Frame Height (Z)" }
        isLength(definition.wallFrameHeight, { (inch) : [1, 96, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "Wall Total on X" }
        isInteger(definition.wallTotalOnX, { (unitless) : [1, 3, 20] } as IntegerBoundSpec);
        
        // Segment parameters (shared with corner and center segments)
        annotation { "Name" : "Segment Height" }
        isLength(definition.segmentHeight, { (inch) : [1, 46, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "Segment Length" }
        isLength(definition.segmentLength, { (inch) : [1, 47, 200] } as LengthBoundSpec);
        
        annotation { "Name" : "End X Position" }
        isLength(definition.endX, { (inch) : [0, 0.5, 100] } as LengthBoundSpec);
        
        // Wall-level facing direction (separate from individual segment facing directions)
        annotation { "Name" : "Wall Facing Direction (degrees)" }
        isAngle(definition.wallFacingDirection, { (degree) : [0, 0, 360] } as AngleBoundSpec);
        
        // Segment rotation parameters
        annotation { "Name" : "Corner Segment 1 Rotation Normalized", "Default" : 0 }
        isReal(definition.cornerSegment1RotationNormalized, { (unitless) : [0, 0, 2] } as RealBoundSpec);
        
        annotation { "Name" : "Center Segment Rotation Normalized", "Default" : 0 }
        isReal(definition.centerSegmentRotationNormalized, { (unitless) : [0, 0, 2] } as RealBoundSpec);
        
        annotation { "Name" : "Corner Segment 2 Rotation Normalized", "Default" : 0 }
        isReal(definition.cornerSegment2RotationNormalized, { (unitless) : [0, 0, 2] } as RealBoundSpec);
        
        // Wall-level rotation parameter (controls all three segments simultaneously)
        // Range 0-2: Rotation groups 1-4 for segments
        // Range 2-3: Footer broad face transition
        // Range 3-4: (reserved for future transitions)
        annotation { "Name" : "Wall Rotation Normalized", "Default" : 0 }
        isReal(definition.wallRotationNormalized, { (unitless) : [0, 0, 4] } as RealBoundSpec);
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
        
        // Footer frame defaults
        if (definition.footerFrameWidth == undefined || definition.footerFrameWidth == 0 * inch)
        {
            definition.footerFrameWidth = 48 * inch;
        }
        else if (definition.footerFrameWidth == 1 * inch)
        {
            definition.footerFrameWidth = 48 * inch;
        }
        
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
        
        if (definition.footerTotalOnX == undefined)
        {
            definition.footerTotalOnX = 3;
        }
        
        // Wall frame defaults
        if (definition.wallFrameWidth == undefined || definition.wallFrameWidth == 0 * inch)
        {
            definition.wallFrameWidth = 48 * inch;
        }
        else if (definition.wallFrameWidth == 1 * inch)
        {
            definition.wallFrameWidth = 48 * inch;
        }
        
        if (definition.wallFrameHeight == undefined || definition.wallFrameHeight == 0 * inch)
        {
            definition.wallFrameHeight = 96 * inch;
        }
        else if (definition.wallFrameHeight == 1 * inch)
        {
            definition.wallFrameHeight = 96 * inch;
        }
        
        if (definition.wallTotalOnX == undefined)
        {
            definition.wallTotalOnX = 3;
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
        
        // Wall-level facing direction defaults
        if (definition.wallFacingDirection == undefined)
        {
            definition.wallFacingDirection = 0 * degree;
        }
        definition.wallFacingDirection = normalizeFacingDirection(definition.wallFacingDirection);
        
        // Rotation normalized defaults
        if (definition.cornerSegment1RotationNormalized == undefined)
        {
            definition.cornerSegment1RotationNormalized = 0;
        }
        if (definition.centerSegmentRotationNormalized == undefined)
        {
            definition.centerSegmentRotationNormalized = 0;
        }
        if (definition.cornerSegment2RotationNormalized == undefined)
        {
            definition.cornerSegment2RotationNormalized = 0;
        }
        
        const innerWidth = definition.tubeWidth - 2 * definition.tubeWallThickness;
        const halfTube = definition.tubeWidth / 2;
        const halfInner = innerWidth / 2;
        const zero = 0 * inch;
        
        // Create footer frames using new joiner-based design (at Z=0)
        // Duplicate footer frames along X axis
        for (var i = 0; i < definition.footerTotalOnX; i += 1)
        {
            // Calculate base offset for this footer instance
            const offsetX = i * definition.footerFrameWidth;
            const baseOffset = vector(offsetX, zero, zero);
            
            // Unique ID suffix for this footer instance
            const footerInstanceId = id + "footer" + "x" + i;
            
            // Create footer using new joiner-based design
            createBothBroadSideFaces(context, footerInstanceId,
                definition.tubeWidth, definition.tubeWallThickness, innerWidth, halfTube, halfInner,
                definition.footerFrameWidth, definition.frameDepth, definition.footerFrameHeight,
                baseOffset);
        }
        
        // Calculate footer dimensions for positioning segments
        // Footer extends from X=0 to X=footerFrameWidth*3, Y=0 to Y=frameDepth+tubeWidth, Z=0 to Z=footerFrameHeight
        const footerBackY = definition.frameDepth + definition.tubeWidth; // Back face Y position
        
        // Instantiate corner segment 1 above footer 0 (first footer)
        // Position: X = 0, Y = -0.5*tubeWidth, Z = footerFrameHeight - 0.5*tubeWidth, facingDirection = 0
        const corner1SegmentId = id + "cornerSegment1";
        const corner1Offset = vector(0 * inch, -0.5 * definition.tubeWidth, definition.footerFrameHeight - 0.5 * definition.tubeWidth);
        const corner1FacingDirection = definition.wallFacingDirection; // No additional rotation
        createCornerSegmentBodies(context, corner1SegmentId,
            definition.tubeWidth, definition.tubeWallThickness,
            definition.frameDepth, definition.segmentHeight, definition.segmentLength,
            definition.endX, corner1Offset, corner1FacingDirection, definition.cornerSegment1RotationNormalized, false);
        
        // Instantiate center segment above footer 1 (second footer)
        // Position: X = footerFrameWidth, Y = -0.5*tubeWidth, Z = footerFrameHeight - 0.5*tubeWidth, facingDirection = 0
        const centerSegmentId = id + "centerSegment";
        const centerOffset = vector(definition.footerFrameWidth, -0.5 * definition.tubeWidth, definition.footerFrameHeight - 0.5 * definition.tubeWidth);
        const centerFacingDirection = definition.wallFacingDirection; // No additional rotation
        createCenterSegmentBodies(context, centerSegmentId,
            definition.tubeWidth, definition.tubeWallThickness,
            definition.frameDepth, definition.segmentHeight, definition.segmentLength,
            definition.endX, centerOffset, centerFacingDirection, definition.centerSegmentRotationNormalized, false);
        
        // Instantiate corner segment 2 above footer 2 (third footer)
        // Position: X = 3*segmentLength + 3*tubeWidth, Y = +1*frameDepth - 0.5*tubeWidth, Z = footerFrameHeight - 0.5*tubeWidth
        // First create at position with no rotation, then rotate 180 degrees around its local position
        const corner2SegmentId = id + "cornerSegment2";
        const corner2OffsetX = 3 * definition.segmentLength + 3 * definition.tubeWidth; // Move over +3 x segmentLength + 3*tubeWidth on X
        const corner2OffsetY = definition.frameDepth - 0.5 * definition.tubeWidth; // Move over +1 x frameDepth, then -0.5*tubeWidth adjustment on Y
        const corner2OffsetZ = definition.footerFrameHeight - 0.5 * definition.tubeWidth; // Apply -0.5*tubeWidth adjustment on Z
        const corner2Offset = vector(corner2OffsetX, corner2OffsetY, corner2OffsetZ);
        // Create with wallFacingDirection (no additional 180 yet)
        createCornerSegmentBodies(context, corner2SegmentId,
            definition.tubeWidth, definition.tubeWallThickness,
            definition.frameDepth, definition.segmentHeight, definition.segmentLength,
            definition.endX, corner2Offset, definition.wallFacingDirection, definition.cornerSegment2RotationNormalized, false);
        
        // Now apply 180 degree rotation around the segment's local position (offset)
        const corner2RotationAxis = line(corner2Offset, vector(0, 0, 1)); // Z axis through the segment's position
        const corner2RotationTransform = rotationAround(corner2RotationAxis, 180 * degree);
        opTransform(context, corner2SegmentId + "localRotation", {
            "bodies" : queryAllBodies(corner2SegmentId),
            "transform" : corner2RotationTransform
        });
        
        // Apply transition sequence for range 2-3+: Translate front and back broad face bodies of all footers up on Z
        // Map 2.0-3.0 normalized to 0-1.5 x footerFrameHeight movement, stays at max when >= 3.0
        if (definition.wallRotationNormalized >= 2.0)
        {
            // Calculate movement amount: at 2.0 = 0, at 3.0+ = 1.5 * footerFrameHeight (stays up)
            var transitionNormalized = (definition.wallRotationNormalized - 2.0) / 1.0; // 0-1 range for transition
            if (transitionNormalized > 1.0)
            {
                transitionNormalized = 1.0; // Clamp to max at 3.0 and above
            }
            const movementAmount = transitionNormalized * 1.5 * definition.footerFrameHeight;
            
            println("Footer broad face transition - normalized: " ~ transitionNormalized ~ ", movement: " ~ movementAmount);
            
            // Collect all front and back broad face bodies from all footers
            var allBroadFaceBodies = [];
            for (var footerIndex = 0; footerIndex < definition.footerTotalOnX; footerIndex += 1)
            {
                const footerId = id + "footer" + "x" + footerIndex;
                
                // Query all bodies from front broad side face (bottomX, topBottomX, post1, post2)
                const frontBroadFaceBodies = qBodyType(qCreatedBy(footerId + "front", EntityType.BODY), BodyType.SOLID);
                
                // Query all bodies from back broad side face (bottomX, topBottomX, post1, post2)
                const backBroadFaceBodies = qBodyType(qCreatedBy(footerId + "back", EntityType.BODY), BodyType.SOLID);
                
                // Add to collection
                allBroadFaceBodies = append(allBroadFaceBodies, frontBroadFaceBodies);
                allBroadFaceBodies = append(allBroadFaceBodies, backBroadFaceBodies);
            }
            
            // Translate all footer broad face bodies in a single operation
            const moveTransform = transform(vector(0 * inch, 0 * inch, movementAmount));
            opTransform(context, id + "footerBroadFaceTransition", {
                "bodies" : qUnion(allBroadFaceBodies),
                "transform" : moveTransform
            });
        }
        
        // Apply transition sequence for first footer: Move front/back face bodies
        // Range 3.0-3.25: Move -1 x tubeWidth on Y
        // Range 3.25-3.75: Move 1 x segmentLength on X
        const firstFooterId = id + "footer" + "x" + 0;
        const firstFooterFrontBodies = qBodyType(qCreatedBy(firstFooterId + "front", EntityType.BODY), BodyType.SOLID);
        const firstFooterBackBodies = qBodyType(qCreatedBy(firstFooterId + "back", EntityType.BODY), BodyType.SOLID);
        const firstFooterBroadFaceBodies = qUnion([firstFooterFrontBodies, firstFooterBackBodies]);
        
        // Calculate Y movement: -1 x tubeWidth across range 3.0-3.25
        var yMovement = 0 * inch;
        if (definition.wallRotationNormalized >= 3.0)
        {
            var yTransitionNormalized = (definition.wallRotationNormalized - 3.0) / 0.25; // 0-1 range for 3.0-3.25
            if (yTransitionNormalized > 1.0)
            {
                yTransitionNormalized = 1.0; // Clamp to max at 3.25 and above
            }
            yMovement = -yTransitionNormalized * definition.tubeWidth;
        }
        
        // Calculate X movement: 1 x segmentLength + 1 x tubeWidth across range 3.25-3.75
        var xMovement = 0 * inch;
        if (definition.wallRotationNormalized >= 3.25)
        {
            var xTransitionNormalized = (definition.wallRotationNormalized - 3.25) / 0.5; // 0-1 range for 3.25-3.75
            if (xTransitionNormalized > 1.0)
            {
                xTransitionNormalized = 1.0; // Clamp to max at 3.75 and above
            }
            xMovement = xTransitionNormalized * (definition.segmentLength + definition.tubeWidth);
        }
        
        // Apply combined X and Y translation to first footer broad face bodies
        if (abs(xMovement) > 1e-6 * inch || abs(yMovement) > 1e-6 * inch)
        {
            const firstFooterMoveTransform = transform(vector(xMovement, yMovement, 0 * inch));
            opTransform(context, id + "firstFooterBroadFaceXYTransition", {
                "bodies" : firstFooterBroadFaceBodies,
                "transform" : firstFooterMoveTransform
            });
            println("First footer broad face transition - X: " ~ xMovement ~ ", Y: " ~ yMovement);
        }
        
        // Apply transition sequence for third footer: Move front/back face bodies
        // Range 3.0-3.25: Move +1 x tubeWidth on Y
        // Range 3.25-3.75: Move -1 x segmentLength - 1 x tubeWidth on X
        const thirdFooterId = id + "footer" + "x" + 2;
        const thirdFooterFrontBodies = qBodyType(qCreatedBy(thirdFooterId + "front", EntityType.BODY), BodyType.SOLID);
        const thirdFooterBackBodies = qBodyType(qCreatedBy(thirdFooterId + "back", EntityType.BODY), BodyType.SOLID);
        const thirdFooterBroadFaceBodies = qUnion([thirdFooterFrontBodies, thirdFooterBackBodies]);
        
        // Calculate Y movement: +1 x tubeWidth across range 3.0-3.25
        var thirdFooterYMovement = 0 * inch;
        if (definition.wallRotationNormalized >= 3.0)
        {
            var thirdFooterYTransitionNormalized = (definition.wallRotationNormalized - 3.0) / 0.25; // 0-1 range for 3.0-3.25
            if (thirdFooterYTransitionNormalized > 1.0)
            {
                thirdFooterYTransitionNormalized = 1.0; // Clamp to max at 3.25 and above
            }
            thirdFooterYMovement = thirdFooterYTransitionNormalized * definition.tubeWidth; // Positive Y
        }
        
        // Calculate X movement: -1 x segmentLength - 1 x tubeWidth across range 3.25-3.75
        var thirdFooterXMovement = 0 * inch;
        if (definition.wallRotationNormalized >= 3.25)
        {
            var thirdFooterXTransitionNormalized = (definition.wallRotationNormalized - 3.25) / 0.5; // 0-1 range for 3.25-3.75
            if (thirdFooterXTransitionNormalized > 1.0)
            {
                thirdFooterXTransitionNormalized = 1.0; // Clamp to max at 3.75 and above
            }
            thirdFooterXMovement = -thirdFooterXTransitionNormalized * (definition.segmentLength + definition.tubeWidth); // Negative X
        }
        
        // Apply combined X and Y translation to third footer broad face bodies
        if (abs(thirdFooterXMovement) > 1e-6 * inch || abs(thirdFooterYMovement) > 1e-6 * inch)
        {
            const thirdFooterMoveTransform = transform(vector(thirdFooterXMovement, thirdFooterYMovement, 0 * inch));
            opTransform(context, id + "thirdFooterBroadFaceXYTransition", {
                "bodies" : thirdFooterBroadFaceBodies,
                "transform" : thirdFooterMoveTransform
            });
            println("Third footer broad face transition - X: " ~ thirdFooterXMovement ~ ", Y: " ~ thirdFooterYMovement);
        }
        
        // Apply transition sequence for range 3.75-4: Separate front and back faces
        // All three front face bodies: -4 x tubeWidth on Y
        // All three back face bodies: +4 x tubeWidth on Y
        if (definition.wallRotationNormalized >= 3.75)
        {
            var frontBackTransitionNormalized = (definition.wallRotationNormalized - 3.75) / 0.25; // 0-1 range for 3.75-4
            if (frontBackTransitionNormalized > 1.0)
            {
                frontBackTransitionNormalized = 1.0; // Clamp to max at 4.0 and above
            }
            
            const frontFaceYMovement = frontBackTransitionNormalized * 4 * definition.tubeWidth; // Positive Y for front faces
            const backFaceYMovement = -frontBackTransitionNormalized * 4 * definition.tubeWidth; // Negative Y for back faces
            
            // Collect all front face bodies from all three footers
            var allFrontFaceBodies = [];
            var allBackFaceBodies = [];
            for (var footerIndex = 0; footerIndex < definition.footerTotalOnX; footerIndex += 1)
            {
                const footerId = id + "footer" + "x" + footerIndex;
                
                // Query front face bodies
                const frontFaceBodies = qBodyType(qCreatedBy(footerId + "front", EntityType.BODY), BodyType.SOLID);
                allFrontFaceBodies = append(allFrontFaceBodies, frontFaceBodies);
                
                // Query back face bodies
                const backFaceBodies = qBodyType(qCreatedBy(footerId + "back", EntityType.BODY), BodyType.SOLID);
                allBackFaceBodies = append(allBackFaceBodies, backFaceBodies);
            }
            
            // Apply Y translation to all front face bodies
            if (abs(frontFaceYMovement) > 1e-6 * inch)
            {
                const frontFaceTransform = transform(vector(0 * inch, frontFaceYMovement, 0 * inch));
                opTransform(context, id + "allFrontFacesYTransition", {
                    "bodies" : qUnion(allFrontFaceBodies),
                    "transform" : frontFaceTransform
                });
                println("All front face bodies transition - Y: " ~ frontFaceYMovement);
            }
            
            // Apply Y translation to all back face bodies
            if (abs(backFaceYMovement) > 1e-6 * inch)
            {
                const backFaceTransform = transform(vector(0 * inch, backFaceYMovement, 0 * inch));
                opTransform(context, id + "allBackFacesYTransition", {
                    "bodies" : qUnion(allBackFaceBodies),
                    "transform" : backFaceTransform
                });
                println("All back face bodies transition - Y: " ~ backFaceYMovement);
            }
        }
        
        // Apply transition sequence for range 3-3.75: Translate joiner groups 2-5 on X
        // Joiner groups: Footer 0 left (1st), Footer 0 right (2nd), Footer 1 left (3rd), Footer 1 right (4th), Footer 2 left (5th), Footer 2 right (6th)
        // Translate 2nd & 3rd: +1/2 x segmentLength on X
        // Translate 4th & 5th: -1/2 x segmentLength on X
        if (definition.wallRotationNormalized >= 3.0)
        {
            var joinerTransitionNormalized = (definition.wallRotationNormalized - 3.0) / 0.75; // 0-1 range for 3.0-3.75
            if (joinerTransitionNormalized > 1.0)
            {
                joinerTransitionNormalized = 1.0; // Clamp to max at 3.75 and above
            }
            
            const joinerXMovement = joinerTransitionNormalized * (definition.segmentLength - 1 * inch) / 2;
            
            // Collect joiner bodies for all joiners
            // 1st: Footer 0 left joiner
            const footer0LeftJoinerBodies = qBodyType(qCreatedBy(id + "footer" + "x" + 0 + "leftEndJoiner", EntityType.BODY), BodyType.SOLID);
            // 2nd: Footer 0 right joiner
            const footer0RightJoinerBodies = qBodyType(qCreatedBy(id + "footer" + "x" + 0 + "rightEndJoiner", EntityType.BODY), BodyType.SOLID);
            // 3rd: Footer 1 left joiner
            const footer1LeftJoinerBodies = qBodyType(qCreatedBy(id + "footer" + "x" + 1 + "leftEndJoiner", EntityType.BODY), BodyType.SOLID);
            // 4th: Footer 1 right joiner
            const footer1RightJoinerBodies = qBodyType(qCreatedBy(id + "footer" + "x" + 1 + "rightEndJoiner", EntityType.BODY), BodyType.SOLID);
            // 5th: Footer 2 left joiner
            const footer2LeftJoinerBodies = qBodyType(qCreatedBy(id + "footer" + "x" + 2 + "leftEndJoiner", EntityType.BODY), BodyType.SOLID);
            // 6th: Footer 2 right joiner
            const footer2RightJoinerBodies = qBodyType(qCreatedBy(id + "footer" + "x" + 2 + "rightEndJoiner", EntityType.BODY), BodyType.SOLID);
            
            // Calculate movement for 1st and 6th joiners
            const joiner1XMovement = joinerTransitionNormalized * (1.5 * definition.segmentLength - 1.5 * definition.tubeWidth); // +1.5 x segmentLength - 1.5 x tubeWidth
            const joiner6XMovement = joinerTransitionNormalized * (-1.5 * definition.segmentLength + 1.5 * definition.tubeWidth); // -1.5 x segmentLength + 1.5 x tubeWidth
            
            // Translate 1st joiner: +1.5 x segmentLength - 1.5 x tubeWidth on X
            const joiner1Transform = transform(vector(joiner1XMovement, 0 * inch, 0 * inch));
            opTransform(context, id + "joiner1XTransition", {
                "bodies" : footer0LeftJoinerBodies,
                "transform" : joiner1Transform
            });
            
            // Translate 2nd & 3rd joiners: +(segmentLength - 1)/2 on X
            const joiner23Transform = transform(vector(joinerXMovement, 0 * inch, 0 * inch));
            opTransform(context, id + "joiner23XTransition", {
                "bodies" : qUnion([footer0RightJoinerBodies, footer1LeftJoinerBodies]),
                "transform" : joiner23Transform
            });
            
            // Translate 4th & 5th joiners: -(segmentLength - 1)/2 on X
            const joiner45Transform = transform(vector(-joinerXMovement, 0 * inch, 0 * inch));
            opTransform(context, id + "joiner45XTransition", {
                "bodies" : qUnion([footer1RightJoinerBodies, footer2LeftJoinerBodies]),
                "transform" : joiner45Transform
            });
            
            // Translate 6th joiner: -1.5 x segmentLength + 1.5 x tubeWidth on X
            const joiner6Transform = transform(vector(joiner6XMovement, 0 * inch, 0 * inch));
            opTransform(context, id + "joiner6XTransition", {
                "bodies" : footer2RightJoinerBodies,
                "transform" : joiner6Transform
            });
            
            println("Joiner transition - normalized: " ~ joinerTransitionNormalized ~ ", X movement (2-5): " ~ joinerXMovement);
            println("Joiner 1 X movement: " ~ joiner1XMovement ~ ", Joiner 6 X movement: " ~ joiner6XMovement);
        }
        
        // Apply transition sequence for corner segments: Translate 1st and 2nd corner segment bodies
        // Range 3.0-3.5: Z translations
        //   - 1st corner segment: -5 x tubeWidth on Z
        //   - 2nd corner segment: -8 x tubeWidth on Z
        // Range 3.5-4.0: X translations
        //   - 1st corner segment: +1.5 x segmentLength on X
        //   - 2nd corner segment: -1.5 x segmentLength on X
        // Use existing corner segment IDs (already declared earlier)
        const corner1Bodies = queryAllBodies(corner1SegmentId);
        const corner2Bodies = queryAllBodies(corner2SegmentId);
        
        // Calculate Z movements for range 3.0-3.5
        var corner1ZMovement = 0 * inch;
        var corner2ZMovement = 0 * inch;
        if (definition.wallRotationNormalized >= 3.0)
        {
            var zTransitionNormalized = (definition.wallRotationNormalized - 3.0) / 0.5; // 0-1 range for 3.0-3.5
            if (zTransitionNormalized > 1.0)
            {
                zTransitionNormalized = 1.0; // Clamp to max at 3.5 and above
            }
            corner1ZMovement = -zTransitionNormalized * 5 * definition.tubeWidth; // -5 x tubeWidth for corner 1
            corner2ZMovement = -zTransitionNormalized * 8 * definition.tubeWidth; // -8 x tubeWidth for corner 2
        }
        
        // Calculate X movements for range 3.5-4.0
        var corner1XMovement = 0 * inch;
        var corner2XMovement = 0 * inch;
        if (definition.wallRotationNormalized >= 3.5)
        {
            var xTransitionNormalized = (definition.wallRotationNormalized - 3.5) / 0.5; // 0-1 range for 3.5-4.0
            if (xTransitionNormalized > 1.0)
            {
                xTransitionNormalized = 1.0; // Clamp to max at 4.0 and above
            }
            // At 4.0, xTransitionNormalized should be 1.0, so movement should be 1.0 * segmentLength
            corner1XMovement = xTransitionNormalized * 1.0 * definition.segmentLength; // +1.0 x segmentLength for corner 1
            corner2XMovement = -xTransitionNormalized * 1.0 * definition.segmentLength; // -1.0 x segmentLength for corner 2
        }
        
        // Apply combined Z and X translation to 1st corner segment bodies
        if (abs(corner1ZMovement) > 1e-6 * inch || abs(corner1XMovement) > 1e-6 * inch)
        {
            const corner1Transform = transform(vector(corner1XMovement, 0 * inch, corner1ZMovement));
            opTransform(context, id + "corner1SegmentTransition", {
                "bodies" : corner1Bodies,
                "transform" : corner1Transform
            });
            println("Corner 1 segment transition - X: " ~ corner1XMovement ~ ", Z: " ~ corner1ZMovement);
        }
        
        // Apply combined Z and X translation to 2nd corner segment bodies
        if (abs(corner2ZMovement) > 1e-6 * inch || abs(corner2XMovement) > 1e-6 * inch)
        {
            const corner2Transform = transform(vector(corner2XMovement, 0 * inch, corner2ZMovement));
            opTransform(context, id + "corner2SegmentTransition", {
                "bodies" : corner2Bodies,
                "transform" : corner2Transform
            });
            println("Corner 2 segment transition - X: " ~ corner2XMovement ~ ", Z: " ~ corner2ZMovement);
        }
        
        // Add manipulator for wall-level rotation control (controls all three segments)
        // Position manipulator at center segment location
        const centerSegmentBodies = queryAllBodies(centerSegmentId);
        const centerBodiesArray = evaluateQuery(context, centerSegmentBodies);
        if (size(centerBodiesArray) > 0)
        {
            // Find a body to position the manipulator (use first body or rectangular tube)
            const centerRectTubeQuery = qBodyType(qCreatedBy(centerSegmentId + "rectTubeOuter", EntityType.BODY), BodyType.SOLID);
            const centerRectTubeArray = evaluateQuery(context, centerRectTubeQuery);
            var manipulatorBody;
            if (size(centerRectTubeArray) > 0)
            {
                manipulatorBody = centerRectTubeArray[0];
            }
            else
            {
                manipulatorBody = centerBodiesArray[0];
            }
            
            const manipulatorBodyBox = evBox3d(context, {
                "topology" : manipulatorBody
            });
            
            // Position manipulator at center of center segment, above it
            const manipulatorBaseX = centerOffset[0] + definition.segmentLength / 2;
            const manipulatorBaseY = centerOffset[1];
            const manipulatorBaseZ = manipulatorBodyBox.maxCorner[2] + definition.tubeWidth * 0.5;
            const manipulatorBase = vector(manipulatorBaseX, manipulatorBaseY, manipulatorBaseZ);
            
            const manipulatorDirection = vector(0, 0, 1);
            const manipulatorRange = definition.tubeWidth * 20; // Range for 0-4 normalized (5x per unit)
            const clampedWallRotationNormalized = clamp(definition.wallRotationNormalized, 0, 4);
            const manipulatorOffset = clampedWallRotationNormalized * manipulatorRange / 4;
            
            addManipulators(context, id, {
                "wallRotationManipulator" : linearManipulator({
                    "base" : manipulatorBase,
                    "direction" : manipulatorDirection,
                    "offset" : manipulatorOffset,
                    "minOffset" : 0 * inch,
                    "maxOffset" : manipulatorRange
                })
            });
        }
    }, {
        editingLogic : function(context is Context, definition is map) returns map
        {
            // Override 1 inch values (OnShape's fallback) with correct defaults when editing
            if (definition.tubeWallThickness == 1 * inch)
            {
                definition.tubeWallThickness = 0.0625 * inch;
            }
            
            if (definition.footerFrameWidth == 1 * inch)
            {
                definition.footerFrameWidth = 48 * inch;
            }
            
            if (definition.frameDepth == 1 * inch)
            {
                definition.frameDepth = 12 * inch;
            }
            
            if (definition.footerFrameHeight == 1 * inch)
            {
                definition.footerFrameHeight = 24 * inch;
            }
            
            if (definition.wallFrameWidth == 1 * inch)
            {
                definition.wallFrameWidth = 48 * inch;
            }
            
            if (definition.wallFrameHeight == 1 * inch)
            {
                definition.wallFrameHeight = 96 * inch;
            }
            
            return definition;
        }
    });

// Wall frame creator manipulator change function
// Updates wallRotationNormalized, which controls all three segments simultaneously
export function lbeastWallFrameCreatorOnChange(context is Context, definition is map, newManipulators is map) returns map
{
    // Get the new offset from the manipulator
    if (newManipulators["wallRotationManipulator"] == undefined)
    {
        println("ERROR: wallRotationManipulator not found in newManipulators");
        return definition;
    }
    
    // Get the manipulator range (extended to support 0-4)
    const tubeWidth = definition.tubeWidth == undefined || definition.tubeWidth == 1 * inch ? 1 * inch : definition.tubeWidth;
    const manipulatorRange = tubeWidth * 20; // Range for 0-4 normalized (5x per unit)
    
    // Get the raw offset from the manipulator
    var newOffset = newManipulators["wallRotationManipulator"].offset;
    println("Raw wall manipulator offset: " ~ toString(newOffset));
    println("Manipulator range: " ~ toString(manipulatorRange));
    
    // Handle offset that exceeds the range - if it's at or beyond max, keep it at 4.0
    // If it's below 0, keep it at 0.0
    var normalizedValue;
    if (newOffset >= manipulatorRange)
    {
        // At or beyond maximum - set to 4.0
        normalizedValue = 4.0;
        println("Offset at or beyond maximum, setting normalized to 4.0");
    }
    else if (newOffset <= 0 * inch)
    {
        // At or below minimum - set to 0.0
        normalizedValue = 0.0;
        println("Offset at or below minimum, setting normalized to 0.0");
    }
    else
    {
        // Within range - map normally (0 to manipulatorRange maps to 0 to 4)
        normalizedValue = (newOffset / manipulatorRange) * 4;
        println("Normalized value: " ~ toString(normalizedValue));
    }
    
    // Update wallRotationNormalized
    definition.wallRotationNormalized = clamp(normalizedValue, 0, 4);
    
    // For rotation groups 1-4, only use values 0-2 (clamp segment rotations to 0-2)
    const segmentRotationValue = clamp(definition.wallRotationNormalized, 0, 2);
    definition.cornerSegment1RotationNormalized = segmentRotationValue;
    definition.centerSegmentRotationNormalized = segmentRotationValue;
    definition.cornerSegment2RotationNormalized = segmentRotationValue;
    
    println("Updated wallRotationNormalized: " ~ toString(definition.wallRotationNormalized));
    println("Updated all three segment rotationNormalized values to: " ~ toString(definition.wallRotationNormalized));
    
    return definition;
}

// Helper function to create the composite structure
// Builds at origin (0,0,0) with lowest corner at origin, then applies offset and forward vector
// Returns a map with indexed references to all created objects
function createComposite(context is Context, baseId is Id,
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

annotation { "Feature Type Name" : "Corner Wall Segment",
             "Manipulator Change Function" : "createCornerWallSegmentOnChange" }
export const createCornerWallSegment = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Tube Width" }
        isLength(definition.tubeWidth, { (inch) : [0.1, 1, 10] } as LengthBoundSpec);

        annotation { "Name" : "Tube Wall Thickness" }
        isLength(definition.tubeWallThickness, { (inch) : [0.01, 0.0625, 1] } as LengthBoundSpec);

        annotation { "Name" : "Frame Depth (Y)" }
        isLength(definition.frameDepth, { (inch) : [1, 12, 200] } as LengthBoundSpec);

        annotation { "Name" : "Segment Height" }
        isLength(definition.segmentHeight, { (inch) : [1, 46, 200] } as LengthBoundSpec);

        annotation { "Name" : "Segment Length" }
        isLength(definition.segmentLength, { (inch) : [1, 46, 200] } as LengthBoundSpec);

        annotation { "Name" : "End X Position" }
        isLength(definition.endX, { (inch) : [0, 0.5, 100] } as LengthBoundSpec);
        
        annotation { "Name" : "Offset X" }
        isLength(definition.offsetX, { (inch) : [-1000, 0, 1000] } as LengthBoundSpec);
        
        annotation { "Name" : "Offset Y" }
        isLength(definition.offsetY, { (inch) : [-1000, 0, 1000] } as LengthBoundSpec);
        
        annotation { "Name" : "Offset Z" }
        isLength(definition.offsetZ, { (inch) : [-1000, 0, 1000] } as LengthBoundSpec);
        
        annotation { "Name" : "Facing Direction (degrees)" }
        isAngle(definition.facingDirection, { (degree) : [0, 0, 360] } as AngleBoundSpec);
        
        annotation { "Name" : "Rotation Normalized", "Default" : 0 }
        isReal(definition.rotationNormalized, { (unitless) : [0, 0, 2] } as RealBoundSpec);
    }
    {
        // Set explicit defaults
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
        
        if (definition.frameDepth == undefined || definition.frameDepth == 0 * inch)
        {
            definition.frameDepth = 12 * inch;
        }
        else if (definition.frameDepth == 1 * inch)
        {
            definition.frameDepth = 12 * inch;
        }
        
        if (definition.segmentHeight == undefined || definition.segmentHeight == 0 * inch)
        {
            definition.segmentHeight = 46 * inch;
        }
        else if (definition.segmentHeight == 1 * inch)
        {
            definition.segmentHeight = 46 * inch;
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
            definition.endX = 0.5 * inch; // halfTube for 1" tube
        }
        else if (definition.endX == 1 * inch)
        {
            definition.endX = 0.5 * inch;
        }
        
        // Set defaults for offset and forward vector
        const zero = 0 * inch;
        if (definition.offsetX == undefined || definition.offsetX == 0 * inch)
        {
            definition.offsetX = zero;
        }
        if (definition.offsetY == undefined || definition.offsetY == 0 * inch)
        {
            definition.offsetY = zero;
        }
        if (definition.offsetZ == undefined || definition.offsetZ == 0 * inch)
        {
            definition.offsetZ = zero;
        }
        if (definition.facingDirection == undefined)
        {
            definition.facingDirection = 0 * degree;
        }
        // Normalize to 0-360 range
        definition.facingDirection = normalizeFacingDirection(definition.facingDirection);
        
        // Initialize rotationNormalized if not set
        if (definition.rotationNormalized == undefined)
        {
            definition.rotationNormalized = 0;
        }
        
        const offset = vector(definition.offsetX, definition.offsetY, definition.offsetZ);
        
        // Delegate geometry and rotation logic to shared helper, with manipulator enabled
        createCornerSegmentBodies(context, id,
            definition.tubeWidth, definition.tubeWallThickness,
            definition.frameDepth, definition.segmentHeight, definition.segmentLength,
            definition.endX, offset, definition.facingDirection, definition.rotationNormalized, true);
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
            
            if (definition.segmentHeight == 1 * inch)
            {
                definition.segmentHeight = 46 * inch;
            }
            
            if (definition.endX == 1 * inch)
            {
                definition.endX = 0.5 * inch;
            }
            
            return definition;
        }
    });

annotation { "Feature Type Name" : "Create Center Wall Segment",
             "Manipulator Change Function" : "createCenterWallSegmentOnChange" }
export const createCenterWallSegment = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Tube Width" }
        isLength(definition.tubeWidth, { (inch) : [0.1, 1, 10] } as LengthBoundSpec);

        annotation { "Name" : "Tube Wall Thickness" }
        isLength(definition.tubeWallThickness, { (inch) : [0.01, 0.0625, 1] } as LengthBoundSpec);

        annotation { "Name" : "Frame Depth (Y)" }
        isLength(definition.frameDepth, { (inch) : [1, 12, 200] } as LengthBoundSpec);

        annotation { "Name" : "Segment Height" }
        isLength(definition.segmentHeight, { (inch) : [1, 46, 200] } as LengthBoundSpec);

        annotation { "Name" : "Segment Length" }
        isLength(definition.segmentLength, { (inch) : [1, 46, 200] } as LengthBoundSpec);

        annotation { "Name" : "End X Position" }
        isLength(definition.endX, { (inch) : [0, 0.5, 100] } as LengthBoundSpec);
        
        annotation { "Name" : "Offset X" }
        isLength(definition.offsetX, { (inch) : [-1000, 0, 1000] } as LengthBoundSpec);
        
        annotation { "Name" : "Offset Y" }
        isLength(definition.offsetY, { (inch) : [-1000, 0, 1000] } as LengthBoundSpec);
        
        annotation { "Name" : "Offset Z" }
        isLength(definition.offsetZ, { (inch) : [-1000, 0, 1000] } as LengthBoundSpec);
        
        annotation { "Name" : "Facing Direction (degrees)" }
        isAngle(definition.facingDirection, { (degree) : [0, 0, 360] } as AngleBoundSpec);
        
        annotation { "Name" : "Rotation Normalized", "Default" : 0 }
        isReal(definition.rotationNormalized, { (unitless) : [0, 0, 2] } as RealBoundSpec);
    }
    {
        // Set explicit defaults
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
        
        if (definition.frameDepth == undefined || definition.frameDepth == 0 * inch)
        {
            definition.frameDepth = 12 * inch;
        }
        else if (definition.frameDepth == 1 * inch)
        {
            definition.frameDepth = 12 * inch;
        }
        
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
        
        // Initialize rotationNormalized if not set
        if (definition.rotationNormalized == undefined)
        {
            definition.rotationNormalized = 0;
        }
        
        // Set defaults for offset and facing direction
        const zero = 0 * inch;
        if (definition.offsetX == undefined || definition.offsetX == 0 * inch)
        {
            definition.offsetX = zero;
        }
        if (definition.offsetY == undefined || definition.offsetY == 0 * inch)
        {
            definition.offsetY = zero;
        }
        if (definition.offsetZ == undefined || definition.offsetZ == 0 * inch)
        {
            definition.offsetZ = zero;
        }
        if (definition.facingDirection == undefined)
        {
            definition.facingDirection = 0 * degree;
        }
        // Normalize to 0-360 range
        definition.facingDirection = normalizeFacingDirection(definition.facingDirection);
        
        const offset = vector(definition.offsetX, definition.offsetY, definition.offsetZ);
        
        // Delegate geometry and rotation logic (including rotation groups) to shared helper, with manipulator enabled
        createCenterSegmentBodies(context, id,
            definition.tubeWidth, definition.tubeWallThickness,
            definition.frameDepth, definition.segmentHeight, definition.segmentLength,
            definition.endX, offset, definition.facingDirection, definition.rotationNormalized, true);
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
            
            return definition;
        }
    });

// Shared manipulator change function for both corner and center wall segments
// Converts manipulator offset to rotationNormalized value (0-2 range)
function handleRotationManipulatorChange(context is Context, definition is map, newManipulators is map) returns map
{
    // Get the new offset from the manipulator
    if (newManipulators["rotationManipulator"] == undefined)
    {
        println("ERROR: rotationManipulator not found in newManipulators");
        return definition;
    }
    
    // Get the manipulator range (we'll calculate it the same way)
    const tubeWidth = definition.tubeWidth == undefined || definition.tubeWidth == 1 * inch ? 1 * inch : definition.tubeWidth;
    const manipulatorRange = tubeWidth * 10; // Match the range - 10x for 0-2 normalized (5x per unit)
    
    // Get the raw offset from the manipulator
    var newOffset = newManipulators["rotationManipulator"].offset;
    println("Raw manipulator offset: " ~ toString(newOffset));
    println("Manipulator range: " ~ toString(manipulatorRange));
    
    // Handle offset that exceeds the range - if it's at or beyond max, keep it at 2.0
    // If it's below 0, keep it at 0.0
    var normalizedValue;
    if (newOffset >= manipulatorRange)
    {
        // At or beyond maximum - set to 2.0
        normalizedValue = 2.0;
        println("Offset at or beyond maximum, setting normalized to 2.0");
    }
    else if (newOffset <= 0 * inch)
    {
        // At or below minimum - set to 0.0
        normalizedValue = 0.0;
        println("Offset at or below minimum, setting normalized to 0.0");
    }
    else
    {
        // Within range - map normally (0 to manipulatorRange maps to 0 to 2)
        normalizedValue = (newOffset / manipulatorRange) * 2;
        println("Normalized value: " ~ toString(normalizedValue));
    }
    
    // Ensure the value is in valid range (should already be, but double-check)
    definition.rotationNormalized = clamp(normalizedValue, 0, 2);
    println("Updated rotationNormalized: " ~ toString(definition.rotationNormalized));
    
    return definition;
}

// Center wall segment manipulator change function
export function createCenterWallSegmentOnChange(context is Context, definition is map, newManipulators is map) returns map
{
    println("createCenterWallSegmentOnChange called!");
    return handleRotationManipulatorChange(context, definition, newManipulators);
}

// Corner wall segment manipulator change function
export function createCornerWallSegmentOnChange(context is Context, definition is map, newManipulators is map) returns map
{
    println("createCornerWallSegmentOnChange called!");
    return handleRotationManipulatorChange(context, definition, newManipulators);
}

// Helper function to create an upper frame piece
// Creates an upper frame piece by rotating a joiner 90 degrees around X and translating it to worldspace origin
// Returns a map with references to the flat bars after all transformations
function createUpperFramePiece(context is Context, baseId is Id,
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

// Helper function to create an end face joiner
// Creates a joiner assembly consisting of two horizontal Y-direction tubes that form one end face of the footer
// This joiner will be welded together as a single composite part
// Each joiner makes up one end face (left or right) of the footer
// The two tubes run horizontally (Y-direction) from front to back, connecting the two broad side faces
// Returns a map with references to the created flat bars after transformation
// isFooterJoiner: if true, applies footer-specific depth adjustments (shorten tubes and adjust flat bar position)
function createEndFaceJoiner(context is Context, baseId is Id,
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

// Helper function to find rotation axis from a body's top face inner long edge
// Returns a map with "axisLine" (Line) and "found" (boolean)
function findRotationAxisFromBody(context is Context, body is Query, bodyIndex is number, useLargerX is boolean) returns map
{
    // Find the top face of the body
    const allFaces = qOwnedByBody(qBodyType(qEntityFilter(body, EntityType.BODY), BodyType.SOLID), EntityType.FACE);
    const faceArray = evaluateQuery(context, allFaces);
    
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
    
    if (topFace == undefined)
    {
        println("ERROR: Could not find top face of body at index " ~ bodyIndex);
        return { "axisLine" : undefined, "found" : false };
    }
    
    // Get all edges of the body, then filter to find edges on the top face
    const allBodyEdges = qOwnedByBody(qBodyType(qEntityFilter(body, EntityType.BODY), BodyType.SOLID), EntityType.EDGE);
    const allEdgeArray = evaluateQuery(context, allBodyEdges);
    
    // Get the top face plane to check which edges are on it
    const topFacePlane = evPlane(context, {
        "face" : topFace
    });
    
    // Filter edges that are on the top face (check if edge midpoint is on the face plane)
    var topFaceEdges = [];
    for (var e = 0; e < size(allEdgeArray); e += 1)
    {
        const edge = allEdgeArray[e];
        const edgeBox = evBox3d(context, {
            "topology" : edge
        });
        const edgeMidpoint = (edgeBox.minCorner + edgeBox.maxCorner) / 2;
        
        // Check if edge midpoint is on the top face plane (within tolerance)
        const distanceToPlane = abs(dot(topFacePlane.normal, edgeMidpoint - topFacePlane.origin));
        if (distanceToPlane < 1e-6 * meter)
        {
            topFaceEdges = append(topFaceEdges, edge);
        }
    }
    
    if (size(topFaceEdges) < 2)
    {
        println("ERROR: Could not find at least 2 edges on top face of body at index " ~ bodyIndex);
        return { "axisLine" : undefined, "found" : false };
    }
    
    // Find the longest edges (there should be 2 long edges and 2 short edges)
    var edgesWithLength = [];
    for (var e = 0; e < size(topFaceEdges); e += 1)
    {
        const edge = topFaceEdges[e];
        const edgeBox = evBox3d(context, {
            "topology" : edge
        });
        // Calculate edge length from bounding box dimensions
        const edgeVector = edgeBox.maxCorner - edgeBox.minCorner;
        const edgeLength = norm(edgeVector);
        edgesWithLength = append(edgesWithLength, {
            "edge" : edge,
            "length" : edgeLength
        });
    }
    
    // Sort by length (descending - longest first)
    edgesWithLength = sort(edgesWithLength, function(a, b) { return b.length.value - a.length.value; });
    
    // The two longest edges are the long edges
    const longestEdge = edgesWithLength[0].edge;
    const secondLongestEdge = edgesWithLength[1].edge;
    
    // Get the center points of both long edges to determine which is "inner"
    const longestEdgeBox = evBox3d(context, {
        "topology" : longestEdge
    });
    const secondLongestEdgeBox = evBox3d(context, {
        "topology" : secondLongestEdge
    });
    
    const longestEdgeCenterX = (longestEdgeBox.minCorner[0] + longestEdgeBox.maxCorner[0]) / 2;
    const secondLongestEdgeCenterX = (secondLongestEdgeBox.minCorner[0] + secondLongestEdgeBox.maxCorner[0]) / 2;
    
    // Select "inner" edge based on useLargerX parameter
    var innerLongEdge;
    if (useLargerX)
    {
        // The "inner" edge is the one with the LARGER X coordinate
        innerLongEdge = longestEdgeCenterX > secondLongestEdgeCenterX ? longestEdge : secondLongestEdge;
    }
    else
    {
        // The "inner" edge is the one with the SMALLER X coordinate (mirrored side)
        innerLongEdge = longestEdgeCenterX < secondLongestEdgeCenterX ? longestEdge : secondLongestEdge;
    }
    
    // Get the rotation axis from the edge
    // Get the actual vertices of the edge to construct an accurate axis line
    const edgeVertices = evaluateQuery(context, qAdjacent(innerLongEdge, AdjacencyType.VERTEX, EntityType.VERTEX));
    
    var axisLine;
    if (size(edgeVertices) >= 2)
    {
        // Get the actual vertex points (these are the real endpoints of the edge)
        const vertex0Point = evVertexPoint(context, {
            "vertex" : edgeVertices[0]
        });
        const vertex1Point = evVertexPoint(context, {
            "vertex" : edgeVertices[1]
        });
        
        // Create the axis line from the actual edge endpoints
        const edgeDirection = normalize(vertex1Point - vertex0Point);
        axisLine = line(vertex0Point, edgeDirection);
    }
    else
    {
        // Fallback to bounding box method if we can't get vertices
        const innerLongEdgeBox = evBox3d(context, {
            "topology" : innerLongEdge
        });
        const edgeStart = innerLongEdgeBox.minCorner;
        const edgeEnd = innerLongEdgeBox.maxCorner;
        const edgeDirection = normalize(edgeEnd - edgeStart);
        axisLine = line(edgeStart, edgeDirection);
        println("WARNING: Could not get edge vertices for body " ~ bodyIndex ~ ", using bounding box fallback");
    }
    
    return { "axisLine" : axisLine, "found" : true };
}

// Helper function to apply rotation groups 1 and 2 to bodies
// This function can be used by both corner and center wall segments
// Parameters:
//   - context: FeatureScript context
//   - id: Feature ID
//   - definition: Feature definition map (must contain rotationNormalized, facingDirection, segmentLength, tubeWidth)
//   - allBodiesArray: Array of body entities after facingDirection rotation
//   - facingDirectionRotationTransform: Transform for facingDirection rotation
//   - rectTubeIndex: Index of rectangular tube in allBodiesArray (or -1 if not found)
//   - addManipulator: Whether to add the rotation manipulator (true for center wall segment, false for corner)
function applyRotationGroups1And2(context is Context, id is Id, definition is map, 
    allBodiesArray is array, facingDirectionRotationTransform is Transform, 
    rectTubeIndex is number, addManipulator is boolean)
{
    // Get rotationNormalized value, defaulting to 0
    const rotationNormalized = definition.rotationNormalized == undefined ? 0 : definition.rotationNormalized;
    println("Initial rotationNormalized from definition: " ~ rotationNormalized);
    
    // Calculate rotation angles for groups 1 and 2
    // First rotation: map 0-0.5 normalized to 0-180 degrees for indices 0-3
    var firstRotationAngle = 0 * degree;
    if (rotationNormalized <= 0.5)
    {
        const firstNormalized = rotationNormalized / 0.5; // 0-1 range for first rotation
        firstRotationAngle = firstNormalized * 180 * degree; // Positive for clockwise
        println("First rotation active - normalized: " ~ firstNormalized ~ ", angle: " ~ firstRotationAngle);
    }
    else
    {
        firstRotationAngle = 180 * degree;
        println("First rotation at maximum (180 degrees)");
    }
    
    // Second rotation: map 0.5-1.0 normalized to 0-90 degrees for indices 0-7
    var secondRotationAngle = 0 * degree;
    if (rotationNormalized > 0.5 && rotationNormalized <= 1.0)
    {
        const secondNormalized = (rotationNormalized - 0.5) / 0.5; // 0-1 range for second rotation
        secondRotationAngle = secondNormalized * 90 * degree; // Positive for clockwise
        println("Second rotation active - normalized: " ~ secondNormalized ~ ", angle: " ~ secondRotationAngle);
    }
    else if (rotationNormalized > 1.0)
    {
        secondRotationAngle = 90 * degree;
        println("Second rotation at maximum (90 degrees, clamped)");
    }
    else
    {
        println("Second rotation inactive (at 0 degrees)");
    }
    
    // Find rotation axes from rotated bodies
    const targetBodyIndex = 2; // Index for first rotation axis
    const secondTargetBodyIndex = 6; // Index for second rotation axis
    
    var rotationAxisLine;
    var rotationAxisFound = false;
    var secondRotationAxisLine;
    var secondRotationAxisFound = false;
    
    // Find first rotation axis (index 2)
    if (size(allBodiesArray) > targetBodyIndex)
    {
        const targetBody = allBodiesArray[targetBodyIndex];
        const axisResult = findRotationAxisFromBody(context, targetBody, targetBodyIndex, true); // true = use larger X (inner edge)
        if (axisResult.found)
        {
            rotationAxisLine = axisResult.axisLine;
            rotationAxisFound = true;
            
            // Add manipulator if requested (only for center wall segment)
            if (addManipulator)
            {
                const targetBodyBox = evBox3d(context, {
                    "topology" : targetBody
                });
                
                // Position manipulator at 1/2 segmentLength along X, adjusted for facingDirection
                const manipulatorBaseX = definition.segmentLength / 2;
                const manipulatorBaseY = 0 * inch;
                const manipulatorBaseZ = targetBodyBox.maxCorner[2] + definition.tubeWidth * 0.5;
                const manipulatorBasePosition = vector(manipulatorBaseX, manipulatorBaseY, manipulatorBaseZ);
                
                // Apply facingDirection rotation to the manipulator position
                const manipulatorBase = facingDirectionRotationTransform * manipulatorBasePosition;
                
                const manipulatorDirection = vector(0, 0, 1);
                const manipulatorRange = definition.tubeWidth * 10;
                const clampedRotationNormalized = clamp(rotationNormalized, 0, 2);
                const manipulatorOffset = clampedRotationNormalized * manipulatorRange / 2;
                println("Setting manipulator - rotationNormalized: " ~ clampedRotationNormalized ~ ", offset: " ~ manipulatorOffset ~ ", range: " ~ manipulatorRange);
                
                addManipulators(context, id, {
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
        else
        {
            rotationAxisFound = false;
        }
    }
    else
    {
        println("ERROR: Target body index " ~ targetBodyIndex ~ " is out of range (only " ~ size(allBodiesArray) ~ " bodies found)");
        rotationAxisFound = false;
    }
    
    // Find second rotation axis (index 6)
    if (size(allBodiesArray) > secondTargetBodyIndex)
    {
        const secondTargetBody = allBodiesArray[secondTargetBodyIndex];
        const axisResult = findRotationAxisFromBody(context, secondTargetBody, secondTargetBodyIndex, true); // true = use larger X (inner edge)
        if (axisResult.found)
        {
            secondRotationAxisLine = axisResult.axisLine;
            secondRotationAxisFound = true;
        }
        else
        {
            secondRotationAxisFound = false;
        }
    }
    else
    {
        println("ERROR: Index 6 is out of range (only " ~ size(allBodiesArray) ~ " bodies found)");
        secondRotationAxisFound = false;
    }
    
    // Apply first rotation to indices 0-3
    if (rotationAxisFound && size(allBodiesArray) >= 4)
    {
        const firstRotationTransform = rotationAround(rotationAxisLine, firstRotationAngle);
        
        println("Applying first rotation to " ~ 4 ~ " bodies (indices 0-3), angle: " ~ firstRotationAngle);
        
        const firstBodiesToRotate = qUnion([
            qBodyType(allBodiesArray[0], BodyType.SOLID),
            qBodyType(allBodiesArray[1], BodyType.SOLID),
            qBodyType(allBodiesArray[2], BodyType.SOLID),
            qBodyType(allBodiesArray[3], BodyType.SOLID)
        ]);
        
        println("First rotation axis line: " ~ toString(rotationAxisLine));
        
        opTransform(context, id + "rotateTubesFirst", {
            "bodies" : firstBodiesToRotate,
            "transform" : firstRotationTransform
        });
    }
    else
    {
        if (!rotationAxisFound)
        {
            println("ERROR: First rotation axis not found, cannot apply first rotation");
        }
        else
        {
            println("ERROR: Not enough bodies for first rotation (need 4, have " ~ size(allBodiesArray) ~ ")");
        }
    }
    
    // Apply second rotation to indices 0-7, excluding the rectangular tube
    if (secondRotationAxisFound && size(allBodiesArray) >= 8)
    {
        const secondRotationTransform = rotationAround(secondRotationAxisLine, secondRotationAngle);
        
        // Build the list of bodies to rotate: indices 0-7 plus index 10, excluding the rectangular tube
        var secondBodiesToRotateList = [];
        for (var i = 0; i < 8; i += 1)
        {
            // Exclude the rectangular tube if it's in the range 0-7
            if (rectTubeIndex < 0 || i != rectTubeIndex)
            {
                secondBodiesToRotateList = append(secondBodiesToRotateList, 
                    qBodyType(allBodiesArray[i], BodyType.SOLID));
            }
        }
        // Add index 10 if it exists and is not the rectangular tube
        if (size(allBodiesArray) > 10 && (rectTubeIndex < 0 || 10 != rectTubeIndex))
        {
            secondBodiesToRotateList = append(secondBodiesToRotateList, 
                qBodyType(allBodiesArray[10], BodyType.SOLID));
        }
        
        const secondBodiesToRotate = qUnion(secondBodiesToRotateList);
        
        println("Applying second rotation to " ~ size(secondBodiesToRotateList) ~ " bodies (indices 0-7 plus index 10, excluding rectangular tube at index " ~ rectTubeIndex ~ "), angle: " ~ secondRotationAngle);
        
        println("Second rotation axis line: " ~ toString(secondRotationAxisLine));
        
        opTransform(context, id + "rotateTubesSecond", {
            "bodies" : secondBodiesToRotate,
            "transform" : secondRotationTransform
        });
    }
    else
    {
        if (!secondRotationAxisFound)
        {
            println("ERROR: Second rotation axis not found, cannot apply second rotation");
        }
        else
        {
            println("ERROR: Not enough bodies for second rotation (need 8, have " ~ size(allBodiesArray) ~ ")");
        }
    }
}

// Helper function to apply rotation groups 3 and 4 to bodies (center segments)
function applyRotationGroups3And4(context is Context, id is Id, rotationNormalized is number, allBodiesArray is array)
{
    // Declare rotation axis variables for groups 3 and 4
    var thirdRotationAxisLine;
    var thirdRotationAxisFound = false;
    var fourthRotationAxisLine;
    var fourthRotationAxisFound = false;
    
    // Calculate rotation angles for groups 3 and 4
    // Third rotation: map 1.0-1.5 normalized to 0-180 degrees for indices 11-14
    var thirdRotationAngle = 0 * degree;
    if (rotationNormalized > 1.0 && rotationNormalized <= 1.5)
    {
        const thirdNormalized = (rotationNormalized - 1.0) / 0.5; // 0-1 range for third rotation
        thirdRotationAngle = -thirdNormalized * 180 * degree; // Negative for counter-clockwise
        println("Third rotation active - normalized: " ~ thirdNormalized ~ ", angle: " ~ thirdRotationAngle);
    }
    else if (rotationNormalized > 1.5)
    {
        thirdRotationAngle = -180 * degree;
        println("Third rotation at maximum (180 degrees)");
    }
    
    // Fourth rotation: map 1.5-2.0 normalized to 0-90 degrees for indices 11-18
    var fourthRotationAngle = 0 * degree;
    if (rotationNormalized > 1.5 && rotationNormalized <= 2.0)
    {
        const fourthNormalized = (rotationNormalized - 1.5) / 0.5; // 0-1 range for fourth rotation
        fourthRotationAngle = -fourthNormalized * 90 * degree; // Negative for counter-clockwise
        println("Fourth rotation active - normalized: " ~ fourthNormalized ~ ", angle: " ~ fourthRotationAngle);
    }
    else if (rotationNormalized > 2.0)
    {
        fourthRotationAngle = -90 * degree;
        println("Fourth rotation at maximum (90 degrees, clamped)");
    }
    
    // Find rotation axis for group 3 (index 13)
    const thirdTargetBodyIndex = 13;
    if (size(allBodiesArray) > thirdTargetBodyIndex)
    {
        const thirdTargetBody = allBodiesArray[thirdTargetBodyIndex];
        const axisResult = findRotationAxisFromBody(context, thirdTargetBody, thirdTargetBodyIndex, false); // false = use smaller X
        if (axisResult.found)
        {
            thirdRotationAxisLine = axisResult.axisLine;
            thirdRotationAxisFound = true;
        }
    }
    
    // Find rotation axis for group 4 (index 17)
    const fourthTargetBodyIndex = 17;
    if (size(allBodiesArray) > fourthTargetBodyIndex)
    {
        const fourthTargetBody = allBodiesArray[fourthTargetBodyIndex];
        const axisResult = findRotationAxisFromBody(context, fourthTargetBody, fourthTargetBodyIndex, false); // false = use smaller X
        if (axisResult.found)
        {
            fourthRotationAxisLine = axisResult.axisLine;
            fourthRotationAxisFound = true;
        }
    }
    
    // Apply third rotation to indices 11-14
    if (thirdRotationAxisFound && size(allBodiesArray) >= 15)
    {
        const thirdRotationTransform = rotationAround(thirdRotationAxisLine, thirdRotationAngle);
        println("Applying third rotation to 4 bodies (indices 11-14), angle: " ~ thirdRotationAngle);
        
        const thirdBodiesToRotate = qUnion([
            qBodyType(allBodiesArray[11], BodyType.SOLID),
            qBodyType(allBodiesArray[12], BodyType.SOLID),
            qBodyType(allBodiesArray[13], BodyType.SOLID),
            qBodyType(allBodiesArray[14], BodyType.SOLID)
        ]);
        
        opTransform(context, id + "rotateTubesThird", {
            "bodies" : thirdBodiesToRotate,
            "transform" : thirdRotationTransform
        });
    }
    
    // Apply fourth rotation to indices 11-18
    if (fourthRotationAxisFound && size(allBodiesArray) >= 19)
    {
        const fourthRotationTransform = rotationAround(fourthRotationAxisLine, fourthRotationAngle);
        println("Applying fourth rotation to 8 bodies (indices 11-18), angle: " ~ fourthRotationAngle);
        
        const fourthBodiesToRotate = qUnion([
            qBodyType(allBodiesArray[11], BodyType.SOLID),
            qBodyType(allBodiesArray[12], BodyType.SOLID),
            qBodyType(allBodiesArray[13], BodyType.SOLID),
            qBodyType(allBodiesArray[14], BodyType.SOLID),
            qBodyType(allBodiesArray[15], BodyType.SOLID),
            qBodyType(allBodiesArray[16], BodyType.SOLID),
            qBodyType(allBodiesArray[17], BodyType.SOLID),
            qBodyType(allBodiesArray[18], BodyType.SOLID)
        ]);
        
        opTransform(context, id + "rotateTubesFourth", {
            "bodies" : fourthBodiesToRotate,
            "transform" : fourthRotationTransform
        });
    }
}

// Helper function to create both broad side faces of a frame (front and back)

// Helper function to create a frame set (duplicated frames along X axis)
// @deprecated This function is no longer used - wall frames now use createCornerSegmentBodies and createCenterSegmentBodies
// Helper function to create a single broad side face of a frame
// Creates 4 pieces: 2 horizontal tubes (top and bottom) and 2 vertical posts (left and right corners)
// This forms one of the large rectangular faces (width x height) of the frame
function createBroadSideFace(context is Context, baseId is Id,
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

// Helper function to create corner segment bodies
// This contains the core logic from createCornerWallSegment feature
// Can be called from both the feature definition and the wall creator
function createCornerSegmentBodies(context is Context, baseId is Id,
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
    
    // Create definition map for rotation helper function
    var segmentDefinition = {
        "rotationNormalized" : rotationNormalized,
        "facingDirection" : facingDirection,
        "segmentLength" : segmentLength,
        "tubeWidth" : tubeWidth
    };
    
    // Apply rotation groups 1 and 2 using helper function
    // addManipulator controls whether a manipulator is created (true for feature, false for wall creator)
    applyRotationGroups1And2(context, baseId, segmentDefinition, allBodiesArray, facingDirectionRotationTransform, rectTubeIndex, addManipulator);
}

// Helper function to create center segment bodies
// This contains the core logic from createCenterWallSegment feature
// Can be called from both the feature definition and the wall creator
function createCenterSegmentBodies(context is Context, baseId is Id,
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
    
    // Apply rotation groups 1 and 2 using helper function
    // addManipulator controls whether a manipulator is created (true for feature, false for wall creator)
    applyRotationGroups1And2(context, baseId, segmentDefinition, allBodiesArray, facingDirectionRotationTransform, rectTubeIndex, addManipulator);
    
    // Apply rotation groups 3 and 4 for center segments using shared helper
    applyRotationGroups3And4(context, baseId, rotationNormalized, allBodiesArray);
}

// Helper function to create both broad side faces of a frame (front and back)
// Also creates end face joiners for both left and right ends to complete the footer
function createBothBroadSideFaces(context is Context, baseId is Id,
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

// Helper function to create a single tube (local copy, now deprecated in favor of LBEASTWallUtil.createTube)
function createTube_local(context is Context, id is Id, startPoint is Vector, endPoint is Vector,
    halfTube is ValueWithUnits, halfInner is ValueWithUnits, tubeWidth is ValueWithUnits, wallThickness is ValueWithUnits)
{
    const delta = endPoint - startPoint;
    const direction = normalize(delta);
    const length = norm(delta);
    
    // Determine sketch plane based on direction
    // For axis-aligned directions, use world coordinate planes
    // Normalized direction components are unitless, so we can compare directly
    var sketchPlane = plane(startPoint, vector(0, 0, 1)); // Default to XY plane
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
}
