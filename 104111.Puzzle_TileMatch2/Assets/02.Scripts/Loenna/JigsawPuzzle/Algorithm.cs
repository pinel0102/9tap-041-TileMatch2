using System.Collections.Generic;

using UnityEngine;

public static class Algorithm
{
	public static void FloodFill(int row, int column, Texture2D original, Texture2D result, Stack<Vector2Int> stack, ref bool[,] visited)
	{
		int sizeWithPadding = JigsawPuzzleSetting.Instance.PieceSizeWithPadding;
		int size = JigsawPuzzleSetting.Instance.PieceSize;

		while (stack.Count > 0)
		{
			//FloodFillStep();
			Vector2Int v = stack.Pop();

			int xx = v.x;
			int yy = v.y;
			Fill(v.x, v.y);

			// check right.
			int x = xx + 1;
			int y = yy;
			if (x < sizeWithPadding)
			{
				Color c = result.GetPixel(x, y);
				if (!visited[x, y])
				{
					visited[x, y] = true;
					stack.Push(new Vector2Int(x, y));
				}
			}

			// check left.
			x = xx - 1;
			y = yy;
			if (x >= 0)
			{
				Color c = result.GetPixel(x, y);
				if (!visited[x, y])
				{
					visited[x, y] = true;
					stack.Push(new Vector2Int(x, y));
				}
			}

			// check up.
			x = xx;
			y = yy + 1;
			if (y < sizeWithPadding)
			{
				Color c = result.GetPixel(x, y);
				if (!visited[x, y])
				{
					visited[x, y] = true;
					stack.Push(new Vector2Int(x, y));
				}
			}

			// check down.
			x = xx;
			y = yy - 1;
			if (y >= 0)
			{
				Color c = result.GetPixel(x, y);
				if (!visited[x, y])
				{
					visited[x, y] = true;
					stack.Push(new Vector2Int(x, y));
				}
			}
		}

		void Fill(int x, int y)
		{
			Color c = original.GetPixel(x + row * size, y + column * size);
			c.a = 1.0f;
			result.SetPixel(x, y, c);
		}
	}

	public static void FloodFillInit(PuzzleCurveType[] types, Stack<Vector2Int> stack, ref bool[,] visited)
	{
		int sizeWithPadding = JigsawPuzzleSetting.Instance.PieceSizeWithPadding;

		for (int i = 0; i < sizeWithPadding; ++i)
		{
			for (int j = 0; j < sizeWithPadding; ++j)
			{
				visited[i, j] = false;
			}
		}

		List<Vector2> pts = new List<Vector2>();
		for (int i = 0; i < types.Length; ++i)
		{
			pts.AddRange(CreateCurve((DirectionType)i, types[i]));
		}

		for (int i = 0; i < pts.Count; ++i)
		{
			var (x, y) = pts[i];
			visited[(int)x, (int)y] = true;
		}

		// start from center.
		Vector2Int start = new Vector2Int(sizeWithPadding / 2, sizeWithPadding / 2);

		visited[start.x, start.y] = true;
		stack.Push(start);
	}

	public static List<Vector2> CreateCurve(DirectionType direction, PuzzleCurveType type)
	{
		int padding = JigsawPuzzleSetting.Instance.PiecePadding;
		int size = JigsawPuzzleSetting.Instance.PieceSize;


		var bezCurve = BezierCurve.PointList2(JigsawPuzzleSetting.Instance.CurvePoints, 0.001f);

		List<Vector2> pts = new List<Vector2>(bezCurve);
		switch (direction)
		{
			case DirectionType.UP:
				if (type == PuzzleCurveType.POSITIVE)
				{
					TranslatePoints(pts, new Vector2(padding, size + padding));
				}
				else if (type == PuzzleCurveType.NEGATIVE)
				{
					InvertY(pts);
					TranslatePoints(pts, new Vector2(padding, size + padding));
				}
				else if (type == PuzzleCurveType.NONE)
				{
					pts.Clear();
					for (int i = 0; i < size; ++i)
					{
						pts.Add(new Vector2(i + padding, padding + size));
					}
				}
				break;

			case DirectionType.RIGHT:
				if (type == PuzzleCurveType.POSITIVE)
				{
					SwapXY(pts);
					TranslatePoints(pts, new Vector2(padding + size, padding));
				}
				else if (type == PuzzleCurveType.NEGATIVE)
				{
					InvertY(pts);
					SwapXY(pts);
					TranslatePoints(pts, new Vector2(padding + size, padding));
				}
				else if (type == PuzzleCurveType.NONE)
				{
					pts.Clear();
					for (int i = 0; i < size; ++i)
					{
						pts.Add(new Vector2(padding + size, i + padding));
					}
				}
				break;

			case DirectionType.DOWN:
				if (type == PuzzleCurveType.POSITIVE)
				{
					InvertY(pts);
					TranslatePoints(pts, new Vector2(padding, padding));
				}
				else if (type == PuzzleCurveType.NEGATIVE)
				{
					TranslatePoints(pts, new Vector2(padding, padding));
				}
				else if (type == PuzzleCurveType.NONE)
				{
					pts.Clear();
					for (int i = 0; i < size; ++i)
					{
						pts.Add(new Vector2(i + padding, padding));
					}
				}
				break;

			case DirectionType.LEFT:
				if (type == PuzzleCurveType.POSITIVE)
				{
					InvertY(pts);
					SwapXY(pts);
					TranslatePoints(pts, new Vector2(padding, padding));
				}
				else if (type == PuzzleCurveType.NEGATIVE)
				{
					SwapXY(pts);
					TranslatePoints(pts, new Vector2(padding, padding));
				}
				else if (type == PuzzleCurveType.NONE)
				{
					pts.Clear();
					for (int i = 0; i < size; ++i)
					{
						pts.Add(new Vector2(padding, i + padding));
					}
				}
				break;
		}
		return pts;

		static void TranslatePoints(List<Vector2> iList, Vector2 offset)
		{
			for (int i = 0; i < iList.Count; ++i)
			{
				iList[i] += offset;
			}
		}

		static void InvertY(List<Vector2> iList)
		{
			for (int i = 0; i < iList.Count; ++i)
			{
				iList[i] = new Vector2(iList[i].x, -iList[i].y);
			}
		}

		static void SwapXY(List<Vector2> iList)
		{
			for (int i = 0; i < iList.Count; ++i)
			{
				iList[i] = new Vector2(iList[i].y, iList[i].x);
			}
		}
	}
}

