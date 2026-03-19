## Concept expansion menu: classification and placement

**Already in main (no prototype required):** Core object **generation** (stars, planets, moons, asteroids - StarGenerator, PlanetGenerator, MoonGenerator, AsteroidGenerator, RingSystemGenerator) and **appearance** (object rendering: BodyRenderer, MaterialFactory, StarShaderParams, GasGiantShaderParams, TerrestrialShaderParams, ring/atmosphere shaders) are in the main program. So are system and galaxy generation, population framework, stations, detailed station design, and jump lanes. Once a prototype is fully folded into main, remove it from Concepts/ or src/app/prototypes/ and from the Placement column below (see CLAUDE.md).

**Separation from Roadmap:** This document holds exploratory concepts and tools. When a prototype meets all migration gates (see CLAUDE.md), it can be added as an effort (or part of one) in Docs/Roadmap.md; once it is folded into main, remove it from this menu and from Concepts/. See Docs/Roadmap.md for formal efforts.

**Human-audit note:** Prototypes touching civilisation, culture, religion, language, species framing, or other culture-adjacent worldbuilding are higher-risk. They may use AI for bounded ideation or critique, but they require explicit human audit for bias, analogy boundaries, unsupported realism claims, and publication suitability before fold-in or release.

**Selected for current fold-in:** EcologyGenerator, ReligionGenerator, CivilisationEngine, ConlangGenerator, DiseaseSimulator, and EvoTechTree are now assigned to the active effort **Concept Atlas and concept tool fold-in** on branch `codex/concept-atlas-fold-in`. Their main-app atlas replacements are present, end-user accessible, and now persisted through generated world state; they remain listed here until explicit human release audit and clean prototype retirement are complete.

Items are classified as **Generator** (creates data), **Simulator** (changes data over time), **Validator** (checks plausibility/consistency), **Viewer/Editor** (inspect/tune), or **Exporter** (use elsewhere). **Placement** gives the status (Prototype TODO, Roadmap effort TODO, or Other) and, where one exists, the Concepts/ folder or effort holding the current prototype.

| # | Suggestion | Type | Placement | Note |
|---|------------|------|-----------|------|
| 1 | Planetary Geology / Geophysics Generator | Generator | Prototype TODO | Crust type, tectonics, volcanism, crater density, core/magnetic dynamo. Feeds climate, resources, hazards, ecology, settlement. |
| 2 | Climate & Weather Systems (planet-scale) | Generator | Prototype TODO | Latitudinal bands, axial tilt/seasons, ocean effects, wind/precipitation, ice caps/deserts. Bridge from planet gen to ecology. |
| 3 | Oceanography & Hydrology Module | Generator | Prototype TODO | Salinity, currents, tides, river basins, lakes/wetlands, groundwater. Feeds agriculture, trade, biodiversity, civ placement. |
| 4 | Biome Synthesis Engine | Generator | Roadmap effort TODO · EcologyGenerator | Main has BiomeType/ClimateZone; EcologyGenerator has biomes + niches. Incorporate: ecotones, productivity score, habitat complexity, niche richness. |
| 5 | Planet Chemistry & Biochemistry Constraints | Validator | Prototype TODO | Atmospheric chemistry, solvent candidates, energy gradients, pH/extremes. Constrains “what life is possible”; prevents impossible ecology. |
| 6 | Speciation Pressure / Adaptive Landscape Simulator | Simulator | Prototype · EvoTechTree | Selection by biome, predator/prey, sexual selection, bottlenecks, isolation. Backbone for evolution branch. |
| 7 | Trait Grammar / Morphology Builder (species) | Generator | Prototype · EvoTechTree | Body plans, locomotion, senses, reproduction, feeding, defense. Composable species generation. |
| 8 | Food Web / Trophic Network Generator | Generator | Roadmap effort TODO · EcologyGenerator | EcologyGenerator has trophic web. Incorporate: keystone species flags, invasive species sim, collapse/resilience indicators. |
| 9 | Disease & Epidemiology Simulator (Bio + Civ) | Simulator | Prototype · DiseaseSimulator | Pathogens, transmission, host range, virulence/transmissibility, immunity, population impact. History, ecology, emergent narrative. |
| 10 | Resource Geology & Extraction Potential | Generator | Prototype TODO | Ore belts, volatiles, extraction difficulty, resource maps, strategic scarcity. Feeds economy, trade, conflict, colonization. |
| 11 | Economic Simulation (local to interstellar) | Simulator | Prototype TODO | Supply/demand, trade goods, comparative advantage, transport cost, price variation, black markets. Living trade network. |
| 12 | Logistics & Transport Network Planner | Generator | Roadmap effort TODO | Jump lanes exist in main. Add: shipping lanes, travel time/capacity, port throughput, chokepoints, fuel/maintenance. |
| 13 | Political Geography & Borders Module | Generator | Prototype TODO | Territory, core/periphery, border friction, buffer states, frontier expansion. Gives civs spatial shape. |
| 14 | Diplomacy / Faction Relations Engine | Simulator | Roadmap effort TODO | Trust, rivalry, ideology, trade dependency, alliances, proxy conflict. Part of population/civ detail. |
| 15 | Internal Politics / Power Bloc Simulator | Simulator | Prototype · CivilisationEngine | Elites, military, clergy, merchants, labor; influence, reform/reaction, coups, succession. Align with CivilisationEngine. |
| 16 | Religion / Belief System Generator | Generator | Prototype · ReligionGenerator | Cosmology, ritual intensity, clerical structure, moral axes, syncretism/schism, missionary vs insular. Deterministic seeded generator in Concepts/ReligionGenerator (F+E fixes: Fisher–Yates, weighted sampling, sigmoid-bounded %). |
| 17 | Culture Generator (values, norms, aesthetics) | Generator | Prototype · CivilisationEngine | CivilisationEngine has culture sim. Incorporate: value axes, family/kinship, gender/role norms, aesthetics, diffusion. |
| 18 | Language Family / Linguistics Generator | Generator | Prototype · ConlangGenerator | Proto-language, sound changes, branching families, naming conventions, loanwords. ConlangGenerator: phonology, grammar, concept lexicon, inflection, sentence builder (Mulberry32 seeded). |
| 19 | Law, Justice & Social Order Systems | Generator | Roadmap effort TODO | Legal tradition, enforcement, rights/stratification, punishment, corruption/legitimacy. Traveller Law Level ties in. |
| 20 | Technology Diffusion & Innovation Model | Simulator | Prototype · CivilisationEngine | CivilisationEngine has tech tree. Incorporate: invention rate, adoption friction, diffusion, cultural filters, leapfrogging/stagnation. |
| 21 | Demographics & Population Dynamics | Simulator | Roadmap effort TODO | Birth/death, age structure, urbanization, migration, labor pools, population pressure. Extend population framework. |
| 22 | Urban Generation / Settlement Morphology | Generator | Prototype TODO | Settlement types, layout archetypes, growth drivers, infrastructure, district specialization. Pairs with stations/civ. |
| 23 | Architecture & Material Culture Generator | Generator | Prototype TODO | Material palette, structural style, decorative motifs, climate adaptations. Visual/cultural identity. |
| 24 | Military Doctrine & Strategic Balance Module | Generator | Prototype TODO | Force composition, doctrine types, logistics constraints, tech/doctrine mismatch, deterrence/escalation. |
| 25 | Conflict & War Simulator | Simulator | Prototype TODO | Casus belli, campaign progression, occupation/annexation, war exhaustion, post-war settlements. |
| 26 | Catastrophe / Hazard Engine (natural + technological) | Generator | Prototype TODO | Supervolcanoes, impacts, droughts, pandemics (link to #9), solar storms, station failures, recovery. History/ecology resets. |
| 27 | Archaeology / Deep Time Remnants Module | Generator | Prototype TODO | Ruins, lost cultures/species traces, artifact distribution, stratified history, recoverability/mythic distortion. |
| 28 | Myth, Legend & Narrative Memory Generator | Generator | Prototype TODO | Heroic myths from events, religious reinterpretation, propaganda, founding stories, contradictory traditions. |
| 29 | Information Systems / Media & Communication Networks | Simulator | Prototype TODO | Communication speed/coverage, censorship, propaganda, fragmentation, rumor/misinformation. Politics, culture, rebellion. |
| 30 | AI / Synthetic Life / Post-biological Population Module | Generator | Prototype TODO | Synthetic populations as actors, replication models, rights/legal status, coexistence/conflict with biological. |
| 31 | Colonization Pipeline Simulator | Simulator | Prototype TODO | Launch conditions, viability phases, supply dependence, terraforming/adaptation, success/failure. Bridges stations, civ, ecology. |
| 32 | Terraforming / Planetary Engineering Module | Simulator | Prototype TODO | Atmosphere modification, hydrological seeding, orbital mirrors/shades, biosphere introduction risks, timescales/failure. |
| 33 | Biosphere Impact / Anthropocene Layer | Simulator | Prototype TODO | Deforestation, pollution, extinction pressure, conservation, climate shift from industry. Civ-ecology feedback. |
| 34 | Time Viewer / Timeline Query System | Viewer/Editor | Roadmap effort TODO | Event timeline browser, filters (planet/civ/species/station/conflict), causal-chain view, snapshot compare. |
| 35 | Consistency Validator / Plausibility Audit Tool | Validator | Roadmap effort TODO | Extend beyond Celestial/System validators: flag ecological, demographic, economic contradictions; severity; fix suggestions. |
| 36 | Scenario / Premise Generator (worldbuilding prompt layer) | Generator | Roadmap effort TODO | Conflict premise from data, key factions/resources/hazards/myths, campaign snapshots, story hooks. Overlap with Filters effort. |
| 37 | Cartography & Map Product Generator | Exporter | Roadmap effort TODO | Political, climate, biome, resource, jump-lane, trade-route maps. Shareable outputs for TTRPG/fiction/preproduction. |
| 38 | Modding / Plugin API for Generators | Viewer/Editor | Roadmap effort TODO | Register generator modules, extend tables, custom event injectors, validation hooks, scriptable export transforms. |

*Removed: Data Export/Interop (#38), Ruleset/Setting Preset (#39) — these are Roadmap efforts (Export function, Filters that match game needs).*

**Summary by type:** Generator 21, Simulator 12, Validator 2, Viewer/Editor 2, Exporter 1.

---

## Unique tools on top of generators: classification and placement

Same type and placement scheme. **Tool**-oriented items are mostly Viewer/Editor (inspect/tune/explore) or Validator (check consistency/plausibility); a few are Generator or Exporter where they produce new data or export packages.

| # | Tool / feature | Type | Placement | Note |
|---|----------------|------|-----------|------|
| 1 | Causality Inspector ("Why is this like this?") | Viewer/Editor | Roadmap effort TODO | Trace which generators touched output, what inputs mattered, which roll/seed branch, what constraints forced it. Proc-gen explainability. |
| 2 | Counterfactual Sandbox ("What if one thing changed?") | Viewer/Editor | Roadmap effort TODO | Duplicate world state, tweak one variable (metallicity, axial tilt, trade route, regime, extinction), compare outcomes. Design exploration. |
| 3 | Minimal-Change Regenerate Tool | Viewer/Editor | Roadmap effort TODO | Regenerate system/planet/region while preserving names, settlements, species IDs; e.g. rebuild only climate layer. Avoid "one fix, everything exploded." |
| 4 | Retcon Manager | Viewer/Editor | Prototype TODO | Canon override, lock, track contradictions, reconciliation suggestions. Supports part procedural, part authorial worldbuilding. |
| 5 | Lore Contradiction Finder | Validator | Roadmap effort TODO | Scan for population > carrying capacity, impossible territory, myth before event, species before speciation. Extends Consistency Validator (#35). |
| 6 | Plausibility Heatmap | Viewer/Editor | Roadmap effort TODO | Overlay confidence/plausibility across orbital stability, climate, ecology, socio-political. "Hard-ish sci-fi" vs "cool but dodgy." |
| 7 | Generation Diff Viewer | Viewer/Editor | Roadmap effort TODO | Compare two seeds or versions: added/removed systems, changed populations, shifted borders, extinct/new species, event timeline deltas. Iteration and testing. |
| 8 | Simulation Replay / Time Scrubber | Viewer/Editor | Roadmap effort TODO | Scrub across years; watch borders, populations, ecosystems shift; pause at events; inspect state snapshots. Complements Time Viewer (#34). |
| 9 | Branch Merger for World States | Viewer/Editor | Roadmap effort TODO | Merge ecology + civ + station branch outputs; conflict resolution for overlapping IDs; merge report with warnings. Turns concept branches into workflow. |
| 10 | Canonical Snapshot / Milestone System | Viewer/Editor | Roadmap effort TODO | Named snapshots ("Pre-colonization", "After First Jump War", etc.); branch from them. Eras without duplicating whole projects. |
| 11 | Seed Archaeology Tool | Viewer/Editor | Roadmap effort TODO | From output, reconstruct exact seed, generator settings, realism profile, version, source branch. "How did I make this?" recovery. |
| 12 | Constraint Debugger | Viewer/Editor | Roadmap effort TODO | When constraints fail: which conflict, nearest valid alternatives, which requirement fails, how to relax minimally. Toy vs tool. |
| 13 | Sensitivity Analyzer | Viewer/Editor | Prototype TODO | Multiple samples with tiny parameter changes; show which settings heavily affect outcomes (e.g. metallicity → planet count, axial tilt → climate). |
| 14 | Ensemble Generator / Range Explorer | Generator | Prototype TODO | Generate 50–500 variants; summarize distribution, rare edge cases, median/outliers, "best fit" candidates. Curated pick, not one blind roll. |
| 15 | Narrative Hook Extractor | Generator | Roadmap effort TODO | Turn sim state into prompts: conflicts, mysteries, expedition goals, political crises, ecological disasters, station incidents. Overlap with Scenario/Premise (#36). |
| 16 | Diegetic Report Generator | Exporter | Roadmap effort TODO | Export as in-universe docs: explorer survey, colonial census, military brief, xenobiology paper, station maintenance report. Games/TTRPG/writing. |
| 17 | Perspective Filter | Viewer/Editor | Prototype TODO | Same world through different actors: empire, settler, scientist, rebel, religious. Same data, different interpretation. |
| 18 | Uncertainty / Fog-of-Knowledge Layer | Viewer/Editor | Prototype TODO | Separate ground truth from known/observed: incomplete maps, misidentified species, false histories, sensor noise, propaganda. Narrative reality. |
| 19 | Survey Mission Simulator | Simulator | Prototype TODO | What a survey team would detect over time: scan resolution limits, false positives, missed features, confidence upgrades with repeated scans. |
| 20 | Regional Patch Tool | Viewer/Editor | Roadmap effort TODO | Paint local changes without rerolling planet: crater zone, desertification, terraform bubble, urban sprawl, biosphere collapse patch. |
| 21 | Species Niche Occupancy Editor with Suggestions | Viewer/Editor | Roadmap effort TODO | Show open/overcrowded niches; suggest plausible traits to fill gaps; warn on unstable food webs. Half editor, half design coach. EcologyGenerator tie-in. |
| 22 | Cross-Layer Dependency Graph Viewer | Viewer/Editor | Prototype TODO | Graph climate → biome → species → settlement → trade → conflict; chokepoint → famine → unrest. Shows "world machine." |
| 23 | Worldbuilding Goal Wizard | Viewer/Editor | Roadmap effort TODO | User picks intention (e.g. grim frontier, high biodiversity ocean, post-imperial sector, TTRPG conflict zone); tool proposes settings/constraints and generates candidates. Overlap with Filters. |
| 24 | Theme / Motif Injector | Generator | Prototype TODO | Bias names, conflicts, myths, institutions, events by theme: decline/decay, corporate control, syncretic religion, frontier improvisation, fragmentation. |
| 25 | Canon Style Guide Enforcer | Validator | Roadmap effort TODO | Naming conventions, transliteration, calendar/date formats, unit systems, faction naming, description tone. Prevents "Kharuul Prime" next to "Bobtown 7." |
| 26 | Atlas / Gazetteer Auto-Compiler | Exporter | Roadmap effort TODO | Browsable compendium from generated content: systems, worlds, species, factions, stations, events, glossary; auto cross-links. Front-end many users will live in. |
| 27 | Reference Image / Prompt Pack Generator | Exporter | Prototype TODO | Art briefs from world data: environment concept, species design sheet, station interior, faction emblem, map. For artists and solo devs. |
| 28 | Playable Slice Exporter | Exporter | Roadmap effort TODO | Scoped chunk for game prototyping: one system, station, 2–3 factions, local economy, conflict hooks, map + data bundle. Game production, not just lore. |
| 29 | World QA Test Pack Generator | Generator | Roadmap effort TODO | Auto-generate regression scenarios: known seeds, expected ranges, edge cases, consistency checks, output snapshots. For StarGen's own dev workflow. |
| 30 | Ethos / Assumption Switchboard | Viewer/Editor | Roadmap effort TODO | Expose assumptions globally: realism vs drama, convergent evolution strength, institutional inertia, catastrophe frequency, conflict escalation. Control philosophy of generation. Overlap with Filters effort. |

*Removed: Data Provenance Watermarking — Roadmap effort (Export function). Collaborative Notes / Annotation Layer — Roadmap effort (Favorites and notes).*

**Summary by type:** Viewer/Editor 18, Validator 2, Generator 4, Exporter 4, Simulator 1.

**Genuinely unusual (prioritise for prototype):** Perspective Filter (#17), Uncertainty / Fog-of-Knowledge Layer (#18), Theme / Motif Injector (#24), Retcon Manager (#4), Causality Inspector (#1), Counterfactual Sandbox (#2), Constraint Debugger (#12), Branch Merger (#9), Diegetic Report Generator (#16), Playable Slice Exporter (#28).
