using UnityEngine;

using System.Linq;
using System.Collections.Generic;

using TMPro;

using NineTap.Common;

public class MainSceneFragmentContentParameter_Settings
: ScrollViewFragmentContentParameter
{
	public string TitleText;
	public UIImageButtonParameter CloseButtonParameter;
	public List<UIToggleButtonParameter> ToggleButtonParameters;
	public UITextButtonParameter ServiceButtonParameter;
	public UITextButtonParameter PrivacyButtonParameter;
	public UITextButtonParameter ContactButtonParameter;
}

[ResourcePath("UI/Fragments/Fragment_Settings")]
public class MainSceneFragmentContent_Settings : ScrollViewFragmentContent
{
	[SerializeField]
	private TMP_Text m_title;
	
	[SerializeField]
	private UIImageButton m_closeButton;

	[SerializeField]
	private List<UIToggleButton> m_toggleButtons;

	[SerializeField]
	private UITextButton m_achievementButton;

	[SerializeField]
	private UITextButton m_privacyButton;

	[SerializeField]
	private UITextButton m_contactButton;

	[SerializeField]
	private TMP_Text m_versionText;

	public override void OnSetup(ScrollViewFragmentContentParameter contentParameter)
	{
		if (contentParameter is MainSceneFragmentContentParameter_Settings parameter)
		{
			m_title.text = parameter.TitleText;
			m_closeButton.OnSetup(parameter.CloseButtonParameter);

			m_toggleButtons = m_toggleButtons
			.Zip(
				parameter.ToggleButtonParameters, 
				(toggle, param) => {
					toggle.OnSetup(param);
					return toggle;
				}
			).ToList();

			m_achievementButton.OnSetup(parameter.ServiceButtonParameter);
			m_privacyButton.OnSetup(parameter.PrivacyButtonParameter);
			m_contactButton.OnSetup(parameter.ContactButtonParameter);

			m_versionText.text = $"Ver.{Application.version}";
		}
	}
}
