using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LevelEditor;

public class MenuViewParameter
{
	public SelectLevelContainerParameter SelectLevelContainerParameter;
	public NumberOfTileTypesContainerParameter NumberOfContainerParameter;
    public MenuBlockerContainerParameter MenuBlockerContainerParameter;
	public GridOptionContainerParameter GridOptionContainerParameter;
	public LayerContainerParameter LayerContainerParameter;
}

public class MenuView : MonoBehaviour
{
	[SerializeField]	private SelectLevelContainer m_levelContainer;
	[SerializeField]	private NumberOfTileTypesContainer m_tileTypeContainer;
    [SerializeField]	private MenuBlockerContainer m_menuBlockerContainer;
	[SerializeField]	private GridOptionContainer m_gridOptionContainer;
	[SerializeField]	private LayerContainer m_layerContainer;
	[SerializeField]	private LevelInfoContainer m_levelInfoContainer;

    public void OnSetup(MenuViewParameter parameter)
	{
		m_levelContainer.OnSetup(parameter.SelectLevelContainerParameter);
		m_tileTypeContainer.OnSetup(parameter.NumberOfContainerParameter);
        m_menuBlockerContainer.OnSetup(parameter.MenuBlockerContainerParameter);
		m_gridOptionContainer.OnSetup(parameter.GridOptionContainerParameter);
		m_layerContainer.OnSetup(parameter.LayerContainerParameter);
	}

	public void UpdateLevelUI(int lastLevel, int selectedLevel, int countryCodeIndex)
	{
		m_levelContainer.OnUpdateUI(lastLevel, selectedLevel);
		m_gridOptionContainer.SetCountryCode(countryCodeIndex);
	}

	public void UpdateGrades(DifficultType difficultType, bool hardMode)
	{
		m_levelContainer.OnUpdateGrades(difficultType, hardMode);
	}

	public void UpdateNumberOfTileTypesUI(int boardIndex, int number, int missionCount, int goldTileIcon)
	{
	   m_tileTypeContainer.OnUpdateUI(boardIndex, number, missionCount);
	   m_tileTypeContainer.SetMissionTileIcon(goldTileIcon);
	}

    public void UpdateBlockerUI(BlockerTypeEditor blockerType, int blockerCount, int blockerVariableICD, int blockerTargetLayer, int layerCount)
    {
        m_menuBlockerContainer.OnUpdateUI(blockerType, blockerCount, blockerVariableICD, blockerTargetLayer, layerCount);
    }

    public void UpdateLayerUI(IReadOnlyList<LayerInfo> layers, IReadOnlyList<int> invisibleList)
    {
		m_layerContainer.OnUpdateUI(layers.Select(layer => layer.Color).ToArray(), invisibleList);
		//m_tileTypeContainer.OnUpdateUI(layers.Select((_, index) => index).ToArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="boardCount"></param>
    /// <param name="tilesInBoard"></param>
    /// <param name="tilesInLevel"></param>
    /// <param name="goldTilesInLevel"></param>
    /// <param name="blockerDic">None / Glue_Right 제외한 Dictionary.</param>
	public void UpdateLevelInfoUI(int boardCount, int tilesInBoard, int tilesInLevel, int addTilesInBoard, int goldTilesInLevel, Dictionary<BlockerType, int> blockerDic)
	{
        CurrentBlockerDic = blockerDic;

        List<(string text, string value)> items = new List<(string text, string value)>();
        items.Clear();
        items.Add((LevelInfoContainer.TILE_COUNT_IN_LEVEL, tilesInLevel.ToString()));
        
        if (boardCount > 1)
        {
            items.Add((LevelInfoContainer.TILE_COUNT_IN_BOARD, tilesInBoard.ToString()));
            if (addTilesInBoard > 0)
                items.Add((LevelInfoContainer.TILE_ADDITIONAL_COUNT_IN_BOARD, addTilesInBoard.ToString()));
            items.Add((LevelInfoContainer.BOARD_COUNT_IN_LEVEL, boardCount.ToString()));
        }
        else
        {
            if (addTilesInBoard > 0)
                items.Add((LevelInfoContainer.TILE_ADDITIONAL_COUNT_IN_LEVEL, addTilesInBoard.ToString()));
        }
        
        foreach(var item in blockerDic)
        {
            if (item.Value > 0)
                items.Add((string.Format(LevelInfoContainer.BLOCKER_COUNT_FORMAT, item.Key), item.Value.ToString()));
        }

        if (goldTilesInLevel > 0)
        {
            items.Add((LevelInfoContainer.GOLDTILE_COUNT_IN_LEVEL, goldTilesInLevel.ToString()));
        }

		m_levelInfoContainer.OnUpdateUI(items);
	}

	public void OnVisibleLayerOption(bool brushMode)
	{
		m_layerContainer.OnVisibleLayerOption(brushMode);
	}
}
