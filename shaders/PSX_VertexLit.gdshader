// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
shader_type spatial;

render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_lambert, specular_schlick_ggx;

uniform sampler2D albedo : source_color, filter_nearest, repeat_enable;
uniform vec4 tint_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float snap_strength = 1.0;
uniform int triplanar_mode : hint_range(0,2,1);
uniform float uv_scale = 1;

varying vec3 normal_local;

void vertex()
{
	normal_local = NORMAL;
	//Vertex snapping
	vec4 snapToPixel = vec4(VERTEX.x, VERTEX.y, VERTEX.z, 1.0);
	snapToPixel = MODELVIEW_MATRIX * snapToPixel;
	snapToPixel = PROJECTION_MATRIX * snapToPixel;
	vec4 vertex = snapToPixel;
	vertex.xyz = snapToPixel.xyz / snapToPixel.w;
	float snap = 1.0 / snap_strength;
	vertex.x = floor(160.0 * snap * vertex.x) / (160.0 * snap);
	vertex.y = floor(120.0 * snap * vertex.y) / (120.0 * snap);
	vertex.xyz *= snapToPixel.w;
	POSITION = vertex;

	/*
	//Vertex lighting 
//	o.color =  float4(ShadeVertexLights(v.vertex, v.normal), 1.0);
	o.color = float4(ShadeVertexLightsFull(v.vertex, v.normal, 4, true), 1.0);
	o.color *= v.color;
	*/

	//float distance = length(MODELVIEW_MATRIX * vec4(VERTEX.x, VERTEX.y, VERTEX.z, 1.0));

	//Affine Texture Mapping
	//vec4 affinePos = vertex; //vertex;				
	//UV = TRANSFORM_TEX(v.texcoord, _MainTex);
	//UV *= distance + (vertex.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / distance / 2;
	//NORMAL = distance + (vertex.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / distance / 2;

	//Fog
	/*float4 fogColor = unity_FogColor;

	float fogDensity = (unity_FogEnd - distance) / (unity_FogEnd - unity_FogStart);
	o.normal.g = fogDensity;
	o.normal.b = 1;

	o.colorFog = fogColor;
	o.colorFog.a = clamp(fogDensity,0,1);

	//Cut out polygons
	if (distance > unity_FogStart.z + unity_FogColor.a * 255)
	{
		o.pos.w = 0;
	}

	return o;*/
}

void fragment()
{
	
	vec3 vertex_world = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	/*half4 c = tex2D(_MainTex, IN.uv_MainTex / IN.normal.r)*IN.color;
	half4 color = c * (IN.colorFog.a);
	color.rgb += IN.colorFog.rgb*(1 - IN.colorFog.a);
	color.a = c.a * _Transparency;*/
	vec2 triplanar_uv_xz = vertex_world.xz * uv_scale * round(normal_local.y);
	vec2 triplanar_uv_xy = vertex_world.xy * uv_scale * round(normal_local.z);
	vec2 triplanar_uv_zy = vertex_world.zy * uv_scale * round(normal_local.x);
	
	vec2 triplanar_uv_combined = (triplanar_uv_xy + triplanar_uv_xz + triplanar_uv_zy) / 3.0;
	vec4 color_triplanar_uv = texture(albedo, triplanar_uv_combined);
	
	vec4 color_xz = texture(albedo, triplanar_uv_xz) * abs(normal_local.y);
	vec4 color_xy = texture(albedo, triplanar_uv_xy) * abs(normal_local.z);
	vec4 color_yz = texture(albedo, triplanar_uv_zy) * abs(normal_local.x);
	
	vec4 color_triplanar_sample = color_xz + color_xy + color_yz;
	
	vec4 color_normal = texture(albedo, UV * uv_scale);
	
	vec4 color = mix(color_normal, color_triplanar_sample, float(triplanar_mode));
	color = mix(color, color_triplanar_uv, max(0, float(triplanar_mode - 1)));
	
	ALBEDO = color.rgb * tint_color.rgb;
}
