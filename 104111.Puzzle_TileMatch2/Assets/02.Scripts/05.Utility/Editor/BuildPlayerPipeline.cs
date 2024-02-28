#define USE_MENU_ITEMS
#define USE_ADDRESSABLE_ASSETS
//#define USE_AMAZON_BUILD
//#define USE_NINETAP_MODULE	// Ninetap Module 사용 여부.

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using UnityEditor.Rendering;

#if USE_ADDRESSABLE_ASSETS
using UnityEditor.AddressableAssets.Settings;
#endif
#if USE_AMAZON_BUILD
using UnityEditor.Purchasing;
using UnityEngine.Purchasing;
#endif

/// <author>Pinelia Luna</author>
/// <Summary>BuildPlayerPipeline v2.0.0
/// <para>빌드 메뉴 제공 및 플랫폼 빌드 파이프라인 클래스.</para>
/// </Summary>
[InitializeOnLoad]
public class BuildPlayerPipelineScript : IActiveBuildTargetChanged
{
#region Parameters

    private const string version = "2.0.0";

    private static BuildPlayerPipeline buildPlayerOptionsObject
    {
		get {
			if (m_buildPlayerOptionsObject == null)
				m_buildPlayerOptionsObject = AssetDatabase.LoadAssetAtPath<BuildPlayerPipeline>(string.Format(scriptableObjectFormat, scriptableObjectPath, scriptableObjectFileName));

            return m_buildPlayerOptionsObject;
		} set {
			m_buildPlayerOptionsObject = value;
		}
	}
    
#if USE_NINETAP_MODULE
    private const string scriptableObjectPath = "Assets/01-GameEngine/Scripts/Editor/Scriptable/";
#else
    private const string scriptableObjectPath = "Assets/02.Scripts/05.Utility/Editor/Scriptable/";
#endif
    private static BuildPlayerPipeline m_buildPlayerOptionsObject;
    private static BuildPlayerOptions currentOptions;
    private static bool isBuildEditorScene;
    private static bool includeStreamingAssets;
    private static string buildName_Android;
    private static string buildName_iOS;
    private static string buildName_MacOS;
    private static string buildName_Windows;
    private static string buildName_Internal;
    private static string buildName_Expected;
    private const string SCENE_EDITOR = "Editor";
    private const string scriptableObjectFormat = "{0}{1}";
    private const string scriptableObjectFileName = "BuildPlayerPipeline.asset";
    private const string combinePathFormat = "{0}/{1}";
    private const string combinePathFormatMeta = "{0}/{1}.meta";
    private const string fullPathFormatMeta = "{0}.meta";
    private const string streamingAssetsPath = "Assets/StreamingAssets";
    private const string streamingAssetsExcludePath = "Assets/StreamingAssets_Exclude";
    private const string folderName_Android = "Builds/Android";
    private const string folderName_iOS = "Builds/iOS";
    private const string folderName_Standalone = "Builds/Standalone";
    private const string internalOutput = "InternalOutput";
    private const string junkSearchPattern = "*DoNotShip*";
    private const string externalDatasFolder = "ExternalDatas";
    
    /// <summary>Android/InternalOutput.apk/aab</summary>
    private static readonly string fileNameInternal_Android = $"{folderName_Android}/{internalOutput}.{{0}}";
    /// <summary>Android/buildName_Release_v1.0.0_10.apk/aab</summary>
    private static readonly string fileNameOutput_Android = $"{folderName_Android}/{{0}}_{{1}}_v{{2}}_{{3}}.{{4}}";
    
    /// <summary>Android/InternalOutput_Amazon.apk</summary>
    private static readonly string fileNameInternal_Amazon = $"{folderName_Android}/{internalOutput}_Amazon.{{0}}";
    /// <summary>Android/buildName_Release_v1.0.0_10_Amazon.apk</summary>
    private static readonly string fileNameOutput_Amazon = $"{folderName_Android}/{{0}}_{{1}}_v{{2}}_{{3}}_Amazon.{{4}}";
    
    /// <summary>iOS/InternalOutput</summary>
    private static readonly string folderNameInternal_iOS = $"{folderName_iOS}/{internalOutput}";
    /// <summary>iOS/buildName_Release_v1.0.0</summary>
    private static readonly string folderNameOutput_iOS = $"{folderName_iOS}/{{0}}_{{1}}_v{{2}}";
    
    /// <summary>Standalone/InternalOutput.app</summary>
    private static readonly string folderNameInternal_Standalone_MacOS = $"{folderName_Standalone}/{internalOutput}.app";
    /// <summary>Standalone/buildName_v1.0.0_mac</summary>
    private static readonly string folderNameOutput_Standalone_MacOS = $"{folderName_Standalone}/{{0}}_v{{1}}_mac";
    /// <summary>Standalone/buildName_v1.0.0_mac/buildName.app</summary>
    private static readonly string fileNameFormat_Standalone_MacOS = $"{{0}}/{{1}}.app";

    /// <summary>Standalone/InternalOutput</summary>
    private static readonly string folderNameInternal_Standalone_Windows = $"{folderName_Standalone}/{internalOutput}";
    /// <summary>Standalone/buildName_v1.0.0</summary>
    private static readonly string folderNameOutput_Standalone_Windows = $"{folderName_Standalone}/{{0}}_v{{1}}_win";
    /// <summary>Standalone/InternalOutput/productName.exe</summary>
    private static readonly string fileNameFormat_Standalone_Windows = $"{{0}}/{{1}}.exe";

    private const string menu_Build_Android_Debug_APK = "Tools/Build/Android/Debug.apk";
    private const string menu_Build_Android_Debug_AAB = "Tools/Build/Android/Debug.aab";
    private const string menu_Build_Android_Release_APK = "Tools/Build/Android/Release.apk";
    private const string menu_Build_Android_Release_AAB = "Tools/Build/Android/Release.aab";
    private const string menu_Build_Android_Amazon_APK = "Tools/Build/Android/Amazon.apk";
	private const string menu_Build_iOS_Debug = "Tools/Build/iOS/Debug";
	private const string menu_Build_iOS_Release = "Tools/Build/iOS/Release";
    private const string menu_Build_Standalone_MacOS = "Tools/Build/Standalone/MacOS";
	private const string menu_Build_Standalone_Windows = "Tools/Build/Standalone/Windows";
    private const string menu_SwitchPlatfoorm_Android = "Tools/Switch Platform/Android";
    private const string menu_SwitchPlatfoorm_iOS = "Tools/Switch Platform/iOS";
    private const string menu_SwitchPlatfoorm_Standalone_MacOS = "Tools/Switch Platform/MacOS";
    private const string menu_SwitchPlatfoorm_Standalone_Windows = "Tools/Switch Platform/Windows";
    private const string menu_Pipeline_Settings = "Tools/Pipeline Settings";
    private const string pref_RequestBuildAsync = "/BuildPlayerPipeline.RequestBuildAsync";
    private const string strDebug = "Debug";
    private const string strRelease = "Release";
    private const string strApk = "apk";
    private const string strAab = "aab";

#if USE_AMAZON_BUILD
    private static AppStore currentAppStore;
    private const string strStoreTargetGoogle = "STORE_TARGET_GOOGLE";
    private const string strStoreTargetAmazon = "STORE_TARGET_AMAZON";
#endif

#endregion Parameters


#region Initialize

#if USE_MENU_ITEMS

    static BuildPlayerPipelineScript()
    {
        EditorApplication.delayCall += () =>
        {
            isBuildEditorScene = EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Standalone;
            CheckScriptableObject();
            RefreshCurrentTarget();
        };
    }

#endif

    static void CheckScriptableObject()
    {
        if (!buildPlayerOptionsObject)
        {
            Debug.Log(CodeManager.GetMethodName() + "Create Scriptable Object");
            
            if (!Directory.Exists(scriptableObjectPath))
                Directory.CreateDirectory(scriptableObjectPath);

            m_buildPlayerOptionsObject = ScriptableObject.CreateInstance<BuildPlayerPipeline>();
            AssetDatabase.CreateAsset(m_buildPlayerOptionsObject, string.Format(scriptableObjectFormat, scriptableObjectPath, scriptableObjectFileName));
        }
    }

    static void SetBuildFileNames()
    {
        buildName_Android = buildPlayerOptionsObject.buildName_Android;
        buildName_iOS = buildPlayerOptionsObject.buildName_iOS;
        buildName_MacOS = buildPlayerOptionsObject.buildName_MacOS;
        buildName_Windows = buildPlayerOptionsObject.buildName_Windows;
    }

    static void RefreshCurrentTarget()
    {
        SetBuildFileNames();

        buildPlayerOptionsObject.pipelineVersion = version;
        buildPlayerOptionsObject.currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        buildPlayerOptionsObject.currentTarget = EditorUserBuildSettings.activeBuildTarget;
        buildPlayerOptionsObject.isBuildEditorScene = isBuildEditorScene;

        EditorUtility.SetDirty(buildPlayerOptionsObject);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

#endregion Initialize


#region Menu Items

#if USE_MENU_ITEMS

    [MenuItem(menu_Build_Android_Debug_APK, false, 1)]
	public static void Build_Android_Debug_APK()
	{
        SetAndroidStore_Google();

        isBuildEditorScene = false;
        includeStreamingAssets = isBuildEditorScene;
        EditorUserBuildSettings.buildAppBundle = false;

        currentOptions = new BuildPlayerOptions();
        currentOptions.targetGroup = BuildTargetGroup.Android;
        currentOptions.target = BuildTarget.Android;

        buildName_Internal = GetInternalPath(currentOptions.target);
        buildName_Expected = GetLocationPath(currentOptions.target);
        
        currentOptions.locationPathName = buildName_Internal;
        if (buildPlayerOptionsObject.useDevelopmentFlag)
            currentOptions.options = BuildOptions.Development;
        else
            currentOptions.options = buildPlayerOptionsObject.buildOptions_Android;

        RequestBuildPlayer();
	}

    [MenuItem(menu_Build_Android_Debug_AAB, false, 2)]
	public static void Build_Android_Debug_AAB()
	{
        SetAndroidStore_Google();

        isBuildEditorScene = false;
        includeStreamingAssets = isBuildEditorScene;
        EditorUserBuildSettings.buildAppBundle = true;

        currentOptions = new BuildPlayerOptions();
        currentOptions.targetGroup = BuildTargetGroup.Android;
        currentOptions.target = BuildTarget.Android;

        buildName_Internal = GetInternalPath(currentOptions.target);
        buildName_Expected = GetLocationPath(currentOptions.target);
        
        currentOptions.locationPathName = buildName_Internal;
        if (buildPlayerOptionsObject.useDevelopmentFlag)
            currentOptions.options = BuildOptions.Development;
        else
            currentOptions.options = buildPlayerOptionsObject.buildOptions_Android;
        
        RequestBuildPlayer();
	}

    [MenuItem(menu_Build_Android_Release_APK, false, 51)]
	public static void Build_Android_Release_APK()
	{
        SetAndroidStore_Google();

		isBuildEditorScene = false;
        includeStreamingAssets = isBuildEditorScene;
        EditorUserBuildSettings.buildAppBundle = false;

        currentOptions = new BuildPlayerOptions();
        currentOptions.targetGroup = BuildTargetGroup.Android;
        currentOptions.target = BuildTarget.Android;

        buildName_Internal = GetInternalPath(currentOptions.target, strRelease);
        buildName_Expected = GetLocationPath(currentOptions.target, strRelease);
        
        currentOptions.locationPathName = buildName_Internal;
        currentOptions.options = buildPlayerOptionsObject.buildOptions_Android;
        
        RequestBuildPlayer();
	}    

    [MenuItem(menu_Build_Android_Release_AAB, false, 52)]
	public static void Build_Android_Release_AAB()
	{
        SetAndroidStore_Google();

        isBuildEditorScene = false;
        includeStreamingAssets = isBuildEditorScene;
        EditorUserBuildSettings.buildAppBundle = true;

        currentOptions = new BuildPlayerOptions();
        currentOptions.targetGroup = BuildTargetGroup.Android;
        currentOptions.target = BuildTarget.Android;

        buildName_Internal = GetInternalPath(currentOptions.target, strRelease);
        buildName_Expected = GetLocationPath(currentOptions.target, strRelease);
        
        currentOptions.locationPathName = buildName_Internal;
        currentOptions.options = buildPlayerOptionsObject.buildOptions_Android;
        
        RequestBuildPlayer();
	}

#if USE_AMAZON_BUILD
    [MenuItem(menu_Build_Android_Amazon_APK, false, 71)]
	public static void Build_Android_Amazon_APK()
	{
        SetAndroidStore_Amazon();

        isBuildEditorScene = false;
        includeStreamingAssets = isBuildEditorScene;
        EditorUserBuildSettings.buildAppBundle = false;

        currentOptions = new BuildPlayerOptions();
        currentOptions.targetGroup = BuildTargetGroup.Android;
        currentOptions.target = BuildTarget.Android;

        buildName_Internal = GetInternalPath(currentOptions.target, strRelease);
        buildName_Expected = GetLocationPath(currentOptions.target, strRelease);
        
        currentOptions.locationPathName = buildName_Internal;
        currentOptions.options = buildPlayerOptionsObject.buildOptions_Android;
        
        RequestBuildPlayer();
	}    
#endif

    [MenuItem(menu_Build_iOS_Debug, false, 101)]
	public static void Build_iOS_Debug()
	{
        isBuildEditorScene = false;
        includeStreamingAssets = isBuildEditorScene;

        currentOptions = new BuildPlayerOptions();
        currentOptions.targetGroup = BuildTargetGroup.iOS;
        currentOptions.target = BuildTarget.iOS;

        buildName_Internal = GetInternalPath(currentOptions.target);
        buildName_Expected = GetLocationPath(currentOptions.target);
        
        currentOptions.locationPathName = buildName_Internal;
        if (buildPlayerOptionsObject.useDevelopmentFlag)
            currentOptions.options = BuildOptions.Development;
        else
            currentOptions.options = buildPlayerOptionsObject.buildOptions_iOS;
        
        RequestBuildPlayer();
	}

     [MenuItem(menu_Build_iOS_Release, false, 151)]
	public static void Build_iOS_Release()
	{
		isBuildEditorScene = false;
        includeStreamingAssets = isBuildEditorScene;

        currentOptions = new BuildPlayerOptions();
        currentOptions.targetGroup = BuildTargetGroup.iOS;
        currentOptions.target = BuildTarget.iOS;
        
        buildName_Internal = GetInternalPath(currentOptions.target, strRelease);
        buildName_Expected = GetLocationPath(currentOptions.target, strRelease);
        
        currentOptions.locationPathName = buildName_Internal;
        currentOptions.options = buildPlayerOptionsObject.buildOptions_iOS;
        
        RequestBuildPlayer();
	}
    
    [MenuItem(menu_Build_Standalone_MacOS, false, 201)]
	public static void Build_Standalone_MacOS()
	{
		isBuildEditorScene = true;
        includeStreamingAssets = isBuildEditorScene;

        currentOptions = new BuildPlayerOptions();
        currentOptions.targetGroup = BuildTargetGroup.Standalone;
        currentOptions.target = BuildTarget.StandaloneOSX;

        buildName_Internal = GetInternalPath(currentOptions.target);
        buildName_Expected = GetLocationPath(currentOptions.target);
        
        currentOptions.locationPathName = buildName_Internal;
        currentOptions.options = buildPlayerOptionsObject.buildOptions_MacOS;

        RequestBuildPlayer();
	}

    [MenuItem(menu_Build_Standalone_Windows, false, 202)]
	public static void Build_Standalone_Windows()
	{
		isBuildEditorScene = true;
        includeStreamingAssets = isBuildEditorScene;

        currentOptions = new BuildPlayerOptions();
        currentOptions.targetGroup = BuildTargetGroup.Standalone;
        currentOptions.target = BuildTarget.StandaloneWindows64;

        buildName_Internal = GetInternalPath(currentOptions.target);
        buildName_Expected = GetLocationPath(currentOptions.target);
        
        currentOptions.locationPathName = buildName_Internal;
        currentOptions.options = buildPlayerOptionsObject.buildOptions_Windows;

        RequestBuildPlayer();
	}

    [MenuItem(menu_SwitchPlatfoorm_Android, false, 1)]
	public static void SwitchPlatform_Android()
	{
        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
		EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
	}

    [MenuItem(menu_SwitchPlatfoorm_iOS, false, 2)]
	public static void SwitchPlatform_iOS()
	{
        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.iOS;
		EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.iOS, BuildTarget.iOS);
	}

    [MenuItem(menu_SwitchPlatfoorm_Standalone_MacOS, false, 51)]
	public static void SwitchPlatform_Standalone_MacOS()
	{
        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
		EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
	}

    [MenuItem(menu_SwitchPlatfoorm_Standalone_Windows, false, 52)]
	public static void SwitchPlatform_Standalone_Windows()
	{
        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
		EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
	}

    [MenuItem(menu_Pipeline_Settings, false, 1)]
    public static void Select_Settings()
	{
        Selection.activeObject = buildPlayerOptionsObject;
	}

#endif

#endregion Menu Items


#region File IO
    static void CleanInternalOutput(BuildTarget buildTarget, string from)
    {
        switch (buildTarget)
        {
            case BuildTarget.Android:
                DeleteFile(from);
                break;
            case BuildTarget.iOS:
                DeleteFolder(from, true);
                break;
            case BuildTarget.StandaloneOSX:
                DeleteFolder(from, true);
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                DeleteFolder(folderNameInternal_Standalone_Windows, true);
                break;
        }
    }

    static void RenameOutput(BuildTarget buildTarget, string from, string to)
    {
        switch (buildTarget)
        {
            case BuildTarget.Android:
                MoveOutputFile(from, to);
                break;
            case BuildTarget.iOS:
                MoveOutputFolder(from, to);
                break;
            case BuildTarget.StandaloneOSX:
                string newFolder = string.Format(folderNameOutput_Standalone_MacOS, buildName_MacOS, Application.version);
                string newAppName = string.Format(fileNameFormat_Standalone_MacOS, newFolder, buildName_MacOS);
                DeleteFolder(newFolder, true);
                CreateFolder(newFolder);
                MoveOutputFolder(from, newAppName);
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                MoveOutputFolder(folderNameInternal_Standalone_Windows, to);
                break;
        }
    }

    static void CopyExternalDatas(BuildTarget buildTarget, string from, string to)
    {
        if(!Directory.Exists(from))
            return;
        
        string destFolder;

        switch (buildTarget)
        {
            case BuildTarget.StandaloneOSX:
                string newFolder = string.Format(folderNameOutput_Standalone_MacOS, buildName_MacOS, Application.version);
                destFolder = Path.Combine(newFolder, from);

                Debug.Log(CodeManager.GetMethodName() + destFolder);
                CopyFolder(from, destFolder);

                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                destFolder = Path.Combine(to, from);

                Debug.Log(CodeManager.GetMethodName() + destFolder);
                CopyFolder(from, destFolder);

                break;
        }
    }

    static void MoveOutputFolder(string from, string to)
    {
        if (Directory.Exists(from))
        {
            DeleteFolder(to, true);            
            Directory.Move(from, to);

            Debug.Log(CodeManager.GetMethodName() + to);
        }
    }

    static void MoveOutputFile(string from, string to)
    {
        if (File.Exists(from))
        {
            DeleteFile(to);            
            File.Move(from, to);

            Debug.Log(CodeManager.GetMethodName() + to);
        }
    }

    static void MoveExcludeFolder(string folderName, string from, string to)
    {
        if (Directory.Exists(string.Format(combinePathFormat, from, folderName)))
            Directory.Move(string.Format(combinePathFormat, from, folderName), string.Format(combinePathFormat, to, folderName));
        if (File.Exists(string.Format(combinePathFormatMeta, from, folderName)))
            File.Move(string.Format(combinePathFormatMeta, from, folderName), string.Format(combinePathFormatMeta, to, folderName));
    }

    static void MoveExcludeFile(string fileName, string from, string to)
    {
        if (File.Exists(string.Format(combinePathFormat, from, fileName)))
            File.Move(string.Format(combinePathFormat, from, fileName), string.Format(combinePathFormat, to, fileName));
        if (File.Exists(string.Format(combinePathFormatMeta, from, fileName)))
            File.Move(string.Format(combinePathFormatMeta, from, fileName), string.Format(combinePathFormatMeta, to, fileName));
    }

    static void CreateFolder(string folderFullPath)
    {
        if (!Directory.Exists(folderFullPath))
            Directory.CreateDirectory(folderFullPath);
    }
    
    static void DeleteFolder(string folderFullPath, bool isRecursive = false)
    {
        if (Directory.Exists(folderFullPath))
            Directory.Delete(folderFullPath, isRecursive);
        
        DeleteFile(string.Format(fullPathFormatMeta, folderFullPath));
    }

    static void DeleteFile(string fileFullPath)
    {
        if (File.Exists(fileFullPath))
            File.Delete(fileFullPath);
    }

    static void CopyFolder(string sourceFolder, string destFolder)
    {
        if (destFolder.ContainsAny(buildPlayerOptionsObject.externalExcludeFolders.ToArray()))
        {
            //Debug.Log(string.Format("[Exclude Folder] {0}", destFolder));

            if (Directory.Exists(destFolder))
                Directory.Delete(destFolder);
            
            return;
        }

        if (!Directory.Exists(destFolder))
            Directory.CreateDirectory(destFolder);
        
        string[] files = Directory.GetFiles(sourceFolder);
        string[] folders = Directory.GetDirectories(sourceFolder);
        
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);

            if (name.ContainsAny(buildPlayerOptionsObject.externalExcludeFiles.ToArray()))
            {
                //Debug.Log(string.Format("[Exclude File] {0}", name));
                continue;
            }
            
            string dest = Path.Combine(destFolder, name);
            
            //Debug.Log(string.Format("<color=yellow>{0} => {1}</color>", file, dest));
            
            File.Copy(file, dest);
        }
        
        foreach (string folder in folders)
        {
            string name = Path.GetFileName(folder);
            string dest = Path.Combine(destFolder, name);
            CopyFolder(folder, dest);
        }
    }

#endregion File IO


#region Build Pipeline

    private static void SetAndroidStore_Google()
    {
#if USE_AMAZON_BUILD
        currentAppStore = AppStore.GooglePlay;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, SetDefineSymbol(BuildTargetGroup.Android, strStoreTargetAmazon, strStoreTargetGoogle));
        UnityPurchasingEditor.TargetAndroidStore(currentAppStore);
#endif
    }

    private static void SetAndroidStore_Amazon()
    {
#if USE_AMAZON_BUILD
        currentAppStore = AppStore.AmazonAppStore;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, SetDefineSymbol(BuildTargetGroup.Android, strStoreTargetGoogle, strStoreTargetAmazon));
        UnityPurchasingEditor.TargetAndroidStore(currentAppStore);
#endif
    }

    private static string[] SetDefineSymbol(BuildTargetGroup targetGroup, string symbolToRemove, string symbolToAdd)
    {
        bool removeExist = false;
        string[] definesArray;
        List<string> definesList = new List<string>();
        PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup, out definesArray);
        
        for (int i=0; i<definesArray.Length; i++)
        {
            if(definesArray[i].Equals(symbolToRemove))
            {
                definesArray[i] = symbolToAdd;
                removeExist = true;
            }
            definesList.Add(definesArray[i]);
        }

        if (!removeExist)
        {
            definesList.Add(symbolToAdd);
        }

        return definesList.ToArray();
    }

    static string GetInternalPath(BuildTarget buildTarget, string buildType = strDebug)
    {
        SetBuildFileNames();

        string internalPath = string.Empty;

        switch (buildTarget)
        {
            case BuildTarget.Android:
                string extension = EditorUserBuildSettings.buildAppBundle ? strAab : strApk;
#if USE_AMAZON_BUILD
                if (currentAppStore == AppStore.AmazonAppStore)
                    internalPath = string.Format(fileNameInternal_Amazon, extension);
                else
                    internalPath = string.Format(fileNameInternal_Android, extension);                
#else
                internalPath = string.Format(fileNameInternal_Android, extension);
#endif
                break;
            case BuildTarget.iOS:
                internalPath = string.Format(folderNameInternal_iOS);
                break;
            case BuildTarget.StandaloneOSX:
                internalPath = string.Format(folderNameInternal_Standalone_MacOS);
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                internalPath = string.Format(fileNameFormat_Standalone_Windows, folderNameInternal_Standalone_Windows, buildName_Windows);
                break;
        }

        return internalPath;
    }

    static string GetLocationPath(BuildTarget buildTarget, string buildType = strDebug)
    {
        SetBuildFileNames();

        string locationPath = string.Empty;

        switch (buildTarget)
        {
            case BuildTarget.Android:
                string extension = EditorUserBuildSettings.buildAppBundle ? strAab : strApk;
#if USE_AMAZON_BUILD
                if (currentAppStore == AppStore.AmazonAppStore)
                    locationPath = string.Format(fileNameOutput_Amazon, buildName_Android, buildType, Application.version, PlayerSettings.Android.bundleVersionCode, extension);
                else
                    locationPath = string.Format(fileNameOutput_Android, buildName_Android, buildType, Application.version, PlayerSettings.Android.bundleVersionCode, extension);                
#else
                locationPath = string.Format(fileNameOutput_Android, buildName_Android, buildType, Application.version, PlayerSettings.Android.bundleVersionCode, extension);
#endif
                break;
            case BuildTarget.iOS:
                locationPath = string.Format(folderNameOutput_iOS, buildName_iOS, buildType, Application.version);
                break;
            case BuildTarget.StandaloneOSX:
                locationPath = string.Format(fileNameFormat_Standalone_MacOS, string.Format(folderNameOutput_Standalone_MacOS, buildName_MacOS, Application.version), buildName_MacOS);
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                locationPath = string.Format(folderNameOutput_Standalone_Windows, buildName_Windows, Application.version);
                break;
        }

        return locationPath;
    }

    static string[] SetBuildScenes()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        var scenePathList = new List<string>();
        
        for (int i = 0; i < scenes.Length; i++) 
        {
            string scenePath = scenes[i].path;

#if USE_NINETAP_MODULE
            bool isEditorScene = scenePath.Contains(KCDefine.B_SCENE_N_LEVEL_EDITOR);
#else
            bool isEditorScene = scenePath.Contains(SCENE_EDITOR);
#endif
            bool isExcludeScene = false;

            for(int j=0; j < buildPlayerOptionsObject.excludeScenes.Count; j++)
            {
                if (scenePath.Contains(buildPlayerOptionsObject.excludeScenes[j]))
                {
                    isExcludeScene = true;
                    break;
                }
            }

            if (isBuildEditorScene || (!isEditorScene && !isExcludeScene)) 
            {
                scenes[i].enabled = true;
                scenePathList.Add(scenePath);
            }
            else
            {
                scenes[i].enabled = false;
            }
        }

        EditorBuildSettings.scenes = scenes;

        return scenePathList.ToArray();
    }

    static void ExcludeStreamingAssets()
    {
        if (buildPlayerOptionsObject.excludeFolders.Count + buildPlayerOptionsObject.excludeFiles.Count == 0)
            return;
        
        if (includeStreamingAssets)
        {
            if (Directory.Exists(streamingAssetsExcludePath))
            {
                Debug.Log(CodeManager.GetMethodName() + includeStreamingAssets);

                try
                {
                    for (int i=0; i < buildPlayerOptionsObject.excludeFolders.Count; i++)
                    {
                        MoveExcludeFolder(buildPlayerOptionsObject.excludeFolders[i], streamingAssetsExcludePath, streamingAssetsPath);
                    }
                    for (int i=0; i < buildPlayerOptionsObject.excludeFiles.Count; i++)
                    {
                        MoveExcludeFile(buildPlayerOptionsObject.excludeFiles[i], streamingAssetsExcludePath, streamingAssetsPath);
                    }

                    DeleteFolder(streamingAssetsExcludePath);
                }
                catch (Exception e)
                {
                    Debug.Log(CodeManager.GetMethodName() + e.ToString());
                }

                AssetDatabase.Refresh();
            }
        }
        else
        {
            if (Directory.Exists(streamingAssetsPath))
            {
                Debug.Log(CodeManager.GetMethodName() + includeStreamingAssets);

                try
                {
                    CreateFolder(streamingAssetsExcludePath);
                    
                    for (int i=0; i < buildPlayerOptionsObject.excludeFolders.Count; i++)
                    {
                        MoveExcludeFolder(buildPlayerOptionsObject.excludeFolders[i], streamingAssetsPath, streamingAssetsExcludePath);
                    }
                    for (int i=0; i < buildPlayerOptionsObject.excludeFiles.Count; i++)
                    {
                        MoveExcludeFile(buildPlayerOptionsObject.excludeFiles[i], streamingAssetsPath, streamingAssetsExcludePath);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(CodeManager.GetMethodName() + e.ToString());
                }

                AssetDatabase.Refresh();
            }
        }
    }

    static void RecoverStreamingAssets()
    {
        if (Directory.Exists(streamingAssetsExcludePath))
        {
            Debug.Log(CodeManager.GetMethodName());

            try
            {
                for (int i=0; i < buildPlayerOptionsObject.excludeFolders.Count; i++)
                {
                    MoveExcludeFolder(buildPlayerOptionsObject.excludeFolders[i], streamingAssetsExcludePath, streamingAssetsPath);
                }
                for (int i=0; i < buildPlayerOptionsObject.excludeFiles.Count; i++)
                {
                    MoveExcludeFile(buildPlayerOptionsObject.excludeFiles[i], streamingAssetsExcludePath, streamingAssetsPath);
                }

                DeleteFolder(streamingAssetsExcludePath);
            }
            catch (Exception e)
            {
                Debug.Log(CodeManager.GetMethodName() + e.ToString());
            }

            AssetDatabase.Refresh();
        }
    }

    static void RemoveJunkFolders()
    {
        Debug.Log(CodeManager.GetMethodName());

        try
        {
            CleanFolder(folderName_Android);
            CleanFolder(folderName_iOS);
            CleanFolder(folderName_Standalone);
        }
        catch (Exception e)
        {
            Debug.Log(CodeManager.GetMethodName() + e.ToString());
        }

        void CleanFolder(string folderToClean)
        {
            if (Directory.Exists(folderToClean))
            {
                string[] dirs = Directory.GetDirectories(folderToClean, junkSearchPattern);
                if (dirs.Length > 0)
                {
                    //Debug.Log(CodeManager.GetMethodName() + folderToClean);
                    for(int i=0; i < dirs.Length; i++)
                    {
                        //Debug.Log(CodeManager.GetMethodName() + dirs[i]);
                        DeleteFolder(dirs[i], true);
                    }
                }
            }
        }
    }

    static void SetBuildName(string _internal, string _expected)
    {
        buildName_Internal = _internal;
        buildName_Expected = _expected;
    }

    static void BuildAddressable()
    {
#if USE_ADDRESSABLE_ASSETS
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent();
#endif
    }

    static void RequestBuildPlayer()
    {
        CheckScriptableObject();

        PlayerSettings.SetAdditionalIl2CppArgs("--compiler-flags=\"-fbracket-depth=1024\"");

        buildPlayerOptionsObject.buildOptions = currentOptions.options;
        buildPlayerOptionsObject.buildLocationPathName = currentOptions.locationPathName;
        buildPlayerOptionsObject.buildTargetGroup = currentOptions.targetGroup;
        buildPlayerOptionsObject.buildTarget = currentOptions.target;
        buildPlayerOptionsObject.buildName_Internal = buildName_Internal;
        buildPlayerOptionsObject.buildName_Expected = buildName_Expected;

        RefreshCurrentTarget();
        
        if (buildPlayerOptionsObject.currentGroup != currentOptions.targetGroup || buildPlayerOptionsObject.currentTarget != currentOptions.target)
        {
            EditorPrefs.SetBool(Application.identifier + pref_RequestBuildAsync, true);
            EditorUserBuildSettings.selectedBuildTargetGroup = currentOptions.targetGroup;
            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(currentOptions.targetGroup, currentOptions.target);
        }
        else
        {
            BuildPlayer(currentOptions);
        }
    }

    public int callbackOrder { get { return 0; } }
    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        if (EditorPrefs.GetBool(Application.identifier + pref_RequestBuildAsync, false))
        {
            SetBuildName(buildPlayerOptionsObject.buildName_Internal, buildPlayerOptionsObject.buildName_Expected);

            currentOptions = new BuildPlayerOptions();
            currentOptions.targetGroup = buildPlayerOptionsObject.buildTargetGroup;
            currentOptions.target = buildPlayerOptionsObject.buildTarget;
            currentOptions.options = buildPlayerOptionsObject.buildOptions;
            currentOptions.locationPathName = buildPlayerOptionsObject.buildLocationPathName;
            
            isBuildEditorScene = buildPlayerOptionsObject.isBuildEditorScene;
            includeStreamingAssets = isBuildEditorScene;

            BuildPlayer(currentOptions);
        }
        else
        {
            isBuildEditorScene = EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Standalone;
            includeStreamingAssets = isBuildEditorScene;
            CheckScriptableObject();
            RefreshCurrentTarget();
            SetBuildScenes();

            Debug.Log(CodeManager.GetMethodName() + string.Format("Platform Changed : {0}", newTarget));
        }
    }

    static async void BuildPlayer(BuildPlayerOptions buildOptions)
    {
        try
        {
            EditorPrefs.SetBool(Application.identifier + pref_RequestBuildAsync, false);
            buildOptions.scenes = SetBuildScenes();

            CleanInternalOutput(buildOptions.target, buildName_Internal);
            ExcludeStreamingAssets();
            BuildAddressable();

            if (!Application.isBatchMode)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log(CodeManager.GetMethodName() + summary.outputPath);
                Debug.Log(CodeManager.GetMethodName() + string.Format("Build Success : {0} ({1:f1} seconds)", summary.platform, summary.totalTime.TotalSeconds));
                
                RenameOutput(buildOptions.target, buildName_Internal, buildName_Expected);

                if (buildPlayerOptionsObject.includeExternalDatas)
                    CopyExternalDatas(buildOptions.target, externalDatasFolder, buildName_Expected);
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError(CodeManager.GetMethodName() + string.Format("Build Failed : {0} / ({1:f1} seconds)", summary.platform, summary.totalTime.TotalSeconds));
            }

            if (summary.totalErrors > 0)
                Debug.LogError(CodeManager.GetMethodName() + string.Format("Errors : {0}", summary.totalErrors));
        }
        catch (Exception e)
        {
            Debug.LogError(CodeManager.GetMethodName() + e.ToString());
        }
        finally
        {
            SetAndroidStore_Google();
            RecoverStreamingAssets();
            RemoveJunkFolders();
            SetBuildName(string.Empty, string.Empty);

            if (buildPlayerOptionsObject.currentGroup != buildOptions.targetGroup || buildPlayerOptionsObject.currentTarget != buildOptions.target)
            {
                if (buildPlayerOptionsObject.useRevertPlatform)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("Revert Platform : {0}", buildPlayerOptionsObject.currentTarget));
                    
                    await Task.Delay(100);
                    
                    EditorUserBuildSettings.selectedBuildTargetGroup = buildPlayerOptionsObject.currentGroup;               
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(buildPlayerOptionsObject.currentGroup, buildPlayerOptionsObject.currentTarget);
                }
                else
                {
                    isBuildEditorScene = EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Standalone;
                    includeStreamingAssets = isBuildEditorScene;
                    CheckScriptableObject();
                    RefreshCurrentTarget();
                    SetBuildScenes();
                }
            }
        }
    }

#endregion Build Pipeline
}

[CreateAssetMenu(fileName ="BuildPlayerPipeline", menuName ="BuildPlayerPipeline/Scriptable Object", order = 1)]
public class BuildPlayerPipeline : ScriptableObject
{
#region Settings

    [BPP_ReadOnly] public string pipelineVersion;

    [Header("★ [Settings] Pipeline Settings")]
    public bool useRevertPlatform = true;
    public bool useDevelopmentFlag = false;

    [Header("★ [Settings] Build Name")]
    public string buildName_Android = "AppName";
    public string buildName_iOS = "AppName";
    public string buildName_MacOS = "AppName";
    public string buildName_Windows = "AppName";

    [Header("★ [Settings] Build Options")]
    public BuildOptions buildOptions_Android = BuildOptions.CompressWithLz4;
    public BuildOptions buildOptions_iOS = BuildOptions.CompressWithLz4;
    public BuildOptions buildOptions_MacOS = BuildOptions.None;
    public BuildOptions buildOptions_Windows = BuildOptions.None;

    [Header("★ [Settings] StreamingAssets Exclude Mobile")]
    public List<string> excludeFolders = new List<string>{"LevelData"};
    public List<string> excludeFiles = new List<string>{"StringData.csv", "LevelCount.txt", "LevelCount_Editor.txt"};
    public List<string> excludeScenes = new List<string>{"Editor"};

    [Header("★ [Settings] Editor Include ExternalDats")]
    public bool includeExternalDatas = true;
    public List<string> externalExcludeFiles = new List<string>{".DS_Store", ".backup"};
    public List<string> externalExcludeFolders = new List<string>{"Handover", "Backup"};

#endregion Settings


#region Readonly

    [Header("★ [Readonly] Current Target Info")]
    [BPP_ReadOnly] public BuildTargetGroup currentGroup;
    [BPP_ReadOnly] public BuildTarget currentTarget;
    [BPP_ReadOnly] public bool isBuildEditorScene;
    
    [Header("★ [Readonly] Last Build Target Info")]
    [BPP_ReadOnly] public BuildTargetGroup buildTargetGroup = BuildTargetGroup.Android;
    [BPP_ReadOnly] public BuildTarget buildTarget = BuildTarget.Android;
    [BPP_ReadOnly] public BuildOptions buildOptions = BuildOptions.CompressWithLz4;
    [BPP_ReadOnly] public string buildLocationPathName = "Not build yet";

    [HideInInspector] public string buildName_Internal;
    [HideInInspector] public string buildName_Expected;

#endregion Readonly
}

[CustomPropertyDrawer(typeof(BPP_ReadOnlyAttribute))]
public class BPP_ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string valueStr;

        switch (property.propertyType)
        {
            case SerializedPropertyType.String:
                valueStr = property.stringValue;
                break;
            case SerializedPropertyType.Integer:
                valueStr = property.intValue.ToString();
                break;
            case SerializedPropertyType.Boolean:
                valueStr = property.boolValue.ToString();
                break;
            case SerializedPropertyType.Float:
                valueStr = property.floatValue.ToString("0.0000");
                break;
            case SerializedPropertyType.Enum:
                valueStr = property.enumDisplayNames[property.enumValueIndex];
                break;
            case SerializedPropertyType.Rect:
                valueStr = property.rectIntValue.ToString();
                break;
            case SerializedPropertyType.Vector2Int:
                valueStr = property.vector2IntValue.ToString();
                break;
            case SerializedPropertyType.Vector3Int:
                valueStr = property.vector3IntValue.ToString();
                break;
            case SerializedPropertyType.Vector2:
                valueStr = property.vector2Value.ToString();
                break;            
            case SerializedPropertyType.Vector3:
                valueStr = property.vector3Value.ToString();
                break;
            case SerializedPropertyType.Vector4:
                valueStr = property.vector4Value.ToString();
                break;
            default:
                //valueStr = string.Format("propertyType : {0}", property.propertyType);
                bool previousGUIState = GUI.enabled;
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label);
                GUI.enabled = previousGUIState;
                return;
        }

        EditorGUI.LabelField(position, label.text, valueStr);
    }
}

public class BPP_ReadOnlyAttribute : PropertyAttribute { }

#endif