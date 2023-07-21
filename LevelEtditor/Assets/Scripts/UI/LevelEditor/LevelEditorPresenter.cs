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

	public LevelDataManager DataManager => m_dataManager;

	public LevelEditorPresenter(LevelEditor view, string path, float cellSize, float cellCount)
	{
		m_view = view;
		m_dataManager = new LevelDataManager(path);
		m_cancellationTokenSource = new();
		m_boardBounds = new Bounds(Vector2.zero, Vector2.one * cellSize * cellCount);
		m_brushInfo = new AsyncReactiveProperty<BrushInfo>(
			new BrushInfo(Color.white, cellSize, cellSize, Position: Vector2.zero, DrawOrder.BOTTOM)
		).WithDispatcher();
		m_internalState = new AsyncReactiveProperty<InternalState>(InternalState.Empty).WithDispatcher();
		m_savable = new(false);
		m_brushMessageBroker = new();

		UpdateState = m_internalState
			.Select(info => info.ToCurrentState())
			.ToReadOnlyAsyncReactiveProperty(m_cancellationTokenSource.Token);
		
		m_internalState.WithoutCurrent().Subscribe(UpdateSavable);

		m_brushInfo.Subscribe(
			info =>
			{
				m_brushMessageBroker.Publish(
					new BrushWidgetInfo(
						Interactable: info.Overlap(m_boardBounds),
						LocalPosition: info.Position
					)
				);
			}
		);

		#region Local Function
		void UpdateSavable(InternalState state)
		{
			int requiredMultiples = m_dataManager.Config.RequiredMultiples;

			m_savable.Value = state.Boards.All(
				board => board.Layers.All(layer => layer.TileCount > 0) && board.TileCountAll % requiredMultiples == 0
			);
		}
		#endregion

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
	public async UniTask Initialize()
	{
		LevelData data = await m_dataManager.LoadConfig();
		LoadLevelInternal(data);
	}

	public async UniTask LoadLevel(int level)
	{
		LevelData data = await m_dataManager.LoadLevelData(level);
		LoadLevelInternal(data);
	}

	public async UniTask LoadLevelByStep(int direction)
	{
		LevelData data = await m_dataManager.LoadLevelDataByStep(direction);
		LoadLevelInternal(data);
	}

	private void LoadLevelInternal(LevelData levelData)
	{
		if (levelData == null)
		{
			return;
		}

		(_, float size, _, _, _) = m_brushInfo.Value;

		m_internalState.Update(info => 
			InternalState.ToInternalInfo(levelData, m_dataManager.Config.LastLevel, size)
		);

		ResetPlacedTilesInLayer(0);
	}

	public async UniTask SaveLevel()
	{
		await m_dataManager.SaveLevelData();
		m_internalState.Update(state => state with { UpdateType = UpdateType.NONE });
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
		}

		LoadBoardInternal(index, boardCount);
	}

	public void RemoveBoard()
	{
		int removeIndex = State.BoardIndex;

		if (m_dataManager.TryRemoveBoardData(removeIndex, out int boardCount))
		{
			int index = removeIndex < boardCount? removeIndex : removeIndex - 1;
			LoadBoardInternal(index, boardCount);
		}

	}

	private void LoadBoardInternal(int index, int boardCount)
	{
		(_, float size, _, _, _) = m_brushInfo.Value;

		var boardInfos = BoardInfo.Create(m_dataManager.CurrentLevelData, size);

		m_internalState.Update(state =>
			state with {
				UpdateType = UpdateType.BOARD,
				BoardCount = boardCount,
				BoardIndex = index,
				TileCountInBoard = boardInfos[index].TileCountAll,
				TileCountAll = boardInfos.Sum(board => board.TileCountAll),
				Boards = boardInfos
			}
		);

		ResetPlacedTilesInLayer(0);
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
		(Color color, float size, _, Vector2 position, DrawOrder drawOrder) = m_brushInfo.Value;

		int boardIndex = State.BoardIndex;

		switch (state)
		{
			case LEFT_BUTTON_PRESSED:
			case LEFT_BUTTON_RELEASED:
				m_dataManager.TryAddTileData(drawOrder, boardIndex, new Bounds(position, Vector2.one * size), out int layerIndex);
				break;
			case RIGHT_BUTTON_PRESSED:
			case RIGHT_BUTTON_RELEASED:
				m_dataManager.TryRemoveTileData(boardIndex, new Bounds(position, Vector2.one * size));
				break;
		}

		var boardInfos = BoardInfo.Create(m_dataManager.CurrentLevelData, size);

		m_internalState.Update(
			state => state with {
				UpdateType = UpdateType.TILE,
				TileCountInBoard = boardInfos[boardIndex].TileCountAll,
				TileCountAll = boardInfos.Sum(board => board.TileCountAll),
				Boards = boardInfos
			}
		);

	}

	public void IncrementNumberOfTileTypes(int increment)
	{
		if (m_dataManager.CurrentLevelData is var data and not null)
		{
			int numbers = data?[State.BoardIndex]?.NumberOfTileTypes ?? 0;
			var number = m_dataManager.UpdateNumberOfTypes(State.BoardIndex, numbers + increment);

			if (number.HasValue)
			{
				m_internalState.Update(info => 
					info with { 
						UpdateType = UpdateType.NUMBER_OF_TILE_TYPES,
						Boards = State.Boards.Select(
							(board, index) => {
								if (State.BoardIndex == index)
								{
									return new BoardInfo(number.Value, board.Layers);
								}
								return board;
							}
						).ToArray()
					} 
				);
			}
		}

	}

	public void SetNumberOfTileTypes(int value)
	{
		if (m_dataManager.CurrentLevelData is var data and not null)
		{
			var number = m_dataManager.UpdateNumberOfTypes(State.BoardIndex, value);

			if (number.HasValue)
			{
				m_internalState.Update(info => 
					info with { 
						UpdateType = UpdateType.NUMBER_OF_TILE_TYPES,
						Boards = State.Boards.Select(
							(board, index) => {
								if (State.BoardIndex == index)
								{
									return new BoardInfo(number.Value, board.Layers);
								}
								return board;
							}
						).ToArray()
					} 
				);
			}
		}

	}

	private void ResetPlacedTilesInLayer(int layerIndex)
	{
		(Color c, float size, _, _, _) = m_brushInfo.Value;
		
		List<Bounds> bounds = new();

		if (State.CurrentBoard.TryGetValue(layerIndex, out var layer))
		{
			foreach (TileInfo tile in layer.Tiles)
			{
				bounds.Add(
					new Bounds(tile.Position, Vector2.one * tile.Size)
				);
			}
		}

		m_brushInfo.Update(info => info with { Color = layer.Color });
	}
	#endregion

	#region Layer
	public void RemoveLayer()
	{
		(Color c, float size, _, _, _) = m_brushInfo.Value;

		if (m_dataManager.TryRemovePeekLayer(State.BoardIndex))
		{
			var boardInfos = BoardInfo.Create(m_dataManager.CurrentLevelData, size);

			m_internalState.Update(
				state => state with {
					UpdateType = UpdateType.TILE,
					TileCountInBoard = boardInfos[State.BoardIndex].TileCountAll,
					TileCountAll = boardInfos.Sum(board => board.TileCountAll),
					Boards = boardInfos
				}
			);
		}
	}
	#endregion

	public void SetDifficult(int increment)
	{
		int currentDifficult = m_dataManager.CurrentLevelData.Difficult;
		int difficult = m_dataManager.SetDifficult(
			Mathf.Clamp(currentDifficult + increment, 0, (int)DifficultType.HARD)
		) ?? (int)DifficultType.NORMAL;

		m_internalState.Update( 
			state => state with {
				UpdateType = UpdateType.DIFFICULT,
				Difficult = (DifficultType)difficult
			}
		);
	}

	public void ChangeDrawOrder(DrawOrder order)
	{
		//Debug.Log(order);
		m_brushInfo.Update(info => info with { DrawOrder = order });
	}
}
