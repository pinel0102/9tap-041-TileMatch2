#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

using NineTap.Constant;

[Serializable]
public record LevelData(
	Guid Guid,
	int Key,
	List<Board> Boards,
	bool HardMode,
	string CountryCode
) : TableRowData<int>(Key)
{
	[JsonConstructor]
	public LevelData(int key, List<Board> boards)
		: this (Guid.NewGuid(), key, boards, false, "default")
	{
		
	}

	public static LevelData CreateData(int level)
	{
        //UnityEngine.Debug.Log(CodeManager.GetMethodName() + level);
        
		List<Board> boards = new();
		List<Layer> layers = new();
		layers.Add(new Layer());
		boards.Add(new Board(layers));

		return new LevelData(level, boards);
	}

	[JsonIgnore]
	public Board? this[int index] => Boards.ElementAtOrDefault(index);
	
	[JsonIgnore]
	public int TileCountAll => Boards?.Sum(board => board?.Layers?.Sum(Layer => Layer.Tiles?.Count() ?? 0)) ?? 0;

	[JsonIgnore]
	public int MissionCountAll => Boards?.Sum(board => board?.Layers?.Sum(Layer => Layer.Tiles?.Where(tile => tile.IncludeMission).Count() ?? 0)) ?? 0;

	public string GetMainButtonText()
	{
		StringBuilder builder = new(Text.LevelText(Key));
		if (HardMode)
		{
			builder.Append("\n<size=39>HARD</size>");
		}

		return builder.ToString();
	}
}
