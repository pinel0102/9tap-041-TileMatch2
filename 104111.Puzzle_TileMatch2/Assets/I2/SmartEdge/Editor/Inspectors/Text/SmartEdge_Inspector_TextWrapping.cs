using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace I2.SmartEdge
{
	public partial class SmartEdge_Inspector
	{
		SerializedProperty 	mSerialized_TextWrapping, mSerialized_TextWrapping_Enabled;

		SerializedProperty  mSerialized_TextWrapping_VerticalWrapMode, mSerialized_TextWrapping_HorizontalWrapMode,
							mSerialized_TextWrapping_CurrentPage, mSerialized_TextWrapping_Pages,
							mSerialized_TextWrapping_HorizontalAlignment, mSerialized_TextWrapping_VerticalAlignment,
							mSerialized_TextWrapping_JustifiedWordCharacterRatio, mSerialized_TextWrapping_JustifiedMinWordSpace,
                            mSerialized_TextWrapping_MaxVisibleCharacters, mSerialized_TextWrapping_MaxVisibleLines, mSerialized_TextWrapping_MaxVisibleWords,
                            mSerialized_TextWrapping_CaseType;


		void RegisterProperty_Text_TextWrapping()
		{
			mSerialized_TextWrapping            = mSerialized_Text.FindPropertyRelative("_TextWrapping");

			mSerialized_TextWrapping_Enabled             = mSerialized_TextWrapping.FindPropertyRelative("_Enabled");
			mSerialized_TextWrapping_VerticalWrapMode    = mSerialized_TextWrapping.FindPropertyRelative("_VerticalWrapMode");
			mSerialized_TextWrapping_HorizontalWrapMode  = mSerialized_TextWrapping.FindPropertyRelative("_HorizontalWrapMode");

			mSerialized_TextWrapping_CurrentPage         = mSerialized_TextWrapping.FindPropertyRelative("_CurrentPage");
			mSerialized_TextWrapping_Pages               = mSerialized_TextWrapping.FindPropertyRelative("_Pages");
			mSerialized_TextWrapping_HorizontalAlignment = mSerialized_TextWrapping.FindPropertyRelative("_HorizontalAlignment");
			mSerialized_TextWrapping_VerticalAlignment   = mSerialized_TextWrapping.FindPropertyRelative("_VerticalAlignment");

			mSerialized_TextWrapping_JustifiedWordCharacterRatio = mSerialized_TextWrapping.FindPropertyRelative("_JustifiedWordCharacterRatio");
			mSerialized_TextWrapping_JustifiedMinWordSpace       = mSerialized_TextWrapping.FindPropertyRelative("_JustifiedMinWordSpace");

            mSerialized_TextWrapping_MaxVisibleCharacters = mSerialized_TextWrapping.FindPropertyRelative("_MaxVisibleCharacters");
            mSerialized_TextWrapping_MaxVisibleLines      = mSerialized_TextWrapping.FindPropertyRelative("_MaxVisibleLines");
            mSerialized_TextWrapping_MaxVisibleWords      = mSerialized_TextWrapping.FindPropertyRelative("_MaxVisibleWords");

            mSerialized_TextWrapping_CaseType             = mSerialized_TextWrapping.FindPropertyRelative("_CaseType");

        }

		void OnGUI_TextWrapping ()
		{
			if (!GUITools.DrawHeader ("Modifiers", "SE TextWrapping", true, mSerialized_TextWrapping_Enabled.boolValue, EnableTextWrapping, HelpURL: SE_InspectorTools.HelpURL_Text_TextWrapping, disabledColor:GUITools.LightGray))
				return;

			EditorGUI.BeginChangeCheck();

			GUITools.BeginContents();
			   OnGUI_TextWrapping_Content ();
			GUITools.EndContents ();

			if (EditorGUI.EndChangeCheck())
			{
				if (!mSerialized_TextWrapping_Enabled.boolValue)
					EnableTextWrapping(true);
                ApplyOriginalTextToLabel();
				mMakeVerticesDirty = true;
			}
		}

		void EnableTextWrapping( bool enable )
		{
			mSerialized_TextWrapping_Enabled.boolValue = enable;
			mMakeVerticesDirty = true;
            ApplyOriginalTextToLabel();
		}

		void OnGUI_TextWrapping_Content()
		{
			EditorGUIUtility.labelWidth = 80;
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginVertical();

				OnGUI_TextWrapping_Properties();
                GUILayout.Space(5);
                OnGUI_TextWrapping_Region();
                GUILayout.Space(10);
                OnGUI_TextWrapping_Modifier();



            var uiText = (mTarget.mGraphic as Text);
				if (uiText != null && (uiText.verticalOverflow != VerticalWrapMode.Overflow || uiText.horizontalOverflow != HorizontalWrapMode. Overflow))
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.HelpBox("Overflow is set to Truncate which could hide some characters.", MessageType.Warning);
					var rect = GUILayoutUtility.GetLastRect();
					if (GUI.Button(rect, "", EditorStyles.label))
						Application.OpenURL(SE_InspectorTools.HelpURL_Text_TextWrapping);

					if (GUILayout.Button("Fix", GUILayout.ExpandHeight(true)))
					{
						uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
						uiText.verticalOverflow = VerticalWrapMode.Overflow;
						EditorUtility.SetDirty(uiText);
						GUITools.Editor_MarkSceneDirty();
					}
					GUILayout.EndHorizontal();
				}

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		void OnGUI_TextWrapping_Properties()
		{
			GUILayout.Label("Overflow", EditorStyles.boldLabel);
			GUI.enabled = true;

            GUILayout.BeginHorizontal();
			    EditorGUILayout.PropertyField(mSerialized_TextWrapping_HorizontalWrapMode, new GUIContent("Horizontal"), GUILayout.Width(Screen.width - 190));
                GUILayout.Space(20);

                OnGUI_TextWrapping_Toggle(mSerialized_TextWrapping_HorizontalAlignment, EditorGUIUtility.IconContent("align_horizontally_left", "Align Left"), (int)SmartEdge.eHorizontalAlignment.Left, EditorStyles.miniButtonLeft);
                OnGUI_TextWrapping_Toggle(mSerialized_TextWrapping_HorizontalAlignment, EditorGUIUtility.IconContent("align_horizontally_center", "Align Center"), (int)SmartEdge.eHorizontalAlignment.Center, EditorStyles.miniButtonMid);
                OnGUI_TextWrapping_Toggle(mSerialized_TextWrapping_HorizontalAlignment, EditorGUIUtility.IconContent("align_horizontally_right", "Align Right"), (int)SmartEdge.eHorizontalAlignment.Right, EditorStyles.miniButtonMid);
                OnGUI_TextWrapping_Toggle(mSerialized_TextWrapping_HorizontalAlignment, EditorGUIUtility.IconContent("align_horizontally_left", "Justify"), (int)SmartEdge.eHorizontalAlignment.Justified, EditorStyles.miniButtonRight);

                // There is no JUSTIFIED icon, so fake it by using the _left and then drawing the _right on top
                var rect = GUILayoutUtility.GetLastRect();
                rect.x += 5; rect.y += 1;
                GUI.Label(rect, EditorGUIUtility.IconContent("align_horizontally_right"));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(mSerialized_TextWrapping_VerticalWrapMode, new GUIContent("Vertical"), GUILayout.Width(Screen.width-190));
                GUILayout.Space(20);

                OnGUI_TextWrapping_Toggle(mSerialized_TextWrapping_VerticalAlignment, EditorGUIUtility.IconContent("align_vertically_top", "Align To Top"), (int)SmartEdge.eVerticalAlignment.Top, EditorStyles.miniButtonLeft);
                OnGUI_TextWrapping_Toggle(mSerialized_TextWrapping_VerticalAlignment, EditorGUIUtility.IconContent("align_vertically_center", "Align To Middle"), (int)SmartEdge.eVerticalAlignment.Center, EditorStyles.miniButtonMid);
                OnGUI_TextWrapping_Toggle(mSerialized_TextWrapping_VerticalAlignment, EditorGUIUtility.IconContent("align_vertically_bottom", "Align To Bottom"), (int)SmartEdge.eVerticalAlignment.Bottom, EditorStyles.miniButtonRight);

            GUILayout.EndHorizontal();
            GUI.enabled = true;

			if (mSerialized_TextWrapping_VerticalWrapMode.enumValueIndex == (int)SE_TextEffect_TextWrapping.eVerticalWrapMode.Page)
			{
				EditorGUILayout.PropertyField(mSerialized_TextWrapping_CurrentPage, new GUIContent("Page"));

                if (mSerialized_BestFit_Enabled.boolValue)
                {
                    EditorGUILayout.PropertyField(mSerialized_TextWrapping_Pages, new GUIContent("Num Pages"));
                    if (mSerialized_TextWrapping_Pages.intValue < 1)
                        mSerialized_TextWrapping_Pages.intValue = 1;
                }
			}
			if (mSerialized_TextWrapping_HorizontalAlignment.enumValueIndex == (int)SmartEdge.eHorizontalAlignment.Justified)
			{
				var w = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 120;
				EditorGUILayout.PropertyField(mSerialized_TextWrapping_JustifiedMinWordSpace, new GUIContent("Justify Min Space", "Only expand the characters if the space required for the justification is bigger than this"));
				EditorGUILayout.PropertyField(mSerialized_TextWrapping_JustifiedWordCharacterRatio, new GUIContent("Justify Characters", "When 0, only the Words are moved, as it grows to 1, the characters start to be separated as well"));
				EditorGUIUtility.labelWidth = w;
			}
		}

        void OnGUI_TextWrapping_Region()
        {
			//GUILayout.Label("Region", EditorStyles.boldLabel);

            var lwidth = EditorGUIUtility.labelWidth;

            GUILayout.BeginHorizontal();
                Color transpWhite = new Color(1, 1, 1, 0.5f);

                GUI.color = mSerialized_TextWrapping_MaxVisibleCharacters.intValue >= 0 ? GUITools.White : transpWhite;
                EditorGUIUtility.labelWidth = 40;
                EditorGUILayout.PropertyField(mSerialized_TextWrapping_MaxVisibleCharacters, new GUIContent("Chars"));
                if (mSerialized_TextWrapping_MaxVisibleCharacters.intValue < -1) mSerialized_TextWrapping_MaxVisibleCharacters.intValue = -1;
                GUI.color = GUITools.White;


                EditorGUIUtility.labelWidth = 45;
                GUI.color = mSerialized_TextWrapping_MaxVisibleWords.intValue >= 0 ? GUITools.White : transpWhite;
                EditorGUILayout.PropertyField(mSerialized_TextWrapping_MaxVisibleWords, new GUIContent("Words"));
                if (mSerialized_TextWrapping_MaxVisibleWords.intValue < -1) mSerialized_TextWrapping_MaxVisibleWords.intValue = -1;
                GUI.color = GUITools.White;

                EditorGUIUtility.labelWidth = 40;
                GUI.color = mSerialized_TextWrapping_MaxVisibleLines.intValue >= 0 ? GUITools.White : transpWhite;
                EditorGUILayout.PropertyField(mSerialized_TextWrapping_MaxVisibleLines, new GUIContent("Lines"));
                if (mSerialized_TextWrapping_MaxVisibleLines.intValue < -1) mSerialized_TextWrapping_MaxVisibleLines.intValue = -1;
                GUI.color = GUITools.White;
                EditorGUIUtility.labelWidth = lwidth;

            GUILayout.EndHorizontal();
        }

        void OnGUI_TextWrapping_Modifier()
        {
            GUILayout.Label("Modifier", EditorStyles.boldLabel);

            var lwidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.PropertyField(mSerialized_TextWrapping_CaseType, new GUIContent("Convert Case"));
            EditorGUIUtility.labelWidth = lwidth;
        }


        void OnGUI_TextWrapping_Toggle( SerializedProperty prop, GUIContent content, int index, GUIStyle style )
		{
			int current = prop.enumValueIndex;
			if (GUILayout.Toggle(index == current, content, style, GUITools.DontExpandWidth) && index != current)
				prop.enumValueIndex = index;                
		}

	}
}