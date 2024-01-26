using UnityEngine;

using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using DG.Tweening;

using NineTap.Payment;
using NineTap.Common;

[ResourcePath("UI/Scene/InitScene")]
public class InitScene : UIScene
{
	[SerializeField]
	private CanvasGroup m_splashView;

	[SerializeField]
	private LoadingView m_loadingView;

    public GameObject m_waitPanel;
	private IAsyncReactiveProperty<PuzzleManager> m_puzzleManager;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

        m_waitPanel.SetActive(false);
		m_puzzleManager = new AsyncReactiveProperty<PuzzleManager>(null);

		m_splashView.alpha = 0f;
		m_loadingView.OnSetup(Game.Inst.Get<TimeManager>());

		string mode = PlayerPrefs.GetString(Constant.Editor.DEVELOP_MODE_SCENE_KEY, Constant.Scene.CLIENT);
		bool editorMode = mode == Constant.Scene.EDITOR;

		if (editorMode)
		{
			m_loadingView.Completed
			.SubscribeAwait(
				async completed => {
					if (!completed)
					{
						return;
					}
					await UniTask.Defer(() => UniTask.Delay(TimeSpan.FromSeconds(0.5f)));
					UIManager.ShowSceneUI<PlayScene>(new PlaySceneParameter());
				}
			);
		}
		else
		{
			m_loadingView.Completed
			.CombineLatest(
				m_puzzleManager,
				(completed, manager) => (completed, manager)
			)
			.SubscribeAwait(
				async result => {
					await UniTask.Defer(() => UniTask.Delay(TimeSpan.FromSeconds(0.5f)));
					if (!result.completed && result.manager == null)
					{
						return;
					}
                    
					UIManager.ShowSceneUI<MainScene>(new MainSceneParameter(PuzzleManager: result.manager));

                    if (GlobalData.Instance.userManager.Current.AppOpenCount == 1)
                    {
                        Debug.Log(CodeManager.GetMethodName() + "Play Level 1");
                        SDKManager.SendAnalytics_C_Scene(NineTap.Constant.Text.Button.PLAY);
                        UIManager.ShowSceneUI<PlayScene>(new PlaySceneParameterCustom(1));
                    }
				}
			);
		}

		UniTask.Void(
			async token => {
				if (!editorMode)
				{
					await UniTask.Defer(() => m_splashView.DOFade(1f, 1f).ToUniTask());
					await UniTask.Defer(() => UniTask.Delay(TimeSpan.FromSeconds(1f)));
					await UniTask.Defer(() => m_splashView.DOFade(0f, 0.5f).ToUniTask());
				}

				await UniTask.Defer(() => UniTask.Delay(TimeSpan.FromSeconds(0.25f)));
				m_loadingView.Show();
				await UniTask.Defer(() => UniTask.Delay(TimeSpan.FromSeconds(0.5f)));
				m_loadingView.StartProgress();
				m_loadingView.Report(0f);

				await UniTask.Defer(() => Initialize(m_loadingView));
			},
			this.GetCancellationTokenOnDestroy()
		);
	}

	public override void OnShow()
	{
		base.OnShow();
	}

	private UniTask Initialize(IProgress<float> progress)
	{
        string mode = PlayerPrefs.GetString(Constant.Editor.DEVELOP_MODE_SCENE_KEY, Constant.Scene.CLIENT);
		bool editorMode = mode == Constant.Scene.EDITOR;

        Debug.Log(CodeManager.GetAsyncName() + string.Format("<color=yellow>Current Mode : {0}</color>", mode));

		TableManager tableManager = Game.Inst.Get<TableManager>();
		UserManager userManager = Game.Inst.Get<UserManager>();
		SoundManager soundManager= Game.Inst.Get<SoundManager>();
		PaymentService paymentService = Game.Inst.Get<PaymentService>();

		soundManager.Load();

		List<UniTask<bool>> tasks = new List<UniTask<bool>>
		{
			userManager.LoadAsync(editorMode, m_waitPanel),
			tableManager.LoadGameData(),
			tableManager.LoadLevelData(editorMode),
		};

		ProductDataTable productDataTable = tableManager.ProductDataTable;
		tasks.Add(paymentService.LoadProducts(productDataTable));

		if (!editorMode)
		{
			tasks.Add(
				UniTask.Defer(
					async () => {
						PuzzleManager puzzleManager = new PuzzleManager(userManager);
						PuzzleDataTable puzzleDataTable = tableManager.PuzzleDataTable;
						User user = userManager.Current;
						PuzzleData puzzleData = puzzleDataTable.FirstOrDefault(index => index == user.CurrentPlayingPuzzleIndex);
						uint placedPieces = user.PlayingPuzzleCollection.TryGetValue(user.CurrentPlayingPuzzleIndex, out uint result)? result : 0;
						uint unlockedPieces = user.UnlockedPuzzlePieceDic == null? 0 : 
							user.UnlockedPuzzlePieceDic.TryGetValue(puzzleData.Key, out uint result2)? 
							result2 : 0;

						await puzzleManager.LoadAsync(puzzleData, placedPieces, unlockedPieces);
						m_puzzleManager.Value = puzzleManager;
                        
						return true;
					}
				)
			);
		}

		int max = tasks.Count;

		return tasks
			.ToUniTaskAsyncEnumerable()
			.ForEachAwaitAsync(
				async (task, index) => {
					bool completed = await UniTask.Defer(() => task).Preserve();
					await UniTask.WaitUntil(() => completed);
					float value = Mathf.Clamp01((index + 1) / (float)max);
					progress.Report(value);
				}
			);
	}
}
