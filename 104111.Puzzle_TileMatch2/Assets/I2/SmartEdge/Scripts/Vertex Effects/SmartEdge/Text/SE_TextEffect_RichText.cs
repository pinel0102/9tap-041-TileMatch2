using UnityEngine;
using System.Collections.Generic;

using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace I2.SmartEdge
{
    [System.Serializable]
    public class SE_TextEffect_RichText
    {
        public bool _Enabled = false;

        static Regex mRichTextPattern;
        static System.Text.StringBuilder mStringBuilder = new System.Text.StringBuilder();

        public static Dictionary<string, RichTextTagDesc> mTagDescriptors = new Dictionary<string, RichTextTagDesc>();

        [System.NonSerialized]public List<RichTextTag> mTags = new List<RichTextTag>();

        public static SE_TextEffect_TextWrapping.eTextCaseType mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.None;



        public void ModifyVertices(SmartEdge se)
        {
            if (!_Enabled || SmartEdge.mCharacters.Size==0)
                return;
            ApplyTags_ModifyVertices(se);
        }


        public bool ModifyText( ref string labelText )
        {
            if (string.IsNullOrEmpty(labelText))
                return false;

            // Initialize all the Tag Finding
            if (mRichTextPattern == null)
                BuildPattern();

            // Parse the text, adds all the tags to the mCurrentTags list and 
            // the mStringBuilder will contain the text without the tags
            ParseTags( labelText );

            if (mTags.Count>0)
            {
                labelText = mStringBuilder.ToString();
                return true;
            }

            return false;
        }

        void ParseTags( string text )
        {
            int lastPos = 0; // this is used to skip the tags and copy the text in-between tags
            mStringBuilder.Length = 0;
            mTags.Clear();

            mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.None;
            var mCurrentCase = SE_TextEffect_TextWrapping.eTextCaseType.None;

            // Find everything that matches an opening or closing tag
            foreach (Match match in mRichTextPattern.Matches(text))
            {
                // The tag name is stored in the capture named 'tag'
                var tagName = match.Groups["tag"].Value ?? string.Empty;

                // Find the Tag type
                RichTextTagDesc desc;
                if (!mTagDescriptors.TryGetValue(tagName, out desc))
                    continue;

                var iCharacter = mStringBuilder.Length + match.Index - lastPos;
                if (RichTextTag.IsCloseTag(match))
                    RichTextTag_Close.CreateTag(desc.TagType, mTags, iCharacter);
                else
                if (!desc.OnCreateTag(match, mTags, iCharacter)) // Try creating that tag, will return null if it has no valid parameters e.g. [align=123] instead of [align=left]
                    continue;

                // Concatenate the string with the text up to this tag
                if (match.Index > lastPos)
                {
                    if (mCurrentCase == SE_TextEffect_TextWrapping.eTextCaseType.None)
                    {
                        mStringBuilder.Append(text, lastPos, match.Index - lastPos);
                    }
                    else
                    {
                        var mtext = text.Substring(lastPos, match.Index - lastPos);
                        mStringBuilder.Append( SE_TextEffect_TextWrapping.ConvertTextCase(mtext, mCurrentCase) );
                    }
                }
                if (mCurrentCase != mConvertCase)
                    mCurrentCase = mConvertCase;

                // skip the tag's text
                lastPos = match.Index + match.Length;
            }

            // concatenate the remaining text
            if (lastPos > 0)
            {
                if (mCurrentCase == SE_TextEffect_TextWrapping.eTextCaseType.None)
                {
                    mStringBuilder.Append(text, lastPos, text.Length - lastPos);
                }
                else
                {
                    var mtext = text.Substring(lastPos, text.Length - lastPos);
                    mStringBuilder.Append(SE_TextEffect_TextWrapping.ConvertTextCase(mtext, mCurrentCase));
                }
            }

        }

        void ApplyTags_ModifyVertices( SmartEdge se )
        {
            if (mTags.Count == 0)
                return;

            int iFinal = 0, iNextTag = 0;
            int finalTextLength = se.mFinalText.Length;

            for (int i = 0; i < mTags.Count; ++i)
                mTags[i].Reset(se);


            // these are use to know how much to offset each line
            float currentLineTop=float.MinValue,          newLineTop = float.MinValue,
                  currentLineBottom = float.MaxValue,     newLineBottom = float.MaxValue;

            float offsetY = 0;  // Accumulated offset of all lines
            int prevLine = 0;
            int iLineStart = 0;

            // iterate all input characters and map them to the corresponding se.mFinalText character. (SmartEdge.mCharacters doesn't have spaces, \n, and other characters that are not in the font)
            for (var c=0; c<SmartEdge.mCharacters.Size; ++c)
            {
                // Make the iFinal match the character c  (i.e.   finalText[iFinal]==SmartEdge.mCharacters[c] ) by skipping spaces and others
                char ch = SmartEdge.mCharacters.Buffer[c].Character;
                while (iFinal < finalTextLength && se.mFinalText[iFinal] != ch)
                    iFinal++;

                // If there is a new line, then apply offsets if needed
                var iLine = SmartEdge.mCharacters.Buffer[c].iLine;
                if (iLine != prevLine)
                {
                    prevLine = iLine;
                    var lineOffset = (newLineTop - currentLineTop);        // extra space on top
                    offsetY -= lineOffset;

                    if (offsetY> float.Epsilon || offsetY<float.Epsilon)
                        OffsetLine(iLineStart, c, offsetY);

                    offsetY -= (currentLineBottom - newLineBottom); // extra space in the bottom

                    currentLineTop = newLineTop = float.MinValue;
                    currentLineBottom = newLineBottom = float.MaxValue;
                    iLineStart = c;
                }

                // Apply all the tags for this character
                while (iNextTag < mTags.Count && mTags[iNextTag].iCharacter <= iFinal)
                {
                    mTags[iNextTag].Activate(se, c, iNextTag);
                    iNextTag++;
                }

                // Find the Line Top and bottom (before applying the tags)
                var linebottom = SmartEdge.mCharacters.Buffer[c].TopY - SmartEdge.mCharacters.Buffer[c].Height;
                if (currentLineTop < SmartEdge.mCharacters.Buffer[c].TopY)     currentLineTop = SmartEdge.mCharacters.Buffer[c].TopY;
                if (currentLineBottom > linebottom)                            currentLineBottom = linebottom;


                // Apply the tags
                for (var i=0; i<iNextTag; ++i)
                    mTags[i].Apply(se, c, i);

                // Find the Line Top and bottom after any of the tags grew it
                linebottom = SmartEdge.mCharacters.Buffer[c].TopY - SmartEdge.mCharacters.Buffer[c].Height;
                if (newLineTop < SmartEdge.mCharacters.Buffer[c].TopY)       newLineTop = SmartEdge.mCharacters.Buffer[c].TopY;
                if (newLineBottom > linebottom)                              newLineBottom = linebottom;

                // We already process this character, so move along
                iFinal++;
            }

            // Finalize any remaining action
            for (var i = 0; i < mTags.Count; ++i)
                mTags[i].FinishApplying(se, i);

            // Apply remaining offset
            offsetY -= (newLineTop - currentLineTop);        // extra space on top
            if (offsetY > float.Epsilon || offsetY < float.Epsilon)
                OffsetLine(iLineStart, SmartEdge.mCharacters.Size, offsetY);
        }

        void OffsetLine(int iLineStart, int iLineEnd, float offsetY )
        {
            for (var c = iLineStart; c < iLineEnd; ++c)
            {
                SmartEdge.mOriginalVertices.Buffer[c * 4].position.y     += offsetY;         // TopLeft
                SmartEdge.mOriginalVertices.Buffer[c * 4 + 1].position.y += offsetY;         // TopRight
                SmartEdge.mOriginalVertices.Buffer[c * 4 + 2].position.y += offsetY;         // BottomRight
                SmartEdge.mOriginalVertices.Buffer[c * 4 + 3].position.y += offsetY;         // BottomLeft
                SmartEdge.mCharacters.Buffer[c].Min.y     += offsetY;
                SmartEdge.mCharacters.Buffer[c].Max.y     += offsetY;
                SmartEdge.mCharacters.Buffer[c].BaselineY += offsetY;
                SmartEdge.mCharacters.Buffer[c].TopY      += offsetY;
                
                //SmartEdge.mOriginalVertices.Buffer[c * 4].position.y = SmartEdge.mOriginalVertices.Buffer[c * 4 + 1].position.y = SmartEdge.mCharacters.Buffer[c].TopY;
                //SmartEdge.mOriginalVertices.Buffer[c * 4 + 2].position.y = SmartEdge.mOriginalVertices.Buffer[c * 4 + 3].position.y = SmartEdge.mCharacters.Buffer[c].TopY - SmartEdge.mCharacters.Buffer[c].Height;
                //SmartEdge.mOriginalVertices.Buffer[c * 4].position.y = SmartEdge.mOriginalVertices.Buffer[c * 4 + 1].position.y = SmartEdge.mCharacters.Buffer[c].Max.y;
                //SmartEdge.mOriginalVertices.Buffer[c * 4 + 2].position.y = SmartEdge.mOriginalVertices.Buffer[c * 4 + 3].position.y = SmartEdge.mCharacters.Buffer[c].Min.y;
            }
        }

        void BuildPattern()
        {
            mRichTextPattern = new Regex(@"\[[\\|\/|-]?(?'tag'\w*)(?:\s*=\s*(?'value'\w+))?]");

            mTagDescriptors.Clear();
            RichTextTag_Bold.Initialize();
            RichTextTag_Italic.Initialize();
            RichTextTag_Align.Initialize();
            RichTextTag_CloseAll.Initialize();
            RichTextTag_Case.Initialize();
            RichTextTag_Size.Initialize();
        }
    }
}