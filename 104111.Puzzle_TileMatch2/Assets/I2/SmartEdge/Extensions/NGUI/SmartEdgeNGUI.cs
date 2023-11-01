using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

#if SE_NGUI


/**************************************************************
 * If you are getting errors in this file:
 * 
 * - Check that you have NGUI installed in your project
 * - If you don't have NGUI installed, then remove SE_NGUI from 
 *      your "Scripting Define Symbols"  (Unity Editor Menu => Edit \ Project Settings \ Player)
 *      
 *  HELP: http://inter-illusion.com/assets/I2SmartEdgeManual/NGUIIntegration.html    
 *      
 ***************************************************************/


namespace I2.SmartEdge
{
    public partial class SmartEdge
    {
        [System.NonSerialized]
        public UIWidget mNGUI_Widget;
        UIPanel mNGUI_panel;

        public static Shader pShader_SDF_NGUI
        {
            get
            {
                if (mShader_SDF_NGUI == null)
                    mShader_SDF_NGUI = Shader.Find("Unlit/I2NGUI SmartEdge/SDF");
                return mShader_SDF_NGUI;
            }
        }
        static Shader mShader_SDF_NGUI;



        protected void OnEnableNGUI()
        {
            mNGUI_Widget = GetComponent<UIWidget>();
            if (mNGUI_Widget != null)
            {
                mNGUI_Widget.geometry.onCustomWrite += NGUI_ModifyVertices;
                mNGUI_Widget.onCreateMaterial = NGUI_OnCreateMaterial;
                if (mNGUI_Widget.panel != null)
                    mNGUI_Widget.panel.generateUV2 = true;
            }
        }

        protected void OnDisableNGUI()
        {
            if (mNGUI_Widget != null)
            {
                mNGUI_Widget.geometry.onCustomWrite -= NGUI_ModifyVertices;
                mNGUI_Widget.onCreateMaterial = null;
            }
        }

        void ValidateSettingsNGUI()
        {
            if (mNGUI_Widget == null)
                return;

            if (mNGUI_panel != mNGUI_Widget.panel)
            {
                mNGUI_panel = mNGUI_Widget.panel;
                if (mNGUI_panel!=null)
                    mNGUI_panel.generateUV2 = true;
            }


            var label = mNGUI_Widget as UILabel;
            if (label!=null)
            {
                if (_TextEffect._TextWrapping._Enabled && _TextEffect._TextWrapping._CaseType!=SE_TextEffect_TextWrapping.eTextCaseType.None &&
                    ( label.customModifier==null || label.modifier != UILabel.Modifier.Custom))
                {
                    switch (label.modifier)
                    {
                        case UILabel.Modifier.ToLowercase: _TextEffect._TextWrapping._CaseType = SE_TextEffect_TextWrapping.eTextCaseType.LowerCase; break;
                        case UILabel.Modifier.ToUppercase: _TextEffect._TextWrapping._CaseType = SE_TextEffect_TextWrapping.eTextCaseType.UpperCase; break;
                    }
                    label.customModifier = ModifyTextNGUI;
                    label.modifier = UILabel.Modifier.Custom;
                }

                // if TextWrapping is enabled, make the text to use overflow, and pass its original value to the textwrapping (e.g. truncate/wrap)
                if (_TextEffect._TextWrapping._Enabled)
                {
                    /*if (label.verticalOverflow != VerticalWrapMode.Overflow)
                    {
                        if (text.verticalOverflow == VerticalWrapMode.Truncate) _TextEffect._TextWrapping._VerticalWrapMode = SE_TextEffect_TextWrapping.eVerticalWrapMode.Truncate;
                        text.verticalOverflow = VerticalWrapMode.Overflow;
                    }

                    if (text.horizontalOverflow != HorizontalWrapMode.Overflow)
                    {
                        if (text.horizontalOverflow == HorizontalWrapMode.Wrap) _TextEffect._TextWrapping._HorizontalWrapMode = SE_TextEffect_TextWrapping.eHorizontalWrapMode.Wrap;
                        text.horizontalOverflow = HorizontalWrapMode.Overflow;
                    }*/


                    var horzAlignment = GetHorizontalAlignment_NGUI();
                    var vertAlignment = GetVerticalAlignment_NGUI();
                    if (horzAlignment != eHorizontalAlignment.Left || vertAlignment != eVerticalAlignment.Top)
                    {
                        if (horzAlignment != eHorizontalAlignment.Left) _TextEffect._TextWrapping._HorizontalAlignment = horzAlignment;
                        if (vertAlignment != eVerticalAlignment.Top) _TextEffect._TextWrapping._VerticalAlignment = vertAlignment;
                        label.pivot = UIWidget.Pivot.TopLeft;
                        label.alignment = NGUIText.Alignment.Automatic;
                    }
                }
            }
        }

        string ModifyTextNGUI( string s )
        {
            ModifyText(ref s);
            return s;
        }

        Material NGUI_OnCreateMaterial(Material mat)
        {
            if (!isActiveAndEnabled)
                return mat;

            if (mNGUI_Widget==null)
                mNGUI_Widget = GetComponent<UIWidget>();

            var mainTexture = (mat && mat.mainTexture) ? mat.mainTexture as Texture2D : null;
            if (mat == null)
                Debug.LogFormat(this, "Invalid Material");
            else
            if (mainTexture == null)
                Debug.LogFormat(this, "Material {0} doesn't have a texture assigned", mat.name);

            mWidgetColor = mNGUI_Widget.color;
            return GetMaterial(mat, mainTexture, pShader_SDF_NGUI);
        }

        void SetWidgetColor_NGUI( Color32 color )
        {
            if (mNGUI_Widget!=null)
                mNGUI_Widget.color = color;
        }

        bool UpdateNGUIspread()
        {
            var text = mNGUI_Widget as UILabel;
            if (text==null)
                return false;
            if (text != null && text.bitmapFont != null)
                mSpread = 15;//text.bitmapFont._SDF_Spread;
            return true;
        }

        public void MarkWidgetAsChanged_NGUI(bool MarkVertices = true, bool MarkMaterial = false)
        {
            if (mNGUI_Widget)
            {
                if (MarkMaterial)
                    mNGUI_Widget.RemoveFromPanel();
                mNGUI_Widget.MarkAsChanged();
            }
        }

        public void NGUI_ModifyVertices(List<Vector3> vertices, List<Vector2> uvs, List<Color> colors, List<Vector3> normals, List<Vector4> tangents, List<Vector4> uvs2)
        {
            //--[ Cache ]------------

            mWidgetColor = mNGUI_Widget.color;
            if (mWidgetColor.a < 1/255f && GetPlayingAnimation()==null)
                return;

            Bounds bounds = mNGUI_Widget.CalculateBounds();
            mRect = Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);
            mRectPivot.x = Mathf.InverseLerp(mRect.min.x, mRect.max.x, mRect.center.x);
            mRectPivot.y = Mathf.InverseLerp(mRect.min.y, mRect.max.y, mRect.center.y);

            UILabel label = mNGUI_Widget as UILabel;
            if (label != null)
            {
                mCharacterSize = label.printedSize.y;// fontSize;
                mLineHeight = label.height;
            }
            else
                mCharacterSize = mLineHeight = 1;
            Setup(mNGUI_Widget.mainTexture as Texture2D);

            ImportVerticesFromNGUI(vertices, uvs, colors, normals, tangents, uvs2);
            ModifyVertices();
            ExportVerticesToNGUI(vertices, uvs, colors, normals, tangents, uvs2);
        }

        void ImportVerticesFromNGUI(List<Vector3> vertices, List<Vector2> uvs, List<Color> colors, List<Vector3> normals, List<Vector4> tangents, List<Vector4> uvs2)
        {
            for (int layer = 0; layer < (int)SEVerticesLayers.length; ++layer)
                mSEMesh.mLayers[layer].Clear();
            mOriginalVertices = mSEMesh.mLayers[(int)SEVerticesLayers.Original];


            int nVerts = mNGUI_Widget.geometry.verts.Count;
            int iFirstVert = vertices.Count - nVerts;

            int iLine = 0, nCharactersInLine = 0;
            int iWord = 0, nWordsInLine = 0;
            int iParagraph = 0;
            UILabel label = mNGUI_Widget as UILabel;
            string strText = mNGUI_Widget.mRealVisualText;//(label == null ? null : label.processedText);
            string orgText = (label == null) ? strText : label.text;
            int iOrgChar = 0;
            char orgChr = (char)0;


            mOriginalVertices.Reset(nVerts);
            mCharacters.Reset(mOriginalVertices.Size / 4);

            mAllCharactersMin = MathUtils.v2max;
            mAllCharactersMax = MathUtils.v2min;
            var cIndex = 0;
            //var MatrixWidgetToPanel = mNGUI_Widget.panel.worldToLocal * mNGUI_Widget.cachedTransform.localToWorldMatrix;
            var MatrixPanelToWidget = mNGUI_Widget.cachedTransform.worldToLocalMatrix * mNGUI_Widget.panel.cachedTransform.localToWorldMatrix;
            for (int c = 0; c < strText.Length; ++c)
            {
                char chr = (strText == null || strText.Length<=c) ? (char)0 : strText[c];
                bool orgHasWhiteSpace = false;
                while (iOrgChar < orgText.Length && orgChr != chr)
                {
                    orgChr = orgText[iOrgChar];
                    orgHasWhiteSpace |= char.IsWhiteSpace(orgChr);

                    iOrgChar++;
                    if (chr == '\n')
                        break;
                }

                if (orgHasWhiteSpace && !char.IsWhiteSpace(chr))
                {
                    iWord++;
                    nWordsInLine++;
                }
                if (chr == '\n' && orgChr=='\n')
                    iParagraph++;

                if (chr == '\n')
                {
                    iLine++;
                    nCharactersInLine = 0;
                    nWordsInLine = 0;
                    continue;
                }
                
                mCharacters.Buffer[cIndex].Min = MathUtils.v2max;
                mCharacters.Buffer[cIndex].Max = MathUtils.v2min;

                for (int i = 0; i < 4; i++)
                {
                    int idx = iFirstVert + cIndex * 4 + i;

                    seVertex.position = vertices[idx];
                    seVertex.color = colors[idx];
                    seVertex.uv = uvs[idx];
                    //seVertex.normal = normals[idx];
                    //seVertex.tangent = tangents[idx];
                    seVertex.characterID = cIndex;

                    seVertex.position = MatrixPanelToWidget.MultiplyPoint3x4(seVertex.position);

                    if (mCharacters.Buffer[cIndex].Min.x > seVertex.position.x) mCharacters.Buffer[cIndex].Min.x = seVertex.position.x;
                    if (mCharacters.Buffer[cIndex].Min.y > seVertex.position.y) mCharacters.Buffer[cIndex].Min.y = seVertex.position.y;
                    if (mCharacters.Buffer[cIndex].Max.x < seVertex.position.x) mCharacters.Buffer[cIndex].Max.x = seVertex.position.x;
                    if (mCharacters.Buffer[cIndex].Max.y < seVertex.position.y) mCharacters.Buffer[cIndex].Max.y = seVertex.position.y;

                    mOriginalVertices.Buffer[cIndex * 4 + i] = seVertex;
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
                    mCharacters.Buffer[cIndex].iCharacterInText = cIndex;
                    mCharacters.Buffer[cIndex].iWord = iWord;
                    mCharacters.Buffer[cIndex].iWordInLine = nWordsInLine;
                    mCharacters.Buffer[cIndex].iParagraph = iParagraph;
                    mCharacters.Buffer[cIndex].TopY = iLine * mLineHeight;
                    mCharacters.Buffer[cIndex].BaselineY = iLine * mLineHeight;

                    cIndex++;
                }
            }
            mCharacterSize /= iLine+1;
            mCharacters.Size = cIndex;
            mOriginalVertices.Size = cIndex*4;
        }

        void ExportVerticesToNGUI(List<Vector3> vertices, List<Vector2> uvs, List<Color> colors, List<Vector3> normals, List<Vector4> tangents, List<Vector4> uvs2)
        {
            int nVerts = mNGUI_Widget.geometry.verts.Count;
            int iFirstVert = vertices.Count - nVerts;

            vertices.RemoveRange(iFirstVert, nVerts);
            uvs.RemoveRange(iFirstVert, nVerts);
            colors.RemoveRange(iFirstVert, nVerts);
            if (uvs2!=null)
                uvs2.RemoveRange(iFirstVert, nVerts);
            if (normals != null)
            {
                normals.RemoveRange(iFirstVert, nVerts);
                tangents.RemoveRange(iFirstVert, nVerts);
            }

            var MatrixWidgetToPanel = mNGUI_Widget.panel.worldToLocal * mNGUI_Widget.cachedTransform.localToWorldMatrix;

            Vector4 v4 = MathUtils.v4zero;

            for (int layer = 0; layer < mSEMesh.numLayers; ++layer)
            {
                var layerVertices = mSEMesh.mLayers[layer];

                for (int i = 0; i < layerVertices.Size; ++i)
                {
                    vertices.Add( MatrixWidgetToPanel.MultiplyPoint3x4(layerVertices.Buffer[i].position ));
                    colors.Add(layerVertices.Buffer[i].color);
                    uvs.Add(layerVertices.Buffer[i].uv);

                    if (uvs2 != null)
                    {
                        v4.x = layerVertices.Buffer[i].uv1.x;
                        v4.y = layerVertices.Buffer[i].uv1.y;
                        v4.z = layerVertices.Buffer[i].tangent.w;
                        uvs2.Add(v4);
                    }
                    if (normals != null)
                    {
                        normals.Add(layerVertices.Buffer[i].normal);
                        tangents.Add(layerVertices.Buffer[i].tangent);
                    }
                }
            }
        }

        eHorizontalAlignment GetHorizontalAlignment_NGUI()
        {
            var text = mNGUI_Widget as UILabel;
            if (text.alignment == NGUIText.Alignment.Automatic)
            {
                if (text.pivot == UIWidget.Pivot.TopLeft || text.pivot == UIWidget.Pivot.Left || text.pivot == UIWidget.Pivot.BottomLeft)
                    return eHorizontalAlignment.Left;

                if (text.pivot == UIWidget.Pivot.TopRight || text.pivot == UIWidget.Pivot.Right || text.pivot == UIWidget.Pivot.BottomRight)
                    return eHorizontalAlignment.Right;

                return eHorizontalAlignment.Center;
            }
            else
            {
                switch (text.alignment)
                {
                    case NGUIText.Alignment.Center:     return eHorizontalAlignment.Center;
                    case NGUIText.Alignment.Right:      return eHorizontalAlignment.Right;
                    case NGUIText.Alignment.Justified:  return eHorizontalAlignment.Justified;
                    default:                            return eHorizontalAlignment.Left;
                }
            }
        }

        eVerticalAlignment GetVerticalAlignment_NGUI()
        {
            var text = mNGUI_Widget as UILabel;

            if (text.pivot == UIWidget.Pivot.BottomLeft || text.pivot == UIWidget.Pivot.Bottom || text.pivot == UIWidget.Pivot.BottomRight)
                return eVerticalAlignment.Bottom;

            if (text.pivot == UIWidget.Pivot.TopLeft || text.pivot == UIWidget.Pivot.Top || text.pivot == UIWidget.Pivot.TopRight)
                return eVerticalAlignment.Top;

            return eVerticalAlignment.Center;
        }
    }
}

    public partial class UIWidget
    {
        public OnCreateMaterial onCreateMaterial;
        public delegate Material OnCreateMaterial(Material mat);

        public Shader GetShaderAndMaterial( ref Material mat )
        {
            if (onCreateMaterial != null)
            {
                mat = onCreateMaterial(mat);
                return mat==null ? null : mat.shader;
            }
            return shader;
        }

        // Used in the UILabel
        public string mRealVisualText;  // each character correspond to a quad
        public static System.Text.StringBuilder mStringBuilder = new System.Text.StringBuilder();
    }

    public partial class UIFont
    {
        public int _SDF_Spread = 15;
    }


#endif