shader_type spatial;

uniform vec3 base_color : source_color = vec3(0.8, 0.7, 0.5);
uniform vec3 band_color : source_color = vec3(0.9, 0.85, 0.7);
uniform vec3 storm_color : source_color = vec3(0.6, 0.4, 0.3);
uniform int band_count : hint_range(4, 30) = 12;
uniform float turbulence : hint_range(0.0, 1.0) = 0.3;
uniform float storm_intensity : hint_range(0.0, 1.0) = 0.2;
uniform float rotation_speed : hint_range(0.0, 1.0) = 0.1;
uniform sampler2D noise_texture : hint_default_white;

varying vec3 world_pos;
varying float latitude;

void vertex() {
	world_pos = VERTEX;
	// Calculate latitude from vertex position (-1 to 1)
	latitude = world_pos.y;
}

void fragment() {
	// Create banding pattern
	float band_frequency = float(band_count);
	float band_pattern = sin(latitude * band_frequency * PI);
	band_pattern = smoothstep(-0.5, 0.5, band_pattern);
	
	// Add turbulence to bands
	vec2 uv = UV;
	uv.x += TIME * rotation_speed * 0.1;
	float noise1 = texture(noise_texture, uv * 2.0).r;
	float noise2 = texture(noise_texture, uv * 5.0 + vec2(0.5)).r;
	float combined_noise = mix(noise1, noise2, 0.5);
	
	band_pattern += (combined_noise - 0.5) * turbulence;
	band_pattern = clamp(band_pattern, 0.0, 1.0);
	
	// Mix base and band colors
	vec3 surface_color = mix(base_color, band_color, band_pattern);
	
	// Add storm features (Great Red Spot-like)
	if (storm_intensity > 0.0) {
		vec2 storm_uv = uv * 3.0;
		storm_uv.x += TIME * rotation_speed * 0.05;
		float storm_noise = texture(noise_texture, storm_uv).r;
		
		// Create oval storm shapes
		float storm = 0.0;
		vec2 storm_pos = vec2(0.3, 0.4);
		float dist = distance(UV, storm_pos);
		if (dist < 0.1) {
			storm = smoothstep(0.1, 0.02, dist) * storm_noise;
		}
		
		surface_color = mix(surface_color, storm_color, storm * storm_intensity);
	}
	
	// Add subtle atmospheric scattering at edges
	vec3 normal = normalize(world_pos);
	float fresnel = pow(1.0 - dot(normal, VIEW), 2.0);
	surface_color += vec3(0.1, 0.15, 0.3) * fresnel * 0.3;
	
	ALBEDO = surface_color;
	ROUGHNESS = 0.8;
	METALLIC = 0.0;
}
