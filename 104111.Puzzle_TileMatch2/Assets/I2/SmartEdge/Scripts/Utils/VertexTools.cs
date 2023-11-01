using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace I2.SmartEdge
{
	public enum eEffectRegion { RectTransform, Element, AllElements };

	public static class VertexTools
	{
		#region Blending

		public static UIVertex BlendVertices( UIVertex v0, UIVertex v1, float t )
		{
			UIVertex v = new UIVertex ();

			v.color 	= Color32.Lerp (v0.color, v1.color, t);
			v.uv0 		= Vector2.Lerp (v0.uv0, v1.uv0, t);
			v.uv1 		= Vector2.Lerp (v0.uv1, v1.uv1, t);
			v.tangent 	= Vector4.Lerp (v0.tangent, v1.tangent, t);
			v.normal 	= Vector3.Lerp (v0.normal, v1.normal, t);
			v.position 	= Vector3.Lerp (v0.position, v1.position, t);
			
			return v;
		}

		#endregion

		#region Subdivision

		// Subdivides the quad starting in iStart into 4 sub quads
		public static int Subdivide( List<UIVertex> verts, int iStart, int QuadCount, int Subdivisions )
		{
			if (Subdivisions <= 0)
				return 0;

			int nNewVerts = 0;
			for (int i = 0; i<QuadCount; i++)
				nNewVerts += SubdivideQuad (verts, iStart+i*4+nNewVerts, Subdivisions-1);
			return nNewVerts;
		}
		
		// Subdivides the quad starting in iStart into 4 sub quads
		public static int SubdivideQuad( List<UIVertex> verts, int i, int RemainingSubdivisions )
		{
			UIVertex v0 = VertexTools.BlendVertices(verts[i+0], verts[i+1], 0.5f);
			UIVertex v1 = VertexTools.BlendVertices(verts[i+1], verts[i+2], 0.5f);
			UIVertex v2 = VertexTools.BlendVertices(verts[i+2], verts[i+3], 0.5f);
			UIVertex v3 = VertexTools.BlendVertices(verts[i+3], verts[i+0], 0.5f);
			UIVertex vc = VertexTools.BlendVertices(verts[i+0], verts[i+2], 0.5f);
			
			//   [0] v0 [1]
			//   v3  vc v1
			//   [3] v2 [2]
			
			verts.Insert(i+4, verts[i+0] );	// Quad:
			verts.Insert(i+5, v0 );			//    [0] v0
			verts.Insert(i+6, vc );			//    v3  vc
			verts.Insert(i+7, v3 );
			
			verts.Insert(i+8, v0 );			// Quad:
			verts.Insert(i+9, verts[i+1] );	//    v0  [1]
			verts.Insert(i+10, v1 );		//    vc  v1
			verts.Insert(i+11, vc );
			
			verts.Insert(i+12,  vc );		// Quad:
			verts.Insert(i+13,  v1 );		//    vc  v1
			verts.Insert(i+14, verts[i+2] );//    v2  [2]
			verts.Insert(i+15, v2 );
			
			verts[i+0] = v3;				// Quad:
			verts[i+1] = vc;				//    v3  vc
			verts[i+2] = v2;				//   [3]  v2
			//verts[i+3] = verts[i+3];

			int nNewVerts = 4 * 3;

			if (RemainingSubdivisions > 0)
			{
				nNewVerts += SubdivideQuad (verts, i+12, RemainingSubdivisions - 1);
				nNewVerts += SubdivideQuad (verts, i+8,  RemainingSubdivisions - 1);
				nNewVerts += SubdivideQuad (verts, i+4,  RemainingSubdivisions - 1);
				nNewVerts += SubdivideQuad (verts, i, 	 RemainingSubdivisions - 1);
			}
			return nNewVerts;
		}
		#endregion

		#region Split

		public static int Split( List<UIVertex> verts, int StartIdx, int NumVerts, Plane plane )
		{
			int nNewVerts = 0;
			for (int i=0; i<NumVerts; i+=4)
			{
				nNewVerts += SplitQuad (verts, StartIdx+i+nNewVerts, plane);
			}
			return nNewVerts;
		}
		
		public static int SplitQuad (List<UIVertex> verts, int StartIdx, Plane plane)
		{
			List<int> Vertices = new List<int> ();
			
			Vector3 LastP = verts [StartIdx+3].position;
			bool HasSlices = false;
			for (int i=StartIdx; i<StartIdx+4; ++i)
			{
				Vector3 newP = verts[i].position;
				if (!plane.SameSide(LastP, newP))
				{
					Vertices.Add (-1);
					HasSlices = true;
				}
				Vertices.Add (i);
				LastP = newP;
			}
			
			if (!HasSlices)
			{
				return 0;
			}
			
			while (!(Vertices[1]==-1 && (Vertices[3]==-1 || Vertices[4]==-1)))	// while not Case 1 or Case 2 shift the list
			{
				int v = Vertices[0];
				Vertices.RemoveAt(0);
				Vertices.Add(v);
			}
			
			if (Vertices[3]==-1)  // Case 1:  1A2B34 ->  1ACB, A2BC, 1B34
			{
				UIVertex v1 = verts[Vertices[0]];
				UIVertex v2 = verts[Vertices[2]];
				UIVertex v3 = verts[Vertices[4]];
				UIVertex v4 = verts[Vertices[5]];
				UIVertex vA = VertexTools.BlendVertices( v1, v2, Segment2Plane( v1.position, v2.position, plane));
				UIVertex vB = VertexTools.BlendVertices( v2, v3, Segment2Plane( v2.position, v3.position, plane));
				UIVertex vC = VertexTools.BlendVertices( vA, vB, 0.5f);
				
				verts[StartIdx+0] = v1;
				verts[StartIdx+1] = vA;
				verts[StartIdx+2] = vC;
				verts[StartIdx+3] = vB;
				
				verts.Insert(StartIdx+4, vA);
				verts.Insert(StartIdx+5, v2);
				verts.Insert(StartIdx+6, vB);
				verts.Insert(StartIdx+7, vC);
				
				verts.Insert(StartIdx+8,  v1);
				verts.Insert(StartIdx+9,  vB);
				verts.Insert(StartIdx+10, v3);
				verts.Insert(StartIdx+11, v4);
				
				return 2*4; // Num new vertices
			}
			else // Case 2: 1A23B4 -> 1AB4, A23B
			{
				UIVertex v1 = verts[Vertices[0]];
				UIVertex v2 = verts[Vertices[2]];
				UIVertex v3 = verts[Vertices[3]];
				UIVertex v4 = verts[Vertices[5]];
				UIVertex vA = VertexTools.BlendVertices( v1, v2, Segment2Plane( v1.position, v2.position, plane));
				UIVertex vB = VertexTools.BlendVertices( v3, v4, Segment2Plane( v3.position, v4.position, plane));
				
				verts[StartIdx+0] = v1;
				verts[StartIdx+1] = vA;
				verts[StartIdx+2] = vB;
				verts[StartIdx+3] = v4;
				
				verts.Insert(StartIdx+4, vA);
				verts.Insert(StartIdx+5, v2);
				verts.Insert(StartIdx+6, v3);
				verts.Insert(StartIdx+7, vB);
				
				return 4; // Num new vertices
			}
		}

		public static float Segment2Plane( Vector3 P0, Vector3 P1, Plane plane)
		{
			Ray ray = new Ray (P0, P1 - P0);
			float d;
			plane.Raycast(ray, out d);
			
			return d / (P1-P0).magnitude;
			//return ray.GetPoint(d);
		}

		#endregion

		#region Element Finding (consecutive quads sharing vertices)

		public static Rect GetEffectRegion(List<UIVertex> verts, ref int iStart, int MaxIndex, eEffectRegion Region, Rect WidgetRect )
		{
			switch (Region)
			{
				case eEffectRegion.Element 		 : return GetElementRect(verts, ref iStart, MaxIndex, false);
				case eEffectRegion.AllElements 	 : return GetElementRect(verts, ref iStart, MaxIndex, true);
				case eEffectRegion.RectTransform : iStart = MaxIndex;
													return WidgetRect;
			}
			return Rect.MinMaxRect (0, 0, 0, 0);
		}
		
		private static Rect GetElementRect(List<UIVertex> verts, ref int iStart, int MaxIndex, bool AllElements=false )
		{
			Rect rect = new Rect();
			rect.min = rect.max = verts[iStart].position;
			int iBegin = iStart;
			
			for (int i=iStart; i<MaxIndex; i+=4)
			{
				if (!AllElements && !QuadBelongsToElement(verts, iBegin, i))
					break;
				else
				{
					for (int j=i; j<i+4; ++j)
					{
						rect.min = Vector2.Min(rect.min, (Vector2)verts[j].position);
						rect.max = Vector2.Max(rect.max, (Vector2)verts[j].position);
					}
				}
				iStart+=4;
			}
			return rect;
		}
		
		static bool QuadBelongsToElement( List<UIVertex> verts, int iElementStart, int iQuad )
		{
			if (iElementStart==iQuad)
				return true;
			
			for (int i=0; i<4; ++i)
				if (IsVertexInList(verts, iElementStart, iQuad, verts[iQuad+i].position))
					return true;
			return false;
		}
		
		static bool IsVertexInList(  List<UIVertex> verts, int iStart, int iLast, Vector3 v )
		{
			for (int i=iStart; i<iLast; ++i)
				if ((verts[i].position-v).sqrMagnitude < 0.001f)
					return true;
			return false;
		}
		
		#endregion
	}

    public struct SE_Character
    {
        public Vector2 Min, Max;
        public char Character;
        public float TopY, BaselineY, Height;
        public int iCharacterInText, iCharacterInLine, 
                   iLine, 
                   iWord, iWordInLine,
                   iParagraph;
        public SE_RichTextData RichText;
    }

    public struct SE_RichTextData
    {
        public RichTextTag_Align.eAlignmentOverride Alignment;
        public float Bold;
    }

    /*public struct SE_Line
    {
        public float lineHeight;
        public float topY;
    }*/
}