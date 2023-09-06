shader_type spatial;
instance uniform vec4 input_color = vec4(0,0,0,1.0);
uniform float time_cycle = 9.0f;
uniform float box_size = 0.1f;

varying vec3 lv;

float random (in float x) {
    return fract(sin(x)*1e4);
}

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

float random3(in vec3 st){
	return fract(sin(dot(st, vec3(64.25375463, 23.27536534, 86.29678483))) * 59482.7542);
}

void vertex(){
	lv = VERTEX;
}

void fragment() {
	vec3 world_vertex = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	vec3 local_vertex = lv;
	
	float speed = random(local_vertex.y - mod(local_vertex.y, box_size)) * 2.0f;
	local_vertex.z = mod(local_vertex.z + speed * TIME / 3.0f, 3.0f) - 1.5f;
	
	vec3 vodo = mod(local_vertex, box_size);
	vec3 vods = local_vertex - vodo + vec3(TIME/10000000.0f * random3(local_vertex-vodo));
	float facv = random3(vods);
	ALBEDO = input_color.xyz * (facv + 0.5f) * 1.1f;
	float time_factor = (mod(TIME, time_cycle) - time_cycle/2.0f) / (time_cycle/2.0f);
	float cycle_factor = (1.0f - abs(time_factor));
	float z_factor = cycle_factor * cycle_factor * 5.0f - (local_vertex.z+1.5f)*0.5f;
	EMISSION = input_color.xyz * 0.2f * (z_factor) + input_color.xyz * 0.3f;
	ALPHA = facv + lv.z * random(UV.y) * 2.0f;
	ALPHA_SCISSOR_THRESHOLD = 0.45f;
}
