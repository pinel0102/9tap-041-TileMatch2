using UnityEngine;
using System.Collections.Generic;

using System;
using System.Text.RegularExpressions;

namespace I2.SmartEdge
{
    // [-]  or [/]  or [\]
    class RichTextTag_CloseAll : RichTextTag
    {
        public static void Initialize()
        {
            SE_TextEffect_RichText.mTagDescriptors[""] = new RichTextTagDesc() { TagType=null, OnCreateTag = CreateTag };
        }

        public static bool CreateTag(Match match, List<RichTextTag> tags, int index)
        {
            
            if (match.Value.Length == 3)
            {
                SE_TextEffect_RichText.mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.None;
                tags.Add(new RichTextTag_CloseAll() { iCharacter = index });
                return true;
            }
            return false;
        }
        public override void Activate( SmartEdge se, int c, int iTag )
        {
            for (int i = 0; i < iTag; ++i)
                se._TextEffect._RichText.mTags[i].State = eRichTextTagState.Closed;
        }
        public override void Apply(SmartEdge se, int c, int iTag) { }
    }


    // [/tagname]:  e.g.  [/b]  or  [/align] 
    class RichTextTag_Close : RichTextTag
    {
        System.Type tagType;

        public static bool CreateTag(Type mtype, List<RichTextTag> tags, int index)
        {
            if (mtype==typeof(RichTextTag_Case) || mtype==null)
                SE_TextEffect_RichText.mConvertCase = SE_TextEffect_TextWrapping.eTextCaseType.None;

            if (mtype==null)
                tags.Add(new RichTextTag_CloseAll() { iCharacter = index });
            else
                tags.Add(new RichTextTag_Close() { tagType = mtype, iCharacter = index });
            return true;
        }
        public override void Activate( SmartEdge se, int c, int iTag )
        {
            var mTags = se._TextEffect._RichText.mTags;

            // queue any tag of the same type (nested tags)
            for (iTag--; iTag >= 0; iTag--)
                if (mTags[iTag].GetType() == tagType && mTags[iTag].State==eRichTextTagState.Active)
                {
                    mTags[iTag].State = eRichTextTagState.Closed;
                    break;
                }

            // restore any queued tag of the same type
            for (iTag--; iTag >= 0; iTag--)
                if (mTags[iTag].GetType() == tagType && mTags[iTag].State == eRichTextTagState.Queued)
                {
                    mTags[iTag].State = eRichTextTagState.Active;
                    break;
                }
        }
        public override void Apply(SmartEdge se, int c, int iTag) { }
    }
}