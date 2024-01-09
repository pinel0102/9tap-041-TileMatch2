using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.CasualGame;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using AssetKits.ParticleImage;
using NineTap.Common;

[ResourcePath("UI/Widgets/MissionCollectedFx")]
public class MissionCollectedFx : CachedBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private RectTransform m_moveRectTransform;

	[SerializeField]
	private RectTransform m_rotateRectTransform;

	[SerializeField]
	private UIParticleSystem m_circleEffect;

	[SerializeField]
	private ParticleImage m_particleImage;

    [SerializeField]
	private Image m_image;

	private CancellationTokenSource m_cancellationTokenSource;

	public void OnSetup()
	{
		m_moveRectTransform.Reset();
		m_rotateRectTransform.Reset();
		m_canvasGroup.alpha = 0f;
		m_circleEffect.gameObject.SetActive(true);
		m_cancellationTokenSource = new();
	}

    public void SetImage(string spriteName)
    {
        var sprite = SpriteManager.GetSprite(spriteName);
		m_image.sprite = sprite;
    }

	public void Play(Vector2 startPosition, Vector2 direction, float duration, Action onRelease, float sizeFrom = 70f, float sizeTo = 82f)
	{
		m_cancellationTokenSource?.Cancel();
		DOTween.Kill(m_moveRectTransform, true);
		DOTween.Kill(m_rotateRectTransform, true);

		m_moveRectTransform.SetLocalPosition(startPosition * UIManager.SceneCanvas.scaleFactor);
		m_rotateRectTransform.SetSize(sizeFrom);
		m_rotateRectTransform.SetRotation(Quaternion.identity);
		
		m_canvasGroup.alpha = 1f;

		m_circleEffect.StopParticleEmission();
		m_circleEffect.StartParticleEmission();

		UniTask.Void(
			async token => {
				m_particleImage.gameObject.SetActive(true);
				m_particleImage.Play();
				await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
				await UniTask.Defer(
					() => UniTask.WhenAll(
						UniTask.Create(
							async () => {
								await UniTask.Delay(TimeSpan.FromSeconds(duration - 0.25f));
								m_particleImage.Stop();
							}
						),
						m_rotateRectTransform
							.DOSizeDelta(Vector2.one * sizeTo, duration)
							.SetDelay(0.25f)
							.SetEase(Ease.OutExpo)
							.ToUniTask(),
						m_moveRectTransform
							.DOLocalMove(direction, duration)
							.SetDelay(0.25f)
							.SetEase(Ease.OutExpo)
							.ToUniTask(),
						m_rotateRectTransform
							.DOLocalRotate(Vector3.forward * 360f, duration, RotateMode.FastBeyond360)
							.SetDelay(0.25f)
							.SetEase(Ease.OutExpo)
							.OnComplete(
								() => {
									onRelease?.Invoke();
								}
							)
							.ToUniTask()
					)
				);
			}, 
			m_cancellationTokenSource.Token
		);	
	}

	public void OnRelease()
	{
		m_canvasGroup.alpha = 0f;
		m_particleImage.gameObject.SetActive(false);
		m_cancellationTokenSource?.Cancel();
	}

	private void OnDestroy()
	{
		m_cancellationTokenSource?.Dispose();
	}
}
