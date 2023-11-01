using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
    public partial class SERenderParams_Inspector
    {
        public SerializedProperty mSerialized_RenderParams;
        public SmartEdgeRenderParams mRenderParams;
        public SmartEdge mSmartEdge;

        public static bool mMakeMaterialDirty, mMakeVerticesDirty;


        public enum SmartEdgeRenderEffect { Color, Outline, Glow, Shadow, InnerShadow, Bevel, NormalMap, Reflection, FloorReflection }
        public static SmartEdgeRenderEffect mSelectedRenderEffect = SmartEdgeRenderEffect.Color;


        public SERenderParams_Inspector( SerializedProperty prop_renderParams, SmartEdgeRenderParams renderParams, SmartEdge smartEdge )
        {
            mRenderParams            = renderParams;
            mSerialized_RenderParams = prop_renderParams;
            mSmartEdge               = smartEdge;

            RegisterProperty_Face();
            RegisterProperty_Outline();
            RegisterProperty_Bevel();
            RegisterProperty_Lighting();
            RegisterProperty_NormalMap();
            RegisterProperty_Glow();
            RegisterProperty_Shadow();
            RegisterProperty_InnerShadow();
            RegisterProperty_Reflection();
            RegisterProperty_FloorReflection();
        }


        public void OnGUI_RenderParams()
        {
            mMakeMaterialDirty = mMakeVerticesDirty = false;

            EditorGUIUtility.labelWidth = 50;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(50));

            OnGUI_LeftTab("Color", SmartEdgeTools.SmartEdgeSkin.FindStyle("Button_Color").normal.background, SmartEdgeRenderEffect.Color, mSerialized_EnableFace.boolValue, EnableFace, "Face Color");
            OnGUI_LeftTab("Outline", SmartEdgeTools.SmartEdgeSkin.FindStyle("Button_Outline").normal.background, SmartEdgeRenderEffect.Outline, mSerialized_EnableOutline.boolValue, EnableOutline, "Outline");
            OnGUI_LeftTab("Glow", SmartEdgeTools.SmartEdgeSkin.FindStyle("Button_Glow").normal.background, SmartEdgeRenderEffect.Glow, mSerialized_EnableGlow.boolValue, EnableGlow, "Glow");
            OnGUI_LeftTab("Shadow", SmartEdgeTools.SmartEdgeSkin.FindStyle("Button_Shadow").normal.background, SmartEdgeRenderEffect.Shadow, mSerialized_EnableShadow.boolValue, EnableShadows, "Shadow");
            OnGUI_LeftTab("Inner\nShadow", SmartEdgeTools.SmartEdgeSkin.FindStyle("Button_InnerShadow").normal.background, SmartEdgeRenderEffect.InnerShadow, mSerialized_EnableInnerShadows.boolValue, EnableInnerShadows, "Inner Shadow");
            OnGUI_LeftTab("Bevel", SmartEdgeTools.SmartEdgeSkin.FindStyle("Button_Bevel").normal.background, SmartEdgeRenderEffect.Bevel, mSerialized_EnableBevel.boolValue, EnableBevel, "Bevel");
            OnGUI_LeftTab("Normals", SmartEdgeTools.SmartEdgeSkin.FindStyle("Button_NormalMap").normal.background, SmartEdgeRenderEffect.NormalMap, mNormalMapDef.mSerialized_Enable.boolValue, EnableNormalMap, "Normal Map");
            OnGUI_LeftTab("Reflection", SmartEdgeTools.SmartEdgeSkin.FindStyle("Button_Reflection").normal.background, SmartEdgeRenderEffect.Reflection, mSerialized_EnableReflection.boolValue, EnableReflection, "Reflection");
            OnGUI_LeftTab("Floor", SmartEdgeTools.SmartEdgeSkin.FindStyle("Button_FloorReflection").normal.background, SmartEdgeRenderEffect.FloorReflection, mSerialized_EnableFloorReflection.boolValue, EnableFloorReflection, "Floor Reflection");
            GUILayout.EndVertical();

            GUILayout.Space(-3);
            GUILayout.BeginVertical("AS TextArea");
            switch (mSelectedRenderEffect)
            {
                case SmartEdgeRenderEffect.Color: OnGUI_Face(); break;
                case SmartEdgeRenderEffect.Outline: OnGUI_Outline(); break;
                case SmartEdgeRenderEffect.Glow: OnGUI_Glow(); break;
                case SmartEdgeRenderEffect.Shadow: OnGUI_Shadow(); break;
                case SmartEdgeRenderEffect.InnerShadow: OnGUI_InnerShadow(); break;
                case SmartEdgeRenderEffect.Bevel: OnGUI_Bevel(); GUILayout.Space(5); OnGUI_Lighting(); break;
                case SmartEdgeRenderEffect.NormalMap: OnGUI_NormalMap(); GUILayout.Space(5); OnGUI_Lighting(); break;
                case SmartEdgeRenderEffect.Reflection: OnGUI_Reflection(); break;
                case SmartEdgeRenderEffect.FloorReflection: OnGUI_FloorReflection(); break;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        void OnGUI_LeftTab(string text, Texture image, SmartEdgeRenderEffect effect, bool used, System.Action<bool> OnToggle, string tooltip)
        {
            bool selected = mSelectedRenderEffect == effect;

            var rect = GUILayoutUtility.GetRect(50, 50);
            var rToggle = Rect.MinMaxRect(rect.xMin, rect.yMin + rect.height * 0.4f - 8, rect.xMin+14, rect.yMin + rect.height * 0.4f + 8);

            var rButton = rect;
            //if (!selected) rButton.xMin += 5;

            var orgColor = GUI.color;
            var newColor = orgColor;
            newColor.a *= (selected ? 1f : used ? 0.3f : 0/*0.05f*/);

            GUI.color = newColor;
            GUI.Label(rButton, "", "ButtonLeft");
            GUI.color = orgColor;


            var newUsed = GUI.Toggle(rToggle, used, "");
            if (newUsed != used)
                OnToggle(newUsed);

            //GUI.color = newColor;
            if (GUI.Button(rButton, "", EditorStyles.label)) mSelectedRenderEffect = effect;
            //if (GUI.Button(rButton, "", "ButtonLeft")) mSelectedRenderEffect = effect;

            var rect1 = Rect.MinMaxRect(rect.xMin, rect.yMax - 20, rect.xMax, rect.yMax);
            GUI.contentColor = GUITools.LightGray;
            var labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.fontSize = 8;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(rect1, text, labelStyle);

            GUI.color = orgColor;
            rect = Rect.MinMaxRect(rect.xMin + 12, rect.yMin, rect.xMax - 1, rect.yMax - 12);
            //rect = Rect.MinMaxRect(rect.xMin + 6, rect.yMin + 6, rect.xMax - 6, rect.yMax - 6);
            GUI.Label(rect, new GUIContent(image, tooltip));
        }  
    }
}