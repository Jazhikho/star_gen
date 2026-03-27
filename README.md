# StarGen

StarGen is a deterministic worldbuilding tool for creating stars, planets, solar systems, and galaxies, then exploring them through dedicated viewer screens.

It is aimed at science-fiction worldbuilding, setting design, and procedural exploration. The long-term goal is realistic, adjustable generation rather than one-off random flavor text.

## What You Can Do In StarGen

- Generate galaxies, star systems, and individual celestial bodies from dedicated studios on the main menu.
- Explore generated results in viewers with scientific and worldbuilding readouts.
- Save and load generated content.
- Use Traveller-oriented world profile support where available.
- Open the Concept Atlas to experiment with in-development ecology, species, sentience, religion, civilisation, language, and disease tools.

## Current Status

StarGen `0.8.0.0` is the current public release baseline.

- The galaxy, system, and object studios are the main supported entry points.
- The station studio now uses the production station-generation flow rather than a placeholder shell.
- The Concept Atlas remains available from the main menu as an in-development worldbuilding tool.
- The concept pipeline now follows a deterministic dependency chain from environment through ecology, species, sentience, and downstream society layers.

The current release version is `0.8.0.0`. Detailed version history and patch notes live in [VERSION.md](VERSION.md).

## Running StarGen

StarGen is a Godot .NET project.

1. Open `project.godot` in Godot 4.6.x .NET.
2. Build the C# project if prompted.
3. Run the main scene from the editor.

You can also build the C# solution directly:

```bash
dotnet build StarGen.sln
```

## Testing

Run the main automated checks with:

```bash
dotnet build StarGen.sln
godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd
```

To regenerate the current life-and-settlement baseline outside the main test suite, run:

```bash
godot-mono.exe --path . --headless --script res://Tests/Baselines/RunLifeDistributionBaseline.gd
```

## Project Docs

- [VERSION.md](VERSION.md) for release notes and version history
- [Docs/Roadmap.md](Docs/Roadmap.md) for planned efforts and development status
- [Docs/ProjectStructure.md](Docs/ProjectStructure.md) for the codebase layout
- [AI-Use-Statement.md](AI-Use-Statement.md) for the repository AI-use policy
- [AI-Provenance-Log.md](AI-Provenance-Log.md) for significant AI-assisted artifacts

## License

MIT. See [LICENSE](LICENSE).
