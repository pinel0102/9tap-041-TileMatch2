using UnityEngine;
using System.Collections.Generic;
using NineTap.Common;
using TMPro;

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

    public RectTransform rewardPosition_puzzlePiece;
    public RectTransform rewardPosition_goldPiece;
    public TMP_Text goldPieceText;

	public override void OnSetup(ScrollViewFragmentContentParameter contentParameter)
	{
		if (contentParameter is not MainSceneFragmentContentParameter_Home parameter)
        {
            return;
        }

		m_puzzleButton.OnSetup(parameter.PuzzleButtonParam);
		m_playButton.OnSetup(parameter.PlayButtonParam);

		m_sideContainers.ForEach(container => container.OnSetup());
	}

	public override void OnUpdateUI(User user)
	{
		m_puzzleButton.OnUpdateUI(user);
	}
}
