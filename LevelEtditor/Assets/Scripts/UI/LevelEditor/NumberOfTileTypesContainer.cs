using UnityEngine;
using UnityEngine.UI;

using System;
using TMPro;

public class NumberOfTileTypesContainer : MonoBehaviour
{
	[SerializeField]
	private Button m_subtractButton;

	[SerializeField]
	private TMP_InputField m_inputField;

	[SerializeField]
	private Button m_addButton;

	public void OnSetup(Action<int> onClick, Action<int> onEndEdit)
	{
		m_subtractButton.onClick.AddListener(() => onClick?.Invoke(-1));
		m_addButton.onClick.AddListener(() => onClick?.Invoke(1));
		m_inputField.onEndEdit.AddListener(
			text => {
				if (int.TryParse(text, out int result))
				{
					onEndEdit?.Invoke(result);
				} 
			}
		);
	}

    public void UpdateUI(int number)
    {
		m_inputField.SetTextWithoutNotify(number.ToString());
		m_subtractButton.interactable = number > 1;
    }
}
