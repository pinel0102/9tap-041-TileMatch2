#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Board
{
	public List<Layer> Layers;
	
	public Board(IEnumerable<Layer> layers)
	{
		Layers = new(layers);
	}

	public Layer? this[int index] => Layers.ElementAtOrDefault(index);
}
