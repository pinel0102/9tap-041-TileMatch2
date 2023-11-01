using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	public partial class SERenderParams_Inspector
    {
        SerializedProperty  mSerialized_EnableShadow, mSerialized_SimpleShadow, mSerialized_ShadowOffset, mSerialized_ShadowColor, mSerialized_ShadowEdgeOffset,
                            mSerialized_ShadowSmoothWidth, mSerialized_ShadowSmoothPower,
                            mSerialized_ShadowInnerSmoothWidth, mSerialized_ShadowHollow, mSerialized_ShadowEdgeWidth,
                            mSerialized_ShadowHeight, mSerialized_ShadowBorderLeft, mSerialized_ShadowBorderRight,
                            mSerialized_ShadowSmoothTop, mSerialized_ShadowSmoothBottom, mSerialized_ShadowSubdivisionLevel,
                            mSerialized_ShadowOpacityTop, mSerialized_ShadowOpacityBottom;

        void RegisterProperty_Shadow()
		{
			mSerialized_ShadowOffset = mSerialized_RenderParams.FindPropertyRelative ("_ShadowOffset");
			mSerialized_ShadowColor  = mSerialized_RenderParams.FindPropertyRelative ("_ShadowColor");
			
			mSerialized_ShadowEdgeOffset  = mSerialized_RenderParams.FindPropertyRelative ("_ShadowEdgeOffset");
			mSerialized_ShadowSmoothWidth = mSerialized_RenderParams.FindPropertyRelative ("_ShadowSmoothWidth");
			mSerialized_ShadowSmoothPower = mSerialized_RenderParams.FindPropertyRelative ("_ShadowSmoothPower");

			mSerialized_ShadowInnerSmoothWidth = mSerialized_RenderParams.FindPropertyRelative ("_ShadowInnerSmoothWidth");
			mSerialized_ShadowEdgeWidth        = mSerialized_RenderParams.FindPropertyRelative ("_ShadowEdgeWidth");
			mSerialized_ShadowHollow           = mSerialized_RenderParams.FindPropertyRelative ("_ShadowHollow");

			mSerialized_ShadowHeight      = mSerialized_RenderParams.FindPropertyRelative ("_ShadowHeight");
			mSerialized_ShadowBorderLeft  = mSerialized_RenderParams.FindPropertyRelative ("_ShadowBorderLeft");
			mSerialized_ShadowBorderRight = mSerialized_RenderParams.FindPropertyRelative ("_ShadowBorderRight");
			
			mSerialized_ShadowSmoothTop     = mSerialized_RenderParams.FindPropertyRelative ("_ShadowSmoothTop");
			mSerialized_ShadowSmoothBottom  = mSerialized_RenderParams.FindPropertyRelative ("_ShadowSmoothBottom");
			mSerialized_ShadowOpacityTop    = mSerialized_RenderParams.FindPropertyRelative ("_ShadowOpacityTop");
			mSerialized_ShadowOpacityBottom = mSerialized_RenderParams.FindPropertyRelative ("_ShadowOpacityBottom");

			mSerialized_ShadowSubdivisionLevel = mSerialized_RenderParams.FindPropertyRelative ("_ShadowSubdivisionLevel");
			mSerialized_EnableShadow           = mSerialized_RenderParams.FindPropertyRelative ("_EnableShadows");
			mSerialized_SimpleShadow           = mSerialized_RenderParams.FindPropertyRelative ("_SimpleShadows");
		}
		
		void OnGUI_Shadow()
        {
			if (!GUITools.DrawHeader ("Shadow", true/*"SmartEdge Shadow"*/, true, mRenderParams.HasShadow(), EnableShadows, HelpURL: SE_InspectorTools.HelpURL_Shadow, disabledColor:GUITools.LightGray, allowCollapsing: false))
				return;
			EditorGUIUtility.labelWidth = 50;

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();
            			
			GUITools.BeginContents();
                var c = GUI.color;
                if (!mRenderParams.HasShadow()) GUI.color = new Color(1, 1, 1, 0.3f);

                GUILayout.Space (2);
				GUILayout.BeginHorizontal ();
					EditorGUILayout.PropertyField (mSerialized_ShadowColor, new GUIContent("Color"));

                    EditorGUI.BeginChangeCheck();
                        mSerialized_SimpleShadow.boolValue = (EditorGUILayout.Popup (mSerialized_SimpleShadow.boolValue ? 0 : 1, new string[]{ "Simple", "Back" }, GUILayout.ExpandWidth (false))==0);
                    if (EditorGUI.EndChangeCheck())
                        mMakeVerticesDirty = mMakeMaterialDirty = true;
                GUILayout.EndHorizontal ();

				GUILayout.Space (2);

				if (mSerialized_SimpleShadow.boolValue)
				{
					Vector3 offset = mSerialized_ShadowOffset.vector3Value;
					GUILayout.BeginHorizontal ();
						offset.x = EditorGUILayout.Slider ("Offset X", offset.x, -1, 1);
						offset.y = EditorGUILayout.Slider ("Offset Y", offset.y, -1, 1);
					GUILayout.EndHorizontal ();
					mSerialized_ShadowOffset.vector3Value = offset;
				}
				else
					EditorGUILayout.PropertyField (mSerialized_ShadowOffset, new GUIContent("Offset"));

				GUILayout.Space (5);
				EditorGUILayout.PropertyField (mSerialized_ShadowEdgeOffset, new GUIContent("Expand"));

            if (EditorGUI.EndChangeCheck())
            {
                if (mSerialized_SimpleShadow.boolValue)
                    mMakeMaterialDirty = true;
                mMakeVerticesDirty = true;
            }

                GUI.enabled = !mSerialized_SimpleShadow.boolValue;
				GUILayout.Space (5);
				OnGUI_Shadow_Deformation ();

				OnGUI_Shadow_SubdivisionLevel ();
				GUI.enabled = true;
                GUI.color = c;
			GUITools.EndContents ();

            if (EditorGUI.EndChangeCheck() && !mSerialized_EnableShadow.boolValue)
                EnableShadows(true);
        }

		void EnableShadows (bool Enable)
		{
			mSerialized_EnableShadow.boolValue = Enable;
            if (mSerialized_SimpleShadow.boolValue)
                mMakeMaterialDirty = true;
            mMakeVerticesDirty = true;
            SERenderParams_Inspector.mSelectedRenderEffect = SmartEdgeRenderEffect.Shadow;
        }

        void OnGUI_Shadow_Deformation()
		{
            EditorGUI.BeginChangeCheck();

			GUI.backgroundColor = GUITools.LightGray;
			GUITools.BeginContents ();
			GUI.backgroundColor = Color.white;
			
			OnGUI_Shadow_DeformationPresets ();
			
			EditorGUILayout.PropertyField (mSerialized_ShadowHeight, new GUIContent("Height"));
			
			float xCenter = Mathf.Lerp(mSerialized_ShadowBorderLeft.floatValue, mSerialized_ShadowBorderRight.floatValue, 0.5f);
			float xWidth = -mSerialized_ShadowBorderLeft.floatValue + mSerialized_ShadowBorderRight.floatValue;
			GUILayout.BeginHorizontal ();
				xCenter = EditorGUILayout.Slider ("Center", xCenter, -1, 1);
				xWidth = EditorGUILayout.Slider ("Width", xWidth, -1, 1);
			GUILayout.EndHorizontal ();

            if (EditorGUI.EndChangeCheck())
            {
                mSerialized_ShadowBorderLeft.floatValue = xCenter - xWidth * 0.5f;
                mSerialized_ShadowBorderRight.floatValue = xCenter + xWidth * 0.5f;

                if (mSerialized_SimpleShadow.boolValue)
                    mMakeMaterialDirty = true;
                mMakeVerticesDirty = true;
            }
			
			GUILayout.Space (5);
			OnGUI_Shadow_Smoothing ();
			GUITools.EndContents (false);
		}

		void OnGUI_Shadow_DeformationPresets()
		{
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal (GUILayout.Width (1));
			int width = (Screen.width-50)/4-20;
			if (GUILayout.Button (SmartEdgeTools.SmartEdgeSkin.FindStyle ("Shadow Flat").normal.background, GUILayout.Height (40), GUILayout.Width (width))) 
			{
				mSerialized_ShadowOffset.vector3Value = new Vector3(0.03f, -0.03f,0);
				mSerialized_ShadowHeight.floatValue = 0;
				mSerialized_ShadowBorderLeft.floatValue = 0;
				mSerialized_ShadowBorderRight.floatValue = 0;
				mSerialized_ShadowSmoothTop.floatValue = 1;
				mSerialized_ShadowSmoothBottom.floatValue = 1;
				mSerialized_ShadowOpacityTop.floatValue = 1;
				mSerialized_ShadowOpacityBottom.floatValue = 1;
                GUI.changed = true;                
			}
			if (GUILayout.Button (SmartEdgeTools.SmartEdgeSkin.FindStyle ("Shadow Front").normal.background, GUILayout.Height(40), GUILayout.Width(width)))
			{
				mSerialized_ShadowOffset.vector3Value = Vector3.zero;
				mSerialized_ShadowHeight.floatValue = -0.5f;
				mSerialized_ShadowBorderLeft.floatValue = -0.1f;
				mSerialized_ShadowBorderRight.floatValue = 0.1f;
				mSerialized_ShadowSmoothTop.floatValue = 1;
				mSerialized_ShadowSmoothBottom.floatValue = 0;
				mSerialized_ShadowOpacityTop.floatValue = 0.4f;
				mSerialized_ShadowOpacityBottom.floatValue = 1;
                GUI.changed = true;
            }
            if (GUILayout.Button (SmartEdgeTools.SmartEdgeSkin.FindStyle ("Shadow Side").normal.background, GUILayout.Height(40), GUILayout.Width(width)))
			{
				mSerialized_ShadowOffset.vector3Value = Vector3.zero;
				mSerialized_ShadowHeight.floatValue = -0.5f;
				mSerialized_ShadowBorderLeft.floatValue = 0.2f;
				mSerialized_ShadowBorderRight.floatValue = 0.2f;
				mSerialized_ShadowSmoothTop.floatValue = 1;
				mSerialized_ShadowSmoothBottom.floatValue = 0;
				mSerialized_ShadowOpacityTop.floatValue = 0.4f;
				mSerialized_ShadowOpacityBottom.floatValue = 1;
                GUI.changed = true;
            }
            if (GUILayout.Button (SmartEdgeTools.SmartEdgeSkin.FindStyle ("Shadow Back").normal.background, GUILayout.Height(40), GUILayout.Width(width)))
			{
				mSerialized_ShadowOffset.vector3Value = Vector3.zero;
				mSerialized_ShadowHeight.floatValue = -1.5f;
				mSerialized_ShadowBorderLeft.floatValue = -0.1f;
				mSerialized_ShadowBorderRight.floatValue = -0.1f;
				mSerialized_ShadowSmoothTop.floatValue = 1;
				mSerialized_ShadowSmoothBottom.floatValue = 0;
				mSerialized_ShadowOpacityTop.floatValue = 0.4f;
				mSerialized_ShadowOpacityBottom.floatValue = 1;
                GUI.changed = true;
            }
            GUILayout.EndHorizontal ();

            if (EditorGUI.EndChangeCheck())
            {
                if (mSerialized_SimpleShadow.boolValue)
                    mMakeMaterialDirty = true;
                mMakeVerticesDirty = true;
            }
        }

		void OnGUI_Shadow_Smoothing()
		{
            bool isGeometryShadow = !mSerialized_SimpleShadow.boolValue;

            EditorGUI.BeginChangeCheck();
             
			EditorGUIUtility.labelWidth = 50;
			GUITools.BeginContents ();

			GUI.enabled = true;
			EditorGUILayout.PropertyField (mSerialized_ShadowSmoothWidth, new GUIContent("Smooth"));
			GUI.enabled = isGeometryShadow;

			GUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField (mSerialized_ShadowHollow, new GUIContent(""), GUILayout.Width(13));
				GUILayout.Label ("Inner:", EditorStyles.boldLabel, GUITools.DontExpandWidth);
				GUILayout.BeginVertical ();
					GUI.enabled = mSerialized_ShadowHollow.boolValue && isGeometryShadow;
					EditorGUIUtility.labelWidth = 70;
					EditorGUILayout.PropertyField (mSerialized_ShadowEdgeWidth, new GUIContent("Edge"));
					EditorGUILayout.PropertyField (mSerialized_ShadowInnerSmoothWidth, new GUIContent("Smooth"));
					EditorGUIUtility.labelWidth = 50;
					GUI.enabled = isGeometryShadow;
				GUILayout.EndVertical ();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal ();
				GUILayout.BeginVertical (GUILayout.Width(80));
					GUILayout.Space (10);
					GUILayout.Label("Blur:", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
				GUILayout.EndVertical ();
				GUILayout.BeginVertical ();
					EditorGUILayout.Slider (mSerialized_ShadowSmoothTop, 0, 1, "Top");
					EditorGUILayout.Slider (mSerialized_ShadowSmoothBottom, 0, 1, "Bottom");
				GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
				GUILayout.BeginVertical (GUILayout.Width(80));
					GUILayout.Space (10);
					GUILayout.Label("Opacity:", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
				GUILayout.EndVertical ();
				GUILayout.BeginVertical ();
					EditorGUILayout.Slider (mSerialized_ShadowOpacityTop, 0, 1, "Top");
					EditorGUILayout.Slider (mSerialized_ShadowOpacityBottom, 0, 1, "Bottom");
				GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();


			EditorGUILayout.Separator ();
			GUI.enabled = isGeometryShadow;
			EditorGUILayout.PropertyField (mSerialized_ShadowSmoothPower, new GUIContent("Power"));
			GUI.enabled = isGeometryShadow;

			GUITools.EndContents (false);

            if (EditorGUI.EndChangeCheck())
            {
                if (mSerialized_SimpleShadow.boolValue)
                    mMakeMaterialDirty = true;
                mMakeVerticesDirty = true;
            }
        }

		void OnGUI_Shadow_SubdivisionLevel()
		{
            EditorGUI.BeginChangeCheck();

			GUILayout.BeginHorizontal ();
				GUILayout.Label ("Subdivisions");
				if (GUILayout.Toggle (mSerialized_ShadowSubdivisionLevel.intValue == 0, "None", EditorStyles.toolbarButton) && GUI.changed)   mSerialized_ShadowSubdivisionLevel.intValue = 0;
				if (GUILayout.Toggle (mSerialized_ShadowSubdivisionLevel.intValue == 1, "1x",   EditorStyles.toolbarButton) && GUI.changed)   mSerialized_ShadowSubdivisionLevel.intValue = 1;
				if (GUILayout.Toggle (mSerialized_ShadowSubdivisionLevel.intValue == 2, "2x",   EditorStyles.toolbarButton) && GUI.changed)   mSerialized_ShadowSubdivisionLevel.intValue = 2;
				if (GUILayout.Toggle (mSerialized_ShadowSubdivisionLevel.intValue == 3, "3x",   EditorStyles.toolbarButton) && GUI.changed)   mSerialized_ShadowSubdivisionLevel.intValue = 3;
			GUILayout.EndHorizontal ();

            if (EditorGUI.EndChangeCheck())
            {
                mSerialized_EnableShadow.boolValue = true;
                mMakeVerticesDirty = true;
            }
		}
	}
}