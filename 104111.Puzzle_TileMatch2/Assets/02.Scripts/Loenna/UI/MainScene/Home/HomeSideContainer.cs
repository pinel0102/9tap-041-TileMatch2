using UnityEngine;
using System.Collections.Generic;
using NineTap.Common;
using System;

#pragma warning disable 8321
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
	private ProductDataTable m_productDataTable;
    private List<EventCircleIcon> m_cachedIcons = new List<EventCircleIcon>();
    public List<EventCircleIcon> CachedIcons => m_cachedIcons;

    public EventCircleIcon dailyRewardIcon;

	public void OnSetup()
	{
		m_productDataTable = Game.Inst.Get<TableManager>().ProductDataTable;
		m_iconPrefab = ResourcePathAttribute.GetResource<EventCircleIcon>();
		m_cachedIcons.Clear();

		CreateIcons();
        RefreshIcons();
	}

	public void CreateIcons()
	{
        switch (m_direction)
		{
			case Direction.LEFT:
                m_cachedIcons.Add(dailyRewardIcon = CreateIcon(30000, async () => { await GlobalData.Instance.ShowDailyRewardPopup(); }));
				//CreateIcon(20301); // Piggy Bank
				break;
			case Direction.RIGHT:
				//CreateIcon(20207); // Beginner
				//CreateIcon(20211); // Weekend
				break;
		}

		EventCircleIcon CreateIcon(int index, Action onClick = null)
		{
			ProductData product = m_productDataTable[index];
			EventCircleIcon icon = Instantiate(m_iconPrefab);
			icon.CachedTransform.SetParentReset(CachedTransform, true);
			icon.OnSetup(
				new EventCircleIconParameter {
					Product = product,
                    OnClick = onClick
				}
			);
			icon.OnUpdateUI(product);

            return icon;
		}
	}

    public void RefreshIcons()
    {
        switch (m_direction)
		{
			case Direction.LEFT:
                if (GlobalDefine.IsOpenDailyRewards())
                {
                    dailyRewardIcon.gameObject.SetActive(true);
                    
                    DateTime targetTime = GlobalDefine.ToDateTime(GlobalData.Instance.userManager.Current.DailyRewardDate);
                    TimeSpan ts = targetTime.Subtract(DateTime.Now);
                    dailyRewardIcon.RefreshTime(!GlobalDefine.IsEnableDailyRewards(), string.Format(GlobalDefine.remainTimeFormat, ts.Hours, ts.Minutes, ts.Seconds));
                }
                else
                {
                    dailyRewardIcon.gameObject.SetActive(false);
                }
                break;
        }
    }
}
