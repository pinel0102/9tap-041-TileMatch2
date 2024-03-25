using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Text;

public partial class PuzzlePieceGenerator
{
    [Header("★ [Parameter] Template Settings")]
    [SerializeField]	private Button m_generateTemplateButton;
    [SerializeField]	private Button m_saveTemplateButton;
    [SerializeField]	private Button m_loopTemplateButton;
    [SerializeField]	private Texture2D m_templateTexture;
    public string templateFolder = "../ExternalDatas/PieceTemplate/";
    public int loopCount = 10;

    private const string jigsawInitial_None = "D";
    private const string jigsawInitial_Positive = "P";
    private const string jigsawInitial_Negative = "N";

    private void LoopTemplate()
    {
        for(int i=0; i < loopCount; i++)
        {
            CreateTemplate();
            SaveTemplateData();
        }
    }
    private void CreateTemplate()
	{
        m_templateTexture = CreateWhiteTexture(750, 750);
        
        m_background.texture = m_templateTexture;
        m_pieces.Clear();
        m_sprites.Clear();
        
        ClearPieces();

		var puzzlePieces = PuzzlePieceMaker.CreatePieceSources(source: m_templateTexture, 5, 5);

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

    private Texture2D CreateWhiteTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                texture.SetPixel(i, j, Color.white);
            }
        }

        texture.Apply();

        return texture;
    }

    private void SaveTemplateData()
    {
		SaveTemplateSprites();
    }

    private void SaveTemplateSprites()
    {
        SetTempleteSavePath();

        for(int i=0; i < m_sprites.Count; i++)
        {
            StartCoroutine(SaveTemplateImage(m_sprites[i].texture, GetTemplateName(m_pieces[i])));
        }

        //Debug.Log(CodeManager.GetMethodName() + fullPathFolder);
    }

    private IEnumerator SaveTemplateImage(Texture2D tex, string fileName)
    {
        SetExtension();
        
        yield return new WaitForEndOfFrame();

        string outputFile = string.Format("{0}.{1}", fileName, ext);
        string fullPath = Path.Combine(fullPathFolder, outputFile);

        if(!File.Exists(fullPath))
        {
            var bytes = Encode(tex, _encodeFormat);
            //Destroy(tex);

            File.WriteAllBytes(fullPath, bytes);
            
            Debug.Log(CodeManager.GetMethodName() + fullPath);
        }
    }

    private void SetTempleteSavePath()
    {
        fullPathFolder = Path.Combine(Application.dataPath, templateFolder);

        if(!Directory.Exists(fullPathFolder))
            Directory.CreateDirectory(fullPathFolder);
    }

    private string GetTemplateName(PieceData piece)
    {
        StringBuilder sb = new StringBuilder();
        for(int i=0; i < piece.PuzzleCurveTypes.Count; i++)
        {
            switch(piece.PuzzleCurveTypes[i])
            {
                case PuzzleCurveType.POSITIVE:
                    sb.Append(jigsawInitial_Positive);
                    break;
                case PuzzleCurveType.NEGATIVE:
                    sb.Append(jigsawInitial_Negative);
                    break;
                case PuzzleCurveType.NONE:
                default:
                    sb.Append(jigsawInitial_None);
                    break;
            }
        }
        return sb.ToString();
    }
}
