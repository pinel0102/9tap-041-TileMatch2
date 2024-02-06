#define ENABLE_IRONSOURCE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#pragma warning disable 162
public partial class SDKManager
{
    [Header("★ [Settings] IronSource")]
    public bool m_showLogIronSource = false;
    /// <Summary>최초 설치시 광고 비표시 기간 (전면) (클리어 레벨).</Summary>
    public int firstAdsFreeLevel = 20;
    /// <Summary>최초 설치시 광고 비표시 기간 (배너, 전면) (초).</Summary>
    public int firstAdsFreeDelay = 180;
    /// <Summary>전면 광고 주기 (초).</Summary>
    public int interstitialDelay = 120;
    /// <Summary>배너 광고 조건.</Summary>
    private static bool IsBannerAvailable => globalData.CURRENT_SCENE == GlobalDefine.SCENE_PLAY;
    
    [Header("★ [Live] IronSource")]
    [SerializeField] private bool m_isAdFreeUser;
    /// <Summary>현재 타임 카운트 (초).</Summary>///
    [SerializeField] private int currentTimeCount;
    /// <Summary>리워드 광고 보상 인덱스.</Summary>
    [SerializeField] private int rewardNum = -1;
    private Action<bool> rewardVideoCallback;
    public bool IsAdFreeUser => m_isAdFreeUser;
    private static bool m_isBannerLoaded;
    public static bool IsBannerLoaded => m_isBannerLoaded;
    private static bool m_isRewardVideoLoaded;
    public static bool IsRewardVideoLoaded => m_isRewardVideoLoaded;
    private static bool m_openRemoveAdsPopup;
    public static bool OpenRemoveAdsPopup => m_openRemoveAdsPopup;
    private static string ironSourceAppKey;
    private static bool isInitialized_IronSource = false;    
    private static WaitForSecondsRealtime wUpdateDelay = new WaitForSecondsRealtime(4f);
    private static WaitForSeconds wOneSecond = new WaitForSeconds(1f);

#region Initialize

    private void Initialize_IronSource(string appKey_AOS, string appKey_iOS, bool isADFreeUser = false)
    {
        isInitialized_IronSource = false;
        
#if ENABLE_IRONSOURCE

#if UNITY_STANDALONE
        return;
#endif

#if UNITY_ANDROID
        ironSourceAppKey = appKey_AOS;
#elif UNITY_IOS
        ironSourceAppKey = appKey_iOS;
#endif
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=#EC46EB>IronSource [{0}] {1}</color>", IronSource.pluginVersion(), ironSourceAppKey));
        
        SetAdFreeUser(isADFreeUser);
        SetRemoveAdsPopup(false);

        m_isBannerLoaded = false;
        m_isRewardVideoLoaded = false;
        currentTimeCount = 0;
        rewardNum = -1;

        IronSourceConfig.Instance.setClientSideCallbacks(true);
        IronSource.Agent.validateIntegration();
        
        AddEvents();

#if UNITY_EDITOR
        m_isBannerLoaded = true;
        m_isRewardVideoLoaded = true;
#endif

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

        IronSource.Agent.init(ironSourceAppKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
        isInitialized_IronSource = true;
        
        while (true)
        {
            if (!IronSource.Agent.isInterstitialReady())
                LoadInterstitial();
            
            if (!m_isBannerLoaded)
                ShowBanner(true);
            
            if (!IsBannerAvailable)
                HideBanner();
            
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
        
        ShowBanner(true);
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

    public void ShowBanner(bool forceLoad = false)
    {
#if ENABLE_IRONSOURCE

        if (m_isAdFreeUser) return;
        if (!IsBannerAvailable) return;

		if (appOpenCount <= 1)
        {
			var stStartTime = ToDateTime(installDate);
			var stDeltaTime = DateTime.Now.Subtract(stStartTime);

			// 최초 설치 딜레이 적용.
			if(stDeltaTime.TotalSeconds >= firstAdsFreeDelay) 
            {
                LoadBanner();
			}
		}
        else
        {
            LoadBanner();
		}

        void LoadBanner()
        {
            if (m_showLogIronSource)
                Debug.Log(CodeManager.GetMethodName() + m_isBannerLoaded);

			if (m_isBannerLoaded && !forceLoad)
                IronSource.Agent.displayBanner();
            else
                IronSource.Agent.loadBanner(IronSourceBannerSize.SMART, IronSourceBannerPosition.BOTTOM);
        }
#endif
    }

    public void HideBanner()
    {
#if ENABLE_IRONSOURCE
        if (m_isBannerLoaded)
        {
            if (m_showLogIronSource)
                Debug.Log(CodeManager.GetMethodName());

            IronSource.Agent.hideBanner();
        }
#endif
    }

    public void ShowInterstitial(bool openRemoveAdsPopup)
    {
#if ENABLE_IRONSOURCE

        if (m_isAdFreeUser) return;

        SetRemoveAdsPopup(false);

        if (globalData.CURRENT_LEVEL > firstAdsFreeLevel)
        {
            if(currentTimeCount > interstitialDelay)
            {
#if UNITY_EDITOR
                SetRemoveAdsPopup(openRemoveAdsPopup);
                Interstitial_OnAdClosedEvent(null);
                currentTimeCount = 0;
#endif
                if (IronSource.Agent.isInterstitialReady())
                {
                    SetRemoveAdsPopup(openRemoveAdsPopup);
                    IronSource.Agent.showInterstitial();
                    currentTimeCount = 0;

                    SendAnalytics_Interstitial_Ads_Show();
                }
                else
                {
                    LoadInterstitial();
                }
            }
        }
#endif
    }

    public static void SetRemoveAdsPopup(bool open)
    {
        m_openRemoveAdsPopup = open;
    }

#endregion Handle AD



#region Reward AD

    public void ShowRewardVideo(int num, Action<bool> onRewardVideoComplete)
    {
#if ENABLE_IRONSOURCE
        rewardNum = num;
        rewardVideoCallback = onRewardVideoComplete;

        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            if (m_showLogIronSource)
                Debug.Log(CodeManager.GetMethodName());

            IronSource.Agent.showRewardedVideo();

            SendAnalytics_Video_Ads_Show();
        }

#if UNITY_EDITOR
        GetReward();
#endif
#endif
    }

    public void GetReward()
    {
        Debug.Log(CodeManager.GetMethodName() + "Get Reward : " + rewardNum);

        rewardVideoCallback?.Invoke(true);
        rewardVideoCallback = null;
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

        m_isBannerLoaded = true;

        if (!IsBannerAvailable)
        {
            HideBanner();
        }
    }

    private void Banner_OnAdLoadFailedEvent(IronSourceError error)
    {
        if (error.getCode() != 606) // No ads to show
            Debug.Log(CodeManager.GetMethodName() + string.Format("code: {0} / description : {1}", error.getCode(), error.getDescription()));
        
        //isBannerLoaded = false;
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

        m_isRewardVideoLoaded = true;
    }

    private void RewardedVideo_OnAdUnavailableEvent()
    {
        Debug.Log(CodeManager.GetMethodName());

        //isRewardVideoLoaded = false;
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

        rewardVideoCallback?.Invoke(false);
        rewardVideoCallback = null;
    }

    private void RewardedVideo_OnAdRewardedEvent(IronSourcePlacement ssp, IronSourceAdInfo adInfo)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("amount : {0} / name : {1}", ssp.getRewardAmount(), ssp.getRewardName()));

        GetReward();
    }

#endregion IronSource Callbacks
}
