using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;

namespace I2.SmartEdge
{
    public partial class SmartEdge_Inspector
    {
        SerializedProperty mSerialized_FreeTransform_TransformType, mSerialized_FreeTransform_SubdivisionLevel,
            mSerialized_FreeTransform_SliderIsCenter, mSerialized_FreeTransform_LinkSliders, mSerialized_FreeTransform_InvertEndSlider,
            mSerialized_FreeTransform_StartSlider,  mSerialized_FreeTransform_EndSlider, mSerialized_FreeTransform_EasyType;

        SerializedProperty mSerialized_FreeTransform_WaveRangeMin, mSerialized_FreeTransform_WaveRangeMax, mSerialized_FreeTransform_WaveRangeSize;

        void OnGUI_FreeTransformContent()
		{
			EditorGUIUtility.labelWidth = 50;

			var mTabImages = new Texture2D[]{	SmartEdgeTools.SmartEdgeSkin.FindStyle ("Deformation TopBottom").normal.background,
												SmartEdgeTools.SmartEdgeSkin.FindStyle ("Deformation LeftRight").normal.background,
												SmartEdgeTools.SmartEdgeSkin.FindStyle ("Deformation Wave").normal.background,
												SmartEdgeTools.SmartEdgeSkin.FindStyle ("Deformation Custom").normal.background};


			GUIStyle dragtabImage = SmartEdgeTools.SmartEdgeSkin.FindStyle("dragtabImage");

			EditorGUI.BeginChangeCheck();
			mSerialized_FreeTransform_TransformType.enumValueIndex = GUITools.DrawTabs( mSerialized_FreeTransform_TransformType.enumValueIndex, mTabImages, dragtabImage, 40 );

			if (EditorGUI.EndChangeCheck() && mSerialized_FreeTransform_TransformType.enumValueIndex != (int)SE_Deformation_FreeTransform.eTransformType.Custom)
			{
                FreeTransform_ResetCurves(mSerialized_FreeTransform_CurveTop_Points, new Vector3(0, 1, 0), new Vector3(1, 1, 0));
                FreeTransform_ResetCurves(mSerialized_FreeTransform_CurveBottom_Points, new Vector3(0, 0, 0), new Vector3(1, 0, 0));
                FreeTransform_ResetCurves(mSerialized_FreeTransform_CurveLeft_Points, new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                FreeTransform_ResetCurves(mSerialized_FreeTransform_CurveRight_Points, new Vector3(1, 0, 0), new Vector3(1, 1, 0));
                mSerialized_FreeTransform_LinkCurve_LeftRight.boolValue = false;
                mSerialized_FreeTransform_LinkCurve_TopDown.boolValue = false;
            }

            GUITools.BeginContents ();
                GUILayout.Space(5);

                switch ((SE_Deformation_FreeTransform.eTransformType)mSerialized_FreeTransform_TransformType.enumValueIndex)
                {
                    case SE_Deformation_FreeTransform.eTransformType.TopDown: OnGUI_Transform_TopDown(); break;
                    case SE_Deformation_FreeTransform.eTransformType.LeftRight: OnGUI_Transform_LeftRight(); break;
                    case SE_Deformation_FreeTransform.eTransformType.Wave: OnGUI_Transform_Wave(); break;
                    case SE_Deformation_FreeTransform.eTransformType.Custom: OnGUI_FreeTransform_Custom(); break;
                }
                GUILayout.Space(5);
            GUITools.EndContents (false);

			GUILayout.Space (5);

			GUILayout.BeginHorizontal ();
				GUILayout.Label ("Subdivisions");

				if (GUILayout.Toggle (mSerialized_FreeTransform_SubdivisionLevel.intValue == 0, "None", EditorStyles.toolbarButton))    mSerialized_FreeTransform_SubdivisionLevel.intValue = 0;
				if (GUILayout.Toggle (mSerialized_FreeTransform_SubdivisionLevel.intValue == 1, "1x",   EditorStyles.toolbarButton))    mSerialized_FreeTransform_SubdivisionLevel.intValue = 1;
				if (GUILayout.Toggle (mSerialized_FreeTransform_SubdivisionLevel.intValue == 2, "2x",   EditorStyles.toolbarButton))    mSerialized_FreeTransform_SubdivisionLevel.intValue = 2;
                if (GUILayout.Toggle (mSerialized_FreeTransform_SubdivisionLevel.intValue == 3, "3x",   EditorStyles.toolbarButton))    mSerialized_FreeTransform_SubdivisionLevel.intValue = 3;
                if (GUILayout.Toggle (mSerialized_FreeTransform_SubdivisionLevel.intValue == 4, "4x",   EditorStyles.toolbarButton))    mSerialized_FreeTransform_SubdivisionLevel.intValue = 4;
            GUILayout.EndHorizontal ();                      

			EditorGUIUtility.labelWidth = 0;
		}

		void OnGUI_Transform_TopDown()
		{
			OnGUI_Transform_TwoCurves ("Top", "Bottom", "Center", "Border", UpdateCurve_TopDown);
		}

		void OnGUI_Transform_LeftRight()
		{
			OnGUI_Transform_TwoCurves ("Left", "Right", "Top", "Bottom", UpdateCurve_LeftRight);
		}


        void OnGUI_Transform_TwoCurves(string sTop, string sBottom, string sCenter, string sBorder, System.Action updateCurves)
        {
			GUILayout.BeginHorizontal ();
				mSerialized_FreeTransform_SliderIsCenter.boolValue = GUILayout.Toggle (mSerialized_FreeTransform_SliderIsCenter.boolValue, sCenter, EditorStyles.toolbarButton);
				mSerialized_FreeTransform_SliderIsCenter.boolValue = !GUILayout.Toggle (!mSerialized_FreeTransform_SliderIsCenter.boolValue, sBorder, EditorStyles.toolbarButton);

				GUILayout.Space (15);

                GUI.enabled = false;
                mSerialized_FreeTransform_EasyType.enumValueIndex = EditorGUILayout.Popup (mSerialized_FreeTransform_EasyType.enumValueIndex, mSerialized_FreeTransform_EasyType.enumNames);
                GUI.enabled = true;
			GUILayout.EndHorizontal ();

			GUILayout.Space (5);

			OnGUI_Transform_Sliders (sTop, sBottom );

			updateCurves ();
		}

		void OnGUI_Transform_Sliders(string ParamTop, string ParamBottom)
		{
			GUILayout.BeginHorizontal ();
				GUILayout.BeginVertical ();
					mSerialized_FreeTransform_StartSlider.floatValue = EditorGUILayout.Slider (ParamTop, mSerialized_FreeTransform_StartSlider.floatValue, -1, 1);

					if (mSerialized_FreeTransform_LinkSliders.boolValue)
						mSerialized_FreeTransform_EndSlider.floatValue = mSerialized_FreeTransform_InvertEndSlider.boolValue ? -mSerialized_FreeTransform_StartSlider.floatValue : mSerialized_FreeTransform_StartSlider.floatValue;

					GUI.enabled = !mSerialized_FreeTransform_LinkSliders.boolValue;
					mSerialized_FreeTransform_EndSlider.floatValue = EditorGUILayout.Slider (ParamBottom, mSerialized_FreeTransform_EndSlider.floatValue, -1, 1);
					GUI.enabled = true;
				GUILayout.EndVertical ();

				GUILayout.Space (15);

				GUILayout.BeginVertical (GUILayout.Width(1));
					GUILayout.FlexibleSpace ();
					GUILayout.BeginHorizontal ();
						mSerialized_FreeTransform_LinkSliders.boolValue = GUILayout.Toggle(mSerialized_FreeTransform_LinkSliders.boolValue, GUI.skin.FindStyle("IN LockButton").onActive.background, "Button", GUILayout.Width(30), GUILayout.Height(30));
						mSerialized_FreeTransform_InvertEndSlider.boolValue = GUILayout.Toggle (mSerialized_FreeTransform_InvertEndSlider.boolValue, GUI.skin.FindStyle ("ColorPickerHorizThumb").normal.background, "Button", GUILayout.Width(30), GUILayout.Height(30));
					GUILayout.EndHorizontal ();
					GUILayout.FlexibleSpace ();
				GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();
		}

		#region UpdateCurve Top Down
		void UpdateCurve_TopDown ()
		{
            float dist = 0.3333f;
			if (mSerialized_FreeTransform_SliderIsCenter.boolValue)
			{
				UpdateCurve (new Vector3(0,1,0), new Vector3(0.5f, 1+mSerialized_FreeTransform_StartSlider.floatValue,0), 	new Vector3(1,1,0), mSerialized_FreeTransform_CurveTop_Points, (SE_Deformation_FreeTransform.eEasyType)mSerialized_FreeTransform_EasyType.enumValueIndex, new Vector3(1,0,0));
				UpdateCurve (new Vector3(0,0,0), new Vector3(0.5f, mSerialized_FreeTransform_EndSlider.floatValue,0), new Vector3(1, 0, 0), mSerialized_FreeTransform_CurveBottom_Points, (SE_Deformation_FreeTransform.eEasyType)mSerialized_FreeTransform_EasyType.enumValueIndex, new Vector3(1,0,0));
			}
			else
			{
                dist = 0.3333f*((1 + mSerialized_FreeTransform_StartSlider.floatValue) - mSerialized_FreeTransform_EndSlider.floatValue);
                UpdateCurve (new Vector3(0, 1+mSerialized_FreeTransform_StartSlider.floatValue,0), 	  new Vector3(0.5f,1,0), new Vector3(1,1+mSerialized_FreeTransform_StartSlider.floatValue,0), mSerialized_FreeTransform_CurveTop_Points, (SE_Deformation_FreeTransform.eEasyType)mSerialized_FreeTransform_EasyType.enumValueIndex, new Vector3(1, 0, 0));
				UpdateCurve (new Vector3(0, mSerialized_FreeTransform_EndSlider.floatValue, 0), new Vector3(0.5f,0,0), new Vector3(1,mSerialized_FreeTransform_EndSlider.floatValue,0), mSerialized_FreeTransform_CurveBottom_Points, (SE_Deformation_FreeTransform.eEasyType)mSerialized_FreeTransform_EasyType.enumValueIndex, new Vector3(1, 0, 0));
			}

            var left0 = mSerialized_FreeTransform_CurveLeft_Points.GetArrayElementAtIndex(0);
            left0.FindPropertyRelative("point").vector3Value = mSerialized_FreeTransform_CurveBottom_Points.GetArrayElementAtIndex(0).FindPropertyRelative("point").vector3Value;
            left0.FindPropertyRelative("tangent1").vector3Value = new Vector3(0, dist, 0);

            var right0 = mSerialized_FreeTransform_CurveRight_Points.GetArrayElementAtIndex(0);
            right0.FindPropertyRelative("point").vector3Value = mSerialized_FreeTransform_CurveBottom_Points.GetArrayElementAtIndex(mSerialized_FreeTransform_CurveBottom_Points.arraySize - 1).FindPropertyRelative("point").vector3Value;
            right0.FindPropertyRelative("tangent1").vector3Value = new Vector3(0, dist, 0);

            var left1 = mSerialized_FreeTransform_CurveLeft_Points.GetArrayElementAtIndex(mSerialized_FreeTransform_CurveLeft_Points.arraySize - 1);
            left1.FindPropertyRelative("point").vector3Value = mSerialized_FreeTransform_CurveTop_Points.GetArrayElementAtIndex(0).FindPropertyRelative("point").vector3Value;
            left1.FindPropertyRelative("tangent0").vector3Value = new Vector3(0, -dist, 0);

            var right1 = mSerialized_FreeTransform_CurveRight_Points.GetArrayElementAtIndex(mSerialized_FreeTransform_CurveRight_Points.arraySize - 1);
            right1.FindPropertyRelative("point").vector3Value = mSerialized_FreeTransform_CurveTop_Points.GetArrayElementAtIndex(mSerialized_FreeTransform_CurveTop_Points.arraySize - 1).FindPropertyRelative("point").vector3Value;
            right1.FindPropertyRelative("tangent0").vector3Value = new Vector3(0, -dist, 0);
        }

        void UpdateCurve( Vector3 value0, Vector3 value1, Vector3 value2, SerializedProperty prop_CurvePoints, SE_Deformation_FreeTransform.eEasyType easyType, Vector3 tangent)
		{
            prop_CurvePoints.arraySize = 3;
            tangent *= 0.333f/2f;

            var prop_Point0 = prop_CurvePoints.GetArrayElementAtIndex(0);
            prop_Point0.FindPropertyRelative("time").floatValue = 0;
            prop_Point0.FindPropertyRelative("point").vector3Value = value0;
            prop_Point0.FindPropertyRelative("tangent0").vector3Value = -tangent;
            prop_Point0.FindPropertyRelative("tangent1").vector3Value = tangent;
            prop_Point0.FindPropertyRelative("lerpType").enumValueIndex = (int)SE_Spline01.eSplineLerp.Auto;

            var prop_Point1 = prop_CurvePoints.GetArrayElementAtIndex(1);
            prop_Point1.FindPropertyRelative("time").floatValue = 0.5f;
            prop_Point1.FindPropertyRelative("point").vector3Value = value1;
            prop_Point1.FindPropertyRelative("tangent0").vector3Value = -tangent;
            prop_Point1.FindPropertyRelative("tangent1").vector3Value = tangent;
            prop_Point1.FindPropertyRelative("lerpType").enumValueIndex = (int)SE_Spline01.eSplineLerp.Auto;

            var prop_Point2 = prop_CurvePoints.GetArrayElementAtIndex(2);
            prop_Point2.FindPropertyRelative("time").floatValue = 1;
            prop_Point2.FindPropertyRelative("point").vector3Value = value2;
            prop_Point2.FindPropertyRelative("tangent0").vector3Value = -tangent;
            prop_Point2.FindPropertyRelative("tangent1").vector3Value = tangent;
            prop_Point2.FindPropertyRelative("lerpType").enumValueIndex = (int)SE_Spline01.eSplineLerp.Auto;
        }

        #endregion

        #region UpdateCurve Left Right

        void UpdateCurve_LeftRight (  )
		{
            float dist = 0.3333f*((1 + mSerialized_FreeTransform_EndSlider.floatValue) - mSerialized_FreeTransform_StartSlider.floatValue);
            float distBottom, distTop;
            if (mSerialized_FreeTransform_SliderIsCenter.boolValue)
			{
                distBottom = 0.3333f; distTop = dist;
                UpdateCurve(new Vector3(0, 0, 0), new Vector3(mSerialized_FreeTransform_StartSlider.floatValue, 1, 0), mSerialized_FreeTransform_CurveLeft_Points, (SE_Deformation_FreeTransform.eEasyType)mSerialized_FreeTransform_EasyType.enumValueIndex, new Vector3(0, 1, 0));
                UpdateCurve(new Vector3(1, 0, 0), new Vector3(1 + mSerialized_FreeTransform_EndSlider.floatValue, 1, 0), mSerialized_FreeTransform_CurveRight_Points, (SE_Deformation_FreeTransform.eEasyType)mSerialized_FreeTransform_EasyType.enumValueIndex, new Vector3(0, 1, 0));
            }
            else
			{
                distBottom = dist; distTop = 0.3333f;
                UpdateCurve(new Vector3(mSerialized_FreeTransform_StartSlider.floatValue, 0, 0), new Vector3(0, 1, 0), mSerialized_FreeTransform_CurveLeft_Points, (SE_Deformation_FreeTransform.eEasyType)mSerialized_FreeTransform_EasyType.enumValueIndex, new Vector3(0, 1, 0));
                UpdateCurve(new Vector3(1 + mSerialized_FreeTransform_EndSlider.floatValue, 0, 0), new Vector3(1, 1, 0), mSerialized_FreeTransform_CurveRight_Points, (SE_Deformation_FreeTransform.eEasyType)mSerialized_FreeTransform_EasyType.enumValueIndex, new Vector3(0, 1, 0));
			}

            var bottom0 = mSerialized_FreeTransform_CurveBottom_Points.GetArrayElementAtIndex(0);
            bottom0.FindPropertyRelative("point").vector3Value = mSerialized_FreeTransform_CurveLeft_Points.GetArrayElementAtIndex(0).FindPropertyRelative("point").vector3Value;
            bottom0.FindPropertyRelative("tangent1").vector3Value = new Vector3(distBottom,0,0);

            var top0 = mSerialized_FreeTransform_CurveTop_Points.GetArrayElementAtIndex(0);
            top0.FindPropertyRelative("point").vector3Value = mSerialized_FreeTransform_CurveLeft_Points.GetArrayElementAtIndex(mSerialized_FreeTransform_CurveLeft_Points.arraySize - 1).FindPropertyRelative("point").vector3Value;
            top0.FindPropertyRelative("tangent1").vector3Value = new Vector3(distTop, 0, 0);

            var bottom1 = mSerialized_FreeTransform_CurveBottom_Points.GetArrayElementAtIndex(mSerialized_FreeTransform_CurveBottom_Points.arraySize - 1);
            bottom1.FindPropertyRelative("point").vector3Value = mSerialized_FreeTransform_CurveRight_Points.GetArrayElementAtIndex(0).FindPropertyRelative("point").vector3Value;
            bottom1.FindPropertyRelative("tangent0").vector3Value = new Vector3(-distBottom, 0, 0);

            var top1 = mSerialized_FreeTransform_CurveTop_Points.GetArrayElementAtIndex(mSerialized_FreeTransform_CurveTop_Points.arraySize - 1);
            top1.FindPropertyRelative("point").vector3Value = mSerialized_FreeTransform_CurveRight_Points.GetArrayElementAtIndex(mSerialized_FreeTransform_CurveRight_Points.arraySize - 1).FindPropertyRelative("point").vector3Value;
            top1.FindPropertyRelative("tangent0").vector3Value = new Vector3(-distTop, 0, 0);
        }

        void UpdateCurve( Vector3 value0, Vector3 value1, SerializedProperty prop_CurvePoints, SE_Deformation_FreeTransform.eEasyType easyType, Vector3 tangent)
		{
            prop_CurvePoints.arraySize = 2;
            tangent *= 0.333f;

            var prop_Point0 = prop_CurvePoints.GetArrayElementAtIndex(0);
            prop_Point0.FindPropertyRelative("time").floatValue = 0;
            prop_Point0.FindPropertyRelative("point").vector3Value = value0;
            prop_Point0.FindPropertyRelative("tangent0").vector3Value = -tangent;
            prop_Point0.FindPropertyRelative("tangent1").vector3Value = tangent;
            prop_Point0.FindPropertyRelative("lerpType").enumValueIndex = (int)SE_Spline01.eSplineLerp.Auto;

            var prop_Point1 = prop_CurvePoints.GetArrayElementAtIndex(1);
            prop_Point1.FindPropertyRelative("time").floatValue = 1f;
            prop_Point1.FindPropertyRelative("point").vector3Value = value1;
            prop_Point1.FindPropertyRelative("tangent0").vector3Value = -tangent;
            prop_Point1.FindPropertyRelative("tangent1").vector3Value = tangent;
            prop_Point1.FindPropertyRelative("lerpType").enumValueIndex = (int)SE_Spline01.eSplineLerp.Auto;
        }

        #endregion

        void OnGUI_Transform_Wave(  )
		{
            GUI.enabled = false;
			float minVal = mSerialized_FreeTransform_WaveRangeMin.floatValue;
			float maxVal = mSerialized_FreeTransform_WaveRangeMax.floatValue;
			float sizeVal = mSerialized_FreeTransform_WaveRangeSize.floatValue;
            EditorGUI.BeginChangeCheck();

			GUILayout.BeginHorizontal ();
				EditorGUILayout.MinMaxSlider(new GUIContent("Range"), ref minVal, ref maxVal, 0, sizeVal);
				maxVal = Mathf.Max (maxVal, minVal + 0.5f);
				sizeVal = EditorGUILayout.FloatField (sizeVal, GUILayout.Width(50));
			GUILayout.EndHorizontal ();

           if (EditorGUI.EndChangeCheck())
           {
               mSerialized_FreeTransform_WaveRangeMin.floatValue = minVal;
               mSerialized_FreeTransform_WaveRangeMax.floatValue = maxVal;
               mSerialized_FreeTransform_WaveRangeSize.floatValue = sizeVal;
           }

           mSerialized_FreeTransform_StartSlider.floatValue = mSerialized_FreeTransform_EndSlider.floatValue = EditorGUILayout.Slider ("Size", Mathf.Max (mSerialized_FreeTransform_EndSlider.floatValue, mSerialized_FreeTransform_StartSlider.floatValue), -1, 1);

           UpdateCurve_Wave ();
            GUI.enabled = true;
		}

		void UpdateCurve_Wave(  )
		{
			UpdateCurve_Wave (mSerialized_FreeTransform_StartSlider.floatValue, mSerialized_FreeTransform_CurveTop);
			UpdateCurve_Wave (mSerialized_FreeTransform_StartSlider.floatValue, mSerialized_FreeTransform_CurveBottom);
		}

		void UpdateCurve_Wave( float Range, SerializedProperty serializedCurve)
		{
			/*AnimationCurve curve = serializedCurve.animationCurveValue;
			float val = mSerialized_FreeTransform_WaveRangeMin.floatValue;
			int iKey = 0;
			while (val < mSerialized_FreeTransform_WaveRangeMax.floatValue)
			{
				Keyframe key = new Keyframe();
				key.time = (val-mSerialized_FreeTransform_WaveRangeMin.floatValue)/(mSerialized_FreeTransform_WaveRangeMax.floatValue-mSerialized_FreeTransform_WaveRangeMin.floatValue);
				key.value = Range * ((Mathf.FloorToInt(val)%2)*2-1);
				key.tangentMode=0; key.inTangent=0; key.outTangent=0;

				if (curve.length<=iKey)
					curve.AddKey(key);
				else
					curve.MoveKey(iKey, key);

				iKey++;
				val = Mathf.Ceil (val + 0.1f);
			}

			serializedProp.FindPropertyRelative("Curve_Top").animationCurveValue = curve;
			serializedCurve.animationCurveValue = curve;
			serializedProp.serializedObject.SetIsDifferentCacheDirty ();*/
		}

		/*void OnGUI_Transform_Sphere(  )
		{
			EditorGUIUtility.labelWidth = 80;

			SerializedProperty mSerialized_Radius = serializedProp.FindPropertyRelative ("_SphereRadius");
			SerializedProperty mSerialized_Bias = serializedProp.FindPropertyRelative ("_SphereStart");

			EditorGUILayout.Slider (serializedProp.FindPropertyRelative ("_Roundness"), 0, 1, "Roundness");
			GUILayout.Space (5);

			GUI.backgroundColor = GUITools.LightGray;
			GUITools.BeginContents ();
				GUILayout.BeginHorizontal ();
					mSerialized_Radius.floatValue = EditorGUILayout.FloatField ("Radius", mSerialized_Radius.floatValue);
					mSerialized_Bias.floatValue = EditorGUILayout.Slider ( "Arc Bias", Mathf.Repeat(mSerialized_Bias.floatValue,1), 0, 1);
				GUILayout.EndHorizontal ();
			GUITools.EndContents (false);
			GUI.backgroundColor = Color.white;
		}*/
	}
}