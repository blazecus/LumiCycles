shader_type spatial;
render_mode world_vertex_coords;

void vertex() {
    //VERTEX = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
    //NORMAL = normalize((MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz);
    // same as above for binormal and tangent, if normal mapping is used
	VERTEX += vec3(sin(TIME), sin(TIME), sin(TIME));
}

uniform vec4 input_color = vec4(0.7f,0.7f,1.0f,1.0f);
float random (in float x) {
    return fract(sin(x)*1e4);
}

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

void fragment() {
	vec3 world_vertex = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	ALBEDO = vec3(0.4f, 1.0f, 0.4f);
	float af = length(VERTEX);
	
	/*float grid_width = 0.15f;
	vec3 grid_check = mod(world_vertex, 0.6f);
	if(
		(grid_check.x < grid_width) ||
		(grid_check.x < grid_width) ||
		(grid_check.y < grid_width)
	){
		ALPHA = 1.0f;
	}
	else{
		ALPHA = 0.0f;
	}*/
	
	float min_cutoff = abs(sin(TIME + random2(world_vertex.xy))) * 30.0f;
	//float max_cutoff = abs(sin(TIME)) * 5.0f + 15.0f;
	float max_cutoff = 70.0f;
	
	float chance = smoothstep(min_cutoff, max_cutoff, world_vertex.y);
	
	vec3 cubes = floor(world_vertex / 0.3f) + vec3(0.05f);
	float check = abs(random(random2(cubes.xy) * random(cubes.z)));
	ALPHA = step(check, chance);
	
	ALPHA *= (1.0f - smoothstep(0, 200.0f, af)) * smoothstep(25.0f, 50.0f, af);
}

