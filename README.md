# StarGen

StarGen is a deterministic worldbuilding tool for creating stars, planets, solar systems, and galaxies, then exploring them through dedicated viewer screens.

It is aimed at science-fiction worldbuilding, setting design, and procedural exploration. The long-term goal is realistic, adjustable generation rather than one-off random flavor text.

## What You Can Do In StarGen

- Generate galaxies, star systems, and individual celestial bodies from dedicated studios on the main menu.
- Explore generated results in viewers with scientific and worldbuilding readouts.
- Save and load generated content.
- Use Traveller-oriented world profile support where available.
- Open the Concept Atlas to experiment with in-development ecology, religion, civilisation, language, disease, and evolution tools in a standalone sandbox.

## Current Status

StarGen is in active development. The core generation and viewer flow is working, but many areas are still being expanded or refined.

- The galaxy, system, and object studios are the main supported entry points.
- Stations are present as an in-progress studio and broader population/station systems exist in the project.
- The Concept Atlas is a tool in development. It is intended to grow into a realistic, user-adjustable set of worldbuilding models, but for now it should be treated as a standalone exploration surface rather than a fully integrated simulation layer.

The current user-facing version is `0.7.0.0`. Detailed version history and patch notes live in [VERSION.md](VERSION.md).

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

## Project Docs

- [VERSION.md](VERSION.md) for release notes and version history
- [Docs/Roadmap.md](Docs/Roadmap.md) for planned efforts and development status
- [Docs/ProjectStructure.md](Docs/ProjectStructure.md) for the codebase layout
- [AI-Use-Statement.md](AI-Use-Statement.md) for the repository AI-use policy
- [AI-Provenance-Log.md](AI-Provenance-Log.md) for significant AI-assisted artifacts

## License

MIT. See [LICENSE](LICENSE).
