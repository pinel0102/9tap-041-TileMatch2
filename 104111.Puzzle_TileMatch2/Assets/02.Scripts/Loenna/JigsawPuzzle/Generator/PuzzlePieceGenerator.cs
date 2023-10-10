using UnityEngine;
using UnityEngine.UI;

using System.IO;
using System.Collections.Generic;
using System;

using Newtonsoft.Json;

public class PuzzlePieceGenerator : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_parent;

	[SerializeField]
	private Texture2D m_texture;

	[SerializeField]
	private Button m_generateButton;

	[SerializeField]
	private Button m_saveButton;

	[SerializeField]
	private int m_puzzleIndex;

	[SerializeField]
	private RawImage m_background;

	private List<PieceData> m_pieces = new();

	private void Start()
	{
		m_generateButton.onClick.AddListener(CreatePuzzle);
		m_saveButton.onClick.AddListener(SaveData);
	}

    private void SaveData()
    {
		string path = Path.Combine(Application.dataPath, 
		$"Resources/DB/GameDataTable/PuzzlePieceDatas/PuzzlePieceData_{m_puzzleIndex}.json");		

		PiecesData data = new PiecesData(m_puzzleIndex, m_pieces);
		string json = JsonConvert.SerializeObject(data);

		using (FileStream fs = new FileStream(path, access: FileAccess.Write, mode: FileMode.OpenOrCreate))
		{
			using (StreamWriter writer = new StreamWriter(fs))
			{
				writer.Write(json);
			}
		}
    }

    private void CreatePuzzle()
	{
		m_background.texture = m_texture;
		m_pieces.Clear();

		var puzzlePieces = PuzzlePieceMaker.CreatePieceSources(source: m_texture, 5, 5);

		Array.ForEach(
			puzzlePieces, 
			puzzlePiece => {
				var (row, column, position, sprite, curveTypes) = puzzlePiece;

				GameObject go = new GameObject($"piece[{row}, {column}]");
				Image image = go.AddComponent<Image>();
				image.rectTransform.SetParentReset(m_parent);
				image.sprite = sprite;
				image.SetNativeSize();
				image.rectTransform.anchoredPosition = position;

				m_pieces.Add(new PieceData(curveTypes, row, column));
			}
		);
	}

	private void Test_1()
	{
		PuzzlePiece puzzle = new PuzzlePiece(m_texture);
		puzzle.SetCurveType(DirectionType.UP, PuzzleCurveType.POSITIVE);
		puzzle.SetCurveType(DirectionType.DOWN, PuzzleCurveType.NEGATIVE);
		puzzle.SetCurveType(DirectionType.RIGHT, PuzzleCurveType.NONE);
		puzzle.SetCurveType(DirectionType.LEFT, PuzzleCurveType.NEGATIVE);

		puzzle.DrawCurve(DirectionType.UP, PuzzleCurveType.POSITIVE, Color.yellow);
		puzzle.DrawCurve(DirectionType.DOWN, PuzzleCurveType.POSITIVE, Color.yellow);
		puzzle.DrawCurve(DirectionType.RIGHT, PuzzleCurveType.POSITIVE, Color.yellow);
		puzzle.DrawCurve(DirectionType.LEFT, PuzzleCurveType.POSITIVE, Color.yellow);
		puzzle.DrawCurve(DirectionType.UP, PuzzleCurveType.NEGATIVE, Color.yellow);
		puzzle.DrawCurve(DirectionType.DOWN, PuzzleCurveType.NEGATIVE, Color.yellow);
		puzzle.DrawCurve(DirectionType.RIGHT, PuzzleCurveType.NEGATIVE, Color.yellow);
		puzzle.DrawCurve(DirectionType.LEFT, PuzzleCurveType.NEGATIVE, Color.yellow);
		puzzle.DrawCurve(DirectionType.UP, PuzzleCurveType.NONE, Color.yellow);
		puzzle.DrawCurve(DirectionType.DOWN, PuzzleCurveType.NONE, Color.yellow);
		puzzle.DrawCurve(DirectionType.RIGHT, PuzzleCurveType.NONE, Color.yellow);
		puzzle.DrawCurve(DirectionType.LEFT, PuzzleCurveType.NONE, Color.yellow);

		//m_image.sprite = puzzle.GetSprite(1, 1);
	}
}
