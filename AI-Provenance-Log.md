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

### 2026-04-20 - Codex (GPT-5)

- Task Purpose: Bring the main menu/viewer options dialogs, checkbox styling, System Viewer inspector, and studio override surfaces up to the same standard as the Galaxy Viewer and studios, including fixing the explicit `Apply`/`Close` dialog behavior and replacing the System Viewer inspector with a focus-oriented orbit preview.
- Input Materials Used: User reports and screenshots about Galaxy Viewer options not persisting, unreadable unchecked checkboxes, mismatched System/Object viewer menu and camera-control standards, the duplicate population toggle in System Studio, the misplaced `Advanced Controls` toggle in Object Studio, and the desired System Viewer inspector behavior; `AGENTS.md`; repo `claude.md`; `MainMenuScreen.cs`; `MainMenuScreen.tscn`; `GalaxyViewer.LocalSpace.cs`; `GalaxyViewer.Setup.cs`; `GalaxyViewer.cs`; `GalaxyViewerCSharp.tscn`; `SystemViewer.cs`; `SystemViewer.Setup.cs`; `SystemViewer.Interaction.cs`; `SystemViewer.Menu.cs`; `SystemViewer.Parameters.cs`; `SystemViewer.tscn`; `SystemInspectorPanel.cs`; `ObjectViewer.cs`; `ObjectViewer.Display.cs`; `ObjectViewer.Menu.cs`; `ObjectViewer.tscn`; `ObjectGenerationScreen.EnhancedUi.cs`; `SystemGenerationScreen.cs`; `SystemGenerationScreen.tscn`; `GalaxyGenerationScreen.tscn`; `DarkTheme.tres`; and the app and studio integration test suites.
- AI Produced: Switched the remaining viewer and options surfaces to explicit `CheckBox` controls with white-bordered checkbox glyphs; fixed Main Menu, Galaxy Viewer, System Viewer, and Object Viewer options dialogs so `Apply` persists the saved window and studio UI preferences and both `Close` button and titlebar `X` hide the dialog; added compact bottom-right collapsible camera panels to the viewers and test coverage around their default collapsed state; removed the duplicate System Studio population override; kept Object Studio override checkboxes and `Advanced Controls` in `Generation Overrides`; replaced the System Viewer inspector with a compact overview and selection panel that shows clickable star/orbit preview entries, numbered asteroid belts, compact selected-body summaries, UWP-on-selection only, and open-in-object-viewer handoff; updated selection status text to use body and numbered belt names; and added viewer/app regressions for the options dialogs, checkbox types, and compact camera panels.
- Human Accepted: Pending Christopher B. Del Gesso review of the final live viewer/studio presentation and the System Viewer inspector interaction flow.
- Human Rejected: The user rejected the earlier behavior where options changes did not survive an `Apply` round-trip, unchecked boxes were effectively invisible, the System Studio still exposed a duplicate population toggle, and the System Viewer inspector was carrying stale or low-value information such as the synthetic system name and orbit-host dump.
- Human Changed: The user clarified that Galaxy Studio generation-override checkboxes should match the System Studio presentation, that Object Studio override checkboxes should match that same side/presentation, that `Advanced Controls` belongs in `Generation Overrides`, and that the System Viewer should move to a compact orbit preview with clickable focus entries while leaving detailed object data to the Object Viewer.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (reported `Total: 1835 | Passed: 1835 | Failed: 0` before the known Godot mono shutdown leak noise raised a non-zero process exit).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-20 - Codex (GPT-5)

- Task Purpose: Fix Galaxy Viewer options so the seed-toggle wording matches the actual preference scope and the viewer options window can be closed from the titlebar `X`, while keeping the preference wording consistent in the main menu and preserving a green deterministic test suite.
- Input Materials Used: User report with screenshot from Galaxy Viewer options; `AGENTS.md`; repo `claude.md`; `src/app/galaxy_viewer/GalaxyViewer.LocalSpace.cs`; `src/app/galaxy_viewer/GalaxyViewerCSharp.tscn`; `src/app/MainMenuScreen.cs`; `src/app/MainMenuScreen.tscn`; `Tests/Framework/DotNetNativeTestSuite.App.cs`; `Tests/Integration/TestAppAudio.cs`; and version/provenance metadata files.
- AI Produced: Wired `CloseRequested` on the Galaxy Viewer options window so the titlebar close control hides the dialog; hardened the viewer dialog-open helper so it works in both live runtime and off-tree test contexts; renamed the seed toggle to `Show all studio seeds` in the Galaxy Viewer and Main Menu options surfaces; updated the options status copy to the same wording; added Galaxy Viewer regression coverage for the renamed label, status text, and options-dialog open/close behavior; fixed a persisted-preference isolation issue in the shared intro-audio integration test; and synced internal version metadata to `0.8.18.3`.
- Human Accepted: Pending Christopher B. Del Gesso review of the final Galaxy Viewer options behavior and wording.
- Human Rejected: No broader Galaxy Viewer tooling or inspector changes were made in this patch; the work stayed focused on the options dialog behavior and wording mismatch.
- Human Changed: The user clarified that the `Show studio seed controls` label was misleading because the expectation was that all relevant seeds would be shown, and specifically reported that the Galaxy Viewer options window could not be closed.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1832 | Passed: 1832 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-20 - Codex (GPT-5)

- Task Purpose: Fix the System Generation Studio help popup so it stays within the visible window, bring the System Studio science panel up to the same audited standard as Galaxy Studio, and make checkboxes compact and clearly visible across the UI.
- Input Materials Used: User report about the off-screen System Studio help dialog and hard-to-see checkboxes; `AGENTS.md`; repo `claude.md`; `Docs/Roadmap.md`; `SystemGenerationScreen.tscn`; `SystemGenerationScreen.cs`; `SystemGenerationScreen.Help.cs`; `SystemGenerationScreen.Stellar.cs`; `SystemGenerationScreen.Planetary.cs`; `SystemGenerationScreen.Life.cs`; `GalaxyGenerationScreen.tscn`; `GalaxyGenerationScreen.Science.cs`; `ObjectGenerationScreen.Help.cs`; `HelpDialogLayoutHelper.cs`; `DarkTheme.tres`; `GenerationParameterCatalog.cs`; and `Tests/Integration/TestStudioScienceUi.cs`.
- AI Produced: Tightened the shared help-dialog size clamp and kept modal opening on the explicit size/position path so Galaxy, System, and Object Studio help windows stay within the viewport while still working in the headless scene harness; retitled the System Studio left column to `Scientific Assumptions`; added section-header `(i)` source buttons and source-tooltips for System Controls, Stellar Priors, Planetary Priors, and Life Models; simplified checkbox and check-button theme chrome so unchecked boxes remain legible and compact instead of rendering as large filled toggles; added regressions for the compact checkbox styling and the upgraded System Studio science/help surface; and synced patch metadata to `0.8.18.2`.
- Human Accepted: Pending review of the final System Studio visual fit against the Galaxy Studio standard and the exact compact checkbox look in the live UI.
- Human Rejected: The user explicitly rejected the existing oversized help popup behavior and the low-visibility checkbox presentation.
- Human Changed: The user specified that the System Studio should be brought up to the Galaxy Studio standard and that checkboxes in general should be small white-bordered boxes wherever they appear.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1832 | Passed: 1832 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-20 - Codex (GPT-5)

- Task Purpose: Fix Galaxy Viewer local-space caching so moving the view outside the last built area does not make the cache behave as though it was deleted, and so subsequent `Build Local Space` runs append newly covered systems instead of replacing the existing temporary cache.
- Input Materials Used: User report about local-space cache behavior after movement; `AGENTS.md`; repo `Claude.md`; `GalaxyLocalSpaceCache`; `GalaxyViewer.LocalSpace`; `GalaxyViewer.Setup`; `GalaxyViewer.Menu`; `StarViewCamera`; `DotNetNativeTestSuite.App.cs`; and version/project-structure metadata files.
- AI Produced: Reworked `GalaxyLocalSpaceCache` into an aggregate cache that tracks multiple covered local-space areas and appends only uncached systems into one merged `JumpLaneRegion`; updated `GalaxyViewer.LocalSpace` so repeated builds append coverage and preserve the existing cache; kept jump-route availability tied to whether the current position is covered while leaving the cache itself intact across movement; added a viewer regression proving the cache survives movement and that a second build appends systems and restores route availability; and synced metadata to `0.8.18.1`.
- Human Accepted: Pending user review of the new cache-persistence and append behavior in the Galaxy Viewer.
- Human Rejected: No attempt was made to auto-build local space on movement; the workflow remains explicit, with movement only making a new local build necessary when the user wants coverage in the new area.
- Human Changed: The user clarified that moving the view should not eliminate the temporary cache and that rebuilding local space from a new position should append new systems into the existing cache rather than replacing it.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-20 - Codex (GPT-5)

- Task Purpose: Update the Galaxy Viewer so its overview reports live quadrant, sector, and local coordinates; replace the old top-bar menu split with `Tools` and `Options`; and add an explicit `Build Local Space` tool that caches nearby-system summaries for jump-route and later local-space tools.
- Input Materials Used: User requirements for the Galaxy Viewer overview/menu/local-space tool flow; `AGENTS.md`; repo `claude.md`; `GalaxyViewer`; `GalaxyViewer.Menu`; `GalaxyViewer.Selection`; `GalaxyViewer.Setup`; `GalaxyViewer.JumpRoutes`; `GalaxyInspectorPanel`; `GalaxyInspectorSelectionFormatter`; `GalaxyViewerCSharp.tscn`; `SubSectorNeighborhood`; `JumpLaneSystem`; `WindowSettingsService`; `StudioUiPreferencesService`; and the native test suites.
- AI Produced: Expanded the inspector formatter to include sector coordinates and hierarchical coordinate formatting; updated the visible Galaxy Viewer overview to show `Quadrant`, `Sector`, and `Local`; added selected-system hierarchical coordinates to `System Preview`; replaced the `View`/`Window` top-bar split with `Tools` and `Options`; added a scene-owned options dialog; added a scene-owned `Build Local Space` dialog with extent controls, exact nearby star counts, and cache-build status; generalized subsector-neighborhood generation to accept custom extents; added a temporary local-space cache type; switched jump-route calculation to consume the explicit local-space cache instead of silently rebuilding neighborhood data; extended `JumpLaneSystem` with habitability, resource, government, and trade-code summaries; added viewer-scene regression coverage for the new menu/dialog/cache flow; and synced metadata to `0.8.18.0`.
- Human Accepted: Pending review of the exact Galaxy Viewer menu wording, the local-space dialog sizing and extent UX, and the choice to use the current local view position as the build center while keeping the cache explicit instead of auto-refreshing it.
- Human Rejected: The previous inspector-only cleanup passes were rejected because the visible Galaxy Viewer still did not expose the requested quadrant/sector/local breakdown, still mixed menu responsibilities, and still lacked an explicit local-space build tool.
- Human Changed: The user clarified that the overview should use `quadrant, sector, local`; that selected stars should expose hierarchical coordinates in `System Preview`; that `Options` should move to the menu bar; that `View` should become `Tools`; that `Window` should be removed; and that lazy local-space generation must become an explicit cache-building step before route-oriented tools run.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1830 | Passed: 1830 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-20 - Codex (GPT-5)

- Task Purpose: Finish the Galaxy Viewer inspector cleanup so the visible panel behaves as a live overview tied to the active location, remove the unused top-level seed and compass controls, and keep system-only data confined to the preview section.
- Input Materials Used: User bug report with screenshot showing the still-visible top seed field, `Show Compass` checkbox, stale `Selection` block, and non-updating overview data; `AGENTS.md`; repo `claude.md`; `GalaxyViewerCSharp.tscn`; `GalaxyViewer`; `GalaxyViewer.Selection`; `GalaxyViewer.Setup`; `GalaxyViewer.Menu`; `GalaxyInspectorPanel`; `GalaxyInspectorSelectionFormatter`; `StudioUiPreferencesService`; and the existing viewer test suites.
- AI Produced: Reworked the inspector formatter to derive quadrant, local coordinates, azimuth, inclination, and distance from a single live overview position; replaced the visible `Selection` block behavior with a galaxy-level `Overview` display that shows galaxy type, optional seed, live quadrant/local/density, and polar coordinates; moved all system-specific details to `System Preview`; removed the top-level seed input, `Show Compass` checkbox, compass viewport, and compass menu handling from the live C# viewer scene and controller; added movement-driven inspector refresh logic; updated the viewer regression tests to assert the removed controls stay gone and that the visible block is labeled `Overview`; and synced patch metadata to `0.8.17.4`.
- Human Accepted: Pending review of the final live inspector behavior in the Galaxy Viewer.
- Human Rejected: The user rejected the earlier hide-only inspector patch because the visible scene still showed the top-level seed input, `Show Compass` control, stale `Selection` heading, and non-updating location fields.
- Human Changed: The user clarified that the overview should use the current location unless a system is selected, that `Type` should always mean galaxy type, that seed should only appear inside the overview when studio seed controls are enabled, and that all system details belong only in `System Preview`.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1829 | Passed: 1829 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-20 - Codex (GPT-5)

- Task Purpose: Trim the Galaxy View inspector down to the requested selection fields and make its location readout reflect the active local view context instead of dumping raw world-space coordinates and oversized preview detail.
- Input Materials Used: User request specifying the desired Galaxy View inspector fields; `AGENTS.md`; repo `claude.md`; `GalaxyInspectorPanel`; `GalaxyViewer.Selection`; `GalaxyViewer.Setup`; `StarViewCamera`; `GalaxyCoordinates`; `HomePosition`; current version metadata; and the existing headless test harness.
- AI Produced: Added `GalaxyInspectorSelectionFormatter` to derive current-quadrant context, subsector-local `0..9` grid coordinates, home-relative polar angle, inclination, and distance-from-core formatting; rewired `GalaxyInspectorPanel` so selected star systems now show only type, seed, quadrant, local XYZ, azimuth, inclination, and distance from core; reduced system preview output to a compact stars/bodies/settlement summary; updated `GalaxyViewer.Selection` to pass the active local view position into the inspector instead of only the selected star position; added unit tests covering quadrant context, subsector-grid mapping, polar readout, and distance formatting; and synced version metadata to `0.8.17.1`.
- Human Accepted: Pending review of the exact compact preview shape and the choice to measure `Distance from Core` as planar radial distance while reporting inclination separately.
- Human Rejected: The user explicitly rejected the previous verbose inspector output, including the raw parsec XYZ dump and other unnecessary details.
- Human Changed: The user required that location be framed from the current local view context without mentioning the camera and that local XYZ be represented as subsector-grid coordinates from `0` to `9`.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1828 | Passed: 1828 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-20 - Codex (GPT-5)

- Task Purpose: Correct the Galaxy View inspector patch after confirming the visible issue was the still-present legacy `Active Profile` and `Overview` sections, which kept the trimmed selection details buried below unrelated profile content.
- Input Materials Used: User follow-up with screenshot showing the unchanged visible Galaxy View inspector; `GalaxyViewerCSharp.tscn`; `GalaxyInspectorPanel`; existing viewer accessors; headless test harness; and current version metadata.
- AI Produced: Hid the legacy config and overview sections directly in `GalaxyInspectorPanel` initialization, exposed small visibility accessors for regression testing, added a viewer-level test that instantiates `GalaxyViewerCSharp.tscn` and asserts those sections stay hidden, and bumped patch metadata to `0.8.17.2`.
- Human Accepted: Pending review of the final visible Galaxy inspector layout now that the legacy sections no longer occupy the top of the panel.
- Human Rejected: The user explicitly rejected a patch that only changed the buried selection subsection while leaving the visible top of the Galaxy inspector filled with profile and overview data.
- Human Changed: The user's screenshot clarified that the real requirement was to simplify the actual visible Galaxy View inspector, not just the deeper star-selection block.
- Validation Method: Pending `dotnet build StarGen.sln` and `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-20 - Codex (GPT-5)

- Task Purpose: Finish simplifying the Galaxy View inspector by removing the remaining jump-route tools after the user clarified that the inspector should contain only the requested informational fields.
- Input Materials Used: User clarification excluding jump-route tools from the inspector window; current `GalaxyInspectorPanel`; current viewer-level inspector regression; and version/provenance metadata.
- AI Produced: Hid the `ColonizationSection` in `GalaxyInspectorPanel`, added a visibility accessor for regression coverage, extended the viewer-level inspector test to assert the jump-route tools stay hidden, and bumped patch metadata to `0.8.17.3`.
- Human Accepted: Pending review of the final minimal Galaxy inspector surface.
- Human Rejected: The user explicitly rejected leaving jump-route tools in the inspector window.
- Human Changed: The user clarified that the target surface is informational only and should not include the route-control tools.
- Validation Method: Pending `dotnet build StarGen.sln` and `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Finish wiring the fixed non-Space-Opera RPG compatibility profiles so `Cepheus`, `Starfinder`, and `Starforged` materially change system and population generation instead of collapsing into near-identical variants of the same small bias set.
- Input Materials Used: User request to make sure the rest of the systems were fully wired; `AGENTS.md`; `claude.md`; existing `RpgCompatibilityProfile`; `GenerationUseCaseSettings`; `SystemPlanetGenerator`; `PopulationProbability`; `PopulationGenerator`; `ColonyGenerator`; `ColonyType`; existing compatibility tests in `TestGenerationParameters`, `TestPopulationProbability`, `TestColonyGenerator`, and `TestSystemPlanetGenerator`; and current version/provenance metadata.
- AI Produced: Added fixed compatibility-profile fields for harsh-world colony tolerance, colony-type weighting, and mainworld hydrosphere leaning; wired those profile defaults through `GenerationUseCaseSettings.GetCompatibilityProfile()`; extended colony probability so frontier-friendly profiles can materially tolerate harsh colony targets instead of only applying a late cosmetic multiplier; threaded compatibility profiles into colony generation and added ruleset-specific colony-type weighting for civil-settlement, technical outpost, and frontier patterns; replaced the one-off Starfinder hydrosphere special case with a profile-driven mainworld hydrosphere bias; added population, colony-generator, and system-generator regressions proving the non-Space-Opera profiles now diverge in harsh-world colony tolerance, colony-type distribution, and system-level orbit-fill behavior; and synced version metadata to `0.8.17.0`.
- Human Accepted: Pending review of the exact fixed profile signatures for `Cepheus`, `Starfinder`, and `Starforged`, especially the balance between Starforged frontier tolerance and its lower overall settlement density.
- Human Rejected: No export, standardized inspector-output, or additional RPG readout work was added because the user had already kept output cleanup and export behavior out of scope for version `0.9d`.
- Human Changed: The user narrowed the goal to “fully wired” profile behavior, which kept this pass focused on actual generation outcomes rather than expanding the Space Opera UI again in the same checkpoint.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1825 | Passed: 1825 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Expose the concrete `Space Opera` compatibility levers in Galaxy Studio and System Studio so the RPG override can be tuned by users instead of remaining a fixed clean-room preset, while keeping those controls hidden outside the active `Space Opera` profile.
- Input Materials Used: User request asking whether the various systems were wired and then requesting that `Space Opera` expose the parameters touched by the TTRPG override; `AGENTS.md`; `claude.md`; existing `RpgCompatibilityProfile`; `GenerationUseCaseSettings`; `GalaxyGenerationScreen` and `SystemGenerationScreen` scenes/controllers; `SystemPlanetGenerator`; `PopulationProbability`; current compatibility-profile tests; and current version/docs metadata.
- AI Produced: Added persisted compatibility override fields for mainworld policy, temperate-slot fill pressure, harsh-slot fill pressure, terrestrial or mainworld-class weighting, native-life weighting, and settlement weighting; updated Galaxy Studio and System Studio scenes to expose those controls only when `Space Opera` is selected; increased slider precision and fixed the Galaxy settlement slider range so the UI can actually represent the stored values; wired the new settings through config/spec serialization and the shared compatibility-profile resolution path; corrected ruleset-default application so active profiles seed the real profile defaults rather than neutral placeholders; expanded integration tests to prove the studios write the custom override values through; and updated the system-generation contract test fixture so custom `Space Opera` pressures now measurably change slot-fill outcomes instead of saturating on a forced mainworld case.
- Human Accepted: Pending review of the specific `Space Opera` default multipliers and the decision to expose only the currently active compatibility levers rather than inventing new RPG-facing knobs.
- Human Rejected: No standardized inspector-output cleanup or export work was added, because the user explicitly kept output cleanup out of scope for `0.9d`.
- Human Changed: The user required that `Space Opera` remain the only profile with exposed tuning at this stage and that those settings reflect the actual generation levers already touched by the RPG override path.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --build-solutions --quit`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1820 | Passed: 1820 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Implement the first clean-room RPG compatibility scaffolding in the mainline generator so `Space Opera`, `Cepheus`, `Starfinder`, and `Starforged` can materially bias generation without expanding standardized inspector output or export behavior in `0.9d`.
- Input Materials Used: User request to start implementing the code scaffolding for the systems and how they override generation while leaving output/export out of scope; `AGENTS.md`; `claude.md`; current `GenerationUseCaseSettings`, studio controller files, `SystemPlanetGenerator`, population pipeline files, existing Traveller/UWP support, and the reviewed RPG licensing and source notes already added under `Sources/Texts/`.
- AI Produced: Added `RpgCompatibilityProfile` as the shared clean-room compatibility-profile resolver; expanded `RulesetModeType` to include `Cepheus`, `Starfinder`, and `Starforged`; added shared ruleset presentation labels and default-application helpers; updated Galaxy Studio, System Studio, and Object Studio to populate and apply the new ruleset options consistently; generalized guarded UWP-like behavior away from Traveller-only checks; biased system generation through the compatibility profile by changing mainworld targeting, temperate or harsh slot fill pressure, terrestrial-world weighting, and mainworld candidate shaping; threaded compatibility multipliers into population forcing and native-life/colony probability pressure; added integration and unit tests for ruleset defaults, studio option surfaces, system fill pressure, and population shifts; and synced version/docs metadata to `0.8.15.0`.
- Human Accepted: Pending review of the clean-room compatibility-profile definitions, the chosen generation-pressure differences between profiles, and the decision to leave inspector output and export formatting out of scope for this checkpoint.
- Human Rejected: The user explicitly deferred standardized inspector output cleanup and export work for version `0.9d`, so this pass does not add ruleset-specific output formatting or export adapters.
- Human Changed: The user constrained the scope to generation scaffolding only, which kept the implementation focused on settings, generation pressure, and testable domain behavior rather than output/readout redesign.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1818 | Passed: 1818 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Align System Studio and Object Studio with the same science-surface standards already enforced in Galaxy Studio, while preserving the studio scope split so System Studio carries sourced aggregate assumptions and Object Studio stays limited to direct single-object controls.
- Input Materials Used: User request to make System Studio and especially Object Studio follow the same standards; `AGENTS.md`; `claude.md`; current `SystemGenerationScreen` and `ObjectGenerationScreen` scene/controller files; `GenerationUseCaseSettings`; `GenerationParameterCatalog`; `LifeScienceReferenceCatalog`; the new scientific parameter audit; `ObjectGenerationRequest`; `PlanetSpec`; `TestStudioScienceUi`; and current version/docs metadata.
- AI Produced: Added `SystemGenerationScreen.Life.cs` so System Studio now exposes the audited life-model stack (`Life Framework`, `Abiogenesis Model`, `Complex Life Model`, `Civilization Model`, `Environmental Window Weight`) and writes those settings into `SolarSystemSpec`; updated System Studio summaries, help, and parameter metadata to match the audited Galaxy Studio surface; removed the old coarse life slider from System Studio; added `ObjectGenerationParameterCatalog` and rewired Object Studio tooltips so direct planet, star, asteroid, comet, Traveller, and advanced controls explain what they actually change; removed leftover aggregate life/population sliders from Object Studio; explicitly kept Object Studio scoped to direct single-object controls only; fixed the runtime `Show UWP Code` row label in Object Studio; expanded integration coverage to verify the renamed `Generation Overrides` panel, `Show UWP Code` wording, absence of unsupported aggregate rows, direct tooltip presence, and persistence of direct planet controls into `PlanetSpec`; and synced docs/version metadata to `0.8.12.0`.
- Human Accepted: Pending review of the System Studio life-surface parity, the narrower Object Studio contract, and the final wording/tooltips before merge or release.
- Human Rejected: The user rejected leaving System Studio or Object Studio behind the audited Galaxy Studio standard and implicitly rejected continuing to surface aggregate life/population sliders in Object Studio once the direct-object scope was made explicit.
- Human Changed: The user clarified that Object Studio is expected to need the most work because it assumes the user is authoring a specific object, which pushed the implementation toward a stricter direct-control contract rather than mirroring the aggregate science surfaces there.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1812 | Passed: 1812 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Audit every shipped scientific parameter family so each exposed galaxy, stellar, planetary, and life control is backed by relevant reviewed sources, materially changes generation in the claimed direction, and has tests that enforce that contract.
- Input Materials Used: User-approved scientific-parameter audit plan; `AGENTS.md`; `claude.md`; current galaxy, stellar, planetary, and life science reference catalogs; `GenerationParameterCatalog`; `GenerationUseCaseSettings`; `GalaxyRealismProfileBuilder`; `GalaxyScientificFieldEvaluator`; `StellarMassSampler`; `StellarConfigGenerator`; `SystemPlanetGenerator`; `LifePotentialModeling`; `BiologySupportEvaluator`; `PopulationGenerator`; current unit/integration/population tests; `Sources/AnnotatedBibliography.md`; and additional galaxy-morphology research notes for Hart 2017, Lingard 2021, and Rodriguez & Padilla 2013.
- AI Produced: Added `Docs/ScientificParameterAudit.md` as the tracked audit matrix for shipped science-backed controls; added missing galaxy-morphology source notes and source-catalog mappings for arm count, pitch, arm-mechanism framing, and ellipticity; tightened science-facing wording where the implementation is a bounded StarGen tuning rather than a direct empirical law; narrowed the planetary rogue-world wording to match the actual disturbed-outcome bias currently implemented; removed the civilization-model leak from the sentience multiplier; split summary population outcomes so sentient-but-non-technological worlds are represented distinctly from technological civilizations; strengthened source-resolution tests to fail on empty source lists; added new galaxy, stellar, planetary, and life materiality tests; and synced version/project-structure metadata to `0.8.11.0`.
- Human Accepted: Pending review of the audit dispositions, the narrower wording on partially bounded controls, and the scientific interpretation of the added galaxy-morphology citations before merge or release.
- Human Rejected: The user explicitly rejected leaving parameters half-valid, meaning any control with weak source linkage or inert behavior had to be fixed, narrowed, or downgraded instead of being left in the science-backed UI on trust.
- Human Changed: The user raised the standard from feature implementation to an explicit audit contract: each science setting now has to be relevantly cited, materially active, and covered by tests that match the expected directional outcome.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1812 | Passed: 1812 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Correct the Galaxy Studio layout so scientific assumptions stay in the left column and the center column is Generation Overrides only, then expand the life controls from one coarse selector into a source-aligned life-model stack with explicit composite assumptions.
- Input Materials Used: User-approved life-model and layout-correction plan; follow-up user correction replacing `Earth History` with `Earth-Anchored Composite` and requiring explicit composite assumptions in help text; `AGENTS.md`; `claude.md`; current `GalaxyGenerationScreen` scene/controller files; `GenerationUseCaseSettings`; `LifePotentialModeling`; `BiologySupportEvaluator`; `PopulationGenerator`; `ConceptDependencyChainGenerator`; `GenerationParameterCatalog`; `LifeScienceReferenceCatalog`; integration and unit tests; version/docs metadata; and the reviewed life source notes already in `Sources/Texts/`.
- AI Produced: Renamed the Galaxy Studio center column to `Generation Overrides` and kept scientific priors grouped on the left; replaced the single life selector with `Life Framework`, `Abiogenesis Model`, `Complex Life Model`, `Civilization Model`, and `Environmental Window Weight`; added new generation settings enums and dictionary serialization with backward compatibility from legacy `life_potential_model`; refactored life tuning into separate biosphere, abiogenesis, complex-life, sentience, civilization, and window-weight stages; moved oxygen or technosphere bottlenecks to the civilization stage; rewrote the life help catalog around source-backed per-stage models plus the documented `Earth-Anchored Composite` synthesis preset; updated Galaxy Studio summaries/tooltips/help wiring; expanded life-focused integration and unit tests; and synced version, README, project structure, release-note copy, and provenance metadata to `0.8.10.0`.
- Human Accepted: Pending review of the Galaxy Studio layout correction, the `Earth-Anchored Composite` wording, and the source interpretation behind the new life-model split before merge or release.
- Human Rejected: The user explicitly rejected vague `Earth History` wording and rejected any unsupported `Baseline` option; the user also rejected leaving life controls bundled into one coarse selector or leaving them in the overrides column.
- Human Changed: The user clarified that `Earth-Anchored Composite` is acceptable only if the help text explicitly states what assumptions the composite makes and which reviewed papers inform those assumptions.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Fix the intro skip and intro-audio regressions, add a persisted skip-intro option plus Patreon credit, rename and reposition the requested Galaxy Studio controls, and replace the Galaxy Studio `Life Potential` slider with research-backed life-model selection that materially changes biosphere and civilization generation.
- Input Materials Used: User request covering splash click-skip behavior, skip-intro settings, intro music playback, Leo Patreon credit, Galaxy Studio wording/layout changes, and life-potential research; `AGENTS.md`; `claude.md`; current `MainApp`, `SplashScreen`, `MainMenuScreen`, `GalaxyGenerationScreen`, `GenerationUseCaseSettings`, `BiologySupportEvaluator`, `PopulationProbability`, `PopulationLikelihood`, `PopulationGenerator`, `GenerationParameterCatalog`, test harness files, and primary-source research on Lineweaver & Davis 2002, Spiegel & Turner 2012, Forgan & Rice 2010, Mills et al. 2024, and Balbi & Frank 2023.
- AI Produced: Fixed splash input handling to use `_Input` and corrected the splash-to-audio-controller node path so intro clicks and key presses bypass the intro and the shared intro music cue resolves again; added persisted `SkipIntro` settings support in `StudioUiPreferencesService`, `MainApp`, and `MainMenuScreen`; added Leo to the main-menu credits; renamed the Galaxy Studio center column to `Generator Overrides`, changed the user-facing RPG ruleset label to `Space Opera`, renamed the readout toggle to `Show UWP Code`, moved `Life Potential` into the left column, added `LifeScienceReferenceCatalog` and `LifePotentialModeling`, threaded life-model selection into biology-support, complex-life, and sentience gating, added reviewed source-note files plus bibliography updates for the life-model papers, expanded Galaxy Studio integration coverage for the new life UI, added a skip-intro integration regression, added life-model unit coverage, and synced version/project-structure metadata to `0.8.9.0`.
- Human Accepted: Pending review of the intro behavior, the new Galaxy Studio life-model UX, and the scientific interpretation of the selected life-potential model families before merge or release.
- Human Rejected: The user explicitly rejected leaving `Life Potential` as a simple slider and required model-backed selection instead, and explicitly wanted the intro to skip on click as well as via a persisted settings toggle.
- Human Changed: The user kept the broader `Space Opera` behavior definition for later and asked that the current pass stop at wording/layout changes there, while pushing the life-potential section toward sourced model selection grounded in science rather than another freeform scalar.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --build-solutions --quit`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Remove legacy StarGen planetary-prior notes and the legacy mass-radius option from the upstream planetary studios, make Chen-Kipping the default active pipeline model, add a second research-backed mass-radius model worth keeping, and tighten the scientific support behind the remaining planetary-prior options.
- Input Materials Used: User request to remove the legacy planetary-prior path, verify whether another mass-radius model besides Chen-Kipping was worth adding, add any worthwhile research into the repository, and clarify what model families support Gas Loss, Giant Growth, Metal Link, Rogue Worlds, Moon Bias, and Outer Debris; `AGENTS.md`; `claude.md`; current planetary profile, system planet generator, planet physical generator, planetary science catalog, Galaxy Studio/System Studio planetary controls, existing planetary source notes, and primary-source web research for Chen and Kipping 2017, Otegi et al. 2020, Owen and Wu 2017, Ginzburg et al. 2018, Mordasini et al. 2007, Lambrechts and Johansen 2012, and Mróz et al. 2020.
- AI Produced: Removed the surfaced `Legacy` mass-radius option and made `ChenKipping` the default supported model in `PlanetaryGenerationProfile`; added `PlanetMassRadiusTable` so Chen-Kipping and Otegi now drive real radius and density resolution in the physical generation pipeline; updated system-level formation trace wiring to pass the selected cited model into planet generation; added reviewed source-note files for the new planetary-prior papers; rewrote planetary help/tooltips and source listings so the remaining upstream options map to explicit literature-backed model families or observed trends rather than legacy StarGen notes; updated compatibility handling so older serialized `Legacy` values resolve cleanly to Chen-Kipping; added regressions for compatibility, Otegi branching, and giant-world fallback; and synced docs/version metadata to `0.8.8.0`.
- Human Accepted: Pending review of the chosen second mass-radius model, the updated help wording, and the tighter interpretation of the remaining planetary-prior options before merge or release.
- Human Rejected: The user explicitly rejected keeping legacy StarGen planetary-prior notes and the legacy mass-radius option in the upstream studios, and rejected leaving the other planetary-prior selectors as unsupported comparative language without a clear scientific model or observed trend behind them.
- Human Changed: The user required Chen-Kipping to be the baseline pipeline model, asked whether another model was genuinely worth adding before expanding the UI, and required any added model to come with repository-local source notes plus clearer support for each remaining comparative option.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --build-solutions --quit`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1800 / 1800` passed).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Replace the splash screen's fragile root-directory intro audio scan with a shared application audio controller and exported audio library so desktop builds reliably include startup audio and the app has an extendable audio foundation.
- Input Materials Used: User report that the desktop build did not export the intro sounds and request to add an audio controller plus audio resource; `AGENTS.md`; `claude.md`; `src/app/SplashScreen.cs`; `src/app/SplashScreen.tscn`; `src/app/MainApp.cs`; `src/app/MainApp.tscn`; `src/app/MainMenuScreen.cs`; `export_presets.cfg`; existing root `.ogg` audio asset; integration test harness files; version/docs metadata.
- AI Produced: Added `AppAudioCueId`, `AppAudioLibrary`, and `AppAudioController`; added `Resources/Audio/MainAudioLibrary.tres` referencing the intro `.ogg`; attached the controller and shared players to `MainApp.tscn`; rewired `SplashScreen` to use the shared controller for intro playback and fade-out instead of scanning for `.ogg` files or owning a local player; updated the release-notes copy and project-structure docs; added `TestAppAudio` integration coverage; and bumped internal patch metadata to `0.8.7.2`.
- Human Accepted: Pending review of the shared audio architecture, the exported-library resource path, and the decision to move startup music ownership from the splash into `MainApp`.
- Human Rejected: The user explicitly did not want another one-off local splash fix and asked for an audio controller plus audio resource that remains extendable as more sounds are added later.
- Human Changed: The user framed this as a patch-level fix but also required the solution to be extendable, which drove the shared controller/library design instead of only hard-wiring the current intro track.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Add section-level source affordances to Galaxy Studio so each major galaxy-parameter heading exposes the relevant cited sources on hover, and inventory the current `Sources/` directory contents for the user.
- Input Materials Used: User request to list all source files and add per-heading source tooltips in `GalaxyGenerationScreen`; `AGENTS.md`; `claude.md`; `src/app/GalaxyGenerationScreen.tscn`; `src/app/GalaxyGenerationScreen.Science.cs`; `GalaxyScienceReferenceCatalog.cs`; `StellarScienceReferenceCatalog.cs`; `PlanetaryScienceReferenceCatalog.cs`; `Tests/Integration/TestStudioScienceUi.cs`; current version metadata files.
- AI Produced: Added scene-owned `(i)` buttons beside the Galaxy Type, Scientific Priors, Structure, Size and Density, Stellar Priors, and Planetary Priors headings in `GalaxyGenerationScreen.tscn`; wired those controls in `GalaxyGenerationScreen.Science.cs` to build multiline source tooltips from the existing science catalogs; added missing planetary catalog helpers for parameter-to-source lookup; expanded the galaxy-studio integration test to assert that the new source buttons exist and expose expected citations; and synced internal patch metadata to `0.8.7.1`.
- Human Accepted: Pending review of the tooltip wording, section-to-source grouping, and the overall usefulness of the new hover affordances in Galaxy Studio.
- Human Rejected: No separate popups or duplicated hard-coded source strings were introduced; the user asked specifically for lightweight tooltip affordances next to section headings.
- Human Changed: The user narrowed the request to section-level source visibility in Galaxy Studio rather than a broader help-surface redesign, and asked for the current contents of the `Sources/` area to be listed first.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-29 - Codex (GPT-5)

- Task Purpose: Implement the `0.8.6.0` planetary retrofit by threading a shared aggregate planetary-formation profile through galaxy and system generation, exposing the new upstream controls in Galaxy Studio and System Studio, and keeping Object Studio limited to direct single-planet controls.
- Input Materials Used: User-approved `StarGen Planetary Retrofit Plan`; follow-up user correction that aggregate planetary-formation parameters belong only at galaxy/system scope while object-level controls must stay direct; `AGENTS.md`; `claude.md`; `Sources/Texts/planets.md`; existing galaxy/system/object generation, UI, and test files on `codex/release-0.9-mainline`.
- AI Produced: Added `PlanetaryGenerationProfile`, `PlanetarySystemState`, and `PlanetaryScienceReferenceCatalog`; extended `GalaxyConfig`, `SolarSystemSpec`, `SystemPlanetGenerator`, `PlanetGenerator`, and `PlanetSpec` to support shared aggregate planetary assumptions plus additive direct planet overrides and provenance; added matching aggregate planetary controls and help text to Galaxy Studio and System Studio; expanded Object Studio planet controls only with direct single-planet settings; added deterministic serialization, propagation, help-catalog, and generator regressions; and synced internal version/docs metadata to `0.8.6.0`.
- Human Accepted: Pending review of the aggregate-control defaults, the retrofit-level generator behavior shifts, and the wording of the new planetary help surfaces before merge or release.
- Human Rejected: The user explicitly rejected putting aggregate planetary-formation controls into Object Studio and rejected treating `Sources/Texts/planets.md` as a mandate to rewrite the full planetary architecture from scratch.
- Human Changed: The user clarified the studio-boundary rule after the initial retrofit planning pass: Galaxy/System own population-level formation priors, while Object Studio owns only direct planet-facing controls.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1789 / 1789` passed).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-29 - Codex (GPT-5)

- Task Purpose: Apply the `0.8.5.1` patch update for Object Studio moon controls and checkbox visibility by adding a moon-count target selector, carrying that target into deterministic moon generation, and making unchecked checkboxes visibly readable in the dark theme.
- Input Materials Used: User follow-up patch request clarifying that this work should be treated as a patch update, that moon generation needs a target-count control rather than a pure boolean, and that the current checkbox styling makes unchecked boxes effectively invisible; `AGENTS.md`; `claude.md`; the existing `0.8.5.0` Object Studio, Object Viewer, theme, and test files.
- AI Produced: Added a scene-owned moon target-count dropdown under the planet moon controls, updated Object Studio visibility and summary logic for the new moon-count control, carried the selected target count into `PlanetSpec` overrides, expanded the object-viewer generation path so planet launches can generate multiple moons deterministically with planet-size-based caps, updated the dark theme checkbox styles so unchecked boxes remain visible, added regression coverage for the new UI row and moon-count resolution logic, and synced internal patch metadata to `0.8.5.1`.
- Human Accepted: Pending review of the new moon-count UX, the size-based cap behavior, and the checkbox visibility fix before merge or release.
- Human Rejected: The earlier all-or-nothing moon checkbox was rejected as too limited, and the earlier invisible unchecked checkbox presentation was rejected as unusable.
- Human Changed: The user clarified that the moon count should be a target rather than an unconditional exact promise, because the final generated count must still respect the generated planet size.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1780 / 1780` passed).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-29 - Codex (GPT-5)

- Task Purpose: Implement the current `0.8.5.0` object-taxonomy and Object Studio patch by making Object Studio context-sensitive, moving moon generation under planets, adding comet support, and broadening the practical asteroid and planet controls.
- Input Materials Used: User-approved `0.8.5.0` object-taxonomy plan; follow-up user correction that upstream planetary models must eventually surface in higher-level studios; follow-up user patch request describing the remaining Object Studio problems; `AGENTS.md`; `claude.md`; the existing object generation, viewer, rendering, persistence, and test files already on the `codex/release-0.9-mainline` branch.
- AI Produced: Added `CometSpec` and `CometGenerator`; introduced comet handling across celestial typing, rendering, persistence, viewer generation, and save/load metadata; expanded asteroid taxonomy and added orbit-band, density, and albedo shaping controls; removed standalone moon selection from the Object Studio top-level picker and moved moon generation under planets with context-sensitive moon controls; expanded planet profile overrides in Object Studio; simplified star object editing to subclass-first controls while dropping unreliable direct metallicity and age fields; updated Object Studio visibility logic and assumptions text; added comet and Object Studio context-gating tests; and synced roadmap/version/project-structure metadata to `0.8.5.0`.
- Human Accepted: Pending review of the new comet defaults, the broadened Object Studio control surface, and the partial `0.8.5.0` scope implemented in this checkpoint before merge or release.
- Human Rejected: The user explicitly rejected keeping moon generation as a top-level Object Studio type, rejected showing Traveller-only parameters in the general Parameters section, rejected the earlier star UI that exposed metallicity and age even though they were intertwined and not reliably direct-editable, and rejected the earlier limited asteroid/planet control surfaces.
- Human Changed: The user clarified that patches and feature slices must be committed as checkpoints, that Object Studio should expose exactly one object context at a time, that moons belong under planets in the UI, that comets must actually exist in the tool, and that asteroid/planet authoring should expose materially richer controls than the earlier limited profile.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1780 / 1780` passed).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-28 - Codex (GPT-5)

- Task Purpose: Implement the `0.8.4.0` stellar-population expansion by broadening practical stellar outputs beyond main-sequence stars, adding supporting research notes to `Sources/`, updating downstream system-generation behavior, and rewriting tests around the expanded offerings.
- Input Materials Used: User request to expand the stellar offerings and add any needed research to `Sources/`; `AGENTS.md`; `claude.md`; existing stellar generation, system generation, rendering, help, and test files; reviewed source notes and source metadata for Cummings et al. 2018, Kirkpatrick et al. 2000, Kirkpatrick et al. 2011, Kirkpatrick et al. 2024, Moe and Di Stefano 2017, and Tokovinin 2021.
- AI Produced: Added the new roadmap effort and reviewed source-note files for the stellar expansion; extended spectral support to `L/T/Y`; expanded `StarTable`, `StellarMassSampler`, `StarGenerator`, `ColorUtils`, and `StellarConfigGenerator` so brown dwarfs, evolved stars, white dwarfs, and stronger hierarchical multiplicity are generated deterministically; updated parameter/help text for the broader stellar outputs; rewrote scientific benchmark handling to cover brown-dwarf and white-dwarf populations; added new unit/statistical coverage for the expanded stellar model; and synced internal version/docs metadata to `0.8.4.0`.
- Human Accepted: Pending review of the broadened stellar defaults, the practical scope chosen for evolved/compact outcomes, and the updated scientific benchmark ranges before merge or release.
- Human Rejected: The prior effectively main-sequence-only stellar offering was rejected as too narrow for the current research-backed scope. Unsupported compact-remnant end states such as neutron stars and black holes were kept out of this pass.
- Human Changed: The user explicitly required practical stellar expansion rather than discussion-only planning, required any extra research used to be added into the repository `Sources/` folder, and pushed the implementation toward all practical offerings now supported by the reviewed research set.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1776 / 1776` passed after the stellar-expansion pass).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-28 - Codex (GPT-5)

- Task Purpose: Apply the `0.8.3.2` tooltip-formatting patch by turning the science-facing tooltip copy into multiline text so each sentence appears on its own line.
- Input Materials Used: User request to make tooltips multiline if Godot supports it; current galaxy/system tooltip wiring and science-reference catalogs; `AGENTS.md`; `claude.md`.
- AI Produced: Updated the galaxy and stellar science tooltip strings, related system and galaxy parameter assumption text, and the two `Help` button tooltips so sentence breaks are explicit newline breaks in the tooltip text; synced the internal patch metadata to `0.8.3.2`.
- Human Accepted: Pending review of the in-editor tooltip presentation.
- Human Rejected: The user implicitly rejected keeping the tooltip text as single wrapped paragraphs when a clearer sentence-per-line layout was possible.
- Human Changed: The user specified the exact formatting behavior desired for this patch: one sentence per tooltip line.
- Validation Method: `dotnet build StarGen.sln`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-28 - Codex (GPT-5)

- Task Purpose: Apply the `0.8.3.1` help-surface patch by removing the internal `galactic_formation.md` document from the app's source lists, rewriting the galaxy and stellar help text/tooltips for non-experts, cleaning up the help-popup presentation, and adding a matching `Help` popup to System Studio.
- Input Materials Used: User patch direction to remove `galactic_formation.md` as a source, improve the popup visuals, expand the plain-language explanations so non-experts understand what changing settings actually does, and add System Studio help; `AGENTS.md`; `claude.md`; current galaxy/system studio scenes and controllers; `GalaxyScienceReferenceCatalog`; `StellarScienceReferenceCatalog`; studio integration tests.
- AI Produced: Reworked both science catalogs so they cite only external research sources, rewrote tooltip and popup copy around practical outcomes rather than jargon, upgraded the galaxy help window layout in `.tscn`, added a scene-owned help popup to System Studio, added non-UI unit checks for the revised help copy, and updated the studio help integration tests to cover the new system help surface.
- Human Accepted: Pending review of the revised wording, popup presentation, and the expanded system help flow.
- Human Rejected: The user explicitly rejected using the internal `Docs/galactic_formation.md` file as a visible help/source citation and rejected help copy that defined terms without explaining what changing the controls would do.
- Human Changed: The user clarified that the help text must work for non-experts at roughly an eighth-grade reading level, that the tooltip copy must stay brief but still actionable, and that System Studio should expose the same kind of help entry point as Galaxy Studio.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1768 / 1768` passed when rerun on its own after one transient Godot crash during an overlapping build/test run).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-28 - Codex (GPT-5)

- Task Purpose: Implement the `0.8.3.0` follow-up feature slice by checkpointing the `0.8.2.0` branch state, replacing the galaxy studio's inline science panel with a plain-language `Help` popup, adding the stellar-tier science model and controls, and removing the old `4`-star UI cap in favor of the supported `10`-star limit.
- Input Materials Used: User-approved `StarGen 0.8.3 Plan: Checkpoint, Plain-Language Help, and Stellar-Tier Science`; `AGENTS.md`; `claude.md`; existing galaxy/system/star generation code and tests; `Docs/galactic_formation.md`; the current `0.8.2.0` branch state that had already implemented galaxy-tier science and citations.
- AI Produced: Created a focused `0.8.2.0` checkpoint commit before feature work; moved galaxy help into a scene-owned header popup with a scrollable `RichTextLabel` and close controls; rewrote galaxy science tooltips/help in plain language for non-experts; added a shared `StellarGenerationProfile`, deterministic IMF-driven stellar mass sampling, lightweight isochrone-style stellar property lookup, and downstream profile propagation from galaxies into systems and stars; exposed stellar model controls in both Galaxy Studio and System Studio; raised the system-studio star-count UI limit from `4` to `10`; and added non-visual UI plus stellar-science regression coverage.
- Human Accepted: Pending review of the final wording, science defaults, citations, and the broader `0.8.3.0` checkpoint before merge or release.
- Human Rejected: The user rejected keeping the galaxy science content inline in the studio and explicitly rejected expert-only wording that assumes the player already knows astrophysical terms. The old `4`-star UI cap was also rejected as artificial.
- Human Changed: The user required the current branch state to be committed first as an explicit `0.8.2.0` checkpoint, renamed the galaxy science entry point to `Help`, required the help and tooltips to be understandable at roughly an eighth-grade reading level, required stellar-tier research to be incorporated as the `0.8.3.0` feature goal, and required the studios to expose real model choices where the research offered them.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1765 / 1765` passed after the stellar-tier implementation and UI changes).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-28 - Codex (GPT-5)

- Task Purpose: Implement the `0.8.2.0` scientifically grounded galaxy-generation feature by replacing the older galaxy shape-only model with a deterministic science-backed profile, propagating galaxy-origin context into downstream star/system generation, exposing cited scientific assumptions in the galaxy studio, and rewriting tests around the new behavior.
- Input Materials Used: User-approved `StarGen 0.8.2 Scientifically Grounded Galaxy Generation` plan; `AGENTS.md`; `claude.md`; existing galaxy/system/star generation code and tests; `Docs/galactic_formation.md`; source list supplied in the approved plan, including Park et al. 2007, Tanaka et al. 2004, Behroozi et al. 2019, the UniverseMachine dwarf extension paper, Oohama et al. 2009, Laurikainen et al. 2010, Diaz-Garcia et al. 2016, Kennicutt 1998, Forgan et al. 2017, Spitoni et al. 2017, APOGEE 2024 metallicity-gradient work, and C-MetaLL 2024.
- AI Produced: Added galaxy-science enums, realism-profile and galaxy-origin-context types, deterministic profile-building and scientific-field evaluation services, a lenticular density model, expanded `GalaxyConfig`/`GalaxySpec`/`GalaxyStar`, propagated galaxy context into `SolarSystemSpec`, `StarSpec`, `StellarConfigGenerator`, and `StarGenerator`, expanded galaxy parameter metadata and validation, added a scene-first galaxy-studio science panel plus scientific control wiring, and replaced the older galaxy/unit test coverage with deterministic scientific and downstream-propagation tests.
- Human Accepted: Pending review of the scientific defaults, citations, control language, and downstream biasing behavior before merge or release.
- Human Rejected: The user explicitly rejected turning StarGen into a time-evolving galaxy simulation; clustered spatial star placement beyond scaffolding was kept out of `0.8.2`. The user also maintained the policy that UI-surface tests should remain discarded rather than reintroduced for the new science panel.
- Human Changed: The user set the scope and acceptance criteria: scientifically sound principles rather than full simulation, explicit user family choice with variation inside that family, user-facing info surfaces that explain the science and cite sources, explicit handling of paper gaps via additional sources, and downstream coupling so galaxy-level priors shape star/system generation.
- Validation Method: `dotnet clean StarGen.sln`; `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1756 / 1756` passed after a clean rebuild).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-27 - Codex (GPT-5)

- Task Purpose: Implement the first `0.9` mainline release-branch slice by removing shipped concept/persistence runtime paths, switching user-facing versioning to branch-channel suffixes, trimming the test suite to non-UI mainline coverage, and syncing release docs to the new scope.
- Input Materials Used: User-approved `StarGen v0.9 Release Branch Plan`; follow-up user directives that the authoritative branch point is the latest local `master`, UI tests should be discarded, and tests conflicting with `0.9` goals should be removed; `AGENTS.md`; `Claude.md`; current mainline app/viewer/test/doc files.
- AI Produced: Removed Concept Atlas launch/runtime integration from the shipped mainline app flow; removed mainline load/save/export affordances from studios, viewers, menus, and the object edit dialog; changed `UserFacingVersionHelper` to compose the display label from base version plus release channel (`d` mainline / `e` export); updated `project.godot`, export metadata, README, VERSION notes, and v0.9 docs; rewrote harness registration to drop concept, persistence, and UI-focused suites from mainline.
- Human Accepted: Pending review of the `0.9` mainline branch cut, the narrowed test policy, and the synced release docs/metadata.
- Human Rejected: UI-focused tests were explicitly rejected for migration; mainline persistence/save-load/export behavior was explicitly rejected for the shipped `0.9` line; concept additions were explicitly kept constrained to StarGen-relevant prototype work only.
- Human Changed: The user clarified that the branch point must track the latest local `master`, not the earlier prep commit, and that tests should be deleted when they no longer protect real `0.9` behavior or conflict with the release goals.
- Validation Method: Pending final `dotnet build StarGen.sln` and reduced-scope test-harness validation after the remaining stale test files are removed.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-26 - Codex (GPT-5)

- Task Purpose: Convert the concept-pipeline hardening branch from its pre-release split state into the actual `0.8.0.0` release candidate, remove the non-menu Concept Atlas entrypoint after user clarification, sync release-facing docs/metadata, and prepare the branch for merge/review/publish.
- Input Materials Used: User-approved merge/release plan; follow-up user clarification that Concept Atlas should only exist on the main menu; `AGENTS.md`; `claude.md`; release/version/docs files; splash/main-app/object-viewer files; headless/build validation results; root media assets in the repo.
- AI Produced: Removed the object-viewer Concept Atlas signal path and its obsolete tests, kept the splash/video transition scene-owned while adding the fade-to-black main-menu handoff, fixed the splash scene's direct video resource reference, collapsed the remaining internal/docs/release notes text to the shipped `0.8.0.0` line, tightened the credits/release copy for the intro media, and prepared the branch for a local release merge/review flow.
- Human Accepted: Pending final review of the merged release candidate and publication steps.
- Human Rejected: The object-viewer Concept Atlas path and the reintroduced inspector button were explicitly rejected after the user clarified that Atlas access should stay hidden outside the main menu.
- Human Changed: The user corrected the intended UI contract mid-pass: Concept Atlas is main-menu-only, not available from the object viewer or other viewers, and confirmed that the substantive music attribution remains correct but should be presented more neatly.
- Validation Method: `dotnet build StarGen.sln` (passed); `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` passed cleanly (`1962 / 1962`) before the user limited further reruns to targeted failures only.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-26 - Codex (GPT-5)

- Task Purpose: Correct the intro presentation after review by constraining the video to the logo footprint and wiring the new root `.ogg` music file into the splash automatically.
- Input Materials Used: User feedback that the intro video was visually wrong, needed to match the icon height, should resolve directly into the icon, and should use the only root `.ogg` file for music; `AGENTS.md`; `claude.md`; `src/app/SplashScreen.cs/.tscn`; splash integration tests; version/export metadata files.
- AI Produced: Reworked the splash layout so video and logo share a centered `512 px` media frame, made the logo resolve in place over that frame, added root-directory `.ogg` discovery for the intro music player when exactly one file is present, updated splash regressions, and synced the internal hardening line to `0.7.10.1` while keeping the app/project label on `0.8.0.0`.
- Human Accepted: Pending review of the corrected intro sizing, transition feel, and music playback.
- Human Rejected: The earlier full-screen video presentation was implicitly rejected by the user as visually incorrect.
- Human Changed: The user explicitly set the new acceptance bar: video height must match the icon height, the transition must resolve into the icon itself, and the intro music should use the only `.ogg` in the repo root.
- Validation Method: `dotnet build StarGen.sln` (passed); `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` reached `1962 / 1963` passing, with one remaining unrelated object-viewer Concept Atlas failure.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-26 - Codex (GPT-5)

- Task Purpose: Replace the timer-based startup splash with the root intro video, leave an explicit hook for future music, add a logo fade transition, and sync the related version/docs metadata.
- Input Materials Used: User request for an `.ogv` intro splash and logo transition; `AGENTS.md`; `claude.md`; `Docs/Roadmap.md`; `src/app/MainApp.cs`; `src/app/MainApp.Navigation.cs`; `src/app/SplashScreen.cs/.tscn`; `src/app/MainMenuScreen.cs/.tscn`; integration tests; version/export metadata files.
- AI Produced: Rebuilt `SplashScreen` around a `VideoStreamPlayer`, logo overlay, optional `AudioStreamPlayer`, skip-aware transition tweening, and root-asset loading; added startup regressions for splash media wiring and initial `MainApp` splash state; added a roadmap effort entry for startup presentation polish; and later folded the change into the internal `0.7.10.1` hardening line while keeping the app/project label on `0.8.0.0`.
- Human Accepted: Pending review of the intro playback, fade timing, and the metadata sync.
- Human Rejected: No broader main-menu redesign or startup-sequence branching beyond the requested video/logo transition was added.
- Human Changed: The user explicitly directed that the root `.ogv` replace the normal splash and that the splash wiring keep space for future music rather than baking in a temporary soundtrack choice.
- Validation Method: `dotnet build StarGen.sln` (passed); `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` reached `1962 / 1963` passing, with one remaining failure in the unrelated object-viewer Concept Atlas test.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-23 - Codex (GPT-5)

- Task Purpose: Prepare the 0.8 UI/wiring pass by moving Concept Atlas access to main-menu-only usage, wiring Station Studio to live generation at prototype parity, hiding the galaxy jump-routes tool button, and syncing version metadata to `0.8.0.0`.
- Input Materials Used: User-approved implementation plan; `CLAUDE.md`; `AI-Use-Statement.md`; `src/app/viewer/InspectorPanel.cs`; `src/services/concepts/ConceptContextBuilder.cs`; `src/app/StationStudioScreen.cs`; `src/app/StationStudioScreen.tscn`; `src/app/prototypes/StationGeneratorPrototype.cs/.tscn`; `src/app/galaxy_viewer/GalaxyInspectorPanel.cs`; `VERSION.md`; `project.godot`; `src/app/shared/UserFacingVersionHelper.cs`; `src/app/MainMenuScreen.tscn`; `src/app/SplashScreen.tscn`; `README.md`; `Docs/ProjectStructure.md`.
- AI Produced: Removed the object-view Concept Atlas button while retaining underlying wiring, strengthened the default Concept Atlas snapshot toward an Earth-like baseline, replaced the Station Studio placeholder with an interactive generation/results studio flow based on the existing prototype logic, hid the galaxy colonization simulation button without removing signal/callback code paths, and synchronized internal/user-facing version strings and docs to `0.8.0.0`.
- Human Accepted: Pending review of the 0.8 prep UI behavior and metadata sync.
- Human Rejected: No removal of latent jump-route wiring or broader Station Studio scope beyond prototype parity was included.
- Human Changed: The user selected exact scope (prototype parity for Station Studio and internal/user-facing version both set to `0.8.0.0`) before implementation.
- Validation Method: Pending full validation run in this pass (`dotnet build StarGen.sln`, targeted navigation tests, and lint checks on edited files).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-22 - Codex (GPT-5)

- Task Purpose: Lock in the now-correct generator-studio UI by moving panel minimum widths fully back into the scene tree, removing helper-side width overrides, and updating regressions so they protect the current visual layout instead of older code-driven sizing behavior.
- Input Materials Used: User direction that the current UI is correct and must not regress; `claude.md`; `src/app/shared/StudioScreenLayoutHelper.cs`; `src/app/GalaxyGenerationScreen.tscn`; `src/app/SystemGenerationScreen.tscn`; `src/app/ObjectGenerationScreen.tscn`; `src/app/GalaxyGenerationScreen.cs`; `src/app/SystemGenerationScreen.cs`; `src/app/ObjectGenerationScreen.cs`; `src/app/ObjectGenerationScreen.EnhancedUi.cs`; `Tests/Integration/TestStudioScreenLayoutHelper.cs`; `VERSION.md`; `project.godot`; `export_presets.cfg`; `Docs/ProjectStructure.md`.
- AI Produced: Set all studio panels to scene-owned `200 px` minimum widths, removed child and runtime-created label minimums that were effectively overriding those widths, reduced `StudioScreenLayoutHelper` to orientation-only behavior, updated studio regressions to assert the scene-owned contract, and bumped the internal bugfix version to `0.7.9.5`.
- Human Accepted: Pending review of the scene-owned studio sizing and the updated regression contract.
- Human Rejected: Earlier helper-driven or test-driven assumptions that code should own studio sizing were rejected; the user explicitly confirmed the visual target in the running UI and directed that regressions should preserve that state instead of reshaping it.
- Human Changed: The user clarified that the current UI appearance is the correct reference point and that any further work should avoid regressions in the UI space, which redirected the fix from margin tweaking to scene/test alignment.
- Validation Method: `dotnet build StarGen.sln` (passed); `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1956 / 1962` passed, with only the repo's existing unrelated population/concept failures remaining and the studio layout regressions passing).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-22 - Codex (GPT-5)

- Task Purpose: Finish the generator-studio border adjustment by matching the horizontal inset to the already-correct top and bottom shell spacing after iterative user review.
- Input Materials Used: User feedback that the studio side border existed but did not visually match the top/bottom margin; `src/app/GalaxyGenerationScreen.tscn`; `src/app/SystemGenerationScreen.tscn`; `src/app/ObjectGenerationScreen.tscn`; `Tests/Integration/TestGalaxyGenerationScreen.cs`; `VERSION.md`; `project.godot`; `export_presets.cfg`; `Docs/ProjectStructure.md`.
- AI Produced: Set the studio `MainPanel` left/right inset to `18 px` so it matches the top/bottom shell spacing, kept the galaxy-studio inset regression aligned with that equal-spacing value, and bumped the internal bugfix version to `0.7.9.4`.
- Human Accepted: Pending review of the equalized studio border spacing.
- Human Rejected: Earlier `32 px` and `10 px` attempts were rejected or superseded during iterative review.
- Human Changed: The user clarified that the correct target was equal visual border size on all four sides, not simply more or less left/right inset.
- Validation Method: `dotnet build StarGen.sln` (passed); the full Godot headless harness remains blocked by the repo's existing unrelated population and concept-pipeline failures.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-22 - Codex (GPT-5)

- Task Purpose: Correct the follow-up studio spacing pass after user review showed the stronger inset moved in the wrong direction, keeping the adjustment scene-first and narrowly scoped to the generator studio shell.
- Input Materials Used: User feedback that the previous change made the spacing worse; `src/app/GalaxyGenerationScreen.tscn`; `src/app/SystemGenerationScreen.tscn`; `src/app/ObjectGenerationScreen.tscn`; `Tests/Integration/TestGalaxyGenerationScreen.cs`; `VERSION.md`; `project.godot`; `export_presets.cfg`; `Docs/ProjectStructure.md`.
- AI Produced: Reduced the generator-studio `MainPanel` left/right inset to `10 px`, updated the matching galaxy-studio inset regression source, and bumped the internal bugfix version line to `0.7.9.3`.
- Human Accepted: Pending review of the tighter studio-shell spacing.
- Human Rejected: The previous `32 px` inset increase was rejected by the user as making the spacing worse.
- Human Changed: The user explicitly redirected the fix after seeing the stronger inset, so the correction was reversed instead of iterating further in the same direction.
- Validation Method: `dotnet build StarGen.sln` (passed); full Godot headless harness rerun, with remaining failures still confined to the repo's unrelated population and concept suites.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-22 - Codex (GPT-5)

- Task Purpose: Apply a follow-up spacing correction after the first studio inset adjustment still looked too edge-tight in user review, keeping the fix scene-first and limited to the generator studio shells.
- Input Materials Used: User feedback that the first spacing pass still did not border correctly; `src/app/GalaxyGenerationScreen.tscn`; `src/app/SystemGenerationScreen.tscn`; `src/app/ObjectGenerationScreen.tscn`; `Tests/Integration/TestGalaxyGenerationScreen.cs`; `VERSION.md`; `project.godot`; `export_presets.cfg`; `Docs/ProjectStructure.md`.
- AI Produced: Increased the generator-studio `MainPanel` left/right inset from `24 px` to `32 px`, kept the galaxy-studio inset regression aligned with the stronger value, and bumped the internal bugfix line to `0.7.9.2`.
- Human Accepted: Pending review of the stronger inset correction.
- Human Rejected: No helper-based spacing hack or broader layout redesign was introduced; the adjustment stayed as a scene-defined shell change.
- Human Changed: The user explicitly rejected the first inset pass as still incorrect, so the correction was made stronger instead of being left at the earlier value.
- Validation Method: `dotnet build StarGen.sln` (passed); Godot headless validation rerun after the inset change, with remaining failures still confined to the repo's unrelated population/concept suites.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-22 - Codex (GPT-5)

- Task Purpose: Match the generator-studio edge spacing to the main menu shell by widening the scene-defined horizontal inset on the studio main panels, then sync patch-version metadata and regression coverage.
- Input Materials Used: User screenshots comparing the generator studios against the main menu; `claude.md`; `src/app/GalaxyGenerationScreen.tscn`; `src/app/SystemGenerationScreen.tscn`; `src/app/ObjectGenerationScreen.tscn`; `Tests/Integration/TestGalaxyGenerationScreen.cs`; `VERSION.md`; `project.godot`; `export_presets.cfg`; `Docs/ProjectStructure.md`.
- AI Produced: Increased the `MainPanel` left/right margin in the galaxy/system/object studio scenes, updated the galaxy-studio integration test to target the current scene structure and assert the widened inset, and bumped the internal bugfix line to `0.7.9.1`.
- Human Accepted: Pending review of the studio spacing adjustment and metadata sync.
- Human Rejected: No broader redesign of studio panel sizing, breakpoint behavior, or theme styling was added; the change stayed focused on the horizontal edge spacing.
- Human Changed: The user-defined visual target for the spacing was the main menu shell, and the fix was kept scene-first rather than moving the adjustment into shared code.
- Validation Method: `dotnet build StarGen.sln` (passed); `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1953 / 1959` passed, with only the same unrelated population and concept-pipeline failures remaining).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-22 - Codex (GPT-5)

- Task Purpose: Continue the scene-first UI cleanup by moving the main menu's utility dialog shell out of runtime C# construction and into the Godot scene tree, then sync version/provenance metadata for the refactor.
- Input Materials Used: User direction that screen structure should live in `.tscn` files whenever possible; `claude.md`; `src/app/MainMenuScreen.cs/.tscn`; `Tests/Integration/TestMainMenuScreen.cs`; `VERSION.md`; `project.godot`; `export_presets.cfg`; `Docs/ProjectStructure.md`.
- AI Produced: Added scene-owned `InfoDialog` and `OptionsDialog` nodes to `MainMenuScreen.tscn`, removed the corresponding runtime dialog builders from `MainMenuScreen.cs`, updated the main-menu regression to assert scene-backed dialog nodes, and synced the internal refactor version line to `0.7.9.0` across metadata/docs.
- Human Accepted: Pending review of the main-menu scene-first refactor and metadata sync.
- Human Rejected: No broader redesign of the main menu layout or conversion to embedded utility content panels was added; the change stayed focused on moving fixed dialog structure into the scene.
- Human Changed: The user-set rule for this pass was that if a screen element can live in the `.tscn`, it should be defined there instead of being constructed in script.
- Validation Method: `dotnet build StarGen.sln` (passed); `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1948 / 1958` passed, with remaining failures in unrelated population/concept-pipeline/galaxy-generation suites and the updated main-menu dialog regression passing).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-22 - Cursor (agent)

- Task Purpose: Remove Concept Atlas entry points from the galaxy and system viewers (keep main menu and object viewer); bump internal version to `0.7.8.2` and sync documentation; create a focused git commit (exclude unrelated working-tree edits).
- Input Materials Used: User direction; grep of `OpenConceptAtlas` / `GalaxyInspectorPanel` / `SystemInspectorPanel` / `MainApp`; existing integration tests.
- AI Produced: UI and navigation wiring removed; tests updated; `VERSION.md` / `project.godot` / `Docs/ProjectStructure.md` / this log entry; repository commit with message `feat: remove Concept Atlas from galaxy and system viewers`.
- Human Accepted: Pending review.
- Human Rejected: None.
- Human Changed: The user requested commit discipline for this slice.
- Validation Method: `dotnet build StarGen.sln` (succeeded before commit).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-22 - Cursor (agent)

- Task Purpose: Commit the system-viewer camera follow-focus feature with full repository commit discipline: document the change, bump internal version to `0.7.8.1`, sync `project.godot` and `export_presets.cfg`, update `Docs/ProjectStructure.md`, and record provenance.
- Input Materials Used: Prior implementation on branch `codex/concept-pipeline-hardening` (`SystemCameraController`, `SystemViewer` partials, `TestSystemCameraController`); `VERSION.md` versioning rules; `CLAUDE.md` workflow notes.
- AI Produced: Version metadata and documentation sync (`0.7.8.0` → `0.7.8.1`); `export_presets.cfg` Android `version/code` `70800` → `70801`; this log entry; Project Structure “Recent 0.7.8.1 additions” bullet.
- Human Accepted: Pending review of the combined commit.
- Human Rejected: None.
- Human Changed: The user requested commit discipline for this slice.
- Validation Method: `dotnet build StarGen.sln` (run before commit).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-22 - Cursor (agent)

- Task Purpose: Create a single repository commit for the in-progress colonization-simulation, Traveller routing, studio-shell, population, and test updates; bump the internal feature version line to `0.7.8.0` and sync version metadata (`VERSION.md`, `project.godot`, `export_presets.cfg`, `Versions.GeneratorVersion`).
- Input Materials Used: Git working tree on branch `codex/concept-pipeline-hardening`; existing `VERSION.md` and project versioning conventions in `claude.md`.
- AI Produced: Version and provenance documentation updates accompanying the commit; no new feature code beyond metadata and log edits in this step.
- Human Accepted: Pending review of the combined feature slice and version bump.
- Human Rejected: None.
- Human Changed: The user requested the commit and feature-version increment.
- Validation Method: `dotnet build StarGen.sln` (to be run immediately before commit).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-22 - Codex (GPT-5)

- Task Purpose: Repair the studio-screen layout regression that broke node paths, scrunched the panels vertically, and failed to honor the intended `640x800` minimum window size.
- Input Materials Used: `claude.md`; `project.godot`; `src/app/GalaxyGenerationScreen.tscn`; `src/app/SystemGenerationScreen.tscn`; `src/app/ObjectGenerationScreen.tscn`; `src/app/shared/StudioScreenLayoutHelper.cs`; `Tests/Integration/TestStudioScreenLayoutHelper.cs`.
- AI Produced: Restored the studio scene hierarchy so code-side node paths and button hookups resolve again, returned the studio row containers to `BoxContainer`, moved responsive stacking/stretch rules into `StudioScreenLayoutHelper`, raised the project minimum height to `800`, and updated the layout integration tests to cover stacked compact layouts and stretched wide layouts.
- Human Accepted: Pending in-editor verification of the Galaxy, System, and Object studio screens.
- Human Rejected: None.
- Human Changed: The user clarified that the studios should fill height at the minimum `640x800` window and otherwise expand to the available screen size, instead of being globally wrapped in a scroll shell.
- Validation Method: `dotnet build D:\\Game Creation\\star_gen\\StarGen.sln`.
- Final Approver: Pending user review.

### 2026-03-22 - Codex (GPT-5)

- Task Purpose: Carry the main-menu bordered shell and outer margin treatment into the Galaxy, System, and Object generation studios.
- Input Materials Used: `claude.md`; `src/app/MainMenuScreen.tscn`; `src/app/GalaxyGenerationScreen.tscn`; `src/app/SystemGenerationScreen.tscn`; `src/app/ObjectGenerationScreen.tscn`; `src/app/GalaxyGenerationScreen.cs`; `src/app/SystemGenerationScreen.cs`; `src/app/ObjectGenerationScreen.cs`.
- AI Produced: Reworked each studio scene to use the same `ScrollContainer -> Layout -> HeroPanel + MainPanel` shell pattern as the main menu, moved the studio header controls into the new hero panel, updated the C# node paths to match the new scene structure, and verified the project still builds cleanly.
- Human Accepted: Pending in-editor verification of the studio framing and spacing.
- Human Rejected: None.
- Human Changed: The user clarified that the main-menu bordered margin treatment should carry through all generation studios, not just the raw responsive sizing behavior.
- Validation Method: `dotnet build D:\\Game Creation\\star_gen\\StarGen.sln`.
- Final Approver: Pending user review.

### 2026-03-22 - Codex (GPT-5)

- Task Purpose: Align the Galaxy, System, and Object generation studios with the main-menu margin/scroll layout so the UI remains readable on the 640 px minimum width window while keeping the three-panel structure.
- Input Materials Used: `claude.md`; `src/app/MainMenuScreen.tscn`; `src/app/GalaxyGenerationScreen.tscn`; `src/app/SystemGenerationScreen.tscn`; `src/app/ObjectGenerationScreen.tscn`.
- AI Produced: Wrapped each studio in a margined `ScrollContainer`, swapped the studio rows from `BoxContainer` to `HFlowContainer` for responsive wrapping, and reduced the panel minimum widths so the three columns can stack gracefully at narrow widths without losing the intended layout language.
- Human Accepted: Pending the user's visual verification at 640 px width.
- Human Rejected: None.
- Human Changed: N/A.
- Validation Method: Not run (visual layout change only).
- Final Approver: Pending user review.

### 2026-03-21 - Codex (GPT-5)

- Task Purpose: Keep the System/Object studios permanently side-by-side like Galaxy Studio and repair the native-life generation regression that left viable worlds empty after the generation/simulation split.
- Input Materials Used: User layout and life-generation requirements; `AGENTS.md`; `claude.md`; `StudioScreenLayoutHelper.cs`; `PopulationGenerator.cs`; `BiologySupportEvaluator.cs`; `PopulationProbability.cs`; population/layout tests; `VERSION.md`; `project.godot`.
- AI Produced: Changed the shared studio-layout helper so the studio shells always remain horizontal and adapt panel widths instead of stacking vertically, updated compact-width layout tests to lock that rule in, removed the native-population dependency on pre-existing concept-pipeline sentience, synchronized sentience state from generated native populations, tightened strict life support toward earthlike worlds while making permissive life settings strongly favor viable wet `HabitabilityScore 5+` worlds, and synced internal version metadata to the `0.7.7.3` bug-fix slice.
- Human Accepted: Pending review of the restored always-horizontal studio layout and the returned native-life distribution.
- Human Rejected: The user explicitly rejected the earlier responsive behavior that collapsed the studio columns into a vertical stack at smaller resolutions.
- Human Changed: The user clarified that smaller resolutions should be handled without ever leaving the side-by-side layout, and that life settings should interpolate from `0 = earthlike only` to `1 = viable 5+ worlds have high life odds`.
- Validation Method: `dotnet build D:\Game Creation\star_gen\StarGen.sln`; `godot-mono.exe --headless --path . --script Tests/RunTestsHeadless.gd` (`1957 / 1957` passed; the repo still prints the same post-summary popup/layout/ObjectDB/RID warning noise after completion).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-21 - Codex (GPT-5)

- Task Purpose: Restore missing viewer-level file-menu exits for system/object inspection and bring the System/Object studios onto the same three-panel layout structure as Galaxy Studio.
- Input Materials Used: User navigation/layout requirements; `AGENTS.md`; `claude.md`; `MainApp.cs`; `MainApp.Navigation.cs`; `SystemGenerationScreen.cs/.tscn`; `ObjectGenerationScreen.cs/.tscn`; `ObjectGenerationScreen.EnhancedUi.cs`; `SystemViewer.cs`; `SystemViewer.Menu.cs`; `ObjectViewer.cs`; `ObjectViewer.Menu.cs`; integration tests; `VERSION.md`; `project.godot`.
- AI Produced: Added explicit `New System...` / `New Object...` and `Return to Main Menu` file-menu actions in the system/object viewers, wired those actions through `MainApp`, restructured the System Studio and Object Studio scenes into Galaxy-style `settings + rules + summary` columns, updated controller node-path bindings and responsive layout calls, refreshed integration coverage for the new menu entries and studio paths, and synced internal version metadata to the `0.7.7.2` bug-fix slice.
- Human Accepted: Pending review of the new file-menu exits and studio layout parity.
- Human Rejected: No attempt was made in this pass to redesign the viewer rendering shells or to reopen hidden in-view generation flows; launching a fresh generation still routes back through the corresponding studio.
- Human Changed: The user clarified that the viewer needs explicit file-menu escape hatches to start a fresh system/object or return to the main menu, while the studios themselves should visually match the Galaxy Studio shell instead of keeping bespoke footer-in-settings layouts.
- Validation Method: `dotnet build D:\Game Creation\star_gen\StarGen.sln`; `godot-mono.exe --headless --path . --script Tests/RunTestsHeadless.gd` (`1957 / 1957` passed; the repo still prints the same post-summary popup/layout/ObjectDB/RID warning noise after completion).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-21 - Codex (GPT-5)

- Task Purpose: Restore the missing studio/viewer back-navigation behavior, remove hidden regeneration paths from viewer-only contexts, and keep object edits flowing back into the active system when drilling down from system view.
- Input Materials Used: User report about missing back buttons and incorrect viewer layering; `AGENTS.md`; `claude.md`; `MainApp.cs`; `MainApp.Navigation.cs`; `SystemViewer` and `ObjectViewer` scene/controller files; navigation/save-load integration tests; `VERSION.md`; `project.godot`.
- AI Produced: Added explicit top-bar back-button state to system/object viewers, hid back navigation for system-studio and object-studio result views while preserving it for galaxy/system drill-down flows, removed regeneration actions from viewer-only contexts by tying menu/button availability to generation-section visibility, added an object-view back button to the object viewer scene, updated `MainApp` launch behavior to respect studio origin, extended the body-edit callback so standalone-system object edits persist back into the current system viewer, updated integration coverage for the new navigation contract, and synced the internal version metadata to the `0.7.7.1` bug-fix slice.
- Human Accepted: Pending review of the restored back-button behavior and the stricter separation between studio generation and viewer inspection/edit flows.
- Human Rejected: No attempt was made in this patch to turn standalone object-studio edits into broader pipeline persistence beyond the active viewed object; the immediate requirement stayed on viewer navigation and persistence back into the currently open system path.
- Human Changed: The user clarified the desired hierarchy explicitly: galaxy-studio system/object views should step back up one level at a time, system-studio object view should return only to system view, and object-studio result view should be one layer deep with no back navigation.
- Validation Method: `dotnet build D:\Game Creation\star_gen\StarGen.sln`; `godot-mono.exe --headless --path . --script Tests/RunTestsHeadless.gd` (`1957 / 1957` passed; the repo still prints the same post-summary popup/layout/ObjectDB warning noise after completion).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-21 - Codex (GPT-5)

- Task Purpose: Implement the accepted plan that separates top-down generation from bottom-up colonization simulation, making native-only generation the initial-condition layer and moving colonies/non-Traveller routes into an explicit persisted simulation tool.
- Input Materials Used: User-approved implementation plan; `AGENTS.md`; `claude.md`; generation/population/galaxy/jump-lane/viewer/save-load code paths; existing colonization-route and galaxy-viewer tests; `VERSION.md`; `Docs/ProjectStructure.md`; `Docs/Roadmap.md`; `Docs/GDD.md`.
- AI Produced: Removed generation-side colony rebuilding and generation-side expansion settings, added typed colonization simulation settings/request/state/settlement/route records plus deterministic simulator/overlay services, persisted simulation state through galaxy save/load and region caching, updated galaxy-viewer/open-system flows to consume the saved simulation state authoritatively, aligned generation/viewer UI/tests with the new contract, refreshed the legacy colonization-route wrapper expectations, and synced version/docs metadata to the `0.7.7.0` feature slice.
- Human Accepted: Pending review of the native-only generation baseline, explicit subsector colonization simulation flow, and the generation-vs-simulation UI split.
- Human Rejected: Automatic viewer-driven colonization recalculation based on camera movement was explicitly not kept; simulation is now a tool-driven, explicit-scope action with cached results per region.
- Human Changed: The user explicitly chose the recommendation that generation should only establish starting conditions while emergent structures come from later simulation tools, and then requested the full plan be implemented.
- Validation Method: `dotnet build D:\Game Creation\star_gen\StarGen.sln`; `godot-mono.exe --headless --path . --script Tests/RunTestsHeadless.gd` (`1957 / 1957` passed; the repo still prints the same post-summary popup/layout/ObjectDB warning noise after completion).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-21 - Codex (GPT-5)

- Task Purpose: Formalize the new project standard that separates top-down generation from bottom-up simulation so future features consistently treat initial conditions and emergent structures as different pipeline stages.
- Input Materials Used: User design direction for making generation vs simulation a project standard; `AGENTS.md`; `claude.md`; `Docs/Roadmap.md`.
- AI Produced: Updated the repository guidance in `claude.md` and the roadmap guiding principles in `Docs/Roadmap.md` to state that studio-driven generation establishes initial conditions while tool-driven simulation creates emergent outcomes, with deterministic requirements applying to both.
- Human Accepted: Pending review of the new project-standard wording.
- Human Rejected: No code-path or UI behavior changes were introduced in this documentation-only follow-up.
- Human Changed: The user explicitly clarified that this distinction should become a general project standard, not just a note attached to jump routes or colonization.
- Validation Method: Documentation update only; no build or test rerun required.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-21 - Codex (GPT-5)

- Task Purpose: Replace the old non-Traveller heuristic jump-route population/linking path with a deterministic colonization-driven route network so visible routes represent interstellar expansion from export-capable populations rather than abstract nearest-population graph edges.
- Input Materials Used: User bug report about jump routes forming toward unpopulated areas; follow-up design direction to tie routes to native/colony expansion pressure; `AGENTS.md`; `claude.md`; `GalaxyViewer.Setup.cs`; `GalaxyViewer.JumpRoutes.cs`; `JumpLaneSystem.cs`; `JumpLaneCalculator.cs`; `PopulationGenerator.cs`; `PlanetPopulationData.cs`; `NativePopulation.cs`; `Colony.cs`; jump-lane and galaxy-viewer test files; version/project-structure metadata files.
- AI Produced: Added a new `ColonizationRouteCalculator` domain path for non-Traveller jump routes, expanded `JumpLaneSystem` with deterministic colonization-summary fields, changed the galaxy-viewer route-region builder to derive exporter pressure and colony-target viability from generated system data, updated route background/result reconstruction to preserve simulated colony populations, added unit coverage for colonization-driven route formation and richer route-system serialization, updated the galaxy-viewer regression to assert connected systems no longer remain empty, and synced version/project-structure metadata to the `0.7.6.1` bug-fix slice.
- Human Accepted: Pending review of the new colonization-driven route behavior and whether this first region-scoped slice should become the foundation for a later fuller galaxy colonization simulator.
- Human Rejected: The broader UI redesign to remove the Galaxy Studio `Expansion Pressure` control and expose a dedicated colonization-simulator configuration surface was discussed but not implemented in this patch.
- Human Changed: The user redirected the route-fix work away from lazy background graph recalculation and toward a colonization-linked model where jump routes reflect settlement spread from export-capable populations.
- Validation Method: `dotnet build D:\Game Creation\star_gen\StarGen.sln`; `godot-mono --headless --path . --script res://Tests/RunTestsHeadless.gd` (the full harness remains blocked by the same pre-existing CLR crash in `TestGalaxySystemGenerator::test_generate_system_with_galaxy_context_deterministic_population` before the new jump-route tests execute).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-21 - Codex (GPT-5)

- Task Purpose: Implement deterministic colony pressure from nearby native-inhabited worlds so expansion pressure can depend on same-system native proximity and lightweight nearby-system summaries without making results depend on generation order.
- Input Materials Used: User design direction for native-proximity-driven expansion pressure and performance constraints; `AGENTS.md`; `claude.md`; `PopulationGenerator.cs`; `PopulationLikelihood.cs`; `PopulationProbability.cs`; `PopulationSeeding.cs`; `Galaxy.cs`; `GalaxySystemGenerator.cs`; `StarSystemPreview.cs`; `SystemFixtureGenerator.cs`; population and galaxy test suites; version/project-structure/GDD files.
- AI Produced: Added typed colony-pressure and native-summary models, added `GalaxyNativePressureCalculator` with cached deterministic nearby-system summaries, moved authoritative colony generation to a second-pass `PopulationGenerator.RebuildColoniesForSystem(...)`, threaded galaxy context into preview/runtime generation so galaxy-opened systems use the same pressure-aware path, added unit coverage for pressure-driven colony probability/likelihood/rebuild behavior and galaxy-context determinism, and synced version/docs metadata to the `0.7.6.0` feature slice.
- Human Accepted: Pending review of the new second-pass colony pipeline and its performance in normal runtime use.
- Human Rejected: The alternative post-generation colonization tool flow was explicitly deferred for now; this pass stays on the built-in deterministic pipeline approach first.
- Human Changed: The user narrowed the implementation order to “try the in-pipeline native-pressure approach first, then consider the separate tool only if performance is not good enough.”
- Validation Method: `dotnet build D:\Game Creation\star_gen\StarGen.sln`; `godot-mono --headless --path . --script res://Tests/RunTestsHeadless.gd` (`1953 / 1953` assertions passed; this repo still emits the same pre-existing post-summary Godot .NET popup/unsafe-reference cleanup errors and exits non-zero after completion).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Verify that the realistic life/population fixes actually reach the Galaxy Studio and galaxy-viewer runtime tool path, then close the gap where the deterministic baseline improved but the user still saw no populated worlds in the tool.
- Input Materials Used: User report that `Life Potential = 1` still showed no populated worlds in the tool; `claude.md`; `AGENTS.md`; `StarSystemPreview.cs`; `MainApp.Navigation.cs`; `TestStarSystemPreview.cs`; `TestMainAppNavigation.cs`; `DotNetNativeTestSuite.cs`; version metadata files.
- AI Produced: Traced the runtime generation path, identified that realistic population generation was only auto-enabled for Traveller mode in galaxy previews and systems opened from the galaxy viewer, changed both runtime paths to always generate population, added regression coverage for realistic preview generation and realistic open-system navigation, and synced internal version metadata for the `0.7.5.3` bug-fix slice.
- Human Accepted: Pending review of the runtime-path fix and the restored populated-world behavior in the Galaxy Studio / galaxy-viewer flow.
- Human Rejected: No Traveller population behavior was changed in this pass; the scope stayed on the realistic runtime path.
- Human Changed: The user required a direct answer about whether the fix was actually making it into the tool, which narrowed this pass from baseline validation to runtime-path verification.
- Validation Method: `dotnet build D:\Game Creation\star_gen\StarGen.sln`; `godot-mono --headless --path . --script res://Tests/RunTestsHeadless.gd` (`1947 / 1947` passed; the same pre-existing popup/layout/ObjectDB/RID warnings still print after completion).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Finish the realistic life-generation fix so biosphere generation actually matches the expected low/mid/high baseline behavior instead of merely getting closer.
- Input Materials Used: User bug report and follow-up requirements about realistic biosphere scarcity; `claude.md`; `AGENTS.md`; `BiologySupportEvaluator.cs`; `PopulationProbability.cs`; `PopulationLikelihood.cs`; `PopulationGenerator.cs`; `PlanetEnvironmentProfile.cs`; `ConceptDependencyChainGenerator.cs`; population tests; `LifeDistributionBaselineRunner.cs`; current baseline artifacts; version metadata files.
- AI Produced: Added typed biology-support failure diagnostics, refreshed the realistic baseline runner to report wet-world and support-failure counts, verified that the realistic biosphere gate and native-life expectation path now agree, made the native-life probability ceiling explicit in code, aligned the stale clamp regression with the actual documented ceiling, reran the deterministic baseline, and synced internal version metadata for the `0.7.5.2` bug-fix slice.
- Human Accepted: Pending review of the realistic biosphere fix and the updated deterministic baseline numbers.
- Human Rejected: Traveller population generation remained out of scope; this pass stays on the realistic ruleset path only.
- Human Changed: The user explicitly required continuing until the realistic biosphere results matched expectations rather than accepting a partial improvement.
- Validation Method: `dotnet build D:\Game Creation\star_gen\StarGen.sln`; `godot-mono --headless --path . --script res://Tests/RunTestsHeadless.gd` (`1946 / 1946` passed; the same pre-existing popup/layout/ObjectDB/RID warnings still print after completion); `godot-mono --headless --path . --script res://Tests/Baselines/RunLifeDistributionBaseline.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Fix the realistic life/settlement generation issue where high `Life Potential` and `Expansion Pressure` still produced too few inhabited worlds, then rerun the deterministic distribution baseline to measure the change.
- Input Materials Used: User bug report about realistic-mode inhabited-world scarcity near the home sector; `claude.md`; `AGENTS.md`; `PopulationGenerator.cs`; `PopulationLikelihood.cs`; `PopulationProbability.cs`; population tests; `LifeDistributionBaselineRunner.cs`; current baseline artifacts; version metadata files.
- AI Produced: Normalized realistic auto-population decisions to the persisted population seed, removed the duplicate post-approval colony reroll so approved realistic colony worlds materialize instead of being silently dropped, strengthened colony-generation regression coverage, reran the life-distribution baseline, and synced internal version/provenance metadata for the `0.7.5.1` bug-fix slice.
- Human Accepted: Pending review of the realistic population fix and the updated baseline numbers.
- Human Rejected: Traveller population generation was explicitly left alone in this pass; the scope is limited to the realistic ruleset path and its baseline behavior.
- Human Changed: The user clarified that this work remains part of the internal `0.7` build line and should not be treated as the `0.8` release itself.
- Validation Method: `dotnet build D:\Game Creation\star_gen\StarGen.sln`; `godot-mono --headless --path . --script res://Tests/RunTestsHeadless.gd` (`1945 / 1945` passed; the same pre-existing popup/layout/ObjectDB/RID warnings still print after completion); `godot-mono --headless --path . --script res://Tests/Baselines/RunLifeDistributionBaseline.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Implement the Galaxy Studio sync and Traveller ruleset expansion so the edited galaxy-studio scene, shared wording, deterministic Traveller mainworld takeover, typed Traveller persistence, and Traveller-specific route logic all behave as one integrated 0.7 feature slice.
- Input Materials Used: User implementation plan for Galaxy Studio sync and full Traveller expansion; `claude.md`; `AGENTS.md`; `Docs/TravellerWorldCreation.md`; Galaxy Studio scene/script files; generation-parameter metadata; inspector/viewer surfaces; Traveller world-generation code; system/jump-lane generation and persistence paths; related unit and integration tests; version/project-structure/provenance files.
- AI Produced: Rebound Galaxy Studio to the edited scene and updated its summary/tooltip behavior; standardized `Realistic`, `Traveller`, and `Expansion Pressure` wording across shared presentation helpers and UI surfaces; added typed Traveller system/trade-code/route models; made Traveller mode deterministically select and rewrite one authoritative mainworld with Traveller-generated UWP-facing values; persisted that Traveller profile through system and route serialization; added a Traveller-only route calculator using the `2 pc per jump number` rule; updated inspectors/viewers to read the typed Traveller profile; and expanded unit/integration coverage for wording, Traveller determinism, routes, and persistence.
- Human Accepted: Pending review of the 0.7 Traveller expansion slice and the resulting Galaxy Studio / system / galaxy route behavior.
- Human Rejected: No public `0.8.0.0` release was assumed, and non-mainworld bodies were not rewritten into full Traveller generation in this slice; they remain on the realistic path except for compatible readout/state backfill where required.
- Human Changed: The user clarified during implementation that all of this work remains part of the internal `0.7` build line and should not be treated as the `0.8` public release.
- Validation Method: `dotnet build D:\Game Creation\star_gen\StarGen.sln`; `godot-mono --headless --path . --script res://Tests/RunTestsHeadless.gd` (`1944 / 1944` passed; the same pre-existing headless popup/layout leak warnings still print after completion).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Finish the Galaxy Studio scene-first refactor by giving it a dedicated `GalaxyGenerationScreen` identity instead of leaving the runtime and tests wired through the legacy `WelcomeScreen` name.
- Input Materials Used: User report that a `galaxyStudioScreen` did not appear to exist; `claude.md`; `AGENTS.md`; `src/app/MainApp.cs`; `src/app/MainApp.Navigation.cs`; `src/app/MainApp.GdCompat.cs`; `src/app/GalaxyGenerationScreen.cs/.tscn`; the legacy `src/app/WelcomeScreen.cs/.tscn`; `Tests/Integration/TestMainAppNavigation.cs`; `Tests/Integration/TestWelcomeScreen.cs`; `Tests/Framework/DotNetNativeTestSuite.Integration.cs`; version/project-structure/source docs.
- AI Produced: Renamed the active Galaxy Studio scene/controller flow to `GalaxyGenerationScreen`, updated `MainApp` navigation and GDScript-compatible accessors to load and expose that scene directly, migrated the integration coverage to `TestGalaxyGenerationScreen`, removed the obsolete `WelcomeScreen` scene/script pair from the active UI layer, and synced version/project-structure/source metadata to the new naming.
- Human Accepted: Pending review of the dedicated Galaxy Studio screen identity and the removal of the misleading `WelcomeScreen` runtime ownership.
- Human Rejected: No broader redesign of the Galaxy Studio itself was attempted in this pass; the scope is limited to making the screen actually exist as a first-class scene/controller and keeping the existing UI behavior intact.
- Human Changed: The user-set requirement is that if a screen exists conceptually in the app, it should also exist concretely in the Godot scene/code structure instead of hiding behind legacy naming.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1935 / 1935` passed; the same pre-existing fallback-dialog, layout, and ObjectDB/RID warnings still print afterward).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Refactor the studio UIs away from runtime-built controls and toward scene-first Godot node structures so stable launch-screen forms are visible and editable in the editor.
- Input Materials Used: User report about generator-first/UI architecture concerns; `claude.md`; `AGENTS.md`; `src/app/WelcomeScreen.cs/.tscn`; `src/app/SystemGenerationScreen.cs/.tscn`; `src/app/ObjectGenerationScreen.cs/.tscn`; `src/app/ObjectGenerationScreen.EnhancedUi.cs`; existing studio integration tests and headless harness.
- AI Produced: Moved the fixed Galaxy Studio generation-rules form, the System Studio parameter form, and the Object Studio parameter shell/sections into their `.tscn` scene trees; rewrote the related C# scripts to cache scene nodes and handle wiring/state instead of creating stable controls at runtime; and synced version/project-structure metadata for the refactor pass.
- Human Accepted: Pending review of the scene-first studio refactor and the remaining UI areas that may still justify hybrid runtime rows.
- Human Rejected: No claim was made that every runtime-generated UI element in the entire app must disappear; data-driven inspectors, validation lists, and similarly variable content remain runtime-built where that is still the appropriate representation.
- Human Changed: The user-set requirement is that if UI can live naturally in Godot's node structure, it should, with the Galaxy Generation Studio being the immediate concrete example.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1935 / 1935` passed; the same pre-existing fallback-dialog/layout/ObjectDB warnings still print afterward).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Correct the galaxy-studio responsive layout after user review showed the three intended columns were still collapsing into stacked rows on normal desktop widths.
- Input Materials Used: User feedback from reviewing Galaxy Generation Studio; `src/app/WelcomeScreen.cs`; `src/app/shared/StudioScreenLayoutHelper.cs`; existing version metadata.
- AI Produced: Lowered the effective compact breakpoint for the three-panel galaxy studio, reduced the wide-mode minimum widths for the settings/rules/summary panels, and kept the welcome screen bound to the new tighter breakpoint so the layout stays in columns unless the window is actually narrow.
- Human Accepted: Pending review of the corrected galaxy-studio layout.
- Human Rejected: No broader redesign of the studio content structure was attempted in this patch; the change is scoped to the responsive layout behavior.
- Human Changed: The user-set requirement is that Galaxy Studio present as three columns, not stacked rows, during normal desktop use.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1935 / 1935` passed; the same pre-existing fallback-dialog/layout/ObjectDB warnings still print afterward).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Tighten the user-facing menu/studio surfaces and correct the baseline comparison so galaxy-studio review feedback is reflected in the running app rather than only in internal tooling.
- Input Materials Used: User review notes and screenshot of Galaxy Generation Studio; `claude.md`; `AGENTS.md`; `MainMenuScreen.cs/.tscn`; `SplashScreen.cs`; `StationStudioScreen.tscn`; `WelcomeScreen.cs/.tscn`; `StudioScreenLayoutHelper.cs`; `PopulationGenerator.cs`; `LifeDistributionBaselineRunner.cs`; existing integration/concept tests; `VERSION.md`; `Docs/ProjectStructure.md`.
- AI Produced: Added a Sources button and user-facing help/credits/release-note copy, ensured splash/menu/studio version labels use the public `0.8.0.0` label, rebuilt Galaxy Studio into three columns with separate parameters and generation-rules panels, removed the misleading galaxy mainworld control, updated tooltips and station placeholder copy, fixed runtime concept-state suppression when native life does not emerge, stabilized the baseline runner to compare the same deterministic world sample across scenarios, and added splash/menu/studio/native-life regression coverage.
- Human Accepted: Pending review of the revised menu copy, the three-column galaxy-studio layout, and the updated baseline artifacts.
- Human Rejected: No claim was made that the current life-distribution tuning is final; the patch corrects the comparison method and concept-state gating, but wider habitability-distribution tuning still remains subject to further human-directed iteration.
- Human Changed: The user-set requirement is that user-facing UI never reference repo files or backend workflow, that the app advertise the next planned public release label rather than the current internal branch version, and that Galaxy Studio separate morphology parameters from generation-rule assumptions.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/Baselines/RunLifeDistributionBaseline.gd`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1934 / 1934` passed; the same pre-existing fallback-dialog/layout/ObjectDB warnings still print afterward).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Tighten the galaxy-studio presentation and parameter semantics so the user-facing UI reflects the next planned release, galaxy controls explain real generation behavior, and life/settlement permissiveness stop acting like cosmetic sliders.
- Input Materials Used: User review notes and screenshot from the galaxy studio; `claude.md`; `AGENTS.md`; `project.godot`; `MainMenuScreen.cs`; `WelcomeScreen.cs/.tscn`; `DarkTheme.tres`; galaxy density-model code; population probability/generator code; integration and population tests; README/version/project-structure/source docs.
- AI Produced: Added a dedicated user-facing version-label helper and project setting, restored the split galaxy-studio layout and summary panel, replaced the advanced-assumptions hover text with an information button, fixed checkbox highlight styling, added Cursor to the in-app credits text, expanded morphology explanations and source references, wired `Life Potential` and `Settlement Density` into native-life and colony generation logic, and added a separate 1000-world baseline runner plus regression coverage.
- Human Accepted: Pending review of the revised galaxy-studio layout, the clarified parameter behavior, and the baseline artifacts.
- Human Rejected: No claim was made that the current galaxy-type model is a full astrophysical formation simulation; the implementation remains a deterministic morphology proxy informed by reviewed structure papers and still subject to human realism review.
- Human Changed: The user-set requirement is that the app always show the next public release label to end users, that Cursor receive explicit credit alongside other AI tools, that galaxy controls become more than window dressing, and that life/settlement distribution changes be measured through a separate baseline run rather than silently folded into the normal suite.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/Baselines/RunLifeDistributionBaseline.gd`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1926 / 1926` passed; the same pre-existing headless popup/layout leak warnings still print afterward).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Implement the 0.7 concept-pipeline hardening pass so concept generation follows deterministic worldbuilding dependencies instead of the earlier flattened showcase context.
- Input Materials Used: User-approved 0.7 hardening plan; `Claude.md`; `AGENTS.md`; concept atlas/runtime code; population and celestial domain models; concept prototypes in `Concepts/`; persistence services; concept and integration tests; roadmap/version/project-structure/readme surfaces.
- AI Produced: Added typed pipeline states for environment, ecology, species evolution, sentience, society, religion, language, and disease; reworked world/population generation to gate downstream concepts on biological and sentient applicability; hardened concept serialization and provenance; removed concept-path ternary operators and silent-fallback behavior; updated deterministic concept tests; and synced roadmap/version/readme/project-structure documentation for the internal `0.7.1.0` hardening line toward a future `0.8.0.0` public release.
- Human Accepted: Pending review and approval of the 0.7 hardening branch after manual inspection of the pipeline behavior and concept quality.
- Human Rejected: No public `0.8.0.0` release, prototype retirement, or bypass of the required human audit for culture-, religion-, language-, civilisation-, and species-adjacent outputs was assumed.
- Human Changed: The user-set requirement is that StarGen behave as a constrained deterministic worldbuilding tool where each concept layer depends on explicit upstream results, and that 0.7 remain an internal iteration line until the user is satisfied.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1921 / 1921` passed); concept-path ternary scan enforced by `DotNetNativeTestSuite.Concepts`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-19 - Codex (GPT-5)

- Task Purpose: Rewrite the repository README into a more user-facing orientation ahead of commit, push, build, and storefront publication.
- Input Materials Used: User request; existing `README.md`; `claude.md`; `AGENTS.md`; `VERSION.md`.
- AI Produced: Replaced the developer-heavy README with a shorter orientation covering what StarGen is, what users can currently do, the current development state, how to run it, how to validate it, and where to find deeper project documentation.
- Human Accepted: Pending review and commit approval.
- Human Rejected: No attempt was made to treat the README as a full technical architecture reference; that detail was intentionally moved behind linked docs instead.
- Human Changed: The user-set requirement is that the README act as an orientation to StarGen for end users rather than a contributor-facing dump of internal project history.
- Validation Method: Pending commit/build/release execution after the README rewrite is finalized.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-19 - Codex (GPT-5)

- Task Purpose: Apply the post-review concept-showcase presentation fixes requested after manual acceptance review.
- Input Materials Used: User feedback and screenshots; `claude.md`; `AGENTS.md`; `src/app/MainMenuScreen.cs/.tscn`; `src/app/concepts/ConceptAtlasScreen.cs`; concept result factory files; integration and concept tests; current version/docs surfaces.
- AI Produced: Moved the Concept Atlas entry into the Generation Studios area, expanded help text to mention the Station Studio, added brief AI-usage credits, added scroll-safe atlas sidebar behavior, normalized concept-facing identifier text into sentence case, and updated related tests and docs.
- Human Accepted: Pending review and commit approval.
- Human Rejected: No claim was made that the concept tools are final or authoritative; the in-app framing continues to mark them as a tool in development.
- Human Changed: The user-set requirement is that the concept tools be presented as realistic, user-adjustable worldbuilding tools in development rather than as completed integrated simulation layers.
- Validation Method: Pending build/test run after the full requested patch set is complete.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-19 - Codex (GPT-5)

- Task Purpose: Apply the post-review system-view and pipeline rollback fixes so Concept Atlas remains standalone for the showcase branch.
- Input Materials Used: User review notes; `claude.md`; `AGENTS.md`; `src/app/system_viewer/*`; `src/app/MainApp.Navigation.cs`; `src/app/viewer/ObjectViewer.cs`; persistence services; `src/domain/galaxy/StarSystemPreview.cs`; related integration tests; current docs/version surfaces.
- AI Produced: Replaced the system-level concept summary with populated-world focus actions, added viewer-side handling to jump selection and camera, removed automatic concept generation hooks from generation/save/load/preview/viewer flows, revised release/docs copy to reflect the standalone-atlas state, and updated persistence/system-viewer tests.
- Human Accepted: Pending review and commit approval.
- Human Rejected: The concept tools were not presented as fully integrated simulation layers, and no release/version bump beyond the retained `0.7.0.0` user-facing label was assumed.
- Human Changed: The user-set requirement is that these be treated as patch follow-ups to the concept showcase branch, with the atlas remaining a standalone tool in development until broader integration is ready.
- Validation Method: Pending full build/test run after the patch set is complete.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Prepare the Release 2 concept-integration milestone and sync the repository/docs surfaces to `0.7.0.0`.
- Input Materials Used: The completed `codex/concept-atlas-fold-in` branch after cross-layer integration; `Docs/Roadmap.md`; `Docs/ProjectStructure.md`; `README.md`; `VERSION.md`; `project.godot`; `src/app/MainMenuScreen.cs`; existing provenance entries.
- AI Produced: Updated branch-level release/version metadata and user-facing release-note surfaces to reflect the completed concept-integration milestone, while preserving the explicit human-audit gate for culture-adjacent outputs.
- Human Accepted: Pending review and final release approval.
- Human Rejected: No public publication, prototype deletion, or audit bypass was assumed in this pass.
- Human Changed: The user-set requirement remains that showcase framing stay present while merge/release authority for cultural outputs remains human-owned.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Implement the Release 2 cross-layer concept integration so folded-in concept outputs persist through generation, save/load, inspectors, histories, and atlas re-entry.
- Input Materials Used: Existing `codex/concept-atlas-fold-in` branch state; concept atlas presenters and registry; world/system/body/population domain models; save/load services; inspector panels; concept fold-in plan and roadmap/docs; headless and C# test harnesses.
- AI Produced: Added persisted concept-result stores and serialization helpers, a shared concept result factory and world-state generator, save/load and generation-pipeline wiring for systems/bodies/populations, inspector/history summaries, atlas persisted-state reuse behavior, and regression coverage for concept persistence and atlas behavior.
- Human Accepted: Pending review with the broader concept fold-in and final release approval.
- Human Rejected: Prototype deletion and public release sign-off were not assumed; culture-, religion-, language-, civilisation-, and species-adjacent outputs still require explicit human audit before merge or release.
- Human Changed: The user-set requirement remains that the showcase emphasis be preserved while concept outputs become normal StarGen state rather than a disposable demo layer.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Prepare the Release 1 Concept Atlas showcase milestone and sync the repository/docs surfaces to `0.6.0.0`.
- Input Materials Used: Current `codex/concept-atlas-fold-in` branch state; `Docs/Roadmap.md`; `Concepts/Additions.md`; `README.md`; `Docs/ProjectStructure.md`; `VERSION.md`; `project.godot`; `src/app/MainMenuScreen.cs`; existing provenance entries.
- AI Produced: Updated roadmap and additions status for the completed showcase surface, synced versioning and release-note surfaces to `0.6.0.0`, and documented the Release 1 milestone as the showcase-ready completion of the selected concept fold-in work.
- Human Accepted: Pending review and final showcase sign-off.
- Human Rejected: No release publication, prototype deletion, or cross-layer persistence claims were added in this pass; those remain separate steps.
- Human Changed: The user-set requirement remains that culture-adjacent outputs are showcase-ready for demonstration but still require explicit human audit before public release approval.
- Validation Method: `dotnet build StarGen.sln`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Add end-user-facing Concept Atlas launch points across the existing galaxy, system, and object inspection surfaces for showcase use.
- Input Materials Used: Existing Concept Atlas shell and module registry; `MainApp*`; galaxy/system/object viewer controllers and inspector panels; concept context builder; integration test harness; current version/docs surfaces.
- AI Produced: Added inspector-driven Concept Atlas entry points, context-aware atlas seeding from galaxy preview/system/body context, return-to-origin atlas navigation, and regression coverage across `MainApp`, `GalaxyViewer`, `SystemViewer`, and `ObjectViewer`.
- Human Accepted: Pending review with the broader concept fold-in and Release 1 showcase milestone.
- Human Rejected: No separate prototype viewer embedding or save-schema expansion was introduced in this slice; the work stayed in the release-1 atlas/navigation layer.
- Human Changed: The user-set requirement remains that cultural, religious, language, civilisation, and species-framing outputs require explicit human audit before merge or release sign-off.
- Validation Method: `dotnet build StarGen.sln`; `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Fold the evolution/species prototype into the Concept Atlas as a deterministic lineage and species-profile tool.
- Input Materials Used: `Concepts/EvoTechTree/` as conceptual reference; shared concept context types; atlas registry and test harness; current docs/version surfaces.
- AI Produced: Added `EvolutionConceptGenerator`, `EvolutionConceptSnapshot`, `EvolutionAtlasModulePresenter`, registry wiring, and deterministic regression coverage for lineage milestones, trait bundles, and species-facing summaries.
- Human Accepted: Pending review with the broader concept fold-in and the required human audit for species-framing outputs before release.
- Human Rejected: No embedded prototype UI or non-deterministic evo-mode simulation loop was used; the fold-in stayed native and deterministic.
- Human Changed: The user-set requirement remains that species-framing outputs require explicit human audit before merge or release.
- Validation Method: `dotnet build StarGen.sln`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Fold the disease simulator concept into the Concept Atlas as a deterministic outbreak-analysis tool seeded from world and population context.
- Input Materials Used: `Concepts/DiseaseSimulator/` as conceptual reference; shared concept context types; atlas registry and test harness; current version/docs surfaces.
- AI Produced: Added `DiseaseConceptGenerator`, `DiseaseConceptSnapshot`, `DiseaseAtlasModulePresenter`, registry wiring, and deterministic regression coverage for outbreak traits, symptoms, and epidemic summary outputs.
- Human Accepted: Pending review with the broader concept fold-in.
- Human Rejected: No embedded prototype charts or web view were used; the fold-in stayed native and deterministic with atlas-facing summaries.
- Human Changed: The user-set requirement remains that concept work stays deterministic and showcase-ready while remaining suitable for later cross-layer integration.
- Validation Method: `dotnet build StarGen.sln`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Fold the conlang prototype into the Concept Atlas as a deterministic phonology/grammar/lexicon tool for showcase use.
- Input Materials Used: `Concepts/ConlangGenerator/` as conceptual reference; shared concept context types; atlas registry and concept test harness; current versioning/docs surfaces.
- AI Produced: Added `LanguageConceptGenerator`, `LanguageConceptSnapshot`, `LanguageAtlasModulePresenter`, registry wiring, and deterministic regression coverage for phonology, grammar, lexicon, and example utterances.
- Human Accepted: Pending review with the broader concept fold-in and the required human audit for language/culture-adjacent outputs before release.
- Human Rejected: No web embedding or full prototype UI port was used; the fold-in stayed native, deterministic, and atlas-oriented.
- Human Changed: The user-set requirement remains that language and related culture-adjacent outputs require explicit human audit before merge or release.
- Validation Method: `dotnet build StarGen.sln`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Fold the civilisation concept into the Concept Atlas as a deterministic polity-history layer grounded in StarGen's population and government data.
- Input Materials Used: `Concepts/CivilisationEngine/` as conceptual reference; StarGen population domain types (`GovernmentType`, `TechnologyLevel`); `ConceptContextSnapshot`; atlas registry and test harness; current documentation/version surfaces.
- AI Produced: Added `CivilizationConceptGenerator`, `CivilizationConceptSnapshot`, `CivilizationAtlasModulePresenter`, registry wiring, and deterministic regression coverage for civilisation summaries spanning regime, economy, culture, and historical trajectory.
- Human Accepted: Pending review with the broader concept fold-in and the required human audit for culture/civilisation-adjacent outputs before release.
- Human Rejected: No embedded prototype web UI or non-deterministic simulation loop was used; the fold-in stays native and deterministic.
- Human Changed: The user-set requirement remains that civilisation and other culture-adjacent outputs are not final authority material and require explicit human audit before merge or release.
- Validation Method: `dotnet build StarGen.sln`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Fold the religion concept into the Concept Atlas as a deterministic, context-aware in-app module for the showcase branch.
- Input Materials Used: `Concepts/ReligionGenerator/` deterministic C# prototype files; `ConceptContextSnapshot`; population regime/technology context types; atlas registry and concept test harness; repo documentation/version surfaces.
- AI Produced: Added `ReligionAtlasModulePresenter`, a religion atlas snapshot model, context-to-belief-parameter mapping, doctrinal/ritual/landscape summaries for atlas presentation, project wiring to compile the prototype generator sources safely, and deterministic regression coverage for the religion presenter.
- Human Accepted: Pending review with the broader concept fold-in and the required human audit for religion/culture-adjacent outputs before release.
- Human Rejected: No web embedding or duplicate prototype UI was used; the fold-in stays native C# + Godot and reuses deterministic prototype domain logic only.
- Human Changed: The user-set requirement remains that religion, language, civilisation, and related cultural outputs require explicit human audit before merge or public release.
- Validation Method: `dotnet build StarGen.sln`.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Begin the Concept Atlas and concept fold-in effort by adding shared concept plumbing, the first in-app atlas shell, and menu/navigation/test scaffolding for upcoming concept migrations.
- Input Materials Used: User-approved fold-in plan; `AGENTS.md`; `claude.md`; `Docs/Roadmap.md`; `Concepts/Additions.md`; `README.md`; `Docs/ProjectStructure.md`; `MainApp*`; `MainMenuScreen*`; existing population/body/system context models; integration tests.
- AI Produced: Added shared concept domain types, `ConceptContextBuilder`, the first `ConceptAtlasScreen` and module registry shell, main-menu atlas entry wiring, initial atlas/manual-input UI, roadmap/additions/readme/project-structure updates, and regression coverage for the new menu/navigation path.
- Human Accepted: Pending review and release approval after the broader concept fold-in is completed.
- Human Rejected: No web-view embedding or non-deterministic shortcut implementation; the shell stayed native C# + Godot and used seeded/manual context plumbing only.
- Human Changed: The user set the two-release structure, the branch strategy, the subtle showcase framing, and the requirement that culture-adjacent outputs remain subject to human audit before release.
- Validation Method: `dotnet build StarGen.sln` and added integration coverage for the new Concept Atlas entry point.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-18 - Codex (GPT-5)

- Task Purpose: Fold the ecology concept into the Concept Atlas as the first deterministic content module.
- Input Materials Used: The new atlas shell on `codex/concept-atlas-fold-in`; Ecology prototype domain files in `Concepts/EcologyGenerator/`; `ConceptContextSnapshot`; roadmap/docs; test harness files.
- AI Produced: Added `EcologyAtlasModulePresenter`, an ecology atlas snapshot model, deterministic context-to-environment mapping, trophic-profile and niche summaries for the atlas surface, and a concept-specific regression in `DotNetNativeTestSuite.Concepts.cs`.
- Human Accepted: Pending review with the broader concept fold-in.
- Human Rejected: No separate web-view or duplicate prototype viewer embedding was used; the fold-in reuses deterministic C# ecology generation and presents it through the atlas surface.
- Human Changed: The user-set requirement remains that concept work stay deterministic, visualized, and suitable for later cross-layer integration rather than becoming a throwaway showcase-only dead end.
- Validation Method: `dotnet build StarGen.sln`.
- Final Approver: Pending Christopher B. Del Gesso review.

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
### 2026-03-20 - Codex (GPT-5)

- Task Purpose: Continue the scene-first refactor by moving the remaining fixed viewer/editor/atlas UI shells out of runtime C# construction and into Godot scene trees.
- Input Materials Used: User direction to move anything that did not need to live in code into node structure; `claude.md`; current viewer/editor/concept scene/script files; `SystemViewer*.cs/.tscn`; `ObjectViewer*.cs/.tscn`; `EditDialog.cs/.tscn`; `ConceptAtlasScreen.cs/.tscn`; `Tests/Framework/DotNetNativeTestSuite.Concepts.cs`; version/project-structure metadata files.
- AI Produced: Replaced runtime-built fixed shells in `SystemViewer`, `ObjectViewer`, `ConceptAtlasScreen`, and `EditDialog` with scene-defined control trees; reduced the related scripts to node binding, option population, signal wiring, and dynamic row rendering only where content is inherently data-driven; updated the atlas regression test to mount the real scene tree; and synced internal refactor metadata to `0.7.3.0`.
- Human Accepted: Pending user review of the broader viewer/editor scene-first pass.
- Human Rejected: No attempt was made to force fully data-driven inspector/property-list content into static scenes; those rows remain runtime-built because they vary by generated body, validation state, or concept output.
- Human Changed: The refactor stayed focused on live runtime screens and did not remove unrelated untracked prototype/baseline artifacts from the working tree.
- Validation Method: `dotnet build StarGen.sln` and the Godot headless harness (`Total: 1935 | Passed: 1935 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-23 - Codex (GPT-5)

- Task Purpose: Improve the native-life pipeline so biosphere support, complexity, and sentience stay deterministic and internally consistent across population generation, concept generation, and preview summaries.
- Input Materials Used: User-provided design brief contrasting rare-earth and space-opera life assumptions; `claude.md`; `Docs/Roadmap.md`; current population and concept pipeline files; `StarSystemPreview*`; population/concept/unit tests; version and provenance docs.
- AI Produced: Replaced the split native-life logic with a unified biosphere assessment in `BiologySupportEvaluator`, propagated stellar age and moon biosphere inputs through planet/environment profiles, made species and sentience depend on the shared assessment, stopped biosphere generation from automatically materializing sentient native populations, added preview-side biosphere/sentient-world counts, added a population-focused headless test path, updated regressions, and synced metadata to `0.7.10.0`.
- Human Accepted: Pending user review of the revised native-life calibration, the stricter biosphere-versus-sentience separation, and the preview-summary changes.
- Human Rejected: No attempt was made to turn AI output into final scientific authority; alternative-biochemistry calibration remains a deterministic game-model approximation that still requires human review against source materials before merge or release.
- Human Changed: Kept the work inside the existing population and concept dependency pipeline seams instead of introducing a separate prototype, and adapted preview tests to track biosphere-bearing worlds directly rather than using inhabited-world counts as a proxy for life.
- Validation Method: `dotnet build StarGen.sln` and the Godot headless harness (`Total: 1962 | Passed: 1962 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-26 - Codex (GPT-5)

- Task Purpose: Finalize the `0.8.0.0` release merge on `master`, review the merged payload, correct export packaging so shipped builds exclude archived local release folders, and prepare the Windows/Linux release artifacts for git and itch publication.
- Input Materials Used: User-approved release plan and follow-up corrections, local `master` merge state, `export_presets.cfg`, `VERSION.md`, `README.md`, `AI-Provenance-Log.md`, Godot export presets, the local release directory structure, and the itch target `jazhikho/stargen`.
- AI Produced: Reviewed the merged release payload, found that Godot exports were embedding repository `release/` folders, patched all export presets to exclude `release/*` and `release/**`, rebuilt the Windows and Linux release artifacts with the corrected filter, updated release notes to document the packaging fix, and prepared artifacts/notes for push and itch publication.
- Human Accepted: Pending user review of the final `0.8.0.0` release payload and the published builds.
- Human Rejected: Did not treat AI as the final release authority; the release proceeded only from the user-approved plan and user-supplied itch project target.
- Human Changed: User clarified that `test.tscn` should be deleted, Concept Atlas should remain menu-scoped and hidden elsewhere, only failing tests should be rerun rather than the full suite, and the music attribution is substantively correct but should simply be presented neatly in credits.
- Validation Method: `dotnet build StarGen.sln`, release export verification for Windows and Linux, and merged-diff review against `origin/master`; no additional full-suite rerun was performed after the user's instruction to avoid it.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-27 - Codex (GPT-5)

- Task Purpose: Push the v0.9 scene-first UI requirement further by moving shipped inspector/editor presentation and fixed UI settings out of runtime C# and into `.tscn` scenes/templates.
- Input Materials Used: User instruction to aggressively favor `.tscn` over `.cs` for UI structure and settings; `claude.md`; `Docs/V0.9Plan.md`; shipped viewer/studio scenes and scripts under `src/app/`; `Tests/Integration/TestObjectViewerMoons.cs`; build and headless harness outputs.
- AI Produced: Added reusable scene-authored UI templates for sections, property rows, message labels, action buttons, labeled input rows, and edit-dialog rows; rewired `GalaxyInspectorPanel`, `SystemInspectorPanel`, `InspectorPanel`, and `EditDialog` to consume those templates instead of constructing fixed UI directly in C#; moved the fixed galaxy/system inspector shells into the viewer `.tscn` files; moved several static viewer empty-state and tooltip settings into scene files; updated generation screens to use scene-authored message labels; and removed one brittle moon-button UI assertion from the integration suite in line with the branch policy to discard UI-surface tests.
- Human Accepted: Pending user review of the scene-first refactor breadth and the updated viewer/editor presentation.
- Human Rejected: No attempt was made to force inherently variable property lists or body-specific editor inputs into a single static scene tree; those remain runtime-populated but now use scene-authored row templates where practical.
- Human Changed: The refactor was kept on the shipped mainline screens and did not try to revive parked concept runtime paths or reintroduce removed persistence UI.
- Validation Method: `dotnet build StarGen.sln` and `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1774 | Passed: 1774 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-03-27 - Codex (GPT-5)

- Task Purpose: Complete a second scene-first audit pass for the shipped mainline UI, removing leftover static scene defaults and generic presentation helpers from C# and reviewing the remaining app-layer UI code for misses.
- Input Materials Used: User instruction to continue the `.tscn`-first pass and explicitly review for anything missed; `claude.md`; current `src/app/` viewer/studio scenes and scripts; `Docs/ProjectStructure.md`; `Docs/V0.9Plan.md`; prior scene-template refactor state; build and headless harness outputs.
- AI Produced: Added scene-authored templates for subheaders, dividers, edit sections, edit derived-property rows, and edit validation messages; rewired `SystemInspectorPanel`, `GalaxyInspectorPanel`, `EditDialog`, and `SystemViewer.Parameters` to use templates or existing scene content instead of creating generic controls in code; deleted dead galaxy-inspector UI-construction helpers; moved remaining fixed defaults such as min-star labels, star-count limits, include-belts defaults, viewer zone visibility, and system-studio assumption tooltip text into `.tscn` files; removed redundant startup-state assignments from `SystemGenerationScreen`, `StationStudioScreen`, and `ObjectGenerationScreen.EnhancedUi`; and performed a grep-based review of shipped app scripts to confirm that remaining C# UI writes are predominantly behavioral or state-driven.
- Human Accepted: Pending user review of the second-pass cleanup and the residual review assessment.
- Human Rejected: No attempt was made to move inherently stateful runtime messages, dynamic constraint-bound slider ranges, or generated-content strings out of code when those values depend on current data or interaction state.
- Human Changed: The user explicitly raised the bar from an implementation pass to an implementation-plus-review pass, which drove the second audit and cleanup sweep after the first template migration.
- Validation Method: `dotnet build StarGen.sln` and `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1774 | Passed: 1774 | Failed: 0`); additional grep review across shipped `src/app/` scripts excluding parked concepts/prototypes.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-03 - Codex (GPT-5)

- Task Purpose: Research and implement non-cosmetic generation improvements for planet demographics and atmospheres, moon and small-body placement, and biosphere or habitability refinement so aggregate planetary assumptions materially constrain downstream outputs.
- Input Materials Used: User direction to research the first, second, and fifth next-step science areas; `claude.md`; `Sources/Texts/planets.md`; current planetary/system/population generator files; `PlanetarySystemState.cs`; `SystemPlanetGenerator.cs`; `PlanetAtmosphereGenerator.cs`; `SystemMoonGenerator.cs`; `SystemAsteroidGenerator.cs`; `ProfileGenerator.cs`; `BiologySupportEvaluator.cs`; affected unit and population tests; and reviewed source notes/metadata for Fulton et al. (2017), Fischer & Valenti (2005), Canup & Ward (2006), DeMeo & Carry (2014), Lamy et al. (2004), Kopparapu et al. (2014), and Heller & Barnes (2013).
- AI Produced: Extended the derived planetary-system state with habitable-zone, stellar-flux, XUV, volatile-delivery, bombardment, and outer-reservoir scalars; threaded those fields through system planet generation, atmosphere retention, moon architecture, asteroid-belt placement, and profile/biology evaluation; added new population-side XUV and tidal-heating regressions; authored reviewed source-note files for the research used in this pass; and updated roadmap, version, bibliography, and project-structure documentation for the `0.8.7.0` feature slice.
- Human Accepted: Pending user review of the new scientific calibration and the resulting downstream behavior shifts in planet, moon, small-body, and biosphere generation.
- Human Rejected: Did not turn the reviewed papers into a full formation or life simulation, and did not surface aggregate formation controls in Object Studio after the user had explicitly reserved those controls for Galaxy/System scope only.
- Human Changed: The user explicitly narrowed the requested research to the first, second, and fifth follow-on areas and required that any added science be important to other generated items or impose real constraints, which drove the focus on flux, XUV, volatile delivery, moon architecture, belt composition, and biosphere gating instead of cosmetic labels.
- Validation Method: `dotnet build StarGen.sln` and `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1796 | Passed: 1796 | Failed: 0`; Godot still emitted its known shutdown leak/finalizer crash after the green run).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Bring Object Studio up to the same science-surface standard as Galaxy and System Studio by adding direct planet-local life controls, a help popup, smaller viewport-safe help dialogs, and source-backed conflict notes for contradictory direct settings.
- Input Materials Used: User report about missing Object Studio life/help surfaces and oversized help popups; `claude.md`; `AGENTS.md`; current `ObjectGenerationScreen` partials and scene; `GenerationUseCaseSettings`; `LifeScienceReferenceCatalog`; `ObjectGenerationParameterCatalog`; `PlanetSpec`; `PlanetAtmosphereGenerator`; `PlanetSurfaceGenerator`; existing `TestStudioScienceUi` integration coverage; and reviewed/online source material for Wordsworth & Kreidberg (2022).
- AI Produced: Added `ObjectGenerationScreen.Help.cs`, `ObjectGenerationScreen.Life.cs`, and `ObjectScienceReferenceCatalog.cs`; extended `ObjectGenerationScreen.tscn` with a header Help button, a scene-owned help dialog, and a planet-local life section; introduced `HelpDialogLayoutHelper.cs` and applied it to Galaxy, System, and Object help popups; added direct-setting conflict notes tied to reviewed sources; added `Sources/Texts/WordsworthKreidberg2022.txt`; expanded integration coverage for Object Studio help, life controls, popup sizing, and science conflict notes; and updated version/project-structure metadata for `0.8.13.0`.
- Human Accepted: Pending user review of the Object Studio help flow, popup sizing, direct life controls, and conflict-note wording.
- Human Rejected: No attempt was made to turn Object Studio into an aggregate scientific-priors surface; aggregate formation controls remain in Galaxy and System Studio only, while Object Studio remains a direct-authoring tool with advisory science notes.
- Human Changed: The user clarified that Object Studio needed source-backed notes about contradictory direct settings rather than aggregate science controls, and that help popups across the studios needed to be shrunk so the close button stayed visible.
- Validation Method: `dotnet build StarGen.sln` and `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1813 | Passed: 1813 | Failed: 0`). One earlier headless attempt hit Godot's intermittent native finalizer crash before completion; the immediate rerun was clean.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Add a generation override that forces native life to appear on worlds that pass the biology support gate, while keeping unsupported worlds lifeless.
- Input Materials Used: User request for a force-life setting; `claude.md`; `AGENTS.md`; `GenerationUseCaseSettings`; `PopulationLikelihood`; `BiologySupportEvaluator`; Galaxy/System/Object studio scenes and controllers; integration and population test suites; and version/provenance metadata files.
- AI Produced: Added `ForceLifeOnSupportableWorlds` to `GenerationUseCaseSettings` serialization, wired the override into `PopulationLikelihood.ShouldGenerateNatives(...)`, surfaced the control in Galaxy Studio, System Studio, and Object Studio, added studio integration coverage for the new control, added a population unit test proving the override respects the biology support gate, and updated internal version metadata to `0.8.14.0`.
- Human Accepted: Pending user review of the new force-life override behavior and studio placement.
- Human Rejected: No attempt was made to classify the override as a science-backed life model; it remains documented as a generation override, not an academic assumption.
- Human Changed: The user explicitly constrained the feature so worlds that cannot support life must remain lifeless, which drove the implementation to sit after the support gate instead of bypassing it.
- Validation Method: `dotnet build StarGen.sln` and `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1814 | Passed: 1814 | Failed: 0`).
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Add a staged regression that forces a Solar-System-like generation chain through the live system generators and fails loudly at the exact stage where Solar compatibility breaks.
- Input Materials Used: User request for a Solar System forcing test; `claude.md`; `AGENTS.md`; `Tests/Unit/TestSystemGoldenMasters.cs`; `Tests/Unit/TestSystemPlanetGenerator.cs`; `src/domain/system/fixtures/SystemFixtureGenerator.cs`; `src/domain/system/OrbitSlotGenerator.cs`; `src/domain/generation/specs/StarSpec.cs`; `src/domain/generation/specs/PlanetSpec.cs`; `src/domain/generation/generators/StarGenerator.cs`; `src/domain/generation/generators/PlanetGenerator.cs`; `src/domain/generation/generators/planet/PlanetPhysicalGenerator.cs`; `src/domain/generation/generators/planet/PlanetSurfaceGenerator.cs`; `src/domain/generation/generators/planet/PlanetAtmosphereGenerator.cs`; and the native test-suite registration in `Tests/Framework/DotNetNativeTestSuite.cs`.
- AI Produced: Added `Tests/Unit/TestSolarSystemReferenceChain.cs`, a staged Solar reference-chain regression that preflights a Sun analogue, matches solar-compatible orbit slots, validates per-planet candidate worlds against Solar-target ranges, then locks exact Solar orbital and physical values only after each stage remains in range; registered the test in the native suite; and updated version/project-structure metadata for `0.8.14.1`.
- Human Accepted: Pending user review of the staged Solar reference-chain contract and its failure wording.
- Human Rejected: Did not add Solar-specific branches or overrides to production generation code; the forcing logic is test-only and uses the existing override pathways already honored by the live generators.
- Human Changed: The user specified that each generation step should only lock exact Solar values after the expected Solar-compatible value remains in range and that failures must point directly to the broken stage.
- Validation Method: `dotnet build StarGen.sln` and `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1815 | Passed: 1815 | Failed: 0`). Godot still emitted the repo's known shutdown leak warnings after the green run.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-19 - Codex (GPT-5)

- Task Purpose: Add reviewed ruleset and licensing source notes for the RPG systems that currently have the clearest path to built-in compatibility overrides, then record the planned override effort in the roadmap without implementing the generator changes yet.
- Input Materials Used: User request to pull the rule sets into the repo and sketch how they would override generation; `claude.md`; `AGENTS.md`; current `GenerationUseCaseSettings` and studio override wiring; official licensing and rules pages for Mongoose Traveller, Cepheus Engine SRD, Tomkin Press Starforged, and Paizo Starfinder; `Sources/AnnotatedBibliography.md`; `Docs/Roadmap.md`; `Docs/ProjectStructure.md`; `VERSION.md`; `project.godot`; and `README.md`.
- AI Produced: Added reviewed source notes for Traveller licensing, Cepheus Engine SRD world generation, Starforged licensing and free-primer material, and Starfinder compatibility/playtest material; added a new roadmap effort for RPG compatibility overrides; updated bibliography and project-structure tracking; and synced internal version metadata to `0.8.14.2`.
- Human Accepted: Pending user review of the selected systems, the clean-room licensing boundaries, and the override-design sketch.
- Human Rejected: Did not pull in Stars Without Number or GURPS material because the current reviewed rights basis is not strong enough for a built-in override path; did not implement override code in this pass.
- Human Changed: The user narrowed the work from a general licensing discussion to putting the defendable ruleset basis into the repo first and then sketching how the overrides should map onto the existing main-program generation seam.
- Validation Method: `dotnet build StarGen.sln` (build passed with 0 warnings and 0 errors). User review of the selected systems and override design is still pending.
- Final Approver: Pending Christopher B. Del Gesso review.

### 2026-04-21 - Codex (GPT-5)

- Task Purpose: Fix the viewer-side compact camera panels so they behave like true bottom-right roll-up controls, move viewer menu shells into `.tscn` scenes to better match the engine-first UI standard, align Galaxy/System studio override checkbox rows, and record the remaining viewer `.tscn` migration gaps in a tracked audit.
- Input Materials Used: User direction to relabel the camera boxes as `Controls`, make them expand upward, bring the active viewers closer to the Galaxy Viewer and Studio standard, and assess what still needs to move from code into `.tscn`; `AGENTS.md`; repo `claude.md`; `GalaxyViewerCSharp.tscn`; `SystemViewer.tscn`; `ObjectViewer.tscn`; `GalaxyViewer.Menu.cs`; `SystemViewer.Menu.cs`; `ObjectViewer.Menu.cs`; `GalaxyViewer.LocalSpace.cs`; `SystemViewer.Options.cs`; `ObjectViewer.Options.cs`; `GalaxyGenerationScreen.tscn`; `SystemGenerationScreen.tscn`; `GalaxyGenerationScreen.cs`; `SystemGenerationScreen.cs`; `Tests/Framework/DotNetNativeTestSuite.App.cs`; and `Tests/Integration/TestStudioScienceUi.cs`.
- AI Produced: Converted the top-level viewer menu button shells for Galaxy, System, and Object viewers from runtime-created controls to scene-owned nodes; reversed all three compact control panels so their content appears above the toggle and renamed the toggle text to `Controls`; aligned Galaxy Studio and System Studio override toggles into labeled row layouts; added `Docs/ViewerSceneOwnershipAudit.md` documenting the remaining engine-first gaps; updated the relevant integration and app-level tests; and synced version/project-structure metadata for `0.8.18.5`.
- Human Accepted: Pending user review of the updated viewer control panels, scene-owned viewer menus, and the `.tscn` ownership audit.
- Human Rejected: No attempt was made in this patch to fully remove the embedded generation/editor sections from System Viewer and Object Viewer; those remain called out as the next high-priority engine-first migration rather than being folded into this bugfix checkpoint.
- Human Changed: The user clarified that the compact camera box should be labeled `Controls`, should expand upward instead of downward, and that the request should include an explicit assessment of what still violates the engine-first `.tscn` approach.
- Validation Method: `dotnet build StarGen.sln` and `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`Total: 1835 | Passed: 1835 | Failed: 0`; Godot still emitted its known shutdown leak warnings after the green run).
- Final Approver: Pending Christopher B. Del Gesso review.
