using UnityEngine;
using System.Collections.Generic;

namespace I2.SmartEdge
{
    public partial class SmartEdge
    {
        private byte mGlowSurface, mGlowEdge;

        protected void AddGlowVertices()
        {
            if (!rparams.HasGlow() || rparams._GlowLayer == SmartEdgeRenderParams.eGlowLayer.Simple)
                return;

            float edge = rparams.GetEdge_Outline(0) - rparams._GlowOffset;
            float surface = edge + rparams._GlowEdgeWidth;

            if (edge < 0) edge = 0;
            if (surface > 1) surface = 1;

            // Make range 1..255 to avoid weird issues at the border
            //float maxVal = 0.5f / (mSpread);
            //surface = maxVal + (1 - maxVal) * surface;
            //edge = maxVal + (1 - maxVal) * edge;

            mGlowSurface = (byte)(0xff * surface);
            mGlowEdge = (byte)(0xff * edge);

            byte innerWidth = (byte)((0xff - mGlowSurface) * rparams._GlowInnerWidth);
            byte outerWidth = (byte)(mGlowEdge * rparams._GlowOuterWidth);

            if (rparams._GlowLayer == SmartEdgeRenderParams.eGlowLayer.Front)
                AddGlowVertices(mSEMesh.mLayers[(int)SEVerticesLayers.Glow_Front], innerWidth, outerWidth);
            else
                AddGlowVertices(mSEMesh.mLayers[(int)SEVerticesLayers.Glow_Back], innerWidth, outerWidth);

            if (rparams._GlowLayer == SmartEdgeRenderParams.eGlowLayer.TwoLayers)
                ReuseGlowVertices(innerWidth, 15);
        }

        protected void AddGlowVertices(ArrayBufferSEVertex vertices, byte innerWidth, byte outerWidth)
        {
            var mNumVertices = mOriginalVertices.Size;

            vertices.CopyFrom(mOriginalVertices);

            Color mGlowColor;
            mGlowColor.r = rparams._GlowColor.r / 255f;
            mGlowColor.g = rparams._GlowColor.g / 255f;
            mGlowColor.b = rparams._GlowColor.b / 255f;
            mGlowColor.a = mWidgetColor.a * rparams._GlowColor.a / 255f;


            ////--[ UV Mapping Setup ]-------------------
            bool hasGlowTexture = rparams._GlowTexture._Enable && rparams._GlowTexture._Texture;
            if (hasGlowTexture)
                rparams._GlowTexture.InitMapping(mRectPivot, mCharacters.Size);

            for (int index = 0; index < mNumVertices; index++)
            {
                vertices.Buffer[index].mode = SEVertex.MODE_Glow;

                vertices.Buffer[index].color.r = (byte)(vertices.Buffer[index].color.r * mGlowColor.r);
                vertices.Buffer[index].color.g = (byte)(vertices.Buffer[index].color.g * mGlowColor.g);
                vertices.Buffer[index].color.b = (byte)(vertices.Buffer[index].color.b * mGlowColor.b);
                vertices.Buffer[index].color.a = (byte)(vertices.Buffer[index].color.a * mGlowColor.a);

                vertices.Buffer[index].byte0 = mGlowSurface;
                vertices.Buffer[index].byte1 = mGlowEdge;

                vertices.Buffer[index].position.x += rparams._GlowPosition.x;
                vertices.Buffer[index].position.y += rparams._GlowPosition.y;
                vertices.Buffer[index].position.z += rparams._GlowPosition.z;

                // FaceUV
                if (hasGlowTexture)
                {
                    v2 = rparams._GlowTexture.GetUV_inlined(this, vertices, index, mWidgetRectMin, mWidgetRectMax);
                    vertices.Buffer[index].float0 = v2.x;
                    vertices.Buffer[index].float1 = v2.y;
                }

                vertices.Buffer[index].byte2 = innerWidth;                                            // glowInnerWidth
                vertices.Buffer[index].byte3 = outerWidth;                                            // glowOuterWidth
                vertices.Buffer[index].byte4 = (byte)(rparams._GlowIntensity > 10 ? 0xff : rparams._GlowIntensity < 0 ? 0 : (0xff * rparams._GlowIntensity / 10f));        // glowIntensity
                vertices.Buffer[index].byte5 = (byte)(rparams._GlowPower > 1 ? 0xff : rparams._GlowPower < 0 ? 0 : 1+(254 * rparams._GlowPower));                     // glowPower
            }

            if (rparams._EnableGlowGradient)
                rparams._GlowGradient.ModifyVertices(this, vertices, false);
        }

        protected void ReuseGlowVertices(byte innerWidth, byte outerWidth)
        {
            var backGlowVertices  = mSEMesh.mLayers[(int)SEVerticesLayers.Glow_Back];
            var frontGlowVertices = mSEMesh.mLayers[(int)SEVerticesLayers.Glow_Front];

            frontGlowVertices.CopyFrom(backGlowVertices);

            for (int index = 0; index < frontGlowVertices.Size; index++)
            {
                frontGlowVertices.Buffer[index].byte2 = innerWidth;                                            // glowInnerWidth
                frontGlowVertices.Buffer[index].byte3 = outerWidth;                                            // glowOuterWidth
            }
        }
    }
}