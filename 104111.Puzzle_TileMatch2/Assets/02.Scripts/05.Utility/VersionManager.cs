//#define USE_NINETAP_MODULE	// Ninetap Module 사용 여부.
//#define USE_GAME_ANALYTICS	// GameAnalytics SDK 사용시 버전 동기화 여부.
#define USE_PROJECT_MANAGER	// ProjectManager 클래스 사용 여부.
#define HIDE_REMOVE_PREFS	// EditorPrefs 삭제 메뉴 숨김 여부.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

/// <author>Pinelia Luna</author>
/// <Summary>VersionManager v1.6
/// <para>Android / iOS / macOS 통합 앱 버전 관리용 클래스.</para>
/// <para>유니티 에디터 상단의 Version 메뉴에서 앱 버전 관리를 지원합니다.</para>
/// <para>(int)Major.(int)Minor.(int)Patch ((int)Build) 이며, 0.0.1 (build 1) 부터 시작합니다.</para>
/// <para>상위 버전이 올라가면 하위 버전은 00부터 시작합니다.</para>
/// <para>버전이 올라가면 Build Number도 같이 올라갑니다.</para>
/// <para>Adjust 메뉴에서 각 수치를 미세 조정할 수 있습니다.</para>
/// </Summary>
[InitializeOnLoad]
#endif
public class VersionManager
{
#if UNITY_EDITOR

    private static bool showLog = false;

    #if USE_NINETAP_MODULE

    /// <Summary>ProjInfoTable 오버라이드용 구조체.
    /// <para>Override ProjInfoTable이 켜져 있으면 ProjInfoTable을 이 구조체의 내용으로 업데이트합니다.</para>	
    /// </Summary>
    private struct ProjecInfo
    {		
        public ProjecInfo(bool useProjectManager = false)
        {
            if (!useProjectManager)
            {
                projectName = "201911.Genre_GameNameA";
                productName = "Game Name";
                shortProductName = "Game Name";
                appIdentifier = "com.ninetap.gamename";
                appStoreConnectId = "1569121045";

                companyName = "9tap";
                servicesURL = "https://www.ninetap.com/terms_of_service.html";
                privacyURL = "https://www.ninetap.com/privacy_policy.html";
                storeURLAndroid = string.Format("https://play.google.com/store/apps/details?id={0}", appIdentifier);
                storeURLiOS = string.Format("https://apps.apple.com/app/id{0}", appStoreConnectId);
                supportsMail = "cs@ninetap.com";
            }
            else
            {
                projectName = ProjectManager.projectName;
                productName = ProjectManager.productName;
                shortProductName = ProjectManager.shortProductName;
                appIdentifier = ProjectManager.appIdentifier;
                appStoreConnectId = ProjectManager.appStoreConnectId;

                companyName = ProjectManager.companyName;
                servicesURL = ProjectManager.servicesURL;
                privacyURL = ProjectManager.privacyURL;
                storeURLAndroid = string.Format(ProjectManager.storeURLAndroid, ProjectManager.appIdentifier);
                storeURLiOS = string.Format(ProjectManager.storeURLiOS, ProjectManager.appStoreConnectId);
                supportsMail = ProjectManager.supportsMail;
            }
        }
        public string projectName, productName, shortProductName, appIdentifier, appStoreConnectId;
        public string companyName, servicesURL, privacyURL, storeURLAndroid, storeURLiOS, supportsMail;
    }
        
    private static bool overrideProjInfoTable;
    private const bool overrideProjInfoTableDefault = true;
    private const string menuOverrideProjInfoTable = "Version/Override ProjInfoTable";
    private const string prefOverrideProjInfoTable = "/VersionManager.OverrideProjInfoTable";

    #endif

    public static bool isEnable { get {return useManager;} }
    private static bool useManager;
    private static string bundleVersion;
    private static int[] bundleVersionArray;
    private static int buildNumber;
    private const bool useManagerDefault = true;
    private const string versionFormat = "{0}.{1:0}.{2:0}";
    private const string iniBundleVersion = "0.0.1";
    private const int iniBuildNumber = 1;
    private const string menuCurrentVersion = "Version/Log Current Version";
    private const string menuUseManager = "Version/Use Version Manager";
    private const string menuResetVersion = "Version/Reset Version";
    private const string menuRemovePrefs = "Version/Remove Prefs";	
    private const string menuIncreaseMajorVersion = "Version/Major Version +1 (Build +1)";
    private const string menuIncreaseMinorVersion = "Version/Minor Version +1 (Build +1)";
    private const string menuIncreasePatchVersion = "Version/Patch Version +1 (Build +1)";
    private const string menuIncreaseBuildNumber = "Version/Build Number +1";
    private const string menuIncreaseMajorVersionN = "Version/Adjust Version (+)/Major Version +1";
    private const string menuIncreaseMinorVersionN = "Version/Adjust Version (+)/Minor Version +1";
    private const string menuIncreasePatchVersionN = "Version/Adjust Version (+)/Patch Version +1";
    private const string menuIncreaseBuildNumberN0 = "Version/Adjust Version (+)/Build Number +1";
    private const string menuIncreaseBuildNumberN1 = "Version/Adjust Version (+)/Build Number +5";
    private const string menuIncreaseBuildNumberN2 = "Version/Adjust Version (+)/Build Number +10";
    private const string menuDecreaseMajorVersionN = "Version/Adjust Version (-)/Major Version -1";
    private const string menuDecreaseMinorVersionN = "Version/Adjust Version (-)/Minor Version -1";
    private const string menuDecreasePatchVersionN = "Version/Adjust Version (-)/Patch Version -1";
    private const string menuDecreaseBuildNumberN0 = "Version/Adjust Version (-)/Build Number -1";
    private const string menuDecreaseBuildNumberN1 = "Version/Adjust Version (-)/Build Number -5";
    private const string menuDecreaseBuildNumberN2 = "Version/Adjust Version (-)/Build Number -10";
    private const string sessionInitialized = "VersionManager.Initialized";
    private const string prefUseManager = "/VersionManager.UseManager";	
    private const string prefBundleVersion = "/VersionManager.BundleVersion";
    private const string prefBuildNumber = "/VersionManager.BuildNumber";	

    static VersionManager()
    {
        if (!SessionState.GetBool(sessionInitialized, false))
        {
            if (showLog)
                Debug.Log(CodeManager.GetMethodName());

            ReadPrefs();
            LogVersionManager();
            SavePrefs();
            
            EditorApplication.delayCall += () =>
            {
                Menu.SetChecked(menuUseManager, useManager);
                
                #if USE_NINETAP_MODULE
                Menu.SetChecked(menuOverrideProjInfoTable, overrideProjInfoTable);
                #endif
            };

            SessionState.SetBool(sessionInitialized, true);
        }
    }

    [DidReloadScripts]
    private static void CheckVersion()
    {
        if (showLog)
            Debug.Log(CodeManager.GetMethodName());

        ReadPrefs();

        if (useManager)
        {
            if (PlayerSettings.bundleVersion != bundleVersion || PlayerSettings.Android.bundleVersionCode != buildNumber)
                UpdateVersion(bundleVersion, buildNumber);

#if USE_PROJECT_MANAGER
            if (PlayerSettings.productName != ProjectManager.productName || PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) != ProjectManager.appIdentifier)
                UpdateVersion(bundleVersion, buildNumber);
#endif
        }
    }

    // ~/Library/Preferences/com.unity3d.UnityEditor5.x.plist
    private static void ReadPrefs()
    {
        useManager = EditorPrefs.GetBool(Application.identifier + prefUseManager, useManagerDefault);
        bundleVersion = EditorPrefs.GetString(Application.identifier + prefBundleVersion, GetVersionFormat(GetVersionArray(PlayerSettings.bundleVersion)));		
        buildNumber = EditorPrefs.GetInt(Application.identifier + prefBuildNumber, PlayerSettings.Android.bundleVersionCode);
    
    #if USE_NINETAP_MODULE
        overrideProjInfoTable = EditorPrefs.GetBool(Application.identifier + prefOverrideProjInfoTable, overrideProjInfoTableDefault);
    #endif

        if (buildNumber < iniBuildNumber)
            buildNumber = iniBuildNumber;

        CheckVersionArray();		
    }

    private static void SavePrefs()
    {
        EditorPrefs.SetBool(Application.identifier + prefUseManager, useManager);
        EditorPrefs.SetString(Application.identifier + prefBundleVersion, PlayerSettings.bundleVersion);		
        EditorPrefs.SetInt(Application.identifier + prefBuildNumber, PlayerSettings.Android.bundleVersionCode);

    #if USE_NINETAP_MODULE
        EditorPrefs.SetBool(Application.identifier + prefOverrideProjInfoTable, overrideProjInfoTable);
    #endif

        CheckVersionArray();
    }

    private static void UpdateVersion(string newVersion, int newBuildNumber)
    {
        if (newBuildNumber < iniBuildNumber)
            newBuildNumber = iniBuildNumber;

        UpdatePlayerSettings(newVersion, newBuildNumber);
        
        SavePrefs();
        LogCurrentVersion();
    }

    private static void UpdatePlayerSettings(string newVersion, int newBuildNumber)
    {
        PlayerSettings.bundleVersion = newVersion;
        PlayerSettings.Android.bundleVersionCode = newBuildNumber;
        PlayerSettings.iOS.buildNumber = newBuildNumber.ToString();
        PlayerSettings.macOS.buildNumber = newBuildNumber.ToString();

#if USE_PROJECT_MANAGER
        PlayerSettings.companyName = ProjectManager.companyName;
        PlayerSettings.productName = ProjectManager.productName;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ProjectManager.appIdentifier);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, ProjectManager.appIdentifier);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, ProjectManager.appIdentifier);
#endif

#if USE_NINETAP_MODULE
        OverrideProjInfoTable(newVersion, newBuildNumber);
#endif

#if USE_GAME_ANALYTICS
        OverrideGameAnalytics(newVersion);
#endif

        AssetDatabase.SaveAssets();
    }

    private static void LogVersionManager()
    {
        if (useManager)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#FFFF00>Version Manager ON : ver {0} ({1})</color>", PlayerSettings.bundleVersion, PlayerSettings.Android.bundleVersionCode));

            #if USE_NINETAP_MODULE
            LogOverrideProjInfoTable();
            #endif
        }
    }

    [MenuItem(menuCurrentVersion, false, 1)]
    public static void LogCurrentVersion()
    {
        if (useManager)
            CheckVersion();
        
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#FFFF00>ver {0} ({1})</color>", PlayerSettings.bundleVersion, PlayerSettings.Android.bundleVersionCode));
    }
    
    [MenuItem(menuUseManager, false, 101)]
    private static void ToggleVersionManager()
    {	
        useManager = !useManager;
        EditorPrefs.SetBool(Application.identifier + prefUseManager, useManager);
        Menu.SetChecked(menuUseManager, useManager);
        LogVersionManager();

        CheckVersionArray();	

        if (useManager)		
            CheckVersion();
    }

    [MenuItem(menuIncreaseMajorVersion, false, 501)]
    private static void IncreaseMajorVersion()
    {
        CheckVersionArray();
        bundleVersionArray[0]++;
        bundleVersionArray[1] = 0;
        bundleVersionArray[2] = 0;
        buildNumber++;		
        bundleVersion = GetVersionFormat(bundleVersionArray);
        UpdateVersion(bundleVersion, buildNumber);
    }

    [MenuItem(menuIncreaseMinorVersion, false, 502)]
    private static void IncreaseMinorVersion()
    {
        CheckVersionArray();
        bundleVersionArray[1]++;
        bundleVersionArray[2] = 0;
        buildNumber++;		
        bundleVersion = GetVersionFormat(bundleVersionArray);
        UpdateVersion(bundleVersion, buildNumber);
    }	

    [MenuItem(menuIncreasePatchVersion, false, 503)]
    private static void IncreasePatchVersion()
    {
        CheckVersionArray();
        bundleVersionArray[2]++;
        buildNumber++;		
        bundleVersion = GetVersionFormat(bundleVersionArray);
        UpdateVersion(bundleVersion, buildNumber);
    }	

    [MenuItem(menuIncreaseBuildNumber, false, 504)]
    private static void IncreaseBuildNumber()
    {
        CheckVersionArray();
        buildNumber++;		
        bundleVersion = GetVersionFormat(bundleVersionArray);
        UpdateVersion(bundleVersion, buildNumber);
    }

    [MenuItem(menuIncreaseMajorVersionN, false, 701)]
    private static void IncreaseMajorVersionN()
    {
        CheckVersionArray();		
        bundleVersionArray[0] += 1;
        bundleVersion = GetVersionFormat(bundleVersionArray);
        UpdateVersion(bundleVersion, buildNumber);
    }

    [MenuItem(menuIncreaseMinorVersionN, false, 702)]
    private static void IncreaseMinorVersionN()
    {
        CheckVersionArray();		
        bundleVersionArray[1] += 1;
        bundleVersion = GetVersionFormat(bundleVersionArray);
        UpdateVersion(bundleVersion, buildNumber);
    }	

    [MenuItem(menuIncreasePatchVersionN, false, 703)]
    private static void IncreasePatchVersionN()
    {
        CheckVersionArray();
        bundleVersionArray[2] += 1;
        bundleVersion = GetVersionFormat(bundleVersionArray);
        UpdateVersion(bundleVersion, buildNumber);
    }

    [MenuItem(menuIncreaseBuildNumberN0, false, 704)]
    private static void IncreaseBuildNumberN0()
    {	
        CheckVersionArray();
        buildNumber += 1;
        bundleVersion = GetVersionFormat(bundleVersionArray);
        UpdateVersion(bundleVersion, buildNumber);
    }

    [MenuItem(menuIncreaseBuildNumberN1, false, 705)]
    private static void IncreaseBuildNumberN1()
    {	
        CheckVersionArray();
        buildNumber += 5;
        bundleVersion = GetVersionFormat(bundleVersionArray);
        UpdateVersion(bundleVersion, buildNumber);
    }

    [MenuItem(menuIncreaseBuildNumberN2, false, 706)]
    private static void IncreaseBuildNumberN2()
    {	
        CheckVersionArray();
        buildNumber += 10;
        bundleVersion = GetVersionFormat(bundleVersionArray);
        UpdateVersion(bundleVersion, buildNumber);
    }

    [MenuItem(menuDecreaseMajorVersionN, false, 711)]
    private static void DecreaseMajorVersionN()
    {
        if (bundleVersion != iniBundleVersion)
        {
            CheckVersionArray();
            if (bundleVersionArray[0] >= 1)			
                bundleVersionArray[0] -= 1;
            bundleVersion = GetVersionFormat(bundleVersionArray);
            CheckVersionZero();			
            UpdateVersion(bundleVersion, buildNumber);
        }
    }

    [MenuItem(menuDecreaseMinorVersionN, false, 712)]
    private static void DecreaseMinorVersionN()
    {
        if (bundleVersion != iniBundleVersion)
        {
            CheckVersionArray();
            if (bundleVersionArray[1] >= 1)			
                bundleVersionArray[1] -= 1;
            bundleVersion = GetVersionFormat(bundleVersionArray);
            CheckVersionZero();
            UpdateVersion(bundleVersion, buildNumber);
        }
    }	

    [MenuItem(menuDecreasePatchVersionN, false, 713)]
    private static void DecreasePatchVersionN()
    {
        if (bundleVersion != iniBundleVersion)
        {
            CheckVersionArray();
            if (bundleVersionArray[2] >= 1)
                bundleVersionArray[2] -= 1;
            bundleVersion = GetVersionFormat(bundleVersionArray);
            CheckVersionZero();
            UpdateVersion(bundleVersion, buildNumber);
        }		
    }

    [MenuItem(menuDecreaseBuildNumberN0, false, 714)]
    private static void DecreaseBuildNumberN0()
    {
        if (buildNumber != iniBuildNumber)
        {
            CheckVersionArray();
            if (buildNumber > 1)
                buildNumber -= 1;
            bundleVersion = GetVersionFormat(bundleVersionArray);
            UpdateVersion(bundleVersion, buildNumber);
        }		
    }

    [MenuItem(menuDecreaseBuildNumberN1, false, 715)]
    private static void DecreaseBuildNumberN1()
    {
        if (buildNumber != iniBuildNumber)
        {
            CheckVersionArray();
            if (buildNumber > 5)
                buildNumber -= 5;
            else
                buildNumber = iniBuildNumber;
            bundleVersion = GetVersionFormat(bundleVersionArray);
            UpdateVersion(bundleVersion, buildNumber);
        }		
    }

    [MenuItem(menuDecreaseBuildNumberN2, false, 716)]
    private static void DecreaseBuildNumberN2()
    {
        if (buildNumber != iniBuildNumber)
        {
            CheckVersionArray();
            if (buildNumber > 10)
                buildNumber -= 10;
            else
                buildNumber = iniBuildNumber;
            bundleVersion = GetVersionFormat(bundleVersionArray);
            UpdateVersion(bundleVersion, buildNumber);
        }		
    }

    [MenuItem(menuResetVersion, false, 901)]
    private static void ResetVersion()
    {
        bundleVersion = iniBundleVersion;
        buildNumber = iniBuildNumber;
        UpdateVersion(iniBundleVersion, iniBuildNumber);
    }

    private static void CheckVersionZero()
    {
        if (bundleVersion == "0.0.0")
            bundleVersion = iniBundleVersion;
    }

    private static void CheckVersionArray()
    {
        bundleVersionArray = GetVersionArray(PlayerSettings.bundleVersion);
    }

    private static string GetVersionFormat(int[] versionArray)
    {		
        if (versionArray.Length != 3)
            versionArray = new int[3] {0, 0, 1};
        
        return string.Format(versionFormat, versionArray[0], versionArray[1], versionArray[2]);
    }

    public static int[] GetVersionArray(string versionString)
    {
        string[] arr = versionString.Split('.');
        
        if (arr.Length != 3)
            arr = new string[3] {"0", "0", "1"};

        int[] intArr = new int[3] {0, 0, 1};

        for(int i=0; i<arr.Length; i++)
        {
            int.TryParse(arr[i], out intArr[i]);
        }
        return intArr;
    }

    [MenuItem(menuIncreaseMajorVersion, true)]    
    [MenuItem(menuIncreaseMinorVersion, true)]
    [MenuItem(menuIncreasePatchVersion, true)]
    [MenuItem(menuIncreaseBuildNumber, true)]
    [MenuItem(menuIncreaseMajorVersionN, true)]
    [MenuItem(menuIncreaseMinorVersionN, true)]
    [MenuItem(menuIncreasePatchVersionN, true)]
    [MenuItem(menuIncreaseBuildNumberN0, true)]
    [MenuItem(menuIncreaseBuildNumberN1, true)]
    [MenuItem(menuIncreaseBuildNumberN2, true)]
    [MenuItem(menuDecreaseMajorVersionN, true)]
    [MenuItem(menuDecreaseMinorVersionN, true)]
    [MenuItem(menuDecreasePatchVersionN, true)]
    [MenuItem(menuDecreaseBuildNumberN0, true)]
    [MenuItem(menuDecreaseBuildNumberN1, true)]
    [MenuItem(menuDecreaseBuildNumberN2, true)]
    private static bool ValidateOn()
    {   return useManager;    }

    [MenuItem(menuResetVersion, true)]
    private static bool ValidateOff()
    {   return !useManager;    }

#if !HIDE_REMOVE_PREFS

    [MenuItem(menuRemovePrefs, false, 1001)]
    private static void RemovePrefs()
    {
        Debug.Log(CodeManager.GetMethodName());

        EditorPrefs.DeleteKey(Application.identifier + prefUseManager);
        EditorPrefs.DeleteKey(Application.identifier + prefBundleVersion);
        EditorPrefs.DeleteKey(Application.identifier + prefBuildNumber);

        #if USE_NINETAP_MODULE
        EditorPrefs.DeleteKey(Application.identifier + prefOverrideProjInfoTable);
        #endif
    }

    [MenuItem(menuRemovePrefs, true)]
    private static bool ValidateRemovePrefs()
    {   return !useManager;    }

#endif

#if USE_NINETAP_MODULE

    private static void OverrideProjInfoTable(string newVersion, int newBuildNumber)
    {
        if (overrideProjInfoTable)
        {
            if (CPlatformOptsSetter.ProjInfoTable != null)
            {
                if (showLog)
                    Debug.Log(CodeManager.GetMethodName() + "<color=#FFFF00>Override ProjInfoTable</color>");

            #if USE_PROJECT_MANAGER
                ProjecInfo _pInfo = new ProjecInfo(true);
            #else
                ProjecInfo _pInfo = new ProjecInfo();
            #endif

                var oProjInfoTable = CPlatformOptsSetter.ProjInfoTable;

                oProjInfoTable.SetCompanyName(_pInfo.companyName);
                oProjInfoTable.SetServicesURL(_pInfo.servicesURL);
                oProjInfoTable.SetPrivacyURL(_pInfo.privacyURL);

                oProjInfoTable.SetProjName(_pInfo.projectName);
                oProjInfoTable.SetProductName(_pInfo.productName);
                oProjInfoTable.SetShortProductName(_pInfo.shortProductName);

                oProjInfoTable.SetMacProjInfo(new STProjInfo() {
                    m_stBuildVer = new STBuildVer() {
                        m_nNum = newBuildNumber,
                        m_oVer = newVersion,
                    },
                    m_oAppID = _pInfo.appIdentifier,
                    m_oStoreURL = string.Empty,
                    m_oSupportsMail = _pInfo.supportsMail
                });

                oProjInfoTable.SetWndsProjInfo(new STProjInfo() {
                    m_stBuildVer = new STBuildVer() {
                        m_nNum = newBuildNumber,
                        m_oVer = newVersion,
                    },
                    m_oAppID = _pInfo.appIdentifier,
                    m_oStoreURL = string.Empty,
                    m_oSupportsMail = _pInfo.supportsMail
                });

                oProjInfoTable.SetiOSProjInfo(new STProjInfo() {
                    m_stBuildVer = new STBuildVer() {
                        m_nNum = newBuildNumber,
                        m_oVer = newVersion,
                    },
                    m_oAppID = _pInfo.appIdentifier,
                    m_oStoreURL = _pInfo.storeURLiOS,
                    m_oSupportsMail = _pInfo.supportsMail
                });

                oProjInfoTable.SetGoogleProjInfo(new STProjInfo() {
                    m_stBuildVer = new STBuildVer() {
                        m_nNum = newBuildNumber,
                        m_oVer = newVersion,
                    },
                    m_oAppID = _pInfo.appIdentifier,
                    m_oStoreURL = _pInfo.storeURLAndroid,
                    m_oSupportsMail = _pInfo.supportsMail
                });

                oProjInfoTable.SetOneStoreProjInfo(new STProjInfo() {
                    m_stBuildVer = new STBuildVer() {
                        m_nNum = newBuildNumber,
                        m_oVer = newVersion,
                    },
                    m_oAppID = _pInfo.appIdentifier,
                    m_oStoreURL = string.Empty,
                    m_oSupportsMail = _pInfo.supportsMail
                });

                oProjInfoTable.SetGalaxyStoreProjInfo(new STProjInfo() {
                    m_stBuildVer = new STBuildVer() {
                        m_nNum = newBuildNumber,
                        m_oVer = newVersion,
                    },
                    m_oAppID = _pInfo.appIdentifier,
                    m_oStoreURL = string.Empty,
                    m_oSupportsMail = _pInfo.supportsMail
                });

                EditorUtility.SetDirty(oProjInfoTable);
            }			
        }
    }

    private static void LogOverrideProjInfoTable()
    {
        if (overrideProjInfoTable)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#FFFF00>Override ProjInfoTable ON</color>"));
    }

    [MenuItem(menuOverrideProjInfoTable, false, 301)]
    private static void ToggleOverrideProjInfoTable()
    {	
        overrideProjInfoTable = !overrideProjInfoTable;
        EditorPrefs.SetBool(Application.identifier + prefOverrideProjInfoTable, overrideProjInfoTable);
        Menu.SetChecked(menuOverrideProjInfoTable, overrideProjInfoTable);
        LogOverrideProjInfoTable();
        
        OverrideProjInfoTable(PlayerSettings.bundleVersion, PlayerSettings.Android.bundleVersionCode);
        AssetDatabase.SaveAssets();
    }

    [MenuItem(menuOverrideProjInfoTable, true)]
    private static bool ValidateNinetapModules()
    {   return useManager;    }

#endif

#if USE_GAME_ANALYTICS

    private static void OverrideGameAnalytics(string newVersion)
    {
        GameAnalyticsSDK.Setup.Settings gaSettings = GameAnalyticsSDK.GameAnalytics.SettingsGA;
        
        if (gaSettings != null)
        {
            for (int i=0; i < gaSettings.Build.Count; i++)
            {
                gaSettings.Build[i] = newVersion;
            }
            EditorUtility.SetDirty(gaSettings);
        }
    }

#endif

#else
    public const bool isEnable = false;

    public static void LogCurrentVersion()
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#EC46EB>ver {0}</color>", Application.version));
    }

    public static int[] GetVersionArray()
    {
        string[] arr = Application.version.Split('.');
        
        if (arr.Length != 3)
            arr = new string[3] {"0", "0", "1"};

        int[] intArr = new int[3] {0, 0, 1};

        for(int i=0; i<arr.Length; i++)
        {
            int.TryParse(arr[i], out intArr[i]);
        }
        return intArr;
    }

#endif
}
