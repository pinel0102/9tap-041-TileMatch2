using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
    [CustomEditor(typeof(SmartEdgeRenderPreset))]
    public class SmartEdgeRenderPreset_Inspector : Editor
    {
        public bool mMakeMaterialDirty, mMakeVerticesDirty;

        SERenderParams_Inspector mRenderParamsInspector;

        public void OnEnable()
        {
            if (target == null)
                return;

            var preset            = target as SmartEdgeRenderPreset;
            var prop_RenderParams = serializedObject.FindProperty("_RenderParams");

            mRenderParamsInspector = new SERenderParams_Inspector( prop_RenderParams, preset._RenderParams, null );
        }

        public override void OnInspectorGUI()
         {
            GUITools.Editor_UpdateIfRequiredOrScript(serializedObject);

            EditorGUIUtility.labelWidth = 50;

            //--[ Header ]------------------------------
            {
                GUI.backgroundColor = Color.Lerp(Color.black, Color.gray, 1);
                GUILayout.BeginVertical(SE_InspectorTools.GUIStyle_Background, GUILayout.Height(1));
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Layer", SE_InspectorTools.GUIStyle_Header))
                {
                    //Application.OpenURL(SE_InspectorTools.HelpURL_Documentation);
                }

                GUILayout.Space(5);
            }

            mRenderParamsInspector.OnGUI_RenderParams();

            //--[ Footer ]------------------------------
            {
                GUILayout.Space(10);
                GUILayout.FlexibleSpace();

                I2AboutWindow.OnGUI_Footer("I2 SmartEdge", SE_InspectorTools.GetVersion(), SE_InspectorTools.HelpURL_forum, SE_InspectorTools.HelpURL_Documentation);

                EditorGUIUtility.labelWidth = 0;

                GUILayout.EndVertical();
            }

            if (serializedObject.ApplyModifiedProperties() || SERenderParams_Inspector.mMakeMaterialDirty || SERenderParams_Inspector.mMakeVerticesDirty)
            {
                foreach (var t in targets)
                    (t as SmartEdgeRenderPreset).NotifyRenderPresetChanged(SERenderParams_Inspector.mMakeMaterialDirty, SERenderParams_Inspector.mMakeVerticesDirty);
            }
        }
    }
}