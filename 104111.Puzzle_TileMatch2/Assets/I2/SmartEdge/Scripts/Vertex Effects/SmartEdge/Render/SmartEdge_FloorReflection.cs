using UnityEngine;
using System.Collections.Generic;

namespace I2.SmartEdge
{
    public partial class SmartEdge
    {
        static ArrayBufferSEVertex mTempReflectionBuffer = new ArrayBufferSEVertex();
        void AddReflectionVertices()
        {
            if (!rparams._EnableFloorReflection)
                return;

            int approxNumVertices = 0;
            for (int iLayer = 1; iLayer < mSEMesh.numLayers; ++iLayer)
                approxNumVertices += mSEMesh.mLayers[iLayer].Size;
            mTempReflectionBuffer.Clear(approxNumVertices);

            var layer = (int)(rparams._FloorReflection_Back ? SEVerticesLayers.FloorReflection_Back : SEVerticesLayers.FloorReflection_Front);

            if (rparams._FloorReflection_EnableFloorClamp)
            {
                for (int iLayer=1; iLayer<mSEMesh.numLayers; ++iLayer)
                    if (iLayer != (int)SEVerticesLayers.Shadow && mSEMesh.mLayers[iLayer].Size>0)
                        Reflection_CopyCappedVertices(mTempReflectionBuffer, mSEMesh.mLayers[iLayer], rparams._FloorReflectionFloor, -1, true);

                Reflection_CopyCappedVertices(mSEMesh.mLayers[layer], mTempReflectionBuffer, rparams._FloorReflectionDistance, 1, false);
            }
            else
            {
                for (int iLayer = 1; iLayer < mSEMesh.numLayers; ++iLayer)
                    if (iLayer != (int)SEVerticesLayers.Shadow && mSEMesh.mLayers[iLayer].Size > 0)
                        Reflection_CopyCappedVertices(mSEMesh.mLayers[layer], mSEMesh.mLayers[iLayer], rparams._FloorReflectionDistance, 1, false);
            }

            Reflection_ApplyFading(mSEMesh.mLayers[layer]);  
        }

        void Reflection_CopyCappedVertices(ArrayBufferSEVertex dest, ArrayBufferSEVertex source, float distance, int side, bool ignoreElementRegion)
        {
            float AllElementsHeight = (rparams._FloorReflection_Region == eEffectRegion.RectTransform) ? (mWidgetRectMax.y - mWidgetRectMin.y) : (mAllCharactersMax.y - mAllCharactersMin.y);
            float MaxDistance = distance * AllElementsHeight;

            var sourceVerts = source.Buffer;
            var sourceSize = source.Size;

            int consecutiveVertsIn = 0;
            int iDestVertex = dest.Size;
            for (int i = 0; i < sourceSize; i += 4)
            {
                int i1 = 0; int i2 = 1; int i3 = 2; int i4 = 3;

                int charID = sourceVerts[i+i1].characterID;

                float BottomY = mCharacters.Buffer[charID].Min.y;

                if (rparams._FloorReflection_Region == eEffectRegion.Element && !ignoreElementRegion)
                {
                    AllElementsHeight = (mCharacters.Buffer[charID].Max.y - BottomY);
                    MaxDistance = distance * AllElementsHeight;
                }
                else
                if (rparams._FloorReflection_Region == eEffectRegion.RectTransform)
                    BottomY = mWidgetRectMin.y;
                else
                    BottomY = mAllCharactersMin.y;

                float MaxY = BottomY + MaxDistance;


                int side1 = sourceVerts[i + i1].position.y > MaxY + 0.001f ? 1 : (sourceVerts[i + i1].position.y < MaxY - 0.001f ? -1 : side);
                int side2 = sourceVerts[i + i2].position.y > MaxY + 0.001f ? 1 : (sourceVerts[i + i2].position.y < MaxY - 0.001f ? -1 : side);
                int side3 = sourceVerts[i + i3].position.y > MaxY + 0.001f ? 1 : (sourceVerts[i + i3].position.y < MaxY - 0.001f ? -1 : side);
                int side4 = sourceVerts[i + i4].position.y > MaxY + 0.001f ? 1 : (sourceVerts[i + i4].position.y < MaxY - 0.001f ? -1 : side);

                // All vertices in
                if (side1 != side && side2 != side && side3 != side && side4 != side)
                {
                    consecutiveVertsIn += 4;
                    continue;
                }

                if (consecutiveVertsIn > 0)
                {
                    if (dest.Buffer.Length < iDestVertex + consecutiveVertsIn)
                        dest.ReserveTotal(iDestVertex + consecutiveVertsIn);

                    System.Array.Copy(sourceVerts, i - consecutiveVertsIn, dest.Buffer, iDestVertex, consecutiveVertsIn);
                    iDestVertex += consecutiveVertsIn;
                    consecutiveVertsIn = 0;
                }



                // All vertices out
                if (side1 == side && side2 == side && side3 == side && side4 == side)
                    continue;

                while (true)
                { 
                    if (side1 == side2 && side1 != side3 && side1 != side4 && side1==side) break;// Case 1 (12A34B) 
                    if (side1 != side2 && side1 != side3 && side1 != side4) break;// Case 2 (1A234B)
                    var temp = i1; i1 = i2; i2 = i3; i3 = i4; i4 = temp;
                    temp = side1; side1 = side2; side2 = side3; side3 = side4; side4 = temp;
                }
                

                int iA, iB, iA1, iA2, iB1, iB2;

                // Case 1 (12A34B) -> (BA34)
                if (side1==side2)
                {
                    if (dest.Buffer.Length < iDestVertex + 4)
                        dest.ReserveTotal(iDestVertex + 4);

                    iA1 = i+i2;   iA2 = i+i3;   iB1 = i+i4;   iB2 = i+i1;
                    iB = iDestVertex;
                    iA = iDestVertex + 1;
                    dest.Buffer[iDestVertex + 2] = sourceVerts[i+i3];
                    dest.Buffer[iDestVertex + 3] = sourceVerts[i+i4];
                    iDestVertex += 4;
                }

                // Case 2.1 (1A234B) -> (BA24, 2344)
                else
                if (side1==side)
                {
                    if (dest.Buffer.Length < iDestVertex + 8)
                        dest.ReserveTotal(iDestVertex+8);

                    iA1 = i+i1; iA2 = i+i2; iB1 = i+i4; iB2 = i+i1;

                    iB = iDestVertex;
                    iA = iDestVertex + 1;
                    dest.Buffer[iDestVertex + 2] = sourceVerts[i+i2];
                    dest.Buffer[iDestVertex + 3] = sourceVerts[i+i4];

                    dest.Buffer[iDestVertex + 4] = sourceVerts[i+i2];
                    dest.Buffer[iDestVertex + 5] = sourceVerts[i+i3];
                    dest.Buffer[iDestVertex + 6] = sourceVerts[i+i4];
                    dest.Buffer[iDestVertex + 7] = sourceVerts[i+i4];

                    iDestVertex += 8;
                }
                else
                {   // Case 2.2 (1A234B) -> (BA24)
                    if (dest.Buffer.Length < iDestVertex + 4)
                        dest.ReserveTotal(iDestVertex + 4);

                    iA1 = i+i2; iA2 = i+i1; iB1 = i+i1; iB2 = i+i4;
                    iA = iDestVertex;
                    iB = iDestVertex + 1;
                    dest.Buffer[iDestVertex + 2] = sourceVerts[i+i1];
                    dest.Buffer[iDestVertex + 3] = sourceVerts[i+i1];
                    iDestVertex += 4;
                }


                float t = (MaxY - sourceVerts[iA2].position.y) / (sourceVerts[iA1].position.y - sourceVerts[iA2].position.y);
                dest.SplitSegment_Inlined(iA, sourceVerts, iA2, iA1, t>0?t:-t);

                t = (MaxY - sourceVerts[iB1].position.y) / (sourceVerts[iB2].position.y - sourceVerts[iB1].position.y);
                dest.SplitSegment_Inlined(iB, sourceVerts, iB1, iB2, t>0?t:-t);
            }
            if (consecutiveVertsIn > 0)
            {
                if (dest.Buffer.Length < iDestVertex + consecutiveVertsIn)
                    dest.ReserveTotal(iDestVertex + consecutiveVertsIn);
                System.Array.Copy(sourceVerts, sourceSize - consecutiveVertsIn, dest.Buffer, iDestVertex, consecutiveVertsIn);
                iDestVertex += consecutiveVertsIn;
            }
            dest.Size = iDestVertex;
        }

        void Reflection_ApplyFading( ArrayBufferSEVertex dest )
        {
            float GlobalAllElementsHeight = (rparams._FloorReflection_Region == eEffectRegion.RectTransform) ? (mWidgetRectMax.y - mWidgetRectMin.y) : (mAllCharactersMax.y - mAllCharactersMin.y);
            float GlobalOffset = -rparams._FloorReflectionPlane * GlobalAllElementsHeight;

            for (int i = 0; i < dest.Size; i++)
            {
                int charID = dest.Buffer[i].characterID;

                float BottomY = mCharacters.Buffer[charID].Min.y;

                float offset = GlobalOffset;
                float AllElementsHeight = GlobalAllElementsHeight;

                if (rparams._FloorReflection_Region == eEffectRegion.Element && !rparams._FloorReflection_EnableFloorClamp)
                {
                    AllElementsHeight = (mCharacters.Buffer[charID].Max.y - BottomY);
                    offset = -rparams._FloorReflectionPlane * GlobalAllElementsHeight;
                }
                else
                if (rparams._FloorReflection_Region == eEffectRegion.RectTransform)
                    BottomY = mWidgetRectMin.y;
                else
                    BottomY = mAllCharactersMin.y;

                float floorD = 0;
                if (rparams._FloorReflection_EnableFloorClamp)
                {
                    floorD = rparams._FloorReflectionFloor * AllElementsHeight;
                    BottomY += floorD;
                    offset -= floorD;
                }

                float dist = (dest.Buffer[i].position.y - BottomY);
                dest.Buffer[i].position.y = BottomY - dist;
                dest.Buffer[i].position.y += offset;

                float f = dist / (rparams._FloorReflectionDistance * AllElementsHeight-floorD);           f = f < 0 ? 0 : f > 1 ? 1 : f;
                float opacity = rparams._FloorReflectionOpacity * (1 - f);

                //mesh.Colors[i] = SmartEdge.ColorMix(mesh.Colors[i], rparams._FloorReflectionTint_Color, rparams._FloorReflectionTint_BlendMode, rparams._FloorReflectionTint_Opacity);
                #region SmartEdge.ColorMix(mesh.Colors[i], rparams._FloorReflectionTint_Color, rparams._FloorReflectionTint_BlendMode, rparams._FloorReflectionTint_Opacity);
                //-- Color Mix -----------------
                switch (rparams._FloorReflectionTint_BlendMode)
                {
                    case eColorBlend.BlendRGBA:
                        if (rparams._FloorReflectionTint_Opacity > 0)
                        {
                            dest.Buffer[i].color.r = (byte)(dest.Buffer[i].color.r + (rparams._FloorReflectionTint_Color.r - dest.Buffer[i].color.r) * rparams._FloorReflectionTint_Opacity);
                            dest.Buffer[i].color.g = (byte)(dest.Buffer[i].color.g + (rparams._FloorReflectionTint_Color.g - dest.Buffer[i].color.g) * rparams._FloorReflectionTint_Opacity);
                            dest.Buffer[i].color.b = (byte)(dest.Buffer[i].color.b + (rparams._FloorReflectionTint_Color.b - dest.Buffer[i].color.b) * rparams._FloorReflectionTint_Opacity);
                            dest.Buffer[i].color.a = (byte)(dest.Buffer[i].color.a + (rparams._FloorReflectionTint_Color.a - dest.Buffer[i].color.a) * rparams._FloorReflectionTint_Opacity);
                        }
                        break;

                    case eColorBlend.BlendRGB:
                        if (rparams._FloorReflectionTint_Opacity > 0 && rparams._FloorReflectionTint_Color.a > 0)
                        {
                            var ff = rparams._FloorReflectionTint_Opacity * rparams._FloorReflectionTint_Color.a / 255f;
                            dest.Buffer[i].color.r = (byte)(dest.Buffer[i].color.r + (rparams._FloorReflectionTint_Color.r - dest.Buffer[i].color.r) * ff);
                            dest.Buffer[i].color.g = (byte)(dest.Buffer[i].color.g + (rparams._FloorReflectionTint_Color.g - dest.Buffer[i].color.g) * ff);
                            dest.Buffer[i].color.b = (byte)(dest.Buffer[i].color.b + (rparams._FloorReflectionTint_Color.b - dest.Buffer[i].color.b) * ff);
                        }
                        break;

                    case eColorBlend.Combine: //currentColor = Color32.Lerp(currentColor, color, _Opacity * color.a/ 255f);
                        if (rparams._FloorReflectionTint_Opacity > 0 && rparams._FloorReflectionTint_Color.a > 0)
                        {
                            var ff = rparams._FloorReflectionTint_Opacity * rparams._FloorReflectionTint_Color.a / 255f;
                            dest.Buffer[i].color.r = (byte)(dest.Buffer[i].color.r + (rparams._FloorReflectionTint_Color.r - dest.Buffer[i].color.r) * ff);
                            dest.Buffer[i].color.g = (byte)(dest.Buffer[i].color.g + (rparams._FloorReflectionTint_Color.g - dest.Buffer[i].color.g) * ff);
                            dest.Buffer[i].color.b = (byte)(dest.Buffer[i].color.b + (rparams._FloorReflectionTint_Color.b - dest.Buffer[i].color.b) * ff);
                            dest.Buffer[i].color.a = (byte)(dest.Buffer[i].color.a + (rparams._FloorReflectionTint_Color.a - dest.Buffer[i].color.a) * ff);
                        }
                        break;

                    case eColorBlend.Multiply:
                        if (rparams._FloorReflectionTint_Opacity < 1) // mesh.Colors[i] * Color.Lerp(Color.white, mesh.Colors[i], Opacity)
                        {
                            dest.Buffer[i].color.r = (byte)(dest.Buffer[i].color.r * (1 - (1 - rparams._FloorReflectionTint_Color.r / 255f) * rparams._FloorReflectionTint_Opacity));
                            dest.Buffer[i].color.g = (byte)(dest.Buffer[i].color.g * (1 - (1 - rparams._FloorReflectionTint_Color.g / 255f) * rparams._FloorReflectionTint_Opacity));
                            dest.Buffer[i].color.b = (byte)(dest.Buffer[i].color.b * (1 - (1 - rparams._FloorReflectionTint_Color.b / 255f) * rparams._FloorReflectionTint_Opacity));
                            dest.Buffer[i].color.a = (byte)(dest.Buffer[i].color.a * (1 - (1 - rparams._FloorReflectionTint_Color.a / 255f) * rparams._FloorReflectionTint_Opacity));
                        }
                        else
                        {
                            dest.Buffer[i].color.r = (byte)(rparams._FloorReflectionTint_Color.r * dest.Buffer[i].color.r / 255f);
                            dest.Buffer[i].color.g = (byte)(rparams._FloorReflectionTint_Color.g * dest.Buffer[i].color.g / 255f);
                            dest.Buffer[i].color.b = (byte)(rparams._FloorReflectionTint_Color.b * dest.Buffer[i].color.b / 255f);
                            dest.Buffer[i].color.a = (byte)(rparams._FloorReflectionTint_Color.a * dest.Buffer[i].color.a / 255f);
                        }
                        break;

                }
                #endregion
                dest.Buffer[i].color.a = (byte)(dest.Buffer[i].color.a * opacity);

                if (dest.Buffer[i].mode == SEVertex.MODE_Main)
                {
                    Color32 outlineColor;
                    outlineColor.r = dest.Buffer[i].byte2;
                    outlineColor.g = dest.Buffer[i].byte3;
                    outlineColor.b = dest.Buffer[i].byte4;
                    outlineColor.a = dest.Buffer[i].byte5;

                    #region outlineColor = SmartEdge.ColorMix(outlineColor, rparams._FloorReflectionTint_Color, rparams._FloorReflectionTint_BlendMode, rparams._FloorReflectionTint_Opacity);
                    switch (rparams._FloorReflectionTint_BlendMode)
                    {
                        case eColorBlend.BlendRGBA:
                            if (rparams._FloorReflectionTint_Opacity > 0)
                            {
                                outlineColor.r = (byte)(outlineColor.r + (rparams._FloorReflectionTint_Color.r - outlineColor.r) * rparams._FloorReflectionTint_Opacity);
                                outlineColor.g = (byte)(outlineColor.g + (rparams._FloorReflectionTint_Color.g - outlineColor.g) * rparams._FloorReflectionTint_Opacity);
                                outlineColor.b = (byte)(outlineColor.b + (rparams._FloorReflectionTint_Color.b - outlineColor.b) * rparams._FloorReflectionTint_Opacity);
                                outlineColor.a = (byte)(outlineColor.a + (rparams._FloorReflectionTint_Color.a - outlineColor.a) * rparams._FloorReflectionTint_Opacity);
                            }
                            break;

                        case eColorBlend.BlendRGB:
                            if (rparams._FloorReflectionTint_Opacity > 0 && rparams._FloorReflectionTint_Color.a > 0)
                            {
                                var ff = rparams._FloorReflectionTint_Opacity * rparams._FloorReflectionTint_Color.a / 255f;
                                outlineColor.r = (byte)(outlineColor.r + (rparams._FloorReflectionTint_Color.r - outlineColor.r) * ff);
                                outlineColor.g = (byte)(outlineColor.g + (rparams._FloorReflectionTint_Color.g - outlineColor.g) * ff);
                                outlineColor.b = (byte)(outlineColor.b + (rparams._FloorReflectionTint_Color.b - outlineColor.b) * ff);
                            }
                            break;

                        case eColorBlend.Combine: //outlineColor = Color32.Lerp(outlineColor, rparams._FloorReflectionTint_Color, rparams._FloorReflectionTint_Opacity * rparams._FloorReflectionTint_Color.a/ 255f);
                            if (rparams._FloorReflectionTint_Opacity > 0 && rparams._FloorReflectionTint_Color.a > 0)
                            {
                                var ff = rparams._FloorReflectionTint_Opacity * rparams._FloorReflectionTint_Color.a / 255f;
                                outlineColor.r = (byte)(outlineColor.r + (rparams._FloorReflectionTint_Color.r - outlineColor.r) * ff);
                                outlineColor.g = (byte)(outlineColor.g + (rparams._FloorReflectionTint_Color.g - outlineColor.g) * ff);
                                outlineColor.b = (byte)(outlineColor.b + (rparams._FloorReflectionTint_Color.b - outlineColor.b) * ff);
                                outlineColor.a = (byte)(outlineColor.a + (rparams._FloorReflectionTint_Color.a - outlineColor.a) * ff);
                            }
                            break;

                        case eColorBlend.Multiply:
                            if (rparams._FloorReflectionTint_Opacity < 1) // outlineColor * Color.Lerp(Color.white, rparams._FloorReflectionTint_Color, Opacity)
                            {
                                outlineColor.r = (byte)(outlineColor.r * (1 - (1 - rparams._FloorReflectionTint_Color.r / 255f) * rparams._FloorReflectionTint_Opacity));
                                outlineColor.g = (byte)(outlineColor.g * (1 - (1 - rparams._FloorReflectionTint_Color.g / 255f) * rparams._FloorReflectionTint_Opacity));
                                outlineColor.b = (byte)(outlineColor.b * (1 - (1 - rparams._FloorReflectionTint_Color.b / 255f) * rparams._FloorReflectionTint_Opacity));
                                outlineColor.a = (byte)(outlineColor.a * (1 - (1 - rparams._FloorReflectionTint_Color.a / 255f) * rparams._FloorReflectionTint_Opacity));
                            }
                            else
                            {
                                outlineColor.r = (byte)(rparams._FloorReflectionTint_Color.r * outlineColor.r / 255f);
                                outlineColor.g = (byte)(rparams._FloorReflectionTint_Color.g * outlineColor.g / 255f);
                                outlineColor.b = (byte)(rparams._FloorReflectionTint_Color.b * outlineColor.b / 255f);
                                outlineColor.a = (byte)(rparams._FloorReflectionTint_Color.a * outlineColor.a / 255f);
                            }
                            break;

                    }
                    #endregion

                    dest.Buffer[i].byte2 = outlineColor.r;
                    dest.Buffer[i].byte3 = outlineColor.g;
                    dest.Buffer[i].byte4 = outlineColor.b;
                    dest.Buffer[i].byte5 = (byte)(outlineColor.a * opacity);
                }
            }
        }

    }
}