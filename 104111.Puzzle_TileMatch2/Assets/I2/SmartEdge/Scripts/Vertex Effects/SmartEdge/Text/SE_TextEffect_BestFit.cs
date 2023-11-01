using UnityEngine;
using System;

namespace I2.SmartEdge
{
    [System.Serializable]
    public class SE_TextEffect_BestFit
    {
        public bool _Enabled = false;

        public RectTransform _TargetRect;
        public bool _FitWidth = true;
        public bool _FitHeight = true;
        public bool _KeepAspectRatio = true;

        public RectOffset _Border = new RectOffset();
        public Vector2 _Scale = Vector2.one;
        public float _Snapping = 0; // Snap size to this amount (if more than 0)


        public void ModifyVertices(SEMesh mesh, int firstLayer, SmartEdge se)
        {
            if (!_Enabled || SmartEdge.mCharacters.Size==0 || (!_FitWidth && !_FitHeight))
                return;
            
            var rect = se.mRect;
            if (_TargetRect != null)
            {
                Vector2 min = _TargetRect.TransformPoint(_TargetRect.rect.min);
                Vector2 max = _TargetRect.TransformPoint(_TargetRect.rect.max);
                var tr = se.transform;
                min = tr.InverseTransformPoint(min);
                max = tr.InverseTransformPoint(max);
                rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }

            float dx = se.mAllCharactersMax.x - se.mAllCharactersMin.x;
            float dy = se.mAllCharactersMax.y - se.mAllCharactersMin.y;

            Rect finalRect = Rect.MinMaxRect(se.mAllCharactersMin.x, se.mAllCharactersMin.y, se.mAllCharactersMax.x, se.mAllCharactersMax.y);

            //--[ Fitting ]------------------------------------------------------------

            if (_FitWidth)
            {
                finalRect.xMin = rect.xMin + _Border.left;
                finalRect.xMax = rect.xMax - _Border.right;
            }
            if (_FitHeight)
            {
                finalRect.yMin = rect.yMin + _Border.bottom;
                finalRect.yMax = rect.yMax - _Border.top;
            }


            //--[ Aspect Ratio ]------------------------------------------------------------

            if (_KeepAspectRatio )
            {
                float scaleW = finalRect.width / dx;
                float scaleH = finalRect.height / dy;
                if (scaleW < 0) scaleW = 0;
                if (scaleH < 0) scaleH = 0;
                if (!_FitWidth) scaleW = float.MaxValue;
                if (!_FitHeight) scaleH = float.MaxValue;

                if (scaleW < scaleH)
                {
                    finalRect.yMax = rect.yMax - _Border.top;
                    finalRect.yMin = finalRect.yMax - dy * scaleW;
                }
                else
                {
                    finalRect.width = dx * scaleH;
                }
            }


            //--[ Extra Scale ]------------------------------------------------------------

            finalRect.width *= _Scale.x;
            finalRect.yMin = finalRect.yMax - finalRect.height * _Scale.y;

            //--[ Snapping ]------------------------------------------------------------

            if (_Snapping > 0)
            {
                if (_FitWidth)  finalRect.width  = Mathf.Floor(finalRect.width / _Snapping) * _Snapping;
                if (_FitHeight) finalRect.height = Mathf.Floor(finalRect.height / _Snapping) * _Snapping;
            }


            //--[ Alignment ]------------------------------------------------------------

            float w = finalRect.width;
            switch (se.GetHorizontalAlignment())
            {
                case SmartEdge.eHorizontalAlignment.Left:   finalRect.xMin = rect.xMin + _Border.left; break;
                case SmartEdge.eHorizontalAlignment.Right:  finalRect.xMin = rect.xMax - w - _Border.right; break;
                default:                                    finalRect.xMin = (rect.xMax + rect.xMin - w) / 2.0f; break;
            }
            finalRect.xMax = finalRect.xMin + w;

            float h = finalRect.height;
            switch (se.GetVerticalAlignment())
            {
                case SmartEdge.eVerticalAlignment.Bottom:   finalRect.yMin = rect.yMin + _Border.bottom; break;
                case SmartEdge.eVerticalAlignment.Top:      finalRect.yMin = rect.yMax - h - _Border.top; break;
                default:                                    finalRect.yMin = (rect.yMax + rect.yMin - h) / 2.0f; break;
            }
            finalRect.yMax = finalRect.yMin+h;


            //--[ Apply Result ]------------------------------------------------------------

            Vector2 finalMin = new Vector2(finalRect.xMin, finalRect.yMin);
            Vector2 finalSize = new Vector2(finalRect.width, finalRect.height);

            for (int iLayer = firstLayer; iLayer < mesh.numLayers; ++iLayer)
            {
                ApplyBestFitResult(se, mesh.mLayers[iLayer], finalMin, finalSize, dx, dy);
            }
		}

        private void ApplyBestFitResult(SmartEdge se, ArrayBufferSEVertex mesh, Vector2 finalMin, Vector2 finalSize, float dx, float dy)
        {
            for (int i = 0; i < mesh.Size; ++i)
            {
                var point = mesh.Buffer[i].position;

                var pctX = (point.x - se.mAllCharactersMin.x) / dx;
                var pctY = (point.y - se.mAllCharactersMin.y) / dy;

                point.x = finalMin.x + finalSize.x * pctX;
                point.y = finalMin.y + finalSize.y * pctY;

                mesh.Buffer[i].position = point;
            }
        }
    }
}