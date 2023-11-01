using UnityEngine;
using System.Collections;

namespace I2.SmartEdge
{
	public class BytePacking
	{
		#region RGB8
		// Pack 3 bytes into a float (each byte has 8 bits of precision)
		public static float PackRGB8( byte R, byte G, byte B )
		{
			//int r = (R>>1);//2;
			//int g = (G>>1);//2;
			//int b = (B>>1);//2;

			//R = (byte)(R>>1);
			//G = (byte)(G>>1);
			return (float)((R << 16) | (G << 8) | B);
			//return (float)((R << 15) | (G << 8) | B);
		}
		
		public static float PackRGB8( float R, float G, float B )
		{
			return PackRGB8 ((byte)(Mathf.Clamp01 (R) * 255), (byte)(Mathf.Clamp01 (G) * 255), (byte)(Mathf.Clamp01 (B) * 255));
		}		
		
		public static Color32 UnPackRGB8( float Bytes )
		{
			int val = (int)Bytes;
			int R = val>>16;
			int G = (val>>8) % 256;
			int B = val % 256;
			return new Color32( (byte)R, (byte)G, (byte)B, 255);
		}

		#endregion

		#region RGBA6
		// Pack 4 bytes into a float (each byte has 6 bits of precision)
		public static float PackRGBA6( Color32 c )
		{
			//return PackRGBA6 (c.r, c.g, c.b, c.a);
			int r = (c.r>>2);
			int g = (c.g>>2);
			int b = (c.b>>2);
			int a = (c.a>>2);
			return (float)((r << 18) | (g << 12) | (b << 6) | a);
		}

		// Pack 4 float [0..1] into a float (each byte has 6 bits of precision)
		public static float PackRGBA6( float R, float G, float B, float A )
		{
			//return PackRGBA6 ((byte)(Mathf.Clamp01 (R) * 255), (byte)(Mathf.Clamp01 (G) * 255), (byte)(Mathf.Clamp01 (B) * 255), (byte)(Mathf.Clamp01 (A) * 255));

			int r = R>1?63: (R<0?0: ((int)(R*0xff))>>2);
			int g = G>1?63: (G<0?0: ((int)(G*0xff))>>2);
			int b = B>1?63: (B<0?0: ((int)(B*0xff))>>2);
			int a = A>1?63: (A<0?0: ((int)(A*0xff))>>2);
			return (float)((r << 18) | (g << 12) | (b << 6) | a);
		}
		
		// Pack 4 bytes into a float (each byte has 6 bits of precision)
		public static float PackRGBA6( byte R, byte G, byte B, byte A )
		{
			int r = (R>>2);
			int g = (G>>2);
			int b = (B>>2);
			int a = (A>>2);
			return (float)((r << 18) | (g << 12) | (b << 6) | a);
/*			int r = (R>>3);
			int g = (G>>3);
			int b = (B>>2);
			int a = (A>>2);
			return (float)((r << 17) | (g << 12) | (b << 6) | a);
			int r = (R>>2);
			int g = (G>>2);
			int b = (B>>3);
			int a = (A>>3);
			return (float)((r << 17) | (g << 11) | (b << 6) | (a<<1));*/
		}
		
		public static Color32 UnPackRGBA6( float Bytes )
		{
			int val = (int)Bytes;
			int R = val>>18;
			int G = (val>>12) % (1<<6);
			int B = (val>>6) % (1<<6);
			int A = val % (1<<6);
			float Factor = 255/63.0f;
			return new Color32( (byte)(R*Factor), (byte)(G*Factor), (byte)(B*Factor), (byte)(A*Factor));
		}

		#endregion

		#region XY4
		// Pack 2 values [0..1] into a byte (each value has 4 bits of precision)
		public static byte PackXY4( float X, float Y )
		{
			int x = (int)(Mathf.Clamp01 (X)*15);
			int y = (int)(Mathf.Clamp01 (Y)*15);
			return (byte)((x<<4) + y);
		}
		
		public static Vector2 UnPackXY4( byte Value )
		{
			float x = (Value>>4)/15f;
			float y = (Value % 16)/15f;
			return new Vector2(x, y);
		}

		#endregion

		#region UVUV6

		public static float PackUV( Vector2 uv )
		{
			// Move [-32..32) into [0..64)
			//uv.x = Mathf.Sign(uv.x) * Mathf.Repeat(Mathf.Abs(uv.x), 32) + 31;
			//uv.y = Mathf.Sign(uv.y) * Mathf.Repeat(Mathf.Abs(uv.y), 32) + 31;
			//int u = Mathf.FloorToInt(uv.x*64) % 4096;
			//int v = Mathf.FloorToInt(uv.y*64) % 4096;

			int u = ((int)(uv.x*64) + (32*64)) % 4096;
			int v = ((int)(uv.y*64) + (32*64)) % 4096;

			return (float)((u*4096) | v);
		}
		
		public static Vector2 UnPackUV( float Bytes )
		{
			float u = Mathf.FloorToInt(Bytes/4096f)/64f;
			float v = Mathf.Repeat(Bytes,4096)/64f;

			return new Vector2(u,v);
		}
		#endregion

        #region CharUV
        public static float PackCharUV( Vector2 uv )
        { 
            // Move [0..1] -> [0..4096]
            int range = (1<<12)-1;   // 8191
			int u= (uv.x>1) ? range : (uv.x<0 ? 0 : ((int)(uv.x*range)));
			int v= (uv.y>1) ? range : (uv.y<0 ? 0 : ((int)(uv.y*range)));
            //int u = (int)(Mathf.Clamp01 (uv.x)*range);
            //int v = (int)(Mathf.Clamp01 (uv.y)*range);

            return (float)((u<<12) | v);
        }

		public static float PackCharUV( float UVx, float UVy )
		{ 
			// Move [0..1] -> [0..4096]
			int range = (1<<12)-1;   // 8191
			int u= (UVx>1) ? range : (UVx<0 ? 0 : ((int)(UVx*range)));
			int v= (UVy>1) ? range : (UVy<0 ? 0 : ((int)(UVy*range)));
			//int u = (int)(Mathf.Clamp01 (uv.x)*range);
			//int v = (int)(Mathf.Clamp01 (uv.y)*range);

			return (float)((u<<12) | v);
		}

        public static Vector2 UnPackCharUV( float Bytes)
        {
            int range = (1<<12)-1;   // 8191
            int val = (int)Bytes;
            float u = (val>>12)/(float)range;
            float v = (val & range)/(float)range;
            return new Vector2(u,v);
        }

        #endregion

		#region Mass Packing

		public static Vector3 Pack9Floats( byte v1, byte v2, byte v3, byte v4, byte v5, byte v6, byte v7, byte v8, byte v9 )
		{
			return new Vector3( PackRGB8(v1, v2, v3), 
			                   	PackRGB8(v4, v5, v6), 
			                   	PackRGB8(v7, v8, v9) );
		}
		
		public static Vector4 Pack12Floats( byte v1, byte v2, byte v3, byte v4, byte v5, byte v6, byte v7, byte v8, byte v9, byte v10, byte v11, byte v12 )
		{
			return new Vector4( PackRGB8(v1,  v2,  v3), 
			                   	PackRGB8(v4,  v5,  v6), 
			                   	PackRGB8(v7,  v8,  v9), 
			                   	PackRGB8(v10, v11, v12) );
		}
		
		#endregion
	}
}