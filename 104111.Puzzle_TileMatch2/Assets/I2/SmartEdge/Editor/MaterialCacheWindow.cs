using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//TODO: Removing 1 pixel lines
//TODO: Antialising borders

namespace I2.SmartEdge
{
	public class MaterialCacheWindow : EditorWindow
	{
        #region Variables

        Vector3 mScrollPos;
        List<Material> mExpandedMaterials = new List<Material>();
        int mInspectorWidth;
        string mFilter = string.Empty;
		
		#endregion
		
		#region Editor
		[MenuItem("Tools/I2 SmartEdge/Material Cache Window", false, 3)]
		[MenuItem("Assets/I2 SmartEdge/Open Material Cache Window", false, 3)]
		static void ShowWindow()
		{
			EditorWindow.GetWindow<MaterialCacheWindow>(false, "Material Cache", true);
		}
		
		public void OnEnable()
		{
		}


		#endregion
		
		#region Main Window

        void OnGUI()
        {
            mInspectorWidth = Screen.width;

            EditorGUIUtility.labelWidth = 50;

			GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 1);
			GUILayout.BeginVertical(SE_InspectorTools.GUIStyle_Background);
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Material Cache", SE_InspectorTools.GUIStyle_Header))
			{
				    //Application.OpenURL(SE_InspectorTools.HelpURL_Documentation);
			}

			GUILayout.Space (5);

            OnGUI_Materials();

			GUILayout.Space (10);
			GUILayout.FlexibleSpace();

            I2AboutWindow.OnGUI_Footer("I2 SmartEdge", SE_InspectorTools.GetVersion(), SE_InspectorTools.HelpURL_forum, SE_InspectorTools.HelpURL_Documentation);

            EditorGUIUtility.labelWidth = 0;

			GUILayout.EndVertical();
        }
		
		void OnGUI_Materials()
		{
            GUILayout.BeginHorizontal();
                GUILayout.Label("Number of Materials:", GUILayout.Width(150));
                GUILayout.Label(MaterialCache.mMaterialInstances.Count.ToString(), GUILayout.Width(30));
            //GUILayout.FlexibleSpace();

                mFilter = GUILayout.TextField(mFilter, "ToolbarSeachTextField");
                string buttonMode = string.IsNullOrEmpty(mFilter) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton";
                if (GUILayout.Button("", buttonMode))
                    mFilter = string.Empty;
            GUILayout.EndHorizontal();

            mScrollPos = GUILayout.BeginScrollView(mScrollPos);

            var filters = mFilter.Split(";, ".ToCharArray());

            foreach (var matInstance in MaterialCache.mMaterialInstances)
            {
                if (MaterialContainsFilter(matInstance.material, filters))
                    OnGUI_MaterialInstance(matInstance.material, matInstance);
            }

			GUILayout.EndScrollView();

            mExpandedMaterials.RemoveAll(x => (MaterialCache.GetMaterialInstance(x)==null));
        }

        bool MaterialContainsFilter(Material mat, string[] filters)
        {
            if (mat == null)
                return false;

            if (filters==null || filters.Length==0)
                return true;

            var keywords = mat.shaderKeywords;
            if (keywords == null || keywords.Length == 0)
                return true;

            for (int i = 0, imax = filters.Length; i < imax; ++i)
            {
                bool found = false;
                for (int k = 0; k < keywords.Length; ++k)
                    if (string.IsNullOrEmpty(keywords[k]) || keywords[k].IndexOf(filters[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found = true;
                        break;
                    }
                 if (!found)
                    return false;
            }

            return true;
        }



        void OnGUI_MaterialInstance( Material material, MaterialCache.MaterialInstance matInstance )
        {
            bool expanded = mExpandedMaterials.Contains(material);

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
                var newExpanded = GUILayout.Toggle(expanded, "", EditorStyles.foldout, GUITools.DontExpandWidth);
                if (GUILayout.Button(matInstance.NumberInstances.ToString(), EditorStyles.toolbarButton, GUILayout.Width(30))) SelectSmartEdgeUsing(material);
                GUILayout.Label(material.shader.name, GUILayout.Width(150));
                GUILayout.Label(string.Join(", ", material.shaderKeywords));
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (expanded)
            {
                OnGUI_MaterialInstance_Expanded(matInstance);
            }

            if (newExpanded!=expanded)
            {
                if (newExpanded) mExpandedMaterials.Add(material);
                            else mExpandedMaterials.Remove(material);
            }
        }

        private void OnGUI_MaterialInstance_Expanded( MaterialCache.MaterialInstance matInstance )
        {
            var matDef = matInstance.definition as MaterialDef_SDF;

            GUILayout.BeginVertical("AS TextArea", GUILayout.Height(1), GUILayout.Width(mInspectorWidth));

            GUILayout.BeginHorizontal();
                OnGUI_Texture("MainTexture", matDef.MainTexture);
                OnGUI_Texture("Face", matDef.TexFace);
                OnGUI_Texture("Outline", matDef.TexOutline);
                OnGUI_Texture("BumpMap", matDef.TexBumpMap);
                OnGUI_Texture("GlowMap", matDef.TexGlowMap);
                OnGUI_Texture("Environment", matDef.TexEnvironment);
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var transpWhite = new Color(1, 1, 1, 0);
            var transpBlack = new Color(0, 0, 0, 0);

            OnGUI_Int("Flags", matDef.Flags);
            OnGUI_V4f("GlobalData", matDef.GlobalData, MathUtils.v4zero);
            OnGUI_V4f("LightData", matDef.LightData, MathUtils.v4zero);
            OnGUI_V3f("LightDir", matDef.LightDir);
            OnGUI_Color("SpecularColor", matDef.LightSpecularColor, transpWhite);

            OnGUI_V4f("BevelData", matDef.BevelData, MathUtils.v4zero);
            OnGUI_V4f("BevelWave", matDef.BevelWaveData, MathUtils.v4zero);
            OnGUI_V4f("BumpData", matDef.BumpData, MathUtils.v4zero);
            OnGUI_V4f("EnvReflection", matDef.EnvReflectionData, new Vector4(100, 0, 0, 0));
            OnGUI_Color("EnvRefl Face", matDef.EnvReflectionColor_Face, transpWhite);
            OnGUI_Color("EnvRefl Outline", matDef.EnvReflectionColor_Outline, transpWhite);

            OnGUI_Color("GlowColor", matDef.GlowColor, transpWhite);
            OnGUI_V4f("GlowData", matDef.GlowData, MathUtils.v4zero);
            OnGUI_V4f("GlowOffset", matDef.GlowOffset, MathUtils.v4zero);

            OnGUI_Color("ShadowColor", matDef.ShadowColor, transpBlack);
            OnGUI_V4f("ShadowData", matDef.ShadowData, MathUtils.v4zero);
            OnGUI_Color("InnerShadow", matDef.InnerShadowColor, transpBlack);

            OnGUI_V4f("InnerShadowData", matDef.InnerShadowData, MathUtils.v4zero);
            OnGUI_V4f("InnerShadowOffset", matDef.InnerShadowOffset, MathUtils.v4zero);

            GUILayout.EndVertical();
        }

        void OnGUI_Texture(string text, Texture value)
        {
            if (value == null)
                return;
            GUILayout.BeginVertical();
            GUILayout.Label(text, SE_InspectorTools.Style_LabelCenterAligned);
            EditorGUILayout.ObjectField("", value, typeof(Texture), false, GUILayout.Width(mInspectorWidth/7.0f));
            GUILayout.EndVertical();
        }

        void OnGUI_Int(string text, int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, GUILayout.Width(100));
            EditorGUILayout.IntField("", value);
            GUILayout.EndHorizontal();
        }

        void OnGUI_V4f( string text, Vector4 value, Vector4 defValue)
        {
            if (value == defValue)
                return;
            GUILayout.BeginHorizontal();
                GUILayout.Label(text, GUILayout.Width(100));
                EditorGUILayout.Vector4Field("", value);
            GUILayout.EndHorizontal();
        }

        void OnGUI_V3f(string text, Vector3 value)
        {
            if (value == MathUtils.v3zero)
              return;
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, GUILayout.Width(100));
            EditorGUILayout.Vector3Field("", value);
            GUILayout.EndHorizontal();
        }

        void OnGUI_Color(string text, Color value, Color defColor)
        {
            if (value == defColor)
              return;
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, GUILayout.Width(100));
            EditorGUILayout.ColorField("", value);
            GUILayout.EndHorizontal();
        }



        #endregion


        void SelectSmartEdgeUsing( Material mat )
        {
            Selection.objects = Resources.FindObjectsOfTypeAll<SmartEdge>().Where(x=>x.GetRenderParams().mMaterial==mat).Select(x=>x.gameObject).ToArray();
        }
    }
}