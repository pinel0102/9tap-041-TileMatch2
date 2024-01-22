#define ENABLE_FACEBOOK

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_FACEBOOK
using Facebook.Unity;
#endif

#pragma warning disable 162
public partial class SDKManager
{
    private void Initialize_Facebook()
    {
#if ENABLE_FACEBOOK

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#EC46EB>Facebook [{0}]</color>", FacebookSdkVersion.Build));

#if UNITY_STANDALONE || UNITY_EDITOR
        return;
#endif
        
        if (!FB.IsInitialized)
            FB.Init(InitCallback, OnHideUnity);
        else
            FB.ActivateApp();
#endif
    }

    private void Start_Facebook()
    {
#if ENABLE_FACEBOOK

#if UNITY_STANDALONE || UNITY_EDITOR
        return;
#endif
        if (FB.IsInitialized)
        {
#if UNITY_IOS || UNITY_ANDROID
            FB.Mobile.SetAutoLogAppEventsEnabled(false);
#endif
            FB.ActivateApp();
        }
        else
        {
#if UNITY_IOS || UNITY_ANDROID
            FB.Mobile.SetAutoLogAppEventsEnabled(false);
#endif
            FB.Init(() => { FB.ActivateApp(); }); 
        }   
#endif
    }


#region delegates

    private void InitCallback()
    {
#if ENABLE_FACEBOOK

#if UNITY_STANDALONE || UNITY_EDITOR
        return;
#endif
        if (FB.IsInitialized)
            FB.ActivateApp();
        else
            Debug.Log("Failed to Initialize the Facebook SDK");
#endif
    }
    
    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            //Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            //Time.timeScale = 1;
        }
    }

#endregion delegates

}
