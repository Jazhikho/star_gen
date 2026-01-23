# StarGen

A deterministic procedural generator + viewer for celestial objects, solar systems, and galactic structures.

Built with **Godot 4.x** and **GDScript**.

## Project Status

**Current Phase**: Phase 6 - Solar system generator and viewer (Stages 1-4, 7-12 Complete)

**Next Phase**: Phase 6 Stage 5 (Planet Generation) and Stage 6 (Moon Generation)

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
- SystemViewer: Main viewer scene with 3D viewport, environment, and UI structure
- SystemCameraController: Top-down orbital view with smooth zoom, pan, and orbit controls (mouse + keyboard)
- SystemScaleManager: Astronomical distance/size conversions with Kepler's equation solver for orbital positioning
- OrbitRenderer: 3D line mesh rendering for orbital paths with type-based coloring and selection highlighting
- Integration tests (SystemCameraController, SystemScaleManager, OrbitRenderer)

**Phase 6 - Stage 11**: ✅ Complete - System Viewer Bodies
- SystemBodyNode: 3D body representation with materials, selection rings, hover effects, and click detection
- Body rendering in system context with type-specific scaling and materials (reuses MaterialFactory)
- SystemInspectorPanel: System overview and selected body details with property formatting
- Body selection with camera focus and orbit highlighting
- Star lighting with temperature-based colors and luminosity scaling
- Unit tests (SystemBodyNode, SystemInspectorPanel)

**Phase 6 - Stage 12**: ✅ Complete - System Viewer Polish
- Zone visualization: Habitable zone and frost line rings for each orbit host
- View toggles: Show/hide orbits and zones via checkboxes
- System generation UI: Star count selector (1-4), seed input, generate/reroll buttons
- Status messages and error handling
- Asteroid rendering limit (50 max) for performance
- Selection indicators and hover effects
- Note: Save/load UI for systems deferred (system persistence exists but UI integration pending)

**Test Status**: All 501 tests passing ✅

See [claude.md](claude.md) for detailed architecture, roadmap, and working agreement.

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
│   │   └── TestSystemCameraController.gd
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
