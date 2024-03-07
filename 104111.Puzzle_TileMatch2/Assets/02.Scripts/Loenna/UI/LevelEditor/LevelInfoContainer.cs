using UnityEngine;
using UnityEngine.UI;

using System.Text;
using System.Linq;
using System.Collections.Generic;

public class LevelInfoContainer : MonoBehaviour
{
    public static readonly string VERSION = "Version: {0}\n\n";
    public static readonly string TILE_COUNT_IN_LEVEL = "Level Tiles";
    public static readonly string TILE_COUNT_IN_BOARD = "Board Tiles";
    public static readonly string TILE_ADDITIONAL_COUNT_IN_LEVEL = "Level Tiles+";
    public static readonly string TILE_ADDITIONAL_COUNT_IN_BOARD = "Board Tiles+";
    public static readonly string BOARD_COUNT_IN_LEVEL = "Board Count";
	public static readonly string BLOCKER_COUNT_FORMAT = "<color=yellow>{0}</color>";
    public static readonly string GOLDTILE_COUNT_IN_LEVEL = "<color=yellow>Gold Tiles</color>";

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

    public void OnUpdateUI(List<(string text, string value)> items)
	{
		m_builder.Clear();
        m_builder.Append(string.Format(VERSION, Application.version));
		m_builder.AppendJoin("\n", items.Select(item => $"{item.text}: {item.value}"));
		m_textBox.text = m_builder.ToString();
	}
}
