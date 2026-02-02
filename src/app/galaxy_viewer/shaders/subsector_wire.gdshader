shader_type spatial;
render_mode blend_mix, depth_draw_never, depth_test_disabled, cull_disabled, unshaded;

uniform vec4 wire_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void fragment() {
	ALBEDO = COLOR.rgb * wire_color.rgb;
	ALPHA = COLOR.a * wire_color.a;
}
