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
	Color Color,
	float Size,
	float Snapping,
	Vector2 Position,
	IReadOnlyList<Bounds> PlacedTiles
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
	private readonly IAsyncReactiveProperty<BrushInfo> m_brushInfo;
	private readonly AsyncMessageBroker<BrushWidgetInfo> m_brushMessageBroker;
	private readonly IAsyncReactiveProperty<InternalState> m_internalState;
	private readonly CancellationTokenSource m_cancellationTokenSource;
	private readonly AsyncReactiveProperty<bool> m_savable;
	
	private InternalState State => m_internalState.Value;
	
	public IReadOnlyAsyncReactiveProperty<CurrentState> UpdateState { get; }
	public IUniTaskAsyncEnumerable<BrushWidgetInfo> BrushMessageBroker => m_brushMessageBroker.Subscribe();
	public IReadOnlyAsyncReactiveProperty<bool> Savable => m_savable;

	public LevelEditorPresenter(LevelEditor view, float cellSize, float cellCount)
	{
		m_view = view;
		m_dataManager = new LevelDataManager();
		m_cancellationTokenSource = new();
		m_boardBounds = new Bounds(Vector2.zero, Vector2.one * cellSize * cellCount);
		m_brushInfo = new AsyncReactiveProperty<BrushInfo>(
			new BrushInfo(Color.white, cellSize, cellSize, Position: Vector2.zero, PlacedTiles: Array.Empty<Bounds>())
		).WithDispatcher();
		m_internalState = new AsyncReactiveProperty<InternalState>(InternalState.Empty).WithDispatcher();
		m_savable = new(false);
		m_brushMessageBroker = new();

		UpdateState = m_internalState
			.Select(info => info.ToCurrentState())
			.ToReadOnlyAsyncReactiveProperty(m_cancellationTokenSource.Token);
		
		m_internalState.Subscribe(_=> UpdateSavable());

		m_brushInfo.Subscribe(
			info =>
			{
				m_brushMessageBroker.Publish(
					new BrushWidgetInfo(
						Interactable: info.Overlap(m_boardBounds),
						Drawable: info.PlacedTiles.All(
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
		m_brushMessageBroker.Dispose();
		m_cancellationTokenSource.Dispose();
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

		(_, float size, _, _, _) = m_brushInfo.Value;

		var layer = levelData[0]
			.Layers?
			.Select(
				(layer, index) => {
					return new LayerInfo(
						layer
						.Tiles
						.Select(tile => new TileInfo(tile.Position, size))
						.AsReadOnlyList()
					);
				}
			)?
			.FirstOrDefault();

		List<Bounds> bounds = new();

		if (layer?.Tiles != null)
		{
			foreach (TileInfo tile in layer.Tiles)
			{
				bounds.Add(
					new Bounds(tile.Position, Vector2.one * tile.Size)
				);
			}
		}

		m_brushInfo.Update(info => 
			info with { 
				Color = layer.Color,
				PlacedTiles = bounds
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

	#region Board
	public void LoadBoardByStep(int direction)
	{
		int index = Mathf.Max(0, State.BoardIndex + direction);

		int boardCount = State.BoardCount;

		if (State.BoardCount <= index)
		{
			//보드 추가
			m_dataManager.AddBoardData(out boardCount);
			m_savable.Value = false;
		}

		var layers = GetLayerInfos(index);

		m_internalState.Update(state =>
			state with {
				UpdateType = UpdateType.BOARD,
				BoardCount = boardCount,
				BoardIndex = index,
				LayerIndex = 0,
				TileCountInBoard = layers.Sum(layer => layer.Tiles.Count()),
				TileCountAll = m_dataManager.CurrentLevelData.TileCountAll,
				Layers = layers.ToList()
			}
		);

		ResetPlacedTilesInLayer(0);

	}

	public void LoadBoard(int boardIndex)
	{
		int boardCount = State.BoardCount;
		var layers = GetLayerInfos(boardIndex);

		m_internalState.Update(state =>
			state with {
				UpdateType = UpdateType.BOARD,
				BoardCount = boardCount,
				BoardIndex = boardIndex,
				TileCountInBoard = layers.Sum(layer => layer.Tiles.Count()),
				TileCountAll = m_dataManager.CurrentLevelData.TileCountAll,
				Layers = layers.ToList()
			}
		);

	}

	public void RemoveBoard()
	{
		int removeIndex = State.BoardIndex;

		if (m_dataManager.TryRemoveBoardData(removeIndex, out int boardCount))
		{
			int index = removeIndex < boardCount? removeIndex : removeIndex - 1;
			var layers = GetLayerInfos(index);

			m_internalState.Update(state =>
				state with {
					UpdateType = UpdateType.BOARD,
					BoardCount = boardCount,
					BoardIndex = index,
					TileCountInBoard = layers.Sum(layer => layer.Tiles.Count()),
					TileCountAll = m_dataManager.CurrentLevelData.TileCountAll,
					Layers = layers.ToList()
				}
			);

		}

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
		(Color color, float size, _, Vector2 position, var placedTiles) = m_brushInfo.Value;

		int boardIndex = State.BoardIndex;
		int layerIndex = State.LayerIndex;

		List<Bounds> bounds = new(placedTiles);

		switch (state)
		{
			case LEFT_BUTTON_PRESSED:
			case LEFT_BUTTON_RELEASED:
				if (m_dataManager.TryAddTileData(boardIndex, layerIndex, position))
				{
					bounds.Add(new Bounds(position, Vector2.one * size));
					m_view.OnDrawTile(layerIndex, position, size, color);
				}
				break;
			case RIGHT_BUTTON_PRESSED:
			case RIGHT_BUTTON_RELEASED:
				if (m_dataManager.TryRemoveTileData(boardIndex, layerIndex, position))
				{
					bounds.RemoveAll(tile => Vector2.Distance(position, tile.center) == 0);
					m_view.OnEraseTile(layerIndex, position);
				}
				break;
		}

		m_brushInfo.Value = m_brushInfo.Value with {
			PlacedTiles = bounds
		};

		var layers = GetLayerInfos(boardIndex);

		m_internalState.Update(
			state => state with {
				UpdateType = UpdateType.TILE,
				TileCountInBoard = layers.Sum(layer => layer.Tiles.Count()),
				TileCountAll = m_dataManager.CurrentLevelData.TileCountAll,
			}
		);

	}

	public void ClearTilesInLayer()
	{
		m_brushInfo.Value = m_brushInfo.Value with {
			PlacedTiles = Array.Empty<Bounds>()
		};
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

	private void ResetPlacedTilesInLayer(int layerIndex)
	{
		(Color c, float size, _, _, _) = m_brushInfo.Value;
		
		List<Bounds> bounds = new();

		if (State.Layers.TryGetValue(layerIndex, out var layer))
		{
			foreach (TileInfo tile in layer.Tiles)
			{
				bounds.Add(
					new Bounds(tile.Position, Vector2.one * tile.Size)
				);
			}
		}

		m_brushInfo.Update(info => info with { Color = layer.Color, PlacedTiles = bounds });
	}
	#endregion

	#region Layer
	public void AddLayer()
	{
		if (m_dataManager.TryAddLayerData(State.BoardIndex, State.LayerIndex, out int index))
		{
			SelectLayer(index);
		}
	}

	public void RemoveLayer()
	{
		if (State.Layers.Count <= 1)
		{
			//레이어가 최소 하나는 있어야함.
			return;
		}

		m_dataManager.RemoveLayerData(State.BoardIndex, State.LayerIndex);
		int replaceIndex = Mathf.Max(0, State.LayerIndex - 1);
		SelectLayer(replaceIndex);
	}

	public void SelectLayer(int layerIndex)
	{
		var layers = GetLayerInfos(State.BoardIndex);

		m_internalState.Update( 
			state => state with {
				UpdateType = UpdateType.LAYER,
				LayerIndex = layers.HasIndexReadOnly(layerIndex)? layerIndex : State.LayerIndex,
				TileCountInBoard = layers.Sum(layer => layer.Tiles.Count()),
				TileCountAll = m_dataManager.CurrentLevelData.TileCountAll,
				Layers = layers.ToList()
			}
		);

		ResetPlacedTilesInLayer(layerIndex);
	}
	#endregion

	private IReadOnlyList<LayerInfo> GetLayerInfos(int boardIndex)
	{
		(_, float size, _, _, _) = m_brushInfo.Value;

		return m_dataManager
			.CurrentLevelData[boardIndex]?
			.Layers?
			.Select(
				(layer, index) => {
					return new LayerInfo(
						layer
						.Tiles
						.Select(tile => new TileInfo(tile.Position, size))
						.AsReadOnlyList()
					);
				}
			)?.ToArray() ?? Array.Empty<LayerInfo>();
	}

	private void UpdateSavable()
	{
		int countInBoard = State.TileCountInBoard;
		int countAll = State.TileCountAll;

		if (countAll == 0)
		{
			m_savable.Value = false;
			return;
		}

		int requiredMultiples = m_dataManager.Config.RequiredMultiples;
		m_savable.Value = countInBoard % requiredMultiples == 0 && countAll % requiredMultiples == 0;
	}
}
