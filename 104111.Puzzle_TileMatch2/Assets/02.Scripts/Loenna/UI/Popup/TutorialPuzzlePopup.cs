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

public record TutorialPuzzlePopupParameter(): DefaultParameterWithoutHUD;

[ResourcePath("UI/Popup/TutorialPuzzlePopup")]
public class TutorialPuzzlePopup : UIPopup
{
    [SerializeField]
    private RectTransform m_unmask = default!;

    [SerializeField]
    private List<GameObject> m_tutorialObject = default!;

    [SerializeField]
    private List<RectTransform> m_tutorialPanel = default!;

    public bool isButtonInteractable;
    private int pieceIndex = 12;

    public override void OnSetup(UIParameter uiParameter)
    {
        base.OnSetup(uiParameter);

		if (uiParameter is not TutorialPuzzlePopupParameter parameter)
		{
			OnClickClose();
			return;
		}
        
        SetButtonInteractable(false);
        
        m_unmask.gameObject.SetActive(false);

        for(int i=0; i < m_tutorialObject.Count; i++)
        {
            m_tutorialObject[i].SetActive(false);
        }
    }

    public override void OnShow()
    {
        base.OnShow();

        SetButtonInteractable(true);
        OpenTutorial(0);
    }

    private void OpenTutorial(int index)
    {
        SetPanelPosition(index);
        m_tutorialObject[index].SetActive(true);
    }

    private void SetPanelPosition(int index)
    {
        switch (index)
        {
            case 0: // Main Button
                m_tutorialPanel[index].position = GlobalData.Instance.fragmentHome.PuzzleButton.position;
                break;
            case 1: // 
                m_tutorialPanel[index].position = GlobalData.Instance.HUD.behaviour.Fields[0].transform.position;
                m_tutorialPanel[index+1].position = GlobalData.Instance.fragmentPuzzle.GetSlotTransform(pieceIndex).position;
                break;
            default:
                break;
        }
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
            case 0: // Move To Puzzle Tab
                m_tutorialObject[index].SetActive(false);
                GlobalData.Instance.mainScene.scrollView.MoveTo((int)MainMenuType.JIGSAW_PUZZLE);
                
                UniTask.Void(
                    async () =>
                    {
                        await UniTask.WaitUntil(
                            () => GlobalData.Instance.currentTab == MainMenuType.JIGSAW_PUZZLE
                        );
                        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
                        OpenTutorial(1);
                    }
                );
                break;
            case 1: // Select Jigsaw
                /*GlobalData.Instance.fragmentPuzzle.GetSlotPiece(pieceIndex).OnClick_Unlock(() => {
                    GlobalData.Instance.ShowReadyPopup();
                });*/

                GlobalData.Instance.fragmentPuzzle.GetSlotPiece(pieceIndex).OnClick_Unlock();
                OnClickClose();
                break;
            default:
                break;
        }
    }
}
