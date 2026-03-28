# Version

Current version: `0.8.3.0`

Current user-facing version: `0.9d`

Date: `2026-03-28`

Versioning method: release/refactor `+0.1`, feature `+0.0.1`, bug fix `+0.0.0.1`, save-breaking release `+1.0`.

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
- Feature: Mainline remains generation-and-view focused, with Concept Atlas and save/load/export paths removed from the shipped runtime flow while related parked or compatibility code stays in-repo for branch-specific follow-up.
- Test: The current checkpoint still builds and passes the trimmed mainline headless harness (`1774 / 1774`) after the UI audit and test-suite rewrite.

## 0.9.0.0

- Release: Cut the `0.9` mainline release branch around generation-and-view workflows only, with branch-aware user-facing version composition now resolving to `0.9.0.0d` on mainline and `0.9.0.0e` on the export branch.
- Release: Removed shipped Concept Atlas entry points and concept-summary runtime surfaces from the mainline app flow; concept-heavy systems remain parked in-repo for later migration, while `Concepts/Additions.md` stays the StarGen-only prototype backlog.
- Release: Removed mainline load/save/export entry points from studios, viewers, and the object edit dialog so the public `0.9` line no longer exposes persistence behavior.
- Test: Rewrote the mainline harness around deterministic generation, realism, provenance, and non-UI integration coverage; UI-focused tests, concept harness registration, and mainline persistence/save-load suites were discarded from the `0.9` branch.

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
