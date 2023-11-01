using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;


namespace I2.SmartEdge
{
	public enum eColorBlend { Multiply, BlendRGB, BlendRGBA, Combine }

    public partial class SmartEdge
    {
        protected Vector2 v2;
        protected Vector3 v3;
        SmartEdgeRenderParams rparams;  // active params

        public void ModifyVertices()
        {
            mWidgetRectMin = mRect.min;
            mWidgetRectMax = mRect.max;

            //mIsDirty_Vertices = false;

            _TextEffect.ModifyVertices(this);

            ApplyAnimations_Characters();
            

            if (mSETextureFormat != SETextureFormat.Unknown)
            {
                rparams = GetRenderParams(0);

                AddShadowVertices();
                AddFaceVertices();
                AddGlowVertices();

                AddReflectionVertices();

                _TextEffect._BestFit.ModifyVertices(mSEMesh, 1, this);
                _Deformation.ModifyVertices(mSEMesh, 1, this);
                ApplyAnimations_Vertices();

                bool hasFaceTexture = rparams._FaceTexture._Enable && rparams._FaceTexture._Texture;
                bool hasBumpTexture = rparams._NormalMap._Enable && rparams._NormalMap._Texture;
                bool hasGlowTexture = rparams.HasGlow() && rparams._GlowTexture._Enable && rparams._GlowTexture._Texture;
                bool hasOutlineTexture = rparams._OutlineTexture._Enable && rparams._OutlineTexture._Texture;

                for (int iLayer=1; iLayer<mSEMesh.numLayers; ++iLayer)
                    ConvertToUI(mSEMesh.mLayers[iLayer], hasFaceTexture || hasBumpTexture, hasGlowTexture, rparams._EnableOutline, hasOutlineTexture);
                mOriginalVertices.Clear();
            }
            else
            {
                _TextEffect._BestFit.ModifyVertices(mSEMesh, 0, this);
                _Deformation.ModifyVertices(mSEMesh, 0, this);
                ApplyAnimations_Vertices();
            }
        }


        public static Color32 ColorMix(Color32 oldC, Color32 newC, eColorBlend colorBlend, float Opacity)
        {
            switch (colorBlend)
            {
                case eColorBlend.Multiply: oldC *= Color.Lerp(Color.white, newC, Opacity); break;
                case eColorBlend.BlendRGBA: oldC = Color32.Lerp(oldC, newC, Opacity); break;
                case eColorBlend.BlendRGB:
                    {
                        oldC.a = newC.a;
                        oldC = Color32.Lerp(oldC, newC, newC.a * Opacity);
                        break;
                    }
                case eColorBlend.Combine: oldC = Color32.Lerp(oldC, newC, newC.a * Opacity); break;
            }
            return oldC;
        }

        void ConvertToUI(ArrayBufferSEVertex aVertices, bool faceUV, bool glowUV, bool outline, bool outlineUV)
        {
            int UVrange = (1 << 12) - 1;   // 8191
            int u, v;
            faceUV |= (outline && outlineUV) || glowUV;


            // Parameters:
            // UV.x       = PackCharUV( UV0 )
            // UV.y       = PackUV( FaceUV )
            // UV1.y      = PackRGB8 (surface, edge, FaceAlpha, mode)
            //
            //                      MODE_Main           |           MODE_Glow
            // UV1.x      = PackRGBA6( OutlineColor )   |   PackRGBA6 (glowInnerWidth, glowOuterWidth, glowIntensity, glowPower )
            // Tangent.w  = free

            var verts = aVertices.Buffer;
            for (int i = 0; i < aVertices.Size; ++i)
            {
                // byte2..5 = (MODE_Main:Outline)  || (MODE_Glow: glowInnerWidth, glowOuterWidth, glowIntensity, glowPower)
                verts[i].uv1.x = (float)(((verts[i].byte2 >> 2) << 18) | ((verts[i].byte3 >> 2) << 12) | ((verts[i].byte4 >> 2) << 6) | (verts[i].byte5 >> 2));

                // BytePacking.PackRGBA6 (surface, edge, color.a, mode)
                verts[i].uv1.y = (float)(((verts[i].byte0 >> 1) << 17) | ((verts[i].byte1 >> 1) << 10) | ((verts[i].color.a >> 1) << 3) | (verts[i].mode));

                //--[ UV0.x ]-----------------------------------
                // BytePacking.PackCharUV( UV0 )
                u = (verts[i].uv.x > 1) ? UVrange : (verts[i].uv.x < 0 ? 0 : ((int)(verts[i].uv.x * UVrange)));
                v = (verts[i].uv.y > 1) ? UVrange : (verts[i].uv.y < 0 ? 0 : ((int)(verts[i].uv.y * UVrange)));
                verts[i].uv.x = (float)((u << 12) | v);

                verts[i].color.a = 255;

                //--[ UV0.y ]-----------------------------------
                bool addFaceUV = (verts[i].mode == SEVertex.MODE_Main) ? faceUV : glowUV;
                if (addFaceUV)
                {
                    // BytePacking.PackUV( FaceUV )
                    u = ((int)(verts[i].float0 * 64) + (32 * 64)) % 4096;
                    v = ((int)(verts[i].float1 * 64) + (32 * 64)) % 4096;
                    verts[i].uv.y = (float)((u * 4096) | v);
                }
            }
        }
    }
}
