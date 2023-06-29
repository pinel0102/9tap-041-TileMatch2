using UnityEngine;

using System;
using System.Collections.Generic;

using TMPro;
using Gpm.Ui;

using static TMPro.TMP_Dropdown;

public class LayerContainerParameter
{
	public Action OnCreate;
	public Action OnRemove;
	public Action OnClear;
}

public class LayerContainer : MonoBehaviour
{
	[SerializeField]
	private InfiniteScroll m_scrollView;

	[SerializeField]
	private TMP_Dropdown m_selectLayerDropdown;

	[SerializeField]
	private LevelEditorButton m_createLayerButton;

	[SerializeField]
	private LevelEditorButton m_removeLayerButton;

	[SerializeField]
	private LevelEditorButton m_clearTilesButton;

	public void OnSetup(LayerContainerParameter parameter)
	{
		m_clearTilesButton.OnSetup("Clear Tiles in Layer", () => parameter?.OnClear?.Invoke());
		m_createLayerButton.OnSetup("Add Layer", () => parameter?.OnCreate?.Invoke());
		m_removeLayerButton.OnSetup("Remove Layer", () => parameter?.OnRemove?.Invoke());
	}

	public void UpdateUI(int layerCount)
	{
		m_scrollView.ClearData(true);
		m_selectLayerDropdown.ClearOptions();

		List<LayerListScrollItemData> items = new();
		List<OptionData> options = new();

		for (int layer = 0; layer < layerCount; layer++)
		{
			items.Add(new LayerListScrollItemData { Index = layer });
			options.Add(new OptionData($"Layer {layer}"));
		}

		m_scrollView.InsertData(items.ToArray(), true);
		m_selectLayerDropdown.AddOptions(options);
	}
}
