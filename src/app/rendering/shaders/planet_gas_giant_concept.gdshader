shader_type canvas_item;

// Gas giant planet visual generator (extracted from Concepts/planetgenerator.html).
// Use on a fullscreen ColorRect or rect; UV is 0..1.

uniform float u_time = 0.0;
uniform vec2 u_resolution = vec2(1920.0, 1080.0);
uniform float u_gBandCount = 14.0;
uniform float u_gBandContrast = 0.5;
uniform float u_gBandTurb = 0.6;
uniform float u_gFlowSpeed = 0.3;
uniform float u_gStormIntensity = 0.5;
uniform float u_gStormScale = 2.0;
uniform float u_gVortex = 0.7;
uniform float u_gOblateness = 0.06;
uniform int u_gFlowDetail = 5;
uniform float u_atmoDensity = 0.6;
uniform float u_atmoFalloff = 2.5;
uniform float u_scatterStrength = 0.8;
uniform float u_limbDark = 0.8;
uniform float u_terminatorSharp = 0.12;
uniform float u_lightX = 0.7;
uniform float u_lightY = 0.4;
uniform float u_ambient = 0.04;
uniform float u_rotSpeed = 0.12;
uniform float u_seed = 0.0;
uniform float u_axialTilt = 0.4;
uniform float u_starDensity = 1.0;
uniform float u_nebulaIntensity = 0.15;
uniform vec3 u_gColBandLight = vec3(0.91, 0.835, 0.627);
uniform vec3 u_gColBandDark = vec3(0.545, 0.42, 0.227);
uniform vec3 u_gColStorm = vec3(0.8, 0.4, 0.267);
uniform vec3 u_gColPolar = vec3(0.4, 0.533, 0.667);
uniform vec3 u_atmoColor = vec3(0.667, 0.733, 0.6);
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

// ---- Noise ----
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
	float ob = u_gOblateness;
	vec2 cObl = vec2(c.x, c.y / (1.0 - ob));
	float R = 0.38;
	float d = length(cObl);
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

	if (d > R + 0.1 && !inRingBounds) {
		COLOR = vec4(bg, 1.0);
		return;
	}
	vec3 color = bg;

	color = applyRings(color, c, d, R, u_ringType, u_ringNormal, u_ringInner, u_ringOuter, u_ringBands, u_ringColor1, u_ringColor2, u_ringColor3, u_ringDensity, u_ringGap, u_seed, L, false);

	if (d > R && d <= R + 0.1) {
		float ae = smoothstep(R + 0.1 * u_atmoDensity, R, d);
		float gw = pow(ae, u_atmoFalloff) * u_atmoDensity;
		vec3 scatterCol = mix(u_atmoColor, u_atmoColor * vec3(0.8, 0.9, 1.15), u_scatterStrength * 0.4);
		float sunGlow = max(0.0, dot(normalize(vec3(c, 0.0)), L)) * 0.35 + 0.65;
		color = mix(color, scatterCol * sunGlow, gw * 0.85);
	}

	if (d <= R) {
		float nd = d / R;
		float z = sqrt(1.0 - nd * nd);
		vec3 N = normalize(vec3(cObl / R, z));
		vec3 Nlight = normalize(vec3(c.x / R, c.y / ((1.0 - ob) * R), z));

		float ang = u_time * u_rotSpeed;
		float ca = cos(ang);
		float sa = sin(ang);
		vec3 k = normalize(u_ringNormal);
		vec3 kxN = cross(k, N);
		float kdotN = dot(k, N);
		vec3 sp = N * ca + kxN * sa + k * kdotN * (1.0 - ca);

		vec3 so = vec3(u_seed * 13.7, u_seed * 7.3, u_seed * 19.1);

		float lat = dot(sp, k);
		float absLat = abs(lat);
		float lon = atan(sp.x, sp.z);
		float bandLat = asin(clamp(lat, -1.0, 1.0)) * 0.63662;
		float bandPhase = bandLat * u_gBandCount * 3.14159;
		float h1 = sin(bandPhase);
		float h2 = sin(bandPhase * 1.618 + 1.7) * 0.35;
		float h3 = sin(bandPhase * 2.414 - 0.9) * 0.15;
		float bandBase = h1 + h2 + h3;
		float flowDir = sin(bandPhase);
		float flowTime = u_time * u_gFlowSpeed;
		float jetMag = 1.0 + cos(bandPhase) * 0.35;
		float localFlow = flowTime * flowDir * jetMag;

		int turb2Oct = u_gFlowDetail - 2;
		if (turb2Oct < 1) { turb2Oct = 1; }
		float turb1 = fbm(vec3(lon * 2.8 + localFlow * 0.75, bandLat * u_gBandCount * 0.75, sp.z * 2.5 + so.x), u_gFlowDetail, 0.55);
		float turb2 = fbm(vec3(lon * 5.5 + localFlow * 1.05, bandLat * 5.0, sp.x * 3.5 + so.y), turb2Oct, 0.52);
		float turbulence = (turb1 * 0.65 + turb2 * 0.35) * u_gBandTurb;
		float bandPerturbed = bandBase + turbulence * 2.2;
		float bandVal = clamp(bandPerturbed * 0.5 + 0.5, 0.0, 1.0);
		float latTemp = 1.0 - absLat * 0.55;
		vec3 zoneCol = mix(u_gColPolar * 0.92 + vec3(0.015), u_gColBandLight * 1.04, latTemp * 0.85);
		zoneCol += vec3(0.015, 0.01, -0.008) * latTemp;
		vec3 beltCol = mix(u_gColPolar * 0.72, u_gColBandDark * 1.06, latTemp * 0.82);
		beltCol += vec3(0.035, 0.018, -0.025) * latTemp * u_gBandContrast;
		float zoneBelt = smoothstep(0.38, 0.62, bandVal);
		vec3 bandColor = mix(beltCol, zoneCol, zoneBelt);
		float streakFreq = u_gBandCount * 5.5;
		float streakDisp = snoise(vec3(lon * 11.0 + localFlow * 1.6, bandLat * streakFreq + 0.4, so.z * 1.5)) * 0.5 + 0.5;
		float streak = sin(bandLat * streakFreq + streakDisp * 0.5) * 0.5 + 0.5;
		streak = smoothstep(0.4, 0.6, streak);
		bandColor *= 1.0 + ((streak - 0.5) * 0.11 * u_gBandContrast);
		float bdryProx = 1.0 - abs(zoneBelt - 0.5) * 2.0;
		float waveNum = 6.0 + u_gBandCount * 0.4;
		float rWave = sin(lon * waveNum + bandPhase * 0.5 + localFlow * 0.6) * 0.5 + 0.5;
		float waveAmp = bdryProx * rWave * 0.1 * u_gBandTurb;
		bandColor = mix(bandColor, zoneCol * 0.92 + beltCol * 0.08, waveAmp);

		for (int i = 0; i < 3; i++) {
			float fi = float(i);
			float sLat = sin(u_seed * 7.3 + fi * 2.1) * 0.38;
			float sLonBase = u_seed * 4.1 + fi * 3.7;
			float sFlowDir = sin(sLat * u_gBandCount * 3.14159);
			float sLon = sLonBase + flowTime * 0.12 * sFlowDir;
			float dLat = lat - sLat;
			float dLon = lon - sLon;
			dLon = dLon - round(dLon / 6.28318) * 6.28318;
			float stW = u_gStormScale * 0.12 * (1.0 - 0.18 * fi);
			float stH = stW * 0.45;
			float sDist = sqrt((dLat / stH) * (dLat / stH) + (dLon / stW) * (dLon / stW));
			if (sDist < 2.5) {
				float sAngle = atan(dLat / stH, dLon / stW);
				float spiral = sAngle + sDist * u_gVortex * 3.14159;
				float ring2 = sin(sDist * 6.5 - u_time * 0.4 * u_gFlowSpeed) * 0.5 + 0.5;
				float sNoise = fbm(vec3(cos(spiral) * sDist * 4.5 + so.x * 2.0 + fi * 7.0, sin(spiral) * sDist * 4.5, so.z + fi * 3.5), 3, 0.55) * 0.5 + 0.5;
				float stormTex = ring2 * 0.3 + sNoise * 0.7;
				float corePull = smoothstep(1.8, 0.25, sDist);
				vec3 stormCol = mix(bandColor, u_gColStorm, corePull * 0.72);
				stormCol += vec3(0.02, 0.012, -0.008) * corePull * u_gStormIntensity;
				stormCol = mix(stormCol, stormCol * (1.0 + (stormTex - 0.5) * 0.22), 0.6);
				float eyeWall = smoothstep(0.75, 0.35, sDist) * smoothstep(0.12, 0.5, sDist);
				stormCol = mix(stormCol, zoneCol * 0.88 + vec3(0.025, 0.015, 0.0), eyeWall * 0.28);
				float sMask = smoothstep(2.5, 0.7, sDist) * u_gStormIntensity * (1.0 - 0.15 * fi);
				bandColor = mix(bandColor, stormCol, sMask);
			}
		}

		float eddyN = fbm(sp * u_gStormScale * 2.5 + so * 0.35, 3, 0.52);
		float eddyAng = eddyN * u_gVortex * 6.28;
		vec3 eddyP = sp + vec3(cos(eddyAng), 0.0, sin(eddyAng)) * 0.04 * u_gVortex;
		float eddyDet = fbm(eddyP * u_gStormScale * 3.0 + so * 0.7, 3, 0.52);
		float eddyMask = smoothstep(0.35, 0.6, eddyN) * smoothstep(0.4, 0.65, eddyDet) * u_gStormIntensity * 0.3 * (bdryProx * bdryProx);
		vec3 eddyCol = mix(beltCol, zoneCol, 0.45);
		bandColor = mix(bandColor, eddyCol, eddyMask);

		float polarFade = smoothstep(0.55, 0.88, absLat);
		float pAngle = atan(sp.x, sp.z) + flowTime * 0.12;
		float pDist = 1.0 - absLat;
		float pSwirl = fbm(vec3(cos(pAngle * 4.0) * pDist * 9.0 + flowTime * 0.18, sin(pAngle * 4.0) * pDist * 9.0, so.x * 3.0 + sp.y * 6.0), 4, 0.55) * 0.5 + 0.5;
		float pRing = sin(pDist * 14.0 - flowTime * 0.25) * 0.5 + 0.5;
		float pTex = pSwirl * 0.55 + pRing * 0.45;
		vec3 polCol = mix(u_gColPolar * 0.82, u_gColPolar * 1.18, pTex);
		polCol += vec3(0.008, 0.018, 0.04) * smoothstep(0.72, 0.95, absLat);
		bandColor = mix(bandColor, polCol, polarFade);

		float NdL = dot(Nlight, L);
		float terminator = smoothstep(-u_terminatorSharp, u_terminatorSharp, NdL);
		float diff = max(NdL, 0.0);
		float wrap = max(NdL * 0.5 + 0.5, 0.0);
		float light = mix(diff, wrap, 0.38) * terminator + u_ambient * (1.0 - terminator * 0.5);

		float limbDark = pow(z, u_limbDark * 0.6 + 0.4);
		limbDark = mix(0.6, 1.0, limbDark);

		vec3 lit = bandColor * (light + u_ambient) * limbDark;
		vec3 H = normalize(L + vec3(0.0, 0.0, 1.0));
		float spec = pow(max(dot(Nlight, H), 0.0), 28.0) * 0.22;
		lit += vec3(1.0) * spec * diff * 0.4;

		float rim = 1.0 - z;
		float aR = pow(rim, u_atmoFalloff) * u_atmoDensity;
		vec3 rimCol = mix(u_atmoColor, u_atmoColor * vec3(0.85, 0.92, 1.1), rim * u_scatterStrength);
		rimCol *= (diff + 0.3);
		lit = mix(lit, rimCol, aR * 0.6);

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
							color *= (1.0 - sA * 0.5);
						}
					}
				}
			}
		}
	}

	color = applyRings(color, c, d, R, u_ringType, u_ringNormal, u_ringInner, u_ringOuter, u_ringBands, u_ringColor1, u_ringColor2, u_ringColor3, u_ringDensity, u_ringGap, u_seed, L, true);

	COLOR = vec4(color, 1.0);
}
