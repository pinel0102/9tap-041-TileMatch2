using UnityEngine;
using UnityEngine.UI;

using System;

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
		m_text.text = $"Layer {scrollData}";
    }
}
