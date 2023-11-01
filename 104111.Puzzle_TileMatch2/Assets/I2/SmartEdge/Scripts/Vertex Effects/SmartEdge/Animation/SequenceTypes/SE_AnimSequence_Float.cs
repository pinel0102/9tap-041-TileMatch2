using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace I2.SmartEdge
{
    [Serializable]
    public class SE_AnimSequence_Float : SE_AnimSequence
    {
        public float _From, _FromRandom;
        public float _To=1,   _ToRandom;
        public enum eAnimBlendMode { Offset, Replace, Blend };
        public eAnimBlendMode _AnimBlend_From = eAnimBlendMode.Replace;
        public eAnimBlendMode _AnimBlend_To = eAnimBlendMode.Replace;

        public enum eAlphaTarget { Global, Face, Outline, Glow, Shadow }
        public eAlphaTarget _Target = eAlphaTarget.Global;

        public override void UpdateSequence(float dt, SmartEdge se, SE_Animation anim, int sequenceIndex, ref bool makeMaterialDirty, ref bool makeVerticesDirty) 
        { 
            base.UpdateSequence(dt, se, anim, sequenceIndex, ref makeMaterialDirty, ref makeVerticesDirty);
            makeVerticesDirty = true;
        }

        public override void Apply_Characters(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            if (_Target != eAlphaTarget.Global)
                return;

            if (anim.mTime < mDelay && !_InitAllElements)
                return;

            bool applyRandomFrom = HasRandom(_FromRandom);
            bool applyRandomTo = HasRandom(_ToRandom);
            DRandom.mCurrentSeed = GetRandomSeed(anim, sequenceIndex);


            // Iterate through all the valid range
            for (int iElement = mElementRangeStart; iElement < mElementRangeEnd; ++iElement)
            {
                float progress = GetProgress(anim.mTime, anim, iElement);
                if (!_InitAllElements && progress < 0)
                    continue;
                progress = progress < 0 ? 0 : progress;

                progress = _EasingCurve.Evaluate(progress);
                var currentAlpha = SmartEdge.mOriginalVertices.Buffer[iElement * 4].color.a;

                //--[ From ]----------------------------
                float aFrom = _From*255;
                if (_AnimBlend_From == eAnimBlendMode.Offset) aFrom = aFrom + currentAlpha;
                if (_AnimBlend_From == eAnimBlendMode.Blend) aFrom = _From * currentAlpha;

                if (applyRandomFrom)
                    aFrom += 255*_FromRandom * DRandom.GetUnit(iElement);

                //--[ To ]----------------------------
                float aTo = 255*_To;
                if (_AnimBlend_To == eAnimBlendMode.Offset) aTo = (currentAlpha + _To);
                if (_AnimBlend_To == eAnimBlendMode.Blend) aTo = (currentAlpha * _To);


                if (applyRandomTo)
                    aTo += 255*_ToRandom * DRandom.GetUnit(iElement*2+90);


                // Find Alpha for this Character
                float falpha = (aFrom + (aTo - aFrom) * progress);
                byte alpha = (byte)(falpha<0?0 : falpha>255?255: falpha);


                // Apply to all Vertices
                for (int v=iElement*4; v<iElement*4+4; ++v)
                    SmartEdge.mOriginalVertices.Buffer[v].color.a = alpha;
            }
        }

        public override void Apply_Vertices(SmartEdge se, SE_Animation anim, int sequenceIndex)
        {
            if (_Target == eAlphaTarget.Global)
                return;

            if (anim.mTime < mDelay && !_InitAllElements)
                return;

            bool applyRandomFrom = HasRandom(_FromRandom);
            bool applyRandomTo = HasRandom(_ToRandom);
            DRandom.mCurrentSeed = GetRandomSeed(anim, sequenceIndex);


            // Iterate through all the valid range
            for (int layer = 0; layer < SmartEdge.mSEMesh.numLayers; ++layer)
            {
                var layerType = (SEVerticesLayers)layer;
                if (_Target == eAlphaTarget.Face && layerType != SEVerticesLayers.Face)
                    continue;
                if (_Target == eAlphaTarget.Outline && layerType != SEVerticesLayers.Face && layerType != SEVerticesLayers.Outline)
                    continue;
                if (_Target == eAlphaTarget.Glow && layerType != SEVerticesLayers.Glow_Back && layerType != SEVerticesLayers.Glow_Front)
                    continue;
                if (_Target == eAlphaTarget.Shadow && layerType != SEVerticesLayers.Shadow)
                    continue;

                var layerVertices = SmartEdge.mSEMesh.mLayers[layer];

                for (int i = 0; i < layerVertices.Size; i += 4)
                {
                    int iElement = layerVertices.Buffer[i].characterID;
                    if (iElement < mElementRangeStart || iElement >= mElementRangeEnd)
                        continue;

                    float progress = GetProgress(anim.mTime, anim, iElement);
                    if (!_InitAllElements && progress < 0)
                        continue;
                    progress = progress < 0 ? 0 : progress;

                    progress = _EasingCurve.Evaluate(progress);
                    byte currentValue = (byte)0;

                    switch (_Target)
                    {
                        case eAlphaTarget.Face:     currentValue = layerVertices.Buffer[i].color.a; break;
                        case eAlphaTarget.Outline:  currentValue = layerVertices.Buffer[i].outlineColor.a; break;
                        case eAlphaTarget.Glow:     currentValue = layerVertices.Buffer[i].color.a; break;
                        case eAlphaTarget.Shadow:   currentValue = layerVertices.Buffer[i].color.a; break;
                    }

                    //--[ From ]----------------------------
                    float vFrom = _From * 255;
                    if (_AnimBlend_From == eAnimBlendMode.Offset) vFrom = vFrom + currentValue;
                    if (_AnimBlend_From == eAnimBlendMode.Blend) vFrom = _From * currentValue;

                    if (applyRandomFrom)
                        vFrom += 255 * _FromRandom * DRandom.GetUnit(iElement);

                    //--[ To ]----------------------------
                    float vTo = 255 * _To;
                    if (_AnimBlend_To == eAnimBlendMode.Offset) vTo = (currentValue + _To);
                    if (_AnimBlend_To == eAnimBlendMode.Blend) vTo = (currentValue * _To);


                    if (applyRandomTo)
                        vTo += 255 * _ToRandom * DRandom.GetUnit(iElement * 2+90);


                    // Find Alpha for this Character
                    float falpha = (vFrom + (vTo - vFrom) * progress);
                    byte newValue = (byte)(falpha < 0 ? 0 : falpha > 255 ? 255 : falpha);

                    bool hasOutlineLayer = SmartEdge.mSEMesh.mLayers[(int)SEVerticesLayers.Outline].Size>0;
                    // Apply to all Vertices
                    for (int v = i; v < i + 4; ++v)
                    {
                        switch (_Target)
                        {
                            case eAlphaTarget.Face: layerVertices.Buffer[v].color.a = newValue; break;
                            case eAlphaTarget.Outline:  layerVertices.Buffer[v].byte5 = (layerType == SEVerticesLayers.Outline || !hasOutlineLayer) ? newValue : (byte)0;
                                                        break;
                            case eAlphaTarget.Glow: layerVertices.Buffer[v].color.a = newValue; break;
                            case eAlphaTarget.Shadow: layerVertices.Buffer[v].color.a = newValue; break;
                        }
                    }
                }
            }
        }

        public bool HasRandom(float maxRandom) { return maxRandom < -0.001f || maxRandom > 0.001f; }


#if UNITY_EDITOR
        public override void InspectorGUI()
        {
            var color = GUI.color;
            GUILayout.BeginHorizontal();
                _From = EditorGUILayout.Slider("From", _From, 0, 1);
                _AnimBlend_From = (eAnimBlendMode)EditorGUILayout.EnumPopup(_AnimBlend_From, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUI.color = HasRandom(_FromRandom) ? color : new Color(color.r, color.g, color.b, color.a * 0.5f);
            GUILayout.BeginHorizontal();
                _FromRandom = EditorGUILayout.Slider("Random", _FromRandom, 0, 1);
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    _FromRandom = 0;
                    GUIUtility.keyboardControl = -1;
                }
                GUILayout.Space(81);
            GUILayout.EndHorizontal();
            GUI.color = color;



            GUILayout.Space(15);


            GUILayout.BeginHorizontal();
                _To = EditorGUILayout.Slider("To", _To, 0, 1);
                _AnimBlend_To = (eAnimBlendMode)EditorGUILayout.EnumPopup(_AnimBlend_To, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUI.color = HasRandom(_ToRandom) ? color : new Color(color.r, color.g, color.b, color.a * 0.5f);
            GUILayout.BeginHorizontal();
                _ToRandom = EditorGUILayout.Slider("Random", _ToRandom, 0, 1);
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    _ToRandom = 0;
                    GUIUtility.keyboardControl = -1;
                }
            GUILayout.Space(81);
            GUILayout.EndHorizontal();
            GUI.color = color;

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
                _EasingCurve = EditorGUILayout.CurveField("Easing", _EasingCurve);
            GUILayout.EndHorizontal();
        }
#endif
    }
}
