using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using static TMPro.TMP_Dropdown;

public class MenuBlockerContainerParameter
{
    // Blocker Count
	public Action<int> OnTakeStep; 
	public Action<int> OnNavigate;

    // Blocker Change
    public Action<int> OnChangeBlocker;

    // Blocker ICD
    public Action<int> OnTakeStepBlockerICD;
    public Action<int> OnNavigateICD;

    // Blocker Function
    public Action<BlockerTypeEditor, int> OnAddBlocker;
    public Action<BlockerTypeEditor, int> OnApplyBlocker;
    public Action OnClearAllBlocker;
}

public class MenuBlockerContainer : MonoBehaviour
{
    [Header("★ [Live] Blocker")]
    [SerializeField]	private BlockerTypeEditor m_blockerType;
    [SerializeField]	private int m_blockerCount;
    [SerializeField]	private int m_blockerVariableICD;

    [Header("★ [Reference] Blocker")]
    [SerializeField]	private TMP_Dropdown m_dropdown;
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
        m_buttonAdd.onClick.AddListener(() => parameter?.OnAddBlocker?.Invoke(m_blockerType, m_blockerCount));
        m_buttonApply.onClick.AddListener(() => parameter?.OnApplyBlocker?.Invoke(m_blockerType, m_blockerCount));
        m_buttonClear.onClick.AddListener(() => parameter?.OnClearAllBlocker?.Invoke());

        //m_buttonAdd.onClick.AddListener(() => AddBlocker(m_blockerType, m_blockerCount));
        //m_buttonApply.onClick.AddListener(() => ApplyBlocker(m_blockerType, m_blockerCount));
        //m_buttonClear.onClick.AddListener(() => ClearAllBlocker());
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

		var icons = LevelEditor.Instance.blockerList
				.Select(item => new OptionData(item.ToString()))
				.ToArray();

		options.AddRange(icons);
		
		m_dropdown.AddOptions(options);
		m_dropdown.onValueChanged.AddListener(
			index => {
				parameter?.OnChangeBlocker?.Invoke(index);
			}
		);

        SetBlocker(0);
    }
    
    private void SetBlocker(int index)
	{
		m_dropdown.SetValueWithoutNotify(index);
	}

#endregion Initialize    


#region OnUpdateUI

    public void OnUpdateUI(BlockerTypeEditor blockerType, int blockerCount, int blockerVariableICD)
	{
        m_blockerType = blockerType;
        m_blockerCount = blockerCount;
        m_blockerVariableICD = blockerVariableICD;

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

        bool showLog = true;

        if (showLog)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} : {1} (ICD : {2})</color>", m_blockerType, m_blockerCount, GlobalDefine.GetBlockerICD(m_blockerType, m_blockerVariableICD)));
        }
	}

#endregion OnUpdateUI    


/*#region Blocker Function

    private void ApplyBlocker(BlockerTypeEditor _blockerType, int _count)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} : {1}</color>", _blockerType, _count));
        
        ClearBlocker(_blockerType);
        AddBlocker(_blockerType, _count);
    }

    private void ClearBlocker(BlockerTypeEditor _blockerType)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0}</color>", _blockerType));

        //
    }

    private void AddBlocker(BlockerTypeEditor _blockerType, int _count)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} : {1}</color>", _blockerType, _count));

        //
    }

    private void ClearAllBlocker()
    {
        Debug.Log(CodeManager.GetMethodName());

        LevelEditor.Instance.blockerList.Where(item => item != BlockerTypeEditor.None).ToList()
        .ForEach(_blockerType => {
            ClearBlocker(_blockerType);
        });
    }

#endregion Blocker Function*/
}
