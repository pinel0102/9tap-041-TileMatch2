using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using static TMPro.TMP_Dropdown;

public class MenuBlockerContainerParameter
{
	public Action<int> OnTakeStep; 
	public Action<int> OnNavigate;
    public Action<int> OnChangeBlocker;

    // Suitcase AddCount
    public Action<int> OnTakeStepAddCount;
}

public class MenuBlockerContainer : MonoBehaviour
{
    [Header("★ [Live] Blocker")]
    [SerializeField]	private BlockerTypeEditor m_blockerType;
    [SerializeField]	private int m_blockerCount;
    [SerializeField]	private int m_addCount;

    [Header("★ [Reference] Blocker")]
    [SerializeField]	private TMP_Dropdown m_blockerDropdown;
    [SerializeField]	private TMP_InputField m_blockerCountText;
    [SerializeField]	private Button m_buttonMinus;
	[SerializeField]	private Button m_buttonPlus;

    [Header("★ [Reference] AddCount")]
    [SerializeField]	private GameObject m_addCountContainer;
    [SerializeField]	private TMP_InputField m_addCountText;
    [SerializeField]	private Button m_buttonAddCountMinus;
	[SerializeField]	private Button m_buttonAddCountPlus;
    
    [Header("★ [Reference] Function Button")]
    [SerializeField]	private Button m_buttonAdd;
    [SerializeField]	private Button m_buttonApply;
    [SerializeField]	private Button m_buttonClear;

#region Initialize

    public void OnSetup(MenuBlockerContainerParameter parameter)
    {
        SetupDefaultButton(parameter);
        SetupSuitcaseAddCount(parameter);
        SetupBlockerDropdown(parameter);
    }

    private void SetupDefaultButton(MenuBlockerContainerParameter parameter)
    {
        m_buttonMinus.onClick.AddListener(() => parameter?.OnTakeStep?.Invoke(-1));
		m_buttonPlus.onClick.AddListener(() => parameter?.OnTakeStep?.Invoke(1));
        m_blockerCountText.onEndEdit.AddListener(
			text => {
				parameter?.OnNavigate?.Invoke(
                    int.TryParse(text, out int result) switch {
						true => result,
						_=> -1
					}
				);
			}
		);
        m_buttonAdd.onClick.AddListener(() => AddBlocker(m_blockerType, m_blockerCount));
        m_buttonApply.onClick.AddListener(() => ApplyBlocker(m_blockerType, m_blockerCount));
        m_buttonClear.onClick.AddListener(() => ClearAllBlocker());
    }

    private void SetupSuitcaseAddCount(MenuBlockerContainerParameter parameter)
    {
        m_addCountContainer.SetActive(LevelEditor.Instance.blockerList.Contains(BlockerTypeEditor.Suitcase));
        m_buttonAddCountMinus.onClick.AddListener(() => parameter?.OnTakeStepAddCount?.Invoke(-1));
		m_buttonAddCountPlus.onClick.AddListener(() => parameter?.OnTakeStepAddCount?.Invoke(1));
        m_addCountText.onEndEdit.AddListener(
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

    private void SetupBlockerDropdown(MenuBlockerContainerParameter parameter)
    {
        List<OptionData> options = new();

		var icons = LevelEditor.Instance.blockerList
				.Select(item => new OptionData(item.ToString()))
				.ToArray();

		options.AddRange(icons);
		
		m_blockerDropdown.AddOptions(options);
		m_blockerDropdown.onValueChanged.AddListener(
			index => {
				parameter?.OnChangeBlocker?.Invoke(index);
			}
		);

        SetBlocker(0);
    }

    

    private void SetBlocker(int index)
	{
		m_blockerDropdown.SetValueWithoutNotify(index);
	}

#endregion Initialize    


#region OnUpdateUI

    public void OnUpdateUI(BlockerTypeEditor blockerType, int blockerCount, int addCount)
	{
        m_blockerType = blockerType;
        m_blockerCount = blockerCount;
        m_addCount = addCount;

		m_blockerCountText.SetTextWithoutNotify(blockerCount.ToString());
		m_buttonMinus.interactable = blockerType != BlockerTypeEditor.None && blockerCount > 1;
        m_buttonPlus.interactable = blockerType != BlockerTypeEditor.None;
        m_blockerCountText.interactable = blockerType != BlockerTypeEditor.None;

        m_buttonAdd.interactable = blockerType != BlockerTypeEditor.None && blockerCount > 0;
        m_buttonApply.interactable = blockerType != BlockerTypeEditor.None;

        m_addCountText.SetTextWithoutNotify(addCount.ToString());
        m_buttonAddCountMinus.interactable = blockerType == BlockerTypeEditor.Suitcase && addCount > 1;
        m_buttonAddCountPlus.interactable = blockerType == BlockerTypeEditor.Suitcase;
        m_addCountText.interactable = blockerType == BlockerTypeEditor.Suitcase;
        m_addCountContainer.SetActive(blockerType == BlockerTypeEditor.Suitcase);

        bool showLog = false;

        if (showLog)
        {
            if (blockerType == BlockerTypeEditor.Suitcase)
                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} : {1} ({2})</color>", m_blockerType, m_blockerCount, m_addCount));
            else
                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} : {1}</color>", m_blockerType, m_blockerCount));
        }
	}

#endregion OnUpdateUI    


#region Function

    private void ApplyBlocker(BlockerTypeEditor blockerType, int count)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} : {1}</color>", blockerType, count));
        
        ClearBlocker(blockerType);
        AddBlocker(blockerType, count);
    }

    private void ClearBlocker(BlockerTypeEditor blockerType)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0}</color>", blockerType));

        //
    }

    private void AddBlocker(BlockerTypeEditor blockerType, int count)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} : {1}</color>", blockerType, count));

        //
    }

    private void ClearAllBlocker()
    {
        Debug.Log(CodeManager.GetMethodName());

        LevelEditor.Instance.blockerList.Where(item => item != BlockerTypeEditor.None).ToList()
        .ForEach(blockerType => {
            ClearBlocker(blockerType);
        });
    }

#endregion Function    
}
