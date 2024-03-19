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

public record PlayEndPopupParameter
(
	CurrentPlayState.Finished.State State,
	UITextButtonParameter ContinueButtonParameter,
	Action OnQuit,
    Action OnOpened
) : DefaultParameterWithoutHUD();

[ResourcePath("UI/Popup/PlayEndPopup")]
public class PlayEndPopup : UIPopup
{
	[SerializeField]
	private EventTrigger m_eventTrigger;

	[SerializeField]
	private CanvasGroup m_viewCanvasGroup;

	[SerializeField]
	private CanvasGroup m_labelCanvasGroup;

	[SerializeField]
	private TMP_Text m_labelText;

	[SerializeField]
	private DynamicImage m_label;

	[SerializeField]
	private TMP_Text m_message;

	[SerializeField]
	private List<CanvasGroup> m_canvasGroups;

	[SerializeField]
	private UITextButton m_continueButton;

	[SerializeField]
	private UITextButton m_quitButton;

    [SerializeField]
	private UIImageButton m_exitButton;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not PlayEndPopupParameter parameter)
		{
			return;
		}

        CurrentPlayState.Finished.State state = parameter.State;
		
		m_labelCanvasGroup.alpha = 0f;

		(string label, string param) = (Text.GameEndLabelText(false), "_Fail");

		m_labelText.text = label;
		m_label.ChangeSprite(param);
		m_message.text = Text.PLAY_END_POPUP_MESSAGE;

		m_canvasGroups.ForEach(
			canvasGroup => canvasGroup.alpha = 1f
		);

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

		UniTask.Void(
			async token => {
                await m_labelCanvasGroup.DOFade(1f, 0.25f);

                List<EventTrigger.Entry> entries = new List<EventTrigger.Entry> {
                    new EventTrigger.Entry { eventID = EventTriggerType.PointerDown },
                    new EventTrigger.Entry { eventID = EventTriggerType.PointerUp }
                };

                foreach (var entry in entries)
                {
                    entry.callback
                    .OnInvokeAsAsyncEnumerable(this.GetCancellationTokenOnDestroy())
                    .SubscribeAwait(_ => OnTriggerCallback(entry.eventID));
                }

                m_eventTrigger.triggers.AddRange(entries);

                parameter.OnOpened?.Invoke();
			},
			this.GetCancellationTokenOnDestroy()
		);

		UniTask OnTriggerCallback(EventTriggerType type)
		{
			return m_viewCanvasGroup
				.DOFade(type is EventTriggerType.PointerDown? 0f : 1f, 0.25f)
				.SetEase(Ease.OutQuad)
				.ToUniTask(TweenCancelBehaviour.KillWithCompleteCallbackAndCancelAwait, this.GetCancellationTokenOnDestroy());
		}
	}
}
