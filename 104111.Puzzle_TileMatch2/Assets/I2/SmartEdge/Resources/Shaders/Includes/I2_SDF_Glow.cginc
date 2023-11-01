half4 _GlowColor;
half4 _GlowData;  // innerGlow, edgeGlow, outerGlow, glowPower
half4 _GlowOffset;

inline float GetGlowHeight(half4 Heights, half height, half Softness, half edge)
{
#ifdef MSDFA
	//half tOut = saturate((edge- height) / (Softness)); // Sharp Outer transition // smoothstep(Edge + Softness, Edge - Softness, height);
	//half tIn = saturate((height - edge) * 20);		   // Soft inner
	//height = lerp(height, Heights.a, max(tOut, tIn));
    height = Heights.a;
#endif
	return height;
}

inline half4 GetGlowColor(half4 GlowColor, half height, half Edge, half Surface, half intensity, half innerGlow, half outerGlow, half glowPower, half2 GlowUV)
{
    #ifdef TEXTURES
        GlowColor *= tex2D(_GlowTex, GlowUV);
    #endif

    half OuterGlowFactor = saturate((Edge - height) / outerGlow);
    half InnerGlowFactor = saturate((height - Surface) / innerGlow);

    half t = max(InnerGlowFactor, OuterGlowFactor);
	t = t*t*(3 - 2*t);  // EasyInOut that normally is applied inside smoothstep
	t = 1 - pow(t, glowPower);

	return saturate(GlowColor * intensity*t);
}


inline half4 ApplyGlow(v2f i, half4 col, bool vertexModeGlow, half4 faceColor, half4 Heights, half height, half Softness, half Surface, half Edge, half2 GlowUV)
{
    half gInner, gOuter, gPower, gIntensity;
    half4 gColor;

    if (vertexModeGlow)
    {
        EXTRACT_PARAMS_GLOW
        gInner     = InnerGlow;
        gOuter     = OuterGlow;
        gPower     = GlowPower;
        gIntensity = GlowIntensity;
        gColor     = faceColor;
    }
    else
    {
        Edge      -= _GlowOffset.x;
        Surface    = Edge + _GlowData.y;
        gInner     = _GlowData.x;
        gOuter     = _GlowData.z;
        gPower     = _GlowData.w;
        gIntensity = 1;
        gColor     = _GlowColor;
    }

#ifdef MSDFA
    height = Heights.a;
#endif

    half4 glow = GetGlowColor(gColor*gColor.a, height, Edge, Surface, gIntensity, gInner, gOuter, gPower, GlowUV);
	return lerp(col + glow, glow, glow.a);
}