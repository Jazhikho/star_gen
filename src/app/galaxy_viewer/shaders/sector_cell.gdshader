shader_type spatial;
render_mode blend_mix, depth_draw_never, depth_test_disabled, cull_disabled, unshaded;

void fragment() {
	ALBEDO = COLOR.rgb;
	ALPHA = COLOR.a;
}
