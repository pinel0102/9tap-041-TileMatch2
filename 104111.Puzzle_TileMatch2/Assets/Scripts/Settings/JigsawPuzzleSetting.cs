#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Client/Settings/JigsawPuzzleSetting")]
public class JigsawPuzzleSetting : ScriptableObject
{
	private static JigsawPuzzleSetting s_inst = null;
	public static JigsawPuzzleSetting Instance
	{
		get
        {
			if (s_inst == null)
            {
				s_inst = Resources.Load<JigsawPuzzleSetting>(nameof(JigsawPuzzleSetting));

#if UNITY_EDITOR
				if (s_inst == null)
				{
					s_inst = CreateInstance<JigsawPuzzleSetting>();
					string fullPath = $"Assets/Resources/{nameof(JigsawPuzzleSetting)}.asset";
					UnityEditor.AssetDatabase.CreateAsset(s_inst, fullPath);
				}
#endif
			}

			return s_inst;
		}
	}

	[SerializeField]
	private int m_imageSize = 750;

	[SerializeField]
	private int m_piecePadding = 20;

	[SerializeField]
	private int m_pieceSize = 100;

	[SerializeField]
	private List<Vector2> m_curvePoints = new();

	public int ImageSize => Instance.m_imageSize;
	public int PiecePadding => Instance.m_piecePadding;
	public int PieceSize => Instance.m_pieceSize;
	public List<Vector2> CurvePoints => Instance.m_curvePoints;

	public int PieceSizeWithPadding => PieceSize + (PiecePadding * 2);
}
