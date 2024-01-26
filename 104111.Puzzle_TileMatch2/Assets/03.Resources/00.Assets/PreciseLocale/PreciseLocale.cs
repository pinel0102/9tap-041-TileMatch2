#if UNITY_IOS && !IOS_SIMULATOR
#define IOS_BUILD
#endif

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class PreciseLocale : System.Object
{

    private interface PlatformBridge
    {
        string GetRegion();

        string GetLanguage();

        string GetLanguageID();

        string GetCurrencyCode();

        string GetCurrencySymbol();
    }

    private class EditorBridge : PlatformBridge
    {

        public string GetRegion()
        {
            return "US";
        }

        public string GetLanguage()
        {
            return "en";
        }

        public string GetLanguageID()
        {
            return "en_US";
        }

        public string GetCurrencyCode()
        {
            return "USD";
        }

        public string GetCurrencySymbol()
        {
            return "$";
        }
    }

    private static PlatformBridge _platform;

    private static PlatformBridge platform
    {
        get
        {
            if (_platform == null)
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
				_platform = new PreciseLocaleAndroid();

                #elif IOS_BUILD && !UNITY_EDITOR
				_platform = new PreciseLocaleiOS();

                #elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
				_platform = new PreciseLocaleOSX();

                #elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
				_platform = new PreciseLocaleWindows();

                #else
                _platform = new EditorBridge();
                #endif
            }

            return _platform;
        }
    }

    public static string GetRegion()
    {
        return platform.GetRegion();
    }

    public static string GetLanguageID()
    {
        return platform.GetLanguageID();
    }

    public static string GetLanguage()
    {
        return platform.GetLanguage();
    }

    public static string GetCurrencyCode()
    {
        return platform.GetCurrencyCode();
    }

    public static string GetCurrencySymbol()
    {
        return platform.GetCurrencySymbol();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
	private class PreciseLocaleAndroid: PlatformBridge {
		private static AndroidJavaClass _preciseLocale = new AndroidJavaClass("com.kokosoft.preciselocale.PreciseLocale");

		public string GetRegion() {
			return _preciseLocale.CallStatic<string>("getRegion");                                 
		}

		public string GetLanguage() {
			return _preciseLocale.CallStatic<string>("getLanguage");                                 
		}

		public string GetLanguageID() {
			return _preciseLocale.CallStatic<string>("getLanguageID");                                 
		}

		public string GetCurrencyCode() {
			return _preciseLocale.CallStatic<string>("getCurrencyCode");                                 
		}

		public string GetCurrencySymbol() {
			return _preciseLocale.CallStatic<string>("getCurrencySymbol");                                 
		}

	}


#endif

#if IOS_BUILD && !UNITY_EDITOR
	private class PreciseLocaleiOS: PlatformBridge {

		[DllImport ("__Internal")]
		private static extern string _getRegion();

		[DllImport ("__Internal")]
		private static extern string _getLanguageID();

		[DllImport ("__Internal")]
		private static extern string _getLanguage();

		[DllImport ("__Internal")]
		private static extern string _getCurrencyCode();

		[DllImport ("__Internal")]
		private static extern string _getCurrencySymbol();

		public string GetRegion() {
			return _getRegion();
		}

		public string GetLanguage() {
			return _getLanguage();
		}

		public string GetLanguageID() {
			return _getLanguageID();
		}

		public string GetCurrencyCode() {
			return _getCurrencyCode();
		}

		public string GetCurrencySymbol() {
			return _getCurrencySymbol();
		}

	}


#endif

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
	private class PreciseLocaleOSX: PlatformBridge {

		[DllImport ("PreciseLocaleOSX")]
		private static extern string _getRegion();

		[DllImport ("PreciseLocaleOSX")]
		private static extern string _getLanguageID();

		[DllImport ("PreciseLocaleOSX")]
		private static extern string _getLanguage();

		[DllImport ("PreciseLocaleOSX")]
		private static extern string _getCurrencyCode();

		[DllImport ("PreciseLocaleOSX")]
		private static extern string _getCurrencySymbol();

		public string GetRegion() {
			return _getRegion();
		}

		public string GetLanguage() {
			return _getLanguage();
		}

		public string GetLanguageID() {
			return _getLanguageID();
		}

		public string GetCurrencyCode() {
			return _getCurrencyCode();
		}

		public string GetCurrencySymbol() {
			return _getCurrencySymbol();
		}

	}


#endif

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
	private class PreciseLocaleWindows: PlatformBridge {
		[DllImport ("PreciseLocale", EntryPoint="_getLanguage")]
		private static extern IntPtr _getLanguage ();

		[DllImport ("PreciseLocale", EntryPoint="_getRegion")]
		private static extern IntPtr _getRegion ();

		[DllImport ("PreciseLocale", EntryPoint="_getCurrencyCode")]
		private static extern IntPtr _getCurrencyCode ();

		[DllImport ("PreciseLocale", EntryPoint ="_getCurrencySymbol")]
		private static extern IntPtr _getCurrencySymbol ();

        public string GetLanguage() {
            return Marshal.PtrToStringUni(_getLanguage());
        }

		public string GetLanguageID() {
            return GetLanguage() + "_" + GetRegion();
        }

		public string GetRegion () {
            return Marshal.PtrToStringUni(_getRegion());
        }

		public string GetCurrencyCode () {
            return Marshal.PtrToStringUni(_getCurrencyCode());
        }

		public string GetCurrencySymbol () {
            return Marshal.PtrToStringUni(_getCurrencySymbol());
        }
    }



#endif
}