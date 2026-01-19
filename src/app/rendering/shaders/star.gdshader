shader_type spatial;
render_mode unshaded, cull_disabled;

uniform vec3 star_color : source_color = vec3(1.0, 0.9, 0.7);
uniform float temperature : hint_range(1000.0, 40000.0) = 5778.0;
uniform float emission_intensity : hint_range(0.1, 10.0) = 1.0;
uniform float corona_size : hint_range(1.0, 2.0) = 1.2;
uniform float corona_intensity : hint_range(0.0, 1.0) = 0.3;
uniform sampler2D noise_texture : hint_default_white;

varying vec3 world_pos;
varying vec3 view_normal;

void vertex() {
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	view_normal = (MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz;
}

void fragment() {
	// Base star color with emission
	vec3 base_emission = star_color * emission_intensity;
	
	// Add surface variation using noise
	vec2 uv = UV;
	float noise = texture(noise_texture, uv * 3.0).r;
	float surface_variation = mix(0.9, 1.0, noise);
	
	// Limb darkening effect
	float limb = dot(normalize(view_normal), vec3(0.0, 0.0, 1.0));
	limb = clamp(limb, 0.0, 1.0);
	float limb_darkening = mix(0.3, 1.0, pow(limb, 0.6));
	
	// Corona glow at edges
	float edge_glow = 1.0 - limb;
	edge_glow = pow(edge_glow, 3.0) * corona_intensity;
	
	// Combine effects
	vec3 final_emission = base_emission * surface_variation * limb_darkening;
	final_emission += star_color * edge_glow * emission_intensity * 2.0;
	
	// Add slight pulsation over time
	float pulse = sin(TIME * 2.0) * 0.02 + 1.0;
	final_emission *= pulse;
	
	// In unshaded mode, ALBEDO is what's displayed directly (no lighting)
	// EMISSION is used for bloom/glow post-processing effects
	// Set both so the star is always visible and glows properly
	ALBEDO = final_emission;
	EMISSION = final_emission;
}
