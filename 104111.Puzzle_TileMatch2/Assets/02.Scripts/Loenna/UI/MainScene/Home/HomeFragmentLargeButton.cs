using System.Diagnostics;

public class HomeFragmentLargeButtonParameter : UIButtonParameter
{
	public string ButtonText;
	public UIProgressBarParameter GaugeBarParameter;
}

public class HomeFragmentLargeButton : UITextButton
{
	private UIProgressBar m_gaugeBar;
	
	public override void OnSetup(UIButtonParameter buttonParameter)
	{
		base.OnSetup(buttonParameter);

		m_gaugeBar = m_subWidgetParent.GetComponentInChildren<UIProgressBar>();

		if (buttonParameter is HomeFragmentLargeButtonParameter parameter)
		{
			m_textField.text = parameter.ButtonText;
			m_gaugeBar.OnSetup(parameter.GaugeBarParameter);
		}
	}

	public void OnUpdateUI(User user)
	{
		PuzzleData puzzleData = GlobalData.Instance.GetLatestPuzzle();
        if (puzzleData == null) 
            return;

        uint placedPieces = user.PlayingPuzzleCollection.TryGetValue(puzzleData.Index, out uint result)? result : 0;
        
        //UnityEngine.Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", puzzleData.Index, placedPieces));

		int current = 0;
		int max = PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT;

		for (var i = 0; i < max; i++)
		{
			if ((placedPieces & PuzzlePieceMaker.CHECKER << i) != 0)
			{
				current += 1;
			}
		}

		m_textField.text = puzzleData.Name;
		m_gaugeBar.OnUpdateUI(current, max);
	}
}
