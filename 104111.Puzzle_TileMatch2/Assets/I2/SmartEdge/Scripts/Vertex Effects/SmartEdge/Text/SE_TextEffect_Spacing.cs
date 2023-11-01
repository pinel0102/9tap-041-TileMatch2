using UnityEngine;
using System;

namespace I2.SmartEdge
{
    /*[System.Serializable]
    public class SE_FontSettings
    {
        public float _Space_Character, _Space_Word, _Space_Line;
    }*/


    [System.Serializable]
    public class SE_TextEffect_Spacing
    {
        public bool _Enabled = false;

        //public SE_FontSettings _FontSettings;
        public float _Space_Character, _Space_Word, _Space_Line, _Space_Paragraph;
        public float _Paragraph_FirstLineMargin;

        public CustomKerningPair[] _KerningPairs = new CustomKerningPair[0];

        [System.Serializable]
        public struct CustomKerningPair
        {
            public string PrevCharacters, NextCharacters;
            public float HSpace;
        }


        public void ModifyVertices(SmartEdge se)
        {
            if (!_Enabled || (_Space_Character<=0 && _Space_Word <= 0 && _Space_Line <= 0 && _Space_Paragraph <= 0 && _KerningPairs.Length==0 && _Paragraph_FirstLineMargin <= 0))
                return;

            float space_Character = _Space_Character  *se.mCharacterSize;// + (_FontSettings != null ? _FontSettings._Space_Character : 0);
            float space_Word      = _Space_Word       * se.mCharacterSize;//      + (_FontSettings != null ? _FontSettings._Space_Word : 0);
            float space_Line      = _Space_Line       * se.mCharacterSize;//      + (_FontSettings != null ? _FontSettings._Space_Line : 0);
            float space_Paragraph = _Space_Paragraph  * se.mCharacterSize;

            float paragraph_FirstLineMargin = _Paragraph_FirstLineMargin * se.mCharacterSize;

            float kerning_HSpace = 0;
            char prevChr = (char)0;
            int iLine = -1;
            float hSpace=0, vSpace = 0;
            int iPrevChrID = -1;

            se.mAllCharactersMin = MathUtils.v2max;
            se.mAllCharactersMax = MathUtils.v2min;

            var horizontalAlignment = se.GetHorizontalAlignment();
            var verticalAlignment = se.GetVerticalAlignment();
            var firstVertexInLine = 0;
            int iPrevParagraph = -1;

            for (int i = 0; i < SmartEdge.mOriginalVertices.Size; ++i)
            {
                var point = SmartEdge.mOriginalVertices.Buffer[i].position;

                int chrID = SmartEdge.mOriginalVertices.Buffer[i].characterID;

                if (iPrevChrID != chrID)
                {
                    iPrevChrID = chrID;
                    char chr = SmartEdge.mCharacters.Buffer[chrID].Character;


                    if (iLine != SmartEdge.mCharacters.Buffer[chrID].iLine)
                    {
                        iLine = SmartEdge.mCharacters.Buffer[chrID].iLine;
                        int iParagraph = SmartEdge.mCharacters.Buffer[chrID].iParagraph;
                        kerning_HSpace = iPrevParagraph != iParagraph ? paragraph_FirstLineMargin : 0;
                        if (horizontalAlignment != SmartEdge.eHorizontalAlignment.Left)
                            FixHorizontalAlignment(se, firstVertexInLine, i, hSpace, horizontalAlignment);
                        firstVertexInLine = i;
                        iPrevParagraph = iParagraph;
                    }

                    kerning_HSpace += GetKerning(prevChr, chr, se);
                    prevChr = chr;

                    hSpace = SmartEdge.mCharacters.Buffer[chrID].iCharacterInLine * space_Character +
                                SmartEdge.mCharacters.Buffer[chrID].iWordInLine * space_Word +
                                kerning_HSpace;
                    vSpace = iLine * space_Line +
                                SmartEdge.mCharacters.Buffer[chrID].iParagraph * space_Paragraph;
                }


                point.x += hSpace;
                point.y -= vSpace;

                SmartEdge.mOriginalVertices.Buffer[i].position = point;
            }

            if (horizontalAlignment != SmartEdge.eHorizontalAlignment.Left)
                FixHorizontalAlignment(se, firstVertexInLine, SmartEdge.mOriginalVertices.Size, hSpace, horizontalAlignment);

            if (verticalAlignment != SmartEdge.eVerticalAlignment.Top)
                FixVerticalAlignment(se, vSpace, verticalAlignment);

            se.UpdateCharactersMinMax();
        }

        public float GetKerning(char firstChr, char secondChr, SmartEdge se)
        {
            for (int k = 0, maxK = _KerningPairs.Length; k < maxK; ++k)
                if ((string.IsNullOrEmpty(_KerningPairs[k].PrevCharacters) || _KerningPairs[k].PrevCharacters.IndexOf(firstChr) >= 0) && _KerningPairs[k].NextCharacters.IndexOf(secondChr) >= 0)
                    return _KerningPairs[k].HSpace * se.mCharacterSize;
            return 0;
        }

        void FixHorizontalAlignment(SmartEdge se, int iFirstVertex, int iLastVertex, float hSpace, SmartEdge.eHorizontalAlignment alignment)
        {
            if (alignment == SmartEdge.eHorizontalAlignment.Right)
                hSpace = -hSpace;
            else
                hSpace = -hSpace / 2.0f;

            for (int v = iFirstVertex; v < iLastVertex; ++v)
                SmartEdge.mOriginalVertices.Buffer[v].position.x += hSpace;
        }

        void FixVerticalAlignment(SmartEdge se, float vSpace, SmartEdge.eVerticalAlignment alignment)
        {
            if (alignment == SmartEdge.eVerticalAlignment.Bottom)
                vSpace = -vSpace;
            else
                vSpace = -vSpace / 2.0f;

            for (int i = 0; i < SmartEdge.mOriginalVertices.Size; ++i)
                SmartEdge.mOriginalVertices.Buffer[i].position.y -= vSpace;
        }
    }
}