using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	public partial class SERenderParams_Inspector
    {
        SerializedProperty mSerialized_EnableReflection, 
							mSerialized_ReflectionFace, mSerialized_ReflectionOutline, 
							mSerialized_ReflectionMap, mSerialized_ReflectionRotation,
                            mSerialized_ReflectionFresnel_Bias, mSerialized_ReflectionFresnel_Scale,
                            mSerialized_ReflectionIntensity, mSerialized_ReflectionGlass;

        void RegisterProperty_Reflection()
		{
			mSerialized_EnableReflection 	    = mSerialized_RenderParams.FindPropertyRelative ("_EnableReflection");
			mSerialized_ReflectionFace 		    = mSerialized_RenderParams.FindPropertyRelative ("_ReflectionColor_Face");
			mSerialized_ReflectionOutline 	    = mSerialized_RenderParams.FindPropertyRelative ("_ReflectionColor_Outline");
			mSerialized_ReflectionRotation 	    = mSerialized_RenderParams.FindPropertyRelative ("_ReflectionRotation");
            mSerialized_ReflectionFresnel_Bias  = mSerialized_RenderParams.FindPropertyRelative ("_ReflectionFresnel_Bias");
            mSerialized_ReflectionFresnel_Scale = mSerialized_RenderParams.FindPropertyRelative ("_ReflectionFresnel_Scale");
            mSerialized_ReflectionIntensity     = mSerialized_RenderParams.FindPropertyRelative ("_ReflectionIntensity");
            mSerialized_ReflectionGlass         = mSerialized_RenderParams.FindPropertyRelative ("_ReflectionGlass");

            mSerialized_ReflectionMap = mSerialized_RenderParams.FindPropertyRelative("_ReflectionMap");
		}

        void OnGUI_Reflection()
        {
			if (!GUITools.DrawHeader ("Reflection", true/*"SmartEdge Reflection"*/, true, mSerialized_EnableReflection.boolValue, EnableReflection, HelpURL: SE_InspectorTools.HelpURL_Reflection, disabledColor:GUITools.LightGray, allowCollapsing: false))
				return;
			EditorGUIUtility.labelWidth = 60;

            GUITools.BeginContents();
                var c = GUI.color;
                if (!mSerialized_EnableReflection.boolValue) GUI.color = new Color(1, 1, 1, 0.3f);

                GUILayout.Space (2);
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginHorizontal();
					mSerialized_ReflectionMap.objectReferenceValue = EditorGUILayout.ObjectField(mSerialized_ReflectionMap.objectReferenceValue, typeof(Cubemap), false, GUILayout.Width(140), GUILayout.Height(140));

					GUILayout.BeginVertical ();
						EditorGUILayout.PropertyField (mSerialized_ReflectionFace, new GUIContent("Face"));
                        EditorGUILayout.PropertyField(mSerialized_ReflectionOutline, new GUIContent("Outline"));

                        EditorGUILayout.PropertyField(mSerialized_ReflectionIntensity, new GUIContent("Intensity"));
                        GUILayout.BeginHorizontal();
                            GUILayout.Label("Mirror", GUITools.DontExpandWidth);
                            mSerialized_ReflectionGlass.floatValue = GUILayout.HorizontalSlider(mSerialized_ReflectionGlass.floatValue, 0, 1);
                            GUILayout.Label("Glass", GUITools.DontExpandWidth);
                        GUILayout.EndHorizontal();

                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(mSerialized_ReflectionRotation, new GUIContent("Rotation"));


                        GUILayout.Space(5);
                        GUILayout.BeginHorizontal();
                            GUILayout.Label("Fresnel:", EditorStyles.boldLabel, GUILayout.Width(55));
                            GUILayout.BeginVertical();
                                GUILayout.BeginHorizontal();
                                    EditorGUIUtility.labelWidth = 30;
                                    EditorGUILayout.PropertyField(mSerialized_ReflectionFresnel_Bias, new GUIContent("Bias"));
                                    EditorGUIUtility.labelWidth = 55;

                                    if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
                                        mSerialized_ReflectionFresnel_Bias.floatValue = mSerialized_ReflectionFresnel_Scale.floatValue >= 0 ? 0 : 1;
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                    float softness = Mathf.Abs(mSerialized_ReflectionFresnel_Scale.floatValue);
                                    bool inside = mSerialized_ReflectionFresnel_Scale.floatValue >= 0;
                                    softness = EditorGUILayout.Slider(new GUIContent("Softness"), softness, 0.01f, 1);
                                    inside = GUILayout.Toggle(inside, new GUIContent("inside"), GUITools.DontExpandWidth);
                                    mSerialized_ReflectionFresnel_Scale.floatValue = inside ? softness : -softness;
                                GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                        GUILayout.EndHorizontal();

                    GUILayout.EndVertical ();

				GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                if (!mSerialized_EnableReflection.boolValue)
                    EnableReflection(true);
                mMakeMaterialDirty = true;
            }
            GUI.color = c;
            GUITools.EndContents ();
		}

        void EnableReflection( bool enable )
		{
			mSerialized_EnableReflection.boolValue = enable;
            mMakeMaterialDirty = true;
            SERenderParams_Inspector.mSelectedRenderEffect = SmartEdgeRenderEffect.Reflection;
        }
    }
}