using UnityEngine;
using UnityEngine.UI;

using System;
using System.Threading;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class TileBrush : MonoBehaviour
{
	[SerializeField]
	private Button m_button;

	[SerializeField]
	private Image m_image;

	public RectTransform RectTransform => transform as RectTransform;

	public void OnSetup(int size, Action onClick)
	{
		CancellationToken token = this.GetCancellationTokenOnDestroy();

		RectTransform.SetSize(size);
		if (onClick != null)
		{
			m_button.onClick.AddListener(onClick.Invoke);
		}
	}

	public void UpdateUI(bool interactable, bool drawable)
	{
		m_button.interactable = interactable && drawable;
		m_image.enabled = interactable;
		m_image.color = drawable switch {
			true => Color.green,
			_ => Color.red
		};
	}
}
