using UnityEngine;

using System.Collections.Generic;

public class PuzzlePiece
{
	private PuzzleCurveType[] m_curveTypes = new PuzzleCurveType[4] {
		PuzzleCurveType.NONE,
		PuzzleCurveType.NONE,
		PuzzleCurveType.NONE,
		PuzzleCurveType.NONE
	};

	public PuzzleCurveType this[DirectionType directionType]  
	{
		get => m_curveTypes[(int)directionType];
	}

	public PuzzleCurveType[] PuzzleCurveTypes { get => m_curveTypes; set => m_curveTypes = value; }

	private readonly Texture2D m_original;
	private Dictionary<(DirectionType, PuzzleCurveType), LineRenderer> m_lineRenderers = new();

	private Stack<Vector2Int> m_stack = new();
	private bool[,] m_visited;

	public PuzzlePiece(Texture2D texture)
	{
		int sizeWithPadding = JigsawPuzzleSetting.Instance.PieceSizeWithPadding;
		m_visited = new bool[sizeWithPadding, sizeWithPadding];

		m_original = texture;
	}

	public Sprite GetSprite(int row, int column)
	{
		int sizeWithPadding = JigsawPuzzleSetting.Instance.PieceSizeWithPadding;
		var texture = new Texture2D(sizeWithPadding, sizeWithPadding, TextureFormat.ARGB32, false);

		for (int i = 0; i < sizeWithPadding; i++)
		{
			for (int j = 0; j < sizeWithPadding; j++)
			{
				texture.SetPixel(i, j, Color.clear);
			}
		}

		Algorithm.FloodFillInit(m_curveTypes, m_stack, ref m_visited);
		Algorithm.FloodFill(row, column, m_original, texture, m_stack, ref m_visited);
		texture.Apply();

		return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
	}

	public void SetCurveType(DirectionType directionType, PuzzleCurveType curveType)
	{
		m_curveTypes[(int)directionType] = curveType;
	}

	public static LineRenderer CreateLineRenderer(Color color, float lineWidth = 1.0f)
	{
		GameObject obj = new GameObject();

		LineRenderer lr = obj.AddComponent<LineRenderer>();

		lr.startColor = color;
		lr.endColor = color;
		lr.startWidth = lineWidth;
		lr.endWidth = lineWidth;
		lr.material = new Material(Shader.Find("Sprites/Default"));
		return lr;
	}

	// An utility/helper function to show the curve
	// using a LineRenderer.
	public void DrawCurve(DirectionType dir, PuzzleCurveType type, Color color)
	{
		if (!m_lineRenderers.ContainsKey((dir, type)))
		{
			m_lineRenderers.Add((dir, type), CreateLineRenderer(color));
		}

		LineRenderer lr = m_lineRenderers[(dir, type)];
		lr.startColor = color;
		lr.endColor = color;
		lr.gameObject.name = "LineRenderer_" + dir.ToString() + "_" + type.ToString();
		List<Vector2> pts = Algorithm.CreateCurve(dir, type);

		lr.positionCount = pts.Count;
		for (int i = 0; i < pts.Count; ++i)
		{
			lr.SetPosition(i, pts[i]);
		}
	}
}