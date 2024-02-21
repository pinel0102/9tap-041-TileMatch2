using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class ExpManager
{
    /// <summary>
    /// 경험치를 획득하고 레벨을 계산한다.
    /// </summary>
    /// <param name="totalExp"></param>
    /// <param name="addExp"></param>
    /// <param name="maxExp"></param>
    /// <param name="currentLevel"></param>
    /// <param name="maxLevel"></param>
    /// <param name="expList"></param>
    /// <returns>currentLevel, currentExp, requiredExp, isLevelUp</returns>
    public static (int, int, int, bool) GetEXP(int totalExp, int addExp, int maxExp, int currentLevel, int minLevel, int maxLevel, List<int> expList)
    {
        if (totalExp >= maxExp || addExp < 0) 
        {
            var (_currentLevel, _currentExp, _requiredExp, _) = CalculateLevel(totalExp, -1, minLevel, maxLevel, expList, false);
            return (_currentLevel, _currentExp, _requiredExp, false);
        }

        totalExp = Mathf.Min(maxExp, totalExp + addExp);
        
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Get Exp : {0}</color>", addExp));

        return CalculateLevel(totalExp, currentLevel, minLevel, maxLevel, expList, true);
    }

    /// <summary>
    /// TotalExp 를 레벨과 경험치로 환산한다.
    /// </summary>
    /// <param name="totalExp"></param>
    /// <param name="maxLevel"></param>
    /// <param name="expList"></param>
    /// <returns>currentLevel, currentExp, requiredExp</returns>
    public static (int, int, int) CalculateLevel(int totalExp, int minLevel, int maxLevel, List<int> expList)
    {
        var (_currentLevel, _currentExp, _requiredExp, _) = CalculateLevel(totalExp, -1, minLevel, maxLevel, expList, false);
        return (_currentLevel, _currentExp, _requiredExp);
    }

    /// <summary>
    /// TotalExp 를 레벨과 경험치로 환산하고 경험치를 획득한 경우 레벨업 여부를 확인한다.
    /// </summary>
    /// <param name="totalExp"></param>
    /// <param name="currentLevel"></param>
    /// <param name="maxLevel"></param>
    /// <param name="expList"></param>
    /// <param name="isGetExp"></param>
    /// <returns>currentLevel, currentExp, requiredExp, isLevelUp (isGetExp = true)</returns>
    private static (int, int, int, bool) CalculateLevel(int totalExp, int currentLevel, int minLevel, int maxLevel, List<int> expList, bool isGetExp)
    {
        bool isLevelUp = false;

        int prevLevel = currentLevel;
        int tempExp = totalExp;
        int tempLevel = minLevel;
        int requiredExp = 0;

        for(int i=0; i < expList.Count; i++)
        {
            requiredExp = expList[i];
            if (tempExp >= requiredExp)
            {
                tempExp = Mathf.Max(0, tempExp - requiredExp);
                tempLevel = Mathf.Min(maxLevel, tempLevel + 1);
            }
            else break;
        }

        currentLevel = tempLevel;
        int currentExp = tempExp;

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Level : {0} / Exp : {1}/{2} / Total Exp : {3}</color>", currentLevel, currentExp, requiredExp, totalExp));

        if (isGetExp)
        {
            if(currentLevel > prevLevel)
            {
                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Level UP!</color>"));
                isLevelUp = true;
            }
        }

        return (currentLevel, currentExp, requiredExp, isLevelUp);
    }
}
