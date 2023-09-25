using Gpm.Common.Indicator;
using Gpm.Common.Log;
using Gpm.Common.Multilanguage;
using Gpm.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Gpm.Dlst
{
    public class DlstWindow : EditorWindow
    {
        private class LibraryInfo
        {
            public bool     isDeleteChecked = false;
            public string   filePath        = string.Empty;
            public string   fileName        = string.Empty;
        }

        private class LibraryGroupInfo
        {
            public string name;
            public List<LibraryInfo> list = new List<LibraryInfo>();

            public bool IsShow
            {
                get { return list.Count > 1; }
            }

            public int ListItemHeight
            {
                get { return LIST_ITEM_LABRARY_NAME_HEIGHT + (list.Count * LIST_ITEM_LABRARY_PATH_HEIGHT); }
            }
        }

        public const    int SIZE_BUTTON_WIDTH           = 70;
        public const    int SIZE_BUTTON_HEIGHT          = 30;
                            
        public const    int SIZE_SETTING_BUTTON_WIDTH   = 70;
        public const    int SIZE_SETTING_BUTTON_HEIGHT  = 30;
                
        private static List<string> excludeStringList       = null;
        private static List<string> excludeLibraryList      = null;

        private static readonly     Rect windowRect              = new Rect(100, 100, 768, 600);                                         
        private readonly            Rect listRect                = new Rect(12, 190, 744, 379);
        private readonly            Rect scrollRect              = new Rect(13, 210, 742, 358);

        private const int SIZE_TEXT_FIELD_WIDTH     = 658;
        private const int SIZE_TEXT_FIELD_HEIGHT    = 26;

        private const int LIST_ITEM_LABRARY_NAME_HEIGHT = 26;
        private const int LIST_ITEM_LABRARY_PATH_HEIGHT = 18;

        private static readonly Regex SEARCH_REGEX = new Regex(
            DlstConstants.FILE_SEARCH_NAMING_REGEX_RULE,
            RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private static DlstWindow window;

        private static Dictionary<string, LibraryGroupInfo> duplicateLibraryDictionary   = null;        
        private static List<LibraryGroupInfo> duplicateDrawList;
                
        private static Vector2 scrollPos;
        private int listScrollTotalHeight = 0;
        private float listDrawStartPosY = 0;
        private float listDrawEndPosY = 0;
        private int listDrawStartIndex = -1;
        private int listDrawEndIndex = -1;

        private static StringSettingWindow      stringSettingWindow     = null;
        private static LibrarySettingWindow     librarySettingWindow    = null;

        public static string savePathString     = string.Empty;
        public static string savePathLibrary    = string.Empty;

        public const string EMPTY_LANGUAGES_VALUE = "-";
        public const int LANGUAGE_NOT_FOUND = -1;

        private string[] languages;
        private int selectedLanguageIndex;

        private static object lockObject = new object();

        public static void ShowWindow()
        {
            LanguageLoad(
                () =>
                {
                    scrollPos = Vector2.zero;

                    if (window != null)
                    {
                        window.Close();
                        window = null;
                    }

                    GetWindowWithRect<DlstWindow>(
                        windowRect,
                        true,
                        DlstUi.GetString(DlstStrings.KEY_TITLE_BAR));
                });
        }

        private static void LanguageLoad(Action callback, bool opened = true)
        {
            GpmMultilanguage.Load(
                DlstConstants.SERVICE_NAME,
                DlstConstants.LANGUAGE_FILE_PATH,
                (result, resultMsg) =>
                {
                    if (result != MultilanguageResultCode.SUCCESS && result != MultilanguageResultCode.ALREADY_LOADED)
                    {
                        GpmLogger.Error(string.Format("Language load failed. (type= {0})", result), DlstConstants.SERVICE_NAME, typeof(DlstWindow));
                        return;
                    }

                    callback();
                });
        }

        private void OnEnable()
        {
            window = this;
            Initialize();
        }

        private void OnDestroy()
        {
            if (librarySettingWindow != null)
            {
                librarySettingWindow.Close();
                librarySettingWindow = null;
            }

            if (stringSettingWindow != null)
            {
                stringSettingWindow.Close();
                stringSettingWindow = null;
            }
        }

        private void Initialize()
        {
            titleContent = DlstUi.GetContent(DlstStrings.KEY_TITLE_BAR);

            EditorApplication.playModeStateChanged -= OnPlaymodeChanged;
            EditorApplication.playModeStateChanged += OnPlaymodeChanged;

            duplicateLibraryDictionary = null;

            savePathString = DlstConstants.SavePathExcludeString;
            savePathLibrary = DlstConstants.SavePathExcludeLibrary;

            LoadExcludeList(
                string.Format("{0}{1}", savePathString, DlstConstants.FILE_NAME_EXCLUDE_STRING_XML),
                (stringList) =>
                {
                    lock (lockObject)
                    {
                        excludeStringList = stringList;
                    }
                    LoadExcludeList(
                            string.Format("{0}{1}", savePathLibrary, DlstConstants.FILE_NAME_EXCLUDE_LIBRARY_XML),
                            (libraryList) =>
                            {
                                lock (lockObject)
                                {
                                    excludeLibraryList = libraryList;
                                }

                                duplicateLibraryDictionary = new Dictionary<string, LibraryGroupInfo>();

                                SearchDuplicateFiles();
                            });
                });

            if (GpmMultilanguage.IsLoadService(DlstConstants.SERVICE_NAME) == true)
            {
                languages = GpmMultilanguage.GetSupportLanguages(DlstConstants.SERVICE_NAME, true);
                if (languages != null)
                {
                    string lastLanguageName = DlstConstants.LastLanguageName;
                    if (string.IsNullOrEmpty(lastLanguageName) == false)
                    {
                        GpmMultilanguage.SelectLanguageByNativeName(
                            DlstConstants.SERVICE_NAME,
                            lastLanguageName,
                            (result, resultMessage) =>
                            {
                                if (result != MultilanguageResultCode.SUCCESS)
                                {
                                    GpmLogger.Warn(
                                        string.Format("{0} (Code= {1})", DlstUi.GetString(DlstStrings.KEY_CHANGE_LANGUAGE_ERROR_MESSAGE), result),
                                        DlstConstants.SERVICE_NAME,
                                        GetType());
                                }
                            });
                    }

                    selectedLanguageIndex = GetSelectLanguageIndex(GpmMultilanguage.GetSelectLanguage(DlstConstants.SERVICE_NAME, true));
                }
                else
                {
                    languages = new[] { EMPTY_LANGUAGES_VALUE };
                    selectedLanguageIndex = 0;
                }
            }
            else
            {
                languages = new[] { EMPTY_LANGUAGES_VALUE };
                selectedLanguageIndex = 0;

                Reload();
            }
        }

        private void OnPlaymodeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Reload();
            }
        }

        private void Reload()
        {
            LanguageLoad(() =>
            {
                Initialize();
                Repaint();
            });
        }

        private void Update()
        {
            int startItemIndex = -1;
            int endItemIndex = -1;

            listScrollTotalHeight = 0;

            if (duplicateDrawList != null && duplicateDrawList.Count > 0)
            {
                for (int i = 0; i < duplicateDrawList.Count; i++)
                {
                    var data = duplicateDrawList[i];

                    listScrollTotalHeight += data.ListItemHeight;

                    if (startItemIndex == -1 && listScrollTotalHeight >= scrollPos.y)
                    {
                        startItemIndex = i;
                        listDrawStartPosY = listScrollTotalHeight - data.ListItemHeight;
                    }
                    if (endItemIndex == -1 && listScrollTotalHeight >= scrollPos.y + scrollRect.height)
                    {
                        endItemIndex = i;
                        listDrawEndPosY = listScrollTotalHeight;
                    }
                }

                if (endItemIndex == -1)
                {
                    endItemIndex = duplicateDrawList.Count - 1;
                    listDrawEndPosY = listScrollTotalHeight;
                }
                
                bool isChanged = false;
                if (startItemIndex != listDrawStartIndex)
                {
                    listDrawStartIndex = startItemIndex;
                    isChanged = true;
                }
                if (endItemIndex != listDrawEndIndex)
                {
                    listDrawEndIndex = endItemIndex;
                    isChanged = true;
                }

                if (isChanged == true)
                {
                    Repaint();
                }
            }
        }

        private void OnGUI()
        {
            lock (lockObject)
            {
                EditorGUILayout.BeginVertical();
                {
                    DrawSearchOption();

                    DrawList();

                    DrawCopyright();

                    DrawVersion();
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSearchOption()
        {
            EditorGUILayout.BeginVertical(DlstUiStyle.GroupBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    DlstUi.Label(DlstStrings.KEY_TITLE_SEARCH_OPTION, DlstUiStyle.TitleLabel);

                    EditorGUI.BeginChangeCheck();
                    {
                        selectedLanguageIndex = DlstUi.PopupValue(selectedLanguageIndex, languages, GUILayout.Width(80));
                    }
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        string languageName = GetSelectLanguageCode();
                        GpmMultilanguage.SelectLanguageByNativeName(
                            DlstConstants.SERVICE_NAME,
                            languageName,
                            (result, resultMessage) =>
                            {
                                if (result == MultilanguageResultCode.SUCCESS)
                                {
                                    DlstConstants.LastLanguageName = languageName;
                                }
                                else
                                {
                                    GpmLogger.Warn(
                                        string.Format("{0} (Code= {1})", DlstUi.GetString(DlstStrings.KEY_CHANGE_LANGUAGE_ERROR_MESSAGE), result),
                                        DlstConstants.SERVICE_NAME,
                                        GetType());
                                }
                            });
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical(DlstUiStyle.HideBox);
                {
                    DrawExcludeString();
                    EditorGUILayout.Space();
                    DrawExcludeLibrary();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawExcludeString()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {   
                    GUILayout.BeginHorizontal();
                    {
                        DlstUi.Label(DlstStrings.KEY_TITLE_EXCLUDE_STRING, DlstUiStyle.CategoryLabel);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {                        
                        if (DlstUi.Button(
                                DlstStrings.KEY_BUTTON_SETTING, 
                                GUILayout.Width(SIZE_SETTING_BUTTON_WIDTH), 
                                GUILayout.Height(SIZE_SETTING_BUTTON_HEIGHT)) == true)
                        {
                            stringSettingWindow = StringSettingWindow.ShowWindow(
                                (excludeList) => 
                                {
                                    lock (lockObject)
                                    {
                                        excludeStringList = excludeList;

                                        duplicateLibraryDictionary = new Dictionary<string, LibraryGroupInfo>();

                                        SearchDuplicateFiles();

                                        window.Repaint();
                                    }
                                });                            
                        }
                        
                        GUILayout.BeginVertical();
                        {
                            StringBuilder context = new StringBuilder();

                            if (excludeStringList != null)
                            {
                                foreach (var text in excludeStringList)
                                {
                                    context.Append(text).Append("; ");
                                }
                            }

                            DlstUi.TextField(context.ToString(), GUILayout.Width(SIZE_TEXT_FIELD_WIDTH), GUILayout.Height(SIZE_TEXT_FIELD_HEIGHT));
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawExcludeLibrary()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        DlstUi.Label(DlstStrings.KEY_TITLE_EXCLUDE_LIBRARY, DlstUiStyle.CategoryLabel);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (DlstUi.Button(
                                DlstStrings.KEY_BUTTON_SETTING,
                                GUILayout.Width(SIZE_SETTING_BUTTON_WIDTH),
                                GUILayout.Height(SIZE_SETTING_BUTTON_HEIGHT)) == true)
                        {
                            librarySettingWindow = LibrarySettingWindow.ShowWindow(
                                (excludeList) =>
                                {
                                    excludeLibraryList = excludeList;

                                    duplicateLibraryDictionary = new Dictionary<string, LibraryGroupInfo>();

                                    SearchDuplicateFiles();

                                    window.Repaint();
                                });
                        }
                        
                        GUILayout.BeginVertical();
                        {
                            StringBuilder context = new StringBuilder();

                            if (excludeLibraryList != null)
                            {
                                foreach (var text in excludeLibraryList)
                                {
                                    context.Append(text).Append("; ");
                                }
                            }

                            DlstUi.TextField(context.ToString(), GUILayout.Width(SIZE_TEXT_FIELD_WIDTH), GUILayout.Height(SIZE_TEXT_FIELD_HEIGHT));
                        }
                        GUILayout.EndVertical();                        
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawList()
        {
            EditorGUILayout.BeginVertical(DlstUiStyle.GroupBox);
            {
                DlstUi.Label(DlstStrings.KEY_TITLE_SEARCH_LIST, DlstUiStyle.TitleLabel);

                EditorGUILayout.BeginVertical(DlstUiStyle.ListBox, GUILayout.Height(listRect.height));
                {
                    DlstUi.DrawOutline(listRect);

                    DrawListButton();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (duplicateDrawList != null && duplicateDrawList.Count > 0)
                        {
                            Rect scrollViewRect = new Rect(0, 0, scrollRect.width - 18, Mathf.Max(listScrollTotalHeight, scrollRect.height));

                            scrollPos = GUI.BeginScrollView(scrollRect, scrollPos, scrollViewRect);
                            {
                                GUILayout.BeginArea(new Rect(8, listDrawStartPosY, scrollRect.width, listDrawEndPosY));
                                for (int i = listDrawStartIndex; i <= listDrawEndIndex; i++)
                                {
                                    if (i >= duplicateDrawList.Count)
                                    {
                                        break;
                                    }

                                    var info = duplicateDrawList[i];

                                    GUILayout.Space(6);

                                    EditorGUILayout.BeginVertical();
                                    {
                                        DlstUi.LabelValue(info.name, DlstUiStyle.DuplicateCellLabel);
                                        foreach (LibraryInfo libraryInfo in info.list)
                                        {
                                            libraryInfo.isDeleteChecked = GUILayout.Toggle(libraryInfo.isDeleteChecked,
                                                string.Format(" {0}", libraryInfo.filePath.Replace(ReplaceDirectorySeparator(Application.dataPath), string.Empty)));
                                        }
                                    }
                                    EditorGUILayout.EndVertical();
                                }
                                GUILayout.EndArea();
                            }
                            GUI.EndScrollView();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawListButton()
        {
            EditorGUILayout.BeginHorizontal(DlstUiStyle.Toolbar);
            {
                GUILayout.Space(4);

                if (DlstUi.Button(DlstStrings.KEY_BUTTON_REFRESH, DlstUiStyle.ToolbarButton, GUILayout.ExpandWidth(false)) == true)
                {
                    SearchDuplicateFiles();
                }

                GUI.enabled = false;
                if (duplicateLibraryDictionary != null)
                {
                    foreach (string keyString in duplicateLibraryDictionary.Keys)
                    {
                        LibraryGroupInfo info = duplicateLibraryDictionary[keyString];
                        if (info.IsShow == true)
                        {
                            foreach (LibraryInfo libraryInfo in info.list)
                            {
                                if (libraryInfo.isDeleteChecked == true)
                                {
                                    GUI.enabled = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (DlstUi.Button(DlstStrings.KEY_BUTTON_DELETE, DlstUiStyle.ToolbarButton, GUILayout.ExpandWidth(false)) == true)
                {
                    DeleteLibrary();
                }

                GUI.enabled = true;

                int count = 0;

                if (duplicateLibraryDictionary != null)
                {
                    foreach (string key in duplicateLibraryDictionary.Keys)
                    {
                        LibraryGroupInfo info = duplicateLibraryDictionary[key];
                        if (info.IsShow == true)
                        {
                            count++;
                        }
                    }
                }

                DlstUi.LabelValue(string.Format("{0} : {1}", DlstUi.GetString(DlstStrings.KEY_TITLE_SEARCH_COUNT), count), DlstUiStyle.CountLabel);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCopyright()
        {
            GUILayout.Space(2);

            GUI.Label(new Rect(0, windowRect.height - 22, 768, 20), DlstStrings.TEXT_COPYRIGHT, DlstUiStyle.CopyrightLabel);
        }

        private void DrawVersion()
        {
            GUILayout.Space(2);

            GUI.Label(new Rect(10, windowRect.height - 22, 748, 20), DlstVersion.VERSION, DlstUiStyle.VersionLabel);
        }

        private void DrawDuplicateCell(string keyName, List<LibraryInfo> libraryInfoList)
        {
            EditorGUILayout.BeginVertical(DlstUiStyle.CellBox);
            {
                DlstUi.LabelValue(keyName, DlstUiStyle.DuplicateCellLabel);

                foreach (LibraryInfo libraryInfo in libraryInfoList)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        libraryInfo.isDeleteChecked = GUILayout.Toggle(
                            libraryInfo.isDeleteChecked,
                            string.Format(" {0}", libraryInfo.filePath.Replace(ReplaceDirectorySeparator(Application.dataPath), string.Empty)));
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static List<string> GetDetectPlugins(string sourceFolder)
        {                        
            string[] pluginsDirectories = Directory.GetDirectories(sourceFolder, "Plugins", SearchOption.AllDirectories);

            List<string> directoryList = new List<string>();
            foreach (string pluginsPath in pluginsDirectories)
            {
                directoryList.Add(pluginsPath);
            }

            List<string> plugins = new List<string>();
            foreach(string directory in directoryList)
            {
                foreach(string pattern in DlstConstants.PLUGIN_FILE_PATTERN)
                {
                    string[] patternFiles = Directory.GetFiles(directory, string.Format("*{0}", pattern), SearchOption.AllDirectories);

                    foreach (string plugin in patternFiles)
                    {
                        plugins.Add(plugin);
                    }
                }

                foreach (string pattern in DlstConstants.PLUGIN_FOLDER_PATTERN)
                {
                    string[] patternDirectories = Directory.GetDirectories(directory, string.Format("*{0}", pattern), SearchOption.AllDirectories);

                    foreach (string plugin in patternDirectories)
                    {
                        plugins.Add(plugin);
                    }
                }
            }

            return plugins;            
        }

        private static void SearchDuplicateFiles()
        {
            lock (lockObject)
            {
                duplicateLibraryDictionary.Clear();

                string path = ReplaceDirectorySeparator(Application.dataPath);

                List<string> plugins = GetDetectPlugins(path);
                if (plugins == null || plugins.Count == 0)
                {
                    return;
                }

                foreach (var pluginPath in plugins)
                {
                    string[] fileNameSplit = pluginPath.Split(Path.DirectorySeparatorChar);
                    string fileName = fileNameSplit[fileNameSplit.Length - 1];

                    Match match = SEARCH_REGEX.Match(fileName);

                    string libraryName = match.Groups[DlstConstants.FILE_SEARCH_NAMING_RULE_KEY_FILENAME].Value;
                    string extension = match.Groups[DlstConstants.FILE_SEARCH_NAMING_RULE_KEY_EXTENSION].Value;

                    if (string.IsNullOrEmpty(libraryName) == true)
                    {
                        continue;
                    }
                    
                    if (excludeLibraryList == null || excludeLibraryList.Contains(libraryName) == true)
                    {
                        continue;
                    }

                    foreach (string excludeString in excludeStringList)
                    {
                        libraryName = libraryName.Replace(excludeString, string.Empty);
                    }

                    if (string.IsNullOrEmpty(libraryName) == false)
                    {
                        LibraryInfo libraryInfo = new LibraryInfo();
                        libraryInfo.isDeleteChecked = false;
                        libraryInfo.fileName = fileName;
                        libraryInfo.filePath = pluginPath;

                        string libraryKey = string.Format("{0}.{1}", libraryName, extension);

                        if (duplicateLibraryDictionary.ContainsKey(libraryKey) == true)
                        {
                            duplicateLibraryDictionary[libraryKey].list.Add(libraryInfo);
                            continue;
                        }

                        LibraryGroupInfo libraryInfoList = new LibraryGroupInfo() { name = libraryKey };
                        libraryInfoList.list.Add(libraryInfo);
                        duplicateLibraryDictionary.Add(libraryKey, libraryInfoList);
                    }
                }

                duplicateDrawList = duplicateLibraryDictionary.Values.Where(data => data.IsShow).ToList();
            }
        }
        
        private void DeleteLibrary()
        {
            if (DlstUi.TryDialog(DlstStrings.KEY_POPUP_DELETE_TITLE, DlstStrings.KEY_POPUP_DELETE_CONTEXT) == true)
            {
                StringBuilder deleteLibraries = new StringBuilder();

                foreach (string keyString in duplicateLibraryDictionary.Keys)
                {
                    LibraryGroupInfo info = duplicateLibraryDictionary[keyString];
                    if (info.IsShow == true)
                    {
                        foreach (LibraryInfo libraryInfo in info.list)
                        {
                            if (libraryInfo.isDeleteChecked == true)
                            {
                                deleteLibraries.AppendFormat("{0}\n", libraryInfo.fileName);
                                DeleteFile (libraryInfo.filePath);
                            }
                        }
                    }
                }

                GpmIndicator.Send(
                    DlstConstants.SERVICE_NAME,
                    DlstVersion.VERSION,
                    new Dictionary<string, string>()
                    {
                        { "ACTION", "LibraryDelete" },
                        { "ACTION_DETAIL_1", deleteLibraries.ToString() }
                    });

                SearchDuplicateFiles();

                AssetDatabase.Refresh();
                
                if (DlstUi.ConfirmDialog(DlstStrings.KEY_POPUP_COMPLETED_TITLE, DlstStrings.KEY_POPUP_COMPLETED_CONTEXT) == true)
                {
                }
            }
        }

		private void DeleteFile(string path)
		{
            try
            {
                FileUtil.DeleteFileOrDirectory(path);
                FileUtil.DeleteFileOrDirectory(string.Format("{0}.meta", path));
            }
            catch (Exception ex)
            {
                GpmLogger.Debug(string.Format("File not delete. Path : {0}", ex.Message), DlstConstants.SERVICE_NAME, GetType());
            }
		}

        public static string ReplaceDirectorySeparator(string path)
        {
            return path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

        public static void CheckXMLManagerError(XmlHelper.ResponseCode responseCode, string message, Action callback = null)
        {
            string msg = string.Empty;

            switch (responseCode)
            {
                case XmlHelper.ResponseCode.FILE_NOT_FOUND_ERROR:
                    {
                        msg = string.Format(DlstUi.GetString(DlstStrings.KEY_POPUP_006_MESSAGE), message);
                        break;
                    }
                case XmlHelper.ResponseCode.DATA_IS_NULL_ERROR:
                    {
                        msg = DlstUi.GetString(DlstStrings.KEY_POPUP_007_MESSAGE);
                        break;
                    }
                case XmlHelper.ResponseCode.PATH_IS_NULL_ERROR:
                    {
                        msg = DlstUi.GetString(DlstStrings.KEY_POPUP_008_MESSAGE);
                        break;
                    }
                case XmlHelper.ResponseCode.UNKNOWN_ERROR:
                    {
                        msg = string.Format(DlstUi.GetString(DlstStrings.KEY_POPUP_009_MESSAGE), message);
                        break;
                    }
            }

            if (DlstUi.ConfirmDialog(DlstStrings.KEY_POPUP_XML_TITLE, msg, false) == true)
            {
                if (callback != null)
                {
                    callback();
                }
            }
        }

        public static void LoadExcludeList(string path, Action<List<string>> callback)
        {
            if (File.Exists(path) == true)
            {
                XmlHelper.LoadXmlFromFile<ExcludeVO>(
                    path,
                    (responseCode, data, message) =>
                    {
                        if (XmlHelper.ResponseCode.SUCCESS != responseCode)
                        {
                            CheckXMLManagerError(
                                responseCode,
                                message);
                            callback(new List<string>());
                            return;
                        }

                        callback(data.excludeList);
                    });
            }
            else
            {
                callback(new List<string>());
            }
        }

        private string GetSelectLanguageCode()
        {
            if (selectedLanguageIndex >= languages.Length)
            {
                return string.Empty;
            }

            return languages[selectedLanguageIndex];
        }

        private int GetSelectLanguageIndex(string languageCode)
        {
            for (int i = 0; i < languages.Length; i++)
            {
                if (languages[i].Equals(languageCode) == true)
                {
                    return i;
                }
            }

            return LANGUAGE_NOT_FOUND;
        }
    }
}
