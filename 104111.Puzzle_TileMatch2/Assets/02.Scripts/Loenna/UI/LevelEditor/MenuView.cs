using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LevelEditor;

public class MenuViewParameter
{
	public SelectLevelContainerParameter SelectLevelContainerParameter;
	public NumberOfTileTypesContainerParameter NumberOfContainerParameter;
	public GridOptionContainerParameter GridOptionContainerParameter;
	public LayerContainerParameter LayerContainerParameter;
}

public class MenuView : MonoBehaviour
{
	[SerializeField]
	private SelectLevelContainer m_levelContainer;

	[SerializeField]
	private NumberOfTileTypesContainer m_tileTypeContainer;

	[SerializeField]
	private GridOptionContainer m_gridOptionContainer;

	[SerializeField]
	private LayerContainer m_layerContainer;

	[SerializeField]
	private LevelInfoContainer m_levelInfoContainer;

	public void OnSetup(MenuViewParameter parameter)
	{
		m_levelContainer.OnSetup(parameter.SelectLevelContainerParameter);
		m_tileTypeContainer.OnSetup(parameter.NumberOfContainerParameter);
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

	public void UpdateLevelInfoUI(int boardCount, int tileCountInBoard, int allTileCount, int missionCountInBoard, int missionCountInLevel)
	{
		m_levelInfoContainer.OnUpdateUI(
            (LevelInfoContainer.TILE_COUNT_IN_BOARD, tileCountInBoard.ToString()), 
			(LevelInfoContainer.TILE_COUNT_IN_LEVEL, allTileCount.ToString()),
			(LevelInfoContainer.MISSION_COUNT_IN_BOARD, missionCountInBoard.ToString()),
			(LevelInfoContainer.MISSION_COUNT_IN_LEVEL, missionCountInLevel.ToString()),
            (LevelInfoContainer.BOARD_COUNT_IN_LEVEL, boardCount.ToString())
		);
	}

	public void OnVisibleLayerOption(bool brushMode)
	{
		m_layerContainer.OnVisibleLayerOption(brushMode);
	}
}
