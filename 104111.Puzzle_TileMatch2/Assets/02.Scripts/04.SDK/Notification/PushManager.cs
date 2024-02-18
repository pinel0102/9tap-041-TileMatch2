using UnityEngine;
using System;
using Gley.Notifications;

/// <summary>
/// Need Package : <see cref="Gley.Notifications"/>
/// </summary>
public static partial class PushManager
{
    private static bool m_showInternalLog = false;

    private static bool m_isInitialized = false;
    private static int m_totalDays = 0;
    private static string m_appTitle = string.Empty;
    private static DateTime m_installDate = default;

    public static void Initialize(string appTitle, DateTime installDate)
    {
        if (m_isInitialized) return;

        m_appTitle = appTitle;
        m_installDate = new DateTime(installDate.Year, installDate.Month, installDate.Day, 0, 0, 0);
        m_totalDays = (int)DateTime.Now.Subtract(m_installDate).TotalDays;

        Debug.Log(CodeManager.GetMethodName() + string.Format("totalDays : {0}", m_totalDays));

        pushOnceList.Clear();
        pushRepeatList.Clear();

        InitPushText();

        API.Initialize();
        API.CancelAllNotifications();
        
        m_isInitialized = true;
    }

    public static void PushAgree(bool agree)
    {
        if (!m_isInitialized)
        {
            Debug.LogWarning(CodeManager.GetMethodName() + "Call Initialize First");
            return;
        }

        API.CancelAllNotifications();

        if (agree)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("Register Pushes"));

            SetPush_Once();
            SetPush_Repeat();
            
            //PushTest();
        }
        else
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("Remove Pushes"));
        }
    }

    private static void PushTest()
    {
        Debug.Log(CodeManager.GetMethodName());

        API.SendNotification(m_appTitle, "test 1", new TimeSpan(0, 1, 0));
        API.SendNotification(m_appTitle, "test 2", new TimeSpan(0, 2, 0));
        API.SendNotification(m_appTitle, "test 3", new TimeSpan(0, 3, 0));
    }
}