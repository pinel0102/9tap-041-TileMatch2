#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System;

using NineTap.Common;

[ResourcePath("UI/Widgets/JigsawPuzzlePiece")]
public class JigsawPuzzlePiece : CachedBehaviour, IDragHandler
{
	[SerializeField]
	private Image m_image = null!;

	private bool m_draggable = false;
	private Action<Vector2> m_onCheck = null!;

	public void OnSetup(PuzzlePieceItemData itemData)
	{
		m_image.sprite = itemData.Sprite;
		m_draggable = false;
		m_image.rectTransform.SetSize(itemData.Size);
	}

	public void OnSetup(PuzzlePieceItemData itemData, PointerEventData eventData, Action onCheck)
	{
		m_image.sprite = itemData.Sprite;
		m_image.rectTransform.SetSize(itemData.Size);
		m_draggable = true;
		m_onCheck = pos => onCheck?.Invoke();
		eventData.pointerDrag = CachedGameObject;

		OnDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (m_draggable && UIManager.SceneCanvas != null)
		{	
			if (CachedRectTransform == null)
			{
				return;
			}

			float factor = UIManager.SceneCanvas.scaleFactor;
			CachedRectTransform.anchoredPosition += eventData.delta / factor;
			m_onCheck?.Invoke(CachedRectTransform.position);
		}
	}

	public void Attached()
	{
		m_draggable = false;
	}
}
