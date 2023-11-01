using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	public partial class SERenderParams_Inspector
	{
		SerializedProperty mSerialized_EnableBevel,
		                   mSerialized_BevelOffset, mSerialized_BevelWidth, mSerialized_BevelInnerSoftness, mSerialized_BevelOuterSoftness, mSerialized_BevelDepth, 
                           mSerialized_BevelCurve, mSerialized_BevelClamp;

		Texture2D mBevelTexture;
		float mBevelLastUpdatedEdge = -100;

		void RegisterProperty_Bevel()
		{
			mSerialized_EnableBevel 	   = mSerialized_RenderParams.FindPropertyRelative ("_EnableBevel");

			mSerialized_BevelOffset        = mSerialized_RenderParams.FindPropertyRelative ("_BevelOffset");
			mSerialized_BevelWidth         = mSerialized_RenderParams.FindPropertyRelative ("_BevelWidth");
			mSerialized_BevelDepth         = mSerialized_RenderParams.FindPropertyRelative ("_BevelDepth");
			mSerialized_BevelCurve         = mSerialized_RenderParams.FindPropertyRelative ("_BevelCurve");
			mSerialized_BevelClamp         = mSerialized_RenderParams.FindPropertyRelative ("_BevelClamp");

			mSerialized_BevelInnerSoftness = mSerialized_RenderParams.FindPropertyRelative("_BevelInnerSoftness");
			mSerialized_BevelOuterSoftness = mSerialized_RenderParams.FindPropertyRelative("_BevelOuterSoftness");

			mBevelTexture = new Texture2D(256, 64);
			mBevelTexture.hideFlags = HideFlags.HideAndDontSave;
		}

		void OnGUI_Bevel()
        {
			if (!GUITools.DrawHeader ("Bevel", true/*"SmartEdge Bevel"*/, true, mSerialized_EnableBevel.boolValue, EnableBevel, HelpURL: SE_InspectorTools.HelpURL_Bevel, disabledColor:GUITools.LightGray, allowCollapsing: false))
				return;

            EditorGUI.BeginChangeCheck();

			EditorGUIUtility.labelWidth = 100;
			GUITools.BeginContents();
                var c = GUI.color;
                if (!mSerialized_EnableBevel.boolValue) GUI.color = new Color(1, 1, 1, 0.3f);

                GUILayout.Space (2);

				GUI.changed = false;
				Rect rect = GUILayoutUtility.GetRect ( 0, 100000, 64, 64);
				rect.width /= 2;
				EditorGUI.DrawPreviewTexture(rect, mBevelTexture);

				rect.x += rect.width*2;
				rect.width = -rect.width;
				EditorGUI.DrawPreviewTexture(rect, mBevelTexture);

				GUILayout.Space (2);

				GUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
					float minRange = mSerialized_BevelOffset.floatValue;
					float maxRange = minRange + mSerialized_BevelWidth.floatValue;
					EditorGUILayout.MinMaxSlider(new GUIContent("Range"), ref minRange, ref maxRange, 0, 1);
					if (GUILayout.Button("Edge", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
					{
						minRange = 0;
						maxRange = mSerialized_OutlineWidth.floatValue;
						if (mSerialized_OutlinePosition.enumValueIndex != (int)SmartEdgeRenderParams.eOutlinePosition.Center)
							maxRange *= 0.5f;
					}
					if (EditorGUI.EndChangeCheck())
					{
						mSerialized_BevelOffset.floatValue = minRange;
						mSerialized_BevelWidth.floatValue = maxRange - minRange;
					}
				GUILayout.EndHorizontal();


				GUILayout.Space(5);
				EditorGUILayout.PropertyField(mSerialized_BevelDepth, new GUIContent("Depth"));
				EditorGUILayout.PropertyField(mSerialized_BevelCurve, new GUIContent("Curve"));
				EditorGUILayout.PropertyField(mSerialized_BevelClamp, new GUIContent("Clamp"));
				GUILayout.Space (5);

				GUILayout.Label ("Softness:", EditorStyles.boldLabel);
				GUILayout.BeginHorizontal ();
					EditorGUIUtility.labelWidth = 40;
					EditorGUILayout.PropertyField (mSerialized_BevelInnerSoftness, new GUIContent("Inner"));
					EditorGUILayout.PropertyField (mSerialized_BevelOuterSoftness, new GUIContent("Outer"));
				GUILayout.EndHorizontal();
            
			EditorGUIUtility.labelWidth = 50;
            GUI.color = c;
            GUITools.EndContents ();
            
            var newBevelEdge = 1 - mRenderParams.GetEdge_Outline(0);
            
            bool hasChanged = EditorGUI.EndChangeCheck();
            if (hasChanged || newBevelEdge != mBevelLastUpdatedEdge)
			{
				mMakeMaterialDirty = true;
				UpdateBevelTexture();

                if (hasChanged && !mSerialized_EnableBevel.boolValue)
                    EnableBevel(true);
			}

		}

		void UpdateBevelTexture()
		{
			Color32[] colors = mBevelTexture.GetPixels32();
			int height = mBevelTexture.height;
			int halfHeight = height/2;
			int width = mBevelTexture.width;

			mBevelLastUpdatedEdge = 1- mRenderParams.GetEdge_Outline(0);
			float iStart = mBevelLastUpdatedEdge - mSerialized_BevelOffset.floatValue;
			float iEnd = iStart + mSerialized_BevelWidth.floatValue;

			int ClampHeight = Mathf.FloorToInt((1-mSerialized_BevelClamp.floatValue)*height);
			float LastY = GetBevelTextureHeight( 0, halfHeight, ClampHeight );
			for (int x=0; x<width; ++x)
			{
				float faceX = x/(float)width;

				Color fillColor = Color.gray;
				if (faceX>=iStart && faceX<=iEnd)
				{
					float waveX = (faceX-iStart)/(iEnd-iStart);
					LastY  = GetBevelTextureHeight( waveX, halfHeight, ClampHeight );
					fillColor = Color.gray*0.8f;
				}
	
				for (int i=0; i<height; ++i)
				{
					Color empty = Color.white * (i>ClampHeight ? 1 : 0.9f);
					colors[i*width+x] = (i>LastY ? empty : fillColor);
				}
			}
			mBevelTexture.SetPixels32(colors);
			mBevelTexture.Apply();
		}

		int GetBevelTextureHeight( float waveX, int halfHeight, int ClampHeight )
		{
			float waveCurve = mSerialized_BevelCurve.floatValue;
			float smoothIn = mSerialized_BevelInnerSoftness.floatValue;
			float smoothOut = mSerialized_BevelOuterSoftness.floatValue;
            //float depth = mSerialized_BevelDepth.floatValue;

			float lastY = 0;
			if (waveX < waveCurve)
			{
				waveX = Mathf.Clamp01(waveX/waveCurve);

				float wOut = Mathf.Lerp(waveX, EaseInCubic(waveX), smoothOut);
				float wIn = Mathf.Lerp(waveX, EaseOutCubic(waveX), smoothIn);
				waveX = Mathf.Lerp(wOut, wIn, waveX);
				waveX = waveX * waveCurve + (1-waveCurve);

				lastY = waveX;
			}
			else
			{
				waveX = 1 - Mathf.Clamp01(Mathf.Abs(waveX - waveCurve) / (1 - waveCurve));

				float wOut = Mathf.Lerp(waveX, EaseInCubic(waveX), smoothOut);
				float wIn = Mathf.Lerp(waveX, EaseOutCubic(waveX), smoothIn);
				waveX = Mathf.Lerp(wOut, wIn, waveX);

				waveX = waveX * (1-waveCurve) + waveCurve;
				lastY = waveX;
			}
			lastY = lastY * 2 * halfHeight*0.9f + halfHeight * 0.1f;

			return Mathf.Min((int)lastY, ClampHeight);
		}

		float EaseInCubic(float t)
		{
			return t * t * t;
		}

		float EaseOutCubic(float t)
		{
			t = 1 - t;
			return 1- t * t * t;
		}


		float EaseInOutCubic( float t)
		{
			t = (t > 1) ? 2.0f : t*2;
			if (t< 1) return 0.5f*t*t*t;
			t -= 2;
			return 0.5f*(t*t*t + 2);
		}

	    void EnableBevel( bool enable )
		{
			mSerialized_EnableBevel.boolValue = enable;
            SERenderParams_Inspector.mSelectedRenderEffect = SmartEdgeRenderEffect.Bevel;
            mMakeMaterialDirty = true;
		}
	}
}