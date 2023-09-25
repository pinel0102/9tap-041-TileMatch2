using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Collections.Generic;

using Gpm.Ui;

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

	[SerializeField]
	private LevelEditorButton m_browserButton;

	[SerializeField]
	private CanvasGroup m_optionCanvasGroup;

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

		string dataFolder = PlayerPrefs.GetString(Constant.Editor.DATA_PATH_KEY);
		m_browserButton.OnSetup(() => Application.OpenURL($"file:///{dataFolder}"));
	}

	public void OnUpdateUI(IReadOnlyList<Color> layerColors, IReadOnlyList<int> invisibleList)
	{
		m_scrollView.ClearData(true);

		List<LayerListScrollItemData> items = new();

		int layerCount = layerColors.Count;

		for (int index = 0; index < layerCount; index++)
		{
			Color color = layerColors[index];
			items.Add(
				new LayerListScrollItemData { 
					Index = index,
					Color = color,
					Visible = !invisibleList.Contains(index),
					OnToggle = (index, isOn) => m_onVisible?.Invoke(index, isOn)
				}
			);
		}

		m_scrollView.InsertData(items.ToArray(), true);
	}

	public void OnVisibleLayerOption(bool brushMode)
	{
		m_optionCanvasGroup.alpha = brushMode? 1f : 0f;
	}
}
