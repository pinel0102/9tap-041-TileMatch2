using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System;
using System.Threading;

using Cysharp.Threading.Tasks;
using TMPro;

public class TileBrush : MonoBehaviour
{
	[SerializeField]	private EventTrigger m_button;
	[SerializeField]	private Image m_image;
	[SerializeField]	private GameObject m_frame;
    [SerializeField]	private TMP_Text m_textBlocker;
    [SerializeField]	private TMP_Text m_textICD;

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

		m_frame.SetActive(false);
        m_textBlocker.gameObject.SetActive(false);
	}

	public void UpdateUI(bool interactable, BrushMode mode)
	{
		m_button.enabled = interactable;
		m_image.enabled = interactable;
		m_image.color = mode is BrushMode.MISSION_STAMP? Color.yellow : Color.green;
	}

	public void UpdateColor(Color color, BlockerType blockerType, int blockerICD, bool variableICD)
	{
		m_button.enabled = false;
		m_image.enabled = true;
		m_image.color = color;

        bool isBlocker = blockerType != BlockerType.None;

		m_textBlocker.SetText(blockerType.ToString());
        m_textICD.SetText(blockerICD.ToString());
        m_frame.SetActive(isBlocker);
        m_textBlocker.gameObject.SetActive(isBlocker);
        m_textICD.gameObject.SetActive(isBlocker && variableICD);
	}
}
