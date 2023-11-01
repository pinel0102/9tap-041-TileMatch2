using UnityEngine;
using UnityEditor;
using System.Collections;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif


namespace I2.SmartEdge
{
	public static partial class TextureTools 
	{
		public static bool MakeTextureReadable (Texture2D tex)
		{
			string sPath = AssetDatabase.GetAssetPath(tex);
			return MakeTextureReadable( sPath, false );
		}

		public static bool MakeTextureReadable (string path, bool force, bool forceUncompressed = false)
		{
			if (string.IsNullOrEmpty(path)) return false;
			TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
			if (ti == null) return false;
			
			TextureImporterSettings settings = new TextureImporterSettings();
			ti.ReadTextureSettings(settings);

            #if UNITY_5_5_OR_NEWER
				bool isUncompressed = ti.textureCompression == TextureImporterCompression.Uncompressed;
            #else
                bool isUncompressed = ( settings.textureFormat == TextureImporterFormat.Alpha8 ||
                                        settings.textureFormat == TextureImporterFormat.ARGB32 ||
                                        settings.textureFormat == TextureImporterFormat.RGBA32 ||
                                        settings.textureFormat == TextureImporterFormat.AutomaticTruecolor);
            #endif


            if (force || !settings.readable || settings.npotScale != TextureImporterNPOTScale.None || (!isUncompressed && forceUncompressed))
			{
				settings.readable = true;
				//settings.textureFormat = TextureImporterFormat.ARGB32;
				settings.npotScale = TextureImporterNPOTScale.None;
				settings.alphaIsTransparency = false;
#if UNITY_5_5_OR_NEWER
				settings.sRGBTexture = false;
                if (!isUncompressed && forceUncompressed) 
                    ti.textureCompression = TextureImporterCompression.Uncompressed;
#else
                settings.linearTexture = true;
                if (forceUncompressed && !isUncompressed)
                    settings.textureFormat = TextureImporterFormat.ARGB32;
#endif

                ti.SetTextureSettings(settings);
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
			}
			return true;
		}
		
	
		static public Texture2D ImportTexture (string path, bool forInput, bool force, bool alphaTransparency, bool forceUncompressed)
		{
			if (!string.IsNullOrEmpty(path))
			{
				if (forInput) { if (!MakeTextureReadable(path, force, forceUncompressed)) return null; }
				//else if (!MakeTextureAnAtlas(path, force, alphaTransparency)) return null;
				//return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
				
				Texture2D tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
				AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
				return tex;
			}
			return null;
		}
		
		static public Texture2D ImportTexture (Texture2D tex, bool forInput, bool force, bool alphaTransparency, bool forceUncompressed)
		{
			if (tex != null)
			{
				string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
				if (string.IsNullOrEmpty(path))
					return tex;
				return ImportTexture(path, forInput, force, alphaTransparency, forceUncompressed);
			}
			return null;
		}

		static public Color32[] GetPixels32(Texture2D tex)
		{
			if (tex == null)
				return null;
			try
			{
				if (tex.format != TextureFormat.ARGB32)
					tex = TextureTools.ImportTexture (tex, true, true, true, true);

				Color32[] Colors = tex.GetPixels32();
				return Colors;
			}
			catch (System.Exception)
			{
				Texture2D inTexture = TextureTools.ImportTexture (tex, true, false, true, true);
				if (inTexture==null)
					return null;
				return inTexture.GetPixels32 ();
			}
		}

		static public bool SaveTexture( Texture2D texture, string path = null )
		{
			TextureImporterFormat format = GetImporterFormat(texture.format);
			return SaveTexture( texture, path, format );
		}

		static public bool SaveTexture( Texture2D texture, string path, TextureImporterFormat format, bool linear = false)
		{
			if (string.IsNullOrEmpty (path))
				path = AssetDatabase.GetAssetPath(texture);

			if (string.IsNullOrEmpty (path))
				return false;

			// Clear the read-only flag in texture file attributes
			if (System.IO.File.Exists(path))
			{
				System.IO.FileAttributes newPathAttrs = System.IO.File.GetAttributes(path);
				if ((newPathAttrs & System.IO.FileAttributes.ReadOnly)>0)
				{
					newPathAttrs &= ~System.IO.FileAttributes.ReadOnly;
					System.IO.File.SetAttributes(path, newPathAttrs);
				}
			}

			if (path.StartsWith(Application.dataPath, System.StringComparison.OrdinalIgnoreCase))
			{
				path.Substring(Application.dataPath.Length);
				path = string.Concat("Assets/", path);
			}

			byte[] PNG = texture.EncodeToPNG ();
			System.IO.File.WriteAllBytes(path, PNG);
			if (!string.IsNullOrEmpty(path))
			{
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

				TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
				if (ti == null) return true;

				bool bReimport = false;
#if UNITY_5_5_OR_NEWER
				if (ti.textureType != TextureImporterType.Default) { ti.textureType = TextureImporterType.Default; bReimport = true; }
				if (ti.textureCompression != TextureImporterCompression.Uncompressed) { ti.textureCompression = TextureImporterCompression.Uncompressed; bReimport = true; }
				if (ti.maxTextureSize <= texture.width ||
					ti.maxTextureSize <= texture.height) { ti.maxTextureSize = 8192; bReimport = true; }
#else
				if (ti.textureType != TextureImporterType.Advanced) { ti.textureType = TextureImporterType.Advanced; bReimport = true; }
#endif
				TextureImporterSettings settings = new TextureImporterSettings();
				ti.ReadTextureSettings(settings);


				if (settings.npotScale != TextureImporterNPOTScale.None) { settings.npotScale = TextureImporterNPOTScale.None; bReimport=true; }
				if (settings.readable) 						 			 { settings.readable = false; bReimport=true; }
				if (!settings.alphaIsTransparency) 						 { settings.alphaIsTransparency = false; bReimport=true; }
				if (settings.mipmapEnabled) 							 { settings.mipmapEnabled = false; bReimport=true; }

#if UNITY_5_5_OR_NEWER
				if (settings.sRGBTexture == linear) { settings.sRGBTexture = !linear; bReimport = true; }
#else
				if (settings.textureFormat!=format)				 		 { settings.textureFormat=format; bReimport=true; }
				if (settings.maxTextureSize<=texture.width ||
					settings.maxTextureSize<=texture.height)			 { settings.maxTextureSize=8192; bReimport=true; }
				if (settings.linearTexture!=linear)				 		 { settings.linearTexture=linear; bReimport=true; }
#endif
				if (bReimport)
				{
					ti.SetTextureSettings(settings);
					AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
				}
			}
			return true;
		}

		static public Texture2D SaveTexture(Texture2D texture, Object rootAsset, TextureImporterFormat format, bool linear = false)
		{
			string path = AssetDatabase.GetAssetPath(rootAsset);
			if (string.IsNullOrEmpty(path))
				return null;

			// Clear the read-only flag in texture file attributes
			if (System.IO.File.Exists(path))
			{
				System.IO.FileAttributes newPathAttrs = System.IO.File.GetAttributes(path);
				if ((newPathAttrs & System.IO.FileAttributes.ReadOnly) > 0)
				{
					newPathAttrs &= ~System.IO.FileAttributes.ReadOnly;
					System.IO.File.SetAttributes(path, newPathAttrs);
				}
			}

			if (path.StartsWith(Application.dataPath, System.StringComparison.OrdinalIgnoreCase))
			{
				path.Substring(Application.dataPath.Length);
				path = string.Concat("Assets/", path);
			}

			AssetDatabase.AddObjectToAsset(texture, rootAsset);

			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

			string ss = AssetDatabase.GetAssetPath(texture);
			var mti = AssetImporter.GetAtPath(ss);
			TextureImporter ti = mti as TextureImporter;
			if (ti == null) return null;

			bool bReimport = false;
#if UNITY_5_5_OR_NEWER
			if (ti.textureType != TextureImporterType.Default) { ti.textureType = TextureImporterType.Default; bReimport = true; }
			if (ti.textureCompression != TextureImporterCompression.Uncompressed) { ti.textureCompression = TextureImporterCompression.Uncompressed; bReimport = true; }
			if (ti.maxTextureSize <= texture.width ||
				ti.maxTextureSize <= texture.height) { ti.maxTextureSize = 8192; bReimport = true; }
#else
			if (ti.textureType != TextureImporterType.Advanced) { ti.textureType = TextureImporterType.Advanced; bReimport = true; }
#endif

			TextureImporterSettings settings = new TextureImporterSettings();
			ti.ReadTextureSettings(settings);


			if (settings.npotScale != TextureImporterNPOTScale.None) { settings.npotScale = TextureImporterNPOTScale.None; bReimport = true; }
			if (settings.readable) { settings.readable = false; bReimport = true; }
			if (!settings.alphaIsTransparency) { settings.alphaIsTransparency = false; bReimport = true; }
			if (settings.mipmapEnabled) { settings.mipmapEnabled = false; bReimport = true; }
#if UNITY_5_5_OR_NEWER
			if (settings.sRGBTexture == linear) { settings.sRGBTexture = !linear; bReimport = true; }
#else
			if (settings.textureFormat != format) { settings.textureFormat = format; bReimport = true; }
			if (settings.maxTextureSize <= texture.width ||
				settings.maxTextureSize <= texture.height) { settings.maxTextureSize = 8192; bReimport = true; }
			if (settings.linearTexture != linear) { settings.linearTexture = linear; bReimport = true; }
#endif
			if (bReimport)
			{
				ti.SetTextureSettings(settings);
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
			}

			return texture;
		}

		static TextureImporterFormat GetImporterFormat( TextureFormat format )
		{
			TextureImporterFormat outFormat = (TextureImporterFormat)0;
			string formatStr = format.ToString ();
			while (true)
			{
				string Str = outFormat.ToString();
				if (string.IsNullOrEmpty(Str))
				{
					return TextureImporterFormat.ARGB32;
				}
				if (Str==formatStr)
					return outFormat;
				outFormat++;
			}
		}

		static public bool SaveTexture( Color32[] Colors, TextureFormat Format, int width, int height, string path )
		{
			if (width * height != Colors.Length)
				return false;

			Texture2D OutputTexture = new Texture2D(width, height, Format, false);
			OutputTexture.SetPixels32 (Colors);

			return SaveTexture (OutputTexture, path);
		}

		public static string GetTextureMemory(Texture2D inTex)
		{
			return EditorUtility.FormatBytes((int)(inTex.width * inTex.height * GetBpp(inTex)));
		}
		
		public static float GetBpp(Texture2D inTex)
		{
			switch(inTex.format)
			{
			case TextureFormat.PVRTC_RGB2:
			case TextureFormat.PVRTC_RGBA2:
				return 0.25f;
				
			case TextureFormat.PVRTC_RGB4:
			case TextureFormat.PVRTC_RGBA4:
			case TextureFormat.DXT1:
				return 0.5f;
				
			case TextureFormat.Alpha8:
			case TextureFormat.DXT5:
				return 1f;
				
				//		case TextureFormat.RGB16:
				//		case TextureFormat.RGBA16:
				//			return 2f;
				
			case TextureFormat.RGB24:
				return 3f;
				
			case TextureFormat.RGBA32:
			case TextureFormat.ARGB32:
			case TextureFormat.ETC_RGB4:
				
			case TextureFormat.ETC2_RGBA8:
				return 8f;
			}

#if UNITY_5_6_OR_NEWER
			return Profiler.GetRuntimeMemorySizeLong(inTex);
#else
            return Profiler.GetRuntimeMemorySize(inTex);
#endif
        }

        public static Texture2D Clone( Texture2D texture )
		{
			Texture2D newTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount>0);
			newTexture.SetPixels32(texture.GetPixels32());
			newTexture.Apply();
			return newTexture;
		}
	 }
}