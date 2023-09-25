using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extensions
{
    public static void Deconstruct(this Vector2 v, out float x, out float y)
    {
        x = v.x;
        y = v.y;
    }

    public static Vector2 Swap(this Vector2 v)
    {
        return new Vector2(v.y, v.x);
    }
}
