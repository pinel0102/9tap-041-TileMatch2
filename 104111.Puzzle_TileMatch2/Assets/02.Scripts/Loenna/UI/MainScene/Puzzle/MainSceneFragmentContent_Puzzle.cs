using UnityEngine;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using NineTap.Common;

[ResourcePath("UI/Fragments/Fragment_Puzzle")]
public class MainSceneFragmentContent_Puzzle : ScrollViewFragmentContent
{
	[SerializeField]
	private PuzzlePlayView m_puzzlePlayView;

    public RectTransform objectPool;

	public override void OnSetup(ScrollViewFragmentContentParameter contentParameter)
	{
		if (contentParameter is MainSceneFragmentContentParameter_Puzzle parameter)
		{
			m_puzzlePlayView.OnSetup(parameter.PuzzleManager, parameter.OnClick);

			parameter.OnUpdated.SubscribeAwait(
				result => {
					if (result != null)
					{
						return OnUpdateUI(result.PuzzleData, result.PlacedPieces, result.UnlockedPieces);
					}

					return UniTask.CompletedTask;
				}
			);

			m_puzzlePlayView.OnShow();
		}
	}

	private async UniTask OnUpdateUI(PuzzleData data, uint placePieces, uint unlockedPieces)
	{
		await m_puzzlePlayView.OnShowAsync(data, placePieces, unlockedPieces);
		UIManager.HideLoading();
	}

    public override void OnUpdateUI(User user)
	{ 
        base.OnUpdateUI(user);
        m_puzzlePlayView.CheckUserLevel(user.Level);
	}
}
