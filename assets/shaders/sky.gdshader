shader_type sky;

uniform float grid_size = 0.015f;
uniform float grid_width = 0.0015f;

void sky() {
	COLOR = vec3(0.01f, 0.01f, 0.1f) * 0.1f;
	vec2 uvb = mod(SKY_COORDS, grid_size);
	if(uvb.x < grid_width || uvb.y < grid_width){
		COLOR = vec3(.05);
	}
}