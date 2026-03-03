## Config for tuning generation between scientifically calibrated and stylized output.
## Profile choice plus seed fully defines outcomes when generators respect it; no global state.
## Future: generators read profile to select weight tables, fill probabilities, etc.
class_name GenerationRealismProfile
extends RefCounted

const GENERATION_REALISM_PROFILE_BRIDGE_CLASS: StringName = &"CSharpGenerationRealismProfileBridge"


enum Mode {
	CALIBRATED, ## Tracks literature-derived distributions (IMF, exoplanet demographics).
	BALANCED, ## Default: visually rich diversity, roughly plausible.
	STYLIZED, ## More rings, habitable worlds, spectacular systems; still physically bounded.
}


## Current mode. Used by generators to choose parameter sets (when wired).
var mode: Mode = Mode.BALANCED

## Slider value in [0, 1]. 0 = STYLIZED, 0.5 = BALANCED, 1 = CALIBRATED.
var realism_slider: float = 0.5


## Builds a profile from a [0, 1] slider. 0 -> STYLIZED, 0.5 -> BALANCED, 1 -> CALIBRATED.
## @param slider: Value in [0, 1].
## @return: New profile with mode and realism_slider set.
static func from_slider(slider: float) -> GenerationRealismProfile:
	var bridge: Object = null
	if ClassDB.class_exists(GENERATION_REALISM_PROFILE_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(GENERATION_REALISM_PROFILE_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("FromSlider"):
		var payload: Variant = bridge.call("FromSlider", slider)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)

	var profile: GenerationRealismProfile = GenerationRealismProfile.new()
	profile.realism_slider = clampf(slider, 0.0, 1.0)
	if profile.realism_slider <= 0.33:
		profile.mode = Mode.STYLIZED
	elif profile.realism_slider >= 0.67:
		profile.mode = Mode.CALIBRATED
	else:
		profile.mode = Mode.BALANCED
	return profile


## Returns a profile for the Calibrated mode (realism-oriented runs).
## @return: Calibrated profile.
static func calibrated() -> GenerationRealismProfile:
	var bridge: Object = null
	if ClassDB.class_exists(GENERATION_REALISM_PROFILE_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(GENERATION_REALISM_PROFILE_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("Calibrated"):
		var payload: Variant = bridge.call("Calibrated")
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)

	var profile: GenerationRealismProfile = GenerationRealismProfile.new()
	profile.mode = Mode.CALIBRATED
	profile.realism_slider = 1.0
	return profile


## Returns a profile for the Balanced default mode.
## @return: Balanced profile.
static func balanced() -> GenerationRealismProfile:
	var bridge: Object = null
	if ClassDB.class_exists(GENERATION_REALISM_PROFILE_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(GENERATION_REALISM_PROFILE_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("Balanced"):
		var payload: Variant = bridge.call("Balanced")
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)

	var profile: GenerationRealismProfile = GenerationRealismProfile.new()
	profile.mode = Mode.BALANCED
	profile.realism_slider = 0.5
	return profile


## Returns a profile for the Stylized mode (cinematic emphasis).
## @return: Stylized profile.
static func stylized() -> GenerationRealismProfile:
	var bridge: Object = null
	if ClassDB.class_exists(GENERATION_REALISM_PROFILE_BRIDGE_CLASS):
		bridge = ClassDB.instantiate(GENERATION_REALISM_PROFILE_BRIDGE_CLASS)
	if bridge != null and bridge.has_method("Stylized"):
		var payload: Variant = bridge.call("Stylized")
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)

	var profile: GenerationRealismProfile = GenerationRealismProfile.new()
	profile.mode = Mode.STYLIZED
	profile.realism_slider = 0.0
	return profile


## Builds a profile instance from a bridge payload.
## @param payload: Dictionary containing mode and realism_slider values.
## @return: Profile reconstructed from the payload.
static func _from_payload(payload: Dictionary) -> GenerationRealismProfile:
	var profile: GenerationRealismProfile = GenerationRealismProfile.new()
	profile.mode = int(payload.get("mode", Mode.BALANCED)) as Mode
	profile.realism_slider = payload.get("realism_slider", 0.5) as float
	return profile
