using UnityEngine;

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

[Serializable]
public class Layer
{
	[JsonIgnore]
	public readonly Color Color;
	public readonly List<Tile> Tiles;

	public Layer()
	{
		Color = UnityEngine.Random.ColorHSV();
		Tiles = new();
	}
}
