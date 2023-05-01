shader_type spatial;
render_mode world_vertex_coords;

uniform sampler2D letters;
//uniform float columns = 8;
instance uniform vec2 UV_scaling =  vec2(80.0f,80.0f);
instance uniform float xwidth = 1.5f;
uniform float sl = 200.0f;
uniform float sl_portion = 3.0f;
void vertex() {
    //VERTEX = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
    //NORMAL = normalize((MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz);
    // same as above for binormal and tangent, if normal mapping is used
	//VERTEX += vec3(sin(TIME) * 5.0f, 0, sin(TIME) * 5.0f);
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
	
	//float min_cutoff = abs(sin(TIME + random2(world_vertex.xy))) * 30.0f;
	//float max_cutoff = abs(sin(TIME)) * 5.0f + 15.0f;
	//float max_cutoff = 70.0f;
	
	//float chance = smoothstep(min_cutoff, max_cutoff, world_vertex.y);
	
	/*vec3 cubes = floor(world_vertex / 0.3f) + vec3(0.05f);
	float check = abs(random(random2(cubes.xy) * random(cubes.z)));
	ALPHA = step(check, 0.3f);
	
	ALPHA *= (1.0f - smoothstep(0, 200.0f, af)) * smoothstep(25.0f, 50.0f, af);*/
	float columns = 5.0f;
	vec3 uv = vec3(UV.x * UV_scaling.x, UV.y * UV_scaling.y, world_vertex.z);
	
	float xmod = mod(uv.x, xwidth/columns);
	float column = floor(uv.x / (xwidth/columns));

	float velocity = (random(column) + 0.2f) * -1.5f;
	float fall = TIME * velocity;
	float streak_length = (1.0f/columns) * sl;//random2(vec2(column, 0.7f));
	float new_y = uv.y + ((random(floor(uv.x * xwidth/columns)) - 0.5f) * 2.0f) * streak_length;
	float brightness = 1.0f - smoothstep(0, streak_length/sl_portion, mod(new_y - mod(new_y, xwidth/columns) - mod(fall, streak_length), streak_length));
	
	float sample_letter = floor(random2(vec2(column, floor(new_y * columns / xwidth))) * 45.0f + random(floor((TIME * -velocity + new_y + streak_length / sl_portion) / (streak_length / -velocity))) * 45.0f);
	float xuv = ((1.0f - xmod * (columns / xwidth)) / 90.0f) + sample_letter / 90.0f;
	vec4 sample = texture(letters, vec2(xuv, 1.0f - fract(new_y * columns / xwidth)));
	ALBEDO = vec3((vec3(1) - sample.xyz) * (vec3(brightness/3.0f,brightness,brightness/3.0f) + vec3(step(0.967f, brightness)) ));
	//ALPHA = brightness + 0.8f;
	//ALPHA = (1.0f - smoothstep(50.0f, 200.0f, af));// * smoothstep(25.0f, 50.0f, af);
	ALPHA = step(0.2f, brightness);
	ALPHA_SCISSOR_THRESHOLD = 0.5f;
	EMISSION = ALBEDO * 16.0f / max(10.0f,af);
	RADIANCE = vec4(ALBEDO,0.5f);
	//ALBEDO = vec3(UV.x, UV.y, 1.0f);
	//ALPHA = step(sample.r, -1);
}

