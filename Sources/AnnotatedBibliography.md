# Annotated Bibliography

Track sources used to keep StarGen’s scientific assumptions accurate. Supports calibration, distribution benchmarks, and the future “Calibrated” realism mode. When a paper is reviewed in full, add a copy under `Texts/` as `AuthorYear.txt` (e.g. `Chambers1996.txt`). Full-text copies: add `AuthorYear.txt` in `Texts/` after extracting from PDF (e.g. `pdftotext`).

---

## Citation style: APA 7 (author-date)

We use **APA 7th edition author-date** for in-project references.

- **In-text / in-code:** `(Chambers, 1996)`, `(Chabrier, 2005)`.
- **Reference list:** Author, A. A. (Year). *Title of article*. *Journal Name*, *Volume*(Issue), pages. https://doi.org/xxxx

**Example — in text:**  
Orbital spacing follows a rule of thumb of ~10 mutual Hill radii for long-term stability (Chambers, 1996).

**Example — reference list:**  
Chambers, J. E., Wetherill, G. W., & Boss, A. P. (1996). The stability of multi-planet systems. *Icarus*, *119*(2), 261–268. https://doi.org/10.1006/icar.1996.0019

---

## Stellar distribution (IMF / spectral types)

**Current use in StarGen:** [Tests/ScientificBenchmarks.gd](../Tests/ScientificBenchmarks.gd) — M-dwarf fraction 60–90%, G+K 10–35%, OBAF &lt;12%. [Roadmap](../Docs/Roadmap.md): local IMF / 20 pc census.

| Citation | Summary | Used in |
|----------|----------|---------|
| Bovy, J. (2017). Stellar inventory of the solar neighborhood using Gaia DR1. *Monthly Notices of the Royal Astronomical Society*, *470*(2), 1360–1387. https://doi.org/10.1093/mnras/stx1278 — Full text (abridged): [Texts/Bovy2017.txt](Texts/Bovy2017.txt). | Gaia DR1/TGAS selection function; local number density and vertical sech² profiles by spectral type (A–K, giants); high-mass present-day mass function dn/dM = 0.016 (M/M☉)^(−4.7); luminosity function. **Supports:** Same body of local-census work our M-dwarf / G+K / OBAF bands rely on; Bovy extends to Gaia. **Fidelity:** Could use Bovy’s dn/dM for M &gt; 1 M☉ or scale heights (≈50–150 pc) if we add vertical structure or finer spectral calibration. | ScientificBenchmarks.gd; StarGenerator distribution tests; documentation of local IMF |
| Li, J., Liu, C., Zhang, Z.-Y., Tian, H., Fu, X., Li, J., & Yan, Z.-Q. (2023). Stellar initial mass function varies with metallicities and time. *Nature*. arXiv:2301.07029 — Full text (abridged): [Texts/Li2023.txt](Texts/Li2023.txt). | ~93k M dwarfs (0.3–0.7 M☉) in 100–300 pc; LAMOST+Gaia. Variable IMF: α = 1.9–2.5 as [M/H] −0.8→+0.1; early populations fewer low-mass stars; present-day low-mass fraction increases with metallicity; dα/d[M/H] ≈ 0.5 (dyn-cold). **Supports:** ScientificBenchmarks M-dwarf band (60–90%) is consistent—Li shows variation with metallicity/age; our band encompasses it. StarGen has metallicity (StarSpec, GalaxyStar) but does not yet vary IMF by it. **Fidelity:** Could add metallicity-dependent (or age-dependent) IMF slope or M-dwarf fraction in calibration mode (α or fraction vs [M/H]); document as solar-neighborhood mix. | ScientificBenchmarks.gd; StarGenerator; GalaxyStar/StellarProps metallicity |
| *Others from ToReview* | Stellar IMF, 20 pc census, M-dwarf / G+K fractions. | ScientificBenchmarks.gd; StarGenerator distribution tests |

---

## Solar neighborhood density

**Current use in StarGen:** [src/domain/galaxy/SubSectorGenerator.gd](../src/domain/galaxy/SubSectorGenerator.gd) — ~0.004 systems/pc³ (SOLAR_NEIGHBORHOOD_DENSITY). [Tests/domain/galaxy/TestSubSectorGenerator.gd](../Tests/domain/galaxy/TestSubSectorGenerator.gd): solar-neighborhood density test.

| Citation | Summary | Used in |
|----------|----------|---------|
| Bovy, J. (2017). Stellar inventory of the solar neighborhood using Gaia DR1. *MNRAS*, *470*(2), 1360–1387. https://doi.org/10.1093/mnras/stx1278 — Full text (abridged): [Texts/Bovy2017.txt](Texts/Bovy2017.txt). | **Mass density** (not system count): total mid-plane stellar density **0.040 ± 0.002 M☉/pc³**. Vertical profiles; number densities by spectral type. **Supports:** Same solar-neighborhood scale; StarGen uses *system number* density 0.004 pc⁻³ (different quantity). No code change required for consistency. **Fidelity:** Document Bovy’s 0.040 M☉/pc³ in density docs; optionally cross-check system count vs mass density (mean mass per system) in calibration mode. | SubSectorGenerator.gd; TestSubSectorGenerator; documentation of density assumptions |
| *Others from ToReview* | Stellar or system density (per pc³) in solar neighborhood / mid-plane. | SubSectorGenerator.gd; TestSubSectorGenerator |

---

## Exoplanet demographics (hot Jupiters, giant planets)

**Current use in StarGen:** [Tests/ScientificBenchmarks.gd](../Tests/ScientificBenchmarks.gd) — hot Jupiter fraction &lt;5%. [Roadmap](../Docs/Roadmap.md): inner vs outer large-planet fractions. [Tests/Unit/TestSystemPlanetDistributions.gd](../Tests/Unit/TestSystemPlanetDistributions.gd): close-in gas giants rare.

| Citation | Summary | Used in |
|----------|----------|---------|
| *To be filled from ToReview* | Hot Jupiter occurrence rate; Kepler/TESS demographics; giant planet frequency. | ScientificBenchmarks.gd; SystemPlanetGenerator; TestSystemPlanetDistributions.gd |

---

## Orbital stability (planet spacing)

**Current use in StarGen:** ~10 mutual Hill radii for long-term stability. [src/domain/system/OrbitalMechanics.gd](../src/domain/system/OrbitalMechanics.gd) (line 424), [OrbitSlotGenerator.gd](../src/domain/system/OrbitSlotGenerator.gd) (line 139), [SystemValidator.gd](../src/domain/system/SystemValidator.gd) (line 268).

| Citation | Summary | Used in |
|----------|----------|---------|
| Chambers, J. E., Wetherill, G. W., & Boss, A. P. (1996). The stability of multi-planet systems. *Icarus*, *119*(2), 261–268. https://doi.org/10.1006/icar.1996.0019 | N-body stability; systems with Δ &lt; 10 mutual Hill radii always unstable (log t = bΔ + c); likely unstable for Δ &gt; 10 on longer timescales. | OrbitalMechanics.gd; OrbitSlotGenerator.gd; SystemValidator.gd; Roadmap (Scientific calibration) |

---

## Changelog

- Li et al. (2023) added: reviewed from abridged full text in Texts/Li2023.txt; annotated under Stellar distribution (supports current M-dwarf band; fidelity: metallicity/age-dependent IMF).
- Bovy (2017) added: reviewed from abridged full text in Texts/Bovy2017.txt; annotated under Stellar distribution and Solar neighborhood density (supports current use; fidelity notes for mass density and dn/dM).
- Initial structure and APA 7; placeholders for Chambers 1996 and benchmark topics (stellar, density, exoplanets). ToReview.md holds abstract-only papers until full text is added.
