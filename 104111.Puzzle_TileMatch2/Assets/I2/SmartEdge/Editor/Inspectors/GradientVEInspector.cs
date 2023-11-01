using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
    //[CustomEditor(typeof(GradientVertexEffect))]
    public class GradientVEInspector : Editor
    {
//         SerializedObject mSerializedObj;
//         GradientVertexEffect mTarget;

//        SerializedProperty mSerialized_Gradient;

//         public void OnEnable()
//         {
//             mTarget = target as GradientVertexEffect;
//             mSerializedObj = new SerializedObject(mTarget);
//             mSerialized_Gradient = mSerializedObj.FindProperty("_Gradient");
//         }
// 
//         public override void OnInspectorGUI()
//         {
//             mSerializedObj.UpdateIfDirtyOrScript();
// 
//             GUI.backgroundColor = Color.Lerp(Color.black, Color.gray, 1);
//             GUILayout.BeginVertical(SmartEdge_Inspector.GUIStyle_Background, GUILayout.Height(1));
//             GUI.backgroundColor = Color.white;
// 
//             /*if (GUILayout.Button("Gradient", SmartEdge_Inspector.GUIStyle_SmallHeader))
//             {
//                 Application.OpenURL(SmartEdge_Inspector.HelpURL_Gradients);
//             }*/
//             GUILayout.Space(5);
// 
//             EditorGUIUtility.labelWidth = 60;
// 
//             OnGUIGradient(mSerialized_Gradient);
// 
//             GUILayout.BeginHorizontal();
//             GUILayout.FlexibleSpace();
// 
//             if (GUILayout.Button("Ask a Question", EditorStyles.miniLabel))
//                 Application.OpenURL(SmartEdge_Inspector.HelpURL_forum);
// 
//             GUILayout.Space(10);
// 
//             if (GUILayout.Button("Documentation", EditorStyles.miniLabel))
//                 Application.OpenURL(SmartEdge_Inspector.HelpURL_Gradients);
// 
//             GUILayout.Space(10);
// 
//             if (GUILayout.Button("v" + SmartEdge_Inspector.GetVersion(), EditorStyles.miniLabel))
//                 Application.OpenURL(SmartEdge_Inspector.HelpURL_ReleaseNotes);
// 
//             GUILayout.EndHorizontal();
//             EditorGUIUtility.labelWidth = 0;
// 
//             GUILayout.EndVertical();
// 
// 
//             if (mSerializedObj.ApplyModifiedProperties())
//                 UIEffect.MarkWidgetAsChanged(mTarget, true, false);
//         }

        public static void OnGUIGradient(SerializedProperty gradientEffect, GradientEffect gradEffect)
        {
            SerializedProperty mSerialized_Angle = gradientEffect.FindPropertyRelative("_Angle");
            SerializedProperty mSerialized_Gradient = gradientEffect.FindPropertyRelative("_Gradient");
            SerializedProperty mSerialized_Scale = gradientEffect.FindPropertyRelative("_Scale");
            SerializedProperty mSerialized_Bias = gradientEffect.FindPropertyRelative("_Bias");
            SerializedProperty mSerialized_BlendMode = gradientEffect.FindPropertyRelative("_BlendMode");
            SerializedProperty mSerialized_Opacity = gradientEffect.FindPropertyRelative("_Opacity");
            SerializedProperty mSerialized_Precision = gradientEffect.FindPropertyRelative("_Precision");
            SerializedProperty mSerialized_Region = gradientEffect.FindPropertyRelative("_Region");

            EditorGUIUtility.labelWidth = 60;

            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(mSerialized_Gradient, GUILayout.Height(20));
            if (EditorGUI.EndChangeCheck())
                gradEffect.SetGradientDirty();

            EditorGUIUtility.labelWidth = 50;
            GUILayout.BeginHorizontal();
                EditorGUILayout.Slider(mSerialized_Scale, 0, 5);
                EditorGUILayout.Slider(mSerialized_Bias, -1, 1);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

                GUILayout.BeginVertical(GUILayout.Width(1));
                    Rect rect = GUILayoutUtility.GetRect(50, 50);
                    Texture2D texBackground = SmartEdgeTools.SmartEdgeSkin.FindStyle("Angle Circle").normal.background;
                    Texture2D texLine = SmartEdgeTools.SmartEdgeSkin.FindStyle("Angle KnobLine").normal.background;
                    mSerialized_Angle.floatValue = GUITools.AngleCircle(rect, mSerialized_Angle.floatValue, 1, 0, 360, texBackground, texLine);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal();
                        mSerialized_Angle.floatValue = EditorGUILayout.FloatField(mSerialized_Angle.floatValue, GUILayout.Width(50));
                        GUILayout.Label("°");
                    GUILayout.EndHorizontal();

                    GUILayout.Label("Region:", EditorStyles.miniLabel);
                    EditorGUILayout.PropertyField(mSerialized_Region, new GUIContent(""));
                GUILayout.EndVertical();

                GUITools.BeginContents();
                GUILayout.Space(5);

                    EditorGUIUtility.labelWidth = 70;
                    mSerialized_BlendMode.enumValueIndex = EditorGUILayout.Popup("Color Mix", mSerialized_BlendMode.enumValueIndex, mSerialized_BlendMode.enumDisplayNames);
                    EditorGUIUtility.labelWidth = 50;
                    EditorGUILayout.PropertyField(mSerialized_Opacity);

                GUILayout.Space(5);
                GUITools.EndContents(false);

            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUITools.ToggleToolbar(mSerialized_Precision);
            GUILayout.Space(5);

            EditorGUIUtility.labelWidth = 0;
        }

    }
}