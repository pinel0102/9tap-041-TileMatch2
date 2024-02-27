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
	PRESENT,
    SWEETHOLIC
}

public record RewardPopupParameter
(
	RewardPopupType PopupType, 
	RewardData Reward,
    int NewLandmark,
    bool isADBlockProduct,
    Action OnComplete,
	params HUDType[] VisibleHUD
): UIParameter(VisibleHUD);

public record EventRewardPopupParameter
(
	RewardPopupType PopupType, 
    ChestType ChestType,
	List<IReward> Rewards,
    Action OnComplete,
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
    public int rewardCoin;
    public bool m_showBlank;

    private Action onComplete;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		m_confirmButton.Alpha = 0f;
		CancellationToken token = this.GetCancellationTokenOnDestroy();

		if (uiParameter is RewardPopupParameter parameter)
		{
            m_popupType = parameter.PopupType;
            m_showBlank = false;
            onComplete = parameter.OnComplete;

            switch(m_popupType)
            {
                case RewardPopupType.CHEST:
                    SetupChest(parameter, token);
                    break;
                case RewardPopupType.PRESENT:
                    SetupPresent(parameter, token);
                    break;
                default:
                    Debug.Log(CodeManager.GetMethodName() + m_popupType);
                    break;
            }
		}
        else if (uiParameter is EventRewardPopupParameter eventParameter)
		{
            m_popupType = eventParameter.PopupType;
            m_showBlank = false;
            onComplete = eventParameter.OnComplete;

            switch(m_popupType)
            {
                case RewardPopupType.SWEETHOLIC:
                    m_showBlank = eventParameter.ChestType == ChestType.Default;
                    SetupEvent(eventParameter, token);
                    break;
                default:
                    Debug.Log(CodeManager.GetMethodName() + m_popupType);
                    break;
            }
		}
        else
        {
            OnClickClose();
			return;
        }
	}

#region 일반 보상.

    private void SetupChest(RewardPopupParameter parameter, CancellationToken token)
    {
        rewardCoin = parameter.Reward.Coin;
        GlobalData.Instance.HUD?.behaviour.Fields[2].SetIncreaseText(GlobalData.Instance.oldCoin);

        m_animatedRewardContainer.OnSetup(
			new AnimatedRewardContainerParameter {
                PopupType = parameter.PopupType,
				Rewards = parameter.Reward.Rewards,
                NewLandmark = parameter.NewLandmark,
                IsADBlockProduct = parameter.isADBlockProduct,
                ShowBlank = false,
				OnFinishedAnimation = () => {
					UniTask.Void(
						async () => {
                            if (rewardCoin > 0)
                            {
                                GlobalData.Instance.soundManager?.PlayFx(Constant.Sound.SFX_GOLD_PIECE);
                                GlobalData.Instance.HUD_LateUpdate_Coin(rewardCoin, autoTurnOff_IncreaseMode:false);
                            }
							
                            await UniTask.Delay(
								TimeSpan.FromSeconds(1.5f), 
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
				OnClick = () => { 
                    m_confirmButton.interactable = false;
                    m_confirmButton.Alpha = 0f;
                    m_animatedRewardContainer.ShowParticle(OnClickClose);
                }
			}
		);
    }

    private void SetupPresent(RewardPopupParameter parameter, CancellationToken token)
    {
        m_animatedRewardContainer.OnSetup(
			new AnimatedRewardContainerParameter {
                PopupType = parameter.PopupType,
				Rewards = parameter.Reward.Rewards,
                NewLandmark = parameter.NewLandmark,
                IsADBlockProduct = parameter.isADBlockProduct,
                ShowBlank = false,
				OnFinishedAnimation = null
			}
		);

		m_confirmButton.OnSetup(
			new UITextButtonParameter {
				ButtonText = Text.Button.CLAIM,
				FadeEffect = true,
				OnClick = () => { 
                    m_confirmButton.interactable = false;
                    m_confirmButton.Alpha = 0f;
                    OnClickClose();
                }
			}
		);
    }

#endregion 일반 보상.


#region 이벤트 보상.

    private void SetupEvent(EventRewardPopupParameter parameter, CancellationToken token)
    {
        m_animatedRewardContainer.OnSetup(
			new AnimatedRewardContainerParameter {
                PopupType = parameter.PopupType,
				Rewards = parameter.Rewards,
                NewLandmark = 0,
                IsADBlockProduct = false,
                ShowBlank = m_showBlank,
				OnFinishedAnimation = null
			}
		);

		m_confirmButton.OnSetup(
			new UITextButtonParameter {
				ButtonText = Text.Button.CLAIM,
				FadeEffect = true,
				OnClick = () => { 
                    m_confirmButton.interactable = false;
                    m_confirmButton.Alpha = 0f;
                    OnClickClose();
                }
			}
		);
    }

#endregion 이벤트 보상.


	public override void OnShow()
	{
		base.OnShow();

        if (m_showBlank)
        {
            UniTask.Void(
                async token => {
                    foreach (var box in m_animatedBoxes)
                    {
                        bool matched = box.Type == m_popupType;
                        box.CachedGameObject.SetActive(matched);
                        if (matched)
                        {
                            box.HideImage();
                        }
                    }

                    await m_confirmButton.ShowAsync(0.5f, token);
                },
                this.GetCancellationTokenOnDestroy()
            );
        }
        else
        {
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
	}

	public override void OnHide()
	{
		base.OnHide();
        GlobalData.Instance.HUD_Preferred();
	}

    public override void OnClickClose()
    {
        base.OnClickClose();

        onComplete?.Invoke();
        GlobalData.Instance.HUD?.behaviour.Fields[2].SetIncreaseMode(false);
    }
}
