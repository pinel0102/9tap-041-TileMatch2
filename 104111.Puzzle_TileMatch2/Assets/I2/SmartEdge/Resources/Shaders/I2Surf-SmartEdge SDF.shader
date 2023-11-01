Shader "GUI/I2 SmartEdge/Surf SDF"
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

        _I2MainTex_TexelSize("_I2MainTex_TexelSize", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        LOD 400

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
        //ZWrite Off
        //ZTest[unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask[_ColorMask]


        CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members vertex)
#pragma exclude_renderers d3d11 xbox360
        #pragma surface surf Lambert alpha nofog nolightmap nodirlightmap vertex:vert
        #pragma target 3.0

        #include "UnityCG.cginc"
        #include "UnityUI.cginc"

        //#pragma multi_compile SDF MSDF MSDFA
        /*#pragma multi_compile __ MULTI_MODES
        #pragma multi_compile __ TEXTURES
        #pragma multi_compile __ GLOW_SHADOW
        #pragma multi_compile __ ENVIRONMENT_MAP
        #pragma multi_compile __ BEVEL
        #pragma multi_compile __ INNER_SHADOW
        #pragma multi_compile __ SUPER_SAMPLING
        */
        #if TEXTURES && BEVEL
			#define NORMAL_MAP
		#endif
#define BEVEL true
		#if GLOW_SHADOW
			#define GLOW
			#define SHADOW
		#endif

        #define SURFACE_SHADER true
#define MSDFA true

        struct appdata_t {
            float4 vertex     : POSITION;
            float4 color      : COLOR;
            float2 texcoord   : TEXCOORD0;
            float2 texcoord1  : TEXCOORD1;

            float3 normal   : NORMAL;
            float4 tangent    : TANGENT;
        };

        struct v2f
        {
            //float4 vertex;
            float4 color     : TEXCOORD0;
            float4 texcoord  : TEXCOORD1;

            half4  Params1   : TEXCOORD2;
            half4  Params2   : TEXCOORD3;
            float4 Params3   : TEXCOORD4;
        };
        #define Input v2f


        sampler2D _MainTex;
        float4 _Color;
        float _UseWidgetTexture;

        #define EXTRA_VERT_PARAM v.tangent.w


        #include "Includes/I2_SDF.cginc"


        void vert(inout appdata_t v, out Input o)
        {
            o = PrepareParams(v);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            half3 vnormal;
            float4 color = ExecuteSDF_GlobalSurface(IN, vnormal);
                //vnormal = vnormal.xyyz
                o.Albedo = color.rgb;
                //o.Albedo = 1;//vnormal*0.5+0.5;// color.rgb;
            //o.Normal = normalize(vnormal);

            //o.Specular = _Specular.a;
            //o.Gloss = _Shininess;
            o.Alpha = color.a;

            #ifdef UNITY_UI_ALPHACLIP
              //  clip(color.a - 0.001);
            #endif
        }

        ENDCG
    }
}
