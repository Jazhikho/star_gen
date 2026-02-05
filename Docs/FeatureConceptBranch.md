# Feature Concept Branch: Jump Lanes Tool

## Purpose

The **jump-lanes-tool** branch implements a **Jump Lanes** feature that calculates a jump-gate network within a user-defined region (subsector up to sector). Connections are driven by **population**: systems are connected from lowest- to highest-populated, with distance and optional bridging rules. The result is shown in the galaxy viewer as colored lines (and orphan highlighting). This branch depends on **population information** to form connections between star systems.

---

## Scope

| Area | Scope |
|------|--------|
| **Range** | User chooses how far the network is calculated: from the **subsector** the user is currently in, up to the full **sector**. |
| **Data** | Population of each system is captured and used to order and connect systems. |
| **Output** | Jump gates (lines) between systems; line color indicates connection type. Orphan systems (no connections) highlighted in red. |
| **Placement** | Phase 1: standalone prototype. Phase 2: integrated into the main program (galaxy viewer). |

---

## How Jump Lanes Work

### Order of processing

1. **Capture populations** of each system in the selected region.
2. **Sort systems** by population from **lowest to highest**.
3. **Process each system** in that order: try to connect it to a higher-populated system within allowed distances, using the rules below.

### Connection rules (in order of attempt)

- **3 parsecs**  
  Look for the **highest-populated** world within 3 pc.  
  If found → connect the two with a **jump gate**.  
  **Visual:** **green** line.

- **5 parsecs**  
  If no populated world within 3 pc, look within 5 pc (again, highest populated).  
  If found → connect.  
  **Visual:** **green** line.

- **7 parsecs**  
  If none within 5 pc, look within 7 pc.
  - If a populated system is found:
    - **Bridging check:** Look for an **unpopulated** system that can act as a bridge: it must be **no more than 5 parsecs** from **both** the lower-populated and the higher-populated world.
      - **If a bridge exists:** Connect lower → bridge and bridge → higher (jump gates). Assign the bridge a **false population** = (higher system population − 10,000). The bridge is used as a **destination** for other systems but is **not** used as a source for new connections.  
        **Visual:** **yellow** lines.
      - **If no bridge and distance is 7 pc:** Connect the two systems directly.  
        **Visual:** **orange** line.
      - **If no bridge and distance is 9 pc:** Do **not** draw a line; move on.

- **9 parsecs**  
  If none within 7 pc, look within 9 pc.
  - Same bridging logic as 7 pc: if bridge exists → yellow (two segments); bridge gets false population and is destination-only.
  - If no bridge and distance is 7 pc → direct **orange** line.
  - If no bridge and distance is 9 pc → **no line**; move on.

### Orphans

- A system that ends up with **no connections** is an **orphan**.  
- **Visual:** highlight the system in **red**.

### Summary of line colors

| Color | Meaning |
|--------|--------|
| **Green** | Direct jump gate (target within 3 pc or 5 pc). |
| **Yellow** | Connection via a bridging (unpopulated) system; bridge has false population and is destination-only. |
| **Orange** | Direct jump gate at 7 pc when no bridge exists. |
| **Red** | Orphan system (no connections). |

---

## Phases

- **Phase 1:** Build the **prototype** (algorithm, data model, and optional standalone UI or test harness).
- **Phase 2:** **Integrate** the tool into the main program (galaxy viewer): user controls for range (subsector/sector), run calculation, and display lines and orphan highlighting.

---

## Dependencies

- **Population information** must be available for star systems in the selected region so that:
  - Systems can be ordered by population (low to high).
  - “Populated” vs “unpopulated” can be distinguished for bridging and connection rules.

---

## Architecture Notes

- **Domain:** Jump-lane calculation (distances, ordering, bridging, connection types) should live in **domain** so it is testable and independent of the viewer.
- **Viewer:** Galaxy viewer is responsible for range selection (subsector/sector), invoking the calculator, and drawing lines (green/yellow/orange) and orphan highlighting (red).
- **Determinism:** For a given region and population data, the resulting network should be deterministic.

---

## Minimal File List for Contributors

(To be refined in the implementation plan.)

- **Docs:** This document; `Docs/FeatureConceptBranchImplementationPlan.md`; `Docs/Roadmap.md` (for phase mapping).
- **Domain (Phase 1):** Jump-lane calculator (systems, distances, sort order, bridge detection, connection type); data structures for “system + population” and “connection” in the selected region.
- **App (Phase 2):** Galaxy viewer integration: range control, run tool, draw lines and orphans.
