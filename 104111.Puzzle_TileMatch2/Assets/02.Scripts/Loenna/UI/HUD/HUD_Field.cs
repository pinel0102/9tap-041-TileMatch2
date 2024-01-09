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
	private TMP_Text m_textIncrease;

    [SerializeField]
	private TMP_Text m_timeText;

    [SerializeField]
    private GameObject boosterTimeObject;

    [SerializeField]
	private RectTransform m_attractorTarget;

	public Transform AttractorTarget => m_attractorTarget;
    public RectTransform Icon;

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

    public void SetIncreaseMode(bool isIncrease)
    {
        if(m_textIncrease == null) return;

        m_textIncrease.gameObject.SetActive(isIncrease);
        m_text.gameObject.SetActive(!isIncrease);
    }

    public void SetIncreaseText(long _text, bool autoTurnOn_IncreaseMode = true)
    {
        SetIncreaseText(_text.ToString());
        
        if (autoTurnOn_IncreaseMode)
            SetIncreaseMode(true);
    }

    private void SetIncreaseText(string _text)
    {
        if(m_textIncrease == null) return;
        
        m_textIncrease.SetText(_text);
    }

    public void IncreaseText(long from, int count, float duration = 0.5f, bool autoTurnOff_IncreaseMode = true, Action<long> onUpdate = null)
    {
        SetIncreaseText(from);
        onUpdate?.Invoke(from);

        SetIncreaseMode(true);

        UniTask.Void(
			async token => {
                float delay = GetDelay(duration, count);

                for(int i=1; i <= count; i++)
                {
                    SetIncreaseText(from + i);
                    onUpdate?.Invoke(from + i);
                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                }

                if (autoTurnOff_IncreaseMode)
                    SetIncreaseMode(false);
            },
			this.GetCancellationTokenOnDestroy()
        );

        float GetDelay(float time, int amount)
        {
            return time/(float)amount;
        }
    }
}
