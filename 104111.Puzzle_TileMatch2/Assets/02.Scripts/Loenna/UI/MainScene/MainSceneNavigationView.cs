using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Collections.Generic;

using NineTap.Common;

public class MainSceneNavigationViewParameter
{
	public Action<MainMenuType> OnClickTab;
}

public class MainSceneNavigationView : CachedBehaviour
{
	[SerializeField]
	private ToggleGroup m_toggleGroup;

	[SerializeField]
	private List<MainSceneTabButton> m_tabObjects;

	public void OnSetup(MainSceneNavigationViewParameter parameter)
	{
		m_tabObjects.ForEach(tab => tab.OnSetup(parameter.OnClickTab, m_toggleGroup, tab.MenuType is MainMenuType.HOME));
	}

	public void SetIsOnWithoutNotify(MainMenuType type)
	{
		var toggle = m_tabObjects.FirstOrDefault(toggle => toggle.MenuType == type);
		if (toggle == null)
		{
			return;
		}

        GlobalData.Instance.currentTab = type;

		toggle.SetIsOnWithoutNotify(true);
	}
}
