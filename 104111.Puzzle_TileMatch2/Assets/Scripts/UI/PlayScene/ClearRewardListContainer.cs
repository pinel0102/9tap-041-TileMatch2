using UnityEngine;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using NineTap.Common;

public class ClearRewardListContainer : CachedBehaviour
{
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Transform m_parent;

	public float Alpha { set => m_canvasGroup.alpha = value; }

	public void OnSetup(RewardData rewardData)
	{
		m_container.SetActive(rewardData != null);

		var prefab = ResourcePathAttribute.GetResource<RewardGoodsItem>();

		rewardData.Rewards.ForEach(
			reward => {
				var item = Instantiate(prefab);
				item.OnSetup(
					new RewardGoodsItemParameter{
						Animated = false,
						IconSize = 100,
						FontSize = 49
					}
				);
				item.CachedTransform.SetParentReset(m_parent);
				item.UpdateUI(reward.Type.GetIconName(), reward.GetAmountString());
			}
		);
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
