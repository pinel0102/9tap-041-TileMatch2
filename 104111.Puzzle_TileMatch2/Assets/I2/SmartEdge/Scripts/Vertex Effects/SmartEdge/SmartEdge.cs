using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;


namespace I2.SmartEdge
{
    [ExecuteInEditMode]
    [AddComponentMenu("I2/Smart Edge/Smart Edge", 1)]
    [Serializable]
    public partial class SmartEdge : MonoBehaviour//UnityEngine.EventSystems.UIBehaviour
    {
        [NonSerialized] public static SEMesh mSEMesh = new SEMesh();
        [NonSerialized] public static ArrayBufferSEVertex mOriginalVertices;       // This its a direct access to mSEMesh.mLayers[SEVerticesLayers.Original]

        [NonSerialized] public static ArrayBuffer<SE_Character> mCharacters = new ArrayBuffer<SE_Character>();

        [NonSerialized] public Vector2 mAllCharactersMin, mAllCharactersMax;    // Bounding Rect of all vertices in mCharacters
        public static SEVertex seVertex = new SEVertex();



        [NonSerialized]public Rect mRect;
		[NonSerialized]public Vector2 mRectPivot;
		[NonSerialized]public Color mWidgetColor;
        [NonSerialized]public Vector2 mWidgetRectMin, mWidgetRectMax; // Cached from mRect
        [NonSerialized]public float mCharacterSize = 1;   // When using a text, it is the Font Size, otherwise the Image Height
        [NonSerialized]public float mLineHeight=1;        // When using a text, it is the Font LineHeight, otherwise the Image Height

        [NonSerialized]public bool mIsDirty_Material = true/*,  mIsDirty_Vertices = true*/;

        [NonSerialized]public Texture cache_MainTexture;

        [NonSerialized]public float mSpread = -1;



		public enum SETextureFormat{ Unknown, SDF, MSDF, MSDFA };
		public SETextureFormat mSETextureFormat = SETextureFormat.SDF;

        public DeformationEffect _Deformation   = new DeformationEffect();
        public SE_TextEffect     _TextEffect    = new SE_TextEffect();



        private static int matParam_Spread = -1;


        public void Setup( Texture2D mainTexture )
		{
			if (mainTexture == cache_MainTexture)
				return;
			cache_MainTexture = mainTexture;

			string textureName = mainTexture ? mainTexture.name : string.Empty;
			var hasAlpha = false;
			if (mainTexture)
			{
				if (mainTexture.format==TextureFormat.ARGB32 ||
					mainTexture.format==TextureFormat.RGBA32)
					hasAlpha = true;
			}

            //--[ Update TextureFormat and Target  ]-----------------------

            if (textureName.Contains("MSDFA"))
                mSETextureFormat = SETextureFormat.MSDFA;
            else
            if (textureName.Contains("MSDF"))
                mSETextureFormat = hasAlpha ? SETextureFormat.MSDFA : SETextureFormat.MSDF;
            else
            if (textureName.Contains("SDF"))
            {
                mSETextureFormat = SETextureFormat.SDF;
            }
            else
            {
                mSETextureFormat = SETextureFormat.Unknown;
                mSpread = -1;
				//if (textureName!="UnityWhite" && textureName!= "Font Texture")
                	//Debug.LogFormat("[{0}]Unrecognized texture format '{1}'. (the texture name should contain 'SDF', 'MSDF' or 'MSDFA') Fallback to SDF", gameObject.name, textureName);
            }
            UpdateSpread();
		}


		public Material GetMaterial(Material baseMaterial, Texture2D mainTexture, Shader shader )
		{
            if (!isActiveAndEnabled)
                return baseMaterial;

            var rparam = GetRenderParams();

            if (!mIsDirty_Material && (rparam.mMaterial != null && rparam._MaterialDef.MainTexture==mainTexture))
                return rparam.mMaterial;

            mIsDirty_Material = false;

            Setup (mainTexture);
            if (mSETextureFormat == SETextureFormat.Unknown)
                return baseMaterial;

			//if (baseMaterial && baseMaterial.mainTexture != mainTexture)
				//baseMaterial.mainTexture = mainTexture;

            if (_RenderPresets!=null)
            {
                foreach (var preset in _RenderPresets)
                    if (preset != null && preset._RenderParams != null)
                    {
                        preset._RenderParams.UpdateMaterial(baseMaterial, mainTexture, shader, this);
                        return preset._RenderParams.mMaterial;
                    }
            }

            _RenderParams.UpdateMaterial(baseMaterial, mainTexture, shader, this);
            return _RenderParams.mMaterial;
        }

        protected void OnDestroy()
		{
            _RenderParams.ReleaseMaterial();
            if (_RenderPresets!=null)
            {
                foreach (var preset in _RenderPresets)
                    if (preset != null)
                    {
                        if (preset._RenderParams != null) preset._RenderParams.ReleaseMaterial();
                        preset.UnRegisterRenderPresetDependency(this);
                    }
            }
        }

        protected void OnEnable()
		{
            #if SE_NGUI
                OnEnableNGUI();
            #endif
            #if SE_TMPro
                OnEnableTMPro();
            #endif
            OnEnableUGUI();

            GetRenderParams();
            if (_RenderPresets != null)
            {
                foreach (var preset in _RenderPresets)
                    if (preset != null)
                        preset.RegisterRenderPresetDependency(this);
            }

            if (Application.isPlaying && _OnEnable_PlayAnim >= 0 && _OnEnable_PlayAnim<_AnimationSlots.Length)
              _AnimationSlots[_OnEnable_PlayAnim].Play(this);

            MarkWidgetAsChanged(true, true);

            if (matParam_Spread<0)
                matParam_Spread = Shader.PropertyToID("_Spread");
            mSpread = -1;
            UpdateSpread();
        }

        public void UpdateSpread()
        {
            mSpread = -1;
            if (UpdateUGUIspread())
                return;
            #if SE_NGUI
            if (mSpread < 0 && UpdateNGUIspread())
                return;
            #endif

            if (mSpread < 0)
                mSpread = 15;    // Initialize here just in case the default material wasn't correctly setup
        }

        protected void OnDisable()
		{
            SmartEdgeManager.UnregisterAnimation(this);

            #if SE_NGUI
                OnDisableNGUI();
            #endif
            #if SE_TMPro
                OnDisableTMPro();
            #endif
                OnDisableUGUI();

            // If component was disabled, but not the GO, then retrieve the old material and recreate the vertices
            if (gameObject.activeSelf)
                MarkWidgetAsChanged(true, true);

            _RenderParams.ReleaseMaterial();
            if (_RenderPresets != null)
            {
                foreach (var preset in _RenderPresets)
                    if (preset != null && preset._RenderParams != null)
                    {
                        preset._RenderParams.ReleaseMaterial();
                        preset.UnRegisterRenderPresetDependency(this);
                    }
            }
        }

        //public void LateUpdate()
        //{
            //ValidateSettings();
        //}

        public void ValidateSettings()
        {
            #if SE_NGUI
            ValidateSettingsNGUI();
            #endif

            ValidateSettingsUGUI();
            //Debug.Log("Validate");
        }

        public void SetWidgetColor( Color32 color )
        {
            #if SE_NGUI
            SetWidgetColor_NGUI(color);
            #endif

            #if SE_TMPro
            SetWidgetColor_TMPro(color);
            #endif
            SetWidgetColor_UGUI(color);
        }

        bool ModifyText(ref string labelText)
        {
            return _TextEffect.ModifyText(this, ref labelText);
        }

        public bool IsMSDF()
        {
            return (mSETextureFormat == SmartEdge.SETextureFormat.MSDF || mSETextureFormat == SmartEdge.SETextureFormat.MSDFA);
        }

        public enum eHorizontalAlignment { Left, Center, Right, Justified}
        public eHorizontalAlignment GetHorizontalAlignment()
        {
            if (mGraphic as Text != null)
                return GetHorizontalAlignment_UGUI();

#if SE_NGUI
            if (mNGUI_Widget!=null)
                return GetHorizontalAlignment_NGUI();
#endif

#if SE_TMPro
            if (mTMP_Label!=null)
                return GetHorizontalAlignment_TMPro();
#endif

            return eHorizontalAlignment.Left;
        }

        public enum eVerticalAlignment { Top, Center, Bottom}
        public eVerticalAlignment GetVerticalAlignment()
        {
            if (mGraphic as Text != null)
                return GetVerticalAlignment_UGUI();

#if SE_NGUI
            if (mNGUI_Widget!=null)
                return GetVerticalAlignment_NGUI();
#endif

#if SE_TMPro
            if (mTMP_Label!=null)
                return GetVerticalAlignment_TMPro();
#endif

            return eVerticalAlignment.Bottom;
        }
    }
}
