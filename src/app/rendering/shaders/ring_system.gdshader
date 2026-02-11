shader_type spatial;
render_mode cull_disabled, depth_draw_alpha_prepass, blend_mix;

// Ring system shader with band-based density, gaps, composition coloring, and optical depth.
// Rendered on an annulus mesh aligned to the planet's equatorial plane.
// Uses UV: x = angle (0-1), y = radial position (0 = inner, 1 = outer).

#include "res://src/app/rendering/shaders/noise_lib.gdshaderinc"

// === Ring structure ===
uniform int u_bandCount : hint_range(1, 12) = 5;
uniform float u_innerRadius : hint_range(1.0, 2.0) = 1.4;
uniform float u_outerRadius : hint_range(1.5, 4.0) = 2.6;
uniform float u_density : hint_range(0.1, 1.0) = 0.65;
uniform float u_gapSize : hint_range(0.0, 0.5) = 0.15;

// === Colors ===
uniform vec3 u_colorInner : source_color = vec3(0.8, 0.6, 0.4);
uniform vec3 u_colorMid : source_color = vec3(0.73, 0.67, 0.53);
uniform vec3 u_colorOuter : source_color = vec3(0.53, 0.6, 0.67);

// === Lighting ===
uniform vec3 u_lightDir = vec3(0.7, 0.4, 0.6);
uniform float u_ambient : hint_range(0.0, 0.5) = 0.15;

// === Animation/Variation ===
uniform float u_seed : hint_range(0.0, 100.0) = 0.0;

// Ring density profile with gaps and band structure
float ring_profile(float t, int num_bands, float gap_size, float seed_val) {
	if (num_bands < 1) {
		return 0.0;
	}

	float bands = float(num_bands);
	float v = 0.0;

	for (int i = 0; i < 12; i++) {
		if (i >= num_bands) {
			break;
		}
		float fi = float(i);

		// Deterministic variation per band
		float h1 = fract(sin(fi * 127.1 + seed_val * 311.7) * 43758.5453);
		float h2 = fract(sin(fi * 269.5 + seed_val * 183.3) * 22578.5453);
		float h3 = fract(sin(fi * 71.3 + seed_val * 53.1) * 12345.6789);

		// Band center with slight variation
		float base_center = (fi + 0.5) / bands;
		float center_offset = (h1 - 0.5) * 0.3 / bands;
		float band_center = clamp(base_center + center_offset, fi / bands + 0.02, (fi + 1.0) / bands - 0.02);

		// Band width variation
		float width_var = 0.3 + h2 * 0.7;
		float base_width = (1.0 / bands) * (1.0 - gap_size) * 0.45;
		float band_width = base_width * width_var;

		// Band strength variation (inner bands slightly stronger)
		float strength_var = 0.5 + h3 * 0.5;
		float band_strength = strength_var * (1.0 - 0.12 * fi / bands);

		// Gaussian-like profile
		float d = abs(t - band_center) / max(band_width, 0.005);
		float sharpness = 1.2 + 1.5 * (1.0 - width_var);
		float profile = exp(-d * d * sharpness);

		// Add noise variation
		float n = snoise(vec3(t * 22.0 + seed_val * 2.0 + fi * 3.7, seed_val * 5.0 + fi * 1.3, fi * 0.5)) * 0.18;

		v = max(v, profile * (band_strength + n));
	}

	// Soft edges at inner and outer boundaries
	float inner_edge = smoothstep(0.0, 0.015, t);
	float outer_edge = smoothstep(1.0, 0.97, t);

	return clamp(v * inner_edge * outer_edge, 0.0, 1.0);
}

void vertex() {
	// Pass through for UV usage in fragment
}

void fragment() {
	// Radial position from UV.y (0 = inner, 1 = outer) - matches BodyRenderer ring mesh
	float t = UV.y;

	// Skip if outside ring bounds
	if (t < 0.0 || t > 1.0) {
		discard;
	}

	// Calculate ring density/alpha
	float ring_alpha = ring_profile(t, u_bandCount, u_gapSize, u_seed) * u_density;

	if (ring_alpha < 0.005) {
		discard;
	}

	// Ring color gradient (inner to outer)
	vec3 ring_color;
	if (t < 0.5) {
		ring_color = mix(u_colorInner, u_colorMid, t * 2.0);
	} else {
		ring_color = mix(u_colorMid, u_colorOuter, (t - 0.5) * 2.0);
	}

	// Add noise variation to color
	float angle = UV.x * TAU;
	float dist = length(vec2(cos(angle), sin(angle)));
	float color_noise = snoise(vec3(dist * 38.0 + u_seed * 1.3, angle * 4.0, u_seed * 3.0)) * 0.12;
	ring_color *= (1.0 + color_noise);

	// Fine particle detail
	float fine_noise = snoise(vec3(UV.x * 100.0, t * 20.0, u_seed * 7.0)) * 0.5 + 0.5;
	float fine_detail = smoothstep(0.3, 0.7, fine_noise) * 0.1;
	ring_color += vec3(fine_detail);

	// Soft lighting so rings don't fade in/out as the planet rotates (min 0.65, max 1.0)
	vec3 N = normalize(NORMAL);
	vec3 L = normalize(u_lightDir);
	float NdotL = abs(dot(N, L));
	float light = 0.65 + 0.35 * NdotL;

	ring_color *= light;

	// No view-dependent alpha so rings stay visible as the planet rotates
	ALBEDO = ring_color;
	ALPHA = ring_alpha * 0.88;
	ROUGHNESS = 0.9;
	METALLIC = 0.0;
}
