using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NineTap.Common;

public class EventBanner_SweetHolic : MonoBehaviour
{
    [Header("★ [Reference] Sweet Holic Locked")]
    [SerializeField]	private GameObject sweetHolicLock;
    [SerializeField]	private TMP_Text m_lockedText;

    [Header("★ [Reference] Sweet Holic")]
    [SerializeField]	private GameObject sweetHolicUnlock;
    [SerializeField]	private Slider expSlider;
    [SerializeField]	private TMP_Text m_text;
    [SerializeField]	private TMP_Text m_textIncrease;
    [SerializeField]	private TMP_Text timeText;
    [SerializeField]	private Image targetItemImage;
    public RectTransform targetItemPosition;

    [Header("★ [Live] Sweet Holic")]
    public bool isUnlocked;
    public string targetName;
    public int targetIndex;
    public int currentLevel;
    public int currentExp;
    public int requiredExp;
    public int totalExp;
    public EventData nextEventData;
    private GameEventType eventType = GameEventType.SweetHolic;

    private TableManager tableManager;
    private EventDataTable eventDataTable;    
    private ExpTable ExpTable;
    private GlobalData globalData { get { return GlobalData.Instance; } }

    private const string textFormatExp = "{0}/{1}";
    private const string textExpMax = "Complete!";
    private const string textFormatTime = "";
    private const string textFormatLocked = "Unlock at Level {0}!";

    public void Initialize(User user, TableManager _tableManager)
    {
        tableManager = _tableManager;
        eventDataTable = tableManager.EventDataTable;
        ExpTable = globalData.eventSweetHolic_ExpTable;

        m_lockedText.SetText(string.Format(textFormatLocked, Constant.User.MIN_OPENLEVEL_EVENT_SWEETHOLIC));
        RefreshEventState(user);
    }

    /// <summary>
    /// 자동 반복.
    /// </summary>
    /// <param name="user"></param>
    public void OnUpdateUI(User user)
    {
        RefreshTimeText(user.Event_SweetHolic_EndDate);
    }

    /// <summary>
    /// 1회 새로 고침.
    /// </summary>
    /// <param name="user"></param>
    public void Refresh(User user)
    {
        RefreshEventState(user);
    }

    private void RefreshEventState(User user)
    {
        isUnlocked = GlobalDefine.IsOpen_Event_SweetHolic();
        sweetHolicUnlock.SetActive(isUnlocked);
        sweetHolicLock.SetActive(!isUnlocked);

        if (!isUnlocked)
            return;

        Debug.Log(CodeManager.GetMethodName());
        
        RefreshItemIcon();
        RefreshTimeText(user.Event_SweetHolic_EndDate);

        targetName = GlobalData.Instance.eventSweetHolic_TargetName;
        targetIndex = GlobalData.Instance.eventSweetHolic_TargetIndex;
        totalExp = user.Event_SweetHolic_TotalExp;

        (currentLevel, currentExp, requiredExp) = ExpManager.CalculateLevel(totalExp, ExpTable);

        if(eventDataTable.IsMaxLevel(eventType, currentLevel))
        {
            RefreshExpSlider(1, 1);
            RefreshExpText(textExpMax);
        }
        else
        {
            if(eventDataTable.TryGetNextEventData(eventType, currentLevel, out nextEventData))
            {
                RefreshExpSlider(currentExp, nextEventData.EXP);
                RefreshExpText(currentExp, nextEventData.EXP);
            }
            else
            {
                RefreshExpSlider(0, 1);
                RefreshExpText(string.Empty);
            }
        }
    }

    private void RefreshExpSlider(int _currentExp, int _nextExp)
    {
        if (_nextExp <= 0) return;

        float percent = (float)_currentExp / (float)_nextExp;
        expSlider.value = percent;
    }

    private void MoveExpSlider(int _currentExp, int _nextExp, float duration)
    {
        if (_nextExp <= 0) return;

        float percent = (float)_currentExp / (float)_nextExp;
        expSlider.DOValue(percent, duration);
    }

    private void RefreshExpText(int _currentExp, int _nextExp)
    {
        m_text.SetText(string.Format(textFormatExp, _currentExp, _nextExp));
    }

    public void SetIncreaseText(int _totalExp)
    {
        var (oldLevel, oldExp, oldReqExp) = ExpManager.CalculateLevel(_totalExp, ExpTable);
        RefreshExpSlider(oldExp, oldReqExp);
        SetIncreaseText(oldExp, oldReqExp);
    }

    private void SetIncreaseText(int _currentExp, int _requiredExp, bool autoTurnOn_IncreaseMode = true)
    {
        SetIncreaseText(string.Format(textFormatExp, _currentExp, _requiredExp));
        
        if (autoTurnOn_IncreaseMode)
            SetIncreaseMode(true);
    }

    public async UniTask<bool> IncreaseText(int totalExp, int addExp, float duration = 0.5f, bool autoTurnOff_IncreaseMode = true, Action<int> onUpdate = null)
    {
        duration = 3f;

        var (fromLevel, fromExp, fromReqExp) = ExpManager.CalculateLevel(totalExp, ExpTable);
        var (toLevel, toExp, toReqExp) = ExpManager.CalculateLevel(globalData.userManager.Current.Event_SweetHolic_TotalExp, ExpTable);

        RefreshExpSlider(fromExp, fromReqExp);
        SetIncreaseText(fromExp, fromReqExp);
        SetIncreaseMode(true);

        float delay = GetDelay(duration, addExp);

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Start Level : {0} ({1}/{2})</color>", fromLevel, fromExp, fromReqExp));

        int resExp = totalExp;
        while(addExp > 0)
        {
            int oldLevel = fromLevel;
            int addExpSlice = Mathf.Min(addExp, fromReqExp - fromExp);            
            addExp -= addExpSlice;
            resExp += addExpSlice;

            bool increaseFinished = await IncreaseText_UntilLevelUp(fromExp, addExpSlice, fromReqExp, delay);            
            await UniTask.WaitUntil(() => increaseFinished);

            (fromLevel, fromExp, fromReqExp) = ExpManager.CalculateLevel(resExp, ExpTable);

            if (fromLevel > oldLevel)
            {
                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Level Up : {0} ({1}/{2})</color>", fromLevel, fromExp, fromReqExp));
            }
            else
            {
                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Increase Finished : {0} ({1}/{2})</color>", fromLevel, fromExp, fromReqExp));
            }
        }

        if (autoTurnOff_IncreaseMode)
            SetIncreaseMode(false);

        return true;

        float GetDelay(float time, int amount)
        {
            return time/(float)amount;
        }
    }

    private async UniTask<bool> IncreaseText_UntilLevelUp(int fromCount, int addCount, int reqExp, float delay)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} + {1} = {2}/{3}</color>", fromCount, addCount, fromCount + addCount, reqExp));

        for(int i=1; i <= addCount; i++)
        {
            MoveExpSlider(fromCount + i, reqExp, delay);
            SetIncreaseText(fromCount + i, reqExp);

            await UniTask.Delay(TimeSpan.FromSeconds(delay));
        }

        return true;
    }


#region Privates
    
    private void RefreshItemIcon()
    {
        targetItemImage.sprite = SpriteManager.GetSprite(GlobalDefine.GetSweetHolic_ItemImagePath());
    }

    private void RefreshTimeText(string endDate)
    {
        timeText.SetText(GlobalDefine.GetRemainEventTime(endDate));
    }

    private void RefreshExpText(string text)
    {
        m_text.SetText(text);
    }
    
    private void SetIncreaseMode(bool isIncrease)
    {
        if(m_textIncrease == null) return;

        m_textIncrease.gameObject.SetActive(isIncrease);
        m_text.gameObject.SetActive(!isIncrease);
    }

    private void SetIncreaseText(string _text)
    {
        if(m_textIncrease == null) return;
        
        m_textIncrease.SetText(_text);
    }

#endregion Privates

}
