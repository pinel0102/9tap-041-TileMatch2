using UnityEngine;
using System.Collections;

namespace I2.SmartEdge
{
    public class AnimateSmartEdgeGlowUV : MonoBehaviour
    {
        public Vector2 _UVSpeed;

        SmartEdge mSmartEdge;

        public void Update()
        {
            if (mSmartEdge == null)
                mSmartEdge = GetComponent<SmartEdge>();

            mSmartEdge.GetRenderParams()._GlowTexture._Offset += Time.deltaTime * _UVSpeed;
            mSmartEdge.MarkWidgetAsChanged(true, true);
        }
    }
}