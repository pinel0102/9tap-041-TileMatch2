using UnityEngine;
using UnityEngine.UI;

using System;
using TMPro;

public class NumberOfTileTypesContainerParameter
{
	public Action<int> OnTakeStep; 
	public Action<int> OnNavigate; 
}

public class NumberOfTileTypesContainer : MonoBehaviour
{
	[SerializeField]
	private Button m_subtractButton;

	[SerializeField]
	private TMP_InputField m_inputField;

	[SerializeField]
	private Button m_addButton;

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
	}

	public void UpdateUI(int number)
	{
		m_inputField.SetTextWithoutNotify(number.ToString());
		m_subtractButton.interactable = number > 1;
	}
}
