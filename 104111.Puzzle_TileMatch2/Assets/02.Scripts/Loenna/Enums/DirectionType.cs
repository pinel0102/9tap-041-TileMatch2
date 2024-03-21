using System;

public enum DirectionType
{
	UP,
	LEFT,
	DOWN,
	RIGHT
}

/// <summary>
/// <para>Up, Left, Down, Right</para>
/// <para>NONE : Default</para>
/// <para>POSITIVE : In</para>
/// <para>NEGATIVE : Out</para>
/// </summary>
public enum PuzzleCurveType : uint
{
	NONE = 0,
	POSITIVE = 1 << 0,
	NEGATIVE = 1 << 1
}