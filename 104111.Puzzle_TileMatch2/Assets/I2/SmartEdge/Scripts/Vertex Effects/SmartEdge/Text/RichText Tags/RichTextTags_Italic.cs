using UnityEngine;
using System.Collections.Generic;

using System;
using System.Text.RegularExpressions;

namespace I2.SmartEdge
{
    // [b] or [bold]
    class RichTextTag_Italic : RichTextTag
    {

        public static void Initialize()
        {
            var desc = new RichTextTagDesc() { TagType = typeof(RichTextTag_Italic), OnCreateTag = CreateTag };
            SE_TextEffect_RichText.mTagDescriptors["i"] = desc;
            SE_TextEffect_RichText.mTagDescriptors["italic"] = desc;
        }

        static bool CreateTag(Match match, List<RichTextTag> tags, int index)
        {
            tags.Add( new RichTextTag_Italic() { iCharacter = index } );
            return true;
        }

        public override void Apply(SmartEdge se, int c, int iTag)
        {
            if (State != eRichTextTagState.Active)
                return;

            float factor = 0.32f;
            float offsetTop = (SmartEdge.mCharacters.Buffer[iCharacter].Max.y - SmartEdge.mCharacters.Buffer[iCharacter].BaselineY) * factor;
            float offsetBottom = (SmartEdge.mCharacters.Buffer[iCharacter].BaselineY - SmartEdge.mCharacters.Buffer[iCharacter].Min.y) * factor;

            SmartEdge.mOriginalVertices.Buffer[iCharacter * 4].position.x += offsetTop;
            SmartEdge.mOriginalVertices.Buffer[iCharacter * 4 + 1].position.x += offsetTop;
            SmartEdge.mOriginalVertices.Buffer[iCharacter * 4 + 2].position.x -= offsetBottom;
            SmartEdge.mOriginalVertices.Buffer[iCharacter * 4 + 3].position.x -= offsetBottom;
            SmartEdge.mCharacters.Buffer[iCharacter].Max.x += offsetTop;
        }
    }
   
}