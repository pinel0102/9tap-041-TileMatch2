using UnityEngine;

using NineTap.Common;
using TMPro;

public record ReviewPopupParameter
(
	string Title,
	string Message,
    string Message2,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
	UITextButtonParameter LeftButtonParameter,
    UITextButtonParameter CloseButtonParameter,
	params HUDType[] HUDTypes
) : PopupBaseParameter(Title, Message, ExitParameter, BaseButtonParameter, false, HUDTypes);

[ResourcePath("UI/Popup/ReviewPopup")]
public class ReviewPopup : PopupBase
{
	[SerializeField]
	private UITextButton m_otherButton;

    [SerializeField]
	private UITextButton m_closeButton;

    [SerializeField]
	private GameObject m_container1;

    [SerializeField]
	private GameObject m_container2;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not ReviewPopupParameter parameter)
		{
			OnClickClose();
            return;
		}

        OnSetupButton(m_otherButton, parameter.LeftButtonParameter, parameter.AllPressToClose);
        OnSetupButton(m_closeButton, parameter.CloseButtonParameter, parameter.AllPressToClose);
        m_button.onClick.AddListener(OpenReviewPage);
        m_container1.SetActive(true);
        m_container2.SetActive(false);
	}

    private void OpenReviewPage()
    {
        Debug.Log(CodeManager.GetMethodName());

        GlobalSettings.OpenURL_Review();

        m_container1.SetActive(false);
        m_container2.SetActive(true);
    }
}