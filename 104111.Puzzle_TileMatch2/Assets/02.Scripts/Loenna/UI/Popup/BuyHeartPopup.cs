using UnityEngine;
using NineTap.Common;
using Cysharp.Threading.Tasks;
using TMPro;

public record BuyHeartPopupParameter
(
	string Title,
	string Message,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
    IUniTaskAsyncEnumerable<(bool, string, string, bool)> LifeStatus
) : PopupBaseParameter(Title, Message, ExitParameter, BaseButtonParameter, false, HUDType.ALL);

[ResourcePath("UI/Popup/BuyHeartPopup")]
public class BuyHeartPopup : PopupBase
{
    [SerializeField] private TMP_Text m_text;
    [SerializeField] private TMP_Text m_timeText;

    private GlobalData globalData { get { return GlobalData.Instance; } }

    public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not BuyHeartPopupParameter parameter)
		{
            OnClickClose();
            return;
		}

        m_text.SetText(globalData.HUD.behaviour.Fields[1].Text.text);
        m_timeText.SetText(globalData.HUD.behaviour.Fields[1].TimeText.text);

        parameter.LifeStatus.BindTo(m_text, (component, status) => {
                component.text = status.Item2;
            });
        parameter.LifeStatus.BindTo(m_timeText, (component, status) => {
                component.text = status.Item3;
            });
	}

	public override void OnClickClose()
	{
        UIManager.HUD.Show(HUDType.ALL);
		base.OnClickClose();
	}
}
