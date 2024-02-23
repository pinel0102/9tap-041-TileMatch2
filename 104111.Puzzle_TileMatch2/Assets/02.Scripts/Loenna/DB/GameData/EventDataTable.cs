using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class EventDataTable : Table<long, EventData>
{
    public EventDataTable(string path) : base(path)
	{
	}

	protected override void OnLoaded()
	{
		base.OnLoaded();

		foreach (var data in m_rowDataDic.Values)
		{
			data.CreateRewards();
		}
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns>Max Level 까지 필요한 총 Exp.</returns>
    public int GetMaxExp(GameEventType eventType)
    {
        var eventDatas = GetEventDatas(eventType);
        return eventDatas.Sum(data => data.EXP);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns>Max Level</returns>
    public int GetMinLevel(GameEventType eventType)
    {
        var eventDatas = GetEventDatas(eventType);
        var firstLevel = eventDatas.Where(data => data.EXP > 0).Min(data => data.Level);
        
        return Mathf.Max(0, firstLevel - 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns>Max Level</returns>
    public int GetMaxLevel(GameEventType eventType)
    {
        var eventDatas = GetEventDatas(eventType);
        return eventDatas.Max(data => data.Level);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="currentLevel"></param>
    /// <returns>Max Level 여부.</returns>
    public bool IsMaxLevel(GameEventType eventType, int currentLevel)
    {
        var eventDatas = GetEventDatas(eventType);
        return !eventDatas.Any(data => data.Level > currentLevel);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="currentLevel"></param>
    /// <returns>현재 레벨의 EventData</returns>
	public EventData GetEventData(GameEventType eventType, int currentLevel)
	{
		return m_rowDataDic
			.Values?
			.FirstOrDefault(
				data => 
					data.EventType == eventType && 
                    data.Level == currentLevel
			);
	}

    public EventData[] GetEventDatas(GameEventType eventType, int fromLevel_exclusive, int toLevel_inclusive)
	{
		return m_rowDataDic
			.Values?
			.Where(
				data => 
					data.EventType == eventType && 
                    data.Level > fromLevel_exclusive && data.Level <= toLevel_inclusive
			).ToArray();
	}

    /// <summary>
    /// 다음 레벨의 EventData가 존재하면 가져온다.
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="currentLevel"></param>
    /// <param name="eventData"></param>
    /// <returns>다음 레벨 존재 여부.</returns>
    public bool TryGetNextEventData(GameEventType eventType, int currentLevel, out EventData eventData)
	{
        eventData = default;
        var eventDatas = GetEventDatas(eventType);

		if (eventDatas.Count() <= 0)
		{
            UnityEngine.Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>eventDatas.Count() : {0}</color>", eventDatas.Count()));
			return false;
		}

		if (!eventDatas.Any(data => data.Level > currentLevel))
		{
            UnityEngine.Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>!eventDatas.Any(data => data.Level > currentLevel)/color>"));
			return false;
		}

		eventData = eventDatas.First(data => data.Level == currentLevel + 1);
        
        UnityEngine.Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>eventData.Level : {0} / EXP : {1}</color>", eventData.Level, eventData.EXP));
		
        return true;
	}

    public EventData[] GetEventDatas(GameEventType eventType) => m_rowDataDic.Values?.Where(data => data.EventType == eventType)?.OrderBy(data => data.Level).ToArray() ?? Array.Empty<EventData>();
    public int[] GetExpArray(GameEventType eventType) => GetEventDatas(eventType).Select(data => data.EXP).ToArray();
    public List<int> GetExpList(GameEventType eventType) => GetEventDatas(eventType).Select(data => data.EXP).ToList();
}
