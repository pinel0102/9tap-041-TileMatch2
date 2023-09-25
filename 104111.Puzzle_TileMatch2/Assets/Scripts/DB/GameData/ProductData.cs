using UnityEngine;

using System.Collections.Generic;

public enum PaymentType
{
	None,
	Coin,
	IAP
}

public enum UIType : int
{
	None,
	Gold,
	Common,
	Uncommon,
	Epic,
	Hard,
	CheerUp,
	Piggy,
	Pass,
	Banner
}

public static class UITypeExtensions
{
	public static Color GetValueWidgetColor(this UIType type)
	{
		return type switch {
			UIType.Common => new Color32(0xb6, 0x84, 0xf1, 0xff),
			UIType.Uncommon => new Color32(0x57, 0xa7, 0x96, 0xff),
			UIType.Epic => new Color32(0xe7, 0x8b, 0xec, 0xff),
			UIType.Gold => new Color32(0xea, 0xbe, 0xf4, 0xff),
			_ => Color.clear
		};
	}

	public static string GetBundleImagePath(this UIType type)
	{
		return type switch {
			UIType.Common => "UI_Shop_bg_Bundle_Common",
			UIType.Uncommon => "UI_Shop_bg_Bundle_Uncommon",
			UIType.Epic => "UI_Shop_bg_Bundle_Epic",
			_ => "UI_Popup_Item"
		};
	}

	public static Color GetBundleInfoColor(this UIType type)
	{
		return type switch {
			UIType.Common => new Color32(0xcd, 0x9b, 0xff, 0xff),
			UIType.Uncommon => new Color32(0x6f, 0xc3, 0xa0, 0xff),
			UIType.Epic => new Color32(0xf0, 0xaa, 0xf4, 0xff),
			_ => Color.clear
		};
	}

	public static string GetCircleIconPath(this UIType type)
	{
		return type switch {
			UIType.Common => "UI_Lobby_Bg_Common",
			UIType.Uncommon => "UI_Lobby_Bg_Uncommon",
			UIType.Epic => "UI_Lobby_Bg_Epic",
			UIType.Piggy => "UI_Lobby_Bg_Piggy",
			_ => "UI_Lobby_Bg_Common"
		};
	}

	public static string GetEventLabelImagePath(this UIType type)
	{
		return type switch {
			UIType.Common => "UI_Lobby_Bg_Label_Common",
			UIType.Uncommon => "UI_Lobby_Bg_Label_Uncommon",
			UIType.Epic => "UI_Lobby_Bg_Label_Epic",
			_ => "UI_Lobby_Bg_Label_Common"
		};
	}

	public static string GetEventTimeImagePath(this UIType type)
	{
		return type switch {
			UIType.Uncommon => "UI_Lobby_Bg_Time_Uncommon",
			_ => "UI_Lobby_Bg_Time_Epic",
		};
	}
}

public record ProductData
(
	int Index,
	string ProductId,
	PaymentType PaymentType,
	UIType UIType,
	string ImagePath,
	float Price,
	int EfficiencyPercent,
	int DiscountPercent,
	string FullName,
	string SimplifiedName,
	string Description,
	ProductExposingType Required,
	int Coin,
	IReadOnlyDictionary<int, long> Contents
) : TableRowData<int>(Index)
{

	public string GetShopItemImagePath()
	{
		if (string.IsNullOrWhiteSpace(ImagePath))
		{
			return string.Empty;
		}

		return $"UI_Shop_{ImagePath}";
	}

	public string GetLobbyItemImagePath()
	{
		if (string.IsNullOrWhiteSpace(ImagePath))
		{
			return string.Empty;
		}

		return $"UI_Lobby_Icon_{ImagePath}";
	}

	public string GetPriceString()
	{
		if (Price <= 0)
		{
			return "Free";
		}

		return $"{Price} USD";
	}

	public string GetSaleString()
	{
		if (DiscountPercent <= 0)
		{
			return string.Empty;
		}
		return $"{DiscountPercent}% SALE";
	}
}