using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System;
using System.Threading;

using Cysharp.Threading.Tasks;

public class TileBrush : MonoBehaviour
{
	[SerializeField]
	private EventTrigger m_button;

	[SerializeField]
	private Image m_image;

	public RectTransform RectTransform => transform as RectTransform;

	public void OnSetup(int size, Action onClick)
	{
		CancellationToken token = this.GetCancellationTokenOnDestroy();

		RectTransform.SetSize(size);
		EventTrigger.Entry entry = new EventTrigger.Entry{
			eventID = EventTriggerType.PointerClick,
			callback = new EventTrigger.TriggerEvent()
		};

		entry.callback.AddListener(
			eventData => {
				onClick?.Invoke();
			}
		);
		m_button.triggers.Add(entry);
	}

	public void UpdateUI(bool interactable, bool drawable)
	{
		m_button.enabled = interactable;
		m_image.enabled = interactable;
		m_image.color = drawable switch {
			true => Color.green,
			_ => Color.red
		};
	}
}
