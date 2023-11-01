using UnityEngine;

using System.Collections.Generic;

using NineTap.Common;

public class HomeSideContainer : CachedBehaviour
{
	public enum Direction
	{
		LEFT,
		RIGHT
	}
	
	[SerializeField]
	private Direction m_direction;

	private EventCircleIcon m_iconPrefab;

	private List<EventCircleIcon> m_cachedIcons;

	private ProductDataTable m_productDataTable;

	public void OnSetup()
	{
		m_productDataTable = Game.Inst.Get<TableManager>().ProductDataTable;
		m_iconPrefab = ResourcePathAttribute.GetResource<EventCircleIcon>();
		m_cachedIcons = new List<EventCircleIcon>();

		OnTest();
	}

	public void OnTest()
	{
        switch (m_direction)
		{
			case Direction.LEFT:
				CreateIcon(20301);
				break;
			case Direction.RIGHT:
				CreateIcon(20207);
				CreateIcon(20211);
				break;
		}

		void CreateIcon(int index)
		{
			ProductData product = m_productDataTable[index];
			EventCircleIcon icon = Instantiate(m_iconPrefab);
			icon.CachedTransform.SetParentReset(CachedTransform, true);
			icon.OnSetup(
				new EventCircleIconParameter {
					Product = product,
                    OnClick = () => { }
				}
			);
			icon.OnUpdateUI(product);
		}
	}
}
