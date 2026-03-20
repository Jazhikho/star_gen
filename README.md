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

StarGen is in active development. The core generation and viewer flow is working, but many areas are still being expanded or refined.

- The galaxy, system, and object studios are the main supported entry points.
- Stations are present as an in-progress studio and broader population/station systems exist in the project.
- The Concept Atlas and concept pipeline are in active 0.7 hardening work. The target is a constrained deterministic chain where environment drives ecology, ecology drives species and sentience, and only sentient populations generate downstream society, religion, and language layers.
- This is still an internal iteration line. The next public release target is `0.8.0.0` after the concept pipeline, realism tuning, and human audit gates are complete.

The current internal development version is `0.7.1.1`, while the app UI intentionally shows the next planned public build label, `0.8.0.0`. Detailed version history and patch notes live in [VERSION.md](VERSION.md).

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

To regenerate the current life-and-settlement baseline outside the main test suite, run:

```bash
godot-mono.exe --path . --headless --script res://Tests/Baselines/RunLifeDistributionBaseline.gd
```
```

## Project Docs

- [VERSION.md](VERSION.md) for release notes and version history
- [Docs/Roadmap.md](Docs/Roadmap.md) for planned efforts and development status
- [Docs/ProjectStructure.md](Docs/ProjectStructure.md) for the codebase layout
- [AI-Use-Statement.md](AI-Use-Statement.md) for the repository AI-use policy
- [AI-Provenance-Log.md](AI-Provenance-Log.md) for significant AI-assisted artifacts

## License

MIT. See [LICENSE](LICENSE).
