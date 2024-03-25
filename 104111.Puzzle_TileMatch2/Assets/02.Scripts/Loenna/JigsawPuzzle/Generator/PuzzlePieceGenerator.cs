using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public partial class PuzzlePieceGenerator : MonoBehaviour
{
    [Header("★ [Reference] Reference")]
	[SerializeField]	private RectTransform m_parent;
	[SerializeField]	private Button m_generateButton;
	[SerializeField]	private Button m_saveButton;
    [SerializeField]	private RawImage m_background;
	private List<PieceData> m_pieces = new();
    private List<Sprite> m_sprites = new();

    [Header("★ [Parameter] Output Settings")]
    public TextureFormat _textureFormat = TextureFormat.RGB24;
    public EncodeFormat _encodeFormat = EncodeFormat.PNG;
    [Range(0, 100)]
    public int _jpgQuality = 100;
    public string outputFolder = "Resources/Images/Puzzle/Piece/";
    private string fullPathFolder;
    private string ext;

    [Header("★ [Settings] Create Puzzle")]
    [SerializeField]
	private Texture2D m_texture;

	[SerializeField]
	private int m_puzzleIndex;

    private void Start()
	{
		m_generateButton.onClick.AddListener(CreatePuzzle);
		m_saveButton.onClick.AddListener(SaveData);

        m_generateTemplateButton.onClick.AddListener(CreateTemplate);
        m_saveTemplateButton.onClick.AddListener(SaveTemplateData);
        m_loopTemplateButton.onClick.AddListener(LoopTemplate);
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

        SaveSprites();
    }

    private void SaveSprites()
    {
        SetSavePath();

        for(int i=0; i < m_sprites.Count; i++)
        {
            StartCoroutine(SaveImage(m_sprites[i].texture, string.Format("{0:00}", i)));
        }

        Debug.Log(CodeManager.GetMethodName() + fullPathFolder);
    }

    private void ClearPieces()
    {
        for(int i = m_parent.childCount-1; i >= 0; i--)
        {
            Destroy(m_parent.GetChild(i).gameObject);
        }
    }

    private void CreatePuzzle()
	{
        m_background.texture = m_texture;
        m_pieces.Clear();
        m_sprites.Clear();
        
        ClearPieces();

		var puzzlePieces = PuzzlePieceMaker.CreatePieceSources(source: m_texture, 5, 5);

		Array.ForEach(
			puzzlePieces, 
			puzzlePiece => {
				var (row, column, position, sprite, filter, curveTypes, _) = puzzlePiece;

				GameObject go = new GameObject($"piece[{row}, {column}]");
				Image image = go.AddComponent<Image>();
				image.rectTransform.SetParentReset(m_parent);
				image.sprite = sprite;
				image.SetNativeSize();
				image.rectTransform.anchoredPosition = position;

				m_pieces.Add(new PieceData(curveTypes, row, column));
                m_sprites.Add(sprite);
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

    


    private IEnumerator SaveImage(Texture2D tex, string fileName)
    {
        SetExtension();
        
        yield return new WaitForEndOfFrame();
    
        var bytes = Encode(tex, _encodeFormat);
        //Destroy(tex);

        string outputFile = string.Format("{0}.{1}", fileName, ext);
        string fullPath = Path.Combine(fullPathFolder, outputFile);
    
        File.WriteAllBytes(fullPath, bytes);

        //Debug.Log(CodeManager.GetMethodName() + fullPath);
    }

    private byte[] Encode(Texture2D texture, EncodeFormat format)
    {
        switch(format)
        {
            case EncodeFormat.PNG:
                return texture.EncodeToPNG();
            case EncodeFormat.JPG:
                return texture.EncodeToJPG(_jpgQuality);
            default:
                return texture.EncodeToJPG(_jpgQuality);
        }
    }

    private void SetSavePath()
    {
        fullPathFolder = Path.Combine(Application.dataPath, outputFolder, string.Format("{0}/", m_puzzleIndex));

        if(!Directory.Exists(fullPathFolder))
            Directory.CreateDirectory(fullPathFolder);
    }

    private void SetExtension()
    {
        switch(_encodeFormat)
        {
            case EncodeFormat.PNG:
                ext = "png";
                break;
            case EncodeFormat.JPG:
                ext = "jpg";
                break;
            default:
                ext = "jpg";
                break;
        }
    }

    public enum EncodeFormat
    {
        PNG,
        JPG
    }
}
