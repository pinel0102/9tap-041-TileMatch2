using UnityEngine;

using System;

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

	public void OnSetup(MenuViewParameter parameter)
	{
		m_levelContainer.OnSetup(parameter.SelectLevelContainerParameter);
		m_tileTypeContainer.OnSetup(parameter.NumberOfContainerParameter);
		m_gridOptionContainer.OnSetup(parameter.GridOptionContainerParameter);
		m_layerContainer.OnSetup(parameter.LayerContainerParameter);
	}

	public void UpdateLevelUI(int lastLevel, int selectedLevel)
	{
		m_levelContainer.UpdateUI(lastLevel, selectedLevel);
	}

	public void UpdateNumberOfTileTypesUI(int number)
	{
	   m_tileTypeContainer.UpdateUI(number);
	}

    public void UpdateLayerUI(int count, int layerIndex)
    {
		m_layerContainer.UpdateUI(count, layerIndex);
    }
}
