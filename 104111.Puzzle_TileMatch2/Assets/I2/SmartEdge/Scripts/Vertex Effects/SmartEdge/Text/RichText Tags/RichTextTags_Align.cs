using UnityEngine;
using System.Collections.Generic;

using System;
using System.Text.RegularExpressions;

namespace I2.SmartEdge
{
    // [align=xxx]   where   xxx= left|center|right|justified
    public class RichTextTag_Align : RichTextTag
    {
        public enum eAlignmentOverride { Automatic, Left, Center, Right, Justified };
        public eAlignmentOverride Alignment = eAlignmentOverride.Automatic;

        public static void Initialize()
        {
            SE_TextEffect_RichText.mTagDescriptors["align"]     = new RichTextTagDesc() { TagType = typeof(RichTextTag_Align), OnCreateTag = CreateTag }; 
            SE_TextEffect_RichText.mTagDescriptors["left"]      = new RichTextTagDesc() { TagType = typeof(RichTextTag_Align), OnCreateTag = CreateTagLeft };
            SE_TextEffect_RichText.mTagDescriptors["center"]    = new RichTextTagDesc() { TagType = typeof(RichTextTag_Align), OnCreateTag = CreateTagCenter };
            SE_TextEffect_RichText.mTagDescriptors["right"]     = new RichTextTagDesc() { TagType = typeof(RichTextTag_Align), OnCreateTag = CreateTagRight };
            SE_TextEffect_RichText.mTagDescriptors["justified"] = new RichTextTagDesc() { TagType = typeof(RichTextTag_Align), OnCreateTag = CreateTagJustified };
        }

        static bool CreateTag(Match match, List<RichTextTag> tags, int index)
        {
            var value = match.Groups["value"];
            if (value == null) return false;
            else
            if (string.Compare(value.Value, "left", StringComparison.OrdinalIgnoreCase) == 0) tags.Add(new RichTextTag_Align() { Alignment = eAlignmentOverride.Left, iCharacter = index });
            else
            if (string.Compare(value.Value, "right", StringComparison.OrdinalIgnoreCase) == 0) tags.Add(new RichTextTag_Align() { Alignment = eAlignmentOverride.Right, iCharacter = index });
            else
            if (string.Compare(value.Value, "center", StringComparison.OrdinalIgnoreCase) == 0) tags.Add(new RichTextTag_Align() { Alignment = eAlignmentOverride.Center, iCharacter = index });
            else
            if (string.Compare(value.Value, "justified", StringComparison.OrdinalIgnoreCase) == 0) tags.Add(new RichTextTag_Align() { Alignment = eAlignmentOverride.Justified, iCharacter = index });
            else
                return false;

            return true;
        }

        static bool CreateTagLeft(Match match, List<RichTextTag> tags, int index)
        {
            tags.Add(new RichTextTag_Align() { Alignment = eAlignmentOverride.Left, iCharacter = index });
            return true;
        }

        static bool CreateTagRight(Match match, List<RichTextTag> tags, int index)
        {
            tags.Add(new RichTextTag_Align() { Alignment = eAlignmentOverride.Right, iCharacter = index });
            return true;
        }

        static bool CreateTagCenter(Match match, List<RichTextTag> tags, int index)
        {
            tags.Add(new RichTextTag_Align() { Alignment = eAlignmentOverride.Center, iCharacter = index });
            return true;
        }

        static bool CreateTagJustified(Match match, List<RichTextTag> tags, int index)
        {
            tags.Add(new RichTextTag_Align() { Alignment = eAlignmentOverride.Justified, iCharacter = index });
            return true;
        }



        public override void Apply(SmartEdge se, int c, int iTag)
        {
            if (State != eRichTextTagState.Active)
                return;
            SmartEdge.mCharacters.Buffer[iCharacter].RichText.Alignment = Alignment; 
        }
    }
}