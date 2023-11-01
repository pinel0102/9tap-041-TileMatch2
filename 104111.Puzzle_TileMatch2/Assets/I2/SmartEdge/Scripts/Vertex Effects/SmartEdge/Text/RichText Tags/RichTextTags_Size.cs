using UnityEngine;
using System.Collections.Generic;

using System;
using System.Text.RegularExpressions;

namespace I2.SmartEdge
{
    // [size=xx]
    class RichTextTag_Size : RichTextTag
    {
        public float ScaleX = 1;
        public float ScaleY = 1;

        float mOffsetX;//, mOffsetY;
        int iCurrentLine;

        public static void Initialize()
        {
            var desc = new RichTextTagDesc() { TagType = typeof(RichTextTag_Size), OnCreateTag = CreateTag };
            SE_TextEffect_RichText.mTagDescriptors["size"] = desc;
        }

        static bool CreateTag(Match match, List<RichTextTag> tags, int index)
        {
            float size;
            if (!float.TryParse(match.Groups["value"].Value, out size))
                return false;

            tags.Add( new RichTextTag_Size() { iCharacter = index, ScaleX = size, ScaleY = size } );
            return true;
        }

        public override void Activate(SmartEdge se, int c, int iTag)
        {
            base.Activate(se, c, iTag);

            mOffsetX = 0;
            //mOffsetY = 0;
            iCurrentLine = SmartEdge.mCharacters.Buffer[c].iLine;
        }


        public override void Apply(SmartEdge se, int c, int iTag)
        {
            if (State == eRichTextTagState.Waiting)
                return;

            float curOffsetX = 0;

            if (State == eRichTextTagState.Active)
            {
                var baseline = SmartEdge.mCharacters.Buffer[c].BaselineY;

                var sizeX = (SmartEdge.mCharacters.Buffer[c].Max.x - SmartEdge.mCharacters.Buffer[c].Min.x);
                curOffsetX = sizeX * ScaleX - sizeX;

                SmartEdge.mOriginalVertices.Buffer[c * 4  ].position.y = baseline + (SmartEdge.mOriginalVertices.Buffer[c * 4  ].position.y-baseline)*ScaleY;
                SmartEdge.mOriginalVertices.Buffer[c * 4+1].position.y = baseline + (SmartEdge.mOriginalVertices.Buffer[c * 4+1].position.y-baseline)*ScaleY;
                SmartEdge.mOriginalVertices.Buffer[c * 4+2].position.y = baseline + (SmartEdge.mOriginalVertices.Buffer[c * 4+2].position.y-baseline)*ScaleY;
                SmartEdge.mOriginalVertices.Buffer[c * 4+3].position.y = baseline + (SmartEdge.mOriginalVertices.Buffer[c * 4+3].position.y-baseline)*ScaleY;

                SmartEdge.mCharacters.Buffer[c].Height *= ScaleY;
                SmartEdge.mCharacters.Buffer[c].TopY   = baseline + (SmartEdge.mCharacters.Buffer[c].TopY -baseline)*ScaleY;
                SmartEdge.mCharacters.Buffer[c].Max.y  = baseline + (SmartEdge.mCharacters.Buffer[c].Max.y-baseline)*ScaleY;
                SmartEdge.mCharacters.Buffer[c].Min.y  = baseline + (SmartEdge.mCharacters.Buffer[c].Min.y-baseline)*ScaleY;
            }

            // Reset the values if we reached a new line
            if (SmartEdge.mCharacters.Buffer[c].iLine != iCurrentLine)
            {
                mOffsetX = 0;
                iCurrentLine = SmartEdge.mCharacters.Buffer[c].iLine;
            }

            // Apply Horizontal offset
            SmartEdge.mOriginalVertices.Buffer[c * 4].position.x += mOffsetX;                          // TopLeft
            SmartEdge.mOriginalVertices.Buffer[c * 4 + 1].position.x += mOffsetX + curOffsetX;         // TopRight
            SmartEdge.mOriginalVertices.Buffer[c * 4 + 2].position.x += mOffsetX + curOffsetX;         // BottomRight
            SmartEdge.mOriginalVertices.Buffer[c * 4 + 3].position.x += mOffsetX;                      // BottomLeft
            SmartEdge.mCharacters.Buffer[c].Min.x += mOffsetX;
            SmartEdge.mCharacters.Buffer[c].Max.x += mOffsetX + curOffsetX;

            mOffsetX += curOffsetX;
        }
    }

}