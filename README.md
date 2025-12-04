# LBEAST OnShape CAD Models

CAD models and FeatureScripts for LBEAST hardware components.

## The Purpose of LBEAST OnShape Procedural CAD Models

The LBEAST VR SDK enables rapid development and delivery of new content for VR pop-up arcades. LBEAST is purpose-built to work with a variety of physical on-site systems, including large haptics and microcontroller embedded systems. It also has a recommended front-end lobby layout for optimized foot traffic, including dual mirrored ingress/egress stations containing up to 4 VR HMDs apiece.

These installations are not cheap to build from scratch. LBEAST attempts to streamline physical R&D in addition to software R&D in order to make VR LBE (Location-Based Entertainment) as affordable as possible for industry players, who can then pass cost savings on to the masses.

In that spirit, LBEAST includes procedural CAD models built in OnShape that demonstrate how a team might weld custom-built wall frames out of aluminum or steel and insert captured bolts paired with rivnuts in such a way as to make each framing piece simple and efficient to assemble and disassemble.

**Important Note:** LBEAST software does not require these physical parts. These JavaScript FeatureScript files are included so that teams can:
- Choose the size of their framing
- Specify the number of frames
- Quickly develop schematics for welders
- Generate documentation for on-site operations technicians

The procedural nature of these models allows rapid iteration and customization without manual CAD work, reducing both design time and manufacturing costs.

## Rectangular Frame

**File:** `RectangularFrame.fs`

A FeatureScript that procedurally generates a rectangular frame made of square steel tube.

### Parameters

- **Tube Width:** Square tube outer dimension (default: 1")
- **Tube Wall Thickness:** Wall thickness of the square tube (default: 1/16" = 0.0625")
- **Frame Width (X):** Overall frame width (default: 6' = 72")
- **Frame Depth (Y):** Overall frame depth (default: 1' = 12")
- **Frame Height (Z):** Overall frame height (default: 2' = 24")

### Usage

#### Step 1: Open OnShape and Create a Part Studio

1. Log in to [OnShape](https://www.onshape.com) (create a free account if needed)
2. Create a new document or open an existing one
3. Create a new **Part Studio** tab (or use an existing one)

#### Step 2: Create Feature Studio

1. In your OnShape document, click the **"+"** icon next to your existing tabs (or go to **Insert > Create Feature Studio**)
2. Select **"Create Feature Studio"** from the dropdown menu
3. This opens a new **Feature Studio** tab - this is where you write and edit FeatureScripts
4. A FeatureScript editor will be available in this tab

#### Step 3: Copy and Paste the Script

1. Open the `RectangularFrame.fs` file in a text editor
2. Select all the code (Ctrl+A / Cmd+A)
3. Copy it (Ctrl+C / Cmd+C)
4. Paste it into the OnShape FeatureScript editor dialog (Ctrl+V / Cmd+V)

#### Step 4: Save and Use the Feature

1. In the Feature Studio tab, paste your script into the editor
2. Click **"Accept"** or **"Save"** to save the FeatureScript
3. The feature is now available in your document
4. Switch back to your **Part Studio** tab
5. Go to **Insert** menu and look for your custom feature (it should appear in the list)
6. Select your **"Rectangular Frame"** feature
7. A feature dialog will appear with all the parameters:
   - **Tube Width:** Adjust the square tube outer dimension
   - **Tube Wall Thickness:** Adjust the wall thickness
   - **Frame Width (X):** Adjust the overall frame width
   - **Frame Depth (Y):** Adjust the overall frame depth
   - **Frame Height (Z):** Adjust the overall frame height
8. Modify any parameters as needed
9. Click **"Accept"** to generate the frame

#### Step 5: View and Edit

- The frame will appear in your Part Studio
- You can edit the feature at any time by clicking on it in the feature tree
- Adjust parameters and click **"Accept"** to regenerate with new dimensions

#### Troubleshooting

For troubleshooting guides, debugging best practices, and agent instructions, see [AGENTS.md](AGENTS.md).

### Frame Structure

The script generates a simple rectangular frame with:
- **4 vertical corner posts** at each corner
- **8 horizontal beams** connecting the posts (4 at bottom, 4 at top)
- All members are square tubes with the specified dimensions

### Notes

- No casters, holes, or additional features - just the basic frame structure
- All tubes are hollow (square tube profile)
- Frame is centered at the origin (0, 0, 0)
- Frame extends in positive Z direction (height)

## Capture Washer System

**File:** `BoltAndCaptureWasher.fs`

A two-layer welded washer system that allows a bolt to be captured inside a square tube with 1/8" of play, enabling flush tube-to-tube connections via rivnuts.

### Overview

The capture washer system consists of:
1. **Outer Washer:** 1" diameter circular washer with three holes
2. **Inner Washer:** 0.75" diameter circular washer with hex nut clearance hole
3. **Bolt:** 1/4-20 hex bolt (typically 1.25" shank length)
4. **Two Lock Nuts:** Locked together on the bolt to create a captured assembly

### Outer Washer Specifications

- **Diameter:** 1" (default, parameterized)
- **Thickness:** Parameterized (typically 1/16" to 1/8")
- **Three Holes (in a line):**
  - **Center Hole:** 1/4" diameter (for 1/4-20 bolt) - parameterized
  - **Two Flanking Holes:** 1/8" diameter each, positioned 5/16" from center (3/16" from edge on 1" washer, for plug-welding the two washer layers together)

### Inner Washer Specifications

- **Diameter:** 0.75" (default, parameterized)
- **Thickness:** Parameterized (typically 1/16" to 1/8")
- **Center Hole:** Hex nut clearance shape (to fit 1/4-20 hex nut) - parameterized
- **Rotation:** Inner washer is rotated 30° around Z-axis so plug-weld holes align with flat faces of hex nut clearance (not corners)

### Assembly Process

1. **Weld Washers Together:**
   - Place inner washer on top of outer washer (aligned by center hole)
   - Plug-weld through the two 5/16" flanking holes to join the layers
   - Result: One thicker sandwich washer

2. **Insert Bolt:**
   - Insert bolt from outer washer side
   - Insert 1.125" of the 1.25" shank (leaving 1/8" play remaining)
   - The bolt head sits against the outer washer

3. **Install First Nut:**
   - Screw a 1/4-20 hex nut onto the bolt from the inner washer side
   - Snug it into the hex nut clearance slot in the inner washer

4. **Install Second Nut (Lock Nuts):**
   - While bolt still has 1/8" entry play remaining
   - Add a second nut and tighten with impact gun
   - This locks the two nuts together
   - Result: Bolt is captured with exactly 1/8" of play (can slide 1/8" toward head or toward nuts)

5. **Install in Tube:**
   - Cut a 0.75" hole in the target square tube
   - Insert the inner washer (0.75" diameter) into the hole
   - Lap-weld the outer lip of the outer washer (1" diameter) to the tube on at least two sides
   - This fully captures the bolt and nut assembly inside the tube

6. **Mate with Adjacent Tube:**
   - The tube is now ready to mate flush with an adjacent tube
   - The adjacent tube has an aligned rivnut
   - The captured bolt can slide 1/8" to accommodate alignment during assembly

### Use Case: LBEAST Wall Frame Connections

This system is designed for connecting square tubes in the `LBEASTWallFrameCreator`:
- Tubes are connected flush-to-flush
- Capture washer allows 1/8" play for alignment tolerance
- Rivnut in receiving tube provides threaded connection
- No exposed bolt heads or nuts (clean appearance)

### Development Roadmap

1. ✅ Create `createBolt` function (hex bolt with configurable shank length)
2. ✅ Create `createCaptureWasher` function (outer + inner washers)
3. ✅ Create `createBoltCaptureWasherAssembly` function (complete assembly with proper positioning)
4. ⏳ Combine bolt and capture washer in `BoltPattern_Simple` for testing
5. ⏳ Apply capture washer system to `LBEASTWallFrameCreator` at various connection points

## Design Trade-offs: Welded vs. Modular Connection System

### Current Design: Welded Capture Washer System

**Approach:** Capture washer assemblies are welded into tubes, creating a very durable and rigid connection system.

**Advantages:**
- **High Durability:** Welded connections are extremely strong and permanent
- **Clean Appearance:** No exposed bolt heads or nuts
- **Cost Effective:** Fewer parts (one bolt per connection point)
- **Rigid Connections:** Like-depth frames can join post-for-post with just one bolt through each connection

**Constraints:**
- **Height Alignment Requirement:** For like-depth frames to join post-for-post, all bolts must be at the same height level. This creates a constraint when frames need to turn 90 degrees:
  - **Problem:** Orthogonal bolts at the same height level would hit each other cross-wise
  - **Solution Options:**
    1. Stagger bolt heights (but this breaks the post-for-post alignment requirement)
    2. Use column joiners for 90-degree turns (separate component)
    3. Accept that welded design requires more planning for orthogonal connections

**Use Case:** Best for permanent installations where rigidity and durability are prioritized over modularity.

### Alternate Design: Modular Flanking Bolt System

**Approach:** Replace welded connections with flanking bolts that can be repositioned to accommodate different connection angles.

**Advantages:**
- **High Modularity:** Bolts can be repositioned to accommodate 90-degree turns without interference
- **No Welding Required:** Easier assembly/disassembly
- **Flexible Connections:** Same frame ends can easily line up at 90 degrees to each other
- **80/20-like Flexibility:** Similar modularity to 80/20 systems but at lower cost

**Disadvantages:**
- **More Parts:** More bolts and rivnuts required per connection
- **Potentially More Work:** May not be less work to build (more fasteners to install)
- **Less Rigid:** May not achieve the same level of rigidity as welded connections

**Use Case:** Best for modular systems where frames need to be reconfigured or where welding is not practical.

### Hybrid Option: Column Joiners with Welded Design

**Approach:** Keep the welded capture washer system but add separate column joiner components for 90-degree turns and complex connections.

**Advantages:**
- **Maintains Durability:** Keeps the strength of welded connections
- **Adds Modularity:** Column joiners provide flexibility for orthogonal connections
- **Best of Both Worlds:** Rigid where needed, modular where needed

**Disadvantages:**
- **Additional Components:** Requires designing and manufacturing column joiners
- **More Complexity:** Two different connection systems to manage

**Use Case:** Best for systems that need both permanent rigid connections and occasional modular reconfiguration.

### Alternative: Welded Channel Flanges (External Bolts)

**Approach:** Instead of through-bolts inside tubes, weld channel flanges to the outside of tubes. Bolts and nuts are external to the tubes, accessible after welding.

**Advantages:**
- **Easy Corner Turns:** No interference issues with orthogonal connections - flanges can be positioned to avoid cross-wise bolt collisions
- **Maintainable/Replaceable:** Bolts and nuts are outside the tube, fully accessible for tightening, loosening, and replacement after welding is complete
- **No Internal Access Required:** No need to access the inside of tubes for bolt installation or maintenance
- **Flexible Positioning:** Flanges can be welded at different positions/angles to accommodate various connection geometries
- **Clean Internal Tubes:** Tubes remain hollow (no rivnuts or internal hardware), potentially useful for routing cables or other purposes
- **Post-Weld Assembly:** All bolt tightening happens after welding, ensuring proper alignment during welding process

**Disadvantages:**
- **Visible Hardware:** Bolts and nuts are exposed (may not be desired for aesthetic reasons)
- **Flange Design Required:** Need to design and manufacture channel flanges (additional component)
- **Welding Required:** Still requires welding capability (flanges must be welded to tubes)
- **Potential Corrosion:** External bolts may be more exposed to environmental conditions

**Use Case:** Best for systems where:
- Maintainability and accessibility of fasteners is important
- Corner turns and complex geometries are common
- External hardware appearance is acceptable
- Post-weld bolt tightening is preferred

**Key Benefit:** Solves the orthogonal bolt interference problem by keeping bolts external and allowing flexible flange positioning.

### Alternative: Frame-End Corner Posts (Built-in Corner Columns)

**Approach:** Modify frame ends with extra posts to create corner-capable connections. Wall frames incorporate corner posts as part of their default structure, eliminating the need for separate corner column joiners.

**How It Works:**
- Frame ends are designed with additional posts positioned to accommodate 90-degree turns
- When two frames meet at a corner, their built-in corner posts align to form a corner column
- Bolts connect through the aligned corner posts, creating a rigid corner connection
- No separate corner column components needed - corners are created by the frames themselves

**Advantages:**
- **No Separate Components:** Corner columns are built into the frame structure itself
- **Simplified Assembly:** Fewer unique parts to manufacture and manage
- **Inherent Corner Capability:** Every frame end is already corner-ready by design
- **Consistent Structure:** Corner posts are part of the standard frame geometry
- **Cost Effective:** No additional corner joiner components to design/manufacture
- **Rigid Connections:** Direct post-to-post connections maintain structural integrity

**Disadvantages:**
- **Frame Complexity:** Frames become more complex with additional posts at ends
- **Material Usage:** Extra posts add material cost (though may be offset by eliminating separate joiners)
- **Design Constraints:** Frame ends must be designed with corner capability in mind from the start
- **Less Flexibility:** Corner posts are fixed positions - less flexible than separate joiners for non-standard angles
- **Potential Over-Engineering:** If corners aren't needed, extra posts may be unnecessary

**Use Case:** Best for systems where:
- Corner connections are common and predictable
- Standardization is preferred over maximum flexibility
- Reducing component count is a priority
- Frame ends can be standardized with corner posts

**Key Benefit:** Eliminates the need for separate corner column joiners by building corner capability directly into the frame structure.

### Stowage/Shipping/Assembly Bolt Patterns

**Overview:** Temporary bolt patterns used during manufacturing, shipping, and assembly must be designed to not interfere with permanent connection systems.

**Purpose:**
- **Stowage:** Secure frames/components during storage
- **Shipping:** Bundle and protect components during transport
- **Assembly:** Temporarily hold components in position during welding or final assembly
- **Alignment:** Help align components before permanent connections are made

**Design Requirements:**
1. **Non-Interference:** Stowage/assembly bolt patterns must not conflict with permanent connection bolt patterns
2. **Removable:** Temporary bolts must be easily removable after permanent connections are complete
3. **Accessible:** Bolt locations must be accessible for installation and removal
4. **Standardized:** Patterns should be consistent across frame types for manufacturing efficiency

**Example Implementation:**
- **Channel Bracket Design:** If a channel bracket is welded to a frame joiner with all open faces utilized:
  - **Center face:** Reserved for stowage/assembly bolts (temporary)
  - **Two flange faces:** Used for permanent through-hole joining connections
  - This separation ensures temporary and permanent bolt patterns never interfere

**Application Across Connection Systems:**
- **Welded Capture Washers:** Stowage bolts must avoid permanent bolt height levels
- **Modular Flanking Bolts:** Stowage patterns must not occupy repositionable bolt locations
- **Column Joiners:** Temporary bolts must not interfere with joiner connection points
- **Welded Channel Flanges:** Stowage bolts can use unused flange positions or separate mounting points
- **Frame-End Corner Posts:** Temporary bolts must avoid corner post connection points

**Design Considerations:**
- **Bolt Size:** May use smaller/lighter bolts for temporary connections (cost savings)
- **Pattern Standardization:** Consistent stowage patterns across frame types simplify manufacturing
- **Removal Sequence:** Design should allow easy removal of temporary bolts after permanent connections
- **Documentation:** Clear marking or documentation of which bolts are temporary vs. permanent

**Key Benefit:** Properly designed stowage/assembly patterns enable efficient manufacturing and shipping without compromising permanent connection integrity.

### Rapid-Deployment Assembly

**Overview:** Design frame components and connection systems to enable rapid on-site assembly with minimal setup time and equipment.

**Concept:**
Frame parts are pre-loaded onto pallets that are already positioned on forklifts inside truck cargo. Upon arrival at the deployment site, forklifts can immediately begin positioning themselves face-to-face, allowing parts to be pulled directly from the top of each stack and paired together for assembly.

**Key Principles:**
1. **Pre-Loaded Pallets:** Components are stacked on pallets in the correct sequence for assembly
2. **Forklift Integration:** Pallets are already loaded on forklifts inside trucks, ready for immediate deployment
3. **Face-to-Face Positioning:** Multiple forklifts coordinate to position themselves opposite each other
4. **Direct Pairing:** Parts pulled from the top of each stack pair together immediately
5. **No Loose Hardware:** All bolts/nuts are pre-installed or captured, requiring only impact gun tightening
6. **Integrated Process:** Unpacking from trucks and assembly occur simultaneously, not sequentially

**Assembly Workflow:**
1. **Truck Arrival:** Trucks arrive with forklifts already loaded with palletized frame components
2. **Forklift Deployment:** Forklifts exit trucks and position themselves face-to-face at assembly location
3. **Part Extraction:** Operators pull matching parts from the top of each stack
4. **Immediate Pairing:** Parts align and connect directly (pre-installed bolts or captured hardware)
5. **Impact Gun Assembly:** Quick tightening with impact guns (no loose hardware to manage)
6. **Continuous Flow:** As parts are removed from top of stacks, next parts are ready for pairing

**Design Requirements:**
- **Stackable Configuration:** Parts must stack efficiently on pallets in assembly sequence
- **Pre-Installed Hardware:** Bolts/nuts must be captured or pre-installed to eliminate loose hardware
- **Quick Alignment:** Connection points must align easily without complex positioning
- **Impact Gun Compatible:** All fasteners must be accessible for impact gun operation
- **Standardized Patterns:** Consistent connection patterns across frame types for operator familiarity

**Benefits:**
- **Speed:** Dramatically reduced on-site assembly time
- **Efficiency:** Eliminates separate unpacking and staging phases
- **Reduced Labor:** Fewer operators needed due to streamlined process
- **Less Equipment:** Forklifts serve dual purpose (transport + assembly positioning)
- **Lower Risk:** Less handling and staging reduces damage risk
- **Scalability:** Multiple forklift pairs can work simultaneously on different sections

**Connection System Implications:**
- **Welded Capture Washers:** Pre-installed bolts in capture washers enable rapid assembly
- **Modular Flanking Bolts:** Pre-positioned bolts reduce assembly time
- **Welded Channel Flanges:** External bolts are ideal for impact gun access
- **Frame-End Corner Posts:** Built-in posts simplify alignment and pairing

**Considerations:**
- **Pallet Design:** Must accommodate frame dimensions and weight distribution
- **Forklift Capacity:** Stack height limited by forklift lift capacity
- **Truck Loading:** Efficient truck space utilization with forklifts and pallets
- **Site Access:** Requires adequate space for forklift positioning and movement
- **Coordination:** Multiple forklift pairs must coordinate to avoid conflicts
- **Weather:** Outdoor assembly may require weather protection for operators

**Key Benefit:** Transforms on-site assembly from a multi-phase process (unpack → stage → assemble) into a single integrated operation, dramatically reducing deployment time and complexity.

### Design Decision Factors

**Key Constraint:** Like-depth frames joining post-for-post require bolts at the same height level. This constraint affects:
- Orthogonal connections (90-degree turns)
- Connection flexibility
- Assembly complexity

**How Each Approach Addresses the Constraint:**
1. **Welded Capture Washers:** Bolts must be at same height → orthogonal interference problem → requires column joiners or careful planning
2. **Modular Flanking Bolts:** Bolts can be repositioned → solves interference but requires more parts
3. **Column Joiners:** Separate components handle orthogonal connections → adds complexity but maintains welded durability
4. **Welded Channel Flanges:** External bolts with flexible positioning → solves interference problem while maintaining maintainability
5. **Frame-End Corner Posts:** Built-in corner posts align to form corner columns → eliminates need for separate joiners, frames are inherently corner-capable

**Considerations:**
1. **Primary Use Case:** Permanent installation vs. reconfigurable system
2. **Assembly Method:** Welding capability vs. bolt-only assembly
3. **Cost Targets:** Lowest possible cost (suggests welded) vs. modularity (suggests flanking bolts)
4. **Rigidity Requirements:** Maximum rigidity (welded) vs. acceptable rigidity (modular)
5. **Maintainability:** Internal bolts (harder to access) vs. external bolts (easier to maintain)
6. **Aesthetics:** Hidden hardware (capture washers) vs. visible hardware (channel flanges)
7. **Component Count:** Separate joiners (more parts) vs. built-in corner posts (fewer parts, more complex frames)
8. **Manufacturing/Shipping:** Stowage/assembly bolt patterns must be designed to not interfere with permanent connections
9. **Rapid Deployment:** Pre-installed/captured hardware vs. loose hardware affects on-site assembly speed

**Status:** Design direction pending - evaluating trade-offs between durability, modularity, maintainability, and cost.

