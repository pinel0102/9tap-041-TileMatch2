using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;

using Gpm.Ui;

using static TMPro.TMP_Dropdown;

public enum DrawOrder
{
	TOP,
	BOTTOM
}

public class LayerContainerParameter
{
	public Action OnRemove;
	public Action<int, bool> OnVisible;
	public Action<DrawOrder> OnChangedDrawOrder;
}

public class LayerContainer : MonoBehaviour
{
	[SerializeField]
	private InfiniteScroll m_scrollView;

	[SerializeField]
	private LevelEditorButton m_removeLayerButton;

	[SerializeField]
	private ToggleGroup m_toggleGroup;

	[SerializeField]
	private LevelEditorToggleButton m_topOrderButton;

	[SerializeField]
	private LevelEditorToggleButton m_bottomOrderButton;

	private Action<int, bool> m_onVisible;

	public void OnSetup(LayerContainerParameter parameter)
	{
		m_onVisible = parameter?.OnVisible;
		m_removeLayerButton.OnSetup(() => parameter?.OnRemove?.Invoke());
		m_topOrderButton.OnSetup(
			"Top", 
			isOn => {
				if (isOn)
				{
					parameter?.OnChangedDrawOrder?.Invoke(DrawOrder.TOP);
				}
			},
			awakeOn: false,
			group: m_toggleGroup
		);

		m_bottomOrderButton.OnSetup(
			"Bottom", 
			isOn => {
				if (isOn)
				{
					parameter?.OnChangedDrawOrder?.Invoke(DrawOrder.BOTTOM);
				}
			},
			awakeOn: false,
			group: m_toggleGroup
		);
	}

	public void OnUpdateUI(IReadOnlyList<Color> layerColors)
	{
		m_scrollView.ClearData(true);

		List<LayerListScrollItemData> items = new();
		List<OptionData> options = new();

		int layerCount = layerColors.Count;

		for (int index = 0; index < layerCount; index++)
		{
			Color color = layerColors[index];
			items.Add(new LayerListScrollItemData { 
					Index = index,
					Color = color,
					OnToggle = (index, isOn) => m_onVisible?.Invoke(index, isOn)
				}
			);
			options.Add(new OptionData($"Layer {index}"));
		}

		m_scrollView.InsertData(items.ToArray(), true);
	}
}
