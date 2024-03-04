using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public record PuzzleData
(
    int Index,
    string Name,
    string Path,
    string CountryCode,
    int Cost,
    int Level,
	IReadOnlyList<int> Orders,
    IReadOnlyList<PieceData> Pieces
): TableRowData<int>(Index)
{
    public PieceData GetPiece(int row, int column)
    {
        return Pieces.FirstOrDefault(piece => piece.Row == row && piece.Column == column);
    }

	public string GetBaseImagePath() => string.Format("Images/Puzzle/Base/{0}", Path);
    public string GetGameImagePath() => string.Format("Images/Puzzle/Game/UI_Ingame_BG_{0}", Path);
}

//퍼즐 제너레이터에서 제너레이트 후 조각의 정보를 json화 한다.
[Serializable]
public record PieceData
(
	IReadOnlyList<PuzzleCurveType> PuzzleCurveTypes,
	int Row,
	int Column
);

[Serializable]
public record PiecesData(int Index, List<PieceData> Pieces);