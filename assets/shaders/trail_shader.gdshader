shader_type spatial;
instance uniform vec4 input_color = vec4(0.0f);

float random(in float x){
	return fract(sin(x) * 1e4);
}

float random2(in vec2 st){
	return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

float pattern(vec2 st, vec2 v, float t){
	vec2 p = floor(st + v);
	return step(t, random2(100.+p*.000001)+random(p.x)*0.5);
}

void fragment() {
	vec2 st = UV.xy;
	vec2 grid = vec2(20.0, 10.);
	st *= grid;
	
	vec2 ipos = floor(st);
	vec2 fpos = fract(st);
	
	vec2 vel = vec2(TIME*2.*max(grid.x, grid.y));
	vel *= vec2(-1.,0.0) * random(1.0 + ipos.y);
	
	vec2 offset = vec2(0.1,0.);
	
	vec3 color = vec3(0.);
	color.r = pattern(st+offset,vel,0.8);
	color.g = pattern(st,vel,0.8);
	color.b = pattern(st-offset,vel,0.8);
	
	color *= step(0.2, fpos.y);
	
	ALBEDO = vec3((1.0-color) * input_color.xyz);
	
}
