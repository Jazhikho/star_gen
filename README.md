# StarGen

A deterministic procedural generator + viewer for celestial objects, solar systems, and galactic structures.

Built with **Godot 4.x** and a **C#-first** codebase. Remaining GDScript files are limited to launchers, fallback harness glue, and historical reference copies.

## AI Use

This repository allows human-directed, AI-assisted work under the rules in [AI-Use-Statement.md](AI-Use-Statement.md).

Significant AI-assisted artifacts should be logged in [AI-Provenance-Log.md](AI-Provenance-Log.md).

## Project Status

**Status:** Core object, solar system, and galaxy viewer flows are implemented. Development is organized by **efforts** (see [Docs/Roadmap.md](Docs/Roadmap.md)).

**Done:** Object model, generators, viewer, save/load, rendering (stars, planets, atmospheres, rings). Solar system generator and viewer (full pipeline: stellar config, orbit slots, planets, moons, belts, validation, persistence, 3D layout). Galaxy data model (Galaxy, Sector, GalaxyStar, GalaxySystemGenerator) with lazy system generation; galaxy viewer (welcome screen, GalaxyConfig, density models, save/load). Population framework, stations, jump lanes (domain + prototype).

**Remaining efforts:** Save format and compatibility (ZSTD .sgg/.sgb C# port); solar system constraints; system viewer rendering improvements; object editing; rendering v2; galactic refinement; solar system tools; galactic tools; galactic polish; jump lanes optimization and polish; code quality & simplifications.

**Test Status:** 1900+ tests. The supported runtime for both headless and interactive execution is **Godot .NET 4.6.x**. Headless runs start from `godot-mono.exe --headless --script res://Tests/RunTestsHeadless.gd`, which launches the C# harness. Interactive runs use `res://Tests/TestScene.tscn`, which boots the same C# suite manifest through a thin GDScript launcher. `dotnet build StarGen.sln` validates the C# codebase.

**UI minimum supported width:** `640 px`. Wrapped labels should be given a sensible `CustomMinimumSize.X` based on their panel width so word wrapping remains stable instead of stretching layouts.

### Version history

| Version | Commit (tag) | Summary |
|---------|--------------|---------|
| **0.1.0** | `90e2636` | First unofficial release. Object and system viewers, galaxy data model and viewer (welcome screen, GalaxyConfig, density models, save/load). Population framework, stations, jump lanes (domain + prototype). |
| **0.2.0** | `a99ef2c` | Asteroid belt generation and rendering in system viewer; scientific calibration (GenerationRealismProfile, benchmarks, ensemble harness, distribution tests); belt renderer/generator integration; OrbitSlotGenerator, OrbitalMechanics, StellarConfigGenerator, SystemValidator updates; GalaxyInspectorPanel and test suite updates. Removed Concepts/AsteroidBelt demo scenes and Tests/RunGalaxyTests.gd. |
| **0.3.0** | - | User-facing main menu and release notes, body/system save-load, gas giant variety, optional Traveller size-code support in object editing, and the completed C# refactor. |
| **0.4.0** | - | Config-first standalone system/object entry, shared Traveller/worldbuilding settings across galaxy/system/object flows, persisted Traveller readouts, and deterministic mainworld readiness summaries. |
| **0.5.0.0** | - | First public release since `0.3.0`, rolling up the internal `0.4.x` work: config-first studios, Traveller-aligned launch/readout support, station design fold-in, UI/navigation polish, and AI provenance/release sync. (current) |
| **0.4.3.5** | - | Fixed the main-menu scene/script mismatch after the user-driven menu rewrite and added direct regression coverage for the restored main-menu actions. |
| **0.4.3.3** | - | Removed launch-summary clutter from the studios, hid studio seeds behind the Options preference with reroll-on-launch behavior, added a Station Studio entry point, and kept the galaxy viewer on a single new-galaxy path. |
| **0.4.3.2** | - | Moved viewer menus below the header, removed duplicate header back affordances, fixed Traveller world-generation edge cases, and clarified `Auto / Yes / No` plus population-assumption wording across the launch flows. |
| **0.4.3.1** | - | Reworked the studio layouts to stack responsively, added summary scrolling/tooltips, and fixed the worst screen-clipping issues across the menu, splash, and launch studios. |
| **0.4.3.0** | - | Reworked object generation into an explicit spec builder, added Traveller planet profile generation/UWP output, and moved UWP world-profile readouts to the top of the inspector. |
| **0.4.2.3** | - | Fixed window/resolution application so display changes take effect immediately and loosened the galaxy studio layout. |
| **0.4.2.2** | - | Moved galaxy profile editing fully into the galaxy studio, split galaxy-viewer studio vs main-menu return actions, and expanded object-viewer inspector readouts. |
| **0.4.2.1** | - | Fixed station-design compact save/load to preserve full spec data and aligned hull-band sizing with the generated station class. |
| **0.4.2.0** | - | Folded detailed station design into the main station framework, added exporter/save-load/regression coverage, and retired the SpaceStationBuilder prototype. |
| **0.4.1.1** | - | Patch update: added galaxy snapshot GC regression coverage and consolidated the 0.4.0 MVP / Traveller documentation into the current docs set. |
| **0.4.1.0** | - | Studio-first generation flow: redesigned main menu, dedicated system/object setup screens, and viewer launches that open with generated content. |
| **0.4.0.1** | - | Fixed galaxy star snapshot lifetime so returned sector stars remain valid under full headless test execution. |

### Release notes (summary)

- **0.5.0.0** - First public release since `0.3.0`, rolling up the internal `0.4.x` work: config-first studios, Traveller-aligned launch/readout support, station design fold-in, UI/navigation polish, and AI provenance/release sync.
- **0.4.3.5** - Fixed the main-menu scene/script mismatch after the user-driven menu rewrite and added direct regression coverage for the restored main-menu actions.
- **0.4.3.4** - Reformatted the AI provenance log into readable entries, documented/enforced a 640 px minimum supported width, fixed object-viewer main-menu return routing, and added sensible wrap minima across key wrapped UI labels.
- **0.4.3.3** - Removed studio launch-summary clutter, hid studio seeds behind the Options preference with reroll-on-launch behavior, added the Station Studio entry point, and kept the galaxy viewer on a single `New Galaxy...` path.
- **0.4.3.2** - Moved viewer menus below the header, removed duplicate header return affordances, fixed Traveller blank-world edge cases, and clarified optional-feature and population-assumption wording.
- **0.4.3.1** - Reworked the launch studio layouts to stack responsively, added summary scrolling/tooltips, and reduced clipping across the menu, splash, and pre-launch screens.
- **0.4.3.0** - Reworked object generation into an explicit spec builder, added Traveller planet profile generation and deterministic UWP output, and surfaced world-profile readouts at the top of the inspector.
- **0.4.2.3** - Fixed immediate window/fullscreen application and loosened the galaxy studio layout so the parameter panel has more room.
- **0.4.2.2** - Moved galaxy profile editing fully into the galaxy studio, split galaxy-viewer studio vs main-menu return actions, and expanded object-viewer inspector readouts and Traveller context.
- **0.4.2.1** - Fixed station-design compact reloads to preserve full spec fields and aligned detailed hull sizing with the generated station class.
- **0.4.2.0** - Folded the SpaceStationBuilder prototype into the main station framework with deterministic station design, classification, persistence, export, and regression fixtures.
- **0.4.1.1** - Added the galaxy snapshot lifetime regression test, folded the 0.4.0 MVP scope into the roadmap/docs, and aligned the repo-local versioning note.
- **0.4.1.0** - Redesigned the menu into a studio dashboard, added dedicated system/object generation studios, and shifted menu-driven generation setup out of the viewers.
- **0.4.0.1** - Fixed galaxy star snapshot lifetime so returned sector stars remain valid under full headless test execution.
- **0.4.0** - Config-first standalone generation for system and object entry. Shared Traveller/worldbuilding settings across galaxy, system, and object flows. Traveller/UWP readouts and mainworld readiness summaries are persisted and exposed in inspectors.
- **0.3.0** - Main menu, release notes, body/system save-load, gas giant variety, optional Traveller size code in object editing, and the C# refactor.
- **0.2.0** - Asteroid belts in system viewer; scientific calibration and benchmarks; belt renderer and generator integration; test suite and GalaxyInspectorPanel updates.
- **0.1.0** - First release: object and system viewers, galaxy data model and viewer (welcome, config, density models, save/load), population framework, stations, jump lanes.

### Merged features (master)

All development is on **master**. Branches `object-view` and `effort/traveller-use-case` have been merged into master. The following are merged and current:

- **Galaxy data model** — Galaxy, Sector, GalaxyStar, GalaxySystemGenerator; lazy sector and system generation; metallicity/age from galactic position; wired into GalaxyViewer.
- **Population framework** — PlanetProfile, PopulationLikelihood, native populations, colonies, history; unit tests in `Tests/Unit/Population/`.
- **Station framework** - Outposts, SpaceStations, StationSpec, StationGenerator, detailed station design (`src/domain/population/station_design/`), and `src/services/export/StationStatBlockExporter.cs`.
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

