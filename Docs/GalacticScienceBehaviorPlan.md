# Galactic Science Behavior Plan

Date: `2026-05-12`

Purpose: identify the Galaxy Studio and Galaxy Viewer behavior changes needed after the diagnostic galactic science-hardening surfaces are in place. This plan does not approve scientific claims or activate dynamics behavior; human review remains required before diagnostic fields become generation drivers.

## Current Diagnostic Baseline

The domain layer now serializes these galaxy-level diagnostic surfaces through `GalaxyRealismProfile` and `GalaxySpec`:

- Milky-Way structural schema: thin/thick disk dimensions, bar half-length, solar-circle radius, circular velocity, stellar mass, GHZ, metallicity, and star-formation priors.
- `GalaxyMassComponentBudget`: halo, dark matter, baryonic, stellar disk/spheroid/halo, nuclear stellar, cold gas, hot gas, baryon/gas fraction, and local stellar-density diagnostics.
- `GalaxyRotationCurveDiagnostic`: inner/reference/outer velocity anchors and disk/spheroid/gas/dark-matter velocity contributions.
- `GalaxyDynamicsDiagnostic`: bar pattern speed, corotation radius, corotation/bar ratio, local mass-density and surface-density proxies, analog calibration mode, and non-Milky-Way comparison status.

These are currently audit/export/readout facts only. They must not affect star placement, local-space builds, system generation, population pressure, jump routes, or gravitational potential until a separate behavior pass is reviewed.

## Test Baseline Mapping

The current test baseline uses two labels:

- `P2P` means present-to-present coverage: behavior that already exists or is already intentionally absent, and therefore should stay protected in the current suite.
- `F2P` means future-to-present coverage: executable tests for expected future Galaxy Studio, Galaxy Viewer, and behavior-mode behavior. These tests are expected to fail until the future behavior is implemented.

Current `P2P` baseline tests cover full diagnostic field serialization, star-position/density-sampling stability when diagnostics change, unchanged `GalaxyOriginContext` behavior while diagnostics are readout-only, and comparison-source caveats for non-Sb family proxies. These tests run in the default headless harness and must stay green.

Current `F2P` baseline tests assert the actual future behavior surfaces: `GalaxyDynamicsBehaviorMode` serialization, `GalaxyOriginContext` local-dynamics annotations, Galaxy Studio diagnostics and disabled dynamics controls, Galaxy Viewer diagnostics readouts, non-Milky-Way comparison calibration presets, and future overlay surfaces. They run only through the explicit future-behavior harness:

```powershell
godot-mono.exe --path . --headless --script res://Tests/RunTestsHeadless.gd -- f2p
```

The initial baseline is expected to fail. Each future implementation slice should turn its corresponding F2P failures green, then move those tests or equivalent direct tests into the default harness.

## Galaxy Studio Changes Needed

1. Add a read-only `Resolved Galactic Diagnostics` panel in the Active Profile column.
   - Show compact rows for mass budget, rotation curve, bar dynamics, local mass budget, and analog calibration status.
   - Label diagnostic-only fields clearly as inactive for generation behavior.

2. Add future behavior-gated controls, disabled by default.
   - `Dynamics Mode`: `Diagnostics Only` initially; later `Affect Region Context` and `Affect Placement` only after review.
   - `Analog Calibration`: `Milky Way Analog`, `Family Proxy`, and later source-backed non-Milky-Way family sets.
   - `Bar Dynamics`: pattern-speed/corotation visibility for barred galaxies only.

3. Update Galaxy Studio help/source text.
   - Explain the difference between structural schema, diagnostic dynamics, and active generation behavior.
   - Cite Bland-Hawthorn/Gerhard, Bovy, Khoperskov, Hunt/Vasiliev, Kennicutt, and future comparison sources separately.

4. Keep source uncertainty visible.
   - For non-Sb spiral, elliptical, lenticular, and irregular families, show `comparison sources needed` until family-specific calibration is implemented.

## Galaxy Viewer Changes Needed

1. Add a read-only `Science Diagnostics` inspector section.
   - Structure: subtype, GHZ, metallicity gradient, stellar mass, disk/bar fields.
   - Mass Budget: baryonic, dark halo, stellar disk/spheroid/halo, gas fractions.
   - Rotation: reference velocity, inner/outer velocity, component contributions, curve shape.
   - Dynamics: pattern speed, corotation radius, local density, analog calibration caveat.

2. Add local-position overlays later, behind explicit mode selection.
   - Bar/corotation zones.
   - GHZ and hazard bands.
   - Local mass-density or density-ratio heat hints.
   - These should be viewer overlays first, not generation drivers.

3. Tie local-space previews to diagnostics only after review.
   - First behavior pass should annotate selected systems with local diagnostics.
   - Later passes may let local mass/dynamics affect route risk, settlement pressure, or stellar context.

## Domain Behavior Changes Needed

1. Add a behavior flag before using diagnostics downstream.
   - Existing saves and default generation should remain diagnostic-only.
   - A future `GalaxyDynamicsBehaviorMode` should gate all behavioral effects.

2. If activated, apply dynamics in stages.
   - Stage 1: enrich `GalaxyOriginContext` with local dynamics diagnostics.
   - Stage 2: adjust hazard, age cohort, cluster probability, and local stellar-profile context.
   - Stage 3: optionally alter star placement/density sampling only after source review and stronger tests.

3. Add non-Milky-Way family calibration before broad behavior changes.
   - Do not apply Milky-Way bar/local-density assumptions blindly to ellipticals, lenticulars, irregulars, or non-Sb spirals.
   - Add comparison sources and per-family diagnostics before active behavior.

## Test Requirements

- Serialization round-trip tests for every diagnostic field and future behavior flag.
- Galaxy Studio tests proving diagnostics are visible but controls remain disabled in `Diagnostics Only`.
- Galaxy Viewer tests proving readouts surface diagnostic fields without mutating generation.
- Regression tests proving default galaxy seeds produce the same star positions before and after diagnostic readouts.
- Behavior-mode tests, when added, proving each active mode changes only the documented downstream surface.

## Recommended Next Implementation Order

1. Add read-only Galaxy Studio and Galaxy Viewer diagnostic readouts.
2. Add disabled/future-gated dynamics-mode UI so users can see the planned behavior boundary.
3. Add `GalaxyOriginContext` diagnostic annotations without behavior changes.
4. Add source-backed non-Milky-Way family comparison presets.
5. Only then branch active dynamics behavior for region context, routes, population pressure, or placement.
