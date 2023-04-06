shader_type spatial;

instance uniform vec4 input_color = vec4(0.0f);

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
		
	ALPHA = 0.85f + col.r / 6.0f;
	ALBEDO = input_color.xyz / 2.0f + vec3(col * col * input_color.xyz);
	
	float vy = UV.y - sign(UV.y - 0.5f) * random2(UV + VERTEX.xz) * .06f;
	if(vy < 0.09f || vy > 0.91f){
	ALBEDO += vec3(0.85f);
	}
}