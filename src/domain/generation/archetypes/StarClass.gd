## Stellar spectral classifications for main sequence stars.
## Uses Harvard spectral classification (O, B, A, F, G, K, M).
class_name StarClass
extends RefCounted


## Main spectral class enumeration (hottest to coolest).
enum SpectralClass {
	O, ## Blue, very hot (>30,000 K)
	B, ## Blue-white (10,000 - 30,000 K)
	A, ## White (7,500 - 10,000 K)
	F, ## Yellow-white (6,000 - 7,500 K)
	G, ## Yellow, Sun-like (5,200 - 6,000 K)
	K, ## Orange (3,700 - 5,200 K)
	M, ## Red, cool (<3,700 K)
}


## Returns the string letter for a spectral class.
## @param spectral_class: The class enum value.
## @return: Single letter designation.
static func to_letter(spectral_class: SpectralClass) -> String:
	match spectral_class:
		SpectralClass.O:
			return "O"
		SpectralClass.B:
			return "B"
		SpectralClass.A:
			return "A"
		SpectralClass.F:
			return "F"
		SpectralClass.G:
			return "G"
		SpectralClass.K:
			return "K"
		SpectralClass.M:
			return "M"
		_:
			return "?"


## Parses a letter to a spectral class.
## @param letter: The letter to parse (case insensitive).
## @return: The corresponding class, or -1 if invalid.
static func from_letter(letter: String) -> int:
	match letter.to_upper():
		"O":
			return SpectralClass.O
		"B":
			return SpectralClass.B
		"A":
			return SpectralClass.A
		"F":
			return SpectralClass.F
		"G":
			return SpectralClass.G
		"K":
			return SpectralClass.K
		"M":
			return SpectralClass.M
		_:
			return -1


## Builds a full spectral classification string.
## @param spectral_class: The main spectral class.
## @param subclass: The numeric subclass (0-9).
## @param luminosity_class: The luminosity class (default "V" for main sequence).
## @return: Full spectral string like "G2V".
static func build_spectral_string(
	spectral_class: SpectralClass,
	subclass: int,
	luminosity_class: String = "V"
) -> String:
	var clamped_subclass: int = clampi(subclass, 0, 9)
	return to_letter(spectral_class) + str(clamped_subclass) + luminosity_class


## Parses a spectral string to extract components.
## @param spectral_str: The spectral string (e.g., "G2V").
## @return: Dictionary with "class", "subclass", "luminosity_class" or empty if invalid.
static func parse_spectral_string(spectral_str: String) -> Dictionary:
	if spectral_str.length() < 2:
		return {}
	
	var letter: String = spectral_str.substr(0, 1)
	var spectral_class: int = from_letter(letter)
	if spectral_class < 0:
		return {}
	
	var subclass_char: String = spectral_str.substr(1, 1)
	if not subclass_char.is_valid_int():
		return {}
	var subclass: int = subclass_char.to_int()
	
	var luminosity_class: String = "V"
	if spectral_str.length() > 2:
		luminosity_class = spectral_str.substr(2)
	
	return {
		"spectral_class": spectral_class,
		"subclass": subclass,
		"luminosity_class": luminosity_class,
	}


## Returns the number of spectral classes.
## @return: Total count of spectral classes.
static func count() -> int:
	return 7
