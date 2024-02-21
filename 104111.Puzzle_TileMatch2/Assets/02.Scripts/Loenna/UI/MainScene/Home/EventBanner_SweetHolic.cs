using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Cysharp.Threading.Tasks;
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
    public string itemName;
    public int currentLevel;
    public int currentExp;
    public int requiredExp;
    public int totalExp;
    public EventData nextEventData;
    private GameEventType eventType = GameEventType.SweetHolic;

    private TableManager tableManager;
    private EventDataTable eventDataTable;
    private int MinLevel = -1;
    private int MaxLevel = -1;
    private List<int> ExpList = default;

    private const string textFormatExp = "{0}/{1}";
    private const string textExpMax = "Complete!";
    private const string textFormatTime = "";
    private const string textFormatLocked = "Unlock at Level {0}!";

    public void Initialize(User user, TableManager _tableManager)
    {
        tableManager = _tableManager;
        eventDataTable = tableManager.EventDataTable;
        
        MinLevel = eventDataTable.GetMinLevel(eventType);
        MaxLevel = eventDataTable.GetMaxLevel(eventType);
        ExpList = eventDataTable.GetExpList(eventType);

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
        
        RefreshItemIcon();
        RefreshTimeText(user.Event_SweetHolic_EndDate);

        itemName = GlobalData.Instance.eventSweetHolic_ItemName;
        totalExp = user.Event_SweetHolic_TotalExp;

        (currentLevel, currentExp, requiredExp) = ExpManager.CalculateLevel(totalExp, MinLevel, MaxLevel, ExpList);

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

    private void RefreshExpText(int _currentExp, int _nextExp)
    {
        m_text.SetText(string.Format(textFormatExp, _currentExp, _nextExp));
    }

    public void SetIncreaseText(int _currentExp)
    {
        SetIncreaseText(_currentExp, requiredExp);
    }

    private void SetIncreaseText(int _currentExp, int _requiredExp, bool autoTurnOn_IncreaseMode = true)
    {
        SetIncreaseText(string.Format(textFormatExp, _currentExp, _requiredExp));
        
        if (autoTurnOn_IncreaseMode)
            SetIncreaseMode(true);
    }

    public void IncreaseText(int fromCount, int addCount, float duration = 0.5f, bool autoTurnOff_IncreaseMode = true, Action<int> onUpdate = null)
    {
        SetIncreaseText(fromCount, requiredExp);
        onUpdate?.Invoke(fromCount);

        SetIncreaseMode(true);

        UniTask.Void(
			async token => {
                float delay = GetDelay(duration, addCount);

                for(int i=1; i <= addCount; i++)
                {
                    SetIncreaseText(fromCount + i, requiredExp);
                    onUpdate?.Invoke(fromCount + i);
                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                }

                if (autoTurnOff_IncreaseMode)
                    SetIncreaseMode(false);
            },
			this.GetCancellationTokenOnDestroy()
        );

        float GetDelay(float time, int amount)
        {
            return time/(float)amount;
        }
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
