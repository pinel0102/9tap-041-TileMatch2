using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

//! 액티비티 인디케이터 관리자
public static class ActivityIndicatorManager
{   
#if !UNITY_EDITOR && UNITY_IOS
    [DllImport("__Internal")]
    private static extern void HandleUnityMsg(string a_oCmd, string a_oMsg);
#endif

    public static void Initialize()
    {
        StopActivityIndicator();
    }

    //! 액티비티 인디케이터를 시작한다
    public static void StartActivityIndicator() 
    {
#if !UNITY_EDITOR && UNITY_IOS
        ActivityIndicatorManager.HandleUnityMsg("ActivityIndicator", "True");
#endif
    }

    //! 액티비티 인디케이터를 중지한다
    public static void StopActivityIndicator() 
    {
#if !UNITY_EDITOR && UNITY_IOS
        ActivityIndicatorManager.HandleUnityMsg("ActivityIndicator", "False");
#endif
    }
}
