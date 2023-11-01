// Unpacks RGBA with 6 bits of precision from a float
inline half4 FloatToRGBA6( float Bytes )
{
	float4 vFloor = float4(64*64*64, 64*64, 64, 1);

	float4 Col = floor(Bytes.xxxx/vFloor);
		   Col = fmod(Col, 64);
		   
	return (Col / 63.0);
}

// A3 is not returned correctly
inline half4 FloatToRGB7A3(float Bytes)
{
    float4 vFloor = float4(128 * 128 * 8, 128 * 8, 8, 1);

    float4 Col = floor(Bytes.xxxx / vFloor);
           Col = fmod(Col, 128);

    return (Col / 127);
}



// Unpacks RGB with 8 bits of precision from a float
inline half3 FloatToRGB8( float Bytes )
{
	float3 vFloor = float3(256*256, 256, 1);
	
	float3 Col = floor( Bytes.xxx / vFloor );
  		   Col = fmod(Col, 256);
	return Col / 255.0;
}	


// Unpacks to 4bits values (0..1) from a 8 bits (0..1) float
inline float2 FloatToRG4( float Value )
{
	float v = Value*16;
	return float2(floor(v)/16.0, frac(v));
}

inline float2 FloatToUV( float Bytes )
{
	float2 vFloor = float2(4096, 1);
	
	//FaceUV.x = (floor(v.texcoord1.x/4096)/64)-32;
	//FaceUV.y = (fmod(v.texcoord1.x,4096)/64)-32;

	
	float2 Col = floor (Bytes.xx / vFloor);
		   Col = fmod(Col, 4096);
	return Col/64-32;
}

float4 UnpackFlags_First4( float  Bytes )
{
	return fmod(Bytes.xxxx, float4(2,4,8,16)) - float4(0,1,2,4);
}

inline float2 FloatToCharUV( float Bytes )
{
	Bytes/=4096;
	return float2( round(Bytes)/4095, frac(Bytes));
}
