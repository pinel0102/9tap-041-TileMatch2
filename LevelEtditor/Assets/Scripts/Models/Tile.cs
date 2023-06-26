using UnityEngine;

using System;

[Serializable]
public record Tile(
    int Type,
    Vector2 Position
);
