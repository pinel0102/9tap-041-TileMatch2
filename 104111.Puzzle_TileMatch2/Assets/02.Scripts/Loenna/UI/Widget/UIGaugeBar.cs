using UnityEngine;
using UnityEngine.UI;

using System.Threading;

using TMPro;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using NineTap.Common;

public class UIGaugeBarParameter
{
	public int CurrentNumber;
	public int MaxNumber;
}

public class UIGaugeBar : CachedBehaviour
{
	public const string TEXT_FORMAT = "{0}/{1}";

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Image m_barImage;

	[SerializeField]
	private TMP_Text m_text;

	public float Alpha { set => m_canvasGroup.alpha = value; }

	public void OnSetup(UIGaugeBarParameter parameter)
	{
		m_barImage.fillAmount = 0f;

		if (m_text != null)
		{
			m_text.text = string.Format(TEXT_FORMAT, parameter.CurrentNumber, parameter.MaxNumber);
		}
	}

	public void OnUpdateUI(int amount, int max)
	{
		m_barImage.fillAmount = max > 0? Mathf.Clamp01((float)amount / max) : 0f;
		
		if (m_text != null)
		{
			m_text.text = string.Format(TEXT_FORMAT, amount, max);
		}
	}

	public async UniTask OnUpdateUIAsync(int prev, int next, int max)
	{
		CancellationToken token = this.GetCancellationTokenOnDestroy();
		m_barImage.fillAmount = max > 0? Mathf.Clamp01((float)prev / max) : 0f;

		await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);

		if (max <= 0)
		{
			return;
		}

		Alpha = 1f;
		await m_barImage.DOFillAmount(Mathf.Clamp01((float)next / max), 0.5f).ToUniTask().SuppressCancellationThrow();
		
		if (m_text != null)
		{
			m_text.text = string.Format(TEXT_FORMAT, next, max);
		}
	}
}
