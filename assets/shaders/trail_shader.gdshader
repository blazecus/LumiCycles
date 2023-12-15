shader_type spatial;
render_mode world_vertex_coords;

instance uniform vec4 input_color = vec4(0.0f);
instance uniform vec3 player_position = vec3(0.0f);

uniform float edge_thickness_base = 0.25f;
uniform float edge_thickness_minimum = 0.15f;

float random (in float x) {
    return fract(sin(x)*1e4);
}

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

void fragment() {
	vec2 grid = vec2(20, 10);
	vec2 uvt = UV * grid;
	
	vec2 ipos = floor(uvt);
	vec2 fpos = fract(uvt);
	
	float vel = (random(ipos.y) + 0.3) * 7.5f;
	float len = random(abs(sin(ipos.y + 0.5)) * 6.0f) * 7.0f;
	
	//ALBEDO = vec3(vel);
	float lin = mod(UV.x - TIME * vel, len) / len;
	vec3 col = vec3(pow(6, lin * 2.0f) / 32.0f + 0.2f);
		
	//ALPHA = 0.85f + col.r / 6.0f;
	ALPHA_SCISSOR_THRESHOLD = 0.2f;
	ALBEDO = input_color.xyz / 2.0f + vec3(col * col * input_color.xyz);
	//ALBEDO *= 1.3f;
	
	//float vy = UV.y - sign(UV.y - 0.5f) * random2(UV + VERTEX.xz) * .06f;
	//if(vy < 0.09f || vy > 0.91f){
	//	ALBEDO += vec3(0.85f);
	//}
	EMISSION = ALBEDO;
	
	/*
	float ya = 0.2f;
	if(UV.y > .5f){
		ya = 0.8f;
	}
	float edge_thickness = random2(vec2(UV.x / 10000000.0f, ya * TIME / 15000000.0f));
	edge_thickness += random2(vec2((UV.x + 1000.0f) / 32342400.0f, ya/2.0f * (TIME + 500.0f) / 30000000.0f));
	edge_thickness += random2(vec2((UV.x + 1500.0f) / 86435600.0f, ya/3.0f * (TIME + 1500.0f) / 60000000.0f));
	edge_thickness *= edge_thickness_base / 3.0f + edge_thickness_minimum;
	*/
	float edge_thickness = 0.2f;

	if(UV.x < edge_thickness || UV.y < edge_thickness)
	{
		vec2 uvb = ((edge_thickness - UV) / edge_thickness) * 2.0f;
		uvb = uvb * uvb * 2.0f;
		float lx_factor = 0.0f;
		float ly_factor = 0.0f;
		if(UV.x < edge_thickness){
			lx_factor = uvb.x;
		}
		if(UV.y < edge_thickness){
			ly_factor = uvb.y;
		}
		float line_factor = max(lx_factor,ly_factor) * 1.8f;
		line_factor -= mod(line_factor, 2.0f);
		vec3 check_emission = vec3(0.1f,0.1f,1.5f) * line_factor;
		//if(check_emission.x > 0.075f){
		//	EMISSION = check_emission;			
		//}
		EMISSION += check_emission;
	}
	
	float bike_edge_thickness = 12.0f;
	vec3 world_vertex = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	float dist = length(player_position - world_vertex);
	if(dist < bike_edge_thickness)
	{
		float uvb = ((bike_edge_thickness - dist) / bike_edge_thickness) * 2.0f;
		uvb = uvb * uvb * 2.0f;
		float lx_factor = 0.0f;
		float ly_factor = 0.0f;
		lx_factor = uvb;
		float line_factor = max(lx_factor,ly_factor) * 1.8f;
		line_factor -= mod(line_factor, 2.0f);
		vec3 check_emission = vec3(0.1f,0.1f,1.5f) * line_factor;
		//if(check_emission.x > 0.075f){
		//	EMISSION = check_emission;			
		//}
		float uvt = ((bike_edge_thickness - dist) / bike_edge_thickness);
		EMISSION += check_emission * 0.2f * uvt * uvt * uvt * vel;
	}
	
	if(UV.y > 1.0f - edge_thickness){
		vec2 uvb = ((edge_thickness - (1.0f - UV)) / edge_thickness) * 2.0f;
		uvb = uvb * uvb * 2.0f;
		float lx_factor = 0.0f;
		float ly_factor = 0.0f;
		if(UV.y > 1.0f- edge_thickness){
			ly_factor = uvb.y;
		}
		float line_factor = max(lx_factor,ly_factor) * 1.8f;
		line_factor -= mod(line_factor, 2.0f);
		if(UV.y > 1.0f-UV.x){
			vec3 check_emission = vec3(0.1f,0.1f,1.5f) * line_factor;
			//if(check_emission.x > 0.075f){
			//	EMISSION = check_emission;			
			//}
			EMISSION += check_emission;
		}

	}
	
	EMISSION = EMISSION * 0.8f;
	RADIANCE = vec4(ALBEDO, 1.0f);
}