// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

inline half invLerp(half a, half b, half v)
{
	return saturate((v - a) / (b - a));
	//return smoothstep(a, b, v);
}

inline half invLerp01(half a, half b, half v)
{
	a = saturate(a);
	b = saturate(b);
	return saturate((v - a) / (b - a));
	//return smoothstep(a, b, v);
}

inline half _invLerp01(half a, half b, half v)
{
	return saturate((v - a) / (b - a));
}


float median( float r, float g, float b )
{
	return max(min(r,g), min(max(r,g), b));
}

inline half GetSurfaceHeight(float2 uv, out half4 Heights)
{
	//Heights = tex2Dlod(_MainTex, float4(uv, 0,0));
	Heights = tex2D(_MainTex, uv);

	#if MSDF || MSDFA
		return median(Heights.r, Heights.g, Heights.b);
	#else
		return Heights.a;
	#endif
}

inline float GetSurfaceHeightSDF(float2 uv)
{
	//return tex2Dlod(_MainTex, float4(uv, 0, 0)).a;
	return tex2D(_MainTex, uv).a;
}


inline half GetSurfaceHeight( float2 uv )
{
	half4 Heights;
	return GetSurfaceHeight( uv, Heights );
}

inline void ApplySoftMSDFA(inout float height, float4 Heights, float edge, float Softness)
{
	#ifdef MSDFA
		height = lerp(height, Heights.a, saturate(abs(height - edge) * 20));

		//float dg = saturate((edge- height) / (Softness)); // smoothstep(Edge + Softness, Edge - Softness, height);
		//dg = max(dg, saturate(abs(height - edge) * 20));
		//height = lerp(height, Heights.a, dg);
	#endif
}

inline half GetSurfaceHeightMSDFA(float2 uv, float edge, float Softness)
{
	half4 Heights;
	float height = GetSurfaceHeight(uv, Heights);
	ApplySoftMSDFA(height, Heights, edge, Softness);
	return height;
}


#if (UNITY_VERSION < 540 && !defined(SURFACE_SHADER))
inline float4 UnityObjectToClipPos(in float3 pos)
{
	return UnityObjectToClipPos(float4(pos, 1.0));
}
#endif

