using System;
using System.Collections.Generic;

[Serializable]
public record Layer(
    int Index,
    List<Tile> Tiles
);
