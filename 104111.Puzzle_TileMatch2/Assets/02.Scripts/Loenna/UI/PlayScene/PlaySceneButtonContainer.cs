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
	private GameObject m_stashLocked;

	[SerializeField]
	private GameObject m_undoLocked;

	[SerializeField]
	private GameObject m_shuffleLocked;

	[SerializeField]
	private UIImageButton m_pauseButton;

	public void OnSetup(PlaySceneBottomUIViewParameter parameter)
	{
		m_stashButton.OnSetup(parameter.Stash);
		m_undoButton.OnSetup(parameter.Undo);
		m_shuffleButton.OnSetup(parameter.Shuffle);
		m_pauseButton.OnSetup(parameter.Pause);
	}

    public void RefreshSkillLocked(int level)
    {
        m_undoLocked.SetActive(level < GlobalDefine.TutorialLevels[1]);
        m_stashLocked.SetActive(level < GlobalDefine.TutorialLevels[2]);
        m_shuffleLocked.SetActive(level < GlobalDefine.TutorialLevels[3]);
        m_undoButton.gameObject.SetActive(!m_undoLocked.activeSelf);
        m_stashButton.gameObject.SetActive(!m_stashLocked.activeSelf);
        m_shuffleButton.gameObject.SetActive(!m_shuffleLocked.activeSelf);
    }
}
