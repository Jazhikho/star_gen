## Reference ranges for generator distribution tests, from astrophysical literature.
## Sources: local 20 pc census (IMF/spectral types), exoplanet demographics (Kepler/TESS/RV),
## Milky Way structure and solar-neighborhood density. Used by calibrated distribution tests.
class_name ScientificBenchmarks
extends RefCounted


## M dwarfs: ~70–80% of stars by number in the solar neighborhood (20 pc census).
const M_DWARF_FRACTION_MIN: float = 0.60
const M_DWARF_FRACTION_MAX: float = 0.90

## G + K combined: significant but sub-dominant.
const GK_FRACTION_MIN: float = 0.10
const GK_FRACTION_MAX: float = 0.35

## O, B, A, F combined: only a few percent of stars.
const OBAF_FRACTION_MAX: float = 0.12

## Hot Jupiters: well under a few percent of all planets (exoplanet surveys).
const HOT_JUPITER_FRACTION_MAX: float = 0.05

## Solar neighborhood: ~0.004–0.01 systems/pc³ (SubSectorGenerator uses ~4 per 1000 pc³).
## No single constant here; tests use SubSectorGenerator expectations directly.
