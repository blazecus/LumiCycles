shader_type spatial;
varying vec3 lv;

void vertex() {
	lv = VERTEX;
}

void fragment() {
	ALBEDO = vec3(0.3f,0.4f,0.6f);
	ALPHA = 0.0f;
	float lvz = lv.z + TIME * 6.0f;
	float ll = mod(lvz, 1.5f);
	if(ll < 0.2f){
		ALPHA = 1.0f;
		float t = abs(ll - 0.1f) * 10.0f;
		EMISSION = ALBEDO * t * 1.2f;
	}
}
