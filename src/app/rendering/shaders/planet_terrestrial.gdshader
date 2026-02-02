shader_type canvas_item;

// Terrestrial planet visual generator (extracted from Concepts/planetgenerator.html).
// Use on a fullscreen ColorRect or rect; UV is 0..1.

uniform float u_time = 0.0;
uniform vec2 u_resolution = vec2(1920.0, 1080.0);
uniform float u_terrainScale = 4.0;
uniform float u_terrainHeight = 0.5;
uniform float u_roughness = 0.55;
uniform float u_continentSize = 2.0;
uniform int u_octaves = 6;
uniform float u_seaLevel = 0.4;
uniform float u_seaSpecular = 0.8;
uniform float u_iceCap = 0.3;
uniform float u_fresnelStrength = 1.0;
uniform float u_atmoDensity = 0.6;
uniform float u_atmoFalloff = 2.5;
uniform float u_cloudCoverage = 0.4;
uniform float u_cloudScale = 3.5;
uniform float u_cloudShadow = 0.3;
uniform float u_scatterStrength = 0.8;
uniform float u_limbDark = 0.8;
uniform float u_terminatorSharp = 0.12;
uniform float u_lightX = 0.7;
uniform float u_lightY = 0.4;
uniform float u_ambient = 0.04;
uniform float u_rotSpeed = 0.06;
uniform float u_cloudDrift = 0.03;
uniform float u_seed = 0.0;
uniform float u_axialTilt = 0.4;
uniform float u_starDensity = 1.0;
uniform float u_nebulaIntensity = 0.15;
uniform vec3 u_colLow = vec3(0.165, 0.29, 0.102);
uniform vec3 u_colMid = vec3(0.353, 0.541, 0.227);
uniform vec3 u_colHigh = vec3(0.541, 0.478, 0.353);
uniform vec3 u_colPeak = vec3(0.8, 0.733, 0.667);
uniform vec3 u_colSeaShallow = vec3(0.133, 0.533, 0.733);
uniform vec3 u_colSeaDeep = vec3(0.039, 0.133, 0.267);
uniform vec3 u_colIce = vec3(0.867, 0.933, 1.0);
uniform vec3 u_atmoColor = vec3(0.267, 0.533, 0.8);
uniform vec3 u_cloudColor = vec3(1.0, 1.0, 1.0);
uniform int u_ringType = 0;
uniform int u_ringBands = 5;
uniform float u_ringInner = 1.4;
uniform float u_ringOuter = 2.6;
uniform float u_ringDensity = 0.65;
uniform float u_ringGap = 0.15;
uniform vec3 u_ringColor1 = vec3(0.8, 0.6, 0.4);
uniform vec3 u_ringColor2 = vec3(0.733, 0.667, 0.533);
uniform vec3 u_ringColor3 = vec3(0.533, 0.6, 0.667);
uniform vec3 u_ringNormal = vec3(0.0, 1.0, 0.12);

// ---- Noise (from NOISE_GLSL) ----
vec3 mod289_3(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec4 mod289_4(vec4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec4 permute(vec4 x) { return mod289_4(((x * 34.0) + 1.0) * x); }
vec4 taylorInvSqrt(vec4 r) { return 1.79284291400159 - 0.85373472095314 * r; }

float snoise(vec3 v) {
	vec2 C = vec2(1.0 / 6.0, 1.0 / 3.0);
	vec4 D = vec4(0.0, 0.5, 1.0, 2.0);
	vec3 i = floor(v + dot(v, vec3(C.y, C.y, C.y)));
	vec3 x0 = v - i + dot(i, vec3(C.x, C.x, C.x));
	vec3 g = step(x0.yzx, x0.xyz);
	vec3 l = 1.0 - g;
	vec3 i1 = min(g, l.zxy);
	vec3 i2 = max(g, l.zxy);
	vec3 x1 = x0 - i1 + vec3(C.x, C.x, C.x);
	vec3 x2 = x0 - i2 + vec3(C.y, C.y, C.y);
	vec3 x3 = x0 - vec3(D.y, D.y, D.y);
	i = mod289_3(i);
	vec4 p = permute(permute(permute(
		i.z + vec4(0.0, i1.z, i2.z, 1.0))
		+ i.y + vec4(0.0, i1.y, i2.y, 1.0))
		+ i.x + vec4(0.0, i1.x, i2.x, 1.0));
	float n_ = 0.142857142857;
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
	vec4 norm = taylorInvSqrt(vec4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;
	vec4 m = max(0.6 - vec4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), vec4(0.0));
	m = m * m;
	return 42.0 * dot(m * m, vec4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
}

float fbm(vec3 p, int octs, float rough) {
	float v = 0.0;
	float a = 0.5;
	float f = 1.0;
	float s = 0.0;
	for (int i = 0; i < 8; i++) {
		if (i >= octs) { break; }
		v += a * snoise(p * f);
		s += a;
		a *= rough;
		f *= 2.0;
	}
	return v / s;
}

float wfbm(vec3 p, int o, float r) {
	vec3 q = vec3(fbm(p, o, r), fbm(p + vec3(5.2, 1.3, 2.8), o, r), fbm(p + vec3(1.7, 9.2, 4.1), o, r));
	return fbm(p + 1.5 * q, o, r);
}

vec3 starField(vec2 uv, float density, float seed) {
	vec3 col = vec3(0.008, 0.01, 0.018);
	for (int l = 0; l < 3; l++) {
		float layerScale = 200.0;
		if (l == 1) { layerScale = 400.0; }
		if (l == 2) { layerScale = 800.0; }
		float scale = layerScale * density;
		vec2 gv = fract(uv * scale) - 0.5;
		vec2 id = floor(uv * scale);
		float n = fract(sin(dot(id, vec2(12.9898 + seed, 78.233 + seed * 2.0))) * 43758.5453);
		float n2 = fract(sin(dot(id, vec2(39.346 + seed, 11.135 + seed * 3.0))) * 22578.5453);
		if (n > 0.97 - density * 0.02) {
			float sz = 1.2;
			if (l == 1) { sz = 0.8; }
			if (l == 2) { sz = 0.5; }
			float brightness = pow(n2, 2.0) * sz;
			float star = exp(-length(gv) * (40.0 / sz)) * brightness;
			vec3 starCol = vec3(1.0);
			if (n2 < 0.3) { starCol = vec3(1.0, 0.85, 0.7); }
			if (n2 > 0.7) { starCol = vec3(0.7, 0.85, 1.0); }
			col += starCol * star;
		}
	}
	return col;
}

vec3 nebula(vec2 uv, float intensity, float seed) {
	if (intensity < 0.01) { return vec3(0.0); }
	float n1 = fbm(vec3(uv * 2.0, seed), 4, 0.5);
	float n2 = fbm(vec3(uv * 3.0 + 10.0, seed * 2.0), 4, 0.5);
	vec3 col1 = vec3(0.1, 0.05, 0.15) * smoothstep(0.2, 0.8, n1);
	vec3 col2 = vec3(0.05, 0.1, 0.15) * smoothstep(0.3, 0.9, n2);
	return (col1 + col2) * intensity * 0.5;
}

// ---- Rings (from RING_GLSL) ----
float ringProfile(float t, int rType, int numBands, float gapSize, float seed) {
	if (rType < 1) { return 0.0; }
	float bands = float(numBands);
	float v = 0.0;
	for (int i = 0; i < 12; i++) {
		if (i >= numBands) { break; }
		float fi = float(i);
		float h1 = fract(sin(fi * 127.1 + seed * 311.7) * 43758.5453);
		float h2 = fract(sin(fi * 269.5 + seed * 183.3) * 22578.5453);
		float h3 = fract(sin(fi * 71.3 + seed * 53.1) * 12345.6789);
		float baseCenter = (fi + 0.5) / bands;
		float centerOffset = (h1 - 0.5) * 0.3 / bands;
		float bandCenter = clamp(baseCenter + centerOffset, fi / bands + 0.02, (fi + 1.0) / bands - 0.02);
		float widthVar = 0.3 + h2 * 0.7;
		float baseWidth = (1.0 / bands) * (1.0 - gapSize) * 0.45;
		float bandWidth = baseWidth * widthVar;
		float strengthVar = 0.5 + h3 * 0.5;
		float bandStrength = strengthVar * (1.0 - 0.12 * fi / bands);
		float d = abs(t - bandCenter) / max(bandWidth, 0.005);
		float sharpness = 1.2 + 1.5 * (1.0 - widthVar);
		float profile = exp(-d * d * sharpness);
		float n = snoise(vec3(t * 22.0 + seed * 2.0 + fi * 3.7, seed * 5.0 + fi * 1.3, fi * 0.5)) * 0.18;
		v = max(v, profile * (bandStrength + n));
	}
	float innerEdge = smoothstep(0.0, 0.015, t);
	float outerEdge = smoothstep(1.0, 0.97, t);
	return clamp(v * innerEdge * outerEdge, 0.0, 1.0);
}

vec3 applyRings(vec3 col, vec2 c, float dP, float R, int rType, vec3 ringNormal,
	float rInner, float rOuter, int numBands, vec3 rC1, vec3 rC2, vec3 rC3,
	float rDens, float gapSize, float seed, vec3 L, bool frontPass) {
	if (rType < 1) { return col; }
	float rI = R * rInner;
	float rO = R * rOuter;
	vec3 Nrp = normalize(ringNormal);
	vec3 rayDir = vec3(0.0, 0.0, 1.0);
	float denom = dot(Nrp, rayDir);
	if (abs(denom) < 0.005) { return col; }
	float Zcross = -(Nrp.x * c.x + Nrp.y * c.y) / Nrp.z;
	vec3 hitPt = vec3(c.x, c.y, Zcross);
	float rDist = length(hitPt);
	if (rDist < rI || rDist > rO) { return col; }
	bool ringBehind = (Zcross < 0.0);
	if (frontPass != (!ringBehind)) { return col; }
	if (ringBehind && dP <= R) { return col; }
	float t = (rDist - rI) / (rO - rI);
	float rA = ringProfile(t, rType, numBands, gapSize, seed) * rDens;
	if (rA < 0.005) { return col; }
	vec3 rC;
	if (t < 0.5) { rC = mix(rC1, rC2, t * 2.0); } else { rC = mix(rC2, rC3, (t - 0.5) * 2.0); }
	float nv = snoise(vec3(rDist * 38.0 + seed * 1.3, atan(hitPt.y, hitPt.x) * 4.0, seed * 3.0)) * 0.12;
	rC *= (1.0 + nv);
	float rLight = abs(dot(Nrp, L)) * 0.55 + 0.45;
	rC *= rLight;
	float shadow = 1.0;
	if (ringBehind) {
		vec3 oc = hitPt;
		float b = dot(oc, L);
		float c2 = dot(oc, oc) - R * R;
		float disc = b * b - c2;
		if (disc > 0.0 && b < 0.0) { shadow = 0.12; }
	}
	rC *= shadow;
	float alphaMul = 0.88;
	if (ringBehind) { alphaMul = 0.72; }
	float alpha = rA * alphaMul;
	return mix(col, rC, alpha);
}

void fragment() {
	vec2 uv = UV;
	float asp = u_resolution.x / u_resolution.y;
	vec2 c = (uv - 0.5) * vec2(asp, 1.0);
	float R = 0.38;
	float d = length(c);
	vec3 L = normalize(vec3(u_lightX, u_lightY, 0.6));

	vec3 bg = starField(uv, u_starDensity, u_seed);
	bg += nebula(uv, u_nebulaIntensity, u_seed);

	float rO = R * u_ringOuter;
	bool inRingBounds = false;
	if (u_ringType > 0) {
		vec3 Nrp = normalize(u_ringNormal);
		if (abs(Nrp.z) > 0.005) {
			float Zc = -(Nrp.x * c.x + Nrp.y * c.y) / Nrp.z;
			float rd = length(vec3(c.x, c.y, Zc));
			inRingBounds = rd < rO;
		}
	}

	if (d > R + 0.08 && !inRingBounds) {
		COLOR = vec4(bg, 1.0);
		return;
	}
	vec3 color = bg;

	color = applyRings(color, c, d, R, u_ringType, u_ringNormal, u_ringInner, u_ringOuter, u_ringBands, u_ringColor1, u_ringColor2, u_ringColor3, u_ringDensity, u_ringGap, u_seed, L, false);

	if (d > R && d <= R + 0.08) {
		float ae = smoothstep(R + 0.08 * u_atmoDensity, R, d);
		float gw = pow(ae, u_atmoFalloff) * u_atmoDensity;
		vec3 scatterCol = mix(u_atmoColor, u_atmoColor * vec3(0.7, 0.85, 1.2), u_scatterStrength * 0.5);
		float sunGlow = max(0.0, dot(normalize(vec3(c, 0.0)), L)) * 0.3 + 0.7;
		color = mix(color, scatterCol * sunGlow, gw * 0.8);
	}

	if (d <= R) {
		float nd = d / R;
		float z = sqrt(1.0 - nd * nd);
		vec3 N = normalize(vec3(c / R, z));

		float ang = u_time * u_rotSpeed;
		float ca = cos(ang);
		float sa = sin(ang);
		vec3 k = normalize(u_ringNormal);
		vec3 kxN = cross(k, N);
		float kdotN = dot(k, N);
		vec3 sp = N * ca + kxN * sa + k * kdotN * (1.0 - ca);

		vec3 so = vec3(u_seed * 13.7, u_seed * 7.3, u_seed * 19.1);
		float cont = fbm(sp * u_continentSize + so, 3, 0.5);
		float det = wfbm(sp * u_terrainScale + so, u_octaves, u_roughness);
		float ter = mix(cont, det, 0.6) * 0.5 + 0.5;
		ter = pow(ter, 1.0 + (1.0 - u_terrainHeight));

		float NdL = dot(N, L);
		float terminator = smoothstep(-u_terminatorSharp, u_terminatorSharp, NdL);
		float diff = max(NdL, 0.0);
		float wrap = max(NdL * 0.5 + 0.5, 0.0);
		float light = mix(diff, wrap, 0.3) * terminator + u_ambient * (1.0 - terminator * 0.5);

		vec3 V = vec3(0.0, 0.0, 1.0);
		vec3 H = normalize(L + V);
		float spec = pow(max(dot(N, H), 0.0), 80.0);

		float sl = u_seaLevel;
		bool isOc = ter < sl;
		vec3 sCol;
		float sSpec = 0.0;

		if (isOc) {
			float dp = smoothstep(sl, sl - 0.25, ter);
			vec3 deepC = u_colSeaDeep * vec3(0.7, 0.85, 1.0);
			vec3 shallowC = u_colSeaShallow;
			sCol = mix(shallowC, deepC, dp);
			float waveT = u_time * 0.8;
			float w1 = snoise(sp * 12.0 + vec3(waveT * 0.7, waveT * 0.5, 0.0)) * 0.5 + 0.5;
			float w2 = snoise(sp * 24.0 + vec3(-waveT * 0.5, waveT * 0.8, waveT * 0.3)) * 0.5 + 0.5;
			float w3 = snoise(sp * 48.0 + vec3(waveT * 0.3, -waveT * 0.6, waveT * 0.4)) * 0.5 + 0.5;
			float wave = w1 * 0.5 + w2 * 0.35 + w3 * 0.15;
			float depthFade = 1.0 - dp * 0.6;
			sCol += vec3(0.04, 0.06, 0.08) * wave * depthFade;
			sCol = mix(sCol, shallowC * 1.15, wave * 0.18 * depthFade);
			vec3 waveN = normalize(N + vec3(w1 - 0.5, w2 - 0.5, 0.0) * 0.08);
			float fresnel = pow(1.0 - max(dot(waveN, V), 0.0), 5.0) * u_fresnelStrength;
			vec3 reflectCol = mix(u_atmoColor, vec3(0.9, 0.95, 1.0), 0.5);
			sCol = mix(sCol, reflectCol, fresnel * 0.4);
			vec3 R_ = reflect(-L, waveN);
			float specW = pow(max(dot(R_, V), 0.0), 120.0) * u_seaSpecular;
			float specB = pow(max(dot(R_, V), 0.0), 20.0) * u_seaSpecular * 0.3;
			sSpec = specW + specB;
			float foam = smoothstep(0.35, 0.42, ter / sl) * smoothstep(sl, sl - 0.04, ter);
			foam *= smoothstep(0.55, 0.75, w1 * w2) * 2.0;
			sCol = mix(sCol, vec3(0.85, 0.9, 0.95), foam * 0.25);
		} else {
			float t2 = (ter - sl) / (1.0 - sl);
			float t2s = smoothstep(0.0, 1.0, t2);
			if (t2s < 0.25) { sCol = mix(u_colLow, u_colMid, t2s / 0.25); }
			else if (t2s < 0.55) { sCol = mix(u_colMid, u_colHigh, (t2s - 0.25) / 0.3); }
			else { sCol = mix(u_colHigh, u_colPeak, (t2s - 0.55) / 0.45); }
			float terrainNoise = snoise(sp * 20.0 + so) * 0.08;
			sCol *= (1.0 + terrainNoise);
			sSpec = 0.05;
		}

		float lat = abs(dot(sp, normalize(u_ringNormal)));
		float iceS = 1.0 - u_iceCap;
		float n1 = snoise(sp * 4.0 + so) * 0.18;
		float n2 = snoise(sp * 8.0 + so * 1.3) * 0.12;
		float n3 = snoise(sp * 16.0 + so * 2.7) * 0.06;
		float n4 = snoise(sp * 32.0 + so * 4.1) * 0.03;
		float pLat = lat + n1 + n2 + n3 + n4;
		float rn = abs(snoise(sp * 6.0 + so * 0.7));
		float gr = rn * 0.1 * u_iceCap;
		float iT = iceS - gr;
		float iF = smoothstep(iT - 0.02, iT + 0.06, pLat);
		if (!isOc) {
			float t2_land = (ter - sl) / (1.0 - sl);
			iF = max(iF, smoothstep(0.7, 0.95, t2_land) * smoothstep(iceS - 0.35, iceS - 0.05, lat) * u_iceCap * 0.5);
		}
		iF = clamp(iF, 0.0, 1.0);
		float id = snoise(sp * 20.0 + so) * 0.08;
		sCol = mix(sCol, u_colIce * (1.0 + id), iF);
		sSpec = mix(sSpec, 0.4, iF);

		vec3 lit = sCol * (light + u_ambient) + vec3(1.0) * spec * sSpec * diff;

		float cAng = u_time * (u_rotSpeed + u_cloudDrift);
		float cca = cos(cAng);
		float csa = sin(cAng);
		vec3 k2 = normalize(u_ringNormal);
		vec3 kxN2 = cross(k2, N);
		float kdotN2 = dot(k2, N);
		vec3 cp = N * cca + kxN2 * csa + k2 * kdotN2 * (1.0 - cca);
		float cl = fbm(cp * u_cloudScale + so * 0.5, 5, 0.5);
		cl = smoothstep(0.3 - u_cloudCoverage * 0.5, 0.6, cl) * u_cloudCoverage;
		vec3 shadowOffset = L * 0.02;
		vec3 shadowSample = cp + shadowOffset;
		float shadowCloud = fbm(shadowSample * u_cloudScale + so * 0.5, 5, 0.5);
		shadowCloud = smoothstep(0.3 - u_cloudCoverage * 0.5, 0.6, shadowCloud) * u_cloudCoverage;
		lit *= (1.0 - shadowCloud * u_cloudShadow * 0.4);
		lit *= (1.0 - cl * 0.2);
		lit = mix(lit, u_cloudColor * (light + u_ambient * 2.0), cl * 0.8);

		float rim = 1.0 - z;
		float aR = pow(rim, u_atmoFalloff) * u_atmoDensity;
		vec3 scatterColor = mix(u_atmoColor, u_atmoColor * vec3(0.6, 0.8, 1.3), rim * u_scatterStrength);
		lit = mix(lit, scatterColor, aR * 0.6 * (diff + 0.3));

		float limbFactor = pow(z, u_limbDark * 0.5 + 0.5);
		lit *= mix(0.7, 1.0, limbFactor);

		float nM = smoothstep(0.05, -0.1, NdL);
		float ciN = snoise(sp * 40.0 + so);
		float cities = smoothstep(0.75, 0.95, ciN) * nM * 0.15;
		if (!isOc) { lit += vec3(1.0, 0.85, 0.5) * cities; }

		lit *= terminator * (1.0 - u_ambient) + u_ambient;
		color = lit;

		if (u_ringType > 0) {
			vec3 Nrp = normalize(u_ringNormal);
			if (abs(Nrp.z) > 0.005) {
				vec3 surfPt = vec3(c.x, c.y, z * R);
				float denom2 = dot(Nrp, L);
				if (abs(denom2) > 0.01) {
					float tHit = -dot(Nrp, surfPt) / denom2;
					if (tHit > 0.01) {
						vec3 ringHit = surfPt + L * tHit;
						float rdist2 = length(ringHit);
						float rI2 = R * u_ringInner;
						float rO2 = R * u_ringOuter;
						if (rdist2 > rI2 && rdist2 < rO2) {
							float t2 = (rdist2 - rI2) / (rO2 - rI2);
							float sA = ringProfile(t2, u_ringType, u_ringBands, u_ringGap, u_seed) * u_ringDensity;
							color *= (1.0 - sA * 0.45);
						}
					}
				}
			}
		}
	}

	color = applyRings(color, c, d, R, u_ringType, u_ringNormal, u_ringInner, u_ringOuter, u_ringBands, u_ringColor1, u_ringColor2, u_ringColor3, u_ringDensity, u_ringGap, u_seed, L, true);

	COLOR = vec4(color, 1.0);
}
