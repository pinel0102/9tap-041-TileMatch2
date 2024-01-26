using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineTap.Common;
using UnityEngine.UI;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public record AgreeATTPopupParameter(
    string Region
) : UIParameter(HUDType.NONE);

[ResourcePath("UI/Popup/AgreeATTPopup")]
public class AgreeATTPopup : UIPopup
{
    [Header("★ [Reference] ATT")]
    public GameObject att_ko;
    public GameObject att_en;
    public Button buttonKR;
    public Button buttonEN;
    public string region;
    private UserManager userManager;

    [Header("★ [Settings] ATT")]
    public bool editorTestMode = false;
    private bool IsEnableShowConsentView 
    {
        get
        {
#if UNITY_EDITOR
            return editorTestMode;
#elif !UNITY_IOS
            return false;
#else
            var oVer = new System.Version(UnityEngine.iOS.Device.systemVersion);
            var oMinVer = new System.Version(14, 0, 0);

            return oVer.CompareTo(oMinVer) >= 0;
#endif
        }
    }

    public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

        if (uiParameter is not AgreeATTPopupParameter parameter)
        {
            return;
        }

        userManager = GlobalData.Instance.userManager;
        region = parameter.Region;

        buttonKR.onClick.RemoveAllListeners();
        buttonEN.onClick.RemoveAllListeners();

        buttonKR.onClick.AddListener(OpenATT);
        buttonEN.onClick.AddListener(OpenATT);

        if (region == "KR")
        {
            att_ko.SetActive(true);
            att_en.SetActive(false);
        }
        else
        {
            att_ko.SetActive(false);
            att_en.SetActive(true);
        }
    }

    private void OpenATT()
    {
        if(IsEnableShowConsentView)
        {
            OnTouchConsentViewOKBtn();
        }	
        else
        {
            HandleShowConsentViewMsg("True");
        }
    }

    private void OnTouchConsentViewOKBtn() 
    {
#if UNITY_EDITOR || !UNITY_IOS
        HandleShowConsentViewMsg("True");
#else
        ATTrackingStatusBinding.RequestAuthorizationTracking();
        StartCoroutine(RequestAuthorizationTracking());
#endif
    }

#if UNITY_IOS
    private WaitForSeconds delay = new WaitForSeconds(0.15f);

    private IEnumerator RequestAuthorizationTracking() {
        do {
            yield return delay;
        } while(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED);

        HandleShowConsentViewMsg((ATTrackingStatusBinding.GetAuthorizationTrackingStatus() != ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED) ? "True" : "False");
    }
#endif

    private void HandleShowConsentViewMsg(string a_oMsg) 
    {
        if(bool.TryParse(a_oMsg, out bool bIsSuccess))
        {
            NextStep(bIsSuccess);
        }
        else
        {
            NextStep(true);
        }
    }

    private void NextStep(bool state)
    {
        userManager.UpdateAgree(
            agreeATT:true,
            agreeATTState:state
        );

        OnClickClose();
    }
}
