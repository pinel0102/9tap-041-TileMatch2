using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static partial class GlobalDefine
{
    public const string EventName_SweetHolic = "Sweet Holic";
    public const string BoxName_SweetHolic = "UI_Img_Box_05";
    
    private readonly static Dictionary<DayOfWeek, string> EventCycle_SweetHolic = new Dictionary<DayOfWeek, string>()
    {
        [DayOfWeek.Monday]    = "Juice",
        [DayOfWeek.Tuesday]   = "Juice",
        [DayOfWeek.Wednesday] = "Juice",
        [DayOfWeek.Thursday]  = "Donut",
        [DayOfWeek.Friday]    = "Donut",
        [DayOfWeek.Saturday]  = "Lollipop",
        [DayOfWeek.Sunday]    = "Lollipop"
    };

    /// <Summary>
    /// <para>Not Showed Popup</para>
    /// <para>Required Level</para>
    /// </Summary>
    public static bool IsEnable_EventPopup_SweetHolic()
    {
        CheckSweetHolicExpired();

        bool openCheck = IsOpen_Event_SweetHolic();
        bool notShowedCheck = !globalData.userManager.Current.Event_SweetHolic_ShowedPopup;
        
        //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} / {1}</color>", openCheck, notShowedCheck));
        
        return openCheck && notShowedCheck;
    }

    /// <summary>
    /// <para>Target Item 갱신.</para>
    /// </summary>
    public static void SweetHolic_RefreshTarget()
    {
        string target = EventCycle_SweetHolic[DateTime.Today.DayOfWeek];
        globalData.eventSweetHolic_TargetName = target;
        globalData.eventSweetHolic_TargetIndex = globalData?.tableManager?.TileDataTable?.Dic?.FirstOrDefault(item => item.Value.Name.Equals(target)).Value?.Index ?? 0;
    }

    public static string GetSweetHolic_ItemImagePath()
    {
        return string.Format("UI_Img_{0}", globalData.eventSweetHolic_TargetName);
    }

    private static void CheckSweetHolicExpired()
    {
        if (IsExpired(ToDateTime(globalData.userManager.Current.Event_SweetHolic_EndDate)))
        {
            var (StartDate, EndDate, TargetName) = GetNextEventTime();

            globalData.userManager.UpdateEvent_SweetHolic(
                TotalExp: 0,
                ShowedPopup: false,
                StartDate: TimeToString(StartDate),
                EndDate: TimeToString(EndDate),
                ExpiredSweetHolicBoosterAt: DateTimeOffset.Now
            );

            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Expired] Event Time : {0} ~ {1} / ItemName : {2}</color>", globalData.userManager.Current.Event_SweetHolic_StartDate, globalData.userManager.Current.Event_SweetHolic_EndDate, TargetName));

            CheckEventRefresh();
            globalData.fragmentHome?.Refresh(globalData.userManager.Current);
        }

        (DateTime, DateTime, string) GetNextEventTime()
        {
            DateTime startDate = DateTime.Today;
            string targetName = EventCycle_SweetHolic[startDate.DayOfWeek];

            for(int i=1; i < 7; i++)
            {
                if (EventCycle_SweetHolic[startDate.AddDays(i).DayOfWeek] != targetName)
                {
                    return (startDate, startDate.AddDays(i), targetName);
                }
            }

            return (startDate, startDate.AddDays(7), targetName);
        }
    }
}
