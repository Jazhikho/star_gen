# Implementation Plan: jump-lanes-tool Branch

**Execution order:** Phase 1 (prototype) → Phase 2 (integration into galaxy viewer).

---

## Phase 1: Prototype

**Goal:** Implement the jump-lane calculation algorithm and data model in isolation so it can be tested and tuned before UI integration. Population data is assumed to be available (from population branch or mock).

### Stage 1.1: Data model and region

- **Define** the minimal data needed for the tool:
  - **System in region:** identifier, position (for distance), population (or “unpopulated”), optional “false population” for bridges.
  - **Connection:** system A, system B, connection type (green / yellow / orange).
  - **Region:** subsector or sector scope (list of systems + bounds).
- **Define** how the user’s “current subsector” and “current sector” map to a set of systems (e.g. filter by coordinates or region IDs).
- **Document** units (parsecs) and that all distances are 3D spatial (or 2D galactic plane, per project convention).

### Stage 1.2: Core algorithm

- **Implement** in domain (e.g. `src/domain/jumplanes/` or under `galaxy/` if that exists):
  - **Input:** List of systems in region (id, position, population).
  - **Output:** List of connections (pair + type) and list of orphan system IDs.
- **Steps:**
  1. Sort systems by population ascending (unpopulated last or excluded from “source” list; bridges get false population when created).
  2. For each system (low to high):
     - Find highest-populated system in 3 pc → if found, add green connection, skip to next.
     - Else in 5 pc → green; skip.
     - Else in 7 pc: if bridge exists (unpopulated, ≤5 pc from both), add two yellow connections, assign bridge false population, mark bridge as destination-only; else if distance 7 pc add orange; else if distance 9 pc add nothing.
     - Else in 9 pc: same bridging and 7/9 pc rules.
  3. Collect systems with zero connections as orphans.
- **Edge cases:** Multiple candidates at same distance (use “highest populated”); bridge used by more than one path (document behavior: first-come or allow multiple connections to same bridge).

### Stage 1.3: Bridging and false population

- **Bridging:** Helper that, given two systems A and B, returns an unpopulated system C such that dist(A,C) ≤ 5 and dist(B,C) ≤ 5. If multiple candidates, define rule (e.g. closest to midpoint, or smallest total distance).
- **False population:** When a bridge is chosen, set its “effective” population to (higher system population − 10,000) so it can be used as a **destination** for other systems; it is **not** used as a source for new outbound connections in the same run.

### Stage 1.4: Prototype tests

- **Unit tests:** Sort order; distance thresholds (3/5/7/9 pc); bridge selection; connection type (green/yellow/orange); orphan detection. Use small fixed datasets (e.g. 3–5 systems) for determinism.

### Stage 1.5: Prototype scene

- **Create** a standalone prototype scene that demonstrates the jump-lanes tool without depending on the full galaxy viewer.
- **Scene contents:**
  - Load or define a small set of systems (mock positions + populations) in a fixed region.
  - Run the jump-lane calculator on that data.
  - **Draw** systems as nodes or markers (e.g. in 2D or simple 3D); **draw connections** as lines with the correct colors (green / yellow / orange); **highlight orphans** in red.
  - Optional: simple camera/pan so the user can inspect the result; legend or labels for line types and orphan state.
- **Purpose:** Validate the algorithm visually, tune thresholds, and provide a reference for Phase 2 line/orphan rendering. No dependency on population branch or galaxy viewer; mock data is sufficient.

**Deliverables (Phase 1):** Domain data structures; jump-lane calculator; bridge logic; unit tests; **prototype scene** that visualizes connections and orphans.

---

## Phase 2: Integration into main program

**Goal:** Expose the jump-lanes tool in the galaxy viewer: user selects range (subsector vs sector), runs the calculation, and sees lines (green/yellow/orange) and orphan highlighting (red).

### Stage 2.1: Population data wiring

- **Ensure** the galaxy viewer (or its data source) can provide, for the selected region, the list of systems with:
  - Position (for distance).
  - Population (from population framework or placeholder).
- **Single entry point:** e.g. “get systems in region(region_type, current_subsector/sector)” returning the structure expected by the jump-lane calculator.

### Stage 2.2: User controls

- **Range control:** UI or viewer action to choose “subsector only” vs “sector” (and pass that into the region query).
- **Run tool:** Button or command that triggers: (1) get systems in region, (2) run jump-lane calculator, (3) pass results to the renderer.

### Stage 2.3: Visual representation

- **Lines:** Draw connections as lines in the galaxy view:
  - **Green:** direct 3 pc / 5 pc.
  - **Yellow:** connection via bridge (two segments: source–bridge, bridge–destination).
  - **Orange:** direct 7 pc (no bridge).
- **Orphans:** Highlight systems with no connections in **red** (e.g. icon color, outline, or background).
- **Performance:** Consider limiting line count or LOD for large sectors; document any culling or simplification.

### Stage 2.4: Integration tests and polish

- **Integration test:** “Galaxy viewer + jump-lanes”: load a small sector, run tool, assert line count and orphan count match domain expectations.
- **Docs:** Update README or Docs to mention the jump-lanes tool and where it lives (menu, panel, or shortcut).

**Deliverables (Phase 2):** Region + population wiring; subsector/sector control; line drawing (green/yellow/orange); orphan highlighting (red); integration test; doc update.

---

## File summary (to be refined during implementation)

**New (Phase 1):**

- Domain: e.g. `JumpLaneCalculator.gd`, `JumpLaneData.gd` (or small modules for region, connection, bridge).
- Tests: `Tests/Unit/TestJumpLaneCalculator.gd` (or split by stage); add to `RunTestsHeadless.gd` and `TestScene.gd`.
- Prototype: scene + script under e.g. `Tests/` or `Scenes/` (e.g. `JumpLanesPrototype.tscn` + controller script) to visualize mock systems, lines (green/yellow/orange), and red orphans.

**New (Phase 2):**

- App: viewer integration (scripts/scenes for range control, draw lines, draw orphans); may extend existing galaxy viewer scripts.

**Modified:**

- Galaxy viewer (or equivalent) to call calculator and display results; test runners if new test scripts are added.

---

## Acceptance checklist

**Phase 1**

- [ ] Systems sorted by population (low → high); connection rules (3/5/7/9 pc, bridge, green/yellow/orange) match spec.
- [ ] Orphans correctly identified and exposed in output.
- [ ] Unit tests pass; determinism for fixed input.
- [ ] Prototype scene runs and displays systems, colored lines (green/yellow/orange), and red orphan highlighting with mock data.

**Phase 2**

- [ ] User can select subsector or sector and run the tool.
- [ ] Lines appear with correct colors; orphans appear in red.
- [ ] Integration test passes; no regressions in main test suite.
