using UnityEngine;
using System;

using System.Collections.Generic;

public class SoundManager
{
	private readonly UserManager m_userManager;
	private readonly AudioSource m_audioSource;
	private readonly Dictionary<string, AudioClip> m_audioClipDic;

	public SoundManager(GameObject root, UserManager userManager)
	{
		m_userManager = userManager;
		m_audioClipDic = new();
		m_audioSource = root.AddComponent<AudioSource>();

		userManager.OnUpdated += OnUpdateSettings;
	}

	~SoundManager()
	{
		if (m_userManager != null)
		{
			m_userManager.OnUpdated -= OnUpdateSettings;
		}
	}

	private void OnUpdateSettings(User user)
	{
		//Bgm 처리
	}

	public void Load()
	{
		var clips = Resources.LoadAll<AudioClip>("Sounds");
		if (clips?.Length <= 0)
		{
			return;
		}

		Array.ForEach(
			clips, 
			clip => {
				m_audioClipDic.TryAdd(clip.name, clip);
			}
		);
	}

	public void PlayFx(string clipName)
	{
		var settings = m_userManager?.Current?.Settings;

		if (settings == null || !settings.TryGetValue(SettingsType.Fx, out var fx))
		{
			return;
		}

		if (!fx)
		{
			return;
		}

		if (m_audioClipDic.TryGetValue(clipName, out var clip))
		{
			m_audioSource.PlayOneShot(clip);
		}
	}
}
