using Gpm.Common.Multilanguage;
using UnityEngine;
using UnityEditor;

namespace Gpm.Dlst
{
    internal static class DlstUi
    {
        public static void Label(string key, params GUILayoutOption[] options)
        {
            GUILayout.Label(GetString(key), options);
        }
        public static void Label(string key, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label(GetString(key), style, options);
        }

        public static void LabelValue(string text, params GUILayoutOption[] options)
        {
            GUILayout.Label(text, options);
        }
        public static void LabelValue(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label(text, style, options);
        }
        public static void LabelValue(Rect rect, string text, GUIStyle style)
        {
            GUI.Label(rect, text, style);
        }

        public static bool Button(string key, params GUILayoutOption[] options)
        {
            return GUILayout.Button(GetString(key), options);
        }
        public static bool Button(string key, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Button(GetString(key), style, options);
        }
        public static bool Button(Rect rect, string key)
        {
            return GUI.Button(rect, GetString(key));
        }

        public static string TextField(string text, params GUILayoutOption[] options)
        {
            return TextField(text, DlstUiStyle.TextField, options);
        }
        public static string TextField(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            return EditorGUILayout.TextField(text, style, options);
        }

        public static int Popup(int selectedIndex, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
        {
            string[] strings = new string[displayedOptions.Length];
            for (int i = 0; i < displayedOptions.Length; i++)
            {
                strings[i] = GetString(displayedOptions[i]);
            }

            return EditorGUILayout.Popup(selectedIndex, strings, style, options);
        }

        public static int PopupValue(int selectedIndex, string[] strings, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Popup(selectedIndex, strings, DlstUiStyle.Popup, options);
        }

        public static bool Toggle(bool value, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Toggle(value, options);
        }

        public static bool TryDialog(string titleKey, string message, bool isMessageKey = true)
        {
            return EditorUtility.DisplayDialog(
                GetString(titleKey),
                isMessageKey ? GetString(message) : message,
                GetString(DlstStrings.KEY_POPUP_OK),
                GetString(DlstStrings.KEY_POPUP_CANCEL));
        }

        public static bool ConfirmDialog(string titleKey, string message, bool isMessageKey = true)
        {
            return EditorUtility.DisplayDialog(
                GetString(titleKey),
                isMessageKey ? GetString(message) : message,
                GetString(DlstStrings.KEY_POPUP_OK));
        }

        public static Rect Window(int id, Rect screenRect, GUI.WindowFunction func, string text, params GUILayoutOption[] options)
        {
            return GUILayout.Window(id, screenRect, func, GetString(text), options);
        }

        public static string OpenFolderPanel(string titleKey, string folder, string defaultName)
        {
            return EditorUtility.OpenFolderPanel(GetString(titleKey), folder, defaultName);
        }

        public static GUIContent GetContent(string key)
        {
            return new GUIContent(GetString(key));
        }

        public static string GetString(string key)
        {
            return GpmMultilanguage.GetString(DlstConstants.SERVICE_NAME, key);
        }

        private static readonly Texture2D splitTexture;

        static DlstUi()
        {
            splitTexture = new Texture2D(1, 1);
            splitTexture.SetPixel(0, 0, new Color(0.16f, 0.16f, 0.16f));
            splitTexture.hideFlags = HideFlags.HideAndDontSave;
            splitTexture.name = "SplitTexture";
            splitTexture.Apply();
        }

        public static void DrawOutline(Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Rect rectWidth = new Rect(rect.x, rect.y - 1, rect.width, 1);
            GUI.DrawTexture(rectWidth, splitTexture);

            rectWidth.y += rect.height;
            GUI.DrawTexture(rectWidth, splitTexture);


            Rect rectHeight = new Rect(rect.x, rect.y, 1, rect.height);
            GUI.DrawTexture(rectHeight, splitTexture);

            rectHeight.x += rect.width - 1;
            GUI.DrawTexture(rectHeight, splitTexture);
        }
    }
}