# Traveller World Creation (SRD Reference)

Reference copy of the Traveller SRD world creation rules for use in StarGen.  
Source: [Traveller SRD – World Creation](https://www.traveller-srd.com/core-rules/world-creation/)

---

## Overview

- **World occurrence**: 50% chance per hex (roll 4–6 on 1d6). Referee may adjust: rift -2 DM, sparse -1 DM, dense +1 DM.
- **Starport**, **Bases**, **Gas Giants**, **Travel Zones**, **Polities**, **Communications Routes**, and **Trade Routes** are also defined at subsector level (see SRD for details).

The basic planetary profile is the **Universal World Profile (UWP)**: Size, Atmosphere, Hydrographics, Population, Government, Law Level, Technology Level, Starport, and Bases. These are generated with 2d6 and DMs.

---

## Size

Roll **2d6−2** for inhabitable worlds. Range 0–10 (10 = A).

| Digit | World Size | Surface Gravity (gs) |
| ----- | ---------- | -------------------- |
| 0     | 800 km     | Negligible           |
| 1     | 1,600 km   | 0.05                 |
| 2     | 3,200 km   | 0.15                 |
| 3     | 4,800 km   | 0.25                 |
| 4     | 6,400 km   | 0.35                 |
| 5     | 8,000 km   | 0.45                 |
| 6     | 9,600 km   | 0.7                  |
| 7     | 11,200 km  | 0.9                  |
| 8     | 12,800 km  | 1.0                  |
| 9     | 14,400 km  | 1.25                 |
| 10 (A)| 16,000 km  | 1.4                  |

- **Low gravity**: ≤0.75 g. **High gravity**: ≥1.25 g.

---

## Atmosphere

Roll **2d6−7 + Size**.

| Digit  | Atmosphere          | Pressure      | Survival gear      |
| ------ | ------------------- | ------------- | ------------------- |
| 0      | None                | 0.00          | Vacc suit           |
| 1      | Trace               | 0.001–0.09    | Vacc suit           |
| 2      | Very thin, tainted  | 0.1–0.42      | Respirator, filter  |
| 3      | Very thin           | 0.1–0.42      | Respirator          |
| 4      | Thin, tainted       | 0.43–0.7      | Filter              |
| 5      | Thin                | 0.43–0.7      | —                   |
| 6      | Standard            | 0.71–1.49     | —                   |
| 7      | Standard, tainted   | 0.71–1.49     | Filter              |
| 8      | Dense               | 1.5–2.49      | —                   |
| 9      | Dense, tainted      | 1.5–2.49      | Filter              |
| 10 (A) | Exotic              | Varies        | Air supply          |
| 11 (B) | Corrosive           | Varies        | Vacc suit           |
| 12 (C) | Insidious           | Varies        | Vacc suit           |
| 13 (D) | Dense, high         | 2.5+          | —                   |
| 14 (E) | Thin, low           | 0.5 or less   | —                   |
| 15 (F) | Unusual             | Varies        | Varies              |

---

## Hydrographics

Roll **2d6−7 + Size**, with DMs:

- **Size 0 or 1**: Hydrographics set to 0.
- **Atmosphere 0, 1, A, B or C**: −4.
- **Hot**: −2; **Roasting**: −6.
- If atmosphere is not D (or F that retains water), apply temperature DMs above.

| Digit  | Hydro %   | Description                    |
| ------ | --------- | ------------------------------ |
| 0      | 0–5%      | Desert                         |
| 1      | 6–15%     | Dry                            |
| 2      | 16–25%    | Few small seas                 |
| 3      | 26–35%    | Small seas and oceans          |
| 4      | 36–45%    | Wet                            |
| 5      | 46–55%    | Large oceans                   |
| 6      | 56–65%    | —                              |
| 7      | 66–75%    | Earth-like                     |
| 8      | 76–85%    | Water world                    |
| 9      | 86–95%    | Few small islands/archipelagos |
| 10 (A) | 96–100%   | Almost entirely water          |

---

## Population

Roll **2d6−2**. If population = 0, Government, Law Level, and Tech Level are 0.

| Digit  | Population            | Range        |
| ------ | --------------------- | ------------ |
| 0      | None                  | 0            |
| 1      | Few                   | 1+           |
| 2      | Hundreds              | 100+         |
| 3      | Thousands             | 1,000+       |
| 4      | Tens of thousands     | 10,000+      |
| 5      | Hundreds of thousands | 100,000+     |
| 6      | Millions              | 1,000,000+   |
| 7      | Tens of millions      | 10,000,000+  |
| 8      | Hundreds of millions  | 100,000,000+ |
| 9      | Billions              | 1e9+         |
| 10 (A) | Tens of billions      | 1e10+        |
| 11 (B) | Hundreds of billions  | 1e11+        |
| 12 (C) | Trillions             | 1e12+        |

---

## Government

Roll **2d6−7 + Population**.

| Digit  | Government                  |
| ------ | --------------------------- |
| 0      | None                        |
| 1      | Company/Corporation         |
| 2      | Participating Democracy     |
| 3      | Self-Perpetuating Oligarchy |
| 4      | Representative Democracy    |
| 5      | Feudal Technocracy          |
| 6      | Captive Government          |
| 7      | Balkanisation               |
| 8      | Civil Service Bureaucracy   |
| 9      | Impersonal Bureaucracy      |
| 10 (A) | Charismatic Dictator        |
| 11 (B) | Non-Charismatic Dictator    |
| 12 (C) | Charismatic Oligarchy       |
| 13 (D) | Religious Dictatorship      |

Factions: 1d3 (DM +1 if Gov 0 or 7, −1 if Gov 10+). Strength per faction: 2d6 (1–3 Obscure … 12 Overwhelming).

---

## Law Level

Roll **2d6−7 + Government**. Table defines illegal weapons, drugs, information, technology, travellers, psionics (see SRD for full table). 0 = no restrictions; 9+ = any weapons, all drugs, etc.

---

## Starport

Roll **2d6** (no DMs unless variant):

| Roll   | Starport class |
| ------ | -------------- |
| 2 or less | X          |
| 3–4   | E              |
| 5–6   | D              |
| 7–8   | C              |
| 9–10  | B              |
| 11+   | A              |

---

## Technology Level

Roll **1d6** plus DMs from Starport, Size, Atmosphere, Hydrographics, Population, Government (see SRD tables). Result is TL 0–15+.

---

## Bases

Types: Naval, Scout, Research, Consulate, Pirate. Presence and type determined by separate rolls (see SRD).

---

## Travel Codes

- **Amber**: Dangerous; travellers warned. Consider for Atmosphere 10+, Government 0/7/10, Law 0 or 9+.
- **Red**: Interdicted; travel forbidden. Referee discretion.

---

## Trade Routes

If two worlds within 4 parsecs have a Jump-1 or Jump-2 route and match:

- **Column 1**: Industrial or High Tech ↔ **Column 2**: Asteroid, Desert, Ice Capped, Non-Industrial  
- **Column 1**: High Population or Rich ↔ **Column 2**: Agricultural, Garden, Water World  

… mark a trade route between them.

---

## Trade Codes (Classification)

| Code | Size | Atmos | Hydro | Pop | Gov | Law | TL   |
| ---- | ---- | ----- | ----- | --- | --- | --- | ---- |
| Ag   | —    | 4–9   | 4–8   | 5–7 | —   | —   | —    |
| As   | 0    | 0     | 0     | —   | —   | —   | —    |
| Ba   | —    | —     | —     | 0   | 0   | 0   | —    |
| De   | —    | 2+    | 0     | —   | —   | —   | —    |
| Fl   | —    | 10+   | 1+    | —   | —   | —   | —    |
| Ga   | —    | 5+    | 4–9   | 4–8 | —   | —   | —    |
| Hi   | —    | —     | —     | 9+  | —   | —   | —    |
| Ht   | —    | —     | —     | —   | —   | —   | 12+  |
| Ic   | —    | 0–1   | 1+    | —   | —   | —   | —    |
| In   | —    | 0–2, 4, 7, 9 | — | 9+ | — | — | —    |
| Lo   | —    | —     | —     | 1–3 | —   | —   | —    |
| Lt   | —    | —     | —     | —   | —   | —   | 5−   |
| Na   | —    | 0–3   | 0–3   | 6+  | —   | —   | —    |
| Ni   | —    | —     | —     | 4–6 | —   | —   | —    |
| Po   | —    | 2–5   | 0–3   | —   | —   | —   | —    |
| Ri   | —    | 6, 8  | —     | 6–8 | —   | —   | —    |
| Wa   | —    | —     | 10    | —   | —   | —   | —    |
| Va   | —    | 0     | —     | —   | —   | —   | —    |

(Hi = High Population; Ht = High Technology.)

---

## Variations

### Space Opera

- Size 0–2: set Atmosphere to 0.
- Size 3–4: if Atm 0–2 → 0; 3–5 → 1; 6+ → A.
- Hydro DMs: Size 3–4 and Atm A → −6; Atm 0–1 → −6; Atm 2–3, B, C → −4.

### Hard Science

Use Space Opera modifiers plus Population DMs:

- Size 0–2: −1; Size A: −1.
- Atmosphere not 5, 6, 8: −1; Atmosphere 5, 6, 8: +1.
- Starport: roll **2d6−7 + Population** instead of 2d6 on Starport Table.

---

## StarGen ↔ Traveller UWP: Gap Analysis

This section analyses what is required to capture Traveller UWP codes from **current** StarGen generation (no code changes). It identifies what can be derived from existing data, what must be added or adjusted in a future effort, and how the existing **Traveller use case** effort (see `Docs/Roadmap.md`) fits in.

**Scope:** The Universal World Profile (UWP) has nine elements: **Size, Atmosphere, Hydrographics, Population, Government, Law Level, Technology Level, Starport, Bases.** Traveller applies the UWP to a **mainworld** per system (one designated world per star system). StarGen generates full systems (stars, planets, moons, asteroids) and optional population data per planet.

---

### 1. Size (UWP digit 0–9, A; or 0–9, A–E for gas giants)

| What Traveller needs | What StarGen has | Gap / notes |
|----------------------|------------------|--------------|
| Single digit from diameter (km) | `PhysicalProps.radius_m`; `TravellerSizeCode.diameter_km_to_code(diameter_km)` | **Capture:** Diameter = `2 * radius_m / 1000.0`; pass to existing `TravellerSizeCode`. No code change. |

**Adjustments to consider (later):**

- **Inhabitable mainworld:** Traveller SRD size table is 0–10 (A) only; B/C/D/E are extensions. For “mainworld” UWP, size is typically 0–A. StarGen’s `TravellerSizeCode` already uses 0–9, A–E (B/C = large solid, D/E = gas giant). Using it for mainworlds is fine; for gas giants, Traveller often omits full UWP or uses a different convention.
- **Diameter bands:** SRD lists exact diameters (0=800 km, 1=1600, 2=3200, … A=16000). StarGen’s bounds are contiguous ranges (e.g. 1 = 800–2400 km). That yields a consistent, reversible mapping; only if strict SRD “canonical sizes” are required would the bounds need to align exactly with those points.

**Verdict:** Size can be **captured from current generation** using existing `TravellerSizeCode` and `PhysicalProps.radius_m`.

---

### 2. Atmosphere (UWP digit 0–15 / 0–9, A–F)

| What Traveller needs | What StarGen has | Gap / notes |
|----------------------|------------------|--------------|
| Digit from pressure + composition (pressure bands, taint, exotic/corrosive) | `AtmosphereProps.surface_pressure_pa`, `composition`, optional `SurfaceProps` (temperature) | **Capture:** Pressure in Pa → convert to Earth atm (÷ 101325) and map to Traveller pressure bands. **Gap:** No explicit “taint” or “exotic/corrosive” flag; these must be **derived** from composition. |

**What’s needed for a full mapping (no code change):**

- **Pressure → digit:** Table from pressure (atm or Pa) to Traveller atmosphere digit (e.g. 0 = none, 1 = trace 0.001–0.09, …, 6 = standard 0.71–1.49, …). StarGen stores continuous pressure; a pure function (e.g. in a doc or later mapper) can bucket it.
- **Composition → taint / exotic / corrosive:** Traveller digits 2, 4, 7, 9 add “Tainted”; 10 (A) = Exotic; 11–12 (B–C) = Corrosive/Insidious. StarGen has `composition` (gas name → fraction). Rules: e.g. dominant CO₂ or trace toxins → tainted; no breathable mix (e.g. no O₂ in habitable range) → exotic; presence of highly reactive/corrosive gases → B or C. This requires a **mapping layer** (table or small module) that interprets `composition`; the data is there, the semantics are not yet encoded.
- **Temperature:** Hydrographics DMs use Hot/Roasting. StarGen has `SurfaceProps.temperature_k`; can be used when implementing hydro and atmosphere DMs for consistency.

**Verdict:** Atmosphere digit is **partially capturable**: pressure → digit is straightforward; taint/exotic/corrosive need a **derived mapping** from existing `composition` (and optionally temperature). No new stored fields required; a mapper or table is.

---

### 3. Hydrographics (UWP digit 0–10 / A)

| What Traveller needs | What StarGen has | Gap / notes |
|----------------------|------------------|--------------|
| Digit from hydrographic % (0–5%, 6–15%, … 96–100%) | `HydrosphereProps.ocean_coverage` (0–1); optionally `SurfaceProps.hydrosphere`, `cryosphere` | **Capture:** `ocean_coverage * 100` → bucket into Traveller bands (0=0–5%, 1=6–15%, … A=96–100%). No code change. |

**Caveats:**

- Traveller applies DMs (e.g. Size 0–1 → 0; Atm 0,1,A,B,C → −4; Hot/Roasting). Those are for **generation**. When **mapping from existing** StarGen data, we only need the final hydro %; StarGen’s generator doesn’t use Traveller DMs, so the produced `ocean_coverage` is already “outcome” and can be bucketed directly.
- Ice vs liquid: StarGen has `ice_coverage` and `cryosphere`. Traveller hydro is “water coverage”; if ice is dominant, one could treat frozen water as “no liquid” and reduce effective hydro for the digit, or keep ocean_coverage and document that ice worlds may map to low hydro. Policy choice, no new data.

**Verdict:** Hydrographics digit can be **captured from current generation** via `ocean_coverage` (and optional ice policy). A simple mapping table or function suffices.

---

### 4. Population (UWP digit 0–12 / 0–9, A–C)

| What Traveller needs | What StarGen has | Gap / notes |
|----------------------|------------------|--------------|
| Digit from population count (None, Few, Hundreds, … Trillions) | `PlanetPopulationData.get_total_population()` (int); natives + colonies | **Capture:** Map count to Traveller digit (0=0, 1=1–999, 2=100–999, 3=1000–9999, … 9=1e9–9.99e9, A=1e10–9.99e10, B=1e11–…, C=1e12+). No code change. |

**Caveats:**

- Population is **optional** in StarGen (population framework may not be run for a system). If no population data: treat as uninhabited (digit 0); Government, Law, Tech can be set to 0 per SRD.
- Mainworld: Traveller population is per mainworld. StarGen has per-planet population; the body chosen as “mainworld” (see below) determines which `PlanetPopulationData` is used.

**Verdict:** Population digit can be **captured from current generation** when population data exists; otherwise use 0. A count→digit table or function is enough.

---

### 5. Government (UWP digit 0–13 / 0–9, A–D)

| What Traveller needs | What StarGen has | Gap / notes |
|----------------------|------------------|--------------|
| Single digit (None, Company, Participating Democracy, … Religious Dictatorship) | `GovernmentType.Regime` (many values); `Government` on natives/colonies; dominant population’s government | **Gap:** StarGen’s regime set is **different** from Traveller’s (e.g. Tribal, Chiefdom, Feudal, Constitutional, Corporate, Theocracy). Traveller has 14 government types (0–13/D). |

**Options (no code change):**

- **Mapping table:** Define Regime → Traveller government digit (e.g. Tribal/Chiefdom → 0 or 1, Corporate → 1, Feudal → 5, Constitutional/Mass Democracy → 4, Theocracy → 13, etc.). Multiple StarGen regimes can map to the same Traveller digit; closest fit is enough for export.
- **Roll at export:** Ignore stored government and roll 2d6−7+Population for “Traveller government” when producing UWP. Then we don’t use StarGen’s government for UWP; less consistent with world state.

**Verdict:** Government digit is **capturable** by adding a **Regime → Traveller government** mapping table (or small mapper). No change to existing generation; optional population data provides the dominant regime.

---

### 6. Law Level (UWP digit 0–9+)

| What Traveller needs | What StarGen has | Gap / notes |
|----------------------|------------------|--------------|
| Digit (0 = no restrictions, 9+ = very restrictive) | **None.** No law level in domain or population. | **Gap:** Law level is not modelled. |

**Options:**

- **Derive at export:** Roll 2d6−7+Government (Traveller formula) when building UWP, using the Traveller government digit from step 5. No stored field; UWP becomes a snapshot that may not match “canon” if we later add law.
- **Add later:** New field (e.g. on planet or population) for law level; generation could use 2d6−7+Government or a different rule. Out of scope for “current generation” capture.

**Verdict:** Law level **cannot be captured** from current data. It can be **synthesised at export** by rolling from Traveller government digit, or left as a future extension.

---

### 7. Technology Level (UWP TL 0–15+)

| What Traveller needs | What StarGen has | Gap / notes |
|----------------------|------------------|--------------|
| Numeric TL (0–15+) | `TechnologyLevel.Level` (enum: Stone Age … Advanced); `PlanetPopulationData.get_highest_tech_level()` | **Gap:** StarGen has 12 named levels; Traveller has numeric TL 0–15+. Need Level → TL mapping. |

**What’s needed:**

- **Mapping table:** e.g. Stone Age → 0, Bronze → 1, Iron → 2, Classical/Medieval → 3, Renaissance → 4, Industrial → 5, Atomic → 6, Information → 8, Spacefaring → 9, Interstellar → 12, Advanced → 15. Exact mapping is a design choice; SRD gives TL meaning (e.g. jump drive at TL 12). No code change to generation; only a mapper when building UWP.

**Verdict:** TL can be **captured from current generation** via a **TechnologyLevel.Level → Traveller TL** mapping. Use `get_highest_tech_level()` for the mainworld; if uninhabited, TL 0.

---

### 8. Starport (UWP letter X, E, D, C, B, A)

| What Traveller needs | What StarGen has | Gap / notes |
|----------------------|------------------|--------------|
| Starport class | **Space stations and outposts** per system/body | **Capture:** Map stations to starport class via services and class. |

**Space-station mapping logic (recommended):**

StarGen models **space stations** (`SpaceStation`, `Outpost`) with:

- **StationClass** (U, O, B, A, S): U/O = small (&lt;10k), B = base (up to 100k), A = anchor (up to 1M), S = super.
- **StationPurpose**: UTILITY (refuel, repair), TRADE, MILITARY, SCIENCE, MINING, etc.
- **StationService**: REFUEL, REPAIR, TRADE, SHIPYARD, MEDICAL, CUSTOMS, etc.
- **Location:** `orbiting_body_id`, `system_id` — so we can associate stations with the mainworld or system.

**Mapping rules (for mainworld or system):**

1. **Collect** all stations in the system (or orbiting the mainworld only, depending on policy). Filter to operational only (`is_operational == true`).
2. **Best starport wins:** Traveller has one starport per mainworld. Take the “best” station that can serve as the mainworld starport (e.g. orbital around mainworld, or system-level if no orbital). If multiple, pick by class/service quality.
3. **Class → starport letter:**
   - **No station** (or no orbital at mainworld): **X** (none) or **E** (frontier; roll or default E if population &gt; 0).
   - **U-class** (utility, &lt;10k), basic REFUEL/REPAIR/TRADE: **E** or **D** (poor).
   - **O-class** (outpost), purpose-driven: **D** (poor) or **E**.
   - **B-class** (base, up to 100k), full basic services: **C** (routine). If SHIPYARD present → **B**.
   - **A-class** (anchor, up to 1M), major hub: **B** (good). If SHIPYARD + major services → **A**.
   - **S-class** (super, city-scale): **A** (excellent) or **B**.
4. **Services as tie-breaker or modifier:** REFUEL + REPAIR = minimum for D; + TRADE + LODGING = solid C; SHIPYARD = B capability; SHIPYARD + BANKING + multiple advanced = A.
5. **Population / TL:** If no stations, fall back to Hard Science variant: 2d6−7+Population → starport; or roll 2d6 for X/E/D/C/B/A.

**Data sources:** `SpaceStation.station_class`, `services`, `primary_purpose`, `orbiting_body_id`, `system_id`, `is_operational`. For outposts (`Outpost`), similar role: treat as small facility → E or D when used as sole “port”.

**Verdict:** Starport **can be captured** from current generation by **mapping space stations** (and outposts) to Traveller starport class via the logic above. If no station exists at the mainworld/system, use **X** or **E**, or a roll/derived value from population.

---

### 9. Bases (presence and type)

| What Traveller needs | What StarGen has | Gap / notes |
|----------------------|------------------|--------------|
| Naval, Scout, Research, Consulate, Pirate (and presence) | **None.** No bases in domain. Stations/outposts exist but are not Traveller base types. | **Gap:** Not modelled. |

**Options:**

- **Omit or roll at export:** Leave bases blank, or roll per SRD when building UWP. No link to StarGen stations.
- **Add later:** Base types as separate concept; optional mapping from “space station” to Scout/Research etc. Out of scope for current capture.

**Verdict:** Bases **cannot be captured** from current data. Can be **omitted or rolled at export**.

---

### 10. Mainworld selection

Traveller UWP is per **mainworld** (one per system). StarGen has multiple bodies and optional population per planet.

**Possible rules (policy, no code change):**

- **By population:** Mainworld = planet (or moon) with highest `get_total_population()`; if tie, pick one (e.g. first by orbit or ID).
- **By habitability:** Mainworld = body with best habitability score (e.g. `PlanetProfile.habitability_score`) among those with a profile.
- **By orbit:** Mainworld = first terrestrial body in habitable zone (or innermost habitable), with or without population.
- **Hybrid:** Prefer inhabited; if none, choose by habitability or orbit.

Once mainworld is chosen, UWP is built from that body’s `CelestialBody` + (if present) its `PlanetPopulationData`.

---

### 11. Trade codes and travel zones

- **Trade codes (Ag, As, De, …):** Fully **derived** from UWP digits (Size, Atmosphere, Hydro, Pop, Gov, Law, TL). Once UWP is available, a single table or function can compute trade codes; no extra StarGen data.
- **Travel zones (Amber/Red):** SRD: consider Amber for Atmosphere 10+, Government 0/7/10, Law 0 or 9+; Red at referee discretion. **Derived** from UWP (and optionally custom rules). No new data.

---

### 12. Summary: what’s needed to capture Traveller codes from current generation

| UWP element    | From current generation? | What’s needed |
|----------------|--------------------------|----------------|
| **Size**       | Yes                      | Use `TravellerSizeCode` + `PhysicalProps.radius_m` (diameter). |
| **Atmosphere** | Partially                | Pressure → digit table. Composition → taint/exotic/corrosive rules (mapping layer). |
| **Hydrographics** | Yes                   | Bucket `HydrosphereProps.ocean_coverage * 100` into Traveller bands. |
| **Population** | Yes (if pop data)        | Bucket `get_total_population()` into Traveller digit; else 0. |
| **Government** | Yes (with mapping)       | Table: `GovernmentType.Regime` (and dominant population) → Traveller gov digit. |
| **Law Level**  | No                       | Roll 2d6−7+Government at export, or add field later. |
| **Tech Level** | Yes (with mapping)       | Table: `TechnologyLevel.Level` → Traveller TL 0–15. |
| **Starport**   | Yes (via stations)       | Map `SpaceStation` / `Outpost` (class, services, purpose) to starport letter; else X/E or roll. |
| **Bases**      | No                       | Omit or roll at export. |

**Adjustments that may be needed later (not for “current generation” capture):**

- **Size:** Optionally align `TravellerSizeCode` diameter bounds exactly with SRD canonical sizes (0=800, 1=1600, … A=16000) if strict compatibility is required; current ranges are already consistent for mapping.
- **Atmosphere:** Add optional “taint”/“exotic”/“corrosive” flags on `AtmosphereProps` if we want them stored instead of derived from composition.
- **Government:** If we want Traveller government to be editable or generated per Traveller rules, we could add a Traveller-government enum and store it; for now, mapping from Regime is enough.
- **Law Level / Starport / Bases:** New fields or generators only if we want them as first-class world properties rather than export-only rolls.

---

## Traveller vs StarGen: Generation-time assumptions and constraints

This section analyses **assumptions and constraints at galaxy (and system) generation time**: what Traveller assumes vs what StarGen does by default, and what could be **tweaked** (parameters, constraints, or optional modes) to align output with Traveller expectations without replacing core generation.

**Goal:** Identify knobs and policies so that “Traveller-aligned” galaxy/system generation can be offered as an option (e.g. density, world occurrence, gas giant presence, mainworld bias) while keeping default behaviour unchanged.

---

### 1. World occurrence and hex/subsector density

| Traveller assumption | StarGen default | Tweak at generation time |
|---------------------|-----------------|---------------------------|
| One roll per **hex**; 50% chance of a “world” (system) per hex (4–6 on 1d6). DMs: rift −2, sparse −1, dense +1. | **Subsector**: Poisson-sampled star count from density model. ~0.004 systems/pc³ → ~4 systems per 10×10×10 pc subsector. No per-hex roll. | **Option A:** Expose a “Traveller density mode”: interpret subsector (or hex grid) and roll 1d6 per hex with DM from region (rift/sparse/dense). Override or cap Poisson count so expected “worlds per subsector” matches Traveller (e.g. ~50% of hexes have a world). **Option B:** Scale `reference_density` (or effective rate) so that **expected systems per subsector** ≈ Traveller expectation (e.g. 50% of N hexes). **Option C:** Keep Poisson; document that StarGen’s density is “realistic” and Traveller’s 50% is a game convention; UWP export still works per system. |

**Recommendation:** Document the difference. If “Traveller subsector” layout is desired, add an optional **Traveller density preset** (or DM) that adjusts the Poisson mean or uses a per-hex roll so system count per subsector matches the SRD.

---

### 2. One mainworld per system

| Traveller assumption | StarGen default | Tweak at generation time |
|----------------------|-----------------|---------------------------|
| Each system has **one mainworld**; UWP is for that world. Other bodies may exist but are not the “main” world. | Full system generation: multiple planets, moons, asteroids. No designated “mainworld.” | **Mainworld selection** is a **post-generation policy** (see gap analysis): e.g. by population, habitability, or orbit. No change to generation. Optionally: add a **constraint** “ensure at least one habitable (or habitable-zone) body per system” so mainworld selection always has a sensible candidate in Traveller-focused runs. |

**Recommendation:** Keep generation as-is. Use mainworld selection at export/display time. Optional **system constraint** (e.g. “prefer systems with at least one size 5–8, atmosphere 5–8, hydro 4+ body”) can be a Traveller preset for system or sector generation if we add constraint layers.

---

### 3. Gas giant presence

| Traveller assumption | StarGen default | Tweak at generation time |
|----------------------|-----------------|---------------------------|
| Roll **10+ on 2d6** for “at least one gas giant” in the system; otherwise none. Drives refuelling (fuel scoops). | **Orbit zone + mass:** Planet count and types by zone (hot/habitable/cold); cold zone favours gas giants. No explicit “at least one gas giant” roll. | **Option A:** After generating planets, **retro-check**: if no gas giant and Traveller mode, optionally **force one** gas giant in outer zone (e.g. add one or reseed). **Option B:** In **system spec or sector preset**, add a constraint “require at least one gas giant” and let the generator retry or inject. **Option C:** Bias **planet type weights** in cold zone (or system-wide) so gas giants are more likely when “Traveller gas giant” flag is set. |

**Recommendation:** Document that StarGen does not guarantee a gas giant. For Traveller play, add an optional **constraint or post-pass**: “ensure at least one gas giant per system” (e.g. in system generator or as a Traveller preset in constraints).

---

### 4. Planet count and “world” count

| Traveller assumption | StarGen default | Tweak at generation time |
|----------------------|-----------------|---------------------------|
| One UWP per system (mainworld). Traveller does not generate full system orbits; it generates one world profile per hex that has a world. | Variable planets (and moons) per system; full orbital layout. | No need to reduce planet count. Mainworld selection picks one body for UWP. If “Traveller-only” export is desired (no full system detail), that’s an export/view choice, not a generation change. |

**Recommendation:** No change to planet count. Mainworld + UWP export suffices.

---

### 5. Size / atmosphere / hydro distribution (Traveller tables vs physical generation)

| Traveller assumption | StarGen default | Tweak at generation time |
|----------------------|----------------|---------------------------|
| **Size:** 2d6−2 (0–10). **Atmosphere:** 2d6−7+Size. **Hydro:** 2d6−7+Size + DMs. So distribution is **table-driven**, not physics-driven. | **Physics-driven:** mass/density, orbit zone, equilibrium temp, retention, etc. Outcomes can differ from Traveller distributions (e.g. fewer size 5–8 worlds, different atmosphere spread). | **Option A (current):** Keep physics; **map** results to UWP (gap analysis). **Option B:** Add **Traveller world gen mode**: for mainworld (or a designated body), generate **only** UWP digits (2d6 rolls + DMs) and then **backfill** approximate physical values (diameter from size code, pressure from atm code, etc.) so the body “matches” UWP. **Option C:** **Constrain** physical generation: e.g. “mainworld size 4–9”, “atmosphere 5–9”, so that generated worlds fall in Traveller-friendly bands; then map as today. |

**Recommendation:** Default: keep physics-based generation and map to UWP. Optional **Traveller presets** could add **constraints** (e.g. size code range, atmosphere range) so that generated mainworlds are more often “Traveller-like” without switching to table-only generation.

---

### 6. Population, government, law, starport (Traveller rolls)

| Traveller assumption | StarGen default | Tweak at generation time |
|----------------------|-----------------|---------------------------|
| **Population:** 2d6−2. **Government:** 2d6−7+Pop. **Law:** 2d6−7+Gov. **Starport:** 2d6 (or 2d6−7+Pop in Hard Science). **Bases:** Separate rolls. | **Population framework** (optional): natives, colonies, regime, tech level, stations. No law level; no Traveller starport digit; stations exist and can be **mapped** to starport. | **Mapping** (see gap analysis): population count → digit; regime → government digit; stations → starport; law/starport/bases rolled or derived at export. **Generation-time tweak:** If Traveller mode, ensure **population framework is run** for systems (or mainworlds) so that government, tech, and stations exist for UWP and starport mapping. No change to roll formulae; use our data and map. |

**Recommendation:** When generating for Traveller export, **enable population (and stations)** for the relevant systems so that UWP and starport can be fully derived. No need to replace population/station logic with Traveller rolls.

---

### 7. Density model and “rift / sparse / dense” sectors

| Traveller assumption | StarGen default | Tweak at generation time |
|----------------------|-----------------|---------------------------|
| Referee may set **sector DMs**: rift −2, sparse −1, dense +1 on the 1d6 “world present” roll. So **region** affects how many hexes have a world. | **Density model** (e.g. spiral, elliptical) + **reference_density**. Poisson mean scales with local density. No explicit “rift/sparse/dense” labels. | **Option A:** Define **sector or region types** (rift, sparse, normal, dense) and map them to a **multiplier** on `reference_density` or on the Poisson mean (e.g. rift = 0.25×, sparse = 0.5×, dense = 1.5×). **Option B:** When generating a sector, accept an optional **Traveller density DM** (−2, −1, 0, +1) and adjust the expected system count per subsector accordingly. **Option C:** Paint regions on the galaxy (rift/sparse/dense) in the density model or in a separate “Traveller region” layer and use that when sampling system count. |

**Recommendation:** For Traveller-aligned galaxy generation, support an optional **density modifier** or **region preset** (rift/sparse/dense) that scales the effective system generation rate per subsector (or per hex grid), so that sector-level assumptions match the SRD.

---

### 8. Summary: generation-time knobs for Traveller alignment

| Area | Default | Optional tweak (no code change required for analysis) |
|------|---------|--------------------------------------------------------|
| **World occurrence** | Poisson, ~4/subsector at solar density | Traveller density mode: per-hex 50% roll with DMs, or scale Poisson mean to match. |
| **Mainworld** | None; pick at export | Policy: by population, habitability, or orbit. Optional constraint: “at least one habitable-zone body.” |
| **Gas giant** | By zone; not guaranteed | Constraint or post-pass: “ensure at least one gas giant per system” when Traveller mode. |
| **Planet count** | No change | Keep as-is; mainworld selection only. |
| **Size/atmo/hydro** | Physics-driven | Keep and map to UWP. Optional: constraints (e.g. mainworld size 4–9) or separate “Traveller table” mainworld gen mode. |
| **Population / gov / starport** | Optional population + stations | Run population (and stations) when building for Traveller; map to UWP and starport. |
| **Density / region** | Single reference density, model-based | Optional rift/sparse/dense modifier or region preset scaling system count. |

Implementing these tweaks would be **effort work** (new presets, constraints, or parameters in galaxy/system specs). This analysis only identifies **what** to tweak and **where** (galaxy vs system vs export) so that Traveller-aligned generation can be designed without changing core physics or breaking default behaviour.

---

**Effort alignment (Roadmap):** The existing **Traveller alignment** work (size code layer, `TravellerSizeCode`) covers the Size digit. The **Traveller use case** effort can add a **UWP export/mapper** that: (1) selects mainworld, (2) derives or maps the nine digits from existing body + population data as above, (3) optionally rolls Law, Starport, Bases at export, and (4) derives trade codes and travel zones. No change to current generation logic is required for a first version of Traveller code capture.

---

*This document is a reference copy of the Traveller SRD world creation rules plus a gap analysis for StarGen. "Traveller" and the Traveller logo are trademarks of Far Future Enterprises, Inc. Used for game design reference only.*
