using System;

using Cysharp.Threading.Tasks;

public class MainSceneFragmentContentParameter_Puzzle
	: ScrollViewFragmentContentParameter
{
	public PuzzleManager PuzzleManager;
	public Action OnClick;
	public IUniTaskAsyncEnumerable<PuzzleInfo> OnUpdated;
}