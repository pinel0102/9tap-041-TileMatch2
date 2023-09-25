using Gpm.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gpm.Dlst
{
    public abstract class BaseSettingWindow : EditorWindow
    {
        public class ExcludeInfo
        {
            public bool isDeleteChecked = false;
            public string data;

            public ExcludeInfo(string data)
            {
                this.data = data;
            }
        }

        protected static readonly Rect windowRect = new Rect(100, 100, 512, 366);
        private static readonly Rect scrollRect = new Rect(10, 195, 486, 230);

        private const string ADD_IGNORE_TEXTFILED = "AddIgnoreTextField";

        private const int   SIZE_TEXT_FIELD_WIDTH   = 418;
        private const int   SIZE_TEXT_FIELD_HEIGHT  = 26;

        private List<ExcludeInfo> excludeInfoList;

        public Action<List<string>> mainWindowCallback;        

        private Vector2 scrollPos;

        private bool isDimmed = false;

        private string exclude = string.Empty;

        public string titleExclude;        
        public string SavePathPopup;
        public string addTitle;
        public string listTitle;
        public string popupTitle;
        public string popupMessage;
        public string removePopupTitle;
        public string removePopupMessage;
        public string addButton;
        public string removeTitle;
        public string okButton;
        public string cancelButton;
        public string xmlFileName;

        private bool isRefresh = false;

        protected abstract string SavePath { get; }
        protected abstract string ExcludeFilePath { get; }

        private bool Initialized
        {
            get { return mainWindowCallback != null; }
        }

        private bool IsAllCheck
        {
            get
            {
                if (excludeInfoList == null || excludeInfoList.Count == 0)
                {
                    return false;
                }

                return excludeInfoList.All(data => data.isDeleteChecked == true);
            }
        }

        public virtual void Initialize()
        {
            SavePathPopup = DlstStrings.KEY_SAVE_PATH_POPUP;
            okButton = DlstStrings.KEY_POPUP_OK;
            cancelButton = DlstStrings.KEY_POPUP_CANCEL;
        }

        protected abstract void SetSavePath(string path);

        protected abstract void SetPlayerPrefs();

        private void RefreshCheck()
        {
            if(isRefresh == true)
            {
                isRefresh = false;
                AssetDatabase.Refresh();
            }
        }

        private void OnEnable()
        {
            DlstWindow.LoadExcludeList(
                ExcludeFilePath,
                (excludeList) =>
                {
                    excludeInfoList = new List<ExcludeInfo>();
                    foreach (string data in excludeList)
                    {
                        excludeInfoList.Add(new ExcludeInfo(data));
                    }
                });
        }
        
        private void Update()
        {
            RefreshCheck();

            if (Initialized == false)
            {
                Close();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(DlstUiStyle.CellBox);
            {
                DrawSavePath();
            }
            EditorGUILayout.EndVertical();

            EditorGUI.BeginDisabledGroup(isDimmed);
            EditorGUILayout.BeginVertical(DlstUiStyle.GroupBox);
            {
                DlstUi.Label(listTitle, DlstUiStyle.CategoryLabel);

                Rect listRect = EditorGUILayout.BeginVertical(DlstUiStyle.ListBox);
                {
                    DlstUi.DrawOutline(listRect);
                    DrawListToolbar();
                    DrawList();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();

            DrawCopyright();
        }

        private void DrawSavePath()
        {
            DlstUi.Label(DlstStrings.KEY_SAVE_PATH, DlstUiStyle.CategoryLabel);

            EditorGUILayout.BeginHorizontal();
            {
                if (DlstUi.Button(
                        SavePathPopup,
                        GUILayout.Width(DlstWindow.SIZE_BUTTON_WIDTH),
                        GUILayout.Height(DlstWindow.SIZE_BUTTON_HEIGHT)) == true)
                {
                    string path = DlstUi.OpenFolderPanel(DlstStrings.KEY_SAVE_PATH, string.Empty, string.Empty);
                    if (string.IsNullOrEmpty(path) != true && Directory.Exists(path) == true)
                    {
                        isDimmed = true;

                        SetSavePath(string.Format("{0}/", path));
                        SaveData();
                    }
                }

                DlstUi.TextField(SavePath, GUILayout.Width(SIZE_TEXT_FIELD_WIDTH), GUILayout.Height(SIZE_TEXT_FIELD_HEIGHT));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawListToolbar()
        {
            EditorGUILayout.BeginHorizontal(DlstUiStyle.Toolbar);
            {
                EditorGUI.BeginDisabledGroup(excludeInfoList == null || excludeInfoList.Count == 0);
                {
                    bool isChecked = DlstUi.Toggle(IsAllCheck, GUILayout.Width(20));
                    if (isChecked != IsAllCheck && excludeInfoList != null)
                    {
                        excludeInfoList.ForEach(data => data.isDeleteChecked = isChecked);
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(excludeInfoList == null || excludeInfoList.Any(data => data.isDeleteChecked) == false);
                {
                    if (DlstUi.Button(removeTitle, DlstUiStyle.ToolbarButton, GUILayout.ExpandWidth(false)) == true)
                    {
                        if (DlstUi.TryDialog(removePopupTitle, removePopupMessage) == true)
                        {
                            isDimmed = true;

                            List<ExcludeInfo> deleteList = new List<ExcludeInfo>();

                            foreach (ExcludeInfo data in excludeInfoList)
                            {
                                if (data.isDeleteChecked == true)
                                {
                                    deleteList.Add(data);
                                }
                            }

                            DeleteExcludeList(deleteList);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();

                GUI.SetNextControlName(ADD_IGNORE_TEXTFILED);
                exclude = DlstUi.TextField(exclude, DlstUiStyle.ToolbarTextField, GUILayout.Width(260));

                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(exclude));
                {
                    if (DlstUi.Button(addButton, DlstUiStyle.ToolbarButton, GUILayout.ExpandWidth(false)) == true ||
                        Event.current.isKey == true && Event.current.keyCode == KeyCode.Return)
                    {
                        if (string.IsNullOrEmpty(exclude) == false)
                        {
                            if (excludeInfoList.Find(item => item.data.Equals(exclude)) != null)
                            {
                                if (DlstUi.ConfirmDialog(popupTitle, popupMessage) == true)
                                {
                                }
                            }
                            else
                            {
                                isDimmed = true;
                                EditorApplication.delayCall += delegate
                                {
                                    AddExcludeList(exclude);
                                    Repaint();
                                    EditorGUI.FocusTextInControl(ADD_IGNORE_TEXTFILED);
                                };
                            }
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawList()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(scrollRect.width), GUILayout.Height(scrollRect.height));
            {
                if (excludeInfoList != null)
                {
                    GUILayout.Space(6);

                    bool isFirst = true;
                    foreach (ExcludeInfo data in excludeInfoList)
                    {
                        if (isFirst == true)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            GUILayout.Space(4);
                        }

                        data.isDeleteChecked = GUILayout.Toggle(data.isDeleteChecked, data.data);
                    }

                    GUILayout.Space(6);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawCopyright()
        {
            GUI.Label(new Rect(0, windowRect.height - 22, 512, 20), DlstStrings.TEXT_COPYRIGHT, DlstUiStyle.CopyrightLabel);
        }

        private void AddExcludeList(string keyword)
        {
            excludeInfoList.Add(new ExcludeInfo(keyword));
            SaveData();
        }

        private void DeleteExcludeList(IEnumerable<ExcludeInfo> deleteKeywords)
        {
            foreach (ExcludeInfo data in deleteKeywords)
            {
                excludeInfoList.Remove(data);
            }
            SaveData();
        }

        private void SaveData()
        {
            if (excludeInfoList == null)
            {
                return;
            }

            string path = string.Format("{0}{1}", SavePath, xmlFileName);

            ExcludeVO excludeVO = new ExcludeVO()
            {
                excludeList = new List<string>()
            };

            excludeInfoList.Sort((left, right) => String.Compare(left.data, right.data, StringComparison.Ordinal));
            excludeInfoList.ForEach(item => excludeVO.excludeList.Add(item.data));

            XmlHelper.SaveXmlToFile(path, excludeVO, (responseCode, message) =>
            {
                isDimmed = false;

                if (XmlHelper.ResponseCode.SUCCESS != responseCode)
                {
                    DlstWindow.CheckXMLManagerError(
                        responseCode,
                        message);
                    return;
                }

                SetPlayerPrefs();

                exclude = string.Empty;

                mainWindowCallback(excludeVO.excludeList);

                isRefresh = true;
            });
        }
    }   
}
