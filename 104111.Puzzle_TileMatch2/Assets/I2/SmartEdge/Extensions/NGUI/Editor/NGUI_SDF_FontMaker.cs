#if SE_NGUI
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using I2;
using System.Threading;
using System.Runtime.InteropServices;

namespace I2.SmartEdge
{
	public partial class SDF_FontMaker
	{
		void SaveNGUIFont()
		{
            if (mFonts.Length<0 || mFonts[0]==null)
                return;
            string sPath = AssetDatabase.GetAssetPath(mFonts[0]);

            string sdfFormat = (mSDFFormat == eSDFFormat.RasterSDF) ? "SDF" : mSDFFormat.ToString();
            string prefabPath = EditorUtility.SaveFilePanelInProject("Save As", mFonts[0].name + "_NGUI_" + sdfFormat + ".prefab", "prefab", "Save font as...", System.IO.Path.GetDirectoryName(string.IsNullOrEmpty(OutputPath) ? sPath : OutputPath));

            if (string.IsNullOrEmpty(prefabPath)) return;
            NGUISettings.currentPath = System.IO.Path.GetDirectoryName(prefabPath);

            string texturePath = prefabPath.Replace(".prefab", ".png");
            TextureTools.SaveTexture(mPreviewTexture, texturePath);


            // Load the font's prefab
            GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
            Object prefab = null;
            string fontName;
            UIFont uiFont = null;

            // Font doesn't exist yet
            if (go == null || go.GetComponent<UIFont>() == null)
            {
                // Create a new prefab for the atlas
                prefab = PrefabUtility.SaveAsPrefabAsset(new GameObject(), prefabPath);

                fontName = prefabPath.Replace(".prefab", "");
                fontName = fontName.Substring(prefabPath.LastIndexOfAny(new char[] { '/', '\\' }) + 1);

                // Create a new game object for the font
                go = new GameObject(fontName);
                uiFont = go.AddComponent<UIFont>();
            }
            else
            {
                uiFont = go.GetComponent<UIFont>();
                fontName = go.name;
            }

            //if (create == Create.Import)
            {
                Material mat = null;

                // Create a material for the font
                string matPath = prefabPath.Replace(".prefab", ".mat");
                mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;

                // If the material doesn't exist, create it
                if (mat == null)
                {
                    Shader shader = Shader.Find("Unlit/Transparent Colored");
                    mat = new Material(shader);

                    // Save the material
                    AssetDatabase.CreateAsset(mat, matPath);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                    // Load the material so it's usable
                    mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
                }

                mat.mainTexture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;

                uiFont.dynamicFont = null;
                string fontData = ExportFontTxt(texturePath);
                BMFontReader.Load(uiFont.bmFont, NGUITools.GetHierarchy(uiFont.gameObject), System.Text.Encoding.UTF8.GetBytes(fontData));

                uiFont.atlas = null;
                uiFont.material = mat;
                NGUISettings.FMSize = uiFont.defaultSize;
            }
            uiFont._SDF_Spread = Spread;

            if (prefab != null)
            {
                // Update the prefab
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                DestroyImmediate(go);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                // Select the atlas
                go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
                uiFont = go.GetComponent<UIFont>();
            }

            if (uiFont != null)
            {
                NGUISettings.FMFont = null;
                NGUISettings.BMFont = uiFont;
            }

            NGUITools.SetDirty(uiFont);
            NGUITools.SetDirty(uiFont.material);
            NGUITools.SetDirty(uiFont.texture);
            //MarkAsChanged ()
            {
                Object obj = (Object)NGUISettings.FMFont ?? (Object)NGUISettings.BMFont;

                if (obj != null)
                {
                    List<UILabel> labels = NGUIEditorTools.FindAll<UILabel>();

                    foreach (UILabel lbl in labels)
                    {
                        if (lbl.ambigiousFont == obj)
                        {
                            lbl.ambigiousFont = null;
                            lbl.ambigiousFont = obj;
                        }
                    }
                }
            }
        }

		string ExportFontTxt( string texturePath )
		{
            int TextureWidth = mPreviewTexture.width;
            int TextureHeight = mPreviewTexture.height;


            float scale = 1 / 64.0f;
            //            scale *= mLineHeight / (float)(mLineHeight + Spread);

            float scale1 = 1;// mLineHeight / (float)(mLineHeight + Spread / scale);
            int lineHeight = (int)(mLineHeight * scale1);
            //int ascender = (int)(mAscender * scale1);
            //int descender = (int)(mDescender * scale1);


            StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("info face=\"{0}\" size={1} bold=0 italic=0 charset=\"\" unicode=1 stretchH=100 smooth=1 aa=1 padding=0,0,0,0 spacing=1,1 outline=0\n", System.IO.Path.GetFileNameWithoutExtension(texturePath), mFontSize);
			sb.AppendFormat ("common lineHeight={0} base={1} scaleW={2} scaleH={3} pages=1 packed=0 alphaChnl=0 redChnl=4 greenChnl=4 blueChnl=4\n", lineHeight*scale, 0, TextureWidth, TextureHeight);
			sb.AppendFormat ("page id=0 file=\"{0}\"\n", texturePath);
			sb.AppendFormat ("chars count={0}\n", CharInfos.Count);

            //#if UNITY_5_3_3 || UNITY_5_4_OR_NEWER
            //    float yOffset = mAscender;
            //#else
            //    float yOffset = mAscender;
            //#endif

            for (int i=0, imax= CharInfos.Count; i<imax; ++i)
			{
				CharacterInfo info = CharInfos[i];

                var uvTop = (int)((1 - info.uvTopLeft.y) * TextureHeight);
                var cHeight = (int)((info.uvTopRight - info.uvBottomLeft).y * TextureHeight);

                //int minY = (info.minY < info.maxY) ? info.minY : (info.maxY/* + mAscender*/);
                int maxY = (info.minY < info.maxY) ? info.maxY : (info.minY/* + mAscender*/);

                sb.AppendFormat ("char id={0}   ", info.index);
                sb.AppendFormat ("x={0}   y={1}    width={2}     height={3}     ", (int)(info.uvBottomLeft.x*TextureWidth), uvTop, (int)((info.uvTopRight-info.uvBottomLeft).x*TextureWidth), cHeight);
                sb.AppendFormat("xoffset={0}     yoffset={1}    xadvance={2}     page=0  chnl=15\n", (int)(info.minX * scale), (int)((mAscender - maxY) * scale), (int)(info.advance * scale));
            }

            sb.AppendFormat("kernings count={0}\n", CharKerning.Length);

            for (int i = 0; i < CharKerning.Length; ++i)
            {
                sb.AppendFormat("kerning first={0}  second={1} amount={2}\n", (int)CharKerning[i].Character, (int)CharKerning[i].NextCharacter, CharKerning[i].offset * scale);
            }

            return sb.ToString ();
		}
    }
}
#endif