shader_type spatial;
instance uniform vec4 input_color = vec4(0.0f);
void fragment() {
	ALBEDO = input_color.xyz;
}
