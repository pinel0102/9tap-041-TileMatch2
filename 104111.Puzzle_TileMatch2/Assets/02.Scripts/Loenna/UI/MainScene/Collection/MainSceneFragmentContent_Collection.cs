using UnityEngine;

using System.Linq;

using Cysharp.Threading.Tasks;

using Gpm.Ui;

using NineTap.Common;
using System.Collections.Generic;
using UnityEditor.Rendering;
using System;
using Unity.Burst.Intrinsics;

[ResourcePath("UI/Fragments/Fragment_Collection")]
public class MainSceneFragmentContent_Collection : ScrollViewFragmentContent
{
	[SerializeField]
	private InfiniteScroll m_scrollView;
    public InfiniteScroll ScrollVIew => m_scrollView;
    private PuzzleThemeContainerItemData[] m_puzzleThemeContainerItemDatas;
    private TableManager tableManager;

	public override void OnSetup(ScrollViewFragmentContentParameter contentParameter)
	{
		if (contentParameter is not MainSceneFragmentContentParameter_Collection parameter) 
		{
			return;
		}

		UserManager userManager = Game.Inst.Get<UserManager>();
		
		tableManager = Game.Inst.Get<TableManager>();
		CountryCodeDataTable countryCodeDataTable = tableManager.CountryCodeDataTable;
		PuzzleDataTable puzzleDataTable = tableManager.PuzzleDataTable;

		User user = userManager.Current;

		m_puzzleThemeContainerItemDatas = puzzleDataTable.CollectionKeys
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

		m_scrollView.InsertData(m_puzzleThemeContainerItemDatas);
	}

    public void RefreshState(string countryCode, int index)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("[{0}] {1}", countryCode, index));

        CountryCodeDataTable countryCodeDataTable = tableManager.CountryCodeDataTable;
        string countryName = countryCodeDataTable.GetCountryName(countryCode);

        var container = (PuzzleThemeContainerItem)m_scrollView.Items?.Find(item => ((PuzzleThemeContainerItem)item).CountryName.Equals(countryName));
        if (container != null)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("container"));
            var contentItem = container.Contents.Find(item => item.Index.Equals(index));
            if (contentItem != null)
            {
                Debug.Log(CodeManager.GetMethodName() + string.Format("contentItem"));
                UserManager userManager = Game.Inst.Get<UserManager>();
                User user = userManager.Current;
                uint placedPieces = user.PlayingPuzzleCollection.TryGetValue(index, out uint result)? result : 0;
                
                contentItem.RefreshPuzzle(placedPieces);
            }
        }
    }
}
