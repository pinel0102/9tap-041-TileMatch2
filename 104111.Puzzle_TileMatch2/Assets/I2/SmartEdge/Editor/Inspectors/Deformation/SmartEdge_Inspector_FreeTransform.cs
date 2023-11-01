using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace I2.SmartEdge
{
	public partial class SmartEdge_Inspector
	{
		SerializedProperty 	mSerialized_FreeTransform_Enabled, mSerialized_FreeTransform;

        SerializedProperty  mSerialized_FreeTransform_CurveTop, mSerialized_FreeTransform_CurveTop_Points,
                            mSerialized_FreeTransform_CurveBottom, mSerialized_FreeTransform_CurveBottom_Points,
                            mSerialized_FreeTransform_CurveLeft, mSerialized_FreeTransform_CurveLeft_Points,
                            mSerialized_FreeTransform_CurveRight, mSerialized_FreeTransform_CurveRight_Points,

                            mSerialized_FreeTransform_LinkCurve_TopDown, mSerialized_FreeTransform_LinkCurve_LeftRight;
        Object[] mTargets; // cached for OnSceneGUI



        void RegisterProperty_Deformation_FreeTransform()
		{
			mSerialized_FreeTransform 		= mSerialized_Deformation.FindPropertyRelative("_FreeTransform");
            mSerialized_FreeTransform_Enabled = mSerialized_FreeTransform.FindPropertyRelative("_Enabled");

            mSerialized_FreeTransform_CurveTop           = mSerialized_FreeTransform.FindPropertyRelative("_Curve_Top");
            mSerialized_FreeTransform_CurveTop_Points    = mSerialized_FreeTransform_CurveTop.FindPropertyRelative("_Points");

            mSerialized_FreeTransform_CurveBottom        = mSerialized_FreeTransform.FindPropertyRelative("_Curve_Bottom");
            mSerialized_FreeTransform_CurveBottom_Points = mSerialized_FreeTransform_CurveBottom.FindPropertyRelative("_Points");

            mSerialized_FreeTransform_CurveLeft          = mSerialized_FreeTransform.FindPropertyRelative("_Curve_Left");
            mSerialized_FreeTransform_CurveLeft_Points   = mSerialized_FreeTransform_CurveLeft.FindPropertyRelative("_Points");

            mSerialized_FreeTransform_CurveRight         = mSerialized_FreeTransform.FindPropertyRelative("_Curve_Right");
            mSerialized_FreeTransform_CurveRight_Points  = mSerialized_FreeTransform_CurveRight.FindPropertyRelative("_Points");

            //--------------------------------
            mSerialized_FreeTransform_SubdivisionLevel = mSerialized_FreeTransform.FindPropertyRelative("_SubdivisionLevel");
            mSerialized_FreeTransform_TransformType    = mSerialized_FreeTransform.FindPropertyRelative("_TransformType");
            mSerialized_FreeTransform_SliderIsCenter   = mSerialized_FreeTransform.FindPropertyRelative("_SliderIsCenter");
            mSerialized_FreeTransform_LinkSliders      = mSerialized_FreeTransform.FindPropertyRelative("_LinkSliders");
            mSerialized_FreeTransform_InvertEndSlider  = mSerialized_FreeTransform.FindPropertyRelative("_InvertEndSlider");
            mSerialized_FreeTransform_StartSlider      = mSerialized_FreeTransform.FindPropertyRelative("_StartSlider");
            mSerialized_FreeTransform_EndSlider        = mSerialized_FreeTransform.FindPropertyRelative("_EndSlider");
            mSerialized_FreeTransform_WaveRangeMin     = mSerialized_FreeTransform.FindPropertyRelative("_WaveRangeMin");
            mSerialized_FreeTransform_WaveRangeMax     = mSerialized_FreeTransform.FindPropertyRelative("_WaveRangeMax");
            mSerialized_FreeTransform_WaveRangeSize    = mSerialized_FreeTransform.FindPropertyRelative("_WaveRangeSize");
            mSerialized_FreeTransform_EasyType         = mSerialized_FreeTransform.FindPropertyRelative("_EasyType");

            mSerialized_FreeTransform_LinkCurve_TopDown   = mSerialized_FreeTransform.FindPropertyRelative("_LinkCurve_TopDown");
            mSerialized_FreeTransform_LinkCurve_LeftRight = mSerialized_FreeTransform.FindPropertyRelative("_LinkCurve_LeftRight");

            mTargets = targets;
        }

        void OnGUI_FreeTransform ()
		{
			if (!GUITools.DrawHeader ("FreeTransform", "SE FreeTransform", true, mSerialized_FreeTransform_Enabled.boolValue, EnableFreeTransform, HelpURL: SE_InspectorTools.HelpURL_Deformation, disabledColor:GUITools.LightGray))
				return;

            EditorGUI.BeginChangeCheck();

			GUITools.BeginContents();
               OnGUI_FreeTransformContent ();
			GUITools.EndContents ();

            if (EditorGUI.EndChangeCheck())
            {
                if (!mSerialized_FreeTransform_Enabled.boolValue)
                    EnableFreeTransform(true);
                mMakeVerticesDirty = true;
            }
		}

		void EnableFreeTransform( bool enable )
		{
            mSerialized_FreeTransform_Enabled.boolValue = enable;
            mMakeVerticesDirty = true;
		}

        void OnGUI_FreeTransform_Custom()
        {
            EditorGUIUtility.labelWidth = 80;
            GUILayout.Label("Link Curves:");
            GUILayout.BeginHorizontal("Box");
                EditorGUILayout.PropertyField(mSerialized_FreeTransform_LinkCurve_TopDown, new GUIContent("Top & Down"));
                EditorGUILayout.PropertyField(mSerialized_FreeTransform_LinkCurve_LeftRight, new GUIContent("Left & Right"));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUIUtility.labelWidth = 100;
            GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginVertical();
                    EditorGUILayout.PropertyField(mSerialized_FreeTransform_CurveTop_Points, new GUIContent("Top Curve"), true);
                    EditorGUILayout.PropertyField(mSerialized_FreeTransform_CurveBottom_Points, new GUIContent("Bottom Curve"), true);
                    EditorGUILayout.PropertyField(mSerialized_FreeTransform_CurveLeft_Points, new GUIContent("Left Curve"), true);
                    EditorGUILayout.PropertyField(mSerialized_FreeTransform_CurveRight_Points, new GUIContent("Right Curve"), true);
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Reset"))
            {
                FreeTransform_ResetCurves(mSerialized_FreeTransform_CurveTop_Points, new Vector3(0, 1, 0), new Vector3(1, 1, 0));
                FreeTransform_ResetCurves(mSerialized_FreeTransform_CurveBottom_Points, new Vector3(0, 0, 0), new Vector3(1, 0, 0));
                FreeTransform_ResetCurves(mSerialized_FreeTransform_CurveLeft_Points, new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                FreeTransform_ResetCurves(mSerialized_FreeTransform_CurveRight_Points, new Vector3(1, 0, 0), new Vector3(1, 1, 0));
            }
        }

        void FreeTransform_ResetCurves( SerializedProperty prop_CurvePoints, Vector3 p0, Vector3 p1)
        {
            prop_CurvePoints.arraySize = 2;
            var dir = (p1 - p0).normalized * 0.3333f;

            var prop_Point0 = prop_CurvePoints.GetArrayElementAtIndex(0);
            prop_Point0.FindPropertyRelative("time").floatValue = 0;
            prop_Point0.FindPropertyRelative("point").vector3Value = p0;
            prop_Point0.FindPropertyRelative("tangent0").vector3Value = -dir;
            prop_Point0.FindPropertyRelative("tangent1").vector3Value = dir;
            prop_Point0.FindPropertyRelative("lerpType").enumValueIndex = (int)SE_Spline01.eSplineLerp.Broken;

            var prop_Point1 = prop_CurvePoints.GetArrayElementAtIndex(1);
            prop_Point1.FindPropertyRelative("time").floatValue = 1;
            prop_Point1.FindPropertyRelative("point").vector3Value = p1;
            prop_Point1.FindPropertyRelative("tangent0").vector3Value = -dir;
            prop_Point1.FindPropertyRelative("tangent1").vector3Value = dir;
            prop_Point1.FindPropertyRelative("lerpType").enumValueIndex = (int)SE_Spline01.eSplineLerp.Broken;
            HandleUtility.Repaint();
        }


        void OnSceneGUI()
        {
            var freeTransform = mTarget._Deformation._FreeTransform;
            if (mMainTab == eMainTab.Transform && freeTransform._Enabled)
                OnSceneGUI_FreeTransform();
        }

        void OnSceneGUI_FreeTransform()
        {
            var freeTransform = mTarget._Deformation._FreeTransform;
            EditorGUI.BeginChangeCheck();
            OnSceneGUI_Curve(freeTransform._Curve_Bottom, new Vector2(1, 0), new Vector2(0, 0), freeTransform._Curve_Left, freeTransform._Curve_Right, true);
            freeTransform._Curve_Left._Points[0].point  = freeTransform._Curve_Bottom._Points[0].point;
            freeTransform._Curve_Right._Points[0].point = freeTransform._Curve_Bottom._Points[freeTransform._Curve_Bottom._Points.Length-1].point;

            OnSceneGUI_Curve(freeTransform._Curve_Left, new Vector2(0, 1), new Vector2(0, 0), freeTransform._Curve_Bottom, freeTransform._Curve_Top, true);
            freeTransform._Curve_Bottom._Points[0].point = freeTransform._Curve_Left._Points[0].point;
            freeTransform._Curve_Top._Points[0].point    = freeTransform._Curve_Left._Points[freeTransform._Curve_Left._Points.Length - 1].point;

            OnSceneGUI_Curve(freeTransform._Curve_Right,  new Vector2(0, 1), new Vector2(1, 0), freeTransform._Curve_Bottom, freeTransform._Curve_Top, false);
            freeTransform._Curve_Bottom._Points[freeTransform._Curve_Bottom._Points.Length - 1].point = freeTransform._Curve_Right._Points[0].point;
            freeTransform._Curve_Top._Points[freeTransform._Curve_Top._Points.Length - 1].point       = freeTransform._Curve_Right._Points[freeTransform._Curve_Right._Points.Length - 1].point;

            OnSceneGUI_Curve(freeTransform._Curve_Top,    new Vector2(1, 0), new Vector2(0, 1), freeTransform._Curve_Left, freeTransform._Curve_Right, false);
            freeTransform._Curve_Left._Points[freeTransform._Curve_Left._Points.Length - 1].point   = freeTransform._Curve_Top._Points[0].point;
            freeTransform._Curve_Right._Points[freeTransform._Curve_Right._Points.Length - 1].point = freeTransform._Curve_Top._Points[freeTransform._Curve_Top._Points.Length-1].point;

            if (Event.current.shift)
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ArrowPlus);
            else
            if (Event.current.control)
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ArrowMinus);


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(mTargets, "FreeTransform");
                foreach (var se in mTargets)
                    (se as SmartEdge).MarkWidgetAsChanged(true, false);
            }
        }

        void OnSceneGUI_Curve( SE_Spline01 curve, Vector2 pctDir, Vector2 pctOffset, SE_Spline01 curvePrev, SE_Spline01 curveNext, bool curveLower=false)
        {
            int nPoints = curve._Points.Length;

            if (mRect.width <= 0 || mRect.height <= 0)
                return;

            Vector3 prevPoint = MathUtils.v3zero;
            Vector3 prevTangent = MathUtils.v3zero;

            float tanSize = Mathf.Min(mRect.width, mRect.height) / (10 * nPoints);
            tanSize *= mTarget.transform.TransformVector(Vector3.one.normalized).magnitude;

            for (int i = 0; i < nPoints; ++i)
            {
                var newPoint = curve.TransformToWorldPoint(curve._Points[i].point, mTarget.transform, mRect.min, mRect.max);
                var newTangent0 = curve.TransformToWorldDirection(curve._Points[i].tangent0, mTarget.transform, mRect.min, mRect.max);
                var newTangent1 = curve.TransformToWorldDirection(curve._Points[i].tangent1, mTarget.transform, mRect.min, mRect.max);

                if (i > 0)
                    Handles.DrawBezier(prevPoint, newPoint, prevTangent + prevPoint, newTangent0 + newPoint, MathUtils.white, null, 2);

                if (mSerialized_FreeTransform_TransformType.enumValueIndex == (int)SE_Deformation_FreeTransform.eTransformType.Custom)
                {
                    float size = HandleUtility.GetHandleSize(newPoint) * 0.05f;

                    if (i > 0)
                    {
                        var tanPoint0 = newTangent0 + newPoint;
                        Handles.color = Color.green; Handles.DrawLine(newPoint, tanPoint0); Handles.color = Color.white;

#if UNITY_5_5_OR_NEWER
                        newTangent0 = Handles.FreeMoveHandle(tanPoint0, Quaternion.identity, size * 0.8f, Vector3.zero, Handles.CircleHandleCap) - newPoint;
#else
                        newTangent0 = Handles.FreeMoveHandle(tanPoint0, Quaternion.identity, size * 0.8f, Vector3.zero, Handles.CircleCap) - newPoint;
#endif
                        newTangent0.z = 0;
                        if (newTangent0.magnitude < tanSize)
                            newTangent0 = newTangent0.normalized * tanSize;
                    }

                    if (i < nPoints - 1)
                    {
                        var tanPoint1 = newTangent1 + newPoint;
                        Handles.color = Color.green; Handles.DrawLine(newPoint, tanPoint1); Handles.color = Color.white;
#if UNITY_5_5_OR_NEWER
                        newTangent1 = Handles.FreeMoveHandle(tanPoint1, Quaternion.identity, size * 0.8f, Vector3.zero, Handles.CircleHandleCap) - newPoint;
#else
                        newTangent1 = Handles.FreeMoveHandle(tanPoint1, Quaternion.identity, size * 0.8f, Vector3.zero, Handles.CircleCap) - newPoint;
#endif
                        newTangent1.z = 0;
                        if (newTangent1.magnitude < tanSize)
                            newTangent1 = newTangent1.normalized * tanSize;
                    }

#if UNITY_5_5_OR_NEWER
                    newPoint = Handles.FreeMoveHandle(newPoint, Quaternion.identity, size, Vector3.zero, Handles.RectangleHandleCap);
#else
                    newPoint = Handles.FreeMoveHandle(newPoint, Quaternion.identity, size, Vector3.zero, Handles.RectangleCap);
#endif

                    // Remove Point when CTRL + CLICK
                    if (Event.current.control && i > 0 && i < nPoints - 1)
                    {
                        float mouseDist = HandleUtility.DistanceToCircle(newPoint, 0);
                        if (mouseDist < 10 && Event.current.type == EventType.MouseDown/* && Event.current.button == 0*/)
                        {
                            if (i > 0) curve._Points[i - 1].tangent1 *= 1 / 0.6666f;
                            if (i < curve._Points.Length-1) curve._Points[i + 1].tangent0 *= 1 / 0.6666f;
                            var list = curve._Points.ToList();
                            list.RemoveAt(i);
                            curve._Points = list.ToArray();
                            nPoints--;
                            i--;
                            Event.current.Use();
                            GUI.changed = true;
                            continue;
                        }
                    }

                    // Add Point when SHIFT + CLICK
                    if (i > 0 && Event.current.shift && Event.current.button == 0 && Event.current.type == EventType.MouseDown)
                    {
                        Vector3 splitPoint;
                        float splitPct;
                        if (Bezier_ClosestPointToRay(prevPoint, newPoint, prevTangent + prevPoint, newTangent0 + newPoint, out splitPoint, out splitPct))
                        {
                            splitPoint = curve.TransformFromWorldPoint(splitPoint, mTarget.transform, mRect.min, mRect.max);
                            float splitTime = Mathf.Lerp(curve._Points[i - 1].time, curve._Points[i].time, splitPct);
                            var list = curve._Points.ToList();
                            list.Insert(i, new SE_Spline01.SplinePoint
                            {
                                time = splitTime,
                                point = splitPoint,
                                lerpType = SE_Spline01.eSplineLerp.Auto
                            });
                            curve._Points = list.ToArray();
                            curve._Points[i - 1].tangent1 *= 0.3333f;
                            if (i < curve._Points.Length - 1)
                                curve._Points[i + 1].tangent0 *= 0.333f;
                            curve.Set(i, splitPoint, newTangent0, newTangent1);
                            i++;
                            nPoints++;
                            GUI.changed = true;
                            Event.current.Use();
                        }
                    }

                    if (Event.current.isMouse && Event.current.button == 1 && HandleUtility.DistanceToCircle(newPoint, 0) < 10 && curveNext != null)
                    {
                        var menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Auto"), curve._Points[i].lerpType == SE_Spline01.eSplineLerp.Auto, SplineMenu_SetTangentType, new SplineMenu_SetTangentType_Data { spline = curve, index = i, lerpType = SE_Spline01.eSplineLerp.Auto, prevSpline = curvePrev, nextSpline = curveNext, lowerCurve = curveLower });
                        menu.AddItem(new GUIContent("Connected"), curve._Points[i].lerpType == SE_Spline01.eSplineLerp.Connected, SplineMenu_SetTangentType, new SplineMenu_SetTangentType_Data { spline = curve, index = i, lerpType = SE_Spline01.eSplineLerp.Connected, prevSpline = curvePrev, nextSpline = curveNext, lowerCurve = curveLower });
                        menu.AddItem(new GUIContent("Broken"), curve._Points[i].lerpType == SE_Spline01.eSplineLerp.Broken, SplineMenu_SetTangentType, new SplineMenu_SetTangentType_Data { spline = curve, index = i, lerpType = SE_Spline01.eSplineLerp.Broken, prevSpline = curvePrev, nextSpline = curveNext, lowerCurve = curveLower });
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Delete Keyframe"), false, SplineMenu_DeleteControlPoint, new SplineMenu_SetTangentType_Data { spline = curve, index = i });
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Set Horizontal"), false, SplineMenu_FlattenH, new SplineMenu_SetTangentType_Data { spline = curve, index = i });
                        menu.AddItem(new GUIContent("Set Vertical"), false, SplineMenu_FlattenV, new SplineMenu_SetTangentType_Data { spline = curve, index = i });
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Fix Tangent Distortion"), false, SplineMenu_FixTangentDistortion, new SplineMenu_SetTangentType_Data { spline = curve, index = i });
                        menu.AddItem(new GUIContent("Relocate Keyframes in Spline"), false, SplineMenu_RelocateKeyframesForSpline, new SplineMenu_SetTangentType_Data { spline = curve });
                        menu.AddItem(new GUIContent("Relocate All Keyframes"), false, SplineMenu_RelocateKeyframesForAllSplines, null);


                        menu.ShowAsContext();
                    }

                    curve._Points[i].point    = curve.TransformFromWorldPoint(newPoint, mTarget.transform, mRect.min, mRect.max);
                    curve._Points[i].tangent0 = curve.TransformFromWorldDirection(newTangent0, mTarget.transform, mRect.min, mRect.max);
                    curve._Points[i].tangent1 = curve.TransformFromWorldDirection(newTangent1, mTarget.transform, mRect.min, mRect.max);

                    //    curve.SetWorld(i, mTarget.transform, pctDir, pctOffset, mRect, newPoint, newTangent0, newTangent1);
                }
                prevPoint = newPoint;
                prevTangent = newTangent1;
            }
        }
        struct SplineMenu_SetTangentType_Data
        {
            public SE_Spline01 spline, prevSpline, nextSpline;
            public int index;
            public SE_Spline01.eSplineLerp lerpType;
            public bool lowerCurve;
        }
        void SplineMenu_SetTangentType(object o)
        {
            var data = (SplineMenu_SetTangentType_Data)o;
            data.spline._Points[data.index].lerpType = data.lerpType;
            if (data.index == 0)
                data.prevSpline._Points[ data.lowerCurve ? 0 : (data.prevSpline._Points.Length - 1)].lerpType = data.lerpType;
            if (data.index == data.spline._Points.Length - 1)
                data.nextSpline._Points[data.lowerCurve ? 0 : (data.nextSpline._Points.Length - 1)].lerpType = data.lerpType;

            SplineMenu_UpdateSplines();
        }

        void SplineMenu_FlattenH(object o)
        {
            var data = (SplineMenu_SetTangentType_Data)o;
            data.spline._Points[data.index].lerpType = SE_Spline01.eSplineLerp.Connected;
            data.spline._Points[data.index].tangent0 = new Vector3(-1, 0, 0);
            data.spline._Points[data.index].tangent1 = new Vector3(1, 0, 0);
            SplineMenu_UpdateSplines();
        }

        void SplineMenu_FlattenV(object o)
        {
            var data = (SplineMenu_SetTangentType_Data)o;
            data.spline._Points[data.index].lerpType = SE_Spline01.eSplineLerp.Connected;
            data.spline._Points[data.index].tangent0 = new Vector3(0, -1, 0);
            data.spline._Points[data.index].tangent1 = new Vector3(0, 1, 0);
            SplineMenu_UpdateSplines();
        }

        void SplineMenu_DeleteControlPoint(object o)
        {
            var data = (SplineMenu_SetTangentType_Data)o;
            var list = data.spline._Points.ToList();
            list.RemoveAt(data.index);
            data.spline._Points = list.ToArray();
            SplineMenu_UpdateSplines();
        }

        void SplineMenu_RelocateKeyframesForSpline(object o)
        {
            var data = (SplineMenu_SetTangentType_Data)o;
            data.spline.RelocateKeyframes();
            SplineMenu_UpdateSplines();
        }
        void SplineMenu_RelocateKeyframesForAllSplines(object o)
        {
            mTarget._Deformation._FreeTransform._Curve_Bottom.RelocateKeyframes();
            mTarget._Deformation._FreeTransform._Curve_Top.RelocateKeyframes();
            mTarget._Deformation._FreeTransform._Curve_Left.RelocateKeyframes();
            mTarget._Deformation._FreeTransform._Curve_Right.RelocateKeyframes();
            SplineMenu_UpdateSplines();
        }


        void SplineMenu_UpdateSplines()
        {
            Undo.RecordObjects(mTargets, "FreeTransform");
            foreach (var se in mTargets)
                (se as SmartEdge).MarkWidgetAsChanged(true, false);
        }

        void SplineMenu_FixTangentDistortion(object o)
        {
            mTarget._Deformation._FreeTransform._Curve_Bottom.FixTangentLengths();
            mTarget._Deformation._FreeTransform._Curve_Top.FixTangentLengths();
            mTarget._Deformation._FreeTransform._Curve_Left.FixTangentLengths();
            mTarget._Deformation._FreeTransform._Curve_Right.FixTangentLengths();
            SplineMenu_UpdateSplines();
        }


        bool Bezier_ClosestPointToRay( Vector3 startPoint, Vector3 endPoint, Vector3 tangent0, Vector3 tangent1, out Vector3 point, out float pointPct)
        {
            var ctrlPoints = Handles.MakeBezierPoints(startPoint, endPoint, tangent0, tangent1, 20);
            pointPct = 0;
            point = ctrlPoints[0];

            if (HandleUtility.DistanceToPolyLine(ctrlPoints) > 10)
                return false;

            point = HandleUtility.ClosestPointToPolyLine(ctrlPoints);

#if UNITY_5_5_OR_NEWER
            Handles.FreeMoveHandle(point, Quaternion.identity, HandleUtility.GetHandleSize(point)*0.1f, Vector3.zero, Handles.CircleHandleCap);
#else
            Handles.FreeMoveHandle(point, Quaternion.identity, HandleUtility.GetHandleSize(point) * 0.1f, Vector3.zero, Handles.CircleCap);
#endif

            int idx = 1;
            float t = 0;
            for (idx=1; idx<ctrlPoints.Length; ++idx)
            {
                t = InverseLerp(ctrlPoints[idx-1], ctrlPoints[idx], point);
                if (t >= 0 && t <= 1)
                    break;
            }

            if (t <= 0 || t >= 1)
                return false;

            pointPct = Mathf.Lerp((idx-1) / 19.0f, idx/19.0f, t);
            return true;
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }
    }
}