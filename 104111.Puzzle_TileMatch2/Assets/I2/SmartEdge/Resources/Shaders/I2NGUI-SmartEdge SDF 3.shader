Shader "Hidden/Unlit/I2NGUI SmartEdge/SDF 3"
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

			#define TEXTURES true
			#define BEVEL true
			#define OUTLINE true
			#define GLOW_SHADOW true
			#define INNER_SHADOW true
			#define MULTI_MODES	true
			#define GLOW true
			#define SHADOW true


			#if TEXTURES && BEVEL
				#define NORMAL_MAP true
				#define ENVIRONMENT_MAP true
			#endif

			#include "UnityCG.cginc"


			struct appdata_t {
				float4 vertex    : POSITION;
				half4  color     : COLOR;
				float2 texcoord  : TEXCOORD0;
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
				float4 worldPos : TEXCOORD6;
				float2 worldPos2 : TEXCOORD7;
			};

			sampler2D _MainTex;
			float4 _Color;
			float _UseWidgetTexture;

			float4 _ClipRange0 = float4(0.0, 0.0, 1.0, 1.0);
			float4 _ClipArgs0 = float4(1000.0, 1000.0, 0.0, 1.0);
			float4 _ClipRange1 = float4(0.0, 0.0, 1.0, 1.0);
			float4 _ClipArgs1 = float4(1000.0, 1000.0, 0.0, 1.0);
			float4 _ClipRange2 = float4(0.0, 0.0, 1.0, 1.0);
			float4 _ClipArgs2 = float4(1000.0, 1000.0, 0.0, 1.0);
			
			#define EXTRA_VERT_PARAM v.texcoord1.z

			#include "Includes/I2_SDF.cginc"

			float2 Rotate (float2 v, float2 rot)
			{
				float2 ret;
				ret.x = v.x * rot.y - v.y * rot.x;
				ret.y = v.x * rot.x + v.y * rot.y;
				return ret;
			}
			
			v2f vert (appdata_t v)
			{
				v2f o = PrepareParams(v);

				o.worldPos.xy = v.vertex.xy * _ClipRange0.zw + _ClipRange0.xy;
				o.worldPos.zw = Rotate(v.vertex.xy, _ClipArgs1.zw) * _ClipRange1.zw + _ClipRange1.xy;
				o.worldPos2 = Rotate(v.vertex.xy, _ClipArgs2.zw) * _ClipRange2.zw + _ClipRange2.xy;


				return o;
			}
			
			
			float4 frag (v2f i) : SV_Target
			{
				// First clip region
				float2 factor = (float2(1.0, 1.0) - abs(i.worldPos.xy)) * _ClipArgs0.xy;
				float f = min(factor.x, factor.y);

				// Second clip region
				factor = (float2(1.0, 1.0) - abs(i.worldPos.zw)) * _ClipArgs1.xy;
				f = min(f, min(factor.x, factor.y));

				// Third clip region
				factor = (float2(1.0, 1.0) - abs(i.worldPos2)) * _ClipArgs2.xy;
				f = min(f, min(factor.x, factor.y));

				// Sample the texture
				half4 col = ExecuteSDF_Global(i);
				col.rgba *= clamp(f, 0.0, 1.0);
				return col;
			}
			ENDCG
		}
	}
}