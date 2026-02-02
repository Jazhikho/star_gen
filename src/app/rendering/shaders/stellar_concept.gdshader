shader_type canvas_item;

// Stellar / remnant visual generator (extracted from Concepts/stargenerator.html).
// Use on a fullscreen ColorRect or rect; UV is 0..1. Drive uniforms from domain or UI.

uniform float u_time = 0.0;
uniform vec2 u_resolution = vec2(1920.0, 1080.0);
uniform float u_temperature = 5778.0;
uniform float u_radius = 1.0;
uniform float u_luminosity = 1.0;
uniform float u_limbDark = 0.6;
uniform float u_granScale = 30.0;
uniform float u_granContrast = 0.35;
uniform float u_granTurb = 0.4;
uniform float u_granFlow = 0.08;
uniform float u_spotCount = 5.0;
uniform float u_spotSize = 0.06;
uniform float u_penumbra = 2.0;
uniform float u_spotDark = 0.35;
uniform float u_chromoThick = 0.015;
uniform float u_chromoIntensity = 0.8;
uniform float u_chromoShift = 0.5;
uniform float u_coronaExtent = 0.3;
uniform float u_coronaBright = 0.5;
uniform float u_coronaStreams = 8.0;
uniform float u_coronaLength = 0.5;
uniform float u_coronaAsym = 0.3;
uniform float u_promCount = 3.0;
uniform float u_promHeight = 0.12;
uniform float u_promGlow = 0.8;
uniform float u_flareIntensity = 0.2;
uniform float u_superGranScale = 6.0;
uniform float u_superGranStr = 0.15;
uniform float u_rotSpeed = 0.05;
uniform float u_bloomRadius = 0.15;
uniform float u_bloomIntensity = 0.6;
uniform float u_spikeCount = 4.0;
uniform float u_spikeLength = 0.3;
uniform float u_spikeBright = 0.25;
uniform float u_starDensity = 1.0;
uniform float u_nebulaIntensity = 0.1;
uniform float u_seed = 0.0;
uniform float u_zoom = 1.0;
uniform float u_accretionRate = 0.0;
uniform float u_diskTilt = 60.0;
uniform float u_bhSpin = 0.8;
uniform float u_jetPower = 0.0;
uniform float u_lensing = 0.0;

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

vec2 voronoi(vec3 p) {
	vec3 ip = floor(p);
	vec3 fp = fract(p);
	float dMin = 10.0;
	float dMin2 = 10.0;
	for (int x = -1; x <= 1; x++) {
		for (int y = -1; y <= 1; y++) {
			for (int z = -1; z <= 1; z++) {
				vec3 nb = vec3(float(x), float(y), float(z));
				vec3 off = nb + vec3(
					fract(sin(dot(ip + nb, vec3(127.1, 311.7, 74.7))) * 43758.5453),
					fract(sin(dot(ip + nb, vec3(269.5, 183.3, 246.1))) * 22578.5453),
					fract(sin(dot(ip + nb, vec3(419.2, 371.9, 128.5))) * 12345.6789)
				) - fp;
				float dd = dot(off, off);
				if (dd < dMin) {
					dMin2 = dMin;
					dMin = dd;
				} else if (dd < dMin2) {
					dMin2 = dd;
				}
			}
		}
	}
	return vec2(sqrt(dMin), sqrt(dMin2));
}

vec3 tempToRGB(float T) {
	float t = T / 100.0;
	vec3 c;
	if (t <= 66.0) { c.r = 1.0; } else { float r = t - 60.0; c.r = clamp(1.29293618606 * pow(r, -0.1332047592), 0.0, 1.0); }
	if (t <= 66.0) { float g = t - 2.0; c.g = clamp(0.3900815787 + 0.1302811719 * log(max(g, 1.0)) - 0.00025 * g, 0.0, 1.0); } else { float g = t - 60.0; c.g = clamp(1.12989086 * pow(g, -0.0755148492), 0.0, 1.0); }
	if (t >= 66.0) { c.b = 1.0; } else if (t <= 19.0) { c.b = 0.0; } else { float b = t - 10.0; c.b = clamp(-0.00559 + 0.13155 * log(b), 0.0, 1.0); }
	return c;
}

vec3 limbDarkenColor(vec3 baseCol, float mu, float u_ld) {
	float lr = 1.0 - (u_ld * 0.85) * (1.0 - mu);
	float lg = 1.0 - u_ld * (1.0 - mu);
	float lb = 1.0 - (u_ld * 1.15) * (1.0 - mu);
	float q = (1.0 - mu) * (1.0 - mu);
	lr *= 1.0 - 0.12 * q;
	lg *= 1.0 - 0.15 * q;
	lb *= 1.0 - 0.18 * q;
	return baseCol * vec3(max(0.05, lr), max(0.05, lg), max(0.05, lb));
}

vec3 starField(vec2 uv, float density, float seed) {
	vec3 col = vec3(0.005, 0.007, 0.015);
	for (int l = 0; l < 3; l++) {
		float baseScale = 200.0;
		if (l == 1) { baseScale = 400.0; }
		if (l == 2) { baseScale = 800.0; }
		float sc = baseScale * density;
		vec2 gv = fract(uv * sc) - 0.5;
		vec2 id = floor(uv * sc);
		float n = fract(sin(dot(id, vec2(12.99 + seed, 78.23 + seed * 2.0))) * 43758.5);
		float n2 = fract(sin(dot(id, vec2(39.35 + seed, 11.14 + seed * 3.0))) * 22578.5);
		if (n > 0.97 - density * 0.02) {
			float br_mult = 1.2;
			if (l == 1) { br_mult = 0.8; }
			if (l == 2) { br_mult = 0.5; }
			float br = pow(n2, 2.0) * br_mult;
			float star_sz = 33.0;
			if (l == 1) { star_sz = 40.0; }
			if (l == 2) { star_sz = 50.0; }
			float star = exp(-length(gv) * star_sz) * br;
			vec3 sc2 = vec3(1.0);
			if (n2 < 0.3) { sc2 = vec3(1.0, 0.85, 0.7); }
			if (n2 > 0.7) { sc2 = vec3(0.7, 0.85, 1.0); }
			col += sc2 * star;
		}
	}
	return col;
}

vec3 nebulaBg(vec2 uv, float intensity, float seed) {
	if (intensity < 0.01) { return vec3(0.0); }
	float n1 = fbm(vec3(uv * 2.0, seed), 4, 0.5);
	float n2 = fbm(vec3(uv * 3.0 + 10.0, seed * 2.0), 4, 0.5);
	return (vec3(0.1, 0.04, 0.12) * smoothstep(0.2, 0.8, n1) + vec3(0.04, 0.08, 0.12) * smoothstep(0.3, 0.9, n2)) * intensity * 0.5;
}

vec3 lensedBg(vec2 c, float R, float lensStr, float sd, float ni, float seed) {
	float d = length(c);
	vec2 uv = c;
	if (lensStr > 0.01 && d > R * 0.5) {
		float rs = R * 1.5 * lensStr;
		float bendFactor = rs * rs / (d * d + rs * 0.1);
		vec2 dir = normalize(c);
		uv = c + dir * bendFactor;
		float ringDist = abs(d - rs * 0.9);
		if (ringDist < rs * 0.15) {
			float ringBright = exp(-ringDist * ringDist / (rs * rs * 0.003)) * 0.3 * lensStr;
			vec3 bg = starField(uv * 0.5 + 0.5, sd, seed) + nebulaBg(uv * 0.5 + 0.5, ni, seed);
			return bg * (1.0 + ringBright * 8.0);
		}
	}
	return starField(uv * 0.5 + 0.5, sd, seed) + nebulaBg(uv * 0.5 + 0.5, ni, seed);
}

vec3 accretionDisk(vec2 c, float R, float rate, float tiltDeg, float spin, float t, float seed) {
	if (rate < 0.01) { return vec3(0.0); }
	float tiltR = tiltDeg * 3.14159 / 180.0;
	float ct = cos(tiltR);
	float st = sin(tiltR);
	float y_disk = c.y / ct;
	float x_disk = c.x;
	float diskR = length(vec2(x_disk, y_disk));
	float innerR = R * 1.5;
	float outerR = R * 8.0;
	if (diskR < innerR * 0.5 || diskR > outerR) { return vec3(0.0); }
	float diskThickness = R * 0.15 * (diskR / outerR);
	float vertDist = abs(c.y - y_disk * ct) * 2.0;
	if (vertDist > diskThickness * 3.0) { return vec3(0.0); }
	float vertFade = exp(-vertDist * vertDist / (diskThickness * diskThickness));
	bool isBehind = c.y * st > 0.0 && abs(c.y) > R * 0.3;
	float behindFade = 1.0;
	float d = length(c);
	if (isBehind && d < R * 1.2) { behindFade = smoothstep(R * 0.5, R * 1.3, d); }
	float orbitAngle = atan(y_disk, x_disk);
	float diskT = (diskR - innerR) / (outerR - innerR);
	float keplerSpeed = 1.0 / pow(max(diskR / innerR, 0.5), 1.5);
	float flowAngle = orbitAngle - t * keplerSpeed * 2.0 * spin;
	float tempProfile = pow(clamp(1.0 - diskT, 0.0, 1.0), 0.75);
	vec3 hotCol = vec3(1.0, 0.95, 0.9);
	vec3 warmCol = vec3(1.0, 0.6, 0.2);
	vec3 coolCol = vec3(0.6, 0.15, 0.05);
	vec3 diskCol = mix(coolCol, warmCol, smoothstep(0.0, 0.5, tempProfile));
	diskCol = mix(diskCol, hotCol, smoothstep(0.5, 1.0, tempProfile));
	float spiralN = sin(flowAngle * 3.0 + diskT * 20.0 + fbm(vec3(flowAngle * 2.0, diskT * 8.0, seed + t * 0.1), 3, 0.5) * 4.0) * 0.5 + 0.5;
	float turb = fbm(vec3(x_disk * 6.0 + t * 0.3, y_disk * 6.0, seed * 3.0), 4, 0.55) * 0.5 + 0.5;
	float density = (0.5 + spiralN * 0.3 + turb * 0.2) * rate;
	float doppler = 1.0 + sin(orbitAngle) * 0.3 * spin;
	float radialFade = smoothstep(innerR * 0.5, innerR * 1.5, diskR) * smoothstep(outerR, outerR * 0.7, diskR);
	float alpha = density * vertFade * radialFade * behindFade * doppler;
	return diskCol * alpha * tempProfile * 2.5;
}

vec3 jets(vec2 c, float R, float power, float tiltDeg, float t, float seed) {
	if (power < 0.01) { return vec3(0.0); }
	float tiltR = tiltDeg * 3.14159 / 180.0;
	vec2 jetAxis = vec2(-sin(tiltR), cos(tiltR));
	vec2 jetPerp = vec2(jetAxis.y, -jetAxis.x);
	vec3 col = vec3(0.0);
	for (int s = -1; s <= 1; s += 2) {
		float sf = float(s);
		vec2 axis = jetAxis * sf;
		float along = dot(c, axis);
		float perp = abs(dot(c, jetPerp));
		if (along < R * 0.3) { continue; }
		float coneW = R * 0.08 + along * 0.12;
		if (perp > coneW * 2.5) { continue; }
		float jetFade = exp(-perp * perp / (coneW * coneW)) * exp(-along * 1.2 / (R * 4.0));
		float knots = sin(along * 15.0 / R + t * 3.0) * 0.5 + 0.5;
		float turbJ = fbm(vec3(along * 8.0 / R + t * 2.0, perp * 12.0 / R, seed * 7.0 + sf), 3, 0.5) * 0.5 + 0.5;
		float brightness = jetFade * power * (0.4 + knots * 0.3 + turbJ * 0.3);
		float coreFrac = exp(-perp * perp / (coneW * coneW * 0.15));
		vec3 jetCol = mix(vec3(0.3, 0.4, 1.0), vec3(0.7, 0.8, 1.0), coreFrac);
		col += jetCol * brightness;
	}
	return col;
}

float photonRing(float d, float R, float lensStr) {
	if (lensStr < 0.01) { return 0.0; }
	float ringR = R * 1.5;
	float ringDist = abs(d - ringR);
	float ringW = R * 0.04;
	return exp(-ringDist * ringDist / (ringW * ringW)) * lensStr * 0.6;
}

void fragment() {
	vec2 uv = UV;
	float asp = u_resolution.x / u_resolution.y;
	vec2 c = (uv - 0.5) * vec2(asp, 1.0) * u_zoom;
	float d = length(c);
	float R = 0.32 * u_radius;
	float t = u_time;
	vec3 baseColor = tempToRGB(u_temperature);
	vec3 bg = lensedBg(c, R, u_lensing, u_starDensity, u_nebulaIntensity, u_seed);
	vec3 color = bg;
	float nd = d / R;
	float ang = atan(c.y, c.x);
	float PI = 3.14159265;
	float TAU = 6.2831853;

	if (u_accretionRate > 0.01) {
		float tiltR = u_diskTilt * PI / 180.0;
		bool behindBH = c.y * sin(tiltR) > 0.0;
		if (behindBH) {
			color += accretionDisk(c, R, u_accretionRate, u_diskTilt, u_bhSpin, t, u_seed);
		}
	}

	if (u_spikeCount > 0.5 && u_temperature > 0.5) {
		int spkN = int(u_spikeCount);
		float spikeMax = 0.0;
		for (int i = 0; i < 8; i++) {
			if (i >= spkN) { break; }
			float fi = float(i);
			float sa = PI * fi / float(spkN);
			float da = abs(mod(ang - sa + PI, TAU) - PI);
			float spk = exp(-da * 120.0) / (1.0 + d * d / (R * R * u_spikeLength * 0.5 + 0.001) * 3.0);
			spikeMax = max(spikeMax, spk);
		}
		color += baseColor * spikeMax * u_spikeBright * u_luminosity * 0.5;
	}

	if (u_coronaBright > 0.01 && d > R * 0.7) {
		float coronaD = max(0.0, (d - R)) / (R * u_coronaExtent + 0.001);
		float streamVal = 0.0;
		if (u_coronaStreams > 0.5) {
			float nS = u_coronaStreams;
			vec2 cn = c / max(d, 0.001);
			float sn1 = fbm(vec3(cn.x * nS * 1.2, cn.y * nS * 1.2, u_seed * 5.0 + t * 0.02), 3, 0.5);
			float sn2 = fbm(vec3(cn.x * nS * 0.7 + 7.0, cn.y * nS * 0.7 + 3.0, u_seed * 3.0 + t * 0.015), 2, 0.5);
			streamVal = (sn1 * 0.6 + sn2 * 0.4 + 0.5) * u_coronaLength;
			streamVal += pow(abs(cn.y), 2.0) * 0.3 * u_coronaLength;
			float asymN = fbm(vec3(cn.x * 1.5 + u_seed * 3.0, cn.y * 1.5, t * 0.01), 2, 0.5);
			streamVal *= (1.0 + asymN * u_coronaAsym);
		}
		float coronaFade = exp(-coronaD * (1.5 - min(streamVal, 0.8) * 0.8));
		float corona = coronaFade * u_coronaBright * (0.5 + streamVal * 0.5);
		vec3 coronaCol = mix(baseColor, vec3(1.0), 0.6) * corona;
		float fineN = fbm(vec3(c.x * 8.0 + t * 0.03, c.y * 8.0, u_seed * 7.0), 3, 0.5);
		coronaCol *= (1.0 + fineN * 0.25);
		color += coronaCol * smoothstep(R * 0.85, R * 1.1, d);
	}

	if (u_promCount > 0.5 && d > R * 0.85 && d < R * (1.0 + u_promHeight * 2.5)) {
		int pCount = int(u_promCount);
		for (int i = 0; i < 8; i++) {
			if (i >= pCount) { break; }
			float fi = float(i);
			float pSeed = u_seed * 17.3 + fi * 41.7;
			float pAng = fract(sin(pSeed * 127.1) * 43758.5) * TAU;
			float pW = 0.08 + fract(sin(pSeed * 269.5) * 22578.5) * 0.12;
			float pDir = mod(ang - pAng + PI, TAU) - PI;
			float angDist = abs(pDir);
			if (angDist < pW * 3.0) {
				float radialT = (d - R * 0.92) / (R * u_promHeight * 2.0);
				if (radialT > 0.0 && radialT < 1.0) {
					float archX = pDir / pW;
					float archY = 1.0 - archX * archX;
					archY = max(0.0, archY);
					float loopT = abs(radialT - archY * 0.8);
					float loopShape = exp(-loopT * loopT * 60.0) * archY;
					float fillShape = archY * exp(-radialT * 2.0) * smoothstep(0.0, 0.3, archY - radialT * 0.5);
					float promShape = max(loopShape * 0.7, fillShape * 0.5);
					float promNoise = fbm(vec3(pDir * 20.0 + t * 0.1 + fi * 5.0, radialT * 8.0, pSeed), 3, 0.55) * 0.5 + 0.5;
					float prom = promShape * promNoise * u_promGlow;
					prom *= exp(-angDist / pW * 1.5);
					vec3 promCol = mix(baseColor, vec3(1.0, 0.25, 0.15), 0.65) * prom;
					color += promCol * 0.6;
				}
			}
		}
		if (u_flareIntensity > 0.01) {
			float flareSeed = u_seed * 31.7 + floor(t * 0.15) * 7.3;
			for (int i = 0; i < 3; i++) {
				float fi = float(i);
				float fAng = fract(sin(flareSeed + fi * 77.7) * 43758.5) * TAU;
				float fW = 0.04 + fract(sin(flareSeed + fi * 33.3) * 22578.5) * 0.06;
				float fDir = mod(ang - fAng + PI, TAU) - PI;
				float angD = abs(fDir);
				if (angD < fW * 2.5) {
					float fRad = (d - R * 0.95) / (R * 0.2);
					if (fRad > -0.5 && fRad < 1.0) {
						float flare = exp(-fRad * fRad * 3.0) * exp(-angD * angD / (fW * fW * 2.0)) * u_flareIntensity;
						float pulse = sin(t * 1.5 + fi * 3.0) * 0.3 + 0.7;
						vec3 flareCol = mix(baseColor, vec3(1.0), 0.6);
						color += flareCol * flare * pulse * 2.0;
					}
				}
			}
		}
	}

	if (u_chromoIntensity > 0.01) {
		float chromoEdge = smoothstep(R + u_chromoThick, R, d) * smoothstep(R - 0.005, R + u_chromoThick * 0.5, d);
		vec3 chromoCol = mix(baseColor, vec3(1.0, 0.2, 0.15), u_chromoShift * 0.7);
		vec2 cn = c / max(d, 0.001);
		float spiculeAngle = atan(cn.y, cn.x);
		float spicules = sin(spiculeAngle * 40.0 + fbm(vec3(cn * 8.0, u_seed * 11.0 + t * 0.3), 3, 0.5) * 6.0) * 0.5 + 0.5;
		float spiculeH = smoothstep(R, R + u_chromoThick, d) * smoothstep(R + u_chromoThick * 1.3, R + u_chromoThick * 0.5, d);
		chromoCol *= (0.7 + spicules * 0.5);
		color += chromoCol * (chromoEdge + spiculeH * 0.3) * u_chromoIntensity * 0.5;
	}

	if (d <= R && u_temperature > 0.5) {
		float z = sqrt(max(0.0, 1.0 - nd * nd));
		vec3 N = normalize(vec3(c / R, z));
		float rotAng = t * u_rotSpeed;
		float ca = cos(rotAng);
		float sa = sin(rotAng);
		vec3 sp = vec3(N.x * ca - N.z * sa, N.y, N.x * sa + N.z * ca);
		vec3 so = vec3(u_seed * 13.7, u_seed * 7.3, u_seed * 19.1);

		float superGran = 0.0;
		if (u_superGranStr > 0.01) {
			vec2 sv = voronoi(sp * u_superGranScale + so * 0.3);
			superGran = (sv.y - sv.x) * u_superGranStr;
		}

		float granTime = t * u_granFlow;
		vec3 granP = sp * u_granScale + so + vec3(granTime * 0.3, granTime * 0.2, granTime * 0.15);
		vec2 gv2 = voronoi(granP);
		float cellEdge = gv2.y - gv2.x;
		float granulation = smoothstep(0.0, 0.3, cellEdge);
		float turbDetail = fbm(sp * u_granScale * 2.0 + so + vec3(granTime, granTime, granTime), 4, 0.55) * 0.5 + 0.5;
		granulation = mix(granulation, turbDetail, u_granTurb * 0.4);
		float granBright = 1.0 + granulation * u_granContrast - u_granContrast * 0.5;
		granBright += superGran * 0.5;

		float mbp = 0.0;
		if (u_granContrast > 0.1) {
			float mbpNoise = snoise(sp * u_granScale * 3.0 + so * 2.0);
			float inLane = 1.0 - smoothstep(0.0, 0.12, cellEdge);
			mbp = inLane * smoothstep(0.6, 0.9, mbpNoise) * 0.08 * u_granContrast;
		}

		float spotMask = 1.0;
		int sCount = int(u_spotCount);
		for (int i = 0; i < 20; i++) {
			if (i >= sCount) { break; }
			float fi = float(i);
			float h1 = fract(sin(u_seed * 127.1 + fi * 311.7) * 43758.5);
			float h2 = fract(sin(u_seed * 269.5 + fi * 183.3) * 22578.5);
			float h3 = fract(sin(u_seed * 71.3 + fi * 53.1) * 12345.7);
			float sTheta = (0.8 * h1 - 0.4) * PI * 0.5;
			float sPhi = h2 * TAU;
			float sCos = cos(sTheta);
			vec3 spotPos = vec3(sCos * cos(sPhi), sin(sTheta), sCos * sin(sPhi));
			float spotDist = length(sp - spotPos);
			float sSize = u_spotSize * (0.5 + h3);
			float umbra = smoothstep(sSize * 0.4, sSize * 0.15, spotDist);
			float penumbraR = sSize * u_penumbra;
			float penumbraVal = smoothstep(penumbraR, sSize * 0.25, spotDist);
			float penAngle = atan(sp.y - spotPos.y, sp.x - spotPos.x);
			float penFil = sin(penAngle * 16.0 + fbm(vec3(penAngle * 5.0, spotDist * 30.0, u_seed + fi), 3, 0.5) * 5.0) * 0.5 + 0.5;
			float radFil = sin(spotDist * 80.0 + penAngle * 4.0) * 0.5 + 0.5;
			penumbraVal *= (0.6 + penFil * 0.25 + radFil * 0.15);
			float spotVal = mix(penumbraVal, 1.0, umbra);
			spotMask = min(spotMask, mix(1.0, u_spotDark, spotVal));
		}

		float faculae = 0.0;
		for (int i = 0; i < 20; i++) {
			if (i >= sCount) { break; }
			float fi = float(i);
			float h1 = fract(sin(u_seed * 127.1 + fi * 311.7) * 43758.5);
			float h2 = fract(sin(u_seed * 269.5 + fi * 183.3) * 22578.5);
			float sTheta = (0.8 * h1 - 0.4) * PI * 0.5;
			float sPhi = h2 * TAU;
			float sCos = cos(sTheta);
			vec3 spotPos = vec3(sCos * cos(sPhi), sin(sTheta), sCos * sin(sPhi));
			float spotDist = length(sp - spotPos);
			float facR = u_spotSize * u_penumbra * 2.5;
			float fac = smoothstep(facR, u_spotSize * u_penumbra * 0.8, spotDist) * smoothstep(u_spotSize * 0.3, u_spotSize * u_penumbra * 0.5, spotDist);
			fac *= pow(1.0 - z, 0.8) * 0.9 + 0.1;
			float plageN = fbm(sp * 15.0 + so + vec3(fi * 3.0, fi * 3.0, fi * 3.0), 3, 0.5) * 0.5 + 0.5;
			fac *= (0.5 + plageN * 0.5);
			faculae = max(faculae, fac * 0.15);
		}

		vec3 limbCol = limbDarkenColor(baseColor, z, u_limbDark);
		vec3 surfColor = limbCol * granBright * spotMask * u_luminosity;
		surfColor += baseColor * faculae * u_luminosity;
		surfColor += baseColor * mbp * u_luminosity;
		if (u_chromoIntensity > 0.01) {
			float network = 1.0 - smoothstep(0.0, 0.15, cellEdge);
			vec3 netCol = mix(baseColor, vec3(1.0, 0.4, 0.3), u_chromoShift * 0.3);
			surfColor += netCol * network * u_chromoIntensity * 0.04 * pow(1.0 - z, 0.5);
		}
		color = surfColor;
	}

	if (d <= R && u_temperature < 0.5) {
		color = vec3(0.0);
		float edgeGlow = photonRing(d, R, u_lensing);
		color += vec3(0.8, 0.85, 1.0) * edgeGlow;
	}

	if (u_accretionRate > 0.01) {
		float tiltR = u_diskTilt * PI / 180.0;
		bool isFront = (c.y * sin(tiltR) <= 0.0) || (d > R * 1.2);
		if (isFront) {
			color += accretionDisk(c, R, u_accretionRate, u_diskTilt, u_bhSpin, t, u_seed);
		}
	}

	if (u_lensing > 0.01) {
		float pr = photonRing(d, R, u_lensing);
		vec3 prCol = mix(vec3(1.0, 0.9, 0.7), vec3(0.7, 0.8, 1.0), u_bhSpin);
		color += prCol * pr;
	}

	color += jets(c, R, u_jetPower, u_diskTilt, t, u_seed);

	if (u_bloomIntensity > 0.01) {
		float bloomD = max(0.0, d - R * 0.5) / (R * (1.0 + u_bloomRadius * 3.0));
		float bloom = exp(-bloomD * bloomD * 3.0) * u_bloomIntensity;
		bloom *= smoothstep(R * 0.3, R, d);
		float bloom2 = exp(-bloomD * bloomD * 0.8) * u_bloomIntensity * 0.3;
		bloom2 *= smoothstep(R * 0.1, R * 0.8, d);
		vec3 bloomCol = mix(baseColor, vec3(1.0), 0.3) * bloom * 0.2 + baseColor * bloom2 * 0.1;
		color += bloomCol;
	}

	color = color * (color * 2.51 + 0.03) / (color * (color * 2.43 + 0.59) + 0.14);
	color = pow(clamp(color, vec3(0.0), vec3(1.0)), vec3(1.0 / 2.1));

	COLOR = vec4(color, 1.0);
}
