#if SE_TMPro
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

namespace I2.SmartEdge
{
	public partial class SmartEdge
	{
		[NonSerialized]
		TextMeshProUGUI mTMP_Label;

		protected void OnEnableTMPro()
		{
			mTMP_Label = GetComponent<TextMeshProUGUI>();
			if (mTMP_Label != null)
				mTMP_Label.mEvent_OnPreUploadMeshData += TextMeshPro_OnPreUploadMeshData;
		}

		protected void OnDisableTMPro()
		{
			if (mTMP_Label != null)
				mTMP_Label.mEvent_OnPreUploadMeshData -= TextMeshPro_OnPreUploadMeshData;
		}


		public void MarkWidgetAsChanged_TMPro(bool MarkVertices = true, bool MarkMaterial = false)
		{
			if (mTMP_Label != null)
			{
				mTMP_Label.SetAllDirty();
			}
		}

        void SetWidgetColor_TMPro( Color32 color )
        {
            if (mTMP_Label!=null)
                mTMP_Label.color = color;
        }

        // If you get an error similar to this one: "Assets/I2/SmartEdge/Extensions/TMPro/SmartEdgeTMPro.cs(78,54): error CS0246: The type or namespace name `TMP_TextInfo' could not be found"
        // You need to remove SE_TMPro from the Scripting Define Symbols (Unity Menu-> Edit\Project Settings\Player)
        public void TextMeshPro_OnPreUploadMeshData(TextMeshProUGUI tmpLabel )
		{
			if (!isActiveAndEnabled) return;
			var textInfo = tmpLabel.textInfo;

			if (mRectTransform == null) mRectTransform = transform as RectTransform;
			if (mRectTransform == null) return;

			mRectTransform = tmpLabel.rectTransform;
			mRect = mRectTransform.rect;
			mRectPivot = mRectTransform.pivot;
			mWidgetColor = tmpLabel.color;

			mCharacterSize = tmpLabel.fontSize;
            mLineHeight = (textInfo.lineInfo.Length>0) ? textInfo.lineInfo[0].lineHeight : mCharacterSize;

			//--[ Characters ]--------------------------------------

			mAllCharactersMin.x = mAllCharactersMin.y = float.MaxValue;
			mAllCharactersMax.x = mAllCharactersMax.y = float.MinValue;
            mCharacters.Reset(textInfo.characterCount);


            int iLine = 0, nCharactersInLine = 0, iCharacter = 0;
            int iWord = 0, nWordsInLine = 0;
            int iParagraph = 0;

            int iFirstCharNextLine = 0;
            int iFirstCharNextWord = 0;

			for (int c=0; c< textInfo.characterCount; ++c)
			{
                if (!textInfo.characterInfo[c].isVisible)
                    continue;

				mCharacters.Buffer[iCharacter].Min = textInfo.characterInfo[c].bottomLeft;
				mCharacters.Buffer[iCharacter].Max = textInfo.characterInfo[c].topRight;
				if (mAllCharactersMin.x > mCharacters.Buffer[iCharacter].Min.x) mAllCharactersMin.x = mCharacters.Buffer[iCharacter].Min.x;
				if (mAllCharactersMin.y > mCharacters.Buffer[iCharacter].Min.y) mAllCharactersMin.y = mCharacters.Buffer[iCharacter].Min.y;
				if (mAllCharactersMax.x < mCharacters.Buffer[iCharacter].Max.x) mAllCharactersMax.x = mCharacters.Buffer[iCharacter].Max.x;
				if (mAllCharactersMax.y < mCharacters.Buffer[iCharacter].Max.y) mAllCharactersMax.y = mCharacters.Buffer[iCharacter].Max.y;

                if (c >= iFirstCharNextLine)
                {
                    iFirstCharNextLine = textInfo.lineInfo[iLine].lastCharacterIndex+1;
                    iLine++;
                    nCharactersInLine = 0;
                    nWordsInLine = -1;
                    iFirstCharNextWord = c-1;
                }

                if (c >= iFirstCharNextWord)
                {
                    iFirstCharNextWord = textInfo.wordInfo.Length>iWord ? textInfo.wordInfo[iWord].lastCharacterIndex + 1 : int.MaxValue;
                    iWord++;
                    nWordsInLine++;
                }

                mCharacters.Buffer[iCharacter].Character = textInfo.characterInfo[c].character;
                mCharacters.Buffer[iCharacter].iLine = iLine;
                mCharacters.Buffer[iCharacter].iCharacterInText = iCharacter;
                mCharacters.Buffer[iCharacter].iCharacterInLine = nCharactersInLine++;
                mCharacters.Buffer[iCharacter].iWord = iWord;
                mCharacters.Buffer[iCharacter].iWordInLine = nWordsInLine;
                mCharacters.Buffer[iCharacter].iParagraph = iParagraph;
                mCharacters.Buffer[iCharacter].TopY       = textInfo.lineInfo.Length>iLine ? textInfo.lineInfo[iLine].lineExtents.min.y : mCharacters.Buffer[iCharacter].Min.y;
                mCharacters.Buffer[iCharacter].BaselineY  = textInfo.lineInfo.Length>iLine ? textInfo.lineInfo[iLine].lineExtents.max.y : mCharacters.Buffer[iCharacter].Max.y;
                iCharacter++;
            }
            mCharacters.Size = iCharacter;
            mSETextureFormat = SETextureFormat.Unknown;
			for (int m=0; m<textInfo.meshInfo.Length; ++m)
			{
				ImportVerticesFromTMPro(textInfo, m);

				ModifyVertices();

				ExportVerticesToTMPro(textInfo, m);
			}
		}

        private void ImportVerticesFromTMPro(TMP_TextInfo textInfo, int meshIndex)
		{
            //--[ Clear Layers/Vertices ]----------------
            for (int layer = 0; layer < (int)SEVerticesLayers.length; ++layer)
                mSEMesh.mLayers[layer].Clear();
            mOriginalVertices = mSEMesh.mLayers[(int)SEVerticesLayers.Original];

            mOriginalVertices.Reset(textInfo.meshInfo[meshIndex].vertexCount);
            if (mOriginalVertices.Size == 0)
                return;


            for (int i = 0; i < mOriginalVertices.Size; ++i)
			{
				seVertex.position    = textInfo.meshInfo[meshIndex].vertices[i];
				seVertex.color       = textInfo.meshInfo[meshIndex].colors32[i];
				seVertex.uv          = textInfo.meshInfo[meshIndex].uvs0[i];
				seVertex.normal      = textInfo.meshInfo[meshIndex].normals[i];
				seVertex.tangent     = textInfo.meshInfo[meshIndex].tangents[i];
				seVertex.characterID = Mathf.FloorToInt(i/(float)4);                 // change

				mOriginalVertices.Buffer[i] = seVertex;
			}
		}

		private void ExportVerticesToTMPro(TMP_TextInfo textInfo, int meshIndex)
		{
            int nVerts = 0;
            for (int layer = 0; layer < mSEMesh.numLayers; ++layer)
                nVerts += mSEMesh.mLayers[layer].Size;

			textInfo.meshInfo[meshIndex].ResizeMeshInfo(nVerts/4);

			int indTri = 0, indVert=0;
            for (int layer = 0; layer<(int)SEVerticesLayers.length; ++layer)
            {
                var seMesh = mSEMesh.mLayers[layer];

                for (int i = 0; i < seMesh.Size; ++i)
                {
                    textInfo.meshInfo[meshIndex].vertices[indVert+i] = seMesh.Buffer[i].position;
                    textInfo.meshInfo[meshIndex].colors32[indVert+i] = seMesh.Buffer[i].color;
                    textInfo.meshInfo[meshIndex].uvs0[indVert+i]     = seMesh.Buffer[i].uv;
                    textInfo.meshInfo[meshIndex].normals[indVert+i]  = seMesh.Buffer[i].normal;
                    textInfo.meshInfo[meshIndex].tangents[indVert+i] = seMesh.Buffer[i].tangent;

                    if (i % 4 == 0)
                    {
                        textInfo.meshInfo[meshIndex].triangles[indTri++] = i;
                        textInfo.meshInfo[meshIndex].triangles[indTri++] = i + 1;
                        textInfo.meshInfo[meshIndex].triangles[indTri++] = i + 2;

                        textInfo.meshInfo[meshIndex].triangles[indTri++] = i + 2;
                        textInfo.meshInfo[meshIndex].triangles[indTri++] = i + 3;
                        textInfo.meshInfo[meshIndex].triangles[indTri++] = i;
                    }
                }
                indVert += seMesh.Size;
			}
            textInfo.characterCount = mCharacters.Size;
		}

        eHorizontalAlignment GetHorizontalAlignment_TMPro()
        {
            if ( ((int)mTMP_Label.alignment % 4)==0 )
                return eHorizontalAlignment.Left;

            if ( ((int)mTMP_Label.alignment % 4)==2 )
                return eHorizontalAlignment.Right;

            return eHorizontalAlignment.Center;
        }

        eVerticalAlignment GetVerticalAlignment_TMPro()
        {
            if (mTMP_Label.alignment <= TextAlignmentOptions.TopJustified)
                return eVerticalAlignment.Top;

            if (mTMP_Label.alignment <= TextAlignmentOptions.Justified)
                return eVerticalAlignment.Center;

            return eVerticalAlignment.Bottom;
        }
	}
}

namespace TMPro
{
	public partial class TextMeshProUGUI
	{
		public event System.Action<TextMeshProUGUI> mEvent_OnPreUploadMeshData;

		void Call_OnPreUploadMeshData()
		{
			if (mEvent_OnPreUploadMeshData != null)
				mEvent_OnPreUploadMeshData(this);
		}
	}
}
#endif
