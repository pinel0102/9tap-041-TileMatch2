using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class EventSlider : MonoBehaviour
{
    [SerializeField]	private Slider m_expSlider;
    [SerializeField]	private TMP_Text m_expText;
    [SerializeField]	private Image m_targetItemImage;
    [SerializeField]	private RewardItemEvent m_rewardItemEvent;

    [SerializeField]	private TMP_Text m_timeText;
    [SerializeField]	private int m_realTotalExp;
    [SerializeField]	private string m_targetImagePath;
    
    [Header("★ [Parameter] Privates")]
    private GameEventType m_eventType;
    private EventDataTable m_eventDataTable;
    private ExpTable m_expTable;
    private EventData nextEventData;
    private string m_boxImagePath;
    private bool m_resume;
    private const string textFormatExp = "{0}/{1}";
    private const string textExpMax = "Complete!";

#region Initialize

    public void Initialize(GameEventType eventType, EventDataTable eventDataTable, ExpTable expTable, string boxImagePath)
    {
        m_eventType = eventType;
        m_eventDataTable = eventDataTable;
        m_expTable = expTable;
        m_boxImagePath = boxImagePath;
    }

#endregion Initialize


#region Publics

    public void RefreshEventState(string targetImagePath, string endDate, int currentLevel, int currentExp, int requiredExp, int totalExp)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("Level {0} ({1}/{2})", currentLevel, currentExp, requiredExp));

        RefreshTargetIcon(targetImagePath);
        RefreshRealTotalExp(totalExp);
        RefreshTimeText(endDate);

        if(m_eventDataTable.IsMaxLevel(m_eventType, currentLevel))
        {
            RefreshReward(m_eventDataTable.GetEventData(m_eventType, m_eventDataTable.GetMaxLevel(m_eventType)));
            RefreshExpSlider(1, 1);
            RefreshExpText(textExpMax);
        }
        else
        {
            if(m_eventDataTable.TryGetNextEventData(m_eventType, currentLevel, out nextEventData))
            {
                RefreshReward(nextEventData);
                RefreshExpSlider(currentExp, nextEventData.EXP);
                RefreshExpText(currentExp, nextEventData.EXP);
            }
            else
            {
                RefreshReward(m_eventDataTable.GetEventData(m_eventType, m_eventDataTable.GetMinLevel(m_eventType)));
                RefreshExpSlider(0, 1);
                RefreshExpText(string.Empty);
            }
        }
    }

    public void RefreshRealTotalExp(int _totalExp)
    {
        m_realTotalExp = _totalExp;
    }

    public void RefreshTimeText(string endDate)
    {
        if (!string.IsNullOrEmpty(endDate))
            m_timeText.SetText(GlobalDefine.GetRemainEventTime(endDate));
    }

    public void SetIncreaseText(int _totalExp)
    {
        var (oldLevel, oldExp, oldReqExp) = ExpManager.CalculateLevel(_totalExp, m_expTable);
        if(m_eventDataTable.IsMaxLevel(m_eventType, oldLevel))
            return;
        
        RefreshExpSlider(oldExp, oldReqExp);
        RefreshExpText(oldExp, oldReqExp);
    }

    public async UniTask<bool> IncreaseText(int fromTotalExp, int addExp, float duration = 0.3f, Action<int> onLevelUp = null, Action onComplete = null)
    {
        SetResume(false);

        var (fromLevel, fromExp, fromReqExp) = ExpManager.CalculateLevel(fromTotalExp, m_expTable);
        if(m_eventDataTable.IsMaxLevel(m_eventType, fromLevel))
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Level is Max : {0}</color>", fromLevel));
            return true;
        }

        var (toLevel, toExp, toReqExp) = ExpManager.CalculateLevel(m_realTotalExp, m_expTable);

        RefreshExpSlider(fromExp, fromReqExp);
        RefreshExpText(fromExp, fromReqExp);
        
        // Test Value
        //duration = 1f;

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Start Level : {0} ({1}/{2})</color>", fromLevel, fromExp, fromReqExp));

        int resExp = fromTotalExp;
        while(addExp > 0)
        {
            int oldLevel = fromLevel;
            int addExpSlice = Mathf.Min(addExp, fromReqExp - fromExp);            
            addExp -= addExpSlice;
            resExp += addExpSlice;

            float delay = GetDelay(duration, addExpSlice);

            bool increaseFinished = await IncreaseText_UntilLevelUp(fromExp, addExpSlice, fromReqExp, delay);            
            await UniTask.WaitUntil(() => increaseFinished);

            (fromLevel, fromExp, fromReqExp) = ExpManager.CalculateLevel(resExp, m_expTable);

            if (fromLevel > oldLevel)
            {
                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Level Up : {0} ({1}/{2})</color>", fromLevel, fromExp, fromReqExp));
                
                if (onLevelUp != null)
                {
                    SetResume(false);
                    onLevelUp?.Invoke(fromLevel);
                    await UniTask.WaitUntil(() => m_resume);
                }

                RefreshEventState(string.Empty, string.Empty, fromLevel, fromExp, fromReqExp, m_realTotalExp);

                if(m_eventDataTable.IsMaxLevel(m_eventType, fromLevel))
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Level is Max : {0}</color>", fromLevel));
                    break;
                }
            }
            else
            {
                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Increase Finished : {0} ({1}/{2})</color>", fromLevel, fromExp, fromReqExp));
            }
        }

        SetResume(false);
        onComplete?.Invoke();

        return true;

        float GetDelay(float time, int amount)
        {
            return time/(float)amount;
        }
    }

    public void SetResume(bool resume)
    {
        m_resume = resume;
    }

#endregion Publics


#region Privates

    private async UniTask<bool> IncreaseText_UntilLevelUp(int fromCount, int addCount, int reqExp, float delay)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} + {1} = {2}/{3}</color>", fromCount, addCount, fromCount + addCount, reqExp));

        if (fromCount == 0)
        {
            RefreshExpSlider(0, reqExp);
            RefreshExpText(0, reqExp);
        }
        
        for(int i=1; i <= addCount; i++)
        {
            MoveExpSlider(fromCount + i, reqExp, delay);
            RefreshExpText(fromCount + i, reqExp);

            await UniTask.Delay(TimeSpan.FromSeconds(delay));
        }

        return true;
    }
    
    private void RefreshReward(EventData targetEvent)
    {
        if(targetEvent.Rewards.Count == 1)
        {
            IReward reward = targetEvent.Rewards[0];
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Next Reward : {0} x {1} (Level {2})</color>", reward.Type, reward.GetAmountString(), targetEvent.Level));

            switch(targetEvent.ChestType)
            {
                case ChestType.Chest: 
                    RefreshRewardItem(m_boxImagePath);
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
            RefreshRewardItem(m_boxImagePath);
        }
    }

    private void RefreshRewardItem(string spriteName, string countString = null, float spriteSize = 80, bool isRibbon = false, bool isDouble = false)
    {
        m_rewardItemEvent.Initialize(spriteName, countString, spriteSize, isRibbon, isDouble);
    }

    private void RefreshTargetIcon(string targetImagePath)
    {
        if(string.IsNullOrEmpty(targetImagePath))
            return;
        
        m_targetImagePath = targetImagePath;
        m_targetItemImage.sprite = SpriteManager.GetSprite(m_targetImagePath);
    }

    private void RefreshExpSlider(int _currentExp, int _nextExp)
    {
        if (_nextExp <= 0) return;

        float percent = (float)_currentExp / (float)_nextExp;
        m_expSlider.value = percent;
    }

    private void MoveExpSlider(int _currentExp, int _nextExp, float duration)
    {
        if (_nextExp <= 0) return;

        float percent = (float)_currentExp / (float)_nextExp;
        m_expSlider.DOValue(percent, duration);
    }

    private void RefreshExpText(string text)
    {
        m_expText.SetText(text);
    }

    private void RefreshExpText(int _currentExp, int _nextExp)
    {
        m_expText.SetText(string.Format(textFormatExp, _currentExp, _nextExp));
    }

#endregion Privates
}
