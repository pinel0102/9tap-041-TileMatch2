#nullable enable

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NineTap.Common;
using TMPro;
using DG.Tweening;

public record TutorialPuzzlePopupParameter(
    Action PopupCloseCallback
): DefaultParameterWithoutHUD;

[ResourcePath("UI/Popup/TutorialPuzzlePopup")]
public class TutorialPuzzlePopup : UIPopup
{
    [SerializeField]    private RectTransform m_unmask = default!;
    [SerializeField]    private List<GameObject> m_tutorialObject = default!;
    [SerializeField]    private List<RectTransform> m_tutorialPanel = default!;
    [SerializeField]    private TMP_Text m_badgeCount = default!;
    [SerializeField]    private TMP_Text m_puzzleCount = default!;
    [SerializeField]    private Image m_pieceImage = default!;
    [SerializeField]    private Image m_pieceImageSSU = default!;
    [SerializeField]    private Image m_pieceImageBlur = default!;
    [SerializeField]    private CanvasGroup m_viewGroup = default!;
    [SerializeField]    private GameObject m_touchLock = default!;

    public int currentIndex;
    private Color originColor;
    public Color blinkColor = new Color(1, 1, 0, 0.8f);
    public float blinkTime = 0.5f;
    public int blinkCount = 3;
    private bool isBlinking = false;

    private bool isButtonInteractable;
    private Action? m_popupCloseCallback;
    private int[] pieceIndex = new int[3]{12, 7, 17};
    private const string resPath = "Images/Puzzle/PieceDefault/1001/{0:00}";
    
    public override void OnSetup(UIParameter uiParameter)
    {
        base.OnSetup(uiParameter);

		if (uiParameter is not TutorialPuzzlePopupParameter parameter)
		{
			OnClickClose();
			return;
		}

        m_popupCloseCallback = parameter.PopupCloseCallback;

        currentIndex = 0;
        isBlinking = false;
        SetButtonInteractable(false);        
        m_unmask.gameObject.SetActive(false);

        for(int i=0; i < m_tutorialObject.Count; i++)
        {
            m_tutorialObject[i].SetActive(false);
        }

        originColor = m_pieceImageBlur.color;
        m_viewGroup.alpha = 1f;
    }

    public override void OnShow()
    {
        base.OnShow();

        SetButtonInteractable(true);
        OpenTutorial(currentIndex);
    }

    private void OpenTutorial(int index)
    {
        currentIndex = index;
        Debug.Log(CodeManager.GetMethodName() + currentIndex);

        m_viewGroup.alpha = 1f;

        switch (currentIndex)
        {
            case 0: // Main Button
                m_badgeCount.SetText(GlobalData.Instance.userManager.Current.Puzzle.ToString());
                m_tutorialPanel[currentIndex].position = GlobalData.Instance.fragmentHome.PuzzleButton.position;
                m_tutorialObject[currentIndex].SetActive(true);
                SetButtonInteractable(true);
                break;
            case 1: // Select Jigsaw
            case 2:
            case 3:
                m_puzzleCount.SetText(GlobalData.Instance.userManager.Current.Puzzle.ToString());
                m_tutorialPanel[1].position = GlobalData.Instance.HUD.behaviour.Fields[0].transform.position;
                m_tutorialPanel[2].position = GlobalData.Instance.fragmentPuzzle.GetSlotTransform(pieceIndex[currentIndex-1]).position;
                m_pieceImage.sprite = m_pieceImageSSU.sprite = Resources.Load<Sprite>(string.Format(resPath, pieceIndex[currentIndex-1]));
                m_tutorialObject[1].SetActive(true);

                if (currentIndex == 1)
                {
                    SetButtonInteractable(true);
                    isBlinking = true;

                    UniTask.Void(
                        async token => {     
                            
                            for(int i=0; i < blinkCount; i++)
                            {
                                if (isBlinking)
                                {
                                    await UniTask.Delay(TimeSpan.FromSeconds(blinkTime));
                                    m_pieceImageBlur.color = blinkColor;
                                }
                                
                                if (isBlinking)
                                {
                                    await UniTask.Delay(TimeSpan.FromSeconds(blinkTime));
                                    m_pieceImageBlur.color = originColor;
                                }
                            }

                            m_pieceImageBlur.color = originColor;
                            isBlinking = false;
                        },
                        this.GetCancellationTokenOnDestroy()
                    );
                }
                else
                {
                    SetButtonInteractable(true);
                    m_pieceImageBlur.color = originColor;
                    isBlinking = false;
                }
                break;
            default:
                SetButtonInteractable(true);
                break;
        }
    }

    private void SetButtonInteractable(bool interactable)
    {
        isButtonInteractable = interactable;
        m_touchLock.SetActive(!isButtonInteractable);
        //Debug.Log(CodeManager.GetMethodName() + isButtonInteractable);
    }

    public void OnClick_TutorialClose()
    {
        if (!isButtonInteractable) 
            return;

        SetButtonInteractable(false);
        m_viewGroup.alpha = 0;
        
        switch (currentIndex)
        {
            case 0: // Move To Puzzle Tab
                m_tutorialObject[currentIndex].SetActive(false);
                GlobalData.Instance.mainScene.scrollView.MoveTo((int)MainMenuType.JIGSAW_PUZZLE);
                
                UniTask.Void(
                    async token => {
                        await UniTask.WaitUntil(
                            () => GlobalData.Instance.currentTab == MainMenuType.JIGSAW_PUZZLE
                        );
                        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
                        OpenTutorial(1);
                    },
                    this.GetCancellationTokenOnDestroy()
                );
                break;
            case 1: // Select Jigsaw
            case 2:
                isBlinking = false;
                m_pieceImageBlur.color = originColor;

                GlobalData.Instance.fragmentPuzzle.GetSlotPiece(pieceIndex[currentIndex-1]).OnClick_Unlock_MustCallback(() => {
                    UniTask.Void(
                        async token => {                
                            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
                            OpenTutorial(currentIndex+1);
                        },
                        this.GetCancellationTokenOnDestroy()
                    );
                });
                break;
            case 3:
                GlobalData.Instance.fragmentPuzzle.GetSlotPiece(pieceIndex[currentIndex-1]).OnClick_Unlock_MustCallback(() => {
                    UniTask.Void(
                        async token => {                
                            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
                            OnClickClose();
                        },
                        this.GetCancellationTokenOnDestroy()
                    );
                });
                break;
            default:
                OnClickClose();
                break;
        }
    }

    public override void OnClickClose()
	{
        base.OnClickClose();

        m_popupCloseCallback?.Invoke();
	}
}
