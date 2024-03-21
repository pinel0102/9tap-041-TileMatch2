using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class LevelEditorPrefs
{
#region Parameters

    /// <summary>에디터에서 보드에 설치 또는 추가할 Blocker 타입.</summary>
    public static BlockerTypeEditor UI_BlockerType;
    /// <summary>에디터에서 보드에 설치 또는 추가할 Blocker Dropdown Index.</summary>
    public static int UI_BlockerTypeIndex;
    /// <summary>에디터에서 보드에 설치 또는 추가할 Blocker 개수.</summary>
    public static int UI_BlockerCount;
    /// <summary>에디터에서 보드에 설치 또는 추가할 Blocker의 가변 ICD. (가변 ICD 사용 항목만 적용.)</summary>
    public static int UI_BlockerVariableICD;
    /// <summary>에디터에서 Blocker를 수정할 레이어 인덱스. (-1 : All)</summary>
    public static int UI_BlockerLayerIndex;
    /// <summary>에디터에서 Blocker Tutorial 사용 여부.</summary>
    public static bool UI_ShowBlockerTutorial;

#endregion Parameters

    private static bool showLog = false;
    private const string EDITORPREF_FORMAT = "TileMatch2_Editor/{0}";

    public static void LoadEditorData()
    {
        Debug.Log(CodeManager.GetMethodName());

        UI_BlockerType = LoadData(nameof(UI_BlockerType), BlockerTypeEditor.None);
        UI_BlockerTypeIndex.LoadData(nameof(UI_BlockerTypeIndex), 0);
        UI_BlockerCount.LoadData(nameof(UI_BlockerCount), 1);
        UI_BlockerVariableICD.LoadData(nameof(UI_BlockerVariableICD), 4);
        UI_BlockerLayerIndex.LoadData(nameof(UI_BlockerLayerIndex), -1);
        UI_ShowBlockerTutorial.LoadData(nameof(UI_ShowBlockerTutorial), false);
    }

    public static void SaveEditorData()
    {
        Debug.Log(CodeManager.GetMethodName());

        UI_BlockerType.SaveData(nameof(UI_BlockerType));
        UI_BlockerTypeIndex.SaveData(nameof(UI_BlockerTypeIndex));
        UI_BlockerCount.SaveData(nameof(UI_BlockerCount));
        UI_BlockerVariableICD.SaveData(nameof(UI_BlockerVariableICD));
        UI_BlockerLayerIndex.SaveData(nameof(UI_BlockerLayerIndex));
        UI_ShowBlockerTutorial.SaveData(nameof(UI_ShowBlockerTutorial));
    }


#region LoadData

    private static T LoadData<T>(string Key, T defaultValue) where T : Enum
    {
        return (T)Enum.Parse(typeof(T), LoadData(Key, defaultValue.ToString()));
    }

    private static T LoadData<T>(this T Parameter, string Key, T defaultValue) where T : Enum
    {
        return LoadData(Key, defaultValue);
    }

    private static void LoadData(this ref bool Parameter, string Key, bool defaultValue)
    {
        Parameter = LoadData(Key, defaultValue);
    }

    private static void LoadData(this ref int Parameter, string Key, int defaultValue)
    {
        Parameter = LoadData(Key, defaultValue);
    }

    private static void LoadData(this ref float Parameter, string Key, float defaultValue)
    {
        Parameter = LoadData(Key, defaultValue);
    }

    private static bool LoadData(string Key, bool defaultValue)
    {
        if (showLog)
            Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", string.Format(EDITORPREF_FORMAT, Key), bool.Parse(PlayerPrefs.GetString(string.Format(EDITORPREF_FORMAT, Key), defaultValue.ToString()))));
        return bool.Parse(PlayerPrefs.GetString(string.Format(EDITORPREF_FORMAT, Key), defaultValue.ToString()));
    }

    private static int LoadData(string Key, int defaultValue)
    {
        if (showLog)
            Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", string.Format(EDITORPREF_FORMAT, Key), PlayerPrefs.GetInt(string.Format(EDITORPREF_FORMAT, Key), defaultValue)));
        return PlayerPrefs.GetInt(string.Format(EDITORPREF_FORMAT, Key), defaultValue);
    }

    private static float LoadData(string Key, float defaultValue)
    {
        if (showLog)
            Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", string.Format(EDITORPREF_FORMAT, Key), PlayerPrefs.GetFloat(string.Format(EDITORPREF_FORMAT, Key), defaultValue)));
        return PlayerPrefs.GetFloat(string.Format(EDITORPREF_FORMAT, Key), defaultValue);
    }

    private static string LoadData(string Key, string defaultValue)
    {
        if (showLog)
            Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", string.Format(EDITORPREF_FORMAT, Key), PlayerPrefs.GetString(string.Format(EDITORPREF_FORMAT, Key), defaultValue)));
        return PlayerPrefs.GetString(string.Format(EDITORPREF_FORMAT, Key), defaultValue);
    }

#endregion LoadData


#region SaveData

    private static void SaveData<T>(this T Parameter, string Key) where T : Enum
    {
        SaveData(Key, Parameter.ToString());
    }

    private static void SaveData(this bool Parameter, string Key)
    {
        SaveData(Key, Parameter);
    }

    private static void SaveData(this int Parameter, string Key)
    {
        SaveData(Key, Parameter);
    }

    private static void SaveData(this float Parameter, string Key)
    {
        SaveData(Key, Parameter);
    }

    private static void SaveData(string Key, bool Value)
    {
        PlayerPrefs.SetString(string.Format(EDITORPREF_FORMAT, Key), Value.ToString());
        if (showLog)
            LoadData(Key, Value);
    }

    private static void SaveData(string Key, int Value)
    {
        PlayerPrefs.SetInt(string.Format(EDITORPREF_FORMAT, Key), Value);
        if (showLog)
            LoadData(Key, Value);
    }

    private static void SaveData(string Key, float Value)
    {
        PlayerPrefs.SetFloat(string.Format(EDITORPREF_FORMAT, Key), Value);
        if (showLog)
            LoadData(Key, Value);
    }

    private static void SaveData(string Key, string Value)
    {
        PlayerPrefs.SetString(string.Format(EDITORPREF_FORMAT, Key), Value);
        if (showLog)
            LoadData(Key, Value);
    }

#endregion SaveData

}
