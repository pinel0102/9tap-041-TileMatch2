using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	public partial class SmartEdge_Inspector
	{
		SerializedProperty 	mSerialized_Text,
                            mSerialized_Text_mOriginalText;

		
		void RegisterProperty_Text()
		{
            mSerialized_Text                = serializedObject.FindProperty("_TextEffect");
            mSerialized_Text_mOriginalText  = serializedObject.FindProperty("mOriginalText");

            RegisterProperty_Text_Spacing();
            RegisterProperty_Text_BestFit();
            RegisterProperty_Text_TextWrapping();
            RegisterProperty_Text_RichText();
        }
		
		void OnGUI_TextTab ()
		{
            /*if (GUITools.DrawHeader("Text", "SmartEdge Text", true, true, null, HelpURL: SE_InspectorTools.HelpURL_Text, disabledColor: GUITools.LightGray))
            {
                EditorGUI.BeginChangeCheck();

                GUITools.BeginContents();
                TextVEInspector.OnGUIText(mSerialized_Text);
                GUITools.EndContents();

                if (EditorGUI.EndChangeCheck())
                {
                    mMakeVerticesDirty = true;
                }
            }*/

            OnGUI_FinalText();

            OnGUI_Spacing();
            OnGUI_TextWrapping();
            OnGUI_BestFit();
            OnGUI_RichText();
            OnGUI_CommingSoon("Hyperlinks");
        }

        void OnGUI_FinalText()
        {
            var label = mTarget.mGraphic as UnityEngine.UI.Text;
            if (label == null)
                return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(mSerialized_Text_mOriginalText, new GUIContent("Unprocessed Text:"));
            if (EditorGUI.EndChangeCheck())
            {
                ApplyOriginalTextToLabel();
            }
        }

        void ApplyOriginalTextToLabel()
        {
            mMakeVerticesDirty = true;

            foreach (var tg in mTargets)
            {
                var se = tg as SmartEdge;
                if (se == null || !(se.mGraphic is Text)) continue;

                ((Text) se.mGraphic).text = mSerialized_Text_mOriginalText.stringValue;
                se.mFinalText = string.Empty;
            }
        }
    }
}