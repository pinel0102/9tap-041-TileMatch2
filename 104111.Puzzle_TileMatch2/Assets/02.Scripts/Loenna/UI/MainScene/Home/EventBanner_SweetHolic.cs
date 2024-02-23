using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventBanner_SweetHolic : MonoBehaviour
{
    private GameEventType eventType = GameEventType.SweetHolic;

    [Header("★ [Reference] Sweet Holic")]
    public EventSlider eventSlider;
    public RectTransform targetItemPosition;
    [SerializeField]	private GameObject sweetHolicLock;
    [SerializeField]	private GameObject sweetHolicUnlock;
    [SerializeField]	private TMP_Text m_lockedText;
    
    [Header("★ [Live] Sweet Holic")]
    public bool isUnlocked;
    public string targetName;
    public int targetIndex;
    public int currentLevel;
    public int currentExp;
    public int requiredExp;
    public int totalExp;
    
    [Header("★ [Parameter] Privates")]
    private EventDataTable m_eventDataTable;
    private ExpTable m_expTable;
    private GlobalData globalData { get { return GlobalData.Instance; } }
    private const string textFormatLocked = "Unlock at Level {0}!";
    
    public void Initialize(User user, TableManager tableManager)
    {
        m_eventDataTable = tableManager.EventDataTable;
        m_expTable = globalData.eventSweetHolic_ExpTable;

        m_lockedText.SetText(string.Format(textFormatLocked, Constant.User.MIN_OPENLEVEL_EVENT_SWEETHOLIC));
        eventSlider.Initialize(eventType, m_eventDataTable, m_expTable);
        RefreshEventState(user);
    }

    /// <summary>
    /// 자동 반복.
    /// </summary>
    /// <param name="user"></param>
    public void OnUpdateUI(User user)
    {
        eventSlider.RefreshRealTotalExp(user.Event_SweetHolic_TotalExp);
        eventSlider.RefreshTimeText(user.Event_SweetHolic_EndDate);
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

        targetName = globalData.eventSweetHolic_TargetName;
        targetIndex = globalData.eventSweetHolic_TargetIndex;

        totalExp = user.Event_SweetHolic_TotalExp;
        (currentLevel, currentExp, requiredExp) = ExpManager.CalculateLevel(totalExp, m_expTable);

        eventSlider.RefreshEventState(GlobalDefine.GetSweetHolic_ItemImagePath(), user.Event_SweetHolic_EndDate, currentLevel, currentExp, requiredExp, totalExp);
    }
}
