using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

/// <author>Pinelia Luna</author>
/// <Summary>BuildMachineAutomation v1.0
/// <para>스플래시 제거 및 키스토어 입력 자동화 클래스.</para>
/// </Summary>
[InitializeOnLoad]
public class BuildMachineAutomation
{
    private const string keystoreName = "Keystore.keystore";
    private const string keystorePass = "NT9studio!";
    private const string keyaliasName = "keystore";
    private const string keyaliasPass = "NT9studio!";
    private static bool useAutomation = true; 
    private static bool showLog = false;

    [DidReloadScripts]
    private static void CheckKeystore()
    {
        if (useAutomation)
        {
            if (showLog)
                Debug.Log(CodeManager.GetMethodName());
            
            DisableSplash();
            
            if (NeedUpdate())
                UpdateKeystore();                
        }
    }

    private static void DisableSplash()
    {
        if (UnityEditorInternal.InternalEditorUtility.HasPro())
        {
            if (PlayerSettings.SplashScreen.show || PlayerSettings.SplashScreen.showUnityLogo)
            {
                Debug.Log(CodeManager.GetMethodName() + "<color=#FFFF00>Splash OFF</color>");

                PlayerSettings.SplashScreen.show = false;
                PlayerSettings.SplashScreen.showUnityLogo = false;

                AssetDatabase.SaveAssets();
            }
        }
    }

    private static void UpdateKeystore()
    {
        if (showLog)
            Debug.Log(CodeManager.GetMethodName());

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = keystoreName;
        PlayerSettings.Android.keystorePass = keystorePass;
        PlayerSettings.Android.keyaliasName = keyaliasName;        
        PlayerSettings.Android.keyaliasPass = keyaliasPass;

        AssetDatabase.SaveAssets();
    }

    private static bool NeedUpdate()
    {        
        if (!PlayerSettings.Android.useCustomKeystore)
            return true;
        
        if (!PlayerSettings.Android.keystoreName.Equals(keystoreName))
            return true;

        if (!PlayerSettings.Android.keystorePass.Equals(keystorePass))
            return true;

        if (!PlayerSettings.Android.keyaliasName.Equals(keyaliasName))
            return true;
        
        if (!PlayerSettings.Android.keyaliasPass.Equals(keyaliasPass))
            return true;

        return false;
    }
}

#endif
