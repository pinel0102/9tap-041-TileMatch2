using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Cysharp.Threading.Tasks;

using NineTap.Common;

[ResourcePath("UI/Widgets/ButtonCountWidget")]
public class ButtonCountWidget : CachedBehaviour
{
	[SerializeField]
	private Image m_icon;

	[SerializeField]
	private TMP_Text m_countText;

	public void BindTo(IUniTaskAsyncEnumerable<int> binder)
	{
		binder.BindTo(m_icon, (_, count) => m_icon.gameObject.SetActive(count > 0));
		binder.BindTo(m_countText, (_, count) => m_countText.text = count < 100? count.ToString() : "99+");
	}
}
