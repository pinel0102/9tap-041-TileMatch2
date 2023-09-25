using UnityEngine;

using System.Linq;
using System.Collections.Generic;

using NineTap.Common;

public record PausePopupParameter
(
	string Title,
	List<UIToggleButtonParameter> ToggleButtonParameters,
	UITextButtonParameter TopButtonParameter,
	UITextButtonParameter BottomButtonParameter
) : PopupBaseParameter(Title, string.Empty, ExitBaseParameter.CancelParam, TopButtonParameter, true);

[ResourcePath("UI/Popup/PausePopup")]
public class PausePopup : PopupBase
{
	[SerializeField]
	private List<UIToggleButton> m_toggleButtons;

	[SerializeField]
	private UITextButton m_bottomButton;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not PausePopupParameter parameter)
		{
			return;
		}

		m_toggleButtons = m_toggleButtons
		.Zip(
			parameter.ToggleButtonParameters, 
			(toggle, param) => {
				toggle.OnSetup(param);
				return toggle;
			}
		).ToList();

		OnSetupButton(
			m_bottomButton, 
			parameter.BottomButtonParameter, 
			parameter.AllPressToClose
		);
	}
}
