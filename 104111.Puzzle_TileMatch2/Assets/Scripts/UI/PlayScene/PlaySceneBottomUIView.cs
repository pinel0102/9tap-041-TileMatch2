using UnityEngine;

using NineTap.Common;

public class PlaySceneBottomUIViewParameter
{
	public UIImageButtonParameter Stash;
	public UIImageButtonParameter Undo;
	public UIImageButtonParameter Shuffle;
	public UIImageButtonParameter Pause;
}

public class PlaySceneBottomUIView : CachedBehaviour
{
	[SerializeField]
	private PlaySceneStashContainer m_stashContainser;

	[SerializeField]
	private PlaySceneBasketView m_basketView;

	[SerializeField]
	private PlaySceneButtonContainer m_buttonsView;

	public PlaySceneBasketView BasketView => m_basketView;
	public PlaySceneStashContainer StashView => m_stashContainser;

	public void OnSetup(PlaySceneBottomUIViewParameter parameter)
	{
		m_buttonsView.OnSetup(parameter);
	}
}
