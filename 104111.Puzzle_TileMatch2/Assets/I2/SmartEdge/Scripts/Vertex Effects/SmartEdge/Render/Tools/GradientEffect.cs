using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace I2.SmartEdge
{
    using Gradient = UnityEngine.Gradient;

    [System.Serializable]
	public partial class GradientEffect
	{
		public Gradient _Gradient = new Gradient();
		public float _Angle = 90;
		
		[Range(0,5)]public float _Scale = 1;
		[Range(-1,1)]public float _Bias = 0;

		public eEffectRegion _Region = eEffectRegion.AllElements;

		public eColorBlend _BlendMode = eColorBlend.BlendRGBA;
		
		[Range(0,1)]
		public float _Opacity = 1;
		
		public enum ePrecision { Precise, Optimized, Flat }
		public ePrecision _Precision = ePrecision.Precise;

// 		public int ModifyVertices(List<UIVertex> verts, int iStart, int VertexCount, ApplyVertexColor OnApplyColor, Rect WidgetRect)
// 		{
// 			if (_Opacity <= 0)
// 				return 0;
// 
// 			List<float> keys = GetGradientKeys ();
// 
// 			int NewVerts = 0;
// 			int iNextElement = iStart;
// 
// 			for (int i=iStart; i<iStart+VertexCount+NewVerts; i=iNextElement)
// 			{
// 				VertexTools.GetEffectRegion(verts, ref iNextElement, iStart+VertexCount+NewVerts, _Region, WidgetRect);
// 				Vector3 GradientP0, GradientP1;
// 
// 				if (_Region==eEffectRegion.RectTransform)
// 					ComputeGradientRegion( WidgetRect.min, WidgetRect.max, out GradientP0, out GradientP1);
// 				else
// 					ComputeGradientRegion ( verts, i, iNextElement-i, out GradientP0, out GradientP1);
// 
// 				int newVs = ApplyGradient ( keys, verts, i, iNextElement-i, OnApplyColor, GradientP0, GradientP1 );
// 				NewVerts += newVs;
// 				iNextElement += newVs;
// 			}
// 
// 			return NewVerts;
// 		}

		public List<float> GetGradientKeys()
		{
			List<float> keys = new List<float> ();
			
			for (int i=0, imax=_Gradient.alphaKeys.Length; i<imax; i++)
			{
				float f = _Gradient.alphaKeys [i].time;
				f = (f-0.5f)*_Scale + 0.5f;
				f += _Bias;
				
				if (!keys.Contains(f)) keys.Add(f);
			}
			for (int i=0, imax=_Gradient.colorKeys.Length; i<imax; i++)
			{
				float f = _Gradient.colorKeys [i].time;
				f = (f-0.5f)*_Scale + 0.5f;
				f += _Bias;
				if (!keys.Contains(f)) keys.Add(f);
			}
			
			keys.Remove (0);
			keys.Remove (1);
			keys.Sort ();
			return keys;
		}

		public delegate UIVertex ApplyVertexColor( UIVertex vertex, Color32 color, GradientEffect gradient );

// 		public int ApplyGradient( List<float> keys, List<UIVertex> verts, int StartIdx, int NumVerts, ApplyVertexColor OnApplyColor, Vector3 GradientP0, Vector3 GradientP1 )
// 		{
// 			Vector3 GradientBaseLine = GradientP1 - GradientP0;
// 			
// 			int NewVerts = 0;
// 			if (_Precision==ePrecision.Precise)
// 			{
// 				for (int i=0, imax=keys.Count; i<imax; ++i)
// 				{
// 					Vector3 P0 = Vector3.Lerp(GradientP0, GradientP1, keys[i]);
// 					Plane plane = new Plane ( GradientBaseLine.normalized, P0 );
// 					NewVerts += VertexTools.Split (verts, StartIdx, NumVerts+NewVerts, plane);
// 				}
// 			}
// 			ApplyColors (verts, StartIdx, NumVerts+NewVerts, GradientP0, GradientP1, OnApplyColor);
// 			
// 			return NewVerts;
// 		}
// 		
// 		void ComputeGradientRegion ( List<UIVertex> verts, int StartIdx, int NumVerts, out Vector3 GradientP0, out Vector3 GradientP1)
// 		{
// 			Vector3 GradientNormal = Quaternion.Euler (0, 0, -_Angle) * Vector3.down;
// 			Ray ray = new Ray( Vector3.zero, GradientNormal );
// 			
// 			float 	minF = float.MaxValue, 
// 			maxF = float.MinValue;
// 			for (int i=StartIdx, imax=StartIdx+NumVerts; i<imax; ++i)
// 			{
// 				ComputeGradientRegionMinMax( ray, verts[i].position, ref minF, ref maxF);
// 			}
// 			
// 			GradientP1 = GradientNormal * maxF;
// 			GradientP0 = GradientNormal * minF;
// 		}

		void ComputeGradientRegion ( Vector2 rectMin, Vector2 rectMax, Vector3 gradientNormal, out Vector3 GradientP0, out Vector3 GradientP1)
		{
			float 	minF = float.MaxValue, maxF = float.MinValue;

            float f = rectMin.x * gradientNormal.x + rectMin.y * gradientNormal.y;
            if (f < minF) minF = f;
            if (f > maxF) maxF = f;

            f = rectMin.x * gradientNormal.x + rectMax.y * gradientNormal.y;
            if (f < minF) minF = f;
            if (f > maxF) maxF = f;

            f = rectMax.x * gradientNormal.x + rectMin.y * gradientNormal.y;
            if (f < minF) minF = f;
            if (f > maxF) maxF = f;

            f = rectMax.x * gradientNormal.x + rectMax.y * gradientNormal.y;
            if (f < minF) minF = f;
            if (f > maxF) maxF = f;

            GradientP0 = GradientP1 = MathUtils.v3zero;
            GradientP1.x = gradientNormal.x * maxF;
            GradientP1.y = gradientNormal.y * maxF;
            GradientP0.x = gradientNormal.x * minF;
            GradientP0.y = gradientNormal.y * minF;
        }

        void ComputeGradientRegionMinMax ( Ray ray, Vector3 Point, ref float Min, ref float Max )
		{
			float f =  Vector3.Dot(Point-ray.origin, ray.direction);
			if (f<Min) Min = f;
			if (f>Max) Max = f;
		}

		
		void ApplyColors (List<UIVertex> verts, int startIdx, int numVerts, Vector3 GradientP0, Vector3 GradientP1, ApplyVertexColor OnApplyColor)
		{
			Vector3 GradientBase = (GradientP1 - GradientP0).normalized;
			float BaseSize = Vector3.Distance(GradientP1, GradientP0);
			
			Color c = Color.white;
			for (int i=0; i<numVerts; ++i)
			{
				UIVertex uiVertex = verts[i+startIdx];
				
				if (_Precision!=ePrecision.Flat || i%4==0)
				{
					float f = Vector3.Dot(uiVertex.position-GradientP0, GradientBase)/BaseSize;
					
					f -= _Bias;
					f = (f-0.5f)/_Scale + 0.5f;
					
					c = _Gradient.Evaluate(f);
				}

				uiVertex = OnApplyColor(uiVertex, c, this);//ColorMix( uiVertex.color, c ));
				verts[i+startIdx] = uiVertex;
			}
		}


		public static UIVertex ApplyVertexColor_FaceColor( UIVertex vertex, Color32 color, GradientEffect gradient )
		{
			vertex.color = SmartEdge.ColorMix(vertex.color, color, gradient._BlendMode, gradient._Opacity);
			return vertex;
		}

        public static Color32 ApplyVertexColorSEMesh_FaceColor(Color32 vertexColor, Color32 color, GradientEffect gradient)
        {
            return SmartEdge.ColorMix(vertexColor, color, gradient._BlendMode, gradient._Opacity);
        }

        
    }
}