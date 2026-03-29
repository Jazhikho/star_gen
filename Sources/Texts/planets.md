StarGen Deterministic Planet Formation Specification
An implementation-oriented expansion of the planetary structure paper into code-facing generation rules
Prepared for StarGen design and implementation
Derived from the prior planetary science report and expanded using full-text academic sources
29 March 2026
Document purpose: This report translates the earlier literature review into a deterministic, code-ready specification for StarGen. The goal is not to reproduce full N-body accretion, hydrocode impact physics, or climate modeling. The goal is to define a physically grounded surrogate model whose outputs are reproducible from a seed, traceable to literature, and practical to implement in Godot/C# or adjacent tooling.


Abstract
StarGen needs more than descriptive astronomy. It needs a deterministic parameter stack that can turn a seed into a physically coherent planet, and do so in a way that preserves provenance, supports later simulation layers, and does not collapse into arbitrary flavor text wearing a lab coat. This specification therefore reframes the planetary science review as a generation pipeline. It defines the global inputs that should exist at the system level, the derived state variables that each planet and moon should carry, and the order in which those values should be computed. It gives literature-constrained rules for rocky interiors, volatile and water partitioning, atmospheric acquisition and loss, giant-planet structure classes, moon formation channels, and orbital constraints. Where the literature supplies compact scaling laws, those laws are preserved. Where the literature is too computationally heavy for StarGen, the report converts it into surrogate parameters and bounded heuristics. The result is a specification designed for deterministic planet formation in software rather than a general-purpose astronomy essay.
1. Scope and modeling philosophy
This specification assumes that StarGen is a seed-driven worldbuilding generator, not a research-grade simulator. That distinction matters. A simulator solves continuous physical evolution directly; StarGen instead needs a hierarchy of deterministic approximations that are internally coherent, fast enough to evaluate many systems, and expressive enough to support later layers such as biospheres, civilizations, route planning, or player-facing planet descriptions.
The right design principle is therefore layered realism. Each planet should be generated from a small number of parent variables that are themselves generated from higher-level stellar and disk conditions. Every downstream value should either be derived from earlier state or from a clearly labeled stochastic draw tied to a specific seed stream. Nothing important should be a free-floating random number.
• Formation path should take priority over present-day appearance. A 7-Earth-mass world should not be classed purely by radius if its generation history says it never retained a meaningful H/He envelope.
• Every generated object should store provenance. At minimum: parent seed, generator version, system index, stage key, and the intermediate variables that explain why the final object looks the way it does.
• StarGen should separate intrinsic properties from age-dependent state. Mass, bulk iron fraction, and volatile inventory are formation outputs; atmosphere thickness, surface water phase, and tidal heating are time-evolved states.
• The model should admit multiple thermal states for rocky planets. Noack and Lasbleis (2020) show that hot, warm, and cold end-member cores can share similar bulk radii while differing in interior thermodynamic properties.
Implementation rule: Treat each generation stage as a pure function of upstream state plus a named RNG stream derived from the system seed. Do not draw ad hoc random values in UI layers, description writers, or inspectors. Those always come back to bite people. Usually in public.

2. Required global inputs and deterministic controls
StarGen should not begin with planets. It should begin with a system state. The minimum system state should combine stellar properties, disk properties, and generation controls. These are not all directly observed in real astronomy, but they are the correct latent variables for producing a coherent ensemble.
Table 1. Global inputs that should exist before any planet is generated
Parameter	Type / units	Function in the pipeline
system_seed	64-bit int	Master seed for the stellar system; all child streams derived from this.
generator_version	string	Locks coefficient sets, branch logic, and schema interpretation for deterministic replay.
realism_profile	enum + scalar	Chooses how strictly the generator follows calibrated distributions versus stylized breadth.
stellar_mass_Msun	float	Primary driver for luminosity, habitable zone scale, Hill stability context, and disk mass proxy.
stellar_luminosity_Lsun	float	Sets irradiation, snow-line proxy, atmospheric escape context, and climate regime boundaries.
stellar_metallicity	float	Global abundance scalar affecting solid inventory, refractory enrichment, and giant-core frequency.
stellar_age_Gyr	float	Used for cooling, atmospheric escape integration, tidal evolution, and surface-state aging.
stellar_XUV_class	enum/scalar	Controls early atmospheric erosion risk and long-term upper-atmosphere loss pressure.
disk_solid_mass_scalar	float	System-level multiplier for solid surface density and core growth efficiency.
disk_gas_mass_scalar	float	Controls whether cores can retain or acquire primordial envelopes before gas dispersal.
disk_lifetime_Myr	float	Critical for separating dry rocky systems from sub-Neptune or giant-envelope outcomes.
snow_line_scalar	float	Multiplies the nominal snow-line location to produce system-to-system volatile differences.
oxidation_scalar	float	Controls redox trend in silicates and therefore secondary outgassing chemistry.
migration_strength	float	Sets how much semimajor axes may shift before final architecture freeze-out.
impact_stirring_scalar	float	Raises collision frequency, merger count, stripping probability, and giant-impact moon odds.

3. Recommended generation pipeline
The pipeline below keeps StarGen deterministic while still reflecting broad physical causation. It also gives each major module a clean contract with the next one.
Star + disk	Embryos + cores	Final planets	Interiors + volatiles	Moons + tides	Present-day state

• Stage 1: Generate the star and bulk disk parameters.
• Stage 2: Place embryos or cores using mutual-Hill spacing rules and radial solid/volatile gradients.
• Stage 3: Resolve growth, migration, mergers, and broad class outcome: rocky, volatile-rich, giant, or remnant body.
• Stage 4: Compute interiors, volatile budgets, atmosphere class, and hydrosphere partition.
• Stage 5: Generate moon systems from host class and impact history, then apply Roche/Hill stability filters.
• Stage 6: Age the system to the selected stellar age and derive present-day observables.
Why this order matters: If StarGen generates a finished planet first and then slaps atmosphere, water, and moons on afterwards, the outputs will keep contradicting each other. You will get hot stripped worlds with fat primordial envelopes, impact moons around hosts that never experienced major impacts, and tidy descriptive text trying to paper over the mess.

4. Orbital architecture, feeding zones, and class preconditions
Orbital layout should be handled before detailed planet physics because semimajor axis controls irradiation, feeding-zone composition, Hill stability, and snow-line proximity. The most useful deterministic quantities at this stage are the Hill radius, mutual Hill radius, embryo spacing, and a snow-line proxy.
R_H = a_p · (M_p / (3 M_*))^(1/3)
R_H,mutual = ((a_1 + a_2)/2) · ((m_1 + m_2) / (3 M_*))^(1/3)
For system assembly, embryos or proto-planets should be laid down in units of mutual Hill radii rather than fixed AU intervals. Compact multiplanet stability studies commonly frame orbital spacing in those units, and oligarchic growth arguments place embryos on the order of several to roughly ten mutual Hill radii apart. For StarGen, the pragmatic default is to seed embryos at 8 to 12 mutual Hill radii and reject final adjacent planets below about 3.5 mutual Hill radii unless the architecture is explicitly flagged as unstable or short-lived (Tamayo et al., 2020).
Table 2. Recommended deterministic orbital controls
Variable	Suggested default	Use
embryo_spacing_RHm	8-12	Initial embryo/core placement in mutual Hill radii.
minimum_final_spacing_RHm	>= 3.5	Hard stability floor unless instability is intentional.
preferred_final_spacing_RHm	8-30	Comfort zone for long-lived generated systems.
snow_line_AU	2.7 · snow_line_scalar · sqrt(L_*/L_sun)	Simple volatile boundary proxy; tune by disk model.
feeding_zone_half_width	5-10 Hill radii or profile-based	Determines mix of dry and volatile-rich source material.
migration_delta_a	profile-dependent	Applied before final stability pass, not after interiors are computed.

5. Planet class determination
StarGen should determine broad planet class from formation path first and mass-radius calibration second. The mass-radius relation remains useful as a sanity check and fallback, but it should not overrule explicit formation history. Müller et al. (2024) find that the observational transition from predominantly rocky small planets occurs around 4.4 Earth masses and roughly 1.64 Earth radii, while giant planets begin around 127 Earth masses.
Small rocky calibration:    R ∝ M^0.27
Intermediate volatile-rich: R ∝ M^0.67
Giant planets:             R ∝ M^(-0.06)  [weak dependence]
Table 3. Recommended class logic
Class	Primary decision rule	Secondary calibration
Rocky / metal-rich	No retained primordial envelope; formed largely inside snow line or lost envelope early.	Typically <= ~4.4 M_earth and <= ~1.6 R_earth, but history wins.
Water-rich rocky	Rocky interior with substantial ice/water inventory and little H/He.	May sit above purely rocky radius at the same mass.
Sub-Neptune / volatile-rich	Meaningful H/He envelope or large volatile mantle survives gas-disk era.	Often between ~4.4 and 127 M_earth.
Gas giant	Runaway gas accretion before disk dispersal.	Usually >= ~127 M_earth; radius weakly age- and irradiation-dependent.
Remnant / stripped core	Envelope once existed but was mostly lost by XUV escape, impacts, or photoevaporation.	Radius may look rocky even if history says otherwise.

6. Rocky planet interior module
The most code-ready source in the current corpus for rocky interiors is Noack and Lasbleis (2020), who provide parameterized scaling laws for differentiated rocky planets with Earth-like mineralogy and variable iron content. Their calibrated domain covers roughly 0.8 to 2 Earth masses and Earth-like silicate/iron compositions. That means the formulas below should be treated as the primary StarGen rocky-interior solver in that range, and as controlled extrapolations or fallbacks outside it.
The critical compositional variables are:
XFe = total planetary iron mass fraction.
#FeM = mantle iron number, the fraction of iron-bearing mantle minerals relative to magnesium-bearing mantle minerals.
XFeM = iron mass fraction locked into the mantle.
XCMF = core mass fraction after subtracting mantle iron.
XFeM = [2·#FeM·MFe] / [2·((1-#FeM)·MMg + #FeM·MFe) + MSi + 4·MO]
XCMF = (XFe - XFeM) / (1 - XFeM)
R_p [km] = (7030 - 1840·XFe) · (M_p / M_earth)^0.282
R_c,hot [km]  = 4850 · XCMF^0.328 · (M_p / M_earth)^0.266
R_c,cold[km]  = 4790 · XCMF^0.328 · (M_p / M_earth)^0.266
rho_c,av = XCMF·M_p / ((4/3)·pi·(R_c·1000)^3)
rho_m,av = (1-XCMF)·M_p / ((4/3)·pi·(R_p^3 - R_c^3)·1000^3)
g_surface = G·M_p / (R_p·1000)^2
g_CMB     = G·XCMF·M_p / (R_c·1000)^2
g_m,av    = (g_surface + g_CMB) / 2
p_CMB[GPa]= g_m,av · rho_m,av · (R_p - R_c) · 1000 · 10^-9
The Noack and Lasbleis framework is valuable to StarGen because it keeps the logically important pieces separate. Mass sets the pressure scale. Iron fraction shrinks radius and increases core size. Thermal state changes core radius only slightly, but changes core thermodynamic properties enough that it should still be kept as an explicit state variable.
Table 4. Rocky interior variables StarGen should store for every differentiated rocky planet
Variable	Units	Why it matters
mass_Mearth	Earth masses	Base mass used across radius, escape, climate, and moon modules.
XFe	0-1	Total iron fraction; primary control on radius and metal-rich character.
mantle_iron_number	0-1	Controls mantle composition and melting correction.
core_mass_fraction	0-1	Needed for core radius, density, magnetism proxies, and moment of inertia.
radius_km	km	Observable output and input to gravity, escape, and climate.
core_radius_km	km	Needed for core density and thermal-state interpretation.
mantle_density_avg	kg/m^3	Used for CMB pressure and rough geodynamic proxies.
core_density_avg	kg/m^3	Used for moment of inertia and magnetic-field likelihood proxies.
surface_g	m/s^2	Used for escape, retention, atmosphere scale height, and moon stability.
p_CMB	GPa	Feeds melting, core temperature, and thermodynamic scalings.
thermal_state	hot/warm/cold	Tracks whether the world is recently formed, intermediate, or cooled.

6.1. Thermal state and magma-ocean inheritance
Noack and Lasbleis (2020) also provide a useful way to represent initial or evolved thermal state without solving the full thermal history. StarGen should carry thermal_state as a categorical variable with three end-members: hot, warm, and cold. This is not mere fluff. It changes the inferred state of the core, the likely extent of a magma ocean, the vigor of outgassing, and the plausibility of early tectono-volcanic activity.
T_melt,sol(low P) = 1409.15 + 134.2p - 6.581p^2 + 0.1054p^3 + (102.0 + 64.1p - 3.62p^2)(0.1-#FeM)
T_melt,liq(low P) = 2035.15 + 57.46p - 3.487p^2 + 0.0769p^3
T_CMB,hot[K]  = 5400 · (p_CMB/140 GPa)^0.48 / (1 - ln(1-#FeM))
T_CMB,warm[K] = 5400 · (p_CMB/140 GPa)^0.48 / (1 - ln(1-XM0-#FeM))
• Use hot when the world is newly assembled, has experienced recent giant impacts, or is young enough that magma-ocean memory is still important.
• Use warm as the default post-solidification state for differentiated rocky planets lacking strong evidence of full cooling.
• Use cold for old stagnant-lid or efficiently cooled worlds when only present-day bulk structure is needed.
Recommended simplification: Do not solve a full radial temperature profile during base generation. Store thermal_state, CMB temperature estimate, and a magma_ocean_fraction or magma_ocean_depth proxy instead. Those can drive atmosphere, volcanic, and moon-formation logic later without forcing StarGen to pretend it is running a mantle convection code.

7. Volatiles, atmospheres, and hydrospheres
Atmospheres and surface water should not be generated as standalone flavor layers. The literature instead points to a linked budget problem: delivery, retention, partitioning, and loss. Ikoma et al. (2018) emphasize that water partitioning among atmosphere, magma ocean, solid mantle, and later surface reservoirs is coupled to accretion and thermal state. Wordsworth and Kreidberg (2022) emphasize that initial H/He capture, outgassing chemistry, redox state, condensation, and escape all matter for the eventual atmospheric regime.
7.1. Bulk volatile inventory
For StarGen, define a bulk volatile inventory rather than separate random draws for water, carbon, and nitrogen. The simplest robust structure is:
volatile_scalar_bulk: total volatile richness inherited from source region and delivery.
water_mass_fraction_bulk: fraction of planet mass initially attributable to H2O and hydrous phases.
carbon_scalar and nitrogen_scalar: optional secondary partitions derived from volatile_scalar_bulk and redox.
These values should depend on semimajor axis relative to the snow line, feeding-zone width, and late impact delivery. Gillmann et al. (2024) and Ikoma et al. (2018) both stress that volatile budgets are partly stochastic because delivery and late impacts are irregular. That is exactly the sort of randomness StarGen should keep, provided it is attached to explicit seed streams and bounded by source-region logic.
Table 5. Deterministic volatile budget inputs
Variable	Meaning	Recommended dependency
water_mass_fraction_bulk	Total water/ice inventory before partitioning.	Snow-line position, feeding-zone composition, late volatile delivery.
volatile_scalar_bulk	Overall volatile richness across H2O, C, N, S proxies.	Disk chemistry, metallicity, snow-line access, impact mixing.
redox_scalar	Bulk oxidation state used in outgassing rules.	Disk chemistry + mantle composition + stylized system scalar.
late_delivery_fraction	Volatiles added after primary assembly.	Impactor count, giant impacts, outer-belt scattering strength.
degassing_efficiency	Fraction available to atmosphere during magma-ocean or volcanic stages.	Thermal state, mantle volatile storage, escape history.

7.2. Primary envelope capture
Wordsworth and Kreidberg (2022) review evidence that many rocky or near-rocky planets may initially acquire thin H/He envelopes while gas remains in the disk, with the survivability of those envelopes depending strongly on mass, irradiation, and loss history. For StarGen, primordial envelope acquisition should be a function of at least four variables: core mass, gas-disk persistence at the moment of assembly, equilibrium temperature proxy, and early XUV environment.
f_env,0 = envelope_capture(core_mass, disk_lifetime_remaining, insolation, gas_mass_scalar)
• Below roughly Earth mass, allow only very thin captured envelopes unless the disk is unusually massive and cool.
• Near and above the rocky/sub-Neptune transition, permit a wider envelope distribution, especially if assembly finishes before gas dispersal.
• Flag retained primordial envelopes separately from secondary atmospheres. A planet may later lose the former and keep the latter.
7.3. Secondary atmosphere chemistry
The composition of a secondary atmosphere should primarily depend on redox state, volatile budget, and thermal state. Wordsworth and Kreidberg (2022) note that reducing magma-ocean conditions can outgas H2- and CO-rich mixtures, while more oxidized cases favor CO2- and H2O-rich atmospheres, with N2 as an important long-lived background gas in many regimes. StarGen does not need full equilibrium chemistry to use this insight.
Table 6. Secondary atmosphere branch logic
Regime	Deterministic trigger	Typical dominant gases
Reducing	Low redox_scalar and active magma-ocean / volcanic outgassing.	H2, CO, H2O, some CH4 if cold enough.
Intermediate	Moderate redox and mixed volatile budget.	H2O, CO2, CO, N2.
Oxidizing	High redox_scalar and silicate-dominated outgassing.	CO2, H2O, N2; O2 possible only under specific photochemical or escape histories.
Steam world	Surface too hot for liquid water reservoir.	H2O-rich atmosphere with possible CO2 admixture.
Airless / tenuous	Escape and condensation outpace replenishment.	Trace exosphere only.

7.4. Atmospheric loss and retention
Atmospheric loss is one of the places where StarGen most needs a surrogate rather than full physics. Catling and Kasting (2017) outline the major mechanisms: thermal escape, hydrodynamic escape, sputtering, and solar-wind-related loss. Wordsworth and Kreidberg (2022) also highlight the broad empirical relationship between escape and the combination of stellar flux and escape velocity sometimes called the cosmic shoreline. A practical StarGen solution is to compute a retention index rather than a detailed time-resolved escape model during base generation.
v_escape = sqrt(2GM_p / R_p)
retention_index = (v_escape^4 / insolation_relative) · xuv_survival_scalar
• Use retention_index as a soft classifier, not a hard law. It should bias the probability of losing H/He, shrinking steam atmospheres, or ending in an airless state.
• Apply stronger loss to young close-in planets around active stars, especially low-gravity worlds.
• Allow magnetic shielding only as a weak modifier unless a later module explicitly models magnetospheres.
7.5. Hydrosphere partition
Hydrosphere generation should be a partitioning step, not a new random draw. The bulk water inventory should be divided among mantle storage, atmospheric steam, surface liquid, and surface ice. Ikoma et al. (2018) show why this partition changes with magma-ocean evolution and atmospheric blanketing. For StarGen, a simple but useful state vector is:
water_mantle_fraction
water_atmosphere_fraction
water_surface_fraction
surface_water_phase = none / ice / liquid / mixed / steam
Those outputs should be computed from bulk inventory, insolation, pressure, gravity, and thermal state.
Table 7. Hydrosphere regime suggestions
Regime	Trigger	StarGen output
Dry rocky	Very low bulk water or severe early loss.	surface_water_phase = none
Ice-covered	Cold insolation regime with retained water.	surface_water_phase = ice or mixed
Temperate oceanic	Moderate insolation and adequate pressure.	surface_water_phase = liquid or mixed
Steam world	Water retained but equilibrium surface too hot.	surface_water_phase = steam
Buried/internal water	Water-rich interior but no stable surface reservoir.	hydrosphere below ice lid or crust; surface marked dry/frozen

8. Giant and volatile-rich planet module
Rocky interior scaling laws are not enough for sub-Neptunes and giants. For those classes, StarGen should switch to a structure model centered on heavy-element fraction, envelope mass fraction, irradiation, and age. The observational calibration from Müller et al. (2024) is useful here because it separates the rocky regime from the intermediate volatile-rich regime and the giant regime.
A practical StarGen representation is:
M_core = heavy-element core or deep heavy-element reservoir.
f_heavy = total heavy-element fraction.
f_env = H/He envelope mass fraction.
planet_subclass = gas giant / ice giant / sub-Neptune / stripped sub-Neptune.
For Neptune-like worlds, do not model them as simple scaled-up rocks with a skin of gas. Even simple review treatments emphasize that Uranus- and Neptune-like bodies contain only a modest fraction of H/He by mass compared with Jupiter and Saturn, with much of the mass residing in heavier materials and volatile layers (Helled & Howard, 2024).
Table 8. Recommended volatile-rich and giant state vector
Variable	Meaning	Use
f_env	Current H/He envelope fraction.	Controls radius inflation, atmosphere type, and stripping risk.
f_heavy	Heavy-element enrichment.	Distinguishes ice-giant analogs from H/He-dominated giants.
planet_subclass	Sub-Neptune / ice giant / gas giant / stripped core.	Controls descriptive and physical branches.
entropy_state	Hot/warm/cool giant-envelope state.	Proxy for age and cooling history.
irradiation_class	Cold / warm / hot / ultra-hot.	Affects radius modifier and cloud/chemistry outputs.
radius_model_source	Calibration or branch tag.	Preserves provenance for later validation.

Recommended simplification: Use class-based radius surrogates outside the rocky regime. For volatile-rich planets, calibrate radius primarily from mass, envelope fraction, age, and insolation. For giants, keep a weak mass dependence and stronger age / irradiation dependence. That is more honest than pretending one closed-form rocky relation can handle everything.

9. Moon systems
Moon generation should be host-class dependent. Barr (2016) is the key source for terrestrial impact moons in the current corpus, while Heller et al. (2014) provide the cleanest broad review of moon formation channels and orbital constraints.
9.1. Formation channels
Table 9. Moon channels by host type
Host type	Primary moon channel	Secondary channels
Rocky terrestrial	Giant-impact debris disk	Capture rare; primordial regular systems unlikely.
Water-rich rocky / dwarf	Impact or capture depending dynamical history	Co-accretion generally weak unless around giant host.
Gas giant	Circumplanetary gas disk / regular moon formation	Capture of irregular moons also common.
Ice giant	Circumplanetary disk plus later capture	Irregulars may dominate outer system.
Highly perturbed remnant world	Capture or none	Use only if architecture implies strong scattering.

For Earth-like moon formation, Barr (2016) remains useful because it ties the giant-impact pathway to real constraints: lunar mass, angular momentum, low iron fraction, volatile depletion, and partial melting rather than a completely molten outcome. StarGen does not need hydrocode-quality impact modeling to use this insight. It only needs to decide whether a giant impact occurred in the correct energy and angular-momentum regime to produce an impact disk rather than a clean merger or a failed grazing event.
R_Hill,planet = a_p · (M_p / (3 M_*))^(1/3)
a_Roche ≈ 2.44 · R_p · (rho_p / rho_s)^(1/3)   [fluid approximation]
Heller et al. (2014) summarize a standard practical bound: long-term stable prograde moons generally remain well inside roughly 0.49 planetary Hill radii, while retrograde moons can extend farther, up to roughly 0.93 Hill radii in favorable cases. For StarGen, keep regular moons prograde and inside 0.49 R_Hill by default.
Table 10. Deterministic moon generation constraints
Constraint	Recommended rule	Reason
Inner edge	a_moon > a_Roche	Avoid tidally disrupted regular moons.
Outer edge (prograde)	a_moon < 0.49 R_Hill	Long-term orbital stability.
Regular moon inclination	Near-equatorial	Consistent with disk formation.
Irregular moon inclination	Broad distribution	Consistent with capture/scattering.
Impact moon count around rocky planets	0-1 large moon usually; multiple tiny fragments optional	Earth-Moon-like systems are easier to keep coherent than artificial swarms.
Moon thermal state	Track magma-ocean probability for large fresh impact moons	Needed for geology and early atmosphere branches.

9.2. Practical terrestrial moon surrogate
A clean terrestrial-moon surrogate can be based on a giant-impact flag and a disk-yield score. Barr (2016) makes clear that moon-forming impacts are not arbitrary collisions. They must hit a plausible angular-momentum and composition window. A practical StarGen implementation should therefore compute at least:
impact_mass_ratio
impact_velocity_over_escape
impact_angle
postimpact_disk_mass_proxy
disk_iron_fraction_proxy
impact_moon_probability
If impact_moon_probability passes the chosen threshold, generate one dominant moon from a mass fraction tied to disk_yield and host mass, then age it thermally.
10. Suggested StarGen data schema
The model becomes manageable once the generator carries an explicit state vector instead of recomputing everything from prose categories. The schema below is deliberately implementation-oriented.
SystemState
{
    ulong systemSeed;
    string generatorVersion;
    double stellarMassMsun;
    double stellarLuminosityLsun;
    double stellarAgeGyr;
    double stellarMetallicity;
    double stellarXuvScalar;
    DiskState disk;
    List<PlanetState> planets;
}
PlanetState
{
    ulong localSeed;
    int orbitalIndex;
    double a_AU, e, i_deg;
    double formationMass_Mearth;
    double finalMass_Mearth;
    double bulkIronFraction;
    double mantleIronNumber;
    double coreMassFraction;
    double radius_km;
    double coreRadius_km;
    double mantleDensityAvg_kgm3;
    double coreDensityAvg_kgm3;
    double surfaceGravity_ms2;
    double cmbPressure_GPa;
    ThermalState thermalState;
    PlanetClass planetClass;
    double bulkWaterMassFraction;
    double volatileScalar;
    double redoxScalar;
    double envelopeMassFraction;
    AtmosphereClass atmosphereClass;
    HydrosphereClass hydrosphereClass;
    double waterMantleFraction;
    double waterAtmosphereFraction;
    double waterSurfaceFraction;
    List<MoonState> moons;
    PlanetProvenance provenance;
}
Provenance requirement: Retain feeding-zone bounds, merger count, giant-impact count, volatile-delivery fraction, and atmosphere/moon branch tags.

11. Deterministic algorithm sketch
1. Build SystemState from system_seed and generator_version.
2. Generate stellar outputs and disk controls.
3. Compute snow-line proxy and radial chemistry gradients.
4. Place embryos / cores using mutual-Hill spacing.
5. Advance deterministic growth model:
   - mergers
   - stripping events
   - gas capture if disk remains
   - migration before final freeze-out
6. Determine broad class outcome for each body.
7. If rocky:
   - compute XFe, #FeM, XFeM, XCMF
   - compute Rp, Rc, rho_m, rho_c, g, pCMB
   - assign thermal_state and magma-ocean proxy
8. Partition volatile budget:
   - bulk water
   - mantle storage
   - atmosphere
   - surface reservoir
9. Determine atmosphere class:
   - retained primordial
   - secondary reducing / intermediate / oxidizing
   - steam / tenuous / airless
10. If volatile-rich or giant:
    - compute envelope-based radius surrogate
    - assign subclass, heavy-element fraction, entropy_state
11. Generate moons from host class and impact history.
12. Apply Roche and Hill stability filters.
13. Age system to stellarAgeGyr:
    - escape
    - cooling
    - tidal migration proxy
14. Write observables and provenance.
12. Calibration and validation targets
StarGen should ship with regression tests tied to known Solar System anchors and to the literature domains from which its surrogates were derived. The point is not to force exact replication. The point is to keep the generator inside a defensible physical envelope.
Table 11. Suggested validation set
Target	What to test	Expected behavior
Earth analog	Rocky interior solver	Radius, CMF, gravity, and water partition land in an Earth-like range.
Venus analog	Dry/hot rocky atmosphere branch	Dense secondary atmosphere possible without requiring surface water.
Mars analog	Low-mass escape and tenuous atmosphere	Weaker retention and smaller core-related signatures.
Sub-Neptune analog	Envelope retention	Radius inflated above rocky expectation at same mass.
Neptune/Uranus analog	Ice-giant branch	Heavy-element-rich world with moderate H/He fraction.
Earth-Moon analog	Impact moon surrogate	One large prograde moon outside Roche limit and inside stable Hill fraction.
Compact multiplanet system	Spacing filters	No adjacent planets below the stability floor unless instability flag is set.

13. Limits of the model
Three limits should be stated explicitly.
First, the Noack and Lasbleis rocky interior relations are calibrated for Earth-like mineralogy and a mass range centered on super-Earth-scale rocky bodies, not arbitrary massive planets. Use them confidently inside their stated range and with caution outside it.
Second, atmosphere generation in StarGen is necessarily a surrogate model. It can be physically informed without resolving full chemistry, climate, and escape histories. That is acceptable, provided the generator stores enough latent state to support a later upgrade.
Third, terrestrial moon generation is the most history-sensitive part of the system. Barr (2016) makes clear that moon-forming impacts occupy a constrained window. A single deterministic giant-impact surrogate is appropriate for a generator, but it should be documented as such rather than oversold as a solved origin model.
Bottom line: The strongest version of StarGen is not the one with the most formulas. It is the one where every formula sits in the right place in the pipeline, every random draw has a parent cause, and the final planet can explain itself.

14. AI use statement
This report was prepared using AI assistance. The AI system was used to gather, synthesize, and translate full-text academic literature into an implementation-oriented technical specification for StarGen. Source selection was restricted to academic materials whose full text was directly accessible during drafting, including user-supplied PDFs and openly accessible scholarly sources. The resulting specification should still be treated as a design document rather than as a substitute for direct numerical simulation or domain-expert peer review.
References
Barr, A. C. (2016). On the origin of Earth’s Moon. Journal of Geophysical Research: Planets, 121, 1573–1601.. doi:10.1002/2016JE005098
Catling, D. C., & Kasting, J. F. (2017). Escape of atmospheres to space. In Atmospheric Evolution on Inhabited and Lifeless Worlds (pp. 129–168). Cambridge University Press.
Gillmann, C., Hakim, K., Lourenço, D., Quanz, S. P., & Sossi, P. A. (2024). Interior controls on the habitability of rocky planets. Space: Science & Technology, 4, 0075.. doi:10.34133/space.0075
Heller, R., Williams, D., Kipping, D., Limbach, M. A., Turner, E., Greenberg, R., Sasaki, T., Bolmont, É., Grasset, O., Lewis, K., Barnes, R., & Zuluaga, J. I. (2014). Formation, habitability, and detection of extrasolar moons. Astrobiology, 14(9), 798–835.. doi:10.1089/ast.2014.1147
Ikoma, M., Elkins-Tanton, L., Hamano, K., & Suckale, J. (2018). Water partitioning in planetary embryos and protoplanets with magma oceans. Space Science Reviews, 214(4), 76.. doi:10.1007/s11214-018-0508-3
Müller, S., Baron, J., Helled, R., Bouchy, F., & Parc, L. (2024). The mass-radius relation of exoplanets revisited. Astronomy & Astrophysics, 686, A296.. doi:10.1051/0004-6361/202348690
Noack, L., & Lasbleis, M. (2020). Parameterisations of interior properties of rocky planets: An investigation of planets with Earth-like compositions but variable iron content. Astronomy & Astrophysics, 638, A129.. doi:10.1051/0004-6361/202037723
Tamayo, D., Cranmer, M., Hadden, S., Rein, H., Battaglia, P., Obertas, A., Armitage, P. J., Ho, S., Spergel, D., Gilbertson, C., Hussain, N., Silburt, A., Jontof-Hutter, D., & Menou, K. (2020). Predicting the long-term stability of compact multiplanet systems. Proceedings of the National Academy of Sciences, 117(31), 18194–18205.. doi:10.1073/pnas.2001258117
Wordsworth, R., & Kreidberg, L. (2022). Atmospheres of rocky exoplanets. Annual Review of Astronomy and Astrophysics, 60, 159–201.. doi:10.1146/annurev-astro-052920-125632
Helled, R., & Howard, S. (2024). Giant planet interiors and atmospheres. arXiv preprint arXiv:2407.05853.
