using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	[System.Serializable]
	public class DeformationEffect
	{
        public SE_Deformation_FreeTransform _FreeTransform = new SE_Deformation_FreeTransform();

// 		public float _Roundness = 0;
// 		public float _SphereRadius = 50;
// 		public float _SphereStart = 0;
// 		
// 		public float _RangeMin=0, _RangeMax=1, _RangeSize=10;

		
		public int ModifyVertices (SEMesh mesh, int firstLayer, SmartEdge se)
		{
            if (_FreeTransform._Enabled)
                _FreeTransform.ModifyVertices(mesh, firstLayer, se);

            return 0;
		}		
		
// 		public Vector3 ApplySphericalTransform( Vector3 vPos )
// 		{
// 			if (_Roundness<=0)
// 				return vPos;
// 
//             float radius = _SphereRadius >= 0 ? _SphereRadius : -_SphereRadius; // Abs(r)
// 
//             float SpherePerimeter = 2 * Mathf.PI * radius;
// 			float t = _SphereStart + vPos.x / SpherePerimeter;
// 			float angle = t * 2 * Mathf.PI;
// 			
// 			Vector3 vDir = (new Vector3 (Mathf.Sin (angle), Mathf.Cos (angle), 0)).normalized;
// 			Vector2 vSpherePos = -Vector3.up * _SphereRadius + vDir * (_SphereRadius + vPos.y);
// 			return Vector3.Lerp (vPos, vSpherePos, _Roundness);
// 		}
	}
}