shader_type spatial;
render_mode world_vertex_coords;

uniform float big_cell_size = 0.01f;
uniform float big_thickness = 0.001f;
uniform float small_cell_size = 0.001f;
uniform float small_thickness = 0.0002f;
uniform float tiny_cell_size = 0.002f;
uniform float tiny_thickness = 0.0004f;

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

void fragment() {
	vec3 world_vertex = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	
	vec2 big = mod(UV, big_cell_size);
	vec2 small = mod(UV, small_cell_size);
	//vec2 tiny = mod(UV, tiny_cell_size);
	
	ALBEDO = vec3(0.1f);
	ALPHA = 0.5f;
	float rfactor = sin(TIME + world_vertex.x / 10.0f);
	EMISSION = rfactor * ALBEDO;
	RADIANCE = rfactor * vec4(ALBEDO, 1.0f);
	
	if(abs(big.x) < big_thickness || abs(big.y) < big_thickness || 
	   abs(small.y) < small_thickness || abs(small.x) < small_thickness){// ||
	   //abs(tiny.y) < tiny_thickness || abs(tiny.x) < tiny_thickness){
		ALPHA = 1.0f;
		ALBEDO = vec3(0.3f,0.3f,1.0f);	
	}
	ALPHA_SCISSOR_THRESHOLD = 0.6f;
}
