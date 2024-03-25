using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public record PuzzlePieceSource(
	int Index,
	int Row,
	int Column,
    bool Attached,
	Vector2 Position, 
	Sprite Sprite, 
    Sprite Filter, 
	IReadOnlyList<PuzzleCurveType> PuzzleCurveTypes
)
{
	public void Deconstruct(out int index, out Vector2 position, out Sprite sprite, out Sprite filter, out IReadOnlyList<PuzzleCurveType> curveTypes, out bool attached)
	{
		index = Index;
		position = Position;
		sprite = Sprite;
        filter = Filter;
		curveTypes = PuzzleCurveTypes;
        attached = Attached;
	}

	public void Deconstruct(out int row, out int column, out Vector2 position, out Sprite sprite, out Sprite filter, out IReadOnlyList<PuzzleCurveType> curveTypes, out bool attached)
	{
		row = Row;
		column = Column;
		position = Position;
		sprite = Sprite;
        filter = Filter;
		curveTypes = PuzzleCurveTypes;
        attached = Attached;
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
					//continue;
				}

				Vector2 position = new Vector2(x * pieceSize, y * pieceSize);
				PieceData pieceData = GetPieceData(x, y, pieces);
                Sprite _sprite = GetSprite(pieceData);

				results.Add(
					new PuzzlePieceSource(
						index,
						x,
						y, 
                        places[index],
						position, 
						_sprite, 
                        GlobalDefine.GetPieceFilter(pieceData.PuzzleCurveTypes),
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

    public static PuzzlePieceSource[] LoadPieceSources
	(
		int puzzleIndex,
		int rowCount,
		int columnCount,
		IReadOnlyList<PieceData> pieceDatas = default,
		uint? optional = default
	)
	{
        List<PuzzlePieceSource> results = new();
		List<PieceData> pieces = new();
		if (pieceDatas == null)
		{
            Debug.LogWarning(CodeManager.GetMethodName() + string.Format("pieceDatas : NULL"));
			return null;
		}
		else
		{
			pieces.AddRange(pieceDatas);
		}

        string path = Path.Combine(GlobalDefine.ResPiecePath, puzzleIndex.ToString());
        var sprites = GetSprites(path);

        //Debug.Log(CodeManager.GetMethodName() + path);
        
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

		int pieceSize = JigsawPuzzleSetting.Instance.PieceSize;

		for (int x = 0; x < rowCount; x++)
		{
			for (int y = 0; y < columnCount; y++)
			{
				int index = x + y + x * (rowCount - 1);
				Vector2 position = new Vector2(x * pieceSize, y * pieceSize);
				PieceData pieceData = GetPieceData(x, y, pieces);

                //Debug.Log(CodeManager.GetMethodName() + string.Format("[{0}] : {1}, {2}", index, x, y));

				results.Add(
					new PuzzlePieceSource(
						index,
						x,
						y, 
                        places[index],
						position, 
						sprites[index], 
                        GlobalDefine.GetPieceFilter(pieceData.PuzzleCurveTypes),
                        pieceData.PuzzleCurveTypes
					)
				);
			}
		}

		return results.ToArray();

		Sprite[] GetSprites(string path)
		{
			return Resources.LoadAll<Sprite>(path);
		}

		PieceData GetPieceData(int x, int y, IEnumerable<PieceData> enumerable)
		{
			return enumerable.FirstOrDefault(data => data.Row == x && data.Column == y);
		}
	}
}
