using UnityEngine;
using UnityEngine.UI;

using System;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

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

	private List<GameObject> m_placedTileObjects = new();
	
	private Queue<GameObject> m_tileObjectPool = new();

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

	public void OnDraw(Vector2 position, float size)
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
		rectTransform.SetSize(size);

		m_placedTileObjects.Add(go);
	}

	public void OnRemoveTile(Vector2 position)
	{
		if (m_placedTileObjects.Count > 0)
		{
			GameObject minObject = m_placedTileObjects[0];
			float minDistance = Vector2.Distance(minObject.transform.localPosition, position);

			m_placedTileObjects.ForEach(tile => {
					float distance = Vector2.Distance(tile.transform.localPosition, position);
					if (minDistance > distance)
					{
						minObject = tile;
						minDistance = distance;
					}
				}
			);

			minObject.SetActive(false);
			m_tileObjectPool.Enqueue(minObject);
			m_placedTileObjects.RemoveAll(tile => ReferenceEquals(minObject, tile));
		}
	}

	public void ClearTiles()
	{
		foreach (GameObject tile in m_placedTileObjects)
		{
			tile.SetActive(false);
			m_tileObjectPool.Enqueue(tile);
		}

		m_placedTileObjects.Clear();
	}

	public void VisibleWireFrame(bool enabled)
	{
		m_wireFrame.gameObject.SetActive(enabled);
	}
}
