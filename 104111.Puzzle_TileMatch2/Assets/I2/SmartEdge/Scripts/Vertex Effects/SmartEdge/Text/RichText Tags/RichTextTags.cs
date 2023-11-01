using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace I2.SmartEdge
{
    public abstract class RichTextTag
    {
        public int iCharacter;

        public enum eRichTextTagState { Waiting, Active, Queued, Closed }
        public eRichTextTagState State = eRichTextTagState.Waiting;

        public virtual void Activate( SmartEdge se, int c, int iTag )
        {
            // queue any tag of the same type (nested tags)
            var mtype = GetType();
            var mTags = se._TextEffect._RichText.mTags;

            for (int i = iTag - 1; i >= 0; --i)
            {
                if (mTags[i].GetType() == mtype && mTags[i].State==eRichTextTagState.Active)
                {
                    mTags[i].State = eRichTextTagState.Queued;
                    break;
                }
            }

            mTags[iTag].State = eRichTextTagState.Active;
        }

        public virtual void Apply(SmartEdge se, int c, int iTag) {}
        public virtual void FinishApplying(SmartEdge se, int iTag) {}


        public virtual void Reset(SmartEdge se)
        {
            State = eRichTextTagState.Waiting;
        }

        public static bool IsCloseTag(Match match)
        {
            var ch = match.Value[1];
            if (ch != '/' && ch != '\\' && ch != '-')
                return false;
            if (!string.IsNullOrEmpty(match.Groups["value"].Value))
                return false;
            return true;
        }

    }


    public class RichTextTagDesc
    {
        public delegate bool FnCreateTag(Match match, List<RichTextTag> tags, int iCharacter);

        public System.Type TagType;
        public FnCreateTag OnCreateTag;
    }
}