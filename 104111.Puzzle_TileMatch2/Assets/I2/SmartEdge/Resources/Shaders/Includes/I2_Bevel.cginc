float4 _I2MainTex_TexelSize;// same as _MainTex_TexelSize, but some unity version have that value wrong

half4 _BevelData;  // (Offset, Width, _Depth, _Curfe)
half4 _BevelWave;  // (InnerSoftness, OuterSoftness, _Clamp, --)

//#define HIGH_QUALITY_NORMAL
//#define CRAZY_QUALITY_NORMAL

#ifdef CRAZY_QUALITY_NORMAL
inline half2 GetSurfaceGradient(float2 uv, float min, float width)
{
    float3 dpixel1 = float3(_I2MainTex_TexelSize.xy*0.5, 0);
    float3 dpixel2 = float3(_I2MainTex_TexelSize.xy*1, 0);

    float h00 = GetSurfaceHeight(uv + float2(-dpixel2.x, -dpixel2.y));
    float h10 = GetSurfaceHeight(uv + float2(-dpixel1.x, -dpixel2.y));
    float h20 = GetSurfaceHeight(uv + float2(         0, -dpixel2.y));
    float h30 = GetSurfaceHeight(uv + float2( dpixel1.x, -dpixel2.y));
    float h40 = GetSurfaceHeight(uv + float2( dpixel2.x, -dpixel2.y));

    float h01 = GetSurfaceHeight(uv + float2(-dpixel2.x, -dpixel1.y));
    float h11 = GetSurfaceHeight(uv + float2(-dpixel1.x, -dpixel1.y));
    float h21 = GetSurfaceHeight(uv + float2(         0, -dpixel1.y));
    float h31 = GetSurfaceHeight(uv + float2( dpixel1.x, -dpixel1.y));
    float h41 = GetSurfaceHeight(uv + float2( dpixel2.x, -dpixel1.y));

    float h02 = GetSurfaceHeight(uv + float2(-dpixel2.x, 0));
    float h12 = GetSurfaceHeight(uv + float2(-dpixel1.x, 0));
    //float h22 = GetSurfaceHeight(uv + float2(         0, 0));
    float h32 = GetSurfaceHeight(uv + float2( dpixel1.x, 0));
    float h42 = GetSurfaceHeight(uv + float2( dpixel2.x, 0));

    float h03 = GetSurfaceHeight(uv + float2(-dpixel2.x, dpixel1.y));
    float h13 = GetSurfaceHeight(uv + float2(-dpixel1.x, dpixel1.y));
    float h23 = GetSurfaceHeight(uv + float2(         0, dpixel1.y));
    float h33 = GetSurfaceHeight(uv + float2( dpixel1.x, dpixel1.y));
    float h43 = GetSurfaceHeight(uv + float2( dpixel2.x, dpixel1.y));

    float h04 = GetSurfaceHeight(uv + float2(-dpixel2.x, dpixel2.y));
    float h14 = GetSurfaceHeight(uv + float2(-dpixel1.x, dpixel2.y));
    float h24 = GetSurfaceHeight(uv + float2(         0, dpixel2.y));
    float h34 = GetSurfaceHeight(uv + float2( dpixel1.x, dpixel2.y));
    float h44 = GetSurfaceHeight(uv + float2( dpixel2.x, dpixel2.y));


    // The Sobel X kernel is:     // The Sobel Y kernel is:
    //                            //
    // [ -5   -4  0  4  5 ]       // [ -5  -8 -10  -8 -5 ]
    // [ -8  -10  0 10  8 ]       // [ -4 -10 -20 -10 -4 ]
    // [ -10 -20  0 20 10 ]       // [  0   0   0   0  0 ]
    // [ -8  -10  0 10  8 ]       // [  4  10  20  10  4 ]
    // [ -5   -4  0  4  5 ]       // [  5   8  10   8  5 ]
         

    // Sobel Filter
    float Gx =  -5*h00   -4*h10 + 0*h20 +  4*h30 +  5*h40
                -8*h01  -10*h11 + 0*h21 + 10*h31 +  8*h41
               -10*h02  -20*h12 +       + 20*h32 + 10*h42
                -8*h03  -10*h13 + 0*h23 + 10*h33 +  8*h43
                -5*h04   -4*h14 + 0*h24 +  4*h34 +  5*h44;

    float Gy =  -5*h00  -8*h10  -10*h20  - 8*h30 - 5*h40
                -4*h01 -10*h11  -20*h21 - 10*h31 - 4*h41
                +0*h02  +0*h12          + 0*h32 + 0*h42
                +4*h03 +10*h13  +20*h23 + 10*h33 + 4*h43
                +5*h04  +8*h14  +10*h24  + 8*h34 + 5*h44;

    // Scharr Filter
    //float Gx = 3 * h00 - 3 * h20 + 10 * h01 - 10 * h21 + 3 * h02 - 3 * h22;
    //float Gy = 3 * h00 + 10 * h10 + 3 * h20 - 3 * h02 - 10 * h12 - 3 * h22;
	return normalize(float2(-Gx, -Gy));
}

#elif defined(HIGH_QUALITY_NORMAL)

inline half2 GetSurfaceGradient(float2 uv, float min, float width)
{
    float3 dpixel = float3(_I2MainTex_TexelSize.xy, 0);

    float h00 = GetSurfaceHeight(uv + float2(-dpixel.x, -dpixel.y));
    float h10 = GetSurfaceHeight(uv + float2(0, -dpixel.y));
    float h20 = GetSurfaceHeight(uv + float2(dpixel.x, -dpixel.y));

    float h01 = GetSurfaceHeight(uv + float2(-dpixel.x, 0));
    //float h11 = GetSurfaceHeight(uv + float2(0, 0));
    float h21 = GetSurfaceHeight(uv + float2(dpixel.x, 0));

    float h02 = GetSurfaceHeight(uv + float2(-dpixel.x, dpixel.y));
    float h12 = GetSurfaceHeight(uv + float2(0, dpixel.y));
    float h22 = GetSurfaceHeight(uv + float2(dpixel.x, dpixel.y));

    // The Sobel X kernel is:   // The Sobel Y kernel is:
    //                          //
    // [ 1.0  0.0  -1.0 ]       // [  1.0    2.0    1.0 ]
    // [ 2.0  0.0  -2.0 ]       // [  0.0    0.0    0.0 ]
    // [ 1.0  0.0  -1.0 ]       // [ -1.0   -2.0   -1.0 ]

    // Sobel Filter
    float Gx = h00 - h20 + 2 * h01 - 2 * h21 + h02 - h22;
    float Gy = h00 + 2 * h10 + h20 - h02 - 2 * h12 - h22;

    // Scharr Filter
    //float Gx = 3 * h00 - 3 * h20 + 10 * h01 - 10 * h21 + 3 * h02 - 3 * h22;
    //float Gy = 3 * h00 + 10 * h10 + 3 * h20 - 3 * h02 - 10 * h12 - 3 * h22;
    return normalize(float2(Gx, Gy));
}

#else

inline half2 GetSurfaceGradient(float2 uv, float min, float width)
{
	float3 dpixel = float3(/*1.5 * */3*_I2MainTex_TexelSize.xy, 0);
	//float3 dpixel = float3((1).xx/2048.0, 0);

#if MSDF
        half4 h = half4(GetSurfaceHeight(uv - dpixel.xz), GetSurfaceHeight(uv + dpixel.xz),
						GetSurfaceHeight(uv - dpixel.zy), GetSurfaceHeight(uv + dpixel.zy));
#else
		half4 h = half4(GetSurfaceHeightSDF(uv - dpixel.xz), GetSurfaceHeightSDF(uv + dpixel.xz),
						GetSurfaceHeightSDF(uv - dpixel.zy), GetSurfaceHeightSDF(uv + dpixel.zy));
#endif

		

    //h = saturate((h - min) / width);	// h -> [0..1]
		//half2 grad = normalize(half2(h.x - h.y, h.z - h.w));
		half2 grad = (half2(h.x - h.y, h.z - h.w));
		half2 size = sqrt(dot(grad, grad));
		grad /= max(0.1, size);

        return  grad.xy;
}

#endif


inline half3 GetBevelNormal(half Height, float2 uv, half edge, half Softness)
{
	half Bevel_Offset    = _BevelData.x-Softness;
	half Bevel_Width	 = _BevelData.y;
	half Bevel_Depth     = _BevelData.z;
	half Bevel_Curve     = _BevelData.w;

	half Bevel_InSoften  = _BevelWave.x;
	half Bevel_OutSoften = _BevelWave.y;
	half Bevel_Clamp     = _BevelWave.z;

    half2 grad = GetSurfaceGradient(uv, edge + Bevel_Offset, Bevel_Width);
	half3 n = half3(grad*.5*Bevel_Depth, 0.5);

	//--[ Curve /\ ]----------------------------
	float f = saturate((Height - edge - Bevel_Offset) / Bevel_Width);	// h -> [0..1]
	if (f <= Bevel_Curve)		// section ___/---
	{
		f = f / Bevel_Curve;
	}
	else						// section ---\___
	{
		f = 1 - saturate(f - Bevel_Curve) / (1 - Bevel_Curve);// max(0.001, 1 - Bevel_Curve);
		n.xy *= -1;
	}

	//--[ Smoothing ]----------------

	float fOut = saturate(f / Bevel_OutSoften);		//smoothstep(0, Bevel_OutSoften, f);
	float fIn = invLerp(0, -Bevel_InSoften, saturate(f + Bevel_Clamp) - 1); //saturate((1 - f- Bevel_Clamp) / Bevel_InSoften);	//smoothstep(0, -Bevel_InSoften, f-1);

	n.xy *= min(fIn, fOut);				// lerp(half3(0, 0, 1), n, min(fIn, fOut));

	return normalize(n);
}