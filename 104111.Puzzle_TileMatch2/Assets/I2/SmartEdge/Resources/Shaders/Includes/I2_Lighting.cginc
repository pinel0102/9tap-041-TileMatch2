
half4 _LightData; // (Intensity, Ambient, --, --);
half3 _LightDir;
half4 _LightSpecularData = 0; // (Start, Smoothness, Power, --);
half4 _LightSpecularColor = 0;

half4 _BumpData;
half4 _EnvReflectionColor_Face;
half4 _EnvReflectionColor_Outline;
half3 _EnvReflectionMatrixX;
half3 _EnvReflectionMatrixY;
half3 _EnvReflectionMatrixZ;
half4 _EnvReflectionData;  // (FresnelBias, FresnelScale, --, --)


float2 RotateXY( float2 val, float angle )
{
	float s,c;
	sincos(angle, s, c);
	return float2( val.x * c - val.y * s, 
				   val.x * s + val.y * c);
}

inline half3 ApplyLighting(half3 viewDir, half3 col, half3 n)
{
	half highlight     = _LightData.x;
	half shadow        = _LightData.y;

	half specStart	   = _LightSpecularData.x;
	half specSmoothness= _LightSpecularData.y;
	half specPow	   = _LightSpecularData.z;

	half NdotL = dot(n, _LightDir);
	half MidDif = _LightDir.z;		//dot(half3(0, 0, 1), _LightDir);

	//--[ Overlay blending ]------------
	half dif = 2*saturate((NdotL - MidDif) + 0.5);

	if (dif < 1)
	{
		col = col*lerp(shadow, 1, dif);
	}
	else
	{
		dif = highlight*(dif - 1);
		col = 1 - (1-col) * (1-dif);
	}

	//--[ Specular ]-------------------
	
	float spec = saturate((NdotL - specStart) / specSmoothness);
	spec = pow(spec, specPow);
	col = lerp(col, _LightSpecularColor.rgb, spec*_LightSpecularColor.a);
	return col;
}

#ifdef ENVIRONMENT_MAP
inline half4 ApplyReflections( half4 col, half3 ViewDir, half3 vNormal, half height, half edge, half ouline_OR_face, half borderFactor )
{
	float bias     = _EnvReflectionData.x;
	float softness = _EnvReflectionData.y;
	
	half3 r = reflect(ViewDir, vNormal);
	half4 ReflectionColor = texCUBE(_EnvironmentMap, r);

	half4 ReflectionTint = lerp(_EnvReflectionColor_Outline, _EnvReflectionColor_Face, ouline_OR_face);
	half4 reflection = saturate(ReflectionColor*ReflectionTint);

	// Make sure that the reflection is always shown (even when face.a = 0) this is needed for the glass effect
	reflection.a = max(1-reflection.a, dot(reflection.rgb, 1 / 3.0))*borderFactor;

	// screen blend the color and reflection
	reflection = 1 - (1 - col*ReflectionTint.a)*(1 - reflection);

	float fresnel = saturate((height - edge - bias) / softness);
	col = lerp(col, reflection, fresnel);
	return col;
}
#endif