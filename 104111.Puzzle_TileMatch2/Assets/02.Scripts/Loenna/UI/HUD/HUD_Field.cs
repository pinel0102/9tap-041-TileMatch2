using UnityEngine;
using UnityEngine.UI;

using System;

using TMPro;

using Cysharp.Threading.Tasks;

using NineTap.Common;

public class HUDFieldParameter
{
	public HUDType Type;
	public Action OnClick;
	public IUniTaskAsyncEnumerable<string> FieldBinder;
}

public class HUD_Field : CachedBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Button m_button;

	[SerializeField]
	private TMP_Text m_text;

	[SerializeField]
	private RectTransform m_attractorTarget;

	public Transform AttractorTarget => m_attractorTarget;

	public void OnSetup(HUDFieldParameter parameter)
	{
		m_button.onClick.AddListener(() => parameter?.OnClick?.Invoke());
		
		if (parameter.FieldBinder != null)
		{
			parameter.FieldBinder.BindTo(m_text, (component, text) => component.text = text);
		}
	}

	public void SetVisible(bool visible)
	{
		m_canvasGroup.alpha = visible? 1f : 0f;
	}

    public void AddListener(Action OnClick)
    {
        m_button.onClick.AddListener(() => OnClick?.Invoke());
    }
}
