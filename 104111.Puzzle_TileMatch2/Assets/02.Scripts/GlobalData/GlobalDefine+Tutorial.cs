using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class GlobalDefine
{
    public readonly static List<int> TutorialLevels = new List<int>()       { 1, 3, 5, 7 };
    private readonly static List<int> TutorialItemIndex = new List<int>()   {-1, 2, 1, 3 };
    private readonly static List<int> TutorialCheckCount = new List<int>()  { 0, 1, 3, 0 };

    public const int TutorialPuzzle = 1001;

    public static bool IsTutorialLevel(int level)
    {
        return TutorialLevels.Contains(level);
    }

    public static bool IsTutorialPuzzle(int puzzleIndex)
    {
        return puzzleIndex == TutorialPuzzle;
    }

    public static int GetTutorialIndex(int level)
    {
        return IsTutorialLevel(level) ? TutorialLevels.IndexOf(level) : -1;
    }

    public static int GetTutorialItemIndex(int level)
    {
        return IsTutorialLevel(level) ? TutorialItemIndex[GetTutorialIndex(level)] : -1;
    }

    public static int GetTutorialCheckCount(int level)
    {
        return IsTutorialLevel(level) ? TutorialCheckCount[GetTutorialIndex(level)] : 0;
    }
}
