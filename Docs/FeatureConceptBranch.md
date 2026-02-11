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

### Cluster connector and extended (red) connections

After the per-system rules above, a **cluster connector** runs to link otherwise isolated clusters:

1. **Standard inter-cluster links (up to 9 pc):** Iteratively find the closest pair of clusters within 9 pc and connect them using the same rules (green / yellow / orange) and bridging as above, until no such pair remains.
2. **Extended (red) links:** For any clusters still isolated:
   - If the **shortest distance** between two clusters is **≤10 parsecs**, connect that pair with a single **red** line (extended direct).
   - If the distance is **>10 pc**, search for a **multi-hop path** through other systems where **each hop is ≤10 pc**. If such a path exists, add **red** lines for each hop; intermediate systems are treated as bridges (same as yellow: `is_bridge`, false population). Red connections use the same 10 pc limit per hop.

**Visual:** **red** lines for extended direct or multi-hop links. (Orphan systems are also indicated in red, e.g. icon or highlight; line red = extended connection, marker red = orphan.)

### Orphans

- A system that ends up with **no connections** is an **orphan**.  
- **Visual:** highlight the system in **red** (e.g. icon, outline, or background).

### Summary of line colors

| Color | Meaning |
|--------|--------|
| **Green** | Direct jump gate (target within 3 pc or 5 pc). |
| **Yellow** | Connection via a bridging (unpopulated) system; bridge has false population and is destination-only. |
| **Orange** | Direct jump gate at 7 pc when no bridge exists. |
| **Red (line)** | Extended connection: direct ≤10 pc between clusters, or multi-hop path (each hop ≤10 pc). |
| **Red (marker)** | Orphan system (no connections). |

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
- **Viewer:** Galaxy viewer is responsible for range selection (subsector/sector), invoking the calculator, and drawing lines (green/yellow/orange/red for extended) and orphan highlighting (red).
- **Determinism:** For a given region and population data, the resulting network should be deterministic.

---

## Minimal File List for Contributors

(To be refined in the implementation plan.)

- **Docs:** This document; `Docs/FeatureConceptBranchImplementationPlan.md`; `Docs/Roadmap.md` (for phase mapping).
- **Domain (Phase 1):** Jump-lane calculator; cluster connector (standard 9 pc + extended red ≤10 pc / multi-hop); data structures for system, connection (green/yellow/orange/red), and region.
- **App (Phase 2):** Galaxy viewer integration: range control, run tool, draw lines and orphans.
