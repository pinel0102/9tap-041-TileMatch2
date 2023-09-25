using System.Collections.Generic;

namespace Gpm.Dlst
{
    public class LibrarySettingWindow : BaseSettingWindow
    {
        public static LibrarySettingWindow window;

        public static LibrarySettingWindow ShowWindow(System.Action<List<string>> callback)
        {
            if (window != null)
            {
                window.Close();
                window = null;
            }

            window = GetWindowWithRect<LibrarySettingWindow>(
                        windowRect,
                        true,
                        string.Format(
                            "{0} - {1}",
                            DlstUi.GetString(DlstStrings.KEY_TITLE_BAR),
                            DlstUi.GetString(DlstStrings.KEY_TITLE_EXCLUDE_LIBRARY)));
            window.mainWindowCallback = callback;
            window.Initialize();

            return window;
        }

        protected override string SavePath
        {
            get { return DlstWindow.savePathLibrary; }
        }

        protected override string ExcludeFilePath
        {
            get { return string.Format("{0}{1}", SavePath, DlstConstants.FILE_NAME_EXCLUDE_LIBRARY_XML); }
        }

        public override void Initialize()
        {
            base.Initialize();

            titleExclude            = DlstStrings.KEY_TITLE_EXCLUDE_LIBRARY;            
            addTitle                = DlstStrings.KEY_ADD_LIBRARY_NAME;
            listTitle               = DlstStrings.KEY_LIBRARY_LIST;
            popupTitle              = DlstStrings.KEY_POPUP_TITLE_DUPLICATE_LIBRARY;
            popupMessage            = DlstStrings.KEY_POPUP_MESSAGE_DUPLICATE_LIBRARY;
            removePopupTitle        = DlstStrings.KEY_POPUP_TITLE_REMOVE_LIBRARY;
            removePopupMessage      = DlstStrings.KEY_POPUP_MESSAGE_REMOVE_LIBRARY;
            addButton               = DlstStrings.KEY_BUTTON_ADD_LIBRARY;
            removeTitle             = DlstStrings.KEY_BUTTON_REMOVE_LIBRARY;
            xmlFileName             = DlstConstants.FILE_NAME_EXCLUDE_LIBRARY_XML;
        }

        protected override void SetSavePath(string path)
        {
            DlstWindow.savePathLibrary = path;
        }

        protected override void SetPlayerPrefs()
        {
            DlstConstants.SavePathExcludeLibrary = DlstWindow.savePathLibrary;
        }
        
        private void OnDestroy()
        {
            window = null;
        }
    }
}
 