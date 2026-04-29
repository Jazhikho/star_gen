# RPG Compatibility Generation Audit

This document answers two questions:

1. What do the target RPG systems actually generate during world or sector creation?
2. What academically grounded baseline should StarGen use for sentient-world outputs before those outputs are adapted into Traveller, Cepheus, Starfinder, or Starforged-compatible readouts?

Scope note:

- This is a compatibility and grounding audit, not a claim that StarGen currently matches every external system.
- "Academic baseline" here means science-backed or social-science-backed surrogate modeling, not a claim that sentient societies can be predicted from physics alone.
- Flavor-only outputs such as `alignment`, `magic`, or oracle prompts should not be presented as scientific claims. Those should remain adapter-layer or setting-layer outputs, not realism-layer outputs.

## External system outputs

### Traveller

Official Traveller SRD world creation produces more than a single planet profile. It produces or expects:

- Subsector world occurrence by hex density.
- Mainworld UWP: starport, size, atmosphere, hydrographics, population, government, law level, tech level.
- Bases.
- Gas giant presence.
- Travel zone.
- Trade codes.
- Polities.
- Communications routes.
- Trade routes.
- Factions on the world.

Sources:

- [Traveller SRD: World Creation](https://www.traveller-srd.com/core-rules/world-creation/)
- [Traveller SRD: Technology Levels](https://www.traveller-srd.com/core-rules/technology-levels/)

### Cepheus

Cepheus SRD keeps the Traveller-family structure, but it makes some system-level outputs more explicit in the one-line profile and surrounding procedures. A generated world or system includes:

- Hex location on subsector or sector map.
- UWP: starport, size, atmosphere, hydrographics, population, government, law level, tech level.
- Bases.
- Trade codes or remarks.
- Travel zone.
- Population modifier.
- Number of planetoid belts.
- Number of gas giants.
- Interstellar allegiance.
- Communications routes and trade routes.

Sources:

- [Cepheus SRD: Worlds](https://cepheus-srd.opengamingnetwork.com/cepheus-engine-srd/cepheus-engine-worlds/)

### Starfinder

Starfinder's world-building emphasis is different. The official Galaxy Exploration Manual material is closer to a worldbuilding toolkit than a single compact UWP. The generated outputs described in Paizo's official material include:

- World type.
- Gravity.
- Atmosphere.
- One or more biomes.
- Cultural or setting attributes such as accord, alignment, magic, religion, and technology.
- Inhabitants, creatures, and adventure hooks.
- Settlement-oriented toolboxes for quirks, challenges, and locations.

This is a mixed physical-plus-cultural generation stack, not just a physical world statline.

Sources:

- [Paizo Blog: A Galaxy Of Worlds](https://paizo.com/blog/a-galaxy-of-worlds)
- [Paizo Blog: A Galaxy Of Toolboxes](https://paizo.com/blog/a-galaxy-of-toolboxes)
- [Paizo product page: Starfinder Galaxy Exploration Manual](https://paizo.com/products/btq024zu/)

### Starforged

Starforged is also not a compact UWP-style generator. Its official framing emphasizes campaign-launch truths and oracle-driven discoveries. Official Starforged material describes:

- Setting truths for the Forge.
- A backdrop of challenges at campaign launch.
- Generators for planets and settlements.
- Generators for people, creatures, factions, and starships.
- Generators for derelicts and vaults.
- Sector and discovery-oriented prompts rather than a deterministic government or law statline.

That means Starforged compatibility should be thought of as an oracle-facing frontier profile, not as a Traveller-family export with different names.

Sources:

- [Tomkin Press: Ironsworn Starforged](https://tomkinpress.com/pages/ironsworn-starforged)
- [Tomkin Press: Starforged Truths Workbook](https://tomkinpress.com/products/ironsworn-starforged-truths-workbook)

## Current StarGen coverage

### Traveller

Traveller is the only compatibility path in StarGen that currently has a dedicated generation layer.

Current code already produces:

- A typed `TravellerWorldProfile`.
- UWP string formatting.
- Traveller trade codes.
- Travel zone.
- Route profile.
- Mainworld takeover of a realistic system.
- Traveller-style government and tech backfill into local StarGen population structures.

Current files:

- `src/domain/generation/Traveller/TravellerWorldProfile.cs`
- `src/domain/generation/Traveller/TravellerWorldGenerator.cs`
- `src/domain/generation/Traveller/TravellerWorldGenerator.Systems.cs`
- `src/domain/generation/Traveller/TravellerSystemGenerator.cs`

Current weakness:

- Government, law, starport, and tech backfill are still heuristic adapters over StarGen population data, not academically grounded social models.

### Cepheus

Cepheus currently resolves to a compatibility profile and shares the Traveller-family UWP leaning, but it does not yet have a first-class `CepheusWorldProfile` or a dedicated Cepheus export layer.

Current gaps:

- No explicit PBG output.
- No allegiance output.
- No Cepheus-specific one-line world profile.
- No dedicated system-level belts or gas-giants summary export.

### Starfinder

Starfinder currently exists as a compatibility-pressure profile only.

Current code does bias:

- Mainworld preference.
- Life permissiveness.
- Colony probability.
- Colony-type weighting.
- Harsh-world colony tolerance.
- Hydrosphere lean.

But it does not yet generate Starfinder-style worldbuilding outputs such as:

- Accord.
- Magic level.
- Religion framing.
- Biome-driven cultural synthesis.
- Settlement quirks and settlement challenges.

### Starforged

Starforged also currently exists as a compatibility-pressure profile only.

Current code does bias:

- Frontier harshness.
- Colony pressure.
- Harsh-world settlement tolerance.
- Colony-type distribution toward frontier, scientific, refugee, and separatist settlements.
- Drier mainworld lean.

But it does not yet generate Starforged-style outputs such as:

- Truths.
- Sector-launch backdrop.
- Frontier faction pressures.
- Settlement rank or settlement trouble.
- Oracle-facing prompt bundles for communities, derelicts, or vaults.

## Sentient-world realism audit

For worlds with sentient life, StarGen currently produces some useful raw ingredients, but the social output layer is still much weaker than the physical and biosphere layers.

### What is already usable

- Ecological and biosphere gating is now much more explicit.
- Planetary carrying-capacity surrogates are good enough to inform broad settlement scale.
- Resource richness and resource diversity are now available as world-level signals.
- Colony suitability already tracks survivability, resources, and self-sufficiency pressure.

### What is still weak

- Native government generation is still mostly a tech-era switch plus random selection.
- Traveller law level is derived from coercion, capacity, and inclusiveness, but without a real legal-development model.
- Technology level is still mostly an age ladder with a small habitability or resource modifier.
- There is no academically grounded state-capacity axis.
- There is no academically grounded factional-fragmentation axis.
- There is no academically grounded trade-connectivity or adoption-capacity axis for technology.
- Religion, cultural complexity, and legitimacy are still too heuristic to call source-grounded.

## Proposed academic baseline

The right move is not to generate `government`, `law`, `tech`, `accord`, `allegiance`, or `faction pressure` directly from a single dice roll or a single population count. StarGen should first derive a small set of latent societal variables, then map those variables into ruleset-specific outputs.

### Recommended latent variables

For any sentient world, build a deterministic `SentientWorldProfile` from the biosphere, population, and interstellar context with these axes:

- `SocialScale`: how many people and institutions the world can sustain.
- `SurplusBase`: food, energy, extractable materials, and transportable surplus.
- `TradeConnectivity`: how connected the world is to neighboring worlds and interstellar exchange.
- `ExternalThreat`: war pressure, predation pressure, frontier instability, or contested borders.
- `StateCapacity`: administrative reach, taxation or extraction ability, logistics, and enforcement reach.
- `FiscalContract`: how much governance depends on bargaining with taxpayers, citizens, guilds, or settlements.
- `LegalCentralization`: whether conflict resolution is local and plural or centralized and codified.
- `CulturalAccumulation`: how much cumulative cultural knowledge can be retained and transmitted.
- `TechnologyAdoptionCapacity`: how quickly a society absorbs and scales new techniques.
- `FactionalFragmentation`: how divided the polity is among rival elites, regions, or communities.
- `ReligiousCentralization`: placeholder axis for later human-reviewed religion modeling.

### Academic anchors

#### Population, scale, and technology

Kremer (1993) argues that larger populations can accelerate technological change because more people means more potential innovators. That is too coarse to use alone, but it supports tying technology potential partly to social scale rather than only to elapsed years.

Henrich et al. (2016) on cumulative cultural evolution supports treating technology as a retention-and-transmission problem, not just a genius-inventor problem. That means small, isolated, or fragmented societies should lose or plateau in complex techniques more easily than large, connected societies with strong knowledge transmission.

Technology diffusion and adoption are modeled separately from invention pace: tech level and adapter-facing readouts should depend on trade connectivity and absorption capacity, especially for colonies and worlds inside larger interstellar networks. That split is a StarGen design choice, not a single-citation empirical claim.

#### Governance and regime formation

Fiscal bargaining breadth and public-goods dependence are modeled as governance inputs so that broad-bargain polities and extraction-heavy or conquest-tilted polities are not forced into the same regime distribution. Those axes are design variables for sentient-world output, not single-source empirical claims.

Turchin (2010) and related Seshat work support giving warfare and frontier pressure a real role in pushing institutions toward greater scale, hierarchy, and coordination. That does not mean "war always improves states," but it does justify using external threat as one of the main regime-shaping inputs.

#### Law and legal reach

Pospisil (1967) is useful because it frames law as layered and plural rather than assuming a single monopoly legal order. That is the right baseline for lower-capacity or fragmented worlds: low law level should often mean plural or patchy authority, not simply "freedom."

Chowdhury (2022) supports tying regulation to state capacity. A world cannot sustain high formal restriction if it lacks enough enforcement reach to make those restrictions real.

Katz et al. (2020) support the idea that legal complexity grows with social complexity and interconnection. In StarGen terms, higher law levels or more detailed legal structures should track administrative scale, trade complexity, and institutional density rather than being random flavor.

## Baseline mapping rules

These should become the default sentient-world realism path before ruleset adapters are applied.

### Population and settlement form

Base settlement scale on:

- carrying capacity from biosphere and resource state,
- transportability of surplus,
- environmental harshness,
- trade connectivity,
- and tech adoption capacity.

Suggested outputs:

- `SettlementPattern`: dispersed, clustered, corridor, archipelago, orbital-heavy, arcology-heavy.
- `UrbanizationShare`.
- `PrimarySettlementRank`: outpost, town, city, metroplex, world-city.

### Government or regime

Generate regime from the interaction of:

- `SocialScale`,
- `FiscalContract`,
- `ExternalThreat`,
- `FactionalFragmentation`,
- and `StateCapacity`.

Expected tendencies:

- High threat plus low bargaining tends toward militarized, centralized, or extractive regimes.
- High bargaining plus strong state capacity tends toward constitutional, bureaucratic, or representative regimes.
- Low scale plus low capacity tends toward tribal, chiefdom, city-state, or failed-fragment regimes.

### Law level

Generate law from:

- `StateCapacity`,
- `LegalCentralization`,
- `TradeConnectivity`,
- `ExternalThreat`,
- and `Government coercion`.

Important interpretation rule:

- Low law should usually mean limited reach or plural authority, not automatically liberal rights.
- High law should mean high restriction only when coercion is strong.
- High-capacity participatory states can still have high legal complexity with moderate restriction.

That distinction matters for Traveller and Cepheus mapping, because their published `Law Level` compresses several real dimensions into one digit.

### Technology level

Generate technology from:

- `CulturalAccumulation`,
- `TechnologyAdoptionCapacity`,
- `TradeConnectivity`,
- `SurplusBase`,
- and elapsed development time.

Important rule:

- Technology should not advance purely with age.
- Isolated worlds should stall more often.
- Connected worlds should adopt faster than they invent.
- Harsh frontier colonies can have high imported technology even when local institutional complexity is low.

This is especially important for Traveller, Cepheus, and Starfinder compatibility because all three can produce worlds whose local political order is simpler than the technology they use.

### Starport, logistics, and trade

Starport or port-quality outputs should be derived from:

- `TradeConnectivity`,
- `TechnologyAdoptionCapacity`,
- `StateCapacity`,
- and `Population scale`.

This should be distinct from regime type. A corporate enclave, military fortress, democracy, or monarchy can all support a high-grade port if their logistics and trade integration justify it.

### Factions and fragmentation

Faction count and faction strength should not be random flavor layered after government generation. They should respond to:

- `FactionalFragmentation`,
- `SettlementPattern`,
- `ExternalThreat`,
- `Legitimacy`,
- and `SocialScale`.

This will matter most for:

- Traveller or Cepheus faction summaries,
- Starfinder settlement or culture complications,
- Starforged community trouble and faction pressure.

## Ruleset adapter guidance

The realism layer should produce latent societal state and neutral outputs first. The compatibility layer should then adapt that into each ruleset.

### Traveller and Cepheus adapter

Use the neutral sentient-world profile to derive:

- population code,
- government code,
- law level,
- tech level,
- starport,
- trade codes,
- travel zone,
- allegiance,
- and any faction summary.

Do not roll these independently if realism mode is active. Roll-only generation should be reserved for explicit "table-faithful" compatibility mode.

### Starfinder adapter

Treat these as mostly adapter-layer outputs:

- `Accord` from trade connectivity, polity scale, and diplomatic integration.
- `Technology` from neutral tech outputs.
- `Religion` from later human-reviewed religion modeling.
- `Magic` as setting or genre overlay, not scientific output.
- settlement quirk or settlement challenge from environment plus faction pressure, not from science claims.

### Starforged adapter

Treat Starforged outputs as oracle-facing transformations of the neutral profile:

- frontier pressure from harshness plus low state capacity,
- settlement rank from scale plus connectivity,
- faction tensions from fragmentation plus threat,
- truths hooks from regional scarcity, political fracture, and remoteness.

Starforged compatibility should feel frontier and prompt-rich, but the realism layer underneath should still come from the same neutral societal state.

## What should not be science-washed

These should remain explicit adapter or setting outputs unless a later dedicated review is completed:

- alignment,
- magic prevalence,
- religion doctrine content,
- culture flavor text,
- language flavor text,
- Starforged oracle prose.

Those can still be generated, but they should not be described as academically grounded merely because the underlying world physics is.

## Recommended implementation order

1. Add a `SentientWorldProfile` or similarly named latent-state object in `src/domain/population/`.
2. Rebuild `NativePopulationGenerator.DetermineTechLevel(...)` around scale, accumulation, and adoption instead of age plus noise.
3. Replace direct regime picking in `NativePopulationGenerator.GenerateGovernment(...)` and `ColonyGenerator.GenerateGovernment(...)` with regime resolution from latent societal axes.
4. Replace Traveller law mapping with a real legal-capacity and legal-centralization resolver, then compress that into Traveller or Cepheus law digits.
5. Add neutral settlement-pattern and starport-quality outputs before adding more ruleset-specific adapters.
6. Keep religion and culture under explicit human-audit gating until a dedicated review is done.

## Bottom line

StarGen is already in decent shape for physical world generation and is improving quickly on biosphere realism. The weakest remaining realism area is now the transition from "sentient life exists" to "what kind of society forms here."

The correct next step is not separate ad hoc tables for Traveller, Cepheus, Starfinder, and Starforged. The correct next step is a shared, source-grounded sentient-world baseline that models:

- scale,
- surplus,
- connectivity,
- threat,
- capacity,
- bargaining,
- legal centralization,
- and cumulative culture.

Once that exists, the RPG adapters become much cleaner and much more defensible.

Implementation note:

- The current repo now carries that neutral baseline on `PlanetPopulationData.SentientWorldProfile`, with the field-by-field presentation rationale documented in [SentientWorldBaseline.md](SentientWorldBaseline.md).
