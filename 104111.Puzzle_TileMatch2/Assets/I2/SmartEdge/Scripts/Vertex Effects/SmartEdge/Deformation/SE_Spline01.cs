using UnityEngine;
using System;

namespace I2.SmartEdge
{
    [System.Serializable]
    public class SE_Spline01
    {
        public enum eSplineLerp { Auto, Connected, Broken }

        [Serializable]public struct SplinePoint
        {
            public float time;
            public Vector3 point, tangent0, tangent1;
            public eSplineLerp lerpType;
        }
        public SplinePoint[] _Points = 
        {
            new SplinePoint{ time=0, point = new Vector3(0, 0, 0), lerpType=eSplineLerp.Auto },
            new SplinePoint{ time=1, point = new Vector3(0, 0, 0), lerpType=eSplineLerp.Auto }
        };

        public Vector3 GetPoint(float time)
        {
            int prevIndex = 0, curIndex = 0;
            var nPoints = _Points.Length;
            for (; curIndex < nPoints; ++curIndex)
            {
                float cTime = _Points[curIndex].time;
                if (time == cTime)
                    return _Points[curIndex].point;

                if (cTime > time)
                    break;

                prevIndex = curIndex;
            }

            if (curIndex >= nPoints)
            {
                curIndex = nPoints-1;
                prevIndex = curIndex-1;
            }

            var curTime    = _Points[curIndex].time;
            var curPoint   = _Points[curIndex].point;
            var curTan0    = _Points[curIndex].tangent0;
            //var curTan1    = _Points[curIndex].tangent1;

            var prevTime   = _Points[prevIndex].time;
            var prevPoint  = _Points[prevIndex].point;
            //var prevTan0   = _Points[prevIndex].tangent0;
            var prevTan1   = _Points[prevIndex].tangent1;

            float dt = (time - prevTime) / (curTime - prevTime);
            if (dt <= 0)
                return curPoint - 3*curTan0 * time;
            else
            if (dt >= 1)
                return curPoint - 3*curTan0 * (time - 1);
            else
                return GetBezierPoint(prevPoint, prevTan1, curPoint, curTan0, dt);
        }

        public Vector3 TransformToWorldPoint( Vector3 point, Transform tr, Vector2 mRectMin, Vector3 mRectMax )
        {
            point.x *= mRectMax.x - mRectMin.x;
            point.y *= mRectMax.y - mRectMin.y;
            point.x += mRectMin.x;
            point.y += mRectMin.y;
            return tr.TransformPoint(point);
        }
        public Vector3 TransformToWorldDirection(Vector3 dir, Transform tr, Vector2 mRectMin, Vector3 mRectMax)
        {
            dir.x *= mRectMax.x - mRectMin.x;
            dir.y *= mRectMax.y - mRectMin.y;
            return tr.TransformVector(dir);
            //return tr.TransformDirection(dir);
        }


        public Vector3 TransformFromWorldPoint(Vector3 point, Transform tr, Vector2 mRectMin, Vector3 mRectMax)
        {
            point = tr.InverseTransformPoint(point);
            point.x -= mRectMin.x;
            point.y -= mRectMin.y;
            point.x /= mRectMax.x - mRectMin.x;
            point.y /= mRectMax.y - mRectMin.y;
            return point;
        }
        public Vector3 TransformFromWorldDirection(Vector3 dir, Transform tr, Vector2 mRectMin, Vector3 mRectMax)
        {
            dir = tr.InverseTransformVector(dir);
            //dir = tr.InverseTransformDirection(dir);
            dir.x /= mRectMax.x - mRectMin.x;
            dir.y /= mRectMax.y - mRectMin.y;
            return dir;
        }






        public void Set(int index, Vector3 point, Vector3 tangent0, Vector3 tangent1)
        {
            _Points[index].point = point;

            if (_Points[index].lerpType == eSplineLerp.Connected)
            {
                if (Vector3.SqrMagnitude(tangent0 - _Points[index].tangent0) > Vector3.SqrMagnitude(tangent1 - _Points[index].tangent1))
                    tangent1 = -tangent0;
                else
                    tangent0 = -tangent1;
            }

            _Points[index].tangent0 = tangent0;
            _Points[index].tangent1 = tangent1;

            if (_Points[index].lerpType == eSplineLerp.Auto)
                UpdateAutoTangent(index);
            if (index>0 && _Points[index-1].lerpType == eSplineLerp.Auto)
                UpdateAutoTangent(index - 1);
            if (index < _Points.Length-1 && _Points[index + 1].lerpType == eSplineLerp.Auto)
                UpdateAutoTangent(index + 1);
        }

        void UpdateAutoTangent( int idx )
        {
            var tangent0 = _Points[idx].tangent0;
            var tangent1 = _Points[idx].tangent1;

            int idx1 = idx > 0 ? idx - 1 : 0;
            var idx2 = idx < _Points.Length - 2 ? idx + 1 : _Points.Length - 1;

            Vector3 p0    = _Points[idx1].point;
            Vector3 p1    = _Points[idx2].point;
            Vector3 point = _Points[idx].point;

            tangent0 = (p0 - p1);
            tangent1 = -tangent0;

            float totalDist = tangent0.magnitude;
            _Points[idx].tangent0 = tangent0 * (0.3333f * Mathf.Clamp01(Vector3.Distance(p0, point) / totalDist));
            _Points[idx].tangent1 = tangent1 * (0.3333f * Mathf.Clamp01(Vector3.Distance(p1, point) / totalDist));
        }

        public Vector3 GetBezierPoint(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float t)
        {
            Vector3 c0 = p0 + t0;
            Vector3 c1 = p1 + t1;
            float d = 1f - t;
            return d * d * d * p0 + 3f * d * d * t * c0 + 3f * d * t * t * c1 + t * t * t * p1;
        }

        public Vector3 GetBezierVelocity(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float t)
        {
            if (t0.sqrMagnitude < Mathf.Epsilon && t < 0.0001f)
                t = 0.0001f;
            if (t1.sqrMagnitude < Mathf.Epsilon && t > 0.999f)
                t = 0.999f;

            Vector3 c0 = p0 + t0;
            Vector3 c1 = p1 + t1;
            return (-3f * p0 + 9f * c0 - 9f * c1 + 3f * p1) * t * t
                + (6f * p0 - 12f * c0 + 6f * c1) * t
                    - 3f * p0 + 3f * c0;
        }

        public float GetBezierLength(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, int subdivisions=10)
        {
            var prev = p0;
            float len = 0;
            for (int i=0; i<subdivisions; ++i)
            {
                var newP = GetBezierPoint(p0, t0, p1, t1, (i + 1) / (float)(subdivisions + 1));
                len += Vector3.Distance(prev, newP);
                prev = newP;
            }
            return len + Vector3.Distance(prev, p1);
        }


        public void RelocateKeyframes()
        {
            float[] sectionLen = new float[_Points.Length-1];
            float totalLen = 0;
            for (int i = 0; i < _Points.Length - 1; ++i)
            {
                sectionLen[i] = GetBezierLength(_Points[i].point, _Points[i].tangent1, _Points[i + 1].point, _Points[i + 1].tangent0);
                totalLen += sectionLen[i];
            }

            float len = 0;
            for (int i = 1; i < _Points.Length - 1; ++i)
            {
                len += sectionLen[i-1];
                _Points[i].time = len / totalLen;
            }
        }

        public void FixTangentLengths()
        {
            float[] sectionLen = new float[_Points.Length - 1];
            float totalLen = 0;
            for (int i = 0; i < _Points.Length - 1; ++i)
            {
                sectionLen[i] = GetBezierLength(_Points[i].point, _Points[i].tangent1, _Points[i + 1].point, _Points[i + 1].tangent0);
                totalLen += sectionLen[i];
            }

            _Points[0].tangent1 = _Points[0].tangent1.normalized*sectionLen[0]*0.3333f;

            for (int i = 1; i < _Points.Length; ++i)
            {
                _Points[i].tangent0 = _Points[i].tangent0.normalized * (sectionLen[i - 1]*0.3333f);
                if (i < sectionLen.Length)
                    _Points[i].tangent1 = _Points[i].tangent1.normalized * (sectionLen[i] * 0.3333f);
            }
        }
    }
}