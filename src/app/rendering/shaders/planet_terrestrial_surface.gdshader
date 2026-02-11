shader_type spatial;
render_mode unshaded, cull_back;

// Terrestrial planet surface shader ported from Concepts/planetgenerator.html.
// Renders: continental terrain, oceans, ice caps, clouds, atmospheric effects.
// Drive uniforms from TerrestrialShaderParams.gd via MaterialFactory.

#include "res://src/app/rendering/shaders/noise_lib.gdshaderinc"

// === Terrain ===
uniform float u_terrainScale : hint_range(1.0, 12.0) = 4.0;
uniform float u_terrainHeight : hint_range(0.0, 1.0) = 0.5;
uniform float u_roughness : hint_range(0.0, 1.0) = 0.55;
uniform float u_continentSize : hint_range(0.5, 5.0) = 2.0;
uniform float u_landCoherence : hint_range(0.0, 1.0) = 0.6;
uniform float u_coastalDetail : hint_range(0.0, 1.0) = 0.5;
uniform int u_octaves : hint_range(1, 8) = 6;

// === Surface colors ===
uniform vec3 u_colLow : source_color = vec3(0.165, 0.29, 0.102);
uniform vec3 u_colMid : source_color = vec3(0.353, 0.541, 0.227);
uniform vec3 u_colHigh : source_color = vec3(0.541, 0.478, 0.353);
uniform vec3 u_colPeak : source_color = vec3(0.8, 0.733, 0.667);

// === Ocean ===
uniform float u_seaLevel : hint_range(0.0, 1.0) = 0.4;
uniform float u_seaSpecular : hint_range(0.0, 2.0) = 0.8;
uniform float u_fresnelStrength : hint_range(0.0, 2.0) = 1.0;
uniform vec3 u_colSeaShallow : source_color = vec3(0.133, 0.533, 0.733);
uniform vec3 u_colSeaDeep : source_color = vec3(0.039, 0.133, 0.267);

// === Ice caps ===
uniform float u_iceCap : hint_range(0.0, 1.0) = 0.3;
uniform vec3 u_colIce : source_color = vec3(0.867, 0.933, 1.0);

// === Atmosphere ===
uniform float u_atmoDensity : hint_range(0.0, 2.0) = 0.6;
uniform float u_atmoFalloff : hint_range(0.5, 6.0) = 2.5;
uniform float u_scatterStrength : hint_range(0.0, 2.0) = 0.8;
uniform vec3 u_atmoColor : source_color = vec3(0.267, 0.533, 0.8);

// === Clouds ===
uniform float u_cloudCoverage : hint_range(0.0, 1.0) = 0.4;
uniform float u_cloudScale : hint_range(1.0, 10.0) = 3.5;
uniform float u_cloudShadow : hint_range(0.0, 1.0) = 0.3;
uniform vec3 u_cloudColor : source_color = vec3(1.0, 1.0, 1.0);

// === Lighting ===
uniform float u_limbDark : hint_range(0.0, 2.0) = 0.8;
uniform float u_terminatorSharp : hint_range(0.01, 0.5) = 0.12;
uniform float u_ambient : hint_range(0.0, 0.3) = 0.04;

// === Animation ===
uniform float u_rotSpeed : hint_range(0.0, 0.5) = 0.06;
uniform float u_cloudDrift : hint_range(0.0, 0.3) = 0.03;
uniform float u_seed : hint_range(0.0, 100.0) = 0.0;

// === City lights (optional) ===
uniform float u_cityLightIntensity : hint_range(0.0, 1.0) = 0.0;
uniform vec3 u_cityLightColor : source_color = vec3(1.0, 0.85, 0.5);

// === Varyings ===
varying vec3 v_local_pos;

// Sigmoid function for land coherence
float sigmoid(float x, float k) {
	return 1.0 / (1.0 + exp(-k * x));
}

// Returns the planetary pole axis in model space.
// Axial tilt is applied via mesh rotation in BodyRenderer, not in the shader.
vec3 get_pole_axis() {
	return vec3(0.0, 1.0, 0.0);
}

// Rotate point around axis using Rodrigues' formula
vec3 rotate_point(vec3 p, vec3 axis, float angle) {
	float c = cos(angle);
	float s = sin(angle);
	vec3 k = normalize(axis);
	return p * c + cross(k, p) * s + k * dot(k, p) * (1.0 - c);
}

void vertex() {
	v_local_pos = VERTEX;
}

void fragment() {
	// Model-space normal for noise sampling (terrain, clouds, ice)
	vec3 sphere_normal = normalize(v_local_pos);
	float t = TIME;

	// View-space vectors for lighting (matches HTML reference behavior)
	vec3 N = normalize(NORMAL);
	vec3 L = normalize(vec3(0.7, 0.4, 0.6));
	vec3 V = VIEW;

	// Seed offset for deterministic generation
	vec3 seed_offset = vec3(u_seed * 13.7, u_seed * 7.3, u_seed * 19.1);

	// Pole axis in model space (tilt applied by mesh rotation)
	vec3 pole_axis = get_pole_axis();

	// Apply planetary rotation (model space for noise sampling)
	float rot_angle = t * u_rotSpeed;
	vec3 sp = rotate_point(sphere_normal, pole_axis, rot_angle);

	// === TERRAIN GENERATION ===
	// Continental base - large smooth regions
	float cont_base = fbm(sp * u_continentSize + seed_offset, 3, 0.5);

	// Apply sigmoid for coherent land masses
	float coherence_k = 4.0 + u_landCoherence * 12.0;
	float cont_mask = sigmoid(cont_base, coherence_k);

	// Fractal detail for coastlines
	float detail = fbm(sp * u_terrainScale + seed_offset, u_octaves, u_roughness);

	// Edge detection - apply detail mainly at coastlines
	float edge_dist = abs(cont_mask - 0.5);
	float edge_factor = max(0.0, 1.0 - edge_dist * 4.0);
	edge_factor = pow(edge_factor, 0.7);

	// Coastal detail modulation
	float coastal_mod = detail * edge_factor * u_coastalDetail * 0.4;

	// Combine: solid continental cores with fractal coastal edges
	float terrain = cont_mask + coastal_mod;

	// Add surface detail on land
	float surface_detail = fbm(sp * u_terrainScale * 2.0 + seed_offset * 1.5, max(1, u_octaves - 2), u_roughness);
	terrain += surface_detail * 0.12 * (1.0 - edge_factor) * step(0.5, cont_mask);

	// Normalize and apply height curve
	terrain = terrain * 0.5 + 0.5;
	terrain = clamp(terrain, 0.0, 1.0);
	terrain = pow(terrain, 1.0 + (1.0 - u_terrainHeight));

	// === LIGHTING ===
	float NdotL = dot(N, L);
	float terminator = smoothstep(-u_terminatorSharp, u_terminatorSharp, NdotL);
	float diff = max(NdotL, 0.0);
	float wrap = max(NdotL * 0.5 + 0.5, 0.0);
	float light = mix(diff, wrap, 0.3) * terminator + u_ambient * (1.0 - terminator * 0.5);

	vec3 H = normalize(L + V);
	float spec = pow(max(dot(N, H), 0.0), 80.0);

	// === SURFACE COLOR ===
	float sl = u_seaLevel;
	bool is_ocean = terrain < sl;
	vec3 surface_color;
	float surface_spec = 0.0;

	if (is_ocean) {
		// Ocean depth
		float depth_factor = smoothstep(sl, sl - 0.25, terrain);
		vec3 deep_color = u_colSeaDeep * vec3(0.7, 0.85, 1.0);
		surface_color = mix(u_colSeaShallow, deep_color, depth_factor);

		// Animated waves
		float wave_time = t * 0.8;
		float w1 = snoise(sp * 12.0 + vec3(wave_time * 0.7, wave_time * 0.5, 0.0)) * 0.5 + 0.5;
		float w2 = snoise(sp * 24.0 + vec3(-wave_time * 0.5, wave_time * 0.8, wave_time * 0.3)) * 0.5 + 0.5;
		float w3 = snoise(sp * 48.0 + vec3(wave_time * 0.3, -wave_time * 0.6, wave_time * 0.4)) * 0.5 + 0.5;
		float wave = w1 * 0.5 + w2 * 0.35 + w3 * 0.15;

		float depth_fade = 1.0 - depth_factor * 0.6;
		surface_color += vec3(0.04, 0.06, 0.08) * wave * depth_fade;
		surface_color = mix(surface_color, u_colSeaShallow * 1.15, wave * 0.18 * depth_fade);

		// Fresnel reflection
		vec3 wave_normal = normalize(N + vec3(w1 - 0.5, w2 - 0.5, 0.0) * 0.08);
		float fresnel = pow(1.0 - max(dot(wave_normal, V), 0.0), 5.0) * u_fresnelStrength;
		vec3 reflect_color = mix(u_atmoColor, vec3(0.9, 0.95, 1.0), 0.5);
		surface_color = mix(surface_color, reflect_color, fresnel * 0.4);

		// Specular highlights
		vec3 R = reflect(-L, wave_normal);
		float spec_wave = pow(max(dot(R, V), 0.0), 120.0) * u_seaSpecular;
		float spec_broad = pow(max(dot(R, V), 0.0), 20.0) * u_seaSpecular * 0.3;
		surface_spec = spec_wave + spec_broad;

		// Foam near coastlines
		float foam = smoothstep(0.35, 0.42, terrain / sl) * smoothstep(sl, sl - 0.04, terrain);
		foam *= smoothstep(0.55, 0.75, w1 * w2) * 2.0;
		surface_color = mix(surface_color, vec3(0.85, 0.9, 0.95), foam * 0.25);
	} else {
		// Land elevation coloring
		float t_land = (terrain - sl) / (1.0 - sl);
		float t_smooth = smoothstep(0.0, 1.0, t_land);

		if (t_smooth < 0.25) {
			surface_color = mix(u_colLow, u_colMid, t_smooth / 0.25);
		} else if (t_smooth < 0.55) {
			surface_color = mix(u_colMid, u_colHigh, (t_smooth - 0.25) / 0.3);
		} else {
			surface_color = mix(u_colHigh, u_colPeak, (t_smooth - 0.55) / 0.45);
		}

		// Terrain noise variation
		float terrain_noise = snoise(sp * 20.0 + seed_offset) * 0.08;
		surface_color *= (1.0 + terrain_noise);
		surface_spec = 0.05;
	}

	// === ICE CAPS ===
	float lat = abs(dot(sp, pole_axis));
	float ice_start = 1.0 - u_iceCap;

	// Noise for ice edge variation
	float n1 = snoise(sp * 4.0 + seed_offset) * 0.18;
	float n2 = snoise(sp * 8.0 + seed_offset * 1.3) * 0.12;
	float n3 = snoise(sp * 16.0 + seed_offset * 2.7) * 0.06;
	float n4 = snoise(sp * 32.0 + seed_offset * 4.1) * 0.03;
	float perturbed_lat = lat + n1 + n2 + n3 + n4;

	// Glacier reach variation
	float rn = abs(snoise(sp * 6.0 + seed_offset * 0.7));
	float glacier_reach = rn * 0.1 * u_iceCap;
	float ice_threshold = ice_start - glacier_reach;

	float ice_factor = smoothstep(ice_threshold - 0.02, ice_threshold + 0.06, perturbed_lat);

	// Mountain snow on land
	if (!is_ocean) {
		float t_land = (terrain - sl) / (1.0 - sl);
		ice_factor = max(ice_factor, smoothstep(0.7, 0.95, t_land) * smoothstep(ice_start - 0.35, ice_start - 0.05, lat) * u_iceCap * 0.5);
	}

	ice_factor = clamp(ice_factor, 0.0, 1.0);
	float ice_detail = snoise(sp * 20.0 + seed_offset) * 0.08;
	surface_color = mix(surface_color, u_colIce * (1.0 + ice_detail), ice_factor);
	surface_spec = mix(surface_spec, 0.4, ice_factor);

	// === APPLY LIGHTING ===
	vec3 lit_color = surface_color * (light + u_ambient) + vec3(1.0) * spec * surface_spec * diff;

	// === CLOUDS ===
	float cloud_angle = t * (u_rotSpeed + u_cloudDrift);
	vec3 cloud_pos = rotate_point(sphere_normal, pole_axis, cloud_angle);

	float cloud_noise = fbm(cloud_pos * u_cloudScale + seed_offset * 0.5, 5, 0.5);
	float cloud_alpha = smoothstep(0.3 - u_cloudCoverage * 0.5, 0.6, cloud_noise) * u_cloudCoverage;

	// Cloud shadows on surface
	vec3 shadow_offset = L * 0.02;
	vec3 shadow_sample = cloud_pos + shadow_offset;
	float shadow_cloud = fbm(shadow_sample * u_cloudScale + seed_offset * 0.5, 5, 0.5);
	shadow_cloud = smoothstep(0.3 - u_cloudCoverage * 0.5, 0.6, shadow_cloud) * u_cloudCoverage;
	lit_color *= (1.0 - shadow_cloud * u_cloudShadow * 0.4);

	// Blend clouds
	lit_color *= (1.0 - cloud_alpha * 0.2);
	lit_color = mix(lit_color, u_cloudColor * (light + u_ambient * 2.0), cloud_alpha * 0.8);

	// === ATMOSPHERIC SCATTERING ===
	// z = 1 at sphere center (facing camera), 0 at limb
	float z = max(dot(N, V), 0.0);
	float rim = 1.0 - z;
	float atmo_rim = pow(rim, u_atmoFalloff) * u_atmoDensity;
	vec3 scatter_color = mix(u_atmoColor, u_atmoColor * vec3(0.6, 0.8, 1.3), rim * u_scatterStrength);
	lit_color = mix(lit_color, scatter_color, atmo_rim * 0.6 * (diff + 0.3));

	// === LIMB DARKENING ===
	float limb_factor = pow(z, u_limbDark * 0.5 + 0.5);
	lit_color *= mix(0.7, 1.0, limb_factor);

	// === CITY LIGHTS (night side) ===
	if (u_cityLightIntensity > 0.01 && !is_ocean) {
		float night_mask = smoothstep(0.05, -0.1, NdotL);
		float city_noise = snoise(sp * 40.0 + seed_offset);
		float cities = smoothstep(0.75, 0.95, city_noise) * night_mask * u_cityLightIntensity;
		lit_color += u_cityLightColor * cities;
	}

	// === FINAL TERMINATOR ===
	lit_color *= terminator * (1.0 - u_ambient) + u_ambient;

	ALBEDO = lit_color;
}
