using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace I2.SmartEdge
{
    #if UNITY_5_3_OR_NEWER || UNITY_5_3 || UNITY_5_2
    public partial class SmartEdge : IMeshModifier, IMaterialModifier
	{
        public void ModifyMesh(Mesh mesh)
        {
            if (!enabled) return;

            //if (!this.IsActive()) return;

            using (VertexHelper vertexHelper = new VertexHelper(mesh))
            {
                this.ModifyMesh(vertexHelper);
                vertexHelper.FillMesh(mesh);
            }
        }

        public void ModifyMesh (VertexHelper vh)
		{
            if (!isActiveAndEnabled) return;

			//--[ Cache ]------------
			if (mGraphic==null)	mGraphic=GetComponent<Graphic>();
			if (mGraphic==null)	return;

			if (mRectTransform==null) mRectTransform = transform as RectTransform;
			if (mRectTransform==null) return;

            if (vh.currentVertCount == 0)
                return;

			mRect = mRectTransform.rect;
			mRectPivot = mRectTransform.pivot;
			mWidgetColor = mGraphic.color;

            if (vh.currentVertCount<=0 || (mWidgetColor.a < 1/255f && GetPlayingAnimation()==null))
                return;

            ImportVerticesFromUGUI(vh);

            Setup((mGraphic && mGraphic.mainTexture) ? mGraphic.mainTexture as Texture2D : null);

            if (mTestPerformance)
            { 
                for (int i = 0; i <50; ++i)
			        ModifyVertices ();
            }
            else
                ModifyVertices();

            ExportVerticesToUGUI(vh);

        }

        private void ImportVerticesFromUGUI(VertexHelper vh)
        {
            Text label = mGraphic as Text;
            mCharacterSize = (label == null ? 1 : (float)label.fontSize);
            mLineHeight = mCharacterSize;
            var fontScale = (label == null ? 1 : (mCharacterSize / (float)label.font.fontSize));
            var ascender = (label == null ? mLineHeight : (label.font.ascent*fontScale));

            string strText = (label==null ? null : label.text);
            float Pixel2Units = (label == null ? 1 : 1/label.pixelsPerUnit);

            #if !UNITY_5_3_3 && !UNITY_5_4_OR_NEWER
	            float yOffset = (label != null && label.font != null) ? -label.font.ascent * (mCharacterSize/(float)label.font.fontSize) : 0;
            #else
                float yOffset = 0;
            #endif

            if (mSETextureFormat==SETextureFormat.Unknown)
                yOffset = 0;

            //--[ Populate Original Mesh ]-----------------

            for (int layer = 0; layer<mSEMesh.numLayers; ++layer)
                mSEMesh.mLayers[layer].Clear();
            mOriginalVertices = mSEMesh.mLayers[(int)SEVerticesLayers.Original];


            mOriginalVertices.Reset(vh.currentVertCount);
            mCharacters.Reset(mOriginalVertices.Size / 4);

            mAllCharactersMin = MathUtils.v2max;
            mAllCharactersMax = MathUtils.v2min;
            var cIndex = 0;

            int iLine = 0, nCharactersInLine=0;
            int iWord = 0, nWordsInLine = 0;
            int iParagraph = 0;
            bool inWord = false;
            IList<UILineInfo> lineInfos = null;

            if (label != null)
            {
                if (label.cachedTextGeneratorForLayout.characterCount != strText.Length)
                    label.cachedTextGeneratorForLayout.Populate(strText, label.GetGenerationSettings(mRect.size));
                lineInfos = label.cachedTextGeneratorForLayout.lines;

                if (lineInfos.Count>0)
                    mLineHeight = lineInfos[0].height*Pixel2Units;
            }
            //else
              //  mLines.Clear();

            var iFirstCharOfNextLine = (lineInfos == null || lineInfos.Count <= 1) ? int.MaxValue : lineInfos[1].startCharIdx;
            float lineHeight = (lineInfos == null || lineInfos.Count <= 0) ? mLineHeight : lineInfos[0].height*Pixel2Units;
            float linePosY = (lineInfos == null || lineInfos.Count <= 0) ? 0 : lineInfos[0].topY * Pixel2Units;

            // The array is cleared to initialize all RichText values
            System.Array.Clear(mCharacters.Buffer, 0, mCharacters.Size);

            for (int c = 0; c < mCharacters.Size; ++c)
            {
                mCharacters.Buffer[cIndex].Min = MathUtils.v2max;
                mCharacters.Buffer[cIndex].Max = MathUtils.v2min;

                for (int i = 0; i < 4; i++)
                {
                    vh.PopulateUIVertex(ref uiVertex, c * 4 + i);
	                uiVertex.position.y += yOffset;

                    seVertex.position    = uiVertex.position;
                    seVertex.color       = uiVertex.color;
                    seVertex.uv          = uiVertex.uv0;
                    seVertex.normal      = uiVertex.normal;
                    seVertex.tangent     = uiVertex.tangent;
                    seVertex.characterID = cIndex;

                    if (mCharacters.Buffer[cIndex].Min.x > uiVertex.position.x) mCharacters.Buffer[cIndex].Min.x = uiVertex.position.x;
                    if (mCharacters.Buffer[cIndex].Min.y > uiVertex.position.y) mCharacters.Buffer[cIndex].Min.y = uiVertex.position.y;
                    if (mCharacters.Buffer[cIndex].Max.x < uiVertex.position.x) mCharacters.Buffer[cIndex].Max.x = uiVertex.position.x;
                    if (mCharacters.Buffer[cIndex].Max.y < uiVertex.position.y) mCharacters.Buffer[cIndex].Max.y = uiVertex.position.y;

                    mOriginalVertices.Buffer[cIndex*4 + i] = seVertex;
                }

                char chr = strText == null ? (char)0 : strText[c];
                bool isWhiteSpace = char.IsWhiteSpace(chr);
                if (inWord && (isWhiteSpace || c >= iFirstCharOfNextLine))  // is a new word if we reach a whitespace or started another line
                {
                    iWord++;
                    nWordsInLine++;
                }
                inWord = !isWhiteSpace;
                if (chr == '\n')
                    iParagraph++;

                while (c >= iFirstCharOfNextLine)
                {
                    iLine++;
                    nCharactersInLine = 0;
                    nWordsInLine = 0;
                    iFirstCharOfNextLine = (lineInfos.Count <= (iLine + 1)) ? int.MaxValue : lineInfos[iLine + 1].startCharIdx;

                    if (lineInfos != null && lineInfos.Count > iLine)
                    {
                        lineHeight = lineInfos[iLine].height * Pixel2Units;
                        linePosY = lineInfos[iLine].topY * Pixel2Units;
                    }
                }

                if (mCharacters.Buffer[cIndex].Min.x < mCharacters.Buffer[cIndex].Max.x && mCharacters.Buffer[cIndex].Min.y < mCharacters.Buffer[cIndex].Max.y)
                {
                    if (mCharacters.Buffer[cIndex].Min.x < mAllCharactersMin.x) mAllCharactersMin.x = mCharacters.Buffer[cIndex].Min.x;
                    if (mCharacters.Buffer[cIndex].Min.y < mAllCharactersMin.y) mAllCharactersMin.y = mCharacters.Buffer[cIndex].Min.y;
                    if (mCharacters.Buffer[cIndex].Max.x > mAllCharactersMax.x) mAllCharactersMax.x = mCharacters.Buffer[cIndex].Max.x;
                    if (mCharacters.Buffer[cIndex].Max.y > mAllCharactersMax.y) mAllCharactersMax.y = mCharacters.Buffer[cIndex].Max.y;

                    mCharacters.Buffer[cIndex].Character = chr;
                    mCharacters.Buffer[cIndex].iLine = iLine;
                    mCharacters.Buffer[cIndex].iCharacterInText = cIndex;
                    mCharacters.Buffer[cIndex].iCharacterInLine = nCharactersInLine++;
                    mCharacters.Buffer[cIndex].iWord = iWord;
                    mCharacters.Buffer[cIndex].iWordInLine = nWordsInLine;
                    mCharacters.Buffer[cIndex].iParagraph = iParagraph;
                    mCharacters.Buffer[cIndex].TopY = linePosY;
                    mCharacters.Buffer[cIndex].BaselineY = linePosY - ascender;
                    mCharacters.Buffer[cIndex].Height = lineHeight;

                    cIndex++;
                }
            }
            mCharacters.Size = cIndex;
            mOriginalVertices.Size = cIndex*4;
        }

        private void ExportVerticesToUGUI(VertexHelper vh)
        {
            vh.Clear();
            var ioffset = 0;
            for (int layer = 0; layer<mSEMesh.numLayers; ++layer)
            {
                var layerVertices = mSEMesh.mLayers[layer];

                for (int i = 0; i < layerVertices.Size; ++i)
                    vh.AddVert(layerVertices.Buffer[i].position, layerVertices.Buffer[i].color, layerVertices.Buffer[i].uv, layerVertices.Buffer[i].uv1, layerVertices.Buffer[i].normal, layerVertices.Buffer[i].tangent);

                for (int i = 0; i < layerVertices.Size; i += 4)
                {
                    vh.AddTriangle(ioffset + i, ioffset + i + 1, ioffset + i + 2);
                    vh.AddTriangle(ioffset + i + 2, ioffset + i + 3, ioffset + i);
                }

                ioffset += layerVertices.Size;
            }
        }



#else
    public partial class SmartEdge
    {
    #endif
        [TextArea(3, 10)]public string mOriginalText;        // This is the unprocessed text with all the Rich Text Tags
        [NonSerialized]  public string mFinalText;           // Modified text, (removed Rich Text Tags, Changed Case Type, etc), On Unity UI, this text should match the UI.Text.text

        [NonSerialized] public bool mTestPerformance = false;

        [NonSerialized]public Graphic mGraphic;
		RectTransform mRectTransform;
		static UIVertex uiVertex = default(UIVertex);

        static Shader pShader_SDF_UGUI { 
			get{ 
				if (mShader_SDF_UGUI==null)
                    mShader_SDF_UGUI = Shader.Find("GUI/I2 SmartEdge/SDF");
                  //mShader_SDF_UGUI = Shader.Find("GUI/I2 SmartEdge/Surf SDF");
                return mShader_SDF_UGUI;
			}
		}
		static Shader mShader_SDF_UGUI;

		public void MarkWidgetAsChanged(bool MarkVertices=true, bool MarkMaterial=false)
		{
            mIsDirty_Material |= MarkMaterial;
            //mIsDirty_Vertices |= MarkVertices;

            if (mGraphic==null)	mGraphic=GetComponent<Graphic>();
            if (mGraphic != null)
            {
                if (MarkVertices && MarkMaterial)
                    mGraphic.SetAllDirty();
                else
                {
                    if (MarkMaterial)
                        mGraphic.SetMaterialDirty();

                    if (MarkVertices)
                    {
                        mGraphic.SetVerticesDirty();
                        //mGraphic.SetLayoutDirty();
                    }
                }
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.EditorUtility.SetDirty(this);
                //mGraphic.Rebuild(CanvasUpdate.PreRender);
#endif
            }

#if SE_NGUI
            MarkWidgetAsChanged_NGUI(MarkVertices, MarkMaterial);
#endif
#if SE_TMPro
            MarkWidgetAsChanged_TMPro(MarkVertices, MarkMaterial);
#endif
        }

        public Material GetModifiedMaterial(Material baseMaterial)
		{
			var mainTexture = mGraphic ? mGraphic.mainTexture as Texture2D : null;
            if (mGraphic) mWidgetColor = mGraphic.color;
			var newMat = GetMaterial(baseMaterial, mainTexture, pShader_SDF_UGUI);

            if (baseMaterial != null && newMat != null && mSETextureFormat != SETextureFormat.Unknown && baseMaterial.HasProperty("_Stencil"))
            {
                newMat.SetInt("_Stencil", baseMaterial.GetInt("_Stencil"));
                newMat.SetInt("_StencilOp", baseMaterial.GetInt("_StencilOp"));
                newMat.SetInt("_StencilComp", baseMaterial.GetInt("_StencilComp"));
                newMat.SetInt("_StencilReadMask", baseMaterial.GetInt("_StencilReadMask"));
                newMat.SetInt("_StencilWriteMask", baseMaterial.GetInt("_StencilWriteMask"));
                newMat.SetInt("_ColorMask", baseMaterial.GetInt("_ColorMask"));
                if (newMat.HasProperty("_UseAlphaClip"))
                    newMat.SetInt("_UseAlphaClip", baseMaterial.GetInt("_UseAlphaClip"));
                bool useAlphaClip = System.Array.IndexOf(baseMaterial.shaderKeywords, "_UseAlphaClip") >= 0;
                if (useAlphaClip)
                {
                    newMat.EnableKeyword("UNITY_UI_ALPHACLIP");
                }
                else
                {
                    newMat.DisableKeyword("UNITY_UI_ALPHACLIP");
                }
            }
            return newMat;
        }

        bool UpdateUGUIspread()
        {
            var text = mGraphic as Text;
            if (text == null)
                return false;

            Material fontDataMat = null;
            if (text != null && text.font != null)
                fontDataMat = text.font.material;

            if (fontDataMat != null)
            {
                if (fontDataMat.HasProperty(matParam_Spread))
                    mSpread = fontDataMat.GetFloat(matParam_Spread);
            }
            return true;
        }

        void OnEnableUGUI()
        {
            if (mGraphic == null) mGraphic = GetComponent<Graphic>();
            if (mGraphic == null) return;

            var label = mGraphic as Text;
            if (label != null)
                label.RegisterDirtyVerticesCallback( ValidateSettings );
        }

        void OnDisableUGUI()
        {
            var label = mGraphic as Text;
            if (label != null)
                label.UnregisterDirtyVerticesCallback(ValidateSettings);
        }


        //HACK until a better way to detect a font changed
        int mHack_LastValidFontSize=14;
        void ValidateSettingsUGUI()
        {
            var text = mGraphic as Text;
            if (text != null)
            {
                // if any SDF format, then bestFit should use the Deform's BestFit effect
                if (_TextEffect._BestFit._Enabled || (text.resizeTextForBestFit && mSETextureFormat!=SETextureFormat.Unknown))
                {
                    _TextEffect._BestFit._Enabled = true;
                    text.resizeTextForBestFit = false;
                }

                // Fix that SDF fonts (that come with 150 as the font size) should keep using the original fontsize of this text
                var fontSize = text.fontSize;
                if (fontSize != mHack_LastValidFontSize)
                {
                    if (fontSize == 150 && text.font!=null)
                    {
                        text.fontSize = mHack_LastValidFontSize;
                    }
                    else
                        mHack_LastValidFontSize = fontSize;
                }

                // if TextWrapping is enabled, make the text to use overflow, and pass its original value to the textwrapping (e.g. truncate/wrap)
                if (_TextEffect._TextWrapping._Enabled)
                {
                    if (text.verticalOverflow != VerticalWrapMode.Overflow)
                    {
                        if (text.verticalOverflow == VerticalWrapMode.Truncate) _TextEffect._TextWrapping._VerticalWrapMode = SE_TextEffect_TextWrapping.eVerticalWrapMode.Truncate;
                        text.verticalOverflow = VerticalWrapMode.Overflow;
                    }

                    if (text.horizontalOverflow != HorizontalWrapMode.Overflow)
                    {
                        if (text.horizontalOverflow == HorizontalWrapMode.Wrap) _TextEffect._TextWrapping._HorizontalWrapMode = SE_TextEffect_TextWrapping.eHorizontalWrapMode.Wrap;
                        text.horizontalOverflow = HorizontalWrapMode.Overflow;
                    }


                    var horzAlignment = GetHorizontalAlignment_UGUI();
                    var vertAlignment = GetVerticalAlignment_UGUI();
                    if (horzAlignment != eHorizontalAlignment.Left || vertAlignment!=eVerticalAlignment.Top)
                    {
                        if (horzAlignment != eHorizontalAlignment.Left) _TextEffect._TextWrapping._HorizontalAlignment = horzAlignment;
                        if (vertAlignment != eVerticalAlignment.Top)    _TextEffect._TextWrapping._VerticalAlignment   = vertAlignment;
                        text.alignment = TextAnchor.UpperLeft;
                    }

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
                    text.alignByGeometry = true;
#endif
                }

                string labelText = text.text;
                if (labelText != mFinalText || string.IsNullOrEmpty(mOriginalText))
                {
                    // A recompile cleared the finalText
                    if (string.IsNullOrEmpty(mFinalText) && !string.IsNullOrEmpty(mOriginalText))
                    {
                        mFinalText = mOriginalText;
                        text.text = mOriginalText;
                    }
                    else // The label text was changed, so use that instead
                    {
                        mOriginalText = mFinalText = labelText;
                    }

                    if (ModifyText(ref mFinalText))
                    {
                        text.text = mFinalText;
                    }
                }

            }
        }

        // When useVertices==true, it updates the Character min and max from its vertices and then updates the mAllCharacterMinMax
        // otherwise, it just updates the mAllCharacterMinMax from the min/max of each character
        public void UpdateCharactersMinMax( bool useVertices=true )
        {
            if (useVertices)
            {
                for (var i = 0; i < mOriginalVertices.Size; i++)
                {
                    var cIndex = i / 4;
                    if (i % 4 == 0)
                    {
                        mCharacters.Buffer[cIndex].Min = MathUtils.v2max;
                        mCharacters.Buffer[cIndex].Max = MathUtils.v2min;
                    }
                    var pos = mOriginalVertices.Buffer[i].position;

                    if (mCharacters.Buffer[cIndex].Min.x > pos.x) mCharacters.Buffer[cIndex].Min.x = pos.x;
                    if (mCharacters.Buffer[cIndex].Min.y > pos.y) mCharacters.Buffer[cIndex].Min.y = pos.y;
                    if (mCharacters.Buffer[cIndex].Max.x < pos.x) mCharacters.Buffer[cIndex].Max.x = pos.x;
                    if (mCharacters.Buffer[cIndex].Max.y < pos.y) mCharacters.Buffer[cIndex].Max.y = pos.y;
                }
            }

            mAllCharactersMin = MathUtils.v2max;
            mAllCharactersMax = MathUtils.v2min;
            for (var c = 0; c < mCharacters.Size; c++)
            {
                if (mCharacters.Buffer[c].Min.x < mAllCharactersMin.x) mAllCharactersMin.x = mCharacters.Buffer[c].Min.x;
                if (mCharacters.Buffer[c].Min.y < mAllCharactersMin.y) mAllCharactersMin.y = mCharacters.Buffer[c].Min.y;
                if (mCharacters.Buffer[c].Max.x > mAllCharactersMax.x) mAllCharactersMax.x = mCharacters.Buffer[c].Max.x;
                if (mCharacters.Buffer[c].Max.y > mAllCharactersMax.y) mAllCharactersMax.y = mCharacters.Buffer[c].Max.y;
            }

        }

        eHorizontalAlignment GetHorizontalAlignment_UGUI()
        {
            var text = mGraphic as UnityEngine.UI.Text;
            if (text == null) return eHorizontalAlignment.Center;

            switch (text.alignment)
            {
                case TextAnchor.LowerLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.UpperLeft:
                    return eHorizontalAlignment.Left;

                case TextAnchor.LowerRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.UpperRight:
                    return eHorizontalAlignment.Right;

                default:
                    return eHorizontalAlignment.Center;
            }
        }

        eVerticalAlignment GetVerticalAlignment_UGUI()
        {
            var text = mGraphic as UnityEngine.UI.Text;
            if (text == null) return eVerticalAlignment.Center;

            switch (text.alignment)
            {
                case TextAnchor.LowerLeft:
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerRight:
                    return eVerticalAlignment.Bottom;

                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    return eVerticalAlignment.Top;

                default:
                    return eVerticalAlignment.Center;
            }

            
        }

        void SetWidgetColor_UGUI( Color32 color )
        {
            if (mGraphic!=null)
                mGraphic.color = color;
        }
    }
}
