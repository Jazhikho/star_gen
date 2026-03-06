# Religion Generator (Prototype)

**Status:** Prototype. Not yet folded into main.

**Purpose:** Procedural religion generator for worldbuilding. Multi-factor anthropological modeling: deity structure, cosmology, afterlife, religious specialists, gender roles, misfortune explanation, authority, rituals, sacred time/space, material culture, ethics, taboos, unique features, and religious landscape (hegemony, rivals, non-belief, dynamics).

**Design:** Logic is deterministic given `ReligionParams` and `Seed`. No dependency on StarGen population or galaxy; can be used standalone or later wired to population/civilisation data.

## Fixes applied (vs reference React versions)

- **Seeded RNG:** Same LCG as reference JS (1664525, 1013904223) for cross-port reproducibility.
- **Fisher–Yates shuffle:** All “pick several” uses deterministic shuffle or weighted sample without replacement—no `sort(() => rng() - 0.5)`.
- **Weighted sampling:** Rivals, non-belief forms, dynamics, rituals, sacred spaces, material culture, ethics, taboos use proper weighted draw (or weighted sample without replacement).
- **Bounded percentages:** Hegemony and non-belief % mapped via `SigmoidPct(score)` so output stays in [0, 100]; no unbounded additive + random.
- **No dead code:** No `deity.id == 'dualistic'` (no dualistic deity type); no `misfortune.name.includes('Purity')`—purity-sensitive logic uses `misfortune.Id == "spirit_offense"` or `"cosmic_imbalance"`.
- **WeightedChoice safety:** All-zero weights return first option.

## Usage

```csharp
var p = new ReligionParams
{
    Subsistence = "agricultural",
    SocialOrg = "chiefdom",
    Settlement = "permanent_village",
    Environment = "temperate_fertile",
    ExternalThreat = "moderate",
    Isolation = "trade_contact",
    PoliticalPower = "intertwined",
    WritingSystem = "none",
    PriorTraditions = "indigenous_only",
    GenderSystem = "patrilineal",
    KinshipStructure = "extended_clan",
    Seed = 12345,
};
ReligionResult r = ReligionGenerator.Generate(p);
```

## Theoretical sources

- **Swanson (1960)** – *Birth of the Gods*: social complexity and deity types.
- **Norenzayan (2013)** – *Big Gods*: moralizing deities and scale.
- **Douglas (1966)** – *Purity and Danger*: pollution and social structure.
- **Evans-Pritchard (1937)** – *Witchcraft Among the Azande*: misfortune in small societies.
- **Weber (1922)** – *Sociology of Religion*: priest/prophet typology.
- **Turner (1969)** – *Ritual Process*: liminality and communitas.
- **Sanday (1981)** – *Female Power and Male Dominance*: gender and subsistence.
- **Stark & Bainbridge (1987)** – *Theory of Religion*: religious economies and pluralism.

## Build

```bash
dotnet build
```

No Godot dependency; net8.0 library.
