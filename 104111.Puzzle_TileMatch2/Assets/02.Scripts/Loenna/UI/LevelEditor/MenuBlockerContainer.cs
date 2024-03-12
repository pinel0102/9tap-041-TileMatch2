using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using static TMPro.TMP_Dropdown;

public class MenuBlockerContainerParameter
{
    public List<BlockerTypeEditor> BlockerList;

    // Change Blocker Count
	public Action<int> OnTakeStep; 
	public Action<int> OnNavigate;

    // Change Blocker Index
    public Action<int> OnChangeBlockerIndex;
    // Change Blocker Target Layer
    public Action<int> OnChangeBlockerLayer;
    
    // Change Blocker ICD
    public Action<int> OnTakeStepBlockerICD;
    public Action<int> OnNavigateICD;

    // Blocker Function
    public Action<int, BlockerTypeEditor, int> OnAddBlocker;
    public Action<int, BlockerTypeEditor, int> OnApplyBlocker;
    public Action<int> OnClearAllBlocker;
}

public class MenuBlockerContainer : MonoBehaviour
{
    [Header("★ [Settings] Blocker")]
    [SerializeField]	private bool m_showLog;

    [Header("★ [Live] Blocker")]
    [SerializeField]	private BlockerTypeEditor m_blockerType;
    [SerializeField]	private int m_blockerCount;
    [SerializeField]	private int m_blockerVariableICD;
    [SerializeField]	private int m_blockerTargetLayer;
    [SerializeField]	private int m_layerCount;

    [Header("★ [Reference] Blocker")]
    [SerializeField]	private TMP_Dropdown m_dropdownBlocker;
    [SerializeField]	private TMP_Dropdown m_dropdownLayer;
    [SerializeField]	private TMP_InputField m_CountText;
    [SerializeField]	private Button m_buttonCountMinus;
	[SerializeField]	private Button m_buttonCountPlus;

    [Header("★ [Reference] Blocker ICD")]
    [SerializeField]	private GameObject m_ICDContainer;
    [SerializeField]	private TMP_InputField m_ICDText;
    [SerializeField]	private Button m_buttonICDMinus;
	[SerializeField]	private Button m_buttonICDPlus;
    
    [Header("★ [Reference] Function Button")]
    [SerializeField]	private Button m_buttonAdd;
    [SerializeField]	private Button m_buttonApply;
    [SerializeField]	private Button m_buttonClear;

#region Initialize

    public void OnSetup(MenuBlockerContainerParameter parameter)
    {
        SetupDefaultButton(parameter);
        SetupFunctionButton(parameter);
        SetupBlockerICD(parameter);
        SetupBlockerDropdown(parameter);
        SetupBlockerLayerDropdown(parameter);
    }

    private void SetupDefaultButton(MenuBlockerContainerParameter parameter)
    {
        m_buttonCountMinus.onClick.AddListener(() => parameter?.OnTakeStep?.Invoke(-1));
		m_buttonCountPlus.onClick.AddListener(() => parameter?.OnTakeStep?.Invoke(1));
        m_CountText.onEndEdit.AddListener(
			text => {
				parameter?.OnNavigate?.Invoke(
                    int.TryParse(text, out int result) switch {
						true => result,
						_=> -1
					}
				);
			}
		);
    }

    private void SetupFunctionButton(MenuBlockerContainerParameter parameter)
    {
        m_buttonAdd.onClick.AddListener(() => parameter?.OnAddBlocker?.Invoke(m_blockerTargetLayer, m_blockerType, m_blockerCount));
        m_buttonApply.onClick.AddListener(() => parameter?.OnApplyBlocker?.Invoke(m_blockerTargetLayer, m_blockerType, m_blockerCount));
        m_buttonClear.onClick.AddListener(() => parameter?.OnClearAllBlocker?.Invoke(m_blockerTargetLayer));
    }

    private void SetupBlockerICD(MenuBlockerContainerParameter parameter)
    {
        m_buttonICDMinus.onClick.AddListener(() => parameter?.OnTakeStepBlockerICD?.Invoke(-1));
		m_buttonICDPlus.onClick.AddListener(() => parameter?.OnTakeStepBlockerICD?.Invoke(1));
        m_ICDText.onEndEdit.AddListener(
			text => {
				parameter?.OnNavigateICD?.Invoke(
                    int.TryParse(text, out int result) switch {
						true => result,
						_=> -1
					}
				);
			}
		);
    }

    private void SetupBlockerDropdown(MenuBlockerContainerParameter parameter)
    {
        List<OptionData> options = new();

        for(int i=0; i < parameter.BlockerList.Count; i++)
        {
            options.Add(new OptionData(parameter.BlockerList[i].ToString()));
        }

		m_dropdownBlocker.AddOptions(options);        
		m_dropdownBlocker.onValueChanged.AddListener(
			index => {
				parameter?.OnChangeBlockerIndex?.Invoke(index);
			}
		);

        SetDropdownBlocker(LevelEditorPrefs.UI_BlockerTypeIndex);
    }

    private void SetupBlockerLayerDropdown(MenuBlockerContainerParameter parameter)
    {
        m_dropdownLayer.onValueChanged.AddListener(
			index => {
                parameter?.OnChangeBlockerLayer?.Invoke(index);
			}
		);

        RefreshBlockerLayerDropdown(0, LevelEditorPrefs.UI_BlockerLayerIndex + 1, true);
    }

    private void SetDropdownBlocker(int index)
	{
        m_dropdownBlocker.SetValueWithoutNotify(Mathf.Clamp(index, 0, m_dropdownBlocker.options.Count > 0 ? m_dropdownBlocker.options.Count - 1 : 0));
	}

    private void SetDropdownLayer(int index)
	{   
        m_dropdownLayer.SetValueWithoutNotify(Mathf.Clamp(index, 0, m_dropdownLayer.options.Count > 0 ? m_dropdownLayer.options.Count - 1 : 0));
	}

#endregion Initialize


#region OnUpdateUI

    public void OnUpdateUI(BlockerTypeEditor blockerType, int blockerCount, int blockerVariableICD, int blockerTargetLayer, int layerCount)
	{
        int layerDropdownOldIndex = blockerTargetLayer + 1;
        int layerDropdownNewIndex = layerDropdownOldIndex;
        bool layerDropdownReset = m_layerCount > layerCount && layerDropdownOldIndex > layerCount;
        bool layerCountChanged =  m_layerCount != layerCount;

        if (layerDropdownReset)
            layerDropdownNewIndex = 0;
        
        m_blockerType = blockerType;
        m_blockerCount = blockerCount;
        m_blockerVariableICD = blockerVariableICD;
        m_blockerTargetLayer = blockerTargetLayer;
        m_layerCount = layerCount;

		m_CountText.SetTextWithoutNotify(blockerCount.ToString());
		m_buttonCountMinus.interactable = blockerType != BlockerTypeEditor.None && blockerCount > 0;
        m_buttonCountPlus.interactable = blockerType != BlockerTypeEditor.None;
        m_CountText.interactable = blockerType != BlockerTypeEditor.None;

        m_buttonAdd.interactable = blockerType != BlockerTypeEditor.None && blockerCount > 0;
        m_buttonApply.interactable = blockerType != BlockerTypeEditor.None;

        bool hasVariableICD = GlobalDefine.IsBlockerICD_Variable(blockerType);

        m_ICDText.SetTextWithoutNotify(blockerVariableICD.ToString());
        m_buttonICDMinus.interactable = hasVariableICD && blockerVariableICD > 1;
        m_buttonICDPlus.interactable = hasVariableICD;
        m_ICDText.interactable = hasVariableICD;
        m_ICDContainer.SetActive(hasVariableICD);

        RefreshBlockerLayerDropdown(layerCount, layerDropdownNewIndex, layerCountChanged);

        if (layerDropdownReset)
            m_dropdownLayer.onValueChanged.Invoke(layerDropdownNewIndex);
        
        if (m_showLog)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Layer[{0}] {1} : {2} (ICD : {3})</color>", m_blockerTargetLayer, m_blockerType, m_blockerCount, GlobalDefine.GetBlockerICD(m_blockerType, m_blockerVariableICD)));
	}

    private void RefreshBlockerLayerDropdown(int layerCount, int setIndex, bool countChanged)
    {
        if (countChanged)
        {
            if (m_showLog)
                Debug.Log(CodeManager.GetMethodName() + string.Format("layerCount : {0}", layerCount));

            List<OptionData> options = new()
            { new OptionData("All") };

            for(int i=0; i < layerCount; i++)
            {
                options.Add(new OptionData($"Layer {i}"));
            }

            m_dropdownLayer.ClearOptions();		
            m_dropdownLayer.AddOptions(options);

            SetDropdownLayer(setIndex);
        }
    }

#endregion OnUpdateUI    

}
