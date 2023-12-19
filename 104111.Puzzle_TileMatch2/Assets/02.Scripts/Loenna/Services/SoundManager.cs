using UnityEngine;
using System;

using System.Collections.Generic;

public class SoundManager
{
	private readonly UserManager m_userManager;
    private readonly AudioSource m_audioSource;
    private readonly AudioSource m_bgmSource;
	private readonly Dictionary<string, AudioClip> m_audioClipDic;

	public SoundManager(GameObject root, UserManager userManager)
	{
		m_userManager = userManager;
		m_audioClipDic = new();
		m_audioSource = root.AddComponent<AudioSource>();
        m_bgmSource = root.AddComponent<AudioSource>();

        BgmInitialize();

		userManager.OnUpdated += OnUpdateSettings;
	}

	~SoundManager()
	{
		if (m_userManager != null)
		{
			m_userManager.OnUpdated -= OnUpdateSettings;
		}
	}

    private void BgmInitialize()
    {
        m_bgmSource.mute = true;
        m_bgmSource.playOnAwake = false;
        m_bgmSource.loop = true;
    }

	private void OnUpdateSettings(User user)
	{
		//Bgm 처리
        if (user != null && user.Settings.TryGetValue(SettingsType.Bgm, out var isMusicOn))
        {
            if (isMusicOn)
            {
                m_bgmSource.mute = false;
                if(!m_bgmSource.isPlaying)
                    m_bgmSource.Play();
            }
            else
            {
                m_bgmSource.mute = true;
                if (m_bgmSource.isPlaying)
                    m_bgmSource.Stop();
            }
        }
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

        if (m_audioClipDic.TryGetValue(Constant.Sound.BGM, out var clip))
		{
            m_bgmSource.clip = clip;
		}
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
