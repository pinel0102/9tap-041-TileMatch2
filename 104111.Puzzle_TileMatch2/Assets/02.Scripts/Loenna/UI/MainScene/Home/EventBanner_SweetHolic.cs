using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
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
    [SerializeField]	private GameObject m_boosterTimeBadge;
    [SerializeField]	private Image m_boosterTimeImage;
    [SerializeField]	private TMP_Text m_boosterTimeText;
    
    [Header("★ [Live] Sweet Holic")]
    public bool isUnlocked;
    public string targetName;
    public int targetIndex;
    public int currentLevel;
    public int currentExp;
    public int requiredExp;
    public int totalExp;
    private string m_targetImagePath;
    
    [Header("★ [Parameter] Privates")]
    private EventDataTable m_eventDataTable;
    private ExpTable m_expTable;
    private IUniTaskAsyncEnumerable<(bool, string)> BoosterStatus;
    private GlobalData globalData { get { return GlobalData.Instance; } }
    private const string textFormatLocked = "Unlock at Level {0}!";
    
    public void Initialize(User user, TableManager tableManager)
    {
        m_eventDataTable = tableManager.EventDataTable;
        m_expTable = globalData.eventSweetHolic_ExpTable;

        m_lockedText.SetText(string.Format(textFormatLocked, Constant.User.MIN_OPENLEVEL_EVENT_SWEETHOLIC));
        eventSlider.Initialize(eventType, m_eventDataTable, m_expTable);
        RefreshEventState(user);

        BoosterStatus = globalData.HUD.MessageBroker.Subscribe().Select(user => user.GetEventBoosterStatus(eventType));
        BoosterStatus.BindTo(m_boosterTimeText, (component, status) => {
            m_boosterTimeBadge.SetActive(status.Item1);
            component.text = status.Item2;
            //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Booster Time] {0}</color>", user.ExpiredSweetHolicBoosterAt));
        });
    }

    /// <summary>
    /// 자동 반복.
    /// </summary>
    /// <param name="user"></param>
    public void OnUpdateUI(User user)
    {
        eventSlider.RefreshRealTotalExp(user.Event_SweetHolic_TotalExp);
        eventSlider.RefreshTimeText(user.Event_SweetHolic_EndDate);
        
        RefreshBoosterTime(user);
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

        RefreshBoosterIcon(GlobalDefine.GetSweetHolic_ItemImagePath());
        RefreshBoosterTime(user);

        if (!isUnlocked)
            return;

        Debug.Log(CodeManager.GetMethodName());

        targetName = globalData.eventSweetHolic_TargetName;
        targetIndex = globalData.eventSweetHolic_TargetIndex;

        totalExp = user.Event_SweetHolic_TotalExp;
        (currentLevel, currentExp, requiredExp) = ExpManager.CalculateLevel(totalExp, m_expTable);

        eventSlider.RefreshEventState(m_targetImagePath, user.Event_SweetHolic_EndDate, currentLevel, currentExp, requiredExp, totalExp);
    }

    private void RefreshBoosterIcon(string targetImagePath)
    {
        if(string.IsNullOrEmpty(targetImagePath))
            return;
        
        m_targetImagePath = targetImagePath;
        m_boosterTimeImage.sprite = SpriteManager.GetSprite(m_targetImagePath);
    }

    private void RefreshBoosterTime(User user)
    {
        /*bool isActivated = globalData.eventSweetHolic_Activate && 
                           globalData.eventSweetHolic_IsBoosterTime &&
                           globalData.eventSweetHolic_TargetIndex == targetIndex;
        
        m_boosterTimeBadge.SetActive(isActivated);
        m_boosterTimeText.SetText(isActivated ? GlobalDefine.GetRemainEventTime_OneFormat(user.Event_SweetHolic_BoosterEndDate) : string.Empty);*/
    }
}
