using UnityEngine;
using UnityEngine.UI;

using NineTap.Common;

public class BoardCountItem : CachedBehaviour
{
	[SerializeField]
	private GameObject m_linker;

	[SerializeField]
	private Graphic m_selected;

	public void OnSetup(bool existNext)
	{
		m_linker.SetActive(existNext);
	}

	public void OnUpdateUI(bool isSelected)
	{
		m_selected.color = isSelected switch {
			true => Color.white,
			_=> Color.clear
		};
	} 
}
