using UnityEngine;
using System.Collections.Generic;

namespace I2.SmartEdge
{
	public partial class SmartEdge
	{
		//static Vector3[] mQuadPos = new Vector3[4];	// Used for caching the positions while applying the SDF Spread

		void AddFaceVertices()
		{
            bool hasOutlines = rparams._EnableOutline && (rparams._OutlineColor.a > 1);
            bool hasSimpleGlow = rparams.HasGlow() && rparams._GlowLayer==SmartEdgeRenderParams.eGlowLayer.Simple;

            if (!rparams._EnableFace && !hasOutlines && !hasSimpleGlow)
				return;

			var faceVertices = mSEMesh.mLayers[(int)SEVerticesLayers.Face];
			faceVertices.CopyFrom(mOriginalVertices);

			//float maxVal = 0*0.5f / (mSpread);
			//float fSurface = maxVal + (1 - maxVal) * rparams.GetEdge_Surface();        // Make range 1..255 to avoid weird issues at the border
			//float fEdge = maxVal + (1 - maxVal) * rparams.GetEdge_Outline();


			//byte surface = (byte)(0xff * rparams.GetEdge_Surface());
			//byte edge = (byte)(0xff * rparams.GetEdge_Outline());

			Color faceColor = rparams._FaceColor;
			/*faceColor.r = (rparams._FaceColor.r * mWidgetColor.r)/255f;
			faceColor.g = (rparams._FaceColor.g * mWidgetColor.g)/255f;
			faceColor.b = (rparams._FaceColor.b * mWidgetColor.b)/255f;
			faceColor.a = (rparams._FaceColor.a * mWidgetColor.a)/255f;*/

			Color mOutlineColor;
			bool useOutline = hasOutlines && rparams._OutlineColor.a > 0;

			if (useOutline)
			{
				mOutlineColor.r = rparams._OutlineColor.r / 255f;
				mOutlineColor.g = rparams._OutlineColor.g / 255f;
				mOutlineColor.b = rparams._OutlineColor.b / 255f;
				mOutlineColor.a = mWidgetColor.a * rparams._OutlineColor.a / 255f;
			}
			else
			{
				mOutlineColor = faceColor;
				mOutlineColor.a = 0;
			}

			if (!rparams._EnableFace || faceColor.a <= 3 / 255f)
			{
				faceColor.r = mOutlineColor.r;
				faceColor.g = mOutlineColor.g;
				faceColor.b = mOutlineColor.b;
				faceColor.a = 0;

			}


			var verts = faceVertices.Buffer;
			for (int index = 0; index < faceVertices.Size; index++)
			{
				verts[index].mode = SEVertex.MODE_Main;

				int iCharacter = verts[index].characterID;
				float bold = mCharacters.Buffer[iCharacter].RichText.Bold;

				byte surface = (byte)(0xff * rparams.GetEdge_Surface(bold));
				byte edge = (byte)(0xff * rparams.GetEdge_Outline(bold));

				verts[index].byte0 = surface;
				verts[index].byte1 = edge;

				//if (hasOutlines)
				{
					// outlineColor
					verts[index].byte2 = (byte)(verts[index].color.r * mOutlineColor.r);
					verts[index].byte3 = (byte)(verts[index].color.g * mOutlineColor.g);
					verts[index].byte4 = (byte)(verts[index].color.b * mOutlineColor.b);
					verts[index].byte5 = (byte)(verts[index].color.a * mOutlineColor.a);
				}

				if (!rparams._EnableFace)
					verts[index].color = faceColor;
				else
				{
					verts[index].color.r = (byte)(verts[index].color.r * faceColor.r);
					verts[index].color.g = (byte)(verts[index].color.g * faceColor.g);
					verts[index].color.b = (byte)(verts[index].color.b * faceColor.b);
					verts[index].color.a = (byte)(verts[index].color.a * faceColor.a);
				}
			}

			bool hasFaceTexture = rparams._FaceTexture._Enable && rparams._FaceTexture._Texture && rparams._FaceTexture._Enable;
            bool hasBumpTexture = rparams._NormalMap._Enable && rparams._NormalMap._Texture && rparams._NormalMap._Enable;
            bool hasGlowTexture = rparams.HasGlow() && rparams._GlowTexture._Texture && rparams._GlowTexture._Enable;

			if (hasFaceTexture || hasBumpTexture || hasGlowTexture)
				ApplyUV(rparams._FaceTexture, false);

			if (useOutline && rparams._UseOutlineLayer)
				AddOutlineLayer();

			if (rparams._EnableFace && rparams._EnableFaceGradient)
				rparams._FaceGradient.ModifyVertices(this, mSEMesh.mLayers[(int)SEVerticesLayers.Face], false);

			if (useOutline && rparams._EnableOutlineGradient)
			{
				var layer = rparams._UseOutlineLayer ? SEVerticesLayers.Outline : SEVerticesLayers.Face;
				rparams._OutlineGradient.ModifyVertices(this, mSEMesh.mLayers[(int)layer], true);
			}

		}

		void AddOutlineLayer()
		{
			var faceVertices    = mSEMesh.mLayers[(int)SEVerticesLayers.Face];
			var outlineVertices = mSEMesh.mLayers[(int)SEVerticesLayers.Outline];

			outlineVertices.CopyFrom(faceVertices);

            if (rparams._OutlineTexture._Enable && rparams._OutlineTexture._Texture)
            {
                ApplyUV(rparams._OutlineTexture, true);
            }


			for (int index = 0; index < outlineVertices.Size; index++)
			{
				// Layer 0: No Face, all outline
				outlineVertices.Buffer[index].byte0 = 255; // surface
				//verts[index].color.a = 0;

				// Layer 1: Face, no outline,  outlineColor = faceColor
				//verts[index + count].byte1 = verts[index + count].byte0; // edge = surface
				faceVertices.Buffer[index].byte2 = faceVertices.Buffer[index].color.r;
				faceVertices.Buffer[index].byte3 = faceVertices.Buffer[index].color.g;
				faceVertices.Buffer[index].byte4 = faceVertices.Buffer[index].color.b;
				faceVertices.Buffer[index].byte5 = 0;
			}
		}

		protected void ApplyUV( SmartEdgeTexture seTexture, bool outline )
		{
			seTexture.InitMapping(mRectPivot, mCharacters.Size);

			var layer = mSEMesh.mLayers[(int) (outline ? SEVerticesLayers.Outline : SEVerticesLayers.Face) ];
            var verts = layer.Buffer;

			for (int index = 0; index < layer.Size; index++)
			{
				#region public Vector2 GetUV_inlined(SmartEdge smartEdge, int index, Vector2 widgetMin, Vector2 widgetMax)
				    var pos = verts[index].position;
				    var charID = verts[index].characterID;

				    if (charID >= seTexture.mIdxNextElement)
				    {
					    #region SEMeshTools.GetEffectRegion(mesh, ref mIdxNextElement, numVertices, regionType, rect, out rMin, out rMax);

					    if (seTexture._Region == eEffectRegion.Element)
					    {
						    seTexture.rMin = mCharacters.Buffer[charID].Min;
						    seTexture.rMax = mCharacters.Buffer[charID].Max;
						    seTexture.mIdxNextElement++;
					    }
					    else
					    if (seTexture._Region == eEffectRegion.AllElements)
					    {
						    seTexture.rMin = mAllCharactersMin;
						    seTexture.rMax = mAllCharactersMax;
						    seTexture.mIdxNextElement = mCharacters.Size;
					    }
					    else
					    {
						    seTexture.rMin = mWidgetRectMin;
						    seTexture.rMax = mWidgetRectMax;
						    seTexture.mIdxNextElement = mCharacters.Size;
					    }
					    #endregion

					    #region SetupUVMapping(rMin, rMax);
					    float e = 1 / (seTexture.rMax.x - seTexture.rMin.x); //invWidth;
					    float f = 1 / (seTexture.rMax.y - seTexture.rMin.y); //invHeight;
					    float t = seTexture.rMin.x;
					    float h = seTexture.rMin.y;
					    float k = seTexture._Tile.x;
					    float d = seTexture._Tile.y;
					    float a = seTexture._Offset.x;
					    float b = seTexture._Offset.y;

					    if (seTexture._Angle < -0.001f || 0.001f < seTexture._Angle)
					    {
						    // Scale(_Tile)*Translate(-Offset)* Translate(Pivot)*Rotation(Angle)*Translate(-Pivot)* Scale(1/whidth, 1/height)*Translate(-min)
						    seTexture.mUVMatrix.m00 = e * seTexture.mCosAngle * k;
						    seTexture.mUVMatrix.m11 = seTexture.mCosAngle * d * f;
						    seTexture.mUVMatrix.m10 = e * d * seTexture.mSinAngle;
						    seTexture.mUVMatrix.m01 = -f * k * seTexture.mSinAngle;

						    seTexture.mUVMatrix.m03 = seTexture.Mat03 - k * e * seTexture.mCosAngle * t - seTexture.mUVMatrix.m01 * h;
						    seTexture.mUVMatrix.m13 = seTexture.Mat13 - d * seTexture.mCosAngle * f * h - seTexture.mUVMatrix.m10 * t;
						    //seTexture.mUVMatrix.m23 = seTexture.mUVMatrix.m20 = seTexture.mUVMatrix.m30 = seTexture.mUVMatrix.m21 = seTexture.mUVMatrix.m31 = seTexture.mUVMatrix.m02 = seTexture.mUVMatrix.m12 = seTexture.mUVMatrix.m32 = 0;
						    //seTexture.mUVMatrix.m22 = seTexture.mUVMatrix.m33 = 1;
					    }
					    else
					    {
						    // Scale(_Tile)*Translate(-Offset)*Scale(1/whidth, 1/height)*Translate(-min)
						    seTexture.mUVMatrix.m00 = k * e;
						    seTexture.mUVMatrix.m11 = d * f;
						    seTexture.mUVMatrix.m03 = -a * k - k * t * e;
						    seTexture.mUVMatrix.m13 = -d * f * h - b * d;
						    seTexture.mUVMatrix.m01 = seTexture.mUVMatrix.m10 = 0; // seTexture.mUVMatrix.m02 = seTexture.mUVMatrix.m12 = seTexture.mUVMatrix.m20 = seTexture.mUVMatrix.m21 = seTexture.mUVMatrix.m22 = seTexture.mUVMatrix.m23 = seTexture.mUVMatrix.m30 = seTexture.mUVMatrix.m31 = seTexture.mUVMatrix.m32 = seTexture.mUVMatrix.m33 = 0;
					    }
					    #endregion
				    }

				    #region GetUV(mesh.Positions[index])
					    v2.x = seTexture.mUVMatrix.m00 * pos.x + seTexture.mUVMatrix.m01 * pos.y + seTexture.mUVMatrix.m03;
					    v2.y = seTexture.mUVMatrix.m10 * pos.x + seTexture.mUVMatrix.m11 * pos.y + seTexture.mUVMatrix.m13;
				    #endregion
				#endregion

				verts[index].float0 = v2.x;
				verts[index].float1 = v2.y;
			}
		}
	}
}