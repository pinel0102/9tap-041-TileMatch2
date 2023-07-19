using UnityEngine;

using System;
using System.Collections.Generic;

using TMPro;
using Gpm.Ui;

using static TMPro.TMP_Dropdown;

public class LayerContainerParameter
{
	public Action OnRemove;
	public Action<int, bool> OnVisible;
}

public class LayerContainer : MonoBehaviour
{
	[SerializeField]
	private InfiniteScroll m_scrollView;

	[SerializeField]
	private LevelEditorButton m_removeLayerButton;

	private Action<int, bool> m_onVisible;

	public void OnSetup(LayerContainerParameter parameter)
	{
		m_onVisible = parameter?.OnVisible;
		m_removeLayerButton.OnSetup(() => parameter?.OnRemove?.Invoke());
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
