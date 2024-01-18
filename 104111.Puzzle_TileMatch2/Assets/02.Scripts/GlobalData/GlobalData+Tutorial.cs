using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class GlobalData
{
    public void ShowTutorial_Play(int level)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Show Tutorial : Level {0}</color>", level));

        ShowTutorialPlayPopup(level);
    }

    public void ShowTutorial_Puzzle()
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Show Tutorial : Puzzle</color>"));

        ShowTutorialPuzzlePopup();
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

    public void ShowTutorialPuzzlePopup(Action onComplete = null)
    {
        //
    }
}
