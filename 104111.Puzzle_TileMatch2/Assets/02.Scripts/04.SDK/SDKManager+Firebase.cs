#define ENABLE_FIREBASE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_FIREBASE
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
#endif

#pragma warning disable 162
public partial class SDKManager
{
    /// <Summary>Need Manual Update</Summary>
    public const string sdkVersion_Firebase = "11.8.0";

    private static bool isInitialized_Firebase = false;

#if ENABLE_FIREBASE
    private static FirebaseApp firebaseApp;
#endif
    
    private void Initialize_Firebase()
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#EC46EB>Firebase [{0}] (Need Manual Update)</color>", sdkVersion_Firebase));

        isInitialized_Firebase = false;

#if ENABLE_FIREBASE

#if UNITY_STANDALONE || UNITY_EDITOR
        return;
#endif
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                firebaseApp = FirebaseApp.DefaultInstance;
                isInitialized_Firebase = true;
                
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                
                InitAnalytics();
                InitCrashlytics();

            } else {
                Debug.LogError(string.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        void InitAnalytics()
        {
            FirebaseAnalytics.SetSessionTimeoutDuration(new System.TimeSpan(0, 0, 60));
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            
            SetAnalyticsUserID(deviceID);
        }

        void SetAnalyticsUserID(string a_oID) 
        {
            if(isInitialized_Firebase) {
                FirebaseAnalytics.SetUserId(a_oID);
            }
        }

        void InitCrashlytics()
        {
            SetCrashlyticsUserID(deviceID);
            SetCrashlyticsDatas(new Dictionary<string, string>() {
                        ["CountryCode"] = Application.systemLanguage.ToString()
                    });
        }

        void SetCrashlyticsUserID(string a_oID) 
        {
            if(isInitialized_Firebase) {
                Crashlytics.SetUserId(a_oID);
            }
        }

        void SetCrashlyticsDatas(Dictionary<string, string> a_oDataDict) 
        {
            if(isInitialized_Firebase) {
                foreach(var stKeyVal in a_oDataDict) {
                    Crashlytics.SetCustomKey(stKeyVal.Key, stKeyVal.Value);
                }
            }
        }
#endif
    }

    private static void SendEvent_Firebase(string eventName, Dictionary<string, string> eventParams)
    {
#if ENABLE_FIREBASE

#if UNITY_STANDALONE || UNITY_EDITOR
        return;
#endif
        SendEvent_Firebase(eventName, CreateParams_Firebase(eventParams));

#endif
    }

#if ENABLE_FIREBASE

    private static void SendEvent_Firebase(string eventName, params Parameter[] eventParams)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        return;
#endif
        if (isInitialized_Firebase)
        {
            FirebaseAnalytics.LogEvent(eventName, eventParams);
        }
    }

    private static Parameter[] CreateParams_Firebase(Dictionary<string, string> log)
    {
        var fParams = new List<Parameter>();

#if !UNITY_STANDALONE && !UNITY_EDITOR
        foreach(var item in log)
        {
            fParams.Add(new Parameter(item.Key, item.Value));
        }
#endif
        return fParams.ToArray();
    }

#endif
}
