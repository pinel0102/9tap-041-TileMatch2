using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SmartEdge),true)]
	public partial class SmartEdge_Inspector : Editor 
	{
		SmartEdge mTarget;


        Rect mRect;
        //Vector2 mRectPivot;

		enum eMainTab { Render, Text, Transform, Animation };
		static eMainTab mMainTab = eMainTab.Render;

		bool mMakeMaterialDirty, mMakeVerticesDirty;

		public void OnEnable()
		{
            mTarget = target as SmartEdge;

            RegisterProperty_Presets();
            RegisterProperty_Deformation();
            RegisterProperty_Text();
            RegisterProperty_Animation();

            mMainTab = (eMainTab)EditorPrefs.GetInt("SmartEdge MainTab", 0);
            //EditorApplication.update += UpdateAnims;
        }

        public void OnDisable()
		{
            // EditorApplication.update -= UpdateAnims;
            //DestroyImmediate( mBevelTexture );
            if (!Application.isPlaying && mTarget!=null)
            {
                if (mEditor_SelectedAnim != null)
                    mEditor_SelectedAnim.OnDestroy();
                mTarget.StopAllAnimations(false);
                foreach (var se in targets)
                    (se as SmartEdge).MarkWidgetAsChanged(mMakeVerticesDirty, mMakeMaterialDirty);
            }
        }

 

        /*public void UpdateAnims()
        {
            mTarget.Update();
        }*/

        public override  void OnInspectorGUI()
		{
            mMakeMaterialDirty = mMakeVerticesDirty = false;

			GUITools.Editor_UpdateIfRequiredOrScript(serializedObject);
            if (mSerializedObject_RenderPreset != null)
                GUITools.Editor_UpdateIfRequiredOrScript( mSerializedObject_RenderPreset );


            mRect = mTarget.mRect;
			//mRectPivot = mTarget.mRectPivot;

			EditorGUIUtility.labelWidth = 50;

            GUILayout.BeginHorizontal();
            GUILayout.Space(-15);


			GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 1);
			GUILayout.BeginVertical(SE_InspectorTools.GUIStyle_Background, GUILayout.Height(1));
			GUI.backgroundColor = Color.white;

			/*if (GUILayout.Button("Smart Edge", SE_InspectorTools.GUIStyle_Header) ||
			    GUILayout.Button("Pixel Perfect STYLES for TEXT, SPRITES and IMAGES", SE_InspectorTools.GUIStyle_SubHeader))
			{
				    //Application.OpenURL(SE_InspectorTools.HelpURL_Documentation);
			}*/

			GUILayout.Space (5);

			var newTab = (eMainTab)GUITools.DrawShadowedTabs( (int)mMainTab, System.Enum.GetNames(typeof(eMainTab)));
            if (newTab != mMainTab)
            {
                mMainTab = newTab;
                EditorPrefs.SetInt("SmartEdge MainTab", (int)mMainTab);
                SceneView.RepaintAll();
            }


			switch (mMainTab) 
			{
                case eMainTab.Render:       OnGUI_Render(); break;
                case eMainTab.Text:         OnGUI_TextTab(); break;
                case eMainTab.Transform:	OnGUI_Transform();	break;
				case eMainTab.Animation:	OnGUI_Animations();	break;
			}

			GUILayout.Space (10);
			GUILayout.FlexibleSpace();

            I2AboutWindow.OnGUI_Footer("I2 SmartEdge", SE_InspectorTools.GetVersion(), SE_InspectorTools.HelpURL_forum, SE_InspectorTools.HelpURL_Documentation);

            EditorGUIUtility.labelWidth = 0;

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (serializedObject.ApplyModifiedProperties() || mMakeMaterialDirty || mMakeVerticesDirty) 
			{
                if (mSerializedObject_RenderPreset != null && (mSerializedObject_RenderPreset.ApplyModifiedProperties() || mMakeMaterialDirty || mMakeVerticesDirty))
                {
                    mRenderPreset.NotifyRenderPresetChanged(mMakeMaterialDirty, mMakeVerticesDirty);
                }
                foreach (var se in targets)
				    (se as SmartEdge).MarkWidgetAsChanged (mMakeVerticesDirty, mMakeMaterialDirty);
                SceneView.RepaintAll();
			}
        }

         public void OnGUI_Render()
        {
            if (mTarget.mSETextureFormat == SmartEdge.SETextureFormat.Unknown)
            {
                var textureName = mTarget.cache_MainTexture == null ? "none" : mTarget.cache_MainTexture.name;
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();

                string fontName = null;

                var uiText = mTarget.GetComponent<Text>();
                if (uiText != null && uiText.font != null)
                    fontName = uiText.font.name;

                #if SE_NGUI
                    var label = mTarget.mNGUI_Widget as UILabel;
                    if (label != null && label.ambigiousFont != null)
                        fontName = label.ambigiousFont.name;
                #endif


                if (!string.IsNullOrEmpty(fontName))
                    EditorGUILayout.HelpBox(string.Format("Font '{0}' is not an SDF, MSDF or MSDFA font.\n\nTo access the RENDER effects you will need to change it or create one using the SDF FontMaker\n\nTRANSFORM and ANIMATION effects will still work", fontName), MessageType.Warning);
                else
                    EditorGUILayout.HelpBox(string.Format("Font/Sprite Texture '{0}' is not an SDF, MSDF or MSDFA texture.\n\nTo access the RENDER effects you will need to change it or create one using the SDF FontMaker\n\nTRANSFORM and ANIMATION effects will still work", textureName), MessageType.Warning);

                if (GUILayout.Button(GUITools.Icon_Help, EditorStyles.label, GUILayout.ExpandWidth(false)))
                    Application.OpenURL(SE_InspectorTools.HelpURL_CreateSDFasset);
                GUILayout.EndHorizontal();
                return;
            }

            OnGUI_Presets();

            mRenderParamsInspector.OnGUI_RenderParams();

            mMakeMaterialDirty |= SERenderParams_Inspector.mMakeMaterialDirty;
            mMakeVerticesDirty |= SERenderParams_Inspector.mMakeVerticesDirty;
        }


        public void OnGUI_Transform()
		{
			GUILayout.Space (5);
			OnGUI_Deformation ();
		}



		public static string GetI2SmartEdgePath()
		{
			string[] assets = AssetDatabase.FindAssets("SmartEdgeManager");
			if (assets.Length==0)
				return string.Empty;

			string PluginPath = AssetDatabase.GUIDToAssetPath(assets[0]);
            PluginPath = PluginPath.Substring(0, PluginPath.Length - "/Scripts/SmartEdgeManager.cs".Length);

			return PluginPath;
		}
	}
}