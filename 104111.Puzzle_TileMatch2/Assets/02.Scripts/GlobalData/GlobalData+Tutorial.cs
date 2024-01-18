using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GlobalData
{
    public void ShowTutorial(int level)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Show Tutorial : Level {0}</color>", level));
    }

    public void ShowTutorial_Puzzle()
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Show Tutorial : Puzzle</color>"));
    }
}
