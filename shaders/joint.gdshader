shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_lambert, specular_schlick_ggx;

uniform sampler2D Albedo : source_color, filter_nearest, repeat_enable;
uniform vec3 EndEmission : source_color;
uniform float Smoked : hint_range(0.0, 1.0);
uniform float BurnLength;
uniform float LengthMax;
uniform float LengthMin;

void fragment() {
	
	float maxY = (LengthMax*Smoked)-LengthMin;
	vec3 col = texture(Albedo, UV).rgb;
	vec4 objVert = inverse(MODEL_MATRIX) * INV_VIEW_MATRIX * vec4(VERTEX, 1.0);
	if (objVert.y < maxY) discard;
	float atEnd = step(maxY+BurnLength, objVert.y);
	ALBEDO = col;
	EMISSION = mix(mix(col, EndEmission, 0.5), vec3(0.0,0.0,0.0), atEnd);
}
