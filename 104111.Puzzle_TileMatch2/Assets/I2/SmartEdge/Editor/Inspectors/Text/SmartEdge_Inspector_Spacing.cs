using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace I2.SmartEdge
{
	public partial class SmartEdge_Inspector
	{
		SerializedProperty 	mSerialized_Spacing, mSerialized_Spacing_Enabled;

        SerializedProperty  //mSerialized_Spacing_FontSettings, 
                            mSerialized_Spacing_Space_Character, mSerialized_Spacing_Space_Word, mSerialized_Spacing_Space_Line, mSerialized_Spacing_Space_Paragraph, mSerialization_Spacing_Paragraph_FirstLineMargin,
                            mSerialized_Spacing_KerningPairs;


        void RegisterProperty_Text_Spacing()
		{
            mSerialized_Spacing            = mSerialized_Text.FindPropertyRelative("_Spacing");
            mSerialized_Spacing_Enabled    = mSerialized_Spacing.FindPropertyRelative("_Enabled");

            //mSerialized_Spacing_FontSettings    = mSerialized_Spacing.FindPropertyRelative("_FontSettings");
            mSerialized_Spacing_KerningPairs     = mSerialized_Spacing.FindPropertyRelative("_KerningPairs");

            mSerialized_Spacing_Space_Character = mSerialized_Spacing.FindPropertyRelative("_Space_Character");
            mSerialized_Spacing_Space_Word      = mSerialized_Spacing.FindPropertyRelative("_Space_Word");
            mSerialized_Spacing_Space_Line      = mSerialized_Spacing.FindPropertyRelative("_Space_Line");
            mSerialized_Spacing_Space_Paragraph = mSerialized_Spacing.FindPropertyRelative("_Space_Paragraph");
            mSerialization_Spacing_Paragraph_FirstLineMargin = mSerialized_Spacing.FindPropertyRelative("_Paragraph_FirstLineMargin");
        }

        void OnGUI_Spacing ()
		{
			if (!GUITools.DrawHeader ("Spacing", "SE Spacing", true, mSerialized_Spacing_Enabled.boolValue, EnableSpacing, HelpURL: SE_InspectorTools.HelpURL_Text_Spacing, disabledColor:GUITools.LightGray))
				return;

            EditorGUI.BeginChangeCheck();

			GUITools.BeginContents();
               OnGUI_Spacing_Content ();
			GUITools.EndContents ();

            if (EditorGUI.EndChangeCheck())
            {
                if (!mSerialized_Spacing_Enabled.boolValue)
                    EnableSpacing(true);
                mMakeVerticesDirty = true;
            }
		}

		void EnableSpacing( bool enable )
		{
            mSerialized_Spacing_Enabled.boolValue = enable;
            mMakeVerticesDirty = true;
		}

        void OnGUI_Spacing_Content()
        {
            EditorGUIUtility.labelWidth = 80;
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

                //EditorGUILayout.PropertyField(mSerialized_Spacing_FontSettings, new GUIContent("Font Settings"));
                //GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                    GUILayout.Space(-10);
                    GUILayout.Label("Constant", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(mSerialized_Spacing_Space_Character, new GUIContent("Character"));
                EditorGUILayout.PropertyField(mSerialized_Spacing_Space_Word, new GUIContent("Word"));
                EditorGUILayout.PropertyField(mSerialized_Spacing_Space_Line, new GUIContent("Line"));
                EditorGUILayout.PropertyField(mSerialized_Spacing_Space_Paragraph, new GUIContent("Paragraph"));
                EditorGUILayout.PropertyField(mSerialization_Spacing_Paragraph_FirstLineMargin, new GUIContent("1st Line"));
                
            

                GUILayout.Space(10);
                EditorGUILayout.PropertyField(mSerialized_Spacing_KerningPairs, new GUIContent("Kerning"), true);


            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

    }
}