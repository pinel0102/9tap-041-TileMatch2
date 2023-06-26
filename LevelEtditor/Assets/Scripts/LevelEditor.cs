using UnityEngine;

using System.Threading;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class LevelEditor : MonoBehaviour
{
	[SerializeField]
	private BoardView m_boardView;

	[SerializeField]
	private MenuView m_menuView;

	private LevelDataManager m_dataManager;
	private Palette m_palette;

	private async UniTaskVoid Start()
	{
		Canvas.ForceUpdateCanvases();

		await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, this.GetCancellationTokenOnDestroy());

		m_menuView.OnSetup(snap => m_palette.Snapping.Value = snap, m_boardView.VisibleWireFrame);
		m_boardView.OnSetup(position => m_dataManager.AddTileData(position));

		m_dataManager = new LevelDataManager();
		m_palette = new Palette();

		CancellationToken token = this.GetCancellationTokenOnDestroy();
		
		await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.LastPostLateUpdate))
		{
			if (token.IsCancellationRequested)
			{
				return;
			}

			m_boardView.MoveBrush(m_palette.SnappingAmount, Input.mousePosition);
		}

	}
}
