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

    private EventCircleIcon dailyRewardIcon;
    private EventCircleIcon beginnerBundleIcon;
    private EventCircleIcon weekend1BundleIcon;
    private EventCircleIcon weekend2BundleIcon;

    private GlobalData globalData { get { return GlobalData.Instance; } }

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
                m_cachedIcons.Add(dailyRewardIcon = CreateIcon(GlobalDefine.ProductIndex_DailyBonus, async () => { if (globalData.IsEnableShowPopup_MainScene()) await globalData.ShowPopup_DailyRewards(); }));
				//CreateIcon(20301); // Piggy Bank
				break;
			case Direction.RIGHT:
				m_cachedIcons.Add(beginnerBundleIcon = CreateIcon(GlobalDefine.ProductIndex_Beginner, async () => { if (globalData.IsEnableShowPopup_MainScene()) await globalData.ShowPopup_Beginner(RefreshIcons); }));
                m_cachedIcons.Add(weekend1BundleIcon = CreateIcon(GlobalDefine.ProductIndex_Weekend1, async () => { if (globalData.IsEnableShowPopup_MainScene()) await globalData.ShowPopup_Weekend1(RefreshIcons); }));
                m_cachedIcons.Add(weekend2BundleIcon = CreateIcon(GlobalDefine.ProductIndex_Weekend2, async () => { if (globalData.IsEnableShowPopup_MainScene()) await globalData.ShowPopup_Weekend2(RefreshIcons); }));
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
                if (GlobalDefine.IsOpen_DailyRewards())
                {
                    dailyRewardIcon.gameObject.SetActive(true);
                    
                    DateTime targetTime = GlobalDefine.ToDateTime(GlobalData.Instance.userManager.Current.DailyRewardDate);
                    TimeSpan ts = targetTime.Subtract(DateTime.Now);
                    dailyRewardIcon.RefreshTime(!GlobalDefine.IsEnable_DailyRewards(), string.Format(GlobalDefine.remainTimeFormat, ts.Hours, ts.Minutes, ts.Seconds));
                }
                else
                {
                    dailyRewardIcon.gameObject.SetActive(false);
                }
                break;

            case Direction.RIGHT:
                if (GlobalDefine.IsOpen_BeginnerBundle())
                {
                    beginnerBundleIcon.gameObject.SetActive(true);
                    beginnerBundleIcon.RefreshTime(false, string.Empty);
                }
                else
                {
                    beginnerBundleIcon.gameObject.SetActive(false);
                }

                if (GlobalDefine.IsWeekend())
                {
                    if (GlobalDefine.IsOpen_Weekend1Bundle())
                    {
                        weekend1BundleIcon.gameObject.SetActive(true);
                        
                        DateTime targetTime = GlobalDefine.ToDateTime(GlobalData.Instance.userManager.Current.WeekendEndDate);
                        TimeSpan ts = targetTime.Subtract(DateTime.Now);
                        weekend1BundleIcon.RefreshTime(true, string.Format(GlobalDefine.remainTimeFormat, ts.Hours, ts.Minutes, ts.Seconds));
                    }
                    else
                    {
                        weekend1BundleIcon.gameObject.SetActive(false);

                        if (GlobalDefine.IsOpen_Weekend2Bundle())
                        {
                            weekend2BundleIcon.gameObject.SetActive(true);
                            
                            DateTime targetTime = GlobalDefine.ToDateTime(GlobalData.Instance.userManager.Current.WeekendEndDate);
                            TimeSpan ts = targetTime.Subtract(DateTime.Now);
                            weekend2BundleIcon.RefreshTime(true, string.Format(GlobalDefine.remainTimeFormat, ts.Hours, ts.Minutes, ts.Seconds));
                        }
                        else
                        {
                            weekend2BundleIcon.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    weekend1BundleIcon.gameObject.SetActive(false);
                    weekend2BundleIcon.gameObject.SetActive(false);
                }
                break;
        }
    }
}
