## Additions To Prototype For StarGen

This section is the StarGen-facing prototype backlog only.

**Scope anchor:** StarGen remains focused on deterministic astronomy/world-state generation and viewing (systems, galaxies, stations, jump lanes, and supporting validators/tooling).

**Planet population generation boundary (authoritative):** StarGen may describe planetary population context at generation time (for example native/settled presence, broad viability/distribution signals), but it does **not** run detailed planet-level simulation.

**Roadmap handoff:** Items here should move into `Docs/Roadmap.md` once selected and gated.

### StarGen additions (generators/simulators/validators)

| # | Suggestion | Type | Placement | Note |
|---|------------|------|-----------|------|
| 1 | Resource Geology & Extraction Potential | Generator | Prototype TODO | Ore belts, volatiles, extraction difficulty, resource maps, strategic scarcity. Feeds economy/trade summaries without deep civ simulation. |
| 2 | Logistics & Transport Network Planner | Generator | Roadmap effort TODO | Build on jump lanes: shipping lanes, travel time/capacity, port throughput, chokepoints, fuel/maintenance. |
| 3 | Demographics & Population Dynamics (summary-level) | Simulator | Roadmap effort TODO | Birth/death, migration, labor pressure at summary scale only; no planet-level detail simulation in StarGen. |
| 4 | Catastrophe / Hazard Engine (natural + technological) | Generator | Prototype TODO | Supervolcanoes, impacts, droughts, solar storms, station failures, recovery pressure. |
| 5 | Consistency Validator / Plausibility Audit Tool | Validator | Roadmap effort TODO | Extend Celestial/System validators for StarGen-owned layers; deep ecology/culture checks are out of scope. |
| 6 | Cartography & Map Product Generator | Exporter | Roadmap effort TODO | Produce political, climate, resource, jump-lane, and trade-route maps from StarGen-owned data layers. |
| 7 | Modding / Plugin API for Generators | Viewer/Editor | Roadmap effort TODO | Allow extension hooks for StarGen generators, validators, and export transforms. |

### StarGen additions (tools/workflow)

| # | Tool / feature | Type | Placement | Note |
|---|----------------|------|-----------|------|
| 1 | Causality Inspector ("Why is this like this?") | Viewer/Editor | Roadmap effort TODO | Explain generator decisions for StarGen-owned outputs. |
| 2 | Minimal-Change Regenerate Tool | Viewer/Editor | Roadmap effort TODO | Regenerate scoped layers while preserving stable IDs/names where possible. |
| 3 | Generation Diff Viewer | Viewer/Editor | Roadmap effort TODO | Compare seed/version outputs for regression and iteration. |
| 4 | Canonical Snapshot / Milestone System | Viewer/Editor | Roadmap effort TODO | Named checkpoints for StarGen states and branching workflows. |
| 5 | Seed Archaeology Tool | Viewer/Editor | Roadmap effort TODO | Reconstruct seed, settings, version, and generation profile from output. |
| 6 | Constraint Debugger | Viewer/Editor | Roadmap effort TODO | Explain failed constraints and nearest valid alternatives. |
| 7 | Sensitivity Analyzer | Viewer/Editor | Prototype TODO | Show which small parameter changes create large output shifts. |
| 8 | Ensemble Generator / Range Explorer | Generator | Prototype TODO | Generate variant sets and summarize distributions/outliers. |
| 9 | World QA Test Pack Generator | Generator | Roadmap effort TODO | Auto-build known-seed regression packs and edge-case suites for StarGen development. |

