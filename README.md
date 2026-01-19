# StarGen

A deterministic procedural generator + viewer/editor for celestial objects, solar systems, and galactic structures.

Built with **Godot 4.x** and **GDScript**.

## Project Status

**Current Phase**: Phase 2 - Object generators v1

**Phase 0**: ✅ Complete - Foundations, deterministic RNG, math/validation utilities, and test framework

**Phase 1**: ✅ Complete - Celestial object model with validation, serialization, and persistence

**Phase 2 - Stage 1**: ✅ Complete - Core infrastructure (ParentContext, archetype enums/tables, base specs)

**Phase 2 - Stage 2**: ✅ Complete - Star generator (StarSpec, main sequence relationships, spectral subclass calculation)

**Phase 2 - Stage 3**: ✅ Complete - Planet generator physical core (mass, radius, density, rotation, orbital properties, tidal locking, magnetic field)

**Phase 2 - Stage 4**: ✅ Complete - Planet generator surface & atmosphere (atmospheric retention, composition, greenhouse effect, surface types, terrain, hydrosphere, cryosphere)

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
│   │   │   │   └── AsteroidSpec.gd       # Asteroid generation spec
│   │   │   ├── generators/         # Generator implementations
│   │   │   │   ├── StarGenerator.gd      # Star generation logic
│   │   │   │   ├── PlanetGenerator.gd    # Planet generation logic
│   │   │   │   └── GeneratorUtils.gd     # Shared generator utilities
│   │   │   └── tables/             # Lookup tables for generation
│   │   │       ├── StarTable.gd          # Stellar property tables
│   │   │       ├── SizeTable.gd          # Planet size/mass tables
│   │   │       └── OrbitTable.gd         # Orbital parameter tables
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
│   │       └── CelestialPersistence.gd  # Save/load celestial bodies
│   └── app/                        # Application layer (UI, scenes, rendering)
│       └── (Future: UI, viewer, editor)
├── Tests/                          # Test suite
│   ├── Framework/                 # Test framework
│   │   ├── TestCase.gd            # Base test case class
│   │   ├── TestRunner.gd          # Test execution
│   │   └── TestResult.gd          # Test result data
│   ├── Unit/                       # Unit tests
│   │   ├── TestCelestialBody.gd
│   │   ├── TestStarGenerator.gd
│   │   ├── TestPlanetGenerator.gd
│   │   ├── TestSeededRng.gd
│   │   └── ... (additional unit tests)
│   ├── Integration/                # Integration tests
│   │   └── TestCelestialPersistence.gd
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
