// #define ENABLE_DEBUG_LOG
using UnityEngine;

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using static InputController.State;
using static LevelEditor;

public record BrushInfo
(
	float Size,
	float Snapping,
	Vector2 Position
)
{
	public Bounds GetBounds() => new Bounds(Position, Vector2.one * Size);

	public bool Overlap(Bounds other)
	{
		Bounds bounds = GetBounds();
		return bounds.min.x >= other.min.x &&
			bounds.min.y >= other.min.y &&
			bounds.max.x <= other.max.x &&
			bounds.max.y <= other.max.y;
	}
}

public record BrushWidgetInfo
(
	bool Interactable,
	bool Drawable,
	Vector2 LocalPosition
);

public class LevelEditorPresenter : IDisposable
{
	private readonly LevelDataManager m_dataManager;
	private readonly LevelEditor m_view;
	private readonly Bounds m_boardBounds;
	private readonly List<Bounds> m_placedTileBounds;
	private readonly AsyncReactiveProperty<BrushInfo> m_brushInfo;
	private readonly AsyncMessageBroker<BrushWidgetInfo> m_brushMessageBroker;
	private readonly IAsyncReactiveProperty<InternalState> m_internalState;
	private readonly CancellationTokenSource m_cancellationTokenSource;
	
	public IReadOnlyAsyncReactiveProperty<CurrentState> UpdateState { get; }
	public IUniTaskAsyncEnumerable<BrushWidgetInfo> BrushMessageBroker => m_brushMessageBroker.Subscribe();

	private InternalState State => m_internalState.Value;

	public LevelEditorPresenter(LevelEditor view, float cellSize, float cellCount)
	{
		m_view = view;
		m_dataManager = new LevelDataManager();
		m_cancellationTokenSource = new();
		m_boardBounds = new Bounds(Vector2.zero, Vector2.one * cellSize * cellCount);
		m_brushInfo = new(new BrushInfo(cellSize, cellSize, Position: Vector2.zero));
		m_internalState = new AsyncReactiveProperty<InternalState>(InternalState.Empty).WithDispatcher();

		m_brushMessageBroker = new();
		m_placedTileBounds = new();

		UpdateState = m_internalState
			.Select(info => info.ToCurrentState())
			.ToReadOnlyAsyncReactiveProperty(m_cancellationTokenSource.Token);

		m_brushInfo.Subscribe(
			info =>
			{
				m_brushMessageBroker.Publish(
					new BrushWidgetInfo(
						Interactable: info.Overlap(m_boardBounds),
						Drawable: m_placedTileBounds.All(
							placed => placed.SqrDistance(info.Position) >= Mathf.Pow(info.Size * 0.5f, 2f)
						),
						LocalPosition: info.Position
					)
				);
			}
		);

		#if ENABLE_DEBUG_LOG
		UniTask.Void( 
			async () =>
			{
				await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.LastPostLateUpdate))
				{
					Bounds brushBounds = new(m_brushInfo.Value.Position, Vector2.one * m_brushInfo.Value.Size);
					OnDebugDraw(m_boardBounds, Color.yellow);
					OnDebugDraw(brushBounds, Color.green);
					m_placedTileBounds.ForEach(bounds => OnDebugDraw(bounds, Color.red));
				}
			}
		);

		void OnDebugDraw(Bounds bounds, Color color)
		{
			Vector2 vMin = bounds.min;
			Vector2 vMax = bounds.max;

			var p1 = new Vector2(vMin.x, vMin.y);
			var p2 = new Vector2(vMax.x, vMin.y);
			var p3 = new Vector2(vMax.x, vMax.y);
			var p4 = new Vector2(vMin.x, vMax.y);

			Debug.DrawLine(p1, p2, color);
			Debug.DrawLine(p2, p3, color);
			Debug.DrawLine(p3, p4, color);
			Debug.DrawLine(p4, p1, color);
		}
		#endif
	}

	#region IDisposable Interface
	public void Dispose()
	{
		m_brushInfo.Dispose();
		m_brushMessageBroker.Dispose();
		m_placedTileBounds.Clear();
	}
	#endregion

	#region Level
	public void LoadLevel(int level)
	{
		LevelData data = m_dataManager.LoadLevelData(level);
		LoadLevelInternal(data);
	}

	public void LoadLevelByStep(int direction)
	{
		LevelData data = m_dataManager.LoadLevelDataByStep(direction);
		LoadLevelInternal(data);
	}

	private void LoadLevelInternal(LevelData levelData)
	{
		if (levelData == null)
		{
			return;
		}

		(float size, _, _) = m_brushInfo.Value;
		Layer layer = levelData.GetLayer(0, 0);

		List<(Vector2, float)> drawList = new();

		m_placedTileBounds.Clear();

		layer.Tiles.ForEach(
			tile => {
				m_placedTileBounds.Add(new Bounds(tile.Position, Vector2.one * size));
				drawList.Add((tile.Position, size));
			}
		);

		m_internalState.Update(info => 
			InternalState.ToInternalInfo(levelData, m_dataManager.Config.LastLevel, size)
		);
	}

	public void SaveLevel()
	{
		m_dataManager.SaveLevelData();
	}
	#endregion

	#region Brush
	public void SetBrushPosition(Vector2 inputPosition)
	{
		var (x, y) = inputPosition;
		BrushInfo info = m_brushInfo.Value;
		float snapping = info.Snapping;

		Vector2 positionWithSnapping = new Vector2(
			Mathf.RoundToInt(x / snapping) * snapping, 
			Mathf.RoundToInt(y / snapping) * snapping
		);

		m_brushInfo.Value = info with { Position = positionWithSnapping };
	}

	public void ChangeSnapping(float snapping)
	{
		BrushInfo info = m_brushInfo.Value;
		m_brushInfo.Value = info with { Snapping = snapping };
	}
	#endregion
	
	#region Tile
	public void SetTileInLayer(InputController.State state)
	{
		(float size, _, Vector2 position) = m_brushInfo.Value;

		int boardIndex = State.BoardIndex;
		int layerIndex = State.LayerIndex;

		switch (state)
		{
			case LEFT_BUTTON_PRESSED:
			case LEFT_BUTTON_RELEASED:
				if (m_dataManager.TryAddTileData(boardIndex, layerIndex, position))
				{
					m_placedTileBounds.Add(new Bounds(position, Vector2.one * size));
					m_view.OnDrawCell(layerIndex, position, size);
				}
				break;
			case RIGHT_BUTTON_PRESSED:
			case RIGHT_BUTTON_RELEASED:
				if (m_dataManager.TryRemoveTileData(boardIndex, layerIndex, position))
				{
					m_placedTileBounds.RemoveAll(tile => Vector2.Distance(position, tile.center) == 0);
					m_view.OnEraseCell(layerIndex, position);
				}
				break;
		}
	}

	public void ClearTilesInLayer()
	{
		m_placedTileBounds.Clear();
		m_dataManager.ClearTileDatasInLayer(State.BoardIndex, State.LayerIndex);
		m_view.ClearTilesInLayer(State.LayerIndex);
	}

	public void IncrementNumberOfTileTypes(int increment)
	{
		if (m_dataManager.CurrentLevelData is var data and not null)
		{
			var number = m_dataManager.UpdateNumberOfTypes(data.NumberOfTileTypes + increment);

			if (number.HasValue)
			{
				m_internalState.Update(info => info with {
						UpdateType = UpdateType.NUMBER_OF_TILE_TYPES,
						NumberOfTileTypes = number.Value
					}
				);
			}
		}
	}

	public void SetNumberOfTileTypes(int value)
	{
		if (m_dataManager.CurrentLevelData is var data and not null)
		{
			var number = m_dataManager.UpdateNumberOfTypes(value);

			if (number.HasValue)
			{
				m_internalState.Update(info => info with {
						UpdateType = UpdateType.NUMBER_OF_TILE_TYPES,
						NumberOfTileTypes = number.Value
					}
				);
			}
		}
	}
	#endregion

	#region Layer
	public void AddLayer()
	{
		(float size, _, _) = m_brushInfo.Value;

		if (m_internalState.Value is var state)
		{
			if (m_dataManager.TryAddLayerData(state.BoardIndex, state.LayerIndex))
			{
				var layers = m_dataManager
					.CurrentLevelData[state.BoardIndex]
					.Layers
					.Select(
						layer => layer
							.Tiles
							.Select(tile => new TileInfo(tile.Position, size))
							.AsReadOnlyList()
					);

				m_internalState.Update(state => state with {
						UpdateType = UpdateType.LAYER,
						LayerIndex = state.LayerIndex + 1,
						TileCountInLayer = 0,
						TileCountAll = layers.Select(layer => layer.Count()).Count(),
						Layers = layers.ToList()
					}
				);
			}
		}
	}

	public void RemoveLayer()
	{
	}
	#endregion
}
