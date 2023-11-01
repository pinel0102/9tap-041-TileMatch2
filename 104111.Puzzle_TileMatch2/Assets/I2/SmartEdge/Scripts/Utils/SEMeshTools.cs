using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace I2.SmartEdge
{
	public static class SEMeshTools
	{
        #region Subdivision

        public static void Subdivide( ArrayBufferSEVertex mesh, int startVertex, int NumSubDivisions)
        {
            int finalSize = startVertex+Mathf.CeilToInt((mesh.Size-startVertex) * Mathf.Pow(2, NumSubDivisions*2));
            mesh.ReserveTotal(finalSize);

            NumSubDivisions = NumSubDivisions * 2;
            for (int s = 0; s < NumSubDivisions; s++)
            {
                int lastVertex = mesh.Size;
                for (int i = startVertex; i < lastVertex; i += 4)
                {
                    int a = i, b = i + 1, c = i + 2, d = i + 3;

                    // Find smallest side 
                    float abx = mesh.Buffer[a].position.x - mesh.Buffer[b].position.x; abx = abx > 0 ? abx : -abx;
                    float aby = mesh.Buffer[a].position.y - mesh.Buffer[b].position.y; aby = aby > 0 ? aby : -aby;
                    float ab  = abx > aby ? abx : aby;

                    float bcx = mesh.Buffer[b].position.x - mesh.Buffer[c].position.x; bcx = bcx > 0 ? bcx : -bcx;
                    float bcy = mesh.Buffer[b].position.y - mesh.Buffer[c].position.y; bcy = bcy > 0 ? bcy : -bcy;
                    float bc  = bcx > bcy ? bcx : bcy;

                    float cdx = mesh.Buffer[c].position.x - mesh.Buffer[d].position.x; cdx = cdx > 0 ? cdx : -cdx;
                    float cdy = mesh.Buffer[c].position.y - mesh.Buffer[d].position.y; cdy = cdy > 0 ? cdy : -cdy;
                    float cd  = cdx > cdy ? cdx : cdy;

                    float dax = mesh.Buffer[d].position.x - mesh.Buffer[a].position.x; dax = dax > 0 ? dax : -dax;
                    float day = mesh.Buffer[d].position.y - mesh.Buffer[a].position.y; day = day > 0 ? day : -day;
                    float da  = dax > day ? dax : day;

                    int i1 = a, i2 = b, i3 = c, i4 = d;
                    if (ab <= bc && ab <= cd && ab <= da) { i1 = b; i2 = c; i3 = d; i4 = a; }
                    else
                    if (bc <= ab && bc <= cd && bc <= da) { i1 = c; i2 = d; i3 = a; i4 = b; }
                    else
                    if (cd <= ab && cd <= bc && cd <= da) { i1 = d; i2 = a; i3 = b; i4 = c; }
                    else { i1 = a; i2 = b; i3 = c; i4 = d; }

                    mesh.ReserveExtra(4);
                    int i12 = mesh.Size+1;        int i34 = mesh.Size;
                    mesh.SplitSegment_Inlined(i34, mesh.Buffer, i3, i4, 0.5f);
                    mesh.SplitSegment_Inlined(i12, mesh.Buffer, i1, i2, 0.5f);
                    mesh.Buffer[mesh.Size + 2] = mesh.Buffer[i2];
                    mesh.Buffer[mesh.Size + 3] = mesh.Buffer[i3];

                    //mesh.Buffer[i1]
                    mesh.Buffer[i2] = mesh.Buffer[i12];
                    mesh.Buffer[i3] = mesh.Buffer[i34];
                    //mesh.Buffer[i4]

                    mesh.Size += 4;
                }
            }
        }

 		#endregion

		#region Split

        public static float Segment2Plane(Vector3 P0, Vector3 P1, Vector3 planeNormal, float planeDist)
        {
            var dir = (P1 - P0);
            var dist01 = Mathf.Sqrt(dir.x*dir.x + dir.y*dir.y);
            dir /= dist01;

            float num = dir.x * planeNormal.x + dir.y * planeNormal.y;
            float num2 = -(P0.x * planeNormal.x + P0.y * planeNormal.y) - planeDist;
            return (num > -0.001f && num < 0.001f) ? 0f : ((num2 / num) / dist01);
        }

        #endregion

        #region Element Finding (consecutive quads sharing vertices)

	    #endregion
	}
}