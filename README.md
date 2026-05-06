# StarGen

StarGen is a deterministic worldbuilding tool for creating stars, planets, solar systems, and galaxies, then exploring them through dedicated viewer screens.

It is aimed at science-fiction worldbuilding, setting design, and procedural exploration. The long-term goal is realistic, adjustable generation rather than one-off random flavor text.

## What You Can Do In StarGen

- Generate galaxies, star systems, and individual celestial bodies from dedicated studios on the main menu.
- Explore generated results in viewers with scientific and worldbuilding readouts.
- Use Traveller-oriented world profile support where available.

## Current Status

StarGen is currently working toward the user-facing `0.11` weekly release line. The default public build remains generation-and-view focused, and the export-enabled channel keeps save/load flows available for authoring builds.

- The galaxy, system, and object studios are the main supported entry points.
- The station studio now uses the production station-generation flow rather than a placeholder shell.
- The galaxy studio now uses a scientifically grounded galaxy-generation profile with family-locked variation, cited assumptions, and downstream galaxy-context propagation into stars and systems.
- Galaxy Studio now keeps scientific assumptions in the left column and generation overrides in the center column, including a source-aligned life-model stack for abiogenesis, complex life, civilization emergence, and environmental-window weighting.
- System Studio now mirrors the audited science surfaces for stellar, planetary, and life controls, so its left column carries the same kind of source-backed assumptions instead of a mixed science/override layout.
- System Viewer is now viewer-only again: the active `SystemViewer.tscn` no longer embeds a second generator or save/load panel, and the scene visible in the engine matches the runtime viewer chrome much more closely.
- The stellar generator now supports a broader practical population, including brown dwarfs, evolved stars, white dwarfs, and stronger multi-star hierarchies instead of only main-sequence stars.
- Galaxy Studio and System Studio now share aggregate planetary-formation controls, so envelope loss, gas-giant formation, metallicity coupling, rogue-planet allowance, moon-formation bias, and outer-system small-body bias can shape downstream planet populations without turning Object Studio into a disk-physics editor.
- Object Studio now stays scoped to direct single-object authoring: aggregate formation and life sliders are removed, direct object controls carry explicit tooltips, and the generated request persists those direct choices into the outgoing spec payload.
- Galaxy Studio, System Studio, and Object Studio now share the first clean-room RPG compatibility scaffold for `Space Opera`, `Cepheus`, `Starfinder`, and `Starforged`, and those profiles now bias system fill, mainworld pressure, and population generation rather than existing only as labels.
- Galaxy Studio and System Studio now expose the concrete `Space Opera` override levers that profile touches, so users can tune mainworld pressure, temperate vs harsh fill pressure, mainworld-class pressure, native-life bias, and settlement bias without changing the default realistic mode.
- Galaxy Viewer now exposes `Tools` and `Options` in the top bar, supports explicit `Build Local Space` caching around the current local view, and keeps jump-route work dependent on that cached nearby-system profile instead of silently rebuilding hidden data.
- Galaxy Viewer overview coordinates now report `Quadrant`, `Sector`, and `Local`, while selected systems add their hierarchical coordinates in `System Preview`.
- Planet, moon, small-body, and biology generation now respond more directly to the upstream planetary state: hot worlds are more sensitive to envelope loss, volatile delivery changes wet vs dry outcomes, moon families react to host class and snow-line context, outer belts shift with primitive icy reservoir strength, and biosphere support now considers stellar flux, habitable-zone alignment, XUV exposure, and tidal heating.
- The public `0.11` target is generation-and-view focused.
- The export-enabled `0.11` channel restores save/load flows for paid distribution while keeping the visible release label aligned.
- Mainline no longer ships the Concept Atlas path. `Concepts/Additions.md` remains the StarGen prototype backlog for future work that stays in scope.

The current internal checkpoint is `0.10.2.0`, with user-facing target `0.11`. Detailed version history and patch notes live in [VERSION.md](VERSION.md).

## Running StarGen

StarGen is a Godot .NET project.

1. Open `project.godot` in Godot 4.6.x .NET.
2. Build the C# project if prompted.
3. Run the main scene from the editor.

You can also build the C# solution directly:

```bash
dotnet build StarGen.sln
```

For release exports and itch-ready packaging on Windows, use:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\CreateReleaseBuild.ps1 -GodotExe "C:\Path\To\Godot_v4.6-stable_mono_win64_console.exe"
```

To cut the export-enabled edition instead of the default demo edition, pass `-Edition export`.

That helper runs `dotnet build`, runs the headless harness by default, temporarily stamps the requested edition channel, exports the configured release presets, writes artifacts under `release/<internal-version>/<artifact-label>/`, and prints suggested itch `butler` commands. Export-enabled artifacts use an `-export` artifact label while keeping the in-app version label at the configured user-facing release target.

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
- [Docs/V0.10ReleaseChecklist.md](Docs/V0.10ReleaseChecklist.md) for the `0.10` build, packaging, and itch-release procedure
- [Docs/V0.10AcceptanceChecklist.md](Docs/V0.10AcceptanceChecklist.md) for the live manual verification pass before publishing
- [Docs/V1.0Checklist.md](Docs/V1.0Checklist.md) for the remaining work required before a defensible `1.0`
- [AI-Use-Statement.md](AI-Use-Statement.md) for the repository AI-use policy
- [AI-Provenance-Log.md](AI-Provenance-Log.md) for significant AI-assisted artifacts

## Repository Maintenance Note

- Release output folders are now ignored by git (`release/` and `releases/`).
- On the next push/cleanup cycle, remove any release artifacts that were already uploaded to the remote repository/server.

## License

MIT. See [LICENSE](LICENSE).
