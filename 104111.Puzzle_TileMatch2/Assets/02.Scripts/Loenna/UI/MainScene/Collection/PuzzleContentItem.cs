using UnityEngine;
using UnityEngine.UI;

using System;

using TMPro;
using System.Linq;
using System.Collections.Generic;

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
	private CanvasGroup m_viewButtonObject;

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

    public int Index;
    public bool isActivePuzzle;
    public string puzzleName;
    public List<GameObject> pieceObject = new();
    
	public void OnSetup(UserManager userManager)
	{
		if(m_isInitialized)
		{
			return;
		}

        isActivePuzzle = false;
		m_userManager = userManager;
		m_userManager.OnUpdated += LevelCheck;
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
			//return;
		}
        else
        {   
            if(!isActivePuzzle)
            {
                m_nameText.text = puzzleName;
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
        
        int attachedCount = puzzlePieces.Select(item => item.Attached).Count();
        float completed = attachedCount >= PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT? 1f : 0f;

		m_viewButtonObject.alpha = completed;
		m_gaugeBar.Alpha = 1f - completed;
		m_gaugeBar.OnUpdateUI(attachedCount, PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT);
        pieceObject.Clear();

		Array.ForEach(
			puzzlePieces, 
			puzzlePiece => {
				var (index, position, sprite, _, attached) = puzzlePiece;
				GameObject pieceGameObject = new GameObject($"piece[{index}]");
				Image image = pieceGameObject.AddComponent<Image>();
				RectTransform pieceTransform = image.rectTransform;
				pieceTransform.SetParentReset(m_pieceParent);
				image.sprite = sprite;
				image.SetNativeSize();
				pieceTransform.anchoredPosition = position;
                pieceGameObject.SetActive(attached);
                pieceObject.Add(pieceGameObject);
			}
		);
    }

    public void RefreshPiece(int pieceIndex, bool attached)
    {
        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", pieceIndex, attached));        
        pieceObject[pieceIndex].SetActive(attached);
    }

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		//base.DoStateTransition(state, instant);
	}
}
