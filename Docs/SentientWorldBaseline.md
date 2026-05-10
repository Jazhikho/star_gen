# Sentient World Baseline

This document defines the neutral sentient-world baseline now carried by `PlanetPopulationData.SentientWorldProfile`.

Purpose:

- Give planets and moons a shared, source-grounded societal baseline before any Traveller, Cepheus, Starfinder, or Starforged adapter compresses that state into ruleset-facing outputs.
- Keep the realism layer focused on defensible structural variables rather than pretending that doctrine, alignment, religion content, or oracle prose are scientific outputs.
- Explain why the Object Viewer inspector now presents the specific population fields that it does when a body is inhabited.

Scope note:

- These fields appear only when the body has active population.
- The baseline is neutral and structural. It does not claim to predict culture, belief content, language flavor, or narrative destiny.
- This is still surrogate modeling. It is more defensible than ad hoc dice tables, but it is not a first-principles social simulator.

## Inspector-facing fields

### Highest Tech

Why it is shown:

- Traveller, Cepheus, and Starfinder all need a technology-facing readout.
- The baseline now exposes the highest active technology level directly because adapters often need to distinguish imported advanced technology from lower local institutional complexity.

Grounding:

- Kremer (1993) supports scale-sensitive innovation potential.
- Henrich et al. (2016) supports cumulative cultural retention as a real limit on complex techniques.
- Technology diffusion and adoption are modeled as distinct from invention pace for adapter-facing tech readouts.

Presentation logic:

- The inspector shows the highest active level because that is the most useful adapter-facing summary for port quality, offworld trade, and compatibility exports.

### Elite Tech Access / Median Tech Access

Why they are shown:

- Highest available technology can be imported, elite-held, or institutionally concentrated.
- Median access is a better proxy for what most residents can actually use.

Grounding:

- Knez (2023), Comin and Mestieri (2013), Comin and Lashkari (2013), and Stokey (2020) support treating diffusion and adoption density as distinct from invention or first access.
- Arvidsson et al. (2023) cautions that urban scaling benefits can be strongly driven by upper-tail access rather than uniform gains.

Presentation logic:

- `Elite Tech Access` starts from the highest active technology level.
- `Median Tech Access` is reduced by adoption lag and access inequality, then bounded so it never exceeds elite access.

### Core Tech / Elite Core Tech / Median Core Tech

Why they are shown:

- The legacy `TechnologyLevel.Level` enum is useful as a readable era label, but RPG compatibility needs finer deterministic numeric codes.
- The neutral core scale runs from `0` to `24` and is the source of Traveller/Cepheus tech codes rather than the other way around.

Mapping:

- `0-1`: Stone.
- `2`: Bronze.
- `3`: Iron.
- `4`: Classical.
- `5`: Medieval.
- `6`: Renaissance.
- `7`: Industrial.
- `8`: Atomic.
- `9`: Information.
- `10-11`: Spacefaring.
- `12-15`: Interstellar.
- `16-24`: Advanced.

Traveller/Cepheus mapping:

- Core `0` maps to TL `0`.
- Core `1-2` maps to TL `1`.
- Core `3` maps to TL `2`, then each historical band climbs through TL `12` by core `15-16`.
- Core `17-18` maps to TL `13`, core `19-20` maps to TL `14`, and core `21-24` maps to TL `15`.

Presentation logic:

- `Core Tech` is the overall neutral technology depth after adoption, economy, and lag pressure.
- `Elite Core Tech` is the highest institutional or elite-access level.
- `Median Core Tech` is the broad resident-access level and is bounded below elite access.

### Tech Domains

Why they are shown:

- A world can have high elite technology in one domain and weaker broad access in another. A frontier mining moon may have strong spaceflight and energy maintenance but poor medicine access; a mature biosphere world may have broad medicine and infrastructure but less imported interstellar machinery.
- The neutral profile now carries domain records for `Energy`, `Materials`, `Computing`, `Communications`, `Medicine`, `Biotechnology`, `Spaceflight`, and `Infrastructure`.

Grounding:

- These are adoption-access records, not invention trees. Each domain combines the global core tech scale with trade, infrastructure, state capacity, economic complexity, resources, biology support, harsh-world pressure, and adoption lag.
- This remains a source-hardened proxy. It should not be read as a full technology history or a complete future-tech taxonomy.

Presentation logic:

- Each domain stores core, elite, and median neutral tech levels; adoption capacity; lag pressure; access inequality; and a compact source signal such as `local-capability`, `trade-diffused`, `frontier-lagged`, `elite-concentrated`, `necessity-driven`, or `limited-access`.
- Object Viewer summarizes domains as median/elite pairs. Exporters and RPG adapters can use the detailed records when they need more than a single world tech number.

### Dominant Regime

Why it is shown:

- Traveller-family government codes and several settlement-focused RPG outputs still need a polity-facing baseline.
- The inspector shows the dominant active regime as a summary of which institutional form currently governs the largest share of the population.

Grounding:

- Fiscal bargaining breadth and public-goods dependence are treated as major regime-shaping inputs distinct from pure extraction or conquest logic.
- Turchin (2010) supports threat and large-scale coordination pressure as real institutional drivers.

Presentation logic:

- The regime is shown as a world summary, not as a claim that the world is culturally homogeneous.
- Fragmented or mixed worlds are further described by the fragmentation and coexistence-facing metrics below.

### Settlement Pattern

Why it is shown:

- Traveller, Starfinder, and Starforged compatibility often care whether a world is dispersed, clustered, corridor-linked, archipelagic, orbital-heavy, or arcology-heavy.
- This field supports later mapping into settlement toolboxes, faction geography, and port placement.

Grounding:

- It is derived from geography, water or land partition, moon harshness, urbanization, and trade intensity.
- That is more defensible than direct random settlement labels because transport corridors, fragmented coastlines, and sealed hostile-habitat clustering are physical and logistical consequences of the world state.

Presentation logic:

- `Archipelago` indicates highly water-partitioned settlement geography.
- `Corridor` indicates trade-connected linear settlement concentration.
- `Orbital-Heavy` indicates that harsh moons with active population are likely concentrating activity in sealed hubs and orbital infrastructure.
- `Arcology-Heavy` is reserved for very urban, high-capacity, high-tech cases.

### Primary Settlement

Why it is shown:

- RPG systems repeatedly want a coarse answer to "what is the main settlement scale here?"
- The baseline exposes `Outpost`, `Town`, `City`, `Metroplex`, or `World-City` as a neutral world summary.

Grounding:

- It is derived from total population and urbanization share.
- This is a scale proxy, not a claim about one specific named city.

### Logistics Capacity

Why it is shown:

- Traveller and Cepheus need starport-like thinking.
- Starfinder and Starforged also benefit from knowing whether a world is isolated, frontier-linked, regionally connected, or a major hub.

Grounding:

- It is derived from trade connectivity, state capacity, social scale, technology adoption capacity, and active technology level.
- This keeps logistics quality tied to network reach and institutional support rather than to regime flavor alone.

Presentation logic:

- The inspector deliberately shows a neutral logistics tier instead of a Traveller starport letter because the realism layer should remain ruleset-agnostic.

## Structural axes

### Social Scale

Grounding:

- Larger populations and more active institutional groups can support more specialization and governance complexity.
- Kremer (1993) is the main anchor for tying scale to innovation potential.

Why it is shown:

- It is the base variable behind later tech, governance, settlement-rank, and faction outputs.

### Surplus Base

Grounding:

- Surplus comes from habitability, resources, water support, suitability, and self-sufficiency.
- This is the best available bridge between physical world state and later specialization, state support, and urban concentration.

Why it is shown:

- Many RPG-facing outputs become easier to justify if the user can see whether a world is structurally rich or structurally strained.

### Trade Connectivity

Grounding:

- Trade connectivity is modeled as a driver of technology diffusion and adoption, alongside logistics, political bargaining, and settlement concentration.

Why it is shown:

- This is a key bridge variable for port quality, tech adoption, alliance pressure, and colony-world character.

### External Threat

Grounding:

- Turchin (2010) supports using sustained threat and frontier pressure as inputs to coordination and institutional hardening.
- The baseline treats native-colony conflict, harsh frontier conditions, and coexistence pressure as the current best deterministic proxies.

Why it is shown:

- This helps explain why worlds with similar population size can diverge toward different governance or legal outcomes.

### State Capacity

Grounding:

- Chowdhury (2022) supports tying regulation and enforcement to real administrative reach.
- This baseline estimates capacity from the active governments already present, then scales it by trade, surplus, and social scale.

Why it is shown:

- This is the main anchor for later law, port, and government adaptation.

### Fiscal Contract

Grounding:

- Bargaining scope and public-goods dependence are modeled as central governance variables alongside threat and scale drivers.

Why it is shown:

- It explains why not all large states should compress to the same government or law readout.

### Legal Centralization

Grounding:

- Pospisil (1967) supports plural layered legal authority rather than assuming a single monopoly of law.
- Katz et al. (2020) supports increasing legal complexity with growing social complexity and interconnection.

Why it is shown:

- Traveller-family law digits collapse several real dimensions. This field helps separate centralization from pure restriction.

### Legal Reach

Grounding:

- Legal reach is shown separately from centralization because a world can claim centralized law without applying it consistently across all regions or populations.
- Chowdhury (2022) is the main anchor for keeping formal regulation distinct from the harder state functions needed to enforce it.

Why it is shown:

- This is the clearer realism-layer answer to "how far does formal law claim to reach?"

### Enforcement Reach

Grounding:

- Enforcement reach is the practical companion to legal reach: it is reduced by frontier pressure and terrain fragmentation, and supported by state capacity, centralization, coercive capacity, trade, and surplus.
- This follows the source-hardening rule that regulation, legal claims, and practical state capacity should not be collapsed into one number.

Why it is shown:

- It helps adapters distinguish worlds with high formal law but weak practical enforcement from worlds where institutions can actually project authority.
- It also keeps restriction pressure interpretable: restrictive rules can exist even when enforcement is patchy.

### Restriction Pressure

Grounding:

- Restriction should depend on coercion, threat, and inclusiveness rather than being treated as identical to legal presence.

Why it is shown:

- It gives the adapters a more honest precursor for Traveller-style law levels than a single random digit would.

### Law Level / Law Interpretation

Grounding:

- The compressed `0-15` law level is derived from legal reach, enforcement reach, legal centralization, restriction pressure, state capacity, threat, and regime coercion.
- This keeps low formal reach separate from low restriction: a plural or customary low-reach world should not look the same as a high-capacity low-restriction legal order.

Presentation logic:

- The inspector shows the numeric code alongside one of the compact interpretations: `No Formal Reach`, `Plural/Customary`, `Patchy Formal Law`, `Codified Moderate Reach`, `High-Capacity Legal Order`, or `Restrictive High-Enforcement Order`.
- Traveller/Cepheus adapters use this neutral law code as the preferred UWP law source.

### Jurisdiction Structure / Jurisdiction Pluralism / Jurisdiction Conflict

Grounding:

- Formal law should not imply one unified legal authority. Mixed native-colony worlds, frontier worlds, and fragmented settlement geographies can have overlapping authorities even when a formal code exists.
- `JurisdictionPluralism` responds to weak legal centralization, factional fragmentation, coexistence pressure, terrain fragmentation, and active group count.
- `JurisdictionConflict` responds to fragmentation, external threat, low internal legitimacy, coexistence pressure, restriction pressure, and weak state capacity.

Presentation logic:

- `Jurisdiction Structure` is one of: `Informal Local`, `Layered Customary`, `Patchwork Formal`, `Charter/Federal`, `Centralized Unitary`, `Extraterritorial/Imperial`, or `Contested Jurisdictions`.
- These fields are neutral readouts. They do not name courts, write statutes, assign cultural doctrine, or claim a final legal anthropology model.
- Future adapters should use these fields before generating Traveller factions, Starfinder settlement complications, Starforged community trouble, or export schemas that need multiple legal authorities.

### Cultural Accumulation

Grounding:

- Henrich et al. (2016) supports cumulative culture as a retention-and-transmission problem.

Why it is shown:

- This explains part of the difference between old isolated worlds and old connected worlds, and between raw age and actually retained complexity.

### Tech Adoption

Grounding:

- Diffusion and adoption are explicit axes separate from invention-only or age-only technological depth.

Why it is shown:

- It helps future adapters distinguish imported or network-supported technology from purely local developmental depth.

### Invention Capacity

Grounding:

- Bettencourt et al. (2007) supports population-sensitive urban output and innovation pressure.
- Balland et al. (2022) supports treating capability portfolios and relatedness as constraints on what a society can produce.

Why it is shown:

- It separates local production and invention pressure from the mere presence of imported or elite-held technology.

### Adoption Lag

Grounding:

- Knez (2023), Comin and Mestieri (2013), Comin and Lashkari (2013), and Stokey (2020) support the idea that diffusion timing, implementation costs, and penetration rates vary independently of first availability.

Why it is shown:

- It gives adapters a direct signal for worlds where advanced technology exists but broad adoption is slow because of frontier conditions, weak capacity, low self-sufficiency, or geography.

### Tech Access Gap

Grounding:

- Arvidsson et al. (2023) refines urban scaling by showing that much of the apparent output gain can be concentrated in distribution tails.
- This field remains a human-audit-required proxy and should not be treated as a universal alien-society law.

Why it is shown:

- It distinguishes high-capability but unequal worlds from worlds where advanced techniques are broadly available.

### Factional Fragmentation

Grounding:

- Fragmentation responds to multiple active population blocs, coexistence pressure, conflict, weak state reach, and fragmented settlement geography.

Why it is shown:

- Traveller factions, Starfinder settlement complications, and Starforged community tension all need a defensible precursor.

### Factions

Grounding:

- Factions are now first-class deterministic records, not adapter-only flavor.
- Each record carries an id, name, type, influence share, regime alignment, tension level, source population id, and primary issue.

Presentation logic:

- Names remain generic and structural, such as governing, native, colony, opposition, or security blocs.
- The baseline does not invent ideology, doctrine, faction slogans, or narrative claims; those remain setting or adapter material.

### Religious Centralization

Grounding:

- This is intentionally narrow: it models institutional centralization potential, not doctrine or belief content.
- It is the one baseline field here that must be treated as provisional until a later dedicated human-audited religion pass is complete.

Why it is shown:

- Some compatibility systems want religion-facing structural context, but the realism layer must stop at institutional concentration rather than inventing belief claims.

### Cultural Feature Tags / Religion Structure

Grounding:

- Culture remains a structured tag layer only. It can say that a world is `trade-connected`, `plural-authority`, `tech-stratified`, or `native-colony-contact`; it does not generate doctrine or prose culture.
- Religion structure is institutional only: `None`, `Localized`, `Plural`, `Centralized`, `State-Aligned`, or `Suppressed`.

Presentation logic:

- These fields provide RPG adapter inputs while preserving the human-audit boundary around culture and religion content.
- Starfinder and Starforged can later transform these signals into setting-facing prompts, but the neutral baseline remains compact and auditable.

### Life Biomes

Grounding:

- Physical `PlanetProfile.Biomes` remains the environment and terrain distribution.
- `AvailableLifeBiomes` is a separate readout and is populated only when the biology support evaluator says the world can support native biology.

Presentation logic:

- Barren, volcanic, ice-sheet, and gas-giant physical biomes never appear as life biomes.
- `Subsurface` appears only when the support evaluator exposes protected-biosphere support.
- Non-life-capable worlds show no life-biome list even when their physical terrain data contains barren, ice, gas-giant, volcanic, or other non-supporting biome records.

## Adapter implications

- Traveller and Cepheus should map government, law, and tech from these neutral fields instead of rolling them independently when realism mode is active.
- Starfinder should treat accord, technology, religion, and magic as adapter outputs derived downstream from this baseline. Magic is not present in normal generation and appears only in the Starfinder adapter payload.
- Starforged should treat settlement rank, frontier trouble, and faction pressure as oracle-facing transformations of this baseline.

## Human-audit boundary

Still not scientifically grounded enough to present as realism outputs:

- religion content,
- alignment,
- culture flavor text,
- language flavor text,
- magic,
- oracle prose,
- narrative faction names.

Those remain adapter or setting outputs and should continue to require explicit human audit before merge or release.
