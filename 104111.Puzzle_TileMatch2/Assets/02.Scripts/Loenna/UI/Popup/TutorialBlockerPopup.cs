#nullable enable

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Text = NineTap.Constant.Text;
using NineTap.Common;
using TMPro;
using System.Linq;

public record TutorialBlockerPopupParameter(
    BlockerType BlockerType,
    TileItem TileItem
): DefaultParameterWithoutHUD;

[ResourcePath("UI/Popup/TutorialBlockerPopup")]
public class TutorialBlockerPopup : UIPopup
{
    [Header("★ [Live] Tutorial Blocker")]
    [SerializeField]	private BlockerType m_blockerType;
    [SerializeField]	private int m_popupIndex;
    [SerializeField]	private TileItem m_tileItem = default!;

    [Header("★ [Reference] UI")]
    [SerializeField]	private CanvasGroup m_viewGroup = default!;
    [SerializeField]	private Image m_headLineImage = default!;
    [SerializeField]	private UITextButton m_confirmButton = default!;
    [SerializeField]    private TMP_Text m_unlockedItemText = default!;
    [SerializeField]	private CanvasGroup m_clearStarCanvasGroup = default!;
    [SerializeField]	private CanvasGroup m_clearStarHalo = default!;
    [SerializeField]    private GameObject m_touchLock = default!;
    [SerializeField]	private GameObject m_closeText = default!;

    [Header("★ [Reference] Tutorial")]
    [SerializeField]    private GameObject m_unlockedObject = default!;
    [SerializeField]    private List<GameObject> m_iconObject = default!;
    [SerializeField]    private List<GameObject> m_tutorialObject = default!;
    [SerializeField]    private List<CanvasGroup> m_tutorialCanvasGroup = default!;
    [SerializeField]    private List<RectTransform> m_tutorialPanel = default!;
    [SerializeField]    private List<RectTransform> m_handPosition = default!;
    [SerializeField]    private Button m_jellyCloseButton = default!;
    
    [Header("★ [Reference] Unmask")]
    [SerializeField]    private List<GameObject> m_unmaskObject = default!;
    [SerializeField]    private List<RectTransform> m_unmaskTile = default!;
    [SerializeField]    private List<GameObject> m_unmaskBushAroundTiles = default!;
    [SerializeField]    private List<GameObject> m_unmaskChainLeftRightTiles = default!;
    
    private bool isButtonInteractable;
    private const string FormatUnlockText = "You have unlocked \nthe <color=#ffc324>{0}</color> Item!";

    public override void OnSetup(UIParameter uiParameter)
    {
        base.OnSetup(uiParameter);

		if (uiParameter is not TutorialBlockerPopupParameter parameter)
		{
			OnClickClose();
			return;
		}

        m_tileItem = parameter.TileItem;
        m_blockerType = parameter.BlockerType;
        m_popupIndex = GetPopupIndex(m_blockerType);
        
        m_unlockedItemText.SetText(string.Format(FormatUnlockText, GlobalDefine.GetBlockerName(m_blockerType)));
        
        SetButtonInteractable(false);
        
        m_confirmButton.OnSetup(
			new UITextButtonParameter {
				ButtonText = Text.Button.CONTINUE,
				OnClick = () => {
                    if (isButtonInteractable)
                    {
					    OpenTutorial(m_popupIndex);
                    }
				}
			}
		);

        m_jellyCloseButton.onClick.RemoveAllListeners();
        m_jellyCloseButton.onClick.AddListener(OnClickClose);
        m_jellyCloseButton.interactable = false;

        m_clearStarCanvasGroup.alpha = 0;
        m_confirmButton.Alpha = 0f;
		m_headLineImage.transform.localScale = Vector3.zero;
        m_unlockedObject.SetActive(true);
        m_closeText.SetActive(false);

        m_unmaskObject.ForEach(unmask => unmask.SetActive(false));
        m_unmaskTile.ForEach(unmask => unmask.gameObject.SetActive(false));
        m_unmaskBushAroundTiles.ForEach(unmask => unmask.SetActive(false));
        m_unmaskChainLeftRightTiles.ForEach(unmask => unmask.SetActive(false));
        m_tutorialObject.ForEach(tutorial => tutorial.SetActive(false));
        m_iconObject.ForEach(icon => icon.SetActive(false));
        m_iconObject[m_popupIndex].SetActive(true);
        m_unmaskObject[m_popupIndex].SetActive(true);

        m_viewGroup.alpha = 1f;
    }

    public override void OnShow()
    {
        base.OnShow();

        UniTask.Void(
			async token => {
                SoundManager soundManager = Game.Inst.Get<SoundManager>();
                soundManager?.PlayFx(Constant.Sound.SFX_REWARD_OPEN);

                await m_headLineImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

                await PlayHaloAsync(m_clearStarHalo, m_clearStarCanvasGroup, token);
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

                await ShowContinueButton();
			},
			this.GetCancellationTokenOnDestroy()
		);
    }


#region Tutorial

    private int GetPopupIndex(BlockerType blockerType)
    {
        return blockerType switch{
            BlockerType.Glue_Left or BlockerType.Glue_Right => 0,
            BlockerType.Bush => 1,
            BlockerType.Suitcase => 2,
            BlockerType.Jelly => 3,
            BlockerType.Chain => 4,
            _ => 0
        };
    }

    private void OpenTutorial(int index)
    {
        m_viewGroup.alpha = 1f;

        SetPanelPosition(index);

        m_unlockedObject.SetActive(false);
        m_tutorialObject[index].SetActive(true);

        UniTask.Void(
            async () =>
            {
                await UniTask.WaitUntil(
                    () => m_tutorialObject[index].activeSelf
                );

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

                SetButtonInteractable(true);
            }
        );
    }

    private void SetPanelPosition(int index)
    {
        Vector3 targetPosition = m_tileItem.transform.position;
        List<TileItem> targetTiles = new List<TileItem>
        {
            m_tileItem
        };

        switch (index)
        {
            case 0: // Glue
                switch(m_blockerType)
                {
                    case BlockerType.Glue_Left:
                        var(glueRight, glueRightTile) = m_tileItem.FindRightTile();
                        targetPosition = glueRight ? (targetPosition + glueRightTile.transform.position) * 0.5f : targetPosition;
                        if (glueRight) targetTiles.Add(glueRightTile);
                        break;
                    case BlockerType.Glue_Right:
                        var(glueLeft, glueLeftTile) = m_tileItem.FindLeftTile();
                        targetPosition = glueLeft ? (targetPosition + glueLeftTile.transform.position) * 0.5f : targetPosition;
                        if (glueLeft) targetTiles.Add(glueLeftTile);
                        break;
                }

                m_tutorialPanel[index].position = targetPosition;

                m_unmaskTile[index].position = targetPosition;
                m_unmaskTile[index].gameObject.SetActive(true);

                WaitForMoveTile(targetTiles, OnClickClose);
                break;
            case 1: // Bush
                m_tutorialPanel[index].position = targetPosition;

                var(bushLeft, bushLeftTile) = m_tileItem.FindLeftTile();
                var(bushRight, bushRightTile) = m_tileItem.FindRightTile();
                var(bushTop, bushTopTile) = m_tileItem.FindTopTile();
                var(bushbottom, bushBottomTile) = m_tileItem.FindBottomTile();

                if (bushTop) targetTiles.Add(bushTopTile);
                if (bushLeft) targetTiles.Add(bushLeftTile);
                if (bushbottom) targetTiles.Add(bushBottomTile);
                if (bushRight) targetTiles.Add(bushRightTile);

                m_handPosition[index].position = targetTiles.Last().transform.position;
                
                m_unmaskBushAroundTiles[0].SetActive(bushLeft && bushLeftTile.IsInteractable);
                m_unmaskBushAroundTiles[1].SetActive(bushRight && bushRightTile.IsInteractable);
                m_unmaskBushAroundTiles[2].SetActive(bushTop && bushTopTile.IsInteractable);
                m_unmaskBushAroundTiles[3].SetActive(bushbottom && bushBottomTile.IsInteractable);
                
                m_unmaskTile[index].position = targetPosition;
                m_unmaskTile[index].gameObject.SetActive(true);

                WaitForMoveTile(targetTiles, OnClickClose);
                break;
            case 2: // Suitcase
                m_tutorialPanel[index].position = targetPosition;
                m_unmaskTile[index].position = targetPosition;
                m_unmaskTile[index].gameObject.SetActive(true);

                WaitForMoveTile(targetTiles, OnClickClose);
                break;
            case 3: // Jelly
                m_jellyCloseButton.interactable = false;

                m_tutorialPanel[index].position = targetPosition;
                m_unmaskTile[index].position = targetPosition;
                m_unmaskTile[index].gameObject.SetActive(true);
                
                UniTask.Void(async() => {
                    await UniTask.WaitForSeconds(2f);
                    m_jellyCloseButton.interactable = true;
                    m_closeText.SetActive(true);
                });
                
                break;
            case 4 : // Chain
                m_tutorialPanel[index].position = targetPosition;

                var(chainLeft, chainLeftTile) = m_tileItem.FindLeftTile();
                var(chainRight, chainRightTile) = m_tileItem.FindRightTile();

                if (chainLeft) targetTiles.Add(chainLeftTile);
                if (chainRight) targetTiles.Add(chainRightTile);

                m_handPosition[index].position = targetTiles.Last().transform.position;

                m_unmaskChainLeftRightTiles[0].SetActive(chainLeft && chainLeftTile.IsInteractable);
                m_unmaskChainLeftRightTiles[1].SetActive(chainRight && chainRightTile.IsInteractable);

                m_unmaskTile[index].position = targetPosition;
                m_unmaskTile[index].gameObject.SetActive(true);

                WaitForMoveTile(targetTiles, OnClickClose);
                break;
        }
    }

    private void WaitForMoveTile(List<TileItem> target, Action onComplete)
    {
        UniTask.Void(
            async () =>
            {
                await UniTask.WaitUntil(
                    () => target.Any(tile => tile.IsMoving)
                );

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

                onComplete?.Invoke();
            }
        );
    }

#endregion Tutorial


#region ETC

    private void SetButtonInteractable(bool interactable)
    {
        isButtonInteractable = interactable;
        m_touchLock.SetActive(!isButtonInteractable);
        //Debug.Log(CodeManager.GetMethodName() + isButtonInteractable);
    }

    private async UniTask ShowContinueButton()
    {
        await DOTween.To(
            getter: () => 0f,
            setter: alpha => {
                if (!ObjectUtility.IsNullOrDestroyed(m_confirmButton))
                {
                    m_confirmButton.Alpha = alpha;
                }
            },
            endValue: 1f,
            0.1f
        )
        .ToUniTask()
        .SuppressCancellationThrow();

        SetButtonInteractable(true);
    }

    private async UniTask PlayHaloAsync(CanvasGroup halo, CanvasGroup container, CancellationToken token)
	{
		await container
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

					ObjectUtility.GetRawObject(halo)?.transform.Rotate(Vector3.forward * 0.1f);
				}
			).Forget();
	}

#endregion ETC

}
