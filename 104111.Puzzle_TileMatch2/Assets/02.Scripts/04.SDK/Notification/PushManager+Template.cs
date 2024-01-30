using UnityEngine;
using System.Collections.Generic;
using System;

public static partial class PushManager
{
#region Push Templates

    private readonly static TimeSpan pushOnceTime = new TimeSpan(12, 0, 0);
    private readonly static List<KeyValuePair<int, string>> pushOnceTemplate = new List<KeyValuePair<int, string>>
    {
        //new (1, "Time to play the \"Tile Match world tours\"!!"),
        //new (2, "Need more fun? PLAY NOW"),
        //new (3, "Come on! It’s time to play \"Tile Match world tours\"!!"),
        //new (5, "Time to play the \"Tile Match world tours\"!"),
        //new (7, "Need more fun? PLAY NOW"),
    };

    private readonly static TimeSpan pushRepeatTime1 = new TimeSpan(12, 0, 0);
    private readonly static List<string> pushRepeatTemplate1 = new List<string>
    {
        "Time to play the \"Tile Match world tours\"!!",
        "Need more fun? PLAY NOW",
        "Come on! It’s time to play \"Tile Match world tours\"!!",
        "Time to play the \"Tile Match world tours\"!",
        "Need more fun? PLAY NOW"
    };

    private readonly static TimeSpan pushRepeatTime2 = new TimeSpan(19, 0, 0);    
    private readonly static List<string> pushRepeatTemplate2 = new List<string>
    {
        "Don’t forget to claim your daily reward!",
        "Your daily reward is ready! Come and get!",
        "Play \"Tile Match world tours\", and get daily reward!",
        "Don't miss out on your daily reward!"
    };

#endregion Push Templates


#region Register Templates

    private static void InitPushText()
    {
        //pushOnceList.Add(new PushOnce(SortKV(pushOnceTemplate, m_totalDays), pushOnceTime));
        pushRepeatList.Add(new PushRepeat(SortList(pushRepeatTemplate1, m_totalDays), pushRepeatTime1));
        pushRepeatList.Add(new PushRepeat(SortList(pushRepeatTemplate2, m_totalDays), pushRepeatTime2));
    }

    private static List<string> SortList(List<string> origin, int startIndex)
    {
        List<string> newList = new List<string>();

        for(int i=0; i < origin.Count; i++)
        {
            newList.Add(origin[(startIndex + i) % origin.Count]);
        }

        return newList;
    }

    private static List<KeyValuePair<int, string>> SortKV(List<KeyValuePair<int, string>> origin, int startIndex)
    {
        List<KeyValuePair<int, string>> newKV = new List<KeyValuePair<int, string>>();

        for(int i=0; i < origin.Count; i++)
        {
            newKV.Add(origin[(startIndex + i) % origin.Count]);
        }

        return newKV;
    }

#endregion Register Templates


#region Structs

    private struct PushOnce
    {
        ///<Summary>알림 텍스트 내용. (발송 후 다음 인덱스로 갱신.)</Summary>
        public List<string> PushText { get; private set; }
        ///<Summary>매일 해당 시각에 발송.</Summary>
        public TimeSpan PushTime { get; private set; }
        ///<Summary>인스톨 일자 기준 n일 후 시작. (1회.)</Summary>
        public List<int> StartDay { get; private set; }

        public PushOnce(List<string> _pushText, TimeSpan _pushTime, List<int> _startDay)
        {
            PushText = _pushText;
            PushTime = _pushTime;
            StartDay = _startDay;
        }

        public PushOnce(List<KeyValuePair<int, string>> _pushTemplate, TimeSpan _pushTime)
        {
            PushText = new List<string>();
            StartDay = new List<int>();

            for(int i=0; i < _pushTemplate.Count; i++)
            {
                PushText.Add(_pushTemplate[i].Value);
                StartDay.Add(_pushTemplate[i].Key);
            }

            PushTime = _pushTime;
        }
    }

    private struct PushRepeat
    {
        ///<Summary>알림 텍스트 내용. (발송 후 다음 인덱스로 갱신.)</Summary>
        public List<string> PushText { get; private set; }
        ///<Summary>매일 해당 시각에 발송.</Summary>
        public TimeSpan PushTime { get; private set; }
        ///<Summary>현재 시각 기준 n일 후 시작. (반복.)</Summary>
        public int StartDay { get; private set; }
        ///<Summary>동일한 텍스트가 다시 나올 때까지 걸리는 기간.</Summary>
        public TimeSpan Interval { get; private set; }

        public PushRepeat(List<string> _pushText, TimeSpan _pushTime, int _startDay = 1)
        {
            PushText = _pushText;
            PushTime = _pushTime;
            StartDay = _startDay;
            Interval = new TimeSpan(24 * _pushText.Count, 0, 0);
        }
    }

#endregion Structs
}
