using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Collections.Generic;

using TMPro;
using static TMPro.TMP_Dropdown;

public class NumberOfTileTypesContainerParameter
{
	public TileDataTable Table;

	public Action<int> OnTakeStep; 
	public Action<int> OnNavigate;

	//public Action<BrushMode> OnChangeBrushMode;
	//public Action<int> OnPointToLayer;
	//public Action OnGenerateTiles;

	public Action<int> OnTakeStepMissionCount; 
	public Action<int> OnNavigateMissionCount;
	public Action<int> OnChangeMissionTileIcon;
}

public class NumberOfTileTypesContainer : MonoBehaviour
{
	[SerializeField]	private TMP_Text m_titleText;
	[SerializeField]	private Button m_subtractButton;
	[SerializeField]	private TMP_InputField m_inputField;
	[SerializeField]	private Button m_addButton;
	[SerializeField]	private ToggleGroup m_toggleGroup;
	//[SerializeField]	private LevelEditorToggleButton m_brushToggleButton;
	//[SerializeField]	private LevelEditorToggleButton m_stampToggleButton;
	[SerializeField]	private Button m_subtractButton_2;
	[SerializeField]	private TMP_InputField m_inputField_2;
	[SerializeField]	private Button m_addButton_2;
	[SerializeField]	private TMP_Dropdown m_tileIconDropdown;

	public void OnSetup(NumberOfTileTypesContainerParameter parameter)
	{
		m_subtractButton.onClick.AddListener(() => parameter?.OnTakeStep?.Invoke(-1));
		m_addButton.onClick.AddListener(() => parameter?.OnTakeStep?.Invoke(1));
		m_inputField.onEndEdit.AddListener(
			text => {
				parameter?.OnNavigate?.Invoke(
					int.TryParse(text, out int result) switch {
						true => result,
						_=> -1
					}
				);
			}
		);

		m_subtractButton_2.onClick.AddListener(() => parameter?.OnTakeStepMissionCount?.Invoke(-1));
		m_addButton_2.onClick.AddListener(() => parameter?.OnTakeStepMissionCount?.Invoke(1));
		m_inputField_2.onEndEdit.AddListener(
			text => {
				parameter?.OnNavigateMissionCount?.Invoke(
					int.TryParse(text, out int result) switch {
						true => result,
						_=> -1
					}
				);
			}
		);

		List<OptionData> options = new();

		var icons = parameter.Table.Dic
				.OrderBy(pair => pair.Key)
				.Where(pair => pair.Key >= 0)
				.Select(pair => new OptionData(pair.Value.Name))
				.ToArray();

		options.AddRange(icons);
		options.Insert(0, new OptionData("None"));
		
		m_tileIconDropdown.AddOptions(options);
		SetMissionTileIcon(0);

		m_tileIconDropdown.onValueChanged.AddListener(
			index => {
				parameter?.OnChangeMissionTileIcon?.Invoke(index);
			}
		);

		//m_brushToggleButton.OnSetup(
		//	text: "TileBrush",
		//	(isOn) => OnChangeBrush(BrushMode.TILE_BRUSH),
		//	group: m_toggleGroup
		//);

		//m_stampToggleButton.OnSetup(
		//	text: "MissionBrush",
		//	(isOn) => OnChangeBrush(BrushMode.MISSION_STAMP),
		//	awakeOn: false,
		//	group: m_toggleGroup
		//);

		//m_generateButton.onClick.AddListener(() => parameter.OnGenerateTiles?.Invoke());

		//void OnChangeBrush(BrushMode mode)
		//{
		//	parameter?.OnChangeBrushMode?.Invoke(mode);
		//	m_layerDropdown.gameObject.SetActive(mode is BrushMode.MISSION_STAMP);
		//}

		//m_layerDropdown.onValueChanged.AddListener(index => parameter?.OnPointToLayer?.Invoke(index));
	}

	public void SetMissionTileIcon(int index)
	{
		m_tileIconDropdown.SetValueWithoutNotify(index);
	}

	public void OnUpdateUI(int boardIndex, int number, int missionCount)
	{
		m_titleText.text = $"Tile types on board[{boardIndex}]:";
		m_inputField.SetTextWithoutNotify(number.ToString());
		m_subtractButton.interactable = number > 1;

		m_inputField_2.SetTextWithoutNotify(missionCount.ToString());
		m_subtractButton_2.interactable = missionCount > 0;
	}

	//public void OnUpdateUI(IReadOnlyList<int> layers)
	//{		
	//	int count = layers.Count;

	//	if (count <= 0)
	//	{
	//		m_tileIconDropdown.ClearOptions();
	//		return;
	//	}

	//	int optionCount = m_tileIconDropdown.options.Count;

	//	List<OptionData> options = new(m_tileIconDropdown.options.Take(Mathf.Min(count, optionCount)));

	//	for (int index = 0; index < count; index++)
	//	{
	//		string name = $"Layer {index}";
			
	//		if (options.HasIndex(index))
	//		{
	//			options[index] = new OptionData(name);
	//		}
	//		else
	//		{
	//			options.Add(new OptionData(name));
	//		}
	//	}

	//	m_tileIconDropdown.ClearOptions();
	//	m_tileIconDropdown.AddOptions(options);

	//	int last = Mathf.Max(0, count - 1);

	//	m_tileIconDropdown.SetValueWithoutNotify(last);
	//}
}
