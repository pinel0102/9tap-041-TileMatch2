using Gpm.Common.Multilanguage;
using UnityEngine;

namespace Gpm.Dlst
{
    public static class DlstConstants
    {
        public const string SERVICE_NAME = "DLST";

        public const string LANGUAGE_FILE_PATH = "GPM/DLST/XML/Language.xml";
        public const string FILE_NAME_EXCLUDE_LIBRARY_XML = "ExcludeLibrary.xml";
        public const string FILE_NAME_EXCLUDE_STRING_XML = "ExcludeString.xml";

        private const string PATH_XML = "/GPM/DLST/XML/";

        private const string LANGUAGE_CODE_KEY = "gpm.dlst.language";
        private const string SAVE_PATH_EXCLUDE_LIBRARY_KEY = "gpm.dlst.path_exclude_library";
        private const string SAVE_PATH_EXCLUDE_STRING_KEY = "gpm.dlst.path_exclude_string";

        public static readonly string[] PLUGIN_FILE_PATTERN = {
            "jar",
            "aar",
            "dll",
        };

        public static readonly string[] PLUGIN_FOLDER_PATTERN = {
            "bundle",
            "framework",
        };

        public const string FILE_SEARCH_NAMING_REGEX_RULE = @"((?<filename>[\d\D]*)(?<version>[\-_ ][a-zA-Z]?(\d+)(.\d+)?(.\d+)?(.\d+))|(?<filename>[\d\D]*))\.(?<extension>\w+)";
        public const string FILE_SEARCH_NAMING_RULE_KEY_FILENAME = "filename";
        public const string FILE_SEARCH_NAMING_RULE_KEY_EXTENSION = "extension";
        
        public static string LastLanguageName
        {
            get
            {
                return PlayerPrefs.GetString(LANGUAGE_CODE_KEY);
            }
            set
            {
                PlayerPrefs.SetString(LANGUAGE_CODE_KEY, value);
            }
        }

        public static string SavePathExcludeLibrary
        {
            get
            {
                if (PlayerPrefs.HasKey(SAVE_PATH_EXCLUDE_LIBRARY_KEY) == true)
                {
                    return PlayerPrefs.GetString(SAVE_PATH_EXCLUDE_LIBRARY_KEY);
                }

                return string.Format("{0}{1}", Application.dataPath, PATH_XML);
            }
            set
            {
                PlayerPrefs.SetString(SAVE_PATH_EXCLUDE_LIBRARY_KEY, value);
            }
        }

        public static string SavePathExcludeString
        {
            get
            {
                if (PlayerPrefs.HasKey(SAVE_PATH_EXCLUDE_STRING_KEY) == true)
                {
                    return PlayerPrefs.GetString(SAVE_PATH_EXCLUDE_STRING_KEY);
                }

                return string.Format("{0}{1}", Application.dataPath, PATH_XML);
            }
            set
            {
                PlayerPrefs.SetString(SAVE_PATH_EXCLUDE_STRING_KEY, value);
            }
        }
    }
}
