using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineTap.Common;
using UnityEngine.UI;

public record AgreeGDPRPopupParameter(
    string Region
) : UIParameter(HUDType.NONE);

[ResourcePath("UI/Popup/AgreeGDPRPopup")]
public class AgreeGDPRPopup : UIPopup
{
    [Header("★ [Reference] GDPR")]
    public GameObject gdpr_ko;
    public GameObject gdpr_en;
    public string region;

    [Header("★ [Reference] GDPR_KO")]
    public Toggle toggleService;
    public Toggle togglePrivacy;
    public Scrollbar scrollbarService;
    public Scrollbar scrollbarPrivacy;
    public bool isKRService;
    public bool isKRPrivacy;

    [Header("★ [Reference] GDPR_EN")]
    public Button buttonService;
    public Button buttonPrivacy;
    public Button buttonNext;

    private UserManager userManager;

    public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

        if (uiParameter is not AgreeGDPRPopupParameter parameter)
        {
            return;
        }

        userManager = GlobalData.Instance.userManager;
        region = parameter.Region;

        toggleService.onValueChanged.RemoveAllListeners();
        togglePrivacy.onValueChanged.RemoveAllListeners();
        buttonService.onClick.RemoveAllListeners();
        buttonPrivacy.onClick.RemoveAllListeners();
        buttonNext.onClick.RemoveAllListeners();

        toggleService.onValueChanged.AddListener(CheckGDPR_Service);
        togglePrivacy.onValueChanged.AddListener(CheckGDPR_Privacy);
        buttonService.onClick.AddListener(OpenService);
        buttonPrivacy.onClick.AddListener(OpenPrivacy);
        buttonNext.onClick.AddListener(CheckGDPR_EN);

        if (region == "KR")
        {
            gdpr_ko.SetActive(true);
            gdpr_en.SetActive(false);
        }
        else
        {
            gdpr_ko.SetActive(false);
            gdpr_en.SetActive(true);
        }

        scrollbarService.value = 1f;
        scrollbarPrivacy.value = 1f;
	}

#region KR

    private void CheckGDPR()
    {
        if(isKRService && isKRPrivacy)
        {
            NextStep();
        }
    }

    private void CheckGDPR_Service(bool isChecked)
    {
        isKRService = isChecked;
        CheckGDPR();
    }

    private void CheckGDPR_Privacy(bool isChecked)
    {
        isKRPrivacy = isChecked;
        CheckGDPR();
    }

#endregion KR

#region EN
    
    private void OpenService()
    {
        GlobalSettings.OpenURL_TermsOfService();
    }

    private void OpenPrivacy()
    {
        GlobalSettings.OpenURL_PrivacyPolicy();
    }

    private void CheckGDPR_EN()
    {
        NextStep();
    }

#endregion EN

    private void NextStep()
    {
        userManager.UpdateAgree(
            agreeService:true, 
            agreePrivacy:true
        );

        OnClickClose();
    }
}
