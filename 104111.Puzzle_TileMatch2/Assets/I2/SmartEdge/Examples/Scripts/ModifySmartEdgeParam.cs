using UnityEngine;
using System.Collections;

namespace I2.SmartEdge
{
    public class ModifySmartEdgeParam : MonoBehaviour
    {
        public SmartEdge _SmartEdge;
        public void ModifyRenderParameter( float value )
        {
            // Same approach can be used to modify any other parameter
            _SmartEdge.GetRenderParams()._GlowOuterWidth = value;
            _SmartEdge.MarkWidgetAsChanged(true, true);
        }
    }
}