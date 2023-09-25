using System.IO;
using UnityEngine;

public enum MainMenuType: int
{
	COLLECTION = 0,
	JIGSAW_PUZZLE = 1,
	HOME = 2,
	//DAILY_PUZZLE = 4,
	STORE = 3,
	SETTINGS = 4
 }

 public static class MainMenuTypeExtensions
 {
	public static string GetName(this MainMenuType type)
	{
		return type switch {
			MainMenuType.COLLECTION => "Collection",
			MainMenuType.JIGSAW_PUZZLE => "Puzzle",
			MainMenuType.HOME => "Main",
			MainMenuType.STORE => "Store",
			MainMenuType.SETTINGS => "Settings",
			_ => string.Empty
		};
	}

	public static Sprite GetSprite(this MainMenuType type)
	{
		string path = type switch {
			MainMenuType.COLLECTION => "UI_Icon_Collection",
			MainMenuType.JIGSAW_PUZZLE => "UI_Icon_Puzzle",
			MainMenuType.HOME => "UI_Icon_Home",
			MainMenuType.STORE => "UI_Icon_Shop",
			MainMenuType.SETTINGS => "UI_Icon_Option",
			_ => string.Empty
		};

		if (string.IsNullOrWhiteSpace(path))
		{
			return null;
		}

		var sprite = SpriteManager.GetSprite(path);

		return sprite;
	}
}
