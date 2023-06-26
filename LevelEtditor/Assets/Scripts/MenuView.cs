using UnityEngine;

using System;
using Cysharp.Threading.Tasks;

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

    public void OnSetup(Action<SnapType> onChangedSnapping, Action<bool> onVisibleGuide)
    {
        m_levelContainer.OnSetup();
        m_tileTypeContainer.OnSetup();
        m_gridOptionContainer.OnSetup(onChangedSnapping, onVisibleGuide);
        m_layerContainer.OnSetup();
    }
}
