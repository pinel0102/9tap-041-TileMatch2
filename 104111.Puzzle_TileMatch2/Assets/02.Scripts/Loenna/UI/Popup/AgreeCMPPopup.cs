using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineTap.Common;
using UnityEngine.UI;

public record AgreeCMPPopupParameter(
    string Region
) : UIParameter(HUDType.NONE);

[ResourcePath("UI/Popup/AgreeCMPPopup")]
public class AgreeCMPPopup : UIPopup
{
    [Header("★ [Reference] CMP")]
    public string region;
    private UserManager userManager;

    public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

        if (uiParameter is not AgreeCMPPopupParameter parameter)
        {
            return;
        }

        userManager = GlobalData.Instance.userManager;
        region = parameter.Region;

        CheckCMP();
    }

    private void CheckCMP()
    {
#if UNITY_EDITOR
        NextStep(true);
#else
        string CMPString = PlayerPrefs.GetString("IABTCF_AddtlConsent");
        
        Debug.Log(CodeManager.GetMethodName() + string.Format("CMPString : {0}", CMPString));
        if (CMPString.Contains("2878"))
        {
            Debug.Log(CodeManager.GetMethodName() + "setConsent : True");
            //IronSource.Agent.setConsent(true);
            //NextStep(true);
        }
        else
        {
            Debug.Log(CodeManager.GetMethodName() + "setConsent : False");
            //IronSource.Agent.setConsent(false);
            //NextStep(false);
        }
#endif
    }


    private void NextStep(bool state)
    {
        userManager.UpdateAgree(
            agreeCMP:true,
            agreeCMPState:state
        );

        OnClickClose();
    }
}
