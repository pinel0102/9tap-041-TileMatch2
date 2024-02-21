using UnityEngine;

using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public partial class GameManager : IDisposable
{
	#region Fields
	private readonly UserManager m_userManager;
	private readonly TableManager m_tableManager;
	private LevelDataTable LevelDataTable => m_tableManager?.LevelDataTable;
	private TileDataTable TileDataTable => m_tableManager?.TileDataTable;

	private readonly IDisposableAsyncReactiveProperty<InternalState> m_boardInfo;
	public IUniTaskAsyncEnumerable<InternalState> UpdatedInfo => m_boardInfo;
	private InternalState BoardInfo => m_boardInfo.Value;

	// 현재까지 모은 미션
    private readonly IDisposableAsyncReactiveProperty<int> m_collectedMissionCount;
    private readonly IDisposableAsyncReactiveProperty<int> m_collectedSweetHolicCount;
	private readonly IDisposableAsyncReactiveProperty<Vector2> m_missionCollectedFx;
    private readonly IDisposableAsyncReactiveProperty<Transform> m_sweetHolicCollectedFx;

	private int m_continueCount = 0;

	public IUniTaskAsyncEnumerable<(int count, Vector2 startPosition, int max)> MissionCollected 
	{
		get
		{
			return m_collectedMissionCount
				.WithoutCurrent()
				.CombineLatest(
					m_missionCollectedFx
					.WithoutCurrent(),
					(count, position) => (count, position, BoardInfo.MissionTileCount)
				);
		}
	}
    public IUniTaskAsyncEnumerable<(int count, Transform startPosition)> SweetHolicCollected 
	{
		get
		{
			return m_collectedSweetHolicCount
				.WithoutCurrent()
				.CombineLatest(
					m_sweetHolicCollectedFx
					.WithoutCurrent(),
					(count, position) => (count, position)
				);
		}
	}

	private readonly AsyncReactiveProperty<bool> m_basketNotEmpty;
	public IReadOnlyAsyncReactiveProperty<bool> BasketNotEmpty => m_basketNotEmpty;

	public int CurrentLevel => BoardInfo.Level;
	#endregion

	#region Constructors
	public GameManager(UserManager userManager, TableManager tableManager)
	{
		m_tableManager = tableManager;
		m_userManager = userManager;

		m_collectedMissionCount = new AsyncReactiveProperty<int>(0).WithDispatcher();
        m_collectedSweetHolicCount = new AsyncReactiveProperty<int>(0).WithDispatcher();
		m_missionCollectedFx = new AsyncReactiveProperty<Vector2>(Vector2.zero).WithDispatcher();
        m_sweetHolicCollectedFx = new AsyncReactiveProperty<Transform>(null).WithDispatcher();
        
		m_basketNotEmpty = new(false);

		m_boardInfo = new AsyncReactiveProperty<InternalState>(InternalState.Empty)
			.WithBeforeSetValue(
				(old, value) => {
					if (
						old.StateType is InternalState.Type.NotUpdated &&
						value.StateType is InternalState.Type.Finished or InternalState.Type.NotUpdated
					)
					{
						return value with { StateType = InternalState.Type.NotUpdated };
					}

					return value;
				}
			)
			.WithAfterSetValue(
				value => {
					m_basketNotEmpty.Value = value.Basket.Count > 0;

					if (
						value.StateType is InternalState.Type.Initialized or 
						InternalState.Type.NotUpdated or
						InternalState.Type.Finished
					)
					{
						return;
					}

					(int remainTileCount, bool next, int countInBasket) tuple = (value.RemainedTileCount, value.HasNext, value.Basket.Count);

					switch (tuple)
					{
						case (<= 0, true, _):
							BoardItemModel next = BoardInfo.StandByBoards.Dequeue();
							m_boardInfo.Update(
								info => value with { 
									StateType = InternalState.Type.BoardChanged,
									CurrentBoardIndex = info.CurrentBoardIndex + 1,
									CurrentBoard = next,
									HasNext = info.StandByBoards.Count() > 0
								}
							);
							break;
						case (<= 0, false, _):
						case (_, _, >= Constant.Game.MAX_BASKET_AMOUNT):
							var state = m_boardInfo.Update(
								info => value with {
									StateType = InternalState.Type.Finished
								}
							);

							// 보상 팝업 뜨기 전에 앱이 종료되면 보상을 받을 수 없게 되므로 여기서 보상까지 다 받는다.
							if(state.StateType is InternalState.Type.Finished)
							{
								CurrentPlayState.Finished finished = state.ToCurrentState() as CurrentPlayState.Finished;
								if (finished.Result is CurrentPlayState.Finished.State.CLEAR)
								{
                                    CheckClearRewards();
								}
							}
							break;
					}
				}
			)
			.WithDispatcher();

		m_receiver = new PlaySceneCommandReceiver(this);
		m_commandInvoker = new Command.Invoker();
	}

    public void CheckClearRewards()
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("Level {0}", CurrentLevel));

        GlobalData.Instance.SetOldItems(m_userManager.Current.Coin, m_userManager.Current.Puzzle);

        RewardDataTable rewardDataTable = m_tableManager.RewardDataTable;
        Dictionary<ProductType, long> collectRewardAll = new Dictionary<ProductType, long>();

        RewardData rewardData = rewardDataTable.GetDefaultReward(BoardInfo.HardMode);
        foreach(var item in rewardData.Rewards)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Clear Reward] {0} x {1}</color>", item.Type, item.GetAmount()));
        }
        AddRewards(collectRewardAll, rewardData.Rewards);

        if (rewardDataTable.TryPreparedChestReward(CurrentLevel, out var chestRewardData) && CurrentLevel >= chestRewardData.Level!)
        {
            foreach(var item in chestRewardData.Rewards)
            {
                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Chest Reward] {0} x {1}</color>", item.Type, item.GetAmount()));
            }
            AddRewards(collectRewardAll, chestRewardData.Rewards);
        }

        m_userManager.Update(level: CurrentLevel + 1, collectRewardAll);
        GlobalData.Instance.CURRENT_LEVEL = m_userManager.Current.Level;
    }

    void AddRewards(Dictionary<ProductType, long> dict, List<IReward> rewards)
    {
        foreach (var reward in rewards)
        {
            if (!dict.ContainsKey(reward.Type))
            {
                dict.Add(reward.Type, 0);
            }
            dict[reward.Type] += reward.GetAmount();
        }
    }
	#endregion

	#region IDisposable Interface
	public void Dispose()
	{
		m_boardInfo?.Dispose();
		m_basketNotEmpty?.Dispose();
		m_collectedMissionCount?.Dispose();
        m_collectedSweetHolicCount?.Dispose();
	}
	#endregion

	#region Public Methods
	public void LoadLevel(int level, Transform boardTransform)
	{
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0}</color>", level));

        GlobalData.Instance.playScene.ResetParticlePool();
        GlobalData.Instance.playScene.bottomView.RefreshSkillLocked(level);
        GlobalData.Instance.CURRENT_SCENE = GlobalDefine.SCENE_PLAY;
        GlobalData.Instance.CURRENT_LEVEL = level;
        GlobalData.Instance.eventSweetHolic_GetCount = 0;

        m_continueCount = 0;
		m_boardInfo.Update(info => InternalState.Empty);
		m_collectedMissionCount.Value = 0;
        m_collectedSweetHolicCount.Value = 0;

		// 로컬에 저장된 레벨 데이터를 불러온다
		if (!LevelDataTable.TryGetValue(level, out var levelData))
		{
			Debug.LogError("레벨 데이터 없음");
			return;
		}

        GlobalData.Instance.CURRENT_DIFFICULTY = levelData.HardMode ? 1 : 0;

		// 랜덤 타입 타일의 타입을 임의적으로 설정한다.
		int boardIndex = 0;
		var boards = new Queue<BoardItemModel>();

		if (levelData?.Boards == null)
		{
			Debug.LogError("레벨 맵 데이터 없음");
			return;
		}

		int missionCount = 0;

		foreach (Board board in levelData.Boards)
		{
			if (board?.Layers?.Count <= 0)
			{
				continue;
			}

			List<TileItemModel> tileItemModelAll = CreateTileItemModels(levelData.Key, levelData.CountryCode, board);
			missionCount += board.MissionTileCount;

			if (tileItemModelAll.Count > 0)
			{
				boards.Enqueue(new BoardItemModel(index: boardIndex++, layerCount: board.Layers.Count, tiles: tileItemModelAll));
			}
		}

		int boardCount = boards.Count;

		if (boards.Count == 0)
		{
			return;
		}

		BoardItemModel current = boards.Dequeue();
		
		// 데이터를 맵에 뿌린다.
		m_boardInfo.Update(info => 
			new InternalState(
				Level: level,
				HardMode: levelData.HardMode,
				MissionTileCount: missionCount,
				StateType: InternalState.Type.Initialized,
				StandByBoards: boards,
				BoardCount: boardCount,
				CurrentBoardIndex: 0,
				SelectedTiles: null,
				CurrentBoard: current,
				HasNext: boards.Count() > 0,
				Stash: new(),
				Basket: new()
			)
		);

        m_userManager.UpdateLog(levelPlayCount: m_userManager.Current.LevelPlayCount + 1);
        SDKManager.SendAnalytics_I_Scene_Play();

        #region Local Functions
		List<TileItemModel> CreateTileItemModels(int level, string countryCode, Board board)
		{
			List<TileItemModel> tileItemModelAll = new();
			int missionTileCount = board.MissionTileCount;

			int goldTileIcon = board.MissionTileCount > 0? board.GoldTileIcon : 0;

			int tileCount = board.Layers.Sum(Layer => Layer?.Tiles?.Count ?? 0);
			int requiredTypeCount = tileCount / Constant.Game.REQUIRED_MATCH_COUNT;
			int randomCount = Mathf.Clamp(board.NumberOfTileTypes, 1, requiredTypeCount);

			var randomIcons = TileDataTable
				.GetIndexes(level, countryCode)
				.Shuffle()
				.Where(index => index != goldTileIcon)
				.Take(goldTileIcon > 0? randomCount -1 : randomCount)
				.ToList();
			
			if (goldTileIcon > 0)
			{
				randomIcons.Insert(0, goldTileIcon);
			}

			List<int> randoms = new List<int>();
			for (int i = 0, count = requiredTypeCount; i < count; i++)
			{
				randoms.Add(randomIcons[i % randomCount]);
			}

			int requiredMissionWeight = Mathf.CeilToInt(board.MissionTileCount / (float)Constant.Game.REQUIRED_MATCH_COUNT);
			if (randoms.Count(icon => icon == goldTileIcon) is int missionCount)
			{
				if (missionCount < requiredMissionWeight)
				{
					int changeCount = requiredMissionWeight - missionCount;
					randoms = randoms.Select(
						icon => {
							if (icon != goldTileIcon && changeCount > 0)
							{
								changeCount -= 1;
								return goldTileIcon;
							}

							return icon;
						}
					).ToList();
				}
			}

			List<int> list = new List<int>(randoms.Concat(randoms).Concat(randoms).ToArray());
			//StringBuilder builder = new StringBuilder();
			//builder.AppendJoin(", ", list);
			//Debug.Log(builder.ToString());

			if (board.Difficult is (int)DifficultType.NORMAL)
			{
				list = list.Shuffle().ToList();
			}

			if (board.Difficult is (int)DifficultType.EASY)
			{
				int randomRepeat = UnityEngine.Random.Range(randomCount, requiredTypeCount + 1);
				//Debug.Log(randomRepeat);
				int amount = Constant.Game.REQUIRED_MATCH_COUNT * randomRepeat;

				//Debug.Log(amount);

				var sorting = list.GetRange(0, amount).OrderBy(icon => icon);
				var sub = amount >= list.Count? Array.Empty<int>() : list.GetRange(amount, list.Count - amount).Shuffle();

				list = sorting.Concat(sub).ToList();
			}

			//builder.Clear();
			//builder.AppendJoin(", ", list);
			//Debug.Log(builder.ToString());

			var queue = new Queue<int>(list);

		
			for (int layerIndex = board.Layers.Count() - 1; layerIndex >= 0; layerIndex--)
			{
				Layer layer = board.Layers[layerIndex];
				if (layer.Tiles?.Count <= 0)
				{
					continue;
				}

				var tileItemModels = layer.Tiles
					.OrderBy(tile => tile.Position, new TilePositionComparer())
					.Select(
						tile =>
						{
							var overlaps = tileItemModelAll
								.Where(
									item =>
									{
										var resizePosition = tile.Position * Constant.Game.RESIZE_TILE_RATIOS;
										return item.LayerIndex > layerIndex && Vector2.SqrMagnitude(resizePosition - item.Position) < 7700f;
									}
								).Select(item => {
										var resizePosition = tile.Position * Constant.Game.RESIZE_TILE_RATIOS;
										return (item.Guid, true, Vector2.SqrMagnitude(resizePosition - item.Position));
									}
								)
								.ToList();

							int icon = queue.Dequeue();
							int mission = -1;
							if (icon == goldTileIcon && missionTileCount > 0)
							{
								mission = Constant.Game.GOLD_PUZZLE_PIECE_COUNT;
								missionTileCount -= 1;
							}

							return new TileItemModel(
								layerIndex,
								LocationType.BOARD,
								tile.Guid,
								icon,
								tile.Position * Constant.Game.RESIZE_TILE_RATIOS,
								mission,
								overlaps
							);
						}
					);

				tileItemModelAll.AddRange(tileItemModels);
			}

			return tileItemModelAll;
		}
		#endregion
	}
	#endregion

	// 게임 스킬 아이템 사용
	public void UseSkillItem(SkillItemType skillItemType, bool checkOwned, Action<int> onShowBuyPopup = null)
	{
		if (checkOwned)
		{
			var ownSkillItems = m_userManager.Current.OwnSkillItems;

			bool available = false;

			if (ownSkillItems.TryGetValue(skillItemType, out int count))
			{
				if (count > 0)
				{
					ownSkillItems[skillItemType] = Mathf.Max(0, --count);
					m_userManager.Update(ownSkillItems: ownSkillItems);
					available = true;
				}
			}

			if (!available)
			{
				onShowBuyPopup?.Invoke((int)skillItemType);
				return;
			}
		}

		UseSkill();

		void UseSkill()
		{
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Use Item : {0}</color>", skillItemType));
            SoundManager soundManager = Game.Inst?.Get<SoundManager>();
            
			switch(skillItemType)
			{
				case SkillItemType.Stash: // 바구니에 있는 타일 3개를 보드에 둔다
                    soundManager?.PlayFx(Constant.Sound.SFX_ITEM_STASH);
                    SDKManager.SendAnalytics_C_Item_Use("Return", 1);
					UseStash();
					break;
				case SkillItemType.Undo: // 되돌리기
                    soundManager?.PlayFx(Constant.Sound.SFX_ITEM_UNDO);
                    SDKManager.SendAnalytics_C_Item_Use("Undo", 1);
					m_commandInvoker.UnExecute();
					break;
				case SkillItemType.Shuffle: // 섞기
                    soundManager?.PlayFx(Constant.Sound.SFX_ITEM_SHUFFLE);
                    SDKManager.SendAnalytics_C_Item_Use("Shuffle", 1);
					Shuffle();
                    ShuffleEffect();
					break;
			}
		}

        void ShuffleEffect()
        {
            GlobalData.Instance.SetTouchLock_PlayScene(true);

            var boardTiles = GlobalData.Instance.playScene.TileItems
                .Where(tileItem => tileItem.Current.Location is LocationType.BOARD).ToList();

            boardTiles.ForEach(tile => tile.ShuffleStart(GlobalData.Instance.shuffleRadiusMin, GlobalData.Instance.shuffleRadiusMax, GlobalData.Instance.shuffleSpeed));

            UniTask.Void(
                async () =>
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(GlobalData.Instance.shuffleTime));
                    boardTiles.ForEach(tile => tile.ShuffleStop());
                    await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
                    GlobalData.Instance.SetTouchLock_PlayScene(false);
                }
            );
        }
	}

	private class TilePositionComparer : IComparer<Vector2>
	{
		public int Compare(Vector2 v1, Vector2 v2)
		{
			var (x1, y1) = v1;
			var (x2, y2) = v2;

			int result = y2.CompareTo(y1);

			return result switch {
				0 => x1.CompareTo(x2),
				_ => result
			};
		}
	}

	public int GetSkillPackageCoin(bool checkCount, out List<SkillItemType> skillItemTypes)
	{
        if (checkCount)
		    m_continueCount += 1;

        Debug.Log(CodeManager.GetMethodName() + string.Format("Continue Count : {0}", m_continueCount));

		skillItemTypes = new();

		for (int i = 0; i < m_continueCount; i++)
		{
            SkillItemType type = (SkillItemType)(i + 1);
            switch(type)
            {
                case SkillItemType.Stash:
                case SkillItemType.Undo:
                case SkillItemType.Shuffle:
                    skillItemTypes.Add(type);
                    break;
            }
			
			if (i > 0)
			{
				skillItemTypes.Add(SkillItemType.VAT);
			}
		}

        /*for(int i=0; i < skillItemTypes.Count; i++)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", skillItemTypes[i], skillItemTypes[i].GetPaidCoin()));
        }*/

		return skillItemTypes.Sum(type => type.GetPaidCoin());
	}
}
