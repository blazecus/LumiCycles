shader_type spatial;
render_mode world_vertex_coords;

uniform float big_cell_size = 0.06f;
uniform float big_thickness = 0.007;
uniform float small_cell_size = 0.003f;
uniform float small_thickness = 0.0005f;
uniform float tiny_cell_size = 0.002f;
uniform float tiny_thickness = 0.0004f;

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

vec4 line_check(vec2 uvc, float thickness){
	if(abs(uvc.x) < thickness || abs(uvc.y) < thickness){
		return vec4(0.3f,0.3f,1.0f,1.0f);
	}
	return vec4(0);
}

vec4 line_intensity(vec2 uvc, float thickness){
	vec2 uvb = (abs(uvc - 0.5f * thickness) / thickness) * 2.0f;
	uvb = uvb * uvb * 2.0f;
	vec3 default_albedo = vec3(0.1f);
	float lx_factor = 0.0f;
	float ly_factor = 0.0f;
	if(abs(uvc.x) < thickness){
		lx_factor = uvb.x;
	}
	if(abs(uvc.y) < thickness){
		ly_factor = uvb.y;
	}
	float line_factor = max(lx_factor,ly_factor) * 1.8f;
	float fake_alpha = float(line_factor > 0.0f);
	
	line_factor -= mod(line_factor, 0.5f);
	
	return vec4(line_factor * line_factor * line_factor * default_albedo,fake_alpha);

}

void fragment() {
	vec3 world_vertex = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	
	vec2 big = mod(UV, big_cell_size);
	vec2 small = mod(UV, small_cell_size);
	//vec2 tiny = mod(UV, tiny_cell_size);
	
	vec3 tempa = vec3(0.1f);
	ALPHA = 0.5f;
	float rfactor = 1.0f;// sin(TIME + world_vertex.x / 10.0f);

	
	vec4 big_color = line_check(big, big_thickness);
	vec4 small_color = line_check(small, small_thickness);
	
	ALPHA = big_color.w;
	ALBEDO = big_color.xyz;
	if(big_color.w == 0.0f){
		ALBEDO = small_color.xyz;
		ALPHA = small_color.w;
	}
	
	
	vec4 big_emission = line_intensity(big, big_thickness);
	vec4 small_emission = line_intensity(small, small_thickness);
	
	float dfactor = 1.0f - min(1.0f, length(CAMERA_POSITION_WORLD - world_vertex) / 40.0f);
	rfactor *= dfactor * 0.5f;
	//big_emission -= mod(big_emission, 0.3f);
	//small_emission -= mod(small_emission, 0.3f);
	EMISSION = rfactor * big_emission.xyz;
	RADIANCE = rfactor * big_emission;
	if(big_emission.w == 0.0f){
		EMISSION = rfactor * small_emission.xyz;
		RADIANCE = rfactor * small_emission;
	}
	
	ALPHA_SCISSOR_THRESHOLD = 0.6f;
}


