#define ENABLE_IRONSOURCE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#pragma warning disable 162
public partial class SDKManager
{
    [Header("★ [Settings] IronSource")]
    public bool m_isAdFreeUser;

    /// <Summary>최초 설치시 광고 비표시 기간 (전면) (클리어 레벨).</Summary>
    public int firstAdsFreeLevel = 20;
    /// <Summary>최초 설치시 광고 비표시 기간 (배너, 전면) (초).</Summary>
    public int firstAdsFreeDelay = 180;
    /// <Summary>전면 광고 주기 (초).</Summary>
    public int interstitialDelay = 120;
    /// <Summary>리워드 광고 보상 인덱스.</Summary>
    public int rewardNum;

    /// <Summary>현재 타임 카운트 (초).</Summary>///
    private static int currentTimeCount;
    private static bool isBannerOn;
    private static bool openRemoveAdsPopup;
    private static string ironSource_appKey;
    private static bool isInitialized_IronSource = false;    
    private WaitForSecondsRealtime wUpdateDelay = new WaitForSecondsRealtime(4f);
    private WaitForSecondsRealtime wOneSecond = new WaitForSecondsRealtime(1f);


#region Initialize

    private void Initialize_IronSource(string appKey_AOS, string appKey_iOS, bool isADFreeUser = false)
    {
        isInitialized_IronSource = false;
        SetAdFreeUser(isADFreeUser);

#if ENABLE_IRONSOURCE

#if UNITY_STANDALONE
        return;
#endif

#if UNITY_ANDROID
        ironSource_appKey = appKey_AOS;
#elif UNITY_IOS
        ironSource_appKey = appKey_iOS;
#endif
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#EC46EB>IronSource [{0}] {1}</color>", IronSource.pluginVersion(), ironSource_appKey));

        currentTimeCount = 0;

        IronSourceConfig.Instance.setClientSideCallbacks(true);
        IronSource.Agent.validateIntegration();
        
        AddEvents();
#endif
    }

    private void Start_IronSource()
    {
#if UNITY_STANDALONE
        return;
#endif
        StartCoroutine(CO_TimeCount());
        StartCoroutine(CO_Start());
    }

    private IEnumerator CO_TimeCount()
    {
        while (true)
        {
            yield return wOneSecond;
            currentTimeCount++;
        }
    }
    
    private IEnumerator CO_Start()
    {
        yield return wUpdateDelay;

#if ENABLE_IRONSOURCE

        IronSource.Agent.init(ironSource_appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
        isInitialized_IronSource = true;
        
        while (true)
        {
            if (!IronSource.Agent.isInterstitialReady())
            {
                LoadInterstitial();
            }

            if (!isBannerOn)
            {
                ShowBanner();
            }

            yield return wUpdateDelay;
        }
#endif
    }

    private void OnApplicationPause(bool isPaused)
    {
#if ENABLE_IRONSOURCE

        if (!isInitialized_IronSource) 
            return;
        
        Debug.Log(CodeManager.GetMethodName() + "OnApplicationPause = " + isPaused);

        IronSource.Agent.onApplicationPause(isPaused);        
        
        ShowBanner();
#endif
    }

    public void SetAdFreeUser(bool isAdFreeUser)
    {
        m_isAdFreeUser = isAdFreeUser;

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#EC46EB>isAdFreeUser : {0}</color>", m_isAdFreeUser));
    }

#endregion Initialize


#region Handle AD

    private void LoadInterstitial()
    {
#if ENABLE_IRONSOURCE

		if (appOpenCount <= 1) 
        {
			var stStartTime = ToDateTime(installDate);
			var stDeltaTime = DateTime.Now.Subtract(stStartTime);
			
			// 최초 설치 딜레이 적용.
			if(stDeltaTime.TotalSeconds >= firstAdsFreeDelay) 
            {
				IronSource.Agent.loadInterstitial();	
			}
		}
        else
        {
			IronSource.Agent.loadInterstitial();
		}
#endif
    }

    public void ShowBanner()
    {
#if ENABLE_IRONSOURCE

        if (m_isAdFreeUser) return;

		if (appOpenCount <= 1)
        {
			var stStartTime = ToDateTime(installDate);
			var stDeltaTime = DateTime.Now.Subtract(stStartTime);

			// 최초 설치 딜레이 적용.
			if(stDeltaTime.TotalSeconds >= firstAdsFreeDelay) 
            {
				IronSource.Agent.loadBanner(IronSourceBannerSize.SMART, IronSourceBannerPosition.BOTTOM);
			}
		}
        else
        {
			IronSource.Agent.loadBanner(IronSourceBannerSize.SMART, IronSourceBannerPosition.BOTTOM);
		}
#endif
    }

    public void HideBanner()
    {
#if ENABLE_IRONSOURCE
        IronSource.Agent.hideBanner();
#endif
    }

    public void ShowInterstitial(bool _openRemoveAdsPopup)
    {
#if ENABLE_IRONSOURCE

        if (m_isAdFreeUser) return;

        if (globalData.CURRENT_LEVEL >= firstAdsFreeLevel)
        {
            if(currentTimeCount > interstitialDelay)
            {
                openRemoveAdsPopup = _openRemoveAdsPopup;

                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>openRemoveAdsPopup : {0}</color>", openRemoveAdsPopup));

#if UNITY_EDITOR
                Interstitial_OnAdClosedEvent(null);
                currentTimeCount = 0;
#else
                if (IronSource.Agent.isInterstitialReady())
                {
                    IronSource.Agent.showInterstitial();
                    currentTimeCount = 0;

                    SendAnalytics_Interstitial_Ads_Show();
                }
                else
                {
                    LoadInterstitial();
                }
#endif
            }
        }

#endif
    }

#endregion Handle AD



#region Reward AD

    public void ShowRewardVideo(int num)
    {
#if ENABLE_IRONSOURCE
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            rewardNum = num;
            IronSource.Agent.showRewardedVideo();

            SendAnalytics_Video_Ads_Show();        
        }

#if UNITY_EDITOR
        rewardNum = num;
        GetReward();
#endif
#endif
    }

    public void GetReward()
    {
        Debug.Log(CodeManager.GetMethodName() + "Get Reward : " + rewardNum);

        GlobalDefine.GetReward_FromVideo(rewardNum);
        SendAnalytics_Video_Ads_Reward(rewardNum);
    }

#endregion Reward AD


#region IronSource Callbacks

    private void AddEvents()
    {
#if ENABLE_IRONSOURCE

        //Add Banner Events
        IronSourceBannerEvents.onAdLoadedEvent += Banner_OnAdLoadedEvent;
        IronSourceBannerEvents.onAdLoadFailedEvent += Banner_OnAdLoadFailedEvent;
        IronSourceBannerEvents.onAdClickedEvent += Banner_OnAdClickedEvent;
        IronSourceBannerEvents.onAdScreenPresentedEvent += Banner_OnAdScreenPresentedEvent;
        IronSourceBannerEvents.onAdScreenDismissedEvent += Banner_OnAdScreenDismissedEvent;
        IronSourceBannerEvents.onAdLeftApplicationEvent += Banner_OnAdLeftApplicationEvent;
        
        // Add Interstitial Events
        IronSourceInterstitialEvents.onAdReadyEvent += Interstitial_OnAdReadyEvent;
        IronSourceInterstitialEvents.onAdLoadFailedEvent += Interstitial_OnAdLoadFailedEvent;
        IronSourceInterstitialEvents.onAdClickedEvent += Interstitial_OnAdClickedEvent;
        IronSourceInterstitialEvents.onAdOpenedEvent += Interstitial_OnAdOpenedEvent;
        IronSourceInterstitialEvents.onAdClosedEvent += Interstitial_OnAdClosedEvent;
        IronSourceInterstitialEvents.onAdShowSucceededEvent += Interstitial_OnAdShowSucceededEvent;
        IronSourceInterstitialEvents.onAdShowFailedEvent += Interstitial_OnAdShowFailedEvent;
        
        //Add Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideo_OnAdAvailableEvent;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideo_OnAdUnavailableEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideo_OnAdClickedEvent;
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideo_OnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideo_OnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideo_OnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideo_OnAdRewardedEvent;

#endif
    }

    /************* Banner Delegates *************/
    private void Banner_OnAdLoadedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());

        isBannerOn = true;
    }

    private void Banner_OnAdLoadFailedEvent(IronSourceError error)
    {
        if (error.getCode() != 606) // No ads to show
            Debug.Log(CodeManager.GetMethodName() + string.Format("code: {0} / description : {1}", error.getCode(), error.getDescription()));
        
        isBannerOn = false;
    }

    private void Banner_OnAdClickedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void Banner_OnAdScreenPresentedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void Banner_OnAdScreenDismissedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void Banner_OnAdLeftApplicationEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    /************* Interstitial Delegates *************/
    private void Interstitial_OnAdReadyEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void Interstitial_OnAdLoadFailedEvent(IronSourceError error)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("code: {0} / description : {1}", error.getCode(), error.getDescription()));
    }

    private void Interstitial_OnAdClickedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void Interstitial_OnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void Interstitial_OnAdClosedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());

        LoadInterstitial();

        if (openRemoveAdsPopup)
            globalData.ShowRemoveAdsPopup();
    }

    private void Interstitial_OnAdShowSucceededEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void Interstitial_OnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("code: {0} / description : {1}", error.getCode(), error.getDescription()));

        LoadInterstitial();
    }

    /************* RewardedVideo Delegates *************/
    private void RewardedVideo_OnAdAvailableEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void RewardedVideo_OnAdUnavailableEvent()
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void RewardedVideo_OnAdClickedEvent(IronSourcePlacement ssp, IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("name : {0}", ssp.getRewardName()));
    }

    private void RewardedVideo_OnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void RewardedVideo_OnAdClosedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName());
    }

    private void RewardedVideo_OnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("code: {0} / description : {1}", error.getCode(), error.getDescription()));
    }

    private void RewardedVideo_OnAdRewardedEvent(IronSourcePlacement ssp, IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("amount : {0} / name : {1}", ssp.getRewardAmount(), ssp.getRewardName()));

        GetReward();
    }

#endregion IronSource Callbacks
}
