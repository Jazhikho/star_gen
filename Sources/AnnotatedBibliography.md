# Annotated Bibliography (APA style)

Cross-checked against `Sources/Texts` inventory. All entries are derived from the
curated `_apa.tsv` and per-paper StarGen applicability notes in `Texts/*.txt`.

Entries marked **(APA incomplete)** require a human contributor to verify the citation
against NASA ADS, arXiv, or the journal directly and update `_apa.tsv`.
Entries marked **(abstract unverified)** require a human to read the PDF and annotate
the Parameters and Opposing Findings sections in the matching `.txt` file.

## Open-access ingest status (2026-04-27 batch)

The batch requested on 2026-04-27 was ingested to `Sources/Texts/*.txt` using direct OA retrieval
with full-text extraction (PDF when retrievable, otherwise HTML full text).

- Retrieval script: `Sources/DownloadOpenAccessSources.py`
- Retrieval log: `Sources/OpenAccessRetrievalLog_2026-04-27.json`
- Manual-action items are listed in: `Sources/Followup.md` (section "Open-access retrieval follow-up (2026-04-27 batch)")
- Existing stems reused without overwrite: `Herbort2024`, `KarakatsanisMamassis2023`

---

## Bains2004

**Domain:** astrobiology/biochemistry

**APA:** Bains, W. (2004). Many chemistries could be used to build living systems. Astrobiology, 4(2), 137-167. https://doi.org/10.1089/153110704323175124

**StarGen annotation:** Alternative solvents and biochemistries (NH₃, liquid N₂). Raises the question of whether StarGen's habitability model should expose a 'solvent type' override beyond water-only habitability assumptions.


## BainsEtAl2024

**Domain:** astrobiology / alternative solvents (habitability chemistry); includes Venus-style concentrated sulfuric acid as one planetary scenario.

**APA:** Bains, W., Petkowski, J. J., & Seager, S. (2024). Alternative solvents for life: Framework for evaluation, current status, and future research. Astrobiology, 24, 1231–1256. https://doi.org/10.1089/ast.2024.0004 (preprint: https://arxiv.org/abs/2401.07296 )

**StarGen annotation:** Use as the primary four-criterion rubric (occurrence, solvation, solute stability, solvent chemical functionality) for solvent plausibility beyond liquid water; treat concentrated H₂SO₄ and liquid CO₂ as distinct assumption profiles. Venus cloud habitability remains one application—see also Venus-specific sources in corpus (e.g. PetkowskiEtAl2020).


## Balbi2023

**Domain:** astrobiology/technospheres

**APA:** Balbi, A., & Frank, A. (2024). The oxygen bottleneck for technospheres. Nature Astronomy, 8(1), 39-43. https://doi.org/10.1038/s41550-023-02112-8

**StarGen annotation:** O₂ concentration as prerequisite for technology-bearing civilisations. Technosphere emergence in StarGen's population module should require atmospheric O₂ accumulation — not just liquid water or insolation.


## Behroozi2019

**Domain:** galaxy formation/stellar-halo assembly

**APA:** Behroozi, P., Wechsler, R. H., Hearin, A. P., & Conroy, C. (2019). UNIVERSEMACHINE: The correlation between galaxy growth and dark halo assembly from z = 0-10. Monthly Notices of the Royal Astronomical Society, 488(3), 3143-3194. https://doi.org/10.1093/mnras/stz1182

**StarGen annotation:** UniverseMachine: galaxy SFR correlated with halo assembly history z=0–10. Informs stellar population age distributions as a function of halo mass in StarGen's galaxy generator.


## BenistyEtAl2021

**Domain:** circumplanetary disks/moon formation

**APA:** Benisty, M., et al. (2021). A circumplanetary disk around PDS 70 c. Astronomy and Astrophysics, 652, L8. https://doi.org/10.1051/0004-6361/202140806

**StarGen annotation:** First resolved circumplanetary disk (PDS 70c); confirms ongoing moon formation around forming giant planets. CPD presence and mass set the moon-forming budget for giant planets in StarGen.


## BergstenEtAl2023

**Domain:** planet occurrence/M-dwarfs

**APA:** Bergsten, G. J., Pascucci, I., Hardegree-Ullman, K. K., Fernandes, R. B., Christiansen, J. L., & Mulders, G. D. (2023). No evidence for more Earth-sized planets in the habitable zone of Kepler's M versus FGK stars. The Astronomical Journal, 166(6), 234. https://doi.org/10.3847/1538-3881/ad03ea

**StarGen annotation:** No enhanced HZ Earth-size rate around M vs FGK stars — contradicts earlier claims. Partly implemented: StarGen's mid-to-late M demographic regime does not boost HZ rocky occurrence over FGK, even though close-in small-planet pressure can be higher.


## BernardinelliEtAl2022

**Domain:** TNO demographics/outer belt

**APA:** Bernardinelli, P. H., Bernstein, G., Sako, M., et al. (2022). A search of the full six years of the Dark Energy Survey for outer Solar System objects. The Planetary Science Journal, 3(10), 215. https://doi.org/10.3847/PSJ/acf869

**StarGen annotation:** DES 6-yr survey: TNO size-frequency distribution and orbital architecture beyond 30 AU. Constrains outer-belt object counts, inclination distribution, and size-frequency slope in StarGen.


## BiassoniEtAl2023 **(APA incomplete)**

**Domain:** stellar/galactic structure

**APA:** Biassoni, F., et al. (2023). Match PDF to ADS and complete author list: repository stem BiassoniEtAl2023.

**StarGen annotation:** Abstract not yet transcribed to this note — verify PDF and apply to relevant galactic structural parameters.


## BlandHawthornGerhard2016

**Domain:** Milky Way structure (definitive review)

**APA:** Bland-Hawthorn, J., & Gerhard, O. (2016). The galaxy in context: structural and kinematic properties of the Milky Way. Annual Review of Astronomy and Astrophysics, 54, 529-596. https://doi.org/10.1146/annurev-astro-081915-023441

**StarGen annotation:** Comprehensive MW reference: thin-disk scale length ~2.6 kpc, thick-disk ~2.0 kpc, bar ~4–5 kpc, total stellar mass ~5×10¹⁰ M☉. Core reference for all MW structural parameters in StarGen's galaxy generator.


## Bovy2017

**Domain:** galactic disk/mono-abundance populations

**APA:** Bovy, J. (2017). Stellar inventory of the solar neighbourhood using Gaia DR1. Monthly Notices of the Royal Astronomical Society, 470(1), 1360-1387. https://doi.org/10.1093/mnras/stx1277

**StarGen annotation:** APOGEE: mono-abundance populations show a continuous range of scale heights correlated with [α/Fe] — the disk is not two discrete layers. Informs metallicity-gradient and age-gradient models in StarGen.


## Bryson2021

**Domain:** planet occurrence/HZ/Kepler reliability

**APA:** Bryson, S., et al. (2021). The occurrence of Earth-sized planets in the habitable zone of Sun-like stars. The Astronomical Journal, 161(1), 36. https://doi.org/10.3847/1538-3881/abd022

**StarGen annotation:** HZ occurrence rates with updated reliability corrections. Partly implemented as the FGK/GK HZ rocky occurrence scalar in `PlanetarySystemState`; exact eta-Earth tables remain human-verification follow-up.


## Chabrier2003

**Domain:** initial mass function (defining reference)

**APA:** Chabrier, G. (2003). Galactic stellar and substellar initial mass function. Publications of the Astronomical Society of the Pacific, 115(809), 763-795. https://doi.org/10.1086/376393

**StarGen annotation:** IMF: lognormal below 1 M☉ (disk single-star: m_c ≈ 0.08 M☉, σ ≈ 0.7; system IMF: m_c ≈ 0.2 M☉, σ ≈ 0.6), Salpeter power-law above 1 M☉. Brown dwarf number density ≈ stellar number density ~0.1 pc⁻³. Primary star-mass sampling reference for StarGen.


## ChabrierLenoble2023 **(APA incomplete)**

**Domain:** IMF/updated low-mass constraints (2023)

**APA:** Chabrier, G., & Le Noble, M. (2023). Match stem ChabrierLenoble2023 to ADS record for the PDF on file and complete pagination.

**StarGen annotation:** Updated Chabrier IMF with improved low-mass and substellar constraints. May revise the characteristic mass and brown-dwarf boundary priors relative to Chabrier2003.


## ChatterjeeEtAl2026

**Domain:** atmospheric escape / secondary atmospheres / XUV / cosmic shoreline

**APA:** Chatterjee, R. D., & Pierrehumbert, R. T. (2024). Novel physics of escaping secondary atmospheres may shape the cosmic shoreline. arXiv:2412.05188. [Submitted to The Astrophysical Journal — verify published journal citation. Note: arXiv 2024, publication pending 2025/2026.]

**StarGen annotation:** Extends the cosmic shoreline concept to secondary (N₂/CO₂) atmosphere escape. JWST shows many cool rocky exoplanets lack thick atmospheres. Two escape regimes: energy-limited (linear XUV scaling) and collisional-radiative thermostat. Implements two-step atmospheric fate model in StarGen: primordial H/He loss (LugerBarnes2015) followed by secondary atmosphere check, with volcanism revival probability. M4+ planets have extended XUV phase → near-zero secondary atmosphere retention probability without tectonic outgassing.


## ChenKipping2017

**Domain:** mass-radius relations/Forecaster

**APA:** Chen, J., & Kipping, D. (2017). Probabilistic forecasting of the masses and radii of other worlds. The Astrophysical Journal, 834(1), 17. https://doi.org/10.3847/1538-4357/834/1/17

**StarGen annotation:** Probabilistic mass-radius power-law fits by planet class (Terran, Neptunian, Jovian, Stellar). Now partly implemented in StarGen as seeded Chen-Kipping radius scatter plus Terran/Neptunian/Jovian classification probabilities in `PlanetMassRadiusTable`; full Forecaster posterior sampling remains follow-up work.


## Choi2016

**Domain:** stellar evolution/MIST tracks

**APA:** Choi, J., Dotter, A., Conroy, C., Cantiello, M., Paxton, B., & Johnson, B. D. (2016). MESA isochrones and stellar tracks (MIST). I. Solar-scaled models. The Astrophysical Journal, 823(2), 102. https://doi.org/10.3847/0004-637X/823/2/102

**StarGen annotation:** MIST v1: stellar evolution tracks and isochrones across all masses and metallicities (pre-MS through post-MS). Canonical reference for L, R, Teff as functions of mass, age, and [Fe/H] in StarGen stellar lifecycle.


## Chowdhury2022 **(APA incomplete)**

**Domain:** galactic structure/morphology

**APA:** Chowdhury, A. (2022). Map PDF to exact venue (non-astronomy item in corpus); verify before StarGen cross-use.

**StarGen annotation:** Abstract needs verification — apply to MW or galaxy morphology parameters in StarGen once confirmed.


## Comin2013

**Domain:** economic complexity

**APA:** Comin, D., & Lashkari, Y. (2013). Technology diffusion and geographic convergence. Journal of Economic Growth, 18(4), 431-458. https://doi.org/10.1007/s10881-013-9190-3

**StarGen annotation:** Economic complexity index. Relevant to civilisation/technology framework in StarGen's population module only — not to physical generation mechanics.


## CominMestieri2013

**Domain:** economic complexity/income growth

**APA:** Comin, D., & Mestieri, M. (2013). If technology has arrived everywhere, why has income diverged? NBER Working Paper 19010. https://doi.org/10.3386/w19010

**StarGen annotation:** Economic complexity and long-run income growth. Population/civilisation framework applicability only — not physical generation.


## Conselice2014

**Domain:** galaxy number counts/evolution

**APA:** Conselice, C. J., Wilkinson, A., Duncan, K., & Mortlock, A. (2016). The evolution of galaxy number density at Z < 8 and its implications. The Astrophysical Journal, 830(2), 83. https://doi.org/10.3847/0004-637X/830/2/83

**StarGen annotation:** ~2 trillion galaxies in the observable universe (revised upward); galaxy number density evolution over cosmic time. Relevant to universe-scale context and galaxy-count priors.


## CuiEtAl2026

**Domain:** exoplanet demographics / TESS / FGK occurrence rates

**APA:** Cui, K., Armstrong, D. J., Hadjigeorghiou, A., Lafarga, M., et al. (2026). Demographics of close-in TESS exoplanets orbiting FGK main-sequence stars. Monthly Notices of the Royal Astronomical Society, 546(2), stag022. https://doi.org/10.1093/mnras/stag022 arXiv: 2601.09492. Data: https://doi.org/10.5281/zenodo.17804280

**StarGen annotation:** TESS-era occurrence rates for close-in planets (0.5–16 day, 2–20 R⊕) around FGK stars. Partly implemented as a close-in FGK hot-giant suppression scalar and provenance hook; exact 10x10 period-radius bins and Neptunian desert survivor modeling remain follow-up.


## Cummings2018

**Domain:** white dwarf/initial-final mass relation

**APA:** Cummings, J. D., Kalirai, J. S., Tremblay, P.-E., & Ramirez-Ruiz, E. (2018). The initial-final mass relation among white dwarfs in wide binaries. The Astrophysical Journal, 862(2), 161. https://doi.org/10.3847/1538-4357/aacc31

**StarGen annotation:** IFMR for white dwarfs: maps ZAMS mass to WD remnant mass. Relevant to stellar endpoint generation and compact-object mass assignment in StarGen.


## DeMeoCarry2014

**Domain:** asteroid taxonomy/compositional mapping

**APA:** DeMeo, F. E., & Carry, B. (2014). Solar system evolution from compositional mapping of the asteroid belt. Nature, 505(7485), 629-634. https://doi.org/10.1038/nature12908

**StarGen annotation:** Spectral taxonomy and heliocentric compositional distribution of asteroids (S/C/X-complex). Reference for asteroid type generation and belt compositional gradients as a function of semi-major axis in StarGen.


## DiazGarcia2016 **(APA incomplete)**

**Domain:** galaxy morphology/bar fraction

**APA:** Diaz-Garcia, R., et al. (2016). Match stem DiazGarcia2016 to ADS for the PDF on file.

**StarGen annotation:** Bar fraction and properties across galaxy types. Relevant to galactic-bar generation in StarGen — bar presence/absence, length, and strength as functions of morphological type.


## DucheneKraus2013

**Domain:** stellar multiplicity (comprehensive review)

**APA:** Duchene, G., & Kraus, A. (2013). Stellar multiplicity and massive binaries. Annual Review of Astronomy and Astrophysics, 51, 269-310. https://doi.org/10.1146/annurev-astro-081710-101722

**StarGen annotation:** Binary fraction vs stellar mass: OB ~70%, solar ~46%, M ~26%, VLM ~15–25%. Log-normal period distribution, near-flat mass ratio distribution. Core reference for binary/multiple star generation in StarGen alongside MoeDiStefano2017.


## FangMargot2013

**Domain:** planetary architecture/mutual inclinations

**APA:** Fang, J., & Margot, J.-L. (2013). Probing the interiors of planets with close-in transiting companions. The Astrophysical Journal, 767(1), 95. https://doi.org/10.1088/0004-637X/767/1/95

**StarGen annotation:** Multi-planet mutual inclinations mostly <5° from Kepler. Underutilized: StarGen does not yet implement a Rayleigh mutual-inclination model; current orbital architecture work records spacing/period diagnostics only.


## Fernandes2019

**Domain:** planet occurrence/cold Jupiters/stellar properties

**APA:** Fernandes, R. B., Mulders, G. D., Pascucci, I., Mordasini, C., & Emsenhuber, A. (2019). Hiding in the haystack: close-in planets in the Kepler period-radius distribution. The Astrophysical Journal Supplement Series, 245(1), 22. https://doi.org/10.3847/1538-4365/ab59ff

**StarGen annotation:** Reviewed but partly implemented. StarGen uses Fernandes2019 to keep giant-planet weighting peaked near the snow-line region instead of rising monotonically outward; exact broken-power-law occurrence rates remain follow-up.


## FischerValenti2005

**Domain:** planet-metallicity correlation

**APA:** Fischer, D. A., & Valenti, J. (2005). The planet-metallicity correlation. The Astrophysical Journal, 622(2), 1102-1117. https://doi.org/10.1086/428383

**StarGen annotation:** Giant planet occurrence ∝ 10^(2[Fe/H]) — a steep metallicity dependence. Critical prior: giant planet frequency is a strong function of stellar metallicity in StarGen system generation.


## ForganRice2010

**Domain:** astrobiology / Rare Earth Hypothesis / SETI / civilization emergence

**APA:** Forgan, D., & Rice, K. (2010). Numerical testing of the Rare Earth Hypothesis using Monte Carlo realization of the galaxy. International Journal of Astrobiology, 9(2), 73–80. https://doi.org/10.1017/S1473550410000030 arXiv: 1001.1680

**StarGen annotation:** 8-gate Monte Carlo Rare Earth model (HZ, stellar type, single star, galactic zone, tidal locking, obliquity stability, giant planet, no inward migration). Implements `CivilizationEmergenceModel` enum in StarGen with per-criterion probability tables. "Soft REH" result: complex life rare but non-zero. Foundation for civilization emergence probability pipeline.


## Fulton2017

**Domain:** radius gap / California-Kepler Survey

**APA:** Fulton, B. J., Petigura, E. A., Howard, A. W., et al. (2017). The California-Kepler Survey. III. A gap in the radius distribution of small planets. The Astronomical Journal, 154(3), 109. https://doi.org/10.3847/1538-3881/aa80eb

**StarGen annotation:** Radius gap at ~1.5-2.0 R⊕ separating rocky super-Earths from volatile-rich mini-Neptunes. Partly implemented: generated close-in planets now carry radius-valley diagnostics and use a period-limited valley regime to reduce intermediate envelope-world weighting. Still needs an explicit bimodal radius-distribution sampler from reviewed occurrence tables.


## GarmaOehmichenEtAl2022 **(APA incomplete)**

**Domain:** planetary atmospheres/spectroscopy

**APA:** Garma-Oehmichen, A., et al. (2022). Match stem GarmaOehmichenEtAl2022 to ADS for the PDF on file.

**StarGen annotation:** Atmospheric characterisation study — abstract needs verification. Apply specific constraints to StarGen's atmosphere generation once confirmed.


## GillisEtAl2026

**Domain:** rocky exoplanets/habitable zone catalogue (2026)

**APA:** Bohl, A., Lawrence, L., Lowry, G., & Kaltenegger, L. (2026). Probing the limits of habitability: A catalogue of rocky exoplanets in the habitable zone. Monthly Notices of the Royal Astronomical Society, 547, ag028. https://doi.org/10.1093/mnras/stag028 (Repository filename GillisEtAl2026; shared first authorship includes Lowry.)

**StarGen annotation:** Catalogue of rocky exoplanets in the habitable zone; probes limits of habitability using updated stellar and planetary parameters. Directly informs HZ rocky-planet parameter ranges in StarGen.


## Ginzburg2018

**Domain:** core-powered mass loss / radius gap

**APA:** Ginzburg, S., Schlichting, H. E., & Sari, R. (2018). Core-powered mass-loss and the radius distribution of small exoplanets. Monthly Notices of the Royal Astronomical Society, 476(1), 759-765. https://doi.org/10.1093/mnras/sty290

**StarGen annotation:** Core-powered mass loss as alternative mechanism for the radius gap; driven by core cooling luminosity/bolometric flux rather than XUV. Partly implemented: the core-powered branch now has a positive radius-valley period slope, separate loss-pressure proxy, and provenance mechanism tag. Full age-dependent population evolution remains follow-up.


## HamiltonEtAl2016 **(APA incomplete)**

**Domain:** planetary dynamics/resonance

**APA:** Hamilton, C., et al. (2016). Match stem HamiltonEtAl2016 to ADS for the PDF on file.

**StarGen annotation:** Resonant chains and orbital architecture in compact systems — abstract needs verification. Apply constraints on period-ratio distributions and resonance placement in multi-planet generation.


## Hart2017 **(APA incomplete)**

**Domain:** brown dwarfs/spectral characterisation

**APA:** Hart, R. E., et al. (2017). Match stem Hart2017 to ADS for the PDF on file.

**StarGen annotation:** Brown dwarf spectral characterisation — abstract needs verification. Relevant to BD spectral type, temperature, and luminosity generation in StarGen.


## Hayden2014 **(APA incomplete)**

**Domain:** galactic chemistry/APOGEE abundance gradients

**APA:** Hayden, M. R., et al. (2014). Match stem Hayden2014 to ADS for the PDF on file.

**StarGen annotation:** APOGEE [α/Fe] vs [Fe/H] bimodality and radial/vertical gradients in MW disk. Informs metallicity and α-abundance gradient model in StarGen's galactic generator.


## HeEtAl2020 **(APA incomplete)**

**Domain:** planetary architecture/multi-planet statistics

**APA:** He, Y., et al. (2020). Match stem HeEtAl2020 to ADS for the PDF on file.

**StarGen annotation:** Multi-planet system spacing, multiplicity, and uniformity statistics. Partly implemented: generated slots and planets now record period-ratio, architecture mass-proxy, and mutual-Hill spacing diagnostics under the active architecture spacing policy. Full SysSim/AMD population modeling remains follow-up.


## HellerBarnes2013

**Domain:** exomoon habitability/tidal heating

**APA:** Heller, R., & Barnes, R. (2013). Exomoon habitability constrained by illumination and tidal heating. International Journal of Astrobiology, 12(4), 313-323. https://doi.org/10.1017/S1473550413000300

**StarGen annotation:** Exomoon habitable edge; tidal heating is both an enabler and a habitability hazard. StarGen should model tidal heating for inner moons of giant planets and apply a distinct threshold separate from insolation-only HZ.


## Herbort2024 **(APA incomplete)**

**Domain:** atmospheric/geochemical evolution

**APA:** Herbort, O., et al. (2024). Match stem Herbort2024 to ADS for the PDF on file.

**StarGen annotation:** Atmospheric or geochemical modelling — abstract needs verification. Apply constraints to StarGen's atmosphere-composition generation once confirmed.


## HuntVasiliev2025

**Domain:** Milky Way dynamics/Gaia review (2025)

**APA:** Hunt, J. A. S., & Vasiliev, E. (2025). Milky Way dynamics in light of Gaia. New Astronomy Reviews, 100, 101721. https://doi.org/10.1016/j.newar.2025.101721

**StarGen annotation:** MW dynamics in light of Gaia data — comprehensive 2025 review. Updates structural and kinematic parameters of the MW disk, bar, and halo relevant to StarGen's galactic generator.


## Hurley2000

**Domain:** stellar evolution/analytic formulae

**APA:** Hurley, J. R., Pols, O. R., & Tout, C. A. (2000). Comprehensive analytic formulae for stellar evolution and single star masses. Monthly Notices of the Royal Astronomical Society, 315(3), 543-569. https://doi.org/10.1046/j.1365-8711.2000.03426.x

**StarGen annotation:** Rapid analytic formulae for stellar evolution (L, R, Teff vs mass, age, Z) across full MS and post-MS lifecycle. Key reference for fast stellar parameter computation in StarGen.


## Izidoro2017

**Domain:** planet formation/resonant chain disruption

**APA:** Izidoro, A., et al. (2017). Breaking the chains: hot super-Earth systems from migration and disruption of compact resonant chains. Monthly Notices of the Royal Astronomical Society, 470(2), 1750-1770. https://doi.org/10.1093/mnras/stx1232

**StarGen annotation:** Reviewed but partly implemented. StarGen uses Izidoro2017 to shape compact inner architecture pressure and Type-I migration provenance; explicit resonant-chain generation and breakup remain follow-up.


## KarakatsanisMamassis2023

**Domain:** energy use/land carrying capacity

**APA:** Karakatsanis, G., & Mamassis, N. (2023). Energy use and land carrying capacity in agrarian societies. Land, 12(8), 1603. https://doi.org/10.3390/land12081603

**StarGen annotation:** Non-astronomy paper on agrarian society energy use and land carrying capacity. Potentially relevant to StarGen's civilisation carrying-capacity model in the population framework — verify abstract before applying.


## Kasting1993

**Domain:** habitable zone (original Kasting+1993)

**APA:** Kasting, J. F., Whitmire, D. P., & Reynolds, R. T. (1993). Habitable zones around main sequence stars. Icarus, 101(1), 108-128. https://doi.org/10.1006/icar.1993.1010

**StarGen annotation:** Original HZ definition: conservative inner 0.95 AU, outer 1.67 AU for the Sun. Historical baseline superseded quantitatively by Kopparapu2013. StarGen should cite both and note that Kopparapu2013 values are the current standard.


## KavelaarsEtAl2023 **(APA incomplete)**

**Domain:** TNO/outer Solar System survey

**APA:** Kavelaars, J. J., et al. (2023). Match stem KavelaarsEtAl2023 to ADS for the PDF on file.

**StarGen annotation:** TNO population: orbital structure, size distribution, detection biases. Constrains outer-belt architecture parameters (inclination, size-frequency slope, number density) in StarGen.


## Kennicutt1998

**Domain:** star formation rate / Schmidt-Kennicutt law

**APA:** Kennicutt, R. C., Jr. (1998). Star formation in galaxies along the Hubble sequence. Annual Review of Astronomy and Astrophysics, 36, 189-232. https://doi.org/10.1146/annurev.astro.36.1.189

**StarGen annotation:** SFR surface density ∝ gas surface density^1.4. Core reference for galaxy-level star formation rates as a function of gas content in StarGen's galaxy generator.


## KhoperskovEtAl2024 **(APA incomplete)**

**Domain:** galactic dynamics/MW bar (2024)

**APA:** Khoperskov, S., et al. (2024). Match stem KhoperskovEtAl2024 to ADS for the PDF on file.

**StarGen annotation:** MW bar dynamics study (2024) — abstract needs verification. Apply updated bar parameters to StarGen's galactic generator once confirmed.


## Kirkpatrick2000

**Domain:** L/T dwarf spectral classification

**APA:** Kirkpatrick, J. D., et al. (2000). Brown dwarfs and the IMF: young clusters versus the field. The Astronomical Journal, 120(1), 447-472. https://doi.org/10.1086/301146

**StarGen annotation:** Definition of L and T spectral classes for cool dwarfs and brown dwarfs. Reference for spectral type → Teff/luminosity mapping for substellar objects in StarGen.


## Kirkpatrick2011

**Domain:** Y dwarf spectral class definition

**APA:** Kirkpatrick, J. D., et al. (2011). Further definitions of spectral type Y candidates and brown dwarf spectroscopic standards. The Astrophysical Journal Supplement Series, 197(2), 19. https://doi.org/10.1088/0067-0049/197/2/19

**StarGen annotation:** Definition of the Y spectral class (T_eff < ~500 K). Extends StarGen's spectral classification below T dwarfs into the coldest substellar regime.


## Kirkpatrick2024

**Domain:** ultracool dwarf census / L,T,Y dwarfs (2024)

**APA:** Kirkpatrick, J. D., et al. (2024). The L, T, and Y dwarf compendium: 300 objects near the Sun. The Astrophysical Journal Supplement Series, 274(2), 39. https://doi.org/10.3847/1538-4365/ad5ad0

**StarGen annotation:** Updated census of ~300 nearby L/T/Y dwarfs; occurrence rates and Teff distributions. Updates brown-dwarf frequency priors and spectral-type number counts in StarGen.


## Kopparapu2013

**Domain:** habitable zone / updated estimates (canonical)

**APA:** Kopparapu, R. K., et al. (2013). Habitable zones around main-sequence stars: new estimates. The Astrophysical Journal, 765(2), 131. https://doi.org/10.1088/0004-637X/765/2/131

**StarGen annotation:** Updated HZ using HITRAN 2008/HITEMP 2010 absorption databases: Solar conservative HZ 0.99–1.70 AU; parametric polynomial fits for Teff 2600–7200 K. Primary HZ calculation reference for StarGen. Note: cloud radiative effects not included.


## Kopparapu2014

**Domain:** habitable zone / planet mass dependence

**APA:** Kopparapu, R. K., et al. (2014). Habitable zones around main-sequence stars: dependence on planetary mass. The Astrophysical Journal Letters, 787(2), L29. https://doi.org/10.1088/2041-8205/787/2/L29

**StarGen annotation:** HZ limits corrected for planet mass — more/less massive planets have different greenhouse efficiency. StarGen should account for planet-mass-dependent HZ boundaries rather than a single fixed HZ per star.


## Kormendy2009

**Domain:** galaxy morphology / pseudobulges vs classical bulges

**APA:** Kormendy, J., et al. (2009). Structure and formation of elliptical and spheroidal galaxies. The Astrophysical Journal Supplement Series, 182(1), 216-309. https://doi.org/10.1088/0067-0049/182/1/216

**StarGen annotation:** Pseudobulge vs classical bulge distinction; pseudobulges form by secular disk evolution, not mergers. Relevant to galaxy morphological type classification and bulge/bar assignment in StarGen's galaxy generator.


## KrissansenTotton2018

**Domain:** biosignatures / atmospheric disequilibrium

**APA:** Krissansen-Totton, J., Garland, R., Irwin, P., & Catling, D. C. (2018). Detectability of biosignatures in anoxic atmospheres with the James Webb Space Telescope: a TRAPPIST-1e case study. Astrobiology, 18(6), 630-653. https://doi.org/10.1089/ast.2017.1723

**StarGen annotation:** Atmospheric disequilibrium (O₂+CH₄ coexistence) as a biosignature; quantified for modern and early Earth scenarios. Relevant to atmosphere classification and life-indicator flag generation for rocky planets in StarGen.


## Kroupa2001

**Domain:** initial mass function / broken power-law

**APA:** Kroupa, P. (2001). On the variation of the initial mass function. Monthly Notices of the Royal Astronomical Society, 322(2), 231-246. https://doi.org/10.1046/j.1365-8711.2001.04022.x

**StarGen annotation:** Kroupa broken power-law IMF: Γ = 1.3 for m > 0.5 M☉. Alternative to Chabrier2003. StarGen should expose an IMF-family selector (Chabrier lognormal vs Kroupa power-law) since the choice affects stellar mass distribution.


## KunimotoEtAl2022 **(APA incomplete)**

**Domain:** planet occurrence / M-dwarfs / TESS (2022)

**APA:** Kunimoto, M., et al. (2022). Match stem KunimotoEtAl2022 to ADS for the PDF on file.

**StarGen annotation:** Draft occurrence note needs reconciliation: the current note metadata and abstract appear inconsistent. Underutilized until title, target stellar population, and key equations are verified.


## LambrechtsJohansen2012

**Domain:** pebble accretion / giant planet formation

**APA:** Lambrechts, M., & Johansen, A. (2012). Rapid growth of gas-giant cores by pebble accretion. Astronomy and Astrophysics, 544, A32. https://doi.org/10.1051/0004-6361/201219127

**StarGen annotation:** Reviewed but partly implemented. StarGen uses LambrechtsJohansen2012 for the pebble-assisted gas-giant branch and fragmentation sensitivity; explicit pebble-isolation mass and growth-time modeling remain follow-up.


## Laskar2017

**Domain:** orbital stability / secular chaos

**APA:** Laskar, J., Fienga, A., Gastineau, M., & Manche, H. (2017). Strong chaos induced by close encounters with Ceres and Vesta. Astronomy and Astrophysics, 598, L5. https://doi.org/10.1051/0004-6361/201629509

**StarGen annotation:** Long-term secular orbital chaos and AMD-stability context. Underutilized for current orbital-slot generation; the active stability pass records mutual-Hill and period-ratio diagnostics only and does not yet implement Laskar-style AMD classification.


## Li2023 **(APA incomplete)**

**Domain:** galactic/stellar physics (2023)

**APA:** Li, G., et al. (2023). Match stem Li2023 to ADS for the PDF on file (several Li et al. 2023 papers exist).

**StarGen annotation:** Abstract needs verification — apply to specific StarGen parameters once confirmed.


## LineweaverDavis2002

**Domain:** galactic habitable zone / GHZ

**APA:** Lineweaver, C. H., & Davis, T. M. (2002). Does the rapid appearance of life on Earth suggest that life is common in the universe? Astrobiology, 2(3), 293-304. https://doi.org/10.1089/153110702762027871

**StarGen annotation:** GHZ: habitability probability peaks at ~8 kpc from galactic centre with ~4 Gyr age offset from Sun. Provides spatial and temporal probability distribution for habitable systems across the galaxy in StarGen.


## LingamLoeb2018

**Domain:** astrobiology / tidal effects on life

**APA:** Lingam, M., & Loeb, A. (2018). Implications of tides for life on exoplanets. Astronomische Nachrichten, 339(6-7), 422-423. https://doi.org/10.1002/asna.201811057

**StarGen annotation:** Tidal effects on exoplanet habitability (tidal locking, tidal heating, tidal dissipation). Relevant to habitability modifiers for close-in planets around M dwarfs and tidal heating in StarGen.


## Lingard2021 **(APA incomplete)**

**Domain:** planet formation / pebble accretion

**APA:** Lingard, T., et al. (2021). Match stem Lingard2021 to ADS for the PDF on file.

**StarGen annotation:** Planet formation via pebble accretion — abstract needs verification. Apply specific constraints on core growth rate or isolation mass to StarGen's planet formation model once confirmed.


## LugerBarnes2015

**Domain:** M-dwarf habitability / pre-MS XUV stripping

**APA:** Luger, R., et al. (2015). Habitable evaporated cores: transforming mini-Neptunes into super-Earths in the habitable zones of M dwarfs. Astrobiology, 15(1), 57-88. https://doi.org/10.1089/ast.2014.1145

**StarGen annotation:** Pre-MS M-dwarf high-luminosity phase strips HZ-planet atmospheres before they enter the HZ; XUV saturation lasts up to ~1 Gyr for late M dwarfs. StarGen should flag M-dwarf HZ planets as high-risk for atmosphere loss — especially stars <0.3 M☉.


## LuquePalle2022

**Domain:** rocky planet composition / density dichotomy

**APA:** Luque, R., & Pall, E. (2022). Density, not radius, separates rocky and water-rich exoplanets. Science, 377(6609), 1211-1214. https://doi.org/10.1126/science.abl7164

**StarGen annotation:** Density-based dichotomy: rocky worlds vs water-rich worlds for sub-Neptunes around M dwarfs. Density alone discriminates rocky from volatile-rich in StarGen's interior classification without requiring full interior models.


## MalamudPerets2019 **(APA incomplete)**

**Domain:** tidal disruption / WD planet pollution

**APA:** Malamud, U., & Perets, H. B. (2019). Match stem MalamudPerets2019 to ADS for the PDF on file.

**StarGen annotation:** Tidal disruption of planetesimals around white dwarfs. Relevant to planetary system evolution around WD remnants — informs tidal-stripping rates and WD pollution modelling in StarGen.


## MentCharbonneau2023 **(APA incomplete)**

**Domain:** M-dwarf planet occurrence / ground-based survey

**APA:** Ment, K., & Charbonneau, D. (2023). Match stem MentCharbonneau2023 to ADS for the PDF on file.

**StarGen annotation:** Ground-based mid-to-late M-dwarf planet occurrence rates. Partly implemented for close-in terrestrial abundance and sub-Neptune scarcity, but not used to boost M-dwarf HZ rocky occurrence over FGK because BergstenEtAl2023 contradicts that broader claim.


## Mills2024 **(APA incomplete)**

**Domain:** planetary dynamics / resonance chains (2024)

**APA:** Mills, D. B., et al. (2024). Match stem Mills2024 to ADS for the PDF on file.

**StarGen annotation:** Resonant chain formation or disruption study (2024) — abstract needs verification. Apply to resonance architecture constraints in compact multi-planet systems in StarGen.


## MoeDiStefano2017

**Domain:** stellar multiplicity / comprehensive mass-period-ratio statistics

**APA:** Moe, M., & Di Stefano, R. (2017). Mind your Ps and Qs: the interrelation between period (P) and mass-ratio (Q) distributions of binary stars. The Astrophysical Journal Supplement Series, 230(2), 15. https://doi.org/10.3847/1538-4365/aa6fb4

**StarGen annotation:** Multiplicity fraction vs primary mass: OB ~70%, A ~50%, solar-type ~46%, M ~20–35%, VLM ~10–15%. Period distribution log-normal peaking ~10⁵ days for solar-type; nearly flat mass-ratio distribution. Core reference alongside DucheneKraus2013 for binary/multiple generation in StarGen.


## Mordasini2007 **(APA incomplete)**

**Domain:** planet formation / Bern population synthesis

**APA:** Mordasini, C., Alibert, Y., Benz, W., & Naef, D. (2008). Extrasolar planet population synthesis I. Method, formation tracks, and mass-distance distribution. Astronomy and Astrophysics, 501(3), 1139-1160. https://doi.org/10.1051/0004-6361:20078919 (Stem Mordasini2007; verify year against PDF.)

**StarGen annotation:** Reviewed but partly implemented. StarGen uses Mordasini2007 as core-accretion / population-synthesis grounding for gas-giant formation weights, not as a direct Bern-model simulator.


## Mroz2020

**Domain:** free-floating planets / microlensing

**APA:** Mroz, P., et al. (2020). A rogue Earth-mass planet and candidate brown dwarf in free-floating orbits. The Astrophysical Journal Letters, 903(1), L11. https://doi.org/10.3847/2041-8213/abc77a

**StarGen annotation:** ~1 Jupiter-mass free-floating planet (FFP) per main-sequence star from microlensing surveys. Relevant to rogue/free-floating planet generation rates and ejection probability priors in StarGen.


## NakajimaEtAl2022

**Domain:** terrestrial planet accretion / Moon-size bodies

**APA:** Nakajima, M., Genda, H., Asphaug, E., & Ida, S. (2022). Terrestrial planet compositions controlled by accretion of Moon-size bodies. Nature Communications, 13, 2064. https://doi.org/10.1038/s41467-022-28063-8

**StarGen annotation:** Terrestrial planet compositions controlled by accretion of Moon-size bodies. Relevant to rocky-planet interior composition modelling — accretion history affects bulk elemental ratios in StarGen.


## NapierEtAl2023 **(APA incomplete)**

**Domain:** outer Solar System / Kuiper Belt dynamics (2023)

**APA:** Napier, K. J., et al. (2023). Match stem NapierEtAl2023 to ADS for the PDF on file.

**StarGen annotation:** Kuiper Belt or outer Solar System dynamical study (2023) — abstract needs verification. Apply to outer-belt architecture parameters in StarGen.


## Obertas2017

**Domain:** orbital stability / tightly packed systems

**APA:** Obertas, A., Van Laerhoven, C., & Tamayo, D. (2017). The stable archipelago: the number of mutually stable systems in a Kepler-like sample. Monthly Notices of the Royal Astronomical Society, 470(2), 1657-1666. https://doi.org/10.1093/mnras/stx1316

**StarGen annotation:** Stability timescales for tightly packed planetary systems as a function of mutual-Hill spacing, period ratio, and eccentricity. Partly implemented as an auditable architecture spacing scaffold using exact mutual-Hill spacing, compact/transition/giant mass proxies, and policy/source provenance; this remains a StarGen proxy policy, not a full Obertas stability-time fit.


## ObertasTamayo2023 **(APA incomplete)**

**Domain:** orbital stability / ML-assisted SPOCK (2023)

**APA:** Obertas, A., & Tamayo, D. (2023). Match stem ObertasTamayo2023 to ADS for the PDF on file.

**StarGen annotation:** ML-assisted orbital stability and dynamical-packing study (2023) — abstract needs verification. Underutilized: not active in generation; future work must decide whether this supports packing priors, unseen-planet heuristics, or classifier validation.


## Olson2020 **(APA incomplete)**

**Domain:** planetary habitability / ocean/atmospheric redox

**APA:** Olson, S. L., et al. (2020). Match stem Olson2020 to ADS for the PDF on file (exoplanet/ocean context).

**StarGen annotation:** Planetary redox chemistry and atmospheric oxygenation timeline. Relevant to habitability state and biosignature atmosphere generation for rocky planets in StarGen.


## Otegi2020

**Domain:** mass-radius relations / rocky and volatile-rich

**APA:** Otegi, J. F., Bouchy, F., & Helled, R. (2020). Revisited mass-radius relations for exoplanets below 120 Earth masses. Astronomy and Astrophysics, 640, A135. https://doi.org/10.1051/0004-6361/202038237

**StarGen annotation:** Updated mass-radius relations: rocky regime (R ∝ M^0.27, approximately constant density) and volatile-rich regime. Now partly implemented in `PlanetMassRadiusTable`: rocky branch capped at the source's approximate 25 Earth-mass endpoint, volatile-rich branch used beyond that point, and generated planets sample the reported relation uncertainties with deterministic seeds. The pure-water density separator remains follow-up work pending human review.


## Outland2020 **(APA incomplete)**

**Domain:** outreach / science communication

**APA:** Outland, A., et al. (2020). Match stem Outland2020 to ADS for the PDF on file.

**StarGen annotation:** Abstract needs verification. Documentation-only pending review; no active generation behavior should cite this as support.


## OwenWu2017

**Domain:** photoevaporation / radius gap

**APA:** Owen, J. E., & Wu, Y. (2017). The evaporation valley in the Kepler planets. The Astrophysical Journal, 847(1), 29. https://doi.org/10.3847/1538-4357/aa890a

**StarGen annotation:** XUV-driven photoevaporation model reproducing the radius gap at ~1.7 R⊕ within the first ~100 Myr. Partly implemented: the photoevaporation branch now has a negative radius-valley period slope, XUV-weighted loss-pressure proxy, stronger stripped-envelope bias, and provenance mechanism tag. Full escape-model treatment remains follow-up.


## Pascucci2016

**Domain:** protoplanetary disk / disk-to-star mass scaling

**APA:** Pascucci, I., et al. (2016). A steep rise in the disk mass accretion rate at 1-3 Myr in the Orion cluster. The Astrophysical Journal, 831(1), 41. https://doi.org/10.3847/0004-637X/831/1/41

**StarGen annotation:** Reviewed but partly implemented. StarGen uses Pascucci2016 to scale the active solid-reservoir proxy with host mass and records the disk-dust exponent in formation provenance; exact ALMA disk-population fits remain follow-up.


## Petigura2013

**Domain:** planet occurrence / Kepler / η⊕

**APA:** Petigura, E. A., Howard, A. W., & Marcy, G. W. (2013). Prevalence of Earth-size planets orbiting Sun-like stars. Proceedings of the National Academy of Sciences, 110(48), 19273-19278. https://doi.org/10.1073/pnas.1319909110

**StarGen annotation:** Kepler small-planet occurrence around Sun-like stars. Partly implemented as FGK close-in/small-planet occurrence context in `PlanetarySystemState`; Bryson2021 is the preferred modern HZ rocky occurrence anchor.


## Petit2018

**Domain:** orbital stability / AMD criterion

**APA:** Petit, A. C., & Laskar, J. (2018). AMD-stability: a practical measure of AMD-stability from the planetary masses and orbits. Astronomy and Astrophysics, 617, A93. https://doi.org/10.1051/0004-6361/201732294

**StarGen annotation:** AMD-stability criterion: a system is AMD-stable if no planet pair can exchange enough angular momentum deficit to cause orbit crossing. Underutilized: StarGen does not yet implement the Petit AMD filter; current work only records spacing diagnostics that can feed future validation.


## Petit2020

**Domain:** orbital stability / AMD extended framework

**APA:** Petit, A. C., Pichierri, G., Davies, M. B., & Johansen, A. (2020). Debris from giant impacts in planetary systems: constraints on the collisional parameters of similar-sized embryos. Astronomy and Astrophysics, 641, A176. https://doi.org/10.1051/0004-6361/202038764

**StarGen annotation:** Extended AMD-stability for multi-planet systems; refined application of the AMD criterion. Underutilized: no Petit2020 resonance correction is active yet, though generated slots now expose period-ratio diagnostics for future implementation.


## PetkowskiEtAl2020

**Domain:** astrobiology / Venus cloud habitability

**APA:** Petkowski, J. J., et al. (2020). On the potential habitability of Venusian clouds: a concise review of Venus as a laboratory for exobiology. Astrobiology, 20(8), 900-912. https://doi.org/10.1089/ast.2020.2247

**StarGen annotation:** Venus cloud layer as potential habitat; sulphuric acid droplet environment. Relevant to habitability flagging of Venus-zone rocky worlds — cloud-layer habitability is a distinct non-surface habitability regime to model in StarGen.


## Raghavan2010

**Domain:** stellar multiplicity / FGK solar-type survey

**APA:** Raghavan, D., et al. (2010). Survey of 1187 nearby stars. The Astrophysical Journal Supplement Series, 190(1), 1-42. https://doi.org/10.1088/0067-0049/190/1/1

**StarGen annotation:** Solar-type stars: binary fraction ~46%, period distribution log-normal peaking ~293 yr, mass ratio approximately flat. Key observational prior for FGK binary generation in StarGen.


## RaymondIzidoro2017

**Domain:** asteroid belt / grand tack / giant planet migration

**APA:** Raymond, S. N., & Izidoro, A. (2017). The empty primordial asteroid belt. Science Advances, 3(9), e1701138. https://doi.org/10.1126/sciadv.1701138

**StarGen annotation:** Reviewed but partly implemented. StarGen uses RaymondIzidoro2017 to link outer reservoirs, giant scattering, bombardment, and volatile delivery; asteroid-belt depletion and compositional structure remain follow-up for the small-bodies pass.


## Ribas2015

**Domain:** M-dwarf XUV / stellar activity evolution

**APA:** Ribas, I., et al. (2016). The analysis of Proxima Centauri radial velocities without activity corrections. Astronomy and Astrophysics, 596, L21. https://doi.org/10.1051/0004-6361/201629577 (Stem Ribas2015; journal year 2016.)

**StarGen annotation:** Reviewed but partly implemented. StarGen uses Ribas2015 for host-mass disk-lifetime and low-mass-host activity proxies; full XUV saturation-and-decay history remains follow-up.


## Rice2023

**Domain:** planetary architecture / radius gap vs host properties

**APA:** Rice, D. R., & Steffen, J. H. (2023). The California-Kepler Survey. IX. The radius gap as a function of stellar mass, metallicity, and age. Monthly Notices of the Royal Astronomical Society, 518(1), 1350-1364. https://doi.org/10.1093/mnras/stad393

**StarGen annotation:** Planetary architecture / spacing-uniformity note in the current source corpus. Partly implemented for mutual-Hill spacing, period-ratio diagnostics, and source-marked architecture mass proxies; bibliographic role needs human reconciliation because the current APA/domain text may describe a different Rice/Steffen radius-gap paper.


## Rimmer2018

**Domain:** UV / abiogenesis zone / origin of life

**APA:** Rimmer, P. B., et al. (2018). The origin of life and the photochemistry of meteoritic iron. Science Advances, 4(8), eaar3302. https://doi.org/10.1126/sciadv.aar3302

**StarGen annotation:** UV surface flux threshold for cyanosulfidic prebiotic chemistry. Defines an abiogenesis zone around stars — UV-quiet M dwarfs may inhibit this chemistry pathway. Relevant to life-origin probability flags in StarGen.


## RodriguezPadilla2013 **(APA incomplete)**

**Domain:** galactic structure

**APA:** Rodriguez, D. R., Padilla, N. D., & Nelson, A. F. (2013). Match stem RodriguezPadilla2013 to ADS for the PDF on file.

**StarGen annotation:** Abstract needs verification — apply to relevant MW structural parameters in StarGen once confirmed.


## Ronnet2020

**Domain:** moon formation / pebble accretion in CPD

**APA:** Ronnet, T., & Johansen, A. (2020). Formation of moons and the collisional dynamics of a system of pebble-seeded embryos. Astronomy and Astrophysics, 642, A65. https://doi.org/10.1051/0004-6361/201936804

**StarGen annotation:** Moon formation via pebble accretion within circumplanetary disk. Constrains satellite mass distribution and orbital architecture for moon generation in StarGen. Retained audit warning: do not treat the specific `10 mutual Hill radii` multiplier as an authoritative source-backed rule; exact moon-spacing coefficients remain StarGen tuning unless separately sourced. Bibliography/source closure status: reviewed but underutilized.


## Sasaki2010

**Domain:** satellite formation / resonance trapping

**APA:** Sasaki, T., Stewart, G. R., & Ida, S. (2010). Origin of the different architectures of the Jovian and Saturnian satellite systems. The Astrophysical Journal, 714(2), 1052-1064. https://doi.org/10.1088/0004-637X/714/2/1052

**StarGen annotation:** Satellite system formation and resonance trapping around giant planets. Constrains moon system orbital architecture (resonance chains, mass hierarchy, compositional gradients) in StarGen's moon generator.


## SpiegelTurner2012

**Domain:** life / abiogenesis probability / Bayesian

**APA:** Spiegel, D. S., & Turner, E. L. (2012). Bayesian analysis of the astrobiological implications of life's early emergence on Earth. Proceedings of the National Academy of Sciences, 109(2), 395-400. https://doi.org/10.1073/pnas.1112144910

**StarGen annotation:** Bayesian estimate of abiogenesis probability from Earth's observational record with anthropic selection correction. Relevant to base-rate priors for life-origin in StarGen's population/civilisation module.


## StevensonEtAl2023 **(APA incomplete)**

**Domain:** planetary science (2023)

**APA:** Stevenson, K., et al. (2023). Match stem StevensonEtAl2023 to ADS for the PDF on file.

**StarGen annotation:** Abstract needs verification — apply to specific StarGen parameters once confirmed.


## Stokey2020 **(APA incomplete)**

**Domain:** economics / technology diffusion

**APA:** Stokey, N. L. (2020). Match stem Stokey2020 to library record for the PDF on file (likely economics).

**StarGen annotation:** Economics working paper — abstract needs verification. Potentially relevant to technology diffusion and innovation-rate modelling in StarGen's civilisation development framework.


## Szulagyi2018

**Domain:** circumplanetary disk / moon-forming region simulation

**APA:** Szulagyi, J., Cilibrasi, M., & Mayer, L. (2018). In situ formation of icy moons of Uranus and Neptune. The Astrophysical Journal Letters, 868(1), L13. https://doi.org/10.3847/2041-8213/aaeed6

**StarGen annotation:** CPD simulations: CPD extent ~0.3–0.4 R_Hill, gas/dust structure, temperature profile (moon-forming region ~50–150 K). Constrains CPD mass and temperature for moon-forming conditions in StarGen.


## Tamayo2020

**Domain:** orbital stability / SPOCK / ML

**APA:** Tamayo, D., et al. (2020). A machine learns to predict stable planetary systems. Proceedings of the National Academy of Sciences, 117(39), 24249-24255. https://doi.org/10.1073/pnas.2001258117

**StarGen annotation:** SPOCK ML model for rapid orbital stability classification of compact planetary systems. Underutilized: StarGen does not ship SPOCK feature extraction, model dependencies, or probability thresholds; current stability pass records diagnostics only.


## TanakaTakeuchiWard2002

**Domain:** planet formation / Type I migration / disk-planet coupling

**APA:** Tanaka, H., Takeuchi, T., & Ward, W. R. (2002). Three-dimensional interaction between a planet and an isothermal gaseous disk. I. Corotation and Lindblad torques and planet migration. The Astrophysical Journal, 565(2), 1257–1274. https://doi.org/10.1086/324713

**StarGen annotation:** AI-assisted draft, partly implemented. StarGen now records a Tanaka-style isothermal Type-I migration timescale and likelihood diagnostic in planet formation provenance. It does not yet implement a public `MigrationMode` enum or the non-isothermal/radiative corrections noted in the source file.


## Tokovinin2021

**Domain:** stellar multiplicity / hierarchical systems

**APA:** Tokovinin, A. (2021). From binaries to multiples. III. Statistical properties of hierarchical multiple stars. The Astronomical Journal, 162(6), 267. https://doi.org/10.3847/1538-3881/ac243b

**StarGen annotation:** Hierarchical multiple star systems: triple/quadruple architecture, statistical frequency, and dynamical stability. Extends binary statistics to higher-order multiples for StarGen.


## VanZandtEtAl2025

**Domain:** giant planets / brown dwarf / occurrence distribution (2026)

**APA:** Van Zandt, J., Gilbert, G. J., Petigura, E. A., Giacalone, S., Howard, A. W., & Handley, L. B. (2026). A smooth transition from giant planets to brown dwarfs from the radial occurrence distribution. The Astronomical Journal, 171(5), 267. https://doi.org/10.3847/1538-3881/ae5102 (Preprint arXiv:2511.18758; repository stem uses 2025.)

**StarGen annotation:** Giant planet to brown dwarf occurrence radial distribution (smooth transition). Informs giant planet and brown-dwarf companion occurrence priors as a function of orbital separation in StarGen.


## VissapragadaEtAl2022

**Domain:** atmospheric escape / helium metastable detection

**APA:** Vissapragada, S., et al. (2022). A non-detection of atmospheric helium on WASP-107 b from three transits with CUTE. The Astronomical Journal, 164(1), 24. https://doi.org/10.3847/1538-3881/ac73ea

**StarGen annotation:** Helium 10830 Å non-detection on WASP-107b — constrains metastable He atmospheric escape in sub-Neptunes. Contributes observational constraints on photoevaporation model parameters for radius-gap modelling in StarGen.


## WanderleyEtAl2025

**Domain:** radius gap / M-dwarf hosts (2025)

**APA:** Wanderley, F., Cunha, K., Smith, V. V., et al. (2025). An analysis of the radius gap in a sample of Kepler, K2, and TESS exoplanets orbiting M-dwarf stars. The Astrophysical Journal, 993(2), 233. https://doi.org/10.3847/1538-4357/ae058e

**StarGen annotation:** Radius gap in Kepler, K2, and TESS exoplanets around M-dwarf stars. Underutilized pending metadata verification and reconciliation with GillisEtAl2026; current occurrence pass does not yet implement an M-dwarf radius-gap replacement.


## WeggGerhard2013

**Domain:** MW bar/bulge structure

**APA:** Wegg, C., & Gerhard, O. (2013). General models for the Milky Way's stellar and dark mass distribution. Monthly Notices of the Royal Astronomical Society, 435(3), 1874-1887. https://doi.org/10.1093/mnras/stt631

**StarGen annotation:** MW boxy/peanut bulge and bar from red clump stars: bar half-length ~4 kpc, angle ~27° from the Sun–GC line. Constrains galactic bar parameters (length, mass, orientation) in StarGen's galaxy generator.


## WordsworthKreidberg2022

**Domain:** terrestrial atmospheres / secondary atmosphere retention

**APA:** Wordsworth, R., & Kreidberg, L. (2022). Atmospheric composition of rocky exoplanets. Annual Review of Astronomy and Astrophysics, 60, 139-169. https://doi.org/10.1146/annurev-astro-112420-030055

**StarGen annotation:** Secondary atmosphere formation and retention on rocky exoplanets: volcanic outgassing rates, photodissociation, escape rates, and composition evolution. Constrains atmosphere generation for rocky worlds in StarGen — not all rocky HZ planets retain atmospheres; expose model uncertainty.

---

## Pass 5–6 Additions (2026-04-27)

*These entries were added in audit passes 5 and 6. They should be alphabetically integrated
into the main bibliography in a future pass. All APA citations marked (APA incomplete)
require human verification against the PDF or NASA ADS.*

---

## ArvidssonEtAl2023

**Domain:** urban economics / social science / civilization modeling

**APA:** Arvidsson, O., et al. (2023). Urban scaling laws arise from within-city inequalities. Nature Human Behaviour, 7, 365–374. https://doi.org/10.1038/s41562-022-01509-3

**StarGen annotation:** 36–80% of urban output scaling (β > 1) is driven by the upper income tail, not average citizens. Implements `CivilizationInequalityModel` in StarGen's civilization pipeline: elite vs. median tech access computed separately, with Gini proxy feeding faction count generation.


## AuerbachThachil2024

**Domain:** social science / state capacity / political science

**APA:** Auerbach, A. M., & Thachil, T. (2024). State capacity in comparative perspective. State Politics & Policy Quarterly, 24(4), 349–369. https://doi.org/10.1017/spq.2024.12

**StarGen annotation:** Three-axis state capacity model: administrative, fiscal, and coercive capacity are independently variable. Justifies separate sampling of LawLevel and GovernmentType in StarGen; introduces `GovernmentProfile` struct with three capacity fields.


## BallandEtAl2022

**Domain:** social science / economic complexity / product space

**APA:** Balland, P.-A., et al. (2022). The new paradigm of economic complexity. Research Policy, 51(4), 104311. https://doi.org/10.1016/j.respol.2021.104311

**StarGen annotation:** Economic complexity index (ECI) from product space captures path-dependent technological development. Implements ECI → TechLevel mapping (ECI 0–3.5 → TL 0–15); max ΔTL per generation constraint; trade code assignment from ECI range. `EconomicComplexityModel` enum.


## Baumeister2025Followup

**Domain:** planetary science / interior structure / mass-radius degeneracy

**APA:** Baumeister, P., Miozzi, F., Guimond, C. M., Steinmeyer, M.-L., Dorn, C., Karato, S.-I., Bolmont, É., Revol, A., & Thamm, A. (2025). Fundamentals of interior modelling and challenges in the interpretation of observed rocky exoplanets. Space Science Reviews, 221, 123. https://doi.org/10.1007/s11214-025-01248-5 arXiv: 2511.10269

**StarGen annotation:** Mass+radius alone cannot uniquely constrain planetary interior composition (mass-radius degeneracy). StarGen should represent composition as a probability distribution, not a single value. `CompositionDistribution` replaces single `CompositionType`. Formation location (NC/CC from SchonbachlerEtAl2025) acts as prior.


## BettencourtEtAl2007

**Domain:** social science / urban scaling / population dynamics

**APA:** Bettencourt, L. M. A., Lobo, J., Helbing, D., Kühnert, C., & West, G. B. (2007). Growth, innovation, scaling, and the pace of life in cities. Proceedings of the National Academy of Sciences, 104(17), 7301–7306. https://doi.org/10.1073/pnas.0610172104

**StarGen annotation:** Canonical urban scaling paper. Scaling exponents: patents β=1.27, R&D β=1.21, GDP β=1.13, crime β=1.16, roads β=0.85. Foundation for StarGen's population→output model. `ComputeOutputs()` method uses these exponents with reference population of 1 million.




## FrankEtAl2018

**Domain:** astrobiology / civilization trajectories / Anthropocene analogs

**APA:** Frank, A., Carroll-Nellenback, J., Alberti, M., & Kleidon, A. (2018). The Anthropocene generalized: Evolution of exo-civilizations and their planetary feedback. International Journal of Astrobiology, 17(3), 225–236. https://doi.org/10.1017/S1473550417000258 (arXiv:1709.06435)

**StarGen annotation:** Five-state civilization trajectory: State I (no civ), II (complex biosphere), III (pre-industrial), IV (Anthropocene/fossil fuel burning), Va (sustainable), Vb (collapse). Anthropocene is a generic, predictable phase. Implements `CivilizationState` and `AnthropoceneTransitionModel` enums; ~50% collapse probability at threshold. Observable signatures table by state.


## GillisEtAl2026

**Domain:** exoplanet demographics / M-dwarf planets / radius gap

**APA:** Gillis, E., Pass, E. K., et al. (2026). TESS planet occurrence rates reveal the disappearance of the radius valley around mid-to-late M dwarfs. arXiv:2602.23364. [Submitted to AAS Journals February 2026.]

**StarGen annotation:** TESS survey of 8,134 mid-to-late M dwarfs. Partly implemented as mid-to-late M close-in small-planet boost plus sub-Neptune and hot-giant suppression in `PlanetarySystemState`; radius-valley disappearance and water-rich composition mode remain follow-up.


## HarfstEtAl2024 **(APA incomplete)**

**Domain:** social science / political legitimacy / governance

**APA:** Harfst, P., et al. (2024). [Title pending — verify against PDF.] Frontiers in Political Science, [volume/page pending].

**StarGen annotation:** Two-dimensional legitimacy: internal (citizen belief) vs. external (international recognition) are independent axes. Internal legitimacy drives faction count (inversely); external legitimacy drives trade access and starport class. `PoliticallyStable` flag requires both > 0.40.


## HuntVasiliev2025

**Domain:** galactic dynamics / Gaia / Milky Way structure

**APA:** Hunt, J. A. S., & Vasiliev, E. (2025). Milky Way dynamics in light of Gaia. New Astronomy Reviews, 100, 101721. https://doi.org/10.1016/j.newar.2025.101721

**StarGen annotation:** Post-Gaia review of Milky Way galactic dynamics: disk, bar, and halo kinematics now mapped across large volumes. Bar pattern speed uncertain (35–55 km/s/kpc); dark halo mass 0.7–1.5×10¹² M⊙. Velocity dispersion by disk component (thin disk, thick disk, halo). Disequilibrium processes in disk from satellite interactions. Supersedes BlandHawthornGerhard2016 for kinematic profiles.


## KhoperskovEtAl2024

**Domain:** galactic dynamics / Milky Way bar formation / TNG50

**APA:** Khoperskov, S., Minchev, I., Steinmetz, M., Ratcliffe, B., Walcher, J. C., & Libeskind, N. I. (2024). Why does the Milky Way have a bar? Monthly Notices of the Royal Astronomical Society, 533(4), 3975–3986. https://doi.org/10.1093/mnras/stae1902 arXiv: 2309.07321

**StarGen annotation:** MW bar strength (A₂ = 0.35–0.60) from TNG50 simulations. Early disk formation (z ≳ 2–3) → strong bar; late disk (z ≈ 1–1.5) → weak/no bar. Bar formation age ~8–10 Gyr. Implements bar strength as function of disk formation redshift; extends inner GHZ boundary for strongly-barred galaxies.


## MaysharMoavPascali2022

**Domain:** social science / state formation / economic history / political economy

**APA:** Mayshar, J., Moav, O., & Pascali, L. (2022). The origin of the state: Land productivity or land appropriability? Journal of Political Economy, 130(4), 1091–1144. https://doi.org/10.1086/718372

**StarGen annotation:** States arose from appropriability of cereal grains (storable, taxable) not land productivity. Implements `AppropriabilityIndex` by `EconomyType` enum; government complexity bonus by resource type. `GovernmentOriginModel` enum. High appropriability (industrial, mining) → more complex state formation.


## Mills2024

**Domain:** astrobiology / hard-steps model / complex life evolution

**APA:** Mills, D. B., Macalady, J. L., Frank, A., & Wright, J. T. (2025). A reassessment of the "hard-steps" model for the evolution of intelligent life. Science Advances, 11(7), eads5698. https://doi.org/10.1126/sciadv.ads5698 arXiv: 2408.10293 [Note: ArXiv preprint dated 2024; published February 2025. File named Mills2024 after preprint year.]

**StarGen annotation:** Reassessment of hard-steps model using co-evolutionary interpretation. GOE is not a hard step; multicellularity evolved 25+ times; nervous systems evolved independently. Raises optimistic civilization probability: p_oxygen raised from 0.10 to 0.40 under Mills2024 interpretation. `HardStepsModel` enum (Carter1983 / ForganRice2010 / Mills2024).
*Note: Previous bibliography entry had wrong domain — see TXT file for corrected content.*


## Murphy2023

**Domain:** social science / political economy / governance taxonomy

**APA:** Murphy, R. (2023). Open access orders: A new approach to measuring institutions and their connection to economic performance. Journal of Institutional Economics, 19(5), 618–634. https://doi.org/10.1017/S1744137423000188

**StarGen annotation:** OAO/LAO/Fragile governance taxonomy (North-Wallis-Weingast operationalized). Three-component index: economic freedom + liberal democracy + state capacity. 35 of 161 countries qualify as OAO in 2020. `OAOCategory` enum, `DetermineOAOCategory()` method, GovernmentType weighted sampling by category. OAO requires age > 150 yr AND TL ≥ 7.


## NuezCastieyra2025Followup

**Domain:** galaxy formation / dark matter halos / subhalo dynamics

**APA:** Nuñez-Castiñeyra, A., Nezri, E., Mollitor, P., Michel-Dansac, L., Devriendt, J., & Teyssier, R. (2025). Cosmological simulations of the same spiral galaxy: satellite properties, the role of baryonic physics and star formation history in shaping dark matter cores/cusps. arXiv:2509.07470. [Journal submission pending.]

**StarGen annotation:** Mochima zoom-in simulations: subhalo survival depends on host halo concentration (baryonic feedback modulates). Early star formation → cuspy DM profiles; bursty/late → cored profiles. "Diversity problem" explained by SFH × gravitational environment. Implements DM inner profile field; scales satellite count by host halo concentration.


## RenEtAl2024 **(APA incomplete)**

**Domain:** social science / trade networks / economic complexity

**APA:** Ren, [first initial], et al. (2024). [Title pending.] Humanities and Social Sciences Communications, [volume/page pending].

**StarGen annotation:** Trade partner selection from ECI (Economic Complexity Index) complementarity. Pairwise trade score model: ECI complementarity × product proximity × jump range. Trade flow direction table; trade code assignment by ECI range. Implements trade network formation in StarGen's civilization module.


## Roos2025 **(APA incomplete)**

**Domain:** complexity science / social science / governance

**APA:** Roos, P. (2025). [Title pending.] PLOS Complex Systems, 2(7), e0000055. https://doi.org/[pending]

**StarGen annotation:** Structural complexity (differentiation) vs. dynamic complexity (adaptability) are independent axes. Resilience = geometric mean of both (feeds FrankEtAl2018 adaptability rate). 2×2 governance archetype table: Rigid / Fragile / Flexible / Resilient. `ComplexityProfile` struct.


## SchonbachlerEtAl2025

**Domain:** cosmochemistry / planet formation timescales / protoplanetary disk

**APA:** Schönbächler, M., Bouvier, A., Kita, N. T., & Kruijer, T. S. (2025). Initial conditions of planet formation: Time constraints from small bodies and the lifetime of reservoirs in the solar protoplanetary disk. Space Science Reviews, 221, 97. https://doi.org/10.1007/s11214-025-01216-z

**StarGen annotation:** CAI formation marks t₀; chondrule formation 1–3 Ma; planetesimal accretion < 1 Ma; NC/CC reservoir separation < 1 Ma. Inner disk = NC (rocky/dry); outer disk = CC (volatile-rich/icy). Snow line scales as 2.7×√(L/L☉) AU. ²⁶Al differentiation timing affects iron core fraction. `PlanetFormationTimingModel` enum.


## SchwietermanEtAl2018

**Domain:** astrobiology / biosignatures / atmospheric characterization

**APA:** Schwieterman, E. W., et al. (2018). Exoplanet biosignatures: A review of remotely detectable signs of life. Astrobiology, 18(6), 663–708. https://doi.org/10.1089/ast.2017.1729 (arXiv:1705.05791)

**StarGen annotation:** Comprehensive biosignature review: O₂, O₃, CH₄, N₂O, NO₂, CFCs as biosignature gases with detection thresholds and false-positive risks. Industrial technosignatures: CO₂ + SO₂ + NOₓ combo indicates coal-burning phase. `BiosignatureProfile` class. `BiosignatureModel` enum. Observable signatures indexed to FrankEtAl2018 civilization states.


## SolizWelsh2026

**Domain:** astrobiology / M-dwarf habitability / photosynthesis / PAR

**APA:** Soliz, J. J., & Welsh, W. F. (2026). Dearth of photosynthetically active radiation suggests no complex life on late M-star exoplanets. arXiv:2601.02548. [Journal publication pending.]

**StarGen annotation:** TRAPPIST-1 receives only 0.9% of Earth's PAR → GOE delayed 111× (77.7 Gyr vs. 10 Gyr MS lifetime). Complex life impossible on M4+ HZ planets via conventional route. `MDwarfPARGate` enum. PAR fraction table by spectral type. Updates ForganRice criterion 7: p_oxygen near-zero for M4+.


## SpohnEtAl2026

**Domain:** exo-geoscience / planetary interiors / plate tectonics / habitability

**APA:** Spohn, T., Roberge, A., Way, M. J., Duarte, J. C., Miozzi, F., Baumeister, P., Byrne, P., & Lineweaver, C. H. (2026). Exo-geoscience perspectives beyond habitability. Space Science Reviews, 222, 9. https://doi.org/10.1007/s11214-026-01265-y

**StarGen annotation:** Stagnant-lid is the default tectonic mode (~65% probability); mobile-lid/plate tectonics ~20%. Balanced land/ocean (Earth-like) is the LEAST likely outcome. All-ocean/land worlds: NPP ~1% of Earth's → O₂ productivity may not reach Balbi2023 threshold. Introduces `TectonicRegime` enum, `LandFraction` distribution, `NPPFactor`, `IsEuhabitable` flag.


## Taiz2026

**Domain:** astrobiology / SETI / Drake equation / fossil fuels / civilization prerequisites

**APA:** Taiz, L., Primack, J., Hellinger, D., & Ward, P. D. (2026). How common are oxygenic photosynthesis and large coal deposits on exoplanets? International Journal of Astrobiology, 25, e1. https://doi.org/10.1017/S1473550425100244

**StarGen annotation:** Coal formation is highly contingent (requires oxygenic photosynthesis + continental forests + plate tectonics + taphonomic burial + geological timing synchronicity). Without coal, civilization caps at TL 6 (pre-industrial). Implements `CoalContingencyModel`: p_coal ~ 0.20; TL 7+ requires fossil fuel gate to pass. Industrial technosignature: CO₂ + SO₂ + NOₓ combo.


## TurchinEtAl2021WarMachines

**Domain:** social science / cliodynamics / military technology / Seshat

**APA:** Turchin, P., Hoyer, D., Korotayev, A., Kradin, N., Nefedov, S., Feinman, G., Levine, J., Reddish, J., Cioni, E., Thorpe, C., Bennett, J. S., Francois, P., & Whitehouse, H. (2021). Rise of the war machines: Charting the evolution of military technologies from the Neolithic to the Industrial Revolution. PLOS ONE, 16(10), e0258161. https://doi.org/10.1371/journal.pone.0258161

**StarGen annotation:** Military tech evolution driven by network population and connectivity, not local polity size. Innovation rate scales logarithmically with network population. Gateway phase transitions (weapons, fortifications, cavalry). Implements `TechDiffusionModel` enum; TechLevel cap by connectivity index (isolated → TL 6; core world → TL 15+).


## VanZandtDistantGiants2025

**Domain:** exoplanet demographics / giant planets / outer system architecture

**APA:** Van Zandt, J., Petigura, E. A., Lubin, J., Weiss, L. M., Turtelboom, E. V., Fetherolf, T., Murphy, J. M. A., Crossfield, I. J. M., Gilbert, G., Močnik, T., Batalha, N. M., Dressing, C., & Fulton, B. (2025). The TESS-Keck Survey XXIV: Outer giants may be more prevalent in the presence of inner small planets. The Astronomical Journal, 169, 235. https://doi.org/10.3847/1538-3881/adbbed arXiv: 2501.06342
*Note: The file VanZandtEtAl2025.txt corresponds to this Distant Giants Survey. The
bibliography entry 'VanZandtEtAl2025' refers to a different paper (smooth transition
from giants to brown dwarfs, arXiv:2511.18758). These are distinct papers.*

**StarGen annotation:** P(outer giant | inner small planets) = 30 +14/−12%; field rate 16%. Underutilized: StarGen does not yet have the post-inner-system conditional outer-giant architecture pass needed to use this source correctly.


## Full-text extract cleanup additions (2026-04-28)

*These entries replace raw full-text extract notes with BauerEtAl2017-style source
notes. They should be alphabetically integrated into the main bibliography in a future
cleanup pass.*

## ChacuaEtAl2024

**Domain:** social science / economic complexity / innovation policy

**APA:** Chacua, C., Gadgin Matha, S., Hartog, M., Hausmann, R., & Yildirim, M. A. (2024). Innovation policies under economic complexity. Growth Lab Working Paper Series No. 234. Harvard Kennedy School Growth Lab.

**StarGen annotation:** Capability-based innovation-policy framework. Supports separating technology access from local adoption/production readiness in StarGen's sentient-world and trade modeling.


## EscuderoEtAl2023

**Domain:** astrobiology / dark biosphere / subsurface habitability

**APA:** Escudero, C., & Amils, R. (2023). Hard rock dark biosphere and habitability. Frontiers in Astronomy and Space Sciences, 10, Article 1203845. https://doi.org/10.3389/fspas.2023.1203845

**StarGen annotation:** Supports dark-biosphere and subsurface microbial habitability branches separate from surface HZ habitability, complex life, oxygenation, and technosphere gates.


## HamiltonEtAl2020

**Domain:** social science / sociopolitical complexity / population scaling

**APA:** Hamilton, M. J., Walker, R. S., Buchanan, B., & Sandeford, D. S. (2020). Scaling human sociopolitical complexity. PLOS ONE, 15(7), e0234615. https://doi.org/10.1371/journal.pone.0234615

**StarGen annotation:** Population, range, and density scale with sociopolitical complexity, but with overlap between levels. Supports population as a pressure on `SocialScale` without making population alone determine government complexity.


## Knez2023

**Domain:** economics / technology diffusion / uneven development

**APA:** Knez, K. (2023). Technology diffusion and uneven development. Journal of Evolutionary Economics, 33, 1171-1195. https://doi.org/10.1007/s00191-023-00830-w

**StarGen annotation:** Technology adoption should be modeled as uneven density constrained by local implementation costs, development level, and capability base. Supports separating `HighestTechLevel` from `TechnologyAdoptionCapacity`.


## SavvidouEtAl2023

**Domain:** planet formation / pebble accretion / gas giant formation

**APA:** Savvidou, S., & Bitsch, B. (2023). How to make giant planets via pebble accretion. Astronomy & Astrophysics, 679, A42. https://doi.org/10.1051/0004-6361/202245793

**StarGen annotation:** Gas giant formation through pebble accretion depends on interacting disk conditions: disk mass, disk size, early embryo growth, dust-to-gas ratio, viscosity, fragmentation velocity, and starting location. Current local PDF is an A&A leaflet, not the article PDF; reacquire before final verification.


## VanKleefEtAl2023

**Domain:** social psychology / norm violation / dominance / prestige

**APA:** van Kleef, G. A., Wanders, F., van Vianen, A. E. M., Dunham, R. L., Du, X., & Homan, A. C. (2023). Rebels with a cause? How norm violations shape dominance, prestige, and influence granting. PLOS ONE, 18(11), e0294019. https://doi.org/10.1371/journal.pone.0294019

**StarGen annotation:** Distinguishes dominance, prestige, local norms, global norms, and influence granting. Planning source for future human-audited legitimacy and faction modeling, not a direct physical-generation input.


## WichmannEtAl2025 **(APA incomplete)**

**Domain:** social science / conflict typology / archaeology / cliodynamics

**APA:** Wichmann, S., et al. (2025). [Title pending.] PLOS ONE, [volume/page pending].

**StarGen annotation:** Conflict typology from Bronze/Iron Age archaeological fingerprints: Raiding, IntergroupWarfare, Conquest, InternalFaction, FormalMilitary. `ConflictType` enum; `DetermineConflictType()` from OAOCategory + techLevel + internalLegitimacy. Worldbuilding text descriptors per type.
