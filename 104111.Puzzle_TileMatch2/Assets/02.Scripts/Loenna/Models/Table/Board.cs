#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gpm.Common.ThirdParty.MessagePack;
using Newtonsoft.Json;

[Serializable]
public class Board
{
	public int NumberOfTileTypes;
	public List<Layer> Layers;
	public int Difficult;
	public int MissionTileCount;
	public int GoldTileIcon;
	
	public Board(IEnumerable<Layer> layers)
	{
		NumberOfTileTypes = 1;
		Layers = new(layers);
		Difficult = (int)DifficultType.NORMAL;
		GoldTileIcon = -1;
		MissionTileCount = 0;
	}

	public Layer? this[int index] => Layers.ElementAtOrDefault(index);

    [JsonIgnore]
    public bool IsEmptyBoard => (Layers.Count == 0) || ((Layers.Count == 1) && Layers.FirstOrDefault().Tiles.Count == 0);
}
