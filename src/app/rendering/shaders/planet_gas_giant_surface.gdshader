shader_type spatial;
render_mode unshaded, cull_back;

// Gas giant planet surface shader.
// Renders: latitude bands, zonal flow, turbulence, storms, polar vortices, oblateness.
// Drive uniforms from GasGiantShaderParams.gd via MaterialFactory.

#include "res://src/app/rendering/shaders/noise_lib.gdshaderinc"

// === Bands ===
uniform float u_bandCount : hint_range(4.0, 30.0) = 14.0;
uniform float u_bandContrast : hint_range(0.1, 1.0) = 0.5;
uniform float u_bandTurbulence : hint_range(0.1, 1.0) = 0.4;

// === Flow ===
uniform float u_flowSpeed : hint_range(0.0, 1.0) = 0.3;
uniform int u_flowDetail : hint_range(1, 8) = 5;

// === Shape ===
uniform float u_oblateness : hint_range(0.0, 0.2) = 0.065;

// === Storms ===
uniform float u_stormIntensity : hint_range(0.0, 1.0) = 0.4;
uniform float u_stormScale : hint_range(0.5, 4.0) = 2.0;
uniform float u_vortexStrength : hint_range(0.0, 1.0) = 0.7;

// === Colors ===
uniform vec3 u_colBandLight : source_color = vec3(0.91, 0.835, 0.627);
uniform vec3 u_colBandDark : source_color = vec3(0.545, 0.42, 0.227);
uniform vec3 u_colStorm : source_color = vec3(0.8, 0.4, 0.267);
uniform vec3 u_colPolar : source_color = vec3(0.4, 0.533, 0.667);

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

// === Rings (optional, from RingShaderParams) ===
uniform int u_ringType : hint_range(0, 4) = 0;

// === Varyings ===
varying vec3 v_local_pos;

// Returns the planetary pole axis in model space.
// Axial tilt is applied via mesh rotation in BodyRenderer, not in the shader.
vec3 get_pole_axis() {
	return vec3(0.0, 1.0, 0.0);
}

// Rotate point around axis using Rodrigues' formula
vec3 rotate_point(vec3 p, vec3 axis, float angle) {
	float c = cos(angle);
	float s = sin(angle);
	return p * c + cross(axis, p) * s + axis * dot(axis, p) * (1.0 - c);
}

void vertex() {
	// Store original sphere vertex for noise sampling (pre-oblateness)
	v_local_pos = VERTEX;

	// Apply oblateness: flatten poles (Y axis), Rp = Re * (1 - oblateness)
	vec3 pos = VERTEX;
	pos.y *= (1.0 - u_oblateness);

	// Corrected normal for oblate spheroid (gradient of x^2 + y^2/(1-o)^2 + z^2)
	float ob = 1.0 - u_oblateness;
	vec3 deformed_normal = normalize(vec3(pos.x, pos.y / (ob * ob), pos.z));

	VERTEX = pos;
	NORMAL = deformed_normal;
}

void fragment() {
	vec3 so = vec3(u_seed * 13.7, u_seed * 7.3, u_seed * 19.1);

	// Rotate sample point for animation
	vec3 k = get_pole_axis();
	vec3 sp = rotate_point(v_local_pos, k, TIME * u_rotSpeed);
	sp = normalize(sp);

	float lat = dot(sp, k);
	float abs_lat = abs(lat);
	float lon = atan(sp.x, sp.z);

	// Band structure
	float band_lat = asin(clamp(lat, -1.0, 1.0)) * 0.63662;
	float band_phase = band_lat * u_bandCount * 3.14159;

	// Smoother band harmonics with more gradual transitions
	float h1 = sin(band_phase) * 0.6;
	float h2 = sin(band_phase * 1.618 + 1.7) * 0.25;
	float h3 = sin(band_phase * 2.414 - 0.9) * 0.10;
	float h4 = sin(band_phase * 0.5 + 0.3) * 0.05; // Low frequency smoothing
	float band_base = h1 + h2 + h3 + h4;

	// Zonal flow
	float flow_dir = sin(band_phase);
	float flow_time = TIME * u_flowSpeed;
	float jet_mag = 1.0 + cos(band_phase) * 0.35;
	float local_flow = flow_time * flow_dir * jet_mag;

	// Smoother turbulence with lower frequencies and better blending
	float turb1 = fbm(vec3(lon * 1.8 + local_flow * 0.6, band_lat * u_bandCount * 0.5, sp.z * 1.8 + so.x), u_flowDetail, 0.48);
	float turb2 = fbm(vec3(lon * 3.5 + local_flow * 0.85, band_lat * 3.5, sp.x * 2.5 + so.y), max(u_flowDetail - 1, 2), 0.45);
	float turb_large = fbm(vec3(lon * 0.8 + local_flow * 0.3, band_lat * u_bandCount * 0.25, sp.y * 1.2 + so.z), 3, 0.4);
	float turbulence = (turb1 * 0.5 + turb2 * 0.3 + turb_large * 0.2) * u_bandTurbulence;
	float band_perturbed = band_base + turbulence * 1.6;
	float band_val = clamp(band_perturbed * 0.5 + 0.5, 0.0, 1.0);

	// Color zones
	float lat_temp = 1.0 - abs_lat * 0.55;
	vec3 zone_col = mix(u_colPolar * 0.92 + vec3(0.015), u_colBandLight * 1.04, lat_temp * 0.85);
	zone_col += vec3(0.015, 0.01, -0.008) * lat_temp;
	vec3 belt_col = mix(u_colPolar * 0.72, u_colBandDark * 1.06, lat_temp * 0.82);
	belt_col += vec3(0.035, 0.018, -0.025) * lat_temp * u_bandContrast;
	float zone_belt = smoothstep(0.38, 0.62, band_val);
	vec3 band_color = mix(belt_col, zone_col, zone_belt);

	// Softer streaks with lower frequency
	float streak_freq = u_bandCount * 3.5;
	float streak_disp = snoise(vec3(lon * 6.0 + local_flow * 1.2, band_lat * streak_freq + 0.4, so.z * 1.2)) * 0.5 + 0.5;
	float streak = sin(band_lat * streak_freq + streak_disp * 0.4) * 0.5 + 0.5;
	streak = smoothstep(0.35, 0.65, streak);
	band_color *= 1.0 + ((streak - 0.5) * 0.08 * u_bandContrast);

	// Smoother band boundary waves
	float bdry_prox = 1.0 - abs(zone_belt - 0.5) * 2.0;
	bdry_prox = smoothstep(0.0, 1.0, bdry_prox);
	float wave_num = 4.0 + u_bandCount * 0.3;
	float r_wave = sin(lon * wave_num + band_phase * 0.4 + local_flow * 0.5) * 0.5 + 0.5;
	float wave_amp = bdry_prox * r_wave * 0.08 * u_bandTurbulence;
	band_color = mix(band_color, zone_col * 0.94 + belt_col * 0.06, wave_amp);

	// Major storms (3)
	for (int i = 0; i < 3; i++) {
		float fi = float(i);
		float s_lat = sin(u_seed * 7.3 + fi * 2.1) * 0.38;
		float s_lon_base = u_seed * 4.1 + fi * 3.7;
		float s_flow_dir = sin(s_lat * u_bandCount * 3.14159);
		float s_lon = s_lon_base + flow_time * 0.12 * s_flow_dir;
		float d_lat = lat - s_lat;
		float d_lon = lon - s_lon;
		d_lon = d_lon - round(d_lon / 6.28318) * 6.28318;
		float st_w = u_stormScale * 0.12 * (1.0 - 0.18 * fi);
		float st_h = st_w * 0.45;
		float s_dist = sqrt((d_lat / st_h) * (d_lat / st_h) + (d_lon / st_w) * (d_lon / st_w));
		if (s_dist < 2.5) {
			float s_angle = atan(d_lat / st_h, d_lon / st_w);
			float spiral = s_angle + s_dist * u_vortexStrength * 2.5;
			float ring2 = sin(s_dist * 4.5 - TIME * 0.35 * u_flowSpeed) * 0.5 + 0.5;
			ring2 = smoothstep(0.2, 0.8, ring2);
			float s_noise = fbm(vec3(cos(spiral) * s_dist * 3.0 + so.x * 2.0 + fi * 7.0, sin(spiral) * s_dist * 3.0, so.z + fi * 3.5), 4, 0.45) * 0.5 + 0.5;
			float storm_tex = ring2 * 0.25 + s_noise * 0.75;
			float core_pull = smoothstep(2.0, 0.2, s_dist);
			vec3 storm_col = mix(band_color, u_colStorm, core_pull * 0.65);
			storm_col += vec3(0.015, 0.01, -0.006) * core_pull * u_stormIntensity;
			storm_col = mix(storm_col, storm_col * (1.0 + (storm_tex - 0.5) * 0.18), 0.55);
			float eye_wall = smoothstep(0.8, 0.3, s_dist) * smoothstep(0.1, 0.45, s_dist);
			storm_col = mix(storm_col, zone_col * 0.90 + vec3(0.02, 0.012, 0.0), eye_wall * 0.22);
			float s_mask = smoothstep(2.5, 0.8, s_dist) * u_stormIntensity * (1.0 - 0.12 * fi);
			band_color = mix(band_color, storm_col, s_mask);
		}
	}

	// Smoother eddy features
	float eddy_n = fbm(sp * u_stormScale * 1.8 + so * 0.35, 4, 0.42);
	float eddy_ang = eddy_n * u_vortexStrength * 6.28;
	vec3 eddy_p = sp + vec3(cos(eddy_ang), 0.0, sin(eddy_ang)) * 0.03 * u_vortexStrength;
	float eddy_det = fbm(eddy_p * u_stormScale * 2.2 + so * 0.7, 4, 0.42);
	float eddy_mask = smoothstep(0.35, 0.6, eddy_n) * smoothstep(0.4, 0.65, eddy_det) * u_stormIntensity * 0.3 * (bdry_prox * bdry_prox);
	vec3 eddy_col = mix(belt_col, zone_col, 0.45);
	band_color = mix(band_color, eddy_col, eddy_mask);

	// Smoother polar vortices
	float polar_fade = smoothstep(0.5, 0.9, abs_lat);
	float p_angle = atan(sp.x, sp.z) + flow_time * 0.10;
	float p_dist = 1.0 - abs_lat;
	float p_swirl = fbm(vec3(cos(p_angle * 3.0) * p_dist * 6.0 + flow_time * 0.15, sin(p_angle * 3.0) * p_dist * 6.0, so.x * 2.5 + sp.y * 4.0), 5, 0.45) * 0.5 + 0.5;
	float p_ring = sin(p_dist * 10.0 - flow_time * 0.2) * 0.5 + 0.5;
	p_ring = smoothstep(0.25, 0.75, p_ring);
	float p_tex = p_swirl * 0.6 + p_ring * 0.4;
	vec3 pol_col = mix(u_colPolar * 0.82, u_colPolar * 1.18, p_tex);
	pol_col += vec3(0.008, 0.018, 0.04) * smoothstep(0.72, 0.95, abs_lat);
	band_color = mix(band_color, pol_col, polar_fade);

	// Lighting
	vec3 N = normalize(NORMAL);
	vec3 L = normalize(vec3(u_lightX, u_lightY, 0.6));
	vec3 N_light = N;

	float NdL = dot(N_light, L);
	float terminator = smoothstep(-u_terminatorSharp, u_terminatorSharp, NdL);
	float diff = max(NdL, 0.0);
	float wrap = max(NdL * 0.5 + 0.5, 0.0);
	float light = mix(diff, wrap, 0.38) * terminator + u_ambient * (1.0 - terminator * 0.5);

	float z = abs(dot(N, VIEW));
	float limb_dark = pow(z, u_limbDark * 0.6 + 0.4);
	limb_dark = mix(0.6, 1.0, limb_dark);

	vec3 lit = band_color * (light + u_ambient) * limb_dark;
	vec3 H = normalize(L + vec3(0.0, 0.0, 1.0));
	float spec = pow(max(dot(N_light, H), 0.0), 28.0) * 0.22;
	lit += vec3(1.0) * spec * diff * 0.4;

	// Atmosphere rim
	float rim = 1.0 - z;
	float a_r = pow(rim, u_atmoFalloff) * u_atmoDensity;
	vec3 rim_col = mix(u_atmoColor, u_atmoColor * vec3(0.85, 0.92, 1.1), rim * u_scatterStrength);
	rim_col *= (diff + 0.3);
	lit = mix(lit, rim_col, a_r * 0.6);

	lit *= terminator * (1.0 - u_ambient) + u_ambient;

	ALBEDO = lit;
}
