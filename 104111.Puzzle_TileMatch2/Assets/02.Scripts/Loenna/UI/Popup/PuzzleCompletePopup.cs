#nullable enable

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Text = NineTap.Constant.Text;
using NineTap.Common;
using TMPro;

public record PuzzleCompletePopupParameter(
    int Index, 
    string PuzzleName, 
    Texture2D Background, 
    Action OnContinue
): DefaultParameterWithoutHUD;

[ResourcePath("UI/Popup/PuzzleCompletePopup")]
public class PuzzleCompletePopup : UIPopup
{
    [SerializeField]
	private RawImage m_backgroundImage = default!;

    [SerializeField]
	private TMP_Text m_puzzleNameText = default!;

    [SerializeField]
	private Button m_backgroundButton = default!;

    [SerializeField]
	private GameObject m_effect = default!;

    [SerializeField]
	private GameObject m_touchLock = default!;

    public int PuzzleIndex;
    public string PuzzleName = string.Empty;

    public override void OnSetup(UIParameter uiParameter)
    {
        base.OnSetup(uiParameter);

		if (uiParameter is not PuzzleCompletePopupParameter parameter)
		{
			OnClickClose();
			return;
		}

        //TableManager tableManager = Game.Inst.Get<TableManager>();
		//RewardDataTable rewardDataTable = tableManager.RewardDataTable;
		//LevelDataTable levelDataTable = tableManager.LevelDataTable;

        PuzzleIndex = parameter.Index;
        PuzzleName = parameter.PuzzleName;
        m_backgroundImage.texture = parameter.Background;
        m_puzzleNameText.SetText(PuzzleName);
        m_backgroundButton.onClick.AddListener(() => { OnClick_Close(parameter.OnContinue); });
        m_effect.SetActive(false);
        m_touchLock.SetActive(true);
    }

    public override void OnShow()
    {
        base.OnShow();

        WaitTime();
    }

    private void OnClick_Close(Action onComplete)
    {
        OnClickClose();        
        UIManager.HUD?.Show(HUDType.ALL);

        onComplete?.Invoke();
    }

    private void WaitTime()
    {
        m_touchLock.SetActive(true);
        m_effect.SetActive(true);

        UniTask.Void(
			async token => {                
                await UniTask.Delay(TimeSpan.FromSeconds(2f));
				m_touchLock.SetActive(false);
			},
			this.GetCancellationTokenOnDestroy()
		);
    }
}
