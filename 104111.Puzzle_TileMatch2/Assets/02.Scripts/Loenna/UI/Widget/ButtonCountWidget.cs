using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Cysharp.Threading.Tasks;

using NineTap.Common;

[ResourcePath("UI/Widgets/ButtonCountWidget")]
public class ButtonCountWidget : CachedBehaviour
{
	[SerializeField]	private Image m_icon;
	[SerializeField]	private TMP_Text m_countText;
    [SerializeField]	private GameObject m_priceObject;
    [SerializeField]	private TMP_Text m_priceText;
    
    private const int COUNT_MAX = 100;
    private const string STR_OVER_MAX = "99+";

    public void BindTo(IUniTaskAsyncEnumerable<int> binder, ProductData product)
	{
		binder.BindTo(m_icon, (_, count) => {
            m_icon.gameObject.SetActive(count > 0);
            m_priceObject.SetActive(count <= 0);
            m_countText.text = count < COUNT_MAX ? count.ToString() : STR_OVER_MAX;
            m_priceText.text = Mathf.FloorToInt(product.Price).ToString();
        });
		//binder.BindTo(m_countText, (_, count) => m_countText.text = count < 100? count.ToString() : "99+");
	}
}
