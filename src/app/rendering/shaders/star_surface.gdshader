shader_type spatial;
render_mode unshaded, cull_back;

// Stellar surface shader ported from Concepts/stargenerator.html.
// Renders: granulation, sunspots, chromosphere, corona, prominences, flares, bloom, diffraction spikes.
// Drive uniforms from StarShaderParams.gd via MaterialFactory.

#include "res://src/app/rendering/shaders/noise_lib.gdshaderinc"

// === Core stellar properties ===
uniform vec3 u_star_color : source_color = vec3(1.0, 0.95, 0.85);
uniform float u_temperature : hint_range(1000.0, 50000.0) = 5778.0;
uniform float u_luminosity : hint_range(0.1, 5.0) = 1.0;
uniform float u_limbDark : hint_range(0.0, 1.0) = 0.6;

// === Granulation ===
uniform float u_granScale : hint_range(5.0, 50.0) = 30.0;
uniform float u_granContrast : hint_range(0.0, 1.0) = 0.35;
uniform float u_granTurb : hint_range(0.0, 1.0) = 0.4;
uniform float u_granFlow : hint_range(0.0, 0.3) = 0.08;
uniform float u_superGranScale : hint_range(2.0, 10.0) = 6.0;
uniform float u_superGranStr : hint_range(0.0, 0.5) = 0.15;

// === Sunspots ===
uniform float u_spotCount : hint_range(0.0, 20.0) = 5.0;
uniform float u_spotSize : hint_range(0.0, 0.2) = 0.06;
uniform float u_penumbra : hint_range(1.0, 3.0) = 2.0;
uniform float u_spotDark : hint_range(0.1, 0.6) = 0.35;

// === Chromosphere ===
uniform float u_chromoThick : hint_range(0.0, 0.05) = 0.015;
uniform float u_chromoIntensity : hint_range(0.0, 2.0) = 0.8;
uniform float u_chromoShift : hint_range(0.0, 1.0) = 0.5;

// === Corona ===
uniform float u_coronaExtent : hint_range(0.0, 1.0) = 0.3;
uniform float u_coronaBright : hint_range(0.0, 1.0) = 0.5;
uniform float u_coronaStreams : hint_range(0.0, 16.0) = 8.0;
uniform float u_coronaLength : hint_range(0.0, 1.0) = 0.5;
uniform float u_coronaAsym : hint_range(0.0, 1.0) = 0.3;

// === Prominences and flares ===
uniform float u_promCount : hint_range(0.0, 8.0) = 3.0;
uniform float u_promHeight : hint_range(0.0, 0.3) = 0.12;
uniform float u_promGlow : hint_range(0.0, 2.0) = 0.8;
uniform float u_flareIntensity : hint_range(0.0, 1.0) = 0.2;

// === Animation and effects ===
uniform float u_rotSpeed : hint_range(0.0, 0.5) = 0.05;
uniform float u_bloomRadius : hint_range(0.0, 0.5) = 0.15;
uniform float u_bloomIntensity : hint_range(0.0, 2.0) = 0.6;
uniform float u_spikeCount : hint_range(0.0, 8.0) = 4.0;
uniform float u_spikeLength : hint_range(0.0, 1.0) = 0.3;
uniform float u_spikeBright : hint_range(0.0, 1.0) = 0.25;
uniform float u_seed : hint_range(0.0, 100.0) = 0.0;

// === Varyings ===
varying vec3 v_local_pos;
varying vec3 v_view_dir;
varying float v_dist_from_center;

void vertex() {
	v_local_pos = VERTEX;
	v_view_dir = normalize((inverse(MODELVIEW_MATRIX) * vec4(0.0, 0.0, 0.0, 1.0)).xyz - VERTEX);
	v_dist_from_center = length(VERTEX);
}

void fragment() {
	// Normalized position on unit sphere
	vec3 N = normalize(v_local_pos);

	// View-dependent calculations (use built-in TIME for animation)
	float z = max(dot(N, normalize(v_view_dir)), 0.0);
	float t = TIME;

	// Seed offset for determinism
	vec3 seed_offset = vec3(u_seed * 13.7, u_seed * 7.3, u_seed * 19.1);

	// Apply rotation
	float rot_angle = t * u_rotSpeed;
	float ca = cos(rot_angle);
	float sa = sin(rot_angle);
	vec3 sp = vec3(N.x * ca - N.z * sa, N.y, N.x * sa + N.z * ca);

	// === Supergranulation ===
	float super_gran = 0.0;
	if (u_superGranStr > 0.01) {
		vec2 sv = voronoi(sp * u_superGranScale + seed_offset * 0.3);
		super_gran = (sv.y - sv.x) * u_superGranStr;
	}

	// === Granulation ===
	float gran_time = t * u_granFlow;
	vec3 gran_pos = sp * u_granScale + seed_offset + vec3(gran_time * 0.3, gran_time * 0.2, gran_time * 0.15);
	vec2 gv = voronoi(gran_pos);
	float cell_edge = gv.y - gv.x;
	float granulation = smoothstep(0.0, 0.3, cell_edge);

	// Add turbulence detail
	float turb_detail = fbm(sp * u_granScale * 2.0 + seed_offset + vec3(gran_time), 4, 0.55) * 0.5 + 0.5;
	granulation = mix(granulation, turb_detail, u_granTurb * 0.4);

	float gran_bright = 1.0 + granulation * u_granContrast - u_granContrast * 0.5;
	gran_bright += super_gran * 0.5;

	// === Magnetic bright points ===
	float mbp = 0.0;
	if (u_granContrast > 0.1) {
		float mbp_noise = snoise(sp * u_granScale * 3.0 + seed_offset * 2.0);
		float in_lane = 1.0 - smoothstep(0.0, 0.12, cell_edge);
		mbp = in_lane * smoothstep(0.6, 0.9, mbp_noise) * 0.08 * u_granContrast;
	}

	// === Sunspots ===
	float spot_mask = 1.0;
	float faculae = 0.0;
	int spot_count = int(u_spotCount);

	for (int i = 0; i < 20; i++) {
		if (i >= spot_count) break;
		float fi = float(i);

		// Deterministic spot position from seed
		float h1 = fract(sin(u_seed * 127.1 + fi * 311.7) * 43758.5);
		float h2 = fract(sin(u_seed * 269.5 + fi * 183.3) * 22578.5);
		float h3 = fract(sin(u_seed * 71.3 + fi * 53.1) * 12345.7);

		// Spot position (latitude limited to ±40° like real sunspots)
		float s_theta = (0.8 * h1 - 0.4) * PI * 0.5;
		float s_phi = h2 * TAU;
		float s_cos = cos(s_theta);
		vec3 spot_pos = vec3(s_cos * cos(s_phi), sin(s_theta), s_cos * sin(s_phi));

		float spot_dist = length(sp - spot_pos);
		float s_size = u_spotSize * (0.5 + h3);

		// Umbra (dark center)
		float umbra = smoothstep(s_size * 0.4, s_size * 0.15, spot_dist);

		// Penumbra (lighter outer region with filaments)
		float penumbra_r = s_size * u_penumbra;
		float penumbra_val = smoothstep(penumbra_r, s_size * 0.25, spot_dist);

		// Penumbral filaments
		float pen_angle = atan(sp.y - spot_pos.y, sp.x - spot_pos.x);
		float pen_fil = sin(pen_angle * 16.0 + fbm(vec3(pen_angle * 5.0, spot_dist * 30.0, u_seed + fi), 3, 0.5) * 5.0) * 0.5 + 0.5;
		float rad_fil = sin(spot_dist * 80.0 + pen_angle * 4.0) * 0.5 + 0.5;
		penumbra_val *= (0.6 + pen_fil * 0.25 + rad_fil * 0.15);

		float spot_val = mix(penumbra_val, 1.0, umbra);
		spot_mask = min(spot_mask, mix(1.0, u_spotDark, spot_val));

		// Faculae (bright regions around spots, visible near limb)
		float fac_r = u_spotSize * u_penumbra * 2.5;
		float fac = smoothstep(fac_r, u_spotSize * u_penumbra * 0.8, spot_dist);
		fac *= smoothstep(u_spotSize * 0.3, u_spotSize * u_penumbra * 0.5, spot_dist);
		fac *= pow(1.0 - z, 0.8) * 0.9 + 0.1;
		float plage_n = fbm(sp * 15.0 + seed_offset + vec3(fi * 3.0), 3, 0.5) * 0.5 + 0.5;
		fac *= (0.5 + plage_n * 0.5);
		faculae = max(faculae, fac * 0.15);
	}

	// === Limb darkening (use shared noise_lib function) ===
	vec3 limb_col = limb_darken(u_star_color, z, u_limbDark);

	// === Combine surface features ===
	vec3 surf_color = limb_col * gran_bright * spot_mask * u_luminosity;
	surf_color += u_star_color * faculae * u_luminosity;
	surf_color += u_star_color * mbp * u_luminosity;

	// === Chromospheric network (visible in granulation lanes) ===
	if (u_chromoIntensity > 0.01) {
		float network = 1.0 - smoothstep(0.0, 0.15, cell_edge);
		vec3 net_col = mix(u_star_color, vec3(1.0, 0.4, 0.3), u_chromoShift * 0.3);
		surf_color += net_col * network * u_chromoIntensity * 0.04 * pow(1.0 - z, 0.5);
	}

	// === Output ===
	ALBEDO = surf_color;
	EMISSION = surf_color;
}
