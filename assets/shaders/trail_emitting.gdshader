shader_type spatial;
render_mode world_vertex_coords;

instance uniform vec4 input_color = vec4(0,0,0,1.0);
uniform float big_cell_size = 0.2f;
uniform float big_thickness = 0.05;
uniform float small_cell_size = 0.05f;
uniform float small_thickness = 0.01f;
uniform float tiny_cell_size = 0.002f;
uniform float tiny_thickness = 0.0004f;
uniform float edge_thickness = 0.08f;

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

vec4 line_check(vec2 uvc, float thickness){
	if(abs(uvc.x) < thickness || abs(uvc.y) < thickness){
		return vec4(0.3f,0.3f,1.0f,1.0f);
	}
	return vec4(0);
}

vec4 line_intensity(vec2 uvc, float thickness, float line_mod){
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
	
	ALBEDO = input_color.xyz;
	
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
		line_factor -= mod(line_factor, 1.0f);
		EMISSION = vec3(0.1f,0.1f,1.5f) * line_factor;
	}
	if(UV.x > 1.0f - edge_thickness || UV.y > 1.0f - edge_thickness){
		vec2 uvb = ((edge_thickness - (1.0f - UV)) / edge_thickness) * 2.0f;
		uvb = uvb * uvb * 2.0f;
		float lx_factor = 0.0f;
		float ly_factor = 0.0f;
		if(UV.x > 1.0f- edge_thickness){
			lx_factor = uvb.x;
		}
		if(UV.y > 1.0f- edge_thickness){
			ly_factor = uvb.y;
		}
		float line_factor = max(lx_factor,ly_factor) * 1.8f;
		line_factor -= mod(line_factor, 1.0f);
		if(UV.y > 1.0f-UV.x){
			EMISSION = vec3(0.1f,0.1f,1.5f) * line_factor;
		}

	}
	EMISSION *= 0.25;
	ALPHA = 1.0f;
	ALPHA_SCISSOR_THRESHOLD = 0.5f;
}