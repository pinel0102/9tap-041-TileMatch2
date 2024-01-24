using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#pragma warning disable 162, 219
public partial class SDKManager
{
    private static GlobalData globalData { get { return GlobalData.Instance; } }
    private static UserManager userManager { get { return GlobalData.Instance.userManager; } }

    public static bool showLogParams = false;

    private const string logColor = "yellow";
    private readonly static string logFormat0 = $"<color={logColor}>{{0}}</color>";
    private readonly static string logFormat1 = $"<color={logColor}>{{0}} {{1}}</color>";
    private readonly static string logFormat2 = $"<color={logColor}>{{0}} {{1}} ({{2}})</color>";

#region 분석

    ///<Summary>앱 기동시 유저 정보를 보낸다.</Summary>
    public static void SendAnalytics_User_App_Open()
    {
        Debug.Log(string.Format(logFormat0, CodeManager.GetMethodName()));

        string eventName = "user_app_open";
        var logParams = CreateParams(eventName);

        SendEvent(eventName, logParams);

        if(!userManager.Current.SendAppOpenCount && userManager.Current.AppOpenCount >= 10)
        {
            string eventAccrued = "i_app_open";
            SendAnalytics_Accumulated(eventAccrued);
            userManager.UpdateLog(sendAppOpenCount: true);
        }
    }

    ///<Summary>게임 시작시 관련 정보를 보낸다.</Summary>
    public static void SendAnalytics_I_Scene_Play()
    {
        Debug.Log(string.Format(logFormat0, CodeManager.GetMethodName()));
        
        string eventName = "i_scene_play";
        var logParams = CreateParams(eventName);

        SendEvent(eventName, logParams);
    }

    ///<Summary>게임 성공시 관련 정보를 보낸다.</Summary>
    public static void SendAnalytics_I_Scene_Clear()
    {
        Debug.Log(string.Format(logFormat0, CodeManager.GetMethodName()));

        userManager.UpdateLog(failedLevel: 0, failedCount: 0);

        string eventName = "i_scene_clear";
        var logParams = CreateParams(eventName);

        SendEvent(eventName, logParams);
    }

    ///<Summary>게임 실패시 관련 정보를 보낸다.</Summary>
    public static void SendAnalytics_I_Scene_Fail()
    {
        Debug.Log(string.Format(logFormat0, CodeManager.GetMethodName()));

        userManager.UpdateLog(failedLevel: globalData.CURRENT_LEVEL, failedCount: userManager.Current.FailedCount + 1);

        string eventName = "i_scene_fail";
        var logParams = CreateParams(eventName);
        logParams["option11"] = userManager.Current.FailedCount.ToString();

        SendEvent(eventName, logParams);
    }

    ///<Summary>기타 씬 시작시 관련 정보를 보낸다.</Summary>
    public static void SendAnalytics_I_Scene()
    {
        Debug.Log(string.Format(logFormat1, CodeManager.GetMethodName(), GlobalData.Instance.CURRENT_SCENE));
        
        string eventName = "i_scene";
        var logParams = CreateParams(eventName);

        SendEvent(eventName, logParams);
    }

    ///<Summary>게임 클리어 이후 유저 클릭 정보를 보낸다.</Summary>
    public static void SendAnalytics_C_Scene_Clear(string action)
    {
        Debug.Log(string.Format(logFormat1, CodeManager.GetMethodName(), action));

        string eventName = "c_scene_clear";
        var logParams = CreateParams(eventName);
        logParams["option11"] = action;

        SendEvent(eventName, logParams);
    }

    ///<Summary>게임 실패 이후 유저 클릭 정보를 보낸다.</Summary>
    public static void SendAnalytics_C_Scene_Fail(string action)
    {
        Debug.Log(string.Format(logFormat1, CodeManager.GetMethodName(), action));

        string eventName = "c_scene_fail";
        var logParams = CreateParams(eventName);
        logParams["option11"] = action;

        SendEvent(eventName, logParams);
    }

    ///<Summary>게임 중 유저 클릭 정보를 보낸다.</Summary>
    public static void SendAnalytics_C_Scene(string action)
    {
        Debug.Log(string.Format(logFormat1, CodeManager.GetMethodName(), action));

        string eventName = "c_scene";
        var logParams = CreateParams(eventName);
        logParams["option11"] = action;

        SendEvent(eventName, logParams);
    }

    ///<Summary>유저 클릭으로 아이템 획득시 관련 정보를 보낸다.</Summary>
    public static void SendAnalytics_C_Item_Get(string item_name, int count)
    {
        Debug.Log(string.Format(logFormat2, CodeManager.GetMethodName(), item_name, count));

        string eventName = "c_item_get";
        var logParams = CreateParams(eventName);
        logParams["option11"] = item_name;
        logParams["option12"] = count.ToString();

        SendEvent(eventName, logParams);
    }

    ///<Summary>유저 클릭으로 아이템 사용시 관련 정보를 보낸다.</Summary>
    public static void SendAnalytics_C_Item_Use(string item_name, int count)
    {
        Debug.Log(string.Format(logFormat2, CodeManager.GetMethodName(), item_name, count));

        string eventName = "c_item_use";
        var logParams = CreateParams(eventName);
        logParams["option11"] = item_name;
        logParams["option12"] = count.ToString();

        SendEvent(eventName, logParams);
    }


    ///<Summary>전면 광고 시청 시작시 관련 정보를 보낸다.</Summary>
    public static void SendAnalytics_Interstitial_Ads_Show()
    {
        Debug.Log(string.Format(logFormat0, CodeManager.GetMethodName()));

        string eventName = "Interstitial_Ads_Show";
        var logParams = CreateParams(eventName);

        SendEvent(eventName, logParams);

        userManager.UpdateLog(interstitalViewCount: userManager.Current.InterstitalViewCount + 1);
        if(!userManager.Current.SendInterstitalViewCount && userManager.Current.InterstitalViewCount >= 5)
        {
            string eventAccrued = "i_ad_interstitial_view";
            SendAnalytics_Accumulated(eventAccrued);
            userManager.UpdateLog(sendInterstitalViewCount: true);
        }
    }

    ///<Summary>리워드 광고 시청 시작시 관련 정보를 보낸다.</Summary>
    public static void SendAnalytics_Video_Ads_Show()
    {
        Debug.Log(string.Format(logFormat0, CodeManager.GetMethodName()));

        string eventName = "Video_Ads_Show";
        var logParams = CreateParams(eventName);

        SendEvent(eventName, logParams);

        userManager.UpdateLog(rewardViewCount: userManager.Current.RewardViewCount + 1);
        if(!userManager.Current.SendRewardViewCount && userManager.Current.RewardViewCount >= 3)
        {
            string eventAccrued = "i_ad_rewerd_view";
            SendAnalytics_Accumulated(eventAccrued);
            userManager.UpdateLog(sendRewardViewCount: true);
        }
    }

    ///<Summary>리워드 광고 시청 완료시 관련 정보를 보낸다.</Summary>
    public static void SendAnalytics_Video_Ads_Reward(int rewardNum)
    {
        Debug.Log(string.Format(logFormat1, CodeManager.GetMethodName(), rewardNum));

        string eventName = "Video_Ads_Reward";
        var logParams = CreateParams(eventName);
        logParams["option11"] = rewardNum.ToString();

        SendEvent(eventName, logParams);
    }

    ///<Summary>인앱 결제시 관련 정보를 보낸다.</Summary>
    public static void SendAnalytics_IAP_Purchase(UnityEngine.Purchasing.Product product)
    {
        Debug.Log(string.Format(logFormat2, CodeManager.GetMethodName(), product.definition.id, product.metadata.localizedPriceString));

        string eventName = "IAP_Purchase";

        var logParams = CreateParams(eventName);
        logParams["option11"] = product.definition.id;
        logParams["option12"] = product.metadata.localizedPrice.ToString();
        logParams[AFInAppEvents.CONTENT_ID] = product.definition.id;
        logParams[AFInAppEvents.REVENUE] = product.metadata.localizedPrice.ToString();
		logParams[AFInAppEvents.CURRENCY] = product.metadata.isoCurrencyCode;
		logParams[AFInAppEvents.QUANTITY] = "1";

        SendEvent(eventName, logParams);
    }

    ///<Summary>누적 횟수 달성 정보를 보낸다.</Summary>
    private static void SendAnalytics_Accumulated(string eventName)
    {
        Debug.Log(string.Format(logFormat1, CodeManager.GetMethodName(), eventName));
        
        var logParams = CreateParams(eventName);

        SendEvent(eventName, logParams);
    }

#endregion

    private static void SendEvent(string eventName, Dictionary<string, string> log)
    {
        if(showLogParams)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(eventName);

            foreach(var item in log)
                sb.Append(string.Format("\n[{0}] {1}", item.Key, item.Value));
            
            Debug.Log(CodeManager.GetMethodName() + sb.ToString());
        }

        SendEvent_AppsFlyer(eventName, log);
        SendEvent_Firebase(eventName, log);
    }

    private static Dictionary<string, string> CreateParams(string eventName)
    {
        var logParams = new Dictionary<string, string>();
        logParams["event_name"] = eventName;
        logParams["install_date"] = installDate;
        logParams["log_date"] = GetCurrentTime();
        logParams["option1"] = userGroup;
        logParams["option2"] = globalData.CURRENT_SCENE;
        logParams["option3"] = globalData.CURRENT_LEVEL.ToString();
        logParams["option4"] = userManager.Current.Coin.ToString();
        logParams["option5"] = userManager.Current.Life.ToString();
        logParams["option6"] = userManager.Current.Puzzle.ToString();
        logParams["option7"] = userManager.Current.GoldPiece.ToString();
        logParams["option8"] = userManager.Current.OwnSkillItems[SkillItemType.Undo].ToString();
        logParams["option9"] = userManager.Current.OwnSkillItems[SkillItemType.Stash].ToString();
        logParams["option10"] = userManager.Current.OwnSkillItems[SkillItemType.Shuffle].ToString();

        return logParams;
    }
}
