using UnityEngine;
using UnityEngine.UI;

using Gpm.Ui;

using TMPro;

public class LayerListScrollItem : InfiniteScrollItem
{
	[SerializeField]
	private TMP_Text m_text;

	[SerializeField]
	private Toggle m_toggle;

    public override void UpdateData(InfiniteScrollData scrollData)
    {
        base.UpdateData(scrollData);

		LayerListScrollItemData itemData = scrollData as LayerListScrollItemData;
		m_text.text = $"Layer {itemData.Index}";
		m_toggle.graphic.color = itemData.Color;

		m_toggle.onValueChanged.AddListener(isOn => {
				itemData.OnToggle?.Invoke(itemData.Index, isOn);
			}
		);
		m_toggle.isOn = itemData.Visible;
    }
}
