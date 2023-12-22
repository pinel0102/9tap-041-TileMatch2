using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
				Size = JigsawPuzzleSetting.Instance.PieceSizeWithPadding * ratio,
				IsLocked = !unlockedList.Contains(realIndex),
				//OnMovePiece = OnMovePiece,
				OnTryUnlock = OnTryUnlock,
                MovePiece = MovePiece,
			};

			if (placedList.Contains(realIndex))
			{
				JigsawPuzzlePiece piece = Instantiate(m_piecePrefab);
				piece.name = $"piece[{realIndex}]";
				piece.OnSetup(pieceData);
				m_pieceSlotContainer.UpdateSlot(realIndex, piece.CachedGameObject);
			}
			else
			{
				pieceScrollDatas.Add(pieceData);
			}

		}

		m_pieceScrollView.UpdateUI(pieceScrollDatas.ToArray());
	}

	private void OnTryUnlock(PuzzlePieceItemData itemData, Vector2 position)
	{
        Debug.Log(CodeManager.GetMethodName() + itemData.Index);

		if (m_puzzleManager.TryUnlockPiece(itemData.Index))
		{
            Debug.Log(CodeManager.GetMethodName() + "SUCCESS");

			itemData.IsLocked = false;
			m_pieceScrollView.UpdateUI(itemData);

            itemData.MovePiece?.Invoke(itemData, position);
		}
	}

    private void MovePiece(PuzzlePieceItemData itemData, Vector2 position)
	{
        Debug.Log(CodeManager.GetMethodName() + itemData.Index);

        SoundManager soundManager = Game.Inst?.Get<SoundManager>();
        soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
        
		JigsawPuzzlePiece piece = Instantiate(m_piecePrefab);

		piece.name = $"piece[{itemData.Index}]";
		piece.CachedTransform.SetParentReset(CachedTransform);
		piece.CachedTransform.position = position;
		
		piece.OnSetup(itemData);        
        m_pieceSlotContainer.MoveToSlot(itemData, piece, m_puzzleManager.AddPlacedList);

		m_pieceScrollView.RemoveItem(itemData);
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
