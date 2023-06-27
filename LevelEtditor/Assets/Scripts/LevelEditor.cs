using UnityEngine;

using System.Threading;

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
		m_presenter = new(this, m_cellSize, m_cellCount);
		m_palette = new Palette(m_cellSize);

		m_boardView.OnSetup(
			new BoardParameter {
				TileSize = m_cellSize,
				CellCount = m_cellCount,
				OnPointerClick = m_presenter.AddTileInLayer
			}
		);

		m_menuView.OnSetup(
			new MenuViewParameter {
				OnChangedSnapping = (snap) => { m_palette.Snapping.Value = snap; },
				OnVisibleGuide = m_boardView.VisibleWireFrame,
				OnClearTiles = m_boardView.ClearTiles
			}
		);

		m_presenter.BrushBroker.Subscribe(info => {
			var (interactable, drawable, position) = info;
			m_boardView.OnUpdateBrushWidget(position, interactable, drawable);
		});

		m_palette.Subscriber.Subscribe(m_presenter.ChangeSnapping);

		CancellationToken token = this.GetCancellationTokenOnDestroy();

		await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.LastPostLateUpdate))
		{
			if (token.IsCancellationRequested)
			{
				return;
			}

			Vector2 inputPosition = m_boardView.transform.InverseTransformPoint(Input.mousePosition);
			m_presenter.SetBrushPosition(inputPosition);
		}
	}

	public void OnDrawCell(Vector2 localPosition)
	{
		m_boardView.OnDraw(localPosition);
	}

	public void ClearTilesInLayer()
	{
		m_boardView.ClearTiles();
	}
}
