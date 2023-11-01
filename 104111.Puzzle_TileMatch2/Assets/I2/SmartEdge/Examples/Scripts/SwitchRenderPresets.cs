using UnityEngine;
using System.Collections;

namespace I2.SmartEdge
{
    [RequireComponent(typeof(SmartEdge))]
    public class SwitchRenderPresets : MonoBehaviour
    {
        public float _Time = 1;
        public SmartEdgeRenderPreset[] _Presets;

        public IEnumerator Start()
        {
            if (_Presets == null || _Presets.Length == 0)
                yield break;

            int index = 0;
            while (true)
            {
                SetPreset(_Presets[index]);
                index = (index + 1) % _Presets.Length;

                yield return new WaitForSeconds(_Time);
            }
        }

        public void SetPreset(SmartEdgeRenderPreset preset )
        {
            if (preset == null)
                return;

            var se = GetComponent<SmartEdge>();
            se.SetRenderPreset(preset);
        }
    }
}