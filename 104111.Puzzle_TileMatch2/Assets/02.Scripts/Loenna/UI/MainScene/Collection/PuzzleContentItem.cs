using UnityEngine;
using UnityEngine.UI;

using System;

using TMPro;

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

	public void OnSetup(UserManager userManager)
	{
		if(m_isInitialized)
		{
			return;
		}

		m_isInitialized = true;
		m_userManager = userManager;
		m_userManager.OnUpdated += LevelCheck;
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
		onClick.RemoveAllListeners();
		if (SetLocked(m_userManager.Current.Level, contentData.PuzzleData.Level))
		{
			m_nameText.text = NineTap.Constant.Text.LOCKED;
			return;
		}

		onClick.AddListener(() => contentData?.onClick?.Invoke());
		m_puzzleContentData = contentData;
		puzzleData = contentData.PuzzleData;
        Index = puzzleData.Index;
        
        texture = Resources.Load<Texture2D>(puzzleData.GetImagePath());
		m_thumbnail.texture = texture;
		m_nameText.text = puzzleData.Name;

        RefreshPuzzle(contentData.PlacedPiecesData);

		/*var puzzlePieces = PuzzlePieceMaker.CreatePieceSources(
			texture, 
			Constant.Puzzle.MAX_ROW_COUNT, 
			Constant.Puzzle.MAX_COLUMN_COUNT, 
			puzzleData.Pieces,
			contentData.PlacedPiecesData
		);

        float completed = puzzlePieces.Length >= PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT? 1f : 0f;

		m_viewButtonObject.alpha = completed;
		m_gaugeBar.Alpha = 1f - completed;
		m_gaugeBar.OnUpdateUI(puzzlePieces.Length, PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT);

		Array.ForEach(
			puzzlePieces, 
			puzzlePiece => {
				var (index, position, sprite, _) = puzzlePiece;
				GameObject pieceGameObject = new GameObject($"piece[{index}]");
				Image image = pieceGameObject.AddComponent<Image>();
				RectTransform pieceTransform = image.rectTransform;
				pieceTransform.SetParentReset(m_pieceParent);
				image.sprite = sprite;
				image.SetNativeSize();
				pieceTransform.anchoredPosition = position;
			}
		);*/
	}

    // TODO: 여기 최적화 필요.
    public void RefreshPuzzle(uint placedPiecesData)
    {
        var puzzlePieces = PuzzlePieceMaker.CreatePieceSources(
			texture, 
			Constant.Puzzle.MAX_ROW_COUNT, 
			Constant.Puzzle.MAX_COLUMN_COUNT, 
			puzzleData.Pieces,
			placedPiecesData
		);

        float completed = puzzlePieces.Length >= PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT? 1f : 0f;

		m_viewButtonObject.alpha = completed;
		m_gaugeBar.Alpha = 1f - completed;
		m_gaugeBar.OnUpdateUI(puzzlePieces.Length, PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT);

		Array.ForEach(
			puzzlePieces, 
			puzzlePiece => {
				var (index, position, sprite, _) = puzzlePiece;
				GameObject pieceGameObject = new GameObject($"piece[{index}]");
				Image image = pieceGameObject.AddComponent<Image>();
				RectTransform pieceTransform = image.rectTransform;
				pieceTransform.SetParentReset(m_pieceParent);
				image.sprite = sprite;
				image.SetNativeSize();
				pieceTransform.anchoredPosition = position;
			}
		);
    }

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		//base.DoStateTransition(state, instant);
	}
}
