using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using NineTap.Common;

public class PlaySceneTopUIView : CachedBehaviour
{
	[SerializeField]	private CanvasGroup m_missionCountView;
	[SerializeField]	private TMP_Text m_missionCountText;
    [SerializeField]	private RectTransform m_puzzleIconTransform;

    [SerializeField]	private CanvasGroup m_sweetHolicCountView;
    [SerializeField]	private TMP_Text m_sweetHolicCountText;
    [SerializeField]	private RectTransform m_sweetHolicIconTransform;
    [SerializeField]	private Image m_sweetHolicIconImage;

	[SerializeField]	private TMP_Text m_levelText;
	[SerializeField]	private BoardCountView m_boardCountView;
	[SerializeField]	private GameObject m_hardIcon;

	public BoardCountView BoardCountView => m_boardCountView;
	public RectTransform PuzzleIconTransform => m_puzzleIconTransform;
    public RectTransform SweetHolicIconTransform => m_sweetHolicIconTransform;

	public void OnUpdateUI(int level, bool hardMode, bool includedMission)
	{
		m_levelText.text = $"Level {level}";
		m_hardIcon.SetActive(hardMode);

		m_missionCountView.alpha = 0f;//includedMission? 1f : 0f;
        m_sweetHolicCountView.alpha = GlobalData.Instance.eventSweetHolic_GetCount > 0 ? 1f : 0f;
        m_sweetHolicIconImage.sprite = SpriteManager.GetSprite(GlobalDefine.GetSweetHolic_ItemImagePath());
	}

	public void UpdateMissionCount(int count, int maxCount)
	{
		DOTween.Kill(m_puzzleIconTransform);
		m_puzzleIconTransform.SetLocalScale(Vector2.one);
		m_puzzleIconTransform.DOPunchScale(Vector2.one * 0.25f, 0.25f, vibrato: 1, elasticity: 1f);

		m_missionCountText.text = $"{count}/{maxCount}";
        //GlobalData.Instance.oldGoldPiece = count;
	}

    public void UpdateSweetHolicCount()
	{
        int count = GlobalData.Instance.eventSweetHolic_GetCount;

        DOTween.Kill(m_sweetHolicIconTransform);
		m_sweetHolicIconTransform.SetLocalScale(Vector2.one);
		m_sweetHolicIconTransform.DOPunchScale(Vector2.one * 0.25f, 0.25f, vibrato: 1, elasticity: 1f);

		m_sweetHolicCountText.text = $"{count}";
        m_sweetHolicCountView.alpha = count > 0 ? 1f : 0f;
	}
}
