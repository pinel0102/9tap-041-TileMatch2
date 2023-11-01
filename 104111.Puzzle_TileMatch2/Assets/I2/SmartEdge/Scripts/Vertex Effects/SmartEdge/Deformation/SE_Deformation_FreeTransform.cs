using UnityEngine;
using System;

namespace I2.SmartEdge
{
 	[System.Serializable]
	public class SE_Deformation_FreeTransform
	{
        public bool _Enabled = false;
        public SE_Spline01 _Curve_Top    = new SE_Spline01()
        {
            _Points = new SE_Spline01.SplinePoint[]{
                new SE_Spline01.SplinePoint{ time=0, point = new Vector3(0, 1, 0), tangent0=Vector3.left/3f, tangent1=Vector3.right/3f, lerpType=SE_Spline01.eSplineLerp.Auto },
                new SE_Spline01.SplinePoint{ time=1, point = new Vector3(1, 1, 0), tangent0=Vector3.left/3f, tangent1=Vector3.right/3f, lerpType=SE_Spline01.eSplineLerp.Auto }
            }
        };
        public SE_Spline01 _Curve_Bottom = new SE_Spline01()
        {
            _Points = new SE_Spline01.SplinePoint[]{
                new SE_Spline01.SplinePoint{ time=0, point = new Vector3(0, 0, 0), tangent0=Vector3.left/3f, tangent1=Vector3.right/3f, lerpType=SE_Spline01.eSplineLerp.Auto },
                new SE_Spline01.SplinePoint{ time=1, point = new Vector3(1, 0, 0), tangent0=Vector3.left/3f, tangent1=Vector3.right/3f, lerpType=SE_Spline01.eSplineLerp.Auto }
            }
        };

        public SE_Spline01 _Curve_Left   = new SE_Spline01()
        {
            _Points = new SE_Spline01.SplinePoint[]{
                new SE_Spline01.SplinePoint{ time=0, point = new Vector3(0, 0, 0), tangent0=Vector3.down/3f, tangent1=Vector3.up/3f, lerpType=SE_Spline01.eSplineLerp.Auto },
                new SE_Spline01.SplinePoint{ time=1, point = new Vector3(0, 1, 0), tangent0=Vector3.down/3f, tangent1=Vector3.up/3f, lerpType=SE_Spline01.eSplineLerp.Auto }
            }
        };

        public SE_Spline01 _Curve_Right  = new SE_Spline01()
        {
            _Points = new SE_Spline01.SplinePoint[]{
                new SE_Spline01.SplinePoint{ time=0, point = new Vector3(1, 0, 0), tangent0=Vector3.down/3f, tangent1=Vector3.up/3f, lerpType=SE_Spline01.eSplineLerp.Auto },
                new SE_Spline01.SplinePoint{ time=1, point = new Vector3(1, 1, 0), tangent0=Vector3.down/3f, tangent1=Vector3.up/3f, lerpType=SE_Spline01.eSplineLerp.Auto }
            }
        };


        public int _SubdivisionLevel = 0;
        public float _Strength = 1;

        public bool _LinkCurve_TopDown, _LinkCurve_LeftRight;

        // This are used in the Editor to control the curves-------------------------------
        public enum eTransformType { TopDown, LeftRight, Wave, Custom };
        public eTransformType _TransformType = eTransformType.Custom;

        public enum eEasyType { Linear, EasyInOut, EasyIn, EasyOut };
        public eEasyType _EasyType = eEasyType.EasyInOut;

        public bool _SliderIsCenter = true;
        public float _StartSlider, _EndSlider;
        public bool _LinkSliders = true;
        public bool _InvertEndSlider = true;

        public float _WaveRangeMin = 0, _WaveRangeMax = 1, _WaveRangeSize = 10;
        //----------------------------------------------------------------------------------


        public void ModifyVertices (SEMesh mesh, int firstLayer, SmartEdge se)
		{
            if (SmartEdge.mCharacters.Size == 0)
                return;

            for (int iLayer = firstLayer; iLayer < mesh.numLayers; ++iLayer)
            {
                var layer = mesh.mLayers[iLayer];

                if (_SubdivisionLevel > 0)
                    SEMeshTools.Subdivide(layer, 0, _SubdivisionLevel);

                Vector2 pct;
                float minX = se.mRect.xMin, minY = se.mRect.yMin;
                float w = se.mRect.width, h = se.mRect.height;

                for (int i = 0; i < layer.Size; ++i)
                {
                    var point = layer.Buffer[i].position;
                    pct.x = (point.x - minX) / w;
                    pct.y = (point.y - minY) / h;

                    point = GetBezierSurfacePoint(pct);

                    point.x *= w;
                    point.y *= h;
                    point.x += minX;
                    point.y += minY;

                    layer.Buffer[i].position = point;
                }
            }
		}

        public Vector3 GetBezierSurfacePoint( Vector2 pct )
        {
            Vector3 point_Bottom = _Curve_Bottom.GetPoint(pct.x);
            Vector3 point_Top    = _LinkCurve_TopDown ? (point_Bottom + Vector3.up) : _Curve_Top.GetPoint(pct.x);
            Vector3 point_Left   = _Curve_Left.GetPoint(pct.y);
            Vector3 point_Right  = _LinkCurve_LeftRight ? (point_Left+Vector3.right) : _Curve_Right.GetPoint(pct.y);

            Vector3 Left0 = _Curve_Left._Points[0].point;
            Vector3 Left1 = _Curve_Left._Points[_Curve_Left._Points.Length-1].point;
            Vector3 Right0 = _Curve_Right._Points[0].point;
            Vector3 Right1 = _Curve_Right._Points[_Curve_Right._Points.Length - 1].point;

            var offset_Left = point_Left - Vector3.Lerp(Left0, Left1, pct.y);
            var offset_Right = point_Right - Vector3.Lerp(Right0, Right1, pct.y);


            var point = Vector3.Lerp(point_Bottom, point_Top, pct.y);
            point += Vector3.Lerp(offset_Left, offset_Right, pct.x);
            return point;
        }
    }
}