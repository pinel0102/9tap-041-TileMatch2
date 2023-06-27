using UnityEngine;

using System;
using Cysharp.Threading.Tasks;

public class MenuViewParameter
{
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
		m_levelContainer.OnSetup();
		m_tileTypeContainer.OnSetup();
		m_gridOptionContainer.OnSetup(parameter.OnChangedSnapping, parameter.OnVisibleGuide);
		m_layerContainer.OnSetup(parameter.OnClearTiles);
	}
}
