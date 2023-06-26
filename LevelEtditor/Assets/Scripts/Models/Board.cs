using System;
using System.Collections.Generic;

[Serializable]
public record Board(
    int Index,
    List<Layer> Layers
);
