# StarGen

StarGen is a deterministic worldbuilding tool for creating stars, planets, solar systems, and galaxies, then exploring them through dedicated viewer screens.

It is aimed at science-fiction worldbuilding, setting design, and procedural exploration. The long-term goal is realistic, adjustable generation rather than one-off random flavor text.

## What You Can Do In StarGen

- Generate galaxies, star systems, and individual celestial bodies from dedicated studios on the main menu.
- Explore generated results in viewers with scientific and worldbuilding readouts.
- Use Traveller-oriented world profile support where available.

## Current Status

StarGen `0.9d` is the current mainline checkpoint baseline.

- The galaxy, system, and object studios are the main supported entry points.
- The station studio now uses the production station-generation flow rather than a placeholder shell.
- The galaxy studio now uses a scientifically grounded galaxy-generation profile with family-locked variation, cited assumptions, and downstream galaxy-context propagation into stars and systems.
- The stellar generator now supports a broader practical population, including brown dwarfs, evolved stars, white dwarfs, and stronger multi-star hierarchies instead of only main-sequence stars.
- Galaxy Studio and System Studio now share aggregate planetary-formation controls, so envelope loss, gas-giant formation, metallicity coupling, rogue-planet allowance, moon-formation bias, and outer-system small-body bias can shape downstream planet populations without turning Object Studio into a disk-physics editor.
- Planet, moon, small-body, and biology generation now respond more directly to the upstream planetary state: hot worlds are more sensitive to envelope loss, volatile delivery changes wet vs dry outcomes, moon families react to host class and snow-line context, outer belts shift with primitive icy reservoir strength, and biosphere support now considers stellar flux, habitable-zone alignment, XUV exposure, and tidal heating.
- Mainline is generation-and-view focused; save/load/export flows are reserved for the export branch.
- Mainline no longer ships the Concept Atlas path. `Concepts/Additions.md` remains the StarGen prototype backlog for future work that stays in scope.

The current user-facing release version is `0.9d`, while the current internal checkpoint version is `0.8.7.1`. Detailed version history and patch notes live in [VERSION.md](VERSION.md).

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

## Repository Maintenance Note

- Release output folders are now ignored by git (`release/` and `releases/`).
- On the next push/cleanup cycle, remove any release artifacts that were already uploaded to the remote repository/server.

## License

MIT. See [LICENSE](LICENSE).
