using UnityEngine;
using UnityEngine.UI;

using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using NineTap.Common;

public class AnimatedBox : CachedBehaviour
{
	[SerializeField]
	private RewardPopupType m_type;

	[SerializeField]
	private Graphic m_lid;

	[SerializeField]
	private RectTransform m_box;

    [SerializeField]
	private CanvasGroup m_canvasGroup;

	public RewardPopupType Type => m_type;

	public async UniTask PlayAsync(CancellationToken token)
	{
        m_canvasGroup.alpha = 1;

		CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, this.GetCancellationTokenOnDestroy());
		CancellationToken cancellationToken = cancellationTokenSource.Token;

		CachedRectTransform.localScale = Vector2.one * 0.8f;
		await CachedRectTransform
			.DOScale(1f, 0.25f)
			.SetEase(Ease.InOutBack)
			.ToUniTask()
			.AttachExternalCancellation(cancellationToken);

        await UniTask.WhenAll(
			m_lid.rectTransform.DOAnchorPosY(200f, 0.5f).SetEase(Ease.OutQuad).ToUniTask(),
			UniTask.Create(
					async () => {
                    SoundManager soundManager = Game.Inst?.Get<SoundManager>();
                    soundManager?.PlayFx(Constant.Sound.SFX_REWARD_OPEN);
					
                    await UniTask.Delay(TimeSpan.FromSeconds(0.25f));
					await m_lid.DOFade(0f, 0.25f).SetEase(Ease.OutQuad);
				}
			)
		).AttachExternalCancellation(cancellationToken);
	}

    public void HideImage()
    {
        m_canvasGroup.alpha = 0;

        SoundManager soundManager = Game.Inst?.Get<SoundManager>();
        soundManager?.PlayFx(Constant.Sound.SFX_REWARD_OPEN);
    }
}
