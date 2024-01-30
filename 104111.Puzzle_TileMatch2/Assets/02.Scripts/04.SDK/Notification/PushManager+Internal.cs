using UnityEngine;
using System.Collections.Generic;
using System;

public static partial class PushManager
{
    private readonly static List<PushOnce> pushOnceList = new List<PushOnce>();    
    private readonly static List<PushRepeat> pushRepeatList = new List<PushRepeat>();

    private static void SetPush_Once()
    {
        DateTime now = DateTime.Now;
        for(int i=0; i < pushOnceList.Count; i++)
        {
            for (int j=0; j < pushOnceList[i].PushText.Count; j++)
            {
                DateTime pushStartDay = m_installDate.AddDays(pushOnceList[i].StartDay[j]);
                DateTime pushStartTime = new DateTime(pushStartDay.Year, pushStartDay.Month, pushStartDay.Day, pushOnceList[i].PushTime.Hours, pushOnceList[i].PushTime.Minutes, pushOnceList[i].PushTime.Seconds);
                TimeSpan pushDelay = pushStartTime.Subtract(now);

                if (pushDelay.TotalSeconds > 0)
                {
                    GleyNotifications.SendNotification(m_appTitle, pushOnceList[i].PushText[j], pushDelay);
                    
                    if(m_showLog) Debug.Log(CodeManager.GetMethodName() + string.Format("pushStartTime[{0}][{1}] : {2} / pushDelay : {3} / text : {4}", i, j, pushStartTime, pushDelay, pushOnceList[i].PushText[j]));
                }
            }
        }
    }

    private static void SetPush_Repeat()
    {
        DateTime now = DateTime.Now;
        for(int i=0; i < pushRepeatList.Count; i++)
        {
            DateTime pushStartDay = now.AddDays(pushRepeatList[i].StartDay);
            DateTime pushStartTime = new DateTime(pushStartDay.Year, pushStartDay.Month, pushStartDay.Day, pushRepeatList[i].PushTime.Hours, pushRepeatList[i].PushTime.Minutes, pushRepeatList[i].PushTime.Seconds);

            for (int j=0; j < pushRepeatList[i].PushText.Count; j++)
            {
                TimeSpan pushDelay = pushStartTime.AddDays(j).Subtract(now);
                if (pushDelay.TotalSeconds > 0)
                {
                    GleyNotifications.SendRepeatNotification(m_appTitle, pushRepeatList[i].PushText[j], pushDelay, pushRepeatList[i].Interval, null, null, string.Empty);
                    
                    if(m_showLog) Debug.Log(CodeManager.GetMethodName() + string.Format("pushStartTime[{0}][{1}] : {2} / pushDelay : {3} / interval : {4} / text : {5}", i, j, pushStartTime.AddDays(j), pushDelay, pushRepeatList[i].Interval, pushRepeatList[i].PushText[j]));
                }
            }
        }
    }
}
