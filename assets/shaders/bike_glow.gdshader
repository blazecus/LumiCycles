shader_type spatial;

uniform float time_cycle = 5.0f;

varying vec3 local_vertex;

void vertex(){
	local_vertex = VERTEX;	
}

void fragment() {
	
	float time_factor = (mod(TIME, time_cycle) - time_cycle/2.0f) / (time_cycle/2.0f);
	float cycle_factor = (1.0f - abs(time_factor));
	float z_factor = cycle_factor * cycle_factor * 5.0f - (local_vertex.z+1.5f)*0.5f;
	EMISSION = vec3(0.6f) * 0.4f * (z_factor) + vec3(0.5f);

}
