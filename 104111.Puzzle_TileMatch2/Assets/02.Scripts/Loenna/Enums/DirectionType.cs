using System;

public enum DirectionType
{
	UP,
	LEFT,
	DOWN,
	RIGHT
}

public enum PuzzleCurveType : uint
{
	NONE = 0,
	POSITIVE = 1 << 0,
	NEGATIVE = 1 << 1
}