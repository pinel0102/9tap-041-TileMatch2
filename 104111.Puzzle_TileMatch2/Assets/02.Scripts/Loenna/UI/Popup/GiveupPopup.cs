using NineTap.Common;

public record GiveupPopupParameter(
	string Title,
	string Message,
    bool ignoreBackKey,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
	params HUDType[] HUDTypes
) : PopupBaseParameter(Title, Message, ExitParameter, BaseButtonParameter, true, HUDTypes);

[ResourcePath("UI/Popup/GiveupPopup")]
public class GiveupPopup : PopupBase
{
    public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not GiveupPopupParameter parameter)
		{
			OnClickClose();
            return;
		}

        ignoreBackKey = parameter.ignoreBackKey;
    }
}
