shader_type spatial;

float random (in float x) {
    return fract(sin(x)*1e4);
}

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

void fragment() {
	ALBEDO = vec3(0.1f);
	vec3 world_pos = (CAMERA_POSITION_WORLD + VERTEX);
	float af = length(VERTEX);
	
	vec3 grid_check = mod(floor(world_pos / 0.3f), 2);
	ALPHA = step(0.5f, length(random2(grid_check.xy)));
	
	ALPHA *= (1.0f - smoothstep(0, 200.0f, af)) * smoothstep(25.0f, 50.0f, af);
	
	ALPHA = 1.0f;
	ALBEDO = vec3(1);
}