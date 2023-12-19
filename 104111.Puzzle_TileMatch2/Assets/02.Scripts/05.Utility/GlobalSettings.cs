using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalSettings
{
    public static string GetUserGroup()
    {
        return "A";
    }

    public static void OpenURL_Review()
    {
#if UNITY_IOS        
        Application.OpenURL(string.Format(ProjectManager.storeURL_iOS, ProjectManager.appStoreConnectId));
#elif UNITY_ANDROID
        Application.OpenURL(string.Format(ProjectManager.storeURL_AOS, ProjectManager.appIdentifier));
#endif
    }

    public static void OpenURL_TermsOfService()
    {
        if (Application.systemLanguage == SystemLanguage.Korean)
        {
            Application.OpenURL(ProjectManager.servicesURL_KR);
        }
        else
        {
            Application.OpenURL(ProjectManager.servicesURL_EU);
        }
    }

    public static void OpenURL_PrivacyPolicy()
    {
        if (Application.systemLanguage == SystemLanguage.Korean)
        {
            Application.OpenURL(ProjectManager.privacyURL_KR);
        }
        else
        {
            Application.OpenURL(ProjectManager.privacyURL_EU);
        }
    }

    public static void OpenMail_Contacts()
    {
        string mailto = ProjectManager.supportsMail;
        string subject = EscapeURL(string.Format("{0} CS Report", ProjectManager.productName));
        string body = EscapeURL
            (
                "Please write your questions.\r\n\r\n\r\n\r\n" +
                "Device : " + SystemInfo.deviceModel + "\r\n" +
                "OS : " + SystemInfo.operatingSystem + "\r\n" +
                "Version : v" + Application.version + "-" + GetUserGroup()
            );

        Application.OpenURL(string.Format("mailto:{0}?subject={1}&body={2}", mailto, subject, body));
    }

    private static string EscapeURL(string url)
    {
        return UnityEngine.Networking.UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }
}
