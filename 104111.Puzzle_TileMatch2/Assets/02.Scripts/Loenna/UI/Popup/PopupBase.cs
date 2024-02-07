using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;

using TMPro;

using NineTap.Common;

public class ExitBaseParameter
{
    ///<Summary>배경을 누를때 팝업 닫히기 여부.</Summary>
	public readonly bool IncludeBackground;
	public readonly Action OnExit;

    ///<param name="includeBackground">배경을 누를때 팝업 닫히기 여부.</param>
	public ExitBaseParameter(Action onExit, bool includeBackground = true)
	{
		OnExit = onExit;
		IncludeBackground = includeBackground;
	}

	public static ExitBaseParameter CancelParam = new ExitBaseParameter(null);
}

///<param name="AllPressToClose">기본 버튼을 누를때 팝업 닫히기 여부.</param>
public abstract record PopupBaseParameter
(
	string Title,
	string Message,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
    bool AllPressToClose,
	params HUDType[] HUDTypes
) : UIParameter(HUDTypes);

public sealed record DefaultPopupParameter(
	string Title,
	string Message,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
	params HUDType[] HUDTypes
) : PopupBaseParameter(Title, Message, ExitParameter, BaseButtonParameter, true, HUDTypes);

public class PopupBase : UIPopup
{
	[SerializeField]
	protected UIImageButton m_cancelButton;

	[SerializeField]
	protected TMP_Text m_title;

	[SerializeField]
	protected TMP_Text m_message;

	[SerializeField]
	protected UITextButton m_button;

	protected void Reset()
	{
		transform.TryGetComponentAtPath("View/UI_SafeArea/Layout/Main/Layout_Vertical/Panel/CancelButton", out m_cancelButton);
		transform.TryGetComponentAtPath("View/UI_SafeArea/Layout/Main/Layout_Vertical/Panel/Title/Text", out m_title);
		transform.TryGetComponentAtPath("View/UI_SafeArea/Layout/Main/Layout_Vertical/Panel/Container/Message", out m_message);
		transform.TryGetComponentAtPath("View/UI_SafeArea/Layout/Main/Layout_Vertical/Panel/Container/ButtonLayout/Button_Confirm", out m_button);
	}

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not PopupBaseParameter parameter)
		{
			Debug.LogError(new InvalidCastException(nameof(PopupBaseParameter)));
			OnClickClose();
			return;
		}

		m_title.text = parameter.Title;
		m_message.text = parameter.Message;

		if (parameter.ExitParameter is not null and var exitParameter)
		{
			UnityAction onExit = () => {
				OnClickClose();
				exitParameter.OnExit?.Invoke();
			};

			m_cancelButton.OnSetup(
				new UIImageButtonParameter {
					OnClick = () => onExit?.Invoke()
				}
			);
			
			if (exitParameter.IncludeBackground)
			{
				Button button = Background.gameObject.GetComponentInChildren<Button>(true);
				if (button != null)
				{
					button.onClick.AddListener(onExit);
				}
			}
		}

        if (parameter.BaseButtonParameter is not null and var buttonParameter)
		    OnSetupButton(m_button, buttonParameter, parameter.AllPressToClose);	
	}

	protected void OnSetupButton(UIButton button, UIButtonParameter parameter, bool allPressToClose)
	{
		var buttonParam = parameter;
		
		if (allPressToClose)
		{
			var onClick = buttonParam.OnClick;
			buttonParam.OnClick = () => {
				OnClickClose();
				onClick?.Invoke();
			};
		}

		button.OnSetup(buttonParam);
	}

}
