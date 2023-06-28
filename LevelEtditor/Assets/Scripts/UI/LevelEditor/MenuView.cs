using UnityEngine;

using System;

public class MenuViewParameter
{
	public Action<int> OnMoveLevel;
	public Action<int> OnJumpLevel;
	public Action OnSaveLevel;
	public Action<SnapType> OnChangedSnapping;
	public Action<bool> OnVisibleGuide;
	public Action OnClearTiles;
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
		m_levelContainer.OnSetup(parameter.OnMoveLevel, parameter.OnJumpLevel, parameter.OnSaveLevel);
		m_tileTypeContainer.OnSetup();
		m_gridOptionContainer.OnSetup(parameter.OnChangedSnapping, parameter.OnVisibleGuide);
		m_layerContainer.OnSetup(parameter.OnClearTiles);
	}

	public void UpdateLevelUI(int level)
	{
		m_levelContainer.UpdateUI(level);
	}
}
