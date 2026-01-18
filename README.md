# StarGen

A deterministic procedural generator + viewer/editor for celestial objects, solar systems, and galactic structures.

Built with **Godot 4.x** and **GDScript**.

## Project Status

**Current Phase**: Phase 2 - Object generators v1

**Phase 0**: ✅ Complete - Foundations, deterministic RNG, math/validation utilities, and test framework

**Phase 1**: ✅ Complete - Celestial object model with validation, serialization, and persistence

**Phase 2 - Stage 1**: ✅ Complete - Core infrastructure (ParentContext, archetype enums/tables, base specs)

**Phase 2 - Stage 2**: ✅ Complete - Star generator (StarSpec, main sequence relationships, spectral subclass calculation)

See [claude.md](claude.md) for detailed architecture, roadmap, and working agreement.

## Project Structure

```
star_gen/
├── src/
│   ├── domain/      # Pure logic (no Nodes, no SceneTree, no file IO)
│   ├── services/    # Persistence/export/caching/adapters
│   └── app/         # Scenes, UI, input, rendering
├── Scenes/          # Godot scene files
├── Scripts/         # Additional scripts (if needed)
├── Resources/       # Godot resources (.tres/.res)
├── Tests/           # Test scenes and test scripts
└── Docs/            # Design documents and documentation
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
