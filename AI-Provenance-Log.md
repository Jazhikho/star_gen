# AI Provenance Log

Use this log for significant AI-assisted artifacts in this repository.

## Entry Template

### YYYY-MM-DD - Tool / Model

- Task Purpose:
- Input Materials Used:
- AI Produced:
- Human Accepted:
- Human Rejected:
- Human Changed:
- Validation Method:
- Final Approver:

## Entries

### Before 2026-03-12 - GPT Codex; Cursor; Claude Sonnet; Claude Opus

- Task Purpose: Prior repository assistance before a formal AI policy existed.
- Input Materials Used: Historical repository work, contributor prompts, local code and docs.
- AI Produced: Mixed prior code, documentation, review, drafting, and exploration assistance across earlier work.
- Human Accepted: Historical human-reviewed contributions retained in the repository.
- Human Rejected: Unknown or untracked rejected portions due to missing formal provenance records.
- Human Changed: Humans integrated, revised, tested, or discarded outputs as part of normal repo workflow to the extent then practiced.
- Validation Method: Historical provenance was not fully documented before the policy existed. Existing retained work was treated as human-directed repository history.
- Final Approver: Historical approvals not fully reconstructed. The developer has approved this, accepts this as an incomplete log due to the policy being adopted late. Any part from this timeframe that is found to be non-compliant will be rectified in the future.

### 2026-03-12 - Codex (GPT-5)

- Task Purpose: Draft StarGen AI-use policy and review repo compliance against that policy.
- Input Materials Used: User-provided ForgeWalker policy draft, `claude.md`, `README.md`, `VERSION.md`, `Docs/Roadmap.md`, `Docs/ProjectStructure.md`, project file inventory.
- AI Produced: Drafted `AI-Use-Statement.md`, created this provenance log, and produced a compliance review with remediation steps.
- Human Accepted: Project-specific AI policy structure, provenance fields, and compliance findings format.
- Human Rejected: Generic company naming and non-StarGen-specific language from the source draft.
- Human Changed: Tailored the policy to deterministic generation, scientific realism, contributor workflow, and culture-adjacent concept work in this repo.
- Validation Method: Human review of repository documents and file inventory; no code behavior changed.
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-13 - Codex (GPT-5)

- Task Purpose: Prepare and publish the `0.5.0.0` release after final human acceptance of the release candidate.
- Input Materials Used: User release approval in chat, `VERSION.md`, `README.md`, `Docs/Roadmap.md`, `Docs/ProjectStructure.md`, in-app version and release-note surfaces, export preset metadata, git/GitHub state, and itch publishing channels.
- AI Produced: Updated public and in-app documentation/version surfaces to `0.5.0.0`, prepared release-note summaries for the public release rollup, built the configured release artifacts, and published the release state to GitHub and itch.
- Human Accepted: The `0.5.0.0` version target, the decision to treat the internal `0.4.x` work as a single public release since `0.3.0`, and the request to publish to GitHub and itch.
- Human Rejected: No additional feature work beyond release prep and publication was added in this pass.
- Human Changed: Release approval and final pre-release acceptance were made by Christopher B. Del Gesso after manual review, including confirmation of windowed-resolution behavior, button/functionality checks, Traveller acceptance-for-now, station workflow acceptance, and edits to the provenance log itself.
- Validation Method: `dotnet build StarGen.sln`, Godot headless harness, export builds for the configured release channels, GitHub release publication, and itch channel publication.
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-12 - Codex (GPT-5)

- Task Purpose: Fold the `Concepts/SpaceStationBuilder/` prototype into the main station framework and bring the effort to merge-ready.
- Input Materials Used: User-selected implementation plan; `claude.md`; `AGENTS.md`; existing station and population code; `Docs/Roadmap.md`; `Docs/ProjectStructure.md`; `Concepts/Additions.md`; prototype files in `Concepts/SpaceStationBuilder/`; existing station-design files and tests.
- AI Produced: Implemented the canonical station-design subsystem under `src/domain/population/station_design/`, added classification, mapping, persistence, exporter, regression fixtures, integration tests, roadmap/project-structure/version updates, and retired the folded-in prototype.
- Human Accepted: The fold-in architecture, calculators, serialization, exporter, regression fixtures, test registration, documentation sync, version bump to `0.4.2.0`, and prototype retirement.
- Human Rejected: Temporary fixture-dump helper files created during development were removed before completion; incompatible duplicate root `PortRating*` and prototype-only surfaces were retired in favor of the canonical layout.
- Human Changed: Adjusted the implementation to match repo naming and namespace conventions (`FacilityKind`, `DockingBerthKind`, `AccommodationKind`, `Classification` namespace), fixed compile/test issues discovered during verification, and reduced documentation updates to the repo's actual merged structure.
- Validation Method: `dotnet build StarGen.sln` and the Godot headless harness (`Total: 1878 | Passed: 1878 | Failed: 0`).
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-12 - Codex (GPT-5)

- Task Purpose: Fix post-review station-design merge inconsistencies before merge.
- Input Materials Used: Review findings on `SpaceStation` compact persistence and `DesignMapping` hull-band derivation; `claude.md`; `AGENTS.md`; station-design domain files; station generator; serialization and integration tests; version metadata files.
- AI Produced: Patched station save/load to persist and restore the full compact design payload, preserved legacy scalar fallback with full spec fields, changed hull sizing to use the actual generated station class, and added regression coverage for both behaviors.
- Human Accepted: The persistence and mapping fixes, new regression tests, and bug-fix version bump to `0.4.2.1`.
- Human Rejected: No additional scope beyond the two reviewed inconsistencies was accepted in this pass.
- Human Changed: Kept backward compatibility by supporting both the new compact payload and the older scalar fields during load, and updated user-facing version strings to stay in sync.
- Validation Method: `dotnet build StarGen.sln`, Godot headless test harness, and targeted review of station-design save/load and mapping call sites.
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-12 - Codex (GPT-5)

- Task Purpose: Align the galaxy generation flow with the other studios, split viewer-return actions, and expose richer object-viewer inspection data before the next release.
- Input Materials Used: User screenshots of galaxy/system/object studios; `AGENTS.md`; `claude.md`; `WelcomeScreen.cs/.tscn`; galaxy-viewer scripts/scenes; `GalaxyInspectorPanel`; `InspectorPanel`; `MainApp` navigation; object/system studio screens; integration/UI tests; version metadata files.
- AI Produced: Rebuilt the galaxy generation screen into the same two-panel studio shell as the system/object studios, moved galaxy parameter ownership out of the galaxy viewer into a read-only active-profile summary, added a dedicated galaxy-viewer main-menu return action, expanded the object viewer inspector with orbit/surface/atmosphere/rings/population/generation snapshot readouts, updated tests, and synced version/provenance metadata to `0.4.2.2`.
- Human Accepted: The studio-shell parity changes, navigation split, richer object-viewer inspection surface, updated integration coverage, and internal version/log sync.
- Human Rejected: A larger Traveller/UWP implementation was not invented beyond the domain data and mappings already present in the repo; no speculative full UWP subsystem was added.
- Human Changed: Kept the existing generator and viewer APIs stable where possible, preserved the galaxy-viewer public signals by routing studio-return through the existing `NewGalaxyRequested` path, and exposed existing generated metadata instead of duplicating the object edit dialog inside the inspector.
- Validation Method: `dotnet build StarGen.sln` and the Godot headless harness (`Total: 1883 | Passed: 1883 | Failed: 0`).
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-12 - Codex (GPT-5)

- Task Purpose: Fix cramped galaxy-studio layout and make resolution/fullscreen settings apply immediately.
- Input Materials Used: User screenshot of the cramped galaxy studio and note that window size never changed despite resolution/fullscreen settings; `AGENTS.md`; `claude.md`; `WindowSettingsService.cs`; `WelcomeScreen.tscn`; `MainMenuScreen.cs`; window-setting integration tests and app version metadata.
- AI Produced: Patched the window-settings service to apply mode and size directly to the active root window in addition to persisted display-server settings, centered windowed resolutions after applying them, added direct `ApplyToWindow(...)` regression tests, and loosened the galaxy studio scene margins/split sizing.
- Human Accepted: The root-window display fix, new window-setting regression tests, the galaxy studio layout adjustment, and bug-fix metadata sync to `0.4.2.3`.
- Human Rejected: No new viewer/editor feature scope beyond the reported layout and display-setting bugs was accepted.
- Human Changed: Kept the fix scoped to the display path and studio scene instead of reshaping unrelated app layouts, and preserved the existing menu/settings UI contract while making the underlying application reliable.
- Validation Method: `dotnet build StarGen.sln` and the Godot headless harness (`Total: 1885 | Passed: 1885 | Failed: 0`).
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-12 - Codex (GPT-5)

- Task Purpose: Bring object creation, Traveller world generation, and object inspection into parity before the next release.
- Input Materials Used: User screenshots of the object studio and parameter editor; `claude.md`; `ObjectGenerationScreen.cs/.tscn`; `ObjectGenerationRequest.cs`; `ObjectViewer.SaveLoad.cs`; `InspectorPanel.cs`; `EditDialog.cs`; generation spec/generator files; Traveller archetype helpers; existing object-viewer and test harness files.
- AI Produced: Reworked the object studio into an explicit spec builder with per-type profile controls, hidden seed/advanced toggles, Traveller planet world-profile generation, explicit asteroid studio spec support, richer planet UWP/world-profile inspector readouts, new Traveller domain helpers/tests, and feature-version metadata sync to `0.4.3.0`.
- Human Accepted: The explicit-spec studio flow, Traveller UWP generation/mapping, object inspector world-profile readouts, new unit/integration tests, and version/provenance sync.
- Human Rejected: Full GURPS/Starfinder-specific rulesets were not introduced; the display was kept system-neutral beyond Traveller UWP support and broadly useful RPG-facing world data.
- Human Changed: Matched creation-mode controls to existing generator spec/override paths instead of inventing a second parameter model, and adapted Traveller worldgen mapping to the repo's actual government/technology enums after a build validation pass.
- Validation Method: `dotnet build StarGen.sln` and the Godot headless harness (`Total: 1893 | Passed: 1893 | Failed: 0`).
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-12 - Codex (GPT-5)

- Task Purpose: Stabilize the app UI layout so screens stop clipping text and hiding actions off-screen.
- Input Materials Used: User report that screens were cropped, buttons were pushed off-screen, and layout quality was the main current UX problem; screenshots of the affected studios; `claude.md`; `MainMenuScreen.tscn`; `SplashScreen.tscn`; `WelcomeScreen.cs/.tscn`; `SystemGenerationScreen.cs/.tscn`; `ObjectGenerationScreen.cs/.tscn`; object-generation enhanced partials; integration tests; version/docs files.
- AI Produced: Added a shared responsive studio-layout helper, converted the launch studios to responsive stacked layouts with summary scrolling, reduced hard-coded chrome and label widths, moved more explanatory text into tooltips, refreshed splash/main-menu layout behavior, added `TestStudioScreenLayoutHelper`, and synced docs/version metadata to `0.4.3.1`.
- Human Accepted: The responsive studio structure, summary scrolling, reduced clipping, shared helper/test coverage, and metadata/provenance sync.
- Human Rejected: No full visual redesign or theme replacement was attempted; the pass stayed focused on accessibility, spacing, and reachability of existing controls.
- Human Changed: Adapted the fix toward structural layout behavior instead of only shrinking fonts, updated scene-path-based tests for the new summary scroll container, and added viewport-free fallback sizing so the helper works in both runtime and headless tests.
- Validation Method: `dotnet build StarGen.sln` and the Godot headless harness (`Total: 1895 | Passed: 1895 | Failed: 0`).
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-12 - Codex (GPT-5)

- Task Purpose: Apply a pre-release correction pass for viewer navigation, Traveller launch behavior, and user-facing generation language before the planned 0.5 release.
- Input Materials Used: User bug report and screenshots covering duplicate back navigation, top-menu/header ordering, Traveller blank-world output, optional feature wording, and misleading life/population labels; `claude.md`; viewer scenes/scripts; object/system/galaxy generation screens; `TravellerWorldGenerator`; integration/unit tests; version metadata files.
- AI Produced: Moved viewer menus below the header, removed duplicate header-level return reliance in the object/system viewers, fixed Traveller all-auto blank-world avoidance and conditional ring defaults, kept optional features as `Auto / Yes / No`, renamed user-facing permissiveness labels to `Life Potential` and `Settlement Density`, added regression coverage, and synced version metadata to `0.4.3.2`.
- Human Accepted: The navigation cleanup, Traveller generation fixes, wording changes, new and updated regression tests, and version sync.
- Human Rejected: A full multi-system RPG rules expansion such as GURPS or Starfinder support was not attempted; the scope stayed on Traveller correctness and release-blocking UX issues.
- Human Changed: Kept the fix bounded to release blockers, reused the existing deterministic generation/settings model instead of inventing a new one, and adjusted the new tests after verification to match the repo's actual control behavior.
- Validation Method: `dotnet build StarGen.sln` and the Godot headless harness (`Total: 1898 | Passed: 1898 | Failed: 0`).
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-13 - Codex (GPT-5)

- Task Purpose: Tighten the pre-release UI pass around remaining studio/viewer navigation issues and footer visibility before the 0.5 review.
- Input Materials Used: User follow-up report and screenshots covering galaxy-viewer menu/header ordering, duplicate galaxy-studio affordances, object-viewer return behavior, hidden studio footer actions, seed-visibility preference expectations, windowed-resolution uncertainty in editor, and the request to surface the station studio in the main menu; `claude.md`; `MainApp*`; galaxy/system/object viewer scenes and scripts; studio scenes/scripts; `WindowSettingsService`; integration tests; version metadata files.
- AI Produced: Removed the remaining launch-summary clutter from the galaxy/system/object studios, tightened studio margins so the action footer stays reachable at smaller sizes, kept the galaxy viewer on a single `New Galaxy...` path, made hidden-seed behavior flow from the Options preference across studios, added the Station Studio entry point, aligned system-viewer permissiveness controls with the named scale bands, and synced patch metadata to `0.4.3.3`.
- Human Accepted: The studio/footer cleanup, station-studio menu entry, seed-visibility preference flow, system-viewer wording/control alignment, and patch-version sync.
- Human Rejected: Full acceptance of the criterion `Traveller-generated planets still get sensible non-UWP features when those features follow from world conditions` was explicitly deferred by the user and not treated as a release gate in this pass.
- Human Changed: Kept the windowed-resolution code path intact and documented a post-release build verification note instead of expanding the bugfix into engine-level speculation, and reduced studio chrome rather than redesigning the full application theme.
- Validation Method: Validation pending after that pass: `dotnet build StarGen.sln`, Godot headless test harness, and manual review of the corrected screens; post-release note retained to verify windowed resolution behavior in exported builds and hotfix if needed.
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-13 - Codex (GPT-5)

- Task Purpose: Make the provenance log readable, repair menu-script regressions introduced during recent scene edits, and stabilize wrapped UI text with explicit minimum widths.
- Input Materials Used: User report that the provenance table was hard to read, failing `TestSystemViewer::test_traveller_controls_exist`, current scene/script diffs, `MainMenuScreen.cs/.tscn`, `SplashScreen.cs/.tscn`, `StationStudioScreen.cs/.tscn`, studio/viewer scenes, `WindowSettingsService.cs`, `README.md`, `Docs/ProjectStructure.md`, `VERSION.md`, and the integration tests.
- AI Produced: Reformatted this log into sectioned entries, added explicit wrap-related minimum sizes across static scenes and runtime-created wrapped labels, enforced/documented a `640 x 480` minimum window floor, fixed object-viewer main-menu return routing, and updated the stale system-viewer Traveller-controls regression to match the current slider UI.
- Human Accepted: Pending user review of this readability/layout pass.
- Human Rejected: No new feature scope beyond readability, regression repair, and wrap/min-size stabilization was added.
- Human Changed: Main menu changes included the studio card layout, station-studio entry, and the seed-visibility options surface. Splash-screen changes included the scrollable centered layout and revised status presentation. Station-studio changes included the placeholder scene/copy/navigation now used as the in-progress entry point.
- Validation Method: `dotnet build StarGen.sln`, Godot headless harness, and manual spot-checking of wrapped-label surfaces after the scripted fixes.
- Final Approver: Approved by Christopher B. Del Gesso.

### 2026-03-13 - Codex (GPT-5)

- Task Purpose: Repair the broken main-menu buttons after the recent user-driven scene rewrite changed the menu layout without matching script updates.
- Input Materials Used: User report that none of the main-menu buttons worked, the current `MainMenuScreen.cs/.tscn`, `MainApp.cs`, integration-test harness files, and repo workflow/versioning instructions.
- AI Produced: Updated `MainMenuScreen.cs` to match the current scene paths, kept utility actions working through fallback dialogs when embedded content panels are absent, switched button wiring to Godot signal connections so the tests exercise the same path as runtime, added `TestMainMenuScreen`, and registered the new regression in the headless integration suite.
- Human Accepted: Pending user review of the repaired main-menu behavior and the added regression coverage.
- Human Rejected: No menu redesign or additional feature scope was added beyond restoring the broken actions and preserving the current user-authored layout.
- Human Changed: Left the user’s rewritten main-menu scene structure intact and adapted the script to it instead of reverting the scene back toward the older embedded-content layout.
- Validation Method: `dotnet build StarGen.sln` and the Godot headless harness (`Total: 1901 | Passed: 1901 | Failed: 0`).
- Final Approver: Approved by Christopher B. Del Gesso.
