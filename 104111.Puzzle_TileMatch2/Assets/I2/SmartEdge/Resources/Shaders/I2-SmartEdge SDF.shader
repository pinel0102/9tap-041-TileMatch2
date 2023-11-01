Shader "GUI/I2 SmartEdge/SDF"
{
	Properties
	{
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_FaceTex ("Face Texture", 2D) = "white" {}
		_OutlineTex ("Outline Texture", 2D) = "white" {}
		_GlowTex ("Glow Texture", 2D) = "white" {}
		_NormalMap ("NormalMap", 2D) = "bump" {}

		_Color("Tint", Color) = (1,1,1,1)
		_UseWidgetTexture("Use Widget Texture", Float) = 0

		_GlobalData("Global Data", float) = (0,0,0,0)
		_BevelData("Bevel Data", Vector) = (0,0,0,0)
		_BevelWave("Bevel Wave", Vector) = (0,0,0,0)
		_LightData("Light Data", Vector) = (0,0,0,0)
		_LightDir("LightDir", Vector) = (0,0,1,0)
		_LightSpecularData("Light Data", Vector) = (0,0,0,0)
		_LightSpecularColor("Light Specular Color", Color) = (1,1,1,0)

		_BumpData("Bump Data", Vector) = (0,0,0,0)
		_EnvReflectionColor_Face("ReflectionColor Face", Color) = (0,0,0,0)
		_EnvReflectionColor_Outline("ReflectionColor Outline", Color) = (0,0,0,0)
		_EnvironmentMap("Environment Texture", Cube) = "white" {}
		_EnvReflectionMatrixX("Environment Matrix X", Vector) = (1,0,0,0)
		_EnvReflectionMatrixY("Environment Matrix Y", Vector) = (0,1,0,0)
		_EnvReflectionMatrixZ("Environment Matrix Z", Vector) = (0,0,1,0)
		_EnvReflectionData("Environment Data", Vector) = (0,0,1,0)
			


		// Used only if Glow is baked
		_GlowColor("Glow Color", Color) = (0,0,0,0)
		_GlowData("Glow Data", Vector) = (0,0,0,0)
		_GlowOffset("Glow Offset", Vector) = (0,0,0,0)

		// Used only if Shadow is baked
		_ShadowColor("Shadow Color", Color) = (0,0,0,0)
		_ShadowData("Shadow Data", Vector) = (0,0,0,0)

		_InnerShadowColor("Inner Shadow Color", Color) = (0,0,0,0)
		_InnerShadowOffset("Inner Shadow Offset", Vector) = (0,0,0,0)
		_InnerShadowData("Inner Shadow Data", Vector) = (0,0,0,0)

		_I2MainTex_TexelSize("_I2MainTex_TexelSize", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend One OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
			CGPROGRAM
			//#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			// #pragma multi_compile __ UNITY_UI_ALPHACLIP


			#pragma multi_compile SDF MSDF MSDFA
			#pragma multi_compile __ OUTLINE
			#pragma multi_compile __ GLOW
			#pragma multi_compile __ SHADOW
			#pragma multi_compile __ BEVEL
			#pragma multi_compile __ INNER_SHADOW
			#pragma multi_compile __ SUPER_SAMPLING
			#pragma multi_compile __ TEXTURES
			#pragma multi_compile __ MULTI_MODES

			#include "UnityCG.cginc"

			#if TEXTURES && BEVEL
				#define NORMAL_MAP true
				#define ENVIRONMENT_MAP true
			#endif

			//#if GLOW_SHADOW
			//	#define GLOW
			//	#define SHADOW
			//#endif


			struct appdata_t {
				float4 vertex     : POSITION;
				float4 color      : COLOR;
				float2 texcoord   : TEXCOORD0;
				float2 texcoord1  : TEXCOORD1;

				//float3 normal   : NORMAL;
				float4 tangent    : TANGENT;
			};

			struct v2f {
				float4 vertex    : SV_POSITION;
				float4 color     : TEXCOORD0;
				float4 texcoord  : TEXCOORD1;

				float4 worldPosition : TEXCOORD2;
				half4  Params1   : TEXCOORD3;
				half4  Params2   : TEXCOORD4;
                float4 Params3   : TEXCOORD5;
            };

			sampler2D _MainTex;	
			float4 _Color;		
			float _UseWidgetTexture;

			#define EXTRA_VERT_PARAM v.tangent.w


			#include "Includes/I2_SDF.cginc"

			v2f vert (appdata_t v)
			{
				v2f o = PrepareParams(v);
				o.worldPosition = v.vertex;

				return o;
			}
			
#if (UNITY_VERSION >= 520)
			float4 _ClipRect;
			inline float UnityGet2DClipping(in float2 position, in float4 clipRect)
			{
				float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
				return inside.x * inside.y;
			}
#endif

			half4 frag (v2f i) : SV_Target
			{
                half4 color = ExecuteSDF_Global(i);

				#if UNITY_VERSION >= 520
				color *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
				#endif

				//clip(color.a - 0.001);
				return color;
			}
			ENDCG 
		}		
	}
}