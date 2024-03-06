using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using SimpleFileBrowser;
using NineTap.Common;
using System;
using System.Collections;
using TMPro;

public partial class LevelEditor : SingletonMono<LevelEditor>
{
    [Header("★ [Reference] Editor")]
	[SerializeField]	private GameObject m_loading;
	[SerializeField]	private CanvasGroup m_prevDim;
	[SerializeField]	private int m_cellSize = 80;
	[SerializeField]	private int m_cellCount = 7;
	[SerializeField]	private BoardView m_boardView;
	[SerializeField]	private MenuView m_menuView;
	[SerializeField]	private Text m_dimText;
	[SerializeField]	private Text m_loadingText;
	private LevelEditorPresenter m_presenter;
	[SerializeField]	private Text m_error;
	[SerializeField]	private LevelEditorButton m_revertButton;
	[SerializeField]	private GameObject m_warning;
    [SerializeField]	private Text m_LogMessage;

    [SerializeField]	private TMP_Text m_dataPathText;

    private Coroutine m_LogCoroutine;
    private Coroutine m_DataPathCoroutine;
    private const float delayDelta = 0.1f;
    private WaitForSecondsRealtime wDelayDelta = new WaitForSecondsRealtime(delayDelta);
	
	private TableManager m_tableManager;
	private TimeManager m_timeManager;

	private void Awake()
	{
        GlobalDefine.SetEditorMode(Constant.Scene.EDITOR);

		m_prevDim.gameObject.SetActive(true);
		m_error.gameObject.SetActive(false);
		m_loading.SetActive(false);
		m_prevDim.alpha = 1f;

        InitLog();
        RefreshDataPathText();
	}

	private void OnDestroy()
	{
		m_presenter?.Dispose();
		m_timeManager?.Dispose();
	}

	private void Start()
	{
		m_dimText.text = "데이터 Path 찾는 중...";

		string pathKey = Constant.Editor.DATA_PATH_KEY;

		if (!PlayerPrefs.HasKey(pathKey))
		{
			LoadDataPath();
			return;
		}


		UniTask.Void(
			async token => {
				string path = PlayerPrefs.GetString(pathKey);
				await OnSetup(path);
			},
			this.GetCancellationTokenOnDestroy()
		);
	}

	private void LoadDataPath()
	{
		FileBrowser.ShowLoadDialog(
			onSuccess: async paths => {
				string path = paths[0];
				PlayerPrefs.SetString(Constant.Editor.DATA_PATH_KEY, path);
                RefreshDataPathText();

				await OnSetup(path);
			},
			() => Application.Quit(),
			pickMode: FileBrowser.PickMode.Folders,
			title: "데이터를 저장 할 폴더 선택",
			loadButtonText: "선택"
		);
	}

	private async UniTask OnSetup(string path)
	{
		m_timeManager = new();
		m_tableManager = new();

		await m_tableManager.LoadGameData();

		//Debug.LogError(path);
		m_dimText.text = "데이터 로드 중...";
		m_presenter = new(this, m_cellSize, m_cellCount);

		m_revertButton.OnSetup(async () => await m_presenter.RemoveTemporaryLevel());

		m_boardView.OnSetup(
			new BoardParameter {
				TileSize = m_cellSize,
				CellCount = m_cellCount,
				OnPointerClick = m_presenter.SetTileInLayer,
				OnTakeStep = m_presenter.LoadBoardByStep,
				OnRemove = m_presenter.RemoveBoard,
                OnSeperate = async () => { await m_presenter.SeperateBoard(); },
                OnSeperateAll = async () => { await m_presenter.SeperateBoardAll(); }
			}
		);

		m_menuView.OnSetup(
			new MenuViewParameter {
				SelectLevelContainerParameter = new SelectLevelContainerParameter {
					OnTakeStep = async direction => {
						m_loading.SetActive(true);
						await m_presenter.LoadLevelByStep(direction);
					},
					OnNavigate = async level => {
						m_loading.SetActive(true);
						await m_presenter.LoadLevel(level);
					},
					OnSave = async () => {
						m_loading.SetActive(true);
						await m_presenter.SaveLevel();
					},
					SaveButtonBinder = m_presenter.Savable,
					OnPlay = m_presenter.PlayGame,
					OnControlDifficult = m_presenter.SetDifficult,
					OnChangeMode = m_presenter.UpdateCurrentLevelMode,
                    OnLevelSwap = m_presenter.SwapLevel
				},
				NumberOfContainerParameter = new NumberOfTileTypesContainerParameter {
					Table = m_tableManager.TileDataTable,
					OnTakeStep = m_presenter.IncrementNumberOfTileTypes,
					OnNavigate = m_presenter.SetNumberOfTileTypes,
					OnTakeStepMissionCount = m_presenter.IncrementMissionCount,
					OnNavigateMissionCount = m_presenter.SetMissionCount,
					OnChangeMissionTileIcon = m_presenter.UpdateMissionTileIcon
					//OnChangeBrushMode = mode => {
					//	m_presenter.OnChangeBrushMode(mode);
					//	m_menuView.OnVisibleLayerOption(mode is BrushMode.TILE_BRUSH);
					//},
					//OnPointToLayer = index => {
					//	m_presenter.OnPointToLayer(index);
					//}
				},
                MenuBlockerContainerParameter = new MenuBlockerContainerParameter {
                    OnTakeStep = IncrementBlockerCount,
					OnNavigate =  UpdateBlockerCount,
					OnChangeBlocker = UpdateBlockerIndex,
                    OnTakeStepBlockerICD = IncrementBlockerICD,
                    OnNavigateICD = UpdateBlockerICD,
                    OnAddBlocker = m_presenter.AddBlocker,
                    OnApplyBlocker = m_presenter.ApplyBlocker,
                    OnClearAllBlocker = m_presenter.ClearAllBlocker
                },
				GridOptionContainerParameter = new GridOptionContainerParameter {
					Table = m_tableManager.CountryCodeDataTable,
					OnChangedSnapping = snapping => {
						m_presenter.ChangeSnapping(snapping.GetSnappingAmount(m_cellSize));
					},
					OnVisibleGuide = m_boardView.OnVisibleWireFrame,
					OnReLoadData = () => {
						FileBrowser.ShowLoadDialog(
							onSuccess: async paths => {
								string path = paths[0];
								if (string.IsNullOrWhiteSpace(path))
								{
									return;
								}

								PlayerPrefs.SetString(Constant.Editor.DATA_PATH_KEY, path);
								PlayerPrefs.SetInt(Constant.Editor.LATEST_LEVEL_KEY, 1);
                                RefreshDataPathText();

								await m_presenter.Initialize(path);
							},
							Stub.Nothing,
							pickMode: FileBrowser.PickMode.Folders,
							title: "데이터를 저장 할 폴더 위치 변경",
							loadButtonText: "선택"
						);
					},
					OnChangeCountryCode = index => {	
						if (m_tableManager.CountryCodeDataTable.TryGetValue(index, out var codeData))
						{
							m_presenter.UpdateCountryCode(codeData.Code);
						}
					}
				},
				LayerContainerParameter = new LayerContainerParameter {
					OnRemove = m_presenter.RemoveLayer,
					OnVisible = (index, visible) => m_presenter.SetVisibleLayer(index, !visible),
					OnChangedDrawOrder = m_presenter.ChangeDrawOrder
				}
			}
		);

		try
		{
			await m_presenter.Initialize(path);

			CancellationToken token = this.GetCancellationTokenOnDestroy();

			// 편집 이벤트가 발생해 에디터 state가 변경되어 case에 따라 ui 업데이트를 하도록 함.
			m_presenter.UpdateState.Subscribe(
				action: info => {
					switch (info)
					{
						case CurrentState.AllUpdated all: // 레벨 변경시 모든 ui가 변경되어야 함
							int codeIndex = m_tableManager.CountryCodeDataTable.GetIndex(all.CountryCode);
							codeIndex = Mathf.Max(0, codeIndex);
							var all_current = all.Boards[all.BoardIndex];
							m_boardView.OnUpdateBoardView(all.BoardCount, all.BoardIndex);
							m_boardView.OnUpdateLayerView(all.CurrentLayers);
							m_menuView.UpdateLevelUI(all.LastLevel, all.CurrentLevel, codeIndex);
							m_menuView.UpdateGrades((DifficultType)all_current.DifficultType, all.HardMode);
							m_menuView.UpdateNumberOfTileTypesUI(all.BoardIndex, all.NumberOfTileTypesCurrent, all_current.MissionCount, all_current.GoldTileIcon);
							m_menuView.UpdateLayerUI(all.CurrentLayers, m_presenter.InvisibleLayerIndexes);
							m_menuView.UpdateLevelInfoUI(all.BoardCount, all.TileCountInBoard, all.TileCountAll, all.GoldTileCount, all.BlockerDic);
                            m_menuView.UpdateBlockerUI(blockerType, blockerCount, blockerVariableICD);
							break;
						case CurrentState.BoardUpdated board: //맵
							var current = board.Boards[board.BoardIndex];
							m_boardView.OnUpdateBoardView(board.BoardCount, board.BoardIndex);
							m_boardView.OnUpdateLayerView(board.CurrentLayers);
							m_menuView.UpdateLayerUI(board.CurrentLayers, m_presenter.InvisibleLayerIndexes);
							m_menuView.UpdateGrades((DifficultType)current.DifficultType, board.HardMode);
							m_menuView.UpdateLevelInfoUI(board.BoardCount, board.TileCountInBoard, board.TileCountAll, board.GoldTileCount, board.BlockerDic);
							m_menuView.UpdateNumberOfTileTypesUI(board.BoardIndex, board.NumberOfTileTypesCurrent, current.MissionCount, current.GoldTileIcon);
							break;
						case CurrentState.NumberOfTileTypesUpdated numberOfTileTypes: // 타일 종류 개수
                            m_menuView.UpdateLevelInfoUI(numberOfTileTypes.BoardCount, numberOfTileTypes.TileCountInBoard, numberOfTileTypes.TileCountAll, numberOfTileTypes.GoldTileCount, numberOfTileTypes.BlockerDic);
							m_menuView.UpdateNumberOfTileTypesUI(
								numberOfTileTypes.BoardIndex, 
								numberOfTileTypes.NumberOfTileTypesCurrent,
								numberOfTileTypes.Boards[numberOfTileTypes.BoardIndex].MissionCount,
								numberOfTileTypes.Boards[numberOfTileTypes.BoardIndex].GoldTileIcon
							);
							break;
						case CurrentState.TileUpdated tile: //타일
							m_boardView.OnUpdateLayerView(tile.Layers);
							m_menuView.UpdateLayerUI(tile.Layers, m_presenter.InvisibleLayerIndexes);
							m_menuView.UpdateLevelInfoUI(tile.Boards.Count, tile.TileCountInBoard, tile.TileCountAll, tile.GoldTileCount, tile.BlockerDic);
							break;
						case CurrentState.DifficultUpdated { Difficult: var difficult, HardMode: var mode}:
							m_menuView.UpdateGrades(difficult, mode);
                            break;
                        case CurrentState.BlockerUpdated blocker:
                            m_menuView.UpdateBlockerUI(blockerType, blockerCount, blockerVariableICD);
							break;
					}

					m_prevDim.alpha = 0f;
					m_prevDim.gameObject.SetActive(false);
					m_loading.SetActive(false);
				}
			);

			m_presenter.BrushMessageBroker.Subscribe(info => {
				var (mode, interactable, position) = info;
				m_boardView.OnUpdateBrushWidget(mode, position, interactable);
			});

			m_presenter.InvisibleLayersBroker.Subscribe(m_boardView.OnInvisibleLayer);
		}
		catch
		{
			PlayerPrefs.DeleteKey(Constant.Editor.LATEST_LEVEL_KEY);
			m_error.gameObject.SetActive(true);
		}

		m_timeManager.OnUpdateEveryFrame += _ => {
			Vector2 inputPosition = InputController.Instance.ToLocalPosition(m_boardView.transform);
			m_presenter?.SetBrushPosition(inputPosition);
		};
	}

	public void SetVisibleLoading(bool visible)
	{
		m_loading.SetActive(visible);
	}

	public void SetVisibleWarning(bool visible)
	{
		m_revertButton.gameObject.SetActive(visible);
		m_warning.SetActive(visible);
	}

    private void InitLog()
    {
        if (m_LogCoroutine != null)
            StopCoroutine(m_LogCoroutine);

        m_LogMessage.gameObject.SetActive(false);
    }

    public void SetLog(string message, bool isWarning = false, bool showLog = true)
    {
        if (showLog)
        {
            if (isWarning)
                Debug.LogWarning(CodeManager.GetAsyncName(index:1) + string.Format("<color=yellow>{0}</color>", message));
            else
                Debug.Log(CodeManager.GetAsyncName(index:1) + string.Format("<color=yellow>{0}</color>", message));
        }

        m_LogMessage.text = message;
        m_LogMessage.gameObject.SetActive(true);
        
        if (m_LogCoroutine != null)
            StopCoroutine(m_LogCoroutine);

        m_LogCoroutine = StartCoroutine(CO_WaitTime());
    }

    private IEnumerator CO_WaitTime(float logTime = 3f)
    {
        while (logTime > 0)
        {
            logTime -= delayDelta;
            yield return wDelayDelta;
        }

        m_LogMessage.gameObject.SetActive(false);
    }

    public void RefreshDataPathText()
    {
        string path = PlayerPrefs.GetString(Constant.Editor.DATA_PATH_KEY);

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>DataPath : {0}</color>", path));

        m_dataPathText.SetText(string.Format("DataPath : {0}", path));
        m_dataPathText.gameObject.SetActive(true);

        if (m_DataPathCoroutine != null)
            StopCoroutine(m_DataPathCoroutine);

        m_DataPathCoroutine = StartCoroutine(CO_DataPathWaitTime());
    }

    private IEnumerator CO_DataPathWaitTime(float logTime = 3f)
    {
        while (logTime > 0)
        {
            logTime -= delayDelta;
            yield return wDelayDelta;
        }

        m_dataPathText.gameObject.SetActive(false);
    }
}
