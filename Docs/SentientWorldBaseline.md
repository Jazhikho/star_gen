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
- Comin and Hobijn (2010) supports technology diffusion and adoption as distinct from invention.

Presentation logic:

- The inspector shows the highest active level because that is the most useful adapter-facing summary for port quality, offworld trade, and compatibility exports.

### Dominant Regime

Why it is shown:

- Traveller-family government codes and several settlement-focused RPG outputs still need a polity-facing baseline.
- The inspector shows the dominant active regime as a summary of which institutional form currently governs the largest share of the population.

Grounding:

- Blanton and Fargher (2008) supports bargaining structure and public-goods dependence as major regime-shaping forces.
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

- Comin and Hobijn (2010) supports technology diffusion and adoption through connection.
- Trade and offworld connection also materially affect logistics, political bargaining, and settlement concentration.

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

- Blanton and Fargher (2008) supports bargaining and public-goods dependence as central governance variables.

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

- Legal reach is shown separately from centralization because a world can claim centralized law without actually enforcing it broadly.
- Chowdhury (2022) is the main anchor for capacity-limited enforcement.

Why it is shown:

- This is the clearer realism-layer answer to "how much of the world is actually under effective law?"

### Restriction Pressure

Grounding:

- Restriction should depend on coercion, threat, and inclusiveness rather than being treated as identical to legal presence.

Why it is shown:

- It gives the adapters a more honest precursor for Traveller-style law levels than a single random digit would.

### Cultural Accumulation

Grounding:

- Henrich et al. (2016) supports cumulative culture as a retention-and-transmission problem.

Why it is shown:

- This explains part of the difference between old isolated worlds and old connected worlds, and between raw age and actually retained complexity.

### Tech Adoption

Grounding:

- Comin and Hobijn (2010) supports diffusion and adoption as a major part of technological reality.

Why it is shown:

- It helps future adapters distinguish imported or network-supported technology from purely local developmental depth.

### Factional Fragmentation

Grounding:

- Fragmentation responds to multiple active population blocs, coexistence pressure, conflict, weak state reach, and fragmented settlement geography.

Why it is shown:

- Traveller factions, Starfinder settlement complications, and Starforged community tension all need a defensible precursor.

### Religious Centralization

Grounding:

- This is intentionally narrow: it models institutional centralization potential, not doctrine or belief content.
- It is the one baseline field here that must be treated as provisional until a later dedicated human-audited religion pass is complete.

Why it is shown:

- Some compatibility systems want religion-facing structural context, but the realism layer must stop at institutional concentration rather than inventing belief claims.

## Adapter implications

- Traveller and Cepheus should map government, law, and tech from these neutral fields instead of rolling them independently when realism mode is active.
- Starfinder should treat accord, technology, settlement complexity, and some challenge framing as downstream transformations of this baseline.
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
