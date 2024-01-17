using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gpm.Ui;
using NineTap.Common;
using System.Collections.Generic;
using System;

[ResourcePath("UI/Fragments/Fragment_Collection")]
public class MainSceneFragmentContent_Collection : ScrollViewFragmentContent
{
	[SerializeField]
	private InfiniteScroll m_scrollView;
    public List<PuzzleThemeContainerItem> ContainerList => m_scrollView.Items.Select(item => {return (PuzzleThemeContainerItem)item;}).ToList();
    private PuzzleThemeContainerItemData[] m_puzzleThemeContainerItemDatas;
    private TableManager tableManager;
    private User user => GlobalData.Instance.userManager.Current;
    private Action<PuzzleData, uint, uint> MoveToPuzzle;
    
	public override void OnSetup(ScrollViewFragmentContentParameter contentParameter)
	{
		if (contentParameter is not MainSceneFragmentContentParameter_Collection parameter) 
		{
			return;
		}

		tableManager = Game.Inst.Get<TableManager>();
		CountryCodeDataTable countryCodeDataTable = tableManager.CountryCodeDataTable;
		PuzzleDataTable puzzleDataTable = tableManager.PuzzleDataTable;

        MoveToPuzzle += parameter.MoveToPuzzle;

		m_puzzleThemeContainerItemDatas = puzzleDataTable.CollectionKeys
			.Select(
				key => new PuzzleThemeContainerItemData
				{
                    CountryCode = key,
                    CountryName = countryCodeDataTable.GetCountryName(key),
					ContentDatas = puzzleDataTable.GetCollection(key)?
						.Select(
							puzzleData => {
								return new PuzzleContentData
								{
                                    PlacedPiecesData = GetPlacedPieces(puzzleData.Key),
									PuzzleData = puzzleData,
									onClick = () => {
										//UIManager.ShowLoading();
										try
										{
											if (parameter.MoveToPuzzle == null)
											{
												UIManager.HideLoading();
											}
                                            uint placedPieces = GetPlacedPieces(puzzleData.Key);
                                            uint unlockedPieces = GetUnlockedPieces(puzzleData.Key);
                                            
                                            Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1} / {2}", puzzleData.Index, placedPieces, unlockedPieces));
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

    public void RefreshPieceState(string countryCode, int puzzleIndex, int pieceIndex, bool attached)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("[{0}-{1}] {2:00} : {3}", countryCode, puzzleIndex, pieceIndex, attached));
        
        //var container = (PuzzleThemeContainerItem)m_scrollView.Items?.Find(item => ((PuzzleThemeContainerItem)item).CountryCode.Equals(countryCode));
        var container = ContainerList?.Find(item => item.CountryCode.Equals(countryCode));
        if (container != null)
        {
            //Debug.Log(CodeManager.GetMethodName() + string.Format("found container"));
            var contentItem = container.Contents.Find(item => item.Index.Equals(puzzleIndex));
            if (contentItem != null)
            {
                //Debug.Log(CodeManager.GetMethodName() + string.Format("found contentItem"));
                contentItem.RefreshPiece(pieceIndex, attached);
            }
        }
    }

    public void RefreshLockState()
    {
        Debug.Log(CodeManager.GetMethodName());

        for(int i=0; i < ContainerList.Count; i++)
        {
            ContainerList[i].RefreshLockState();
        }
    }

    public uint GetPlacedPieces(int Key)
    {
        return user.PlayingPuzzleCollection.TryGetValue(Key, out uint result)? result : 0;
    }

    public uint GetUnlockedPieces(int Key)
    {
        return user.UnlockedPuzzlePieceDic == null? 0 : 
            user.UnlockedPuzzlePieceDic.TryGetValue(Key, out uint result2)? 
            result2 : 0;
    }
}
