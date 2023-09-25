using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using TMPro;
using static TMPro.TMP_Dropdown;

public class GridOptionContainerParameter
{
	public CountryCodeDataTable Table;

	public Action<SnapType> OnChangedSnapping;
	public Action<bool> OnVisibleGuide;

	public Action OnReLoadData;
	public Action OnClearClientPath;

	public Action<int> OnChangeCountryCode;
}

public class GridOptionContainer : MonoBehaviour
{
	[SerializeField]
	private ToggleGroup m_toggleGroup;

	[SerializeField]
	private LevelEditorToggleButton m_fullToggleButton;

	[SerializeField]
	private LevelEditorToggleButton m_halfToggleButton;

	[SerializeField]
	private LevelEditorToggleButton m_eighthToggleButton;

	[SerializeField]
	private LevelEditorToggleButton m_guideLineButton;

	[SerializeField]
	private LevelEditorButton m_clearDataPathButton;

	[SerializeField]
	private TMP_Dropdown m_codeDropdown;

	public void OnSetup(GridOptionContainerParameter parameter)
	{
		var onChangedSnapping = parameter.OnChangedSnapping;

		m_fullToggleButton.OnSetup(
			"Full", 
			isOn => {
				if (isOn)
				{
					onChangedSnapping?.Invoke(SnapType.FULL);
				}
			},
			group: m_toggleGroup
		);

		m_halfToggleButton.OnSetup(
			"1/2", 
			isOn => {
				if (isOn)
				{
					onChangedSnapping?.Invoke(SnapType.HALF);
				}
			},
			awakeOn: false,
			group: m_toggleGroup
		);

		m_eighthToggleButton.OnSetup(
			"1/8", 
			isOn => {
				if (isOn)
				{
					onChangedSnapping?.Invoke(SnapType.EIGHTH);
				}
			},
			awakeOn: false,
			group: m_toggleGroup
		);

		m_guideLineButton.OnSetup("GUIDE", isOn => parameter?.OnVisibleGuide?.Invoke(isOn));
		m_clearDataPathButton.OnSetup(parameter.OnReLoadData);

		List<OptionData> options = new();

		options.AddRange(
			parameter.Table.Dic
				.OrderBy(pair => pair.Key)
				.Where(pair => pair.Key >= 0)
				.Select(pair => new OptionData(pair.Value.Name))
		);
		
		m_codeDropdown.AddOptions(options);
		SetCountryCode(0);

		m_codeDropdown.onValueChanged.AddListener(index => parameter?.OnChangeCountryCode?.Invoke(index));
	}

	public void SetCountryCode(int index)
	{
		m_codeDropdown.SetValueWithoutNotify(index);
	}
}
