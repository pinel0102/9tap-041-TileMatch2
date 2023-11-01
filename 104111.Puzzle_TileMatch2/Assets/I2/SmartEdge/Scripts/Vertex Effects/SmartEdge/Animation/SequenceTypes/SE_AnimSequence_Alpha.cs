using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace I2.SmartEdge
{
    [Serializable]
    public class SE_AnimSequence_Alpha : SE_AnimSequence_Float
    {
        public bool _OnFinish_SetAlphaToFinalValue;  // When the animation finishes, sets the SmartEdge.color to the final value

        public override string GetTypeName() { return "Alpha";  }


        public override void Apply_Characters(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            _Target = eAlphaTarget.Global;
            base.Apply_Characters(se, anim, sequenceIndex);
        }

        public override void Apply_Vertices(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            _Target = eAlphaTarget.Global;
            base.Apply_Vertices(se, anim, sequenceIndex);
        }

        public override void OnStop(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            if (!Application.isPlaying || !_OnFinish_SetAlphaToFinalValue)
                return;

            var currentAlpha = se.mWidgetColor.a;

            float t = anim._Backwards ? 0 : 1;

            var progress = _EasingCurve.Evaluate(t);

            //--[ From ]----------------------------
            float aFrom = _From * 255;
            if (_AnimBlend_From == eAnimBlendMode.Offset) aFrom = aFrom + currentAlpha;
            if (_AnimBlend_From == eAnimBlendMode.Blend) aFrom = _From * currentAlpha;

            if (HasRandom(_FromRandom))
                aFrom += 255 * _FromRandom * DRandom.GetUnit(0);

            //--[ To ]----------------------------
            float aTo = 255 * _To;
            if (_AnimBlend_To == eAnimBlendMode.Offset) aTo = (currentAlpha + _To);
            if (_AnimBlend_To == eAnimBlendMode.Blend) aTo = (currentAlpha * _To);


            if (HasRandom(_ToRandom))
                aTo += 255 * _ToRandom * DRandom.GetUnit(0* 2+90);

            // Find Alpha for this Character
            float falpha = (aFrom + (aTo - aFrom) * progress);
            byte alpha = (byte)(falpha < 0 ? 0 : falpha > 255 ? 255 : falpha);

            var color = se.mWidgetColor;
            color.a = alpha;
            se.SetWidgetColor(color);
        }


#if UNITY_EDITOR
        public override void InspectorGUI()
        {
            _OnFinish_SetAlphaToFinalValue = GUILayout.Toggle(_OnFinish_SetAlphaToFinalValue, new GUIContent("On Finish: SetAlphaToFinalValue", "When the animation finishes, sets the SmartEdge.color to the final value"));
            GUILayout.Space(5);
            base.InspectorGUI();
        }
#endif

    }

    [Serializable]
    public class SE_AnimSequence_AlphaFace : SE_AnimSequence_Float
    {
        public override string GetTypeName() { return "Alpha Face"; }


        public override void Apply_Characters(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            _Target = eAlphaTarget.Face;
            base.Apply_Characters(se, anim, sequenceIndex);
        }

        public override void Apply_Vertices(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            _Target = eAlphaTarget.Face;
            base.Apply_Vertices(se, anim, sequenceIndex);
        }
    }


    [Serializable]
    public class SE_AnimSequence_AlphaOutline : SE_AnimSequence_Float
    {
        public override string GetTypeName() { return "Alpha Outline"; }


        public override void Apply_Characters(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            _Target = eAlphaTarget.Outline;
            base.Apply_Characters(se, anim, sequenceIndex);
        }

        public override void Apply_Vertices(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            _Target = eAlphaTarget.Outline;
            base.Apply_Vertices(se, anim, sequenceIndex);
        }
    }

    [Serializable]
    public class SE_AnimSequence_AlphaGlow : SE_AnimSequence_Float
    {
        public override string GetTypeName() { return "Alpha Glow"; }


        public override void Apply_Characters(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            _Target = eAlphaTarget.Glow;
            base.Apply_Characters(se, anim, sequenceIndex);
        }

        public override void Apply_Vertices(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            _Target = eAlphaTarget.Glow;
            base.Apply_Vertices(se, anim, sequenceIndex);
        }
    }

    [Serializable]
    public class SE_AnimSequence_AlphaShadow : SE_AnimSequence_Float
    {
        public override string GetTypeName() { return "Alpha Shadow"; }


        public override void Apply_Characters(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            _Target = eAlphaTarget.Shadow;
            base.Apply_Characters(se, anim, sequenceIndex);
        }

        public override void Apply_Vertices(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            _Target = eAlphaTarget.Shadow;
            base.Apply_Vertices(se, anim, sequenceIndex);
        }
    }
}
