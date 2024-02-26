using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using DG.Tweening;

using NineTap.Common;

public class AnimatedRewardContainerParameter
{
    public RewardPopupType PopupType;
	public List<IReward> Rewards = new();
    public int NewLandmark;
    public bool IsADBlockProduct;
    public bool ShowBlank;
	public Action OnFinishedAnimation;
}

public class AnimatedRewardContainer : CachedBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;
    [SerializeField]
	private List<RectTransform> m_layoutTransform = default;
    private RewardPopupType m_popupType;

	private List<RewardGoodsItem> m_rewardGoodsItems = new();
    private RewardGoodsItem prefab;
    private int m_animationCount;
    private bool m_showBlank;

	public float Alpha { set => m_canvasGroup.alpha = value; }

	public void OnSetup(AnimatedRewardContainerParameter parameter)
	{
        prefab = ResourcePathAttribute.GetResource<RewardGoodsItem>();
		m_rewardGoodsItems.Clear();
        m_animationCount = 0;
        m_canvasGroup.alpha = 0f;
        m_popupType = parameter.PopupType;
        m_showBlank = parameter.ShowBlank;

        int rewardCount = parameter.Rewards.Count + (parameter.NewLandmark > 0 ? 1 : 0);
        int currentCount = 0;

        if (m_showBlank)
            return;

        for(int i=0; i < parameter.Rewards.Count; i++)
        {
            m_rewardGoodsItems.Add(CreateRewadItem(ref currentCount, i, rewardCount, parameter.Rewards[i], parameter.OnFinishedAnimation));
        }

        if (parameter.NewLandmark > 0)
        {
            if(GlobalData.Instance.tableManager.PuzzleDataTable.TryGetValue(parameter.NewLandmark, out PuzzleData puzzleData))
            {
                m_rewardGoodsItems.Add(CreateRewadItem(ref currentCount, parameter.Rewards.Count, rewardCount, 
                    new LandmarkReward(ProductType.Landmark, puzzleData.Name), parameter.OnFinishedAnimation));
            }
        }

        if (parameter.IsADBlockProduct)
        {
            m_rewardGoodsItems.Add(CreateRewadItem(ref currentCount, parameter.Rewards.Count, rewardCount, 
                new ADBlockReward(ProductType.ADBlock), parameter.OnFinishedAnimation));
        }
	}

    RewardGoodsItem CreateRewadItem(ref int currentCount, int index, int rewardCount, IReward reward, Action OnFinishedAnimation)
    {
        GameObject go = new GameObject($"Position {index}");
        LayoutElement layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.minWidth = 120;
        layoutElement.minHeight = 180;

        switch(rewardCount)
        {
            case <= 3: 
                go.transform.SetParentReset(m_layoutTransform[0]);
                break;
            case 4:
                go.transform.SetParentReset(m_layoutTransform[currentCount < 2 ? 0 : 1]);
                break;
            default:
                go.transform.SetParentReset(m_layoutTransform[currentCount < 3 ? 0 : 1]);
                break;
        }

        currentCount++;

        LayoutRebuilder.ForceRebuildLayoutImmediate(CachedRectTransform);

        var item = Instantiate(prefab);
        item.OnSetup(
            new RewardGoodsItemParameter{
                Animated = true,
                IconSize = 114,
                FontSize = reward.Type == ProductType.Landmark ? 40 : 59
            }
        );

        item.CachedTransform.SetParentReset(go.transform);

        Transform target = UIManager.HUD.GetAttractorTarget(reward.Type.GetHUDType());
        if (target != null) 
            m_animationCount++;

        item.UpdateUI(
            reward.Type.GetIconName(), 
            reward.GetAmountString(),
            target,
            OnFinishedAnimation
        );
        item.transform.localScale = Vector2.zero;

        return item;
    }

	public async UniTask ShowAsync(CancellationToken token)
	{
		Vector2 from = CachedTransform.parent.position;
		m_rewardGoodsItems.ForEach(item => item.CachedTransform.position = from);

		await DOTween.To(
			getter: () => 0f,
			setter: alpha => Alpha = alpha,
			endValue: 1f,
			0.1f
		)
		.SetEase(Ease.OutQuad)
		.ToUniTask()
		.AttachExternalCancellation(token);

        var tasks = m_rewardGoodsItems.Select(
            item => UniTask.WhenAll(
                item.CachedRectTransform
                    .DOAnchorPos(Vector2.zero, 0.3f)
                    .SetEase(Ease.OutQuad)
                    .ToUniTask(),
                item.CachedTransform
                    .DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .ToUniTask(),
                UniTask.Create(
                    () => UniTask.WhenAll(
                        UniTask.Delay(TimeSpan.FromSeconds(0.5f)),
                        item.PlayAsync(token, 0.5f)
                    )
                )
            )
        ).ToUniTaskAsyncEnumerable();

        switch(m_popupType)
        {
            case RewardPopupType.CHEST:
                await UniTaskAsyncEnumerable.ForEachAwaitAsync(tasks, task => task, token).AttachExternalCancellation(token);
                break;
            case RewardPopupType.PRESENT:
            default:
                await tasks.ToListAsync(token).AsUniTask();
                break;
        }
	}

	public void ShowParticle(Action onNoAnimation)
	{
        if (m_animationCount > 0)
            m_rewardGoodsItems.ForEach(item => item.ShowParticle());
        else
            onNoAnimation.Invoke();
	}
}
