public enum SkillItemType : int
{
	None = 0,
	Stash = 1, // 3개의 타일을 바구니 앞에 배치
	Undo = 2, // 되돌리기
	Shuffle = 3, // 섞기
	VAT = 4, // 이어하기 추가 코인
}

public enum ProductType
{
	Unknown,
	Coin,
	Heart,
	PuzzlePiece,
	StashItem,
	UndoItem,
	ShuffleItem,
	HeartBooster,
    Landmark,
    ADBlock
}

public static class ProductTypeExtensions
{
	public static string GetIconName(this ProductType type)
	{
		return type switch {
			ProductType.Coin => "UI_Icon_Coin",
			ProductType.PuzzlePiece => "UI_Icon_GoldPuzzle_Big",
			ProductType.StashItem => "UI_Shop_Icon_Hint",
			ProductType.UndoItem => "UI_Btn_Undo",
			ProductType.ShuffleItem => "UI_Shop_Icon_Shuffle",
			ProductType.HeartBooster or ProductType.Heart => "UI_Shop_Icon_Heart",
            ProductType.Landmark => "UI_Icon_LandmarkPuzzle",
            ProductType.ADBlock => "UI_Icon_AdBlock",
			_ => string.Empty
		};
	}

	public static HUDType GetHUDType(this ProductType type)
	{
		return type switch {
			ProductType.Coin => HUDType.COIN,
			ProductType.PuzzlePiece => HUDType.STAR,
			_ => HUDType.NONE
		};
	}
}

public static class SkillItemTypeExtensions
{
	public static int GetPaidCoin(this SkillItemType type)
	{
		return type switch {
			SkillItemType.Stash => 70,
			SkillItemType.Undo => 20,
			SkillItemType.Shuffle => 10,
			SkillItemType.VAT => 10,
			_ => 0
		};
	}

	public static ProductType GetProductType(this SkillItemType type)
	{
		return type switch {
			SkillItemType.Stash => ProductType.StashItem,
			SkillItemType.Undo => ProductType.UndoItem,
			SkillItemType.Shuffle => ProductType.ShuffleItem,
			_ => ProductType.Unknown
		};
	}
}

public static class RewardItemTypeExtensions
{
	public static bool TryGetSkillItemType(this ProductType type, out SkillItemType itemType)
	{
		(bool success, SkillItemType resultType) = type switch {
			ProductType.StashItem => (true, SkillItemType.Stash),
			ProductType.UndoItem => (true, SkillItemType.Undo),
			ProductType.ShuffleItem => (true, SkillItemType.Shuffle),
			_ => (false, SkillItemType.None)
		};

		itemType = resultType;
		return success;
	}
}