shader_type spatial;

instance uniform vec4 input_color = vec4(.25f, .3f, .6f, 1.0f);

float random (in float x) {
    return fract(sin(x)*1e4);
}

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

void fragment() {
	vec3 grid = vec3(4, 4,4);
	vec3 uvt = VERTEX * grid;
	
	vec3 ipos = floor(uvt);
	vec3 fpos = fract(uvt);
	
	float vel = (random(ipos.x) + 0.3) * 15.0f;
	float len = random(abs(sin(ipos.x + 0.5)) * 6.0f) * 40.0f;
	
	//ALBEDO = vec3(vel);
	float lin = mod(UV.y - TIME * vel, len) / len;
	vec3 col = vec3(pow(6, lin * 2.0f) / 32.0f + 0.2f);
	
	float dist = length(VERTEX - CAMERA_POSITION_WORLD);
	float alpha_factor = 1.0f - dist / 200.0f;
	ALPHA = alpha_factor * step(0.0f,alpha_factor);
	//ALBEDO = input_color.xyz / 2.0f + vec3(col * col * input_color.xyz);
	ALBEDO = vec3(VERTEX.y);
	//ALBEDO = vec3( col * input_color.xyz);
}