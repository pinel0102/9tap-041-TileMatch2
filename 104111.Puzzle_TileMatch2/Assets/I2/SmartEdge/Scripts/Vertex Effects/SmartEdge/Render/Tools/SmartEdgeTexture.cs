using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	[Serializable]
	public class SmartEdgeTexture
	{
		public enum eMappingType { Region, Custom }

		public bool _Enable = false;
		public Texture _Texture;

		public eMappingType _MappingType = eMappingType.Region;
		public eEffectRegion _Region = eEffectRegion.Element;
		public RectTransform _CustomRegion;

		public Vector2 _Tile = Vector2.one;
		public Vector2 _Offset = Vector2.zero;
		public Vector2 _Pivot = Vector2.one*0.5f;
		[RangeAttribute(0, 360)]public float   _Angle = 0;

		[NonSerialized] public Matrix4x4 mUVMatrix = Matrix4x4.identity;

        [NonSerialized] public int mIdxNextElement = 0;
        [NonSerialized] public Vector2 rMin, rMax;
        [NonSerialized] public float mSinAngle, mCosAngle, Mat03, Mat13;   // Cached values for computing the UVMatrix


        public eEffectRegion GetRegionType()
		{
			return _Region;
		}
		public Texture GetTexture() { return _Enable?_Texture:null; }



        public void InitMapping( Vector2 rectPivot, int numCharacters )
        {
            if (GetCustomRegion())
            {
                mIdxNextElement = numCharacters;
                SetupUVMapping(rMin, rMax, rectPivot);
            }
            else
                mIdxNextElement = 0;

            // Cache values for computing the UVMatrix
            mSinAngle = Mathf.Sin(Mathf.Deg2Rad * _Angle);
            mCosAngle = Mathf.Cos(Mathf.Deg2Rad * _Angle);
            float x = _Pivot.x;
            float y = _Pivot.y;
            float k = _Tile.x;
            float d = _Tile.y;
            float a = _Offset.x;
            float b = _Offset.y;
            Mat03 = -a*k - k * mCosAngle * x + k * mSinAngle * y + k * x;
            Mat13 = -b*d - d * mCosAngle * y - d * mSinAngle * x + d * y;
        }

        #region UIVertex

        public float GetPackedUV(List<UIVertex> verts, int index, int numVertices, Rect rect, Vector2 rectPivot)
        {
            return BytePacking.PackUV( GetUV(verts, index, numVertices, rect, rectPivot) );
        }

        public Vector2 GetUV(List<UIVertex> verts, int index, int numVertices, Rect rect, Vector2 rectPivot)
        {
            if (index >= mIdxNextElement)
            {
                VertexTools.GetEffectRegion(verts, ref mIdxNextElement, numVertices, GetRegionType(), rect);
                SetupUVMapping(rMin, rMax, rectPivot);
            }

            return GetUV( verts[index].position );
        }

        #endregion

        #region SE_Mesh

         // This is the same as GetUV(....) but with all methods inside inlined
        public Vector2 GetUV_inlined( SmartEdge se, ArrayBufferSEVertex vertices, int index, Vector2 widgetMin, Vector2 widgetMax)
        {
            var pos    = vertices.Buffer[index].position;
            var charID = vertices.Buffer[index].characterID;

            if (charID >= mIdxNextElement)
            {
                #region SEMeshTools.GetEffectRegion(mesh, ref mIdxNextElement, numVertices, regionType, rect, out rMin, out rMax);

                if (_Region == eEffectRegion.Element)
                {
                    rMin = SmartEdge.mCharacters.Buffer[charID].Min;
                    rMax = SmartEdge.mCharacters.Buffer[charID].Max;
                    mIdxNextElement++;
                }
                else
                if (_Region == eEffectRegion.AllElements)
                {
                    rMin = se.mAllCharactersMin;
                    rMax = se.mAllCharactersMax;
                    mIdxNextElement = SmartEdge.mCharacters.Size;
                }
                else
                {
                    rMin = widgetMin;
                    rMax = widgetMax;
                    mIdxNextElement = SmartEdge.mCharacters.Size;
                }
                #endregion

                #region SetupUVMapping(rMin, rMax);
                    float e = 1 / (rMax.x - rMin.x); //invWidth;
                    float f = 1 / (rMax.y - rMin.y); //invHeight;
                    float t = rMin.x;
                    float h = rMin.y;
                    float k = _Tile.x;
                    float d = _Tile.y;
                    float a = _Offset.x;
                    float b = _Offset.y;

                    if (_Angle < -0.001f || 0.001f < _Angle)
                    {
                        // Scale(_Tile)*Translate(-Offset)* Translate(Pivot)*Rotation(Angle)*Translate(-Pivot)* Scale(1/whidth, 1/height)*Translate(-min)
                        mUVMatrix.m00 = e * mCosAngle * k;
                        mUVMatrix.m11 = mCosAngle * d * f;
                        mUVMatrix.m10 = e * d * mSinAngle;
                        mUVMatrix.m01 = -f * k * mSinAngle;

                        mUVMatrix.m03 = Mat03 - k * e * mCosAngle * t - mUVMatrix.m01 * h;
                        mUVMatrix.m13 = Mat13 - d * mCosAngle * f * h - mUVMatrix.m10 * t;
                        //mUVMatrix.m23 = mUVMatrix.m20 = mUVMatrix.m30 = mUVMatrix.m21 = mUVMatrix.m31 = mUVMatrix.m02 = mUVMatrix.m12 = mUVMatrix.m32 = 0;
                        //mUVMatrix.m22 = mUVMatrix.m33 = 1;
                    }
                    else
                    {
                        // Scale(_Tile)*Translate(-Offset)*Scale(1/whidth, 1/height)*Translate(-min)
                        mUVMatrix.m00 = k * e;
                        mUVMatrix.m11 = d * f;
                        mUVMatrix.m03 = -a * k - k * t * e;
                        mUVMatrix.m13 = -d * f * h - b * d;
                        mUVMatrix.m01 = mUVMatrix.m10 = 0; // mUVMatrix.m02 = mUVMatrix.m12 = mUVMatrix.m20 = mUVMatrix.m21 = mUVMatrix.m22 = mUVMatrix.m23 = mUVMatrix.m30 = mUVMatrix.m31 = mUVMatrix.m32 = mUVMatrix.m33 = 0;
                    }
                #endregion
            }

            #region GetUV(mesh.Positions[index])
                MathUtils.tempV2.x = mUVMatrix.m00 * pos.x + mUVMatrix.m01 * pos.y + mUVMatrix.m03;
                MathUtils.tempV2.y = mUVMatrix.m10 * pos.x + mUVMatrix.m11 * pos.y + mUVMatrix.m13;
                return MathUtils.tempV2;
             #endregion
        }



        //         public Vector2 GetUV(SE_Mesh mesh, int index, int numVertices, Vector2 rectMin, Vector2 rectMax, Vector2 rectPivot)
        //         {
        //             if (index >= mIdxNextElement)
        //             {
        //                 var regionType = _MappingType == eMappingType.Border ?  eEffectRegion.Element:_Region;
        //                 SEMeshTools.GetElementRect(mesh, ref mIdxNextElement, numVertices, regionType, rectMin, rectMax, out rMin, out rMax);
        //                 SetupUVMapping(rMin, rMax, rectPivot);
        //             }
        // 
        //             return GetUV(mesh.Positions[index]);
        //         }
        // 
        #endregion
         
        public void SetupUVMapping( Vector2 rMin, Vector2 rMax, Vector2 rectPivot )
		{
            float e = 1 / (rMax.x - rMin.x); //invWidth;
            float f = 1 / (rMax.y - rMin.y); //invHeight;
            float t = rMin.x;
            float h = rMin.y;
            float k = _Tile.x;
            float d = _Tile.y;
            float a = _Offset.x;
            float b = _Offset.y;

            if (_Angle < -0.001f || 0.001f < _Angle)
            {
                // Scale(_Tile)*Translate(-Offset)* Translate(Pivot)*Rotation(Angle)*Translate(-Pivot)* Scale(1/whidth, 1/height)*Translate(-min)
                mUVMatrix.m00 = e*mCosAngle*k;
                mUVMatrix.m11 = mCosAngle * d * f;
                mUVMatrix.m10 = e*d*mSinAngle;
                mUVMatrix.m01 = -f*k*mSinAngle;

                mUVMatrix.m03 = Mat03 -k*e*mCosAngle*t - mUVMatrix.m01*h;
                mUVMatrix.m13 = Mat13 -d * mCosAngle * f * h - mUVMatrix.m10 * t;
                //mUVMatrix.m23 = mUVMatrix.m20 = mUVMatrix.m30 = mUVMatrix.m21 = mUVMatrix.m31 = mUVMatrix.m02 = mUVMatrix.m12 = mUVMatrix.m32 = 0;
                //mUVMatrix.m22 = mUVMatrix.m33 = 1;
            }
            else
            {
                // Scale(_Tile)*Translate(-Offset)*Scale(1/whidth, 1/height)*Translate(-min)
                mUVMatrix.m00 = k*e;
                mUVMatrix.m11 = d*f;
                mUVMatrix.m03 = -a * k - k*t*e;
                mUVMatrix.m13 = -d*f*h - b*d;
                mUVMatrix.m01 = mUVMatrix.m10 = 0; // mUVMatrix.m02 = mUVMatrix.m12 = mUVMatrix.m20 = mUVMatrix.m21 = mUVMatrix.m22 = mUVMatrix.m23 = mUVMatrix.m30 = mUVMatrix.m31 = mUVMatrix.m32 = mUVMatrix.m33 = 0;
            }
        }

        public bool GetCustomRegion()
		{
            // TODO: instead of returning Min/Max retornar la matrix del transform
			if (_MappingType==eMappingType.Custom && _CustomRegion!=null)
			{
                //Vector2 offset = _CustomRegion.localPosition;

                rMin = _CustomRegion.rect.min /*+ offset*/;
                rMax = _CustomRegion.rect.max /*+ offset*/;
				return true;
			}
            rMin = rMax = MathUtils.v2zero;
			return false;
		}

		public bool HasTexture() { return _Enable && _Texture!=null; }

		public Vector2 GetUV( Vector3 position )
		{
            MathUtils.tempV2.x = mUVMatrix.m00*position.x + mUVMatrix.m01*position.y + mUVMatrix.m03;
            MathUtils.tempV2.y = mUVMatrix.m10*position.x + mUVMatrix.m11*position.y + mUVMatrix.m13;
            return MathUtils.tempV2;
		}

		public static bool IsPointInClockwiseTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
		{
			var s = (p0.y*p2.x - p0.x*p2.y + (p2.y - p0.y)*p.x + (p0.x - p2.x)*p.y);
			var t = (p0.x*p1.y - p0.y*p1.x + (p0.y - p1.y)*p.x + (p1.x - p0.x)*p.y);
			
			if (s <= 0 || t <= 0)
				return false;
			
			var A = (-p1.y*p2.x + p0.y*(-p1.x + p2.x) + p0.x*(p1.y - p2.y) + p1.x*p2.y);
			
			return (s + t) < A;
		}
	}
}
