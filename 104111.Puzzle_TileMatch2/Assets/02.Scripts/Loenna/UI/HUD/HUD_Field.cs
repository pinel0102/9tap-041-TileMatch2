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
    public IUniTaskAsyncEnumerable<(bool, string, string)> LifeStatus;
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
	private TMP_Text m_timeText;

    [SerializeField]
    private GameObject boosterTimeObject;

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
        else if (parameter.LifeStatus != null)
		{
            parameter.LifeStatus.BindTo(m_text, (component, status) => component.text = status.Item2);
            parameter.LifeStatus.BindTo(m_timeText, (component, status) => {
                    boosterTimeObject.SetActive(status.Item1); 
                    component.text = status.Item3;
                    component.color = status.Item1 ? Constant.UI.COLOR_BOOSTER_TIME : Constant.UI.COLOR_WHITE;
                });
		}
	}

	public void SetVisible(bool visible)
	{
		m_canvasGroup.alpha = visible? 1f : 0f;
	}

    public void AddListener(Action OnClick)
    {
        m_button.onClick.AddListener(() => {    OnClick?.Invoke();  });
    }

    public void SetText(long _text)
    {
        SetText(_text.ToString());
    }

    public void SetText(string _text)
    {
        m_text.SetText(_text);
    }
}
