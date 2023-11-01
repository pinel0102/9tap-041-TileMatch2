using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	public partial class SERenderParams_Inspector
    {
        SmartEdgeTextureInspector mNormalMapDef = new SmartEdgeTextureInspector();
		SerializedProperty 	mSerialized_NormalMapStrength_Face, mSerialized_NormalMapStrength_Outline, mSerialized_NormalMapDepth;

        void RegisterProperty_NormalMap()
		{
			mNormalMapDef.Set( mSerialized_RenderParams.FindPropertyRelative("_NormalMap"), mSerialized_RenderParams.FindPropertyRelative("_FaceTexture"));
			mSerialized_NormalMapStrength_Face 		= mSerialized_RenderParams.FindPropertyRelative ("_NormalMapStrength_Face");
            mSerialized_NormalMapStrength_Outline   = mSerialized_RenderParams.FindPropertyRelative("_NormalMapStrength_Outline");
            mSerialized_NormalMapDepth              = mSerialized_RenderParams.FindPropertyRelative("_NormalMapDepth");           
        }

        void OnGUI_NormalMap()
        {
			EditorGUIUtility.labelWidth = 60;
			
			if (!GUITools.DrawHeader ("Normal Map", true/*"SmartEdge NormalMap"*/, true, mNormalMapDef.mSerialized_Enable.boolValue, EnableNormalMap, HelpURL: SE_InspectorTools.HelpURL_NormalMap, disabledColor:GUITools.LightGray, allowCollapsing: false))
				return;

            EditorGUI.BeginChangeCheck();
			GUITools.BeginContents();
                var c = GUI.color;
                if (!mNormalMapDef.mSerialized_Enable.boolValue) GUI.color = new Color(1, 1, 1, 0.3f);

                GUILayout.Space (2);
				GUILayout.Label ("Strength:", EditorStyles.boldLabel);

				GUILayout.BeginHorizontal ();
					EditorGUILayout.PropertyField (mSerialized_NormalMapStrength_Face, new GUIContent("Face"));
					EditorGUILayout.PropertyField (mSerialized_NormalMapStrength_Outline, new GUIContent("Outline"));
				GUILayout.EndHorizontal ();
                EditorGUILayout.PropertyField(mSerialized_NormalMapDepth, new GUIContent("Depth"));

            mNormalMapDef.OnGUI_Texture( false, true, ref mMakeVerticesDirty, ref mMakeMaterialDirty);
            GUI.color = c;
			GUITools.EndContents ();

            if (EditorGUI.EndChangeCheck())
            {
                mMakeMaterialDirty = true;
            }
		}


        void EnableNormalMap( bool enable )
		{
			mNormalMapDef.mSerialized_Enable.boolValue = enable;
            mMakeMaterialDirty = true;
            SERenderParams_Inspector.mSelectedRenderEffect = SmartEdgeRenderEffect.NormalMap;
        }
    }
}