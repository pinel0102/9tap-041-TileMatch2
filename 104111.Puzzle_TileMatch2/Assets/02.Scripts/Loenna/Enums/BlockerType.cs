public enum BlockerType : int
{
	None = 0,
	Glue_Left = 1, // Glue_Right 동시 이동.
	Glue_Right = 2, // Glue_Left 동시 이동.
	Bush = 3, // 상하좌우 2타일 선이동 필요.
	Suitcase = 4, // 타일 n개 중첩.
    Jelly = 5, // 다른 타일 3회 이동 후 이동 가능.
    Chain = 6, // 좌우 1타일 선이동 필요.
}

public enum BlockerTypeEditor : int
{
	None = 0,
	Glue = 1, // Glue_Right 동시 이동.
	Bush = 3, // 상하좌우 2타일 선이동 필요.
	Suitcase = 4, // 타일 n개 중첩.
    Jelly = 5, // 다른 타일 3회 이동 후 이동 가능.
    Chain = 6, // 좌우 1타일 선이동 필요.
}