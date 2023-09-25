using UnityEngine;
using UnityEngine.UI;

using System;

using Cysharp.Threading.Tasks;

using NineTap.Common;

public class LoadingView : CachedBehaviour, IProgress<float>
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private CanvasGroup m_sliderCanvasGroup;

	[SerializeField]
	private Slider m_progressBar;

	private IAsyncReactiveProperty<bool> m_loadCompleted;
	public IReadOnlyAsyncReactiveProperty<bool> Completed => m_loadCompleted;

	private TimeManager m_timeManager;

	private float m_currentValue = 0f;
	private float m_progressValue = 0f;
	private bool m_startProgress = false;

	private void Awake()
	{
		m_canvasGroup.alpha = 0f;
	}

	private void OnDestroy()
	{
		if (m_timeManager != null)
		{
			m_timeManager.OnUpdateEveryFrame -= OnProgress;
		}
	}

    public void OnSetup(TimeManager timeManager)
	{
		m_timeManager = timeManager;
		m_loadCompleted = new AsyncReactiveProperty<bool>(false).WithDispatcher();
		m_timeManager.OnUpdateEveryFrame += OnProgress;
		m_progressValue = 0f;
	}

	public void StartProgress()
	{
		m_startProgress = true;
	}

	private void OnProgress(float tick)
	{
		if (m_startProgress)
		{
			m_currentValue += tick;
			var fillAmount = Mathf.Lerp(m_currentValue, m_progressValue, m_currentValue / m_progressValue);
			m_progressBar.value = Mathf.Clamp01(fillAmount);
			
			m_loadCompleted.Value = fillAmount >= 1f;
		}
	}

	public void Report(float value)
	{
		m_progressValue = value;
	}

	public void Show()
	{
		m_progressBar.value = 0f;
		m_canvasGroup.alpha = 1f;
	}
}
