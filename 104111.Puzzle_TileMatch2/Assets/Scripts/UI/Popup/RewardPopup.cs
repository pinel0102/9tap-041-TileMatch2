using UnityEngine;

using System;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using NineTap.Constant;
using NineTap.Common;

public enum RewardPopupType
{
    CHEST,
	PRESENT
}

public record RewardPopupParameter
(
	RewardPopupType PopupType, 
	RewardData Reward,
	params HUDType[] VisibleHUD
): UIParameter(VisibleHUD);

[ResourcePath("UI/Popup/RewardPopup")]
public class RewardPopup : UIPopup
{
    [SerializeField]
	private List<AnimatedBox> m_animatedBoxes;

	[SerializeField]
	private AnimatedRewardContainer m_animatedRewardContainer;

	[SerializeField]
	private UITextButton m_confirmButton = default!;

	private RewardPopupType m_popupType;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		m_confirmButton.Alpha = 0f;
		CancellationToken token = this.GetCancellationTokenOnDestroy();

		if (uiParameter is not RewardPopupParameter parameter)
		{
			OnClickClose();
			return;
		}

		m_popupType = parameter.PopupType;

		m_animatedRewardContainer.OnSetup(
			new AnimatedRewardContainerParameter {
				Rewards = parameter.Reward.Rewards,
				OnFinishedAnimation = () => {
					UniTask.Void(
						async () => {
							await UniTask.Delay(
								TimeSpan.FromSeconds(1f), 
								delayTiming: PlayerLoopTiming.LastPostLateUpdate, 
								cancellationToken: token
							);
							OnClickClose();
						}
					);
				}
			}
		);

		m_confirmButton.OnSetup(
			new UITextButtonParameter {
				ButtonText = Text.Button.CLAIM,
				FadeEffect = true,
				OnClick = m_animatedRewardContainer.ShowParticle
			}
		);
	}

	public override void OnShow()
	{
		base.OnShow();

		UniTask.Void(
			async token => {
				foreach (var box in m_animatedBoxes)
				{
					bool matched = box.Type == m_popupType;
					box.CachedGameObject.SetActive(matched);
					if (matched)
					{
						await box.PlayAsync(token);
					}
				}

				await m_animatedRewardContainer.ShowAsync(token);
				await m_confirmButton.ShowAsync(0.5f, token);

			},
			this.GetCancellationTokenOnDestroy()
		);

	}

	public override void OnHide()
	{
		UIManager.DetachAllHUD();
	}
}
