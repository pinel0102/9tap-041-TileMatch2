#nullable enable

using UnityEngine;
using UnityEngine.UI;

using System;

using DG.Tweening;

using Cysharp.Threading.Tasks;

using Text = NineTap.Constant.Text;
using NineTap.Common;

using static UnityEngine.SceneManagement.SceneManager;

public record GameClearPopupParameter(int Level, Action<int> OnContinue): DefaultParameterWithoutHUD;

[ResourcePath("UI/Popup/GameClearPopup")]
public class GameClearPopup : UIPopup
{
	[SerializeField]
	private Image m_headLineImage = default!;

	[SerializeField]
	private ClearRewardListContainer m_clearRewardContainer = default!;

	[SerializeField]
	private UIGaugeBar m_gaugeBar = default!;

	[SerializeField]
	private UITextButton m_confirmButton = default!;

	private LevelData? m_levelData;	
	private RewardData? m_chestRewardData;

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

		if (!levelDataTable.TryGetValue(parameter.Level, out m_levelData))
		{
			OnClickClose();
			return;
		}

		bool existNextLevel = levelDataTable.TryGetValue(parameter.Level + 1, out var nextLevelData);
		RewardData rewardData = rewardDataTable.GetDefaultReward(m_levelData.HardMode);
		
		m_clearRewardContainer.OnSetup(rewardData);

		bool existChestReward = rewardDataTable.TryPreparedChestReward(parameter.Level, out m_chestRewardData);

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
					OnClickClose();
					if (existNextLevel)
					{
						UIManager.ShowPopupUI<ReadyPopup>(
							new ReadyPopupParameter(
								Level: nextLevelData.Key,
								BaseButtonParameter: new UITextButtonParameter {
									ButtonText = Text.Button.PLAY,
									OnClick = () => parameter?.OnContinue?.Invoke(nextLevelData.Key)
								},
								ExitParameter: new ExitBaseParameter(OnExit, false),
								AllPressToClose: true
							)
						);
						return;
					}

					OnExit();
				}
			}
		);

		m_confirmButton.Alpha = 0f;
		m_gaugeBar.Alpha = 0f;
		m_clearRewardContainer.Alpha = 0f;
		m_headLineImage.transform.localScale = Vector3.one * 0.1f;

		void OnExit()
		{
			string mode = PlayerPrefs.GetString(Constant.Editor.DEVELOP_MODE_SCENE_KEY, Constant.Scene.CLIENT);

			if (mode == Constant.Scene.CLIENT)
			{
				UIManager.ShowSceneUI<MainScene>(new DefaultParameter());
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

		UniTask.Void(
			async token => {
				if (!ObjectUtility.IsNullOrDestroyed(m_headLineImage))
				{
					await m_headLineImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
					await UniTask.Delay(TimeSpan.FromSeconds(0.25f));
				}

				if (!ObjectUtility.IsNullOrDestroyed(m_clearRewardContainer))
				{
					await m_clearRewardContainer.ShowAsync();
				}

				if (!ObjectUtility.IsNullOrDestroyed(m_gaugeBar))
				{
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
				.ToUniTask()
				.SuppressCancellationThrow();

			},
			this.GetCancellationTokenOnDestroy()
		);
    }
}
