using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NineTap.Common;

public class PuzzlePlayView : CachedBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private RawImage m_backgroundImage;

	[SerializeField]
	private PuzzlePieceSlotContainer m_pieceSlotContainer;

	[SerializeField]
	private PuzzlePlayPieceScrollView m_pieceScrollView;

	[SerializeField]
	private UIImageButton m_backButton;

    private JigsawPuzzlePiece m_piecePrefab;
	private PuzzleManager m_puzzleManager;
    
	public void OnSetup(PuzzleManager puzzleManager, Action onClickButton)
	{
		m_puzzleManager = puzzleManager;
		m_piecePrefab = ResourcePathAttribute.GetResource<JigsawPuzzlePiece>();
		m_pieceSlotContainer.OnSetup();
		m_backButton.OnSetup(
			new UIImageButtonParameter
			{
				OnClick = () => onClickButton?.Invoke()
			}
		);
	}

	private void OnDestroy()
	{
		m_puzzleManager?.Dispose();
	}

	public async UniTask OnShowAsync(PuzzleData puzzleData, uint placedPieces, uint unlockedPieces)
	{
		if (puzzleData == null)
		{
			return;
		}

		CachedGameObject.SetActive(true);
		if (await m_puzzleManager.LoadAsync(puzzleData, placedPieces, unlockedPieces))
		{
			OnShow();
		}

		m_canvasGroup.alpha = 1f;
	}

	public void OnShow()
	{
		if (m_puzzleManager == null)
		{
			OnHide();
			return;
		}

		CachedGameObject.SetActive(true);
		
		OnLoadUI(m_puzzleManager.CurrentPlayingPuzzle);

		m_backgroundImage.texture = m_puzzleManager.Background;

		m_canvasGroup.alpha = 1f;
	}

	public void OnLoadUI(CurrentPlayingPuzzleContent content)
	{
		m_pieceSlotContainer.TearDown();
		m_pieceScrollView.TearDown();
		
		float ratio = m_backgroundImage.rectTransform.rect.width / JigsawPuzzleSetting.Instance.ImageSize;

		List<PuzzlePieceItemData> pieceScrollDatas = new();

		int pieceCost = content.PieceCost;
        var pieceSources = content.PieceSources;
		List<int> placedList = content.GetPlacedPieceList();
		List<int> unlockedList = content.GetUnlockedPieceList();

		for (int index = 0, count = PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT; index < count; index++)
		{
			PuzzlePieceSource pieceSource = pieceSources[index];
			int realIndex = pieceSource.Index;
			PuzzlePieceItemData pieceData = new PuzzlePieceItemData { 
				Index = realIndex,
                Cost = pieceCost,
				Sprite = pieceSources[index].Sprite,
                SpriteAttached = pieceSources[index].SpriteAttached,
				Size = JigsawPuzzleSetting.Instance.PieceSizeWithPadding * ratio,
				IsLocked = !unlockedList.Contains(realIndex),
				//OnMovePiece = OnMovePiece,
				OnTryUnlock = OnTryUnlock,
                MovePiece = MovePiece
			};

			//if (placedList.Contains(realIndex))
			{
                bool attached = placedList.Contains(realIndex);

				JigsawPuzzlePiece piece = Instantiate(m_piecePrefab);
				piece.name = $"piece[{realIndex}]";
				piece.OnSetup(pieceData, attached);
                m_pieceSlotContainer.UpdateSlot(realIndex, piece.CachedGameObject);
			}
			/*else
			{
				pieceScrollDatas.Add(pieceData);
			}*/

		}

		m_pieceScrollView.UpdateUI(pieceScrollDatas.ToArray());
	}

	private void OnTryUnlock(JigsawPuzzlePiece piece, PuzzlePieceItemData itemData, Vector2 position, Action<bool> onComplete=null)
	{
        //Debug.Log(CodeManager.GetMethodName() + itemData.Index);

		if (m_puzzleManager.TryUnlockPiece(itemData.Index))
		{
            Debug.Log(CodeManager.GetMethodName() + "SUCCESS");

            GlobalData.Instance.fragmentHome?.RefreshPuzzleBadge(GlobalData.Instance.userManager.Current.Puzzle);

			itemData.IsLocked = false;
			//m_pieceScrollView.UpdateUI(itemData);

            itemData.MovePiece?.Invoke(piece, itemData, position, onComplete);
		}
        else
        {
            User user = GlobalData.Instance.userManager.Current;

            UIManager.ShowPopupUI<ReadyPopup>(
			new ReadyPopupParameter(
				Level: user.Level,
				ExitParameter: ExitBaseParameter.CancelParam,
				BaseButtonParameter: new UITextButtonParameter{
					ButtonText = NineTap.Constant.Text.Button.PLAY,
					OnClick = () => 
					{
						var (_, valid, _) = user.Valid();

						if (!valid) // TODO: 하트 구매 화면으로 옮긴다.
						{
							//하트 구매 요구 (TBD)
							UIManager.ShowPopupUI<GiveupPopup>(
								new GiveupPopupParameter(
									Title: "Purchase",
									Message: "Purchase Life",
                                    ignoreBackKey: false,
									ExitParameter: ExitBaseParameter.CancelParam,
									BaseButtonParameter: new UITextButtonParameter {
										ButtonText = "Go to Shop",
										OnClick = () => GlobalData.Instance.mainScene.scrollView.MoveTo((int)MainMenuType.STORE)
									},
									HUDTypes: HUDType.ALL
								)
							);
							return;
						}
						UIManager.ShowSceneUI<PlayScene>(new PlaySceneParameter());
					}
				},
				AllPressToClose: true,
				HUDTypes: HUDType.ALL,
                OnComplete: () => { onComplete?.Invoke(false); }
			));

            //onComplete?.Invoke();
        }
	}

    private void MovePiece(JigsawPuzzlePiece piece, PuzzlePieceItemData itemData, Vector2 position, Action<bool> onComplete=null)
	{
        //Debug.Log(CodeManager.GetMethodName() + itemData.Index);

        SoundManager soundManager = Game.Inst?.Get<SoundManager>();
        soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);

        GlobalData.Instance.CreateEffect("UI_Icon_GoldPuzzle_Big", 
            GlobalData.Instance.HUD.behaviour.Fields[0].AttractorTarget,
            piece.transform, 
            Constant.Game.TWEENTIME_JIGSAW_MOVE,
            () => {
                m_pieceSlotContainer.Check(piece.Index, piece, null);
                m_puzzleManager.AddPlacedList(piece.Index);
                GlobalData.Instance.fragmentCollection.RefreshPieceState(m_puzzleManager.CurrentPlayingPuzzle.CountryCode, m_puzzleManager.PuzzleIndex, itemData.Index, true);
                CheckPuzzleComplete();  

                onComplete?.Invoke(true);
            });
        
		//JigsawPuzzlePiece piece = Instantiate(m_piecePrefab);

		/*piece.name = $"piece[{itemData.Index}]";
		piece.CachedTransform.SetParentReset(CachedTransform);
		piece.CachedTransform.position = position;
		piece.OnSetup(itemData, true);*/
        
        //m_pieceScrollView.RemoveItem(itemData);
        
        /*var pieceItem = (PuzzlePieceScrollItem)m_pieceScrollView.Scroll.Items?.Find(item => ((PuzzlePieceScrollItem)item).Index.Equals(itemData.Index));
        if(pieceItem != null)
        {
            pieceItem.HideItem();
        }*/

        /*m_pieceSlotContainer.MoveToSlot(itemData, piece, (index) => {
            m_puzzleManager.AddPlacedList(index);
            m_pieceScrollView.RemoveItem(itemData);

            //Debug.Log(CodeManager.GetMethodName() + string.Format("PuzzleIndex : {0}", m_puzzleManager.PuzzleIndex));
            GlobalData.Instance.fragmentCollection.RefreshPieceState(m_puzzleManager.CurrentPlayingPuzzle.CountryCode, m_puzzleManager.PuzzleIndex, itemData.Index, true);

            CheckPuzzleComplete();
            
            onComplete?.Invoke(true);
        });*/
	}

    /*private void MovePiece(PuzzlePieceItemData itemData, Vector2 position, Action<bool> onComplete=null)
	{
        //Debug.Log(CodeManager.GetMethodName() + itemData.Index);

        SoundManager soundManager = Game.Inst?.Get<SoundManager>();
        soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
        
		JigsawPuzzlePiece piece = Instantiate(m_piecePrefab);

		piece.name = $"piece[{itemData.Index}]";
		piece.CachedTransform.SetParentReset(CachedTransform);
		piece.CachedTransform.position = position;
		piece.OnSetup(itemData);
        
        //m_pieceScrollView.RemoveItem(itemData);
        
        var pieceItem = (PuzzlePieceScrollItem)m_pieceScrollView.Scroll.Items?.Find(item => ((PuzzlePieceScrollItem)item).Index.Equals(itemData.Index));
        if(pieceItem != null)
        {
            pieceItem.HideItem();
        }

        m_pieceSlotContainer.MoveToSlot(itemData, piece, (index) => {
            m_puzzleManager.AddPlacedList(index);
            m_pieceScrollView.RemoveItem(itemData);

            //Debug.Log(CodeManager.GetMethodName() + string.Format("PuzzleIndex : {0}", m_puzzleManager.PuzzleIndex));
            GlobalData.Instance.fragmentCollection.RefreshPieceState(m_puzzleManager.CurrentPlayingPuzzle.CountryCode, m_puzzleManager.PuzzleIndex, itemData.Index, true);

            CheckPuzzleComplete();
            
            onComplete?.Invoke(true);
        });
	}*/

    private void CheckPuzzleComplete()
    {
        if (m_pieceScrollView.IsEmpty())
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Puzzle {0} Complete!!</color>", m_puzzleManager.PuzzleIndex));

            GlobalData.Instance.fragmentCollection.RefreshLockState();

            // TODO: 퍼즐 완성 연출.
        }
    }

    /*private void OnMovePiece(PuzzlePieceItemData itemData, PointerEventData eventData)
	{
        Debug.Log(CodeManager.GetMethodName() + itemData.Index);
        
		JigsawPuzzlePiece piece = Instantiate(m_piecePrefab);

		piece.name = $"piece[{itemData.Index}]";
		piece.CachedTransform.SetParentReset(CachedTransform);
		piece.CachedTransform.position = eventData.position;
		//piece.CachedRectTransform.anchoredPosition += Vector2.up * 144f;
		piece.OnSetup(
			itemData, 
			eventData, 
			() => {
				m_pieceSlotContainer.Check(itemData.Index, piece, m_puzzleManager.AddPlacedList);
			}
		);

		m_pieceScrollView.RemoveItem(itemData);
	}*/

	public void OnHide()
	{
		var pieces = CachedTransform.GetComponentsInChildren<JigsawPuzzlePiece>();
		if (pieces != null)
		{
			foreach (var piece in pieces)
			{
				Destroy(piece.CachedGameObject);
			}
		}

		m_pieceSlotContainer.TearDown();
		m_pieceScrollView.TearDown();

		m_canvasGroup.alpha = 0f;
		CachedGameObject.SetActive(false);
	}
}
