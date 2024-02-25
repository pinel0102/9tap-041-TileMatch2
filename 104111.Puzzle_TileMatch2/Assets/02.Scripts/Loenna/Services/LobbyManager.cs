using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;

using NineTap.Constant;

public class LobbyManager : IDisposable
{
	private readonly UserManager m_userManager;
	private readonly TableManager m_tableManager;

	private readonly IDisposableAsyncReactiveProperty<string> m_currentLevel;
	private readonly IDisposableAsyncReactiveProperty<PuzzleInfo> m_onUpdatePuzzle;

	public IReadOnlyAsyncReactiveProperty<string> CurrentLevel => m_currentLevel;
	public IAsyncReactiveProperty<PuzzleInfo> OnUpdatePuzzle => m_onUpdatePuzzle;

	public LobbyManager(UserManager userManager, TableManager tableManager)
	{
        m_userManager = userManager;
		m_tableManager = tableManager;

		User user = m_userManager.Current;

		LevelDataTable levelDataTable = m_tableManager.LevelDataTable;
		LevelData levelData = levelDataTable.FirstOrDefault(level => level == user.Level);
        m_currentLevel = new AsyncReactiveProperty<string>(user.Level > tableManager.LastLevel ? Text.Button.COMING_SOON : levelData?.GetMainButtonText()).WithDispatcher();
		m_onUpdatePuzzle = new AsyncReactiveProperty<PuzzleInfo>(null).WithDispatcher();

		m_userManager.OnUpdated += user => {
            var levelData = levelDataTable.FirstOrDefault(index => index == user.Level);
			m_currentLevel.Value = user.Level > tableManager.LastLevel ? Text.Button.COMING_SOON : levelData?.GetMainButtonText();
		};
	}

	public void OnSelectPuzzle(PuzzleData puzzleData, uint placedPieces, uint unlockedPieces)
	{
		m_onUpdatePuzzle.Value = new PuzzleInfo(puzzleData, placedPieces, unlockedPieces);
        SDKManager.SendAnalytics_I_Puzzle(puzzleData.Index);
	}

	public async void OnCheckShowPopup(Action onMoveShop)
	{
		User user = m_userManager.Current;

        if (user.Level > GlobalData.Instance.tableManager.LastLevel)
            return;

        var (_, valid, _) = user.Valid();

        if (!valid)
        {
            SDKManager.SendAnalytics_C_Scene(Text.Button.STORE);
            onMoveShop?.Invoke();
            return;
        }

        GlobalData.Instance.eventSweetHolic_IsBoosterTime = user.IsEventBoosterTime(GameEventType.SweetHolic);

        if (GlobalDefine.IsEnable_HardBundle())
        {
            await GlobalData.Instance.ShowPopup_HardLevel();
        }

        SDKManager.SendAnalytics_C_Scene(Text.Button.PLAY);
        UIManager.ShowSceneUI<PlayScene>(new PlaySceneParameter());
	}

	public void Dispose()
	{
		m_currentLevel?.Dispose();
		m_onUpdatePuzzle?.Dispose();
	}
}
