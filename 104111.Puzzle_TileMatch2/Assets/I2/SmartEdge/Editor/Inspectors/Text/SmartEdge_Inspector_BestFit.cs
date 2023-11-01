using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace I2.SmartEdge
{
	public partial class SmartEdge_Inspector
	{
		SerializedProperty 	mSerialized_BestFit, mSerialized_BestFit_Enabled;

        SerializedProperty  mSerialized_BestFit_FitWidth, mSerialized_BestFit_FitHeight, mSerialized_BestFit_KeepAspectRatio, mSerialized_BestFit_TargetRect,
                            mSerialized_BestFit_Border, mSerialized_BestFit_Scale,
                            mSerialized_BestFit_Snapping;


        void RegisterProperty_Text_BestFit()
		{
            mSerialized_BestFit            = mSerialized_Text.FindPropertyRelative("_BestFit");

            mSerialized_BestFit_Enabled    = mSerialized_BestFit.FindPropertyRelative("_Enabled");

            mSerialized_BestFit_FitWidth        = mSerialized_BestFit.FindPropertyRelative("_FitWidth");
            mSerialized_BestFit_FitHeight       = mSerialized_BestFit.FindPropertyRelative("_FitHeight");
            mSerialized_BestFit_KeepAspectRatio = mSerialized_BestFit.FindPropertyRelative("_KeepAspectRatio");
            mSerialized_BestFit_TargetRect      = mSerialized_BestFit.FindPropertyRelative("_TargetRect");

            mSerialized_BestFit_Border     = mSerialized_BestFit.FindPropertyRelative("_Border");
            mSerialized_BestFit_Scale      = mSerialized_BestFit.FindPropertyRelative("_Scale");

            mSerialized_BestFit_Snapping   = mSerialized_BestFit.FindPropertyRelative("_Snapping");
        }

        void OnGUI_BestFit ()
		{
			if (!GUITools.DrawHeader ("Best Fit", "SE BestFit", true, mSerialized_BestFit_Enabled.boolValue, EnableBestFit, HelpURL: SE_InspectorTools.HelpURL_Text_BestFit, disabledColor:GUITools.LightGray))
				return;

            EditorGUI.BeginChangeCheck();

			GUITools.BeginContents();
               OnGUI_BestFit_Content ();
			GUITools.EndContents ();

            if (EditorGUI.EndChangeCheck())
            {
                if (!mSerialized_BestFit_Enabled.boolValue)
                    EnableBestFit(true);
                mMakeVerticesDirty = true;
            }
		}

		void EnableBestFit( bool enable )
		{
            mSerialized_BestFit_Enabled.boolValue = enable;
            mMakeVerticesDirty = true;
		}

        void OnGUI_BestFit_Content()
        {
            EditorGUIUtility.labelWidth = 80;
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

                GUILayout.BeginHorizontal("Box");
                    GUILayout.BeginVertical();
                    EditorGUILayout.PropertyField(mSerialized_BestFit_FitWidth, new GUIContent("Width"));
                    EditorGUILayout.PropertyField(mSerialized_BestFit_FitHeight, new GUIContent("Height"));
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
                    GUILayout.Space(9);
                    EditorGUIUtility.labelWidth = 115;
                    EditorGUILayout.PropertyField(mSerialized_BestFit_KeepAspectRatio, new GUIContent("Keep Aspect Ratio"));
                    EditorGUIUtility.labelWidth = 80;
                    GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(mSerialized_BestFit_TargetRect, new GUIContent("Area"));
                EditorGUILayout.PropertyField(mSerialized_BestFit_Border, true);
                EditorGUILayout.PropertyField(mSerialized_BestFit_Scale);

                EditorGUILayout.PropertyField(mSerialized_BestFit_Snapping);


                var uiText = (mTarget.mGraphic as Text);
                if (uiText != null && uiText.verticalOverflow != VerticalWrapMode.Overflow)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Overflow is set to Truncate which could hide some characters.", MessageType.Warning);
                    var rect = GUILayoutUtility.GetLastRect();
                    if (GUI.Button(rect, "", EditorStyles.label))
                        Application.OpenURL(SE_InspectorTools.HelpURL_Text_BestFit);

                    if (GUILayout.Button("Fix", GUILayout.ExpandHeight(true)))
                    {
                        uiText.verticalOverflow = VerticalWrapMode.Overflow;
                        EditorUtility.SetDirty(uiText);
                        GUITools.Editor_MarkSceneDirty();
                    }
                    GUILayout.EndHorizontal();
                }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

    }
}