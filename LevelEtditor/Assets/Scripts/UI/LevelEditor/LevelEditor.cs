using UnityEngine;
using UnityEngine.InputSystem;

using System.Threading;
using System.Linq;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class LevelEditor : MonoBehaviour
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
				OnPointerClick = () => {
					switch (InputController.Instance.WasState)
					{
						case InputController.State.LEFT_BUTTON_PRESSED:
							m_presenter.AddTileInLayer();
							break;
						case InputController.State.RIGHT_BUTTON_PRESSED:
							m_presenter.RemoveTileInLayer();
							break;
					}
				}
			}
		);

		m_menuView.OnSetup(
			new MenuViewParameter {
				OnMoveLevel = m_presenter.LoadLevelBy,
				OnSaveLevel = m_presenter.SaveLevel,
				OnChangedSnapping = (snap) => { m_palette.Snapping.Value = snap; },
				OnVisibleGuide = m_boardView.VisibleWireFrame,
				OnClearTiles = m_boardView.ClearTiles
			}
		);

		m_presenter.LevelMessageBroker.Subscribe(
			info => {
				m_boardView.ClearTiles();
				m_menuView.UpdateLevelUI(info.Level);
				info.Tiles.ForEach(tile => OnDrawCell(tile.position, tile.size));
			}
		);

		m_presenter.BrushMessageBroker.Subscribe(info => {
			var (interactable, drawable, position) = info;
			m_boardView.OnUpdateBrushWidget(position, interactable, drawable);
		});

		m_palette.Subscriber.Subscribe(m_presenter.ChangeSnapping);

		m_presenter.LoadLevelBy(0);

		CancellationToken token = this.GetCancellationTokenOnDestroy();

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

	public void OnDrawCell(Vector2 localPosition, float size)
	{
		m_boardView.OnDraw(localPosition, size);
	}

	public void OnEraseCell(Vector2 localPosition)
	{
		m_boardView.OnRemoveTile(localPosition);
	}

	public void ClearTilesInLayer()
	{
		m_boardView.ClearTiles();
	}
}
