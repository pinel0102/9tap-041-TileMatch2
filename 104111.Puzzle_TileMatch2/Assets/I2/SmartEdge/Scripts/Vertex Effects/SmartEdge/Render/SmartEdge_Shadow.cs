using UnityEngine;
using System.Collections.Generic;

namespace I2.SmartEdge
{
    public partial class SmartEdge
    {
        protected virtual void AddShadowVertices()
        {
            if (!rparams.HasShadow() || rparams._SimpleShadows)
                return;

            Color32 shadowC = rparams._ShadowColor;
            shadowC.a = (byte)(shadowC.a * mWidgetColor.a);

            mSEMesh.mLayers[(int)SEVerticesLayers.Shadow].CopyFrom(mOriginalVertices);


            Shadow_ApplyVertexParams(shadowC);

            int nSubdivisions = rparams._ShadowSubdivisionLevel;
            if (nSubdivisions>0)
                SubdivideShadow(nSubdivisions);
        }


        void Shadow_ApplyVertexParams(Color32 shadowC)
        {
            Vector2 vt = MathUtils.v2zero;
            var shadowAlpha = shadowC.a;

            var verts = mSEMesh.mLayers[(int)SEVerticesLayers.Shadow].Buffer;
            for (int c = 0; c < mCharacters.Size; c++)
            {
                float edge = rparams.GetEdge_Outline( mCharacters.Buffer[c].RichText.Bold);
                if (rparams._ShadowEdgeOffset > 0)
                    edge -= edge * rparams._ShadowEdgeOffset;
                else
                    edge -= (1 - edge) * rparams._ShadowEdgeOffset;


                float BottomY = mCharacters.Buffer[c].Min.y;
                var minX = mCharacters.Buffer[c].Min.x;
                var maxX = mCharacters.Buffer[c].Max.x;

                for (int i = c * 4; i < c * 4 + 4; ++i)
                {
                    verts[i].mode = SEVertex.MODE_Glow;

                    v2.x = verts[i].position.x;
                    v2.y = verts[i].position.y;
                    float cwidth = (maxX - minX);                if (cwidth < 0.0001f) cwidth = 0.0001f;
                    vt.x = (v2.x - minX) / cwidth;               vt.x = (vt.x < 0 ? 0 : (vt.x > 1 ? 1 : vt.x));
                    vt.y = (v2.y - BottomY) / mCharacterSize;    vt.y = (vt.y < 0 ? 0 : (vt.y > 1 ? 1 : vt.y));


                    float SmoothGradient = rparams._ShadowSmoothBottom + (rparams._ShadowSmoothTop - rparams._ShadowSmoothBottom) * vt.y;

                    float ShadowOuterEdge = 0, ShadowInnerEdge = 0, OuterSmooth = 0, InnerSmooth = 0;

                    var smooth = (rparams._ShadowSmoothWidth * SmoothGradient); smooth = (smooth > 1 ? 1 : smooth < 0 ? 0 : smooth);
                    if (rparams._ShadowHollow)
                    {
                        ShadowOuterEdge = edge;
                        ShadowInnerEdge = edge + (1 - edge) * rparams._ShadowEdgeWidth;

                        OuterSmooth = ShadowOuterEdge * smooth;

                        var innerSmooth = (rparams._ShadowInnerSmoothWidth * SmoothGradient); innerSmooth = (innerSmooth > 1 ? 1 : innerSmooth < 0 ? 0 : innerSmooth);
                        InnerSmooth = (1 - ShadowInnerEdge) * innerSmooth;
                    }
                    else
                    {
                        OuterSmooth = edge * smooth;
                        ShadowOuterEdge = edge + OuterSmooth;
                        ShadowInnerEdge = InnerSmooth = 1;
                        OuterSmooth *= 2;
                    }

                    // Make range 1..255 to avoid weird issues at the border
                    //float maxVal = 0.5f / (mSpread);
                    //ShadowOuterEdge = maxVal + (1 - maxVal) * ShadowOuterEdge;
                    //ShadowInnerEdge = maxVal + (1 - maxVal) * ShadowInnerEdge;
                    //OuterSmooth = maxVal + (1 - maxVal) * OuterSmooth;
                    //InnerSmooth = maxVal + (1 - maxVal) * InnerSmooth;



                    shadowC.a = (byte)(shadowAlpha * (rparams._ShadowOpacityBottom + (rparams._ShadowOpacityTop - rparams._ShadowOpacityBottom) * vt.y));

                    byte faceA = mOriginalVertices.Buffer[i].color.a;
                    if (faceA < 255)
                        shadowC.a = (byte)(shadowC.a * faceA / 255f);

                    float xWidth = mAllCharactersMax.x - mAllCharactersMin.x;
                    //float xWidth = mWidgetRectMax.x - mWidgetRectMin.x;

                    float xMin = xWidth * rparams._ShadowBorderLeft;
                    float xMax = xWidth * rparams._ShadowBorderRight;
                    verts[i].position.x += (xMin + (xMax - xMin) * vt.x) * vt.y;
                    verts[i].position.y += mCharacterSize * rparams._ShadowHeight * vt.y;

                    verts[i].position.x += mCharacterSize * rparams._ShadowOffset.x;
                    verts[i].position.y += mCharacterSize * rparams._ShadowOffset.y;
                    verts[i].position.z += mCharacterSize * rparams._ShadowOffset.z;

                    verts[i].color = shadowC;

                    verts[i].byte0 /*surface*/        = (byte)(ShadowInnerEdge > 1 ? 0xff : ShadowInnerEdge < 0 ? 0 : (0xff * ShadowInnerEdge));
                    verts[i].byte1 /*edge*/           = (byte)(ShadowOuterEdge > 1 ? 0xff : ShadowOuterEdge < 0 ? 0 : (0xff * ShadowOuterEdge));

                    verts[i].byte2 /*glowInnerWidth*/ = (byte)(InnerSmooth > 1 ? 0xff : InnerSmooth < 0 ? 0 : (0xff * InnerSmooth));
                    verts[i].byte3 /*glowOuterWidth*/ = (byte)(OuterSmooth > 1 ? 0xff : OuterSmooth < 0 ? 0 : (0xff * OuterSmooth));

                    verts[i].byte4 /*glowIntensity*/  = (byte)(255 / 10f);
                    verts[i].byte5 /*glowPower*/      = (byte)(rparams._ShadowSmoothPower > 1 ? 0xff : rparams._ShadowSmoothPower < 0 ? 0 : (0xff * rparams._ShadowSmoothPower));
                }
            }
        }



        static Vector2[][] ShadowSubdivision_Vertices = {
                                                            new Vector2[]{},
                                                            new Vector2[]{ new Vector2(0f, 0f ), new Vector2(1f, 0f ), new Vector2(0.5f, 0.5f ), new Vector2(0f, 1f ), new Vector2(0.5f, 0.5f), new Vector2(1f, 0f ), new Vector2(1f, 1f ), new Vector2(0f, 1f ) },
                                                            new Vector2[]{ new Vector2(0f, 0f ), new Vector2(0.5f, 0f ), new Vector2(0.5f, 0.5f ), new Vector2(0f, 0.5f ), new Vector2(1f, 0f ), new Vector2(1f, 0.5f ), new Vector2(0.5f, 0.5f ), new Vector2(0.5f, 0f ), new Vector2(0.5f, 0.5f ), new Vector2(0.5f, 1f ), new Vector2(0f, 1f ), new Vector2(0f, 0.5f ), new Vector2(0.5f, 0.5f ), new Vector2(1f, 0.5f ), new Vector2(1f, 1f ), new Vector2(0.5f, 1f ) },
                                                            new Vector2[]{ new Vector2(0f, 0f ), new Vector2(0.25f, 0f ), new Vector2(0.25f, 0.25f ), new Vector2(0f, 0.25f ), new Vector2(0.5f, 0f ), new Vector2(0.5f, 0.25f ), new Vector2(0.25f, 0.25f ), new Vector2(0.25f, 0f ), new Vector2(0.25f, 0.25f ), new Vector2(0.25f, 0.5f ), new Vector2(0f, 0.5f ), new Vector2(0f, 0.25f ), new Vector2(0.25f, 0.25f ), new Vector2(0.5f, 0.25f ), new Vector2(0.5f, 0.5f ), new Vector2(0.25f, 0.5f ), new Vector2(1f, 0f ), new Vector2(1f, 0.25f ), new Vector2(0.75f, 0.25f ), new Vector2(0.75f, 0f ), new Vector2(1f, 0.5f ), new Vector2(0.75f, 0.5f ), new Vector2(0.75f, 0.25f ), new Vector2(1f, 0.25f ), new Vector2(0.75f, 0.25f ), new Vector2(0.5f, 0.25f ), new Vector2(0.5f, 0f ), new Vector2(0.75f, 0f ), new Vector2(0.75f, 0.25f ), new Vector2(0.75f, 0.5f ), new Vector2(0.5f, 0.5f ), new Vector2(0.5f, 0.25f ), new Vector2(0.5f, 0.5f ), new Vector2(0.5f, 0.75f ), new Vector2(0.25f, 0.75f ), new Vector2(0.25f, 0.5f ), new Vector2(0.5f, 1f ), new Vector2(0.25f, 1f ), new Vector2(0.25f, 0.75f ), new Vector2(0.5f, 0.75f ), new Vector2(0.25f, 0.75f ), new Vector2(0f, 0.75f ), new Vector2(0f, 0.5f ), new Vector2(0.25f, 0.5f ), new Vector2(0.25f, 0.75f ), new Vector2(0.25f, 1f ), new Vector2(0f, 1f ), new Vector2(0f, 0.75f ), new Vector2(0.5f, 0.5f ), new Vector2(0.75f, 0.5f ), new Vector2(0.75f, 0.75f ), new Vector2(0.5f, 0.75f ), new Vector2(1f, 0.5f ), new Vector2(1f, 0.75f ), new Vector2(0.75f, 0.75f ), new Vector2(0.75f, 0.5f ), new Vector2(0.75f, 0.75f ), new Vector2(0.75f, 1f ), new Vector2(0.5f, 1f ), new Vector2(0.5f, 0.75f ), new Vector2(0.75f, 0.75f ), new Vector2(1f, 0.75f ), new Vector2(1f, 1f ), new Vector2(0.75f, 1f ) },
                                                            new Vector2[]{ new Vector2(0f, 0f ), new Vector2(0.125f, 0f ), new Vector2(0.125f, 0.125f ), new Vector2(0f, 0.125f ), new Vector2(0.25f, 0f ), new Vector2(0.25f, 0.125f ), new Vector2(0.125f, 0.125f ), new Vector2(0.125f, 0f ), new Vector2(0.125f, 0.125f ), new Vector2(0.125f, 0.25f ), new Vector2(0f, 0.25f ), new Vector2(0f, 0.125f ), new Vector2(0.125f, 0.125f ), new Vector2(0.25f, 0.125f ), new Vector2(0.25f, 0.25f ), new Vector2(0.125f, 0.25f ), new Vector2(0.5f, 0f ), new Vector2(0.5f, 0.125f ), new Vector2(0.375f, 0.125f ), new Vector2(0.375f, 0f ), new Vector2(0.5f, 0.25f ), new Vector2(0.375f, 0.25f ), new Vector2(0.375f, 0.125f ), new Vector2(0.5f, 0.125f ), new Vector2(0.375f, 0.125f ), new Vector2(0.25f, 0.125f ), new Vector2(0.25f, 0f ), new Vector2(0.375f, 0f ), new Vector2(0.375f, 0.125f ), new Vector2(0.375f, 0.25f ), new Vector2(0.25f, 0.25f ), new Vector2(0.25f, 0.125f ), new Vector2(0.25f, 0.25f ), new Vector2(0.25f, 0.375f ), new Vector2(0.125f, 0.375f ), new Vector2(0.125f, 0.25f ), new Vector2(0.25f, 0.5f ), new Vector2(0.125f, 0.5f ), new Vector2(0.125f, 0.375f ), new Vector2(0.25f, 0.375f ), new Vector2(0.125f, 0.375f ), new Vector2(0f, 0.375f ), new Vector2(0f, 0.25f ), new Vector2(0.125f, 0.25f ), new Vector2(0.125f, 0.375f ), new Vector2(0.125f, 0.5f ), new Vector2(0f, 0.5f ), new Vector2(0f, 0.375f ), new Vector2(0.25f, 0.25f ), new Vector2(0.375f, 0.25f ), new Vector2(0.375f, 0.375f ), new Vector2(0.25f, 0.375f ), new Vector2(0.5f, 0.25f ), new Vector2(0.5f, 0.375f ), new Vector2(0.375f, 0.375f ), new Vector2(0.375f, 0.25f ), new Vector2(0.375f, 0.375f ), new Vector2(0.375f, 0.5f ), new Vector2(0.25f, 0.5f ), new Vector2(0.25f, 0.375f ), new Vector2(0.375f, 0.375f ), new Vector2(0.5f, 0.375f ), new Vector2(0.5f, 0.5f ), new Vector2(0.375f, 0.5f ), new Vector2(1f, 0f ), new Vector2(1f, 0.125f ), new Vector2(0.875f, 0.125f ), new Vector2(0.875f, 0f ), new Vector2(1f, 0.25f ), new Vector2(0.875f, 0.25f ), new Vector2(0.875f, 0.125f ), new Vector2(1f, 0.125f ), new Vector2(0.875f, 0.125f ), new Vector2(0.75f, 0.125f ), new Vector2(0.75f, 0f ), new Vector2(0.875f, 0f ), new Vector2(0.875f, 0.125f ), new Vector2(0.875f, 0.25f ), new Vector2(0.75f, 0.25f ), new Vector2(0.75f, 0.125f ), new Vector2(1f, 0.5f ), new Vector2(0.875f, 0.5f ), new Vector2(0.875f, 0.375f ), new Vector2(1f, 0.375f ), new Vector2(0.75f, 0.5f ), new Vector2(0.75f, 0.375f ), new Vector2(0.875f, 0.375f ), new Vector2(0.875f, 0.5f ), new Vector2(0.875f, 0.375f ), new Vector2(0.875f, 0.25f ), new Vector2(1f, 0.25f ), new Vector2(1f, 0.375f ), new Vector2(0.875f, 0.375f ), new Vector2(0.75f, 0.375f ), new Vector2(0.75f, 0.25f ), new Vector2(0.875f, 0.25f ), new Vector2(0.75f, 0.25f ), new Vector2(0.625f, 0.25f ), new Vector2(0.625f, 0.125f ), new Vector2(0.75f, 0.125f ), new Vector2(0.5f, 0.25f ), new Vector2(0.5f, 0.125f ), new Vector2(0.625f, 0.125f ), new Vector2(0.625f, 0.25f ), new Vector2(0.625f, 0.125f ), new Vector2(0.625f, 0f ), new Vector2(0.75f, 0f ), new Vector2(0.75f, 0.125f ), new Vector2(0.625f, 0.125f ), new Vector2(0.5f, 0.125f ), new Vector2(0.5f, 0f ), new Vector2(0.625f, 0f ), new Vector2(0.75f, 0.25f ), new Vector2(0.75f, 0.375f ), new Vector2(0.625f, 0.375f ), new Vector2(0.625f, 0.25f ), new Vector2(0.75f, 0.5f ), new Vector2(0.625f, 0.5f ), new Vector2(0.625f, 0.375f ), new Vector2(0.75f, 0.375f ), new Vector2(0.625f, 0.375f ), new Vector2(0.5f, 0.375f ), new Vector2(0.5f, 0.25f ), new Vector2(0.625f, 0.25f ), new Vector2(0.625f, 0.375f ), new Vector2(0.625f, 0.5f ), new Vector2(0.5f, 0.5f ), new Vector2(0.5f, 0.375f ), new Vector2(0.5f, 0.5f ), new Vector2(0.5f, 0.625f ), new Vector2(0.375f, 0.625f ), new Vector2(0.375f, 0.5f ), new Vector2(0.5f, 0.75f ), new Vector2(0.375f, 0.75f ), new Vector2(0.375f, 0.625f ), new Vector2(0.5f, 0.625f ), new Vector2(0.375f, 0.625f ), new Vector2(0.25f, 0.625f ), new Vector2(0.25f, 0.5f ), new Vector2(0.375f, 0.5f ), new Vector2(0.375f, 0.625f ), new Vector2(0.375f, 0.75f ), new Vector2(0.25f, 0.75f ), new Vector2(0.25f, 0.625f ), new Vector2(0.5f, 1f ), new Vector2(0.375f, 1f ), new Vector2(0.375f, 0.875f ), new Vector2(0.5f, 0.875f ), new Vector2(0.25f, 1f ), new Vector2(0.25f, 0.875f ), new Vector2(0.375f, 0.875f ), new Vector2(0.375f, 1f ), new Vector2(0.375f, 0.875f ), new Vector2(0.375f, 0.75f ), new Vector2(0.5f, 0.75f ), new Vector2(0.5f, 0.875f ), new Vector2(0.375f, 0.875f ), new Vector2(0.25f, 0.875f ), new Vector2(0.25f, 0.75f ), new Vector2(0.375f, 0.75f ), new Vector2(0.25f, 0.75f ), new Vector2(0.125f, 0.75f ), new Vector2(0.125f, 0.625f ), new Vector2(0.25f, 0.625f ), new Vector2(0f, 0.75f ), new Vector2(0f, 0.625f ), new Vector2(0.125f, 0.625f ), new Vector2(0.125f, 0.75f ), new Vector2(0.125f, 0.625f ), new Vector2(0.125f, 0.5f ), new Vector2(0.25f, 0.5f ), new Vector2(0.25f, 0.625f ), new Vector2(0.125f, 0.625f ), new Vector2(0f, 0.625f ), new Vector2(0f, 0.5f ), new Vector2(0.125f, 0.5f ), new Vector2(0.25f, 0.75f ), new Vector2(0.25f, 0.875f ), new Vector2(0.125f, 0.875f ), new Vector2(0.125f, 0.75f ), new Vector2(0.25f, 1f ), new Vector2(0.125f, 1f ), new Vector2(0.125f, 0.875f ), new Vector2(0.25f, 0.875f ), new Vector2(0.125f, 0.875f ), new Vector2(0f, 0.875f ), new Vector2(0f, 0.75f ), new Vector2(0.125f, 0.75f ), new Vector2(0.125f, 0.875f ), new Vector2(0.125f, 1f ), new Vector2(0f, 1f ), new Vector2(0f, 0.875f ), new Vector2(0.5f, 0.5f ), new Vector2(0.625f, 0.5f ), new Vector2(0.625f, 0.625f ), new Vector2(0.5f, 0.625f ), new Vector2(0.75f, 0.5f ), new Vector2(0.75f, 0.625f ), new Vector2(0.625f, 0.625f ), new Vector2(0.625f, 0.5f ), new Vector2(0.625f, 0.625f ), new Vector2(0.625f, 0.75f ), new Vector2(0.5f, 0.75f ), new Vector2(0.5f, 0.625f ), new Vector2(0.625f, 0.625f ), new Vector2(0.75f, 0.625f ), new Vector2(0.75f, 0.75f ), new Vector2(0.625f, 0.75f ), new Vector2(1f, 0.5f ), new Vector2(1f, 0.625f ), new Vector2(0.875f, 0.625f ), new Vector2(0.875f, 0.5f ), new Vector2(1f, 0.75f ), new Vector2(0.875f, 0.75f ), new Vector2(0.875f, 0.625f ), new Vector2(1f, 0.625f ), new Vector2(0.875f, 0.625f ), new Vector2(0.75f, 0.625f ), new Vector2(0.75f, 0.5f ), new Vector2(0.875f, 0.5f ), new Vector2(0.875f, 0.625f ), new Vector2(0.875f, 0.75f ), new Vector2(0.75f, 0.75f ), new Vector2(0.75f, 0.625f ), new Vector2(0.75f, 0.75f ), new Vector2(0.75f, 0.875f ), new Vector2(0.625f, 0.875f ), new Vector2(0.625f, 0.75f ), new Vector2(0.75f, 1f ), new Vector2(0.625f, 1f ), new Vector2(0.625f, 0.875f ), new Vector2(0.75f, 0.875f ), new Vector2(0.625f, 0.875f ), new Vector2(0.5f, 0.875f ), new Vector2(0.5f, 0.75f ), new Vector2(0.625f, 0.75f ), new Vector2(0.625f, 0.875f ), new Vector2(0.625f, 1f ), new Vector2(0.5f, 1f ), new Vector2(0.5f, 0.875f ), new Vector2(0.75f, 0.75f ), new Vector2(0.875f, 0.75f ), new Vector2(0.875f, 0.875f ), new Vector2(0.75f, 0.875f ), new Vector2(1f, 0.75f ), new Vector2(1f, 0.875f ), new Vector2(0.875f, 0.875f ), new Vector2(0.875f, 0.75f ), new Vector2(0.875f, 0.875f ), new Vector2(0.875f, 1f ), new Vector2(0.75f, 1f ), new Vector2(0.75f, 0.875f ), new Vector2(0.875f, 0.875f ), new Vector2(1f, 0.875f ), new Vector2(1f, 1f ), new Vector2(0.875f, 1f ) }
                                                        };

        //static List<Vector2> SubDVertexFactors = new List<Vector2>();
        //void GenerateList(Vector2 a, Vector2 b, Vector2 c, Vector2 d, int Subdivisions)
        //{
        //    if (Subdivisions <= 0)
        //    {
        //        SubDVertexFactors.Add(a);
        //        SubDVertexFactors.Add(b);
        //        SubDVertexFactors.Add(c);
        //        SubDVertexFactors.Add(d);

        //        return;
        //    }

        //    //   [a] v0 [b]
        //    //   v3  vc v1
        //    //   [d] v2 [c]

        //    var v0 = Vector2.Lerp(a, b, 0.5f);
        //    var v1 = Vector2.Lerp(b, c, 0.5f);
        //    var v2 = Vector2.Lerp(c, d, 0.5f);
        //    var v3 = Vector2.Lerp(d, a, 0.5f);

        //    var vc = Vector2.Lerp(Vector2.Lerp(v3, v1, 0.5f), Vector2.Lerp(v0, v2, 0.5f), 0.5f);

        //    GenerateList(a, v0, vc, v3, Subdivisions - 1);
        //    GenerateList(b, v1, vc, v0, Subdivisions - 1);
        //    GenerateList(vc, v2, d, v3, Subdivisions - 1);
        //    GenerateList(vc, v1, c, v2, Subdivisions - 1);
        //}


        private static SEVertex[] mSubdivisionBuffer = new SEVertex[4];
        void SubdivideShadow(int nSubdivisions)
        {
            //string sx = "";
            //for (nSubdivisions = 0; nSubdivisions <= 3; nSubdivisions++)
            //{
            //    SubDVertexFactors.Clear();
            //    GenerateList(new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), nSubdivisions);

            //    sx += "{ ";
            //    for (int i = 0; i < SubDVertexFactors.Count; ++i)
            //        sx += ", new Vector2(" + SubDVertexFactors[i].x + "f, " + SubDVertexFactors[i].y + "f )";
            //    sx += " },\n";

            //}
            //Debug.Log(sx);
            //return;


            var SubDVertexFactors = ShadowSubdivision_Vertices[nSubdivisions];
            int SubDVertexFactors_Length = SubDVertexFactors.Length;

            var shadowVertices = mSEMesh.mLayers[(int)SEVerticesLayers.Shadow];
            int numVertices = shadowVertices.Size;

            int newNumVertices = SubDVertexFactors_Length * mCharacters.Size;

            shadowVertices.ReserveTotal(newNumVertices);
            shadowVertices.Size = newNumVertices;

            var verts = shadowVertices.Buffer;
            var result = shadowVertices.Buffer;
            System.Array.Copy(verts, 0, mSubdivisionBuffer, 0, 4);

            for (int v = numVertices - 4; v >= 0; v -= 4, newNumVertices -= SubDVertexFactors_Length)
            {
                int vert1 = v; int vert2 = v + 1; int vert3 = v + 2; int vert4 = v + 3;

                for (int i = SubDVertexFactors_Length - 1; i >= 0; i--)
                {
                    int newV = newNumVertices - SubDVertexFactors_Length + i;
                    if (v==0 && newV < 0 + 4)
                    {
                        verts = mSubdivisionBuffer;
                        vert1 = 0; vert2 = 1; vert3 = 2; vert4 = 3;
                    }
                    //mesh.SplitQuad_Inlined(mesh.NumVertices + i, vert1, vert2, vert3, vert4, SubDVertexFactors[i].x, SubDVertexFactors[i].y);
                    float x = SubDVertexFactors[i].x;
                    float y = SubDVertexFactors[i].y;

                    result[newV].position.x = (verts[vert1].position.x * x * y - verts[vert2].position.x * x * y + verts[vert3].position.x * x * y - verts[vert4].position.x * x * y - verts[vert1].position.x * x - verts[vert1].position.x * y + verts[vert2].position.x * x + verts[vert4].position.x * y + verts[vert1].position.x);
                    result[newV].position.y = (verts[vert1].position.y * x * y - verts[vert2].position.y * x * y + verts[vert3].position.y * x * y - verts[vert4].position.y * x * y - verts[vert1].position.y * x - verts[vert1].position.y * y + verts[vert2].position.y * x + verts[vert4].position.y * y + verts[vert1].position.y);
                    result[newV].position.z = verts[vert1].position.z;

                    result[newV].color.r = (byte)(verts[vert1].color.r * x * y - verts[vert2].color.r * x * y + verts[vert3].color.r * x * y - verts[vert4].color.r * x * y - verts[vert1].color.r * x - verts[vert1].color.r * y + verts[vert2].color.r * x + verts[vert4].color.r * y + verts[vert1].color.r);
                    result[newV].color.g = (byte)(verts[vert1].color.g * x * y - verts[vert2].color.g * x * y + verts[vert3].color.g * x * y - verts[vert4].color.g * x * y - verts[vert1].color.g * x - verts[vert1].color.g * y + verts[vert2].color.g * x + verts[vert4].color.g * y + verts[vert1].color.g);
                    result[newV].color.b = (byte)(verts[vert1].color.b * x * y - verts[vert2].color.b * x * y + verts[vert3].color.b * x * y - verts[vert4].color.b * x * y - verts[vert1].color.b * x - verts[vert1].color.b * y + verts[vert2].color.b * x + verts[vert4].color.b * y + verts[vert1].color.b);
                    result[newV].color.a = (byte)(verts[vert1].color.a * x * y - verts[vert2].color.a * x * y + verts[vert3].color.a * x * y - verts[vert4].color.a * x * y - verts[vert1].color.a * x - verts[vert1].color.a * y + verts[vert2].color.a * x + verts[vert4].color.a * y + verts[vert1].color.a);

                    result[newV].uv.x = (verts[vert1].uv.x * x * y - verts[vert2].uv.x * x * y + verts[vert3].uv.x * x * y - verts[vert4].uv.x * x * y - verts[vert1].uv.x * x - verts[vert1].uv.x * y + verts[vert2].uv.x * x + verts[vert4].uv.x * y + verts[vert1].uv.x);
                    result[newV].uv.y = (verts[vert1].uv.y * x * y - verts[vert2].uv.y * x * y + verts[vert3].uv.y * x * y - verts[vert4].uv.y * x * y - verts[vert1].uv.y * x - verts[vert1].uv.y * y + verts[vert2].uv.y * x + verts[vert4].uv.y * y + verts[vert1].uv.y);

                    result[newV].uv1.x = (verts[vert1].uv1.x * x * y - verts[vert2].uv1.x * x * y + verts[vert3].uv1.x * x * y - verts[vert4].uv1.x * x * y - verts[vert1].uv1.x * x - verts[vert1].uv1.x * y + verts[vert2].uv1.x * x + verts[vert4].uv1.x * y + verts[vert1].uv1.x);
                    result[newV].uv1.y = (verts[vert1].uv1.y * x * y - verts[vert2].uv1.y * x * y + verts[vert3].uv1.y * x * y - verts[vert4].uv1.y * x * y - verts[vert1].uv1.y * x - verts[vert1].uv1.y * y + verts[vert2].uv1.y * x + verts[vert4].uv1.y * y + verts[vert1].uv1.y);


                    //Tangents[newV] = Tangents[a] + (Tangents[b] - Tangents[a]) * t;
                    //Normals[newV] = Normals[a] + (Normals[b] - Normals[a]) * t;
                    result[newV].characterID = verts[vert1].characterID;

                    result[newV].mode = verts[vert1].mode;

                    result[newV].byte0 = (byte)(verts[vert1].byte0 * x * y - verts[vert2].byte0 * x * y + verts[vert3].byte0 * x * y - verts[vert4].byte0 * x * y - verts[vert1].byte0 * x - verts[vert1].byte0 * y + verts[vert2].byte0 * x + verts[vert4].byte0 * y + verts[vert1].byte0);
                    result[newV].byte1 = (byte)(verts[vert1].byte1 * x * y - verts[vert2].byte1 * x * y + verts[vert3].byte1 * x * y - verts[vert4].byte1 * x * y - verts[vert1].byte1 * x - verts[vert1].byte1 * y + verts[vert2].byte1 * x + verts[vert4].byte1 * y + verts[vert1].byte1);
                    result[newV].byte2 = (byte)(verts[vert1].byte2 * x * y - verts[vert2].byte2 * x * y + verts[vert3].byte2 * x * y - verts[vert4].byte2 * x * y - verts[vert1].byte2 * x - verts[vert1].byte2 * y + verts[vert2].byte2 * x + verts[vert4].byte2 * y + verts[vert1].byte2);
                    result[newV].byte3 = (byte)(verts[vert1].byte3 * x * y - verts[vert2].byte3 * x * y + verts[vert3].byte3 * x * y - verts[vert4].byte3 * x * y - verts[vert1].byte3 * x - verts[vert1].byte3 * y + verts[vert2].byte3 * x + verts[vert4].byte3 * y + verts[vert1].byte3);
                    result[newV].byte4 = (byte)(verts[vert1].byte4 * x * y - verts[vert2].byte4 * x * y + verts[vert3].byte4 * x * y - verts[vert4].byte4 * x * y - verts[vert1].byte4 * x - verts[vert1].byte4 * y + verts[vert2].byte4 * x + verts[vert4].byte4 * y + verts[vert1].byte4);
                    result[newV].byte5 = (byte)(verts[vert1].byte5 * x * y - verts[vert2].byte5 * x * y + verts[vert3].byte5 * x * y - verts[vert4].byte5 * x * y - verts[vert1].byte5 * x - verts[vert1].byte5 * y + verts[vert2].byte5 * x + verts[vert4].byte5 * y + verts[vert1].byte5);

                    result[newV].float0 = (byte)(verts[vert1].float0 * x * y - verts[vert2].float0 * x * y + verts[vert3].float0 * x * y - verts[vert4].float0 * x * y - verts[vert1].float0 * x - verts[vert1].float0 * y + verts[vert2].float0 * x + verts[vert4].float0 * y + verts[vert1].float0);
                    result[newV].float1 = (byte)(verts[vert1].float1 * x * y - verts[vert2].float1 * x * y + verts[vert3].float1 * x * y - verts[vert4].float1 * x * y - verts[vert1].float1 * x - verts[vert1].float1 * y + verts[vert2].float1 * x + verts[vert4].float1 * y + verts[vert1].float1);
                    result[newV].float2 = (byte)(verts[vert1].float2 * x * y - verts[vert2].float2 * x * y + verts[vert3].float2 * x * y - verts[vert4].float2 * x * y - verts[vert1].float2 * x - verts[vert1].float2 * y + verts[vert2].float2 * x + verts[vert4].float2 * y + verts[vert1].float2);
                    result[newV].float3 = (byte)(verts[vert1].float3 * x * y - verts[vert2].float3 * x * y + verts[vert3].float3 * x * y - verts[vert4].float3 * x * y - verts[vert1].float3 * x - verts[vert1].float3 * y + verts[vert2].float3 * x + verts[vert4].float3 * y + verts[vert1].float3);
                }
            }
        }
    }
}