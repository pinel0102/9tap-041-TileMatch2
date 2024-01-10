// #define ENABLE_DEBUG_LOG
using UnityEngine;
using UnityEngine.Assertions;

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

	private readonly List<int> m_invisibleLayerIndexes = new();
	public IReadOnlyList<int> InvisibleLayerIndexes => m_invisibleLayerIndexes.ToArray();
	private readonly AsyncMessageBroker<IReadOnlyList<int>> m_invisibleLayersBroker;
	public IUniTaskAsyncEnumerable<IReadOnlyList<int>> InvisibleLayersBroker => m_invisibleLayersBroker.Subscribe();

	private (float snapping, DrawOrder order) m_cachedTileBrushOptions;

	public LevelEditorPresenter(LevelEditor view, float cellSize, float cellCount)
	{
		m_view = view;
		m_dataManager = new LevelDataManager(m_view);
		m_cancellationTokenSource = new();
		m_boardBounds = new Bounds(Vector2.zero, Vector2.one * cellSize * cellCount);
		m_brushInfo = new AsyncReactiveProperty<BrushInfo>(
			new TileBrushInfo(cellSize, cellSize, Position: Vector2.zero, DrawOrder.BOTTOM)
		).WithDispatcher();

		m_cachedTileBrushOptions = (cellSize, DrawOrder.BOTTOM);

		m_internalState = new AsyncReactiveProperty<InternalState>(InternalState.Empty).WithDispatcher();
		m_savable = new(false);
		m_brushMessageBroker = new();
		m_invisibleLayersBroker = new();

		m_dataManager.Saved.WithoutCurrent().Subscribe((saved) => m_view.SetVisibleWarning(!saved));

		UpdateState = m_internalState
			.Select(info => info.ToCurrentState())
			.ToReadOnlyAsyncReactiveProperty(m_cancellationTokenSource.Token);
		
		m_internalState.WithoutCurrent().Subscribe(UpdateSavable);

		m_brushInfo.Subscribe(
			info =>
			{
				m_brushMessageBroker.Publish(
					new BrushWidgetInfo(
						info.Mode,
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
	public async UniTask Initialize(string path)
	{
		LevelData data = await m_dataManager.LoadConfig(path);
		LoadLevelInternal(data);
	}

    public async UniTask LoadLevel(int level, bool forceLoad = false)
	{
		LevelData data = await m_dataManager.LoadLevelData(level, forceLoad);
		LoadLevelInternal(data);
	}

	public async UniTask LoadLevelByStep(int direction)
	{
		LevelData data = await m_dataManager.LoadLevelDataByStep(direction);
		LoadLevelInternal(data);
	}

	private void LoadLevelInternal(LevelData levelData)
	{
#if !UNITY_EDITOR
		Assert.IsNotNull(levelData);
#else
		if (levelData == null)
		{
			Debug.LogError("레벨데이터가 없음");
		}
#endif

		(_, float size, _) = m_brushInfo.Value;

		m_internalState.Update(info => 
			InternalState.ToInternalInfo(levelData, m_dataManager.Config.LastLevel, size)
		);
        
        //Debug.Log(CodeManager.GetMethodName() + string.Format("Level {0} / BoardCount : {1}", levelData.Key, State.BoardCount));

		ResetPlacedTilesInLayer(0);

		m_invisibleLayerIndexes.Clear();
		m_invisibleLayersBroker.Publish(Array.Empty<int>());
	}

    public async UniTask CreateLevel(int level, Board boardToCopy, bool forceLoad = false)
	{
		LevelData data = await m_dataManager.LoadLevelData(level, forceLoad);
        data.Boards.Clear();
        data.Boards.Add(boardToCopy);

		await SaveLevel(data);

        //LoadLevelInternal(data);
	}

    public async UniTask SaveLevel(LevelData levelData)
	{
        (_, float size, _) = m_brushInfo.Value;

        m_internalState.Update(info => 
			InternalState.ToInternalInfo(levelData, m_dataManager.Config.LastLevel, size)
		);

        await m_dataManager.SaveLevelData();
		m_internalState.Update(state => state with { UpdateType = UpdateType.NONE });
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
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Level {0} : Remove Board [{1}]</color>", m_dataManager.CurrentLevelData.Key, removeIndex));

			int index = removeIndex < boardCount? removeIndex : removeIndex - 1;
			LoadBoardInternal(index, boardCount);
		}
	}

    public void RemoveBoard(int removeIndex)
	{
		if (m_dataManager.TryRemoveBoardData(removeIndex, out int boardCount))
		{
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Level {0} : Remove Board [{1}]</color>", m_dataManager.CurrentLevelData.Key, removeIndex));

			int index = removeIndex < boardCount? removeIndex : removeIndex - 1;
			LoadBoardInternal(index, boardCount);
		}
	}

    public async UniTask<bool> SeperateBoard()
	{
        if (State.BoardCount < 2)
        {
            //Debug.LogWarning(CodeManager.GetMethodName() + string.Format("<color=yellow>Level {0} : Board Count : {1}</color>", m_dataManager.CurrentLevelData.Key, State.BoardCount));
            return true;
        }

        int currentLevel = State.CurrentLevel;
        int boardIndex = 1; // Second Board
		//int boardIndex = State.BoardIndex; // Current Showing Board

		if (m_dataManager.TryCopyBoardData(boardIndex, out int toLevel, out Board boardToCopy))
		{
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Level {0} : Seperate Board [{1}] -> Level {2}</color>", m_dataManager.CurrentLevelData.Key, boardIndex, toLevel));
            
            await CreateLevel(toLevel, boardToCopy);
            await LoadLevel(currentLevel);
            RemoveBoard(boardIndex);
            await SaveLevel();

            return await SeperateBoard();
		}

        Debug.LogWarning(CodeManager.GetMethodName() + string.Format("<color=yellow>Level {0} : Seperate Board Failed</color>", m_dataManager.CurrentLevelData.Key));

        return false;
	}

    public async UniTask SeperateBoardAll()
	{
        int currentLevel = State.CurrentLevel;
        int lastLevel = State.LastLevel;
        //int lastLevel = 3; // For Test

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Seperate Start ({0})</color>", lastLevel));

        int completeCount = 0;
        for(int level=1; level <= lastLevel; level++)
        {
            await LoadLevel(level);

            if(await SeperateBoard())
                completeCount++;
        }

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Seperate Complete ({0}/{1})</color>", completeCount, lastLevel));

        await LoadLevel(currentLevel); 
	}

	private void LoadBoardInternal(int index, int boardCount)
	{
		(_, float size, _) = m_brushInfo.Value;

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
		if (info is TileBrushInfo tileBrush)
		{
			float snapping = tileBrush.Snapping;
			Vector2 positionWithSnapping = new Vector2(
				Mathf.RoundToInt(x / snapping) * snapping, 
				Mathf.RoundToInt(y / snapping) * snapping
			);

			m_brushInfo.Value = tileBrush with { Position = positionWithSnapping };
		}
		else
		if (info is MissionStampInfo stamp)
		{
			if (!State.CurrentBoard?.HasIndex(stamp.LayerIndex) ?? true)
			{
				return;
			}

			TileInfo nearby = State.CurrentBoard[stamp.LayerIndex].Tiles.OrderByDescending(tile => Vector2.Distance(inputPosition, tile.Position)).Last();

			// 근접했을 경우만 
			Bounds inputBounds = new Bounds(inputPosition, Vector2.one * nearby.Size);
			Bounds nearbyBounds = new Bounds(nearby.Position, Vector2.one * nearby.Size);

			m_brushInfo.Value = stamp with {
				TileGuid = nearby.Guid,
				NearBy = nearby.Position, 
				Position = inputBounds.Intersects(nearbyBounds)? nearby.Position : inputPosition 
			};
		}
	}

	public void ChangeSnapping(float snapping)
	{
		m_cachedTileBrushOptions.snapping = snapping;

		if (m_brushInfo.Value is TileBrushInfo info)
		{
			m_brushInfo.Value = info with { Snapping = snapping };
		}
	}
	#endregion
	
	#region Tile
	public void SetTileInLayer(InputController.State state)
	{
		(_, float size, Vector2 position) = m_brushInfo.Value;
		Bounds bounds = new Bounds(position, Vector2.one * size);
		int boardIndex = State.BoardIndex;

		if (m_brushInfo.Value is TileBrushInfo tileBrush)
		{		
			(_, _, _, DrawOrder drawOrder) = tileBrush;

			switch (state)
			{
				case LEFT_BUTTON_PRESSED:
				case LEFT_BUTTON_RELEASED:
					m_dataManager.TryAddTileData(drawOrder, boardIndex, bounds, out int layerIndex);
					if (m_invisibleLayerIndexes.Contains(layerIndex))
					{
						m_invisibleLayerIndexes.Remove(layerIndex);
					}
					break;
				case RIGHT_BUTTON_PRESSED:
				case RIGHT_BUTTON_RELEASED:
					m_dataManager.TryRemoveTileData(boardIndex, bounds);
					break;
			}
		}
		else
		if (m_brushInfo.Value is MissionStampInfo stamp)
		{
			m_dataManager.TrySetMissionTile(boardIndex, stamp.LayerIndex, stamp.TileGuid, state is LEFT_BUTTON_PRESSED or LEFT_BUTTON_RELEASED);
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
			SetNumberOfTileTypes(Mathf.Max(0, numbers + increment));
		}

	}

	public void SetNumberOfTileTypes(int value)
	{
		if (value < 0)
		{
			m_internalState.Update(info => 
				info with { 
					UpdateType = UpdateType.BOARD
				}
			);
			return;
		}

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
									return new BoardInfo(
										number.Value, 
										board.DifficultType, 
										board.MissionCount,
										board.GoldTileIcon,
										board.Layers	
									);
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
		(_, float size, _) = m_brushInfo.Value;
		
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
	}
	#endregion

	#region Layer
	public void RemoveLayer()
	{
		(_, float size, _) = m_brushInfo.Value;

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
		if (m_dataManager.CurrentLevelData is var data and not null)
		{
			int currentDifficult = data.Boards[State.BoardIndex].Difficult;
			int difficult = m_dataManager.SetDifficult(
				State.BoardIndex,
				Mathf.Clamp(currentDifficult + increment, 0, (int)DifficultType.HARD)
			) ?? (int)DifficultType.NORMAL;

			m_internalState.Update( 
				state => state with {
					UpdateType = UpdateType.DIFFICULT,
					Boards = State.Boards.Select(
						(board, index) => {
							if (State.BoardIndex == index)
							{
								return new BoardInfo(
									board.NumberOfTileTypes, 
									difficult, 
									board.MissionCount,
									board.GoldTileIcon,
									board.Layers	
								);
							}
							return board;
						}
					).ToArray()
				}
			);
		}
	}

	public void ChangeDrawOrder(DrawOrder order)
	{
		m_cachedTileBrushOptions.order = order;

		m_brushInfo.Update(
			info => {
				if (info is TileBrushInfo tileBrush)
				{
					return tileBrush with { DrawOrder = order };
				}

				return info;
			}
		);
	}

	public void PlayGame()
	{
		m_view.SetVisibleLoading(true);
		try
		{
			UniTask.Void(
				async token => {
					await m_dataManager.SaveTemporaryLevelData();
					int level = m_dataManager.CurrentLevelData.Key;
					string path = PlayerPrefs.GetString(Constant.Editor.DATA_PATH_KEY);
					PlayerPrefs.SetInt(Constant.Editor.LATEST_LEVEL_KEY, level);
					UnityEngine.SceneManagement.SceneManager.LoadScene("Game");

					m_view.SetVisibleLoading(false);
				}, m_cancellationTokenSource.Token
			);
		}
		catch
		{
			m_view.SetVisibleLoading(false);
		}
	}

	public void UpdateCurrentLevelMode(bool hardMode)
	{
		bool mode = m_dataManager.SetHardMode(hardMode);
		m_internalState.Update( 
			state => state with {
				UpdateType = UpdateType.DIFFICULT,
				HardMode = mode
			}
		);
	}

	public void OnChangeBrushMode(BrushMode mode)
	{
		(float snapping, DrawOrder order) = m_cachedTileBrushOptions;
		(_, float size, Vector2 position) = m_brushInfo.Value;

		switch (mode)
		{
			case BrushMode.MISSION_STAMP:
				if (m_brushInfo.Value is TileBrushInfo tileBrush)
				{
					m_cachedTileBrushOptions = (tileBrush.Snapping, tileBrush.DrawOrder);
				}
				OnPointToLayer(m_dataManager.GetLastLayerInCurrent(State.BoardIndex));
				break;
			default:
				m_invisibleLayerIndexes.Clear();
				m_brushInfo.Value = new TileBrushInfo(size, snapping, position, order);
				m_invisibleLayersBroker.Publish(InvisibleLayerIndexes);
				break;
		}
	}

	public void OnPointToLayer(int layerIndex)
	{
		m_invisibleLayerIndexes.Clear();
		for (int index = layerIndex + 1, count = State.CurrentBoard.Count; index < count; index++)
		{
			m_invisibleLayerIndexes.Add(index);
		}

		(_, float size, Vector2 position) = m_brushInfo.Value;
		m_brushInfo.Value = new MissionStampInfo(size, position, layerIndex);

		m_invisibleLayersBroker.Publish(InvisibleLayerIndexes);
	}

	public void SetVisibleLayer(int layerIndex, bool invisible)
	{
		if (invisible && !m_invisibleLayerIndexes.Contains(layerIndex))
		{
			m_invisibleLayerIndexes.Add(layerIndex);
		}
		else
		if (!invisible && m_invisibleLayerIndexes.Contains(layerIndex))
		{
			m_invisibleLayerIndexes.Remove(layerIndex);
		}

		m_invisibleLayersBroker.Publish(InvisibleLayerIndexes);
	}

	public void UpdateCountryCode(string code)
	{
		m_dataManager.UpdateCountryCode(code);
	}

	public UniTask RemoveTemporaryLevel()
	{
		int level = m_dataManager.CurrentLevelData.Key;
		m_dataManager.RemoveTemporaryLevelData(level);
		return LoadLevel(level, true);
	}

	public void IncrementMissionCount(int increment)
	{
		if (m_dataManager.CurrentLevelData is var data and not null)
		{
			int count = data?[State.BoardIndex]?.MissionTileCount ?? 0;
			SetMissionCount(Mathf.Max(0, count + increment));
		}
	}

	public void SetMissionCount(int value)
	{
		if (value < 0)
		{
			m_internalState.Update(info => 
				info with { 
					UpdateType = UpdateType.BOARD
				}
			);
			return;
		}

		if (m_dataManager.CurrentLevelData is var data and not null)
		{
			var result = m_dataManager.UpdateMissionTileCount(State.BoardIndex, value);

			if (result.HasValue)
			{
				m_internalState.Update(info => 
					info with { 
						UpdateType = UpdateType.BOARD,
						Boards = State.Boards.Select(
							(board, index) => {
								if (State.BoardIndex == index)
								{
									return new BoardInfo(
										board.NumberOfTileTypes, 
										board.DifficultType,
										result.Value,
										board.GoldTileIcon,
										board.Layers
									);
								}
								return board;
							}
						).ToArray()
					} 
				);
			}
		}
	}

	public void UpdateMissionTileIcon(int icon)
	{
		m_dataManager.UpdateMissionTileIcon(State.BoardIndex, icon);
		m_internalState.Update(info => 
			info with { 
				UpdateType = UpdateType.BOARD,
				Boards = State.Boards.Select(
					(board, index) => {
						if (State.BoardIndex == index)
						{
							return new BoardInfo(
								board.NumberOfTileTypes, 
								board.DifficultType,
								board.MissionCount,
								icon,
								board.Layers
							);
						}
						return board;
					}
				).ToArray()
			} 
		);
	}
}
