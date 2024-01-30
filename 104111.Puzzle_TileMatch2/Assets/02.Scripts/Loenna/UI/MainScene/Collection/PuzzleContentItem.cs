using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using NineTap.Common;

public class PuzzleContentData
{
    public uint PlacedPiecesData;
	public PuzzleData PuzzleData;
	public Action onClick;
}

public class PuzzleContentItem : UIButton
{
	[SerializeField]
	private RawImage m_thumbnail;

	[SerializeField]
	private TMP_Text m_nameText;

	[SerializeField]
	private RectTransform m_pieceParent;

	[SerializeField]
	private GameObject m_completeUI;

	[SerializeField]
	private UIGaugeBar m_gaugeBar;

	[SerializeField]
	private GameObject m_lockObject;

	private PuzzleContentData m_puzzleContentData;
	public PuzzleContentData ContentData => m_puzzleContentData;

    private UserManager m_userManager;
	private bool m_isInitialized = false;
    private PuzzleData puzzleData;
    private Texture2D texture;
    private JigsawCollectionPiece piecePrefab;
    private List<JigsawCollectionPiece> pieceList = new();

    public bool isActivePuzzle;
    public int Index;
    public string puzzleName;
    public int attachedCount;
    
	public void OnSetup(UserManager userManager)
	{
		if(m_isInitialized)
		{
			return;
		}

        isActivePuzzle = false;
		m_userManager = userManager;
		m_userManager.OnUpdated += LevelCheck;
        piecePrefab = ResourcePathAttribute.GetResource<JigsawCollectionPiece>();
        m_isInitialized = true;        
	}

	protected override void OnDestroy()
	{
		if (m_userManager != null)
		{
			m_userManager.OnUpdated -= LevelCheck;
		}
		base.OnDestroy();
	}

	private void LevelCheck(User user)
	{
		if (m_puzzleContentData?.PuzzleData == null)
		{
			m_lockObject.SetActive(true);
			onClick.RemoveAllListeners();
			return;
		}

		SetLocked(user.Level, m_puzzleContentData.PuzzleData.Level);
	}

	private bool SetLocked(int currentLevel, int requiredLevel)
	{
        bool locked = currentLevel < requiredLevel;
		m_lockObject.SetActive(locked);
		return locked;
	}

	public void UpdateUI(PuzzleContentData contentData)
	{
        m_puzzleContentData = contentData;
        puzzleData = contentData.PuzzleData;
        puzzleName = puzzleData.Name;
        Index = puzzleData.Index;
        texture = Resources.Load<Texture2D>(puzzleData.GetImagePath());
		m_thumbnail.texture = texture;

        RefreshLockState();

		CreatePuzzle(contentData.PlacedPiecesData);
	}

    public void RefreshLockState()
    {
        if(!isActivePuzzle)
            onClick.RemoveAllListeners();
        
		if (SetLocked(m_userManager.Current.Level, m_puzzleContentData.PuzzleData.Level))
		{
            m_nameText.text = NineTap.Constant.Text.LOCKED;
            isActivePuzzle = false;
		}
        else
        {
            m_nameText.text = puzzleName;

            if(!isActivePuzzle)
            {   
                onClick.AddListener(() => m_puzzleContentData?.onClick?.Invoke());
                isActivePuzzle = true;
            }
        }
    }

    public void CreatePuzzle(uint placedPiecesData)
    {
        var puzzlePieces = PuzzlePieceMaker.LoadPieceSources(
			Index,
			Constant.Puzzle.MAX_ROW_COUNT, 
			Constant.Puzzle.MAX_COLUMN_COUNT, 
			puzzleData.Pieces,
			placedPiecesData
		);

        pieceList.Clear();
        Array.ForEach(
			puzzlePieces, 
			puzzlePiece => {
				var (index, position, sprite, spriteAttached, _, attached) = puzzlePiece;
                var piece = Instantiate(piecePrefab, m_pieceParent);
                piece.OnSetup(index, spriteAttached, attached, position);
                pieceList.Add(piece);
			}
		);

        RefreshGage();
    }

    public void RefreshPiece(int pieceIndex, bool attached)
    {
        pieceList[pieceIndex].RefreshAttached(attached);
        RefreshGage();
    }

    private void RefreshGage()
    {
        attachedCount = GetAttachedCount();
        float completed = attachedCount >= PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT? 1f : 0f;
        
        m_completeUI.SetActive(completed == 1);
		m_gaugeBar.Alpha = 1f - completed;
		m_gaugeBar.OnUpdateUI(attachedCount, PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT);
    }

    private int GetAttachedCount()
    {
        return pieceList.Where(item => item.IsAttached).Count();
    }

    public uint GetPlacedPieces(int Key)
    {
        return GlobalData.Instance.userManager.Current.PlayingPuzzleCollection.TryGetValue(Key, out uint result)? result : 0;
    }

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		//base.DoStateTransition(state, instant);
	}
}
