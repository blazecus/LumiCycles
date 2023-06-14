shader_type sky;

uniform float grid_size = 0.008f;
uniform float grid_width = 0.00025f;
uniform float x_scale = 1.5f;

float random (in float x) {
    return fract(sin(x)*1e4);
}

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

void sky() {
	COLOR = vec3(0.01f, 0.01f, 0.1f) * 0.1f;
	vec2 uva = SKY_COORDS;
	uva.x *= x_scale;
	uva += vec2(TIME / 300.0f, TIME / 450.0f);
	vec2 uvb = mod(uva, grid_size);
	
	vec2 uvc = uva - uvb;
	float time_loop = abs(10.0f - mod(TIME, 20.0f));
	vec2 uvd = mod(vec2(random2(uvc), random2(vec2(uvc.y, uvc.x))) + uvb, grid_size + random2(uvc) / 400.0f + random(time_loop/300000.0f) / 400.0f);
	if(uvd.x < grid_width || uvd.y < grid_width){
		COLOR = vec3(.05);
	}
}