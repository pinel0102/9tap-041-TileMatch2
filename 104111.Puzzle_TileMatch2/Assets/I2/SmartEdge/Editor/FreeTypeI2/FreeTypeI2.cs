using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace I2.SmartEdge
{
	public class FreeTypeI2
	{
        #if UNITY_EDITOR_64 && !UNITY_EDITOR_OSX
        public const string libName = "I2FreeType26_x64";
        #else
        public const string libName = "I2FreeType26";
        #endif


#region Structures

		public enum eDownscaleType { Center, Average, Mixed }
		
        [StructLayout(LayoutKind.Sequential)]
		public struct CharRect
		{
			public int Width, Height;
		}

        [StructLayout(LayoutKind.Sequential)]
		public struct CharInfo
		{
			public int Character;
			public int Width, Height;
			public IntPtr image;

			public int Advance;
			public int Verts_X, Verts_Y, 
					   Verts_Width, Verts_Height;
		};

        [StructLayout(LayoutKind.Sequential)]
        public struct KerningInfo
        {
            public int Character;
            public int NextCharacter;
            public int  offset;
        };

#endregion

		public static Rect[] GetCharactersRect (int FontSize, string characters, int spread, float Downscale)
		{
			var i2rects = new CharRect[characters.Length];
			if (0!=GetCharactersRect(FontSize, StrToIntArray(characters), spread, Downscale, i2rects))
				return null;
			var rects = new Rect[i2rects.Length];
			for (int i=0; i<i2rects.Length; ++i)
				rects[i] = new Rect(0,0,(float)i2rects[i].Width, (float)i2rects[i].Height);
			return rects;
		}

        public static string GetMissingCharacters( string characters )
        {
            var sb = new System.Text.StringBuilder();
            var i2rects = new CharRect[characters.Length];
            if (0 != GetCharactersRect(20, StrToIntArray(characters), 1, 1, i2rects))
                return characters;

            for (int i = 0; i < i2rects.Length; ++i)
                if (i2rects[i].Width == 0 && i2rects[i].Height <= 0)
                    sb.Append(characters[i]);

            return sb.ToString();
        }

        // Gets a texture from an IntPtr and flips it vertically (texture from C++ comes inverted vertically)
        public static Texture2D GetTextureA( IntPtr ptr, int w, int h )
		{
            if (w <= 0 || h <= 0 || ptr.ToInt64()==0)
                return new Texture2D(0,0);

			var bufferSize = w * h;
			var imageBytes = new byte[bufferSize];
			Marshal.Copy(ptr, imageBytes, 0, bufferSize );
			
			Color32[] colors = new Color32[bufferSize];
			for (int y=0; y<h; ++y)
				for (int x=0; x<w; ++x)
				{
					colors[x + y*w] = new Color32(0,0,0, imageBytes[x + (h-y-1)*w]);
				}
			Texture2D texture = new Texture2D(w, h);
			texture.SetPixels32(colors);
			return texture;
		}

        public static Texture2D GetTextureRGB( IntPtr ptr, int w, int h )
        {
            if (w <= 0 || h <= 0 || ptr.ToInt64() == 0)
                return new Texture2D(0,0);

            var bufferSize = w * h*3;
            var imageBytes = new byte[bufferSize];
            Marshal.Copy(ptr, imageBytes, 0, bufferSize );

            Color32[] colors = new Color32[w*h];
            for (int y=0; y<h; ++y)
                for (int x=0; x<w; ++x)
                {
                    int idx = x + (h - y - 1) * w;
                    colors[x + y * w] = new Color32(imageBytes[idx*3], imageBytes[idx*3+1], imageBytes[idx*3+2], 255);
                }
            Texture2D texture = new Texture2D(w, h);
            texture.SetPixels32(colors);
            return texture;
        }

        float median( float r, float g, float b )
        {
            return Mathf.Max(Mathf.Min(r,g), Mathf.Min(Mathf.Max(r,g), b));
        }
        public static Texture2D GetTextureRGBA( IntPtr ptr, int w, int h )
        {
            if (w <= 0 || h <= 0 || ptr.ToInt64() == 0)
                return new Texture2D(0,0);

            var bufferSize = w * h*4;
            var imageBytes = new byte[bufferSize];
            Marshal.Copy(ptr, imageBytes, 0, bufferSize );

            Color32[] colors = new Color32[w*h];
            for (int y=0; y<h; ++y)
                for (int x=0; x<w; ++x)
                {
                    int idx = x + (h - y - 1) * w;
                    int cidx = x + y * w;
                    colors[cidx] = new Color32(imageBytes[idx*4+0], imageBytes[idx*4+1], imageBytes[idx*4+2], imageBytes[idx*4+3]);
                    if (colors[cidx].a<=0)
                        colors[cidx] = new Color32(0,0,0,0);
                }
            Texture2D texture = new Texture2D(w, h);
            texture.SetPixels32(colors);
            return texture;
        }


		public static bool LoadFont( Font font, int faceIdx )
		{
			string fileName = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + UnityEditor.AssetDatabase.GetAssetPath(font);
			return 0==LoadFont (fileName, faceIdx);
		}

		public static string[] GetFontStyles(Font font)
		{
			string fileName = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + UnityEditor.AssetDatabase.GetAssetPath(font);

			int nStyles;
			IntPtr stylesPtr;
			GetFontStyles(fileName, out nStyles, out stylesPtr);
			if (nStyles<=0)
				return null;

			var pIntPtrArray = new IntPtr[nStyles];
			var ManagedStringArray = new string[nStyles];
			
			Marshal.Copy(stylesPtr, pIntPtrArray, 0, nStyles);
			
			for (int i = 0; i < nStyles; i++)
			{
				ManagedStringArray[i] = Marshal.PtrToStringAnsi(pIntPtrArray[i]);
				Marshal.FreeCoTaskMem(pIntPtrArray[i]);
			}
			
			Marshal.FreeCoTaskMem(stylesPtr);
			return ManagedStringArray;
		}

        public static KerningInfo[] GetKerningPairs()
        {
            int nPairs;
            IntPtr arrayPtr = GetKerningInfo( out nPairs );

            var kerning = new KerningInfo[nPairs];
            if (nPairs>0)
            {
                long longPtr = arrayPtr.ToInt64();
                long size = Marshal.SizeOf(typeof(KerningInfo));

                for (int i = 0; i < nPairs; ++i, longPtr+=size)
                {
                    IntPtr ptr = new IntPtr(longPtr);
                    kerning[i] = (KerningInfo)Marshal.PtrToStructure(ptr, typeof(KerningInfo) );
                }
            }
            Marshal.FreeCoTaskMem(arrayPtr);

            return kerning;
        }

        public static int[] StrToIntArray( string text )
        {
            var arr = new int[text.Length+1];
            for (int i = 0; i < text.Length; ++i)
                arr[i] = (int)text[i];
            arr[text.Length] = 0;
            return arr;
        }
		
#region From c++ Dll

		[DllImport("kernel32")]	static extern IntPtr LoadLibrary (string lpFileName);

		[DllImport(libName)] public static extern int LoadFont( string fontName, int faceIdx );
		[DllImport(libName)] public static extern int SetFontSize( int Size );

        [DllImport(libName)] public static extern int GetFontStyles(string fontName, [Out] out int nStyles, [Out] out IntPtr Styles);

        [DllImport(libName)] public static extern bool GetFontData([Out] out int lineHeight, [Out] out int ascender, [Out] out int descender);



        [DllImport(libName)]public static extern int GetCharactersRect (int FontSize, int[] characters, int spread, float Downscale, CharRect[] rects);

        [DllImport(libName)]public static extern void GenerateRasterSDFCharactersAsync(int[] Characters, int Spread, int DownScale, int DownscaleType);
        [DllImport(libName)]public static extern void GenerateGeomSDFCharactersAsync(int[] Characters, int Spread, int DownScale, int DownscaleType, bool pseudoSDF);

        [DllImport(libName)]public static extern void GenerateMSDFCharactersAsync(int[] Characters, int Spread, bool SDFinAlpha);


		[DllImport(libName)] public static extern float GetSDF_Progress();
		[DllImport(libName)] public static extern void GetGeneratedCharacters([Out] CharInfo[] charInfos);
        [DllImport(libName)] public static extern IntPtr GetKerningInfo( [Out] out int nPairs);
       

		[DllImport(libName)] public static extern void ReleaseFontData(bool ReleaseLibrary);
		[DllImport(libName)] public static extern bool GenerateSDF([In,Out] ref byte[] img, int width, int height, int spread, int downscale, int downscaleType);

#endregion
	}
	
}