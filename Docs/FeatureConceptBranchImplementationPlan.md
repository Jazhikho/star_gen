# Implementation Plan: jump-lanes-tool Branch

**Execution order:** Phase 1 (prototype) → Phase 2 (integration into galaxy viewer).

---

## Phase 1: Prototype

**Goal:** Implement the jump-lane calculation algorithm and data model in isolation so it can be tested and tuned before UI integration. Population data is assumed to be available (from population branch or mock).

### Stage 1.1: Data model and region — **Done**

- **Defined** and implemented:
  - **System in region:** identifier, position (for distance), population (or “unpopulated”), optional “false population” for bridges (`JumpLaneSystem`).
  - **Connection:** system A, system B, connection type (green / yellow / orange / **red** for extended) (`JumpLaneConnection`).
  - **Region:** list of systems + bounds (`JumpLaneRegion`); **Result:** connections + orphan IDs + registered systems (`JumpLaneResult`).
- **Define** how the user’s “current subsector” and “current sector” map to a set of systems (e.g. filter by coordinates or region IDs) — for Phase 2.
- **Document** units (parsecs) and that all distances are 3D spatial (or 2D galactic plane, per project convention).

### Stage 1.2: Core algorithm — **Done**

- **Implemented** in `src/domain/jumplanes/` (`JumpLaneCalculator.gd`):
  - **Input:** Region (list of systems: id, position, population).
  - **Output:** Result (connections, orphan IDs, registered systems). Calculator delegates cluster connection to `JumpLaneClusterConnector`.
- **Steps:** Sort by population ascending; per-system rules (3/5/7/9 pc, green/yellow/orange, bridge selection); collect orphans; then cluster connector runs (see Stage 1.6).
- **Edge cases:** Highest populated at same distance; bridge selection by smallest total distance; bridge can serve multiple connections.

### Stage 1.3: Bridging and false population — **Done**

- **Bridging:** In calculator and cluster connector: given two systems A and B, find unpopulated (or bridge) system C with dist(A,C) ≤ 5 and dist(B,C) ≤ 5; multiple candidates use smallest total distance.
- **False population:** Bridge gets effective population (higher − 10,000), destination-only; same treatment for extended (red) multi-hop waypoints.

### Stage 1.4: Prototype tests — **Done**

- **Unit tests** in `Tests/Unit/JumpLanes/`: calculator (sort, thresholds, bridge, connection types, orphans); cluster connector (cluster ID, standard + extended red direct and multi-hop); connection, result, system, region. Determinism and connection-type coverage. All run via `JumpLanesTestRunner.gd` (and optionally main test scene).

### Stage 1.5: Prototype scene — **Done**

- **Standalone prototype** scene demonstrates the tool without the full galaxy viewer.
- **Contents:** Mock systems in a region; run calculator; draw systems and **connections** (green / yellow / orange / **red**); **highlight orphans** (e.g. red). Optional camera/pan and legend.
- **Purpose:** Visual validation and reference for Phase 2 rendering.

### Stage 1.6: Cluster connector and extended (red) connections — **Done**

- **Cluster connector** (`JumpLaneClusterConnector.gd`): After per-system rules, iteratively connect the closest cluster pair within 9 pc using standard rules (green/yellow/orange + bridging) until no such pair exists.
- **Extended phase:** For clusters still isolated: (1) If closest pair ≤10 pc, add a single **red** (extended direct) connection. (2) If not, search for a **multi-hop path** (BFS) where each hop ≤10 pc over all systems; if found, add **red** links for each hop and mark intermediates as bridges. Repeat until no more extended links can be added.
- **Connection type:** `RED` added to domain and result counts; red bridges treated like yellow (e.g. `is_bridge`, false population).

**Deliverables (Phase 1):** Domain data structures (including RED); jump-lane calculator; cluster connector (standard + extended); bridge logic; unit tests; **prototype scene** that visualizes connections (green/yellow/orange/red) and orphans.

---

## Phase 2: Integration into main program

**Goal:** Expose the jump-lanes tool in the galaxy viewer: user selects range (subsector vs sector), runs the calculation, and sees lines (green/yellow/orange/**red**) and orphan highlighting (red).

### Stage 2.1: Population data wiring — **Not started**

- **Ensure** the galaxy viewer (or its data source) can provide, for the selected region, the list of systems with:
  - Position (for distance).
  - Population (from population framework or placeholder).
- **Single entry point:** e.g. “get systems in region(region_type, current_subsector/sector)” returning the structure expected by the jump-lane calculator (`JumpLaneRegion` with `JumpLaneSystem` instances).

### Stage 2.2: User controls — **Not started**

- **Range control:** UI or viewer action to choose “subsector only” vs “sector” (and pass that into the region query).
- **Run tool:** Button or command that triggers: (1) get systems in region, (2) run jump-lane calculator, (3) pass results to the renderer.

### Stage 2.3: Visual representation — **Not started**

- **Lines:** Draw connections as lines in the galaxy view:
  - **Green:** direct 3 pc / 5 pc.
  - **Yellow:** connection via bridge (two segments: source–bridge, bridge–destination).
  - **Orange:** direct 7 pc (no bridge).
  - **Red:** extended connection (direct ≤10 pc or multi-hop path, each hop ≤10 pc).
- **Orphans:** Highlight systems with no connections in **red** (e.g. icon color, outline, or background)—distinguish from red *lines* (extended links) where needed (e.g. line = red, orphan marker = red highlight/badge).
- **Performance:** Consider limiting line count or LOD for large sectors; document any culling or simplification.

### Stage 2.4: Integration tests and polish — **Not started**

- **Integration test:** “Galaxy viewer + jump-lanes”: load a small sector, run tool, assert line count and orphan count match domain expectations (including red connections when applicable).
- **Docs:** Update README or Docs to mention the jump-lanes tool and where it lives (menu, panel, or shortcut).

**Deliverables (Phase 2):** Region + population wiring; subsector/sector control; line drawing (green/yellow/orange/red); orphan highlighting (red); integration test; doc update.

---

## File summary (to be refined during implementation)

**New (Phase 1) — implemented:**

- Domain: `src/domain/jumplanes/` — `JumpLaneCalculator.gd`, `JumpLaneClusterConnector.gd`, `JumpLaneConnection.gd`, `JumpLaneResult.gd`, `JumpLaneSystem.gd`, `JumpLaneRegion.gd`.
- Tests: `Tests/Unit/JumpLanes/` (TestJumpLaneCalculator, TestJumpLaneClusterConnector, TestJumpLaneConnection, TestJumpLaneResult, TestJumpLaneSystem, TestJumpLaneRegion); `Tests/JumpLanesTestRunner.gd`, `Tests/JumpLanesDeps.gd`, `Tests/JumpLanesTestScene.gd` (+ .tscn).
- Prototype: `JumpLanesPrototype.tscn` + controller to visualize mock systems, lines (green/yellow/orange/red), and red orphan highlighting.

**New (Phase 2):**

- App: viewer integration (scripts/scenes for range control, draw lines including red, draw orphans); may extend existing galaxy viewer scripts.

**Modified (Phase 2):**

- Galaxy viewer (or equivalent) to call calculator and display results; test runners if new integration test scripts are added.

---

## Acceptance checklist

**Phase 1**

- [x] Systems sorted by population (low → high); connection rules (3/5/7/9 pc, bridge, green/yellow/orange) match spec.
- [x] Cluster connector connects closest clusters within 9 pc (green/yellow/orange) until no more; extended phase adds red (direct ≤10 pc or multi-hop ≤10 pc per hop).
- [x] Orphans correctly identified and exposed in output.
- [x] Unit tests pass; determinism for fixed input.
- [x] Prototype scene runs and displays systems, colored lines (green/yellow/orange/red), and red orphan highlighting with mock data.

**Phase 2**

- [ ] User can select subsector or sector and run the tool.
- [ ] Lines appear with correct colors (green/yellow/orange/red); orphans appear in red.
- [ ] Integration test passes; no regressions in main test suite.

---

## Assessment: What remains before Phase 2 is implemented

Phase 1 is **complete**: domain model, calculator, cluster connector (including extended red connections), unit tests, and prototype scene are in place.

To **finish Phase 2**, the following work remains:

1. **Stage 2.1 — Population data wiring**
   - Implement or hook up a single entry point that returns a `JumpLaneRegion` (list of `JumpLaneSystem`) for the user’s current subsector or sector.
   - Systems must have: id, position (parsecs), population (0 = unpopulated). If the population branch is not integrated, use a placeholder or mock until real data is available.

2. **Stage 2.2 — User controls**
   - Add UI or viewer action to choose **subsector only** vs **sector**.
   - Add a **Run jump-lanes** (or equivalent) control that: gets systems in region → runs `JumpLaneCalculator.calculate(region)` → passes the `JumpLaneResult` to the renderer.

3. **Stage 2.3 — Visual representation**
   - **Lines:** Draw each connection in the galaxy view using `conn.get_color()` (or equivalent) so **green**, **yellow**, **orange**, and **red** (extended) lines are shown correctly.
   - **Orphans:** Draw orphan systems with a red highlight (icon/outline/background), distinct from red *lines* (extended links) if the same color is used.
   - **Performance:** For large sectors, consider culling, LOD, or limiting line count; document any limits.

4. **Stage 2.4 — Integration tests and polish**
   - Add an integration test: load a small sector (or test region), run the tool, assert connection counts and orphan count match domain expectations (including red when applicable).
   - Update README or Docs with where the jump-lanes tool lives (menu/panel/shortcut) and how to use it.

**Risks / dependencies:** Phase 2 assumes a galaxy viewer (or equivalent) and a way to resolve “current subsector/sector” to a set of systems. If population data is not yet wired, the tool can still be integrated using placeholder population (e.g. 0 or fixed values) so that line drawing and controls are in place; real population can be plugged in later.
