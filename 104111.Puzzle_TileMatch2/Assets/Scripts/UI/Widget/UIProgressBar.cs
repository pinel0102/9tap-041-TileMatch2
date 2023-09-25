using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using TMPro;

using NineTap.Common;

public class UIProgressBarParameter
{
	public UIProgressBar.Type Type;
	public bool VisibleText;
}

public class UIProgressBar : CachedBehaviour
{
	public const string TEXT_FORMAT = "{0}/{1}";

	public enum Type
	{
		STATIC,
		DYNAMIC
	}

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Slider m_progressBar;

	[SerializeField]
	private TMP_Text m_text;

	private UnityAction m_onCompleted;

	private float m_currentValue = 0f;
	private float m_progressValue = 0f;
	private float m_maxValue = 0f;
	private bool m_startProgress = false;

	private Type m_type = Type.STATIC;

	private TimeManager m_timeManager;

	public float Alpha { set => m_canvasGroup.alpha = value; }
	public UnityAction OnCompleted => m_onCompleted;

	public void OnSetup(UIProgressBarParameter parameter)
	{
		m_type = parameter.Type;
		m_timeManager = Game.Inst.Get<TimeManager>();
		m_timeManager.OnUpdateEveryFrame += OnProgress;
		m_progressValue = 0f;

		m_text.gameObject.SetActive(parameter.VisibleText);
	}

	public void OnDestroy()
	{
		if (m_timeManager != null)
		{
			m_timeManager.OnUpdateEveryFrame -= OnProgress;
		}
	}

	public void OnUpdateUI(float value)
	{
		m_maxValue = -1;
		m_progressValue = value;

		if (m_type is Type.STATIC)
		{
			m_progressBar.value = Mathf.Clamp01(m_progressValue);
			UpdateLabel();
			return;
		}

		m_startProgress = true;
	}

	public void OnUpdateUI(int currentValue, int maxValue)
	{
		m_maxValue = maxValue;
		m_currentValue = currentValue / (float)maxValue;
		m_progressValue = 1f;

		if (m_type is Type.STATIC)
		{
			m_progressBar.value = Mathf.Clamp01(m_currentValue);
			UpdateLabel();
			return;
		}

		m_startProgress = true;
	}

	private void OnProgress(float tick)
	{
		if (m_startProgress)
		{
			m_currentValue += tick;
			var fillAmount = Mathf.Lerp(m_currentValue, m_progressValue, m_currentValue / m_progressValue);
			m_progressBar.value = Mathf.Clamp01(fillAmount);
			
			if (fillAmount >= 1f)
			{
				m_onCompleted?.Invoke();
				m_startProgress = false;
			}

			UpdateLabel();
		}
	}

	private void UpdateLabel()
	{
		if (m_maxValue < 0)
		{
			m_text.text = $"{m_currentValue * 100:F}%";
			return;
		}
		m_text.text = string.Format(TEXT_FORMAT, Mathf.RoundToInt(m_currentValue * m_maxValue), (int)m_maxValue);
	}
}
