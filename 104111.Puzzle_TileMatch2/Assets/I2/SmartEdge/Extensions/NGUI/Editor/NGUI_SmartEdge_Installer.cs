using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace I2.SmartEdge
{
	public class NGUI_SmartEdge_Installer : SE_Extension_Installer
	{
		[MenuItem("Tools/I2 SmartEdge/Apply NGUI Patch", false, 41)]
		public static void OpenNGUIPathScreen()
		{
			EditorWindow.GetWindow<NGUI_SmartEdge_Installer>(true, "SmartEdge-NGUI Patcher", true).Show();
		}

		public override void LoadPatchData()
		{
			ExtensionName = "NGUI";
			ScriptingDefineSymbols = "SE_NGUI";
            DisclaimerMessage = "Requires NGUI 3.11.0 or higher";

            RequiredFile = "UIGeometry";
            RequiredLine = "onCustomWrite";


            PatchData.Clear();

			PatchData.Add(new PatchItem() {
				FileName = "UIWidget",
				ExistingLine = "public class UIWidget : UIRect",
				NewLine = "public partial class UIWidget : UIRect"
			});

			PatchData.Add(new PatchItem() {
				FileName = "UIPanel",
				ExistingLine = "Shader sd = w.shader;",
				NewLine = "Shader sd = w.GetShaderAndMaterial(ref mt); // if you get an error here, add SE_NGUI to the Scripting Define Symbols"
			});

			PatchData.Add(new PatchItem()
			{
				FileName = "UIFont",
				ExistingLine = "public class UIFont : MonoBehaviour",
				NewLine = "public partial class UIFont : MonoBehaviour"
			});

            PatchData.Add(new PatchItem()
            {
                FileName = "NGUIText",
                ExistingLine = "int indexOffset = verts.Count;",
                NewLine = "int indexOffset = verts.Count ; UIWidget.mStringBuilder.Length = 0; // SmartEdge"
            });

            PatchData.Add(new PatchItem()
            {
                FileName = "NGUIText",
                ExistingLine = "verts.Add(new Vector3(v0x, v0y",
                NewLine = "UIWidget.mStringBuilder.Append((char)ch); /*SmartEdge*/ verts.Add (new Vector3(v0x, v0y"
            });

            PatchData.Add(new PatchItem()
            {
                FileName = "NGUIText",
                ExistingLine = "verts.Add(new Vector3(v0x - slant, v0y",
                NewLine = "UIWidget.mStringBuilder.Append((char)ch); /*SmartEdge*/ verts.Add (new Vector3(v0x - slant, v0y"
            });

            PatchData.Add(new PatchItem()
            {
                FileName = "NGUIText",
                ExistingLine = "verts.Add(new Vector3(prevX + a, v0y",
                NewLine = "UIWidget.mStringBuilder.Append((char)ch); /*SmartEdge*/ verts.Add (new Vector3(prevX + a, v0y"
            });

            PatchData.Add(new PatchItem()
            {
                FileName = "NGUIText",
                ExistingLine = "verts.Add(new Vector3(v0x + a - slant, v0y",
                NewLine = "UIWidget.mStringBuilder.Append((char)ch); /*SmartEdge*/ verts.Add (new Vector3(v0x + a - slant, v0y"
            });


            PatchData.Add(new PatchItem()
            {
                FileName = "NGUIText",
                ExistingLine = "verts.Add(new Vector3(prevX, v0y",
                NewLine = "UIWidget.mStringBuilder.Append((char)ch); /*SmartEdge*/ verts.Add (new Vector3(prevX, v0y"
            });

            PatchData.Add(new PatchItem()
            {
                FileName = "NGUIText",
                ExistingLine = "// New line character -- skip to the next line",
                NewLine = "if (ch=='\\n') UIWidget.mStringBuilder.Append((char)ch); //SmartEdge"
            });

            PatchData.Add(new PatchItem()
            {
                FileName = "UILabel",
                ExistingLine = "NGUIText.Print(text, verts, uvs, cols);",
                NewLine = "NGUIText.Print(text, verts, uvs, cols) ; mRealVisualText = mStringBuilder.ToString();"
            });


			PatchData.Add(new PatchItem()
			{
				FileName = "I2NGUI-SmartEdge SDF",
				ExistingLine = "//#pragmaOFF multi_compile",
				NewLine = "#pragma multi_compile"
			});

			PatchData.Add(new PatchItem()
			{
				FileName = "I2NGUI-SmartEdge SDF 1",
				ExistingLine = "//#pragmaOFF multi_compile",
				NewLine = "#pragma multi_compile"
			});

			PatchData.Add(new PatchItem()
			{
				FileName = "I2NGUI-SmartEdge SDF 2",
				ExistingLine = "//#pragmaOFF multi_compile",
				NewLine = "#pragma multi_compile"
			});

			PatchData.Add(new PatchItem()
			{
				FileName = "I2NGUI-SmartEdge SDF 3",
				ExistingLine = "//#pragmaOFF multi_compile",
				NewLine = "#pragma multi_compile"
			});

			PatchData.Add(new PatchItem()
			{
				FileName = "I2NGUI-SmartEdge SDF (TextureClip)",
				ExistingLine = "//#pragmaOFF multi_compile",
				NewLine = "#pragma multi_compile"
			});
		}
	}
}