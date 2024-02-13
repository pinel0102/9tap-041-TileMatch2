using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineTap.Common;
using System;

public record DailyRewardPopupParameter
(
	string DailyRewardDate,
    int DailyRewardIndex,
    Action PopupCloseCallback,
    params HUDType[] VisibleHUD
) : UIParameter(VisibleHUD);

[ResourcePath("UI/Popup/DailyRewardPopup")]
public class DailyRewardPopup : UIPopup
{
    [Header("★ [Live] Daily Reward")]
    [SerializeField] private int dailyRewardIndex;

    [Header("★ [Reference] Daily Reward Popup")]
    [SerializeField] private UITextButton getRewardButton;
    [SerializeField] private UITextButton getDoubleButton;
    [SerializeField] private GameObject rewardVideoOnlyObject;
    [SerializeField] private UIImageButton closeButton;
    [SerializeField] private GameObject m_touchLock;

    [Header("★ [Reference] Daily Reward UI")]
    [SerializeField] private List<DailyRewardItem> dailyItems;
    [SerializeField] private List<Transform> lastItemsParent;

    private bool isButtonInteractable;
    private Action m_popupCloseCallback;

    private IconWidget adWidget;
    private List<RewardData> rewardDataList = default;
    private GlobalData globalData { get { return GlobalData.Instance; } }

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not DailyRewardPopupParameter parameter)
		{
            OnClickClose();
			return;
		}

        m_popupCloseCallback = parameter.PopupCloseCallback;

        RewardDataTable rewardDataTable = globalData.tableManager.RewardDataTable;
        rewardDataList = rewardDataTable.GetDailyRewards();
        dailyRewardIndex = parameter.DailyRewardIndex;
        
        Debug.Log(CodeManager.GetMethodName() + string.Format("dailyRewardIndex : {0}", dailyRewardIndex));

        SetupButtons();
        SetupRewards();

        RefreshUI(!GlobalDefine.IsEnable_DailyRewards());
        
        SetButtonInteractable(true);
	}

    private void SetupButtons()
    {
        getRewardButton.OnSetup(new UITextButtonParameter {
            ButtonText = "Claim",
            OnClick = OnClick_GetDailyReward
        });

        getDoubleButton.OnSetup(new UITextButtonParameter {
            ButtonText = "Double",
            SubWidgetBuilder = () => {
					adWidget = Instantiate(ResourcePathAttribute.GetResource<IconWidget>());
					adWidget.OnSetup("UI_Icon_AD");
					return adWidget.CachedGameObject;
				},
            OnClick = OnClick_RewardVideo
        });

        closeButton.OnSetup(new UIImageButtonParameter{
            OnClick = OnClickClose
        });
    }

    private void SetupRewards()
    {
        var prefab = ResourcePathAttribute.GetResource<IconDailyReward>();

        for(int i=0; i < rewardDataList.Count; i++)
        {
            dailyItems[i].Initialize(i+1);

            RewardData rewardData = rewardDataList[i];
            
            if (i < rewardDataList.Count - 1)
            {   
                var widget = Instantiate(prefab);

                if (rewardData.Coin > 0)
                    widget.OnSetup("UI_Shop_Icon_Coins", $"x{rewardData.Coin}");
                else if (rewardData.UndoItem > 0)
                    widget.OnSetup("UI_Shop_Icon_Undo", $"x{rewardData.UndoItem}");
                else if (rewardData.ShuffleItem > 0)
                    widget.OnSetup("UI_Shop_Icon_Shuffle", $"x{rewardData.ShuffleItem}");
                else if (rewardData.StashItem > 0)
                    widget.OnSetup("UI_Shop_Icon_Hint", $"x{rewardData.StashItem}");
                else if (rewardData.HeartBooster > 0)
                    widget.OnSetup("UI_Shop_Icon_Heart", $"{rewardData.HeartBooster}m");
                else continue;

                widget.CachedTransform.SetParentReset(dailyItems[i].IconParent);
            }
            else
            {
                if (rewardData.Coin > 0)
                {
                    var widget = Instantiate(prefab);
                    widget.OnSetup("UI_Shop_Icon_Coins", $"x{rewardData.Coin}");
                    widget.CachedTransform.SetParentReset(lastItemsParent[0]);
                }
                if (rewardData.UndoItem > 0)
                {
                    var widget = Instantiate(prefab);
                    widget.OnSetup("UI_Shop_Icon_Undo", $"x{rewardData.UndoItem}");
                    widget.CachedTransform.SetParentReset(lastItemsParent[1]);
                }
                if (rewardData.ShuffleItem > 0)
                {
                    var widget = Instantiate(prefab);
                    widget.OnSetup("UI_Shop_Icon_Shuffle", $"x{rewardData.ShuffleItem}");
                    widget.CachedTransform.SetParentReset(lastItemsParent[2]);
                }
                if (rewardData.HeartBooster > 0)
                {
                    var widget = Instantiate(prefab);
                    widget.OnSetup("UI_Shop_Icon_Heart", $"{rewardData.HeartBooster}m");
                    widget.CachedTransform.SetParentReset(lastItemsParent[3]);
                }
            }
        }
    }

    private void RefreshUI(bool isGetReward)
    {
        //closeButton.gameObject.SetActive(isGetReward);
        getRewardButton.SetInteractable(!isGetReward);
        getDoubleButton.SetInteractable(!isGetReward);
        adWidget?.SetInteractable(!isGetReward);

#if UNITY_EDITOR
        rewardVideoOnlyObject.SetActive(true);
#else
        rewardVideoOnlyObject.SetActive(GlobalDefine.IsRewardVideoReady());
#endif

        for(int i=0; i < dailyItems.Count; i++)
        {
            bool isToday = i == dailyRewardIndex;

            dailyItems[i].Today.SetActive(isToday);
            dailyItems[i].Box.SetActive(i > dailyRewardIndex || (isToday && !isGetReward));
            dailyItems[i].AlreadyGet.SetActive(i < dailyRewardIndex || (isToday && isGetReward));
        }
    }

    private void OnClick_GetDailyReward()
    {
        if (!isButtonInteractable || !GlobalDefine.IsEnable_DailyRewards())
            return;

        GetDailyReward(1);
    }

    private void OnClick_RewardVideo()
    {
#if !UNITY_EDITOR
        if (!isButtonInteractable || !GlobalDefine.IsEnable_DailyRewards() || !GlobalDefine.IsRewardVideoReady())
            return;
#endif

        GlobalDefine.RequestAD_RewardVideo(0, (success) => {
            if (success)
                GetDailyReward(2);
        });
    }

    private void GetDailyReward(int multiplier)
    {
        Debug.Log(CodeManager.GetMethodName() + multiplier);

        SetButtonInteractable(false);

        RewardData rewardData = rewardDataList[dailyRewardIndex];
        Dictionary<ProductType, long> collectRewardAll = new Dictionary<ProductType, long>();

        for(int i=0; i < multiplier; i++)
            GlobalDefine.AddRewards(collectRewardAll, rewardData.Rewards);

        GlobalDefine.UpdateRewards(collectRewardAll);
        GlobalDefine.UpdateDailyReward(GetNextIndex(dailyRewardIndex));
        
        OpenPresentPopup(rewardData, multiplier);
    }

    private int GetNextIndex(int currentIndex)
    {
        return currentIndex < rewardDataList.Count - 1 ? currentIndex + 1 : 0;
    }

    private void OpenPresentPopup(RewardData rewardData, int multiplier)
    {
        if (multiplier == 1)
        {
            globalData.ShowPresentPopup(rewardData, () => RefreshUI(true));
        }
        else
        {
            RewardData newRewardData = rewardData with {
                Coin = rewardData.Coin * multiplier,
                UndoItem = rewardData.UndoItem * multiplier,
                StashItem = rewardData.StashItem * multiplier,
                ShuffleItem = rewardData.ShuffleItem * multiplier,
                HeartBooster = rewardData.HeartBooster * multiplier,
            };

            newRewardData.ResetRewards();
            newRewardData.CreateRewards();

            globalData.ShowPresentPopup(newRewardData, () => RefreshUI(true));
        }

        //RefreshUI(true);
        SetButtonInteractable(true);
    }

    public override void OnShow()
    {
        base.OnShow();
    }

    public override void OnClickClose()
	{
        base.OnClickClose();

        m_popupCloseCallback?.Invoke();
	}

    private void SetButtonInteractable(bool interactable)
    {
        isButtonInteractable = interactable;
        m_touchLock.SetActive(!isButtonInteractable);
    }
}