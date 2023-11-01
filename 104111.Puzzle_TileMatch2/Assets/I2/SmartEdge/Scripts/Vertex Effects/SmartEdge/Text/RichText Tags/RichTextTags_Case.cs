using UnityEngine;
using System.Collections.Generic;

using System;
using System.Text.RegularExpressions;

namespace I2.SmartEdge
{
    // [case=upper] or [uppercase] [lowercase] [titlecase] [smallcaps]
    class RichTextTag_Case : RichTextTag
    {
        public static void Initialize()
        {
            SE_TextEffect_RichText.mTagDescriptors["case"] = new RichTextTagDesc() { TagType = typeof(RichTextTag_Case), OnCreateTag = CreateTagCase };
            SE_TextEffect_RichText.mTagDescriptors["uppercase"] = new RichTextTagDesc() { TagType = typeof(RichTextTag_Case), OnCreateTag = CreateTagUpperCase };
            SE_TextEffect_RichText.mTagDescriptors["lowercase"] = new RichTextTagDesc() { TagType = typeof(RichTextTag_Case), OnCreateTag = CreateTagLowerCase };
            SE_TextEffect_RichText.mTagDescriptors["sentence"] = new RichTextTagDesc() { TagType = typeof(RichTextTag_Case), OnCreateTag = CreateTagSentence };
            SE_TextEffect_RichText.mTagDescriptors["titlecase"] = new RichTextTagDesc() { TagType = typeof(RichTextTag_Case), OnCreateTag = CreateTagTitleCase };
        }

        static bool CreateTagCase(Match match, List<RichTextTag> tags, int index)
        {
            var value = match.Groups["value"];
            if (value == null) return false;
            else
            if (string.Compare(value.Value, "uppercase", StringComparison.OrdinalIgnoreCase) == 0) SE_TextEffect_RichText.mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.UpperCase;
            else
            if (string.Compare(value.Value, "lowercase", StringComparison.OrdinalIgnoreCase) == 0) SE_TextEffect_RichText.mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.LowerCase;
            else
            if (string.Compare(value.Value, "sentence", StringComparison.OrdinalIgnoreCase) == 0) SE_TextEffect_RichText.mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.Sentence;
            else
            if (string.Compare(value.Value, "tilecase", StringComparison.OrdinalIgnoreCase) == 0) SE_TextEffect_RichText.mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.TitleCase;
            else
                return false;

            tags.Add(new RichTextTag_Case() { iCharacter = index });
            return true;
        }

        static bool CreateTagUpperCase(Match match, List<RichTextTag> tags, int index)
        {
            SE_TextEffect_RichText.mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.UpperCase;
            tags.Add(new RichTextTag_Case() { iCharacter = index });
            return true;
        }
        static bool CreateTagLowerCase(Match match, List<RichTextTag> tags, int index)
        {
            SE_TextEffect_RichText.mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.LowerCase;
            tags.Add(new RichTextTag_Case() { iCharacter = index });
            return true;
        }

        static bool CreateTagSentence(Match match, List<RichTextTag> tags, int index)
        {
            SE_TextEffect_RichText.mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.Sentence;
            tags.Add(new RichTextTag_Case() { iCharacter = index });
            return true;
        }

        static bool CreateTagTitleCase(Match match, List<RichTextTag> tags, int index)
        {
            SE_TextEffect_RichText.mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.TitleCase;
            tags.Add(new RichTextTag_Case() { iCharacter = index });
            return true;
        }


        public static bool IsClosingCaseTag(Match match)
        {
            string val = match.Value;
            return string.Compare(val, 2, "case", 0, "case".Length, StringComparison.OrdinalIgnoreCase) == 0 ||
                   string.Compare(val, 2, "uppercase", 0, "case".Length, StringComparison.OrdinalIgnoreCase) == 0 ||
                   string.Compare(val, 2, "lowercase", 0, "case".Length, StringComparison.OrdinalIgnoreCase) == 0 ||
                   string.Compare(val, 2, "sentence", 0, "case".Length, StringComparison.OrdinalIgnoreCase) == 0 ||
                   string.Compare(val, 2, "titlecase", 0, "case".Length, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public override void Apply(SmartEdge se, int c, int iTag)
        {
        }
    }
   
}