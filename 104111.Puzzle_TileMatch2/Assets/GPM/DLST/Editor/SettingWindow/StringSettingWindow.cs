using System.Collections.Generic;

namespace Gpm.Dlst
{
    public class StringSettingWindow : BaseSettingWindow
    {
        public static StringSettingWindow window;

        public static StringSettingWindow ShowWindow(System.Action<List<string>> callback)
        {
            if (window != null)
            {
                window.Close();
                window = null;
            }

            window = GetWindowWithRect<StringSettingWindow>(
                        windowRect,
                        true,
                        string.Format(
                            "{0} - {1}",
                            DlstUi.GetString(DlstStrings.KEY_TITLE_BAR),
                            DlstUi.GetString(DlstStrings.KEY_TITLE_EXCLUDE_STRING)));

            window.mainWindowCallback = callback;
            window.Initialize();

            return window;
        }

        protected override string SavePath
        {
            get { return DlstWindow.savePathString; }
        }

        protected override string ExcludeFilePath
        {
            get { return string.Format("{0}{1}", SavePath, DlstConstants.FILE_NAME_EXCLUDE_STRING_XML); }
        }

        public override void Initialize()
        {
            base.Initialize();

            titleExclude            = DlstStrings.KEY_TITLE_EXCLUDE_STRING;
            addTitle                = DlstStrings.KEY_ADD_STRING;
            listTitle               = DlstStrings.KEY_STRING_LIST;
            popupTitle              = DlstStrings.KEY_POPUP_TITLE_DUPLICATE_STRING;
            popupMessage            = DlstStrings.KEY_POPUP_MESSAGE_DUPLICATE_STRING;
            removePopupTitle        = DlstStrings.KEY_POPUP_TITLE_REMOVE_STRING;
            removePopupMessage      = DlstStrings.KEY_POPUP_MESSAGE_REMOVE_STRING;
            addButton               = DlstStrings.KEY_BUTTON_ADD_STRING;
            removeTitle             = DlstStrings.KEY_BUTTON_REMOVE_STRING;
            xmlFileName             = DlstConstants.FILE_NAME_EXCLUDE_STRING_XML;
        }

        protected override void SetSavePath(string path)
        {
            DlstWindow.savePathString = path;
        }

        protected override void SetPlayerPrefs()
        {
            DlstConstants.SavePathExcludeString = DlstWindow.savePathString;
        }

        private void OnDestroy()
        {
            window = null;
        }
    }
}
 