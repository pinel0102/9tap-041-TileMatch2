using UnityEngine;
using System.Collections.Generic;
using NineTap.Common;
using TMPro;
using System;
using Cysharp.Threading.Tasks;

public class MainSceneFragmentContentParameter_Home
: ScrollViewFragmentContentParameter
{
	public HomeFragmentLargeButtonParameter PuzzleButtonParam;
	public UITextButtonParameter PlayButtonParam;
}

[ResourcePath("UI/Fragments/Fragment_Home")]
public class MainSceneFragmentContent_Home : ScrollViewFragmentContent
{
	[SerializeField]
	private List<HomeSideContainer> m_sideContainers;

	[SerializeField]
	private HomeFragmentLargeButton m_puzzleButton;

	[SerializeField]
	private UITextButton m_playButton;

    public RectTransform objectPool;
    public RectTransform rewardPosition_puzzlePiece;
    public RectTransform rewardPosition_goldPiece;
    public TMP_Text goldPieceText;

    [SerializeField]
	private  GameObject puzzleBadgeObject;

    [SerializeField]
	private  TMP_Text puzzleBadgeText;

	public override void OnSetup(ScrollViewFragmentContentParameter contentParameter)
	{
		if (contentParameter is not MainSceneFragmentContentParameter_Home parameter)
        {
            return;
        }

		m_puzzleButton.OnSetup(parameter.PuzzleButtonParam);
		m_playButton.OnSetup(parameter.PlayButtonParam);
        m_sideContainers.ForEach(container => container.OnSetup());

        RefreshGoldPiece(GlobalData.Instance.userManager.Current.GoldPiece, GlobalData.Instance.GetGoldPiece_NextLevel());
        RefreshPuzzleBadge(GlobalData.Instance.userManager.Current.Puzzle);
        RefreshPlayButton();
	}

	public override void OnUpdateUI(User user)
	{
		m_puzzleButton.OnUpdateUI(user);
        RefreshPlayButton();
	}

    public void RefreshPlayButton()
    {
        bool isPlayable = GlobalData.Instance.userManager.Current.Level <= GlobalData.Instance.tableManager.LastLevel;

        m_playButton.SetInteractable(isPlayable);
    }

    public void RefreshPuzzleBadge(long count)
    {
        puzzleBadgeText.SetText(count.ToString());
        puzzleBadgeObject.SetActive(count > 0);
    }

    public void RefreshGoldPiece(int count, int max)
    {
        goldPieceText.SetText(string.Format("{0}/{1}", count, max));
    }

    public void IncreaseGoldPiece(int from, int count, int max, float duration = 0.5f)
    {
        RefreshGoldPiece(from, max);

        UniTask.Void(
			async token => {
                float delay = GetDelay(duration, count);

                for(int i=1; i <= count; i++)
                {
                    RefreshGoldPiece(from + i, max);
                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                }
            },
			this.GetCancellationTokenOnDestroy()
        );

        float GetDelay(float time, int amount)
        {
            return time/(float)amount;
        }
    }
}
