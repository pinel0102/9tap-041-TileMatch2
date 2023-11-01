// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#define MODE_Main 0
#define MODE_Glow 2

// Parameters:
// UV.x       = PackCharUV( UV0 )
// UV.y       = PackUV( FaceUV )
// UV1.y      = PackRGB8 (surface, edge, FaceAlpha, mode)
//
//                      MODE_Main           |           MODE_Glow
// UV1.x      = PackRGBA6( OutlineColor )   |   PackRGBA6 (glowInnerWidth, glowOuterWidth, glowIntensity, glowPower )
// Tangent.w  = free

void PrepareParams_General(appdata_t v, inout v2f o, half vertexMode )
{
	//--[ Get Values from Vertex ]------------------

	half3 _Surface_Edge_FaceAlpha = FloatToRGB7A3(v.texcoord1.y).xyz;
	float2 _CharUV				  = FloatToCharUV(v.texcoord.x);

	#ifdef TEXTURES
		float2 _FaceUV				  = FloatToUV(v.texcoord.y);
	#else
		float2 _FaceUV = 0;
	#endif

	#ifdef OUTLINE
		half4 _OutlineColor = FloatToRGBA6(v.texcoord1.x);
	#else
		half4 _OutlineColor = 0;
	#endif

	#ifdef ENVIRONMENT_MAP
		half3x3 envMatrix = half3x3(_EnvReflectionMatrixX, _EnvReflectionMatrixY, _EnvReflectionMatrixZ);

		//half3 viewDir = WorldSpaceViewDir(v.vertex);
		//float3 viewDir = ObjSpaceViewDir(v.vertex);
		//half3 viewDir = v.vertex;
		float3 viewDir = half3(v.vertex.xy / (10 * _ScreenParams.xy), -1); // not accurate, but fast and looks the same in Scene and in Game
		//float3 viewDir = half3(v.vertex.xy*0.0001, -1); // not accurate, but fast and looks the same in Scene and in Game
		viewDir = mul(envMatrix, viewDir);
		viewDir = normalize(viewDir);
		viewDir /= viewDir.z;    // make viewDir have the z=1 to be able of reconstructing it later
	#else
		float3 viewDir = 0;
	#endif


	//--[ Pass values to Pixel Shader]-----------------------------

	o.texcoord = float4(_CharUV, _Surface_Edge_FaceAlpha.rg);

	o.Params1 = half4(_FaceUV, o.color.a, vertexMode);
	o.Params2 = _OutlineColor;
	o.Params3 = half4(viewDir.xy, 0, 0);
	o.color.a = _Surface_Edge_FaceAlpha.b;
}

#define EXTRACT_PARAMS_GENERAL \
		half   Surface       = i.texcoord.z;\
		half   Edge          = i.texcoord.w;\
		float2 FaceUV        = i.Params1.xy;\
		half   GlobalAlpha   = i.Params1.z;\
		half   vertexMode	 = i.Params1.w;\
		half4  FaceColor	 = i.color;\
		half4  EdgeColor = i.Params2; \
		half3  ViewDir = normalize(float3(i.Params3.xy, 1));








void PrepareParams_Glow( appdata_t v, inout v2f o)
{
	//--[ Get Values from Vertex ]------------------

	half4 _GlowInner_Outer_Intensity_Power		= FloatToRGBA6(v.texcoord1.x);	
	_GlowInner_Outer_Intensity_Power.z *= 10;
		
	//--[ Pass values to Pixel Shader]-----------------------------

	o.Params2 = _GlowInner_Outer_Intensity_Power;
}

#define EXTRACT_PARAMS_GLOW \
		half   InnerGlow     = i.Params2.x;\
		half   OuterGlow     = i.Params2.y;\
		half   GlowIntensity = i.Params2.z;\
		half   GlowPower     = i.Params2.w;








inline v2f PrepareParams( appdata_t v )
{
	v2f o = (v2f)0;

#if !defined(SURFACE_SHADER)
	o.vertex = UnityObjectToClipPos( v.vertex.xyz );

	#ifdef UNITY_HALF_TEXEL_OFFSET
		o.vertex.xy += (_ScreenParams.zw-1.0)*half2(-1,1);
	#endif	
#endif

	o.color = v.color * _Color;

	float vertexMode = fmod(v.texcoord1.y, 1<<3)*2;

	PrepareParams_General(v, o, vertexMode);

#if MULTI_MODES
	if (vertexMode >= MODE_Glow)
		PrepareParams_Glow(v, o);
#endif

	return o;
}