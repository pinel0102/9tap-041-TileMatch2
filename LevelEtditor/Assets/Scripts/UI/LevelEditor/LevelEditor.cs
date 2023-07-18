using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using System.Threading;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using SimpleFileBrowser;

public partial class LevelEditor : MonoBehaviour
{
	private const string PLAYER_PREPS_KEY = "path";

	[SerializeField]
	private GameObject m_loading;

	[SerializeField]
	private CanvasGroup m_prevDim;

	[SerializeField]
	private int m_cellSize = 80;

	[SerializeField]
	private int m_cellCount = 7;

	[SerializeField]
	private BoardView m_boardView;

	[SerializeField]
	private MenuView m_menuView;

	[SerializeField]
	private Text m_dimText;

	[SerializeField]
	private Text m_loadingText;

	private LevelEditorPresenter m_presenter;
	private Palette m_palette;

	private void Awake()
	{
		m_loading.SetActive(false);
		m_prevDim.alpha = 1f;
	}

	private void OnDestroy()
	{
		m_presenter?.Dispose();
	}

	private void Start()
	{	
		m_dimText.text = "데이터 Path 찾는 중...";

		FileBrowser.ShowLoadDialog(
			onSuccess: async paths => {
				string path = paths[0];
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
		m_dimText.text = "데이터 로드 중...";
		m_presenter = new(this, path, m_cellSize, m_cellCount);
		m_palette = new Palette(m_cellSize);

		m_boardView.OnSetup(
			new BoardParameter {
				TileSize = m_cellSize,
				CellCount = m_cellCount,
				OnPointerClick = m_presenter.SetTileInLayer,
				OnTakeStep = m_presenter.LoadBoardByStep,
				OnRemove = m_presenter.RemoveBoard
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
					DataPath = path,
					OnVisibleDim = (visible, text) => {
						m_loading.SetActive(visible);
						m_loadingText.text = text;
					},
					DataManager = m_presenter.DataManager,
					OnControlDifficult = m_presenter.SetDifficult
				},
				NumberOfContainerParameter = new NumberOfTileTypesContainerParameter {
					OnTakeStep = m_presenter.IncrementNumberOfTileTypes,
					OnNavigate = m_presenter.SetNumberOfTileTypes
				},
				GridOptionContainerParameter = new GridOptionContainerParameter {
					OnChangedSnapping = (snap) => { m_palette.Snapping.Value = snap; },
					OnVisibleGuide = m_boardView.OnVisibleWireFrame
				},
				LayerContainerParameter = new LayerContainerParameter {
					OnCreate = m_presenter.AddLayer,
					OnRemove = m_presenter.RemoveLayer,
					OnClear = m_presenter.ClearTilesInLayer,
					OnSelect = m_presenter.SelectLayer,
					OnVisible = m_boardView.OnVisibleLayer
				}
			}
		);

		await m_presenter.Initialize();

		CancellationToken token = this.GetCancellationTokenOnDestroy();

		// 편집 이벤트가 발생해 에디터 state가 변경되어 case에 따라 ui 업데이트를 하도록 함.
		m_presenter.UpdateState.SubscribeAwait(
			onNext: async info => {
				await UniTask.Yield(
					timing: PlayerLoopTiming.LastPostLateUpdate,
					cancellationToken: token
				);

				switch (info)
				{
					case CurrentState.AllUpdated all: // 레벨 변경시 모든 ui가 변경되어야 함
						m_boardView.OnUpdateBoardView(all.BoardCount, all.BoardIndex);
						m_boardView.OnUpdateLayerView(all.CurrentBoard, all.LayerIndex);
						m_menuView.UpdateLevelUI(all.LastLevel, all.CurrentLevel);
						m_menuView.UpdateDifficult(all.Difficult);
						m_menuView.UpdateNumberOfTileTypesUI(all.BoardIndex, all.NumberOfTileTypesCurrent);
						m_menuView.UpdateLayerUI(all.CurrentLayerColors, all.LayerIndex);
						m_menuView.UpdateLevelInfoUI(all.TileCountInBoard, all.TileCountAll);
						break;
					case CurrentState.BoardUpdated board: //맵
						m_boardView.OnUpdateBoardView(board.BoardCount, board.BoardIndex);
						m_boardView.OnUpdateLayerView(board.CurrentBoard, board.LayerIndex);
						m_menuView.UpdateLayerUI(board.CurrentLayerColors, board.LayerIndex);
						m_menuView.UpdateLevelInfoUI(board.TileCountInBoard, board.TileCountAll);
						m_menuView.UpdateNumberOfTileTypesUI(board.BoardIndex, board.NumberOfTileTypesCurrent);
						break;
					case CurrentState.NumberOfTileTypesUpdated numberOfTileTypes: // 타일 종류 개수
						m_menuView.UpdateNumberOfTileTypesUI(numberOfTileTypes.BoardIndex, numberOfTileTypes.NumberOfTileTypesCurrent);
						break;
					case CurrentState.LayerUpdated layer: // 레이어
						m_boardView.OnUpdateLayerView(layer.Layers, layer.LayerIndex);
						m_menuView.UpdateLayerUI(layer.CurrentColors, layer.LayerIndex);
						m_menuView.UpdateLevelInfoUI(layer.TileCountInBoard, layer.TileCountAll);
						break;
					case CurrentState.TileUpdated tile: //타일
						m_menuView.UpdateLevelInfoUI(tile.TileCountInBoard, tile.TileCountAll);
						break;
					case CurrentState.DifficultUpdated { difficult : var difficult }:
						m_menuView.UpdateDifficult(difficult);
						break;
				}

				m_prevDim.alpha = 0f;
				m_prevDim.gameObject.SetActive(false);
				m_loading.SetActive(false);
			},
			cancellationToken: token 
		);

		m_presenter.BrushMessageBroker.Subscribe(info => {
			var (interactable, drawable, position) = info;
			m_boardView.OnUpdateBrushWidget(position, interactable, drawable);
		});

		m_palette.Subscriber.Subscribe(m_presenter.ChangeSnapping);

		await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.LastPostLateUpdate))
		{
			if (token.IsCancellationRequested)
			{
				return;
			}

			Vector2 inputPosition = InputController.Instance.ToLocalPosition(m_boardView.transform);
			m_presenter.SetBrushPosition(inputPosition);
		}
	}

	public void OnDrawTile(int layerIndex, Vector2 localPosition, float size, Color color)
	{
		m_boardView.OnDrawTile(layerIndex, localPosition, size, color);
	}

	public void OnEraseTile(int layerIndex, Vector2 localPosition)
	{
		m_boardView.OnEraseTile(layerIndex, localPosition);
	}

	public void ClearTilesInLayer(int layerIndex)
	{
		m_boardView.ClearTilesInLayer(layerIndex);
	}
}
