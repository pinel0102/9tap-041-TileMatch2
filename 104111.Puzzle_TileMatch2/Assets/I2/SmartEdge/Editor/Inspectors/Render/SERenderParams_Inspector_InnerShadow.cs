using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace I2.SmartEdge
{
	public partial class SERenderParams_Inspector
    {
        SerializedProperty mSerialized_EnableInnerShadows, mSerialized_InnerShadowOffset, mSerialized_InnerShadowColor,
		                    mSerialized_InnerShadowEdgeOffset, mSerialized_InnerShadowMaskOffset, mSerialized_InnerShadowSmoothWidth, mSerialized_InnerShadowMaskSoftness;

        void RegisterProperty_InnerShadow()
	                    	{
			mSerialized_EnableInnerShadows = mSerialized_RenderParams.FindPropertyRelative ("_EnableInnerShadows");
			mSerialized_InnerShadowColor   = mSerialized_RenderParams.FindPropertyRelative ("_InnerShadowColor");
			mSerialized_InnerShadowOffset  = mSerialized_RenderParams.FindPropertyRelative ("_InnerShadowOffset");
			
			mSerialized_InnerShadowEdgeOffset   = mSerialized_RenderParams.FindPropertyRelative ("_InnerShadowEdgeOffset");
			mSerialized_InnerShadowMaskOffset   = mSerialized_RenderParams.FindPropertyRelative ("_InnerShadowMaskOffset");
			mSerialized_InnerShadowSmoothWidth  = mSerialized_RenderParams.FindPropertyRelative ("_InnerShadowSmoothWidth");
			mSerialized_InnerShadowMaskSoftness = mSerialized_RenderParams.FindPropertyRelative ("_InnerShadowMaskSoftness");
		}

        void OnGUI_InnerShadow()
                           {
			if (!GUITools.DrawHeader ("Inner Shadow", true/*"SmartEdge InnerShadow"*/, true, mRenderParams.HasInnerShadow(), EnableInnerShadows, HelpURL: SE_InspectorTools.HelpURL_InnerShadow, disabledColor:GUITools.LightGray, allowCollapsing: false))
				return;
			EditorGUIUtility.labelWidth = 50;
            EditorGUI.BeginChangeCheck();

			GUITools.BeginContents();
                var c = GUI.color;
                if (!mRenderParams.HasInnerShadow()) GUI.color = new Color(1, 1, 1, 0.3f);

                GUILayout.Space (2);
				EditorGUILayout.PropertyField (mSerialized_InnerShadowColor, new GUIContent("Color"));

				GUILayout.Space (2);

				Vector2 offset = mSerialized_InnerShadowOffset.vector2Value;
				GUILayout.BeginHorizontal ();
					offset.x = EditorGUILayout.Slider ("Offset X", offset.x, -1, 1);
					offset.y = EditorGUILayout.Slider ("Offset Y", offset.y, -1, 1);
				GUILayout.EndHorizontal ();
				mSerialized_InnerShadowOffset.vector2Value = offset;

				GUILayout.Space (5);
				EditorGUILayout.PropertyField (mSerialized_InnerShadowEdgeOffset, new GUIContent("Width"));
				EditorGUILayout.PropertyField (mSerialized_InnerShadowMaskOffset, new GUIContent("Mask"));

				GUILayout.Space (5);
				EditorGUILayout.PropertyField (mSerialized_InnerShadowSmoothWidth, new GUIContent("Smooth"));
				EditorGUILayout.PropertyField (mSerialized_InnerShadowMaskSoftness, new GUIContent("Outer"));


            GUI.color = c;
			GUITools.EndContents ();

			if (EditorGUI.EndChangeCheck()) 
			{
                if (!mSerialized_EnableInnerShadows.boolValue)
                    EnableInnerShadows(true);
				mMakeMaterialDirty = true;
				mSerialized_EnableInnerShadows.boolValue = true;
			}	
		}

        void EnableInnerShadows(bool Enable)
	                    	{
			mSerialized_EnableInnerShadows.boolValue = Enable;
            mMakeMaterialDirty = true;
            SERenderParams_Inspector.mSelectedRenderEffect = SmartEdgeRenderEffect.InnerShadow;
        }
    }
}