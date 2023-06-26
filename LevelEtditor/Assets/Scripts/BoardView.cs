using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;


public class BoardView : MonoBehaviour
{
	[SerializeField]
	private int m_tileSize = 80;

	[SerializeField]
	private int m_cellCount = 7;

	[SerializeField]
	private RectTransform m_board;

	[SerializeField]
	private TileBrush m_brush;

	[SerializeField]
	private Image m_wireFrame;

	[SerializeField]
	private Transform m_tileParent;

	
	private RectTransform m_rectTransform;
	
	private List<Bounds> m_placedTileBounds = new();

	private void Awake()
	{
		m_rectTransform = transform as RectTransform;
	}

	public void OnSetup(Action<Vector2> onClick)
    {
		CancellationToken token = this.GetCancellationTokenOnDestroy();
		Bounds boardBounds = m_board.GetBounds(m_rectTransform);

		m_wireFrame.pixelsPerUnitMultiplier = 100 / (float)m_tileSize;

		m_board.SetSize(m_tileSize * m_cellCount);
		m_brush.OnSetup(
			m_tileSize,
			onClick: pos => {
				onClick?.Invoke(pos);
				GameObject go = new GameObject("Tile");
				go.transform.SetParent(m_tileParent);
				go.transform.localPosition = pos;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;

				Image image = go.AddComponent<Image>();
				image.color = Color.green;
				image.rectTransform.SetSize(m_tileSize);

				m_placedTileBounds.Add(image.rectTransform.GetBounds(m_board, 0.99f));
			}
		);

		m_brush.Bounds.Subscribe(
			bounds =>
			{
				m_brush.SetBrushState(
					interactable: boardBounds.Intersects(bounds), 
					drawable: m_placedTileBounds.All(placed => !placed.Intersects(bounds))
				);
			}
		);
	}

	public void MoveBrush(float snapping, Vector3 inputPosition)
	{
		var (x, y, _) = m_board.InverseTransformPoint(inputPosition);

		m_board.GetBounds(m_rectTransform);

		m_brush.SetLocalPositionAndBounds(
			localPosition: new Vector2(Mathf.RoundToInt(x / snapping) * snapping, Mathf.RoundToInt(y / snapping) * snapping),
			viewPort: m_rectTransform
		);
	}

	public void VisibleWireFrame(bool enabled)
	{
		m_wireFrame.gameObject.SetActive(enabled);
	}

}
