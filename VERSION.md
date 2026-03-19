# Version

Current version: `0.7.0.0`

Date: `2026-03-19`

Versioning method: release/refactor `+0.1`, feature `+0.0.1`, bug fix `+0.0.0.1`, save-breaking release `+1.0`.

## 0.7.0.0

- Release: Kept the user-facing `0.7.0.0` label in place for the showcase branch while concept tools remain presented as a standalone atlas in development.
- Docs: Synced roadmap, project structure, README, in-app release notes, and provenance to reflect the current standalone-atlas state and the deferred cross-layer integration effort.
- Patch follow-up: Moved Concept Atlas into the Generation Studios section, added Station Studio guidance to Help, added AI-usage credit copy, made the atlas sidebar scroll cleanly, normalized concept-facing identifier text into sentence case, replaced dead system-level concept summaries with populated-world focus actions, and removed automatic concept generation from the normal pipeline.

## 0.6.1.0

- Feature: Post-review concept showcase patch cycle: the Concept Atlas stays standalone, context-aware, and sandbox-friendly while automatic concept generation is removed from generation, save/load, preview, and viewer flows for now.
- Test: Added regression coverage for the relocated main-menu Concept Atlas entry, the updated help/credits copy, sentence-case concept text, populated-world focus actions, and compact system persistence without auto concept state.

## 0.6.0.0

- Release: Prepared the Release 1 showcase milestone with all selected concept prototypes accessible inside StarGen through the Concept Atlas and viewer inspection surfaces.
- Docs: Synced roadmap, additions tracking, README, in-app release notes, and AI provenance for the showcase-ready Concept Atlas presentation.

## 0.5.8.0

- Feature: Added context-aware Concept Atlas launch points from the galaxy, system, and object inspection surfaces, with return navigation back to the originating viewer.
- Test: Added integration coverage for inspector-driven atlas launches in `MainApp`, `GalaxyViewer`, `SystemViewer`, and `ObjectViewer`.

## 0.5.7.0

- Feature: Folded the evolution concept into the Concept Atlas with deterministic lineage, trait, and species-profile generation.
- Test: Added deterministic presenter coverage for the evolution atlas module.

## 0.5.6.0

- Feature: Folded the disease concept into the Concept Atlas with deterministic outbreak traits, symptom bundles, and epidemic summary metrics.
- Test: Added deterministic presenter coverage for the disease atlas module.

## 0.5.5.0

- Feature: Folded the language concept into the Concept Atlas with deterministic phonology, grammar, lexicon, and sample utterance generation.
- Test: Added deterministic presenter coverage for the language atlas module.

## 0.5.4.0

- Feature: Folded the civilisation concept into the Concept Atlas with deterministic regime, economy, culture, and timeline summaries generated from StarGen population context.
- Test: Added deterministic presenter coverage for the civilisation atlas module.

## 0.5.3.0

- Feature: Folded the religion concept into the Concept Atlas with deterministic belief-system generation, doctrinal summaries, and landscape readouts seeded from population context.
- Test: Added deterministic presenter coverage for the religion atlas module.

## 0.5.2.0

- Feature: Folded the ecology concept into the Concept Atlas with deterministic environment mapping, trophic-profile metrics, and niche summaries.
- Test: Added `DotNetNativeTestSuite.Concepts.cs` coverage for the ecology atlas presenter.

## 0.5.1.0

- Feature: Added the first Concept Atlas shell with shared concept context/provenance plumbing, a manual concept sandbox, and a main-menu entry point for upcoming fold-ins.
- Docs: Added roadmap and project-structure tracking for the Concept Atlas and cross-layer concept integration efforts.

## 0.5.0.1

- Hotfix: Set StarGen.png as project/app icon; added Docs/Assets.md with Flaticon/Freepik attribution; added icon credit to in-app Credits.

## 0.5.0.0

- First public release since `0.3.0`, rolling up the internal `0.4.x` work into a single release build.
- Adds config-first galaxy/system/object studios, Traveller-aligned launch settings and UWP/world-profile readouts, richer viewer navigation, and broad UI/layout polish across the app.
- Folds detailed station design into the main population framework with deterministic presets, classification, persistence, export, regression fixtures, and a Station Studio entry point marked as in-progress.
- Formalizes AI-use disclosure and provenance tracking, syncs release notes inside and outside the program, and ships with the current clean build plus passing headless suite.

## 0.4.3.5

- Fixed the main-menu scene/script mismatch after the recent layout rewrite so all studio buttons and utility actions are wired against the current scene graph again.
- Added direct main-menu integration coverage for mode-button navigation signals and fallback utility dialogs so future menu edits fail loudly in tests instead of silently breaking the UI.
- Kept the utility fallback path available when embedded content panels are absent, matching the user-edited main-menu layout without losing help, credits, release notes, or options.

## 0.4.3.4

- Reformatted the AI provenance log from a single wide markdown table into readable per-entry sections and explicitly recorded recent human changes to the main menu, splash screen, and station studio placeholder.
- Fixed object-viewer main-menu return routing after the recent menu changes, aligned the stale system-viewer Traveller-controls regression with the current slider-based UI, and kept the menu-linked flows working again.
- Added sensible minimum widths to wrapped labels across the main menu, splash, studios, station placeholder, object viewer file info, and edit-dialog validation rows; documented a minimum supported window width of 640 px and enforced that floor in the window settings service.

## 0.4.3.3

- Removed the launch-summary clutter from the galaxy, system, and object studios so the action footer stays visible at smaller resolutions.
- Hid studio seeds behind the main-menu Options preference, rerolled hidden seeds on fresh studio launches, and added a placeholder Station Studio entry point in the main menu.
- Kept the galaxy viewer on a single `New Galaxy...` path, aligned permissiveness wording around named worldbuilding bands, and noted post-release build verification for windowed resolution behavior.

## 0.4.3.2

- Moved viewer menus below the header row, removed duplicate header-level return affordances from the object/system viewers, and kept return navigation menu-scoped.
- Fixed Traveller object-generation edge cases so fully auto Traveller worlds avoid the blank all-zero profile, optional feature controls read `Auto / Yes / No`, and viewer summaries show `None` instead of `Forbidden`.
- Renamed permissiveness controls to `Life Potential` and `Settlement Density`, applied Traveller-leaning defaults across galaxy/system/object flows, and added regression coverage for the updated navigation and Traveller launch paths.

## 0.4.3.1

- Reworked the studio-screen layouts to stack responsively on narrower windows, added summary-panel scrolling, and reduced fixed chrome that was clipping text and pushing actions off-screen.
- Tightened visible summary copy, moved more explanatory text into tooltips, and reduced oversized row label widths so object, system, and galaxy studios stay usable at smaller sizes.
- Added shared studio-layout regression coverage and refreshed the splash/main-menu layouts to behave better in constrained window sizes.

## 0.4.3.0

- Reworked the Object Generation Studio into an explicit spec builder so creation mode now carries the same core generator parameters and override paths as the object editor.
- Added Traveller world-profile generation for planets with per-field auto vs fixed selections, deterministic UWP output, and mapped physical/hydrosphere overrides.
- Moved planet UWP and broader world-profile readouts to the top of the object inspector, added studio/request and Traveller generator regression coverage, and added explicit asteroid studio spec support.

## 0.4.2.3

- Fixed window-mode application to update the active root window as well as persisted settings, so resolution and fullscreen changes take effect immediately.
- Reduced galaxy studio chrome and widened the parameter side of the split layout so the launcher no longer feels cramped at common desktop widths.
- Added direct window-application regression coverage for windowed and fullscreen mode changes.

## 0.4.2.2

- Moved galaxy parameter ownership fully into the Galaxy Generation Studio and converted the galaxy viewer profile panel into a read-only active-profile summary.
- Added a dedicated galaxy-viewer top-menu route back to the main menu so studio return and main-menu return are separate actions.
- Expanded the object viewer inspector to surface orbit, surface, atmosphere, rings, population, generation snapshot, and Traveller/mainworld context already present in generated bodies.

## 0.4.2.1

- Fixed the station-design compact save/load path to preserve the full design spec, including non-default auto flags and officer ratios.
- Fixed detailed station hull sizing to use the actual generated station class instead of re-deriving a possibly different small-station span from template alone.
- Added regression coverage for scalar legacy station-design reloads and explicit U-vs-O hull-band mapping.

## 0.4.2.0

- Folded the SpaceStationBuilder prototype into the main station framework with deterministic presets, calculators, classification, persistence, export, and regression fixtures.
- Added detailed station-design integration tests, fixture-backed regressions, and save/load compatibility coverage for full and compact station design payloads.
- Retired the `Concepts/SpaceStationBuilder/` prototype and synced roadmap/project-structure documentation for the merged station-design subsystem.

## 0.4.1.1

- Added a regression test covering returned galaxy-star snapshot lifetime after garbage collection.
- Folded the 0.4.0 MVP scope note into the roadmap and Traveller reference docs, and removed the standalone scope document.
- Updated the repo-local `claude.md` versioning section to match the active repository versioning rules.

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
