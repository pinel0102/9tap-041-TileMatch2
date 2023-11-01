using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	public partial class SERenderParams_Inspector
    {
        SerializedProperty mSerialized_EnableFace, mSerialized_FaceColor,
                            mSerialized_EdgeSize, mSerialized_Softness, mSerialized_FaceSoftness,// mSerialized_SDF_Spread,
							mSerialized_EnableFaceGradient, mSerialized_FaceGradient, mSerialized_UseWidgetTexture, mSerialized_UseSuperSampling;

        SmartEdgeTextureInspector mFaceTextureDef = new SmartEdgeTextureInspector();


        void RegisterProperty_Face()
		{
			mSerialized_EnableFace   = mSerialized_RenderParams.FindPropertyRelative ("_EnableFace");
			mSerialized_FaceColor    = mSerialized_RenderParams.FindPropertyRelative ("_FaceColor");
			mSerialized_EdgeSize     = mSerialized_RenderParams.FindPropertyRelative ("_EdgeSize");
			mSerialized_Softness     = mSerialized_RenderParams.FindPropertyRelative ("_Softness");
            mSerialized_FaceSoftness = mSerialized_RenderParams.FindPropertyRelative("_FaceSoftness");
			//mSerialized_SDF_Spread = mSerialized_ActiveRenderPreset.FindPropertyRelative ("_SDF_Spread");

			mSerialized_UseWidgetTexture	= mSerialized_RenderParams.FindPropertyRelative ("_UseWidgetTexture");
			mFaceTextureDef.Set( mSerialized_RenderParams.FindPropertyRelative("_FaceTexture") );

			mSerialized_EnableFaceGradient 	= mSerialized_RenderParams.FindPropertyRelative ("_EnableFaceGradient");
			mSerialized_FaceGradient 		= mSerialized_RenderParams.FindPropertyRelative ("_FaceGradient");

            mSerialized_UseSuperSampling    = mSerialized_RenderParams.FindPropertyRelative("_UseSuperSampling");

        }

        void OnGUI_Face( /*bool forceExpand = true */)
		{
			EditorGUIUtility.labelWidth = 60;
			
			if (!GUITools.DrawHeader ("Face", true/*"SmartEdge Face"*/, true, mSerialized_EnableFace.boolValue, EnableFace, HelpURL: SE_InspectorTools.HelpURL_Face, disabledColor:GUITools.LightGray, allowCollapsing: false))
				return;

            EditorGUI.BeginChangeCheck();

            GUITools.BeginContents();
                var c = GUI.color;
                if (!mSerialized_EnableFace.boolValue) GUI.color = new Color(1, 1, 1, 0.3f);
                GUILayout.Space (2);

                EditorGUI.BeginChangeCheck();

				mSerialized_EdgeSize.floatValue = 1-EditorGUILayout.Slider ("Edge", 1-mSerialized_EdgeSize.floatValue, 0, 1);
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(mSerialized_Softness, new GUIContent("Softness"));
                    GUILayout.Label("px", EditorStyles.miniLabel, GUITools.DontExpandWidth);
                    GUILayout.Label("Extra", EditorStyles.miniLabel, GUITools.DontExpandWidth);
                    EditorGUILayout.PropertyField(mSerialized_FaceSoftness, GUITools.EmptyContent);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent("SuperSampling", "Enabling this option increases the quality of SMALL texts at the expense of a bit of performance"), GUITools.DontExpandWidth);
                    EditorGUILayout.PropertyField(mSerialized_UseSuperSampling, GUITools.EmptyContent, GUILayout.Width(18));
                GUILayout.EndHorizontal();
                mMakeMaterialDirty |= EditorGUI.EndChangeCheck();


				//EditorGUILayout.PropertyField (mSerialized_SDF_Spread, new GUIContent("Spread"));

                GUILayout.Space (10);

                EditorGUILayout.PropertyField (mSerialized_FaceColor, new GUIContent("Color"));
                mMakeVerticesDirty |= EditorGUI.EndChangeCheck();

                GUILayout.Space (10);
				mMakeMaterialDirty |= OnGUI_Face_UseWidgetTexture();

				GUILayout.Space (10);
				mFaceTextureDef.OnGUI_Texture( false, true, ref mMakeVerticesDirty, ref mMakeMaterialDirty);

				GUILayout.Space (10);
				mMakeVerticesDirty |= OnGUI_Gradient( mSerialized_EnableFaceGradient, mSerialized_FaceGradient, mRenderParams._FaceGradient);

                GUI.color = c;
            GUITools.EndContents ();

            //if (GUILayout.Button ("Generate"))
            //	GenerateSpecularTexture();

            if (EditorGUI.EndChangeCheck() && !mSerialized_EnableFace.boolValue)
                EnableFace(true);
		}
		/*
		void GenerateSpecularTexture()
		{
			Texture2D lookup = new Texture2D(512, 512, TextureFormat.RGBA32, false);
			Color[] colors = lookup.GetPixels();

			for (int x=0; x<lookup.width; ++x)
			for (int y=0; y<lookup.height; ++y)
			{
				float power = 16f*(y/(float)lookup.height);
				float NdotL = (x/(float)lookup.width);
				float intensity = Mathf.Pow(NdotL, power);
				intensity = Mathf.Clamp01 (intensity);
				Color c = new Color(intensity, intensity, intensity, 1);
				colors[y*lookup.width+x] = c;
			}

			lookup.SetPixels(colors);

			TextureTools.Bilinear(lookup, 128, 128);

			byte[] bytes = lookup.EncodeToPNG();
			string AssetPath = "SpecularLookup.png";
			System.IO.File.WriteAllBytes(Application.dataPath + "/" + AssetPath, bytes);
			AssetDatabase.ImportAsset("Assets/" + AssetPath);
			//DestroyImmediate(lookup);
		}*/

        bool OnGUI_Face_UseWidgetTexture()
		{
			bool CanUseTextureWidget = (!mFaceTextureDef.mSerialized_Enable.boolValue &&
			                            !mOutlineTextureDef.mSerialized_Enable.boolValue &&
			                            !mGlowTextureDef.mSerialized_Enable.boolValue &&
										!mNormalMapDef.mSerialized_Enable.boolValue && 
                                        (mSmartEdge!=null && !mSmartEdge.IsMSDF()));
            var wasEnabled = GUI.enabled;
            GUI.enabled = CanUseTextureWidget;
            EditorGUI.BeginChangeCheck();

			GUILayout.BeginHorizontal(GUILayout.Width(1));

				if (CanUseTextureWidget)
					EditorGUILayout.PropertyField(mSerialized_UseWidgetTexture, new GUIContent(""), GUILayout.Width(20));
				else
					GUILayout.Toggle(false, "",  GUILayout.Width(20));

			GUILayout.Label(new GUIContent("Use Widget Texture", "Use the Font/Sprite texture's RGB color"));
			GUILayout.EndHorizontal();
            GUI.enabled = wasEnabled;
            return EditorGUI.EndChangeCheck();
		}

        bool OnGUI_Gradient(SerializedProperty PropEnabled, SerializedProperty PropGradient, GradientEffect gradEffect, bool Collapsable = false)
		{
            EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal();
				if (!PropEnabled.boolValue && Collapsable) 
					GUILayout.FlexibleSpace();

				GUILayout.BeginVertical(GUILayout.Width(1));
					if (PropEnabled.boolValue || !Collapsable) 
						GUILayout.Space(60);
					PropEnabled.boolValue = EditorGUILayout.Toggle(PropEnabled.boolValue, GUILayout.Width(20));
				GUILayout.EndVertical();

				if (PropEnabled.boolValue || !Collapsable)
				{
					GUI.backgroundColor = GUITools.LightGray;
					GUITools.BeginContents();
						GUI.backgroundColor = Color.white;
						GradientVEInspector.OnGUIGradient (PropGradient, gradEffect);
					GUITools.EndContents(false);
				}
				else
					GUILayout.Label("Use Gradient");

			if (!PropEnabled.boolValue && Collapsable) 
				GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();
            return EditorGUI.EndChangeCheck();
		}

        void EnableFace(bool enable)
		{
			mSerialized_EnableFace.boolValue = enable;
            mMakeVerticesDirty = true;
            SERenderParams_Inspector.mSelectedRenderEffect = SmartEdgeRenderEffect.Color;
        }
    }
}