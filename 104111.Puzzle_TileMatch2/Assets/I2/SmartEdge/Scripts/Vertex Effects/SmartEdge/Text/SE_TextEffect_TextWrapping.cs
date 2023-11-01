using UnityEngine;
using System.Collections.Generic;
using System;

namespace I2.SmartEdge
{
    [System.Serializable]
    public class SE_TextEffect_TextWrapping
    {
        public bool _Enabled = false;

        public enum eVerticalWrapMode { Overflow, Truncate, Page/*, Ellipsis*/ };
        public eVerticalWrapMode _VerticalWrapMode = eVerticalWrapMode.Overflow;

        public enum eHorizontalWrapMode { Overflow, Truncate, Wrap };
        public eHorizontalWrapMode _HorizontalWrapMode = eHorizontalWrapMode.Overflow;

        public int  _CurrentPage, 
                    _Pages = 1;      // When Wrap

        public SmartEdge.eHorizontalAlignment _HorizontalAlignment;

        public SmartEdge.eVerticalAlignment _VerticalAlignment;

        [Range(0, 1)] public float _JustifiedWordCharacterRatio = 0.2f;   // when 0, only the Words are moved, as it grows to 1, the characters start to be separated as well
        [Range(0, 1)] public float _JustifiedMinWordSpace = 0.1f;         // when the space required for the justification is less than this, don't expand the characters

        public int _MaxVisibleCharacters = -1;
        public int _MaxVisibleLines = -1;
        public int _MaxVisibleWords = -1;


        public enum eTextCaseType { None, UpperCase, LowerCase, Sentence, TitleCase, SmallCaps };
        public eTextCaseType _CaseType = eTextCaseType.None;


        public void ModifyVertices(SmartEdge se)
        {
            if (!_Enabled || SmartEdge.mCharacters.Size==0)
                return;
            
            int[] outLines = new int[SmartEdge.mCharacters.Size];
            float w, h;
            float finalWidth = se.mRect.width;

            switch (_HorizontalWrapMode)
            {
                case eHorizontalWrapMode.Wrap:         if (se._TextEffect._BestFit._Enabled)
                                                       {
                                                            WrapTextBestFit(se, ref outLines);
                                                            RelocateVertices(se, outLines);
                                                            finalWidth = se.mAllCharactersMax.x - se.mRect.xMin;
                                                       }
                                                       else
                                                       {
                                                            WrapText(se, se.mRect.width, outLines, out w, out h);
                                                            RelocateVertices(se, outLines);                                                            
                                                       }
                                                       break;
            }

            bool truncateWidth = (_HorizontalWrapMode == eHorizontalWrapMode.Truncate) && (!se._TextEffect._BestFit._Enabled || !se._TextEffect._BestFit._FitWidth);
            bool truncateHeight = (_VerticalWrapMode == eVerticalWrapMode.Truncate) && (!se._TextEffect._BestFit._Enabled || !se._TextEffect._BestFit._FitHeight);
            if (truncateWidth || truncateHeight)
                TruncateText(se, truncateWidth, truncateHeight);

            ApplyHorizontalAlign(se, finalWidth);

            switch (_VerticalAlignment)
            {
                case SmartEdge.eVerticalAlignment.Center:         VerticalAlignTo_Center(se); break;
                case SmartEdge.eVerticalAlignment.Bottom:         VerticalAlignTo_Bottom(se); break;
            }

            if (_VerticalWrapMode == eVerticalWrapMode.Page)
            {
                TruncatePage(se);
            }
            TruncateCharacters(se);
        }

        // Returns the next bigger width that changes the line distribution
        float WrapText( SmartEdge se, float width, int[] outLines, out float finalWidth, out float finalHeight )
        {
            //string sPrint = "";

            float nextLength = float.MaxValue; //the next bigger width that changes the line distribution (this will be shrank until found the smalled first word in all line)

            int iLine = 0, iParagraph = 0;   // Position while iterating
            int iOutputLine =0;
            int nWordsInLine = 0;
            finalWidth = finalHeight = 0;

            float lineStartX = se.mRect.xMin;
            float prevLength = 0;
            float lineHeight = 0, lineMinY=float.MaxValue, lineMaxY=float.MinValue;

            for (int c=0; c<SmartEdge.mCharacters.Size;)
            {
                int iWord = SmartEdge.mCharacters.Buffer[c].iWord;
                nWordsInLine++;

                // Find where the word ends
                int iWordEnd = c;
                while (iWordEnd < SmartEdge.mCharacters.Size && iWord == SmartEdge.mCharacters.Buffer[iWordEnd].iWord)
                {
                    if (SmartEdge.mCharacters.Buffer[iWordEnd].Height > lineHeight)
                        lineHeight = SmartEdge.mCharacters.Buffer[iWordEnd].Height;

                    iWordEnd++;
                }
                float wordMinY = SmartEdge.mCharacters.Buffer[c].Min.y;
                float wordMaxY = SmartEdge.mCharacters.Buffer[iWordEnd-1].Max.y;

                if (wordMinY < lineMinY) lineMinY = wordMinY;
                if (wordMaxY > lineMaxY) lineMaxY = wordMaxY;

                // Get Stats of the current word

                int iNewLine = SmartEdge.mCharacters.Buffer[c].iLine;
                int iNewParagraph = SmartEdge.mCharacters.Buffer[c].iParagraph;

                // If the input text has a new line, restart the length counter
                if (iNewLine != iLine)
                {
                    iLine = iNewLine;
                    if (c > 0)
                    {
                        var ChrSPACEwidth = SmartEdge.mCharacters.Buffer[c - 1].Height * 0.2f;
                        prevLength += (SmartEdge.mCharacters.Buffer[c - 1].Max.x - lineStartX + ChrSPACEwidth);
                    }
                    lineStartX = se.mRect.xMin;
                }
                float lineLength = SmartEdge.mCharacters.Buffer[iWordEnd - 1].Max.x - lineStartX;
                float totalLineLength = lineLength + prevLength;

                // Split the output line If its a new paragraph or we reached the target length
                // Account for when the first word in the output line is longer than width
                if (iNewParagraph!=iParagraph || (nWordsInLine>1 && totalLineLength>=width))
                { 
                    nWordsInLine = 1;
                    int nOutLines = iNewParagraph == iParagraph ? 1 : (iNewParagraph - iParagraph);
                    iOutputLine += nOutLines;

                    if (finalHeight <= 0) finalHeight+= lineHeight;
                    finalHeight += lineHeight * nOutLines;
                    lineHeight = 0;
                    lineMinY = wordMinY;
                    lineMaxY = wordMaxY;

                    lineStartX = iNewParagraph != iParagraph ? se.mRect.xMin : SmartEdge.mCharacters.Buffer[c].Min.x;
                    prevLength = 0;

                    // The next width that changes the lines, is by adding the smallest first word in a line
                    if (totalLineLength > width && totalLineLength < nextLength)
                        nextLength = totalLineLength;

                    //sPrint += "\n";
                }
                else
                {
                    if (totalLineLength > finalWidth)
                        finalWidth = totalLineLength;
                }
                //if (nWordsInLine>1)
                  //  sPrint += " ";

                iParagraph = SmartEdge.mCharacters.Buffer[c].iParagraph;
                // Update the word line number
                for (; c < iWordEnd; ++c)
                {
                    //sPrint += SmartEdge.mCharacters.Buffer[c].Character;
                    //sPrint += SmartEdge.mCharacters.Buffer[c].iWord.ToString();
                    //sPrint += iOutputLine.ToString();
                    outLines[c] = iOutputLine;
                }
            }
            if (lineMinY<lineMaxY)
                finalHeight += lineMaxY-lineMinY;
            //Debug.Log(sPrint);
            return nextLength;
        }

        // Assigns each character to the correct line
        void WrapTextBestFit(SmartEdge se, ref int[] outLines)
        {
            int[] bestLines = new int[outLines.Length];

            float targetHeight = se.mRect.height;
            if (se._TextEffect._BestFit._Enabled)
            {
                targetHeight *= _Pages;
            }

            float targetRatio = se.mRect.width / targetHeight;

            // Find the maximum width/height
            float resultWidth, resultHeight;
            WrapText(se, float.MaxValue, bestLines, out resultWidth, out resultHeight);

            float LeftX = 0, RightX = resultWidth;

            float lastX = (LeftX + RightX) * 0.5f;
            float leftRatio = 0, rightRatio = resultWidth / resultHeight;
            float closestDist2Ratio = float.MaxValue;
            float nextPossibleX = 0;

            int nIterations = 0;
            while (RightX-LeftX > se.mCharacterSize*0.0001f && nIterations < 20 && closestDist2Ratio>0.01f)
            {
                nIterations++;

                // try guessing a better split point than 0.5f
                float factor = (targetRatio - leftRatio) / (rightRatio - leftRatio);
                factor = factor < 0.3f ? 0.3f : (factor > 0.7 ? 0.7f : factor);     // don't allow splitting too narrow
                lastX = LeftX + (RightX-LeftX) * factor;
                if (lastX < nextPossibleX)
                    lastX = nextPossibleX;

                // Test this split, nextPossibleX returns the next bigger width that changes the distrituion
                nextPossibleX = WrapText(se, lastX, outLines, out resultWidth, out resultHeight);
                float ratio = resultWidth / resultHeight;

                // Choose the left or right segment where is the targetRatio
                if (targetRatio > ratio && nextPossibleX < RightX)
                {
                    LeftX = lastX;
                    leftRatio = ratio;
                }
                else
                if (targetRatio < ratio)
                {
                    RightX = lastX;
                    rightRatio = ratio;
                    nextPossibleX = 0;
                }
                else
                    break;

                // Check if this is the best split so far
                float dist2ratio = targetRatio - ratio;
                if (dist2ratio < 0) dist2ratio = -dist2ratio;
                if (dist2ratio < closestDist2Ratio)
                {
                    var tmp = bestLines; bestLines = outLines; outLines = tmp;
                    closestDist2Ratio = dist2ratio;
                }

                /*Debug.LogFormat("{0}: X[{1}, {2}, {3}]  ({4}, {5}, {6} | {7})  {8} <{9}>", 
                    nIterations, 
                    LeftX, lastX, RightX, 
                    leftRatio, ratio, rightRatio, targetRatio, 
                    targetRatio - ratio, nextPossibleX);*/
            }
            outLines = bestLines;
        }

        void RelocateVertices(SmartEdge se, int[] outLines)
        {
            float lineBaseY = se.mRect.yMax;
            Vector2 offset = MathUtils.v2zero;
            int iFirstWordInLine = 0;


            //var TextMaxY = se.mAllCharactersMax.y;
            se.mAllCharactersMin = MathUtils.v2max;
            se.mAllCharactersMax = MathUtils.v2min;

            for (int c = 0; c < SmartEdge.mCharacters.Size;)
            {
                int iOutLine = outLines[c];
                float lineTopHeight = 0;   // Distance from the top of the line to the baseline

                // Find end of line and measure its Height
                int iLineEnd = c;
                while (iLineEnd < SmartEdge.mCharacters.Size && iOutLine == outLines[iLineEnd])
                {
                    var height = SmartEdge.mCharacters.Buffer[iLineEnd].TopY - SmartEdge.mCharacters.Buffer[iLineEnd].BaselineY;
                    if (height > lineTopHeight)
                        lineTopHeight = height;
                    iLineEnd++;
                }
                lineBaseY -= lineTopHeight;

                float outMinPosX = se.mRect.xMin;

                float lineMinX   = SmartEdge.mCharacters.Buffer[c].iCharacterInLine==0 ? se.mRect.xMin : SmartEdge.mCharacters.Buffer[c].Min.x;
                int iLine        = SmartEdge.mCharacters.Buffer[c].iLine;
                iFirstWordInLine = SmartEdge.mCharacters.Buffer[c].iWord;
                float lastMaxX   = SmartEdge.mCharacters.Buffer[c].Max.x;

                float lineBottomHeight = 0;   // Extra space after the baseline
                for (int iFirstChr = c; c < iLineEnd; ++c)
                {
                    if (iLine != SmartEdge.mCharacters.Buffer[c].iLine)
                    {
                        var ChrSPACEwidth = SmartEdge.mCharacters.Buffer[c].Height * 0.2f;
                        outMinPosX += lastMaxX - lineMinX + ChrSPACEwidth;
                        lineMinX = SmartEdge.mCharacters.Buffer[c].Min.x;
                        iLine = SmartEdge.mCharacters.Buffer[c].iLine;
                    }

                    // Find how much the line extends after the baselineY
                    var height = SmartEdge.mCharacters.Buffer[c].BaselineY - (SmartEdge.mCharacters.Buffer[c].TopY - SmartEdge.mCharacters.Buffer[c].Height);
                    if (height > lineBottomHeight)
                        lineBottomHeight = height;

                    lastMaxX    = SmartEdge.mCharacters.Buffer[c].Max.x;
                    var orgPosY = /*TextMaxY - */SmartEdge.mCharacters.Buffer[c].BaselineY;
                    var orgPosX = SmartEdge.mCharacters.Buffer[c].Min.x;
                    var outPosX = outMinPosX +(SmartEdge.mCharacters.Buffer[c].Min.x - lineMinX);

                    offset.x = outPosX - orgPosX;
                    offset.y = lineBaseY - orgPosY;

                    SmartEdge.mCharacters.Buffer[c].iLine            = iOutLine;
                    SmartEdge.mCharacters.Buffer[c].iCharacterInLine = c - iFirstChr;
                    SmartEdge.mCharacters.Buffer[c].iWordInLine      = SmartEdge.mCharacters.Buffer[c].iWord - iFirstWordInLine;
                    SmartEdge.mCharacters.Buffer[c].Min             += offset;
                    SmartEdge.mCharacters.Buffer[c].Max             += offset;
                    SmartEdge.mCharacters.Buffer[c].TopY            += offset.y;
                    SmartEdge.mCharacters.Buffer[c].BaselineY       = lineBaseY;

                    if (SmartEdge.mCharacters.Buffer[c].Min.x < se.mAllCharactersMin.x) se.mAllCharactersMin.x = SmartEdge.mCharacters.Buffer[c].Min.x;
                    if (SmartEdge.mCharacters.Buffer[c].Min.y < se.mAllCharactersMin.y) se.mAllCharactersMin.y = SmartEdge.mCharacters.Buffer[c].Min.y;
                    if (SmartEdge.mCharacters.Buffer[c].Max.x > se.mAllCharactersMax.x) se.mAllCharactersMax.x = SmartEdge.mCharacters.Buffer[c].Max.x;
                    if (SmartEdge.mCharacters.Buffer[c].Max.y > se.mAllCharactersMax.y) se.mAllCharactersMax.y = SmartEdge.mCharacters.Buffer[c].Max.y;
                    

                    for (int v = c * 4; v < c * 4 + 4; ++v)
                    {
                        SmartEdge.mOriginalVertices.Buffer[v].position.x += offset.x;
                        SmartEdge.mOriginalVertices.Buffer[v].position.y += offset.y;
                    }
                }

                // expand the line to account for the extra space after the baseline
                lineBaseY -= lineBottomHeight;

                // Add any extra lines
                if (iLineEnd < SmartEdge.mCharacters.Size)
                {
                    int nLines = (outLines[iLineEnd] - iOutLine)-1;
                    lineBaseY -= (lineTopHeight+lineBottomHeight )* nLines;
                }
            }
        }

        void TruncateText(SmartEdge se, bool truncateWidth, bool truncateHeight)
        {
            int iOutCharacter = 0;
            float maxX = se.mRect.xMax;
            float minY = se.mRect.yMin;

            // When truncate is enabled, that axis should show the characters of the Top/Left
            float offsetX = (truncateWidth && se.mAllCharactersMin.x < se.mRect.xMin) ? se.mRect.xMin - se.mAllCharactersMin.x : 0;
            float offsetY = (truncateHeight && se.mAllCharactersMax.y > se.mRect.yMax) ? se.mRect.yMax - se.mAllCharactersMax.y : 0;

            for (int c = 0; c < SmartEdge.mCharacters.Size; )
            {
                float lineMinY = float.MaxValue;

                // Find start and of visible line and its height (a line should be culled all at once using its biggest height)
                int iLineStart = c;
                int iLineEnd = c;
                int iLine = SmartEdge.mCharacters.Buffer[c].iLine;
                while (iLineEnd < SmartEdge.mCharacters.Size && iLine == SmartEdge.mCharacters.Buffer[iLineEnd].iLine)
                {
                    if (SmartEdge.mCharacters.Buffer[iLineEnd].Min.y < lineMinY)
                        lineMinY = SmartEdge.mCharacters.Buffer[iLineEnd].Min.y;

                    iLineEnd++;
                }

                c = iLineEnd;

                // Truncate the line on the max Y Axis
                if (truncateHeight && lineMinY+offsetY < minY)
                    break;

                // Truncate the line at the Right
                while (truncateWidth && iLineEnd > iLineStart && SmartEdge.mCharacters.Buffer[iLineEnd - 1].Max.x + offsetX > maxX)
                    iLineEnd--;

                // Shift the visible characters to the front
                if (iOutCharacter != iLineStart && iLineEnd > iLineStart)
                {
                    SmartEdge.mCharacters.CopyFrom(SmartEdge.mCharacters, iLineStart, iOutCharacter, iLineEnd - iLineStart, false);
                    SmartEdge.mOriginalVertices.CopyFrom(SmartEdge.mOriginalVertices, iLineStart * 4, iOutCharacter * 4, (iLineEnd - iLineStart) * 4, false);
                }
                iOutCharacter += iLineEnd - iLineStart;
            }
            SmartEdge.mCharacters.Size = iOutCharacter;
            SmartEdge.mOriginalVertices.Size = iOutCharacter * 4;

            // Move the polygons to the align to the TopLeft
            if (offsetX>0 || offsetY < 0)
            {
                for (int i=0; i<SmartEdge.mCharacters.Size; ++i)
                {
                    SmartEdge.mCharacters.Buffer[i].Min.x += offsetX;
                    SmartEdge.mCharacters.Buffer[i].Min.y += offsetY;
                    SmartEdge.mCharacters.Buffer[i].Max.x += offsetX;
                    SmartEdge.mCharacters.Buffer[i].Max.y += offsetY;
                }

                for (int i=0; i<SmartEdge.mOriginalVertices.Size; ++i)
                {
                    SmartEdge.mOriginalVertices.Buffer[i].position.x += offsetX;
                    SmartEdge.mOriginalVertices.Buffer[i].position.y += offsetY;
                }
            }

            se.UpdateCharactersMinMax(false);
        }

        void TruncatePage(SmartEdge se)
        {
            // Find Page Boundaries
            float pageHeight = se.mRect.height;
            if (se._TextEffect._BestFit._Enabled)
            {
                float targetRatio = se.mRect.width / se.mRect.height;
                pageHeight  = (se.mAllCharactersMax.x-se.mRect.xMin) / targetRatio;

            }
            
            float maxY = se.mRect.yMax - pageHeight*_CurrentPage;
            float minY = maxY - pageHeight;

            // Find the first character/line
            int iStartChr;
            for (iStartChr = 0; iStartChr < SmartEdge.mCharacters.Size; ++iStartChr)
            {
                if (SmartEdge.mCharacters.Buffer[iStartChr].Min.y < maxY)
                    break;
            }
            // None of the characters is visible in the current page
            if (iStartChr == SmartEdge.mCharacters.Size)
            {
                SmartEdge.mCharacters.Size = 0;
                SmartEdge.mOriginalVertices.Size = 0;
                return;
            }


            // Walk back to the beginning of the line
            int iLine = SmartEdge.mCharacters.Buffer[iStartChr].iLine;
            while (iStartChr > 0 && SmartEdge.mCharacters.Buffer[iStartChr].iLine == iLine)
                iStartChr--;
            if (SmartEdge.mCharacters.Buffer[iStartChr].iLine != iLine)
                iStartChr++;

            //Find the last line that its visible
            int iLastChr = iStartChr;
            float maxChrY = float.MinValue;
            for (int c = iStartChr; c < SmartEdge.mCharacters.Size;)
            {
                float lineMinY = float.MaxValue;

                // Find start and of visible line and its height (a line should be culled all at once using its biggest height)
                int iLineStart = c;
                iLine = SmartEdge.mCharacters.Buffer[c].iLine;
                while (c < SmartEdge.mCharacters.Size && iLine == SmartEdge.mCharacters.Buffer[c].iLine)
                {
                    if (SmartEdge.mCharacters.Buffer[c].Min.y < lineMinY)
                        lineMinY = SmartEdge.mCharacters.Buffer[c].Min.y;
                    if (SmartEdge.mCharacters.Buffer[c].Max.y > maxChrY)
                        maxChrY = SmartEdge.mCharacters.Buffer[c].Max.y;

                    c++;
                }
                if (lineMinY < minY)
                {
                    iLastChr = iLineStart;
                    break;
                }
                else
                    iLastChr = c;
            }


            // Cull the Characters
            int nCharacters = iLastChr - iStartChr;

            if (iStartChr!=0)
            {
                SmartEdge.mCharacters.CopyFrom(SmartEdge.mCharacters, iStartChr, 0, nCharacters, true);
                SmartEdge.mOriginalVertices.CopyFrom(SmartEdge.mOriginalVertices, iStartChr*4, 0, nCharacters*4, true);
            }
            else
            {
                SmartEdge.mCharacters.Size = iLastChr;
                SmartEdge.mOriginalVertices.Size = iLastChr*4;
            }

            // Offset all characters
            float offsetY = se.mRect.yMax - maxChrY;
            se.mAllCharactersMax.y = float.MinValue;
            se.mAllCharactersMin.y = float.MaxValue;
            for (int c = 0; c < SmartEdge.mCharacters.Size; c++)
            {
                SmartEdge.mCharacters.Buffer[c].Min.y += offsetY;
                SmartEdge.mCharacters.Buffer[c].Max.y += offsetY;
                if (SmartEdge.mCharacters.Buffer[c].Min.y < se.mAllCharactersMin.y) se.mAllCharactersMin.y = SmartEdge.mCharacters.Buffer[c].Min.y;
                if (SmartEdge.mCharacters.Buffer[c].Max.y > se.mAllCharactersMax.y) se.mAllCharactersMax.y = SmartEdge.mCharacters.Buffer[c].Max.y;

                for (int i = c * 4; i < c * 4 + 4; ++i)
                    SmartEdge.mOriginalVertices.Buffer[i].position.y += offsetY;
            }
        }

        void TruncateCharacters(SmartEdge se)
        {
            int nCharacters = SmartEdge.mCharacters.Size;

            if (_MaxVisibleCharacters >= 0 && _MaxVisibleCharacters < nCharacters)
                nCharacters = _MaxVisibleCharacters;

            if (_MaxVisibleWords >= 0)
            {
                while (nCharacters > 0 && SmartEdge.mCharacters.Buffer[nCharacters-1].iWord >= _MaxVisibleWords)
                    nCharacters--;
            }

            if (_MaxVisibleLines >= 0)
            {
                while (nCharacters > 0 && SmartEdge.mCharacters.Buffer[nCharacters-1].iLine >= _MaxVisibleLines)
                    nCharacters--;
            }

            if (nCharacters == SmartEdge.mCharacters.Size)
                return;

            SmartEdge.mCharacters.Size = nCharacters;
            SmartEdge.mOriginalVertices.Size = nCharacters * 4;

            se.UpdateCharactersMinMax(false);
        }

        void ApplyHorizontalAlign(SmartEdge se, float finalWidth)
        {
            int iLineEnd;
            se.mAllCharactersMax.x = float.MinValue;
            se.mAllCharactersMin.x = float.MaxValue;

            for (int iLineStart= 0; iLineStart < SmartEdge.mCharacters.Size;)
            {
                FindLine(se, iLineStart, out iLineEnd);

                var alignment = _HorizontalAlignment;
                if (SmartEdge.mCharacters.Buffer[iLineEnd - 1].RichText.Alignment != RichTextTag_Align.eAlignmentOverride.Automatic)
                    alignment = (SmartEdge.eHorizontalAlignment)((int)SmartEdge.mCharacters.Buffer[iLineEnd - 1].RichText.Alignment-1);

                switch (alignment)
                {
                    case SmartEdge.eHorizontalAlignment.Center:     ApplyHorizontalAlignToLine_Center(se, iLineStart, iLineEnd-1); break;
                    case SmartEdge.eHorizontalAlignment.Right:      ApplyHorizontalAlignToLine_Right(se, iLineStart, iLineEnd - 1); break;
                    case SmartEdge.eHorizontalAlignment.Justified:  ApplyHorizontalAlignToLine_Justified(se, finalWidth, iLineStart, iLineEnd - 1); break;
                }
                if (SmartEdge.mCharacters.Buffer[iLineStart].Min.x < se.mAllCharactersMin.x) se.mAllCharactersMin.x = SmartEdge.mCharacters.Buffer[iLineStart].Min.x;
                if (SmartEdge.mCharacters.Buffer[iLineEnd - 1].Max.x > se.mAllCharactersMax.x) se.mAllCharactersMax.x = SmartEdge.mCharacters.Buffer[iLineEnd - 1].Max.x;

                iLineStart = iLineEnd;
            }
        }

        void ApplyHorizontalAlignToLine_Right(SmartEdge se, int iLineStart, int iLineEnd)
        {
            float offsetX = se.mRect.xMax - SmartEdge.mCharacters.Buffer[iLineEnd].Max.x;

            for (int c = iLineStart; c <= iLineEnd; c++)
            {
                SmartEdge.mCharacters.Buffer[c].Min.x += offsetX;
                SmartEdge.mCharacters.Buffer[c].Max.x += offsetX;

                for (int i = c * 4; i < c * 4 + 4; ++i)
                    SmartEdge.mOriginalVertices.Buffer[i].position.x += offsetX;
            }
        }

        void ApplyHorizontalAlignToLine_Center(SmartEdge se, int iLineStart, int iLineEnd)
        {
            float middleX = se.mRect.center.x;
            float lineMiddleX = (SmartEdge.mCharacters.Buffer[iLineStart].Min.x + SmartEdge.mCharacters.Buffer[iLineEnd].Max.x) * 0.5f;
            float offsetX = middleX - lineMiddleX;

            for (int c = iLineStart; c <= iLineEnd; c++)
            {
                SmartEdge.mCharacters.Buffer[c].Min.x += offsetX;
                SmartEdge.mCharacters.Buffer[c].Max.x += offsetX;

                for (int i = c * 4; i < c * 4 + 4; ++i)
                    SmartEdge.mOriginalVertices.Buffer[i].position.x += offsetX;
            }
        }

        void ApplyHorizontalAlignToLine_Justified(SmartEdge se, float width, int iLineStart, int iLineEnd)
        {
            float rightX = se.mRect.xMin + width;
            float minWordSpace = se.mRect.width * _JustifiedMinWordSpace * 0.5f;    // Only expand the characters if the space is bigger than this (if not, just separate the words)

            //This line should only be justified, if the next one belongs to the same paragraph.
            if (iLineEnd+1 >= SmartEdge.mCharacters.Size || SmartEdge.mCharacters.Buffer[iLineStart].iParagraph != SmartEdge.mCharacters.Buffer[iLineEnd].iParagraph)
                return;

            // A line with a single word can't be justified
            int nWordSpacesInLine = SmartEdge.mCharacters.Buffer[iLineEnd].iWordInLine - SmartEdge.mCharacters.Buffer[iLineStart].iWordInLine;
            if (nWordSpacesInLine == 0)
                return;

            int nCharSpacesInLine = SmartEdge.mCharacters.Buffer[iLineEnd].iCharacterInLine - SmartEdge.mCharacters.Buffer[iLineStart].iCharacterInLine;
            float CWratio = _JustifiedWordCharacterRatio;

            // Find how much space should be added to each WORD and CHARACTER
            float extraSpace = rightX - SmartEdge.mCharacters.Buffer[iLineEnd].Max.x;

            // Only move the characters if the space is too small
            float extraSpaceW = minWordSpace > extraSpace ? extraSpace : minWordSpace;
            extraSpace -= extraSpaceW;

            float spacePerWord = (extraSpaceW + (1 - CWratio) * extraSpace) / nWordSpacesInLine;
            float spacePerCharacter = CWratio * extraSpace / nCharSpacesInLine;

            for (int c = iLineStart; c <= iLineEnd; c++)
            {
                var offsetX = SmartEdge.mCharacters.Buffer[c].iWordInLine * spacePerWord + SmartEdge.mCharacters.Buffer[c].iCharacterInLine * spacePerCharacter;

                SmartEdge.mCharacters.Buffer[c].Min.x += offsetX;
                SmartEdge.mCharacters.Buffer[c].Max.x += offsetX;

                for (int i = c * 4; i < c * 4 + 4; ++i)
                    SmartEdge.mOriginalVertices.Buffer[i].position.x += offsetX;
            }
        }



        void VerticalAlignTo_Bottom(SmartEdge se)
        {
            float offset = se.mRect.yMin - se.mAllCharactersMin.y;
            ApplyVerticalOffset(se, offset);
        }

        void VerticalAlignTo_Center(SmartEdge se)
        {
            float centerY = (se.mAllCharactersMin.y + se.mAllCharactersMax.y) * 0.5f;
            float offset = se.mRect.center.y - centerY;
            ApplyVerticalOffset(se, offset);
        }


        void ApplyVerticalOffset( SmartEdge se, float offsetY )
        {
            for (int i = 0; i < SmartEdge.mOriginalVertices.Size; ++i)
                SmartEdge.mOriginalVertices.Buffer[i].position.y += offsetY;

            for (int i=0; i<SmartEdge.mCharacters.Size; ++i)
            {
                SmartEdge.mCharacters.Buffer[i].Min.y += offsetY;
                SmartEdge.mCharacters.Buffer[i].Max.y += offsetY;
            }

            se.mAllCharactersMax.y += offsetY;
            se.mAllCharactersMin.y += offsetY;
        }


        void FindLine( SmartEdge se, int iFirstChar, out int iLastChar )
        {
            int iLine = SmartEdge.mCharacters.Buffer[iFirstChar].iLine;
            iLastChar = iFirstChar;
            while (iLastChar < SmartEdge.mCharacters.Size && iLine == SmartEdge.mCharacters.Buffer[iLastChar].iLine)
                iLastChar++;
        }


        public bool ModifyText( ref string labelText )
        {
            if (string.IsNullOrEmpty(labelText))
                return false;

            if (_CaseType == eTextCaseType.None)
                return false;

            labelText = ConvertTextCase(labelText, _CaseType);
            return true;
        }

        public static string ConvertTextCase(string labelText, eTextCaseType caseType)
        {
            switch (caseType)
            {
                case eTextCaseType.UpperCase: return labelText.ToUpper();
                case eTextCaseType.LowerCase: return labelText.ToLower();
                case eTextCaseType.Sentence : return ToSentenceCase(labelText);
                case eTextCaseType.TitleCase: return ToTitleCase(labelText);
                case eTextCaseType.SmallCaps: return labelText.ToUpper();
            }
            return labelText;
        }

        // First letter of each sentence becomes uppercase.  e.g.  "This is an example. Another sentence"
        static string ToSentenceCase( string text )
        {
            var sentenceRegex = new System.Text.RegularExpressions.Regex(@"(^[a-z])|[?!.:;\n]\s*(.)", System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
            return sentenceRegex.Replace(text.ToLower(), s => s.Value.ToUpper());
        }

        static string ToTitleCase( string text )
        {
            #if NETFX_CORE
			    var sb = new System.Text.StringBuilder(text);
			    sb[0] = char.ToUpper(sb[0]);
			    for (int i = 1, imax=text.Length; i<imax; ++i)
			    {
				    if (char.IsWhiteSpace(sb[i - 1]))
					    sb[i] = char.ToUpper(sb[i]);
				    else
					    sb[i] = char.ToLower(sb[i]);
			    }
			    return sb.ToString();
            #else
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
            #endif
        }
   }
}