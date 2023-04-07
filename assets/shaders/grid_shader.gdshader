shader_type spatial;

instance uniform vec4 input_color = vec4(.25f, .3f, .6f, 1.0f);
instance uniform vec3 player_position = vec3(0);

float random (in float x) {
    return fract(sin(x)*1e4);
}

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

void fragment() {
	ALBEDO = player_position;
	float af = length(player_position - (CAMERA_POSITION_WORLD + VERTEX));

	ALPHA = (1.0f - smoothstep(0, 200.0f, af)) * smoothstep(25.0f, 50.0f, af);
	
}