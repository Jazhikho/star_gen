shader_type spatial;
render_mode blend_add, depth_draw_never, cull_disabled, unshaded;

uniform float global_alpha : hint_range(0.0, 1.0) = 1.0;
uniform float fade_near : hint_range(0.0, 100.0) = 20.0;
uniform float fade_far : hint_range(0.0, 100.0) = 45.0;

instance uniform float shell_alpha : hint_range(0.0, 1.0) = 1.0;

varying float v_fade;

void vertex() {
	vec3 world_pos = (MODEL_MATRIX * vec4(0.0, 0.0, 0.0, 1.0)).xyz;
	vec3 camera_pos = INV_VIEW_MATRIX[3].xyz;

	float dist = distance(world_pos, camera_pos);
	v_fade = 1.0 - smoothstep(fade_near, fade_far, dist);

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

	float core = exp(-dist * dist * 32.0);
	float halo = exp(-dist * dist * 4.0);
	float glow = core * 0.7 + halo * 0.3;

	ALBEDO = COLOR.rgb;
	ALPHA = glow * COLOR.a * global_alpha * v_fade * shell_alpha;
}
