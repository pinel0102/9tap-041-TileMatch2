using UnityEngine;

using TMPro;

using DG.Tweening;

using NineTap.Common;

public class PlaySceneTopUIView : CachedBehaviour
{
	[SerializeField]
	private CanvasGroup m_missionCountView;

	[SerializeField]
	private TMP_Text m_missionCountText;

	[SerializeField]
	private RectTransform m_puzzleIconTransform;

	[SerializeField]
	private TMP_Text m_levelText;

	[SerializeField]
	private BoardCountView m_boardCountView;

	[SerializeField]
	private GameObject m_hardIcon;

	public BoardCountView BoardCountView => m_boardCountView;

	public RectTransform PuzzleIconTransform => m_puzzleIconTransform;

	public void OnUpdateUI(int level, bool hardMode, bool includedMission)
	{
		m_levelText.text = $"Level {level}";
		m_hardIcon.SetActive(hardMode);

		m_missionCountView.alpha = includedMission? 1f : 0f;
	}

	public void UpdateMissionCount(int count, int maxCount)
	{
		DOTween.Kill(m_puzzleIconTransform);
		m_puzzleIconTransform.SetLocalScale(Vector2.one);
		m_puzzleIconTransform.DOPunchScale(Vector2.one * 0.25f, 0.25f, vibrato: 1, elasticity: 1f);

		m_missionCountText.text = $"{count}/{maxCount}";
        GlobalData.Instance.missionCollected = count;
	}
}
