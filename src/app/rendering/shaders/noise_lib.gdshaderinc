// Shared noise functions for celestial body shaders.
// Include with: #include "res://src/app/rendering/shaders/noise_lib.gdshaderinc"

// ============================================================================
// COMMON UTILITIES
// ============================================================================

vec3 mod289_v3(vec3 x) {
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 mod289_v4(vec4 x) {
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 permute(vec4 x) {
	return mod289_v4(((x * 34.0) + 1.0) * x);
}

vec4 taylor_inv_sqrt(vec4 r) {
	return 1.79284291400159 - 0.85373472095314 * r;
}

// ============================================================================
// SIMPLEX NOISE 3D
// ============================================================================

float snoise(vec3 v) {
	vec2 C = vec2(1.0 / 6.0, 1.0 / 3.0);
	vec4 D = vec4(0.0, 0.5, 1.0, 2.0);

	// First corner
	vec3 i = floor(v + dot(v, vec3(C.y, C.y, C.y)));
	vec3 x0 = v - i + dot(i, vec3(C.x, C.x, C.x));

	// Other corners
	vec3 g = step(x0.yzx, x0.xyz);
	vec3 l = 1.0 - g;
	vec3 i1 = min(g, l.zxy);
	vec3 i2 = max(g, l.zxy);

	vec3 x1 = x0 - i1 + vec3(C.x, C.x, C.x);
	vec3 x2 = x0 - i2 + vec3(C.y, C.y, C.y);
	vec3 x3 = x0 - vec3(D.y, D.y, D.y);

	// Permutations
	i = mod289_v3(i);
	vec4 p = permute(permute(permute(
		i.z + vec4(0.0, i1.z, i2.z, 1.0))
		+ i.y + vec4(0.0, i1.y, i2.y, 1.0))
		+ i.x + vec4(0.0, i1.x, i2.x, 1.0));

	// Gradients
	float n_ = 0.142857142857; // 1.0/7.0
	vec3 ns = n_ * vec3(D.w, D.y, D.z) - vec3(D.x, D.z, D.x);

	vec4 j = p - 49.0 * floor(p * ns.z * ns.z);

	vec4 x_ = floor(j * ns.z);
	vec4 y_ = floor(j - 7.0 * x_);

	vec4 x = x_ * ns.x + vec4(ns.y, ns.y, ns.y, ns.y);
	vec4 y = y_ * ns.x + vec4(ns.y, ns.y, ns.y, ns.y);
	vec4 h = 1.0 - abs(x) - abs(y);

	vec4 b0 = vec4(x.xy, y.xy);
	vec4 b1 = vec4(x.zw, y.zw);

	vec4 s0 = floor(b0) * 2.0 + 1.0;
	vec4 s1 = floor(b1) * 2.0 + 1.0;
	vec4 sh = -step(h, vec4(0.0));

	vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
	vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

	vec3 p0 = vec3(a0.xy, h.x);
	vec3 p1 = vec3(a0.zw, h.y);
	vec3 p2 = vec3(a1.xy, h.z);
	vec3 p3 = vec3(a1.zw, h.w);

	// Normalize gradients
	vec4 norm = taylor_inv_sqrt(vec4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;

	// Mix final noise value
	vec4 m = max(0.6 - vec4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), vec4(0.0));
	m = m * m;
	return 42.0 * dot(m * m, vec4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
}

// ============================================================================
// FRACTIONAL BROWNIAN MOTION
// ============================================================================

// Standard FBM with configurable octaves and roughness.
// @param p: Sample position.
// @param octaves: Number of noise layers (1-8).
// @param roughness: Amplitude decay per octave (0.0-1.0, typically 0.5).
// @return: Noise value roughly in range [-1, 1].
float fbm(vec3 p, int octaves, float roughness) {
	float value = 0.0;
	float amplitude = 0.5;
	float frequency = 1.0;
	float sum_amplitude = 0.0;

	for (int i = 0; i < 8; i++) {
		if (i >= octaves) break;
		value += amplitude * snoise(p * frequency);
		sum_amplitude += amplitude;
		amplitude *= roughness;
		frequency *= 2.0;
	}

	return value / sum_amplitude;
}

// Warped FBM for more organic-looking terrain.
// @param p: Sample position.
// @param octaves: Number of noise layers.
// @param roughness: Amplitude decay per octave.
// @return: Warped noise value.
float wfbm(vec3 p, int octaves, float roughness) {
	vec3 q = vec3(
		fbm(p, octaves, roughness),
		fbm(p + vec3(5.2, 1.3, 2.8), octaves, roughness),
		fbm(p + vec3(1.7, 9.2, 4.1), octaves, roughness)
	);
	return fbm(p + 1.5 * q, octaves, roughness);
}

// Ridged FBM for mountain-like features.
// @param p: Sample position.
// @param octaves: Number of noise layers.
// @param roughness: Amplitude decay per octave.
// @return: Ridged noise value in range [0, 1].
float ridged_fbm(vec3 p, int octaves, float roughness) {
	float value = 0.0;
	float amplitude = 0.5;
	float frequency = 1.0;
	float sum_amplitude = 0.0;

	for (int i = 0; i < 8; i++) {
		if (i >= octaves) break;
		float n = 1.0 - abs(snoise(p * frequency));
		n = n * n;
		value += amplitude * n;
		sum_amplitude += amplitude;
		amplitude *= roughness;
		frequency *= 2.0;
	}

	return value / sum_amplitude;
}

// ============================================================================
// VORONOI / CELLULAR NOISE
// ============================================================================

// Returns vec2(distance to closest cell, distance to second closest).
// Useful for granulation, terrain cells, etc.
vec2 voronoi(vec3 p) {
	vec3 ip = floor(p);
	vec3 fp = fract(p);

	float d_min = 10.0;
	float d_min2 = 10.0;

	for (int x = -1; x <= 1; x++) {
		for (int y = -1; y <= 1; y++) {
			for (int z = -1; z <= 1; z++) {
				vec3 neighbor = vec3(float(x), float(y), float(z));
				vec3 cell_pos = ip + neighbor;

				// Pseudo-random point within cell
				vec3 random_offset = vec3(
					fract(sin(dot(cell_pos, vec3(127.1, 311.7, 74.7))) * 43758.5453),
					fract(sin(dot(cell_pos, vec3(269.5, 183.3, 246.1))) * 22578.5453),
					fract(sin(dot(cell_pos, vec3(419.2, 371.9, 128.5))) * 12345.6789)
				);

				vec3 diff = neighbor + random_offset - fp;
				float dist = dot(diff, diff);

				if (dist < d_min) {
					d_min2 = d_min;
					d_min = dist;
				} else if (dist < d_min2) {
					d_min2 = dist;
				}
			}
		}
	}

	return vec2(sqrt(d_min), sqrt(d_min2));
}

// Cell edge detection from voronoi.
// Returns 0 at cell centers, 1 at cell edges.
float voronoi_edge(vec3 p) {
	vec2 v = voronoi(p);
	return v.y - v.x;
}

// ============================================================================
// DOMAIN WARPING
// ============================================================================

// Applies domain warping for more complex patterns.
// @param p: Sample position.
// @param strength: Warp intensity.
// @param scale: Warp noise scale.
// @return: Warped position.
vec3 domain_warp(vec3 p, float strength, float scale) {
	vec3 warp = vec3(
		snoise(p * scale),
		snoise(p * scale + vec3(31.416, 17.532, 23.891)),
		snoise(p * scale + vec3(71.123, 43.789, 59.246))
	);
	return p + warp * strength;
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

// Smooth minimum for blending shapes.
float smin(float a, float b, float k) {
	float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
	return mix(b, a, h) - k * h * (1.0 - h);
}

// Smooth maximum for blending shapes.
float smax(float a, float b, float k) {
	return -smin(-a, -b, k);
}

// Hash function for pseudo-random values.
float hash(float n) {
	return fract(sin(n) * 43758.5453123);
}

// 2D hash for texture-like patterns.
float hash2d(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

// 3D hash for volumetric patterns.
float hash3d(vec3 p) {
	return fract(sin(dot(p, vec3(127.1, 311.7, 74.7))) * 43758.5453);
}

// ============================================================================
// COLOR UTILITIES
// ============================================================================

// Attempt to convert temperature to RGB (blackbody approximation).
// More accurate than simple lookup; based on CIE color matching.
// @param temp_k: Temperature in Kelvin (1000-40000 reasonable range).
// @return: RGB color (not clamped, may need adjustment).
vec3 blackbody_color(float temp_k) {
	float t = temp_k / 100.0;
	vec3 color;

	// Red channel
	if (t <= 66.0) {
		color.r = 1.0;
	} else {
		float r = t - 60.0;
		color.r = clamp(1.29293618606 * pow(r, -0.1332047592), 0.0, 1.0);
	}

	// Green channel
	if (t <= 66.0) {
		float g = t - 2.0;
		color.g = clamp(0.3900815787 + 0.1302811719 * log(max(g, 1.0)) - 0.00025 * g, 0.0, 1.0);
	} else {
		float g = t - 60.0;
		color.g = clamp(1.12989086 * pow(g, -0.0755148492), 0.0, 1.0);
	}

	// Blue channel
	if (t >= 66.0) {
		color.b = 1.0;
	} else if (t <= 19.0) {
		color.b = 0.0;
	} else {
		float b = t - 10.0;
		color.b = clamp(-0.00559 + 0.13155 * log(b), 0.0, 1.0);
	}

	return color;
}

// Wavelength-dependent limb darkening for stars.
// Redder channels darken less at limb (realistic).
// @param base_color: Star's base color.
// @param mu: Cosine of angle from surface normal (0 at limb, 1 at center).
// @param u_ld: Limb darkening coefficient (0-1).
// @return: Darkened color.
vec3 limb_darken(vec3 base_color, float mu, float u_ld) {
	// Different darkening per channel (red darkens least)
	float lr = 1.0 - (u_ld * 0.85) * (1.0 - mu);
	float lg = 1.0 - u_ld * (1.0 - mu);
	float lb = 1.0 - (u_ld * 1.15) * (1.0 - mu);

	// Quadratic term for more realistic falloff
	float q = (1.0 - mu) * (1.0 - mu);
	lr *= 1.0 - 0.12 * q;
	lg *= 1.0 - 0.15 * q;
	lb *= 1.0 - 0.18 * q;

	return base_color * vec3(max(0.05, lr), max(0.05, lg), max(0.05, lb));
}

// ============================================================================
// ROTATION UTILITIES
// ============================================================================

// Rotates a point around an arbitrary axis using Rodrigues' rotation formula.
// @param p: Point to rotate.
// @param axis: Normalized rotation axis.
// @param angle: Rotation angle in radians.
// @return: Rotated point.
vec3 rotate_around_axis(vec3 p, vec3 axis, float angle) {
	float c = cos(angle);
	float s = sin(angle);
	vec3 k = normalize(axis);
	return p * c + cross(k, p) * s + k * dot(k, p) * (1.0 - c);
}

// Creates a rotation from axial tilt.
// Tilts around the Z axis (standard astronomical convention).
// @param p: Point on unit sphere.
// @param tilt_rad: Axial tilt in radians.
// @return: Tilted point.
vec3 apply_axial_tilt(vec3 p, float tilt_rad) {
	float c = cos(tilt_rad);
	float s = sin(tilt_rad);
	return vec3(
		p.x * c - p.y * s,
		p.x * s + p.y * c,
		p.z
	);
}

// Applies planetary rotation around Y axis (spin).
// @param p: Point on unit sphere.
// @param angle: Rotation angle in radians.
// @return: Rotated point.
vec3 apply_rotation(vec3 p, float angle) {
	float c = cos(angle);
	float s = sin(angle);
	return vec3(
		p.x * c - p.z * s,
		p.y,
		p.x * s + p.z * c
	);
}
