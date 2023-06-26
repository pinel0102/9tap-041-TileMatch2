using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Gpm.Ui;
using System;

public class LayerContainer : MonoBehaviour
{
    [SerializeField]
    private InfiniteScroll m_scrollView;

    [SerializeField]
    private TMP_Dropdown m_selectLayerDropdown;

    [SerializeField]
    private LevelEditorButton m_createLayerButton;

    [SerializeField]
    private LevelEditorButton m_removeLayerButton;

    [SerializeField]
    private LevelEditorButton m_clearTilesButton;

    public void OnSetup()
    {
       
    }
}
