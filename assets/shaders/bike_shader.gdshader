shader_type spatial;
instance uniform vec4 input_color = vec4(0.0f);

float random2 (in vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

void fragment() {
	vec2 modo = mod(UV, 0.05f);
	vec2 mods = UV - modo + vec2(TIME/10000000.0f * random2(UV-modo));
	float factor = random2(mods);
	ALBEDO = input_color.xyz * (factor + 0.5f) * 1.1f;
	EMISSION = input_color.xyz * 0.8f * (1.0f - abs(mod((TIME / 3.0f), 1.0f) - UV.x));
}
