using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public partial class DebugPanel : MonoBehaviour
{
    private List<Action> buttonActionList = new List<Action>();
    private List<Action<int>> inputActionListInt = new List<Action<int>>();
    private List<Action<string>> inputActionListString = new List<Action<string>>();
    
    private const string logFormat = "<color=#FFFF00>[Debug] {0}</color>";
    private const string logFormat2 = "<color=#FFFF00>[Debug] {0} : {1}</color>";

#region Initialize

    private void ClearList()
    {
        buttonActionList.Clear();
        inputActionListInt.Clear();
        inputActionListString.Clear();
    }

#endregion Initialize


#region Setup Listeners

    private void SetupButtonListeners()
    {
        buttonActionList.Add(Debug_LevelClear);
        buttonActionList.Add(Debug_LevelFail);
        buttonActionList.Add(Debug_ResetUser);
        buttonActionList.Add(Debug_ResetPuzzle);
        inputActionListInt.Add(Debug_SetLevel);
        inputActionListInt.Add(Debug_LevelPlay);
        inputActionListInt.Add(Debug_SetPuzzle);
        inputActionListInt.Add(Debug_SetHeart);
        inputActionListInt.Add(Debug_SetCoin);
        inputActionListInt.Add(Debug_SetItem);
    }

#endregion Setup Listeners


#region Debug Functions

    private UserManager m_userManager { get { return Game.Inst.Get<UserManager>();} }

    private void Debug_SetLevel(int level)
    {
        if (level < 1) return;

        level = GlobalData.Instance.GetEnableLevel(level);
        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat2, "Set Level", level));

        m_userManager?.Update(level: level);
        GlobalData.Instance.fragmentCollection.ContainerList.ForEach(item => item.RefreshLockState());
        UIManager.ClosePopupUI_ForceAll();
        UIManager.ShowSceneUI<MainScene>(new MainSceneParameter { ShowMenuType = MainMenuType.HOME });
        GlobalDefine.CheckEventActivate();
        GlobalDefine.CheckEventRefresh();
        GlobalData.Instance.fragmentHome.Refresh(m_userManager.Current);
    }

    private void Debug_SetPuzzle(int count)
    {
        if (count < 0) return;

        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat2, "Set Puzzle", count));

        m_userManager?.Update(puzzle: count);
        GlobalData.Instance.fragmentHome?.RefreshPuzzleBadge(m_userManager.Current.Puzzle);
    }

    private void Debug_SetHeart(int count)
    {
        if (count < 0) return;

        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat2, "Set Heart", count));

        m_userManager?.Update(
            life: count, 
            endChargeLifeAt: count >= Constant.User.MAX_LIFE_COUNT ? DateTimeOffset.Now : 
                                    DateTimeOffset.Now.AddMinutes((Constant.User.MAX_LIFE_COUNT - count) * Constant.User.LIFE_CHARGE_TIME_MINUTES));
    }

    private void Debug_SetCoin(int count)
    {
        if (count < 0) return;

        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat2, "Set Coin", count));

        m_userManager?.Update(coin: count);
    }

    private void Debug_SetItem(int count)
    {
        if (count < 0) return;

        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat2, "Set Item", count));

        var OwnSkillItems = new Dictionary<SkillItemType, int>()
        {
            {SkillItemType.Stash, count},
			{SkillItemType.Undo, count},
			{SkillItemType.Shuffle, count}
        };

        m_userManager?.Update(ownSkillItems: OwnSkillItems);
    }

    private void Debug_ResetUser()
    {
        if (GlobalDefine.IsLevelEditor()) return;

        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, "Reset User"));

        m_userManager?.ResetUser();
        
        SceneManager.LoadScene("Game");
    }

    private void Debug_ResetPuzzle()
    {
        if (GlobalDefine.IsLevelEditor()) return;

        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, "Reset Puzzle"));

        m_userManager?.ResetPuzzle();
        
        SceneManager.LoadScene("Game");
    }

    #region Play Scene Only

    private void Debug_LevelPlay(int level)
    {
        if (level <= 0) return;

        switch(UIManager.CurrentScene)
        {
            case PlayScene playScene:
                Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat2, "Level Play", level));
                UIManager.ClosePopupUI_ForceAll();
                playScene.gameManager.LoadLevel(level, playScene.mainView.CachedRectTransform);
                break;
            default:
                UIManager.ClosePopupUI_ForceAll();
                UIManager.ShowSceneUI<PlayScene>(new PlaySceneParameterCustom(Level:level));
                break;
        }
    }

    private void Debug_LevelClear()
    {
        switch(UIManager.CurrentScene)
        {
            case PlayScene playScene:
                Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, "Level Clear"));
                UIManager.ClosePopupUI_ForceAll();
                playScene.LevelClear();
                playScene.gameManager.CheckClearRewards();
                break;
        }
    }

    private void Debug_LevelFail()
    {
        switch(UIManager.CurrentScene)
        {
            case PlayScene playScene:
                Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, "Level Fail"));
                UIManager.ClosePopupUI_ForceAll();
                playScene.LevelFail();
                break;
        }
    }

    #endregion Play Scene Only

#endregion Debug Functions
}
