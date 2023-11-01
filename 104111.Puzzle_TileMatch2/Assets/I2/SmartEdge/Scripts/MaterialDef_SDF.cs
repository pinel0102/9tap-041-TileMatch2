using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace I2.SmartEdge
{
	enum eSEShaderFlag
	{
		TEXTURES		=(1<<0), 
		NORMAL_MAP		=(1<<1), 
		ENVIRONMENT_MAP =(1<<2),

		MSDF            =(1<<3), 
		MSDFA           =(1<<4), 
		BEVEL		    =(1<<5), 
		SUPER_SAMPLING  =(1<<6),
        MULTI_MODES     =(1<<7),

        // Compact
        OUTLINE         =(1<<8), 
        GLOW            =(1<<9),
		SHADOW			=(1<<10),
		INNER_SHADOW	=(1<<11)
	}	
	
	[System.Serializable]
	public class MaterialDef_SDF : MaterialDef
	{
		public Texture MainTexture, TexFace, TexOutline, TexBumpMap, TexGlowMap, TexEnvironment;
		public ushort Flags;	// Combination of SEShaderFlag

		public Vector4 GlobalData;
		public bool UseWidgetTexture;

		public Vector4 LightData;
		public Vector4 LightSpecularData;
		public Color   LightSpecularColor;
		public Vector3 LightDir;

		public Vector4 BevelData;
		public Vector4 BevelWaveData;
		public Vector4 BumpData;
		public Vector4 EnvReflectionData;
		public Color   EnvReflectionColor_Face, EnvReflectionColor_Outline;
		public Vector3 EnvReflectionMatrixX, EnvReflectionMatrixY, EnvReflectionMatrixZ;


		public Vector4 GlowData, GlowOffset;
		public Color   GlowColor;
		public Vector4 ShadowData;
		public Vector4 InnerShadowData, InnerShadowOffset;
		public Color   ShadowColor, InnerShadowColor;


		#region Pooling
		static List<MaterialDef_SDF> mInstancePool = new List<MaterialDef_SDF>();
		public static MaterialDef_SDF CreateFromPool()
		{
			if (mInstancePool.Count > 0) 
			{
				var inst = mInstancePool [0];
				mInstancePool.RemoveAt (0);
				return inst;
			}

			return new MaterialDef_SDF() ;
		}
		public override void ReleaseToPool()
		{
			const int maxInstances = 10;
			if (mInstancePool.Count<maxInstances)
				mInstancePool.Add(this);
		}
		#endregion

		public override MaterialDef Clone()
		{
			var newDef = MaterialDef_SDF.CreateFromPool();
			newDef.shader             = shader;
			newDef.MainTexture        = MainTexture;
			newDef.TexFace            = TexFace;
			newDef.TexOutline         = TexOutline;
			newDef.TexBumpMap         = TexBumpMap;
			newDef.TexGlowMap         = TexGlowMap;
			newDef.TexEnvironment     = TexEnvironment;
			newDef.Flags              = Flags;

			newDef.LightData		  = LightData;
			newDef.LightSpecularData  = LightSpecularData;
			newDef.LightSpecularColor = LightSpecularColor;

			newDef.LightDir			  = LightDir;

			newDef.GlobalData 		  = GlobalData;
			newDef.UseWidgetTexture   = UseWidgetTexture;

			newDef.BevelData		  = BevelData;
			newDef.BevelWaveData	  = BevelWaveData;
			newDef.BumpData 		  = BumpData;
			newDef.EnvReflectionMatrixX         = EnvReflectionMatrixX;
			newDef.EnvReflectionMatrixY         = EnvReflectionMatrixY;
			newDef.EnvReflectionMatrixZ         = EnvReflectionMatrixZ;
			newDef.EnvReflectionColor_Face		= EnvReflectionColor_Face;
			newDef.EnvReflectionColor_Outline   = EnvReflectionColor_Outline;
			newDef.EnvReflectionData            = EnvReflectionData;

			newDef.ShadowData 		  = ShadowData;
			newDef.ShadowColor 		  = ShadowColor;
			newDef.InnerShadowColor	  = InnerShadowColor;
			newDef.InnerShadowData	  = InnerShadowData;
			newDef.InnerShadowOffset  = InnerShadowOffset;

			newDef.GlowColor		  = GlowColor;
			newDef.GlowData 		  = GlowData;
			newDef.GlowOffset 		  = GlowOffset;
			return newDef;
		}

		public override bool IsEqualTo( MaterialDef other )
		{
			if (GetType() != other.GetType())
				return false;
			var other_SDF = (MaterialDef_SDF)other;

			return (Flags 		   	   == other_SDF.Flags &&
					shader 		   	   == other_SDF.shader &&

					MainTexture    	   == other_SDF.MainTexture &&
					TexFace 	   	   == other_SDF.TexFace &&
					TexOutline 	   	   == other_SDF.TexOutline &&
					TexBumpMap 	   	   == other_SDF.TexBumpMap &&
					TexGlowMap 		   == other_SDF.TexGlowMap &&
					TexEnvironment     == other_SDF.TexEnvironment && 

					GlobalData		   == other_SDF.GlobalData &&
					UseWidgetTexture   == other_SDF.UseWidgetTexture &&

					LightData          == other_SDF.LightData &&
					LightSpecularData  == other_SDF.LightSpecularData &&
					LightSpecularColor == other_SDF.LightSpecularColor && 
					LightDir           == other_SDF.LightDir && 
					BevelData          == other_SDF.BevelData && 
					BevelWaveData      == other_SDF.BevelWaveData && 
					BumpData           == other_SDF.BumpData &&
					EnvReflectionMatrixX        == other_SDF.EnvReflectionMatrixX &&
					EnvReflectionMatrixY        == other_SDF.EnvReflectionMatrixY &&
					EnvReflectionMatrixZ        == other_SDF.EnvReflectionMatrixZ &&
					EnvReflectionColor_Face     == other_SDF.EnvReflectionColor_Face &&
					EnvReflectionColor_Outline  == other_SDF.EnvReflectionColor_Outline &&
					EnvReflectionData           == other_SDF.EnvReflectionData &&

					GlowColor == other_SDF.GlowColor &&
					GlowData  			== other_SDF.GlowData &&
					GlowOffset 			== other_SDF.GlowOffset &&
					ShadowColor			== other_SDF.ShadowColor &&
					ShadowData 			== other_SDF.ShadowData &&
					InnerShadowColor	== other_SDF.InnerShadowColor &&
					InnerShadowData		== other_SDF.InnerShadowData &&
					InnerShadowOffset	== other_SDF.InnerShadowOffset);
		}

		
		public static int matParam_GlobalData			= -1;
		public static int matParam_UseWidgetTexture     = -1;
		public static int matParam_Bevel 				= -1;
		public static int matParam_BevelWave 			= -1;
		public static int matParam_LightData            = -1;
		public static int matParam_LightSpecularData    = -1;
		public static int matParam_LightDir 			= -1;
		public static int matParam_LightSpecularColor   = -1;
		public static int matParam_BumpData 			= -1;
		public static int matParam_EnvReflectionMatrixX = -1;
		public static int matParam_EnvReflectionMatrixY = -1;
		public static int matParam_EnvReflectionMatrixZ = -1;
		public static int matParam_EnvReflectionFace 	= -1;
		public static int matParam_EnvReflectionOutline = -1;
		public static int matParam_EnvReflectionData    = -1;
		public static int matParam_ShadowData			= -1;
		public static int matParam_ShadowColor 			= -1;
		public static int matParam_InnerShadowData		= -1;
		public static int matParam_InnerShadowColor 	= -1;
		public static int matParam_InnerShadowOffset 	= -1;
		public static int matParam_GlowColor			= -1;
		public static int matParam_GlowData				= -1;
		public static int matParam_GlowOffset           = -1;
		public static int matParam_I2MainTex_TexelSize  = -1;


		void ToggleFlag( eSEShaderFlag flag, bool Enable )
		{
			if (Enable) 
				Flags |= (ushort)flag;
			else 
				Flags &= (ushort)~(ushort)flag;
		}
		
		bool HasFlag( eSEShaderFlag flag ) { return (Flags & (ushort)flag)>0; }
		
		public void SetDefaultValues()
		{
			MainTexture = TexFace = TexOutline = TexBumpMap = TexGlowMap = TexEnvironment = null;
			Flags = 0;

			UseWidgetTexture = false;
			GlobalData = LightData = LightSpecularData = BevelData = BevelWaveData = BumpData = EnvReflectionData = GlowData = GlowOffset = ShadowData = InnerShadowData = InnerShadowOffset = MathUtils.v4zero;
            EnvReflectionData.x = 100;
			LightSpecularData.z = 0.001f;
			LightSpecularColor = GlowColor = MathUtils.transparentWhite;
			ShadowColor = InnerShadowColor = MathUtils.transparentBlack;
            EnvReflectionColor_Face = EnvReflectionColor_Outline = MathUtils.transparentBlack;
            LightDir = EnvReflectionMatrixX = EnvReflectionMatrixY = EnvReflectionMatrixZ = MathUtils.v3zero;
		}

		public void SetTextures( Texture texMain, Texture texFace, Texture texOutline, Texture texBumpMap, Texture texGlowMap, Texture texEnvironmentMap )
		{
			MainTexture         = texMain;
			TexFace 			= texFace;
			TexOutline 			= texOutline;
			TexBumpMap 			= texBumpMap;
			TexGlowMap			= texGlowMap;
			TexEnvironment 	    = texEnvironmentMap;
		}

		public void SetFlags( bool useWidgetTexture, bool UseSuperSampling, bool UseMultiModes, bool useOutline, SmartEdge.SETextureFormat format )
		{
			if (useWidgetTexture)           UseWidgetTexture = useWidgetTexture;
			if (TexEnvironment!=null)       Flags |= (ushort)eSEShaderFlag.ENVIRONMENT_MAP;
			if (UseSuperSampling)           Flags |= (ushort)eSEShaderFlag.SUPER_SAMPLING;
            if (UseMultiModes)              Flags |= (ushort)eSEShaderFlag.MULTI_MODES;
            if (useOutline)                 Flags |= (ushort)eSEShaderFlag.OUTLINE;

            switch (format)
			{
				case SmartEdge.SETextureFormat.MSDF 	:  Flags |= (ushort)eSEShaderFlag.MSDF; break;
				case SmartEdge.SETextureFormat.MSDFA 	:  Flags |= (ushort)eSEShaderFlag.MSDFA; break;
			}

			if (TexFace!=null || TexOutline!=null || TexBumpMap!=null || TexGlowMap!=null || TexEnvironment!=null)
			{
				Flags |= (ushort)eSEShaderFlag.TEXTURES;
				if (TexBumpMap!=null) 	Flags |= (ushort)eSEShaderFlag.NORMAL_MAP;
			}
			
			//shader = (bNGUI ? pShader_NGUI : pShader_UGUI);
		}

		public void SetSDF( float softness, float faceSoftness, float outlineSoftness, float OnePixelSpread )
		{
			faceSoftness *= 0.3f;
			if (faceSoftness > 1)   faceSoftness = 1;
			outlineSoftness *= 0.3f;
			if (outlineSoftness > 1)   outlineSoftness = 1;

			GlobalData = new Vector4(softness, faceSoftness, outlineSoftness, OnePixelSpread);
		}
		
		public void SetLighting( float difIntensity, float difShadow, float specularStart, float specularSmoothness, float specularPower, Color specularColor, Vector3 lightDir)
		{
			LightData          = new Vector4(difIntensity, 1-difShadow, 0, 0);
			LightSpecularData = new Vector4(1 - specularStart - specularSmoothness, specularSmoothness, 0.001f + 8 * specularPower, 0);
			LightSpecularColor = specularColor;
			LightDir	       = lightDir;
		}

		public void SetBumpData( float faceBumpStrength, float outlineBumpStrength, float depth )
		{
			BumpData    = new Vector4(faceBumpStrength, outlineBumpStrength, 4*(depth*2 - 1), 0);
		}

		
		public void SetBevel(float offset, float width, float depth, float curveWidth, float curveClamp, float innerSoftness, float outerSoftness)
		{
			Flags        |= (ushort)eSEShaderFlag.BEVEL;
			BevelData     = new Vector4(offset, Mathf.Clamp(width, 0.02f, 1), depth, Mathf.Clamp01(curveWidth));
			BevelWaveData = new Vector4(Mathf.Clamp(innerSoftness, 0.02f, 1), Mathf.Clamp(outerSoftness, 0.02f, 1), curveClamp, 0);
		}

		public void SetEnvReflection( Color faceColor, Color outlineColor, float angleX, float angleY, float angleZ, float fresnelBias, float fresnelScale, float intensity, float glass)
		{
			var envMatrix = Matrix4x4.TRS(MathUtils.v3zero, Quaternion.Euler(angleY, angleX, angleZ), MathUtils.v3one);
			EnvReflectionMatrixX = envMatrix.GetRow(0);
			EnvReflectionMatrixY = envMatrix.GetRow(1);
			EnvReflectionMatrixZ = envMatrix.GetRow(2);
			EnvReflectionColor_Face    = faceColor;
			EnvReflectionColor_Outline = outlineColor;
			EnvReflectionColor_Face.r    *= intensity; EnvReflectionColor_Face.g    *= intensity; EnvReflectionColor_Face.b     *= intensity; EnvReflectionColor_Face.a = glass;
			EnvReflectionColor_Outline.r *= intensity; EnvReflectionColor_Outline.g *= intensity; EnvReflectionColor_Outline.b  *= intensity; EnvReflectionColor_Outline.a = glass;
			EnvReflectionData = new Vector4(fresnelBias- fresnelScale, fresnelScale, 0, 0);
		}

		public void SetGlow( Color color, float innerWidth, float edgeWidth, float outerWidth, float power, float offset, float surfaceBorder )
		{
			Flags |= (ushort)eSEShaderFlag.GLOW;


			float edge = surfaceBorder - offset;
			float surface = edge + edgeWidth;

			if (edge < 0) edge = 0;
			if (surface > 1) surface = 1;

			innerWidth = (1 - surface) * innerWidth;
			outerWidth = (edge * outerWidth);

			GlowColor = color;
			GlowData = new Vector4 (innerWidth, edgeWidth, outerWidth, 0.001f+power);
			GlowOffset = new Vector4 (offset, 0, 0, 0);
		}

		public void SetShadow( Color shadowColor, float offsetX, float offsetY, float softness, float expand )
		{
			Flags |= (ushort)eSEShaderFlag.SHADOW;
			ShadowData = new Vector4 (offsetX, offsetY, softness, expand);
			ShadowColor = shadowColor;
		}

		public void SetInnerShadow( Color shadowColor, float offsetX, float offsetY, float softness, float expandEdge, float expandMask, float maskSoftness )
		{
			Flags |= (ushort)eSEShaderFlag.INNER_SHADOW;
			InnerShadowColor = shadowColor;
			InnerShadowData = new Vector4 (expandEdge, expandMask, softness, maskSoftness);
			InnerShadowOffset = new Vector4 (offsetX, offsetY, 0, 0);
		}
		


		void ToggleMaterialKeyword( Material mat, string key, bool enable)
		{
			if (enable)
				mat.EnableKeyword(key);
			else
				mat.DisableKeyword(key);
		}
		
		public override void Apply( Material material )
		{
			if (material.shader != shader) 
				material.shader = shader;

			ToggleMaterialKeyword(material, "MSDF",             HasFlag(eSEShaderFlag.MSDF));
			ToggleMaterialKeyword(material, "MSDFA",         	HasFlag(eSEShaderFlag.MSDFA));
			ToggleMaterialKeyword(material, "TEXTURES",         HasFlag(eSEShaderFlag.TEXTURES));
			ToggleMaterialKeyword(material, "BEVEL",     	    HasFlag(eSEShaderFlag.BEVEL));
            //ToggleMaterialKeyword(material, "ENVIRONMENT_MAP",  HasFlag(eSEShaderFlag.ENVIRONMENT_MAP ));
            //ToggleMaterialKeyword(material, "NORMAL_MAP",   	HasFlag(eSEShaderFlag.NORMAL_MAP));
            ToggleMaterialKeyword(material, "OUTLINE",          HasFlag(eSEShaderFlag.OUTLINE));
            ToggleMaterialKeyword(material, "SHADOW",   		HasFlag(eSEShaderFlag.SHADOW));
			ToggleMaterialKeyword(material, "INNER_SHADOW",		HasFlag(eSEShaderFlag.INNER_SHADOW));
			ToggleMaterialKeyword(material, "GLOW",             HasFlag(eSEShaderFlag.GLOW));
			ToggleMaterialKeyword(material, "SUPER_SAMPLING",   HasFlag(eSEShaderFlag.SUPER_SAMPLING));
            ToggleMaterialKeyword(material, "MULTI_MODES",      HasFlag(eSEShaderFlag.MULTI_MODES));


			//ToggleMaterialKeyword(material, "GLOW_SHADOW",      HasFlag(eSEShaderFlag.GLOW) || HasFlag(eSEShaderFlag.SHADOW));

			material.mainTexture = MainTexture;
			material.SetTexture("_FaceTex", 		TexFace);
			material.SetTexture("_OutlineTex", 		TexOutline);
			material.SetTexture("_GlowTex", 		TexGlowMap);
			material.SetTexture("_NormalMap", 		TexBumpMap);
			material.SetTexture("_EnvironmentMap",  TexEnvironment);

			if (matParam_GlobalData < 0)
			{
				matParam_GlobalData         	= Shader.PropertyToID("_GlobalData");
				matParam_UseWidgetTexture       = Shader.PropertyToID("_UseWidgetTexture");
				matParam_LightDir               = Shader.PropertyToID("_LightDir");
				matParam_LightData              = Shader.PropertyToID("_LightData");
				matParam_LightSpecularData      = Shader.PropertyToID("_LightSpecularData");
				matParam_LightSpecularColor     = Shader.PropertyToID("_LightSpecularColor");
				matParam_Bevel                  = Shader.PropertyToID("_BevelData");
				matParam_BevelWave       		= Shader.PropertyToID("_BevelWave");
				matParam_BumpData 	      		= Shader.PropertyToID("_BumpData");
				matParam_EnvReflectionMatrixX   = Shader.PropertyToID("_EnvReflectionMatrixX");
				matParam_EnvReflectionMatrixY   = Shader.PropertyToID("_EnvReflectionMatrixY");
				matParam_EnvReflectionMatrixZ   = Shader.PropertyToID("_EnvReflectionMatrixZ");
				matParam_EnvReflectionFace      = Shader.PropertyToID("_EnvReflectionColor_Face");
				matParam_EnvReflectionOutline   = Shader.PropertyToID("_EnvReflectionColor_Outline");
				matParam_EnvReflectionData      = Shader.PropertyToID("_EnvReflectionData");
				matParam_ShadowColor            = Shader.PropertyToID("_ShadowColor");
				matParam_ShadowData   			= Shader.PropertyToID("_ShadowData");
				matParam_GlowColor   			= Shader.PropertyToID("_GlowColor");
				matParam_GlowData   			= Shader.PropertyToID("_GlowData");
				matParam_GlowOffset   			= Shader.PropertyToID("_GlowOffset");

				matParam_InnerShadowData		= Shader.PropertyToID("_InnerShadowData");
				matParam_InnerShadowColor 		= Shader.PropertyToID("_InnerShadowColor");
				matParam_InnerShadowOffset 		= Shader.PropertyToID("_InnerShadowOffset");
				matParam_I2MainTex_TexelSize    = Shader.PropertyToID("_I2MainTex_TexelSize");
			}

			material.SetVector(matParam_GlobalData,       	 	GlobalData);
			material.SetFloat(matParam_UseWidgetTexture,        UseWidgetTexture?1:0);
			material.SetVector(matParam_LightDir,       	 	LightDir);
			material.SetVector(matParam_LightData,              LightData);
			material.SetVector(matParam_LightSpecularData,      LightSpecularData);
			material.SetColor (matParam_LightSpecularColor,     LightSpecularColor);
			material.SetVector(matParam_Bevel,         		 	BevelData);
			material.SetVector(matParam_BevelWave,     	 	 	BevelWaveData);
			material.SetVector(matParam_BumpData,   		 	BumpData);
			material.SetVector(matParam_EnvReflectionMatrixX,   EnvReflectionMatrixX);
			material.SetVector(matParam_EnvReflectionMatrixY,   EnvReflectionMatrixY);
			material.SetVector(matParam_EnvReflectionMatrixZ,   EnvReflectionMatrixZ);
			material.SetVector(matParam_EnvReflectionFace,  	EnvReflectionColor_Face);
			material.SetVector(matParam_EnvReflectionOutline,   EnvReflectionColor_Outline);
			material.SetVector(matParam_EnvReflectionData,      EnvReflectionData);
			material.SetVector(matParam_ShadowData, 			ShadowData);
			material.SetColor (matParam_ShadowColor, 			ShadowColor);
			material.SetColor (matParam_GlowColor, 				GlowColor);
			material.SetVector(matParam_GlowData, 				GlowData);
			material.SetVector(matParam_GlowOffset,				GlowOffset);
			material.SetVector(matParam_InnerShadowData, 		InnerShadowData);
			material.SetVector(matParam_InnerShadowOffset, 		InnerShadowOffset);
			material.SetColor (matParam_InnerShadowColor, 		InnerShadowColor);

			// Fix for some versions of unity sending _MainTex_TexelSize as (width,height,0,0) instead of (1/width, 1/height,0,0) : e.g. unity 5.3.3p2
			material.SetVector(matParam_I2MainTex_TexelSize, new Vector4(1 / (float)MainTexture.width, 1 / (float)MainTexture.height, MainTexture.width, MainTexture.height));
		}
	}
}