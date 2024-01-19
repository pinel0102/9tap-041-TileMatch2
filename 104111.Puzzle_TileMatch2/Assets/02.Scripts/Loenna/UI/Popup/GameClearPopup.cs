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

using static UnityEngine.SceneManagement.SceneManager;

public record GameClearPopupParameter(int Level, Action<int> OnContinue): DefaultParameterWithoutHUD;

[ResourcePath("UI/Popup/GameClearPopup")]
public class GameClearPopup : UIPopup
{
	[SerializeField]
	private Image m_headLineImage = default!;

    [SerializeField]
	private RectTransform m_BgEffect = default!;

	[SerializeField]
	private ClearRewardListContainer m_clearRewardContainer = default!;

	[SerializeField]
	private UIGaugeBar m_gaugeBar = default!;

	[SerializeField]
	private UITextButton m_confirmButton = default!;

    [SerializeField]
	private CanvasGroup m_clearStarCanvasGroup = default!;

    [SerializeField]
	private CanvasGroup m_clearStarHalo = default!;

    [SerializeField]
    private GameObject m_landmarkObject = default!;

    [SerializeField]
    private GameObject m_resultObject = default!;

    [SerializeField]
    private GameObject m_openPuzzleObject = default!;

    [SerializeField]
	private CanvasGroup m_openPuzzleStarCanvasGroup = default!;

    [SerializeField]
	private CanvasGroup m_openPuzzleHalo = default!;

    [SerializeField]
	private CanvasGroup m_layoutResult = default!;

    [SerializeField]
	private CanvasGroup m_layoutPuzzle = default!;
    
    [SerializeField]
    private TMP_Text m_openPuzzleText = default!;

    [SerializeField]
	private GameObject m_openPuzzleContinue = default!;

    [SerializeField]
	private GameObject m_effect = default!;

	private LevelData? m_levelData;	
	private RewardData? m_chestRewardData;
    
    public bool isButtonInteractable;
    public bool isPuzzleInteractable;
    public int openPuzzleIndex;

    private const string format_openPuzzle = "You have unlocked\n{0}!";
    
    public override void OnSetup(UIParameter uiParameter)
    {
        base.OnSetup(uiParameter);

		if (uiParameter is not GameClearPopupParameter parameter)
		{
			OnClickClose();
			return;
		}

        TableManager tableManager = Game.Inst.Get<TableManager>();
		RewardDataTable rewardDataTable = tableManager.RewardDataTable;
		LevelDataTable levelDataTable = tableManager.LevelDataTable;

        int goldPieceCount = GlobalData.Instance.missionCollected;    
        openPuzzleIndex = GlobalData.Instance.GetOpenedPuzzleIndex(parameter.Level);            

		if (!levelDataTable.TryGetValue(parameter.Level, out m_levelData))
		{
			OnClickClose();
			return;
		}

        SetButtonInteractable(false);
        SetPuzzleInteractable(false);
        m_openPuzzleContinue.SetActive(false);
        
		bool existNextLevel = levelDataTable.TryGetValue(parameter.Level + 1, out var nextLevelData);
		RewardData rewardData = rewardDataTable.GetDefaultReward(m_levelData.HardMode);
		
        m_clearStarCanvasGroup.alpha = 0;
		m_clearRewardContainer.OnSetup(rewardData);

		bool existChestReward = rewardDataTable.TryPreparedChestReward(parameter.Level, out m_chestRewardData);
        //bool existChestReward = rewardDataTable.TryPreparedChestReward(10, out m_chestRewardData);

		if (existChestReward)
		{
			m_gaugeBar.OnSetup(
				new UIGaugeBarParameter {
					CurrentNumber = Mathf.Max(0, parameter.Level - 1),
					MaxNumber = m_chestRewardData?.Level ?? 0
				}
			);
		}

		m_confirmButton.OnSetup(
			new UITextButtonParameter {
				ButtonText = existNextLevel? Text.Button.CONTINUE : Text.Button.HOME,
				OnClick = () => {
                    if (isButtonInteractable)
                    {
					    OnClickClose();
					    OnExit();
                    }
				}
			}
		);

		m_confirmButton.Alpha = 0f;
        m_confirmButton.interactable = false;
		m_gaugeBar.Alpha = 0f;
		m_clearRewardContainer.Alpha = 0f;
		m_headLineImage.transform.localScale = Vector3.zero;
        m_BgEffect.SetLocalScale(0);
        m_landmarkObject.SetActive(openPuzzleIndex > 0);

        m_resultObject.SetActive(true);
        m_openPuzzleObject.SetActive(false);
        m_effect.SetActive(false);

		void OnExit()
		{
			string mode = PlayerPrefs.GetString(Constant.Editor.DEVELOP_MODE_SCENE_KEY, Constant.Scene.CLIENT);

			if (mode == Constant.Scene.CLIENT)
			{
                UIManager.ShowSceneUI<MainScene>(new MainSceneRewardParameter(
                    clearedLevel:parameter.Level,
                    openPuzzleIndex:openPuzzleIndex,
                    rewardCoin:rewardData.Coin,
                    rewardPuzzlePiece:rewardData.PuzzlePiece,
                    rewardGoldPiece:goldPieceCount
                ));
                
                if (UIManager.IsEnableReviewPopup(parameter.Level))
                    UIManager.ShowReviewPopup();
			}
			else
			{
				LoadScene(Constant.Scene.EDITOR);
			}
		}
    }

    public override void OnShow()
    {
        base.OnShow();

        m_BgEffect.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

		UniTask.Void(
			async token => {
                
                await m_headLineImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                m_effect.SetActive(true);
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

                await PlayHaloAsync(m_clearStarHalo, m_clearStarCanvasGroup, token);
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

				if (!ObjectUtility.IsNullOrDestroyed(m_clearRewardContainer))
				{
					await m_clearRewardContainer.ShowAsync();
				}

				if (!ObjectUtility.IsNullOrDestroyed(m_gaugeBar))
				{
                    SoundManager soundManager = Game.Inst.Get<SoundManager>();
                    soundManager?.PlayFx(Constant.Sound.SFX_PROGRESS);

					int level = m_levelData?.Key ?? 0;
					int prev = Mathf.Max(level - 1, 0);                    
					await m_gaugeBar.OnUpdateUIAsync(prev, m_levelData?.Key ?? 0, m_chestRewardData?.Level ?? 0);

					if (m_levelData?.Key! >= m_chestRewardData?.Level!)
					{
						UIManager.ShowPopupUI<RewardPopup>(
							new RewardPopupParameter (
								PopupType: RewardPopupType.CHEST,
								Reward: m_chestRewardData,
								HUDType.COIN
							)
						);
					}
				}

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
                    await ShowContinueButton();
                }
			},
			this.GetCancellationTokenOnDestroy()
		);
    }

    public async UniTask ShowContinueButton()
    {
        await DOTween.To(
            getter: () => 0f,
            setter: alpha => {
                if (!ObjectUtility.IsNullOrDestroyed(m_confirmButton))
                {
                    m_confirmButton.Alpha = alpha;
                }
            },
            endValue: 1f,
            0.1f
        )
        .OnComplete(() => { m_confirmButton.interactable = true;})
        .ToUniTask()
        .SuppressCancellationThrow();

        SetButtonInteractable(true);
    }

    private async UniTask SetPopupOpenPuzzle(PuzzleData puzzleData, CancellationToken token)
    {
        m_openPuzzleText.SetText(string.Format(format_openPuzzle, puzzleData.Name));
        m_openPuzzleContinue.SetActive(false);

        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        await PlayHaloAsync(m_openPuzzleHalo, m_openPuzzleStarCanvasGroup, token);

        m_layoutPuzzle.alpha = 0;

        /*await m_layoutResult
            .DOFade(0, 0.25f)
            .ToUniTask()
			.SuppressCancellationThrow();*/

        m_resultObject.SetActive(false);
        m_openPuzzleObject.SetActive(true);

        SoundManager soundManager = Game.Inst.Get<SoundManager>();
        soundManager?.PlayFx(Constant.Sound.SFX_REWARD_OPEN);

        await m_layoutPuzzle
            .DOFade(1f, 0.5f)
            .ToUniTask()
			.SuppressCancellationThrow();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        m_openPuzzleContinue.SetActive(true);
        SetPuzzleInteractable(true);
    }

    public async void SetPopupResult()
    {
        if (!isPuzzleInteractable) return;

        m_layoutResult.alpha = 0;

        /*await m_layoutPuzzle
            .DOFade(0, 0.25f)
            .ToUniTask()
			.SuppressCancellationThrow();*/

        m_resultObject.SetActive(true);
        m_openPuzzleObject.SetActive(false);

        /*await m_layoutResult
            .DOFade(1f, 0.25f)
            .ToUniTask()
			.SuppressCancellationThrow();*/

        m_layoutResult.alpha = 1;

        await ShowContinueButton();
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

    private void SetButtonInteractable(bool interactable)
    {
        isButtonInteractable = interactable;
        //Debug.Log(CodeManager.GetMethodName() + isButtonInteractable);
    }

    private void SetPuzzleInteractable(bool interactable)
    {
        isPuzzleInteractable = interactable;
        //Debug.Log(CodeManager.GetMethodName() + isPuzzleInteractable);
    }
}
