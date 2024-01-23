using System.Diagnostics;

public class HomeFragmentLargeButtonParameter : UIButtonParameter
{
	public string ButtonText;
	public UIProgressBarParameter GaugeBarParameter;
}

public class HomeFragmentLargeButton : UITextButton
{
	private UIProgressBar m_gaugeBar;
	private PuzzleDataTable m_puzzleDataTable;

	public override void OnSetup(UIButtonParameter buttonParameter)
	{
		base.OnSetup(buttonParameter);

		m_gaugeBar = m_subWidgetParent.GetComponentInChildren<UIProgressBar>();

		m_puzzleDataTable = Game.Inst.Get<TableManager>()?.PuzzleDataTable;

		if (buttonParameter is HomeFragmentLargeButtonParameter parameter)
		{
			m_textField.text = parameter.ButtonText;
			m_gaugeBar.OnSetup(parameter.GaugeBarParameter);
		}
	}

	public void OnUpdateUI(User user)
	{
		int index = user.CurrentPlayingPuzzleIndex;
        UnityEngine.Debug.Log(CodeManager.GetMethodName() + index);
		PuzzleData puzzleData = m_puzzleDataTable?.FirstOrDefault(key => key == index);
		
		if (puzzleData == null || !user.PlayingPuzzleCollection.TryGetValue(index, out uint placedPieces))
		{
			return;
		}

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
