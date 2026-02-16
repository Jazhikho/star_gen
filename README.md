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

```
star_gen/
├── src/
│   ├── domain/                    # Pure logic (no Nodes, no SceneTree, no file IO)
│   │   ├── celestial/            # Core celestial body model
│   │   │   ├── CelestialBody.gd  # Main data model for celestial objects
│   │   │   ├── CelestialType.gd  # Type enum (star, planet, moon, asteroid)
│   │   │   ├── Provenance.gd     # Generation metadata tracking
│   │   │   ├── components/        # Property components
│   │   │   │   ├── PhysicalProps.gd      # Mass, radius, rotation, etc.
│   │   │   │   ├── OrbitalProps.gd       # Orbital parameters
│   │   │   │   ├── StellarProps.gd       # Star-specific properties
│   │   │   │   ├── AtmosphereProps.gd    # Atmospheric composition
│   │   │   │   ├── SurfaceProps.gd       # Surface characteristics
│   │   │   │   ├── TerrainProps.gd       # Terrain features
│   │   │   │   ├── HydrosphereProps.gd   # Water/ocean properties
│   │   │   │   ├── CryosphereProps.gd    # Ice properties
│   │   │   │   ├── RingSystemProps.gd    # Ring system data
│   │   │   │   └── RingBand.gd          # Individual ring band
│   │   │   ├── serialization/     # Serialization logic
│   │   │   │   └── CelestialSerializer.gd
│   │   │   └── validation/        # Validation logic
│   │   │       ├── CelestialValidator.gd
│   │   │       ├── ValidationResult.gd
│   │   │       └── ValidationError.gd
│   │   ├── generation/            # Procedural generation system
│   │   │   ├── ParentContext.gd   # Context for child body generation
│   │   │   ├── archetypes/        # Type classification enums
│   │   │   │   ├── StarClass.gd          # Spectral class (O/B/A/F/G/K/M)
│   │   │   │   ├── SizeCategory.gd      # Planet size categories
│   │   │   │   ├── OrbitZone.gd          # Orbital zones (hot/temperate/cold)
│   │   │   │   ├── AsteroidType.gd        # Asteroid composition types
│   │   │   │   └── RingComplexity.gd     # Ring system complexity levels
│   │   │   ├── specs/              # Generation specifications
│   │   │   │   ├── BaseSpec.gd           # Base spec with seed/overrides
│   │   │   │   ├── StarSpec.gd           # Star generation spec
│   │   │   │   ├── PlanetSpec.gd         # Planet generation spec
│   │   │   │   ├── MoonSpec.gd           # Moon generation spec
│   │   │   │   ├── AsteroidSpec.gd       # Asteroid generation spec
│   │   │   │   └── RingSystemSpec.gd      # Ring system generation spec
│   │   │   ├── generators/         # Generator implementations
│   │   │   │   ├── StarGenerator.gd      # Star generation logic
│   │   │   │   ├── PlanetGenerator.gd    # Planet generation orchestration
│   │   │   │   ├── MoonGenerator.gd      # Moon generation orchestration
│   │   │   │   ├── AsteroidGenerator.gd  # Asteroid generation logic
│   │   │   │   ├── RingSystemGenerator.gd # Ring system generation logic
│   │   │   │   ├── GeneratorUtils.gd     # Shared generator utilities
│   │   │   │   ├── planet/               # Planet generation components
│   │   │   │   │   ├── PlanetPhysicalGenerator.gd    # Physical properties
│   │   │   │   │   ├── PlanetAtmosphereGenerator.gd  # Atmosphere properties
│   │   │   │   │   └── PlanetSurfaceGenerator.gd     # Surface properties
│   │   │   │   └── moon/                # Moon generation components
│   │   │   │       ├── MoonPhysicalGenerator.gd      # Physical properties
│   │   │   │       ├── MoonAtmosphereGenerator.gd    # Atmosphere properties
│   │   │   │       └── MoonSurfaceGenerator.gd       # Surface properties
│   │   │   ├── utils/              # Generation utilities
│   │   │   │   └── AtmosphereUtils.gd    # Shared atmosphere calculations
│   │   │   ├── fixtures/           # Golden master fixtures
│   │   │   │   └── FixtureGenerator.gd   # Fixture generation utility
│   │   │   └── tables/             # Lookup tables for generation
│   │   │       ├── StarTable.gd          # Stellar property tables
│   │   │       ├── SizeTable.gd          # Planet size/mass tables
│   │   │       └── OrbitTable.gd         # Orbital parameter tables
│   │   ├── system/                 # Solar system data model
│   │   │   ├── SolarSystem.gd          # Main system container
│   │   │   ├── SystemHierarchy.gd      # Stellar hierarchy tree
│   │   │   ├── HierarchyNode.gd        # Hierarchy node (star or barycenter)
│   │   │   ├── OrbitHost.gd            # Orbit host with stability zones
│   │   │   ├── AsteroidBelt.gd         # Asteroid belt definition
│   │   │   ├── OrbitalMechanics.gd     # Orbital mechanics calculations
│   │   │   ├── SolarSystemSpec.gd      # System generation specification
│   │   │   ├── StellarConfigGenerator.gd # Stellar configuration generator
│   │   │   ├── OrbitSlot.gd            # Candidate orbital position
│   │   │   └── OrbitSlotGenerator.gd   # Orbit slot generator
│   │   ├── galaxy/                 # Galaxy-scale data and generation
│   │   │   ├── GalaxyConfig.gd    # Galaxy generation parameters (type, arms, ellipticity, etc.)
│   │   │   ├── GalaxySaveData.gd   # Save format (seed, zoom, camera, selection)
│   │   │   ├── GalaxySpec.gd       # How seed drives galaxy generation
│   │   │   ├── DensityModelInterface.gd  # Base for density models; create_for_spec(spec)
│   │   │   ├── SpiralDensityModel.gd     # Spiral arm + bulge + disk density
│   │   │   ├── EllipticalDensityModel.gd # 3D Gaussian ellipsoid
│   │   │   ├── IrregularDensityModel.gd  # Noise-based 3D blob
│   │   │   ├── DensitySampler.gd   # Type-specific star sampling
│   │   │   ├── HomePosition.gd     # Home system/position for galaxy
│   │   │   └── ... (coordinates, subsectors, star picker, etc.)
│   │   ├── math/                   # Math utilities
│   │   │   ├── MathUtils.gd       # Range checking, remapping, interpolation
│   │   │   └── Units.gd            # Physical constants and unit conversions
│   │   ├── rng/                    # Random number generation
│   │   │   └── SeededRng.gd       # Deterministic RNG wrapper
│   │   ├── validation/             # General validation utilities
│   │   │   └── Validation.gd       # Range and type validation
│   │   ├── population/             # Population framework (native, colony, history)
│   │   │   ├── PlanetProfile.gd   # Planet habitability/profile model
│   │   │   ├── ProfileGenerator.gd # Profile generation
│   │   │   ├── ColonySuitability.gd # Colony desirability
│   │   │   ├── PopulationGenerator.gd # Full population integration
│   │   │   └── ... (ClimateZone, BiomeType, Colony, Government, etc.)
│   │   └── constants/              # Project constants
│   │       └── Versions.gd        # Version tracking
│   ├── services/                   # Services layer (I/O, persistence)
│   │   └── persistence/
│   │       ├── SaveData.gd         # Efficient save/load with compression
│   │       ├── CelestialPersistence.gd # File I/O for celestial objects
│   │       ├── SystemPersistence.gd # System save/load
│   │       └── GalaxyPersistence.gd # Galaxy save/load
│   └── app/                        # Application layer (UI, scenes, rendering)
│       ├── MainApp.gd             # Root application controller (navigation)
│       ├── MainApp.tscn           # Root application scene
│       ├── WelcomeScreen.gd       # Startup screen (Start New / Load / Quit)
│       ├── WelcomeScreen.tscn    # Welcome screen scene
│       ├── components/           # Reusable UI components
│       │   └── CollapsibleSection.gd/.tscn
│       ├── themes/                # UI themes
│       │   └── DarkTheme.tres
│       ├── galaxy_viewer/         # Galaxy map viewer
│       │   ├── GalaxyViewer.gd    # Galaxy viewer controller
│       │   ├── GalaxyViewer.tscn  # Galaxy viewer scene
│       │   ├── GalaxyViewerSaveLoad.gd # Save/load logic
│       │   ├── OrbitCamera.gd     # Galaxy orbit camera
│       │   └── ... (renderers, UI, zoom, etc.)
│       ├── viewer/                # Object viewer
│       │   ├── ObjectViewer.tscn  # Main viewer scene
│       │   ├── ObjectViewer.gd    # Viewer controller
│       │   ├── CameraController.gd # Orbital camera controls
│       │   ├── InspectorPanel.gd  # Dynamic property inspector
│       │   ├── PropertyFormatter.gd # Property formatting utilities
│       │   ├── EditDialog.gd      # Object editing dialog (Phase 4, deferred)
│       │   └── EditDialog.tscn    # Edit dialog scene (Phase 4, deferred)
│       ├── system_viewer/         # Solar system viewer
│       │   ├── SystemViewer.gd    # Main system viewer controller
│       │   ├── SystemViewer.tscn  # System viewer scene
│       │   ├── SystemCameraController.gd # System camera controls
│       │   ├── SystemScaleManager.gd # Scale transformations
│       │   ├── SystemDisplayLayout.gd # Layout + sweep-based separation (multi-star no-overlap)
│       │   ├── SystemBodyNode.gd  # Individual body node
│       │   ├── SystemBodyNode.tscn # Body node scene
│       │   ├── OrbitRenderer.gd   # Orbit path rendering
│       │   └── SystemInspectorPanel.gd # System inspector panel
│       └── rendering/             # Body rendering system
│           ├── BodyRenderer.gd    # Main body rendering logic
│           ├── BodyRenderer.tscn  # Body renderer scene
│           ├── ColorUtils.gd      # Color calculation utilities
│           ├── MaterialFactory.gd # Material generation
│           ├── shaders/           # Shader assets
│           │   ├── star.gdshader  # Star emission shader
│           │   ├── stellar_concept.gdshader # Star concept (granulation, corona)
│           │   ├── gas_giant.gdshader # Gas giant band shader
│           │   ├── planet_terrestrial.gdshader # Terrestrial planet concept
│           │   └── planet_gas_giant_concept.gdshader # Gas giant concept
│           └── textures/          # Texture assets
│               └── noise.tres     # Noise texture
├── Tests/                          # Test suite
│   ├── Framework/                 # Test framework
│   │   ├── TestCase.gd            # Base test case class
│   │   ├── TestRunner.gd          # Test execution
│   │   └── TestResult.gd          # Test result data
│   ├── Unit/                       # Unit tests
│   │   ├── Population/             # Population framework tests
│   │   │   ├── TestPlanetProfile.gd
│   │   │   ├── TestColonyGenerator.gd
│   │   │   └── ...
│   │   ├── TestCelestialBody.gd
│   │   ├── TestStarGenerator.gd
│   │   ├── TestPlanetGenerator.gd
│   │   ├── TestMoonGenerator.gd
│   │   ├── TestAsteroidGenerator.gd
│   │   ├── TestRingSystemGenerator.gd
│   │   ├── TestGoldenMasters.gd
│   │   ├── TestSeededRng.gd
│   │   ├── TestHierarchyNode.gd
│   │   ├── TestSystemHierarchy.gd
│   │   ├── TestOrbitHost.gd
│   │   ├── TestAsteroidBelt.gd
│   │   ├── TestSolarSystem.gd
│   │   ├── TestOrbitalMechanics.gd
│   │   ├── TestSolarSystemSpec.gd
│   │   ├── TestStellarConfigGenerator.gd
│   │   ├── TestOrbitSlot.gd
│   │   ├── TestOrbitSlotGenerator.gd
│   │   ├── TestSystemScaleManager.gd
│   │   ├── TestOrbitRenderer.gd
│   │   ├── TestSystemDisplayLayout.gd
│   │   ├── TestSystemBodyNode.gd
│   │   ├── TestSystemInspectorPanel.gd
│   │   └── ... (additional unit tests)
│   ├── Integration/                # Integration tests
│   │   ├── TestObjectViewer.gd
│   │   ├── TestSaveLoad.gd
│   │   ├── TestCelestialPersistence.gd
│   │   ├── TestSystemCameraController.gd
│   │   ├── TestSystemViewer.gd
│   │   └── TestMainApp.gd
│   ├── TestScene.tscn              # Test scene
│   ├── TestScene.gd                # Test scene script
│   ├── RunTestsHeadless.gd         # Headless test runner
│   ├── RunGalaxyTests.gd           # Galaxy test runner (optional)
│   ├── Phase1Deps.gd               # Phase 1/generation dependencies
│   └── PopulationDeps.gd           # Population domain dependencies
├── Concepts/                       # Visual concept demos (reference)
│   ├── planetgenerator.html        # Planet shader concepts
│   ├── stargenerator.html          # Star shader concepts
│   └── Additions.md                # Future visual features
├── Docs/                           # Documentation
│   ├── Roadmap.md                  # Development roadmap
│   ├── FeatureConceptBranch.md     # Jump Lanes scope
│   ├── FeatureConceptBranchImplementationPlan.md  # Jump Lanes implementation plan
│   ├── CelestialBodyProperties.md  # Celestial body property reference
│   └── RegimeChangeModel.md        # Regime change model (population)
├── .editorconfig                   # Editor configuration
├── .gitattributes                  # Git attributes (LFS, etc.)
├── .gitignore                      # Git ignore rules
├── BACKLOG.md                      # Feature backlog
├── claude.md                       # Architecture and working agreement
├── project.godot                   # Godot project file
└── README.md                       # This file
```

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
