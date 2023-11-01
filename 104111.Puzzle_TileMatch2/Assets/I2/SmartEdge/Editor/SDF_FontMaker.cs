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
        Font[] mFonts = new Font[0];

		int mFontSize = 100;
		float mSpreadFactor = 0.1f;
		int mDownscale = 4;
		int mPadding = 1;
		int mAtlasSize = 512;
		FreeTypeI2.eDownscaleType mDownscaleType = FreeTypeI2.eDownscaleType.Center;

		string[] mSDFFormatDisplayNames = new string[]{ "SDF", "Raster SDF", "PseudoSDF", "MSDF", "MSDF+SDF"};
		enum eSDFFormat { SDF, RasterSDF, PseudoSDF, MSDF, MSDFA};
		eSDFFormat mSDFFormat = eSDFFormat.SDF;

        string[] mFontStyles = new string[]{"Regular"};
		int mCurrentFontStyle=0;


		bool mCharSet_LowerCase = true;
		bool mCharSet_UpperCase = true;
		bool mCharSet_Numeric 	= true;
		bool mCharSet_Symbols 	= true;
		bool mcharSet_Extended  = false;
        int mCharSet_Range = 0;

        Texture2D mPreviewTexture;
        Vector2 ScrollPos_CharacterSet = Vector3.zero;
        Vector2 ScrollPos_All = Vector3.zero;
        int mPreviewFontSize = 15;

		enum ePackingMethod { Fast, Optimum };
		ePackingMethod mPackingMethod = ePackingMethod.Optimum;

//		bool mGenerateFontData = false;

			
		string[] QualityOptions 	= new string[]{"Fastest", "Fast", "Normal", "Good", "Nice", "Nicest" };
		string[] AtlasSizeOptions 	= new string[]{ "Use FontSize", "16x16", "32x32", "64x64", "128x128", "256x256", "512x512", "1024x1024", "2048x2048", "4096x4096" };

		string Help_Spread = "Controls how wide/diffused can be the glow/shadow";
		string Help_Packing = "Fast packing will compute a font size that covers most of the Atlas\nOptimum, runs after the fast and tries to increase the fontsize until it can no longer fit in the Atlas"; 
		//string Help_AtlasSize = "When a size is selected, the font size is computed to best fit that atlas size.\nIf 'Use FontSize' is selected, then fonts are forced to the specified FontSize";

		int Spread { get{ return Mathf.CeilToInt(mSpreadFactor*mFontSize*0.5f); } }
		int Downscale { get { return mSDFFormat==eSDFFormat.RasterSDF ? mDownscale : 1; } }

        Vector2 mPreviewScrollPos = Vector2.zero;

		#region Editor

		[MenuItem("Tools/I2 SmartEdge/SDF Font Maker",false, 0)]
		[MenuItem("Assets/I2 SmartEdge/Open Font Maker", false, 0)]
		static public void OpenFontMaker ()
		{
			EditorWindow.GetWindow<SDF_FontMaker>(false, "I2 Font Maker", true).Show();
		}

		void OnGUI ()
		{
            ScrollPos_All = GUILayout.BeginScrollView(ScrollPos_All);
            GUILayout.BeginVertical(GUILayout.Width(Screen.width-30), GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Click here for Tips and Help", EditorStyles.label, GUILayout.ExpandWidth(false)))
                    Application.OpenURL(SE_InspectorTools.HelpURL_CreateSDFFont);
                GUILayout.Space(20);
            GUILayout.EndHorizontal();



            //Font mNewFont = EditorGUILayout.ObjectField("Font", mFont, typeof(Font), false) as Font;
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
                GUILayout.Label("Fonts:", GUILayout.Width(80));
                GUILayout.BeginVertical();
                    for (int i=0; i<mFonts.Length; ++i)
                        mFonts[i] = EditorGUILayout.ObjectField(GUITools.EmptyContent, mFonts[i], typeof(Font), false) as Font;
                    Font nextFont = EditorGUILayout.ObjectField(GUITools.EmptyContent, null, typeof(Font), false) as Font;
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                if (nextFont != null)
                {
                    System.Array.Resize(ref mFonts, mFonts.Length + 1);
                    mFonts[mFonts.Length - 1] = nextFont;
                }

                // Remove Empty slots or those that are not ttf files
                mFonts = mFonts.Where(x => {
                    if (x != null)
                    {
                        if (!AssetDatabase.GetAssetPath(x).EndsWith(".ttf", System.StringComparison.InvariantCultureIgnoreCase) && !AssetDatabase.GetAssetPath(x).EndsWith(".otf", System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            Debug.LogError("Font needs to be a ttf or otf file");
                            return false;
                        }
                        return true;

                    }
                    return false;
                }).ToArray();

                if (mFonts.Length>0)
                {
                    mFontStyles = null;
                    mFontStyles = FreeTypeI2.GetFontStyles(mFonts[0]);

                    if (mFontStyles == null || mFontStyles.Length == 0)
                        mFontStyles = new string[] { "Regular" };
                    mCurrentFontStyle = 0;
                    mPreviewTexture = null;
                }
            }

			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80f;

			GUILayout.Space(10f);
				
				OnGUI_FontStyleAndSize ();

			GUILayout.Space(10f);
                GUILayout.BeginHorizontal();
				    mSpreadFactor = EditorGUILayout.Slider( new GUIContent("SDF Spread", Help_Spread), mSpreadFactor, 0, 1);
                    if (GUILayout.Button( Spread + "px", EditorStyles.miniLabel, GUITools.DontExpandWidth))
                    {
                        mSpreadFactor = (Spread / (float)mFontSize) * 2;
                    }
                GUILayout.EndHorizontal();
			GUILayout.Space (5);
				GUILayout.BeginHorizontal();
					GUILayout.Label ("Quality", GUILayout.Width(70));
					mSDFFormat = (eSDFFormat)EditorGUILayout.Popup ((int)mSDFFormat, mSDFFormatDisplayNames);

					if (mSDFFormat!=eSDFFormat.RasterSDF)
                    {
                        GUI.enabled = false;
                        EditorGUILayout.Popup (QualityOptions.Length-1, QualityOptions);
                    }
                    else
        			mDownscale = 1 << EditorGUILayout.Popup ((int)Mathf.Log (mDownscale, 2), QualityOptions);

                    GUI.enabled = true;
                    //mDownscaleType = (FreeTypeI2.eDownscaleType)EditorGUILayout.Popup ( (int)mDownscaleType, System.Enum.GetNames(typeof(FreeTypeI2.eDownscaleType)), GUILayout.Width(80));

                    if (GUILayout.Button(EditorGUIUtility.IconContent("_Help"), EditorStyles.label, GUITools.DontExpandWidth))
                        Application.OpenURL(SE_InspectorTools.HelpURL_SDFMethods);

            GUILayout.EndHorizontal();
			GUILayout.Space(20f);

			OnGUI_CharacterSet ();

			//GUILayout.Space (10);

			GUILayout.BeginHorizontal ();
				//GUILayout.FlexibleSpace();
			//mGenerateFontData = GUILayout.Toggle (mGenerateFontData, new GUIContent("Create Font Data", tooltip:Help_FontData), GUILayout.ExpandWidth(false));
			//	GUILayout.Space (10);
				GUI.enabled = mFonts.Length>0;
				if (GUILayout.Button("Generate Font", GUILayout.ExpandWidth(true)))
					GenerateFont_Init();
				GUI.enabled = true;
				//GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal ();

            //			mmFont = (Font)EditorGUILayout.ObjectField (mmFont, typeof(Font), false);
            OnGUI_DrawPreview();

            if (CurrentCharIdx >= 0 && MissingCharacters.Length > 0 && CurrentCharIdx < MissingCharacters.Length)
			{
				float progress = FreeTypeI2.GetSDF_Progress();
				//float progress = CurrentCharIdx / (float)Characters.Length;
                CurrentCharIdx = (int)(progress * (MissingCharacters.Length - 1));
                if (EditorUtility.DisplayCancelableProgressBar("Creating Font", "Current Character: " + MissingCharacters[CurrentCharIdx], progress))
				{
					CurrentCharIdx = -1;
					EditorApplication.update -= GenerateFont_Init;
					EditorApplication.update -= GenerateFont_GenCharacter;
					EditorApplication.update -= GenerateFont_SaveFont;
					FreeTypeI2.ReleaseFontData(true);
				}
			}
			else
				EditorUtility.ClearProgressBar();

			EditorGUIUtility.labelWidth = labelWidth;
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

        }

        void OnInspectorUpdate() 
		{
			if (CurrentCharIdx>=0)
				Repaint();
		}

		#endregion

		#region Font Style and Size

		void OnGUI_FontStyleAndSize()
		{
			//GUIStyle ToolbarWide = new GUIStyle(EditorStyles.toolbar);//SmartEdgeTools.SmartEdgeSkin.GetStyle("Toolbar")
			//ToolbarWide.fixedHeight = 0;
			GUILayout.BeginVertical("LargeButtonMid"/*ToolbarWide*/, GUILayout.ExpandWidth(true), GUILayout.Height(30));
				GUILayout.Space (5);
				GUILayout.Label("Packing", EditorStyles.largeLabel);
            GUILayout.EndHorizontal();
            
			GUITools.BeginContents ();

			GUILayout.BeginHorizontal ();
				GUILayout.BeginVertical ();
					//GUILayout.Label ("Font Size", GUILayout.ExpandWidth(false));
                    EditorGUI.BeginChangeCheck();
					mFontSize = EditorGUILayout.IntField( "Font Size", mFontSize);
                    if (EditorGUI.EndChangeCheck())
                        mAtlasSize = 0;

					GUILayout.Space (5);

					//GUILayout.Label ("Atlas", GUILayout.ExpandWidth(false));
					int AtlasSize = (mAtlasSize<=0 ? 0 : Mathf.Max (0, (int)Mathf.Log (mAtlasSize, 2) - 3));
					int newSize = EditorGUILayout.Popup ("Atlas", AtlasSize, AtlasSizeOptions);//, GUILayout.Width(150));
					mAtlasSize = (newSize<=0 ? 0 : 1<<(newSize + 3));

                    if (AtlasSize != newSize && mAtlasSize != 0)
                    {
                        ComputeFontSize(mAtlasSize);
                    }
				GUILayout.EndVertical ();

				GUILayout.BeginVertical ();
					GUI.enabled = (mFontStyles!= null && mFontStyles.Length>1);
					if (mFontStyles==null)
						mFontStyles = new string[0];
					mCurrentFontStyle = EditorGUILayout.Popup ("Style", mCurrentFontStyle, mFontStyles);
					GUI.enabled = true;
				
					GUILayout.Space (5);

					//GUILayout.Label ("Packing Method", GUILayout.ExpandWidth(false));
					mPackingMethod = (ePackingMethod)EditorGUILayout.EnumPopup (new GUIContent("Packing",Help_Packing), mPackingMethod);
				GUILayout.EndVertical ();

			GUILayout.EndHorizontal ();
			GUITools.EndContents (false);
		}

		void OnGUI_DrawPreview()
		{
			if (mPreviewTexture==null/* || CharInfos.Count==0*/)
				return;
			GUILayout.Space (5);

            mPreviewScrollPos = GUILayout.BeginScrollView(mPreviewScrollPos);

			GUILayout.BeginHorizontal (GUILayout.Height(Screen.width / 2));
				GUILayout.BeginVertical (GUILayout.Height (1));
					Rect rect = GUILayoutUtility.GetRect (Screen.width/2, Screen.width/2 	);
					if (mSDFFormat==eSDFFormat.SDF || mSDFFormat==eSDFFormat.RasterSDF || mSDFFormat == eSDFFormat.PseudoSDF)
						EditorGUI.DrawTextureAlpha(rect, mPreviewTexture);
                    else
						EditorGUI.DrawPreviewTexture(rect, mPreviewTexture);
				GUILayout.EndVertical ();

				GUILayout.BeginVertical (GUILayout.ExpandHeight (true), GUILayout.ExpandWidth(false));
                    EditorGUILayout.IntField ("Width", mPreviewTexture.width, GUILayout.Width(180) );
					EditorGUILayout.IntField ("Height", mPreviewTexture.height, GUILayout.Width(180));
					GUILayout.Space (5);
					EditorGUILayout.TextField ("Format", mPreviewTexture.format.ToString(), GUILayout.Width(180));
					EditorGUILayout.TextField ("Size",  TextureTools.GetTextureMemory(mPreviewTexture), GUILayout.Width(180));
                    GUILayout.Space (10);
                    GUI.enabled = CharInfos.Count > 0;
					if (GUILayout.Button ("Save Unity Font"))
						SaveUnityFont ();

                    #if SE_NGUI
					    if (GUILayout.Button ("Save NGUI UIFont"))
						    SaveNGUIFont ();
                    #endif
                    GUI.enabled = true;
            
                    if (MissingCharacters.Length>0)
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("Missing Characters: ("+MissingCharacters.Length+")");

                        EditorGUILayout.TextArea(MissingCharacters, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(100), GUILayout.Width(270));
                        /*if (GUILayout.Button("Remove Missing"))
                        {
                            Characters = new string(Characters.Where(a=>!MissingCharacters.Contains(a)).ToArray());
                            MissingCharacters = string.Empty;
                        }*/
                    }
                    GUILayout.FlexibleSpace();

            GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();

            GUILayout.EndScrollView();
		}
#endregion

        #region GUI CharacterSet

		void OnGUI_CharacterSet()
		{
			//GUIStyle ToolbarWide = new GUIStyle(EditorStyles.toolbar);//SmartEdgeTools.SmartEdgeSkin.GetStyle("Toolbar")
			//ToolbarWide.fixedHeight = 0;
			GUILayout.BeginVertical(/*ToolbarWide*/"LargeButtonMid", GUILayout.ExpandWidth(true), GUILayout.Height(30));
			GUILayout.Space (5);
			GUILayout.Label("Characters To Include", EditorStyles.largeLabel);
			GUILayout.EndHorizontal();

			GUITools.BeginContents ();
			GUILayout.Space (5);
			GUILayout.BeginHorizontal (GUILayout.Height(1));

				GUILayout.BeginVertical (GUILayout.Width(1));

                EditorGUI.BeginChangeCheck();
                    EditorGUI.BeginChangeCheck();
			 		mCharSet_UpperCase 	= GUILayout.Toggle (mCharSet_UpperCase, "Upper Case", EditorStyles.toolbarButton);
					mCharSet_LowerCase 	= GUILayout.Toggle (mCharSet_LowerCase, "Lower Case", EditorStyles.toolbarButton);
					mCharSet_Numeric 	= GUILayout.Toggle (mCharSet_Numeric, "Numeric", EditorStyles.toolbarButton);
					mCharSet_Symbols 	= GUILayout.Toggle (mCharSet_Symbols, "Symbols", EditorStyles.toolbarButton);
					mcharSet_Extended 	= GUILayout.Toggle (mcharSet_Extended, "Extended", EditorStyles.toolbarButton);

                    bool WasCustom = IsCustomCharacterSet();

                    if (EditorGUI.EndChangeCheck() && WasCustom)
                        Characters = string.Empty;

					GUILayout.Space (10);

                    bool Custom 	= GUILayout.Toggle (WasCustom, "Custom", EditorStyles.toolbarButton);
					if (Custom && !WasCustom)
						SetCustomCharacterSet ();

					if (GUILayout.Button ("From File", EditorStyles.toolbarButton))
						LoadCharactersFromFile ();
                    if (GUILayout.Button("From Font", EditorStyles.toolbarButton))
                        LoadCharactersFromFont();

                    if (EditorGUI.EndChangeCheck())
                        GUI.FocusControl(string.Empty);

                mPreviewFontSize = (int)GUILayout.HorizontalSlider ((float)mPreviewFontSize, 10, 200);

				GUILayout.EndVertical ();
                GUILayout.BeginVertical();

				if (!IsCustomCharacterSet())
				{
					Characters = string.Empty;
					if (mCharSet_UpperCase) Characters += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
					if (mCharSet_LowerCase) Characters += "abcdefghijklmnopqrstuvwxyz";
					if (mCharSet_Numeric)   Characters += "0123456789";
					if (mCharSet_Symbols)   Characters += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
					if (mcharSet_Extended)  Characters += "¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
					if (mCharSet_UpperCase || mCharSet_LowerCase) Characters += " ";
				}

			    GUIStyle fontGUIStyle = new GUIStyle (EditorStyles.textArea);
			    fontGUIStyle.font = mFonts.Length<=0 ? null : mFonts[0];
			    fontGUIStyle.fontSize = mPreviewFontSize;
			    ScrollPos_CharacterSet = GUILayout.BeginScrollView (ScrollPos_CharacterSet, GUILayout.Height (138));
				string newChar = EditorGUILayout.TextArea (Characters, fontGUIStyle, GUILayout.ExpandHeight(true));
				if (newChar!=Characters)
				{
					Characters = newChar;
					SetCustomCharacterSet();
				}
			    GUILayout.EndScrollView ();

                GUILayout.BeginHorizontal();
                    GUILayout.Space(5);


                    int newRange = EditorGUILayout.Popup(mCharSet_Range, new string[] { "Custom", "Arabic", "Hebrew", "Chinese Japanese Korean", "Cyrillic" }, EditorStyles.toolbarDropDown, GUILayout.Width(170));
                    if (newRange!=mCharSet_Range)
                        mCharSet_Range = newRange;

                    GUILayout.TextField( GetCustomCharacterRange(mCharSet_Range), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                    if (GUILayout.Button("Add Range", EditorStyles.toolbarButton, GUITools.DontExpandWidth))
                        AddCharacterRange();


                    GUILayout.Space(20);
                    GUILayout.Label(Characters.Length.ToString(), EditorStyles.miniLabel, GUITools.DontExpandWidth);
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();			
			GUILayout.EndHorizontal ();
			GUILayout.Space (5);
			GUITools.EndContents (false);
		}

        string GetCustomCharacterRange( int rangeIdx )
        {
            switch (rangeIdx)
            {
                case 1: /* Arabic */    return "[[U+600,U+6FF]] [[U+FB50,U+FDFF]] [[U+FE70,U+FEFF]]";
                case 2: /* Hebrew */    return "[[U+590,U+5FF]] [[U+FB1D,U+FB4F]]";
                case 3: /* CJK */       return "[[U+4E00,U+9F00]]";    // CJK Unified Ideographs
                case 4: /* Cyrillic */  return "[[U+0400,U+04FF]]";
            }
            return string.Empty;
        }

        void AddCharacterRange()
        {
            Characters += GetCustomCharacterRange(mCharSet_Range);
            SetCustomCharacterSet();
            GUI.FocusControl(string.Empty);
        }

        int GetRangeNumber( string value )
        {
            try
            {
                if (value.StartsWith("U+", System.StringComparison.OrdinalIgnoreCase) ||
                    value.StartsWith("/u", System.StringComparison.OrdinalIgnoreCase) ||
                    value.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase))
                {
                    return System.Convert.ToInt32(value.Substring(2), 16);
                }
                else
                    return System.Convert.ToInt32(value);
            }
            catch (System.Exception)
            {
                return 0;
            }
        }

        void ValidateRangeFormat( ref string value )
        {
            if (value.Length <= 0)
                return;

            if (value.Length == 1 && value[0] != 'U' && value[0] != 'u' && value[0] != '/')
            {
                if (!char.IsDigit(value[0]))
                    value = string.Empty;
            }
            else
            if (value.StartsWith("U+",System.StringComparison.OrdinalIgnoreCase) || 
                value.StartsWith("/u", System.StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0,2) + Regex.Replace(value.Substring(2), "[^0-9A-Fa-f]", "");
            }
            else
            {
                value = Regex.Replace(value, "[^0-9]", "");
            }
        }


        void LoadCharactersFromFile ()
		{
			string FileName = EditorUtility.OpenFilePanel ("Select Text File", Application.dataPath, "*.*");
			if (string.IsNullOrEmpty (FileName))
				return;
			string Data = System.IO.File.ReadAllText (FileName);
			HashSet<char> list = new HashSet<char> ();
			foreach (char ch in Data)
				if (!char.IsControl(ch))
					list.Add (ch);

			Characters = string.Empty;
			foreach (char ch in list.OrderBy(ch => ch))
				Characters += ch;

			SetCustomCharacterSet();
		}

        void LoadCharactersFromFont()
        {
            string FileName = EditorUtility.OpenFilePanel("Select Unity Font (.fontsettings)", Application.dataPath, "fontsettings");
            if (string.IsNullOrEmpty(FileName) || !FileName.StartsWith(Application.dataPath, System.StringComparison.OrdinalIgnoreCase))
                return;
            FileName = "Assets" + FileName.Substring(Application.dataPath.Length);

            var font = AssetDatabase.LoadAssetAtPath(FileName, typeof(UnityEngine.Font)) as Font;
            if (font == null)
            {
                Debug.Log("Unable to load font '" + FileName + "'");
                return;
            }

            Characters = new string(font.characterInfo.Select(i=>(char)i.index).OrderBy(a=>a).ToArray());
            SetCustomCharacterSet();

            var material = font.material;
            if (material==null || material.mainTexture==null)
                return;

            int power = 1 << (int)(Mathf.Log(material.mainTexture.width, 2));
            mAtlasSize = material.mainTexture.width == power ? power : power << 1;

            if (material.HasProperty("_Spread"))
            {
                float spread = material.GetFloat("_Spread");
                mSpreadFactor = (spread / (float)mFontSize) * 2;
            }
        }

        bool IsCustomCharacterSet() { return !mCharSet_LowerCase && !mCharSet_UpperCase && !mCharSet_Symbols && !mCharSet_Numeric && !mcharSet_Extended; }
		void SetCustomCharacterSet() { mCharSet_LowerCase = mCharSet_UpperCase = mCharSet_Symbols = mCharSet_Numeric = mcharSet_Extended = false; }

        #endregion

        #region Editor Menus

        [MenuItem("Tools/I2 SmartEdge/Unpack Font", false, 21)]
        [MenuItem("Assets/I2 SmartEdge/Unpack Font", false, 21)]
        public static void UnpackFont()
        {
            if (Selection.activeObject == null || (Selection.activeObject as Font==null))
            {
                EditorUtility.DisplayDialog("You need to select an SDF Font", "No Font Asset is selected", "Ok");
                return;
            }

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("You need to select an SDF Font", "No Font Asset is selected", "Ok");
                return;
            }

            Font font = Selection.activeObject as Font;

            if (font.material == null)
            {
                EditorUtility.DisplayDialog("Font doesn't have a material", "Missing Font.material", "Ok");
                return;
            }

            if (!EditorUtility.DisplayDialog("Extract Texture and Material", "Are you sure you want to extract the material and texture into separated assets?", "Extract", "Cancel"))
                return;


            string rootPath = Path.GetDirectoryName(path) + "\\";

            var newMat = Object.Instantiate<Material>(font.material);
            newMat.name = font.material.name;
            AssetDatabase.CreateAsset(newMat, rootPath + font.material.name + ".mat");
            DestroyImmediate(font.material, true);
            font.material = newMat;


            var newTex = font.material.mainTexture as Texture2D; 
            if (newTex != null)
            {
                newTex.name = font.material.mainTexture.name;
                TextureTools.SaveTexture(newTex, rootPath + newTex.name + ".png");
                //AssetDatabase.SaveAssets();
                //AssetDatabase.Refresh();
                newTex = AssetDatabase.LoadAssetAtPath(rootPath + newTex.name + ".png", typeof(Texture2D)) as Texture2D;
                DestroyImmediate(font.material.mainTexture, true);
                font.material.mainTexture = newTex;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/I2 SmartEdge/Pack Font", false, 22)]
        [MenuItem("Assets/I2 SmartEdge/Pack Font", false, 22)]
        public static void PackFont()
        {
            if (Selection.activeObject == null || (Selection.activeObject as Font == null))
            {
                EditorUtility.DisplayDialog("You need to select an SDF Font", "No Font Asset is selected", "Ok");
                return;
            }

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("You need to select an SDF Font", "No Font Asset is selected", "Ok");
                return;
            }

            Font font = Selection.activeObject as Font;

            if (font.material == null)
            {
                EditorUtility.DisplayDialog("Font doesn't have a material", "Missing Font.material", "Ok");
                return;
            }

            var newTex = font.material.mainTexture as Texture2D;
            if (newTex != null)
            {
                var texPath = AssetDatabase.GetAssetPath(newTex);
                TextureTools.MakeTextureReadable(newTex);
                newTex = AssetDatabase.LoadAssetAtPath(texPath, typeof(Texture2D)) as Texture2D;
                newTex = Object.Instantiate<Texture2D>(newTex);
                newTex.name = font.material.mainTexture.name;

                AssetDatabase.AddObjectToAsset(newTex, font);
                DestroyImmediate(font.material.mainTexture, true);
                font.material.mainTexture = newTex;
            }

            var newMat = Object.Instantiate<Material>(font.material);
            newMat.name = font.material.name;
            AssetDatabase.AddObjectToAsset(newMat, font);
            DestroyImmediate(font.material, true);
            font.material = newMat;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/I2 SmartEdge/Delete SubAsset", false, 23)]
        [MenuItem("Assets/I2 SmartEdge/Delete SubAsset", false, 23)]
        public static void DeleteSubAsset()
        {
            if (Selection.activeObject == null)
            {
                EditorUtility.DisplayDialog("Unable to delete object", "No object is selected", "Ok");
                return;
            }

            if (EditorUtility.DisplayDialog("Delete Object", "Are you sure you want to delete object '"+Selection.activeObject+"'", "Delete", "Cancel"))
            {
                Object.DestroyImmediate(Selection.activeObject, true);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        #endregion
    }
}