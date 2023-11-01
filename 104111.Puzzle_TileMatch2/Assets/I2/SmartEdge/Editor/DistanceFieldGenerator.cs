using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace I2.SmartEdge
{
	public class DistanceFieldGenerator
	{
	    #region Variables

		public enum ChannelValue{ R, G, B, A, MaxRGB, MaxRGBA, AvgRGB, AvgRGBA }
		public enum ChannelOutput { DistanceField, Zero, One, SourceR, SourceG, SourceB, SourceA }
		
	    #endregion

	    #region Generation

		public static byte[] GetValuesFromTexture (Texture2D InputTexture, ChannelValue Channel)
		{
			Color32[] colors = InputTexture.GetPixels32 ();
			return GetValuesFromColors (colors, Channel);
		}

		public static byte[] GetValuesFromColors( Color32[] Colors, ChannelValue Channel )
		{
			byte[] values = new byte[Colors.Length];
			for (int i=0, imax=values.Length; i<imax; ++i)
			{
				switch (Channel)
				{
					case ChannelValue.R : values [i] = Colors [i].r; break;
					case ChannelValue.G : values [i] = Colors [i].g; break;
					case ChannelValue.B : values [i] = Colors [i].b; break;
					case ChannelValue.A : values [i] = Colors [i].a; break;
					case ChannelValue.MaxRGB 	: values [i] = (byte)Mathf.Max (Colors [i].r, Mathf.Max(Colors [i].g, Mathf.Max(Colors [i].b, Colors [i].a))); break;
					case ChannelValue.MaxRGBA 	: values [i] = (byte)Mathf.Max (Colors [i].r, Mathf.Max(Colors [i].g, Colors [i].b)); break;
					case ChannelValue.AvgRGB 	: values [i] = (byte)((Colors [i].r + Colors [i].g + Colors [i].b)/3); break;
					case ChannelValue.AvgRGBA 	: values [i] = (byte)((Colors [i].r + Colors [i].g + Colors [i].b + Colors [i].a)/4); break;
				}
			}

			return values;
		}

		public static void GenerateDistanceField( Texture2D InputTexture, ChannelValue Channel, out byte[] values, float spread, int downscale, FreeTypeI2.eDownscaleType downscaleType = FreeTypeI2.eDownscaleType.Center)
		{
			values = GetValuesFromTexture (InputTexture, Channel);
			GenerateDistanceField(ref values,InputTexture.width, InputTexture.height, spread, downscale, downscaleType);
		}

		public static void GenerateDistanceField( Color32[] colors, ChannelValue Channel, int width, int height, float spread, int downscale, out byte[] DistanceField, FreeTypeI2.eDownscaleType downscaleType = FreeTypeI2.eDownscaleType.Center)
		{
			DistanceField = GetValuesFromColors(colors,Channel);
			GenerateDistanceField (ref DistanceField, width, height, spread, downscale, downscaleType);
		}


		public static void GenerateDistanceField( ref byte[] values, int width, int height, float spread, int downscale, FreeTypeI2.eDownscaleType downscaleType)
		{
			FreeTypeI2.GenerateSDF( ref values, width, height, (int)spread, downscale, (int)downscaleType );
		}

		#endregion


		#region Output

		public static void CreateOutput( byte[] DistanceField, Color32[] Source, Color32[] Output, ChannelOutput OutR=ChannelOutput.One, ChannelOutput OutG=ChannelOutput.One, ChannelOutput OutB=ChannelOutput.One, ChannelOutput OutA=ChannelOutput.DistanceField)
		{
			if (Output == null)
				Output = Source;

			for (int i=0, imax=Source.Length; i<imax;++i)
			{
				Color32 c = Color.black;
				c.r = GetChannelOutput( i, DistanceField, Source, OutR );
				c.g = GetChannelOutput( i, DistanceField, Source, OutG );
				c.b = GetChannelOutput( i, DistanceField, Source, OutB );
				c.a = GetChannelOutput( i, DistanceField, Source, OutA );
				Output[i] = c;
			}
		}

		static byte GetChannelOutput( int Index, byte[] DistanceField, Color32[] Source, ChannelOutput Output )
		{
			switch (Output)
			{
				case ChannelOutput.One 				: return (byte)255;
				case ChannelOutput.Zero 			: return (byte)0;
				case ChannelOutput.SourceR 			: return Source[Index].r;
				case ChannelOutput.SourceG 			: return Source[Index].g;
				case ChannelOutput.SourceB 			: return Source[Index].b;
				case ChannelOutput.SourceA 			: return Source[Index].a;
				case ChannelOutput.DistanceField 	: return DistanceField[Index];
				default : return DistanceField[Index];
			}
		}

		#endregion
	}
}