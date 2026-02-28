shader_type spatial;
render_mode unshaded, cull_back;

// Gas giant planet surface shader.
// Renders: latitude bands, zonal flow, turbulence, storms, polar vortices, oblateness, haze.
// Drive uniforms from GasGiantShaderParams.gd via MaterialFactory.
//
// Variety sources (so two same-archetype planets look distinct):
//   - u_bandWarp:        noise-warps latitude → variable band widths per seed
//   - per-band hashing:  each band index gets unique brightness/turb/flow/width
//   - u_bandSharpness:   soft gradients ↔ crisp edges
//   - u_stormCount/dark: variable storm count, some dark (Neptune) vs bright (Jupiter)
//   - u_hazeDensity:    thick haze obscures detail → mini-Neptune / Uranus look
//   - u_detailLevel:    gates fine features globally (streaks, chevrons, eddies)

#include "res://src/app/rendering/shaders/noise_lib.gdshaderinc"

// === Bands ===
uniform float u_bandCount : hint_range(4.0, 30.0) = 14.0;
uniform float u_bandContrast : hint_range(0.1, 1.0) = 0.5;
uniform float u_bandTurbulence : hint_range(0.1, 1.0) = 0.4;
uniform float u_bandSharpness : hint_range(0.0, 1.0) = 0.4; // 0=smooth gradient, 1=crisp edges
uniform float u_bandWarp : hint_range(0.0, 1.0) = 0.3;      // latitude-warp for variable band widths
uniform float u_chevronStrength : hint_range(0.0, 1.0) = 0.2; // scalloped boundaries

// === Flow ===
uniform float u_flowSpeed : hint_range(0.0, 1.0) = 0.3;
uniform int u_flowDetail : hint_range(1, 8) = 5;
uniform float u_jetStrength : hint_range(0.2, 2.0) = 1.0;   // how much per-band flow varies

// === Shape ===
uniform float u_oblateness : hint_range(0.0, 0.2) = 0.0;

// === Storms ===
uniform float u_stormIntensity : hint_range(0.0, 1.0) = 0.4;
uniform float u_stormScale : hint_range(0.5, 4.0) = 2.0;
uniform float u_vortexStrength : hint_range(0.0, 1.0) = 0.7;
uniform int u_stormCount : hint_range(0, 8) = 3;            // number of major storm systems
uniform float u_darkSpotRatio : hint_range(0.0, 1.0) = 0.3; // fraction of storms that are dark spots

// === Detail / Haze ===
uniform float u_detailLevel : hint_range(0.0, 1.0) = 0.7;   // gates fine-scale features
uniform float u_hazeDensity : hint_range(0.0, 1.0) = 0.0;   // overlying haze obscuring bands
uniform vec3 u_hazeColor : source_color = vec3(0.5, 0.55, 0.6);
uniform float u_streakIntensity : hint_range(0.0, 1.0) = 0.3; // fine longitudinal streaks

// === Colors ===
// Zones (light) and belts (dark) each get a 3-stop latitude gradient for far richer palettes.
// Legacy u_colBandLight / u_colBandDark / u_colPolar are mapped in GDScript.
uniform vec3 u_colBandLight : source_color = vec3(0.91, 0.835, 0.627); // legacy alias: zone equatorial
uniform vec3 u_colZoneMid : source_color = vec3(0.85, 0.78, 0.58);
uniform vec3 u_colBandDark : source_color = vec3(0.545, 0.42, 0.227);  // legacy alias: belt equatorial
uniform vec3 u_colBeltMid : source_color = vec3(0.50, 0.38, 0.22);
uniform vec3 u_colPolar : source_color = vec3(0.4, 0.533, 0.667);      // used for both zone_polar & belt_polar blend base
uniform vec3 u_colBeltPolar : source_color = vec3(0.35, 0.35, 0.42);
uniform vec3 u_colStorm : source_color = vec3(0.8, 0.4, 0.267);

// === Atmosphere ===
uniform float u_atmoDensity : hint_range(0.0, 2.0) = 1.2;
uniform float u_atmoFalloff : hint_range(1.0, 6.0) = 2.0;
uniform float u_scatterStrength : hint_range(0.0, 2.0) = 0.7;
uniform vec3 u_atmoColor : source_color = vec3(0.667, 0.733, 0.6);

// === Lighting ===
uniform float u_limbDark : hint_range(0.0, 2.0) = 0.9;
uniform float u_terminatorSharp : hint_range(0.01, 0.5) = 0.15;
uniform float u_ambient : hint_range(0.0, 0.3) = 0.04;

// === Animation ===
uniform float u_rotSpeed : hint_range(0.0, 0.5) = 0.12;
uniform float u_seed : hint_range(0.0, 100.0) = 0.0;

// === Light direction (world space) ===
uniform float u_lightX : hint_range(-1.0, 1.0) = 0.7;
uniform float u_lightY : hint_range(-1.0, 1.0) = 0.3;

// === Rings (optional) ===
uniform int u_ringType : hint_range(0, 4) = 0;

// === Varyings ===
varying vec3 v_local_pos;

// -----------------------------------------------------------------------------
// Helpers
// -----------------------------------------------------------------------------

vec3 get_pole_axis() {
	return vec3(0.0, 1.0, 0.0);
}

vec3 rotate_point(vec3 p, vec3 axis, float angle) {
	float c = cos(angle);
	float s = sin(angle);
	return p * c + cross(axis, p) * s + axis * dot(axis, p) * (1.0 - c);
}

// Cheap 1D→1D hashes (independent channels).
float hash_a(float n, float sd) {
	return fract(sin(n * 127.1 + sd * 311.7) * 43758.5453);
}
float hash_b(float n, float sd) {
	return fract(sin(n * 269.5 + sd * 183.3) * 22578.5453);
}
float hash_c(float n, float sd) {
	return fract(sin(n * 419.2 + sd * 371.9) * 39562.1581);
}

// Per-band properties hashed from band index.
// Returns: x = brightness offset [-0.2, 0.2]
//          y = turbulence multiplier [0.5, 1.5]
//          z = flow multiplier [-1.0, 1.0] (alternating direction with jitter)
//          w = width bias [-0.3, 0.3] (feeds harmonic phase)
vec4 band_properties(float band_idx, float sd) {
	float brightness = hash_a(band_idx * 3.7, sd * 1.1) * 0.4 - 0.2;
	float turb_mult  = 0.5 + hash_b(band_idx * 5.3, sd * 2.3) * 1.0;
	// Avoid ternary: flow_dir 1.0 when mod < 1, else -1.0
	float flow_dir = 1.0 - 2.0 * step(1.0, mod(band_idx, 2.0));
	float flow_mag   = 0.5 + hash_a(band_idx * 11.1, sd * 3.7) * 0.5;
	float flow_mult  = flow_dir * flow_mag;
	float width_bias = hash_b(band_idx * 7.9, sd * 4.1) * 0.6 - 0.3;
	return vec4(brightness, turb_mult, flow_mult, width_bias);
}

// Noise-warp latitude → creates variable-width bands that differ per seed.
float warp_latitude(float lat, float sd) {
	if (u_bandWarp < 0.01) return lat;
	float w1 = snoise(vec3(lat * 1.8 + sd * 5.0, sd * 2.0, 0.0)) * 0.35;
	float w2 = snoise(vec3(lat * 4.5 + sd * 8.0, sd * 3.0, 0.0)) * 0.12;
	return lat + (w1 + w2) * u_bandWarp;
}

// Two separate 3-stop gradients (zone & belt) across |latitude|.
void get_band_colors(float abs_lat, out vec3 zone_col, out vec3 belt_col) {
	float t = clamp(abs_lat, 0.0, 1.0);
	// Zone: equatorial → mid → polar
	if (t < 0.4) {
		float s = t / 0.4;
		zone_col = mix(u_colBandLight, u_colZoneMid, s);
	} else {
		float s = (t - 0.4) / 0.6;
		zone_col = mix(u_colZoneMid, u_colPolar, s);
	}
	// Belt: equatorial → mid → polar
	if (t < 0.4) {
		float s = t / 0.4;
		belt_col = mix(u_colBandDark, u_colBeltMid, s);
	} else {
		float s = (t - 0.4) / 0.6;
		belt_col = mix(u_colBeltMid, u_colBeltPolar, s);
	}
}

// -----------------------------------------------------------------------------
// Vertex
// -----------------------------------------------------------------------------

void vertex() {
	v_local_pos = VERTEX;

	vec3 pos = VERTEX;
	pos.y *= (1.0 - u_oblateness);

	float ob = 1.0 - u_oblateness;
	vec3 deformed_normal = normalize(vec3(pos.x, pos.y / (ob * ob), pos.z));

	VERTEX = pos;
	NORMAL = deformed_normal;
}

// -----------------------------------------------------------------------------
// Fragment
// -----------------------------------------------------------------------------

void fragment() {
	vec3 so = vec3(u_seed * 13.7, u_seed * 7.3, u_seed * 19.1);

	// Rotate sample point for animation
	vec3 k = get_pole_axis();
	vec3 sp = rotate_point(v_local_pos, k, TIME * u_rotSpeed);
	sp = normalize(sp);

	float raw_lat = dot(sp, k);
	float abs_lat = abs(raw_lat);
	float lon = atan(sp.x, sp.z);

	// ==========================================================
	// Band structure
	// ==========================================================

	// Warp latitude → variable-width bands unique to this seed
	float lat = warp_latitude(raw_lat, u_seed);

	float band_lat = asin(clamp(lat, -1.0, 1.0)) * 0.63662; // normalise to [-1, 1]
	float band_phase = band_lat * u_bandCount * 3.14159;
	float band_idx = floor(band_lat * u_bandCount + 0.5);

	// Per-band hashed properties — the key to making each band unique
	vec4 bp = band_properties(band_idx, u_seed);

	// Flow: per-band direction & magnitude, further boosted by u_jetStrength
	float flow_time = TIME * u_flowSpeed;
	float local_flow = flow_time * bp.z * u_jetStrength;

	// Multi-harmonic band structure with per-band phase offset
	float band_off = hash_a(band_idx, u_seed * 7.0) * 2.0;
	float h1 = sin(band_phase);
	float h2 = sin(band_phase * 1.618 + 1.7 + bp.w * 3.0) * 0.25 * sin(band_phase + band_off);
	float h3 = sin(band_phase * 2.414 - 0.9 - bp.w * 1.5) * 0.10;
	float h4 = sin(band_phase * 0.5 + 0.3) * 0.05;
	float band_base = h1 * 0.6 + h2 + h3 + h4;

	// Turbulence — scaled by per-band multiplier AND global detail level
	float turb_amount = u_bandTurbulence * bp.y * mix(0.4, 1.0, u_detailLevel);
	float turb1 = fbm(vec3(lon * 1.8 + local_flow * 0.6, band_lat * u_bandCount * 0.5, sp.z * 1.8 + so.x), u_flowDetail, 0.48);
	float turb2 = fbm(vec3(lon * 3.5 + local_flow * 0.85, band_lat * 3.5, sp.x * 2.5 + so.y), max(u_flowDetail - 1, 2), 0.45);
	float turb_large = fbm(vec3(lon * 0.8 + local_flow * 0.3, band_lat * u_bandCount * 0.25, sp.y * 1.2 + so.z), 3, 0.4);
	float turbulence = (turb1 * 0.5 + turb2 * 0.3 + turb_large * 0.2) * turb_amount;

	float band_perturbed = band_base + turbulence * 1.6;
	float band_val = clamp(band_perturbed * 0.5 + 0.5, 0.0, 1.0);

	// Sharpness control: blend soft ↔ crisp transitions
	float soft_val = band_val;
	float hard_val = smoothstep(0.35, 0.65, band_val);
	float very_hard = smoothstep(0.44, 0.56, band_val);
	float sharp_t = u_bandSharpness;
	band_val = mix(soft_val, mix(hard_val, very_hard, sharp_t), sharp_t);

	// ==========================================================
	// Color composition
	// ==========================================================

	vec3 zone_col, belt_col;
	get_band_colors(abs_lat, zone_col, belt_col);

	// Per-band brightness variation — makes individual bands stand out
	zone_col *= (1.0 + bp.x * u_bandContrast);
	belt_col *= (1.0 - bp.x * u_bandContrast * 0.5);

	vec3 band_color = mix(belt_col, zone_col, band_val);

	// --- Streaks (gated by detail level) ---
	if (u_streakIntensity > 0.01 && u_detailLevel > 0.2) {
		float streak_freq = u_bandCount * 3.5;
		float streak_disp = snoise(vec3(lon * 6.0 + local_flow * 1.2, band_lat * streak_freq + 0.4, so.z * 1.2)) * 0.5 + 0.5;
		float streak = sin(band_lat * streak_freq + streak_disp * 0.4) * 0.5 + 0.5;
		streak = smoothstep(0.35, 0.65, streak);
		band_color *= 1.0 + ((streak - 0.5) * u_streakIntensity * 0.15 * u_detailLevel);
	}

	// --- Chevrons: scalloped boundaries (gated by detail level) ---
	float bdry_prox = 1.0 - abs(band_val - 0.5) * 2.0;
	bdry_prox = smoothstep(0.0, 1.0, bdry_prox);
	if (u_chevronStrength > 0.01 && u_detailLevel > 0.25) {
		float chev_freq = 6.0 + u_bandCount * 0.35;
		float chev1 = sin(lon * chev_freq + band_phase * 0.5 + local_flow * 0.4);
		float chev2 = sin(lon * chev_freq * 1.7 - band_phase * 0.3 + local_flow * 0.6) * 0.4;
		float chev = (chev1 + chev2) * 0.5;
		float chev_amt = pow(bdry_prox, 2.0) * chev * u_chevronStrength * 0.12 * u_detailLevel;
		band_color = mix(band_color, zone_col * 0.92 + belt_col * 0.08, chev_amt);
	}

	// --- Rossby waves at boundaries ---
	{
		float wave_num = 4.0 + u_bandCount * 0.3;
		float r_wave = sin(lon * wave_num + band_phase * 0.4 + local_flow * 0.5) * 0.5 + 0.5;
		float wave_amp = bdry_prox * r_wave * 0.08 * u_bandTurbulence * u_detailLevel;
		band_color = mix(band_color, zone_col * 0.94 + belt_col * 0.06, wave_amp);
	}

	// ==========================================================
	// Storms — variable count, size, type (bright vs dark)
	// ==========================================================

	for (int i = 0; i < 8; i++) {
		if (i >= u_stormCount) break;
		float fi = float(i);

		// Position — fully hashed, spread in latitude & longitude
		float lat_h = hash_a(fi * 3.7 + 0.5, u_seed * 7.3);
		float lat_spread = hash_b(fi * 5.1, u_seed * 11.0);
		float s_lat = (lat_h - 0.5) * 2.0 * mix(0.25, 0.7, lat_spread);

		float s_lon_base = hash_a(fi * 43.1, u_seed * 17.3) * 6.28318;
		float s_flow_dir = sin(s_lat * u_bandCount * 3.14159);
		float s_lon = s_lon_base + flow_time * 0.12 * s_flow_dir;

		// Size — wide variety; squared hash biases toward smaller storms
		float size_h = hash_b(fi * 67.3, u_seed * 11.1);
		float size_factor = mix(0.25, 2.2, size_h * size_h);
		float st_w = u_stormScale * 0.12 * size_factor;
		float aspect = mix(0.35, 0.7, hash_a(fi * 23.1, u_seed * 19.0));
		float st_h = st_w * aspect;

		// Type — bright (Jupiter) vs dark (Neptune)
		bool is_dark = hash_a(fi * 89.1, u_seed * 23.7) < u_darkSpotRatio;

		// Per-storm intensity
		float storm_str = mix(0.3, 1.0, hash_b(fi * 51.3, u_seed * 7.9)) * u_stormIntensity;

		float d_lat = raw_lat - s_lat; // use unwarped lat for storm distance
		float d_lon = lon - s_lon;
		d_lon = d_lon - round(d_lon / 6.28318) * 6.28318;
		float s_dist = sqrt((d_lat / st_h) * (d_lat / st_h) + (d_lon / st_w) * (d_lon / st_w));

		if (s_dist < 3.0) {
			float s_angle = atan(d_lat / st_h, d_lon / st_w);

			float vortex_var = mix(0.5, 1.4, hash_a(fi * 31.7, u_seed * 29.0));
			float spiral = s_angle + s_dist * u_vortexStrength * vortex_var * 2.5;

			float ring_spd = mix(0.2, 0.5, hash_b(fi * 13.3, u_seed * 37.0));
			float ring2 = sin(s_dist * 4.5 - TIME * ring_spd * u_flowSpeed) * 0.5 + 0.5;
			ring2 = smoothstep(0.2, 0.8, ring2);

			float s_noise = fbm(
				vec3(cos(spiral) * s_dist * 3.0 + so.x * 2.0 + fi * 7.0,
					 sin(spiral) * s_dist * 3.0,
					 so.z + fi * 3.5),
				4, 0.45) * 0.5 + 0.5;
			float storm_tex = ring2 * 0.25 + s_noise * 0.75;

			float core_pull = smoothstep(2.2, 0.2, s_dist);

			vec3 storm_col;
			if (is_dark) {
				// Dark spot: depress into darkness rather than add storm color
				storm_col = mix(band_color, band_color * 0.35, core_pull * 0.8);
				storm_col = mix(storm_col, storm_col * (1.0 + (storm_tex - 0.5) * 0.12), 0.5);
			} else {
				storm_col = mix(band_color, u_colStorm, core_pull * 0.65);
				storm_col += vec3(0.015, 0.01, -0.006) * core_pull * storm_str;
				storm_col = mix(storm_col, storm_col * (1.0 + (storm_tex - 0.5) * 0.18), 0.55);
			}

			// Eye wall
			float eye_wall = smoothstep(0.8, 0.3, s_dist) * smoothstep(0.1, 0.45, s_dist);
			vec3 eye_col;
			if (is_dark) {
				eye_col = band_color * 0.25;
			} else {
				eye_col = zone_col * 0.90 + vec3(0.02, 0.012, 0.0);
			}
			storm_col = mix(storm_col, eye_col, eye_wall * 0.22);

			// Wisps (detail-gated, bright storms only)
			if (u_detailLevel > 0.4 && !is_dark) {
				float wisp = snoise(vec3(spiral * 2.0 + TIME * 0.1, s_dist * 8.0, fi * 5.0));
				float wisp_mask = smoothstep(2.0, 0.8, s_dist) * smoothstep(0.3, 0.5, wisp);
				storm_col = mix(storm_col, zone_col * 1.08, wisp_mask * 0.18 * u_detailLevel);
			}

			float s_mask = smoothstep(3.0, 0.8, s_dist) * storm_str;
			band_color = mix(band_color, storm_col, s_mask);
		}
	}

	// ==========================================================
	// Eddies (small boundary vortices) — detail-gated
	// ==========================================================

	if (u_detailLevel > 0.3 && u_stormIntensity > 0.05) {
		float eddy_n = fbm(sp * u_stormScale * 1.8 + so * 0.35, 4, 0.42);
		float eddy_ang = eddy_n * u_vortexStrength * 6.28;
		vec3 eddy_p = sp + vec3(cos(eddy_ang), 0.0, sin(eddy_ang)) * 0.03 * u_vortexStrength;
		float eddy_det = fbm(eddy_p * u_stormScale * 2.2 + so * 0.7, 4, 0.42);
		float eddy_mask = smoothstep(0.35, 0.6, eddy_n) * smoothstep(0.4, 0.65, eddy_det);
		eddy_mask *= u_stormIntensity * 0.3 * (bdry_prox * bdry_prox) * u_detailLevel;
		vec3 eddy_col = mix(belt_col, zone_col, 0.45);
		band_color = mix(band_color, eddy_col, eddy_mask);
	}

	// ==========================================================
	// Polar vortices — optionally polygonal (Saturn hexagon)
	// ==========================================================

	{
		float polar_fade = smoothstep(0.5, 0.9, abs_lat);
		if (polar_fade > 0.005) {
			float p_angle = atan(sp.x, sp.z) + flow_time * 0.10;
			float p_dist = 1.0 - abs_lat;

			float p_swirl = fbm(
				vec3(cos(p_angle * 3.0) * p_dist * 6.0 + flow_time * 0.15,
					 sin(p_angle * 3.0) * p_dist * 6.0,
					 so.x * 2.5 + sp.y * 4.0),
				5, 0.45) * 0.5 + 0.5;

			float p_ring = sin(p_dist * 10.0 - flow_time * 0.2) * 0.5 + 0.5;
			p_ring = smoothstep(0.25, 0.75, p_ring);

			// Polygonal modulation — some seeds get a "hexagon"
			float hex_strength = hash_a(13.0, u_seed * 3.0);     // 0..1: does this planet have it?
			float sides = 5.0 + floor(hash_a(17.0, u_seed * 5.0) * 4.0); // 5–8 sided
			float polygon = sin(p_angle * sides + flow_time * 0.05) * 0.5 + 0.5;
			p_ring = mix(p_ring, p_ring * polygon, hex_strength * smoothstep(0.7, 0.9, abs_lat));

			float p_tex = p_swirl * 0.6 + p_ring * 0.4;
			vec3 pol_col = mix(u_colPolar * 0.82, u_colPolar * 1.18, p_tex);
			pol_col += vec3(0.008, 0.018, 0.04) * smoothstep(0.72, 0.95, abs_lat);
			band_color = mix(band_color, pol_col, polar_fade);
		}
	}

	// ==========================================================
	// Lighting
	// ==========================================================

	vec3 N = normalize(NORMAL);
	vec3 L = normalize(vec3(u_lightX, u_lightY, 0.6));

	float NdL = dot(N, L);
	float terminator = smoothstep(-u_terminatorSharp, u_terminatorSharp, NdL);
	float diff = max(NdL, 0.0);
	float wrap = max(NdL * 0.5 + 0.5, 0.0);
	float light = mix(diff, wrap, 0.38) * terminator + u_ambient * (1.0 - terminator * 0.5);

	float z = abs(dot(N, VIEW));
	float limb_dark = pow(z, u_limbDark * 0.6 + 0.4);
	limb_dark = mix(0.6, 1.0, limb_dark);

	vec3 lit = band_color * (light + u_ambient) * limb_dark;
	vec3 H = normalize(L + vec3(0.0, 0.0, 1.0));
	float spec = pow(max(dot(N, H), 0.0), 28.0) * 0.22;
	lit += vec3(1.0) * spec * diff * 0.4;

	// ==========================================================
	// Haze layer — obscures detail (mini-Neptune / Uranus look)
	// Applied BEFORE the atmosphere rim so the rim still shows cleanly.
	// ==========================================================

	if (u_hazeDensity > 0.01) {
		float limb_haze = pow(1.0 - z, 1.5);
		float haze_var = 1.0 + snoise(sp * 2.0 + vec3(u_seed)) * 0.15;
		float haze_amt = u_hazeDensity * (0.3 + limb_haze * 0.7) * haze_var;
		float haze_bright = mix(0.6, 1.0, diff * 0.5 + 0.5);
		vec3 haze_c = u_hazeColor * haze_bright;
		float haze_n = snoise(sp * 3.0 + vec3(u_seed * 2.0)) * 0.08;
		haze_c *= (1.0 + haze_n);
		lit = mix(lit, haze_c, clamp(haze_amt, 0.0, 0.85));
	}

	// ==========================================================
	// Atmosphere rim
	// ==========================================================

	float rim = 1.0 - z;
	float a_r = pow(rim, u_atmoFalloff) * u_atmoDensity;
	vec3 rim_col = mix(u_atmoColor, u_atmoColor * vec3(0.85, 0.92, 1.1), rim * u_scatterStrength);
	rim_col *= (diff + 0.3);
	lit = mix(lit, rim_col, a_r * 0.6);

	lit *= terminator * (1.0 - u_ambient) + u_ambient;

	ALBEDO = lit;
}
