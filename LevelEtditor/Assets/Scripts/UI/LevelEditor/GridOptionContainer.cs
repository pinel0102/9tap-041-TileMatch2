using UnityEngine;
using UnityEngine.UI;

using System;

using Cysharp.Threading.Tasks;

public class GridOptionContainerParameter
{
	public Action<SnapType> OnChangedSnapping;
	public Action<bool> OnVisibleGuide;
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

	private AsyncReactiveProperty<SnapType> m_snapping = new(SnapType.FULL);
	public IReadOnlyAsyncReactiveProperty<SnapType> Snapping => m_snapping;

	private void OnDestroy()
	{
		m_snapping.Dispose();
	}

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
	}
}
