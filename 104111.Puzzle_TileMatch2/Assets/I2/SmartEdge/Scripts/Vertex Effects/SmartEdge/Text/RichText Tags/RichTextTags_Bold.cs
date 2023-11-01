using UnityEngine;
using System.Collections.Generic;

using System;
using System.Text.RegularExpressions;

namespace I2.SmartEdge
{
    // [b] or [bold]
    class RichTextTag_Bold : RichTextTag
    {
        public float Amount = 0.05f;

        public static void Initialize()
        {
            var desc = new RichTextTagDesc() { TagType = typeof(RichTextTag_Bold), OnCreateTag = CreateTag };
            SE_TextEffect_RichText.mTagDescriptors["b"] = desc;
            SE_TextEffect_RichText.mTagDescriptors["bold"] = desc;
        }

        static bool CreateTag(Match match, List<RichTextTag> tags, int index)
        {
            tags.Add( new RichTextTag_Bold() { iCharacter = index } );
            return true;
        }

        public override void Apply(SmartEdge se, int c, int iTag)
        {
            if (State != eRichTextTagState.Active)
                return;

            SmartEdge.mCharacters.Buffer[iCharacter].RichText.Bold = Amount;
        }
    }
   
}