using UnityEngine;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using NineTap.Common;

public class ClearRewardListContainer : CachedBehaviour
{
	[SerializeField]	private GameObject m_container;
	[SerializeField]	private CanvasGroup m_canvasGroup;
	[SerializeField]	private Transform m_parent;
    private RewardGoodsItem prefab;
    private GlobalData globalData { get { return GlobalData.Instance; } }
	public float Alpha { set => m_canvasGroup.alpha = value; }

	public void OnSetup(RewardData rewardData)
	{
		m_container.SetActive(rewardData != null);

		prefab = ResourcePathAttribute.GetResource<RewardGoodsItem>();

		rewardData.Rewards.ForEach(
			reward => {
                if (reward.Type != ProductType.PuzzlePiece)
                {
                    CreateRewardIcon(reward.Type.GetIconName(), reward.GetAmountString());
                }
			}
		);

        if(globalData.eventSweetHolic_Activate)
        {
            if(globalData.eventSweetHolic_GetCount > 0)
            {
                CreateRewardIcon(GlobalDefine.GetSweetHolic_ItemImagePath(), globalData.eventSweetHolic_GetCount.ToString(), 130);
            }
        }
	}

    private void CreateRewardIcon(string spriteName, string count, int iconSize = 100)
    {
        var item = Instantiate(prefab);
        item.OnSetup(
            new RewardGoodsItemParameter{
                Animated = false,
                IconSize = iconSize,
                FontSize = 48
            }
        );
        item.CachedTransform.SetParentReset(m_parent);
        item.UpdateUI(spriteName, count);
    }

	public UniTask ShowAsync()
	{
		if (!m_container.activeSelf)
		{
			return UniTask.CompletedTask;
		}

		return DOTween.To(
			getter: () => 0f,
			setter: alpha => Alpha = alpha,
			endValue: 1f,
			0.25f
		)
		.ToUniTask()
		.SuppressCancellationThrow();
	}
}
