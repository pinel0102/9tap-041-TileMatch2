#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class DebugPanel : MonoBehaviour
{
    private List<Action> buttonActionList = new List<Action>();
    private List<Action<int>> inputActionListInt = new List<Action<int>>();
    private List<Action<string>> inputActionListString = new List<Action<string>>();
    
    private const string logFormat = "<color=#FFFF00>{0}</color>";
    private const string logFormat2 = "<color=#FFFF00>{0} : {1}</color>";

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
        buttonActionList.Add(DebugLevelClear);
        buttonActionList.Add(DebugLevelFail);
        
        inputActionListInt.Add(DebugLevelPlay);
        inputActionListInt.Add(DebugSetClear);
        inputActionListInt.Add(DebugSetHeart);
        inputActionListInt.Add(DebugSetCoin);
        inputActionListInt.Add(DebugSetItem);
    }

#endregion Setup Listeners


#region Edit Functions

    private UserManager m_userManager { get { return Game.Inst.Get<UserManager>();} }

    private void DebugSetClear(int level)
    {
        if (level < 1) return;

        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, level));

        m_userManager?.Update(level: level);
        UIManager.ShowSceneUI<MainScene>(new NineTap.Common.DefaultParameter());
    }

    private void DebugSetHeart(int count)
    {
        if (count < 0) return;

        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, count));

        m_userManager?.Update(life: count);
    }

    private void DebugSetCoin(int count)
    {
        if (count < 0) return;

        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, count));

        m_userManager?.Update(coin: count);
    }

    private void DebugSetItem(int count)
    {
        if (count < 0) return;

        Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, count));

        var OwnSkillItems = new Dictionary<SkillItemType, int>()
        {
            {SkillItemType.Stash, count},
			{SkillItemType.Undo, count},
			{SkillItemType.Shuffle, count}
        };

        m_userManager?.Update(ownSkillItems: OwnSkillItems);
    }

    #region Play Scene Only

    private void DebugLevelPlay(int level)
    {
        if (level <= 0) return;

        switch(UIManager.CurrentScene)
        {
            case PlayScene playScene:
                Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, level));
                playScene.gameManager.LoadLevel(level, playScene.mainView.CachedRectTransform);
                break;
            default:
                UIManager.ShowSceneUI<PlayScene>(new PlaySceneParameter());
                break;
        }
    }

    private void DebugLevelClear()
    {
        switch(UIManager.CurrentScene)
        {
            case PlayScene playScene:
                Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, "Level Clear"));
                playScene.LevelClear();
                playScene.gameManager.CheckClearRewards();
                break;
        }
    }

    private void DebugLevelFail()
    {
        switch(UIManager.CurrentScene)
        {
            case PlayScene playScene:
                Debug.Log(CodeManager.GetMethodName() + string.Format(logFormat, "Level Fail"));
                playScene.LevelFail();
                break;
        }
    }

    #endregion Play Scene Only

#endregion Edit Functions
}

#endif