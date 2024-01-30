using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public partial class SDKManager : SingletonMono<SDKManager>
{
    [Header("★ [Settings] SDK Manager")]
    public bool showLog = true;

    [Header("★ [Live] SDK Manager")]
    public static string deviceID;
    public static int appOpenCount = 0;
    public static string installDate = dateDefault;
    public static string userGroup = "user_A_set";

    public const string dateDefault = "19990101-00:00:00";
    private const string formatUserGroup = "user_{0}_set";
    private WaitForSecondsRealtime delay_1sec = new WaitForSecondsRealtime(1);
    private WaitForSecondsRealtime delay_2sec = new WaitForSecondsRealtime(2);

    ///<Summary>SDK 초기화. Awake()에서 사용.</Summary>
    public void Initialize(int _appOpenCount = 0, string _installDate = dateDefault, string _userGroup = "A", bool _isADFreeUser = false)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>AppOpenCount : {0} / InstallDate : {1} / UserGroup : {2} / IsADFreeUser = {3}</color>", _appOpenCount, _installDate, _userGroup, _isADFreeUser));

        deviceID = SystemInfo.deviceUniqueIdentifier;
        appOpenCount = _appOpenCount;
        installDate = _installDate;
        userGroup = string.Format(formatUserGroup, _userGroup);

        Initialize_AppsFlyer(ProjectManager.appsflyer_dev_key, ProjectManager.appStoreConnectId);     
        Initialize_Facebook();
        Initialize_Firebase();
        Initialize_IronSource(ProjectManager.ironSource_AppKey_AOS, ProjectManager.ironSource_AppKey_iOS, _isADFreeUser);
    }

    ///<Summary>SDK 사용 시작. 초기 씬이 완료되면 사용.</Summary>
    public void SDKStart()
    {
        Debug.Log(CodeManager.GetMethodName());

        StartCoroutine(CO_SDKStart());
    }

    private IEnumerator CO_SDKStart()
    {
        yield return delay_2sec;
        Start_Facebook();
        yield return delay_1sec;
        Start_IronSource();

        SendAnalytics_User_App_Open();

    }
}
