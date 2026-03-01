# StarGen

A deterministic procedural generator + viewer for celestial objects, solar systems, and galactic structures.

Built with **Godot 4.x** and **GDScript**.

## Project Status

**Status:** Core object, solar system, and galaxy viewer flows are implemented. Development is organized by **efforts** (see [Docs/Roadmap.md](Docs/Roadmap.md)).

**Done:** Object model, generators, viewer, save/load, rendering (stars, planets, atmospheres, rings). Solar system generator and viewer (full pipeline: stellar config, orbit slots, planets, moons, belts, validation, persistence, 3D layout). Galaxy data model (Galaxy, Sector, GalaxyStar, GalaxySystemGenerator) with lazy system generation; galaxy viewer (welcome screen, GalaxyConfig, density models, save/load). Population framework, stations, jump lanes (domain + prototype).

**Remaining efforts:** Solar system constraints, system viewer rendering improvements, object editing, rendering v2, galactic refinement, solar system tools, galactic tools, galactic polish, jump lanes optimization and polish, code quality & simplifications.

**Test Status:** 1800+ tests; headless run via `godot --headless --script res://Tests/RunTestsHeadless.gd`.

### Version history

| Version | Commit (tag) | Summary |
|---------|--------------|---------|
| **0.1** | `90e2636` | First unofficial release. Object and system viewers, galaxy data model and viewer (welcome screen, GalaxyConfig, density models, save/load). Population framework, stations, jump lanes (domain + prototype). |
| **0.2** | (current) | Asteroid belt generation and rendering in system viewer; scientific calibration (GenerationRealismProfile, benchmarks, ensemble harness, distribution tests); belt renderer/generator integration; OrbitSlotGenerator, OrbitalMechanics, StellarConfigGenerator, SystemValidator updates; GalaxyInspectorPanel and test suite updates. Removed Concepts/AsteroidBelt demo scenes and Tests/RunGalaxyTests.gd. |

### Merged features (master)

All development is on **master**. Branches `object-view` and `effort/traveller-use-case` have been merged into master. The following are merged and current:

- **Galaxy data model** — Galaxy, Sector, GalaxyStar, GalaxySystemGenerator; lazy sector and system generation; metallicity/age from galactic position; wired into GalaxyViewer.
- **Population framework** — PlanetProfile, PopulationLikelihood, native populations, colonies, history; unit tests in `Tests/Unit/Population/`.
- **Station framework** — Outposts, SpaceStations, StationSpec, StationGenerator; prototype at `src/app/prototypes/StationGeneratorPrototype.tscn`.
- **Jump Lanes** — Jump lane domain, prototype, and galaxy viewer integration: `src/domain/jumplanes/`, `src/app/jumplanes_prototype/`, and in galaxy viewer (`SectorJumpLaneRenderer`, Calculate Jump Routes in inspector, save/load). Remaining work: see "Jump lanes optimization and polish" in [Docs/Roadmap.md](Docs/Roadmap.md).

See [Docs/Roadmap.md](Docs/Roadmap.md) and [claude.md](claude.md) for architecture, roadmap, and working agreement.

### Galaxy randomization, welcome screen, and galaxy types (merged)

1. **Welcome screen** — On startup, the app shows a welcome screen (Start New Galaxy, Load Galaxy, Quit). Start New Galaxy uses **GalaxyConfig** (type, spiral arms, pitch, ellipticity, irregularity, seed, etc.) and passes config + seed into the galaxy viewer.
2. **Galaxy types** — Supported types: **Spiral** (arm-based density), **Elliptical** (3D Gaussian ellipsoid), **Irregular** (noise-based 3D blob). Each type uses a dedicated density model (`DensityModelInterface` → `SpiralDensityModel`, `EllipticalDensityModel`, `IrregularDensityModel`) so sampling, quadrant view, and star counts are consistent.
3. **Startup galaxy randomization** — When starting a new galaxy, the seed can be random or user-specified. MainApp creates the galaxy viewer only after the user chooses Start or Load.
4. **Save and load of galaxies** — Users can save the current galaxy (seed + view state) to a file and load a previously saved galaxy. Save/load UI and persistence work with the welcome-screen flow; loaded galaxies restore seed and view state.
5. **New Galaxy** — In the galaxy viewer, a "New Galaxy..." button returns to the welcome screen to generate a different galaxy without restarting the app.

**Contributor minimum file list** (files involved in galaxy welcome screen, types, and save/load):

| Purpose | File |
|--------|------|
| Welcome screen first; Start/Load/Quit; pass config + seed to galaxy viewer | `src/app/MainApp.gd` |
| Welcome screen UI (Start New Galaxy, Load, Quit; config options; seed) | `src/app/WelcomeScreen.gd`, `src/app/WelcomeScreen.tscn` |
| Galaxy generation parameters (type, arms, pitch, ellipticity, irregularity, etc.) | `src/domain/galaxy/GalaxyConfig.gd` |
| Density models (spiral, elliptical, irregular) | `src/domain/galaxy/DensityModelInterface.gd`, `SpiralDensityModel.gd`, `EllipticalDensityModel.gd`, `IrregularDensityModel.gd` |
| Galaxy sampling by type | `src/domain/galaxy/DensitySampler.gd` |
| Galaxy viewer controller; seed/config; save/load; New Galaxy | `src/app/galaxy_viewer/GalaxyViewer.gd` |
| Save/load logic (create_save_data, apply_save_data, file dialogs) | `src/app/galaxy_viewer/GalaxyViewerSaveLoad.gd` |
| Galaxy viewer scene (Save/Load buttons, seed UI) | `src/app/galaxy_viewer/GalaxyViewer.tscn` |
| Save format (galaxy_seed, zoom, camera, selection) | `src/domain/galaxy/GalaxySaveData.gd` |
| File I/O for galaxy save/load | `src/services/persistence/GalaxyPersistence.gd` |
| Galaxy spec (how seed drives generation; reference only) | `src/domain/galaxy/GalaxySpec.gd` |
| Galaxy grid bounds by type | `src/domain/galaxy/GalaxyCoordinates.gd` |
| Galaxy data model (top-level container, lazy sectors/systems) | `src/domain/galaxy/Galaxy.gd` |
| Sector (100pc³ region, lazy star generation) | `src/domain/galaxy/Sector.gd` |
| GalaxyStar (position, seed, metallicity, age bias) | `src/domain/galaxy/GalaxyStar.gd` |
| On-demand system generation from GalaxyStar | `src/domain/galaxy/GalaxySystemGenerator.gd` |
| Dependency preload for galaxy viewer (reference only) | `src/app/galaxy_viewer/GalaxyViewerDeps.gd` |

**Tests** (galaxy and welcome screen):

| Test file | Purpose |
|-----------|---------|
| `Tests/Unit/TestGalaxyConfig.gd` | GalaxyConfig defaults, create_milky_way, validation. |
| `Tests/Integration/TestWelcomeScreen.gd` | Welcome screen signals and UI behavior. |
| `Tests/Integration/TestGalaxyStartup.gd` | MainApp shows welcome first; start new galaxy flow. |
| `Tests/Integration/TestGalaxyRandomization.gd` | Random seed generation and viewer receipt of seed/config. |
| `Tests/Integration/TestMainAppNavigation.gd` | Navigation and galaxy seed (non-zero when galaxy active). |
| `Tests/Unit/TestGalaxySaveData.gd` | Save-format round-trip. |
| `Tests/Integration/TestGalaxyPersistence.gd` | Save/load JSON and binary round-trip including `galaxy_seed`. |
| `Tests/domain/galaxy/TestDensitySampler.gd` | Spiral/elliptical/irregular sampling, no-disk elliptical, 3D distribution tests. |
| `Tests/Unit/TestGalaxy.gd` | Galaxy create, sector caching, determinism, get_stars_in_radius, serialization. |
| `Tests/Unit/TestGalaxyStar.gd` | Metallicity/age gradients, distance helpers. |
| `Tests/Unit/TestSector.gd` | Sector creation, lazy generation, subsector indexing, determinism. |
| `Tests/Unit/TestGalaxySystemGenerator.gd` | System generation from GalaxyStar, provenance, parent_id validity. |

**Scenes** (galaxy and welcome flow):

| Scene | Purpose |
|-------|---------|
| `src/app/MainApp.tscn` | Root scene (`run/main_scene`). Holds `ViewerContainer`; MainApp.gd shows WelcomeScreen first, then instantiates GalaxyViewer on Start or Load. |
| `src/app/WelcomeScreen.tscn` | Welcome screen: Start New Galaxy (with config), Load Galaxy, Quit. |
| `src/app/galaxy_viewer/GalaxyViewer.tscn` | Galaxy viewer: UI (TopBar, SidePanel), Save/Load section, seed input. |

MainApp shows WelcomeScreen first; GalaxyViewer is created only after Start New Galaxy or Load Galaxy.

## Project Structure

Full project structure is enumerated in [Docs/ProjectStructure.md](Docs/ProjectStructure.md).

## Development

### Running Tests

**Option 1: Run headless (recommended for CI)**
```bash
godot --headless --script res://Tests/RunTestsHeadless.gd
```

**Option 2: Run via scene**
```bash
godot --path . res://Tests/TestScene.tscn
```

### Building
This is a Godot project. Open `project.godot` in Godot 4.x editor.

## License

MIT License — see [LICENSE](LICENSE).
