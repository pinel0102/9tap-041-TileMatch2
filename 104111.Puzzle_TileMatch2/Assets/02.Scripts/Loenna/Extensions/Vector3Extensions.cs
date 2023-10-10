using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
	public static void Deconstruct(this Vector3 v, out float x, out float y, out float z)
	{
		x = v.x;
		y = v.y;
		z = v.z;
	}
}
