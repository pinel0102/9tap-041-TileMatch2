using UnityEngine;

using System.Linq;
using System.Collections.Generic;

public record PuzzlePieceSource(
	int Index,
	int Row,
	int Column,
	Vector2 Position, 
	Sprite Sprite, 
	IReadOnlyList<PuzzleCurveType> PuzzleCurveTypes
)
{
	public void Deconstruct(out int index, out Vector2 position, out Sprite sprite, out IReadOnlyList<PuzzleCurveType> curveTypes)
	{
		index = Index;
		position = Position;
		sprite = Sprite;
		curveTypes = PuzzleCurveTypes;
	}

	public void Deconstruct(out int row, out int column, out Vector2 position, out Sprite sprite, out IReadOnlyList<PuzzleCurveType> curveTypes)
	{
		row = Row;
		column = Column;
		position = Position;
		sprite = Sprite;
		curveTypes = PuzzleCurveTypes;
	}
}

public static class PuzzlePieceMaker
{
	public static readonly int MAX_PUZZLE_PIECE_COUNT = Constant.Puzzle.MAX_ROW_COUNT * Constant.Puzzle.MAX_COLUMN_COUNT;
	public static readonly uint CHECKER = 1;

	public static PuzzlePieceSource[] CreatePieceSources
	(
		Texture2D source,
		int rowCount,
		int columnCount,
		IReadOnlyList<PieceData> pieceDatas = default,
		uint? optional = default
	)
	{
		bool[] places = new bool[rowCount * columnCount];
		for (int x = 0; x < rowCount * columnCount; x++)
		{
			if (optional == null)
			{
				places[x] = true;
				continue;
			}
			places[x] = (optional & (CHECKER << x)) != 0;
		}

		int imageSize = JigsawPuzzleSetting.Instance.ImageSize;
		int pieceSize = JigsawPuzzleSetting.Instance.PieceSize;
		int sizeWithPadding = JigsawPuzzleSetting.Instance.PieceSizeWithPadding;
		int padding = JigsawPuzzleSetting.Instance.PiecePadding;

		int count = imageSize + padding * 2;

		//가장자리 처리를 위해 패딩 만큼 텍스쳐 사이즈를 늘린다.
		Texture2D texture = new Texture2D(count, count);

		for (int x = 0; x < count; x++)
		{
			for (int y = 0; y < count; y++)
			{
				int px = x - padding;
				int py = y - padding;

				Color color = Color.clear;
				
				if (px >= 0 && px < source.width && py >= 0 && py < source.height)
				{
					color = source.GetPixel(px, py);
				}

				texture.SetPixel(x, y, color);
			}
		}

		texture.Apply();

		List<PuzzlePieceSource> results = new();
		List<PieceData> pieces = new();
		if (pieceDatas == null)
		{
			for (int x = 0; x < rowCount; x++)
			{
				for (int y = 0; y < columnCount; y++)
				{
					pieces.Add(new PieceData(SetCurveType(x, y), x, y));
				}
			}
		}
		else
		{
			pieces.AddRange(pieceDatas);
		}

		for (int x = 0; x < rowCount; x++)
		{
			for (int y = 0; y < columnCount; y++)
			{
				int index = x + y + x * (rowCount - 1);
				if (!places[index])
				{
					continue;
				}

				Vector2 position = new Vector2(x * pieceSize, y * pieceSize);
				PieceData pieceData = GetPieceData(x, y, pieces);

				results.Add(
					new PuzzlePieceSource(
						index,
						x,
						y, 
						position, 
						GetSprite(pieceData), 
						pieceData.PuzzleCurveTypes
					)
				);
			}
		}

		return results.ToArray();

		Sprite GetSprite(PieceData piece)
		{
			if (piece == null)
			{
				return null;
			}

			Texture2D pieceTex = CreateClearTexture();

			Stack<Vector2Int> stack = new();
			bool[, ] visited = new bool[sizeWithPadding, sizeWithPadding];

			Algorithm.FloodFillInit(piece.PuzzleCurveTypes.ToArray(), stack, ref visited);
			Algorithm.FloodFill(piece.Row, piece.Column, texture, pieceTex, stack, ref visited);
			pieceTex.Apply();

			return Sprite.Create(pieceTex, new Rect(0, 0, pieceTex.width, pieceTex.height), Vector2.one * 0.5f);
		}

		Texture2D CreateClearTexture()
		{
			Texture2D texture = new Texture2D(sizeWithPadding, sizeWithPadding, TextureFormat.ARGB32, false);

			for (int i = 0; i < sizeWithPadding; i++)
			{
				for (int j = 0; j < sizeWithPadding; j++)
				{
					texture.SetPixel(i, j, Color.clear);
				}
			}

			return texture;
		}

		PuzzleCurveType[] SetCurveType(int row, int column)
		{
			PuzzleCurveType[] curveTypes = new PuzzleCurveType[4] {
				PuzzleCurveType.NONE,
				PuzzleCurveType.NONE,
				PuzzleCurveType.NONE,
				PuzzleCurveType.NONE
			};

			curveTypes[(int)DirectionType.LEFT] = row switch {
				0 => PuzzleCurveType.NONE,
				_ => GetPieceData(row - 1, column, pieces) is { PuzzleCurveTypes: var info } && info[(int)DirectionType.RIGHT] != PuzzleCurveType.NONE?
					(PuzzleCurveType)(3 ^ (uint)info[(int)DirectionType.RIGHT]):
					PuzzleCurveType.NONE
			};

			curveTypes[(int)DirectionType.DOWN] = column switch {
				0 => PuzzleCurveType.NONE,
				_ => GetPieceData(row, column - 1, pieces) is { PuzzleCurveTypes: var info } && info[(int)DirectionType.UP] != PuzzleCurveType.NONE?
					(PuzzleCurveType)(3 ^ (uint)info[(int)DirectionType.UP]):
					PuzzleCurveType.NONE
			};


			curveTypes[(int)DirectionType.RIGHT] = row switch {
				4 => PuzzleCurveType.NONE,
				_ => (PuzzleCurveType)UnityEngine.Random.Range(1, 3)
			};

			curveTypes[(int)DirectionType.UP] = column switch {
				4 => PuzzleCurveType.NONE,
				_ => (PuzzleCurveType)UnityEngine.Random.Range(1, 3)
			};

			//Debug.Log($"[{row}, {column}] Up:{curveTypes[(int)DirectionType.UP]} Down:{curveTypes[(int)DirectionType.DOWN]}");
			return curveTypes;
		}

		PieceData GetPieceData(int x, int y, IEnumerable<PieceData> enumerable)
		{
			return enumerable.FirstOrDefault(data => data.Row == x && data.Column == y);
		}
	}
}
