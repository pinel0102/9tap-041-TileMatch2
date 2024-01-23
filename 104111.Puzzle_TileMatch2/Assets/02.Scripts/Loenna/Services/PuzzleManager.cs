using UnityEngine;

using System;
using System.Linq;
using System.Threading;

using Cysharp.Threading.Tasks;

public class PuzzleManager : IDisposable
{
	private readonly UserManager m_userManager;
	private readonly CancellationTokenSource m_cancellationTokenSource;
	
	private int m_puzzleIndex = 0;
    private uint m_placedPieces = 0;
	private uint m_unlockedPieces = 0;
    public int PuzzleIndex => m_puzzleIndex;
    private string m_puzzleName;
    public string PuzzleName => m_puzzleName;
	private Texture2D m_background;
	public Texture2D Background => m_background;

	private CurrentPlayingPuzzleContent m_currentPlayingPuzzle;
	public CurrentPlayingPuzzleContent CurrentPlayingPuzzle => m_currentPlayingPuzzle;

	public PuzzleManager(UserManager userManager)
	{
		m_userManager = userManager;
		m_cancellationTokenSource = new();
	}

	public async UniTask<bool> LoadAsync(PuzzleData puzzleData, uint placedPieces, uint unlockedPieces)
	{
		m_puzzleIndex = puzzleData.Index;
        m_puzzleName = puzzleData.Name;
		m_placedPieces = placedPieces;

		m_userManager.Update(playingPuzzle: (m_puzzleIndex, m_placedPieces));

		var req = Resources.LoadAsync<Texture2D>(puzzleData.GetImagePath());
		m_background = await req.ToUniTask(cancellationToken: m_cancellationTokenSource.Token) as Texture2D;

		var puzzlePieces = PuzzlePieceMaker.LoadPieceSources(
			//source: m_background, 
            puzzleIndex: m_puzzleIndex,
			rowCount: Constant.Puzzle.MAX_ROW_COUNT, 
			columnCount: Constant.Puzzle.MAX_COLUMN_COUNT, 
			pieceDatas: puzzleData.Pieces
		);

		m_currentPlayingPuzzle = new CurrentPlayingPuzzleContent(
            CountryCode: puzzleData.CountryCode,
			PieceSources: puzzleData.Orders.Select(index => puzzlePieces[index]).ToArray(),
			PlacedPieces: placedPieces,
			UnlockedPieces: unlockedPieces,
            PieceCost: puzzleData.Cost
		);

        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1} / {2}", puzzleData.Index, puzzleData.Cost, puzzleData.Level));

		return true;
	}
	
	public void AddPlacedList(int index)
	{
        var piece = PuzzlePieceMaker.CHECKER << index;
		m_placedPieces |= piece;
        m_userManager.Update(playingPuzzle: (m_puzzleIndex, m_placedPieces));
	}

	public bool TryUnlockPiece(int index)
	{
		if(m_userManager.TryUpdate(requirePuzzle: m_currentPlayingPuzzle.PieceCost))
		{
			var piece = PuzzlePieceMaker.CHECKER << index;
			m_unlockedPieces |= piece;
			m_userManager.Update(unlockedPuzzlePiece: (m_puzzleIndex, m_unlockedPieces));

			return true;
		}

		return false;
	}

	public void Dispose()
	{
		m_cancellationTokenSource.Dispose();
	}
}
