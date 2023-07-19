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

	public void OnSetup(int size, Action<InputController.State> onClick)
	{
		InputController input = InputController.Instance;

		CancellationToken token = this.GetCancellationTokenOnDestroy();

		RectTransform.SetSize(size);
		EventTrigger.Entry entry = new EventTrigger.Entry{
			eventID = EventTriggerType.PointerClick,
			callback = new EventTrigger.TriggerEvent()
		};

		entry.callback.AddListener(
			eventData => {
				onClick?.Invoke(input.WasState);
			}
		);
		m_button.triggers.Add(entry);
		m_image.color = Color.green;
	}

	public void UpdateUI(bool interactable)
	{
		m_button.enabled = interactable;
		m_image.enabled = interactable;
	}
}
