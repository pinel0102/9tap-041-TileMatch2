using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using TMPro;

using static LevelEditor;

public class BoardParameter
{
	public int TileSize;
	public int CellCount;
	public Action<InputController.State> OnPointerClick;
	public Action<int> OnTakeStep;
	public Action OnRemove;
}

public class BoardView : MonoBehaviour
{
	[SerializeField]
	private GameObject m_layerViewPrefab;

	[SerializeField]
	private Graphic m_frame;

	[SerializeField]
	private TMP_Text m_boardIndexText;

	[SerializeField]
	private RectTransform m_board;

	[SerializeField]
	private TileBrush m_brush;

	[SerializeField]
	private Image m_wireFrame;

	[SerializeField]
	private Transform m_layerContainer;

	[SerializeField]
	private LevelEditorButton m_prevButton;

	[SerializeField]
	private LevelEditorButton m_nextButton;

	[SerializeField]
	private LevelEditorButton m_removeButton;
	
	private List<LayerView> m_placedLayerObjects = new();

	public void OnSetup(BoardParameter parameter)
	{
		CancellationToken token = this.GetCancellationTokenOnDestroy();

		int tileSize = parameter.TileSize;
		int cellCount = parameter.CellCount;

		m_board.SetSize(tileSize * cellCount);
		m_brush.OnSetup(tileSize, onClick: parameter.OnPointerClick);
		m_wireFrame.pixelsPerUnitMultiplier = 100 / (float)tileSize;

		m_prevButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(-1));
		m_nextButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(1));
		m_removeButton.OnSetup("Remove", () => parameter?.OnRemove?.Invoke());
	}

	public void OnUpdateBrushWidget(Vector2 localPosition, bool interactable, bool drawable)
	{
		m_brush.transform.localPosition = localPosition;
		m_brush.UpdateUI(interactable, drawable);
	}

	public void OnDrawTile(int layerIndex, Vector2 position, float size, Color color)
	{
		if (m_placedLayerObjects.TryGetValue(layerIndex, out var layerView))
		{
			layerView.Draw(position, size, color);
		}
	}

	public void OnEraseTile(int layerIndex, Vector2 position)
	{
		if (m_placedLayerObjects.TryGetValue(layerIndex, out var layerView))
		{
			layerView.Erase(position);
		}
	}

	public void OnUpdateBoardView(int boardCount, int boardIndex)
	{
		m_boardIndexText.text = $"Current Board[{boardIndex}]";
		m_removeButton.SetInteractable(boardCount > 1);
		m_prevButton.SetInteractable(boardIndex > 0);
		m_nextButton.UpdateUI(boardIndex + 1 >= boardCount? "+": ">>");
	}

	public void OnUpdateLayerView(IReadOnlyList<LayerInfo> layers, int selectedIndex)
	{
		m_frame.color = layers[selectedIndex].Color;

		m_placedLayerObjects.ForEach(
			each => {
				each.Clear();
				each.gameObject.SetActive(false);
			}
		);

		for (int index = 0, count = layers.Count; index < count; index++)
		{
			LayerInfo layer = layers[index];
            LayerView layerView = m_placedLayerObjects.HasIndex(index) switch {
				true => m_placedLayerObjects[index],
				false => CreateView(index)
			};

			layerView.gameObject.SetActive(true);
			layerView.Draw(layer);
		}

		LayerView CreateView(int index)
		{
			GameObject item = Instantiate<GameObject>(m_layerViewPrefab, m_layerContainer, false);
			var view = item.GetComponent<LayerView>();
			m_placedLayerObjects.Add(view);
			return view;
		}
	}

	public void ClearTilesInLayer(int index)
	{
		if (m_placedLayerObjects.TryGetValue(index, out var layerView))
		{
			layerView.Clear();
		}
	}

	public void OnVisibleWireFrame(bool enabled)
	{
		m_wireFrame.gameObject.SetActive(enabled);
	}

    public void OnVisibleLayer(int layerIndex, bool isOn)
    {
        if (m_placedLayerObjects.TryGetValue(layerIndex, out var layerView))
		{
			layerView.OnVisible(isOn);
		}
    }
}
