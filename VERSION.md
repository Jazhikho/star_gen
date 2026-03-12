# Version

Current version: `0.4.1.0`

Date: `2026-03-12`

Versioning method: release/refactor `+0.1`, feature `+0.0.1`, bug fix `+0.0.0.1`, save-breaking release `+1.0`.

## 0.4.1.0

- Redesigned the main menu into a studio-first dashboard with dedicated galaxy, system, and object entry cards.
- Added standalone `SystemGenerationScreen` and `ObjectGenerationScreen` flows so menu-driven launches configure content before opening viewers.
- Simplified the galaxy viewer generation profile into a read-only summary and routed regeneration back through the galaxy studio.

## 0.4.0.1

- Fixed galaxy-sector star access to return detached snapshots, eliminating disposed `GalaxyStar` failures under full headless test runs.
- Added a garbage-collection regression test covering returned galaxy star snapshots.

## 0.4.0

- Added config-first standalone generation for direct system and object entry.
- Added shared `GenerationUseCaseSettings` across galaxy, system, and object flows.
- Persisted Traveller/worldbuilding settings through galaxy, system, and object save paths.
- Added Traveller-oriented inspector readouts and deterministic mainworld readiness summaries.
- Expanded integration and persistence coverage for config-first startup and use-case round-trips.
