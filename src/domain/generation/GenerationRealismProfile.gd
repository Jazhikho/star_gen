## Config for tuning generation between scientifically calibrated and stylized output.
## Profile choice plus seed fully defines outcomes when generators respect it; no global state.
## Future: generators read profile to select weight tables, fill probabilities, etc.
class_name GenerationRealismProfile
extends RefCounted


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
static func from_slider(slider: float) -> RefCounted:
	var script_class: GDScript = load("res://src/domain/generation/GenerationRealismProfile.gd") as GDScript
	var profile: RefCounted = script_class.new()
	profile.realism_slider = clampf(slider, 0.0, 1.0)
	if profile.realism_slider <= 0.33:
		profile.mode = Mode.STYLIZED
	elif profile.realism_slider >= 0.67:
		profile.mode = Mode.CALIBRATED
	else:
		profile.mode = Mode.BALANCED
	return profile


## Returns a profile for the Calibrated mode (realism-oriented runs).
static func calibrated() -> RefCounted:
	var script_class: GDScript = load("res://src/domain/generation/GenerationRealismProfile.gd") as GDScript
	var profile: RefCounted = script_class.new()
	profile.mode = Mode.CALIBRATED
	profile.realism_slider = 1.0
	return profile


## Returns a profile for the Balanced default mode.
static func balanced() -> RefCounted:
	var script_class: GDScript = load("res://src/domain/generation/GenerationRealismProfile.gd") as GDScript
	var profile: RefCounted = script_class.new()
	profile.mode = Mode.BALANCED
	profile.realism_slider = 0.5
	return profile


## Returns a profile for the Stylized mode (cinematic emphasis).
static func stylized() -> RefCounted:
	var script_class: GDScript = load("res://src/domain/generation/GenerationRealismProfile.gd") as GDScript
	var profile: RefCounted = script_class.new()
	profile.mode = Mode.STYLIZED
	profile.realism_slider = 0.0
	return profile
