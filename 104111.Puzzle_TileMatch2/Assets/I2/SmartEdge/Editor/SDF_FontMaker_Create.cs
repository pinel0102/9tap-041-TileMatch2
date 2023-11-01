
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using I2;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace I2.SmartEdge
{
	public partial class SDF_FontMaker : EditorWindow
	{
		// Generation Variables
		List<CharacterInfo> CharInfos = new List<CharacterInfo>();
		List<Texture2D> CharTextures = new List<Texture2D>();
        FreeTypeI2.KerningInfo[] CharKerning = new FreeTypeI2.KerningInfo[0];

        int mAscender, mDescender, mLineHeight;
		string Characters = string.Empty;
        string MissingCharacters = string.Empty;

        volatile int CurrentCharIdx = -1;
		string OutputPath;

        static Shader pShader_SDF_FontData
        {
            get
            {
                if (mShader_SDF_FontData == null)
                    mShader_SDF_FontData = Shader.Find("GUI/I2 SmartEdge/SDF FontData");
                return mShader_SDF_FontData;
            }
        }
        static Shader mShader_SDF_FontData;

        #region Create Font

        //Font mmFont;
        int iFallback = 0;

        void GenerateFont_Init()
        {
            MissingCharacters = GetFullCharacterSet();
            //Debug.Log(finalCharacters);
            //return;
            iFallback = 0;
            mPreviewTexture = null;
            CharInfos.Clear();
            CharTextures.Clear();
            mPreviewScrollPos = Vector2.zero;

            GenerateNextFallbackFont();
        }

        void GenerateNextFallbackFont()
        { 
			GUI.FocusControl(string.Empty);
			if (mFonts.Length==0)
				return;

			CurrentCharIdx=0;

            if (iFallback <= 0)
            {
                if (mAtlasSize > 0)
                    ComputeFontSize(mAtlasSize);
            }

            if (!FreeTypeI2.LoadFont(mFonts[iFallback], mCurrentFontStyle))
            {
                Debug.LogError("Failed to load FreeType DLL. Please, check the library is in the project folder (outside Assets)");
                return;
            }

            FreeTypeI2.SetFontSize(mFontSize * Downscale);

            if (iFallback <= 0)
            {

                FreeTypeI2.GetFontData(out mLineHeight, out mAscender, out mDescender);
                mLineHeight = (int)(mLineHeight / Downscale);
                mAscender   = (int)(mAscender / Downscale);
                mDescender  = (int)(mDescender / Downscale);
                //Debug.Log(mLineHeight + " " + mAscender + " " + mDescender);
            }

            switch (mSDFFormat) 
			{
                case eSDFFormat.SDF         : FreeTypeI2.GenerateGeomSDFCharactersAsync(FreeTypeI2.StrToIntArray(MissingCharacters), Spread * Downscale, Downscale, (int)mDownscaleType, false); break;
                case eSDFFormat.RasterSDF	: FreeTypeI2.GenerateRasterSDFCharactersAsync (FreeTypeI2.StrToIntArray(MissingCharacters), Spread * Downscale, Downscale, (int)mDownscaleType); 	break;
                case eSDFFormat.PseudoSDF   : FreeTypeI2.GenerateGeomSDFCharactersAsync(FreeTypeI2.StrToIntArray(MissingCharacters), Spread * Downscale, Downscale, (int)mDownscaleType, true); break;
                case eSDFFormat.MSDF		: FreeTypeI2.GenerateMSDFCharactersAsync(FreeTypeI2.StrToIntArray(MissingCharacters), Spread, false ); break;
				case eSDFFormat.MSDFA	    : FreeTypeI2.GenerateMSDFCharactersAsync(FreeTypeI2.StrToIntArray(MissingCharacters), Spread, true ); break;
			}
			EditorApplication.update += GenerateFont_GenCharacter;
		}

		void GenerateFont_GenCharacter()
		{
			var progress = FreeTypeI2.GetSDF_Progress();
			if (progress<1)
				return;
			CurrentCharIdx=-1;
			EditorApplication.update -= GenerateFont_GenCharacter;

            CharKerning = FreeTypeI2.GetKerningPairs();

			FreeTypeI2.CharInfo[] infos = new FreeTypeI2.CharInfo[MissingCharacters.Length];
			FreeTypeI2.GetGeneratedCharacters(infos);
            var sbMissing = new StringBuilder();
            infos = infos.Where(x => {
                if (x.image.ToInt64() == 0 && x.Advance==0)
                {
                    sbMissing.Append((char)x.Character);
                    return false;
                }
                return true;
            }).ToArray();

            MissingCharacters = sbMissing.ToString();

            //float maxUV = 0;
            //float maxV = 0;

			foreach (var charInfo in infos)
			{
                Texture2D texture = null;
				if (mSDFFormat == eSDFFormat.MSDFA) 
                    texture = FreeTypeI2.GetTextureRGBA(charInfo.image, charInfo.Width, charInfo.Height);
                else
				if (mSDFFormat == eSDFFormat.MSDF) 
                    texture = FreeTypeI2.GetTextureRGB(charInfo.image, charInfo.Width, charInfo.Height);
                else
                    texture = FreeTypeI2.GetTextureA(charInfo.image, charInfo.Width, charInfo.Height);

                CharTextures.Add(texture);

                CharacterInfo characterInfo = new CharacterInfo();
				characterInfo.index = charInfo.Character;

#if UNITY_5_3_OR_NEWER || UNITY_5_3 || UNITY_5_2
				characterInfo.advance = charInfo.Advance;
                //characterInfo.advance = charInfo.Advance - Spread * 4;
                characterInfo.uvTopLeft     = new Vector2(0, -charInfo.Height);
				characterInfo.uvBottomRight = new Vector2(charInfo.Width, 0);

                characterInfo.minX = charInfo.Verts_X;
                characterInfo.minY = charInfo.Verts_Y - (int)(charInfo.Verts_Height);
                characterInfo.maxX = charInfo.Verts_X + (int)(charInfo.Verts_Width);
                characterInfo.maxY = charInfo.Verts_Y;

                //characterInfo.uvTopLeft = new Vector2(0, 0);
                //characterInfo.uvBottomRight = new Vector2(charInfo.Width, -charInfo.Height);

                //characterInfo.minX = charInfo.Verts_X;
                //characterInfo.minY = charInfo.Verts_Y - (int)(charInfo.Verts_Height);
                //characterInfo.maxX = charInfo.Verts_X + (int)(charInfo.Verts_Width);
                //characterInfo.maxY = charInfo.Verts_Y;


#else
#pragma warning disable 618
				characterInfo.width = charInfo.Advance;// - Spread*4;
				characterInfo.uv.min = new Vector2(0, -charInfo.Height);
				characterInfo.uv.max = new Vector2(charInfo.Width, 0);

				characterInfo.vert = new Rect(charInfo.Verts_X, charInfo.Verts_Y - charInfo.Verts_Height, charInfo.Verts_Width, charInfo.Verts_Height);

// 				characterInfo.uv.min = new Vector2(0, 0);
// 				characterInfo.uv.max = new Vector2(charInfo.Width, charInfo.Height);
// 
// 				characterInfo.vert = new Rect(charInfo.Verts_X, charInfo.Verts_Y - charInfo.Verts_Height, charInfo.Verts_Width, charInfo.Verts_Height);

#pragma warning restore 618
#endif

                CharInfos.Add(characterInfo);

                /*if (charInfo.Width > maxUV || charInfo.Height > maxUV)
                {
                    maxUV = Mathf.Max(charInfo.Width, charInfo.Height);
                    maxV = Mathf.Max(charInfo.Verts_Width, charInfo.Verts_Height);
                }*/
            }
            //Debug.LogFormat("{0} - {1} : {2} {3} {4}", Spread, mSpreadFactor, maxUV, maxV, Spread*2/(float)mAtlasSize);

            if (MissingCharacters.Length>0 && iFallback<mFonts.Length-1)
            {
                iFallback++;
                GenerateNextFallbackFont();
                return;
            }
            GenerateFont_SaveFont();
		}

		void GenerateFont_SaveFont()
		{
			// Get final data and close TrueType library
			FreeTypeI2.ReleaseFontData(true);

			// Compute baseline so that highest Y is 0
			int nChars = CharInfos.Count;
			/*float MaxY = float.MinValue;
			for (int i=0; i<nChars; ++i)
				MaxY = Mathf.Max (MaxY, CharInfos[i].minY) ;*/

            TextureFormat format;
			if (mSDFFormat == eSDFFormat.MSDFA)
                format = TextureFormat.ARGB32;
			else if (mSDFFormat == eSDFFormat.MSDF)
                format = TextureFormat.RGB24;
            else
                format = TextureFormat.Alpha8;
			
			// Create a packed texture with all the characters
            var Atlas = new Texture2D(32, 32, format, false, true);
			Rect[] rects = Atlas.PackTextures(CharTextures.ToArray(), mPadding, 8192,false);

            mPreviewTexture = new Texture2D(Atlas.width, Atlas.height, format, false, true);
			mPreviewTexture.SetPixels32( Atlas.GetPixels32() );
			mPreviewTexture.Apply();

            // Offset the UV to the Atlas position and the vertices to the baseline
			for (int i=0; i<nChars; ++i)
			{
				CharacterInfo info = CharInfos[i];
#if UNITY_5_3_OR_NEWER || UNITY_5_3 || UNITY_5_2
                info.uvTopRight = rects[i].max;
                info.uvBottomLeft = rects[i].min;

        //#if !UNITY_5_3_3 && !UNITY_5_4_OR_NEWER         // Is 5.3.0 or older
        //        info.minY -= mAscender;
        //        info.maxY -= mAscender;
        //#endif
#else
#pragma warning disable 618
                info.uv.max = rects[i].max;
				info.uv.min = rects[i].min;
                //info.vert.y -= mAscender;
#pragma warning restore 618
#endif
                //info.uv = rects[i];
                //info.vert.y -= MaxY - Spread;

                info.size = 0;
				CharInfos[i] = info;
			}
			CurrentCharIdx = -1;

            // Generate Missing Characters Report---------------
            var sb = new StringBuilder();
            for (int i=0; i<MissingCharacters.Length; ++i)
            {
                sb.AppendFormat("{0}({1:x})\t", MissingCharacters[i], (int)MissingCharacters[i]);
                if ((i + 1) % 4 == 0)
                {
                    sb.Append("\n");
                    if (i > 100)
                    {
                        sb.Append("more....");
                        break;
                    }
                }
            }
            MissingCharacters = sb.ToString();

			Repaint ();
			EditorUtility.UnloadUnusedAssetsImmediate ();
		}

        void SaveUnityFont()
        {
            if (mFonts.Length < 0 || mFonts[0] == null)
                return;
            string sPath = AssetDatabase.GetAssetPath(mFonts[0]);

            string sdfFormat = (mSDFFormat == eSDFFormat.RasterSDF) ? "SDF" : mSDFFormat.ToString();
            OutputPath = EditorUtility.SaveFilePanelInProject("Save As", mFonts[0].name + "_" + sdfFormat + ".fontsettings", "fontsettings", "Save font as...", System.IO.Path.GetDirectoryName(string.IsNullOrEmpty(OutputPath) ? sPath : OutputPath));
            if (string.IsNullOrEmpty(OutputPath))
                return;

            //string TexturePath = OutputPath.Replace(".fontsettings", ".png");
            /*TextureImporterFormat format;
            if (mPreviewTexture.format == TextureFormat.Alpha8)
                format = TextureImporterFormat.Alpha8;
            else
				format = (mSDFFormat==eSDFFormat.MSDFA ? TextureImporterFormat.RGBA32 :TextureImporterFormat.RGB24);
                */
            Font newFont = (Font)AssetDatabase.LoadAssetAtPath(OutputPath, typeof(Font));

            if (newFont == null || newFont.fontSize <= 140)
            {
                Object FontTemplate = Resources.Load("Fonts/I2FontTemplate");
                string FontTemplatePath = AssetDatabase.GetAssetPath(FontTemplate);
                Resources.UnloadAsset(FontTemplate);

                AssetDatabase.CopyAsset(FontTemplatePath, OutputPath);
                AssetDatabase.Refresh();
                newFont = (Font)AssetDatabase.LoadAssetAtPath(OutputPath, typeof(Font));

                //newFont = new Font();
                //newFont.name = Path.GetFileName(OutputPath);
                //AssetDatabase.CreateAsset(newFont, OutputPath);
            }
            else
            {
                AssetDatabase.Refresh();
            }

            if (newFont.material == null)
                newFont.material = AssetDatabase.LoadAssetAtPath(OutputPath, typeof(Material)) as Material;

            if (newFont.material == null)
            {
                newFont.material = new Material(pShader_SDF_FontData);
                newFont.material.name = newFont.name;
                //string MaterialPath = OutputPath.Replace(".fontsettings", ".mat");
                AssetDatabase.AddObjectToAsset(newFont.material, OutputPath);
                //AssetDatabase.CreateAsset(newFont.material, MaterialPath);
            }

            newFont.material.shader = pShader_SDF_FontData;
            newFont.material.SetFloat("_Spread", Spread);

            mPreviewTexture.name = System.IO.Path.GetFileNameWithoutExtension(OutputPath);
            if (mPreviewTexture != newFont.material.mainTexture)
            {
                //TextureTools.SaveTexture(mPreviewTexture, "Assets/temp.png", format, true);
                Object.DestroyImmediate(newFont.material.mainTexture, true);
                newFont.material.mainTexture = mPreviewTexture;
                AssetDatabase.AddObjectToAsset(mPreviewTexture, newFont);
            }

            SerializedObject serObj = new SerializedObject(newFont);
            serObj.Update();

            float FontScale = newFont.fontSize / (float)(mFontSize * 64);

            //if (newFont.fontSize<=0)
            //serObj.FindProperty("m_FontSize").floatValue = mFontSize*64;
            serObj.FindProperty("m_LineSpacing").floatValue = mLineHeight * FontScale;
            serObj.FindProperty("m_CharacterSpacing").intValue = 0;
            serObj.FindProperty("m_CharacterPadding").intValue = 1;

            serObj.FindProperty("m_Ascent").floatValue = mAscender * FontScale;   // space on the top when alignment is TOP
            if (serObj.FindProperty("m_Descent") != null)
                serObj.FindProperty("m_Descent").floatValue = mDescender * FontScale;// space on the bottom when alignment is BOTTOM

            var kerningProp = serObj.FindProperty("m_KerningValues");
            kerningProp.arraySize = CharKerning.Length;
            for (int i=0; i<CharKerning.Length; ++i)
            {
                var kerning = kerningProp.GetArrayElementAtIndex(i);
                kerning.FindPropertyRelative("first.first").intValue = (int)CharKerning[i].Character;
                kerning.FindPropertyRelative("first.second").intValue = (int)CharKerning[i].NextCharacter;
                kerning.FindPropertyRelative("second").floatValue = CharKerning[i].offset;
            }
            serObj.ApplyModifiedProperties();
            serObj.SetIsDifferentCacheDirty();


            var finalInfos = CharInfos.ToArray();

            if (newFont.fontSize != mFontSize)
			{
				for (int i=0, imax= finalInfos.Length; i<imax; ++i)
				{
					CharacterInfo info = finalInfos[i];

#if UNITY_5_3_OR_NEWER || UNITY_5_3 || UNITY_5_2

                    info.minX = (int)(info.minX * FontScale);
                    info.minY = (int)(info.minY * FontScale);
					info.maxX = (int)(info.maxX * FontScale);
                    info.maxY = (int)(info.maxY * FontScale);
					info.advance = (int)(info.advance * FontScale);
#else
#pragma warning disable 618
                    info.vert = new Rect((int)(info.minX * FontScale), (int)(info.minY * FontScale),
						(int)((info.maxX-info.minX) * FontScale), (int)((info.maxY-info.minY) * FontScale));
					info.width = (int)(info.advance * FontScale);
#pragma warning restore 618
#endif
                        finalInfos[i] = info;
				}
			}
			newFont.characterInfo = finalInfos.ToArray ();

			EditorUtility.SetDirty(newFont);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

            // This is needed in unity 5.5 because of some bug with newly created fonts not loading correctly until unity is restarted
            // or this is done
            Resources.UnloadAsset(newFont);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.LoadAssetAtPath(OutputPath, typeof(Font));
            //////////////////////////////////////////////////////////////////////
		}

		/*Font ImportFontIntoTemporalData ()
		{
			//---[ Create font, texture, material ]--------------
			TrueTypeFontImporter tim = AssetImporter.GetAtPath("Assets/Contents/UI/Fonts/calibri.ttf") as TrueTypeFontImporter;
			tim.fontSize = 100;
			tim.characterPadding = 0;
			tim.characterSpacing = 1;
			tim.includeFontData = true;
			tim.fontRenderingMode = FontRenderingMode.HintedSmooth;
			tim.includeFontData = true;
			tim.fontTextureCase = FontTextureCase.CustomSet;
			tim.customCharacters = "a";
			
			AssetDatabase.ImportAsset("Assets/Contents/UI/Fonts/calibri.ttf", ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
			
			Font newFont = tim.GenerateEditableFont("Assets/I2Temp1.fontsettings");
			return newFont;
		}*/

#endregion

        #region Finding Best Font Size
        void ComputeFontSize( int TextureSize )
		{
            if (mFonts.Length <= 0 || !FreeTypeI2.LoadFont(mFonts[0], mCurrentFontStyle))
                return;

			//--[ Get a rough estimation of the font size ]--------
            var chars = GetFullCharacterSet();

            Rect[] rects = GetCharactersRec(mFontSize, chars);
            System.Array.Sort (rects, (a,b) => ((int)(b.height-a.height)));

			float ScaleFactor = FindScaleFactor (TextureSize, 1, rects);
			mFontSize = Mathf.FloorToInt(ScaleFactor*mFontSize);

			//--[ Try bigger fonts until it gets bigger than the TextureSize ]-------------

			Texture2D[] aTextures = new Texture2D[rects.Length];
			Texture2D atlas = new Texture2D (32, 32, TextureFormat.ARGB32, false, true);
            TestFontSizePacking(aTextures, atlas, chars);

			int nIterations = 0;
			int validFontSize = mFontSize;
			while (nIterations<30)
			{
				if (mPackingMethod != ePackingMethod.Optimum)
					return;

                //TextureTools.SaveTexture(atlas, "Assets/ff.png");
                atlas.Resize(32, 32);
                TestFontSizePacking(aTextures, atlas, chars);
                //Debug.Log("Iteration Font: " + mFontSize + " " + atlas.width + " " + atlas.height);

                if (atlas.width<=TextureSize && atlas.height<=TextureSize)
				{
					validFontSize = mFontSize;
					mFontSize += 10;
				}
				else
                if (nIterations==0)
                {
                    mFontSize -= 10;
                    continue;
                }
                else
				{
					if (validFontSize+1 >= mFontSize)
					{
						mFontSize = validFontSize;
						break;
					}

					mFontSize = (validFontSize + mFontSize)/2;
				}
				nIterations++;
			}
			//Debug.Log ("Num extra iterations to find FontSize " + mFontSize + " = " + nIterations);
		}

        Rect[] GetCharactersRec( int fontSize, string chars )
        {
            List<Rect> lRects = new List<Rect>();
            for (int f = 0; f < mFonts.Length && chars.Length > 0; ++f)
            {
                FreeTypeI2.LoadFont(mFonts[f], mCurrentFontStyle);
                Rect[] rects = FreeTypeI2.GetCharactersRect(fontSize * Downscale, chars, Spread * Downscale, Downscale);

                var sb = new StringBuilder();
                int index = -1;
                lRects.AddRange( rects.Where(x => 
                    {
                        index++;
                        if (x.width > 0 && x.height > 0)
                            return true;
                        sb.Append(chars[index]);
                        return false;
                    })
                );
                chars = sb.ToString();
            }

            return lRects.ToArray();
        }


        void TestFontSizePacking ( Texture2D[] aTextures, Texture2D atlas, string chars )
		{
			Rect[] rects = GetCharactersRec(mFontSize, chars);

			for (int i=0, imax=rects.Length; i<imax; ++i)
			{
				int width = Mathf.CeilToInt(rects [i].width);
				int height = Mathf.CeilToInt(rects [i].height);
				if (aTextures[i]==null)
					aTextures[i] = new Texture2D (width,height);
				else
					aTextures[i].Resize(width,height);
			}
			atlas.Resize(32,32);
            atlas.PackTextures (aTextures, mPadding,8192,false);
		}

		
		float FindScaleFactor (int MaxSize, int Padding, Rect[] Rects)
		{
			for (int i=1; i<Rects.Length; ++i)
			{
				float RectSize = GetRectSize (i, Padding, Rects);
				if (RectSize>0)
					return MaxSize/RectSize;
			}
			return 1;
		}
		
		float GetRectSize( int nRectsInFirstRow, int Padding, Rect[] Rects )
		{
			float MinWidth = 0, MaxWidth = 0, RowWidth = 0, RealWidth = 0;
			float LastHeight = 0, RowHeight = 0;
			
			for (int i=0; i<nRectsInFirstRow; ++i)
			{
				MinWidth += Rects[i].width;
				if (Rects[i].height>RowHeight)
					RowHeight = Rects[i].height;
			}
			MaxWidth = MinWidth + Rects [nRectsInFirstRow].width;
			RowWidth = MaxWidth;
			
			for (int i=nRectsInFirstRow; i<Rects.Length; ++i)
			{
				if (RowWidth >= MaxWidth)
				{
					LastHeight += RowHeight + Padding;
					RealWidth = Mathf.Max (RealWidth, RowWidth);
					RowWidth = RowHeight = 0;
					
					if (LastHeight>MaxWidth)
						return -1;
				}
				
				RowWidth += Rects[i].width+Padding;
				if (Rects[i].height>RowHeight)
					RowHeight = Rects[i].height;
			}
			LastHeight += RowHeight+Padding;
			RealWidth = Mathf.Max (RealWidth, RowWidth);
			
			//Debug.Log (RealWidth + "x" + LastHeight);
			return Mathf.Max(RealWidth, LastHeight);
		}
#endregion

        string GetFullCharacterSet()
        {
            var sb = new StringBuilder();
            var chars = Characters;

            var regex = new Regex(@"\[\[(.*?),(.*?)]]");
            var regexMatches = regex.Matches(chars);
            foreach (Match match in regexMatches)
            {
                var param1 = GetRangeNumber(match.Groups[1].Value);
                var param2 = GetRangeNumber(match.Groups[2].Value);

                for (int i = param1; i < param2; ++i)
                    sb.Append((char)i);

                chars = chars.Replace(match.Groups[0].Value, "");
            }
            sb.Append(chars);
            return new string( sb.ToString().Distinct().OrderBy(a=>a).ToArray() );
        }

     }
}