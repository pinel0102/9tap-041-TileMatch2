#ifdef TEXTURES
	sampler2D _FaceTex, _OutlineTex, _GlowTex, _NormalMap;
#endif

#ifdef ENVIRONMENT_MAP
	samplerCUBE _EnvironmentMap;
#endif

half4 _GlobalData;	// (Softness, FaceSoftness, OutlineSoftness, spread1px)  // spread1px is how wide in (0..255 range) is 1 pixel of spreading  (e.g. 0.5/spread)

#include "Includes/I2_Misc.cginc"
#include "Includes/I2_Packing.cginc"
#include "Includes/I2_Lighting.cginc"
#include "Includes/I2_Bevel.cginc"
#include "Includes/I2_Shadow.cginc"

#include "Includes/I2_BasicParameters_SDF.cginc"

#include "Includes/I2_SDF_Glow.cginc"
//#include "Includes/I2_SDF_Main.cginc"

half GetBorderFactor(float2 uv, half height, half edge, half softness)
{

	float minR = saturate(edge - softness);     float maxR = saturate(edge + softness);
	float borderFactor = invLerp(minR, maxR, height);

	#if SUPER_SAMPLING
		float2 superSamplingDeltaUV = fwidth(uv);
		if (superSamplingDeltaUV.x > 0.007)
		{
			// Supersample, 4 extra points
			float4 box = float4(uv - superSamplingDeltaUV*0.354, uv + superSamplingDeltaUV*0.354);

			float4 asum = invLerp(minR, maxR, GetSurfaceHeight(box.xy))
				+ invLerp(minR, maxR, GetSurfaceHeight(box.zw))
				+ invLerp(minR, maxR, GetSurfaceHeight(box.xw))
				+ invLerp(minR, maxR, GetSurfaceHeight(box.zy));

			// weighted average, with 4 extra points having 0.5 weight each,
			// so 1 + 0.5*4 = 3 is the divisor
			borderFactor = (borderFactor + 0.5 * asum) / 2.5;  // 3.;
		}
	#endif

	return borderFactor;

}



inline half4 ExecuteSDF_GlobalSurface( v2f i, out half3 vNormal )
{
	half4 Heights;
	float height = GetSurfaceHeight( i.texcoord.xy, Heights );

    //float2 ssDeltaUV = fwidth(i.texcoord.xy);
	//float deltaTexels = 1.2*length(ssDeltaUV * _I2MainTex_TexelSize.zw);      // how many texels from this pixel to any of its neighbors
	//float toSDF       = _GlobalData.w*deltaTexels;                      // multiply the number of texels by how much the SDF can change each texel (based on the spreading)
	//float Softness    = toSDF * _GlobalData.x;                          // multiply by how many pixels we want the softness to be
	float Softness = fwidth(height)*_GlobalData.x;

#if MULTI_MODES
	bool vertexModeGlow = MODE_Glow <= i.Params1.w;
#else
	bool vertexModeGlow = false;
#endif

	//return invLerp01(-Softness, Softness, height-0.5);
	
	EXTRACT_PARAMS_GENERAL

	half4 col = 0;
	float faceSoftness = Softness + _GlobalData.y;
	half outlineSoftness = Softness + _GlobalData.z;
	vNormal = half3(0, 0, 1);

	if (!vertexModeGlow)
	{
		#ifdef TEXTURES
        FaceColor *= tex2D(_FaceTex, FaceUV);
		#endif

		#ifdef OUTLINE
			#ifdef TEXTURES
				EdgeColor *= tex2D(_OutlineTex, FaceUV);
			#endif

			//--[ Edge -> Face ]------------------
			half FaceFactor = invLerp01(Surface - outlineSoftness, Surface + outlineSoftness, height);
			col = lerp(EdgeColor, FaceColor, FaceFactor);

		#else
			half FaceFactor = 1;
			col = FaceColor;
		#endif

        // WIDGET_TEXTURE
        #if !defined(MSDF) && !defined(MSDFA)
            col.rgb = lerp(col.rgb, col.rgb*Heights.rgb, _UseWidgetTexture);
        #endif


		//--[ Transparent -> Edge ]------------------
		float BorderFactor = GetBorderFactor(i.texcoord.xy, height, Edge, faceSoftness);
		col.a *= BorderFactor;

		//--[ Bevel ]------------------
		#ifdef BEVEL
			vNormal = GetBevelNormal(height, i.texcoord.xy, Edge, faceSoftness);	 // maybe faceSoftness is not needed		
		#endif

		#if defined(NORMAL_MAP) && defined(TEXTURES)
			half3 vBumpNormal = UnpackNormal(tex2D(_NormalMap, FaceUV));
			half BumpStrength = lerp(_BumpData.y, _BumpData.x, FaceFactor);
			vBumpNormal = half3(vBumpNormal.xy*_BumpData.z, vBumpNormal.z);
			vNormal = normalize(vNormal + vBumpNormal*BumpStrength);
		#endif
		
		#ifdef ENVIRONMENT_MAP
			col = ApplyReflections( col, ViewDir, vNormal, height, Edge, FaceFactor, BorderFactor );
		#endif

		#if defined(NORMAL_MAP) || defined(BEVEL)
			col.rgb = ApplyLighting(ViewDir, col.rgb, vNormal);
		#endif
		

		#ifdef INNER_SHADOW
			col = ApplySimpleInnerShadow( col, Heights, height, outlineSoftness, Surface, i.texcoord.xy );
		#endif

        col.rgb *= col.a;

		#ifdef SHADOW
			col = ApplySimpleShadow(col, Heights, height, faceSoftness, Edge, i.texcoord.xy);
		#endif
	}

	#if defined(GLOW) || defined(MULTI_MODES)
		#ifdef GLOW
			bool useGlow = true;
		#else
			bool useGlow = vertexModeGlow;
		#endif
			if (useGlow)
				col = ApplyGlow(i, col, vertexModeGlow, FaceColor, Heights, height, faceSoftness, Surface, Edge, FaceUV);
	#endif

	col *= GlobalAlpha;
	return  col;
}

inline half4 ExecuteSDF_Global(inout v2f i)
{
	half3 n;
	return ExecuteSDF_GlobalSurface(i, n);
}
