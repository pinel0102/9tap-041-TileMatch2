using UnityEngine;

using System.Linq;

using Gpm.Ui;

using TMPro;

using NineTap.Common;

public class MainSceneFragmentContentParameter_Store 
: ScrollViewFragmentContentParameter
{
	public string TitleText;
	public UIImageButtonParameter CloseButtonParameter;
}

[ResourcePath("UI/Fragments/Fragment_Store")]
public class MainSceneFragmentContent_Store : ScrollViewFragmentContent
{
	[SerializeField]
	private TMP_Text m_title;

	[SerializeField]
	private UIImageButton m_closeButton;

	[SerializeField]
	private InfiniteScroll m_scrollView;

    public GameObject banner;

	public override void OnSetup(ScrollViewFragmentContentParameter contentParameter)
	{
		if (contentParameter is not MainSceneFragmentContentParameter_Store parameter) 
		{
			return;
		}

		m_title.text = parameter.TitleText;

		m_closeButton.OnSetup(parameter.CloseButtonParameter);

		ProductDataTable productDataTable = Game.Inst.Get<TableManager>().ProductDataTable;

		var productItemDatas = productDataTable
			.GetProducts(exposingType: ProductExposingType.Store)
			.OrderByDescending(data => (int)data.UIType)
			.Select(data => new ShopProductScrollItemData { ProductData = data })
			.ToArray();
		
		m_scrollView.InsertData(productItemDatas);

        GlobalDefine.RefreshADFreeUI();
	}
}
