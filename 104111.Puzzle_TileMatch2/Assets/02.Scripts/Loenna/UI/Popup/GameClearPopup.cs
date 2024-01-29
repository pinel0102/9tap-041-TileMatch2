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
	[SerializeField]	private Image m_headLineImage = default!;
    [SerializeField]	private RectTransform m_BgEffect = default!;
	[SerializeField]	private ClearRewardListContainer m_clearRewardContainer = default!;
	[SerializeField]	private UIGaugeBar m_gaugeBar = default!;
	[SerializeField]	private UITextButton m_confirmButton = default!;
    [SerializeField]	private CanvasGroup m_clearStarCanvasGroup = default!;
    [SerializeField]	private CanvasGroup m_clearStarHalo = default!;
    [SerializeField]    private GameObject m_landmarkObject = default!;
    [SerializeField]    private GameObject m_resultObject = default!;
    [SerializeField]    private GameObject m_openPuzzleObject = default!;
    [SerializeField]	private CanvasGroup m_openPuzzleStarCanvasGroup = default!;
    [SerializeField]	private CanvasGroup m_openPuzzleHalo = default!;
    [SerializeField]	private CanvasGroup m_layoutResult = default!;
    [SerializeField]	private CanvasGroup m_layoutPuzzle = default!;
    [SerializeField]    private TMP_Text m_rewardRibbonText = default!;
    [SerializeField]    private TMP_Text m_openPuzzleText = default!;
    [SerializeField]	private GameObject m_openPuzzleContinue = default!;
    [SerializeField]	private GameObject m_rewardChest = default!;
    [SerializeField]	private GameObject m_rewardLandmark = default!;
    [SerializeField]	private GameObject m_effect = default!;

	private LevelData? m_levelData;	
	private RewardData? m_chestRewardData;
    
    public bool isButtonInteractable;
    public bool isPuzzleInteractable;
    public int clearedLevel;
    public int openPuzzleIndex;
    private int gageFrom;
    private int gageTo;
    private int gageMax;

    private const string string_reward_default = "Gift for you";
    private const string string_reward_landmark = "New Landmark";
    private const string string_reward_default_landmark = "New Landmark & Reward";    
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

        clearedLevel = parameter.Level;

        int goldPieceCount = GlobalData.Instance.missionCollected;    
        openPuzzleIndex = GlobalData.Instance.GetOpenedPuzzleIndex(clearedLevel);            

		if (!levelDataTable.TryGetValue(parameter.Level, out m_levelData))
		{
			OnClickClose();
			return;
		}

        SetButtonInteractable(false);
        SetPuzzleInteractable(false);
        m_openPuzzleContinue.SetActive(false);
        
		bool existNextLevel = levelDataTable.TryGetValue(clearedLevel + 1, out var nextLevelData);
		RewardData rewardData = rewardDataTable.GetDefaultReward(m_levelData.HardMode);
		
        m_clearStarCanvasGroup.alpha = 0;
		m_clearRewardContainer.OnSetup(rewardData);

		bool existChestTarget = rewardDataTable.TryPreparedChestReward(clearedLevel, out m_chestRewardData);
        bool existChestReward = false;
        
		if (existChestTarget)
		{
            rewardDataTable.TryPreviousChestReward(clearedLevel, out var m_previousReward);

            int prevChestLevel = m_previousReward?.Level ?? (clearedLevel > Constant.Game.LEVEL_PUZZLE_START ? Constant.Game.LEVEL_PUZZLE_START : 0);
            
            gageMax = clearedLevel > Constant.Game.LEVEL_PUZZLE_START ? m_chestRewardData.Level - prevChestLevel : Constant.Game.LEVEL_PUZZLE_START;
            gageTo = clearedLevel - prevChestLevel;
            gageFrom = gageTo - 1;

			m_gaugeBar.OnSetup(
				new UIGaugeBarParameter {
					CurrentNumber = gageFrom,
					MaxNumber = gageMax
				}
			);

            existChestReward = clearedLevel >= m_chestRewardData.Level;
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
        m_rewardLandmark.SetActive(clearedLevel <= Constant.Game.LEVEL_PUZZLE_START);
        m_rewardChest.SetActive(clearedLevel > Constant.Game.LEVEL_PUZZLE_START);
        m_rewardRibbonText.SetText(openPuzzleIndex > 0 ? 
                                    (existChestReward ? string_reward_default_landmark : string_reward_landmark) : 
                                    string_reward_default);
        m_landmarkObject.SetActive(openPuzzleIndex > 0 || existChestReward);

        m_resultObject.SetActive(true);
        m_openPuzzleObject.SetActive(false);
        m_effect.SetActive(false);

		void OnExit()
		{
			string mode = PlayerPrefs.GetString(Constant.Editor.DEVELOP_MODE_SCENE_KEY, Constant.Scene.CLIENT);

			if (mode == Constant.Scene.CLIENT)
			{
                SDKManager.SendAnalytics_C_Scene_Clear(existNextLevel? Text.Button.CONTINUE : Text.Button.HOME);
                GlobalDefine.RequestAD_Interstitial();
                
                UIManager.ShowSceneUI<MainScene>(new MainSceneRewardParameter(
                    clearedLevel:parameter.Level,
                    openPuzzleIndex:openPuzzleIndex,
                    rewardCoin:rewardData.Coin,
                    rewardPuzzlePiece:rewardData.PuzzlePiece,
                    rewardGoldPiece:goldPieceCount
                ));
                
                if (GlobalDefine.IsEnableReviewPopup(parameter.Level))
                    GlobalData.Instance.ShowReviewPopup();
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

					await m_gaugeBar.OnUpdateUIAsync(gageFrom, gageTo, gageMax);

					if (m_levelData?.Key! >= m_chestRewardData?.Level!)
					{
						UIManager.ShowPopupUI<RewardPopup>(
							new RewardPopupParameter (
								PopupType: RewardPopupType.CHEST,
								Reward: m_chestRewardData,
                                VisibleHUD: HUDType.COIN
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
