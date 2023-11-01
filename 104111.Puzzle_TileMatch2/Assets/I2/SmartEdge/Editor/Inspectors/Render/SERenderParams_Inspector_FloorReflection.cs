using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	public partial class SERenderParams_Inspector
    {
        SerializedProperty mSerialized_EnableFloorReflection, mSerialized_FloorReflectionOpacity, 
							mSerialized_FloorReflectionPlane, mSerialized_FloorReflectionDistance, mSerialized_FloorReflectionFloor, mSerialized_FloorReflection_EnableFloorClamp,
                            mSerialized_FloorReflection_Region, mSerialized_FloorReflection_Back,
                            mSerialized_FloorReflectionTint_Color,
							mSerialized_FloorReflectionTint_BlendMode,
							mSerialized_FloorReflectionTint_Opacity;


        void RegisterProperty_FloorReflection()
		{
			mSerialized_EnableFloorReflection 	         = mSerialized_RenderParams.FindPropertyRelative ("_EnableFloorReflection");
			mSerialized_FloorReflectionOpacity 	         = mSerialized_RenderParams.FindPropertyRelative ("_FloorReflectionOpacity");
			mSerialized_FloorReflectionPlane 	         = mSerialized_RenderParams.FindPropertyRelative ("_FloorReflectionPlane");
			mSerialized_FloorReflectionDistance 		 = mSerialized_RenderParams.FindPropertyRelative ("_FloorReflectionDistance");
            mSerialized_FloorReflectionFloor             = mSerialized_RenderParams.FindPropertyRelative ("_FloorReflectionFloor");
            mSerialized_FloorReflection_EnableFloorClamp = mSerialized_RenderParams.FindPropertyRelative ("_FloorReflection_EnableFloorClamp");
            mSerialized_FloorReflection_Region			 = mSerialized_RenderParams.FindPropertyRelative ("_FloorReflection_Region");
            mSerialized_FloorReflection_Back             = mSerialized_RenderParams.FindPropertyRelative ("_FloorReflection_Back");            
            mSerialized_FloorReflectionTint_Color		 = mSerialized_RenderParams.FindPropertyRelative ("_FloorReflectionTint_Color");
			mSerialized_FloorReflectionTint_BlendMode	 = mSerialized_RenderParams.FindPropertyRelative ("_FloorReflectionTint_BlendMode");
			mSerialized_FloorReflectionTint_Opacity		 = mSerialized_RenderParams.FindPropertyRelative ("_FloorReflectionTint_Opacity");
		}

        void OnGUI_FloorReflection(/*bool forceExpand = true*/)
        {
			EditorGUIUtility.labelWidth = 50;
			
			if (!GUITools.DrawHeader ("Floor Reflection", true/*"SmartEdge FloorReflection"*/, true, mSerialized_EnableFloorReflection.boolValue, EnableFloorReflection, HelpURL: SE_InspectorTools.HelpURL_FloorReflection, disabledColor:GUITools.LightGray, allowCollapsing: false))
				return;

            EditorGUI.BeginChangeCheck();

            GUI.changed = false;
			EditorGUIUtility.labelWidth = 60;
			GUITools.BeginContents();
                var c = GUI.color;
                if (!mSerialized_EnableFloorReflection.boolValue) GUI.color = new Color(1, 1, 1, 0.3f);

                GUILayout.Space (2);
				EditorGUILayout.PropertyField (mSerialized_FloorReflectionOpacity, new GUIContent("Opacity"));

				GUILayout.Space (5);
				EditorGUILayout.PropertyField (mSerialized_FloorReflectionPlane, new GUIContent("Offset"));
				EditorGUILayout.PropertyField (mSerialized_FloorReflectionDistance, new GUIContent("Distance"));

                GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(mSerialized_FloorReflection_EnableFloorClamp, new GUIContent(""), GUILayout.Width(15));
                    GUILayout.Label("Floor", GUITools.DontExpandWidth);
                    EditorGUILayout.PropertyField(mSerialized_FloorReflectionFloor, new GUIContent(""));
                GUILayout.EndHorizontal();


            GUILayout.Space (5);
                GUILayout.BeginHorizontal();
				    EditorGUILayout.PropertyField (mSerialized_FloorReflection_Region, new GUIContent("Region:"));
                    mSerialized_FloorReflection_Back.boolValue = 0==EditorGUILayout.Popup("Layer:", mSerialized_FloorReflection_Back.boolValue?0:1, new string[] { "Back", "Front" });
                GUILayout.EndHorizontal();

				GUILayout.Space (5);

				GUILayout.Space (5);

				GUI.backgroundColor = GUITools.LightGray;
				GUITools.BeginContents();
					GUI.backgroundColor = Color.white;
					GUILayout.BeginHorizontal();
						EditorGUILayout.PropertyField (mSerialized_FloorReflectionTint_Color, new GUIContent("Tint"));
						EditorGUILayout.PropertyField (mSerialized_FloorReflectionTint_BlendMode, new GUIContent(""), GUILayout.Width(100));
					GUILayout.EndHorizontal();
					EditorGUILayout.PropertyField (mSerialized_FloorReflectionTint_Opacity, new GUIContent("Blending"));
				GUITools.EndContents(false);
			EditorGUIUtility.labelWidth = 50;

            mMakeVerticesDirty |= GUI.changed;

            GUI.color = c;
            GUITools.EndContents();

            if (EditorGUI.EndChangeCheck() && !mSerialized_EnableFloorReflection.boolValue)
                EnableFloorReflection(true);
		}

        void EnableFloorReflection( bool enable )
		{
			mSerialized_EnableFloorReflection.boolValue = enable;
            mMakeVerticesDirty = true;
            SERenderParams_Inspector.mSelectedRenderEffect = SmartEdgeRenderEffect.FloorReflection;
        }
    }
}