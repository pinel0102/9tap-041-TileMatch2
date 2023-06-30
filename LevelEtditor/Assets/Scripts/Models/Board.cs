#nullable enable

using System;
using System.Collections.Generic;

[Serializable]
public class Board
{
	public List<Layer> Layers;
	
	public Board(IEnumerable<Layer> layers)
	{
		Layers = new(layers);
	}

	public Layer? this[int index] 
	{
		get
		{
			return Layers.HasIndex(index) switch {
				true => Layers[index],
				_ => default(Layer)
			};
		}
	}
}
