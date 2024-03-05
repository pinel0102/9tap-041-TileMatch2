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

    public void UpdateLayerUI(IReadOnlyList<LayerInfo> layers, IReadOnlyList<int> invisibleList)
    {
		m_layerContainer.OnUpdateUI(layers.Select(layer => layer.Color).ToArray(), invisibleList);
		//m_tileTypeContainer.OnUpdateUI(layers.Select((_, index) => index).ToArray());
    }

	public void UpdateLevelInfoUI(int boardCount, int tilesInBoard, int tilesInLevel, int goldTilesInLevel)
	{
        List<(string text, string value)> items = new List<(string text, string value)>();
        items.Clear();
        items.Add((LevelInfoContainer.TILE_COUNT_IN_LEVEL, tilesInLevel.ToString()));
        
        if (boardCount > 1)
        {
            items.Add((LevelInfoContainer.TILE_COUNT_IN_BOARD, tilesInBoard.ToString()));
            items.Add((LevelInfoContainer.BOARD_COUNT_IN_LEVEL, boardCount.ToString()));
        }
        
        /*if (blockerCount > 1)
        {
            for(int i=0; i < blockerList.Count; i++)
            {
                if (blockerList[i] > 0)
                    items.Append((LevelInfoContainer.BLOCKER_COUNT_FORMAT, blockerList[i].ToString()));
            }
        }*/

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
