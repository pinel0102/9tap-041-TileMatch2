using UnityEngine;
using UnityEngine.UI;

using System;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class TileBrush : MonoBehaviour
{
	[SerializeField]
	private Button m_button;

	[SerializeField]
	private Image m_image;

	public RectTransform RectTransform => transform as RectTransform;

	private AsyncReactiveProperty<Bounds> m_bounds = new(new());
	public IReadOnlyAsyncReactiveProperty<Bounds> Bounds => m_bounds;

	private void OnDestroy()
	{
		m_bounds.Dispose();
	}

	public void OnSetup(int size, Action<Vector2> onClick)
	{
		m_image.rectTransform.SetSize(size);
		m_button.onClick.AddListener(
			() => {
				onClick?.Invoke(transform.localPosition);
			}
		);
	}

	public void SetBrushState(bool interactable, bool drawable)
	{
		m_button.interactable = interactable && drawable;
		m_image.enabled = interactable;
		m_image.color = drawable ? Color.green : Color.red;
	}

	public void SetLocalPositionAndBounds(Vector2 localPosition, RectTransform viewPort)
	{
		transform.localPosition = localPosition;
		m_bounds.Value = m_image?.rectTransform?.GetBounds(viewPort) ?? new();
	}

}
