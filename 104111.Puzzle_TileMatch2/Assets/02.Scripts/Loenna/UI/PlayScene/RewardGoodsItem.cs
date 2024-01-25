using UnityEngine;
using UnityEngine.UI;

using System;
using System.Threading;

using TMPro;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using DG.Tweening;

using AssetKits.ParticleImage;

using NineTap.Common;

public class RewardGoodsItemParameter
{
	public bool Animated;
	public int IconSize;
	public int FontSize;
}

[ResourcePath("UI/Widgets/RewardGoodsItem")]
public class RewardGoodsItem : CachedBehaviour
{
	[SerializeField]
	private CanvasGroup m_haloAlpha;

	[SerializeField]
	private Image m_icon;

	[SerializeField]
	private TMP_Text m_countText;

	[SerializeField]
	private ParticleImage m_particleImage;

	private bool m_animated;
	private bool m_showParticle;

	public void OnSetup(RewardGoodsItemParameter parameter)
	{
		m_icon.rectTransform.SetSize(parameter.IconSize);
		m_countText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parameter.FontSize);
		m_countText.fontSize = parameter.FontSize;
		m_particleImage.gameObject.SetActive(false);
		m_haloAlpha.alpha = 0f;
		m_animated = parameter.Animated;
	}

	public void UpdateUI(string spriteName, string count, Transform attractorTarget = null, Action onFinishedParticle = null)
	{
		Sprite sprite = SpriteManager.GetSprite(spriteName);
		m_icon.sprite = sprite;
		m_countText.text = count;
		m_showParticle = false;
		m_haloAlpha.alpha = 0f;

		if (attractorTarget != null)
		{
			m_showParticle = true;
			m_particleImage.texture = SpriteManager.GetTexture(spriteName);
			m_particleImage.attractorTarget = attractorTarget;
			m_particleImage.rectTransform.localRotation = Quaternion.LookRotation(Vector3.forward, attractorTarget.position);
			m_particleImage.rectTransform.anchoredPosition = Vector2.zero;
			m_particleImage.duration = 0.2f;
            if(float.TryParse(count, out float val))
                m_particleImage.rateOverTime = val / m_particleImage.duration;
			m_particleImage.onLastParticleFinish.AddListener(() => onFinishedParticle?.Invoke());
		}
	}

	public void ShowParticle()
	{
		if (m_animated && m_showParticle)
		{
            Debug.Log(CodeManager.GetMethodName());
			m_particleImage.gameObject.SetActive(true);
			m_particleImage.Play();
		}
	}

	public async UniTask PlayAsync(CancellationToken token, float delay = 0.5f)
	{
		await UniTask.Delay(TimeSpan.FromSeconds(delay));

		await m_haloAlpha
			.DOFade(1f, 0.25f)
			.ToUniTask()
			.SuppressCancellationThrow();

		UniTaskAsyncEnumerable
			.EveryUpdate(PlayerLoopTiming.LastPostLateUpdate) 
			.ForEachAsync(
				_ => {
					if (token.IsCancellationRequested)
					{
						return;
					}

					ObjectUtility.GetRawObject(m_haloAlpha)?.transform.Rotate(Vector3.forward * 0.1f);
				}
			).Forget();
	}
}
