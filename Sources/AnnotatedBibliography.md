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
| Chen, J., & Kipping, D. (2017). *Probabilistic forecasting of the masses and radii of other worlds*. *The Astrophysical Journal*, *834*(1), 17. https://arxiv.org/abs/1603.08614 — Notes: [Texts/ChenKipping2017.txt](Texts/ChenKipping2017.txt). | Broad empirical mass-radius relation spanning Terran, Neptunian, and Jovian regimes. **Supports:** the default all-planet size model in StarGen and the removal of the old uncited `Legacy` size curve from aggregate planetary priors. | PlanetaryGenerationProfile.cs; PlanetMassRadiusTable.cs; Galaxy/System planetary controls |
| Otegi, J. F., Bouchy, F., & Helled, R. (2020). *Revisited mass-radius relations for exoplanets below 120 Earth masses*. *Astronomy & Astrophysics*, *634*, A43. https://arxiv.org/abs/1911.04745 — Notes: [Texts/Otegi2020.txt](Texts/Otegi2020.txt). | Distinguishes rocky and volatile-rich branches for planets below 120 Earth masses. **Supports:** a second small-planet mass-radius option that is meaningful because StarGen already tracks rocky-versus-volatile class tendencies. | PlanetMassRadiusTable.cs; Galaxy/System planetary controls |
| Owen, J. E., & Wu, Y. (2017). *The evaporation valley in the Kepler planets*. *The Astrophysical Journal*, *847*(1), 29. https://arxiv.org/abs/1705.10810 — Notes: [Texts/OwenWu2017.txt](Texts/OwenWu2017.txt). | Photoevaporation-driven stripping for hot close-in small planets. **Supports:** the `Photoevaporation` envelope-loss option as a real model branch rather than generic wording. | PlanetaryScienceReferenceCatalog.cs; SystemPlanetGenerator.cs |
| Ginzburg, S., Schlichting, H. E., & Sari, R. (2018). *Core-powered mass loss and the radius distribution of small exoplanets*. *Monthly Notices of the Royal Astronomical Society*, *476*(1), 759-765. https://arxiv.org/abs/1708.01621 — Notes: [Texts/Ginzburg2018.txt](Texts/Ginzburg2018.txt). | Cooling-core-driven atmospheric loss for small planets. **Supports:** the `Core-powered` envelope-loss option as a second explicit model family. | PlanetaryScienceReferenceCatalog.cs; SystemPlanetGenerator.cs |
| Mordasini, C., Alibert, Y., Benz, W., & Naef, D. (2007). *Giant planet formation by core accretion*. https://arxiv.org/abs/0710.5667 — Notes: [Texts/Mordasini2007.txt](Texts/Mordasini2007.txt). | Review of the standard heavy-core-first giant-planet formation model. **Supports:** the `Core Accretion` giant-growth option in the aggregate planetary priors. | PlanetaryScienceReferenceCatalog.cs; PlanetarySystemState.cs |
| Lambrechts, M., & Johansen, A. (2012). *Rapid growth of gas-giant cores by pebble accretion*. *Astronomy & Astrophysics*, *544*, A32. https://arxiv.org/abs/1205.3030 — Notes: [Texts/LambrechtsJohansen2012.txt](Texts/LambrechtsJohansen2012.txt). | Faster giant-core growth through pebble accretion. **Supports:** the `Pebble-assisted` giant-growth option in the aggregate planetary priors. | PlanetaryScienceReferenceCatalog.cs; PlanetarySystemState.cs |
| Mróz, P., Poleski, R., Han, C., Udalski, A., Gould, A., Ryu, Y.-H., Yee, J. C., Shin, I.-G., Skowron, J., Szymański, M. K., et al. (2020). *No large population of unbound or wide-orbit Jupiter-mass planets*. *The Astrophysical Journal Letters*, *903*(1), L11. https://arxiv.org/abs/2009.12377 — Notes: [Texts/Mroz2020.txt](Texts/Mroz2020.txt). | Constrains the abundance of free-floating or very wide-orbit giant planets. **Supports:** keeping rogue-world settings as bounded policy choices rather than a dominant default channel. | PlanetaryScienceReferenceCatalog.cs; PlanetarySystemState.cs |
| *StarGen deterministic planet formation specification* (2026). [Texts/planets.md](Texts/planets.md). | Internal implementation-facing design specification translating the planetary research review into a deterministic pipeline for StarGen. Defines the recommended stage order, the shared system-level latent variables, provenance expectations, and the distinction between aggregate upstream formation controls and direct single-planet controls. **Supports:** the `0.8.6.0` retrofit that adds a shared aggregate planetary profile and derived planetary-system state without rewriting the existing generator stack. | PlanetaryGenerationProfile.cs; PlanetarySystemState.cs; SystemPlanetGenerator.cs; PlanetGenerator.cs; Galaxy/System studio planetary controls |

---

## Planet demographics, small-body placement, moons, and habitability calibration

**Current use in StarGen:** `src/domain/system/SystemPlanetGenerator.cs`, `src/domain/generation/generators/planet/PlanetAtmosphereGenerator.cs`, `src/domain/system/SystemMoonGenerator.cs`, `src/domain/system/SystemAsteroidGenerator.cs`, `src/domain/population/ProfileGenerator.cs`, `src/domain/population/BiologySupportEvaluator.cs`, and the related population/system tests.

| Citation | Summary | Used in |
|----------|----------|---------|
| Fulton, B. J., Petigura, E. A., Howard, A. W., Isaacson, H., Marcy, G. W., Cargile, P. A., Hebb, L., Weiss, L. M., Johnson, J. A., Morton, T. D., Sinukoff, E., Crossfield, I. J. M., & Hirsch, L. A. (2017). *The California-Kepler Survey. III. A gap in the radius distribution of small planets*. *The Astronomical Journal*, *154*(3), 109. https://arxiv.org/abs/1703.10375 — Notes: [Texts/Fulton2017.txt](Texts/Fulton2017.txt). | Observed small-planet radius distribution is split, not continuous; close-in irradiated planets are less likely to retain the same envelopes as cooler cousins. **Supports:** using loss-regime context to bias hot close-in planets toward rocky or stripped outcomes instead of treating all sub-Neptune outcomes as equally likely. | SystemPlanetGenerator.cs; PlanetAtmosphereGenerator.cs; TestSystemPlanetGenerator.cs |
| Fischer, D. A., & Valenti, J. (2005). *The planet-metallicity correlation*. *The Astrophysical Journal*, *622*(2), 1102-1117. PDF: https://www.astro.ucla.edu/~aes/AST278/reading/Fischer_2005_ApJ_622_1102.pdf — Notes: [Texts/FischerValenti2005.txt](Texts/FischerValenti2005.txt). | Giant planets are more common around metal-rich host stars. **Supports:** treating metallicity coupling as a real upstream formation constraint that changes giant-planet weighting and downstream system architecture. | PlanetarySystemState.cs; SystemPlanetGenerator.cs; Galaxy/System planetary controls |
| Canup, R. M., & Ward, W. R. (2006). *A common mass scaling for satellite systems of gaseous planets*. *Nature*, *441*, 834-839. https://doi.org/10.1038/nature04860 — Notes: [Texts/CanupWard2006.txt](Texts/CanupWard2006.txt). | Regular satellite systems around gas giants are constrained by circumplanetary-disk formation, implying richer but not arbitrary moon systems for suitable giant hosts. **Supports:** moon-count and regular-vs-captured differences that depend on host class and snow-line context. | SystemMoonGenerator.cs; TestSystemMoonGenerator.cs |
| DeMeo, F. E., & Carry, B. (2014). *Solar System evolution from compositional mapping of the asteroid belt*. *Nature*, *505*, 629-634. https://arxiv.org/abs/1310.3846 — Notes: [Texts/DeMeoCarry2014.txt](Texts/DeMeoCarry2014.txt). | Asteroid-belt composition changes with heliocentric distance; outer regions preserve more primitive and volatile-rich bodies. **Supports:** cold-belt and outer-reservoir composition shifts for asteroid/comet placement. | SystemAsteroidGenerator.cs; TestSystemAsteroidGenerator.cs |
| Lamy, P. L., Toth, I., Fernandez, Y. R., & Weaver, H. A. (2004). *The sizes, shapes, albedos, and colors of cometary nuclei*. In *Comets II* (pp. 223-264). PDF: https://physics.ucf.edu/~yfernandez/papers/comets2chapter/comets2reprint.pdf — Notes: [Texts/Lamy2004.txt](Texts/Lamy2004.txt). | Comet nuclei are dark, volatile-rich, low-density bodies distinct from ordinary asteroids. **Supports:** treating comet-leaning outer reservoirs and icy primitive belts as a real compositional branch rather than a cosmetic asteroid subtype. | SystemAsteroidGenerator.cs; comet-facing small-body assumptions |
| Kopparapu, R. K., Ramirez, R. M., SchottelKotte, J., Kasting, J. F., Domagal-Goldman, S., & Eymet, V. (2014). *Habitable zones around main-sequence stars: Dependence on planetary mass*. *The Astrophysical Journal Letters*, *787*(2), L29. https://arxiv.org/abs/1404.5292 — Notes: [Texts/Kopparapu2014.txt](Texts/Kopparapu2014.txt). | Habitable-zone boundaries depend on planet mass; habitability is better treated as a weighted orbital constraint than a simple binary label. **Supports:** habitable-zone-alignment fields, flux-aware environment profiles, and weighted biosphere gating. | PlanetarySystemState.cs; ProfileGenerator.cs; BiologySupportEvaluator.cs; TestProfileGenerator.cs |
| Heller, R., & Barnes, R. (2013). *Exomoon habitability constrained by illumination and tidal heating*. *Astrobiology*, *13*(1), 18-46. https://arxiv.org/abs/1209.5323 — Notes: [Texts/HellerBarnes2013.txt](Texts/HellerBarnes2013.txt). | Exomoon viability depends on a bounded tidal-heating window: too little can leave icy moons inert, moderate heating can help, and too much becomes harmful. **Supports:** non-cosmetic tidal-heating effects in moon biology and downstream habitability support. | BiologySupportEvaluator.cs; TestBiologySupportEvaluator.cs |

---

## Life potential, biospheres, and civilization filters

**Current use in StarGen:** `src/domain/generation/GenerationUseCaseSettings.cs`, `src/domain/generation/parameters/LifeScienceReferenceCatalog.cs`, `src/domain/population/LifePotentialModeling.cs`, `src/domain/population/BiologySupportEvaluator.cs`, `src/domain/population/PopulationLikelihood.cs`, `src/domain/population/PopulationProbability.cs`, `src/domain/population/PopulationGenerator.cs`, and Galaxy Studio life-model controls.

| Citation | Summary | Used in |
|----------|----------|---------|
| Lineweaver, C. H., & Davis, T. M. (2002). *Does the rapid appearance of life on Earth suggest that life is common in the Universe?* arXiv:astro-ph/0205014. Notes: [Texts/LineweaverDavis2002.txt](Texts/LineweaverDavis2002.txt). | Early terrestrial biogenesis can be read as support for an optimistic simple-life prior on Earth-like worlds, with explicit caution about assumptions and selection effects. **Supports:** the `Rapid Biospheres` model raising abiogenesis odds without automatically making complex life or civilizations common. | LifeScienceReferenceCatalog.cs; LifePotentialModeling.cs; BiologySupportEvaluator.cs |
| Spiegel, D. S., & Turner, E. L. (2012). *Bayesian analysis of the astrobiological implications of life's early emergence on Earth*. arXiv:1107.3835. Notes: [Texts/SpiegelTurner2012.txt](Texts/SpiegelTurner2012.txt). | Early life on Earth alone does not strongly prove that abiogenesis is easy everywhere. **Supports:** keeping the default life model cautious and separating habitable conditions from guaranteed biosphere emergence. | LifeScienceReferenceCatalog.cs; LifePotentialModeling.cs; BiologySupportEvaluator.cs |
| Forgan, D. H., & Rice, K. (2010). *Numerical Testing of The Rare Earth Hypothesis using Monte Carlo Realisation Techniques*. arXiv:1001.1680. Notes: [Texts/ForganRice2010.txt](Texts/ForganRice2010.txt). | Rare Earth style filters can still allow microbial life while making complex or intelligent life much rarer. **Supports:** the `Rare Complex Life` model family. | LifeScienceReferenceCatalog.cs; LifePotentialModeling.cs; BiologySupportEvaluator.cs; PopulationGenerator.cs |
| Mills, D. B., Macalady, J. L., Frank, A., & Wright, J. T. (2024). *A reassessment of the "hard-steps" model for the evolution of intelligent life*. arXiv:2408.10293. Notes: [Texts/Mills2024.txt](Texts/Mills2024.txt). | Argues that long environmental windows may matter more than intrinsically improbable hard steps. **Supports:** the `Environmental Windows` model, where long stable habitable periods materially improve complex-life and civilization odds. | LifeScienceReferenceCatalog.cs; LifePotentialModeling.cs; BiologySupportEvaluator.cs |
| Balbi, A., & Frank, A. (2023). *The Oxygen Bottleneck for Technospheres*. arXiv:2308.01160. Notes: [Texts/Balbi2023.txt](Texts/Balbi2023.txt). | Technological civilizations may require stronger oxygen-rich atmospheric conditions than complex life alone. **Supports:** stricter civilization gating on breathable or oxygen-rich worlds. | LifeScienceReferenceCatalog.cs; BiologySupportEvaluator.cs; PopulationGenerator.cs |

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
| Hart, R. E., Bamford, S. P., Hayes, W. B., Cardamone, C. N., Keel, W. C., Kruk, S. J., Lintott, C. J., Masters, K. L., Simmons, B. D., & Smethurst, R. J. (2017). *Galaxy Zoo and SpArcFiRe: Constraints on spiral arm formation mechanisms from spiral arm number and pitch angles*. *Monthly Notices of the Royal Astronomical Society*. https://arxiv.org/abs/1708.04628 - Notes: [Texts/Hart2017.txt](Texts/Hart2017.txt). | Large-sample arm-number and pitch-angle study; bars and many-arm structures correlate with looser pitch angles, and the observed diversity argues against one universal arm-formation mechanism. **Supports:** spiral-arm count, pitch-angle, and arm-mechanism controls as simplified observationally motivated structure parameters. | GalaxyScienceReferenceCatalog.cs; GalaxyRealismProfileBuilder.cs; GalaxyScientificFieldEvaluator.cs |
| Lingard, T., Masters, K. L., Krawczyk, C., Lintott, C., Kruk, S., Simmons, B., Keel, W., Nichol, R., & Baeten, E. (2021). *Galaxy Zoo Builder: Morphological Dependence of Spiral Galaxy Pitch Angle*. *Monthly Notices of the Royal Astronomical Society*, *504*(3), 3364-3374. https://arxiv.org/abs/2105.04500 - Notes: [Texts/Lingard2021.txt](Texts/Lingard2021.txt). | Pitch angle is treated as a real morphology observable and is consistent with transient/recurrent winding interpretations. **Supports:** keeping spiral-arm pitch and arm-mode controls while documenting them as simplified proxies instead of a single solved formation theory. | GalaxyScienceReferenceCatalog.cs; GalaxyScientificFieldEvaluator.cs |
| Rodriguez, S., & Padilla, N. D. (2013). *The Intrinsic Shape of Galaxies in SDSS/Galaxy Zoo*. https://arxiv.org/abs/1306.3264 - Notes: [Texts/RodriguezPadilla2013.txt](Texts/RodriguezPadilla2013.txt). | Intrinsic-shape study for spirals and ellipticals, including disc ellipticity and axis-ratio behavior. **Supports:** ellipticity as a real structure parameter rather than a purely cosmetic stretch control. | GalaxyScienceReferenceCatalog.cs; SpiralDensityModel.cs; EllipticalDensityModel.cs |

---

## Orbital stability (planet spacing)

**Current use in StarGen:** ~10 mutual Hill radii for long-term stability. [src/domain/system/OrbitalMechanics.gd](../src/domain/system/OrbitalMechanics.gd) (line 424), [OrbitSlotGenerator.gd](../src/domain/system/OrbitSlotGenerator.gd) (line 139), [SystemValidator.gd](../src/domain/system/SystemValidator.gd) (line 268).

| Citation | Summary | Used in |
|----------|----------|---------|
| Chambers, J. E., Wetherill, G. W., & Boss, A. P. (1996). The stability of multi-planet systems. *Icarus*, *119*(2), 261–268. https://doi.org/10.1006/icar.1996.0019 | N-body stability; systems with Δ &lt; 10 mutual Hill radii always unstable (log t = bΔ + c); likely unstable for Δ &gt; 10 on longer timescales. | OrbitalMechanics.gd; OrbitSlotGenerator.gd; SystemValidator.gd; Roadmap (Scientific calibration) |

---

## Changelog

- Hart et al. (2017), Lingard et al. (2021), and Rodriguez & Padilla (2013) added under Galaxy morphology and structure to support the `0.8.11.0` scientific-parameter audit pass that tightened source coverage for spiral arm count, pitch, arm mechanisms, and ellipticity.
- Lineweaver & Davis (2002), Spiegel & Turner (2012), Forgan & Rice (2010), Mills et al. (2024), and Balbi & Frank (2023) added under Life potential, biospheres, and civilization filters to support the `0.8.9.0` life-model replacement that moves Galaxy Studio from a `Life Potential` slider to sourced model selection and threads those models into biology, complex-life, and sentience gating.
- Fulton et al. (2017), Fischer & Valenti (2005), Canup & Ward (2006), DeMeo & Carry (2014), Lamy et al. (2004), Kopparapu et al. (2014), and Heller & Barnes (2013) added under Planet demographics, small-body placement, moons, and habitability calibration to support the `0.8.7.0` follow-on pass that tied atmosphere retention, moon architecture, outer-belt composition, and biosphere gating more tightly to upstream planetary-system state.
- Chen & Kipping (2017), Otegi et al. (2020), Owen & Wu (2017), Ginzburg et al. (2018), Mordasini et al. (2007), Lambrechts & Johansen (2012), and Mróz et al. (2020) added under Planetary retrofit and deterministic formation surrogates to support the `0.8.8.0` cleanup that removed the uncited `Legacy` size model, added Otegi as the second supported mass-radius option, and rewrote planetary-prior help around named literature-backed model branches.
- Added `Sources/Texts/planets.md` under Planetary retrofit and deterministic formation surrogates to document the `0.8.6.0` aggregate planetary retrofit and its upstream-vs-object control split.
- Cummings et al. (2018), Kirkpatrick et al. (2000, 2011, 2024), Moe & Di Stefano (2017), and Tokovinin (2021) added under Expanded stellar populations to support the `0.8.4.0` stellar-offerings pass (brown dwarfs, evolved stars, white dwarfs, and stronger multiplicity architecture).
- Li et al. (2023) added: reviewed from abridged full text in Texts/Li2023.txt; annotated under Stellar distribution (supports current M-dwarf band; fidelity: metallicity/age-dependent IMF).
- Bovy (2017) added: reviewed from abridged full text in Texts/Bovy2017.txt; annotated under Stellar distribution and Solar neighborhood density (supports current use; fidelity notes for mass density and dn/dM).
- Initial structure and APA 7; placeholders for Chambers 1996 and benchmark topics (stellar, density, exoplanets). ToReview.md holds abstract-only papers until full text is added.
