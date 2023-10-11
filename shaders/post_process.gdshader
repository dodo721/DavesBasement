// inspired by https://godotshaders.com/shader/vhs-and-crt-monitor-effect/

shader_type canvas_item;

uniform float high : hint_range(0,1);
uniform float waveStrength;
uniform float waveSize;

void fragment () {
	
	float uvX = UV.x + waveStrength * sin(TIME + UV.y * waveSize) * high;
	float uvY = UV.y + waveStrength * sin(TIME + UV.x * waveSize) * high;
	
	vec4 texCol = texture(TEXTURE, vec2(uvX, uvY));
	vec4 greenCol = vec4(0.0, (texCol.r + texCol.g + texCol.b) * (1.0 / 3.0), 0.0, 1.0);
	
	COLOR = mix(texCol, greenCol, high);
}