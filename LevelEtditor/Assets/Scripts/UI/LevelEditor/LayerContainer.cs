using UnityEngine;
using UnityEngine.UI;

using System;

using TMPro;
using Gpm.Ui;

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

    public void OnSetup(Action onClear)
    {
       m_clearTilesButton.OnSetup("Clear Tiles in Layer", () => onClear?.Invoke());
    }
}
