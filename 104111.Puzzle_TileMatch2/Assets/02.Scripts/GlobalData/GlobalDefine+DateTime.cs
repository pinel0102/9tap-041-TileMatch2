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
    private const string remainTimeFormat = "{0:D2}:{1:D2}:{2:D2}";

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
}
