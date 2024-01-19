#nullable enable

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Text = NineTap.Constant.Text;
using NineTap.Common;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Coffee.UIExtensions;

public record TutorialPlayPopupParameter(
    int Level,
    int TutorialIndex,
    int ItemIndex
): DefaultParameterWithoutHUD;

[ResourcePath("UI/Popup/TutorialPlayPopup")]
public class TutorialPlayPopup : UIPopup
{
    [SerializeField]
	private Image m_headLineImage = default!;

    [SerializeField]
	private UITextButton m_confirmButton = default!;

    [SerializeField]
    private Image m_unlockedItemImage = default!;
    
    [SerializeField]
    private TMP_Text m_unlockedItemText = default!;

    [SerializeField]
	private CanvasGroup m_clearStarCanvasGroup = default!;

    [SerializeField]
	private CanvasGroup m_clearStarHalo = default!;

    [SerializeField]
    private RectTransform m_unmask = default!;

    [SerializeField]
    private GameObject m_unlockedObject = default!;

    [SerializeField]
    private List<GameObject> m_tutorialObject = default!;

    [SerializeField]
    private List<CanvasGroup> m_tutorialCanvasGroup = default!;

    [SerializeField]
    private List<RectTransform> m_tutorialPanel = default!;

    [SerializeField]
    private List<TransitionEffect> m_tutorialEffect = default!;

    private PlaySceneBottomUIView bottomView => GlobalData.Instance.playScene.bottomView;

    public int Level;
    public int TutorialIndex;
    public int ItemIndex;
    public bool itemExists;
    public bool isButtonInteractable;

    public override void OnSetup(UIParameter uiParameter)
    {
        base.OnSetup(uiParameter);

		if (uiParameter is not TutorialPlayPopupParameter parameter)
		{
			OnClickClose();
			return;
		}
        
        Level = parameter.Level;
        TutorialIndex = parameter.TutorialIndex;
        ItemIndex = parameter.ItemIndex;

        SetButtonInteractable(false);
        
        m_confirmButton.OnSetup(
			new UITextButtonParameter {
				ButtonText = Text.Button.CONTINUE,
				OnClick = () => {
                    if (isButtonInteractable)
                    {
					    OpenTutorial(TutorialIndex);
                    }
				}
			}
		);

        m_clearStarCanvasGroup.alpha = 0;
        m_confirmButton.Alpha = 0f;
		m_headLineImage.transform.localScale = Vector3.zero;
        m_unmask.gameObject.SetActive(false);

        ItemDataTable m_itemDataTable = GlobalData.Instance.tableManager.ItemDataTable;
        if (m_itemDataTable.TryGetValue(ItemIndex, out var itemData))
        {
            itemExists = true;
            m_unlockedItemImage.sprite = SpriteManager.GetSprite(itemData.ImagePath);
            m_unlockedItemText.SetText(itemData.Name);

            m_unlockedObject.SetActive(true);
            for(int i=0; i < m_tutorialObject.Count; i++)
            {
                m_tutorialObject[i].SetActive(false);
            }
        }
        else
        {
            itemExists = false;

            m_unlockedObject.SetActive(false);
            for(int i=0; i < m_tutorialObject.Count; i++)
            {
                m_tutorialObject[i].SetActive(false);
            }
        }
    }

    public override void OnShow()
    {
        base.OnShow();

        UniTask.Void(
			async token => {                
                if(itemExists)
                {
                    SoundManager soundManager = Game.Inst.Get<SoundManager>();
                    soundManager?.PlayFx(Constant.Sound.SFX_REWARD_OPEN);

                    await m_headLineImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

                    await PlayHaloAsync(m_clearStarHalo, m_clearStarCanvasGroup, token);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

                    await ShowContinueButton();
                }
                else
                {
                    SetButtonInteractable(true);
                    OpenTutorial(TutorialIndex);
                }
			},
			this.GetCancellationTokenOnDestroy()
		);
    }

    private void OpenTutorial(int index)
    {
        SetPanelPosition(index);

        m_unlockedObject.SetActive(false);
        m_tutorialObject[index].SetActive(true);

        switch(m_tutorialEffect[index])
        {
            case TransitionEffect.FADE:
                m_tutorialCanvasGroup[index].alpha = 0f;
                break;
            case TransitionEffect.SCALE:
                m_tutorialObject[index].transform.localScale = Vector2.one * 0.1f;
                break;
        }

        UniTask.Void(
            async () =>
            {
                await UniTask.WaitUntil(
                    () => m_tutorialObject[index].activeSelf
                );

                switch (m_tutorialEffect[index])
                {
                    case TransitionEffect.FADE:
                        await m_tutorialCanvasGroup[index]
                            .DOFade(1f, 0.25f)
                            .ToUniTask()
                            .AttachExternalCancellation(this.GetCancellationTokenOnDestroy())
                            .SuppressCancellationThrow();
                        break;
                    case TransitionEffect.SCALE:
                        bool canceled = await m_tutorialObject[index].transform
                            .DOScale(Vector3.one, 0.25f)
                            .SetEase(Ease.OutBack)
                            .ToUniTask()
                            .AttachExternalCancellation(this.GetCancellationTokenOnDestroy())
                            .SuppressCancellationThrow();

                        if (!canceled && Background != null)
                        {
                            Background.transform.localScale = Vector3.one;
                        }
                        break;
                }

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }
        );
    }

    private void SetPanelPosition(int index)
    {
        switch (index)
        {
            case 1: // Undo
                m_tutorialPanel[index].position = bottomView.ButtonsView.m_undoButton.transform.position;
                break;
            case 2: // Return
                m_tutorialPanel[index].position = bottomView.ButtonsView.m_stashButton.transform.position;
                m_unmask.position = bottomView.BasketView.GetBasketAnchorPosition();
                m_unmask.gameObject.SetActive(true);                
                break;
            case 3: // Shuffle
                m_tutorialPanel[index].position = bottomView.ButtonsView.m_shuffleButton.transform.position;
                break;
            default:
                break;
        }
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

    private void SetButtonInteractable(bool interactable)
    {
        isButtonInteractable = interactable;
        Debug.Log(CodeManager.GetMethodName() + isButtonInteractable);
    }

    public void OnClick_TutorialClose(int index)
    {
        switch (index)
        {
            case 1: // Sample Undo
                GlobalData.Instance.playScene.gameManager.UseSkillItem(SkillItemType.Undo, false);
                break;
            case 2: // Sample Return
                GlobalData.Instance.playScene.gameManager.UseSkillItem(SkillItemType.Stash, false);
                break;
            case 3: // Sample Shuffle
                GlobalData.Instance.playScene.gameManager.UseSkillItem(SkillItemType.Shuffle, false);
                break;
            default:
                break;
        }

        OnClickClose();
    }
}
