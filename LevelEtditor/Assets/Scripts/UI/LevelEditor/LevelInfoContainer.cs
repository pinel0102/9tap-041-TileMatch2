using UnityEngine;
using UnityEngine.UI;

using System.Text;

public class LevelInfoContainer : MonoBehaviour
{
	public static readonly string TILE_COUNT_IN_BOARD = "현 보드의 타일 수";
	public static readonly string TILE_COUNT_IN_LEVEL= "현 레벨의 타일 수";

	[SerializeField]
	private Text m_textBox;

	private StringBuilder m_builder = new();

	public void OnUpdateUI(params (string, int)[] item)
	{
		m_builder.Clear();
		m_builder.AppendJoin("\n", item);
		m_textBox.text = m_builder.ToString();
	}
}
