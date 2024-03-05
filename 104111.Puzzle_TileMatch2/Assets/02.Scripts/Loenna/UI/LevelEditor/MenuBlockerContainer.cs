using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using static TMPro.TMP_Dropdown;

public class MenuBlockerContainerParameter
{
	public TileDataTable Table;

	public Action<int> OnTakeStep; 
	public Action<int> OnNavigate;
	public Action<int> OnTakeStepMissionCount; 
	public Action<int> OnNavigateMissionCount;
	public Action<int> OnChangeBlocker;
}

public class MenuBlockerContainer : MonoBehaviour
{
    [SerializeField]	private TMP_Dropdown m_blockerDropdown;
    [SerializeField]	private TMP_InputField m_blockerCount;
    [SerializeField]	private Button m_buttonMinus;
	[SerializeField]	private Button m_buttonPlus;
    [SerializeField]	private Button m_buttonApply;
    [SerializeField]	private Button m_buttonClear;

    private List<BlockerTypeEditor> m_blockerList = new List<BlockerTypeEditor>()
    {
        BlockerTypeEditor.None,
        BlockerTypeEditor.Glue,
        BlockerTypeEditor.Bush,
    };

    public void OnSetup(MenuBlockerContainerParameter parameter)
    {
        SetupDropdown(parameter);
    }

    private void SetupDropdown(MenuBlockerContainerParameter parameter)
    {
        List<OptionData> options = new();

		var icons = m_blockerList
				.Select(item => new OptionData(item.ToString()))
				.ToArray();

		options.AddRange(icons);
		
		m_blockerDropdown.AddOptions(options);
		SetBlocker(0);

		m_blockerDropdown.onValueChanged.AddListener(
			index => {
				parameter?.OnChangeBlocker?.Invoke(index);
			}
		);
    }

    public void SetBlocker(int index)
	{
		m_blockerDropdown.SetValueWithoutNotify(index);
	}
}
