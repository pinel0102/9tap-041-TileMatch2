using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
    public partial class SERenderParams_Inspector
    {
        SerializedProperty mSerialized_OutlineSoftness, mSerialized_OutlineColor, mSerialized_OutlineWidth, mSerialized_OutlinePosition, mSerialized_EnableOutline,
							mSerialized_EnableOutlineGradient, mSerialized_OutlineGradient, mSerialized_UseOutlineLayer;

        SmartEdgeTextureInspector mOutlineTextureDef = new SmartEdgeTextureInspector();

        void RegisterProperty_Outline()
		{
			mSerialized_OutlineSoftness = mSerialized_RenderParams.FindPropertyRelative ("_OutlineSoftness");
			mSerialized_OutlineColor    = mSerialized_RenderParams.FindPropertyRelative ("_OutlineColor");
			mSerialized_OutlineWidth    = mSerialized_RenderParams.FindPropertyRelative ("_OutlineWidth");
			mSerialized_OutlinePosition = mSerialized_RenderParams.FindPropertyRelative ("_OutlinePosition");
			mSerialized_EnableOutline   = mSerialized_RenderParams.FindPropertyRelative ("_EnableOutline");
            mSerialized_UseOutlineLayer = mSerialized_RenderParams.FindPropertyRelative ("_UseOutlineLayer");


            mOutlineTextureDef.Set( mSerialized_RenderParams.FindPropertyRelative("_OutlineTexture") );

            mSerialized_EnableOutlineGradient = mSerialized_RenderParams.FindPropertyRelative ("_EnableOutlineGradient");
            mSerialized_OutlineGradient       = mSerialized_RenderParams.FindPropertyRelative("_OutlineGradient");
        }

        void OnGUI_Outline()
        {
			if (!GUITools.DrawHeader ("Outline", true/*"SmartEdge Outline"*/, true, mSerialized_EnableOutline.boolValue, EnableOutline, HelpURL: SE_InspectorTools.HelpURL_Outline, disabledColor:GUITools.LightGray, allowCollapsing: false))
				return;
            EditorGUI.BeginChangeCheck();

            EditorGUIUtility.labelWidth = 50;

			GUITools.BeginContents();
                var c = GUI.color;
                if (!mSerialized_EnableOutline.boolValue) GUI.color = new Color(1, 1, 1, 0.3f);

                EditorGUI.BeginChangeCheck();

                    GUILayout.Space (2);
				    GUILayout.BeginHorizontal ();
						EditorGUILayout.PropertyField (mSerialized_OutlinePosition, new GUIContent("")/*, GUILayout.Width(100)*/);
					    GUILayout.Space (5);
                        GUILayout.Label("Layer:", GUITools.DontExpandWidth);
                        mSerialized_UseOutlineLayer.boolValue = EditorGUILayout.Popup(mSerialized_UseOutlineLayer.boolValue ? 1 : 0, new string[] { "Simple", "Back" })>0;
				    GUILayout.EndHorizontal ();
                    EditorGUILayout.PropertyField(mSerialized_OutlineWidth, new GUIContent("Width"));

                mMakeVerticesDirty |= EditorGUI.EndChangeCheck();

                GUILayout.Space (10);

                EditorGUIUtility.labelWidth = 70;
                EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField (mSerialized_OutlineColor, new GUIContent("Color"));
                mMakeVerticesDirty |= EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal ();
					    EditorGUILayout.PropertyField (mSerialized_OutlineSoftness, new GUIContent("Softness"));
					    if (GUILayout.Button(new GUIContent("X", "Set softness to 1 pixel"), EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
						    mSerialized_OutlineSoftness.floatValue = 0.05f;
				    GUILayout.EndHorizontal ();
                mMakeMaterialDirty |= EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
	    			GUILayout.Space (10);
		    		GUILayout.Space (10);

			    	mOutlineTextureDef.OnGUI_Texture( false, true, ref mMakeVerticesDirty, ref mMakeMaterialDirty );
				    GUILayout.Space (10);

				    mMakeVerticesDirty |= OnGUI_Gradient( mSerialized_EnableOutlineGradient, mSerialized_OutlineGradient, mRenderParams._OutlineGradient);
                mMakeVerticesDirty |= EditorGUI.EndChangeCheck();

            GUI.color = c;
            GUITools.EndContents ();
            if (EditorGUI.EndChangeCheck() && !mSerialized_EnableOutline.boolValue)
                EnableOutline(true);
		}

        void EnableOutline( bool enable )
		{
			mSerialized_EnableOutline.boolValue = enable;
            mMakeVerticesDirty = true;
            SERenderParams_Inspector.mSelectedRenderEffect = SmartEdgeRenderEffect.Outline;
        }
    }
}