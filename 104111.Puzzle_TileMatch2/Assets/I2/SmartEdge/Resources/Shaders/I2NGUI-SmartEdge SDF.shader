Shader "Unlit/I2NGUI SmartEdge/SDF"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_FaceTex("Face Texture", 2D) = "white" {}
		_OutlineTex("Outline Texture", 2D) = "white" {}
		_GlowTex("Glow Texture", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "bump" {}

		_Color("Tint", Color) = (1, 1, 1, 1)
		_UseWidgetTexture("Use Widget Texture", Float) = 0

		_GlobalData("Global Data", float) = (0, 0, 0, 0)
		_BevelData("Bevel Data", Vector) = (0, 0, 0, 0)
		_BevelWave("Bevel Wave", Vector) = (0, 0, 0, 0)
		_LightData("Light Data", Vector) = (0, 0, 0, 0)
		_LightDir("LightDir", Vector) = (0, 0, 1, 0)
		_LightSpecularData("Light Data", Vector) = (0, 0, 0, 0)
		_LightSpecularColor("Light Specular Color", Color) = (1, 1, 1, 0)

		_BumpData("Bump Data", Vector) = (0, 0, 0, 0)
		_EnvReflectionColor_Face("ReflectionColor Face", Color) = (0, 0, 0, 0)
		_EnvReflectionColor_Outline("ReflectionColor Outline", Color) = (0, 0, 0, 0)
		_EnvironmentMap("Environment Texture", Cube) = "white" {}
		_EnvReflectionMatrixX("Environment Matrix X", Vector) = (1, 0, 0, 0)
		_EnvReflectionMatrixY("Environment Matrix Y", Vector) = (0, 1, 0, 0)
		_EnvReflectionMatrixZ("Environment Matrix Z", Vector) = (0, 0, 1, 0)
		_EnvReflectionData("Environment Data", Vector) = (0, 0, 1, 0)



		// Used only if Glow is baked
		_GlowColor("Glow Color", Color) = (0, 0, 0, 0)
		_GlowData("Glow Data", Vector) = (0, 0, 0, 0)
		_GlowOffset("Glow Offset", Vector) = (0, 0, 0, 0)

		// Used only if Shadow is baked
		_ShadowColor("Shadow Color", Color) = (0, 0, 0, 0)
		_ShadowData("Shadow Data", Vector) = (0, 0, 0, 0)

		_InnerShadowColor("Inner Shadow Color", Color) = (0, 0, 0, 0)
		_InnerShadowOffset("Inner Shadow Offset", Vector) = (0, 0, 0, 0)
		_InnerShadowData("Inner Shadow Data", Vector) = (0, 0, 0, 0)

		_I2MainTex_TexelSize("I2MainTex_TexelSize", Vector) = (0, 0, 0, 0) 
	}


	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			//#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			//#pragmaOFF multi_compile SDF MSDF MSDFA
			//#pragmaOFF multi_compile __ OUTLINE
			//#pragmaOFF multi_compile __ GLOW
			//#pragmaOFF multi_compile __ SHADOW
			//#pragmaOFF multi_compile __ BEVEL
			//#pragmaOFF multi_compile __ INNER_SHADOW
			//#pragmaOFF multi_compile __ SUPER_SAMPLING
			//#pragmaOFF multi_compile __ TEXTURES
			//#pragmaOFF multi_compile __ MULTI_MODES

			#if TEXTURES && BEVEL
				#define NORMAL_MAP true
				#define ENVIRONMENT_MAP true
			#endif

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : TEXCOORD0;
				float4 texcoord : TEXCOORD1;

				half4 Params1    : TEXCOORD2;
				half4 Params2    : TEXCOORD3;
				float4 Params3   : TEXCOORD4;
				float4 Params4   : TEXCOORD5;
			};

			sampler2D _MainTex;			
			float4 _Color;
			float _UseWidgetTexture;

			#define EXTRA_VERT_PARAM v.texcoord1.z


			#include "Includes/I2_SDF.cginc"

			v2f vert (appdata_t v)
			{
				v2f o = PrepareParams(v);

				return o;
			}
			
			
			float4 frag (v2f i) : SV_Target
			{
				return ExecuteSDF_Global(i);
			}
			ENDCG
		}
	}
}