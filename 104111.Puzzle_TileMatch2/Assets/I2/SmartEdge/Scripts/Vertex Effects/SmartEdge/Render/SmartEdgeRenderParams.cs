using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
    [System.Serializable]
	public class SmartEdgeRenderParams
	{
		#region Face
		public Color32 _FaceColor = Color.white;
		[RangeAttribute(0, 1)] public float _EdgeSize = 0.5f;
		[RangeAttribute(0, 6)] public float _Softness = 0.35f;	// Softness between transparent and opaque region (how many pixels for that transition)
        [RangeAttribute(0, 1)] public float _FaceSoftness = 0f;	// extra softness between face and transparent
        [RangeAttribute(0, 1)]
        public float _SDF_Spread = 0.0f;

		public bool _EnableFace = true;

		public bool _UseWidgetTexture = false;
        public bool _UseSuperSampling = false;

		public bool _EnableFaceGradient = false;
		public GradientEffect _FaceGradient = new GradientEffect();
		public SmartEdgeTexture _FaceTexture = new SmartEdgeTexture();

		#endregion

		#region Outline

		public bool _EnableOutline = false;
        public bool _UseOutlineLayer = false;

		[RangeAttribute(0, 1)] public float _OutlineSoftness = 0f;	// extra softness between face and outline

		public Color32 _OutlineColor = Color.black;
		[RangeAttribute(0, 1)] public float _OutlineWidth = 0.1f;

		public enum eOutlinePosition { Outside, Center, Inside };
		public eOutlinePosition _OutlinePosition = eOutlinePosition.Outside;

		public bool _EnableOutlineGradient = false;
		public GradientEffect _OutlineGradient = new GradientEffect();

		public SmartEdgeTexture _OutlineTexture = new SmartEdgeTexture();

		[RangeAttribute(4, 8)] public int _OutlineDivisions = 4;   // number of ouline elements needed to render smooth outlines when not using SDF
		#endregion

		#region Glow

		public bool _EnableGlow = false;

		public Color32 _GlowColor 	= new Color32(255,255,0,255);	// yellow

		[RangeAttribute(0, 1)] public float _GlowOuterWidth = 0.1f;
		[RangeAttribute(0, 1)] public float _GlowInnerWidth = 0.01f;
		[RangeAttribute(0, 1)] public float _GlowPower = 0.25f;
		[RangeAttribute(0, 1)] public float _GlowEdgeWidth = 0f;
		[RangeAttribute(-1,1)] public float _GlowOffset = 0f;
		[RangeAttribute(0,10)] public float _GlowIntensity = 1;
		public Vector3 _GlowPosition = Vector3.zero;

		public bool _EnableGlowGradient = false;
		public GradientEffect _GlowGradient = new GradientEffect();

		public enum eGlowLayer { Simple, Front, Back, TwoLayers }
		public eGlowLayer _GlowLayer = eGlowLayer.Simple;

		public SmartEdgeTexture _GlowTexture = new SmartEdgeTexture();

		#endregion

		#region Shadows

		public bool _EnableShadows = false;
		public bool _SimpleShadows = false;

		public Color32 _ShadowColor 	= new Color32(0,0,0,128);
		public Vector3 _ShadowOffset = new Vector3(0.05f,-0.05f,0);

		[RangeAttribute(-1, 1)] public float _ShadowEdgeOffset = 0.0f;
		[RangeAttribute(0, 1)] public float _ShadowSmoothWidth = 0.2f;
		public bool _ShadowHollow = false;
		[RangeAttribute(0, 1)] public float _ShadowEdgeWidth = 0.0f;
		[RangeAttribute(0, 1)] public float _ShadowInnerSmoothWidth = 0.2f;
		[RangeAttribute(0, 1)] public float _ShadowSmoothPower = 1f;

		[RangeAttribute(-2, 1)] public float _ShadowHeight = 0f;
		[RangeAttribute(-1, 1)] public float _ShadowBorderLeft = 0f;

		[RangeAttribute(-1, 1)] public float _ShadowBorderRight = 0f;
		[RangeAttribute(0, 1)] public float _ShadowSmoothTop = 1f;
		[RangeAttribute(0, 1)] public float _ShadowSmoothBottom = 1f;

		[RangeAttribute(0, 1)] public float _ShadowOpacityTop = 1f;
		[RangeAttribute(0, 1)] public float _ShadowOpacityBottom = 1f;
		[RangeAttribute(4, 8)] public int _ShadowSmoothingDivisions = 4;   // number of shadow elements needed for the soft shadows when not using SDF
		public int _ShadowSubdivisionLevel = 0;

		#endregion

		#region InnerShadows

		public bool _EnableInnerShadows = false;

		public Color32 _InnerShadowColor 	= new Color32(0,0,0,128);
		public Vector2 _InnerShadowOffset = new Vector2(-0.16f,0.16f);

		[RangeAttribute(-1, 1)] public float _InnerShadowEdgeOffset = 0.096f;
		[RangeAttribute(-1, 1)] public float _InnerShadowMaskOffset = 0.0f;
		[RangeAttribute(0, 1)] public float _InnerShadowSmoothWidth = 0.2f;
		[RangeAttribute(0, 1)] public float _InnerShadowMaskSoftness = 1/20f;	// softness between face and outline


		#endregion

		#region Environment Reflections

		public bool _EnableReflection = false;

		public Color _ReflectionColor_Face    = Color.white;
		public Color _ReflectionColor_Outline = Color.white;
        public Vector3 _ReflectionRotation    = Vector3.zero;
        public float _ReflectionFresnel_Scale = 0.5f;
        [RangeAttribute(0, 1)] public float _ReflectionFresnel_Bias = 0;
        [RangeAttribute(0, 20)]public float _ReflectionIntensity = 1f;
        [RangeAttribute(0, 1)] public float _ReflectionGlass = 1f;

        public Cubemap _ReflectionMap;

		#endregion

		#region Floor Reflection

		public bool _EnableFloorReflection = false;

		[RangeAttribute(0, 1)] public float _FloorReflectionOpacity = 0.7f;

		[RangeAttribute(-2, 2)] public float _FloorReflectionPlane = 0;
		[RangeAttribute(0, 1)] public float _FloorReflectionDistance = 0.5f;
		[RangeAttribute(0, 1)] public float _FloorReflectionFloor = 0.0f;
        public bool _FloorReflection_EnableFloorClamp = false;
        public bool _FloorReflection_Back = true;

		public Color32 _FloorReflectionTint_Color = Color.white;
		public eColorBlend _FloorReflectionTint_BlendMode = eColorBlend.Multiply;
		[RangeAttribute(0,1)] public float _FloorReflectionTint_Opacity = 1;

		public eEffectRegion _FloorReflection_Region = eEffectRegion.Element;

		#endregion

		#region Bevel
		public enum BevelType{ Fast, Full };
		public bool _EnableBevel = false;

		[RangeAttribute(-1, 1)] public float _BevelOffset = 0;
		[RangeAttribute(0, 1)]  public float _BevelWidth = 0.2f;
		[RangeAttribute(0, 1)]  public float _BevelInnerSoftness = 0.5f;
		[RangeAttribute(0, 1)]  public float _BevelOuterSoftness = 0.5f;
		[RangeAttribute(-1, 1)] public float _BevelDepth = 1;
        [RangeAttribute(0, 1)]  public float _BevelCurve = 1;
        [RangeAttribute(0, 1)]  public float _BevelClamp = 0;

        #endregion

        #region Lighting

        [RangeAttribute(0, 1)]  public float _LightDiffuseShadow = 0.7f;
		[RangeAttribute(0, 1)]  public float _LightDiffuseHighlight = 0.7f;
		[RangeAttribute(0, 1)] 	public float _LightSpecularPower = 0.05f;
        [RangeAttribute(0, 1)]  public float _LightSpecularStart = 0f;
        [RangeAttribute(0, 1)]  public float _LightSpecularSoftness = 0.3f;
        public Color _LightSpecularColor = Color.white;

		[RangeAttribute(0, 360)]public float _LightAngle = 180+45;
		[RangeAttribute(0, 90)] public float _LightAltitude = 45;

		#endregion

		#region NormalMap

		public SmartEdgeTexture _NormalMap = new SmartEdgeTexture();
		[RangeAttribute(0, 1)] 	public float _NormalMapStrength_Face = 1/5f;
        [RangeAttribute(0, 1)]  public float _NormalMapStrength_Outline = 1 / 5f;
        [RangeAttribute(0, 1)]  public float _NormalMapDepth = 1;

        #endregion

        [NonSerialized] public Material mMaterial; // Cached here in case some script overrides the Material property
        [NonSerialized] public MaterialDef_SDF _MaterialDef = MaterialDef_SDF.CreateFromPool();

        // transparent - outline - surface - opaque
        public float GetEdge_Surface( float bold )
		{
            float edge = _EdgeSize -bold;
            if (edge < 0) edge = 0;

			if (_EnableOutline)
			{
				float width = 0;
				if (_OutlinePosition == eOutlinePosition.Inside)
					width = 1-edge;
				else 
				if (_OutlinePosition == eOutlinePosition.Center)
					width = (edge > 0.5f ? 1 - edge : edge);
				
				edge += width * _OutlineWidth;
			}
			return edge;
		}

		public float GetEdge_Outline(float bold)
		{
            float edge = _EdgeSize - bold;
            if (edge < 0) edge = 0;

            if (_EnableOutline)
			{
				float width = 0;
				if (_OutlinePosition == eOutlinePosition.Outside)
					width = edge;
				else 
				if (_OutlinePosition == eOutlinePosition.Center)
					width = (edge > 0.5f ? 1 - edge : edge);

				edge -= width * _OutlineWidth;
			}
			return edge;
		}

		public Vector3 GetLightDir()
		{
			var lightDir = MathUtils.v3zero;

			float CosPitch = Mathf.Cos (_LightAltitude * Mathf.Deg2Rad);
			lightDir.x = (Mathf.Cos (-_LightAngle * Mathf.Deg2Rad)* (CosPitch));
			lightDir.y = (Mathf.Sin (-_LightAngle * Mathf.Deg2Rad)* (CosPitch));
			lightDir.z = 1-CosPitch;

			return lightDir.normalized;
		}

		public bool HasLighting()
		{
			return _EnableBevel || _NormalMap.HasTexture();
		}

		public bool HasGlow()
		{
			return (_EnableGlow && _GlowIntensity>0 &&
					(_GlowInnerWidth>0 || _GlowOuterWidth>0 || _GlowEdgeWidth>0) && 
					_GlowColor.a>0);
		}

		public bool HasShadow()
		{
			if (!_EnableShadows)
				return false;
			if (_ShadowColor.a == 0)
				return false;

			if (_ShadowOffset.sqrMagnitude > 0)
				return true;
			if (_ShadowSmoothWidth > 0 && (_ShadowSmoothTop > 0 || _ShadowSmoothBottom > 0))
				return true;
			if (_ShadowEdgeOffset != 0)
				return true;
			if (_ShadowBorderLeft != 0 || _ShadowBorderRight != 0 || _ShadowHeight!=0)
				return true;
			return false;
		}

		public bool HasInnerShadow()
		{
			if (!_EnableInnerShadows)
				return false;
			if (_InnerShadowColor.a == 0)
				return false;
			return true;
		}

        public bool UsesShader_GlowMode()
        {
            return (HasGlow() && _GlowLayer != eGlowLayer.Simple) || 
                   (HasShadow() && !_SimpleShadows);
        }


        public void UpdateMaterial(Material baseMaterial, Texture2D mainTexture, Shader shader, SmartEdge se )
        {
            if (mainTexture == null || se == null)
                return;

            if (mMaterial == null || (baseMaterial != null && mMaterial.mainTexture != mainTexture))
            {
                ReleaseMaterial();
                mMaterial = baseMaterial;
            }

            _MaterialDef.Flags = 0;
            if (se.mSpread < 0)
                se.UpdateSpread();



            Texture texFace = _EnableFace ? _FaceTexture.GetTexture() : null;
            Texture texOutline = _EnableOutline ? _OutlineTexture.GetTexture() : null;
            Texture texGlow = _EnableGlow ? _GlowTexture.GetTexture() : null;
            Texture texNormalMap = _NormalMap.GetTexture();
            Texture texEnvironment = _EnableReflection ? _ReflectionMap : null;
            _MaterialDef.SetDefaultValues();
            _MaterialDef.SetTextures(mainTexture, texFace, texOutline, texNormalMap, texGlow, texEnvironment);

            _MaterialDef.shader = shader;

            _MaterialDef.SetFlags(_UseWidgetTexture, _UseSuperSampling, UsesShader_GlowMode(), _EnableOutline, se.mSETextureFormat);
            _MaterialDef.SetSDF( _Softness, _FaceSoftness, _OutlineSoftness, 0.5f / se.mSpread);
            //_MaterialDef.SetSDF(se.mSpread * 2 / (float)mainTexture.width, _Softness, _OutlineSoftness, 0.5f / se.mSpread);
            
            if (_EnableBevel)
                _MaterialDef.SetBevel(_BevelOffset, _BevelWidth, _BevelDepth, _BevelCurve, _BevelClamp, _BevelInnerSoftness, _BevelOuterSoftness);

            if (_NormalMap._Enable)
                _MaterialDef.SetBumpData(_NormalMapStrength_Face, _NormalMapStrength_Outline, _NormalMapDepth);


            if (_EnableReflection)
                _MaterialDef.SetEnvReflection(_ReflectionColor_Face, _ReflectionColor_Outline, _ReflectionRotation.x, _ReflectionRotation.y, _ReflectionRotation.z, _ReflectionFresnel_Bias, _ReflectionFresnel_Scale, _ReflectionIntensity* se.mWidgetColor.a, _ReflectionGlass);

            if (_EnableBevel || _EnableReflection || _NormalMap._Enable)
                _MaterialDef.SetLighting(_LightDiffuseHighlight, _LightDiffuseShadow, _LightSpecularStart, _LightSpecularSoftness, _LightSpecularPower, _LightSpecularColor, GetLightDir());

            if (_EnableGlow && _GlowLayer == SmartEdgeRenderParams.eGlowLayer.Simple)
                _MaterialDef.SetGlow((_GlowColor*se.mWidgetColor) * _GlowIntensity, _GlowInnerWidth, _GlowEdgeWidth, _GlowOuterWidth, _GlowPower, _GlowOffset, GetEdge_Outline(0));

            if (HasShadow() && _SimpleShadows)
                _MaterialDef.SetShadow(_ShadowColor * se.mWidgetColor, -_ShadowOffset.x, _ShadowOffset.y, _ShadowSmoothWidth, _ShadowEdgeOffset);

            if (HasInnerShadow())
                _MaterialDef.SetInnerShadow(_InnerShadowColor * se.mWidgetColor, _InnerShadowOffset.x, _InnerShadowOffset.y, _InnerShadowSmoothWidth, _InnerShadowEdgeOffset, _InnerShadowMaskOffset, _InnerShadowMaskSoftness);


            mMaterial = MaterialCache.GetMaterial(mMaterial, _MaterialDef);
            //MaterialCache.pInstance.Print();
        }

        public void ReleaseMaterial()
        {
            if (mMaterial != null)
            {
                MaterialCache.pInstance.ReleaseMaterial(mMaterial);
                mMaterial = null;
            }
        }

        public SmartEdgeRenderParams Clone()
        {
            return this.MemberwiseClone() as SmartEdgeRenderParams;
        }
	}
}
