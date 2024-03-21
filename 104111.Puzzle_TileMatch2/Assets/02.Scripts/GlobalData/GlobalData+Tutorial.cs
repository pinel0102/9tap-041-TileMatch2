using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public partial class GlobalData
{
    public async UniTask CheckPuzzleOpen(int openPuzzleIndex)
    {
        if (openPuzzleIndex < 0)
        {
            return;
        }

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Open Puzzle {0}</color>", openPuzzleIndex));

        fragmentCollection.RefreshLockState();

        if (GlobalDefine.IsTutorialPuzzle(openPuzzleIndex))
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            await ShowTutorial_Puzzle();
        }
    }

    public void ShowTutorial_Play(int level, Action onClosed = null)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Show Tutorial : Level {0}</color>", level));

        ShowTutorialPlayPopup(level, onClosed);
    }

    public void ShowTutorial_Blocker(BlockerType blockerType, TileItem tileItem)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Show Tutorial : {0} ({1})</color>", blockerType, tileItem.tileName));

        ShowTutorialBlockerPopup(blockerType, tileItem);
    }

    public async UniTask ShowTutorial_Puzzle()
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Show Tutorial : Puzzle</color>"));

        await ShowTutorialPuzzlePopup();
    }

    public void ShowTutorialPlayPopup(int level, Action onClosed = null)
    {
        UIManager.ShowPopupUI<TutorialPlayPopup>(
        new TutorialPlayPopupParameter(
            Level: level,
            GlobalDefine.GetTutorialIndex(level),
            GlobalDefine.GetTutorialItemIndex(level),
            OnClosed: onClosed
        ));
    }

    public void ShowTutorialBlockerPopup(BlockerType blockerType, TileItem tileItem, Action onComplete = null)
    {
        if(!GlobalDefine.isBlockerTutorialTest)
        {
            switch(blockerType)
            {
                case BlockerType.Glue_Left:
                case BlockerType.Glue_Right:
                    userManager.UpdateBlocker_Tutorial(Showed_BlockerTutorial_Glue:true);
                    break;
                case BlockerType.Bush:
                    userManager.UpdateBlocker_Tutorial(Showed_BlockerTutorial_Bush:true);
                    break;
                case BlockerType.Suitcase:
                    userManager.UpdateBlocker_Tutorial(Showed_BlockerTutorial_Suitcase:true);
                    break;
                case BlockerType.Jelly:
                    userManager.UpdateBlocker_Tutorial(Showed_BlockerTutorial_Jelly:true);
                    break;
                case BlockerType.Chain:
                    userManager.UpdateBlocker_Tutorial(Showed_BlockerTutorial_Chain:true);
                    break;
                default:
                    return;
            }
        }

        UIManager.ShowPopupUI<TutorialBlockerPopup>(
        new TutorialBlockerPopupParameter(
            BlockerType: blockerType,
            TileItem: tileItem
        ));
    }

    public async UniTask ShowTutorialPuzzlePopup(Action onComplete = null)
    {
        bool popupClosed = false;

        UIManager.ShowPopupUI<TutorialPuzzlePopup>(
        new TutorialPuzzlePopupParameter(
            () => { popupClosed = true; }
        ));

        await UniTask.WaitUntil(() => popupClosed);
    }
}
