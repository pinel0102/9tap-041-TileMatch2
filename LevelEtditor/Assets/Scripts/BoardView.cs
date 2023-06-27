using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class BoardParameter
{
	public int TileSize;
	public int CellCount;
	public Action OnPointerClick;
}

public class BoardView : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_board;

	[SerializeField]
	private TileBrush m_brush;

	[SerializeField]
	private Image m_wireFrame;

	[SerializeField]
	private Transform m_tileParent;

	public Queue<GameObject> m_tileObjectPool = new();

	public void OnSetup(BoardParameter parameter)
	{
		CancellationToken token = this.GetCancellationTokenOnDestroy();

		int tileSize = parameter.TileSize;
		int cellCount = parameter.CellCount;

		m_board.SetSize(tileSize * cellCount);
		m_brush.OnSetup(tileSize, onClick: parameter.OnPointerClick);
		m_wireFrame.pixelsPerUnitMultiplier = 100 / (float)tileSize;

	}

	public void OnUpdateBrushWidget(Vector2 localPosition, bool interactable, bool drawable)
	{
		m_brush.transform.localPosition = localPosition;
		m_brush.UpdateUI(interactable, drawable);
	}

	public void OnDraw(Vector2 position)
	{
		GameObject go = m_tileObjectPool.Count > 0? m_tileObjectPool.Dequeue() : new GameObject("Tile");
		go.SetActive(true);
		go.transform.SetParent(m_tileParent);
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		
		if (!go.TryGetComponent<Image>(out Image image))
		{
			image = go.AddComponent<Image>();
		}
		image.color = Color.green;

		RectTransform rectTransform = go.transform as RectTransform;
		rectTransform.localPosition = position;
		rectTransform.SetSize(m_brush.RectTransform.sizeDelta);
	}

	public void ClearTiles()
	{
		var children = m_tileParent.GetComponentsInChildren<Transform>(false);
		var enumerable = children.Where(child => !ReferenceEquals(child, m_tileParent)).Select(child => child.gameObject);
		foreach (GameObject child in enumerable)
		{
			child.SetActive(false);
			m_tileObjectPool.Enqueue(child);
		}
	}

	public void VisibleWireFrame(bool enabled)
	{
		m_wireFrame.gameObject.SetActive(enabled);
	}
}
