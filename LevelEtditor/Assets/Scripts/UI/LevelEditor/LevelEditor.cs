using UnityEngine;
using UnityEngine.InputSystem;

using System.Threading;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public partial class LevelEditor : MonoBehaviour
{
	[SerializeField]
	private int m_cellSize = 80;

	[SerializeField]
	private int m_cellCount = 7;

	[SerializeField]
	private BoardView m_boardView;

	[SerializeField]
	private MenuView m_menuView;

	private LevelEditorPresenter m_presenter;
	private Palette m_palette;

	private async UniTaskVoid Start()
	{
		Mouse mouse = Mouse.current;

		m_presenter = new(this, m_cellSize, m_cellCount);
		m_palette = new Palette(m_cellSize);

		m_boardView.OnSetup(
			new BoardParameter {
				TileSize = m_cellSize,
				CellCount = m_cellCount,
				OnPointerClick = () => m_presenter.SetTileInLayer(InputController.Instance.WasState)
			}
		);

		m_menuView.OnSetup(
			new MenuViewParameter {
				SelectLevelContainerParameter = new SelectLevelContainerParameter {
					OnTakeStep = m_presenter.LoadLevelByStep,
					OnNavigate = m_presenter.LoadLevel,
					OnSave = m_presenter.SaveLevel
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

		CancellationToken token = this.GetCancellationTokenOnDestroy();

		// 편집 이벤트가 발생해 에디터 state가 변경되어 case에 따라 ui 업데이트를 하도록 함.
		m_presenter.UpdateState.WithoutCurrent().SubscribeAwait(
			onNext: async info => {
				await UniTask.Yield(
					timing: PlayerLoopTiming.LastPostLateUpdate,
					cancellationToken: token
				);

				switch (info)
				{
					case CurrentState.AllUpdated all: // 레벨 변경시 모든 ui가 변경되어야 함
						m_boardView.OnUpdateLayerView(all.Layers);
						m_menuView.UpdateLevelUI(all.LastLevel, all.CurrentLevel);
						m_menuView.UpdateNumberOfTileTypesUI(all.NumberOfTileTypes);
						m_menuView.UpdateLayerUI(all.Layers.Count, all.LayerIndex);
						break;
					case CurrentState.NumberOfTileTypesUpdated numberOfTileTypes: // 타일 종류 개수
						m_menuView.UpdateNumberOfTileTypesUI(numberOfTileTypes.NumberOfTileTypes);
						break;
					case CurrentState.LayerUpdated layer: // 레이어
						m_boardView.OnUpdateLayerView(layer.Layers);
						m_menuView.UpdateLayerUI(layer.Layers.Count, layer.LayerIndex);
						break;
				}
			},
			cancellationToken: token 
		);

		m_presenter.BrushMessageBroker.Subscribe(info => {
			var (interactable, drawable, position) = info;
			m_boardView.OnUpdateBrushWidget(position, interactable, drawable);
		});

		m_palette.Subscriber.Subscribe(m_presenter.ChangeSnapping);

		m_presenter.LoadLevelByStep(0);


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

	public void OnDrawTile(int layerIndex, Vector2 localPosition, float size)
	{
		m_boardView.OnDrawTile(layerIndex, localPosition, size);
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
