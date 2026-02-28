## Traveller UWP planet size codes (simplified for StarGen).
## Maps diameter in km to a single alphanumeric size digit (0–9, A–E).
## Code 0 = very small / major asteroid; D/E = small/large gas giant.
class_name TravellerSizeCode
extends RefCounted


## Diameter bounds in km for numeric codes 0–9. Code 0: &lt;800; 1: 800–2399; … 9: 13600–15199.
const DIAM_KM_0_MAX: float = 800.0
const DIAM_KM_1_MIN: float = 800.0
const DIAM_KM_1_MAX: float = 2400.0
const DIAM_KM_2_MIN: float = 2400.0
const DIAM_KM_2_MAX: float = 4000.0
const DIAM_KM_3_MIN: float = 4000.0
const DIAM_KM_3_MAX: float = 5600.0
const DIAM_KM_4_MIN: float = 5600.0
const DIAM_KM_4_MAX: float = 7200.0
const DIAM_KM_5_MIN: float = 7200.0
const DIAM_KM_5_MAX: float = 8800.0
const DIAM_KM_6_MIN: float = 8800.0
const DIAM_KM_6_MAX: float = 10400.0
const DIAM_KM_7_MIN: float = 10400.0
const DIAM_KM_7_MAX: float = 12000.0
const DIAM_KM_8_MIN: float = 12000.0
const DIAM_KM_8_MAX: float = 13600.0
const DIAM_KM_9_MIN: float = 13600.0
const DIAM_KM_9_MAX: float = 15200.0
## Huge solid: A 15200–16799, B 16800–18399, C 18400–&lt;40000.
const DIAM_KM_A_MIN: float = 15200.0
const DIAM_KM_A_MAX: float = 16800.0
const DIAM_KM_B_MIN: float = 16800.0
const DIAM_KM_B_MAX: float = 18400.0
const DIAM_KM_C_MIN: float = 18400.0
const DIAM_KM_C_MAX: float = 40000.0
## Gas giants: D 40000–120000, E 120000+.
const DIAM_KM_D_MIN: float = 40000.0
const DIAM_KM_D_MAX: float = 120000.0
const DIAM_KM_E_MIN: float = 120000.0


## Returns the Traveller UWP size code for a given diameter in km.
## @param diameter_km: Diameter in kilometres.
## @return: int 0–9 for numeric codes, or String "A", "B", "C", "D", "E" for alphanumeric. Use to_string_uwp() for a single UWP digit.
static func diameter_km_to_code(diameter_km: float) -> Variant:
	if diameter_km < 0.0:
		return 0
	if diameter_km < DIAM_KM_0_MAX:
		return 0
	if diameter_km < DIAM_KM_1_MAX:
		return 1
	if diameter_km < DIAM_KM_2_MAX:
		return 2
	if diameter_km < DIAM_KM_3_MAX:
		return 3
	if diameter_km < DIAM_KM_4_MAX:
		return 4
	if diameter_km < DIAM_KM_5_MAX:
		return 5
	if diameter_km < DIAM_KM_6_MAX:
		return 6
	if diameter_km < DIAM_KM_7_MAX:
		return 7
	if diameter_km < DIAM_KM_8_MAX:
		return 8
	if diameter_km < DIAM_KM_9_MAX:
		return 9
	if diameter_km < DIAM_KM_A_MAX:
		return "A"
	if diameter_km < DIAM_KM_B_MAX:
		return "B"
	if diameter_km < DIAM_KM_C_MAX:
		return "C"
	if diameter_km < DIAM_KM_D_MAX:
		return "D"
	return "E"


## Returns the diameter range in km for a given Traveller size code.
## @param code: int 0–9 or String "A", "B", "C", "D", "E".
## @return: Dictionary with "min" and "max" in km, or empty dict if code invalid. E has max = -1 (no upper bound).
static func code_to_diameter_range(code: Variant) -> Dictionary:
	if code is int:
		var i: int = code as int
		if i == 0:
			return {"min": 0.0, "max": DIAM_KM_0_MAX}
		if i == 1:
			return {"min": DIAM_KM_1_MIN, "max": DIAM_KM_1_MAX}
		if i == 2:
			return {"min": DIAM_KM_2_MIN, "max": DIAM_KM_2_MAX}
		if i == 3:
			return {"min": DIAM_KM_3_MIN, "max": DIAM_KM_3_MAX}
		if i == 4:
			return {"min": DIAM_KM_4_MIN, "max": DIAM_KM_4_MAX}
		if i == 5:
			return {"min": DIAM_KM_5_MIN, "max": DIAM_KM_5_MAX}
		if i == 6:
			return {"min": DIAM_KM_6_MIN, "max": DIAM_KM_6_MAX}
		if i == 7:
			return {"min": DIAM_KM_7_MIN, "max": DIAM_KM_7_MAX}
		if i == 8:
			return {"min": DIAM_KM_8_MIN, "max": DIAM_KM_8_MAX}
		if i == 9:
			return {"min": DIAM_KM_9_MIN, "max": DIAM_KM_9_MAX}
		return {}
	if code is String:
		var s: String = code as String
		if s == "A":
			return {"min": DIAM_KM_A_MIN, "max": DIAM_KM_A_MAX}
		if s == "B":
			return {"min": DIAM_KM_B_MIN, "max": DIAM_KM_B_MAX}
		if s == "C":
			return {"min": DIAM_KM_C_MIN, "max": DIAM_KM_C_MAX}
		if s == "D":
			return {"min": DIAM_KM_D_MIN, "max": DIAM_KM_D_MAX}
		if s == "E":
			return {"min": DIAM_KM_E_MIN, "max": - 1.0}
		return {}
	return {}


## Returns the code as a single character for UWP (digit or letter).
## @param code: Result of diameter_km_to_code (int 0–9 or String "A"–"E").
## @return: String of length 1 for the UWP size digit.
static func to_string_uwp(code: Variant) -> String:
	if code is int:
		return str(code as int)
	if code is String:
		return code as String
	return "?"
