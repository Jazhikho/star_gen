shader_type spatial;
render_mode blend_add, depth_draw_opaque, cull_front, unshaded;

// Atmosphere rim shader with composition-based scattering, pressure thickness, and greenhouse glow.
// Rendered on a sphere slightly larger than the planet surface.

// === Atmosphere properties ===
uniform vec4 u_atmosphereColor : source_color = vec4(0.4, 0.6, 0.9, 1.0);
uniform float u_density : hint_range(0.0, 2.0) = 1.0;
uniform float u_falloff : hint_range(1.0, 10.0) = 3.0;
uniform float u_scatterStrength : hint_range(0.0, 2.0) = 0.8;

// === Greenhouse effect ===
uniform float u_greenhouseIntensity : hint_range(0.0, 1.0) = 0.0;
uniform vec3 u_greenhouseColor : source_color = vec3(1.0, 0.6, 0.3);

// === Lighting ===
uniform vec3 u_lightDir = vec3(0.7, 0.4, 0.6);
uniform float u_sunGlowStrength : hint_range(0.0, 1.0) = 0.3;

// === Terminator ===
uniform float u_terminatorSoftness : hint_range(0.0, 0.5) = 0.15;

void fragment() {
	vec3 N = NORMAL;
	vec3 V = VIEW;
	vec3 L = normalize(u_lightDir);

	// Basic rim calculation
	float rim = 1.0 - dot(N, V);
	rim = pow(rim, u_falloff);

	// Base atmosphere color
	vec3 atmo_color = u_atmosphereColor.rgb;

	// Rayleigh-like scattering (blue shift at edges)
	vec3 scatter_color = mix(atmo_color, atmo_color * vec3(0.7, 0.85, 1.2), rim * u_scatterStrength);

	// Day/night variation
	float NdotL = dot(N, L);
	float day_factor = smoothstep(-u_terminatorSoftness, u_terminatorSoftness, NdotL);

	// Sun glow on the lit side of the atmosphere
	float sun_alignment = max(0.0, dot(normalize(vec3(N.x, N.y, N.z)), L));
	float sun_glow = pow(sun_alignment, 3.0) * u_sunGlowStrength * day_factor;

	// Apply day/night to scatter color
	scatter_color *= (0.3 + day_factor * 0.7);
	scatter_color += vec3(1.0, 0.95, 0.9) * sun_glow;

	// Greenhouse effect: warm inner glow
	if (u_greenhouseIntensity > 0.0) {
		float inner = dot(N, V);
		inner = pow(inner, 2.0);

		// Greenhouse glow visible even on night side
		float greenhouse_factor = inner * u_greenhouseIntensity * 0.5;
		greenhouse_factor += (1.0 - day_factor) * u_greenhouseIntensity * 0.2;

		scatter_color = mix(scatter_color, u_greenhouseColor, greenhouse_factor);
	}

	// Final alpha based on density and rim
	float alpha = rim * u_atmosphereColor.a * u_density * 0.6;

	// Boost alpha slightly on terminator for visible atmosphere line
	float terminator_boost = (1.0 - abs(NdotL)) * 0.2;
	alpha += terminator_boost * u_density * 0.3;

	ALBEDO = scatter_color;
	ALPHA = clamp(alpha, 0.0, 0.9);
}
