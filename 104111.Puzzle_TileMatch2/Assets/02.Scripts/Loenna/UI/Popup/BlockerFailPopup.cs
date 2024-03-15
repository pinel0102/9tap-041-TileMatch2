using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using DG.Tweening;
using NineTap.Constant;
using NineTap.Common;

public record BlockerFailPopupParameter
(
	CurrentPlayState.Finished.State State,
	UITextButtonParameter ContinueButtonParameter,
	Action OnQuit,
    BlockerType BlockerType
) : DefaultParameterWithoutHUD();

[ResourcePath("UI/Popup/BlockerFailPopup")]
public class BlockerFailPopup : UIPopup
{
    [SerializeField]	private UITextButton m_continueButton;
	[SerializeField]	private UITextButton m_quitButton;
    [SerializeField]	private UIImageButton m_exitButton;
	[SerializeField]	private BlockerType m_blockerType;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not BlockerFailPopupParameter parameter)
		{
			return;
		}

        CurrentPlayState.Finished.State state = parameter.State;
        m_blockerType = parameter.BlockerType;
		
		//m_continueButton.OnSetup(parameter.ContinueButtonParameter);
        m_continueButton.OnSetup(
            new UITextButtonParameter {
                OnClick = () => {
                    SDKManager.SendAnalytics_C_Scene_Fail("Retry");
                    OnClickClose();
                    parameter.ContinueButtonParameter.OnClick?.Invoke();
                },
                ButtonText = parameter.ContinueButtonParameter.ButtonText,
                SubWidgetBuilder = parameter.ContinueButtonParameter.SubWidgetBuilder
            }
        );

        // [Give Up]
		m_quitButton.OnSetup(
			new UITextButtonParameter {
				OnClick = () => {
                    SDKManager.SendAnalytics_C_Scene_Fail("Close");
					OnClickClose();
					parameter.OnQuit?.Invoke();
				},
				ButtonText = Text.Button.GIVE_UP,
				SubWidgetBuilder = () => {
					var widget = Instantiate(ResourcePathAttribute.GetResource<IconWidget>());
					widget.OnSetup("UI_Icon_Heart_Broken");
					return widget.CachedGameObject;
				}
			}
		);

        // [x] == [Give Up]
        m_exitButton.OnSetup(
			new UITextButtonParameter {
				OnClick = () => {
                    SDKManager.SendAnalytics_C_Scene_Fail("Close");
					OnClickClose();
                    parameter.OnQuit?.Invoke();
				}
			}
		);
	}
}
