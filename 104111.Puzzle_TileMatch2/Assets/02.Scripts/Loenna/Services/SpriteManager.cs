using UnityEngine;
using UnityEngine.U2D;

using System;
using System.Collections.Generic;

public class SpriteAtlasData
{
	private readonly List<string> m_spriteList = new();
	private readonly SpriteAtlas m_atlas;

	public SpriteAtlas Atlas => m_atlas;

	public SpriteAtlasData(SpriteAtlas atlas)
	{
		m_atlas = atlas;
		Sprite[] sprites = new Sprite[atlas.spriteCount];
		atlas.GetSprites(sprites);

		Array.ForEach(sprites, 
			sprite => {
				if (!m_spriteList.Contains(sprite.name))
				{
					string spriteName = sprite.name.Replace("(Clone)", string.Empty);
					m_spriteList.Add(spriteName);
				}
			}
		);
	}

	public bool TryGetSprite(string spriteName, out Sprite sprite)
	{
		if (!m_spriteList.Contains(spriteName))
		{
			sprite = null;
			return false;
		}

		sprite = m_atlas.GetSprite(spriteName);
		return true;
	}
}

public static class SpriteManager
{
	private static SpriteManager_Imp s_implementation;

	public static void Initialize()
	{
		s_implementation = new();
	}

	public static Sprite GetSprite(string atlasName, string spriteName)
	{
		if (s_implementation == null)
		{
			Debug.LogError("[SpriteManager] Initialize 하지 않았습니다");
			return null;
		}

		if (!s_implementation.TryGetAtlas(atlasName, out var atlas))
		{
			return null;
		}

		return atlas.GetSprite(spriteName);
	}

	public static Sprite GetSprite(string spriteName)
	{
		if (s_implementation == null)
		{
			Debug.LogError("[SpriteManager] Initialize 하지 않았습니다");
			return null;
		}

		if (s_implementation.TryGetSprite(spriteName, out Sprite result))
		{
			return result;
		}

		return null;
	}

	public static Texture2D GetTexture(string spriteName)
	{
		Sprite sprite = GetSprite(spriteName);
		
		if (sprite == null)
		{
			return null;
		}

		Rect rect = sprite.textureRect;
		int x = ToInt(rect.x);
		int y = ToInt(rect.y);
		int width = ToInt(rect.width);
		int	height = ToInt(rect.height);

		Color[] pixels = sprite.texture.GetPixels(x, y, width, height);

		Texture2D texture = new Texture2D(width, height);
		texture.SetPixels(pixels);
		texture.Apply();

		return texture;

		int ToInt(float value)
		{
			return Mathf.RoundToInt(value);
		}
	}
}

class SpriteManager_Imp : IDisposable
{
	private Dictionary<string, SpriteAtlasData> m_spriteAtlases = new();

	public SpriteManager_Imp()
	{
		SpriteAtlasManager.atlasRequested += RequestAtlas;

		var atlases = Resources.LoadAll<SpriteAtlas>("SpriteAtlas");

		foreach (var atlas in atlases)
		{
			string atlasName = atlas.name.Replace("(Clone)", string.Empty);
			m_spriteAtlases.TryAdd(atlasName, new SpriteAtlasData(atlas));
		}
	}

	public void Dispose()
	{
		SpriteAtlasManager.atlasRequested -= RequestAtlas;
		m_spriteAtlases.Clear();
	}

	public bool TryGetAtlas(string atlasName, out SpriteAtlas atlas)
	{
		bool exist = m_spriteAtlases.ContainsKey(atlasName);

		atlas = exist? m_spriteAtlases[atlasName].Atlas : null;
		return exist;
	}

	public bool TryGetSprite(string spriteName, out Sprite sprite)
	{
		foreach (var (_, data) in m_spriteAtlases)
		{
			if (data.TryGetSprite(spriteName, out sprite))
			{
				return true;
			}
		}

		sprite = null;
		return false;
	}

	private void RequestAtlas(string key, Action<SpriteAtlas> action)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return;
		}

		if (!m_spriteAtlases.ContainsKey(key))
		{
			var atlas = Resources.Load<SpriteAtlas>(key);
			m_spriteAtlases.TryAdd(key, new SpriteAtlasData(atlas));
			action?.Invoke(atlas);
		}
	}
}
