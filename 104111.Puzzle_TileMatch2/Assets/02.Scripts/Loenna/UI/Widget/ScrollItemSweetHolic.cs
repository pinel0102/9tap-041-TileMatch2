using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NineTap.Common;
using TMPro;
using System;

[ResourcePath("UI/Widgets/ScrollItemSweetHolic")]
public class ScrollItemSweetHolic : CachedBehaviour
{
    [Header("★ [Live] Event Item")]
    [SerializeField]    private GameEventType eventType = GameEventType.SweetHolic;
    [SerializeField]	private bool m_descBubbleOn;
    public int m_itemLevel;
    public bool m_isMaxLevel;
    public bool m_achieved;
    public bool m_isCurrent;

    [Header("★ [Reference] Left")]
    [SerializeField]	private GameObject m_lineObject;
    [SerializeField]	private GameObject m_currentEffectObject;
    [SerializeField]	private GameObject m_defaultLevelObject;
    [SerializeField]	private GameObject m_completeLevelObject;
    [SerializeField]	private TMP_Text m_levelText;

    [Header("★ [Reference] Right")]
    [SerializeField]	private RewardItemEvent m_rewardItemEvent;
    [SerializeField]	private GameObject m_checkObject;
    [SerializeField]	private GameObject m_lockObject;
    
    [Header("★ [Reference] Description")]
    [SerializeField]	private Button m_descButton;
    [SerializeField]	private GameObject m_descObject;
    [SerializeField]	private Transform m_descParent;
    
    private EventData m_EventData;
    private int m_userEventLevel;
    private string m_targetImagePath;
    private string m_boxImagepath;
    
    public void Initialize(EventData eventData, bool isMaxLevel, int userEventLevel, string targetImagePath, string boxImagepath, Action onDescClick = null)
    {
        m_EventData = eventData;        
        m_targetImagePath = targetImagePath;
        m_boxImagepath = boxImagepath;
        m_userEventLevel = userEventLevel;
        m_itemLevel = m_EventData.Level;
        m_isMaxLevel = isMaxLevel;
        m_levelText.SetText(m_itemLevel.ToString());

        m_descButton.onClick.RemoveAllListeners();
        m_descButton.onClick.AddListener(() => {
            bool newValue = !m_descBubbleOn;
            onDescClick?.Invoke();
            SetDescBubble(newValue);
        });
        
        RefreshReward(m_EventData);
        RefreshDescBubble(m_EventData);
        RefreshItemState();
        SetDescBubble(false);
    }

    private void RefreshReward(EventData targetEvent)
    {
        if(targetEvent.Rewards.Count == 1)
        {
            IReward reward = targetEvent.Rewards[0];

            //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Next Reward : {0} x {1} (Level {2})</color>", reward.Type, reward.GetAmountString(), targetEvent.Level));

            switch(targetEvent.ChestType)
            {
                case ChestType.Chest: 
                    RefreshRewardItem(m_boxImagepath);
                    break;
                    
                case ChestType.Default: 
                default:
                    if (reward.Type == ProductType.DoubleTime)
                    {
                        RefreshRewardItem(m_targetImagePath, reward.GetAmountString(), 100, true, true);
                    }
                    else
                    {
                        RefreshRewardItem(reward.Type.GetIconName(), reward.GetAmountString(), 80, true);
                    }
                    break;
            }
        }
        else
        {
            RefreshRewardItem(m_boxImagepath);
        }
    }

    private void RefreshDescBubble(EventData targetEvent)
    {
        var prefab = ResourcePathAttribute.GetResource<RewardItemEvent>();
        
        for(int i=0; i < targetEvent.Rewards.Count; i++)
        {
            IReward reward = targetEvent.Rewards[i];
            var widget = Instantiate(prefab, m_descParent);

            if (reward.Type == ProductType.DoubleTime)
            {
                RefreshRewardItem(widget, m_targetImagePath, reward.GetAmountString(), 100, true, true);
            }
            else
            {
                RefreshRewardItem(widget, reward.Type.GetIconName(), reward.GetAmountString(), 80, true);
            }
        }
    }

    private void RefreshRewardItem(string spriteName, string countString = null, float spriteSize = 80, bool isRibbon = false, bool isDouble = false)
    {
        RefreshRewardItem(m_rewardItemEvent, spriteName, countString, spriteSize, isRibbon, isDouble);
    }

    private void RefreshRewardItem(RewardItemEvent rewardItem, string spriteName, string countString = null, float spriteSize = 80, bool isRibbon = false, bool isDouble = false)
    {
        rewardItem.Initialize(spriteName, countString, spriteSize, isRibbon, isDouble);
    }

    private void RefreshItemState()
    {
        m_achieved = m_userEventLevel >= m_itemLevel;
        m_isCurrent = m_itemLevel == m_userEventLevel + 1;

        m_rewardItemEvent.gameObject.SetActive(!m_achieved);

        m_checkObject.SetActive(m_achieved);
        m_lockObject.SetActive(!m_achieved && !m_isCurrent);
        m_defaultLevelObject.SetActive(!m_achieved);
        m_completeLevelObject.SetActive(m_achieved);
        m_currentEffectObject.SetActive(m_isCurrent);
        m_lineObject.SetActive(!m_isMaxLevel);
    }

    public void SetDescBubble(bool active)
    {
        if(!m_achieved && (m_EventData.Rewards.Count > 1 || m_EventData.ChestType == ChestType.Chest))
            m_descBubbleOn = active;
        else
            m_descBubbleOn = false;

        m_descObject.SetActive(m_descBubbleOn);
    }
}
