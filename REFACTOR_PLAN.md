# LBEAST Wall Frame Creator - 3-File Refactor Plan

## Goal
Refactor into exactly 3 files, each under 1000 lines:
1. `LBEASTWallFrameCreator.fs` - Main entry point
2. `LBEASTWallUtil.fs` - Utilities and helpers
3. `LBEASTWallComponents.fs` - Component builders

## Current State
- `LBEASTWallFrameCreator.fs`: 2,486 lines
- `LBEASTWallUtil.fs`: 270 lines

## Target Distribution

### 1. LBEASTWallFrameCreator.fs (~780 lines)
**Purpose**: Main feature definition, orchestration, and manipulator callbacks

**Contents**:
- Feature definition and precondition (~80 lines)
  - `lbeastWallFrameCreator` feature with all parameter definitions
  - `editingLogic` function
- Main feature body orchestration (~550 lines)
  - Default value handling
  - Footer frame creation loops
  - Segment instantiation (corner1, center, corner2)
  - Transition sequences (wallRotationNormalized ranges 2-4)
  - Manipulator setup
- Manipulator callbacks (~150 lines)
  - `lbeastWallFrameCreatorOnChange` (export)
  - `createCenterWallSegmentOnChange` (export)
  - `createCornerWallSegmentOnChange` (export)

**Dependencies**: Imports `LBEASTWallUtil` and `LBEASTWallComponents`

---

### 2. LBEASTWallUtil.fs (~415 lines, currently 270)
**Purpose**: Core utilities, rotation helpers, and query functions

**Current Contents** (270 lines):
- `normalizeFacingDirection` (export)
- `queryAllBodies` (export)
- `createTube` (export)
- Rotation builder pattern:
  - `newRotationGroupBuilder` (export)
  - `withNormalizedRange` (export)
  - `withMaxAngle` (export)
  - `withClockwise` (export)
  - `withBodyIndices` (export)
  - `withAxisLine` (export)
  - `withOpSuffix` (export)
  - `applyRotationGroupBuilder` (export)

**To Add** (~145 lines):
- `findRotationAxisFromBody` (export) - Finds rotation axis from body's top face
- `handleRotationManipulatorChange` (export) - Shared manipulator change logic

**Dependencies**: Only Onshape standard libraries

---

### 3. LBEASTWallComponents.fs (~1,290 lines)
**Purpose**: All component creation functions (composites, segments, footers)

**Contents**:
- `createComposite` (export) (~320 lines)
  - Creates the base composite structure (two upper frame pieces, rectangular tube, cross members)
  - Returns map with flatBars metadata
- `createUpperFramePiece` (export) (~90 lines)
  - Creates a single upper frame piece with joiner
  - Returns map with flatBar references
- `createEndFaceJoiner` (export) (~180 lines)
  - Creates end face joiner with horizontal tubes and flat bars
  - Returns map with flatBar references
- `createCornerSegmentBodies` (export) (~180 lines)
  - Creates corner segment with rotation logic
  - Calls `createComposite`, applies facing direction, handles rotation groups
- `createCenterSegmentBodies` (export) (~420 lines)
  - Creates center segment with duplication logic
  - Calls `createComposite`, deletes second purple cross member, patterns bodies
- `createBroadSideFace` (export) (~40 lines)
  - Creates a single broad side face (front or back) with tubes and posts
- `createBothBroadSideFaces` (export) (~40 lines)
  - Creates both front and back broad side faces plus end joiners

**Dependencies**: Imports `LBEASTWallUtil` for `createTube`, `queryAllBodies`, rotation builders

**Note**: This file will be ~1,290 lines, which exceeds the 1000-line target. However, these functions are tightly coupled and splitting them further would create more files than desired. We can optimize later if needed by:
- Extracting common patterns within functions
- Consolidating duplicate logic
- Further refactoring large functions like `createCenterSegmentBodies`

---

## Migration Steps

### Step 1: Move rotation helpers to Util
1. Copy `findRotationAxisFromBody` from `LBEASTWallFrameCreator.fs` to `LBEASTWallUtil.fs`
2. Copy `handleRotationManipulatorChange` from `LBEASTWallFrameCreator.fs` to `LBEASTWallUtil.fs`
3. Mark both as `export`
4. Update `LBEASTWallFrameCreator.fs` to import and use these functions
5. Delete original functions from `LBEASTWallFrameCreator.fs`

### Step 2: Create LBEASTWallComponents.fs
1. Create new Feature Studio in Onshape
2. Add imports: Onshape standard libraries + `LBEASTWallUtil`
3. Copy all component creation functions from `LBEASTWallFrameCreator.fs`:
   - `createComposite`
   - `createUpperFramePiece`
   - `createEndFaceJoiner`
   - `createCornerSegmentBodies`
   - `createCenterSegmentBodies`
   - `createBroadSideFace`
   - `createBothBroadSideFaces`
4. Mark all as `export`
5. Update function calls to use `createTube`, `queryAllBodies`, and rotation builders from `LBEASTWallUtil`

### Step 3: Update LBEASTWallFrameCreator.fs
1. Add import for `LBEASTWallComponents`
2. Remove all component creation functions (already moved)
3. Update all calls to use imported functions from `LBEASTWallComponents`
4. Keep only:
   - Feature definition
   - Main orchestration logic
   - Manipulator callbacks

### Step 4: Verify and test
1. Check line counts for all three files
2. Verify all imports resolve correctly
3. Test feature regeneration in Onshape
4. Test manipulator interactions
5. Verify all transitions work correctly

---

## Expected Results

| File | Current Lines | Target Lines | Status |
|------|---------------|--------------|--------|
| `LBEASTWallFrameCreator.fs` | 2,486 | ~780 | ✅ Under 1000 |
| `LBEASTWallUtil.fs` | 270 | ~415 | ✅ Under 1000 |
| `LBEASTWallComponents.fs` | 0 | ~1,290 | ⚠️ Over 1000 |

**Note on LBEASTWallComponents.fs**: At ~1,290 lines, it exceeds the target. However:
- It contains 7 tightly-coupled component functions
- Splitting further would require 4+ files total (exceeds 3-file goal)
- We can optimize later by refactoring large functions internally
- The file is still manageable and well-organized by function

---

## Function Dependency Graph

```
LBEASTWallFrameCreator.fs
  ├─> LBEASTWallUtil.fs
  │   ├─ normalizeFacingDirection
  │   ├─ queryAllBodies
  │   ├─ createTube
  │   ├─ findRotationAxisFromBody
  │   ├─ handleRotationManipulatorChange
  │   └─ rotation builder functions
  │
  └─> LBEASTWallComponents.fs
      ├─ createComposite
      ├─ createUpperFramePiece
      ├─ createEndFaceJoiner
      ├─ createCornerSegmentBodies
      ├─ createCenterSegmentBodies
      ├─ createBroadSideFace
      └─ createBothBroadSideFaces

LBEASTWallComponents.fs
  └─> LBEASTWallUtil.fs
      ├─ createTube
      ├─ queryAllBodies
      └─ rotation builder functions
```

---

## Benefits
1. **Clear separation of concerns**: Utils, Components, and Orchestration
2. **Minimal file count**: Only 3 files to manage
3. **Reusability**: Components can be used by other features
4. **Maintainability**: Each file has a clear purpose
5. **Testability**: Components can be tested independently


