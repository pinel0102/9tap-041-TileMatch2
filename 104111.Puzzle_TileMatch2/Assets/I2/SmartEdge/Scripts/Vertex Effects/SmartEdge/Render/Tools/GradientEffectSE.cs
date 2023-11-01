using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace I2.SmartEdge
{
    using Gradient = UnityEngine.Gradient;

    public partial class GradientEffect
	{
// 		public int ModifyVertices(List<SEVertex> verts, int iStart, int VertexCount, SEApplyVertexColor OnApplyColor, Rect WidgetRect)
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
// 				SEVertexTools.GetEffectRegion(verts, ref iNextElement, iStart+VertexCount+NewVerts, _Region, WidgetRect);
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
// 
// 		public delegate SEVertex SEApplyVertexColor( SEVertex vertex, Color32 color, GradientEffect gradient );
// 
// 		public int ApplyGradient( List<float> keys, List<SEVertex> verts, int StartIdx, int NumVerts, SEApplyVertexColor OnApplyColor, Vector3 GradientP0, Vector3 GradientP1 )
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
// 					NewVerts += SEVertexTools.Split (verts, StartIdx, NumVerts+NewVerts, plane);
// 				}
// 			}
// 			ApplyColors (verts, StartIdx, NumVerts+NewVerts, GradientP0, GradientP1, OnApplyColor);
// 			
// 			return NewVerts;
// 		}
// 		
// 		void ComputeGradientRegion ( List<SEVertex> verts, int StartIdx, int NumVerts, out Vector3 GradientP0, out Vector3 GradientP1)
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
// 
// 		void ApplyColors (List<SEVertex> verts, int startIdx, int numVerts, Vector3 GradientP0, Vector3 GradientP1, SEApplyVertexColor OnApplyColor)
// 		{
// 			Vector3 GradientBase = (GradientP1 - GradientP0).normalized;
// 			float BaseSize = Vector3.Distance(GradientP1, GradientP0);
// 			
// 			Color c = Color.white;
// 			for (int i=0; i<numVerts; ++i)
// 			{
// 				SEVertex seVertex = verts[i+startIdx];
// 				
// 				if (_Precision!=ePrecision.Flat || i%4==0)
// 				{
// 					float f = Vector3.Dot(seVertex.position-GradientP0, GradientBase)/BaseSize;
// 					
// 					f -= _Bias;
// 					f = (f-0.5f)/_Scale + 0.5f;
// 					
// 					c = _Gradient.Evaluate(f);
// 				}
// 
// 				seVertex = OnApplyColor(seVertex, c, this);//ColorMix( seVertex.color, c ));
// 				verts[i+startIdx] = seVertex;
// 			}
// 		}

		public static SEVertex ApplyVertexColor_FaceColor( SEVertex vertex, Color32 color, GradientEffect gradient )
		{
			vertex.color = SmartEdge.ColorMix(vertex.color, color, gradient._BlendMode, gradient._Opacity);
			return vertex;
		}
    }
}