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
    private List<ICommand> commandParams = new List<ICommand>();

	public Command.Invoker Invoker => m_commandInvoker;


	public void OnProcess(TileItemModel tileItemModel)
	{
        commandParams.Clear();

        switch(tileItemModel.Location)
        {
            case LocationType.STASH:
                var icdListStash = tileItemModel.FindTilesToChangeICD();
                AddCommandTile(tileItemModel, Type.MOVE_TILE_IN_STASH_TO_BASKET, LocationType.BASKET);
                AddCommandList(icdListStash, Type.CHANGE_BLOCKER_ICD, LocationType.BOARD);
                break;
            case LocationType.BOARD:
                var icdListBoard = tileItemModel.FindTilesToChangeICD();
                
                switch(tileItemModel.BlockerType)
                {
                    case BlockerType.Glue_Left:
                        var(existRight, rightTile) = tileItemModel.FindRightTile();
                        if (existRight)
                        {
                            icdListBoard.AddRange(rightTile.FindTilesToChangeICD().Where(targetTileModel => {
                                return !icdListBoard.Contains(targetTileModel);
                            }));

                            AddCommandTile(tileItemModel, Type.MOVE_TILE_IN_BOARD_TO_BASKET, LocationType.BASKET);
                            AddCommandTile(rightTile, Type.MOVE_TILE_IN_BOARD_TO_BASKET, LocationType.BASKET);
                        }
                        break;
                    case BlockerType.Glue_Right:
                        var(existLeft, leftTile) = tileItemModel.FindLeftTile();
                        if (existLeft)
                        {
                            icdListBoard.AddRange(leftTile.FindTilesToChangeICD().Where(targetTileModel => {
                                return !icdListBoard.Contains(targetTileModel);
                            }));

                            AddCommandTile(leftTile, Type.MOVE_TILE_IN_BOARD_TO_BASKET, LocationType.BASKET);
                            AddCommandTile(tileItemModel, Type.MOVE_TILE_IN_BOARD_TO_BASKET, LocationType.BASKET);
                        }
                        break;

                    default:
                        AddCommandTile(tileItemModel, Type.MOVE_TILE_IN_BOARD_TO_BASKET, LocationType.BASKET);
                        break;
                }
                
                AddCommandList(icdListBoard, Type.CHANGE_BLOCKER_ICD, LocationType.BOARD);
                break;
            default:
                commandParams.Add(DoNothing<Resource>.Command);
                break;
        }

        ConcurrentCommand concurrentCommand = new ConcurrentCommand(commandParams.ToArray());

		m_commandInvoker.SetCommand(concurrentCommand);
		m_commandInvoker.Execute();

		#region Local Functions
        void AddCommandList(List<TileItemModel> list, Type type, LocationType location)
        {
            list.ForEach(tile => {
                AddCommandTile(tile, type, location);
            });
        }

        void AddCommandTile(TileItemModel tile, Type type, LocationType location)
        {
            commandParams.Add(new GameCommand<Resource>(m_receiver, CreateResource(type, tile, location, tile.BlockerType, tile.BlockerICD)));
        }

		Resource CreateResource(Type type, TileItemModel targetModel, LocationType location, BlockerType blockerType, int blockerICD)
        {
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
                        if (changeCount > 0) // [Undo] ICD 증가.
                        {
                            switch(tile.BlockerType)
                            {
                                case BlockerType.Jelly:
                                case BlockerType.Bush:
                                case BlockerType.Chain:
                                    newICD = Mathf.Min(GlobalDefine.GetBlockerICD(tile.BlockerType, oldICD), oldICD + changeCount);
                                    break;
                                case BlockerType.Suitcase:
                                    newICD = Mathf.Min(tile.IconList.Count, oldICD + changeCount);
                                    break;
                                default:
                                    newICD = oldICD;
                                    break;
                            }
                        }
                        else // [Basket] ICD 감소.
                        {
                            switch(tile.BlockerType)
                            {
                                case BlockerType.Jelly: // Jelly는 ICD 음수 가능.
                                    newICD = oldICD + changeCount;
                                    break;
                                case BlockerType.Bush:
                                case BlockerType.Chain:
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
                return (tile.Location is LocationType.BOARD) && (tile.BlockerType is BlockerType.None);
            })
            .Select(tile => tile.Icon).ToList();
		Queue<int> queue = new Queue<int>(types.Shuffle());

		// 랜덤 타입 타일의 타입을 임의적으로 설정한다.
		List<TileItemModel> tileItemModels = BoardInfo
			.CurrentBoard
			.Tiles
			.Select(
				tile => {
					if ((tile.Location is LocationType.BOARD) && (tile.BlockerType is BlockerType.None))
					{
                        List<int> iconList = new List<int>
                        {
                            queue.Dequeue()
                        };
                        
                        return tile with { Icon = iconList.Last(), IconList = iconList };
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
