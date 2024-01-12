using UnityEngine;

using System.Collections.Generic;

using Gpm.Ui;

using TMPro;

public class PuzzleThemeContainerItemData : InfiniteScrollData
{
    public string CountryName;
	public List<PuzzleContentData> ContentDatas;
}

public class PuzzleThemeContainerItem : NestedInfiniteScrollItem
{
	[SerializeField]
	private TMP_Text m_title;

	[SerializeField]
	private List<PuzzleContentItem> m_contents;
    public List<PuzzleContentItem> Contents => m_contents;
    public string CountryName;

	public override void UpdateData(InfiniteScrollData scrollData)
	{
		base.UpdateData(scrollData);
		UserManager userManager = Game.Inst.Get<UserManager>();

		if (scrollData is PuzzleThemeContainerItemData itemData)
		{
            CountryName = itemData.CountryName;
			m_title.text = itemData.CountryName;

			for (int i = 0, count = m_contents.Count; i < count; i++)
			{
				PuzzleContentItem item = m_contents[i];
				if (!itemData.ContentDatas.HasIndex(i))
				{
					item.Alpha = 0f;
					continue;
				}

				item.Alpha = 1f;
				item.OnSetup(userManager);
				item.UpdateUI(itemData.ContentDatas[i]);
			}
		}
	}
}
