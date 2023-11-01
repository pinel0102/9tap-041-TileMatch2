using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	public partial class SERenderParams_Inspector
    {
        SerializedProperty mSerialized_LightSpecularPower, mSerialized_LightSpecularStart, mSerialized_LightSpecularSoftness, mSerialized_LightSpecularColor, mSerialized_LightDiffuseShadow, mSerialized_LightDiffuseHighlight,
							mSerialized_LightAngle, mSerialized_LightAltitude;

        void RegisterProperty_Lighting()
		{
			mSerialized_LightSpecularPower 		= mSerialized_RenderParams.FindPropertyRelative ("_LightSpecularPower");
            mSerialized_LightSpecularStart      = mSerialized_RenderParams.FindPropertyRelative ("_LightSpecularStart");
            mSerialized_LightSpecularSoftness   = mSerialized_RenderParams.FindPropertyRelative ("_LightSpecularSoftness");
            mSerialized_LightSpecularColor      = mSerialized_RenderParams.FindPropertyRelative ("_LightSpecularColor");
            mSerialized_LightDiffuseShadow      = mSerialized_RenderParams.FindPropertyRelative ("_LightDiffuseShadow");
			mSerialized_LightDiffuseHighlight 	= mSerialized_RenderParams.FindPropertyRelative ("_LightDiffuseHighlight");

			mSerialized_LightAngle 		= mSerialized_RenderParams.FindPropertyRelative ("_LightAngle");
			mSerialized_LightAltitude 	= mSerialized_RenderParams.FindPropertyRelative ("_LightAltitude");
		}

        void OnGUI_Lighting()
        {
			if (!GUITools.DrawHeader ("Lighting", true/*"SmartEdge Lighting"*/, true, mSerialized_EnableBevel.boolValue || mNormalMapDef.mSerialized_Enable.boolValue, EnableLighting, HelpURL: SE_InspectorTools.HelpURL_Lighting, disabledColor:GUITools.LightGray, allowCollapsing: false))
				return;

            GUI.changed = false;
			EditorGUIUtility.labelWidth = 60;
			GUITools.BeginContents();
                var c = GUI.color;
                if (!mSerialized_EnableBevel.boolValue && !mNormalMapDef.mSerialized_Enable.boolValue) GUI.color = new Color(1, 1, 1, 0.3f);

                GUILayout.Space (2);
				GUILayout.BeginHorizontal();
					GUILayout.BeginVertical (GUILayout.Width(1));
						Rect rect = GUILayoutUtility.GetRect (50, 50);
						Texture2D texBackground = SmartEdgeTools.SmartEdgeSkin.FindStyle ("Angle Circle").normal.background;
						Texture2D texLine = SmartEdgeTools.SmartEdgeSkin.FindStyle ("Angle KnobLine").normal.background;
                        EditorGUI.BeginChangeCheck();
						mSerialized_LightAngle.floatValue = GUITools.AngleCircle (rect, mSerialized_LightAngle.floatValue, 0.1f, 0, 360, texBackground, texLine);
                        if (EditorGUI.EndChangeCheck())
                            //GUIUtility.ExitGUI();
                            SceneView.RepaintAll();
					GUILayout.EndVertical ();

					GUILayout.Space(5);

					GUILayout.BeginVertical();
						EditorGUILayout.PropertyField (mSerialized_LightAngle, new GUIContent("Angle"));
						EditorGUILayout.PropertyField (mSerialized_LightAltitude, new GUIContent("Altitude"));
					GUILayout.EndVertical();
				GUILayout.EndHorizontal();


                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                    GUILayout.Label("Diffuse:", EditorStyles.boldLabel, GUILayout.Width(80));
                    GUILayout.BeginVertical();
                        EditorGUILayout.PropertyField(mSerialized_LightDiffuseHighlight, new GUIContent("Light"));
                        EditorGUILayout.PropertyField(mSerialized_LightDiffuseShadow, new GUIContent("Shadow"));
                    GUILayout.EndVertical();
                GUILayout.EndHorizontal();


                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                    GUILayout.Label("Specular:", EditorStyles.boldLabel, GUILayout.Width(80));
                    GUILayout.BeginVertical();
                        EditorGUILayout.PropertyField(mSerialized_LightSpecularStart, new GUIContent("Width"));
                        EditorGUILayout.PropertyField(mSerialized_LightSpecularSoftness, new GUIContent("Softness"));
                        GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(mSerialized_LightSpecularPower, new GUIContent("Power"));
                            if (GUILayout.Button("Linear", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false))) mSerialized_LightSpecularPower.floatValue = (1 - 0.001f) / 8.0f;
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(mSerialized_LightSpecularColor, new GUIContent("Color"));
                    GUILayout.EndVertical();
                GUILayout.EndHorizontal();

            GUI.color = c;
            GUITools.EndContents ();
			EditorGUIUtility.labelWidth = 50;
            if (GUI.changed)
			{
				mMakeMaterialDirty = true;
                mMakeVerticesDirty = true;
			}
		}

        void EnableLighting( bool enable )
		{
			//mSerialized_EnableGlow.boolValue = enable;
		}
	}
}