using System.Collections;
using System.Collections.Generic;
using NineTap.Common;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using DG.Tweening;

public record EventPopupSweetHolicParameter
(
	string Title,
	Action PopupCloseCallback,
    EventBanner_SweetHolic EventBanner,
    EventDataTable EventDataTable,
    ExpTable ExpTable
) : DefaultParameter;

[ResourcePath("UI/Popup/EventPopupSweetHolic")]
public class EventPopupSweetHolic : UIPopup
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private UIImageButton closeButton;

    [Header("★ [Reference] Sweet Holic")]
    [SerializeField] private GameEventType eventType = GameEventType.SweetHolic;
    [SerializeField] private EventSlider eventSlider;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private VerticalLayoutGroup contentGroup;
    [SerializeField] private List<ScrollItemSweetHolic> scrollItemList = new List<ScrollItemSweetHolic>();

    private float scrollItemHeight = 150;
    private EventBanner_SweetHolic m_eventBanner;
    private EventDataTable m_eventDataTable;
    private ExpTable m_expTable;
    private Action m_popupCloseCallback;
    private GlobalData globalData { get { return GlobalData.Instance; } }
    private WaitForSecondsRealtime wTimeDelay = new WaitForSecondsRealtime(1.0f);

    public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not EventPopupSweetHolicParameter parameter)
		{
			return;
		}

        m_eventBanner = parameter.EventBanner;
        m_eventDataTable = parameter.EventDataTable;
        m_expTable = parameter.ExpTable;
        m_popupCloseCallback = parameter.PopupCloseCallback;
        scrollItemList = new List<ScrollItemSweetHolic>();

        titleText.SetText(parameter.Title);        
        closeButton.OnSetup(new UIImageButtonParameter{
            OnClick = OnClickClose
        });

        eventSlider.Initialize(eventType, m_eventDataTable, m_expTable, m_eventBanner.boxImagePath);
        RefreshEventState(globalData.userManager.Current);

        CreateScrollItems();
        ResetDescBubbles();        
        SetScrollAnchor(m_eventBanner.currentLevel, m_eventDataTable.GetMaxLevel(eventType), 
                        scrollItemHeight, contentGroup.padding.top + contentGroup.padding.bottom, scrollRect.viewport.rect.height);
    }

#region Event Slider

    private void RefreshEventState(User user)
    {
        eventSlider.RefreshEventState(m_eventBanner.targetImagePath, user.Event_SweetHolic_EndDate, m_eventBanner.currentLevel, m_eventBanner.currentExp, m_eventBanner.requiredExp, m_eventBanner.totalExp);
    }

    private IEnumerator Co_RealTime()
    {
        while(true)
        {
            if(GlobalDefine.IsUserLoaded)
            {
                OnUpdateUI(globalData.userManager.Current);
            }
            
            yield return wTimeDelay;
        }
    }

    /// <summary>
    /// 자동 반복.
    /// </summary>
    /// <param name="user"></param>
    private void OnUpdateUI(User user)
    {
        //Debug.Log(CodeManager.GetMethodName());

        eventSlider.RefreshRealTotalExp(user.Event_SweetHolic_TotalExp);
        eventSlider.RefreshTimeText(user.Event_SweetHolic_EndDate);
    }

#endregion Event Slider


#region Scroll Items

    private void CreateScrollItems()
    {
        //Debug.Log(CodeManager.GetMethodName());

        GlobalDefine.ClearChild(scrollRect.content);

        scrollItemList.Clear();
        
        var prefab = ResourcePathAttribute.GetResource<ScrollItemSweetHolic>();
        var eventDataArray = m_eventDataTable.GetEventDatas(eventType);
        
        for(int i=0; i < eventDataArray.Length; i++)
        {
            EventData eventData = eventDataArray[i];
            var widget = Instantiate(prefab, scrollRect.content);
            widget.Initialize(eventData, m_eventDataTable.IsMaxLevel(eventType, eventData.Level), m_eventBanner.currentLevel, m_eventBanner.targetImagePath, m_eventBanner.boxImagePath, ResetDescBubbles);
            scrollItemList.Add(widget);
        }
    }

    public void ResetDescBubbles()
    {
        for(int i=0; i < scrollItemList.Count; i++)
        {
            scrollItemList[i].SetDescBubble(false);
        }
    }

    public void SetScrollAnchor(int currentLevel, int maxLevel, float itemHeight, float paddingSum, float viewPortHeight)
    {
        float anchorCurrent = (Mathf.Min(currentLevel - 1, maxLevel) * itemHeight) - paddingSum;
        float anchorMin = 0;
        float anchorMax = (itemHeight * scrollItemList.Count) + paddingSum - viewPortHeight;
        float finalValue = Mathf.Clamp(anchorCurrent, anchorMin, anchorMax);

        //Debug.Log(CodeManager.GetMethodName() + string.Format("anchorCurrent : {0} / paddingSum : {1} / viewPortHeight : {2} / anchorMax : {3} / finalValue : {4}", anchorCurrent, paddingSum, viewPortHeight, anchorMax, finalValue));

        scrollRect.content.anchoredPosition = new Vector2(0, finalValue);
    }

#endregion Scroll Items

#region Override Functions

    public override void OnShow()
    {
        base.OnShow();

        StartCoroutine(Co_RealTime());
    }

    public override void OnHide()
    {
        base.OnHide();
        GlobalData.Instance.HUD_Preferred();

        m_popupCloseCallback?.Invoke();
    }

    public override void OnClickClose()
	{
		base.OnClickClose();
	}

#endregion Override Functions

}
