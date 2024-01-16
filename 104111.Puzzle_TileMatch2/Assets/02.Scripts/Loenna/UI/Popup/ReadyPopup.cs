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
    Action OnComplete,
	params HUDType[] HUDTypes
) : PopupBaseParameter(
	Text.Popup.Title.GET_STARS, 
	Text.Popup.Message.GET_STARS, 
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

    private Action onComplete;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not ReadyPopupParameter parameter)
		{
			Debug.LogError(new InvalidCastException(nameof(ReadyPopupParameter)));
			return;
		}

        onComplete = parameter.OnComplete;
        m_hardMark.SetActive(false);

		LevelDataTable levelDataTable = Game.Inst.Get<TableManager>().LevelDataTable;

		if (!levelDataTable.TryGetValue(parameter.Level, out LevelData levelData))
		{
			Debug.LogError(new NullReferenceException($"LevelDataTable[{parameter.Level}]"));
			OnClickClose();
			return;
		}
	}

    public override void OnHide()
    {
        base.OnHide();

        onComplete?.Invoke();
    }
}
