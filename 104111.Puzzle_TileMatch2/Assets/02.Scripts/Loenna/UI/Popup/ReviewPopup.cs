using UnityEngine;

using NineTap.Common;

public record ReviewPopupParameter
(
	string Title,
	string Message,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
	UITextButtonParameter LeftButtonParameter,
	params HUDType[] HUDTypes
) : PopupBaseParameter(Title, Message, ExitParameter, BaseButtonParameter, true, HUDTypes);

[ResourcePath("UI/Popup/ReviewPopup")]
public class ReviewPopup : PopupBase
{
	[SerializeField]
	private UITextButton m_otherButton;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not ReviewPopupParameter parameter)
		{
			OnClickClose();
			return;
		}

		OnSetupButton(m_otherButton, parameter.LeftButtonParameter, parameter.AllPressToClose);	
	}
}