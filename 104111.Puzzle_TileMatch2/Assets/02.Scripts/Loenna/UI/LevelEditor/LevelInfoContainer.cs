using UnityEngine;
using UnityEngine.UI;

using System.Text;
using System.Linq;

public class LevelInfoContainer : MonoBehaviour
{
    public static readonly string VERSION = "Version: {0}\n\n";
    public static readonly string BOARD_COUNT_IN_LEVEL = "Board Count";
	public static readonly string TILE_COUNT_IN_BOARD = "Tiles on board";
	public static readonly string TILE_COUNT_IN_LEVEL = "Tiles on level";
	public static readonly string MISSION_COUNT_IN_BOARD = "<color=yellow>Mission tiles</color> on board";
	public static readonly string MISSION_COUNT_IN_LEVEL = "<color=yellow>Mission tiles</color> on level";

	[SerializeField]
	private Text m_textBox;

	private StringBuilder m_builder = new();

	public void OnUpdateUI(params (string text, string number)[] items)
	{
		m_builder.Clear();
        m_builder.Append(string.Format(VERSION, Application.version));
		m_builder.AppendJoin("\n", items.Select(item => $"{item.text}: {item.number}"));
		m_textBox.text = m_builder.ToString();
	}
}
