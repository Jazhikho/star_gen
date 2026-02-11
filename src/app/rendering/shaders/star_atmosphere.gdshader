shader_type spatial;
render_mode blend_add, depth_draw_opaque, cull_back, unshaded;

// Star atmosphere/corona shader - renders on a larger sphere behind the surface.
// Handles: chromosphere rim, corona streamers, prominences, flares, bloom, diffraction spikes.

#include "res://src/app/rendering/shaders/noise_lib.gdshaderinc"

// === Core properties (must match star_surface) ===
uniform vec3 u_star_color : source_color = vec3(1.0, 0.95, 0.85);
uniform float u_luminosity : hint_range(0.1, 5.0) = 1.0;

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

// === Effects ===
uniform float u_bloomRadius : hint_range(0.0, 0.5) = 0.15;
uniform float u_bloomIntensity : hint_range(0.0, 2.0) = 0.6;
uniform float u_spikeCount : hint_range(0.0, 8.0) = 4.0;
uniform float u_spikeLength : hint_range(0.0, 1.0) = 0.3;
uniform float u_spikeBright : hint_range(0.0, 1.0) = 0.25;
uniform float u_seed : hint_range(0.0, 100.0) = 0.0;

// === Geometry info ===
// The atmosphere mesh is scaled larger than the star surface.
// star_radius_ratio = surface_radius / atmosphere_radius (e.g., 0.8 means surface is 80% of atmo mesh)
uniform float u_star_radius_ratio : hint_range(0.5, 0.95) = 0.8;

varying vec3 v_local_pos;
varying vec3 v_view_dir;

void vertex() {
	v_local_pos = VERTEX;
	v_view_dir = normalize((inverse(MODELVIEW_MATRIX) * vec4(0.0, 0.0, 0.0, 1.0)).xyz - VERTEX);
}

void fragment() {
	vec3 N = normalize(v_local_pos);
	float d = length(v_local_pos);
	float nd = d;

	// The star surface is at radius R (ratio of our mesh)
	float R = u_star_radius_ratio;

	float t = TIME;
	float ang = atan(N.y, N.x);

	vec3 color = vec3(0.0);
	float alpha = 0.0;

	// === Diffraction spikes ===
	if (u_spikeCount > 0.5) {
		int spike_n = int(u_spikeCount);
		float spike_max = 0.0;
		for (int i = 0; i < 8; i++) {
			if (i >= spike_n) break;
			float fi = float(i);
			float sa = PI * fi / float(spike_n);
			float da = abs(mod(ang - sa + PI, TAU) - PI);
			float spk = exp(-da * 120.0) / (1.0 + nd * nd / (R * R * u_spikeLength * 0.5 + 0.001) * 3.0);
			spike_max = max(spike_max, spk);
		}
		color += u_star_color * spike_max * u_spikeBright * u_luminosity * 0.5;
		alpha = max(alpha, spike_max * u_spikeBright * 0.5);
	}

	// === Corona ===
	if (u_coronaBright > 0.01 && nd > R * 0.7) {
		float corona_d = max(0.0, (nd - R)) / (R * u_coronaExtent + 0.001);
		float stream_val = 0.0;

		if (u_coronaStreams > 0.5) {
			float n_s = u_coronaStreams;
			vec2 cn = N.xy / max(length(N.xy), 0.001);
			float sn1 = fbm(vec3(cn.x * n_s * 1.2, cn.y * n_s * 1.2, u_seed * 5.0 + t * 0.02), 3, 0.5);
			float sn2 = fbm(vec3(cn.x * n_s * 0.7 + 7.0, cn.y * n_s * 0.7 + 3.0, u_seed * 3.0 + t * 0.015), 2, 0.5);
			stream_val = (sn1 * 0.6 + sn2 * 0.4 + 0.5) * u_coronaLength;
			stream_val += pow(abs(cn.y), 2.0) * 0.3 * u_coronaLength;

			float asym_n = fbm(vec3(cn.x * 1.5 + u_seed * 3.0, cn.y * 1.5, t * 0.01), 2, 0.5);
			stream_val *= (1.0 + asym_n * u_coronaAsym);
		}

		float corona_fade = exp(-corona_d * (1.5 - min(stream_val, 0.8) * 0.8));
		float corona = corona_fade * u_coronaBright * (0.5 + stream_val * 0.5);

		vec3 corona_col = mix(u_star_color, vec3(1.0), 0.6) * corona;

		float fine_n = fbm(vec3(N.x * 8.0 + t * 0.03, N.y * 8.0, u_seed * 7.0), 3, 0.5);
		corona_col *= (1.0 + fine_n * 0.25);

		float corona_alpha = corona * smoothstep(R * 0.85, R * 1.1, nd);
		color += corona_col * corona_alpha;
		alpha = max(alpha, corona_alpha);
	}

	// === Prominences ===
	if (u_promCount > 0.5 && nd > R * 0.85 && nd < R * (1.0 + u_promHeight * 2.5)) {
		int prom_count = int(u_promCount);
		for (int i = 0; i < 8; i++) {
			if (i >= prom_count) break;
			float fi = float(i);
			float p_seed = u_seed * 17.3 + fi * 41.7;
			float p_ang = fract(sin(p_seed * 127.1) * 43758.5) * TAU;
			float p_w = 0.08 + fract(sin(p_seed * 269.5) * 22578.5) * 0.12;

			float p_dir = mod(ang - p_ang + PI, TAU) - PI;
			float ang_dist = abs(p_dir);

			if (ang_dist < p_w * 3.0) {
				float radial_t = (nd - R * 0.92) / (R * u_promHeight * 2.0);
				if (radial_t > 0.0 && radial_t < 1.0) {
					float arch_x = p_dir / p_w;
					float arch_y = 1.0 - arch_x * arch_x;
					arch_y = max(0.0, arch_y);

					float loop_t = abs(radial_t - arch_y * 0.8);
					float loop_shape = exp(-loop_t * loop_t * 60.0) * arch_y;
					float fill_shape = arch_y * exp(-radial_t * 2.0) * smoothstep(0.0, 0.3, arch_y - radial_t * 0.5);
					float prom_shape = max(loop_shape * 0.7, fill_shape * 0.5);

					float prom_noise = fbm(vec3(p_dir * 20.0 + t * 0.1 + fi * 5.0, radial_t * 8.0, p_seed), 3, 0.55) * 0.5 + 0.5;
					float prom = prom_shape * prom_noise * u_promGlow;
					prom *= exp(-ang_dist / p_w * 1.5);

					vec3 prom_col = mix(u_star_color, vec3(1.0, 0.25, 0.15), 0.65) * prom;
					color += prom_col * 0.6;
					alpha = max(alpha, prom * 0.6);
				}
			}
		}

		// === Flares ===
		if (u_flareIntensity > 0.01) {
			float flare_seed = u_seed * 31.7 + floor(t * 0.15) * 7.3;
			for (int i = 0; i < 3; i++) {
				float fi = float(i);
				float f_ang = fract(sin(flare_seed + fi * 77.7) * 43758.5) * TAU;
				float f_w = 0.04 + fract(sin(flare_seed + fi * 33.3) * 22578.5) * 0.06;
				float f_dir = mod(ang - f_ang + PI, TAU) - PI;
				float ang_d = abs(f_dir);

				if (ang_d < f_w * 2.5) {
					float f_rad = (nd - R * 0.95) / (R * 0.2);
					if (f_rad > -0.5 && f_rad < 1.0) {
						float flare = exp(-f_rad * f_rad * 3.0) * exp(-ang_d * ang_d / (f_w * f_w * 2.0)) * u_flareIntensity;
						float pulse = sin(t * 1.5 + fi * 3.0) * 0.3 + 0.7;
						vec3 flare_col = mix(u_star_color, vec3(1.0), 0.6);
						color += flare_col * flare * pulse * 2.0;
						alpha = max(alpha, flare * pulse);
					}
				}
			}
		}
	}

	// === Chromosphere rim ===
	if (u_chromoIntensity > 0.01) {
		float chromo_edge = smoothstep(R + u_chromoThick, R, nd) * smoothstep(R - 0.005, R + u_chromoThick * 0.5, nd);
		vec3 chromo_col = mix(u_star_color, vec3(1.0, 0.2, 0.15), u_chromoShift * 0.7);

		vec2 cn = N.xy / max(length(N.xy), 0.001);
		float spicule_angle = atan(cn.y, cn.x);
		float spicules = sin(spicule_angle * 40.0 + fbm(vec3(cn * 8.0, u_seed * 11.0 + t * 0.3), 3, 0.5) * 6.0) * 0.5 + 0.5;
		float spicule_h = smoothstep(R, R + u_chromoThick, nd) * smoothstep(R + u_chromoThick * 1.3, R + u_chromoThick * 0.5, nd);

		chromo_col *= (0.7 + spicules * 0.5);
		float chromo_alpha = (chromo_edge + spicule_h * 0.3) * u_chromoIntensity * 0.5;
		color += chromo_col * chromo_alpha;
		alpha = max(alpha, chromo_alpha);
	}

	// === Bloom ===
	if (u_bloomIntensity > 0.01) {
		float bloom_d = max(0.0, nd - R * 0.5) / (R * (1.0 + u_bloomRadius * 3.0));
		float bloom = exp(-bloom_d * bloom_d * 3.0) * u_bloomIntensity;
		bloom *= smoothstep(R * 0.3, R, nd);
		float bloom2 = exp(-bloom_d * bloom_d * 0.8) * u_bloomIntensity * 0.3;
		bloom2 *= smoothstep(R * 0.1, R * 0.8, nd);

		vec3 bloom_col = mix(u_star_color, vec3(1.0), 0.3) * bloom * 0.2 + u_star_color * bloom2 * 0.1;
		color += bloom_col;
		alpha = max(alpha, bloom * 0.3);
	}

	// Inside the star surface, don't render atmosphere
	if (nd < R * 0.98) {
		alpha = 0.0;
		color = vec3(0.0);
	}

	ALBEDO = color;
	EMISSION = color;
	ALPHA = clamp(alpha, 0.0, 1.0);
}
