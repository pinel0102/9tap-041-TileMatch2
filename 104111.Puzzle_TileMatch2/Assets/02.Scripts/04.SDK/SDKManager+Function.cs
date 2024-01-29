using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

public partial class SDKManager
{
    private const string dateDefault_HHmmss = "19990101-00:00:00";
    private const string dateFormat_HHmmss = "yyyyMMdd-HH:mm:ss";
    private const string remainTimeFormat = "{0:D2}:{1:D2}:{2:D2}";

#region DateTime

    public static string GetCurrentTime(string format = dateFormat_HHmmss)
    {
        return DateTime.Now.ToString(format);
    }

    public static DateTime ToDateTime(string timeString, string format = dateFormat_HHmmss)
    {
        if (string.IsNullOrEmpty(timeString))
            timeString = DateTime.ParseExact(dateDefault_HHmmss, format, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString(format);
        
        if(DateTime.TryParseExact(timeString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            return result;
        
        return DateTime.ParseExact(DateTime.Parse(timeString, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString(format), 
                                format, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    public static string TimeToString(DateTime dateTime, string format = dateFormat_HHmmss)
    {
        return dateTime.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string GetRemainTimeString(int totalSeconds)
    {
        return string.Format(remainTimeFormat, totalSeconds / 3600, (totalSeconds / 60) % 60, totalSeconds % 60);
    }

    public static DateTime GetPacificStandardTime(DateTime from)
    {
        //find TimeZoneInfo of PST
        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        //Convert time from Local to PST
        return TimeZoneInfo.ConvertTime(from, TimeZoneInfo.Local, tzi);
    }

#endregion DateTime


#region Event Time

    public static bool IsWeekend(int userCurrentLevel, bool ignoreClearLevel = false)
    {
        bool userCondition = ignoreClearLevel ? true : userCurrentLevel >= 10;
        DateTime nowDt = DateTime.Now;
        
        return userCondition && (nowDt.DayOfWeek == DayOfWeek.Saturday || nowDt.DayOfWeek == DayOfWeek.Sunday);
    }

    public static bool IsRefreshTime(DateTime date)
    {
        return DateTime.Now.Subtract(date).TotalSeconds >= 0;
    }

    public static int GetSeconds_UntilTommorow()
    {
        TimeSpan sp = DateTime.Today.AddDays(1) - DateTime.Now;
        return (int)sp.TotalSeconds;
    }

    public static int GetSeconds_UntilMonday()
    {
        for(int i=1; i < 7; i++)
        {
            if (DateTime.Today.AddDays(i).DayOfWeek == DayOfWeek.Monday)
            {
                TimeSpan sp = DateTime.Today.AddDays(i) - DateTime.Now;
                return (int)sp.TotalSeconds;
            }
        }
        
        return 0;
    }

    /*public static bool IsHardBundleTime(int level)
    {
        int index = gameManager.levelSummaries.FindIndex(item => item.level == level);
        if (index > -1)
        {
            if (gameManager.levelSummaries[index].difficulty != 0)
            {
                //Debug.Log(CodeManager.GetMethodName() + string.Format("Hard Bundle Time : {0}", ObscuredPrefs.GetString("HARD_BUNDLE_TIME", "none")));
                if (IsRefreshTime(ObscuredPrefs.GetString("HARD_BUNDLE_TIME", "none")))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsRefreshTime(string date)
    {
        if (DateTime.TryParse(date, out DateTime o))
            return IsRefreshTime(o);
        else
            return true;
    }*/

#endregion Event Time    

}
