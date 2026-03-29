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

## Expanded stellar populations (brown dwarfs, white dwarfs, multiplicity)

**Current use in StarGen:** `src/domain/generation/generators/StarGenerator.cs`, `src/domain/generation/generators/StellarMassSampler.cs`, `src/domain/system/StellarConfigGenerator.cs`, `src/domain/generation/parameters/StellarScienceReferenceCatalog.cs`, and stellar-generation/system-generation tests.

| Citation | Summary | Used in |
|----------|----------|---------|
| Cummings, J. D., Kalirai, J. S., Tremblay, P.-E., Ramirez-Ruiz, E., & Choi, J. (2018). *The white dwarf initial-final mass relation for progenitor stars from 0.85 to 7.5 M☉*. *The Astrophysical Journal*, *866*(1), 21. https://arxiv.org/abs/1809.01673 — Notes: [Texts/Cummings2018.txt](Texts/Cummings2018.txt). | Cluster-calibrated initial-final mass relation (IFMR) across 0.85–7.5 M☉; low/intermediate/high-mass slope changes; weak metallicity dependence across moderate [Fe/H]. **Supports:** deterministic white-dwarf mass estimation from an older progenitor star in StarGen. | StarGenerator.cs white-dwarf generation; stellar help/source notes |
| Kirkpatrick, J. D., Reid, I. N., Liebert, J., Gizis, J. E., Burgasser, A. J., Monet, D. G., Dahn, C. C., Nelson, B., & Williams, R. J. (2000). *Sixty-seven additional L dwarfs discovered by the Two Micron All Sky Survey (2MASS)*. *The Astronomical Journal*, *120*(1), 447–472. https://arxiv.org/abs/astro-ph/0003317 — Notes: [Texts/Kirkpatrick2000.txt](Texts/Kirkpatrick2000.txt). | Early L-dwarf census; notes that L dwarfs span roughly 1300–2000 K and helps anchor the warm end of the brown-dwarf sequence. **Supports:** approximate L-class temperature band in StarGen. | StarTable.cs brown-dwarf spectral ranges; StarGenerator.cs brown-dwarf classification |
| Kirkpatrick, J. D., Cushing, M. C., Gelino, C. R., et al. (2011). *The first hundred brown dwarfs discovered by the Wide-field Infrared Survey Explorer (WISE)*. *The Astrophysical Journal Supplement Series*, *197*(2), 19. https://arxiv.org/abs/1108.4677 — Notes: [Texts/Kirkpatrick2011.txt](Texts/Kirkpatrick2011.txt). | WISE verification of L, T, and Y dwarfs; color/spectral trends; first volume-limited late-T and Y-dwarf census step. **Supports:** extending StarGen beyond M stars into T and Y brown-dwarf classes. | StarTable.cs brown-dwarf spectral ranges; ColorUtils.cs; stellar help/source notes |
| Kirkpatrick, J. D., Marocco, F., Gelino, C. R., et al. (2024). *The Initial Mass Function Based on the Full-sky 20 pc Census of ∼3600 Stars and Brown Dwarfs*. *The Astrophysical Journal Supplement Series*, *271*(2), 55. https://ui.adsabs.harvard.edu/abs/2024ApJS..271...55K/abstract — Notes: [Texts/Kirkpatrick2024.txt](Texts/Kirkpatrick2024.txt). | 20 pc census spanning stars and brown dwarfs; quadripartite IMF with low-mass substellar segments; star-to-brown-dwarf number ratio about 4:1. **Supports:** practical brown-dwarf frequency and low-mass IMF shaping in StarGen. | StellarMassSampler.cs; StarGenerator brown-dwarf frequency tests |
| Moe, M., & Di Stefano, R. (2017). *Mind your Ps and Qs: The interrelation between period (P) and mass-ratio (Q) distributions of binary stars*. *The Astrophysical Journal Supplement Series*, *230*(2), 15. https://arxiv.org/abs/1606.05347 — Notes: [Texts/MoeDiStefano2017.txt](Texts/MoeDiStefano2017.txt). | Meta-analysis of binary populations across techniques; period and mass-ratio distributions depend strongly on primary mass. **Supports:** moving StarGen companions away from independent random stars toward primary-conditioned companion masses and hierarchical layouts. | StellarConfigGenerator.cs multiplicity architecture; system-generation tests |
| Tokovinin, A. (2021). *Architecture of hierarchical stellar systems and their formation*. *Universe*, *7*(9), 352. https://arxiv.org/abs/2109.09118 — Notes: [Texts/Tokovinin2021.txt](Texts/Tokovinin2021.txt). | Review of hierarchical-system families; orbit alignment trends; comparable masses inside the same hierarchy; architecture as a clue to formation path. **Supports:** building deterministic nested binaries instead of arbitrary random pairing in StarGen. | StellarConfigGenerator.cs hierarchy builder; system-generation tests |

---

## Solar neighborhood density

**Current use in StarGen:** [src/domain/galaxy/SubSectorGenerator.gd](../src/domain/galaxy/SubSectorGenerator.gd) — ~0.004 systems/pc³ (SOLAR_NEIGHBORHOOD_DENSITY). [Tests/domain/galaxy/TestSubSectorGenerator.gd](../Tests/domain/galaxy/TestSubSectorGenerator.gd): solar-neighborhood density test.

| Citation | Summary | Used in |
|----------|----------|---------|
| Bovy, J. (2017). Stellar inventory of the solar neighborhood using Gaia DR1. *MNRAS*, *470*(2), 1360–1387. https://doi.org/10.1093/mnras/stx1278 — Full text (abridged): [Texts/Bovy2017.txt](Texts/Bovy2017.txt). | **Mass density** (not system count): total mid-plane stellar density **0.040 ± 0.002 M☉/pc³**. Vertical profiles; number densities by spectral type. **Supports:** Same solar-neighborhood scale; StarGen uses *system number* density 0.004 pc⁻³ (different quantity). No code change required for consistency. **Fidelity:** Document Bovy’s 0.040 M☉/pc³ in density docs; optionally cross-check system count vs mass density (mean mass per system) in calibration mode. | SubSectorGenerator.gd; TestSubSectorGenerator; documentation of density assumptions |
| *Others from ToReview* | Stellar or system density (per pc³) in solar neighborhood / mid-plane. | SubSectorGenerator.gd; TestSubSectorGenerator |

---

## Planetary retrofit and deterministic formation surrogates

**Current use in StarGen:** `Sources/Texts/planets.md`, `src/domain/generation/PlanetaryGenerationProfile.cs`, `src/domain/generation/PlanetarySystemState.cs`, `src/domain/system/SystemPlanetGenerator.cs`, `src/domain/generation/generators/PlanetGenerator.cs`, `src/app/GalaxyGenerationScreen.Science.cs`, `src/app/SystemGenerationScreen.Planetary.cs`.

| Citation | Summary | Used in |
|----------|----------|---------|
| *StarGen deterministic planet formation specification* (2026). [Texts/planets.md](Texts/planets.md). | Internal implementation-facing design specification translating the planetary research review into a deterministic pipeline for StarGen. Defines the recommended stage order, the shared system-level latent variables, provenance expectations, and the distinction between aggregate upstream formation controls and direct single-planet controls. **Supports:** the `0.8.6.0` retrofit that adds a shared aggregate planetary profile and derived planetary-system state without rewriting the existing generator stack. | PlanetaryGenerationProfile.cs; PlanetarySystemState.cs; SystemPlanetGenerator.cs; PlanetGenerator.cs; Galaxy/System studio planetary controls |

---

## Exoplanet demographics (hot Jupiters, giant planets)

**Current use in StarGen:** [Tests/ScientificBenchmarks.gd](../Tests/ScientificBenchmarks.gd) — hot Jupiter fraction &lt;5%. [Roadmap](../Docs/Roadmap.md): inner vs outer large-planet fractions. [Tests/Unit/TestSystemPlanetDistributions.gd](../Tests/Unit/TestSystemPlanetDistributions.gd): close-in gas giants rare.

| Citation | Summary | Used in |
|----------|----------|---------|
| *To be filled from ToReview* | Hot Jupiter occurrence rate; Kepler/TESS demographics; giant planet frequency. | ScientificBenchmarks.gd; SystemPlanetGenerator; TestSystemPlanetDistributions.gd |

---

## Galaxy morphology and structure

**Current use in StarGen:** `src/domain/galaxy/GalaxySpec.cs`, `src/domain/galaxy/SpiralDensityModel.cs`, `src/domain/galaxy/EllipticalDensityModel.cs`, `src/domain/galaxy/IrregularDensityModel.cs`, `src/domain/galaxy/SubSectorGenerator.cs`, `src/domain/generation/parameters/GenerationParameterCatalog.cs`, and `src/app/GalaxyGenerationScreen.cs`.

| Citation | Summary | Used in |
|----------|----------|---------|
| Bland-Hawthorn, J., & Gerhard, O. (2016). The Galaxy in context: Structural, kinematic, and integrated properties. *Annual Review of Astronomy and Astrophysics*, *54*, 529-596. https://doi.org/10.1146/annurev-astro-081915-023441 | Milky Way structural review covering disk scale lengths and heights, bulge and bar properties, stellar mass distribution, and comparison against other spirals. Supports StarGen's use of an exponential disk plus central bulge for the spiral profile. | GalaxySpec.cs; SpiralDensityModel.cs; GalaxyGenerationScreen.cs parameter explanations |
| van der Kruit, P. C., & Freeman, K. C. (2011). Galaxy disks. *Annual Review of Astronomy and Astrophysics*, *49*, 301-371. https://doi.org/10.1146/annurev-astro-081710-102529 | Review of exponential disks, scale lengths, scale heights, truncations, and observed disk structure. Supports the disk falloff assumptions and user-facing arm and disk controls. | SpiralDensityModel.cs; GenerationParameterCatalog.cs |
| Wegg, C., & Gerhard, O. (2013). Mapping the three-dimensional density of the Galactic bulge with VVV red clump stars. *Monthly Notices of the Royal Astronomical Society*, *435*(3), 1874-1887. https://arxiv.org/abs/1308.4385 | 3D bulge and bar density map for the Milky Way showing boxy and peanut bulge structure, orientation, and concentration. Supports treating StarGen's bulge controls as a simplified stand-in for a richer central stellar concentration model rather than pure decoration. | GalaxySpec.cs; SpiralDensityModel.cs; EllipticalDensityModel.cs |
| Conselice, C. J. (2014). The evolution of galaxy structure over cosmic time. *Annual Review of Astronomy and Astrophysics*, *52*, 291-337. https://doi.org/10.1146/annurev-astro-081913-040037 | Review of morphology classes across spirals, ellipticals, irregulars, and mergers, including how the categories are used observationally. Supports the broad galaxy-type categories exposed in the Galaxy Studio while clarifying that StarGen is using coarse deterministic proxies rather than a full cosmological formation model. | GalaxySpec.cs; EllipticalDensityModel.cs; IrregularDensityModel.cs; GalaxyGenerationScreen.cs |

---

## Orbital stability (planet spacing)

**Current use in StarGen:** ~10 mutual Hill radii for long-term stability. [src/domain/system/OrbitalMechanics.gd](../src/domain/system/OrbitalMechanics.gd) (line 424), [OrbitSlotGenerator.gd](../src/domain/system/OrbitSlotGenerator.gd) (line 139), [SystemValidator.gd](../src/domain/system/SystemValidator.gd) (line 268).

| Citation | Summary | Used in |
|----------|----------|---------|
| Chambers, J. E., Wetherill, G. W., & Boss, A. P. (1996). The stability of multi-planet systems. *Icarus*, *119*(2), 261–268. https://doi.org/10.1006/icar.1996.0019 | N-body stability; systems with Δ &lt; 10 mutual Hill radii always unstable (log t = bΔ + c); likely unstable for Δ &gt; 10 on longer timescales. | OrbitalMechanics.gd; OrbitSlotGenerator.gd; SystemValidator.gd; Roadmap (Scientific calibration) |

---

## Changelog

- Added `Sources/Texts/planets.md` under Planetary retrofit and deterministic formation surrogates to document the `0.8.6.0` aggregate planetary retrofit and its upstream-vs-object control split.
- Cummings et al. (2018), Kirkpatrick et al. (2000, 2011, 2024), Moe & Di Stefano (2017), and Tokovinin (2021) added under Expanded stellar populations to support the `0.8.4.0` stellar-offerings pass (brown dwarfs, evolved stars, white dwarfs, and stronger multiplicity architecture).
- Li et al. (2023) added: reviewed from abridged full text in Texts/Li2023.txt; annotated under Stellar distribution (supports current M-dwarf band; fidelity: metallicity/age-dependent IMF).
- Bovy (2017) added: reviewed from abridged full text in Texts/Bovy2017.txt; annotated under Stellar distribution and Solar neighborhood density (supports current use; fidelity notes for mass density and dn/dM).
- Initial structure and APA 7; placeholders for Chambers 1996 and benchmark topics (stellar, density, exoplanets). ToReview.md holds abstract-only papers until full text is added.
