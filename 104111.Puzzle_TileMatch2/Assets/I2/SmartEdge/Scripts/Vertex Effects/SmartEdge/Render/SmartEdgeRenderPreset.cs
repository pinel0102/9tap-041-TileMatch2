using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
#if !UNITY_5_0
    [CreateAssetMenu(fileName = "RenderPreset", menuName = "SmartEdge/Render Preset", order = 1)]
#endif
    public class SmartEdgeRenderPreset : ScriptableObject
	{
        public SmartEdgeRenderParams _RenderParams;

        #region RenderPreset Dependencies 

        List<SmartEdge> mRenderPresetDependants = new List<SmartEdge>();
        public void RegisterRenderPresetDependency(SmartEdge child)
        {
            if (mRenderPresetDependants!=null && !mRenderPresetDependants.Contains(child) && child!=null)
                mRenderPresetDependants.Add(child);
        }

        public void UnRegisterRenderPresetDependency(SmartEdge child)
        {
            if (mRenderPresetDependants == null)
                return;
            mRenderPresetDependants.Remove(child);
            mRenderPresetDependants.RemoveAll(x=>x == null);
        }

        public void NotifyRenderPresetChanged(bool materialDirty, bool verticesDirty)
        {
            if (mRenderPresetDependants == null)
                return;
            mRenderPresetDependants.RemoveAll(x => x == null);
            for (int i = 0, imax = mRenderPresetDependants.Count; i < imax; ++i)
                mRenderPresetDependants[i].MarkWidgetAsChanged(verticesDirty, materialDirty);
        }
        #endregion
    }
}
