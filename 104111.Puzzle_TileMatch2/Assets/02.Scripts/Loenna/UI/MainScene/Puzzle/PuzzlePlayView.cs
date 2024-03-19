using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NineTap.Common;
using System.Linq;
using TMPro;

public class PuzzlePieceItemData
{
	public int Index;
    public int Cost;
	public Sprite Sprite;
    public Sprite SpriteAttached;
	public float Size;
	public Action<JigsawPuzzlePiece, Action> OnTryUnlock;
}

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
    
    [SerializeField]
	private GameObject m_lockedObject;

    [SerializeField]
	private TMP_Text m_lockedText;

    private const string formatLocked = "Clear Level {0} to Unlock";

    private JigsawPuzzlePiece m_piecePrefab;
	private PuzzleManager m_puzzleManager;
    
	public void OnSetup(PuzzleManager puzzleManager, Action onClickButton)
	{
		m_puzzleManager = puzzleManager;
		m_piecePrefab = ResourcePathAttribute.GetResource<JigsawPuzzlePiece>();
        m_lockedText.SetText(formatLocked, Constant.Game.LEVEL_PUZZLE_START - 1);
		m_pieceSlotContainer.OnSetup();
		m_backButton.OnSetup(
			new UIImageButtonParameter
			{
				OnClick = () => onClickButton?.Invoke()
			}
		);
        
        CheckUserLevel();
	}

	private void OnDestroy()
	{
		m_puzzleManager?.Dispose();
	}

    public void CheckUserLevel()
    {
        CheckUserLevel(GlobalData.Instance.userManager.Current.Level);
    }

    public void CheckUserLevel(int level)
    {
        m_lockedObject.SetActive(level < Constant.Game.LEVEL_PUZZLE_START);
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
		
		float ratio = m_backgroundImage.rectTransform.rect.width / JigsawPuzzleSetting.Instance.ImageSize;
		int pieceCost = content.PieceCost;
        var pieceSources = content.PieceSources;
		List<int> placedList = content.GetPlacedPieceList();
		
		for (int index = 0, count = PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT; index < count; index++)
		{
			PuzzlePieceSource pieceSource = pieceSources[index];
			int realIndex = pieceSource.Index;
            bool placed = placedList.Contains(realIndex);

			PuzzlePieceItemData pieceData = new PuzzlePieceItemData { 
				Index = realIndex,
                Cost = pieceCost,
				Sprite = pieceSources[index].Sprite,
                SpriteAttached = pieceSources[index].SpriteAttached,
				Size = JigsawPuzzleSetting.Instance.PieceSizeWithPadding * ratio,
				OnTryUnlock = OnTryUnlock
			};

            JigsawPuzzlePiece piece = Instantiate(m_piecePrefab);
            piece.name = $"piece[{realIndex:00}]";
            piece.OnSetup(pieceData, placed);
            m_pieceSlotContainer.UpdateSlot(realIndex, piece.CachedGameObject, placed);
		}
	}

	private void OnTryUnlock(JigsawPuzzlePiece piece, Action onComplete=null)
	{
        //Debug.Log(CodeManager.GetMethodName() + itemData.Index);
        GlobalData.Instance.SetTouchLock_MainScene(true);

		if (m_puzzleManager.TryUnlockPiece(piece.Index))
		{
            //Debug.Log(CodeManager.GetMethodName() + string.Format("{0:00} : SUCCESS", piece.Index));

            //GlobalData.Instance.SetTouchLock_MainScene(true);
            GlobalData.Instance.fragmentHome?.RefreshPuzzleBadge(GlobalData.Instance.userManager.Current.Puzzle);

			MovePiece(piece, onComplete);
		}
        else
        {
            if (GlobalData.Instance.userManager.Current.Level <= GlobalData.Instance.tableManager.LastLevel)
            {
                GlobalData.Instance.ShowReadyPopup();
            }
            else
            {
                GlobalData.Instance.SetTouchLock_MainScene(false);
            }
        }
	}

    private void MovePiece(JigsawPuzzlePiece piece, Action onComplete=null)
	{
        //Debug.Log(CodeManager.GetMethodName() + itemData.Index);

        GlobalData.Instance.soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);

        GlobalData.Instance.CreateEffect("UI_Icon_GoldPuzzle_Big", Constant.Sound.SFX_TILE_MATCH,
            GlobalData.Instance.HUD.behaviour.Fields[0].AttractorTarget,
            piece.transform, 
            Constant.Game.TWEENTIME_JIGSAW_MOVE,
            onComplete:() => {
                GlobalData.Instance.fragmentCollection.RefreshPieceState(m_puzzleManager.CurrentPlayingPuzzle.CountryCode, m_puzzleManager.PuzzleIndex, piece.Index, true);
                m_puzzleManager.AddPlacedList(piece.Index);
                m_pieceSlotContainer.Check(piece.Index, piece, () => {
                    CheckPuzzleComplete();
                    onComplete?.Invoke();
                });
            }, 82f, 120f);
	}

    private void CheckPuzzleComplete()
    {
        if (m_pieceSlotContainer.IsPuzzleComplete())
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Puzzle {0} Complete!!</color>", m_puzzleManager.PuzzleIndex));

            GlobalData.Instance.fragmentCollection.RefreshLockState();

            UIManager.ShowPopupUI<PuzzleCompletePopup>(new PuzzleCompletePopupParameter(
                Index: m_puzzleManager.PuzzleIndex,
                PuzzleName: m_puzzleManager.PuzzleName,
                Background: m_puzzleManager.Background,
                Content: m_puzzleManager.CurrentPlayingPuzzle,
                OnContinue: () => {
                    MoveToNextPuzzle();
                    GlobalData.Instance.SetTouchLock_MainScene(false); 
                }
            ));
        }
        else
        {
            GlobalData.Instance.SetTouchLock_MainScene(false);
        }
    }

    private void MoveToNextPuzzle()
    {   
        int currentIndex = m_puzzleManager.PuzzleIndex;
        User user = GlobalData.Instance.userManager.Current;
        PuzzleDataTable puzzleDataTable = GlobalData.Instance.tableManager.PuzzleDataTable;
        PuzzleData puzzleData = puzzleDataTable.Dic.FirstOrDefault(item => (item.Value.Level <= user.Level) && (item.Value.Index > currentIndex)).Value;
        if (puzzleData != null)
        {
            GlobalData.Instance.MoveToPuzzle(puzzleData);
        }
        else
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("No Other Playable Puzzle"));

            /*puzzleData = puzzleDataTable.Dic.LastOrDefault(item => (item.Value.Level <= user.Level) && (item.Value.Index < currentIndex)).Value;
            if (puzzleData != null)
            {
                GlobalData.Instance.MoveToPuzzle(puzzleData);
            }
            else
            {
                Debug.Log(CodeManager.GetMethodName() + string.Format("No Other Playable Puzzle"));
            }*/
        }
    }

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
		//m_pieceScrollView.TearDown();

		m_canvasGroup.alpha = 0f;
		CachedGameObject.SetActive(false);
	}

    public Transform GetSlotTransform(int index)
    {
        return m_pieceSlotContainer.GetSlotTransform(index);
    }

    public JigsawPuzzlePiece GetSlotPiece(int index)
    {
        return m_pieceSlotContainer.GetSlotPiece(index);
    }
}
