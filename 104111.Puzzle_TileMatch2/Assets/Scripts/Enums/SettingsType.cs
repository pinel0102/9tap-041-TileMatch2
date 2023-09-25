using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SettingsType 
{
	None,
	Fx,
	Bgm,
	Vibration,
	Notification
}

public static class SettingsTypeExtensions
{
	public static string GetName(this SettingsType type)
	{
		return type switch {
			SettingsType.Fx => "SOUND",
			SettingsType.Bgm => "BGM",
			SettingsType.Vibration => "VIBRATION",
			SettingsType.Notification => "NOTICE",
			_=> string.Empty
		};
	}

	public static string GetIconPath(this SettingsType type, bool isOn)
	{
		string path = type switch {
			SettingsType.Fx => "UI_Setting_Icon_Sound",
			SettingsType.Bgm => "UI_Setting_Icon_BGM",
			SettingsType.Vibration => "UI_Setting_Icon_Viberation",
			SettingsType.Notification => "UI_Setting_Icon_Notice",
			_=> string.Empty
		};

		return isOn? path : $"{path}Off";
	}
}