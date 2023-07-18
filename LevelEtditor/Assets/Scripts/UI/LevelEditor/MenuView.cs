using System.Collections.Generic;
using UnityEngine;

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

	public void UpdateLevelUI(int lastLevel, int selectedLevel)
	{
		m_levelContainer.OnUpdateUI(lastLevel, selectedLevel);
	}

	public void UpdateDifficult(DifficultType difficultType)
	{
		m_levelContainer.OnUpdateDifficult(difficultType);
	}

	public void UpdateNumberOfTileTypesUI(int boardIndex, int number)
	{
	   m_tileTypeContainer.OnUpdateUI(boardIndex, number);
	}

    public void UpdateLayerUI(IReadOnlyList<Color> layers, int layerIndex)
    {
		m_layerContainer.OnUpdateUI(layers, layerIndex);
    }

	public void UpdateLevelInfoUI(int tileCountInBoard, int allTileCount)
	{
		m_levelInfoContainer.OnUpdateUI(
			(LevelInfoContainer.TILE_COUNT_IN_BOARD, tileCountInBoard), 
			(LevelInfoContainer.TILE_COUNT_IN_LEVEL, allTileCount)
		);
	}
}
