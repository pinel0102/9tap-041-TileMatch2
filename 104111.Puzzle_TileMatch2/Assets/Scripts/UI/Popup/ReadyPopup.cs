using UnityEngine;

using System;
using System.Collections.Generic;

using TMPro;

using NineTap.Constant;
using NineTap.Common;

public record ReadyPopupParameter
(
	int Level,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
	bool AllPressToClose,
	params HUDType[] HUDTypes
) : PopupBaseParameter(
	Text.LevelText(Level), 
	Text.READY_POPUP_MESSAGE, 
	ExitParameter, 
	BaseButtonParameter,
	AllPressToClose,
	HUDTypes
);

[ResourcePath("UI/Popup/ReadyPopup")]
public class ReadyPopup : PopupBase
{
	[SerializeField]
	private GameObject m_hardMark;

	[SerializeField]
	private List<DynamicImage> m_uiImages;

	[SerializeField]
	private TMP_Text m_tileCountText;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not ReadyPopupParameter parameter)
		{
			Debug.LogError(new InvalidCastException(nameof(ReadyPopupParameter)));
			return;
		}

		LevelDataTable levelDataTable = Game.Inst.Get<TableManager>().LevelDataTable;

		if (!levelDataTable.TryGetValue(parameter.Level, out LevelData levelData))
		{
			Debug.LogError(new NullReferenceException($"LevelDataTable[{parameter.Level}]"));
			OnClickClose();
			return;
		}

		m_hardMark.SetActive(levelData.HardMode);
		m_tileCountText.text = levelData.TileCountAll.ToString();

		m_uiImages.ForEach(image => image.ChangeSprite(Text.LevelModeText(levelData.HardMode)));
	}
}
