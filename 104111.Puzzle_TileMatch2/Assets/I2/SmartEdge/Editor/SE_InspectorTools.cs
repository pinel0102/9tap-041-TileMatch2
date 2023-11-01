using UnityEngine;
using System.Collections;
using UnityEditor;

namespace I2.SmartEdge
{ 
	public static class SE_InspectorTools
	{
		public static string HelpURL_forum         = "http://www.inter-illusion.com/forum/i2-smartedge";
		public static string HelpURL_ReleaseNotes  = "http://www.inter-illusion.com/forum/i2-smartedge/187-release-notes";
		public static string HelpURL_Documentation = "http://inter-illusion.com/assets/I2SmartEdgeManual/SmartEdge.html";

		public static string HelpURL_Face            = "http://inter-illusion.com/assets/I2SmartEdgeManual/Face.html";
		public static string HelpURL_Outline         = "http://inter-illusion.com/assets/I2SmartEdgeManual/Stroke.html";
		public static string HelpURL_Glow            = "http://inter-illusion.com/assets/I2SmartEdgeManual/Glow.html";
		public static string HelpURL_Shadow          = "http://inter-illusion.com/assets/I2SmartEdgeManual/Shadow.html";
		public static string HelpURL_InnerShadow     = "http://inter-illusion.com/assets/I2SmartEdgeManual/InnerShadow.html";
		public static string HelpURL_Bevel           = "http://inter-illusion.com/assets/I2SmartEdgeManual/Bevel.html";
		public static string HelpURL_Lighting        = "http://inter-illusion.com/assets/I2SmartEdgeManual/Lighting.html";
		public static string HelpURL_NormalMap       = "http://inter-illusion.com/assets/I2SmartEdgeManual/NormalMap.html";
        public static string HelpURL_Reflection      = "http://inter-illusion.com/assets/I2SmartEdgeManual/FloorReflection.html";
		public static string HelpURL_FloorReflection = "http://inter-illusion.com/assets/I2SmartEdgeManual/FloorReflection.html";
		public static string HelpURL_Layers          = "http://inter-illusion.com/assets/I2SmartEdgeManual/Layers.html";
		public static string HelpURL_Gradients       = "http://inter-illusion.com/assets/I2SmartEdgeManual/GradientEffect.html";

        public static string HelpURL_Deformation         = "http://inter-illusion.com/assets/I2SmartEdgeManual/Deformation.html";

        public static string HelpURL_Text_BestFit        = "http://inter-illusion.com/assets/I2SmartEdgeManual/BestFit.html";
        public static string HelpURL_Text_Spacing        = "http://inter-illusion.com/assets/I2SmartEdgeManual/Spacing.html";
        public static string HelpURL_Text_TextWrapping   = "http://inter-illusion.com/assets/I2SmartEdgeManual/Wrapping.html";
        public static string HelpURL_Text_RichText       = "http://inter-illusion.com/assets/I2SmartEdgeManual/RichText.html";
        


        public static string HelpURL_CreateSDFasset  = "http://inter-illusion.com/assets/I2SmartEdgeManual/CreatingSignalDistanceFieldAsset.html";
        public static string HelpURL_CreateSDFFont   = "http://inter-illusion.com/assets/I2SmartEdgeManual/SDFFontMaker.html";
        public static string HelpURL_CreateSDFImage  = "http://inter-illusion.com/assets/I2SmartEdgeManual/SDFImageMaker.html";
        public static string HelpURL_SDFMethods = "http://inter-illusion.com/assets/I2SmartEdgeManual/WhatSDFFormattouse.html";

        public static string URL_Roadmap = "https://trello.com/b/ImbJ0lQH/i2-smartedge-roadmap";

        public static string GetVersion()
		{
			return "1.0.2 b3";
		}

		#region Styles

		public static GUIStyle GUIStyle_Header
		{
			get
			{
				if (mGUIStyle_Header == null)
				{
					mGUIStyle_Header = new GUIStyle("HeaderLabel");
					mGUIStyle_Header.fontSize = 35;
					mGUIStyle_Header.normal.textColor = Color.Lerp(Color.white, Color.gray, 0.5f);
					mGUIStyle_Header.fontStyle = FontStyle.BoldAndItalic;
					mGUIStyle_Header.alignment = TextAnchor.UpperCenter;
				}
				return mGUIStyle_Header;
			}
		}
		static GUIStyle mGUIStyle_Header;

		public static GUIStyle GUIStyle_SmallHeader
		{
			get
			{
				if (mGUIStyle_SmallHeader == null)
				{
					mGUIStyle_SmallHeader = new GUIStyle("HeaderLabel");
					mGUIStyle_SmallHeader.fontSize = 25;
					mGUIStyle_SmallHeader.normal.textColor = Color.Lerp(Color.white, Color.gray, 0.5f);
					mGUIStyle_SmallHeader.fontStyle = FontStyle.BoldAndItalic;
					mGUIStyle_SmallHeader.alignment = TextAnchor.UpperCenter;
				}
				return mGUIStyle_SmallHeader;
			}
		}
		static GUIStyle mGUIStyle_SmallHeader;
		public static GUIStyle GUIStyle_SubHeader
		{
			get
			{
				if (mGUIStyle_SubHeader == null)
				{
					mGUIStyle_SubHeader = new GUIStyle("HeaderLabel");
					mGUIStyle_SubHeader.fontSize = 13;
					mGUIStyle_SubHeader.fontStyle = FontStyle.Normal;
					mGUIStyle_SubHeader.margin.top = -50;
					mGUIStyle_SubHeader.alignment = TextAnchor.UpperCenter;
				}
				return mGUIStyle_SubHeader;
			}
		}
		static GUIStyle mGUIStyle_SubHeader;

		public static GUIStyle GUIStyle_Background
		{
			get
			{
				if (mGUIStyle_Background == null)
				{
					mGUIStyle_Background = new GUIStyle("AS TextArea");
					mGUIStyle_Background.overflow.left = 50;
					mGUIStyle_Background.overflow.right = 50;
					mGUIStyle_Background.overflow.top = -5;
					mGUIStyle_Background.overflow.bottom = 0;
				}
				return mGUIStyle_Background;
			}
		}
		static GUIStyle mGUIStyle_Background;

        static public GUIStyle Style_LabelRightAligned
        {
            get
            {
                if (mStyle_LabelRightAligned == null)
                {
                    mStyle_LabelRightAligned = new GUIStyle("label");
                    mStyle_LabelRightAligned.alignment = TextAnchor.MiddleRight;
                }
                return mStyle_LabelRightAligned;
            }
        }
        static GUIStyle mStyle_LabelRightAligned;

        static public GUIStyle Style_LabelCenterAligned
        {
            get
            {
                if (mStyle_LabelCenterAligned == null)
                {
                    mStyle_LabelCenterAligned = new GUIStyle("label");
                    mStyle_LabelCenterAligned.alignment = TextAnchor.MiddleCenter;
                }
                return mStyle_LabelCenterAligned;
            }
        }
        static GUIStyle mStyle_LabelCenterAligned;


        static public GUIStyle Style_LabelItalic
        {
            get
            {
                if (mStyle_LabelItalic == null)
                {
                    mStyle_LabelItalic = new GUIStyle("label");
                    mStyle_LabelItalic.fontStyle = FontStyle.Italic;
                }
                return mStyle_LabelItalic;
            }
        }
        static GUIStyle mStyle_LabelItalic;

        #endregion

        [MenuItem("Tools/I2 SmartEdge/Help", false, 92)]
        [MenuItem("Help/I2 SmartEdge")]
        public static void MainHelp()
        {
            Application.OpenURL(SE_InspectorTools.HelpURL_Documentation);
        }

        [MenuItem("Tools/I2 SmartEdge/About", false, 93)]
        public static void AboutWindow()
        {
            I2AboutWindow.DoShowScreen();
        }
    }
}