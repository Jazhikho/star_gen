# StarGen

A deterministic procedural generator + viewer for celestial objects, solar systems, and galactic structures.

Built with **Godot 4.x** and **GDScript**.

## Project Status

**Status:** Core object, solar system, and galaxy viewer flows are implemented. Development is organized by **efforts** (see [Docs/Roadmap.md](Docs/Roadmap.md)).

**Done:** Object model, generators, viewer, save/load, rendering (stars, planets, atmospheres, rings). Solar system generator and viewer (full pipeline: stellar config, orbit slots, planets, moons, belts, validation, persistence, 3D layout). Galaxy viewer (welcome screen, GalaxyConfig, density models, save/load). Population framework, stations, jump lanes (domain + prototype).

**Remaining efforts:** Solar system constraints, save/load polish, galaxy data model & lazy generation, object editing, rendering v2, galactic refinement, solar system tools, galactic tools, galactic polish, jump lanes integration, code quality & simplifications.

**Test Status:** 1000+ tests; headless run via `godot --headless --script res://Tests/RunTestsHeadless.gd`.

### Merged features (master)

All development is on **master**. The following are merged and current:

- **Population framework** — PlanetProfile, native populations, colonies, history; unit tests in `Tests/Unit/Population/`.
- **Station framework** — Outposts, SpaceStations, StationSpec, StationGenerator; prototype at `src/app/prototypes/StationGeneratorPrototype.tscn`.
- **Jump Lanes** — Jump lane domain and prototype in `src/domain/jumplanes/` and `src/app/jumplanes_prototype/`. See [Docs/FeatureConceptBranch.md](Docs/FeatureConceptBranch.md) for scope and [Docs/FeatureConceptBranchImplementationPlan.md](Docs/FeatureConceptBranchImplementationPlan.md) for implementation plan.

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
