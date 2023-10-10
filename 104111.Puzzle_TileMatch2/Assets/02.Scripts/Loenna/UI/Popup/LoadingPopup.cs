using UnityEngine;

using NineTap.Common;

[ResourcePath("UI/Popup/LoadingPopup")]
public class LoadingPopup : UIPopup
{
	public enum Options
	{
		Default,
		OnlyIcon,
		Invisible
	}

	[SerializeField]
	private GameObject m_content;

	public override void OnSetup(UIParameter parameter)
	{
		CanvasGroup.alpha = 0f;
		base.OnSetup(parameter);
	}

	public void SetOption(Options option)
	{
		Background.gameObject.SetActive(option is Options.Default);
		m_content.SetActive(option is not Options.Invisible);
	}

	public override void Show()
	{
		CanvasGroup.alpha = 1f;
		base.Show();
	}
}
