shader_type spatial;

void fragment() {
	EMISSION = vec3(.6,.6,1.0) * (1.3f - abs(mod((TIME / 2.34f), 1.0f) - UV.y));
}
