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
}

public class MenuBlockerContainer : MonoBehaviour
{
    [Header("★ [Live] Blocker")]
    [SerializeField]	private BlockerTypeEditor m_blockerType;
    [SerializeField]	private int m_blockerCount;

    [Header("★ [Reference] Blocker")]
    [SerializeField]	private TMP_Dropdown m_blockerDropdown;
    [SerializeField]	private TMP_InputField m_blockerCountText;
    [SerializeField]	private Button m_buttonMinus;
	[SerializeField]	private Button m_buttonPlus;
    [SerializeField]	private Button m_buttonApply;
    [SerializeField]	private Button m_buttonClear;

#region Initialize

    public void OnSetup(MenuBlockerContainerParameter parameter)
    {
        SetupBlocker(parameter);
    }

    private void SetupBlocker(MenuBlockerContainerParameter parameter)
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
        m_buttonApply.onClick.AddListener(() => ApplyBlocker(m_blockerType, m_blockerCount));
        m_buttonClear.onClick.AddListener(() => ClearAllBlocker());

        SetupBlockerDropdown(parameter);
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

    public void OnUpdateUI(BlockerTypeEditor blockerType, int blockerCount)
	{
        m_blockerType = blockerType;
        m_blockerCount = blockerCount;
		m_blockerCountText.SetTextWithoutNotify(blockerCount.ToString());
		m_buttonMinus.interactable = blockerType != BlockerTypeEditor.None && blockerCount > 1;
        m_buttonPlus.interactable = blockerType != BlockerTypeEditor.None;
        m_blockerCountText.interactable = blockerType != BlockerTypeEditor.None;
        m_buttonApply.interactable = blockerType != BlockerTypeEditor.None;

        //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} : {1}</color>", m_blockerType, m_blockerCount));
	}

#endregion OnUpdateUI    


#region Function

    private void ApplyBlocker(BlockerTypeEditor blockerType, int count)
    {
        ClearBlocker(blockerType);

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} : {1}</color>", blockerType, count));

        //
    }

    private void ClearBlocker(BlockerTypeEditor blockerType)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0}</color>", blockerType));
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
