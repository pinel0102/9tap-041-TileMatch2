using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class GlobalDefine
{
    public readonly static List<int> TutorialLevels = new List<int>()
    {
        1, 3, 5, 7
    };

    public const int TutorialPuzzle = 1001;

    public static bool IsTutorialLevel(int level)
    {
        return TutorialLevels.Contains(level);
    }

    public static bool IsTutorialPuzzle(int puzzleIndex)
    {
        return puzzleIndex == TutorialPuzzle;
    }
}
