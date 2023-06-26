using UnityEngine;

public enum SnapType : int
{
    NONE = 0,
	FULL = 1,
	HALF = 2,
	EIGHTH = 8,
}

public static class SnapTypeExtensions
{
    public static float GetSnappingAmount(this SnapType snap, float tileSize)
    {
        return snap switch {
            SnapType.NONE => 1f,
            _ => tileSize / (float)snap
        };
    }
}
