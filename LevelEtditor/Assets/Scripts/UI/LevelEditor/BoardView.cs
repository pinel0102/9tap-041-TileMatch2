using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using static LevelEditor;

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
	private Transform m_layerContainer;
	
	private readonly List<LayerView> m_placedLayerObjects = new();

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

	public void OnDrawTile(int layerIndex, Vector2 position, float size)
	{
		if (m_placedLayerObjects.TryGetValue(layerIndex, out var layerView))
		{
			layerView.Draw(position, size);
		}
	}

	public void OnEraseTile(int layerIndex, Vector2 position)
	{
		if (m_placedLayerObjects.TryGetValue(layerIndex, out var layerView))
		{
			layerView.Erase(position);
		}
	}

	public void OnUpdateLayerView(List<IReadOnlyList<TileInfo>> layers)
	{
		var enumerable = layers
			.Select((layer, index) => (index, layer))
			.OrderBy(x => x.index);

		foreach (var (index, layer) in enumerable)
		{
			if (!m_placedLayerObjects.TryGetValue(index, out var layerView))
			{
				layerView = LayerView.CreateLayerView(m_layerContainer);
				m_placedLayerObjects.Add(layerView);
			}

			layerView.Clear();
			layerView.Draw(layer.Select(tile => (tile.Position, tile.Size)));
		}
	}

	public void ClearTilesInLayer(int index)
	{
		if (m_placedLayerObjects.TryGetValue(index, out var layerView))
		{
			layerView.Clear();
		}
	}

	public void VisibleWireFrame(bool enabled)
	{
		m_wireFrame.gameObject.SetActive(enabled);
	}
}
