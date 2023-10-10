using UnityEngine;

using NineTap.Common;

public record SimplePopupParameter
(
	string Title,
	string Message,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
	UITextButtonParameter LeftButtonParameter,
	params HUDType[] HUDTypes
) : PopupBaseParameter(Title, Message, ExitParameter, BaseButtonParameter, true, HUDTypes);

[ResourcePath("UI/Popup/SimplePopup")]
public class SimplePopup : PopupBase
{
	[SerializeField]
	private UITextButton m_otherButton;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not SimplePopupParameter parameter)
		{
			OnClickClose();
			return;
		}

		OnSetupButton(m_otherButton, parameter.LeftButtonParameter, parameter.AllPressToClose);	
	}
}
