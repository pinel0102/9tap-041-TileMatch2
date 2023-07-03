using UnityEngine;
using UnityEngine.UI;

using System.Text;
using System.Linq;

public class LevelInfoContainer : MonoBehaviour
{
	public static readonly string TILE_COUNT_IN_BOARD = "The number of tiles on the current board";
	public static readonly string TILE_COUNT_IN_LEVEL= "The number of tiles on the current level";

	[SerializeField]
	private Text m_textBox;

	private StringBuilder m_builder = new();

	public void OnUpdateUI(params (string text, int number)[] items)
	{
		m_builder.Clear();
		m_builder.AppendJoin("\n", items.Select(item => $"{item.text}: {item.number}"));
		m_textBox.text = m_builder.ToString();
	}
}
