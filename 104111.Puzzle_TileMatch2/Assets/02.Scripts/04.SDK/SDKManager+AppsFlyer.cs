#define ENABLE_APPSFLYER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_APPSFLYER
using AppsFlyerSDK;
#endif

#pragma warning disable 162
public partial class SDKManager
{
    private static string appsFlyer_DevKey;
    private static string appsFlyer_AppID;
    private static bool isInitialized_AppsFlyer = false;

    private void Initialize_AppsFlyer(string devKey, string appID)
    {
        appsFlyer_DevKey = devKey;
        isInitialized_AppsFlyer = false;

#if UNITY_IOS
        appsFlyer_AppID = appID;
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#EC46EB>AppsFlyer [{0}] {1} / {2}</color>", AppsFlyer.kAppsFlyerPluginVersion, appsFlyer_DevKey, appsFlyer_AppID));
#elif UNITY_ANDROID
        appsFlyer_AppID = string.Empty;
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#EC46EB>AppsFlyer [{0}] {1}</color>", AppsFlyer.kAppsFlyerPluginVersion, appsFlyer_DevKey));
#else        
        return;
#endif

#if DEBUG || DEVELOPMENT_BUILD
        AppsFlyer.setIsDebug(true);
#else
        AppsFlyer.setIsDebug(false);
#endif

        AppsFlyer.initSDK(appsFlyer_DevKey, appsFlyer_AppID, this);
        AppsFlyer.startSDK();
        
        isInitialized_AppsFlyer = true;
    }

    private static void SendEvent_AppsFlyer(string eventName, Dictionary<string, string> eventParams) 
    {
#if UNITY_STANDALONE
        return;
#endif	
		if (isInitialized_AppsFlyer) 
        {			
            AppsFlyer.sendEvent(eventName, eventParams);
		}
	}


#region delegates

    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        // add deferred deeplink logic here
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("onConversionDataFail", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
        // add direct deeplink logic here
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }

#endregion delegates

}
