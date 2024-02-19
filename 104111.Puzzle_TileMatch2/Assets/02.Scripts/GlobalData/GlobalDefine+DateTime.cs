using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Diagnostics;
using System.Drawing;

public static partial class GlobalDefine
{
#region DateTime

    public const string dateDefault_HHmmss = "19990101-00:00:00";
    public const string dateFormat_HHmmss = "yyyyMMdd-HH:mm:ss";
    public const string remainTimeFormat = "{0:D2}:{1:D2}:{2:D2}";

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

    public static bool IsWeekend()
    {
        DateTime nowDt = DateTime.Now;
        return nowDt.DayOfWeek == DayOfWeek.Saturday || nowDt.DayOfWeek == DayOfWeek.Sunday;
    }

    public static bool IsExpired(DateTime date)
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

    public static DateTime GetNextWeekendStart()
    {
        for(int i=1; i < 7; i++)
        {
            if (DateTime.Today.AddDays(i).DayOfWeek == DayOfWeek.Saturday)
            {
                return DateTime.Today.AddDays(i);
            }
        }

        return DateTime.Today.AddDays(7);
    }

    public static DateTime GetNextBundleTime(TimeSpan addTime)
    {
        return DateTime.Now.Add(addTime);
    }

    public static string GetRemainEventTime(string endDate)
    {
        return GetRemainEventTime(ToDateTime(endDate));
    }

    public static string GetRemainEventTime(DateTime endDate)
    {
        TimeSpan span = endDate.Subtract(DateTime.Now);
        return span.Days > 0 ? string.Format("{0}d {1}h", span.Days, span.Hours) 
            : span.Hours > 0 ?string.Format("{0}h {1}m", span.Hours, span.Minutes)
            : span.Minutes > 0 ?string.Format("{0}m {1}s", span.Minutes, span.Seconds)
            : string.Format("{0}s", span.Seconds);
    }

#endregion Event Time
}
