using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using System;

using UI.Pagination;

using NineTap.Common;

public record MainSceneScrollViewParameter(
	Action<MainMenuType> OnSelectMenu, 
	List<ScrollViewFragmentParameter> FragmentParams
);

public class MainSceneScrollView : CachedBehaviour
{
    [SerializeField]
	private PagedRect m_pagedRect;

	[SerializeField]
	private List<ScrollViewFragmentContent> m_contents;

    public ScrollRect ScrollRect => m_pagedRect.ScrollRect;
    public PagedRect PagedRect => m_pagedRect;

	public void OnSetup(MainSceneScrollViewParameter parameter)
	{
		for (int index = 0, count = parameter.FragmentParams.Count; index < count; index++)
		{
			m_contents[index].OnSetup(parameter.FragmentParams[index].ContentParameter);
		}

		m_pagedRect.OnPageUpdated += (index) => {
			MainMenuType mainMenuType = (MainMenuType)(index - 1);
			parameter.OnSelectMenu(mainMenuType);
			if (mainMenuType is MainMenuType.SETTINGS)
			{
				UIManager.HUD.Hide();
			}
			else
			{
				UIManager.HUD.Show(HUDType.ALL);
			}
		};
	}

	public void MoveTo(int type)
	{
		m_pagedRect.SetCurrentPage(type + 1);
	}

	public void OnUpdateUI(User user)
	{
		m_contents.ForEach(content => content.OnUpdateUI(user));
	}
}
