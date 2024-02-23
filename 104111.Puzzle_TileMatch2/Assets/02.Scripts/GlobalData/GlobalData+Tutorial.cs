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

    public void ShowTutorial_Play(int level)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Show Tutorial : Level {0}</color>", level));

        ShowTutorialPlayPopup(level);
    }

    public async UniTask ShowTutorial_Puzzle()
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Show Tutorial : Puzzle</color>"));

        await ShowTutorialPuzzlePopup();
    }

    public void ShowTutorialPlayPopup(int level, Action onComplete = null)
    {
        UIManager.ShowPopupUI<TutorialPlayPopup>(
        new TutorialPlayPopupParameter(
            Level: level,
            GlobalDefine.GetTutorialIndex(level),
            GlobalDefine.GetTutorialItemIndex(level)
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
