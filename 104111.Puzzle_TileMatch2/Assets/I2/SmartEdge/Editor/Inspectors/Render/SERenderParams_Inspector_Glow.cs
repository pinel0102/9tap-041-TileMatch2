using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	public partial class SERenderParams_Inspector
    {
        SerializedProperty mSerialized_EnableGlow, mSerialized_GlowColor, mSerialized_GlowIntensity,
							mSerialized_GlowOuterWidth, mSerialized_GlowInnerWidth, mSerialized_GlowPower, 
							mSerialized_GlowEdgeWidth,  mSerialized_GlowOffset, mSerialized_GlowPosition, mSerialized_GlowLayer,
							mSerialized_EnableGlowGradient, mSerialized_GlowGradient;

        SmartEdgeTextureInspector mGlowTextureDef = new SmartEdgeTextureInspector();


        void RegisterProperty_Glow()
		{
			mSerialized_EnableGlow 		    = mSerialized_RenderParams.FindPropertyRelative ("_EnableGlow");
			mSerialized_GlowColor 		    = mSerialized_RenderParams.FindPropertyRelative ("_GlowColor");
			mSerialized_GlowOuterWidth 	    = mSerialized_RenderParams.FindPropertyRelative ("_GlowOuterWidth");
			mSerialized_GlowInnerWidth 	    = mSerialized_RenderParams.FindPropertyRelative ("_GlowInnerWidth");
			mSerialized_GlowPower 		    = mSerialized_RenderParams.FindPropertyRelative ("_GlowPower");
			mSerialized_GlowEdgeWidth 	    = mSerialized_RenderParams.FindPropertyRelative ("_GlowEdgeWidth");
			mSerialized_GlowOffset 		    = mSerialized_RenderParams.FindPropertyRelative ("_GlowOffset");
			mSerialized_GlowPosition 	    = mSerialized_RenderParams.FindPropertyRelative ("_GlowPosition");
			mSerialized_GlowIntensity       = mSerialized_RenderParams.FindPropertyRelative ("_GlowIntensity");
			mSerialized_GlowLayer		    = mSerialized_RenderParams.FindPropertyRelative ("_GlowLayer");
			mSerialized_EnableGlowGradient 	= mSerialized_RenderParams.FindPropertyRelative ("_EnableGlowGradient");
			mSerialized_GlowGradient 		= mSerialized_RenderParams.FindPropertyRelative ("_GlowGradient");

			mGlowTextureDef.Set( mSerialized_RenderParams.FindPropertyRelative("_GlowTexture") );
		}

        void OnGUI_Glow()
        {
			if (!GUITools.DrawHeader ("Glow", true/*"SmartEdge Glow"*/, true, mSerialized_EnableGlow.boolValue, EnableGlow, HelpURL: SE_InspectorTools.HelpURL_Glow, disabledColor:GUITools.LightGray, allowCollapsing: false))
				return;
			EditorGUIUtility.labelWidth = 60;

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();
            var c = GUI.color;
            if (!mSerialized_EnableGlow.boolValue) GUI.color = new Color(1, 1, 1, 0.3f);

            GUITools.BeginContents();
				GUILayout.Space (2);
				EditorGUILayout.PropertyField (mSerialized_GlowColor, new GUIContent("Color"));
				EditorGUILayout.PropertyField (mSerialized_GlowIntensity, new GUIContent("Intensity"));
				GUILayout.Space (2);

				GUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField (mSerialized_GlowOffset, new GUIContent("Offset"));

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField (mSerialized_GlowLayer, new GUIContent(""), GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        mMakeVerticesDirty = mMakeMaterialDirty = true;

                        // if layer is SIMPLE, then use the Face UV, otherwise use the GlowUV
                        if (mSerialized_GlowLayer.enumValueIndex == (int)SmartEdgeRenderParams.eGlowLayer.Simple)
                            mGlowTextureDef.Set(mSerialized_RenderParams.FindPropertyRelative("_GlowTexture"), mSerialized_RenderParams.FindPropertyRelative("_FaceTexture")); 
                        else
                            mGlowTextureDef.Set(mSerialized_RenderParams.FindPropertyRelative("_GlowTexture"));
                    }
				GUILayout.EndHorizontal();

                GUI.enabled = mSerialized_GlowLayer.enumValueIndex != (int)SmartEdgeRenderParams.eGlowLayer.Simple;
				EditorGUILayout.PropertyField (mSerialized_GlowPosition, new GUIContent("Position"));
                GUI.enabled = true;

				GUILayout.Space (5);
				EditorGUILayout.PropertyField (mSerialized_GlowPower, new GUIContent("Power"));

				GUILayout.Space (8);
				GUITools.BeginContents ();
					GUILayout.Label ("Width:", EditorStyles.boldLabel);
					GUILayout.BeginHorizontal ();
						GUILayout.Space (5);

						GUILayout.BeginVertical ();
						EditorGUILayout.PropertyField (mSerialized_GlowOuterWidth, new GUIContent("Outer"));
						EditorGUILayout.PropertyField (mSerialized_GlowEdgeWidth, new GUIContent("Edge"));
						EditorGUILayout.PropertyField (mSerialized_GlowInnerWidth, new GUIContent("Inner"));
						GUILayout.EndVertical ();
					GUILayout.EndHorizontal ();
				GUITools.EndContents (false);

			GUILayout.Space (10);
			if (EditorGUI.EndChangeCheck())
			{
                if (mSerialized_GlowLayer.enumValueIndex == (int)SmartEdgeRenderParams.eGlowLayer.Simple)
					mMakeMaterialDirty = true;
				else
					mMakeVerticesDirty = true;
			}
			
			mGlowTextureDef.OnGUI_Texture( false, true, ref mMakeVerticesDirty, ref mMakeMaterialDirty);

			GUILayout.Space (10);
			mMakeVerticesDirty |= OnGUI_Gradient( mSerialized_EnableGlowGradient, mSerialized_GlowGradient, mRenderParams._GlowGradient);
            GUI.color = c;
			GUITools.EndContents ();

            if (EditorGUI.EndChangeCheck() && !mSerialized_EnableGlow.boolValue)
                EnableGlow(true);
        }

        void EnableGlow( bool enable )
		{
			mSerialized_EnableGlow.boolValue = enable;
            mMakeVerticesDirty = true;
            if (mGlowTextureDef.IsUsed() || mSerialized_GlowLayer.enumValueIndex == (int)SmartEdgeRenderParams.eGlowLayer.Simple)
                mMakeMaterialDirty = true;
            SERenderParams_Inspector.mSelectedRenderEffect = SmartEdgeRenderEffect.Glow;
        }
    }
}