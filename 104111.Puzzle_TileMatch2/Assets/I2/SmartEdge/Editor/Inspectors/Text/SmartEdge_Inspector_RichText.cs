using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace I2.SmartEdge
{
	public partial class SmartEdge_Inspector
	{
		SerializedProperty 	mSerialized_RichText, mSerialized_RichText_Enabled;

		//SerializedProperty  mSerialized_RichText_VerticalWrapMode, mSerialized_RichText_HorizontalWrapMode;


		void RegisterProperty_Text_RichText()
		{
			mSerialized_RichText                     = mSerialized_Text.FindPropertyRelative("_RichText");
			mSerialized_RichText_Enabled             = mSerialized_RichText.FindPropertyRelative("_Enabled");

			//mSerialized_RichText_VerticalWrapMode    = mSerialized_RichText.FindPropertyRelative("_VerticalWrapMode");
			//mSerialized_RichText_HorizontalWrapMode  = mSerialized_RichText.FindPropertyRelative("_HorizontalWrapMode");
        }

		void OnGUI_RichText ()
		{
			if (!GUITools.DrawHeader ("Rich Text", "SE RichText", true, mSerialized_RichText_Enabled.boolValue, EnableRichText, HelpURL: SE_InspectorTools.HelpURL_Text_RichText, disabledColor:GUITools.LightGray))
				return;

			EditorGUI.BeginChangeCheck();

			GUITools.BeginContents();
			   OnGUI_RichText_Content ();
			GUITools.EndContents ();

			if (EditorGUI.EndChangeCheck())
			{
				if (!mSerialized_RichText_Enabled.boolValue)
					EnableRichText(true);
                ApplyOriginalTextToLabel();
				mMakeVerticesDirty = true;
			}
		}

		void EnableRichText( bool enable )
		{
            ApplyOriginalTextToLabel();
            mSerialized_RichText_Enabled.boolValue = enable;
			mMakeVerticesDirty = true;
		}

		void OnGUI_RichText_Content()
		{
			EditorGUIUtility.labelWidth = 80;
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginVertical();

				OnGUI_RichText_Properties();

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		void OnGUI_RichText_Properties()
		{
		}
	}
}