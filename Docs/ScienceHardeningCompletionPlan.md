# Science Hardening Completion Plan

Date: `2026-05-13`

Purpose: define the remaining science-hardening surfaces that must exist before StarGen can claim a broadly source-backed generation pipeline. This plan is executable through F2P coverage, but human review remains the final authority for scientific claims, source evaluation, cultural modeling, release readiness, and licensing.

## F2P Scope

`F2P` means future-to-present coverage. For the global hardening pass, F2P verifies that every remaining hardening family has a tracked plan, source IDs, tradeoff statement, and at least one implementation-facing diagnostic or engine surface.

The explicit F2P harness is:

```powershell
godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd -- f2p
```

## Required Engine Families

| Domain | Required engine surface | Default behavior | Alternatives / follow-up engines | Tradeoff |
|---|---|---|---|---|
| Planet habitability | `kopparapu_2014_mass_corrected_hz_diagnostic` | Diagnostic-only planet-level mass-corrected HZ record after mass/radius generation | Kasting 1993, Kopparapu 2013 conservative, Kopparapu 2013 optimistic | Keeps orbit generation deterministic and additive while preserving planet-mass context for life scoring and readouts. |
| Orbital stability | `mutual_hill_amd_packing_stability_diagnostic` | Existing mutual-Hill spacing remains active; AMD, packing, and resonance screens are diagnostics | Mutual-Hill proxy, AMD screen, dynamical-packing screen, resonance proximity | Avoids N-body integration in generation while showing where source-backed stability models would disagree. |
| Moon formation | `moon_channel_architecture_diagnostic` | Deterministic channel trace for CPD regular, captured irregular, and terrestrial impact candidates | CPD population synthesis, capture-family model, terrestrial giant-impact branch, habitable-edge diagnostic | Keeps moon generation fast and deterministic while marking which channel is proxy-only. |
| Small bodies | `small_body_population_export_diagnostic` | Existing reservoir records remain diagnostic/export-prep records | Reservoir family proxy, survey-bias size-luminosity diagnostic, station/habitat follow-up | Prevents survey-biased population counts from becoming station placement until reviewed. |
| Galaxy dynamics | `galaxy_family_dynamics_diagnostic` | Existing behavior-gated galaxy dynamics modes remain default diagnostics | Milky-Way analog, family proxy, region-context behavior, placement behavior | Prevents Milky-Way dynamics from silently driving non-Milky-Way families. |
| Sentient population | `human_audit_required_sentient_population_proxy` | Existing population/governance outputs remain human-audit-required proxies | Aggregate diffusion, per-technology diffusion, jurisdiction hierarchy, state-capacity review surface | Keeps culture, governance, law, religion, and technology outputs advisory rather than final authority. |

## Implementation Order

1. Add F2P coverage for all domains above.
2. Add planet-level HZ diagnostics after physical generation.
3. Add alternative stability diagnostics to orbit slots and planet formation traces.
4. Promote terrestrial-impact moon records from empty-source placeholders to source-marked diagnostics.
5. Keep small-body population counts, galaxy dynamics behavior, and sentient-population retuning behind reviewed follow-up gates.

## Acceptance Criteria

- F2P harness covers the global hardening plan and the six required engine families.
- Generated planets carry `kopparapu2014_mass_corrected_hz_*` provenance without changing orbit placement.
- Orbit slots serialize AMD, packing, and resonance diagnostic fields without changing the default mutual-Hill scaffold.
- Planet formation traces copy those stability diagnostics for downstream audit/export.
- Terrestrial moon candidates record Malamud/Perets and Nakajima source IDs as diagnostic-only support.
- All science claims added in this pass remain pending human verification unless the user explicitly reviews and accepts them.
