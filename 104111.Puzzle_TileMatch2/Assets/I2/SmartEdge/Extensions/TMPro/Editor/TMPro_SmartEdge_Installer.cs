using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace I2.SmartEdge
{
    public class TMPro_SmartEdge_Installer : SE_Extension_Installer
    {
        [MenuItem("Tools/I2 SmartEdge/Apply TextMeshPro Patch", false, 42)]
        public static void OpenTMProPathScreen()
        {
            EditorWindow.GetWindow<TMPro_SmartEdge_Installer>(true, "SmartEdge-TextMeshPro Patcher", true).Show();
        }

        public override void LoadPatchData()
        {
            ExtensionName = "TMPro";
            ScriptingDefineSymbols = "SE_TMPro";

            PatchData.Clear();

            PatchData.Add(new PatchItem()
            {
                FileName = "TMPro_UGUI_Private",
                ExistingLine = "// Upload Mesh Data",
                NewLine = "Call_OnPreUploadMeshData();// Call event and then Upload Mesh Data"
            });
        }
    }
}