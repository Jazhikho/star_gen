# StarGen

A deterministic procedural generator + viewer for celestial objects, solar systems, and galactic structures.

Built with **Godot 4.x** and **GDScript**.

## Project Status

**Current Phase**: Phase 7 - Galactic map v1 (in progress)

**Phase 6**: ✅ Complete - Solar system generator and viewer (Stages 1-11 Complete, Stage 12 Deferred)

**Phase 0**: ✅ Complete - Foundations, deterministic RNG, math/validation utilities, and test framework

**Phase 1**: ✅ Complete - Celestial object model with validation, serialization, and persistence

**Phase 2**: ✅ Complete - Object generators v1
- **Stage 1**: ✅ Core infrastructure (ParentContext, archetype enums/tables, base specs)
- **Stage 2**: ✅ Star generator (StarSpec, main sequence relationships, spectral subclass calculation)
- **Stage 3**: ✅ Planet generator physical core (mass, radius, density, rotation, orbital properties, tidal locking, magnetic field)
- **Stage 4**: ✅ Planet generator surface & atmosphere (atmospheric retention, composition, greenhouse effect, surface types, terrain, hydrosphere, cryosphere)
- **Stage 5**: ✅ Moon generator (MoonSpec, parent planet context, Hill sphere constraints, subsurface oceans)
- **Stage 6**: ✅ Asteroid generator (AsteroidSpec, C/S/M types, physical properties, minimal atmosphere, basic terrain)
- **Stage 7**: ✅ Ring system generator (RingSystemSpec, Roche limit calculation, resonance-based gaps, ice/rock composition)
- **Stage 8**: ✅ Golden masters & tests (28 fixtures, regression tests, range validation, physics relationship tests)

**Phase 3 - Stage 1**: ✅ Complete - Basic viewer infrastructure (ObjectViewer scene, camera controls, UI structure)
- ObjectViewer scene with 3D viewport and environment
- CameraController with orbit/pan/zoom controls
- UI panel structure (top bar, side panel)
- Integration tests (7 tests)

**Phase 3 - Stage 2**: ✅ Complete - Object generation & display
- Generate/re-roll buttons with type selector and seed input
- All four object types (star/planet/moon/asteroid) can be generated
- Type-specific scaling and camera distance
- Basic info display with appropriate units
- Integration tests (4 new tests)

**Phase 3 - Stage 3**: ✅ Complete - Inspector panel
- InspectorPanel class with dynamic property creation
- Collapsible sections for all component types
- All property sections (Physical, Stellar, Orbital, Atmosphere, Surface, Ring System)
- Proper unit formatting with Unicode preserved (M☉, R☉, M⊕, R⊕, ×, superscripts)
- Mouse input handling (UI blocks camera input)
- Soft environmental backlighting
- Integration tests (3 new tests)

**Phase 3 - Stage 4**: ✅ Complete - Save/Load system
- SaveData service with compression (Zstandard) support
- Regeneration-based storage (spec + context, not full bodies)
- Save/load buttons with file dialogs (.sgb binary, .json debug)
- Error handling for invalid files
- File size utilities and formatting
- Deterministic round-trip (save → load → same object)
- Integration tests (12 new tests)

**Phase 3 - Stages 5 & 6**: ✅ Complete - Body rendering system
- Complete rendering pipeline (ColorUtils, MaterialFactory, BodyRenderer)
- Star rendering: blackbody colors, emission shader with limb darkening/corona, OmniLight3D, glow effects
- Planet/moon rendering: surface type colors, albedo, roughness, gas giant bands, icy materials
- Atmosphere rendering: rim-lighting shader, composition-based sky colors, pressure-based thickness
- Ring system rendering: multi-band meshes, composition colors, optical depth opacity, equatorial alignment
- Body rotation animation with axial tilt support
- Material caching for performance
- Unit tests for color utilities (17 new tests)

**Phase 3 - Stage 8**: ✅ Complete - Ring system rendering (implemented with Stages 5 & 6)

**Phase 6 - Stage 1**: ✅ Complete - Core Data Model
- HierarchyNode: Represents single stars and binary pairs with arbitrary nesting support
- SystemHierarchy: Manages hierarchical arrangement of stars with tree traversal
- OrbitHost: Computed orbit hosts with stability zones, habitable zones, and frost line calculations
- AsteroidBelt: Defines asteroid belt regions with boundaries and major asteroid references
- SolarSystem: Main container for complete solar systems with efficient ID-based body lookups
- Complete serialization/deserialization for all data model classes
- Unit tests (45 new tests covering all data model classes)

**Phase 6 - Stage 2**: ✅ Complete - Orbital Mechanics Utilities
- OrbitalMechanics: Comprehensive orbital mechanics calculations (Kepler's laws, Hill sphere, Roche limit, stability limits, resonances, perturbations, synodic periods)
- S-type and P-type stability limit calculations for binary systems
- Resonance spacing with variation support
- Habitable zone and frost line calculations
- Unit tests (26 new tests covering all orbital mechanics functions)

**Phase 6 - Stage 3**: ✅ Complete - Stellar Configuration Generator
- SolarSystemSpec: Specification for system generation with star count ranges, spectral class hints, system age/metallicity
- StellarConfigGenerator: Generates stars, builds hierarchies, calculates orbit hosts
- Weighted star count selection (favors single stars)
- Hierarchical binary system building
- Orbit host calculation with stability limits
- Unit tests (31 new tests covering spec and generator)

**Phase 6 - Stage 4**: ✅ Complete - Orbit Slot Generator
- OrbitSlot: Candidate orbital positions with zone classification, stability, fill probability, and suggested eccentricity
- OrbitSlotGenerator: Generates slots with resonance spacing, exponential probability decay, star radius safety margin
- Utility functions: filters (stable, available, by zone), sorts (by distance, probability), statistics
- Batch generation for multiple hosts
- Unit tests (29 new tests covering slot and generator)

**Phase 6 - Stage 10**: ✅ Complete - System Viewer 3D Setup
- ✅ SystemCameraController: Top-down orbital view with smooth zoom, pan, and orbit controls
- ✅ SystemScaleManager: Astronomical distance/size conversions with Kepler's equation solver
- ✅ OrbitRenderer: 3D line mesh rendering for orbital paths with type-based coloring
- ✅ SystemViewer.gd: Main viewer controller script with system display logic
- ✅ SystemViewer.tscn: Scene file with UI structure, camera rig, containers
- ✅ Integration tests (SystemCameraController, SystemScaleManager, OrbitRenderer, SystemViewer)

**Phase 6 - Stage 11**: ✅ Complete - System Viewer Bodies
- ✅ SystemBodyNode: 3D body representation with materials, selection, hover, click detection
- ✅ SystemInspectorPanel: System overview and selected body details panel
- ✅ Unit tests (SystemBodyNode, SystemInspectorPanel)
- ✅ Body rendering integration in SystemViewer (adaptive sizing based on orbital spacing)
- ✅ Body selection with camera focus (click to select, camera focuses on selected body)
- ✅ Link to ObjectViewer for detailed inspection (MainApp handles navigation)

**Phase 6 - Stage 12**: ⏳ Deferred - System Viewer Polish
- ✅ Zone visualization (habitable zone and frost line rings)
- ✅ View toggles (show/hide orbits and zones)
- ✅ System generation UI (star count, seed input, generate/reroll buttons)
- ⏳ Save/load UI for systems (deferred to Phase 12 - Solar system polish)

**Phase 7 - Galactic map v1** (in progress):
- ✅ Welcome screen: Start New Galaxy (with config), Load Galaxy, Quit; app shows welcome first, then creates galaxy viewer on Start or Load
- ✅ GalaxyConfig: Galaxy generation parameters (type, spiral arms, pitch, ellipticity, irregularity, seed, etc.)
- ✅ Galaxy types: Spiral, Elliptical, Irregular with dedicated density models (DensityModelInterface, SpiralDensityModel, EllipticalDensityModel, IrregularDensityModel)
- ✅ DensitySampler: Type-specific star sampling; GalaxyCoordinates: galaxy-type-aware grid bounds
- ✅ Galaxy viewer: 3D view with quadrant/sector/subsector zoom; MultiMesh star points with type-specific coloring; New Galaxy button
- ✅ Save/load galaxy state (seed, zoom, camera, selection)
- ⏳ Galaxy data model (Galaxy, Sector, GalaxyStar) and lazy system generation (future)

**Test Status**: 951+ tests in suite; headless run reports passing (some integration tests expect pre-welcome flow and are being updated).

### Branch: population (parallel concept)

The **population** branch develops a **population framework for planets** (native populations and their history, plus colonies). It is documented in [Docs/Roadmap.md](Docs/Roadmap.md) under "Branch: population (parallel concept)" and in [Docs/PopulationFrameworkPlan.md](Docs/PopulationFrameworkPlan.md). This work is intentionally separate from the main roadmap for now; it can be run and tested independently, while the normal test suite for the main app continues to run unchanged. The framework will be integrated into the main branch when ready.

- **Stage 1 (Planet Profile Model):** ✅ Complete — PlanetProfile, ClimateZone, BiomeType, ResourceType, HabitabilityCategory; unit tests in `Tests/Unit/Population/`.

See [claude.md](claude.md) for detailed architecture, roadmap, and working agreement.

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

**Tests** (added or updated for this branch):

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

**Scenes** (minimum scenes involved in this branch):

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
│   │   │   └── ... (coordinates, subsectors, etc.)
│   │   ├── math/                   # Math utilities
│   │   │   ├── MathUtils.gd       # Range checking, remapping, interpolation
│   │   │   └── Units.gd            # Physical constants and unit conversions
│   │   ├── rng/                    # Random number generation
│   │   │   └── SeededRng.gd       # Deterministic RNG wrapper
│   │   ├── validation/             # General validation utilities
│   │   │   └── Validation.gd       # Range and type validation
│   │   └── constants/              # Project constants
│   │       └── Versions.gd        # Version tracking
│   ├── services/                   # Services layer (I/O, persistence)
│   │   └── persistence/
│   │       ├── SaveData.gd         # Efficient save/load with compression
│   │       └── CelestialPersistence.gd # File I/O service for persistence
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
│           │   └── gas_giant.gdshader # Gas giant band shader
│           └── textures/          # Texture assets
│               └── noise.tres     # Noise texture
├── Tests/                          # Test suite
│   ├── Framework/                 # Test framework
│   │   ├── TestCase.gd            # Base test case class
│   │   ├── TestRunner.gd          # Test execution
│   │   └── TestResult.gd          # Test result data
│   ├── Unit/                       # Unit tests
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
│   └── Phase1Deps.gd               # Phase dependencies
├── Docs/                           # Documentation
│   └── Roadmap.md                  # Development roadmap
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

_(To be added)_
