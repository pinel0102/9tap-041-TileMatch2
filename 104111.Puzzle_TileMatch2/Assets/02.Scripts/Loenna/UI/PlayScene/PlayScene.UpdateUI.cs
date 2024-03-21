using UnityEngine;
using UnityEngine.Pool;

using System;
using System.Linq;
using System.Collections.Generic;

using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks;

using DG.Tweening;

using NineTap.Constant;
using NineTap.Common;

partial class PlayScene
{
	private IObjectPool<TileItem> m_tileItemPool;
	private IObjectPool<MissionCollectedFx> m_particlePool;
	private List<TileItem> m_tileItems;
    public List<TileItem> TileItems => m_tileItems;
    private PuzzleData m_puzzleData;

    public List<TileItem> m_blockerList = new List<TileItem>();
    public List<BlockerType> m_blockerShownList = new List<BlockerType>();
    public bool blockerFailed;
    public bool BlockerFailed => blockerFailed;
    [SerializeField] private bool m_isFirstFail;
	private int m_progressId = 0;
	private Queue<UniTask> m_queue;

    public void ResetParticlePool()
    {
        for(int i = m_particleParent?.childCount - 1 ?? -1; i >= 0; i--)
        {
            Destroy(m_particleParent.GetChild(i).gameObject);
        }

        m_particlePool = new ObjectPool<MissionCollectedFx>(
			createFunc: () => {
				var item = Instantiate(ResourcePathAttribute.GetResource<MissionCollectedFx>());
				item.OnSetup();
				return item;
			},
			actionOnRelease: item => item.OnRelease()
		);
    }

	private void SetupInternal()
	{
		m_queue = new();
		
        m_tileItemPool = new ObjectPool<TileItem>(
			createFunc: () => {
				var item = Instantiate(ResourcePathAttribute.GetResource<TileItem>());
				item.OnSetup(
					new TileItemParameter
					{
						OnClick = item => {
                            if (!blockerFailed)
                            {
                                m_block.SetActive(true);
                                m_gameManager.OnProcess(item?.Current);
                            }
                        }
					}
				);
				return item;
			},
			actionOnGet: item => item.SetActive(true),
			actionOnRelease: item => item.Release(),
			actionOnDestroy: item => Destroy(item),
			collectionCheck: true,
			maxSize: 10000
		);

		m_gameManager.UpdatedInfo.Queue().SubscribeAwait(state => OnUpdateUI(state.ToCurrentState()));

        ResetParticlePool();

		m_gameManager.MissionCollected
		.Subscribe(
			value => {
				if (value.count <= 0)
				{
					return;
				}

                GlobalData.Instance.CreateEffect(
                    m_particlePool,
                    m_particleParent,
                    null,
                    Constant.Sound.SFX_GOLD_PIECE,
                    value.startPosition,
                    m_topView.PuzzleIconTransform.position,
                    onComplete: () => {
                        m_topView.UpdateMissionCount(value.count, value.max);
                    }
                );
			}
		);

        m_gameManager.SweetHolicCollected
		.Subscribe(
			value => {
                //GlobalData.Instance.eventSweetHolic = value.count;

				if (value.count <= 0)
				{
					return;
				}

                GlobalData.Instance.CreateEffect(
                    m_particlePool,
                    m_particleParent, 
                    GlobalDefine.GetSweetHolic_ItemImagePath(),
                    string.Empty,
                    value.startPosition,
                    m_topView.SweetHolicIconTransform,
                    0.75f,
                    onComplete: () => {
                        m_topView.UpdateSweetHolicCount();
                    }
                );
			}
		);
	}

	private void OnContinue(int coinAmount, List<SkillItemType> itemTypes, Action onStoreClosed)
	{
		if (m_userManager.TryUpdate(requireCoin: coinAmount))
		{
            SDKManager.SendAnalytics_C_Scene(Text.Button.CONTINUE);
            
            Dictionary<SkillItemType, int> addSkillItems = new();
            
            foreach (var itemType in itemTypes)
			{
                switch(itemType)
                {
                    case SkillItemType.Stash:
                        m_gameManager.UseSkillItem(itemType, false);
                        break;
                    case SkillItemType.Undo:
                    case SkillItemType.Shuffle:
                        addSkillItems.Add(itemType, 1);
                        break;
                }
			}

            if (addSkillItems.Count > 0)
            {
                GlobalDefine.GetItems(addSkillItems: addSkillItems);
            }

			UIManager.ClosePopupUI_ForceAll();
			m_block.SetActive(false);
		}
        else
        {
            //[PlayScene:Retry] 코인 부족.
            GlobalDefine.RequestAD_HideBanner();
            
            GlobalData.Instance.ShowStorePopup(onStoreClosed);
        }
	}

	private async UniTask OnUpdateUI(CurrentPlayState currentPlayState)
	{
        ++m_progressId;

        //Debug.Log(CodeManager.GetMethodName() + string.Format("currentPlayState : {0}", currentPlayState));

		switch (currentPlayState)
		{
			case CurrentPlayState.Initialized
			{
				Level: var level,
				HardMode: var hardMode,
				MissionTileCount: var includedMission,
				BoardCount: var count,
				CurrentBoardIndex: var index,
				CurrentBoard: var current
			}:
                Debug.Log(CodeManager.GetMethodName() + "Initialized");

                if (level < Constant.Game.LEVEL_PUZZLE_START)
                {
                    bg_default.SetActive(true);
                    bg_puzzle.SetActive(false);
                }
                else
                {
                    bg_default.SetActive(false);
                    bg_puzzle.SetActive(true);

                    m_puzzleData = m_puzzleDataTable.Dic.LastOrDefault(item => item.Value.Level <= level).Value;
                    SetBackground(m_puzzleData.GetGameImagePath());
                }

                m_isFirstFail = false;
                m_progressId = 0;
				m_block.SetActive(true);
				m_tileItems.ForEach(item => m_tileItemPool.Release(item));
				m_tileItems.Clear();
				m_canvasGroup.alpha = 1f;
				var tileItems = current
					.Tiles
					.Select(
						tile => {
							TileItem tileItem = m_tileItemPool.Get();
                            tileItem.OnUpdateUI(tile, true, out _);
							return tileItem;
						}
					).ToList();
				m_tileItems.AddRange(tileItems);
				m_bottomView.BasketView.Clear();
				m_bottomView.StashView.Clear();
				m_topView.BoardCountView.OnSetup(index, count);
				m_topView.OnUpdateUI(level, hardMode, includedMission > 0);
				m_topView.UpdateMissionCount(0, includedMission);
				await m_mainView.OnUpdateAll(current.LayerCount, m_tileItems);
				tileItems.ForEach(tileItem => tileItem.OnUpdateUI(tileItem.Current, false, out _));
                
                m_bottomView.BasketView.level = level;
                m_bottomView.BasketView.isTutorialLevel = GlobalDefine.IsTutorialLevel(level);
                m_bottomView.BasketView.isTutorialShowed = false;
                m_bottomView.BasketView.tutorialCheckCount = 0;

                m_blockerList.Clear();
                m_blockerList = gameManager.CurrentBlockerList();

                m_blockerShownList.Clear();

                if (GlobalDefine.IsEnablePuzzleOpenPopup())
                {
                    await GlobalData.Instance.ShowPuzzleOpenPopup();
                    
                    m_block.SetActive(true);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
                }

                if (m_bottomView.BasketView.isTutorialLevel)
                {
                    m_bottomView.BasketView.tutorialCheckCount = GlobalDefine.GetTutorialCheckCount(level);
                    m_bottomView.BasketView.CheckTutorialBasket();
                }
                else 
                {
                    if (m_blockerList.Count > 0)
                    {
                        CheckTutorialBlocker(m_blockerList);
                    }
                }

                m_block.SetActive(false);

				break;
			case CurrentPlayState.CurrentUpdated { SelectedTiles: var selectedTiles, CurrentBoard: var board, Basket: var basket, Stash: var stashes }:
				UniTask.Void(
					async token => {
                        //Debug.Log(CodeManager.GetMethodName() + "CurrentUpdated");

                        m_block.SetActive(true);

                        bool start = m_queue.Count <= 0;
                        m_tileItems.ForEach(
                            item => {
                                TileItemModel model = board.Tiles.FirstOrDefault(tile => item.Current?.Guid == tile.Guid);
                                if (model == null)
                                {
                                    return;
                                }

                                item.OnUpdateUI(model, false, out var current);
                            }
                        );
                
                        var stashItems = m_tileItems
                            .Where(tileItem => tileItem.Current.Location is LocationType.STASH)
                            .OrderBy(tileItem => tileItem.basketIndex)
                            .ToArray();
                        
                        await m_bottomView.StashView.OnUpdateUI(stashItems);

                        if (selectedTiles?.Count() > 0)
                        {
                            var enumerable = selectedTiles.ToUniTaskAsyncEnumerable();
                            await UniTask.Defer(
                                () => enumerable.ForEachAwaitAsync(
                                    async itemModel => {
                                        var selectedItem = m_tileItems.FirstOrDefault(item => item.Current?.Guid == itemModel.Guid);
                                        switch (selectedItem.Current?.Location)
                                        {
                                            case LocationType.BOARD:
                                                m_mainView.CurrentBoard.UpdateLayer(itemModel.LayerIndex, selectedItem);
                                                break;
                                            case LocationType.POOL or LocationType.BASKET:
                                                await m_bottomView.BasketView.OnAddItemUI(selectedItem);
                                                break;
                                        }
                                    }
                                )
                            );
                        }

                        await UniTask.Defer(() => m_bottomView.BasketView.OnRemoveItemUI(board.Tiles, basket));

                        m_blockerList = gameManager.CurrentBlockerList();
                        
                        if (m_bottomView.BasketView.isTutorialLevel)
                        {
                            m_bottomView.BasketView.CheckTutorialBasket();
                        }
                        else
                        {
                            if (m_blockerList.Count > 0)
                            {
                                CheckTutorialBlocker(m_blockerList);
                            }
                        }

                        if(!blockerFailed)
                        {
                            var notValidBlocker = gameManager.NotValidBlockerList();
                            if(notValidBlocker.Count > 0)
                            {
                                SetBlockerFailed(true);
                                ShowBlockerFailPopup(notValidBlocker[0].BlockerType);
                            }
                        }

                        m_block.SetActive(false);

                        --m_progressId;
					},
					this.GetCancellationTokenOnDestroy()
				);
				break;
			case CurrentPlayState.BoardChanged { CurrentBoardIndex: var index, CurrentBoard: var board }:
				m_block.SetActive(true); 
				await UniTask.WaitUntil(() => m_progressId <= 1);
				await UniTask.Defer(() => UniTask.Delay(TimeSpan.FromSeconds(0.25f)));

				m_bottomView.BasketView.Clear();
				m_bottomView.StashView.Clear();

				m_tileItems.ForEach(item => m_tileItemPool.Release(item));
				m_tileItems.Clear();

				var nextTileItems = board
					.Tiles
					.Select(
						tile => {
							TileItem tileItem = m_tileItemPool.Get();
							tileItem.OnUpdateUI(tile, false, out _);
							return tileItem;
						}
					);
				m_tileItems.AddRange(nextTileItems);

				await UniTask.Defer(() => m_mainView.MoveNextAndChange(board.LayerCount, m_tileItems));
				m_topView.BoardCountView.OnUpdateUI(index);
				m_block.SetActive(false);
				--m_progressId;
				break;
			case CurrentPlayState.Finished { Result: var result }:
                m_block.SetActive(true);
                await UniTask.WaitUntil(() => m_progressId <= 1);
                await UniTask.Defer(() => UniTask.Delay(TimeSpan.FromSeconds(0.25f)));

                if (result is CurrentPlayState.Finished.State.CLEAR)
                {
                    await UniTask.Defer(() => m_canvasGroup.DOFade(0f, 0.5f).ToUniTask());
                }

                if (result is CurrentPlayState.Finished.State.OVER)
                {
                    LevelFail();
                }
                else
                {
                    LevelClear();
                }
                m_block.SetActive(false);
				--m_progressId;
				break;
			default:
                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=white>currentPlayState : {0}</color>", currentPlayState));
				--m_progressId;
				break;
		}

        CheckTriggerArea();

        void SetBackground(string path)
        {
            Debug.Log(CodeManager.GetMethodName() + path);

            var sprite = Resources.Load<Sprite>(path);
            backgroundImage.sprite = sprite;
        }

        void CheckTriggerArea()
        {
            //Debug.Log(CodeManager.GetMethodName());

            m_tileItems.ForEach(tile => {
                tile.m_triggerArea.SetOffsetX(0, 0);
                tile.m_triggerArea.SetOffsetY(0, 0);
            });

            var boardTiles = m_tileItems
                .Where(tileItem => tileItem.IsInteractable && tileItem.Current.Location is LocationType.BOARD).ToList();

            for(int i=0; i < boardTiles.Count; i++)
            {
                TileItem currentTile = boardTiles[i];
                TileItemModel currentModel = currentTile.Current;
                Vector2 currentPosition = currentModel.Position;

                var checkPosition = Constant.Game.AROUND_TILE_POSITION
                    .Select(pos => { return pos + currentPosition; }).ToList();

                var checkTiles = boardTiles
                    .Where(tileItem => tileItem != currentTile && checkPosition.Contains(tileItem.Current.Position)).ToList();

                bool existLeft   = checkTiles.Count > 0 && checkTiles.Where(item => item.Current.Position.x == currentPosition.x - Constant.Game.TILE_WIDTH).Count() > 0;
                bool existRight  = checkTiles.Count > 0 && checkTiles.Where(item => item.Current.Position.x == currentPosition.x + Constant.Game.TILE_WIDTH).Count() > 0;
                bool existTop    = checkTiles.Count > 0 && checkTiles.Where(item => item.Current.Position.y == currentPosition.y + Constant.Game.TILE_HEIGHT).Count() > 0;
                bool existBottom = checkTiles.Count > 0 && checkTiles.Where(item => item.Current.Position.y == currentPosition.y - Constant.Game.TILE_HEIGHT).Count() > 0;

                currentTile.m_triggerArea.SetOffsetX(existLeft ? 0 : Constant.Game.AROUND_TILE_OFFSET_LEFT, existRight ? 0 : Constant.Game.AROUND_TILE_OFFSET_RIGHT);
                currentTile.m_triggerArea.SetOffsetY(existBottom ? 0 : Constant.Game.AROUND_TILE_OFFSET_BOTTOM, existTop ? 0 : Constant.Game.AROUND_TILE_OFFSET_TOP);
            }
        }
	}

    public void CheckTutorialBlocker(List<TileItem> blockerList)
    {
        for(int i=0; i < blockerList.Count; i++)
        {
            TileItem tileItem = blockerList[i];
            BlockerType blockerType = blockerList[i].blockerType;

            if((GlobalDefine.isBlockerTutorialTest || !IsShowed_BlockerTutorial(blockerType)) && !m_blockerShownList.Contains(blockerType))
            {
                m_blockerShownList.Add(blockerType);
                switch(blockerType)
                {
                    case BlockerType.Glue_Left:
                        m_blockerShownList.Add(BlockerType.Glue_Right);
                        break;
                    case BlockerType.Glue_Right:
                        m_blockerShownList.Add(BlockerType.Glue_Left);
                        break;
                }

                GlobalData.Instance.ShowTutorial_Blocker(blockerType, tileItem);
                break;
            }
        }

        bool IsShowed_BlockerTutorial(BlockerType blocker)
        {
            switch(blocker)
            {
                case BlockerType.Glue_Left:
                case BlockerType.Glue_Right:
                    return m_userManager.Current.Showed_BlockerTutorial_Glue;
                case BlockerType.Bush:
                    return m_userManager.Current.Showed_BlockerTutorial_Bush;
                case BlockerType.Suitcase:
                    return m_userManager.Current.Showed_BlockerTutorial_Suitcase;
                case BlockerType.Jelly:
                    return m_userManager.Current.Showed_BlockerTutorial_Jelly;
                case BlockerType.Chain:
                    return m_userManager.Current.Showed_BlockerTutorial_Chain;
                default:
                    return true;
            }
        }
    }

#region LevelClear Popup

    public void LevelClear()
    {
        Debug.Log(CodeManager.GetMethodName() + m_gameManager.CurrentLevel);

        SoundManager soundManager = Game.Inst?.Get<SoundManager>();
        soundManager?.PlayFx(Constant.Sound.SFX_TILE_MATCH_FINISH);

        m_canvasGroup.alpha = 0f;
        
        GlobalDefine.RequestAD_HideBanner();
        SDKManager.SendAnalytics_I_Scene_Clear();

        UIManager.ShowPopupUI<GameClearPopup>(
            new GameClearPopupParameter(
                m_gameManager.CurrentLevel, 
                OnContinue: level => m_gameManager.LoadLevel(level, m_mainView.CachedRectTransform)
            )
        );
    }

#endregion LevelClear Popup


#region LevelFail Popup

    public void LevelFail(bool isStartPopup = true)
    {
        if (isStartPopup)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("Level {0}", m_gameManager.CurrentLevel));
            
            m_isFirstFail = m_userManager.Current.FailedCountTotal == 0;

            SDKManager.SendAnalytics_I_Scene_Fail();
        }

        CurrentPlayState.Finished.State result = CurrentPlayState.Finished.State.OVER;

        List<SkillItemType> itemTypes = new();

        if (m_isFirstFail)
            itemTypes.Add(SkillItemType.Stash);
        
        int coinAmount = m_isFirstFail ? 0 : m_gameManager.GetSkillPackageCoin(isStartPopup, out itemTypes);
        
        Debug.Log(CodeManager.GetMethodName() + string.Format("Required Coin : {0}", coinAmount));

        UIManager.ShowPopupUI<PlayEndPopup>(
            new PlayEndPopupParameter(
                State: result,
                ContinueButtonParameter: new UITextButtonParameter {
                    OnClick = () => {
                        Continue(coinAmount, itemTypes);
                    },
                    ButtonText = m_isFirstFail ? Text.Button.TRY_FREE : Text.Button.CONTINUE,
                    SubWidgetBuilder = () => {
                        if (m_isFirstFail) return null;
                        
                        var widget = Instantiate(ResourcePathAttribute.GetResource<IconWidget>());
                        widget.OnSetup("UI_Icon_Coin", $"{coinAmount}");
                        return widget.CachedGameObject;
                    }
                },
                OnQuit: () => ReallyFailed(),
                OnOpened: async () => {
                    if (isStartPopup && GlobalDefine.IsEnable_CheerUp())
                    {
                        await UniTask.WaitForSeconds(0.2f);

                        if (GlobalData.Instance.userManager.Current.PurchasedCheerup1)
                        {
                            await GlobalData.Instance.ShowPopup_Cheerup2();
                        }
                        else
                        {
                            await GlobalData.Instance.ShowPopup_Cheerup1();
                        }
                    }
                }
            )
        );

        void Continue(int _coinAmount, List<SkillItemType> _itemTypes)
        {
            OnContinue(_coinAmount, _itemTypes, () => { 
                        bg_ads.SetActive(!GlobalData.Instance.userManager.Current.NoAD);
                        GlobalDefine.RequestAD_ShowBanner();
                        LevelFail(false); 
            });
        }
    }

#endregion LevelFail Popup


#region BlockerFailPopup

    public void ShowBlockerFailPopup(BlockerType blockerType, bool isStartPopup = true)
    {
        if (isStartPopup)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("Level {0} : {1}", m_gameManager.CurrentLevel, blockerType));
            
            SDKManager.SendAnalytics_I_Scene_Fail(false);
        }

        CurrentPlayState.Finished.State result = CurrentPlayState.Finished.State.OVER;

        UIManager.ShowPopupUI<BlockerFailPopup>(
            new BlockerFailPopupParameter(
                State: result,
                ContinueButtonParameter: new UITextButtonParameter {
                    OnClick = () => {
                        Continue(SkillItemType.Undo, () => {
                            ShowBlockerFailPopup(blockerType, false);
                        });
                    },
                    ButtonText = Text.Button.UNDO,
                    SubWidgetBuilder = () => {
                        var widget = Instantiate(ResourcePathAttribute.GetResource<IconWidget>());
                        widget.OnSetup("UI_Shop_Icon_Undo", 60);
                        return widget.CachedGameObject;
                    }
                },
                OnQuit: () => ReallyFailed(),
                BlockerType: blockerType
            )
        );

        void Continue(SkillItemType skillItemType, Action onBuyPopupClosed)
        {
            gameManager.UseSkillItem(skillItemType, true, 
                onShowBuyPopup:(index) => {
                    RequestOpenBuyItemPopup(index);
                }, 
                onSuccess:() => {
                    SetBlockerFailed(false);
                });

            void RequestOpenBuyItemPopup(int index)
            {
                OpenBuyItemPopup(index,
                    onClosed:(storeOpen) => {
                        BuyPopupClosed(storeOpen);
                    },
                    onStoreClosed:() => {
                        BuyPopupClosed(false);
                    },
                    onPurchased:() => {
                        SetBlockerFailed(false);
                    },
                    ignoreBackKey:true
                );
            }

            void BuyPopupClosed(bool openStorePopup)
            {
                if(!openStorePopup)
                {
                    //Debug.Log(CodeManager.GetMethodName() + string.Format("CurrentPopupStage : {0}", UIManager.GetPopupManager().CurrentPopupStage));

                    UniTask.Void(async () => {
                        await UniTask.WaitUntil(() => UIManager.GetPopupManager().CurrentPopupStage == PopupManager.PopupStage.None);
                        
                        onBuyPopupClosed?.Invoke();
                    });
                }
            }
        }
    }

    public void SetBlockerFailed(bool value)
    {
        //Debug.Log(CodeManager.GetMethodName() + value);
        blockerFailed = value;
    }

#endregion BlockerFailPopup


#region ReallyFailed

    private void ReallyFailed(bool requiredLife = true)
    {
        if (requiredLife)
            m_userManager.TryUpdate(requireLife: true);
        
        ShowGiveUpPopup(
            new UITextButtonParameter {
                ButtonText = Text.Button.REPLAY,
                OnClick = () =>
                {
                    if (GlobalDefine.IsLevelEditor())
                    {
                        m_gameManager.LoadLevel(m_gameManager.CurrentLevel, m_mainView.CachedRectTransform);
                    }
                    else
                    {
                        SDKManager.SendAnalytics_C_Scene(Text.Button.REPLAY);

                        var (_, valid, _) = m_userManager.Current.Valid();

                        if (!valid)
                        {
                            //[PlayScene:Replay] 하트 부족시 상점 열기.
                            SDKManager.SendAnalytics_C_Scene(Text.Button.STORE);

                            GlobalDefine.RequestAD_HideBanner();

                            GlobalData.Instance.ShowStorePopup(() => {
                                bg_ads.SetActive(!GlobalData.Instance.userManager.Current.NoAD);
                                GlobalDefine.RequestAD_ShowBanner();
                                ReallyFailed(false);
                            });
                            
                            return;
                        }
                        
                        m_gameManager.LoadLevel(m_gameManager.CurrentLevel, m_mainView.CachedRectTransform);
                    }
                }
            },
            new ExitBaseParameter (
                includeBackground: false,
                onExit: () => {
                    OnExit(false);
                }
            )
        );
    }

#endregion ReallyFailed

}
