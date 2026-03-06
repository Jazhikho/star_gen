# StarGen

A deterministic procedural generator + viewer for celestial objects, solar systems, and galactic structures.

Built with **Godot 4.x** and a **C#-first** codebase. Remaining GDScript files are limited to launchers, fallback harness glue, and historical reference copies.

## Project Status

**Status:** Core object, solar system, and galaxy viewer flows are implemented. Development is organized by **efforts** (see [Docs/Roadmap.md](Docs/Roadmap.md)).

**Done:** Object model, generators, viewer, save/load, rendering (stars, planets, atmospheres, rings). Solar system generator and viewer (full pipeline: stellar config, orbit slots, planets, moons, belts, validation, persistence, 3D layout). Galaxy data model (Galaxy, Sector, GalaxyStar, GalaxySystemGenerator) with lazy system generation; galaxy viewer (welcome screen, GalaxyConfig, density models, save/load). Population framework, stations, jump lanes (domain + prototype).

**Remaining efforts:** Save format and compatibility (ZSTD .sgg/.sgb C# port); solar system constraints; system viewer rendering improvements; object editing; rendering v2; galactic refinement; solar system tools; galactic tools; galactic polish; jump lanes optimization and polish; code quality & simplifications.

**Test Status:** 1800+ tests. The supported runtime for both headless and interactive execution is **Godot .NET 4.6.x**. Headless runs start from `godot-mono.exe --headless --script res://Tests/RunTestsHeadless.gd`, which launches the C# harness. Interactive runs use `res://Tests/TestScene.tscn`, which boots the same C# suite manifest through a thin GDScript launcher. `dotnet build StarGen.sln` validates the C# codebase.

### Version history

| Version | Commit (tag) | Summary |
|---------|--------------|---------|
| **0.1** | `90e2636` | First unofficial release. Object and system viewers, galaxy data model and viewer (welcome screen, GalaxyConfig, density models, save/load). Population framework, stations, jump lanes (domain + prototype). |
| **0.2** | `a99ef2c` | Asteroid belt generation and rendering in system viewer; scientific calibration (GenerationRealismProfile, benchmarks, ensemble harness, distribution tests); belt renderer/generator integration; OrbitSlotGenerator, OrbitalMechanics, StellarConfigGenerator, SystemValidator updates; GalaxyInspectorPanel and test suite updates. Removed Concepts/AsteroidBelt demo scenes and Tests/RunGalaxyTests.gd. |
| **0.3** | — | **User-facing:** Main menu with user-focused copy and Release Notes; save/load for bodies and systems (ObjectViewer save/load partial, path/extension handling); gas giant variety (archetypes, per-planet variation); edit a body in the object viewer and save as file (optional Traveller UWP size code in edit dialog). **Under the hood:** C# refactor (core, tests, harness; GDScript removed from src/). (Traveller Use Case, full object editing, and other efforts remain in progress and are not yet fully live.) (current) |

### Release notes (summary)

- **0.3.0** — Main menu: user-focused copy and Release Notes. Save/load: body and system files with correct extensions. Gas giant variety in system view. Edit a body and save as file (edit dialog; optional UWP size code). C# refactor under the hood. Traveller Use Case and full object editing are in progress, not yet fully in-app.
- **0.2** — Asteroid belts in system viewer; scientific calibration and benchmarks; belt renderer and generator integration; test suite and GalaxyInspectorPanel updates.
- **0.1** — First release: object and system viewers, galaxy data model and viewer (welcome, config, density models, save/load), population framework, stations, jump lanes.

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
| Welcome screen first; Start/Load/Quit; pass config + seed to galaxy viewer | `src/app/MainApp.cs`, `src/app/MainApp.Navigation.cs` |
| Welcome screen UI (Start New Galaxy, Load, Quit; config options; seed) | `src/app/WelcomeScreen.cs`, `src/app/WelcomeScreen.tscn` |
| Galaxy generation parameters (type, arms, pitch, ellipticity, irregularity, etc.) | `src/domain/galaxy/GalaxyConfig.cs` |
| Density models (spiral, elliptical, irregular) | `src/domain/galaxy/DensityModelInterface.cs`, `SpiralDensityModel.cs`, `EllipticalDensityModel.cs`, `IrregularDensityModel.cs` |
| Galaxy sampling by type | `src/domain/galaxy/DensitySampler.cs` |
| Galaxy viewer controller; seed/config; save/load; New Galaxy | `src/app/galaxy_viewer/GalaxyViewer.cs`, `GalaxyViewer.Accessors.cs`, `GalaxyViewer.GdCompat.cs` |
| Save/load logic (create save data, apply save data, file dialogs) | `src/app/galaxy_viewer/GalaxyViewerSaveLoad.cs` |
| Galaxy viewer scene (Save/Load buttons, seed UI) | `src/app/galaxy_viewer/GalaxyViewer.tscn` |
| Save format (galaxy seed, zoom, camera, selection) | `src/domain/galaxy/GalaxySaveData.cs` |
| File I/O for galaxy save/load | `src/services/persistence/GalaxyPersistence.cs` |
| Galaxy spec (how seed drives generation; reference only) | `src/domain/galaxy/GalaxySpec.cs` |
| Galaxy grid bounds by type | `src/domain/galaxy/GalaxyCoordinates.cs` |
| Galaxy data model (top-level container, lazy sectors/systems) | `src/domain/galaxy/Galaxy.cs` |
| Sector (100pc³ region, lazy star generation) | `src/domain/galaxy/Sector.cs` |
| GalaxyStar (position, seed, metallicity, age bias) | `src/domain/galaxy/GalaxyStar.cs` |
| On-demand system generation from GalaxyStar | `src/domain/galaxy/GalaxySystemGenerator.cs` |

**Tests** (galaxy and welcome screen):

| Test file | Purpose |
|-----------|---------|
| `Tests/Unit/TestGalaxyConfig.cs` | GalaxyConfig defaults, create_milky_way, validation. |
| `Tests/Integration/TestWelcomeScreen.cs` | Welcome screen signals and UI behavior. |
| `Tests/Integration/TestGalaxyStartup.cs` | MainApp shows welcome first; start new galaxy flow. |
| `Tests/Integration/TestGalaxyRandomization.cs` | Random seed generation and viewer receipt of seed/config. |
| `Tests/Integration/TestMainAppNavigation.cs` | Navigation and galaxy seed (non-zero when galaxy active). |
| `Tests/Unit/TestGalaxySaveData.cs` | Save-format round-trip. |
| `Tests/Integration/TestGalaxyPersistence.cs` | Save/load JSON and binary round-trip including `galaxy_seed`. |
| `Tests/domain/galaxy/TestDensitySampler.cs` | Spiral/elliptical/irregular sampling, no-disk elliptical, 3D distribution tests. |
| `Tests/Unit/TestGalaxy.cs` | Galaxy create, sector caching, determinism, get_stars_in_radius, serialization. |
| `Tests/Unit/TestGalaxyStar.cs` | Metallicity/age gradients, distance helpers. |
| `Tests/Unit/TestSector.cs` | Sector creation, lazy generation, subsector indexing, determinism. |
| `Tests/Unit/TestGalaxySystemGenerator.cs` | System generation from GalaxyStar, provenance, parent_id validity. |

**Scenes** (galaxy and welcome flow):

| Scene | Purpose |
|-------|---------|
| `src/app/MainApp.tscn` | Root scene (`run/main_scene`). Holds `ViewerContainer`; `MainApp.cs` shows `WelcomeScreen` first, then instantiates `GalaxyViewer` on Start or Load. |
| `src/app/WelcomeScreen.tscn` | Welcome screen: Start New Galaxy (with config), Load Galaxy, Quit. |
| `src/app/galaxy_viewer/GalaxyViewer.tscn` | Galaxy viewer: UI (TopBar, SidePanel), Save/Load section, seed input. |

MainApp shows WelcomeScreen first; GalaxyViewer is created only after Start New Galaxy or Load Galaxy.

## Project Structure

Full project structure is enumerated in [Docs/ProjectStructure.md](Docs/ProjectStructure.md).

## Development

### Running Tests

Use **Godot .NET 4.6.x** for runtime and test execution.

**Option 1: Run headless (recommended for CI)**
```bash
godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd
```

Use a Godot .NET 4.6.x / Mono build for this command. `RunTestsHeadless.gd` is only the launcher; it verifies the runtime version, then boots the C# harness scene, which executes the explicit suite manifest in `Tests/TestRegistry.cs`.

**Option 2: Run via scene**
```bash
godot-mono.exe --path . res://Tests/TestScene.tscn
```

`TestScene.tscn` uses `TestScene.gd`, which verifies the runtime version, boots `TestSceneCSharp.tscn`, and runs the interactive C# suites from the same manifest.

### Building
This is a Godot project. Open `project.godot` in Godot 4.x editor.

Build the C# solution directly:
```bash
dotnet build StarGen.sln
```

## License

MIT License — see [LICENSE](LICENSE).
