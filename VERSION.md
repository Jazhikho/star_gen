# Version

Current version: `0.9.0.0`

Current user-facing version: `0.9`

Date: `2026-04-22`

Versioning method: release/refactor `+0.1`, feature `+0.0.1`, bug fix `+0.0.0.1`, save-breaking release `+1.0`.

## 0.9.0.0

- Release: Promote the current generation-and-view baseline to the approved `0.9` release line and merge the release-hardening work back into the primary branch.
- Release: Sync release-facing metadata and UI labels to plain `0.9`, retire the old `0.9d` display suffix, and keep the Windows export metadata and artifact paths aligned to `0.9.0.0`.
- Bug fix: Keep the studio help dialogs fully within the usable viewport after opening, so the final on-screen rectangle leaves the Close button reachable at smaller selected resolutions.
- Bug fix: Align Galaxy Viewer local-space system caching with the real galaxy-aware system-generation pipeline so built local systems match actual planet, moon, and population outputs.
- Test: Verified the release baseline with `dotnet build StarGen.sln` and `godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd` (`1856 / 1856`).

## 0.8.21.4

- Bug fix: Tightened studio help-dialog clamping so the final on-screen window rectangle, not just the requested content size, stays inside the usable viewport after Godot opens the modal window.
- Bug fix: Help dialogs now reserve frame and title-bar space, set a hard `MaxSize`, and reapply the clamp after opening so the Close button stays reachable at smaller selected resolutions.
- Test: Strengthened the studio help-dialog regression to assert the popup's right and bottom edges remain inside the viewport, not only that its raw size stays under 70%.

## 0.8.21.3

- Bug fix: Rewired Galaxy Viewer local-space system building to use the same galaxy-aware `GalaxySystemGenerator` path as preview and open-system flows, so cached local systems now match the actual planet and moon outputs instead of using the older fixture shortcut.
- Bug fix: Local-space builds now seed the galaxy-level full-system cache, and preview or open-system generation now consults that shared cache before regenerating, keeping local-space summaries aligned with the systems the app later opens.
- Test: Added a galaxy-viewer regression that builds local space, asserts the galaxy-level system cache is populated, and verifies a cached system matches direct galaxy-aware generation for planet count, moon count, and total population.

## 0.8.21.2

- Bug fix: Clamped all studio help dialogs to at most 70% of the active viewport size through the shared `HelpDialogLayoutHelper`, and marked the scene-authored help windows as unresizable so the close controls stay reachable at smaller selected resolutions.
- Test: Tightened the studio help-popup integration tests so Galaxy, System, and Object studio dialogs are validated against the live viewport-relative 70% cap instead of a hard-coded pixel ceiling.

## 0.8.21.1

- Bug fix: Synced the remaining release-facing metadata so `README.md`, `export_presets.cfg`, and the scene-authored version placeholders in the splash, main menu, and station studio no longer advertise stale `0.8.0.0` or `0.9.0.0` labels.
- Bug fix: Tightened the export presets so shipped artifacts exclude non-runtime repo folders such as `Tests/`, `Docs/`, `Sources/`, and parked `Concepts/` content instead of bundling those into release packages.
- Feature: Added a Windows-hosted release helper script at `scripts/CreateReleaseBuild.ps1` that runs the normal build and headless gates, exports release presets into a versioned `release/` folder, archives them, and prints suggested itch upload commands.
- Docs: Added a dedicated `0.9` release checklist, a live acceptance checklist for exported builds, and a concrete `1.0` checklist so the mainline release and post-build review path are explicitly documented.
- Refactor: Split the shared generation parameter surface into explicit `Generation prior`, `Generator override`, `Runtime/orchestration control`, and `Presentation/readout control` classes, keeping runtime and readout controls out of the shared generation catalog while preserving them as auxiliary editor definitions.
- Refactor: Added a machine-readable `ParameterMaterialityRegistry` plus the new [Parameter materiality audit](Docs/ParameterMaterialityAudit.md), documenting how retained parameters and derived planetary-state fields actually flow from studios into generation, provenance, and viewer readouts.
- Bug fix: Population permissiveness baseline resolution now follows the active `LifeFramework`, and deserialization now restores the framework-recommended baseline when no explicit permissiveness override was stored.
- Test: Added catalog-partition and materiality-entry regressions, updated studio UI expectations for readout/override wording, and the full headless harness passed (`1856 / 1856`) after `dotnet build StarGen.sln` succeeded.

## 0.8.21.0

- Feature: Added academically grounded circumstellar habitable-zone model selection to the shared `PlanetaryGenerationProfile`, with `Kasting 1993`, `Kopparapu 2013 (Conservative)`, and `Kopparapu 2013 (Optimistic)` now exposed in both Galaxy Generation Studio and System Generation Studio.
- Feature: Unified the actual HZ pipeline so orbit hosts, orbit-zone classification, derived planetary system state, and downstream environment-profile generation now all use the selected habitable-zone model instead of mixing the updated Kopparapu path with older fixed `0.95 / 1.37 * sqrt(L)` logic.
- Docs: Added reviewed source notes for Kasting 1993 and Kopparapu 2013 and expanded the scientific parameter audit to track the new HZ parameter as a science-backed control.
- Test: Added habitable-zone model regressions for orbital mechanics and stellar helpers, extended planetary-profile serialization coverage to the new model enum, and the headless harness passed (`1853 / 1853`) after the HZ parameter retrofit.

## 0.8.20.1

- Feature: Added a neutral `SentientWorldProfile` baseline for inhabited planets and moons so population-bearing worlds now carry settlement pattern, settlement rank, logistics capacity, dominant regime, and structural governance or law or technology axes before any RPG compatibility adapter compresses them into ruleset-specific outputs.
- Feature: Object Viewer population inspection now hides the population block unless a body actually has active inhabitants, and when inhabited it surfaces the new sentient-world baseline fields alongside the existing population summary.
- Docs: Added [Sentient world baseline grounding notes](Docs/SentientWorldBaseline.md), linked the RPG compatibility audit to the implemented baseline, and updated project-structure tracking and provenance for the new population model.
- Test: Added focused sentient-world baseline derivation and serialization regressions plus object-viewer inspector checks for hidden uninhabited population sections and visible inhabited baseline fields; `dotnet build StarGen.sln` passed and the headless harness passed (`1849 / 1849`) before Godot emitted its known shutdown leak warnings.

## 0.8.20.0

- Feature: Reworked the life-support and sentient-world evaluator so abiogenesis, protected biospheres, surface biospheres, oxygenation, detectability, nutrient access, climatic variability, host-star desiccation risk, and prebiotic UV opportunity are modeled as explicit channels instead of being collapsed into a single habitability proxy.
- Test: Added focused population regressions for nutrient accessibility, icy-moon protected biospheres, high-XUV desiccation suppression, detectability separation, and the updated population-likelihood expectations; the targeted population harness passed (`649 / 649`).
- Docs: Added [RPG compatibility generation audit](Docs/RpgCompatibilityGenerationAudit.md), expanded the bibliography and review tracker with sentient-world governance, law, technology-diffusion, and cumulative-culture sources, and documented the current gap between StarGen's physical realism and its still-heuristic social-output layer.
- Test: `dotnet build StarGen.sln` passed after the life-model tightening and again after the compatibility-audit documentation pass.

## 0.8.19.0

- Feature: Tightened the aggregate planetary-generation surrogates so disk lifetime and solids reservoir respond to host mass, giant-planet formation now peaks near the snow line rather than rising monotonically with distance, compact inner architectures react more directly to migration plus solids context, and inner volatile delivery now responds to giant-driven scattering.
- Test: Added focused `PlanetarySystemState` and `SystemPlanetGenerator` regressions for host-mass disk or solids priors, snow-line giant-weight turnover, and migration-shaped compact inner architectures, and registered them in the native test suite.
- Docs: Added a dedicated [Life science audit](Docs/LifeScienceAudit.md) plus new reviewed life-source notes and bibliography updates covering M-dwarf desiccation and abiotic oxygen, prebiotic UV constraints, nutrient access, ocean productivity, and biosignature context so the current life-model limitations and tightening path are explicit.
- Test: `dotnet build StarGen.sln` passed. A full Godot headless harness run was attempted but hit an existing Godot-side `0xC0000005` finalization crash after hundreds of passing tests, before the new targeted planet tests were reached.

## 0.8.18.16

- Test: Reworked `TestSolarSystemRealization` so it now starts from galaxy inputs (`GalaxyConfig`, `Galaxy`, `GalaxyStar`, and the home galactic position) and exercises the real `GalaxySystemGenerator.GenerateSystem(...)` path instead of a direct hand-built system spec.
- Test: Expanded the dedicated Solar realization suite to cover use-case settings, stellar profile models, and planetary profile models, with staged diagnostics that report where realization fails (`galaxy_config`, `galaxy_context`, `system_parameters`, `stellar_scaffold`, `system_generation`, `system_validation`, or Solar-shape mismatches).
- Test: `dotnet build StarGen.sln` passed. The isolated Solar realization suite itself was not run because the user explicitly asked not to run any harness beyond compile verification until further instruction.

## 0.8.18.17

- Test: Replaced the broken exact-realization rewrite of `TestSolarSystemRealization` with a probabilistic Solar-analog suite focused on the scientific-assumption families only: Sun-like stellar scaffold frequency from galactic context, Solar-like rocky and giant planet analog frequency under a Sun-like scaffold, and Earth-like plus giant-planet moon-channel expectations.
- Test: Updated the dedicated Solar-only native manifest so the isolated harness now runs the new probabilistic Solar-analog checks instead of the stale exact-realization method names.
- Test: `dotnet build StarGen.sln` passed. The isolated Solar realization suite is being run separately from the default harness.

## 0.8.18.11

- Bug fix: Galaxy, System, and Object viewer `Controls` panels now cache the scene-authored expanded size at startup and reuse that stable footprint on the first and later opens, eliminating the remaining first-click geometry jump.
- Bug fix: Galaxy Viewer `Controls` text now reflects the active camera mode again, restoring the correct local subsector instructions instead of showing the earlier mixed orbit or pan copy.
- Test: `dotnet build StarGen.sln` passed, and the headless harness passed (`1837 / 1837`) before Godot emitted its known shutdown leak warnings.

## 0.8.18.12

- Bug fix: Galaxy Viewer now refreshes its expanded `Controls` footprint whenever the help copy changes, so the panel stays anchored correctly instead of dropping too low after the runtime text switches to the active mode-specific instructions.
- Bug fix: Galaxy Viewer local-view movement help now spells out `forward`, `backward`, `left`, `right`, `up`, and `down` instead of assuming the `W / A / S / D / E / C` bindings are self-explanatory, and the scene-owned help block is wider to fit that copy cleanly.
- Test: `dotnet build StarGen.sln` passed, and the headless harness passed (`1837 / 1837`) before Godot emitted its known shutdown leak warnings.

## 0.8.18.13

- Bug fix: Galaxy Viewer `Controls` now restores the editor-authored expanded panel rectangle directly from `GalaxyViewerCSharp.tscn` instead of recalculating that open state in script, bringing the box back under scene ownership and eliminating the mispositioned expanded state.
- Bug fix: The Galaxy Viewer expanded controls panel was resized in the scene to fit the explicit movement text while keeping the compact collapsed corner behavior handled by the toggle script.
- Test: `dotnet build StarGen.sln` passed, and the headless harness passed (`1837 / 1837`) before Godot emitted its known shutdown leak warnings.

## 0.8.18.14

- Bug fix: System Viewer and Object Viewer `Controls` now use the same scene-owned expanded panel layout pattern as Galaxy Viewer, restoring the editor-authored open rectangle directly from their `.tscn` scenes instead of recalculating expanded geometry in script.
- Bug fix: System Viewer and Object Viewer control text is now explicit and accurate to the real inputs, including system-view middle-drag orbit plus view-angle toggle, and object-view moon selection plus primary-body focus wording.
- Test: `dotnet build StarGen.sln` passed, and the headless harness passed (`1837 / 1837`) before Godot emitted its known shutdown leak warnings.

## 0.8.18.15

- Test: Added a dedicated `TestSolarSystemRealization` suite that searches the real top-level `SystemFixtureGenerator.GenerateSystem(...)` path for a Solar-reference scaffold instead of relying on the older manually assembled reference-chain test.
- Test: Split the Solar realization coverage into its own native suite manifest and dedicated headless entrypoint so it can be run in isolation without invoking the rest of the headless harness.
- Test: `dotnet build StarGen.sln` passed. The dedicated Solar realization suite was not run yet because the user explicitly asked not to run the other harnesses until further instruction.

## 0.8.18.10

- Bug fix: Galaxy, System, and Object viewer `Controls` panels now reopen from the correct bottom-right anchor on every click without reusing the stale first-open geometry, so the panel no longer jumps into the wrong place after the initial expand.
- Bug fix: Object Viewer side-panel `File Operations` stays hidden again on the `0.9d` line, matching the current restriction that export and save flows remain disabled in this release channel.
- Test: `dotnet build StarGen.sln` passed, and the headless harness passed (`1837 / 1837`) before Godot emitted its known shutdown leak warnings.

## 0.8.18.9

- Bug fix: Galaxy, System, and Object viewer `Controls` panels now expand from a wider scene-owned help block with shorter scene-owned copy, so the opened panel no longer turns into the narrow, over-tall instruction slab shown in the viewer screenshots.
- Bug fix: The active `ObjectViewer.tscn` now visibly owns the file block, inspector navigation button, edit button, and inspector section shells directly in the editor, while the inspector script only clears editor placeholders, toggles section visibility, and fills dynamic moon buttons and property rows at runtime.
- Docs: Updated `Docs/ViewerSceneOwnershipAudit.md` to reflect the Object Viewer inspector-shell migration and narrow the remaining engine-first gap to truly data-driven rows plus file-status glue.
- Test: `dotnet build StarGen.sln` passed, and the headless harness passed (`1837 / 1837`) before Godot emitted its known shutdown leak warnings.

## 0.8.18.8

- Bug fix: Galaxy, System, and Object viewer `Controls` panels now size themselves from the actual header and help-content minimums, so the collapsed panel hugs the `> Controls` label more closely and the expanded panel fits the full help text and bottom `^ Controls` toggle without clipping.
- Bug fix: Removed the duplicate inspector-script attachment from the outer Object Viewer side-panel container and kept the dedicated `InspectorPanel` node as the only active inspector controller, bringing the runtime Object Viewer scene closer to the engine-visible `.tscn` ownership model.
- Bug fix: Object Viewer inspector UWP sections now stay hidden by default, appear only when explicitly enabled through the carried use-case settings or legacy Traveller provenance, and no longer leak the removed legacy `World Profile` or `Generation Targets` sections.
- Bug fix: `Advanced Controls` in Object Generation Studio now lives under `Generation Overrides` in the active scene instead of reopening inside the `Parameters` column.
- Test: Strengthened Object Viewer and studio regressions to prove the duplicate outer inspector script is gone, UWP sections only appear when enabled, the removed legacy sections stay gone, and `Advanced Controls` remains under `Generation Overrides`; `dotnet build StarGen.sln` passed and the headless harness passed (`1837 / 1837`).

## 0.8.18.7

- Bug fix: Galaxy, System, and Object viewer `Controls` panels now collapse down to a compact bottom-right box sized around the `> Controls` label instead of reserving the full expanded panel footprint while hidden.
- Bug fix: Expanding a viewer `Controls` panel now slides the panel upward into place, keeps the content clipped during motion, and uses `^ Controls` as the expanded bottom toggle; collapsing reverses the same slide animation back to the compact corner box.
- Test: Expanded the viewer regressions so Galaxy, System, and Object viewer control panels now prove the collapsed box is compact and that pressing the toggle expands the panel, reveals the content, and changes the label to `^ Controls`.

## 0.8.18.6

- Bug fix: Removed the embedded generation and save/load editor stack from the active `SystemViewer.tscn`, so the runtime System Viewer now matches the engine scene and no longer carries a second hidden copy of System Studio.
- Bug fix: Simplified the System Viewer controller so it treats generation specs as studio-owned input, keeps only viewer tools in the top menu, and stops binding runtime behavior to deleted viewer-side generator controls.
- Docs: Updated `Docs/ViewerSceneOwnershipAudit.md` to mark the System Viewer generator-strip pass complete and narrow the remaining engine-first migration work to inspector row templating and the Object Viewer cleanup.
- Test: Added a viewer-scene regression proving the active System Viewer no longer mounts the old generation or save/load sections; `dotnet build StarGen.sln` passed, and the headless harness passed (`1835 / 1835`) before Godot emitted its known shutdown leak noise.

## 0.8.18.5

- Bug fix: Galaxy, System, and Object viewer camera panels are now labeled `Controls`, expand upward from the bottom-right corner, and use scene-owned panel ordering so the toggle sits below the revealed controls instead of above them.
- Bug fix: Galaxy, System, and Object viewer top menus now use scene-owned button shells instead of creating the menu chrome in code at runtime, reducing viewer-side UI construction and making the active scenes align better with the engine-first `.tscn` standard.
- Bug fix: Galaxy Studio and System Studio now use labeled override rows for `Show UWP Code` and `Force Life If Supportable`, keeping those checkboxes aligned on the same side as the rest of the override controls.
- Docs: Added `Docs/ViewerSceneOwnershipAudit.md` to track the remaining `.tscn` migration gaps, with System Viewer and Object Viewer generation panels called out as the highest-priority engine-first cleanup still outstanding.
- Test: `dotnet build StarGen.sln` passed, and the full headless harness passed (`1835 / 1835`) before Godot emitted its known shutdown leak noise.

## 0.8.18.4

- Bug fix: Viewer options dialogs in Galaxy, System, Object, and Main Menu now persist the `Show all studio seeds` and `Skip intro on startup` preferences through the explicit `Apply` button and close cleanly from both the titlebar `X` and the explicit `Close` button.
- Bug fix: Galaxy, System, and Object viewer windows now share the same compact camera-panel pattern, and unchecked checkboxes use explicit white bordered glyphs so they remain visible across the app.
- Bug fix: System Viewer selection now behaves like a preview-and-focus panel instead of duplicating object-detail output, with clickable stars and orbit entries, numbered asteroid belts, compact selected-body summaries, and no bogus system name or orbit-host dump.
- Bug fix: System Studio removed the duplicate population toggle, Object Studio keeps `Advanced Controls` and other override checkboxes in `Generation Overrides` with the same checkbox-side presentation as System Studio, and Galaxy Studio now uses the same visible checkbox style for override toggles.
- Test: `dotnet build StarGen.sln` passed, and the headless harness reported `1835 / 1835` passing before Godot emitted its known mono shutdown leak noise.

## 0.8.18.3

- Bug fix: Galaxy Viewer options now labels the seed toggle as `Show all studio seeds`, matching the actual preference behavior instead of implying a narrower studio-only surface.
- Bug fix: Galaxy Viewer options can now be closed from the window `X` as well as the explicit `Close` button, and the dialog open path now works safely in both runtime and off-tree test contexts.
- Bug fix: Main Menu options now uses the same `Show all studio seeds` wording and supports closing its window from the titlebar close affordance as well as the explicit button.
- Test: Added Galaxy Viewer regression coverage for the renamed seed toggle, the updated options status wording, and the options-dialog open/close contract; also hardened the shared intro-audio test so it resets persisted intro-skip preferences before asserting splash behavior.

## 0.8.18.2

- Bug fix: System Studio help now opens within a tighter shared viewport clamp so the close button stays reachable, and the same smaller modal sizing is reused by the Galaxy and Object studio help popups.
- Bug fix: System Studio now mirrors the Galaxy Studio science-panel standard more closely, including a `Scientific Assumptions` title and section-level source `(i)` buttons for system, stellar, planetary, and life controls.
- Bug fix: Checkboxes and check buttons now use compact transparent theme chrome with visible default checkbox glyphs instead of large filled toggle backgrounds, making unchecked boxes readable across the UI.
- Test: Added theme and System Studio UI regressions for the compact checkbox styling, source buttons, and clamped help popup behavior.

## 0.8.18.1

- Bug fix: Galaxy Viewer local-space caches now persist when the viewer moves outside the last built area, instead of behaving like the cache vanished as soon as the current position left the cached bounds.
- Bug fix: Rebuilding local space now appends newly covered systems into the existing temporary cache instead of replacing the old region, so adjacent builds grow one aggregate local-space working set for later tools.
- Test: Added a Galaxy Viewer regression that moves outside the first cached area, proves the cache survives, then rebuilds and verifies the second local-space pass appends coverage and restores jump-route availability.

## 0.8.18.0

- Feature: Galaxy Viewer now reports live `Quadrant`, `Sector`, and `Local` coordinates in the overview block, and selected stars now add full hierarchical system coordinates to `System Preview`.
- Feature: Replaced the old `View`/`Window` menu split with `Tools` and `Options`, added a viewer-local options dialog, and introduced `Build Local Space` so lazy nearby systems can be explicitly profiled and cached before follow-up tools run.
- Feature: Local-space caching now builds and retains system summaries per nearby star, including population, tech, government, trade-code, habitability, resource, and desirability data through the existing jump-lane region payload.
- Test: Added viewer-scene regression coverage for the new menu layout and local-space cache build path, plus formatter coverage for sector-aware overview coordinates and hierarchical system-coordinate formatting; `dotnet build StarGen.sln` passed, and the headless harness passed (`1830 / 1830`).

## 0.8.17.4

- Bug fix: Reworked the Galaxy View inspector into a live overview block that tracks the active location continuously, uses the selected system position only when a system is selected, reports the galaxy type instead of sector/star type, and keeps all system-specific details in the `System Preview` section.
- Bug fix: Removed the unused top-level galaxy seed control, `Show Compass` control, and compass viewport from the live Galaxy Viewer scene and menu so the inspector only shows the requested overview and preview information.
- Test: Expanded the Galaxy Viewer regression coverage to assert the old top-level seed and compass controls are gone and that the live inspector block is labeled `Overview`.

## 0.8.17.3

- Bug fix: Removed the jump-route tools section from the Galaxy View inspector so the panel is limited to the requested selection and preview information instead of mixing in route controls.

## 0.8.17.2

- Bug fix: Hid the legacy `Active Profile` and `Overview` sections in the Galaxy View inspector so the panel now surfaces the trimmed selection and preview information directly instead of burying it below profile data.
- Test: Added a viewer-level regression proving the Galaxy inspector keeps the legacy profile and overview sections hidden.

## 0.8.17.1

- Bug fix: Reduced the Galaxy View inspector selection readout to the requested essentials by replacing raw parsec XYZ and extra preview detail with type, seed, current quadrant, subsector-local grid position, home-relative polar angle, inclination, distance from core, and a compact system preview summary.
- Bug fix: Current location in the Galaxy View inspector now resolves from the active local view context instead of echoing stale raw selection metadata, while the selected system `Local XYZ` now displays subsector-grid coordinates clamped to the expected `0..9` range on each axis.
- Test: Added unit coverage for the inspector selection formatter so quadrant context, subsector-grid mapping, home-relative polar readouts, and parsec-vs-kiloparsec formatting stay stable.

## 0.8.17.0

- Feature: Fully wired the fixed `Cepheus`, `Starfinder`, and `Starforged` compatibility profiles into real generation behavior instead of leaving them as near-cosmetic variations on the same small bias set.
- Feature: Colony generation now uses profile-specific harsh-world settlement tolerance and profile-specific colony-type weighting, so `Cepheus` leans toward classic civil settlements, `Starfinder` leans toward scientific/corporate/industrial harsh-world footholds, and `Starforged` leans toward frontier outposts, scientific footholds, and refugee/separatist colonies.
- Feature: Mainworld candidate shaping now carries profile-specific hydrosphere leanings, letting `Starfinder` favor wetter focal worlds and `Starforged` favor drier frontier-leaning focal worlds without changing readout/export scope.
- Test: Added contract tests proving the non-Space-Opera profiles now diverge in harsh-world colony tolerance, colony-type distribution, and system-level orbit-fill behavior; `dotnet build StarGen.sln` passed, and the headless harness passed (`1825 / 1825`).

## 0.8.16.0

- Feature: Exposed the actual `Space Opera` compatibility levers in Galaxy Studio and System Studio so users can tune the same generation pressures the RPG override profile applies instead of being locked to the baked defaults.
- Feature: Added persisted override fields for mainworld policy, temperate-world bias, harsh-world bias, mainworld-class bias, native-life bias, and settlement bias, and wired those fields through the shared compatibility-profile contract used by system and population generation.
- Feature: Tightened the Space Opera override UI so the controls stay hidden outside the `Space Opera` profile, use finer slider resolution, and keep the default realistic path untouched when no RPG override is active.
- Test: Expanded the compatibility-profile tests to prove ruleset defaults, studio wiring, custom override serialization, and custom slot-fill or population-pressure effects; `dotnet build StarGen.sln` passed, and the headless harness passed (`1820 / 1820`).

## 0.8.15.0

- Feature: Added clean-room RPG compatibility scaffolding for `Space Opera`, `Cepheus`, `Starfinder`, and `Starforged`, including shared profile resolution, presentation labels, and settings defaults that can be reused across Galaxy Studio, System Studio, and Object Studio.
- Feature: System generation now materially responds to the selected compatibility profile by biasing mainworld targeting, temperate or harsh slot fill pressure, terrestrial-world weighting, and population pressure instead of treating the ruleset selector as UI-only state.
- Feature: Population generation and UWP-like handling now flow through the shared compatibility profile contract so compatibility-oriented modes can force population generation where appropriate and reuse the same guarded UWP-style path without expanding output or export scope in `0.9d`.
- Test: Added compatibility-profile integration and unit coverage for studio option wiring, default application, system fill pressure, and population-probability shifts; `dotnet build StarGen.sln` passed, and the headless harness passed (`1818 / 1818`).

## 0.8.14.2

- Docs: Added reviewed source notes for Traveller licensing, Cepheus Engine SRD world generation, Starforged licensing and free primer material, and Starfinder compatibility or playtest material so the repo now has a defended source basis for future RPG override work.
- Docs: Added an explicit `RPG compatibility overrides` planned effort to the roadmap and documented the first-wave clean-room candidates and licensing boundaries.
- Docs: Updated the bibliography and project-structure docs so the new override-supporting source notes are tracked with the rest of the reviewed material.

## 0.8.14.1

- Bug fix: Added a staged Solar-System reference-chain regression that uses the live star, orbit-slot, and planet generators to preflight Sun and the eight major planets, then locks exact Solar values only after each stage remains within the expected Solar-compatible range.
- Bug fix: The new regression fails loudly by stage (`stellar preflight`, `orbital slot`, and per-planet `candidate`/`lock` stages) so generator drift now points directly to the broken link instead of failing later as a vague count mismatch.
- Test: Registered the Solar reference-chain regression in the native headless suite and revalidated with `dotnet build StarGen.sln` plus the full Godot headless harness.

## 0.8.14.0

- Feature: Added a `Force Life On Supportable Worlds` generation override to Galaxy Studio, System Studio, and Object Studio so supportable worlds can be forced to retain native life without bypassing the underlying biology support gate.
- Feature: Wired the override through `GenerationUseCaseSettings` serialization and the deterministic native-life pipeline so it materially changes generation instead of only changing UI state.
- Test: Added settings round-trip coverage, studio integration coverage proving each surface writes the override through, and a population unit test proving the override admits supportable worlds while still rejecting unsupported ones; `dotnet build StarGen.sln` passed, and the headless harness passed (`1814 / 1814`).

## 0.8.13.0

- Feature: Object Studio now exposes planet-local life settings using the same sourced `Life Framework`, `Abiogenesis Model`, `Complex Life Model`, `Civilization Model`, and `Environmental Window Weight` stack already used upstream, but scoped to one directly authored planet instead of aggregate galaxy or system behavior.
- Feature: Added a scene-owned Help popup to Object Studio and introduced a shared help-dialog layout helper so the Galaxy, System, and Object help popups stay small enough for the close controls to remain visible on tighter windows.
- Feature: Object Studio now emits source-backed conflict notes for direct planet-setting combinations that fight known atmospheric, hydrosphere, and civilization-stage constraints, including airless or stripped worlds paired with dense-ocean targets and oxygen-bottleneck civilization assumptions on airless worlds.
- Docs: Added `Wordsworth & Kreidberg (2022)` to the reviewed source set and wired the new Object Studio conflict and help surfaces to the relevant academic sources.
- Test: `dotnet build StarGen.sln` passed, and the headless harness passed (`1813 / 1813`). One earlier headless attempt hit Godot’s intermittent native finalizer crash before completion; the immediate rerun was clean.

## 0.8.12.0

- Feature: System Studio now follows the same audited science-control contract as Galaxy Studio for life settings, replacing the old coarse life slider with the sourced `Life Framework`, `Abiogenesis Model`, `Complex Life Model`, `Civilization Model`, and `Environmental Window Weight` stack while keeping those controls in the left scientific-assumptions column.
- Feature: Object Studio now follows the direct-control standard explicitly: aggregate life/population sliders are gone, `Generation Overrides` only holds ruleset-facing controls, and direct planet, star, asteroid, and comet controls now use a shared object-parameter catalog with plain-language tooltips tied to what those controls actually change.
- Feature: Added request-level coverage proving Object Studio direct planet controls survive into `PlanetSpec`, while System Studio and Object Studio integration tests now enforce the `Generation Overrides` naming, `Show UWP Code` wording, context-sensitive surfaces, and absence of unsupported aggregate controls.
- Test: `dotnet build StarGen.sln` passed, and the headless harness passed (`1812 / 1812`).

## 0.8.10.0

- Feature: Galaxy Studio now keeps all scientific assumptions in the left column and renames the center column to `Generation Overrides`, so life modeling and other science-facing controls no longer sit in the RPG override area.
- Feature: Replaced the single Galaxy Studio life selector with a source-aligned stack for `Life Framework`, `Abiogenesis Model`, `Complex Life Model`, `Civilization Model`, and `Environmental Window Weight`, with separate downstream tuning for biospheres, complex life, sentience, and technological civilizations.
- Feature: Replaced the vague `Earth History` wording with `Earth-Anchored Composite`, documented it as an explicit synthesis of the reviewed life papers, and moved oxygen or technosphere bottlenecks to the civilization stage instead of treating them as early-life assumptions.
- Test: Added legacy-compatibility, Galaxy Studio UI, and life-model behavior regressions for the new split life pipeline; `dotnet build StarGen.sln` passed, and the headless harness passed (`1803 / 1803`).

## 0.8.11.0

- Feature: Added `Docs/ScientificParameterAudit.md` as the tracked audit matrix for the shipped galaxy, stellar, planetary, and life science controls, mapping each parameter to its scientific claim, cited sources, consuming generator code, expected outcome change, coverage status, and disposition.
- Feature: Tightened science-source coverage across Galaxy Studio by adding reviewed citations for arm count, pitch, arm-mechanism framing, and ellipticity, and narrowed generator-facing wording where controls are still bounded StarGen tuning layered on top of the literature-backed model rather than direct laws of nature.
- Feature: Finished the life-stage separation on the shipped population path so civilization-stage assumptions no longer directly suppress sentience chance, and summary generation now distinguishes sentient-but-non-technological outcomes from technological civilizations.
- Test: Upgraded source-resolution and materiality coverage for galaxy, stellar, planetary, and life settings, including new directional tests for morphology, multiplicity, rogue-world bias wording, and life-stage separation; `dotnet build StarGen.sln` passed, and the headless harness passed (`1812 / 1812`).

## 0.8.9.0

- Feature: Intro startup now respects a persisted `Skip intro on startup` option, splash clicks and key presses trigger the skip path reliably, intro audio resolves through the shared app audio controller again, and main-menu credits now include the Patreon thanks for Leo.
- Feature: Galaxy Studio now labels the center column `Generation Overrides`, renames the RPG-facing ruleset to `Space Opera`, renames the readout toggle to `Show UWP Code`, and moves life modeling into the left column as a sourced selector instead of a scalar slider.
- Feature: Added research-backed life-potential models (`Earth-Anchored Composite`, `Rapid Biospheres`, `Environmental Windows`, `Rare Complex Life`) and threaded them into biology-support, complex-life, and sentience gating so the choice now materially changes biosphere and civilization outcomes.
- Test: Added intro-preference, Galaxy Studio life-model, and life-model unit regressions; `dotnet build StarGen.sln`, Godot solution rebuild, and the full headless harness passed after the patch.

## 0.8.8.0

- Feature: Removed the legacy planetary mass-radius option from Galaxy Studio and System Studio, made Chen-Kipping the default supported model, and added Otegi as a second cited mass-radius model for rocky versus volatile-rich transition worlds.
- Feature: Planet physical generation now applies the selected mass-radius model in the actual pipeline instead of only carrying the choice as metadata, with compatibility mapping so older saved `Legacy` values resolve to Chen-Kipping.
- Feature: Reworked planetary-prior help text and citations so Gas Loss, Giant Growth, Metal Link, Rogue Worlds, Moon Bias, and Outer Debris now point to explicit model families or observed trends instead of legacy StarGen notes.
- Test: Added compatibility, mass-radius model, and fallback regressions; `dotnet build StarGen.sln` passed, and the headless harness passed (`1800 / 1800`) after a Godot solution rebuild.

## 0.8.7.2

- Bug fix: Replaced the splash screen's root-directory `.ogg` scan with a shared app-level audio controller and exported audio library resource, so intro music is now a declared dependency that exports with desktop builds instead of relying on runtime file discovery.
- Bug fix: `MainApp` now owns reusable music and UI audio players plus a shared cue library, giving the project one extendable place to add later menu, UI, or ambient sounds without reintroducing screen-local audio hacks.
- Test: Added shared-audio integration coverage and revalidated with `dotnet build StarGen.sln` plus the headless harness.

## 0.8.7.1

- Bug fix: Galaxy Studio section headings now expose hoverable `(i)` source buttons for Galaxy Type, Scientific Priors, Structure, Size and Density, Stellar Priors, and Planetary Priors, with tooltip text listing the cited sources relevant to each section.
- Bug fix: Reused the existing galaxy, stellar, and planetary science-reference catalogs for the new section-level source tooltips so citation wiring stays centralized and testable instead of duplicating source strings in the scene code.
- Test: Added galaxy-studio integration coverage for the new section source buttons and tooltip content; `dotnet build StarGen.sln` and the headless harness passed after the patch.

## 0.8.7.0

- Feature: Enriched `PlanetarySystemState` with habitable-zone, stellar-flux, XUV, volatile-delivery, bombardment, and outer-reservoir fields so aggregate planetary assumptions now impose real downstream constraints instead of stopping at planet-class weighting.
- Feature: Retrofitted system-planet and atmosphere generation so hot inner worlds, volatile-rich temperate worlds, and outer-system ice-rich worlds now branch differently in class bias, envelope retention, hydrosphere tendency, and rocky-atmosphere outcomes based on aggregate formation context.
- Feature: Moon and small-body generation now react to upstream state, with regular-vs-captured moon tendencies, giant-host moon richness, outer-belt mass/composition, and icy primitive reservoir bias all driven by the same deterministic system context.
- Feature: Population/environment profiles now carry stellar flux, habitable-zone alignment, and XUV exposure, and the biology-support evaluator uses those together with bounded tidal-heating effects so biosphere support and native-life likelihood follow system context without becoming a simulation.
- Test: Added and expanded deterministic regressions for planetary-state serialization, hot-world atmosphere stripping, volatile-delivery effects, moon-bias changes, outer-belt composition shifts, profile/environment field propagation, and XUV/tidal-heating biology behavior; `dotnet build StarGen.sln` passed, and the headless harness passed (`1796 / 1796`).

## 0.8.6.0

- Feature: Added a shared aggregate `PlanetaryGenerationProfile` and derived `PlanetarySystemState` so galaxy and system generation now carry deterministic planetary-formation assumptions such as mass-radius model, envelope-loss model, gas-giant formation emphasis, metallicity coupling, rogue-planet allowance, moon-formation bias, and outer-system small-body bias without rewriting the existing planet generator spine.
- Feature: Retrofitted galaxy and system generation to consume the new aggregate planetary state upstream, letting system-level planet outcomes respond to shared formation context and storing formation-trace metadata on generated planets for later inspection and provenance.
- Feature: Galaxy Studio and System Studio now expose the same aggregate planetary model controls with plain-language help, while Object Studio stays limited to direct single-planet controls such as orbit mode, class bias, composition bias, envelope override, volatile richness, hydrosphere tendency, and moon-bundle settings.
- Test: Added deterministic serialization, propagation, help-catalog, and planet-generation regressions for the new planetary retrofit, and revalidated with `dotnet build StarGen.sln` plus the headless harness (`1789 / 1789` passed).

## 0.8.5.1

- Bug fix: Planet moon generation in Object Studio now supports a context-sensitive target moon-count selector instead of a simple yes/no moon toggle, with the final count capped by the generated planet size so small worlds cannot request giant-planet moon counts.
- Bug fix: Object Studio now carries the moon-count target through to viewer generation, producing multiple linked moons deterministically when the target and planet size allow it.
- Bug fix: Updated the shared dark theme so unchecked checkboxes remain visibly boxed instead of disappearing against the background unless they were already checked.
- Test: Expanded Object Studio and moon-generation regressions around the new moon target-count control and size-based moon-count resolution; `dotnet build StarGen.sln` passed, and the headless harness passed (`1780 / 1780`).

## 0.8.5.0

- Feature: Reworked Object Studio into a context-sensitive single-object flow so only the controls and presets for the selected object type stay visible, while Traveller-only controls now live under Generation Rules and only appear when the Traveller ruleset is active.
- Feature: Removed standalone moon generation from the Object Studio top-level type picker and moved moon generation under planets, including a planet-only moon checkbox and context-sensitive captured-moon option.
- Feature: Added comet support as a real generated body type, with comet specs, generator logic, rendering/display handling, save/load metadata, viewer support, and top-level Object Studio controls and presets.
- Feature: Expanded Object Studio asteroid and planet authoring with richer shaping controls, including broader asteroid taxonomy plus orbit-band, density, and albedo tuning, and broader planet profile overrides such as pressure, ocean/ice coverage, albedo, volcanism, and optional moon generation.
- Feature: Simplified Object Studio star editing so users directly choose subclass while metallicity and age remain generator-resolved instead of exposing partially coupled fields that were not behaving as reliable direct controls.
- Test: Added and updated deterministic and non-visual Object Studio/comet coverage; `dotnet build StarGen.sln` passed, and the headless harness passed (`1780 / 1780`).

## 0.8.4.0

- Feature: Expanded the stellar generator beyond main-sequence-only output so it now supports practical brown dwarfs (`L/T/Y`), evolved stars (subgiants, giants, supergiants), white dwarfs, and stronger hierarchical multi-star generation.
- Feature: Recalibrated stellar mass sampling and stellar lookup tables around a research-backed substellar floor, practical white-dwarf progenitor handling, broader stellar color mapping, and downstream coeval-companion behavior in standalone system generation.
- Feature: Added reviewed source notes under `Sources/Texts/` for white dwarfs, brown dwarfs, and multiplicity work, and updated the in-app/help-facing stellar science text to reflect the broader stellar offerings.
- Test: Added and updated unit/statistical coverage for brown-dwarf classes, evolved/white-dwarf generation, expanded spectral helpers/colors, richer hierarchy behavior, and the revised stellar distribution benchmarks; `dotnet build StarGen.sln` passed, and the headless harness passed (`1776 / 1776`).

## 0.8.3.2

- Bug fix: Converted the science-facing tooltips in the galaxy and system studios to multiline text so each sentence appears on its own line, making the brief help easier to scan.
- Bug fix: Kept the user-facing release line at `0.9d` while syncing the internal patch metadata to `0.8.3.2`.
- Test: `dotnet build StarGen.sln` passed after the tooltip-formatting patch.

## 0.8.3.1

- Bug fix: Removed `Docs/galactic_formation.md` from the in-app science/help source lists so the help surfaces now cite only the external research sources that back the current galaxy and stellar controls.
- Bug fix: Reworked the galaxy help popup into a cleaner scene-owned guide window with stronger section formatting, clearer scrolling/close affordances, and plain-language explanations that focus on what changing each setting will do in practice.
- Bug fix: Added the same header-level `Help` popup flow to System Studio and expanded the system and stellar help/tooltips so non-experts get actionable explanations for star count, spectral hints, metallicity, IMF choices, and multiplicity.
- Test: Added plain-language content assertions for the galaxy and stellar help catalogs and updated the studio help integration checks; `dotnet build StarGen.sln` passed, and the mainline headless harness passed (`1768 / 1768`).

## 0.8.3.0

- Feature: Replaced the galaxy studio's inline science section with a scene-owned `Help` popup in the header, including scrollable plain-language explanations, close controls, and source-backed citations for the galactic generator model.
- Feature: Rewrote galaxy science tooltips and help text for non-experts so key terms such as halo mass, environment density, barred galaxies, arm modes, metallicity gradient, GHZ, and star formation efficiency are defined in simple language before explaining their effect.
- Feature: Added a shared stellar-generation profile that now flows from galaxy generation into system and star generation, with deterministic IMF selection, metallicity/age-sensitive variation, empirical multiplicity tuning, and lightweight MIST/PARSEC-inspired stellar property lookup.
- Feature: Galaxy Studio and System Studio now both expose the stellar science controls for IMF form, IMF variation mode, isochrone model, and multiplicity scale, while system-generation star-count controls now support the full `1..10` app limit instead of the old artificial `4`-star UI cap.
- Test: Added stellar-profile, help-surface, and scientific propagation coverage; `dotnet build StarGen.sln` passed, and the mainline headless harness passed (`1765 / 1765`).

## 0.8.2.0

- Feature: Rebuilt galaxy-tier generation around a scientifically grounded realism profile with explicit galaxy families, subtype and bar selection, halo-mass and environment priors, GHZ and metallicity-gradient fields, and literature-backed defaults that remain documented as tunable priors rather than universal laws.
- Feature: Galaxy-origin science now propagates downstream into generated stars and systems through structured origin context, allowing stellar age, metallicity, and related generation biases to follow the authoritative galaxy-level model instead of independent fallback heuristics.
- Feature: Galaxy Studio now exposes the new science-facing controls and a scene-owned in-app science panel with tooltip summaries, assumption text, and source citations for the galactic generator model.
- Test: Replaced the older generic galaxy-behavior coverage with deterministic and propagation-focused scientific regressions across galaxy config, galaxy-origin context, galaxy-to-system handoff, star generation biasing, and science-reference metadata; `dotnet build StarGen.sln` and the mainline headless harness passed (`1756 / 1756`).

## 0.8.1.0

- Feature: Checkpointed the current mainline branch state around the `0.8.1.0` feature update so the ongoing branch split, scene-first UI migration, version-channel plumbing, and mainline scope reductions are preserved in git while the user-facing release line remains `0.9d`.
- Feature: Pushed the shipped UI further toward `.tscn` ownership by moving inspector/editor shells, reusable rows, validation labels, and fixed defaults/tooltips into scene resources and shared UI templates.

## 0.9.0.0 (Reserved Release Target)

- Release target: Reserve the first approved `0.9` mainline release for the generation-and-view workflow only, with branch-aware user-facing version composition resolving to `0.9.0.0d` on mainline and `0.9.0.0e` on the export branch.
- Release target: Keep Concept Atlas entry points and other concept-summary runtime surfaces out of the approved `0.9` mainline app flow; concept-heavy systems remain parked in-repo for later migration, while `Concepts/Additions.md` stays the StarGen-only prototype backlog.
- Release target: Keep mainline load/save/export entry points removed from studios, viewers, and the object edit dialog so the eventual public `0.9` line does not expose persistence behavior.
- Release target: Keep the rewritten mainline harness focused on deterministic generation, realism, provenance, and non-UI integration coverage, without reviving concept-harness registration or mainline persistence/save-load suites.
- Release target: Keep the obsolete app-facing Concept Atlas runtime files, old jump-lane and station prototype scenes/scripts, unreferenced legacy viewer/body-node scene variants, and the stale duplicate `Concepts/` audio asset out of the approved `0.9` release line.

## 0.8.0.0

- Release: Promoted the concept-pipeline hardening branch to the public `0.8.0.0` release baseline and collapsed internal/docs/project/user-facing version surfaces back into one shipped version.
- Bug fix: Export presets now exclude the repository `release/` directories so Windows and Linux release packages do not embed archived local build outputs.
- Feature: Startup now uses the root intro video and optional root `.ogg` music hook, cross-fades directly into the StarGen logo, and then fades to black before returning on the main menu.
- Feature: Station Studio now runs the production station-generation flow with live summaries and detail views instead of the older placeholder shell.
- Feature: The Concept Atlas remains available from the main menu, while non-menu launch points were removed so Atlas access stays menu-scoped.
- Feature: Native-life and concept generation now follow the deterministic environment-to-ecology-to-species-to-sentience chain that this branch hardened across the domain, previews, and inspectors.
- Test: `dotnet build StarGen.sln` passed, and the Godot headless harness passed cleanly (`1962 / 1962`) in the last full validation run for this release prep.

## 0.7.10.1

- Bug fix: Splash startup now plays the root `stargen.ogv` inside a scene-defined centered `512 px` media frame, so the video shares the logo height and resolves directly into the StarGen icon instead of stretching across the full screen.
- Bug fix: Splash wiring now keeps the video-to-logo transition and the intro-audio hook local to the splash, auto-loading the single root `.ogg` track when exactly one music file is present.
- Test: Updated startup regressions for splash media wiring, shared media-frame sizing, root music loading, and the initial `MainApp` splash state; `dotnet build StarGen.sln` passed, and the full Godot headless harness reached `1962 / 1963` passing with one remaining unrelated object-viewer Concept Atlas failure.

## 0.7.10.0

- Feature: Native life now runs through a single deterministic biosphere assessment that selects a viable chemistry, scores biosphere suitability, derives abiogenesis odds, and separates complex-life and sentience potential instead of letting the ecology gate, native-likelihood curve, and native-population materialization drift apart.
- Feature: Planet and environment profiles now carry stellar age and moon-specific biosphere inputs through the concept pipeline, allowing subsurface ocean moons to remain valid biosphere candidates while keeping sentient life materially rarer than mere biospheres.
- Feature: Star-system previews now distinguish life-bearing worlds from inhabited worlds by tracking biosphere and sentient-world counts separately, so high `Life Potential` can surface native biospheres without implying automatic sentient populations.
- Test: Added and updated population, concept-pipeline, and preview regressions for low-human-habitability biospheres, non-sentient biosphere worlds, alternative-chemistry permissiveness, subsurface moons, and preview biosphere counting; revalidated with `dotnet build StarGen.sln` and the full Godot headless harness (`Total: 1962 | Passed: 1962 | Failed: 0`).

## 0.7.9.5

- Bug fix: Galaxy, System, and Object studios now keep panel minimum widths scene-owned at `200 px`, remove child label minimums that were forcing the rules/summary columns wider, and stop the shared helper from overriding studio sizing in code.
- Test: Updated the studio layout regressions to assert scene-owned panel widths and reran `dotnet build StarGen.sln` plus the full Godot headless harness; the studio UI tests now pass and the remaining 6 failures are still the repo's unrelated population/concept cases.

## 0.7.9.4

- Bug fix: Galaxy, System, and Object studio `MainPanel` content now uses `18 px` left/right inset, matching the existing top/bottom shell spacing so the border reads even on all four sides.
- Test: `dotnet build StarGen.sln` passes after the scene spacing correction; the full headless harness still carries the repo's existing unrelated population/concept failures.

## 0.7.9.3

- Bug fix: Reversed the prior studio-shell spacing change and tightened the generator-studio `MainPanel` left/right inset to `10 px`, pulling the first and last content panels back toward the main-menu shell look instead of padding them farther inward.
- Test: `dotnet build StarGen.sln` passes; the full headless harness still carries the repo's existing unrelated population/concept failures, and the updated studio inset regression source is on disk alongside this spacing correction.

## 0.7.9.2

- Bug fix: Galaxy, System, and Object studio main panels now use an even wider `32 px` scene-defined left/right inset so the first and last studio columns pull inward and stop reading as edge-flush.
- Test: Revalidated the current galaxy-studio scene paths and inset assertion; `dotnet build StarGen.sln` passes, and the full Godot headless harness remains blocked only by the same unrelated population/concept failures.

## 0.7.9.1

- Bug fix: Galaxy, System, and Object studio main panels now use a wider scene-defined left/right inset so the column layout sits off the screen edges more like the main menu shell.
- Test: Updated the galaxy-studio integration test to use the current scene structure and assert the widened panel inset; `dotnet build StarGen.sln` passes, and the full Godot headless harness is now down to the same six unrelated population/concept failures (`1953 / 1959` passed).

## 0.7.9.0

- Refactor: `MainMenuScreen` utility dialogs now live in `MainMenuScreen.tscn` as scene-owned window nodes, so the static menu UI shell is editor-visible and the script only binds content, options state, and visibility.
- Test: `dotnet build StarGen.sln` succeeds cleanly; the full Godot headless harness still reports pre-existing unrelated failures in population, concept-pipeline, and galaxy-generation suites (`1948 / 1958` passed), while the updated main-menu dialog regression now passes.

## 0.7.8.2

- Feature: Concept Atlas is no longer launched from the galaxy viewer or system viewer; it remains available from the main menu and from the object viewer inspector.

## 0.7.8.1

- Feature: System view camera follows the selected body while it moves on its orbit, preserving zoom and orbit angles until the user pans, orbits, resets the view, or changes selection; `SystemCameraController` exposes follow state and integration tests cover follow motion and reset behavior.

## 0.7.8.0

- Feature: Deterministic colonization simulation (settings, state, settlement routes, overlays) layered on saved galaxy data; galaxy native-pressure summaries and colonization-route calculators drive non-Traveller jump networks and viewer overlays.
- Feature: Traveller-oriented routing and system typing (`TravellerRouteProfile`, `TravellerSystemProfile`, `TravellerTradeCodeSet`, calculators) integrated with generation and jump-lane plumbing.
- Feature: Generation studios use the main-menu bordered `ScrollContainer` shell and hero/main panel layout; shared `StudioScreenLayoutHelper` rules keep three-column studios readable at the minimum window size.
- Refactor: Population and preview paths updated for native-pressure context, biology gates, and serialization; life-distribution baseline artifacts and harness coverage refreshed.
- Test: Expanded unit and integration coverage for colonization routes, Traveller routes, population, serializers, and studio layout; run `dotnet build StarGen.sln` before shipping.

## 0.7.7.3

- Bug fix: System Studio and Object Studio now stay in a horizontal side-by-side column layout at all resolutions instead of collapsing into a vertical stack, relying on adaptive panel widths and each panel's own scrolling behavior when space gets tight.
- Bug fix: Native life generation once again appears on viable worlds after the generation/simulation split; native-population materialization now follows the ecology/native-life gate instead of waiting for the concept pipeline to predeclare sentience, and the life-permissiveness curve now scales from near-earthlike-only at `0.0` to strong odds for viable `HabitabilityScore 5+` wet worlds at `1.0`.
- Test: Revalidated with `dotnet build StarGen.sln` and the full Godot headless harness (`1957 / 1957` passed). The harness still prints the repo's existing post-summary popup/layout/ObjectDB/RID warning noise after completion, but the run itself is green.

## 0.7.7.2

- Bug fix: System view and object view now expose explicit `New System...` / `New Object...` and `Return to Main Menu` actions in the File menu, so viewers no longer trap the user inside the current inspection path.
- Bug fix: System Studio and Object Studio now use the same three-panel `Parameters / Generation Rules / Active Profile` layout pattern as Galaxy Studio, with launch actions and validation moved into the summary column instead of being buried under the settings form.
- Test: Revalidated with `dotnet build StarGen.sln` and the full Godot headless harness (`1957 / 1957` passed). The harness still prints the repo's existing post-summary popup/layout/ObjectDB/RID warning noise after completion, but the run itself is green.

## 0.7.7.1

- Bug fix: Restored explicit top-bar back buttons for galaxy-opened system/object views, removed regeneration actions from studio-launched viewer contexts, and aligned file-menu/Escape navigation so only views with an actual upstream parent expose back navigation.
- Bug fix: Object edits made from object view now persist back into the currently open standalone system viewer even when there is no galaxy star-seed context, keeping system-studio object inspection/editing usable as a one-level-deep flow.
- Test: Revalidated with `dotnet build StarGen.sln` and the full Godot headless harness (`1957 / 1957` passed). The harness still prints the repo's existing post-summary popup/layout/ObjectDB warning noise after completion, but the test run itself is green.

## 0.7.7.0

- Feature: Generation now stops at initial conditions plus extant native populations, while colonies and non-Traveller jump routes are produced by an explicit deterministic colonization simulation layered on top of saved galaxy state.
- Feature: Added persisted colonization simulation settings/state/settlement-route records, authoritative colony overlays when opening systems, and subsector-scoped simulation caching so revisiting a simulated region restores the same emergent structures without rerunning generation.
- Refactor: Removed generation-side `Expansion Pressure` from use-case settings and generation UI surfaces, keeping `Life Potential` in generation while moving colonization controls into the simulation/tool layer.
- Test: Revalidated with `dotnet build StarGen.sln` and the full Godot headless harness (`1957 / 1957` passed). The harness still prints the repo's existing post-summary popup/layout/ObjectDB warning noise, but the test run itself is green.

## 0.7.6.1

- Bug fix: Non-Traveller galaxy jump routes now derive from a deterministic colonization network instead of the older heuristic nearest-population graph, so routes form only from systems with interstellar-capable export pressure and viable colony targets.
- Bug fix: Route-region systems now carry explicit colonization summaries from generated system data, allowing connected empty systems to receive simulated colony population instead of appearing as route endpoints with zero population.
- Test: Added `ColonizationRouteCalculator` unit coverage plus viewer regressions for connected-system population baselines; `dotnet build StarGen.sln` succeeds cleanly, while the full Godot headless harness is still blocked by the same pre-existing CLR crash in `TestGalaxySystemGenerator::test_generate_system_with_galaxy_context_deterministic_population` before the run reaches the new jump-route cases.

## 0.7.6.0

- Feature: Colony generation now runs as a deterministic second pass over completed systems, so same-system native worlds and cached nearby-system native summaries can raise expansion pressure without making outcomes depend on generation order.
- Feature: Added deterministic native-pressure summary caching at the galaxy layer plus runtime-path updates so galaxy previews and opened systems use the same colony-pressure pipeline while standalone fixture generation keeps the same-system-only pass.
- Test: Added colony-pressure probability/likelihood regressions plus system/preview determinism coverage, then revalidated with `dotnet build StarGen.sln` and the full Godot headless harness (`1953 / 1953` assertions passed; the run still ends with the same pre-existing post-summary Godot .NET cleanup/leak errors in this repo).

## 0.7.5.3

- Bug fix: Galaxy-view realistic generation now actually enables population generation in the runtime tool path, instead of only doing so for Traveller mode while the baseline harness continued to generate realistic population correctly.
- Bug fix: Opening a system from the galaxy viewer now preserves realistic `Life Potential` / `Expansion Pressure` population generation, so populated-world outcomes can make it from Galaxy Studio settings into the generated system data.
- Test: Added regressions for both `StarSystemPreview.Generate(...)` and `MainApp -> GalaxyViewer -> open system` to prove that realistic mode with high life/population settings yields populated runtime systems, then revalidated with `dotnet build StarGen.sln` and the full Godot headless harness (`1947 / 1947` passed).

## 0.7.5.2

- Bug fix: Realistic biosphere support and native-life expectation tracking now use the same deterministic biology gate, eliminating the earlier collapse where high `Life Potential` approved biosphere candidates that the ecology layer later rejected.
- Bug fix: Re-ran the 1000-world life-distribution baseline with support-failure diagnostics; realistic biospheres now track the expected counts across the sampled bands (`0.50 Neutral`: expected `1.3`, actual `1`; `1.00 Space Opera`: expected `11.9`, actual `12`).
- Test: Made the native-life probability ceiling explicit in code and aligned the probability clamp regression with the documented `0.98` maximum, then revalidated with `dotnet build StarGen.sln`, the full Godot headless harness (`1946 / 1946` passed), and a fresh `RunLifeDistributionBaseline.gd` run.

## 0.7.5.1

- Bug fix: Realistic auto-colony generation now uses the persisted population seed consistently and no longer rerolls away colony worlds after the deterministic likelihood gate has already approved them.
- Bug fix: Re-ran the 1000-world life-distribution baseline after the realistic population fix; at `1.00 Space Opera`, active colony worlds rose from `49` to `137`, while native biospheres remained unchanged on the current sample.
- Test: Revalidated with `dotnet build StarGen.sln` and the full Godot headless harness (`1945 / 1945` passed), alongside a fresh `RunLifeDistributionBaseline.gd` artifact refresh.

## 0.7.5.0

- Feature: Galaxy Studio now binds the edited scene layout directly, uses `Realistic` / `Traveller` wording consistently, renames `Settlement Density` to `Expansion Pressure`, moves rules explanations into tooltips, and keeps the active profile column to a concise output-intent summary.
- Feature: Traveller mode now performs deterministic mainworld takeover with typed `TravellerSystemProfile`, `TravellerTradeCodeSet`, and `TravellerRouteProfile` data, applying Traveller world-generation rules across supported UWP-facing elements for the selected mainworld while keeping non-mainworld bodies on the realistic path.
- Feature: Traveller mode now builds jump routes from Traveller-specific world data with the `2 pc per jump number` distance rule, persists the typed Traveller profile through save/load, and updates inspectors/viewers to read the authoritative Traveller profile instead of older fallback strings.
- Test: Revalidated the Traveller/UI expansion with `dotnet build StarGen.sln` and the full Godot headless harness (`1944 / 1944` passed).

## 0.7.4.0

- Refactor: Promoted Galaxy Studio to a first-class `GalaxyGenerationScreen` scene/controller instead of leaving it hidden behind the legacy `WelcomeScreen` identity.
- Refactor: Updated `MainApp`, GDScript compatibility accessors, and integration coverage to instantiate and exercise `GalaxyGenerationScreen.tscn` directly, then removed the obsolete `WelcomeScreen` scene/script pair from the active UI layer.
- Test: Revalidated the renamed galaxy-studio screen through the full headless suite after the navigation and scene-path cleanup.

## 0.7.3.0

- Refactor: Moved the fixed `SystemViewer`, `ObjectViewer`, `ConceptAtlasScreen`, and `EditDialog` shells into their `.tscn` scene trees so the stable viewer/editor UI is now scene-first and editor-visible instead of being rebuilt in C#.
- Refactor: Reduced the remaining viewer/atlas scripts to node binding, option population, signal wiring, and data-driven row rendering while keeping dynamic creation only for variable inspector/detail content and validation/result rows.
- Test: Updated the concept-atlas persisted-result regression to mount the real scene in the tree and revalidated the full headless suite after the scene-backed refactor.

## 0.7.2.0

- Refactor: Moved the fixed Galaxy, System, and Object studio control shells out of runtime C# construction and into their `.tscn` scene trees so those screens are now scene-first and editor-visible.
- Refactor: Reduced `WelcomeScreen`, `SystemGenerationScreen`, and `ObjectGenerationScreen` to node binding, state wiring, and summary logic while keeping only truly data-driven rows generated at runtime.
- Test: Verified the studio scene refactor against the existing headless integration suite to ensure the generated request/spec flows still behave identically after the UI restructuring.

## 0.7.1.3

- Bug fix: Galaxy Studio now keeps the three-panel layout in actual columns at normal desktop widths instead of stacking them into rows too aggressively.
- Bug fix: Tightened the three-column responsive breakpoint and minimum panel widths so the parameters, generation rules, and active profile sections stay side by side until the window is genuinely narrow.

## 0.7.1.2

- Bug fix: The splash screen, main menu, and galaxy studio all now use the upcoming public release label `0.8.0.0`, while the internal hardening line advances separately as `0.7.1.2`.
- Bug fix: Main-menu Help, Credits, Release Notes, Station Studio, and Sources copy were cleaned to stay user-facing, with direct icon attribution and a dedicated Sources button instead of repo-path references.
- Bug fix: Galaxy Studio now uses a three-column layout that separates parameters, generation rules, and the active profile; the galaxy-only `Mainworld` control was removed because it was not meaningfully shaping galaxy generation.
- Bug fix: The life-assumptions tooltip now reads cleanly from the in-app helper copy, and the 1000-world baseline now evaluates the same generated world sample across scenarios instead of silently changing the sample set between runs.
- Bug fix: Native-life absence now correctly suppresses ecology, evolution, and sentience states in runtime population data, preventing concept layers from appearing on worlds where native life never emerged.

## 0.7.1.1

- Bug fix: Main-menu and studio version labels now read from a dedicated public-version setting so the user-facing UI can advertise the upcoming public release while internal 0.7 hardening continues.
- Bug fix: Galaxy Studio is back on a real split layout, with a dedicated summary panel, better use of horizontal space, a hoverable advanced-assumptions info button, and corrected checkbox highlighting that no longer obscures nearby controls.
- Feature: Galaxy morphology controls now explain their actual density-model impact in-app, and the project source notes now include review targets for spiral, elliptical, bulge, and irregular-galaxy structure references.
- Feature: `Life Potential` and `Settlement Density` now affect native-life and colony generation instead of acting as mostly cosmetic labels, and a separate 1000-world baseline runner now records the resulting distribution for future regression review.
- Docs: Added Cursor to the in-app AI credits text, documented the user-facing `0.8.0.0` label policy, and added baseline-runner guidance plus galaxy-morphology review sources.

## 0.7.1.0

- Refactor: Replaced the flattened concept showcase context with a typed dependency pipeline spanning `PlanetEnvironmentProfile`, `EcologyState`, `SpeciesEvolutionState`, `SentienceAssessment`, `SocietyState`, `ReligionState`, `LanguageState`, and `DiseaseState`.
- Feature: Reworked native-life generation so ecology and evolution attach only to biological worlds, sentience is evaluated explicitly, and civilisation/religion/language now generate only for extant sentient populations.
- Feature: Reintroduced persisted concept state on the hardening branch behind deterministic applicability gates, richer provenance, and atlas reuse of typed runtime results instead of fabricated summaries.
- Quality: Removed ternary operators from the concept path, tightened concept serialization and registry failures, and added concept-pipeline regression coverage for lifeless, non-sentient, and sentient worlds plus a static ternary scan.
- Internal milestone: `0.7.1.0` is part of the private 0.7 hardening line. The next public release target remains `0.8.0.0` after realism tuning and explicit human audit on culture-, religion-, language-, civilisation-, and species-adjacent outputs.

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
