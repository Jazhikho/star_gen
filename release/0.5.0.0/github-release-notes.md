# StarGen 0.5.0.0

First public release since 0.3.0.

## Highlights
- config-first Galaxy, System, and Object studios
- Traveller/UWP-oriented object generation and viewer readouts
- station design fold-in with generation, classification, persistence, and export support
- responsive UI/layout cleanup across menus, studios, and viewers
- updated provenance logging and release metadata

## Validation
- `dotnet build StarGen.sln` passed cleanly
- headless test suite passed: `1901 / 1901`

## Downloads
- Windows build included
- Linux build included

## Note on Web
Web is not included in this release build. The current Godot 4 C# export pipeline does not support Web export, so the itch `web` channel was not updated as part of 0.5.0.0.
