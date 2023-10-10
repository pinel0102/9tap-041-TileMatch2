using UnityEngine;
using UnityEngine.UI;

using System;

public class UIImageButtonParameter: UIButtonParameter
{
	public Func<GameObject> SubWidgetBuilder = null;
}

public class UIImageButton : UIButton
{
	[SerializeField]
	protected Image m_icon;

	[SerializeField]
	protected Transform m_subWidgetParent;

	public override void OnSetup(UIButtonParameter buttonParameter)
	{
		base.OnSetup(buttonParameter);

		if (buttonParameter is UIImageButtonParameter parameter)
		{
			if (parameter.SubWidgetBuilder != null)
			{
				GameObject go = parameter.SubWidgetBuilder.Invoke();
				go.transform.SetParentReset(m_subWidgetParent);
			}
		}
	}

	protected override void SetInteractable(Button button, bool interactable)
	{
		base.SetInteractable(button, interactable);

		if (m_icon != null)
		{
			m_icon.color = interactable? Color.white : Color.gray;
		}
	}
}
