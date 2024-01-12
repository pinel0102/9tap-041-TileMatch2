using System.Collections.Generic;

public record PuzzleInfo(PuzzleData PuzzleData, uint PlacedPieces, uint UnlockedPieces)
{
	public static PuzzleInfo Empty = new PuzzleInfo(null, 0, 0);

	public void Deconstruct(out PuzzleData puzzleData, out uint placedPieces, out uint unlockedPieces)
	{
		puzzleData = PuzzleData;
		placedPieces = PlacedPieces;
		unlockedPieces = UnlockedPieces;
	}
}

public record CurrentPlayingPuzzleContent(PuzzlePieceSource[] PieceSources, uint PlacedPieces, uint UnlockedPieces, int PieceCost, string CountryCode)
{
	public void Deconstruct(out PuzzlePieceSource[] pieceSources, out uint placedPieces, out uint unlockedPieces, out int pieceCost, out string countryCode)
	{
		pieceSources = PieceSources;
		placedPieces = PlacedPieces;
		unlockedPieces = UnlockedPieces;
        pieceCost = PieceCost;
        countryCode = CountryCode;
	}

	public List<int> GetPlacedPieceList()
	{
		List<int> list = new();

		for (var i = 0; i < PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT; i++)
		{
			if ((PlacedPieces & PuzzlePieceMaker.CHECKER << i) != 0)
			{
				list.Add(i);
			}
		}

		return list;
	}

	public List<int> GetUnlockedPieceList()
	{
		List<int> list = new();

		for (var i = 0; i < PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT; i++)
		{
			if ((UnlockedPieces & PuzzlePieceMaker.CHECKER << i) != 0)
			{
				list.Add(i);
			}
		}

		return list;
	}
}

