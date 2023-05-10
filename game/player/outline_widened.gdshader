shader_type spatial;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform int width = 4;
uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
void fragment() {
	
	
	// calculate the offset size of a single pixel
	vec2 screen_size = vec2(textureSize(SCREEN_TEXTURE, 1));
	
	vec2 pixel_size = 1.0f / vec2(screen_size.x, screen_size.y);
	
	ALPHA = 0.0f;
	//ALBEDO = vec3(0);
	for(int i = -width; i <= width; i++){
		for(int j = -width; j <= width; j++){
			vec3 s = texture(SCREEN_TEXTURE, SCREEN_UV + vec2(float(i) * pixel_size.x, float(j) * pixel_size.y)).xyz;
			//vec3 dist = s - outline_color.xyz;
			//if(dist.x*dist.x + dist.y*dist.y + dist.z*dist.z < 25.0f){
			if(s == outline_color.xyz){
				ALBEDO = outline_color.xyz;
				ALPHA = 1.0f;
			}
		}
	}
	
}
