#nullable enable

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using NineTap.Common;
using TMPro;

public record PuzzleOpenPopupParameter(int OpenPuzzleIndex): DefaultParameterWithoutHUD;

[ResourcePath("UI/Popup/PuzzleOpenPopup")]
public class PuzzleOpenPopup : UIPopup
{
	[SerializeField]	private RectTransform m_BgEffect = default!;
	[SerializeField]    private GameObject m_openPuzzleObject = default!;
    [SerializeField]	private CanvasGroup m_openPuzzleStarCanvasGroup = default!;
    [SerializeField]	private CanvasGroup m_openPuzzleHalo = default!;
    [SerializeField]	private CanvasGroup m_layoutPuzzle = default!;
    [SerializeField]    private TMP_Text m_openPuzzleText = default!;
    [SerializeField]	private GameObject m_openPuzzleContinue = default!;
    [SerializeField]	private GameObject m_effect = default!;

	public bool isInteractable;
    public int openPuzzleIndex;

    private const string format_openPuzzle = "You have unlocked\n{0}!";
    
    public override void OnSetup(UIParameter uiParameter)
    {
        base.OnSetup(uiParameter);

		if (uiParameter is not PuzzleOpenPopupParameter parameter)
		{
			OnClickClose();
			return;
		}

        GlobalData.Instance.userManager.UpdatePuzzleOpenIndex(puzzleOpenPopupIndex: -1);
        openPuzzleIndex = parameter.OpenPuzzleIndex;

		SetInteractable(false);
        m_openPuzzleContinue.SetActive(false);
		m_openPuzzleObject.SetActive(false);
        m_BgEffect.SetLocalScale(0);
        m_effect.SetActive(false);
    }

    public override void OnShow()
    {
        base.OnShow();

        m_BgEffect.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

		UniTask.Void(
			async token => {
                
                m_effect.SetActive(true);                
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f));
                
                if (openPuzzleIndex > 0)
                {   
                    if(GlobalData.Instance.tableManager.PuzzleDataTable.TryGetValue(openPuzzleIndex, out PuzzleData puzzleData))
                    {
                        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Open Puzzle {0}</color>", openPuzzleIndex));
                        await SetPopupOpenPuzzle(puzzleData, token);
                    }
                }
                else
                {
                    m_openPuzzleContinue.SetActive(true);
                    SetInteractable(true);
                }
			},
			this.GetCancellationTokenOnDestroy()
		);
    }

    private async UniTask SetPopupOpenPuzzle(PuzzleData puzzleData, CancellationToken token)
    {
        m_openPuzzleText.SetText(string.Format(format_openPuzzle, puzzleData.Name));
        m_openPuzzleContinue.SetActive(false);

        await PlayHaloAsync(m_openPuzzleHalo, m_openPuzzleStarCanvasGroup, token);

        m_layoutPuzzle.alpha = 0;

        m_openPuzzleObject.SetActive(true);

        SoundManager soundManager = Game.Inst.Get<SoundManager>();
        soundManager?.PlayFx(Constant.Sound.SFX_REWARD_OPEN);

        await m_layoutPuzzle
            .DOFade(1f, 0.5f)
            .ToUniTask()
			.SuppressCancellationThrow();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        m_openPuzzleContinue.SetActive(true);
        SetInteractable(true);
    }

    public async UniTask PlayHaloAsync(CanvasGroup halo, CanvasGroup container, CancellationToken token)
	{
		await container
			.DOFade(1f, 0.25f)
			.ToUniTask()
			.SuppressCancellationThrow();

		UniTaskAsyncEnumerable
			.EveryUpdate(PlayerLoopTiming.LastPostLateUpdate) 
			.ForEachAsync(
				_ => {
					if (token.IsCancellationRequested)
					{
						return;
					}

					ObjectUtility.GetRawObject(halo)?.transform.Rotate(Vector3.forward * 0.1f);
				}
			).Forget();
	}

    public void OnPress_Close()
    {
        if (isInteractable)
        {
            OnClickClose();
        }
    }

    private void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        //Debug.Log(CodeManager.GetMethodName() + isPuzzleInteractable);
    }
}
