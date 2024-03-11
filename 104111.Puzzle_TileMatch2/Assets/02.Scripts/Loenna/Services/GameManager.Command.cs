using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using Resource = CommandResource.PlayScene;
using Type = CommandType.PlayScene;

partial class GameManager
{
   	private readonly Command.Invoker m_commandInvoker;
	private readonly Command.Receiver<Resource> m_receiver;

	public Command.Invoker Invoker => m_commandInvoker;

	public void OnProcess(TileItemModel tileItemModel)
	{
        List<ICommand> commandParams = new List<ICommand>();

        switch(tileItemModel.Location)
        {
            case LocationType.STASH:
                commandParams.Add(new GameCommand<Resource>(m_receiver, CreateResource(Type.MOVE_TILE_IN_STASH_TO_BASKET, tileItemModel, LocationType.BASKET, tileItemModel.BlockerType, tileItemModel.BlockerICD)));
                break;
            case LocationType.BOARD:
                tileItemModel.FindTilesToChangeICD().ForEach(tagetTileModel => {
                    commandParams.Add(new GameCommand<Resource>(m_receiver, CreateResource(Type.CHANGE_BLOCKER_ICD, tagetTileModel, LocationType.BOARD, tagetTileModel.BlockerType, tagetTileModel.BlockerICD)));
                });
                commandParams.Add(new GameCommand<Resource>(m_receiver, CreateResource(Type.MOVE_TILE_IN_BOARD_TO_BASKET, tileItemModel, LocationType.BASKET, tileItemModel.BlockerType, tileItemModel.BlockerICD)));
                break;
            default:
                commandParams.Add(DoNothing<Resource>.Command);
                break;
        }

        ConcurrentCommand concurrentCommand = new ConcurrentCommand(commandParams.ToArray());

		/*ConcurrentCommand concurrentCommand = new ConcurrentCommand(
			tileItemModel.Location switch {
				LocationType.STASH => 
                    new GameCommand<Resource>(m_receiver, CreateResource(Type.MOVE_TILE_IN_STASH_TO_BASKET, LocationType.BASKET, tileItemModel.BlockerType, tileItemModel.BlockerICD)),
				LocationType.BOARD => 
                    new GameCommand<Resource>(m_receiver, CreateResource(Type.MOVE_TILE_IN_BOARD_TO_BASKET, LocationType.BASKET, tileItemModel.BlockerType, tileItemModel.BlockerICD)),
				_ => DoNothing<Resource>.Command
			}
		);*/

        m_commandInvoker.SetCommand(concurrentCommand);
		m_commandInvoker.Execute();

		#region Local Functions
		Resource CreateResource(Type type, TileItemModel targetModel, LocationType location, BlockerType blockerType, int blockerICD)
        {
            //Debug.Log(CodeManager.GetMethodName() + string.Format("[{0}] {1} : {2}", type, blockerType, blockerICD));
            return Resource.CreateCommand(type, targetModel, location, blockerType, blockerICD);
        }
		#endregion
	}

    public void ChangeICD(TileItemModel tileItemModel, List<TileItemModel> tiles, int changeCount = -1)
	{
		var modifiedTiles = tiles
			.Select(
				tile => {
					var overlapped = tile.
						Overlaps
						.Select(
							x => {
								if (x.guid == tileItemModel.Guid)
								{
									return (x.guid, true, x.distance);
								}
								return x;
							}
						).ToList();

                    int oldICD = tile.BlockerICD;
                    int newICD = 0;
                    if (tile.Guid == tileItemModel.Guid)
                    {
                        if (changeCount > 0)
                        {
                            switch(tile.BlockerType)
                            {
                                case BlockerType.Bush:
                                case BlockerType.Chain:
                                case BlockerType.Jelly:
                                    newICD = Mathf.Min(GlobalDefine.GetBlockerICD(tile.BlockerType, oldICD), oldICD + changeCount);
                                    break;
                                case BlockerType.Suitcase:
                                    newICD = Mathf.Min(tile.FindTileItem().iconList.Count, oldICD + changeCount);
                                    break;
                                default:
                                    newICD = oldICD;
                                    break;
                            }
                        }
                        else
                        {
                            switch(tile.BlockerType)
                            {
                                case BlockerType.Bush:
                                case BlockerType.Chain:
                                case BlockerType.Jelly:
                                case BlockerType.Suitcase:
                                    newICD = Mathf.Max(0, oldICD + changeCount);
                                    break;
                                default:
                                    newICD = oldICD;
                                    break;
                            }
                        }
                    }
					
					return tile with {
						Overlaps = overlapped,
                        BlockerICD = tile.Guid == tileItemModel.Guid ? newICD : oldICD
					};
				}
			).ToList();
		
		BoardInfo.CurrentBoard.Tiles = modifiedTiles;

		var currentBasket = BoardInfo.Basket;
		currentBasket.RemoveAll(b => tiles.Any(tile => tile.Location is LocationType.BOARD && tile.Guid == b.Guid));

		m_boardInfo.Update(
			info => info with { 
				StateType = InternalState.Type.CurrentUpdated,
				Basket = currentBasket,
				SelectedTiles = new TileItemModel[] { tileItemModel }
			}
		);
	}

	// 해당 타일 이동
	public List<TileItemModel> MoveTo(TileItemModel tileItemModel, LocationType location)
	{
        //SoundManager soundManager = Game.Inst?.Get<SoundManager>();
        //soundManager?.PlayFx(Constant.Sound.SFX_TILE_MOVE);

		var tiles = BoardInfo.CurrentBoard.Tiles
			.Select(tile => {
				if (tile.Guid == tileItemModel.Guid)
				{
					if (location is LocationType.BASKET && tile.GoldPuzzleCount > 0)
					{
						m_missionCollectedFx.Update(_ => tileItemModel.Position);
						m_collectedMissionCount.Update(count => count + 1);
					}

					return tile with {
						Location = location,
						GoldPuzzleCount = -1
					};
				}

				if (!tile.Overlapped && tile.GoldPuzzleCount > 0 && location is LocationType.BASKET or LocationType.POOL)
				{
					return tile with { GoldPuzzleCount = tile.GoldPuzzleCount - 1};
				}

				return tile;
			}
		);


		return tiles.ToList();
	}

	public void AddToStash(TileItemModel tileItemModel, List<TileItemModel> tiles)
	{
		BoardInfo.CurrentBoard.Tiles = tiles;
		var currentBasket = BoardInfo.Basket;
		currentBasket.RemoveAll(b => tiles.Any(tile => tile.Location is LocationType.STASH && tile.Guid == b.Guid));

		var stash = BoardInfo.Stash;
		stash.Add(tileItemModel.Guid);

		m_boardInfo.Update(
			info => info with { 
				StateType = InternalState.Type.CurrentUpdated,
				Basket = currentBasket,
				Stash = stash,
				SelectedTiles = new TileItemModel[] { tileItemModel }
			}
		);
	}

	public void AddToBoard(TileItemModel tileItemModel, List<TileItemModel> tiles)
	{
		var modifiedTiles = tiles
			.Select(
				tile => {
					var overlapped = tile.
						Overlaps
						.Select(
							x => {
								if (x.guid == tileItemModel.Guid)
								{
									return (x.guid, true, x.distance);
								}
								return x;
							}
						).ToList();
					
					return tile with {
						Overlaps = overlapped
					};
				}
			).ToList();
		
		BoardInfo.CurrentBoard.Tiles = modifiedTiles;

		var currentBasket = BoardInfo.Basket;
		currentBasket.RemoveAll(b => tiles.Any(tile => tile.Location is LocationType.BOARD && tile.Guid == b.Guid));

		m_boardInfo.Update(
			info => info with { 
				StateType = InternalState.Type.CurrentUpdated,
				Basket = currentBasket,
				SelectedTiles = new TileItemModel[] { tileItemModel }
			}
		);
	}

	public void AddToBasket(TileItemModel tileItemModel, List<TileItemModel> tiles)
	{
        var modifiedTiles = tiles
			.Select(
				tile => {
					var overlapped = tile.
						Overlaps
						.Select(
							x => {
								if (x.guid == tileItemModel.Guid)
								{
									return (x.guid, false, x.distance);
								}
								return x;
							}
						).ToList();
					
					return tile with {
						Overlaps = overlapped
					};
				}
			).ToList();

		BoardInfo.CurrentBoard.Tiles = modifiedTiles;

		var currentBasket = BoardInfo.Basket;
		int index = currentBasket.FindLastIndex(tuple => tuple.Icon == tileItemModel.Icon);
		var result = (tileItemModel.Guid, tileItemModel.Icon);
		int insertIndex = index < 0 ? currentBasket.Count : index + 1;
		currentBasket.Insert(insertIndex, result);

		var currentStash = BoardInfo.Stash;
		currentStash.RemoveAll(guid => guid == tileItemModel.Guid);

		if (CheckTilesInBasket(out int startAt))
		{
            var removed = currentBasket.GetRange(startAt, Constant.Game.REQUIRED_MATCH_COUNT);

			var tileItems = BoardInfo.CurrentBoard.Tiles.Select(
				tile => {
					if (removed.Any(r => r.Guid == tile.Guid))
					{
						tile = tile with { Location = LocationType.POOL };
					}
					return tile;
				}
			);

			BoardInfo.CurrentBoard.Tiles = tileItems.ToList();

			currentBasket.RemoveRange(startAt, 3);

            m_boardInfo.Update(info =>
				info with {
					StateType = InternalState.Type.CurrentUpdated,
					Basket = currentBasket,
					Stash = currentStash,
					SelectedTiles = new TileItemModel[] { tileItemModel }
				}
			);

			m_commandInvoker.ClearHistories();
			return;
		}

		m_boardInfo.Update(info =>
			info with { 
				StateType = InternalState.Type.CurrentUpdated,
				Basket = currentBasket,
				Stash = currentStash,
				SelectedTiles = new TileItemModel[] { tileItemModel }
			}
		);

		bool CheckTilesInBasket(out int removeStartAt)
		{
			var removeIndexes = currentBasket.Where(item => item.Icon == tileItemModel.Icon).Select((_, index) => index);

			if (removeIndexes?.Count() >= Constant.Game.REQUIRED_MATCH_COUNT)
			{
				removeStartAt = currentBasket.FindIndex(x => x.Icon == tileItemModel.Icon);
				if (removeStartAt >= 0)
				{
                    //Debug.Log(CodeManager.GetMethodName() + string.Format("removeStartAt : {0}", removeStartAt));
					return true;
				}
			}

			removeStartAt = -1;
			return false;
		}
	}

    public void ChangeBlockerICD(TileItemModel tileItemModel, List<TileItemModel> tiles)
    {
        //
    }

    public bool IsBasketEnable()
    {
        return GetBasketCount() < Constant.Game.MAX_BASKET_AMOUNT;
    }

    public int GetBasketCount()
    {
        return BoardInfo.Basket.Count;
    }

	#region Private Methods
	public void UseStash()
	{
		// 스태시로 들어간 순간 타일의 원 위치 데이터가 바뀐다.
		var currentStash = BoardInfo.Stash;
		var currentBasket = BoardInfo.Basket;
		int count = Mathf.Min(Constant.Game.STASH_TILE_AMOUNT, currentBasket.Count);
		var stashItems = currentBasket.GetRange(0, count).Select(x => x.Guid);
		currentBasket.RemoveRange(0, count);
		currentStash.AddRange(stashItems);

		//Debug.Log(new StringBuilder().AppendJoin(", ", currentStash));

		var tiles = BoardInfo.CurrentBoard.Tiles
			.Select(tile => {
				if (currentStash.Contains(tile.Guid))
				{
					tile = tile with { Location = LocationType.STASH };
				}
				return tile;
			}
		).ToList();

		BoardInfo.CurrentBoard.Tiles = tiles;

		m_boardInfo.Update(
			info => info with { 
				StateType = InternalState.Type.CurrentUpdated,
				Basket = currentBasket,
				Stash = currentStash,
				SelectedTiles = BoardInfo.CurrentBoard.Tiles.Where(tile => stashItems.Contains(tile.Guid)).ToArray()
			}
		);

		m_commandInvoker.ClearHistories();
	}

	private void Shuffle()
	{
        IList<int> types = BoardInfo.CurrentBoard.Tiles
            .Where(tile => {
                return (tile.Location is LocationType.BOARD) 
                    && (tile.BlockerType is BlockerType.None);
            })
            .Select(tile => tile.Icon).ToList();
		Queue<int> queue = new Queue<int>(types.Shuffle());

		// 랜덤 타입 타일의 타입을 임의적으로 설정한다.
		List<TileItemModel> tileItemModels = BoardInfo
			.CurrentBoard
			.Tiles
			.Select(
				tile => {
					if ((tile.Location is LocationType.BOARD) 
                     && (tile.BlockerType is BlockerType.None))
					{
						return tile with { Icon = queue.Dequeue() };
					}
					return tile;
				}
			).ToList();

		BoardItemModel current = new BoardItemModel ( index: BoardInfo.CurrentBoardIndex, layerCount: BoardInfo.CurrentBoard.LayerCount, tiles: tileItemModels );
	
		m_boardInfo.Update(
			info => info with { 
				StateType = InternalState.Type.CurrentUpdated, 
				CurrentBoard = current, 
				SelectedTiles = Array.Empty<TileItemModel>() 
			}
		);

		m_commandInvoker.ClearHistories();
	}
	#endregion

    public void EventCollect_SweetHolic(Transform from)
    {
        int addCount = GlobalData.Instance.eventSweetHolic_IsBoosterTime ? 2 : 1;

        GlobalData.Instance.eventSweetHolic_GetCount += addCount;

        m_sweetHolicCollectedFx.Update(_ => from);
        m_collectedSweetHolicCount.Update(count => count + addCount);
    }
}
