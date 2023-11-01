using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace I2.SmartEdge
{
	public partial class GradientEffect
	{
        // Cache
        private static Vector2[] mGradientP0  = new Vector2[0];
        private static Vector2[] mGradientDir = new Vector2[0];
        private static float[] mGradientSize  = new float[0];

        // cache for the ApplyColors: Flat
        private static int[] mIndexFirstVertexInRegion = new int[0];
        private static Color32[] mFlatColorOfRegion = new Color32[0];

        struct GradientKey
        {
            public Color32 color;
            public float time;
        }
        GradientKey[] mGradientKeys = new GradientKey[0];
        int mNumGradientKeys;

        public void SetGradientDirty()
        {
            mNumGradientKeys = 0;
            mGradientKeys = null;
        }


        public void ModifyVertices(SmartEdge se, ArrayBufferSEVertex layer, bool IsOutline)
        {
            if (_Opacity <= 0)
                return;

            if (mNumGradientKeys == 0 || mGradientKeys == null || mNumGradientKeys > mGradientKeys.Length)
                GenerateGradientKeys();

            GenerateGradientPoints(se);

            if (_Precision == ePrecision.Precise)
                SplitQuads( layer);

            ApplyColors_Inlined(se, layer, IsOutline);
        }

        void GenerateGradientPoints(SmartEdge se)
        { 
            if (mGradientP0.Length<SmartEdge.mCharacters.Size)
            {
                mGradientP0   = new Vector2[SmartEdge.mCharacters.Size];
                mGradientDir  = new Vector2[SmartEdge.mCharacters.Size];
                mGradientSize = new float  [SmartEdge.mCharacters.Size];
            }

            var GradientNormal = Quaternion.Euler(0, 0, -_Angle) * MathUtils.v2down;
            Vector2 GradP0 = MathUtils.v2zero, GradP1 = MathUtils.v2zero;
            Vector2 rMin = MathUtils.v2zero, rMax=MathUtils.v2zero;
            Vector2 gDir = MathUtils.v2zero;
            float gSize = 1f;

            int iNextRegion = 0;
            for (int c = 0; c < SmartEdge.mCharacters.Size; ++c)
            {
                if (c >= iNextRegion)
                {
                    if (_Region == eEffectRegion.Element)
                    {
                        rMin = SmartEdge.mCharacters.Buffer[c].Min;      rMax = SmartEdge.mCharacters.Buffer[c].Max;
                        iNextRegion++;
                    }
                    else
                    {
                        if (_Region == eEffectRegion.AllElements)
                        {
                            rMin = se.mAllCharactersMin; rMax = se.mAllCharactersMax;
                        }
                        else
                        {
                            rMin = se.mWidgetRectMin; rMax = se.mWidgetRectMax;
                        }
                        iNextRegion = SmartEdge.mCharacters.Size;
                    }

                    #region ComputeGradientRegion(rMin, rMax, GradientNormal, out GradP0, out GradP1);
                    float minF = float.MaxValue, maxF = float.MinValue;

                    float f = rMin.x * GradientNormal.x + rMin.y * GradientNormal.y;
                    if (f < minF) minF = f;
                    if (f > maxF) maxF = f;

                    f = rMin.x * GradientNormal.x + rMax.y * GradientNormal.y;
                    if (f < minF) minF = f;
                    if (f > maxF) maxF = f;

                    f = rMax.x * GradientNormal.x + rMin.y * GradientNormal.y;
                    if (f < minF) minF = f;
                    if (f > maxF) maxF = f;

                    f = rMax.x * GradientNormal.x + rMax.y * GradientNormal.y;
                    if (f < minF) minF = f;
                    if (f > maxF) maxF = f;

                    GradP1.x = GradientNormal.x * maxF;
                    GradP1.y = GradientNormal.y * maxF;
                    GradP0.x = GradientNormal.x * minF;
                    GradP0.y = GradientNormal.y * minF;

                    gSize = Mathf.Sqrt((GradP1.x - GradP0.x) * (GradP1.x - GradP0.x) + (GradP1.y - GradP0.y) * (GradP1.y - GradP0.y));
                    gDir.x = (GradP1.x - GradP0.x) / gSize;
                    gDir.y = (GradP1.y - GradP0.y) / gSize;
                    #endregion                    
                }

                mGradientP0[c]   = GradP0;
                mGradientSize[c] = gSize;
                mGradientDir[c].x = gDir.x;
                mGradientDir[c].y = gDir.y;
            }
		}

        private void SplitQuads(ArrayBufferSEVertex mesh)
        {
            Vector2 vv;

            for (int k = 0; k < mNumGradientKeys; ++k)
            {
                float ff = (mGradientKeys[k].time - 0.5f) * (_Scale + 0.01f) + 0.5f;
                ff += _Bias;

                for (int i = mesh.Size - 4; i >= 0; i -= 4)
                {
                    int charID = mesh.Buffer[i].characterID;
                    var planeNormal = mGradientDir[charID];

                    vv.x = mGradientP0[charID].x + planeNormal.x * ff * mGradientSize[charID];
                    vv.y = mGradientP0[charID].y + planeNormal.y * ff * mGradientSize[charID];

                    float planeDist = -(planeNormal.x * vv.x + planeNormal.y * vv.y);

                    #region SEMeshTools.SplitRegion(mesh, startRegionIndex, lastV, GradientNormal, planeDistance, ref mRegionIndices, ref mNumRegionIndices);
                    var i1 = i; var i2 = i + 1; var i3 = i + 2; var i4 = i + 3;
                    var v1 = mesh.Buffer[i1].position;
                    var v2 = mesh.Buffer[i2].position;
                    var v3 = mesh.Buffer[i3].position;
                    var v4 = mesh.Buffer[i4].position;

                    float fside1 = (planeNormal.x * v1.x + planeNormal.y * v1.y) + planeDist;
                    float fside2 = (planeNormal.x * v2.x + planeNormal.y * v2.y) + planeDist;
                    float fside3 = (planeNormal.x * v3.x + planeNormal.y * v3.y) + planeDist;
                    float fside4 = (planeNormal.x * v4.x + planeNormal.y * v4.y) + planeDist;

                    int side1 = fside1 > 0.001f ? 1 : (fside1 < -0.001f ? -1 : 0);  // plane.Side(v1)
                    int side2 = fside2 > 0.001f ? 1 : (fside2 < -0.001f ? -1 : 0);  // plane.Side(v2)
                    int side3 = fside3 > 0.001f ? 1 : (fside3 < -0.001f ? -1 : 0);  // plane.Side(v3)
                    int side4 = fside4 > 0.001f ? 1 : (fside4 < -0.001f ? -1 : 0);  // plane.Side(v4)

                    // all same side
                    if (side1 >= 0 && side2 >= 0 && side3 >= 0 && side4 >= 0)
                        continue;
                    if (side1 <= 0 && side2 <= 0 && side3 <= 0 && side4 <= 0)
                        continue;


                    //--[ Detect Case1 or Case 2]---------------

                    while (side1 == side2 || side1 != side4)  // while not Case 1 or Case 2 shift the list
                    {
                        var tempI = i1; i1 = i2; i2 = i3; i3 = i4; i4 = tempI;
                        var tempV = v1; v1 = v2; v2 = v3; v3 = v4; v4 = tempV;
                        tempI = side1; side1 = side2; side2 = side3; side3 = side4; side4 = tempI;
                    }

                    //--[ Setup to split and generate vA and vB ]---------------------

                    Vector3 vA1, vA2, vB1, vB2;
                    int iA1, iA2, iB1, iB2, iA, iB;
                    int lastV = mesh.Size;

                    if (side2 != side3)  // Case 1:  1A2B34 ->  1ACB, A2BC, 1B34
                    {
                        mesh.ReserveExtra(8);
                        vA1 = v1; vA2 = v2; vB1 = v2; vB2 = v3;
                        iA1 = i1; iA2 = i2; iB1 = i2; iB2 = i3;
                        iA = lastV;     iB = lastV+1;
                    }
                    else // Case 2: 1A23B4 -> 1AB4, A23B
                    {
                        mesh.ReserveExtra(4);
                        vA1 = v1; vA2 = v2; vB1 = v3; vB2 = v4;
                        iA1 = i1; iA2 = i2; iB1 = i3; iB2 = i4;
                        iA = lastV;     iB = lastV+3;
                    }
                    Vector2 mV2;


                    //--[ Find vA ]-------------------------------------------------------
                    //Segment2Plane(vA1, vA2, planeNormal, planeDist)
                    mV2.x = (vA2.x - vA1.x); mV2.y = (vA2.y - vA1.y);
                    var dist01 = Mathf.Sqrt(mV2.x * mV2.x + mV2.y * mV2.y);
                    mV2.x /= dist01; mV2.y /= dist01;
                    float num = mV2.x * planeNormal.x + mV2.y * planeNormal.y;
                    float num2 = -(vA1.x * planeNormal.x + vA1.y * planeNormal.y) - planeDist;
                    float segment12 = (num > -0.001f && num < 0.001f) ? 0f : ((num2 / num) / dist01);
                    mesh.SplitSegment_Inlined(iA, mesh.Buffer, iA1, iA2, segment12);

                    //--[ Find vB ]------------------------------------------------------
                    //Segment2Plane(vB1, vB2, planeNormal, planeDist)
                    mV2.x = (vB2.x - vB1.x); mV2.y = (vB2.y - vB1.y);
                    dist01 = Mathf.Sqrt(mV2.x * mV2.x + mV2.y * mV2.y);
                    mV2.x /= dist01; mV2.y /= dist01;
                    num = mV2.x * planeNormal.x + mV2.y * planeNormal.y;
                    num2 = -(vB1.x * planeNormal.x + vB1.y * planeNormal.y) - planeDist;
                    float segment23 = (num > -0.001f && num < 0.001f) ? 0f : ((num2 / num) / dist01);
                    mesh.SplitSegment_Inlined(iB, mesh.Buffer, iB1, iB2, segment23);


                    if (side2 != side3)  // Case 1:  1A2B34 ->  1A34, AB33, A2BB
                    {
                        //mesh.Buffer[lastV    ] = vA;
                        //mesh.Buffer[lastV + 1] = vB;
                          mesh.Buffer[lastV + 2] = mesh.Buffer[i3];
                          mesh.Buffer[lastV + 3] = mesh.Buffer[i3];

                          mesh.Buffer[lastV + 4] = mesh.Buffer[lastV];// vA;
                          mesh.Buffer[lastV + 5] = mesh.Buffer[i2];
                          mesh.Buffer[lastV + 6] = mesh.Buffer[lastV + 1];// vB;
                          mesh.Buffer[lastV + 7] = mesh.Buffer[lastV + 1];// vB;


                        //mesh.Buffer[i1] = i1;
                          mesh.Buffer[i2] = mesh.Buffer[lastV];// vA;
                        //mesh.Buffer[i3] = i3;
                        //mesh.Buffer[i4] = i4;

                        mesh.Size = lastV + 8;
                    }
                    else // Case 2: 1A23B4 -> 1AB4, A23B
                    {
                        //mesh.Buffer[lastV    ] = vA;
                          mesh.Buffer[lastV + 1] = mesh.Buffer[i2];
                          mesh.Buffer[lastV + 2] = mesh.Buffer[i3];
                        //mesh.Buffer[lastV + 3] = vB;

                        //mesh.Buffer[i1] = i1;
                          mesh.Buffer[i2] = mesh.Buffer[lastV]; // vA;
                          mesh.Buffer[i3] = mesh.Buffer[lastV + 3];// vB;
                        //mesh.Buffer[i4] = i4;

                        mesh.Size = lastV + 4;
                    }
                }
                #endregion
            }
        }

        private void ApplyColors_Inlined(SmartEdge se, ArrayBufferSEVertex layer, bool IsOutline)
        {
            if (_Precision == ePrecision.Flat)
            {
                if (mIndexFirstVertexInRegion.Length < SmartEdge.mCharacters.Size)
                {
                    mIndexFirstVertexInRegion = new int[SmartEdge.mCharacters.Size];
                    mFlatColorOfRegion = new Color32[SmartEdge.mCharacters.Size];
                }
                for (int i = 0; i < SmartEdge.mCharacters.Size; ++i)
                    mIndexFirstVertexInRegion[i] = -1;
            }

            Color32 color = MathUtils.white32;
            var verts = layer.Buffer;
            for (int i = 0; i < layer.Size; ++i)
            {
                var charID = verts[i].characterID;

                Vector2 GradientBase = mGradientDir[charID];
                float BaseSize = mGradientSize[charID];
                var GradP0 = mGradientP0[charID];

                Color32 currentColor;
                if (IsOutline)
                {
                    currentColor.r = verts[i].byte2;
                    currentColor.g = verts[i].byte3;
                    currentColor.b = verts[i].byte4;
                    currentColor.a = verts[i].byte5;
                }
                else
                {
                    currentColor = verts[i].color;
                }

                if (_Precision != ePrecision.Flat || (mIndexFirstVertexInRegion[charID]<0))
                {
                    var pos = verts[i].position;

                    pos.x = (pos.x - GradP0.x) / BaseSize;  // SqrDist(pos-p0)/size
                    pos.y = (pos.y - GradP0.y) / BaseSize;
                    float f = pos.x * GradientBase.x + pos.y * GradientBase.y; // dot

                    f -= _Bias;
                    f = (f - 0.5f) / (_Scale + 0.01f) + 0.5f;

                    int g = 0;
                    while (g < mNumGradientKeys - 1 && mGradientKeys[g].time < f)
                        g++;

                    //color = _Gradient.Evaluate(f);
                    if (g == 0)
                        color = mGradientKeys[0].color;
                    else
                    if (g == mNumGradientKeys)
                        color = mGradientKeys[g - 1].color;
                    else
                    {
                        float t = (f - mGradientKeys[g - 1].time) / (mGradientKeys[g].time - mGradientKeys[g - 1].time);
                        if (t > 1) t = 1;
                        if (t < 0) t = 0;
                        color.r = (byte)(mGradientKeys[g - 1].color.r + (mGradientKeys[g].color.r - (int)mGradientKeys[g - 1].color.r) * t);
                        color.g = (byte)(mGradientKeys[g - 1].color.g + (mGradientKeys[g].color.g - (int)mGradientKeys[g - 1].color.g) * t);
                        color.b = (byte)(mGradientKeys[g - 1].color.b + (mGradientKeys[g].color.b - (int)mGradientKeys[g - 1].color.b) * t);
                        color.a = (byte)(mGradientKeys[g - 1].color.a + (mGradientKeys[g].color.a - (int)mGradientKeys[g - 1].color.a) * t);
                    }

                    if (_Precision == ePrecision.Flat)
                    {
                        mIndexFirstVertexInRegion[charID] = i;
                        mFlatColorOfRegion[charID] = color;
                    }
                }
                else
                {
                    color = mFlatColorOfRegion[charID];
                }

                //-- Color Mix -----------------
                switch (_BlendMode)
                {
                    case eColorBlend.BlendRGBA:
                        if (_Opacity < 1)
                        {
                            color.r = (byte)(currentColor.r + (color.r - currentColor.r) * _Opacity);
                            color.g = (byte)(currentColor.g + (color.g - currentColor.g) * _Opacity);
                            color.b = (byte)(currentColor.b + (color.b - currentColor.b) * _Opacity);
                            color.a = (byte)(currentColor.a + (color.a - currentColor.a) * _Opacity);
                        }
                        break;

                    case eColorBlend.BlendRGB:
                        if (_Opacity < 1 || color.a < 255)
                        {
                            var f = _Opacity * color.a / 255f;
                            color.r = (byte)(currentColor.r + (color.r - currentColor.r) * f);
                            color.g = (byte)(currentColor.g + (color.g - currentColor.g) * f);
                            color.b = (byte)(currentColor.b + (color.b - currentColor.b) * f);
                        }
                        break;

                    case eColorBlend.Combine: //currentColor = Color32.Lerp(currentColor, color, _Opacity * color.a/ 255f);
                        if (_Opacity < 1 || color.a < 255)
                        {
                            var f = _Opacity * color.a / 255f;
                            color.r = (byte)(currentColor.r + (color.r - currentColor.r) * f);
                            color.g = (byte)(currentColor.g + (color.g - currentColor.g) * f);
                            color.b = (byte)(currentColor.b + (color.b - currentColor.b) * f);
                            color.a = (byte)(currentColor.a + (color.a - currentColor.a) * f);
                        }
                        break;

                    case eColorBlend.Multiply:
                        if (_Opacity < 1) // currentColor * Color.Lerp(Color.white, color, Opacity)
                        {
                            color.r = (byte)(currentColor.r * (1 - (1 - color.r / 255f) * _Opacity));
                            color.g = (byte)(currentColor.g * (1 - (1 - color.g / 255f) * _Opacity));
                            color.b = (byte)(currentColor.b * (1 - (1 - color.b / 255f) * _Opacity));
                            color.a = (byte)(currentColor.a * (1 - (1 - color.a / 255f) * _Opacity));
                        }
                        else
                        {
                            color.r = (byte)(color.r * currentColor.r / 255f);
                            color.g = (byte)(color.g * currentColor.g / 255f);
                            color.b = (byte)(color.b * currentColor.b / 255f);
                            color.a = (byte)(color.a * currentColor.a / 255f);
                        }
                        break;

                }

                if (IsOutline)
                {
                     verts[i].byte2 = color.r;
                     verts[i].byte3 = color.g;
                     verts[i].byte4 = color.b;
                     verts[i].byte5 = color.a;

                }
                else
                {
                    verts[i].color = color;
                }
            }
        }


         public void GenerateGradientKeys()
        {
            var alphaKeys = _Gradient.alphaKeys;
            var colorKeys = _Gradient.colorKeys;

            int newSize = alphaKeys.Length + colorKeys.Length;
            if (mNumGradientKeys == 0 || mGradientKeys.Length < newSize)
                mGradientKeys = new GradientKey[newSize];

            mNumGradientKeys = 0;
            float nextAlphaTime = alphaKeys[0].time;
            float nextColorTime = colorKeys[0].time;

            int iAlpha = 0, iColor = 0;

            while (iAlpha < alphaKeys.Length && iColor < colorKeys.Length)
            {
                if (nextAlphaTime < nextColorTime)
                {
                    mGradientKeys[mNumGradientKeys].time = nextAlphaTime;
                    iAlpha++;
                    nextAlphaTime = iAlpha < alphaKeys.Length ? alphaKeys[iAlpha].time : float.MaxValue;
                }
                else
                if (nextColorTime < nextAlphaTime)
                {
                    mGradientKeys[mNumGradientKeys].time = nextColorTime;

                    iColor++;
                    nextColorTime = iColor < colorKeys.Length ? colorKeys[iColor].time : float.MaxValue;
                }
                else
                {
                    mGradientKeys[mNumGradientKeys].time = nextColorTime;

                    iAlpha++;
                    iColor++;
                    nextAlphaTime = iAlpha < alphaKeys.Length ? alphaKeys[iAlpha].time : float.MaxValue;
                    nextColorTime = iColor < colorKeys.Length ? colorKeys[iColor].time : float.MaxValue;
                }

                mGradientKeys[mNumGradientKeys].color = _Gradient.Evaluate(mGradientKeys[mNumGradientKeys].time);
                mNumGradientKeys++;
            }
        }

        public void GenerateGradientKeys1()
        {
            var alphaKeys = _Gradient.alphaKeys;
            var colorKeys = _Gradient.colorKeys;

            int newSize = alphaKeys.Length + colorKeys.Length;
            if (mNumGradientKeys==0 || mGradientKeys.Length < newSize)
                mGradientKeys = new GradientKey[newSize];

            mNumGradientKeys = 0;
            float nextAlphaTime = alphaKeys[0].time;
            float nextColorTime = colorKeys[0].time;

            int iAlpha = 0, iColor = 0;

            Color32 prevColor = colorKeys[0].color;
            prevColor.a = (byte)(alphaKeys[0].alpha*0xff);
            float prevTime = -1;

            while (iAlpha<alphaKeys.Length && iColor<colorKeys.Length)
            {
                if (nextAlphaTime < nextColorTime)
                {
                    mGradientKeys[mNumGradientKeys].time = nextAlphaTime;
                    mGradientKeys[mNumGradientKeys].color = (iColor==0 || iColor==colorKeys.Length) ? prevColor : Color32.Lerp(prevColor, (Color32)colorKeys[iColor].color, (nextAlphaTime - prevTime) / (colorKeys[iColor].time - prevTime));
                    mGradientKeys[mNumGradientKeys].color.a = (byte)(alphaKeys[iAlpha].alpha*0xff);
                    iAlpha++;
                    nextAlphaTime = iAlpha < alphaKeys.Length ? alphaKeys[iAlpha].time : float.MaxValue;
                }
                else
                if (nextColorTime < nextAlphaTime )
                {
                    mGradientKeys[mNumGradientKeys].time = nextColorTime;

                    mGradientKeys[mNumGradientKeys].color = colorKeys[iColor].color;
                    mGradientKeys[mNumGradientKeys].color.a = (iAlpha == 0 || iAlpha == alphaKeys.Length) ? prevColor.a : (byte)Mathf.Lerp(prevColor.a, alphaKeys[iAlpha].alpha*255, (nextColorTime - prevTime) / (alphaKeys[iAlpha].time - prevTime));
                    iColor++;
                    nextColorTime = iColor < colorKeys.Length ? colorKeys[iColor].time : float.MaxValue;
                }
                else
                {
                    mGradientKeys[mNumGradientKeys].time = nextColorTime;

                    mGradientKeys[mNumGradientKeys].color.a = (byte)(alphaKeys[iAlpha].alpha*0xff);
                    mGradientKeys[mNumGradientKeys].color = colorKeys[iColor].color;
                    iAlpha++;
                    iColor++;
                    nextAlphaTime = iAlpha < alphaKeys.Length ? alphaKeys[iAlpha].time : float.MaxValue;
                    nextColorTime = iColor < colorKeys.Length ? colorKeys[iColor].time : float.MaxValue;
                }
                prevTime = mGradientKeys[mNumGradientKeys].time;
                prevColor = mGradientKeys[mNumGradientKeys].color;

                mNumGradientKeys++;
            }
        }
    }
}