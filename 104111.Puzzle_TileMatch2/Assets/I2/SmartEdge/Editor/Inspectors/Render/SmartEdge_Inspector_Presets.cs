using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System;

namespace I2.SmartEdge
{
	public partial class SmartEdge_Inspector
    {
        SerializedProperty mSerialized_RenderPresets;
        SerializedObject mSerializedObject_RenderPreset;

        SERenderParams_Inspector mRenderParamsInspector;
        SmartEdgeRenderPreset mRenderPreset;

        void RegisterProperty_Presets()
		{
            mSerialized_RenderPresets = serializedObject.FindProperty("_RenderPresets");
            mRenderPreset = null;
            if (mSerialized_RenderPresets.arraySize > 0)
            {
                var propRenderPreset = mSerialized_RenderPresets.GetArrayElementAtIndex(0);
                if (propRenderPreset.objectReferenceValue == null)
                {
                    mSerializedObject_RenderPreset = null;
                }
                else
                {
                    mRenderPreset = propRenderPreset.objectReferenceValue as SmartEdgeRenderPreset;
                    mSerializedObject_RenderPreset = new SerializedObject(mRenderPreset);
                    mRenderParamsInspector = new SERenderParams_Inspector(mSerializedObject_RenderPreset.FindProperty("_RenderParams"), mRenderPreset._RenderParams, mTarget);
                }
            }
            else
                mSerializedObject_RenderPreset = null;

            if (mSerializedObject_RenderPreset == null)
                mRenderParamsInspector = new SERenderParams_Inspector(serializedObject.FindProperty("_RenderParams"), mTarget._RenderParams, mTarget);


            UpdateValidPresets();
		}

        void SetRenderParams( SmartEdgeRenderPreset newPreset )
        {
            if (newPreset == mRenderPreset)
                return;

            if (mRenderPreset!=null)
            {
                foreach (var se in targets)
                    mRenderPreset.UnRegisterRenderPresetDependency(se as SmartEdge);
            }

            int index = 0;

            if (newPreset == null)
            {
                if (mSerialized_RenderPresets.arraySize == index + 1)
                    mSerialized_RenderPresets.DeleteArrayElementAtIndex(index);
                else
                if (mSerialized_RenderPresets.arraySize > index + 1)
                    mSerialized_RenderPresets.GetArrayElementAtIndex(index).objectReferenceValue = null;
            }

            if (newPreset != null)
            {
                if (mSerialized_RenderPresets.arraySize <= index)
                    mSerialized_RenderPresets.arraySize = index + 1;

                mSerialized_RenderPresets.GetArrayElementAtIndex(index).objectReferenceValue = newPreset;

                foreach (var se in targets)
                    newPreset.RegisterRenderPresetDependency(se as SmartEdge);
            }


            mMakeVerticesDirty = mMakeMaterialDirty = true;
            RegisterProperty_Presets();
        }

        string[] mValidPresets;
		string mPresetsPath;

		void OnGUI_Presets ()
		{
			EditorGUIUtility.labelWidth = 60;

            //--[ update first element in the mValidPresets array for the Popup to show the current element ]----------
            var presetIdx = 0;
            mValidPresets[1] = "";

            if (mRenderPreset!=null)
            {
                var assetPath = AssetDatabase.GetAssetPath(mRenderPreset);
                presetIdx = System.Array.FindIndex(mValidPresets, 3, x => assetPath.EndsWith(x, System.StringComparison.Ordinal));
                if (presetIdx > 0)
                {
                    mValidPresets[1] = mRenderPreset.name;
                    presetIdx = 1;
                }
            }


            GUITools.BeginContents();
            GUILayout.Space(2);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
                GUILayout.Toggle(true, "Presets", EditorStyles.foldout, GUILayout.Width(60));
                int newPresetIdx = EditorGUILayout.Popup(presetIdx, mValidPresets);
                if (newPresetIdx != presetIdx)
                {
                    if (newPresetIdx < 2)
                    {
                        SetRenderParams(null);
                    }
                    else
                    {
                        var assetPath = mPresetsPath + mValidPresets[newPresetIdx];
                        SetRenderParams(AssetDatabase.LoadAssetAtPath(assetPath, typeof(SmartEdgeRenderPreset)) as SmartEdgeRenderPreset);
                    }
                }

                if (GUILayout.Button(new GUIContent("\u21bb", "Updates the preset list with the Assets in the folder 'Assets/I2/SmartEdge/Render Presets'"), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
                    UpdateValidPresets();
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
                GUILayout.Space(68);

                var newPreset = EditorGUILayout.ObjectField(mRenderPreset, typeof(SmartEdgeRenderPreset), false) as SmartEdgeRenderPreset;
                if (newPreset != mRenderPreset)
                    SetRenderParams(newPreset);

                //GUI.enabled = hasPreset;
                if (GUILayout.Button(new GUIContent("Custom", "Not longer uses the RenderPreset"), EditorStyles.miniButtonMid, GUILayout.Width(50)))
                {
                    serializedObject.ApplyModifiedProperties();
                    if (mRenderPreset != null)
                    {
                        foreach (var se in targets)
                            (se as SmartEdge)._RenderParams = mRenderPreset._RenderParams.Clone();
                    }
                    serializedObject.Update();
                    SetRenderParams(null);
                }
                //GUI.enabled = true;
                if (GUILayout.Button(new GUIContent("Reset", "Create a new preset with the default values"), EditorStyles.miniButtonMid, GUILayout.Width(50)))
                {
                    SetRenderParams(null);
                    serializedObject.ApplyModifiedProperties();
                    foreach (var se in targets)
                        (se as SmartEdge)._RenderParams = new SmartEdgeRenderParams();
                    mMakeVerticesDirty = mMakeMaterialDirty = true;
                    serializedObject.Update();
                    RegisterProperty_Presets();
                }

                if (GUILayout.Button(new GUIContent("Save", "Saves the Preset in the folder 'Assets/I2/SmartEdge/Render Presets'"), EditorStyles.miniButtonRight, GUILayout.Width(50)))
                    CreateNewRenderPreset();
            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                mMakeVerticesDirty = true;
                mMakeMaterialDirty = true;
            }

            GUITools.EndContents(false);
        }

        void CreateNewRenderPreset()
        {
            serializedObject.ApplyModifiedProperties();

            mPresetsPath = GetI2SmartEdgePath() + "/Presets/Render/";
            string FileName = EditorUtility.SaveFilePanelInProject("Save As", "RenderPreset.asset", "asset", "Save RenderPreset", mPresetsPath);
            if (string.IsNullOrEmpty(FileName))
                return;

            var preset = ScriptableObject.CreateInstance(typeof(SmartEdgeRenderPreset)) as SmartEdgeRenderPreset;
            preset._RenderParams = mTarget.GetRenderParams();
            AssetDatabase.CreateAsset(preset, FileName);
            AssetDatabase.SaveAssets();

            SetRenderParams(preset);

            OnEnable();
        }

        void UpdateValidPresets()
		{
            mPresetsPath = GetI2SmartEdgePath() + "/Presets/Render/";
			var len = mPresetsPath.Length;
			var header = new string[]{ "None", "<custom>", "" };
            
			mValidPresets = header.Union(System.IO.Directory.GetFiles (mPresetsPath, "*.asset", System.IO.SearchOption.AllDirectories).Select(x=>x.Substring(len).Replace('\\', '/'))).ToArray();
		}
	}
}