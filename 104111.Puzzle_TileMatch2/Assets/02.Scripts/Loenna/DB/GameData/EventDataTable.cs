using System;
using System.Collections.Generic;
using System.Linq;

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
    /// Total Exp로 현재 레벨을 계산.
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="totalExp"></param>
    /// <returns>Current Level, Current Exp</returns>
    public (int, int) GetCurrentLevel(GameEventType eventType, int totalExp)
    {
        var eventDatas = GetEventDatas(eventType);
        int currentLevel = 0;
        int currentExp = totalExp;
        
        for(int i=0; i < eventDatas.Length; i++)
        {
            if (currentExp >= eventDatas[i].EXP)
            {
                currentExp -= eventDatas[i].EXP;
                currentLevel++;
            }
            else break;
        }

        return (currentLevel, currentExp);
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
			return false;
		}

		if (!eventDatas.Any(data => data.Level > currentLevel))
		{
			return false;
		}

		eventData = eventDatas.First(data => data.Level > currentLevel);
		return true;
	}

    public EventData[] GetEventDatas(GameEventType eventType) => m_rowDataDic.Values?.Where(data => data.EventType == eventType)?.OrderBy(data => data.Level).ToArray() ?? Array.Empty<EventData>();
}
