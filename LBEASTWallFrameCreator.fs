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
import(path : "1a352bc5f15cd57be34e8ae2", version : "000000000000000000000000"); // LBEASTWallUtil
// TODO: Update with actual Element ID for LBEASTWallComponents when Feature Studio is created
import(path : "PLACEHOLDER_COMPONENTS_ELEMENT_ID", version : "000000000000000000000000"); // LBEASTWallComponents

annotation { "Feature Type Name" : "LBEAST Wall Frame Creator",
             "Manipulator Change Function" : "lbeastWallFrameCreatorOnChange" }
export const lbeastWallFrameCreator = defineFeature(function(context is Context, id is Id, definition is map)
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
        
        annotation { "Name" : "Footer Total on X" }
        isInteger(definition.footerTotalOnX, { (unitless) : [1, 3, 20] } as IntegerBoundSpec);
        
        annotation { "Name" : "Wall Frame Width (X)" }
        isLength(definition.wallFrameWidth, { (inch) : [1, 48, 200] } as LengthBoundSpec);
        
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
        
        // Wall-level rotation parameter (required for manipulator)
        // Range 0-2: Rotation groups 1-4 for segments
        // Range 2-3: Footer broad face transition
        // Range 3-4: (reserved for future transitions)
        // Note: Physical manipulator range increased to 32" (for 1" tube) for dampened sensitivity
        // 2" movement = 1/16th of full transition (0.25 of 0-4 range)
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
        
        // Calculate footerFrameWidth from segmentLength + 1 * tubeWidth
        // This keeps footer and segment widths in sync since they're stacked 1-to-1
        definition.footerFrameWidth = definition.segmentLength + 1 * definition.tubeWidth;
        
        // Calculate wallFrameHeight from segmentHeight + footerFrameHeight
        // This represents the total frame height (footer + segment)
        definition.wallFrameHeight = definition.segmentHeight + definition.footerFrameHeight;
        
        // Wall-level facing direction defaults
        if (definition.wallFacingDirection == undefined)
        {
            definition.wallFacingDirection = 0 * degree;
        }
        definition.wallFacingDirection = normalizeFacingDirection(definition.wallFacingDirection);
        
        // Rotation normalized defaults (internal only, not exposed to user)
        if (definition.wallRotationNormalized == undefined)
        {
            definition.wallRotationNormalized = 0;
        }
        
        // Calculate individual segment rotation values from wallRotationNormalized
        // These are computed locally, not stored in definition (to avoid "Unknown parameter" errors)
        // For rotation groups 1-4, only use values 0-2 (clamp segment rotations to 0-2)
        const segmentRotationValue = clamp(definition.wallRotationNormalized, 0, 2);
        const cornerSegment1RotationNormalized = segmentRotationValue;
        const centerSegmentRotationNormalized = segmentRotationValue;
        const cornerSegment2RotationNormalized = segmentRotationValue;
        
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
            definition.endX, corner1Offset, corner1FacingDirection, cornerSegment1RotationNormalized, false);
        
        // Instantiate center segment above footer 1 (second footer)
        // Position: X = footerFrameWidth, Y = -0.5*tubeWidth, Z = footerFrameHeight - 0.5*tubeWidth, facingDirection = 0
        const centerSegmentId = id + "centerSegment";
        const centerOffset = vector(definition.footerFrameWidth, -0.5 * definition.tubeWidth, definition.footerFrameHeight - 0.5 * definition.tubeWidth);
        const centerFacingDirection = definition.wallFacingDirection; // No additional rotation
        createCenterSegmentBodies(context, centerSegmentId,
            definition.tubeWidth, definition.tubeWallThickness,
            definition.frameDepth, definition.segmentHeight, definition.segmentLength,
            definition.endX, centerOffset, centerFacingDirection, centerSegmentRotationNormalized, false);
        
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
            definition.endX, corner2Offset, definition.wallFacingDirection, cornerSegment2RotationNormalized, false);
        
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
            // Target: 2" movement = 1/16th of full transition (0.25 of 0-4 range)
            // Full transition = 2" * 16 = 32" physical range
            const manipulatorRange = definition.tubeWidth * 32; // Physical range: 32" for 1" tube (2" = 1/16th of transition)
            const functionalRange = 4.0; // The functional range for transitions (0-4)
            const clampedWallRotationNormalized = clamp(definition.wallRotationNormalized, 0, functionalRange);
            const manipulatorOffset = clampedWallRotationNormalized * manipulatorRange / functionalRange;
            
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
            
            // Recalculate footerFrameWidth and wallFrameHeight from their source parameters
            // This ensures they stay in sync even if segmentLength or segmentHeight change
            if (definition.segmentLength != undefined && definition.tubeWidth != undefined)
            {
                definition.footerFrameWidth = definition.segmentLength + 1 * definition.tubeWidth;
            }
            if (definition.segmentHeight != undefined && definition.footerFrameHeight != undefined)
            {
                definition.wallFrameHeight = definition.segmentHeight + definition.footerFrameHeight;
            }
            
            // Initialize wallRotationNormalized if not set
            if (definition.wallRotationNormalized == undefined)
            {
                definition.wallRotationNormalized = 0;
            }
            
            // Note: Individual segment rotation parameters are calculated in main function body
            // from wallRotationNormalized - they are not set here to avoid "Unknown parameter" errors
            
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
        // println("ERROR: wallRotationManipulator not found in newManipulators");
        return definition;
    }
    
    // Get the manipulator range
    // Target: 2" movement = 1/16th of full transition (0.25 of 0-4 range)
    // Full transition = 2" * 16 = 32" physical range
    const tubeWidth = definition.tubeWidth == undefined || definition.tubeWidth == 1 * inch ? 1 * inch : definition.tubeWidth;
    const manipulatorRange = tubeWidth * 32; // Physical range: 32" for 1" tube (2" = 1/16th of transition)
    const functionalRange = 4.0; // The functional range for transitions (0-4)
    
    // Get the raw offset from the manipulator
    var newOffset = newManipulators["wallRotationManipulator"].offset;
    // println("Raw wall manipulator offset: " ~ toString(newOffset));
    // println("Manipulator range: " ~ toString(manipulatorRange));
    
    // Handle offset that exceeds the range - if it's at or beyond max, keep it at functionalRange
    // If it's below 0, keep it at 0.0
    var normalizedValue;
    if (newOffset >= manipulatorRange)
    {
        // At or beyond maximum - set to functionalRange
        normalizedValue = functionalRange;
        // println("Offset at or beyond maximum, setting normalized to " ~ toString(functionalRange));
    }
    else if (newOffset <= 0 * inch)
    {
        // At or below minimum - set to 0.0
        normalizedValue = 0.0;
        // println("Offset at or below minimum, setting normalized to 0.0");
    }
    else
    {
        // Within range - map directly to functional range (0-4)
        // 2" movement maps to (2/32) * 4.0 = 0.25 (1/16th of full transition)
        normalizedValue = (newOffset / manipulatorRange) * functionalRange;
        // println("Normalized value: " ~ toString(normalizedValue));
    }
    
    // Update wallRotationNormalized (only parameter in precondition)
    // Individual segment rotation parameters are calculated in main function body from wallRotationNormalized
    definition.wallRotationNormalized = clamp(normalizedValue, 0, functionalRange);
    
    // println("Updated wallRotationNormalized: " ~ toString(definition.wallRotationNormalized));
    
    return definition;
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
// handleRotationManipulatorChange moved to LBEASTWallUtil.fs

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


// Helper function to create an end face joiner
// Creates a joiner assembly consisting of two horizontal Y-direction tubes that form one end face of the footer
// This joiner will be welded together as a single composite part

// findRotationAxisFromBody moved to LBEASTWallUtil.fs

// Helper function to create both broad side faces of a frame (front and back)

// Helper function to create a frame set (duplicated frames along X axis)




// (deprecated local tube helper removed; use createTube from LBEASTWallUtil instead)
