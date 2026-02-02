shader_type spatial;
render_mode blend_add, depth_draw_never, cull_disabled, unshaded;

void vertex() {
	vec3 world_pos = (MODEL_MATRIX * vec4(0.0, 0.0, 0.0, 1.0)).xyz;

	vec3 cam_right = normalize(INV_VIEW_MATRIX[0].xyz);
	vec3 cam_up    = normalize(INV_VIEW_MATRIX[1].xyz);
	float size = length(MODEL_MATRIX[0].xyz);

	vec3 billboard_pos = world_pos
		+ VERTEX.x * cam_right * size
		+ VERTEX.y * cam_up * size;

	POSITION = PROJECTION_MATRIX * VIEW_MATRIX * vec4(billboard_pos, 1.0);
}

void fragment() {
	vec2 center_uv = UV - vec2(0.5);
	float dist = length(center_uv) * 2.0;

	// Ring band
	float ring = smoothstep(0.6, 0.7, dist) * (1.0 - smoothstep(0.85, 0.95, dist));

	// Crosshair lines (thin)
	float cross_h = step(abs(center_uv.y), 0.02) * step(0.35, abs(center_uv.x));
	float cross_v = step(abs(center_uv.x), 0.02) * step(0.35, abs(center_uv.y));
	float crosshair = max(cross_h, cross_v) * 0.6;

	float pattern = max(ring, crosshair);

	ALBEDO = vec3(0.2, 1.0, 0.4);
	ALPHA = pattern * 0.9;
}
