using UnityEngine;

using System.Linq;

using Cysharp.Threading.Tasks;

using Gpm.Ui;

using NineTap.Common;

[ResourcePath("UI/Fragments/Fragment_Collection")]
public class MainSceneFragmentContent_Collection : ScrollViewFragmentContent
{
	[SerializeField]
	private InfiniteScroll m_scrollView;

	public override void OnSetup(ScrollViewFragmentContentParameter contentParameter)
	{
		if (contentParameter is not MainSceneFragmentContentParameter_Collection parameter) 
		{
			return;
		}

		UserManager userManager = Game.Inst.Get<UserManager>();
		
		TableManager tableManager = Game.Inst.Get<TableManager>();
		CountryCodeDataTable countryCodeDataTable = tableManager.CountryCodeDataTable;
		PuzzleDataTable puzzleDataTable = tableManager.PuzzleDataTable;

		User user = userManager.Current;

		PuzzleThemeContainerItemData[] puzzleThemeContainerItemDatas = puzzleDataTable.CollectionKeys
			.Select(
				key => new PuzzleThemeContainerItemData
				{
					CountryName = countryCodeDataTable.GetCountryName(key),
					ContentDatas = puzzleDataTable.GetCollection(key)?
						.Select(
							puzzleData => {
								uint placedPieces = user.PlayingPuzzleCollection.TryGetValue(puzzleData.Key, out uint result)? result : 0;
								uint unlockedPieces = user.UnlockedPuzzlePieceDic == null? 0 : 
									user.UnlockedPuzzlePieceDic.TryGetValue(puzzleData.Key, out uint result2)? 
									result2 : 0;
									
								return new PuzzleContentData
								{
									PlacedPiecesData = placedPieces,
									PuzzleData = puzzleData,
									onClick = () => {
										UIManager.ShowLoading();
										try
										{
											if (parameter.MoveToPuzzle == null)
											{
												UIManager.HideLoading();
											}
											parameter.MoveToPuzzle?.Invoke(puzzleData, placedPieces, unlockedPieces);
										}
										catch
										{
											UIManager.HideLoading();
										}
									}
								};
							}
					)?.ToList() ?? new()
				}
			).ToArray();

		m_scrollView.InsertData(puzzleThemeContainerItemDatas);
	}
}
