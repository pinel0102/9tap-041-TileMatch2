half4 _ShadowColor;
half4 _ShadowData; // shadowx, shadowy, shadowSoftness, 0;

half4 _InnerShadowColor;
half4 _InnerShadowOffset; // shadowx, shadowy
half4 _InnerShadowData;   // expandEdge, expandMask, shadowWidth, maskSoftness;

inline half4 ApplySimpleShadow( half4 col, half4 Heights, half height, half Softness, half Edge, float2 uv )
{
	half2 shadowOffset = _I2MainTex_TexelSize.xy * 8 * _ShadowData.xy;
	half shadowWidth   = _ShadowData.z;
 	half shadowExpand  = _ShadowData.w;
 	half4 shadowColor  = _ShadowColor;
	half shadowEdge    = Edge - shadowExpand;

 	half s_height = GetSurfaceHeightMSDFA( uv+shadowOffset, shadowEdge, Softness);

	half factor = invLerp01(shadowEdge-Softness - shadowWidth, shadowEdge +Softness + shadowWidth, s_height);
    //shadowColor *= factor;// *col.a;

    return lerp(shadowColor, col, max(col.a, 1-factor));
    //return lerp(shadowColor, col, col.a);
}

#ifdef INNER_SHADOW
inline half4 ApplySimpleInnerShadow( half4 col, half4 Heights, half height, half Softness, half Edge, float2 uv )
{
	half2 shadowOffset     = _I2MainTex_TexelSize.xy * 8 * _InnerShadowOffset.xy;
 	half shadowExpandEdge  = _InnerShadowData.x;
 	half shadowExpandMask  = _InnerShadowData.y;
 	half shadowWidth 	   = _InnerShadowData.z;
 	half maskSoftness 	   = _InnerShadowData.w;

 	half shadowEdge = Edge - shadowExpandEdge;
 	half maskEdge   = Edge - shadowExpandMask;

	//Softness *= 2;

	half s_height = GetSurfaceHeightMSDFA(uv + shadowOffset, shadowEdge, Softness);

	half factorInner = _invLerp01(shadowWidth + Softness, 0, s_height - shadowEdge);
	half factorMask = _invLerp01(-maskSoftness-Softness, 0, height - maskEdge);

	half4 shadowColor = half4(_InnerShadowColor.rgb, factorInner*factorMask*_InnerShadowColor.a);

    col.rgb     = lerp(shadowColor, col, max(col.a,1-factorMask)).rgb;
	col.rgba = lerp(col.rgba, shadowColor.rgba, shadowColor.a);
	col.a = max(col.a, shadowColor.a);

	return col;
}
#endif