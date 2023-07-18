using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System;
using System.Threading;

using Cysharp.Threading.Tasks;
using State = InputController.State;

public class TileBrush : MonoBehaviour
{
	[SerializeField]
	private EventTrigger m_button;

	[SerializeField]
	private Image m_image;

	public RectTransform RectTransform => transform as RectTransform;

	private IAsyncReactiveProperty<InputController.State> m_clickBinder;

	public void OnSetup(int size, Action<InputController.State> onClick)
	{
		m_clickBinder = new AsyncReactiveProperty<InputController.State>(State.NONE).WithDispatcher();
		InputController input = InputController.Instance;

		CancellationToken token = this.GetCancellationTokenOnDestroy();

		RectTransform.SetSize(size);
		EventTrigger.Entry entry = new EventTrigger.Entry{
			eventID = EventTriggerType.PointerClick,
			callback = new EventTrigger.TriggerEvent()
		};

		entry.callback.AddListener(
			eventData => {
				onClick?.Invoke( 
					(m_clickBinder.Value, input.WasState) switch {
						(State.LEFT_BUTTON_PRESSED, State.LEFT_BUTTON_PRESSED or State.LEFT_BUTTON_RELEASED) => State.LEFT_BUTTON_PRESSED,
						(State.RIGHT_BUTTON_PRESSED, State.RIGHT_BUTTON_PRESSED or State.RIGHT_BUTTON_RELEASED) => State.RIGHT_BUTTON_PRESSED,
						_=> State.NONE
					}
				);
			}
		);
		m_button.triggers.Add(entry);
	}

	public void UpdateUI(bool interactable, bool drawable)
	{
		m_clickBinder.Update(binder => 
			binder = (interactable, drawable) switch { 
				(true, true) => State.LEFT_BUTTON_PRESSED,
				(true, false) => State.RIGHT_BUTTON_PRESSED,
				_ => State.NONE
			}
		);

		m_button.enabled = interactable;
		m_image.enabled = interactable;
		m_image.color = drawable switch {
			true => Color.green,
			_ => Color.red
		};
	}
}
