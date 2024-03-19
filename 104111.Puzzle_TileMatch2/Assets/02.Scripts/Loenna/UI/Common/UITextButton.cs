using UnityEngine;
using UnityEngine.UI;

using System;
using System.Threading;

using TMPro;

using Cysharp.Threading.Tasks;

using DG.Tweening;

public class UITextButtonParameter : UIButtonParameter
{
	public Color TextColor = Color.white;
	public string ButtonText;
	public IUniTaskAsyncEnumerable<string> ButtonTextBinder;

	public Func<GameObject> SubWidgetBuilder = null;
}

public class UITextButton : UIImageButton
{
	[SerializeField]
	protected TMP_Text m_textField;

	protected Color m_textOriginalColor = Color.white;

	public override void OnSetup(UIButtonParameter buttonParameter)
	{
		base.OnSetup(buttonParameter);

		if (buttonParameter is UITextButtonParameter parameter)
		{
			m_canvasGroup.alpha = parameter.FadeEffect? 0f : 1f;
			m_textField.text = parameter.ButtonText;
			m_textOriginalColor = parameter.TextColor;

			if (parameter.ButtonTextBinder != null)
			{
				parameter.ButtonTextBinder.BindTo(m_textField);
			}

			if (parameter.SubWidgetBuilder != null)
			{
				GameObject go = parameter.SubWidgetBuilder.Invoke();
				go?.transform.SetParentReset(m_subWidgetParent);
			}

			return;
		}

		m_textField.text = string.Empty;
	}

    public void SetInteractable(bool interactable)
    {
        SetInteractable(this, interactable);
    }

	protected override void SetInteractable(Button button, bool interactable)
	{
		base.SetInteractable(button, interactable);

		m_textField.color = interactable? m_textOriginalColor : Color.gray;
	}

	public async UniTask ShowAsync(float duration, CancellationToken token)
	{
		m_canvasGroup.alpha = 0f;

		CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
			token, 
			this.GetCancellationTokenOnDestroy()
		);

		await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationTokenSource.Token);
		await m_canvasGroup.DOFade(1f, duration).ToUniTask().SuppressCancellationThrow();
	}
}
