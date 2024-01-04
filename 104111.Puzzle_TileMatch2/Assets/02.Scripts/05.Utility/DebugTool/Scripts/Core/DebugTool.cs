using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTool : MonoBehaviour
{
    public Button debugButton;
    public RectTransform debugPanel;
    public Button panelCloseButton;
    public Vector2 initPosition = Vector2.zero;
    public bool isInitPosition;
    public bool isDebugPanelOn;
    
    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        debugButton.onClick.RemoveAllListeners();
        panelCloseButton.onClick.RemoveAllListeners();
        debugButton.onClick.AddListener(OnClick_DebugButton);
        panelCloseButton.onClick.AddListener(DebugPanelClose);
        DebugPanelClose();
    }
    
    private void OnClick_DebugButton()
    {
        if (isDebugPanelOn)
            DebugPanelClose();
        else
            DebugPanelOpen();
    }

    private void DebugPanelOpen()
    {
        if (isInitPosition)
            debugPanel.anchoredPosition = initPosition;
        
        debugPanel.gameObject.SetActive(true);
        isDebugPanelOn = true;
    }

    private void DebugPanelClose()
    {
        debugPanel.gameObject.SetActive(false);
        isDebugPanelOn = false;
    }

}
