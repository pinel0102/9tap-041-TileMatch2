using UnityEngine;

using NineTap.Common;

public class PlaySceneButtonContainer : CachedBehaviour
{
	[SerializeField]
	private UIImageButton m_stashButton;

	[SerializeField]
	private UIImageButton m_undoButton;

	[SerializeField]
	private UIImageButton m_shuffleButton;

	[SerializeField]
	private UIImageButton m_pauseButton;

	public void OnSetup(PlaySceneBottomUIViewParameter parameter)
	{
		m_stashButton.OnSetup(parameter.Stash);
		m_undoButton.OnSetup(parameter.Undo);
		m_shuffleButton.OnSetup(parameter.Shuffle);
		m_pauseButton.OnSetup(parameter.Pause);
	}
}
