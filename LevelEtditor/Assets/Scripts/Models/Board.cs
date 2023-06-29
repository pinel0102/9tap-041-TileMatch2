#nullable enable

using System;
using System.Collections.Generic;

[Serializable]
public record Board(
	List<Layer> Layers
)
{
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
