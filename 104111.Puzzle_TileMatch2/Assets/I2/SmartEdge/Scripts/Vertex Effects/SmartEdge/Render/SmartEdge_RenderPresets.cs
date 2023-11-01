using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
    public partial class SmartEdge
	{
        public SmartEdgeRenderParams _RenderParams = new SmartEdgeRenderParams();
        public SmartEdgeRenderPreset[] _RenderPresets = null;

        public SmartEdgeRenderParams GetRenderParams( int index = 0 )
        {
            if (_RenderPresets==null || _RenderPresets.Length <= index || _RenderPresets[index]==null)
                return _RenderParams;

            return _RenderPresets[index]._RenderParams;
        }

        public SmartEdgeRenderParams GetRenderParams( string Name )
        {
            if (_RenderPresets == null) return null;

            foreach (var preset in _RenderPresets)
                if (preset != null && preset.name==Name)
                    return preset._RenderParams;

            return null;
        }

        public void SetRenderPreset(SmartEdgeRenderPreset preset) 
        { 
            SetRenderPreset(0, preset); 
        }

        public void SetRenderPreset(int index, SmartEdgeRenderPreset preset)
        {
            if (_RenderPresets != null && _RenderPresets.Length > index && _RenderPresets[index] != null)
                _RenderPresets[index].UnRegisterRenderPresetDependency(this);

            if (preset == null)
            {
                if (_RenderPresets == null)
                    return;

                // If its the last element, shrink the array
                if (index == _RenderPresets.Length - 1)
                {
                    if (index == 0)
                        _RenderPresets = null;
                    else
                        System.Array.Resize(ref _RenderPresets, index + 1);
                }
                else
                    _RenderPresets[index] = null;
            }

            if (preset!=null)
            {
                if (_RenderPresets == null)
                    _RenderPresets = new SmartEdgeRenderPreset[index + 1];
                else
                if (_RenderPresets.Length <= index)
                    Array.Resize(ref _RenderPresets, index + 1);

                if (_RenderPresets[index] == preset)
                    return;
                else
                    _RenderPresets[index] = preset;

                preset.RegisterRenderPresetDependency(this);
            }

            MarkWidgetAsChanged(true, true);
        }
    }
}
