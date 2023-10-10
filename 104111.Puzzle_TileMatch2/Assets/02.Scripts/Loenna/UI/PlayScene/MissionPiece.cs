using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;

using Coffee.UIEffects;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using NineTap.Common;

public class MissionPiece : CachedBehaviour
{
	[Serializable]
	public class PieceWidget
	{
		[SerializeField]
		UITransitionEffect m_transition;

		[SerializeField]
		private RectTransform m_root;

		[SerializeField]
		private Image m_icon;

		public void SetIcon(Sprite sprite)
		{
			m_icon.sprite = sprite;
		}

		public void SetActive(bool active)
		{
			if (!active)
			{
				UniTask.Void(
					async () => {
						await DOTween.To(() => m_transition.effectFactor, factor => m_transition.effectFactor = factor, 0f, 0.25f)
							.ToUniTask()
							.SuppressCancellationThrow();
						m_root.gameObject.SetActive(false);
					}
				);

				return;
			}

			m_transition.effectFactor = 1f;
			m_root.gameObject.SetActive(true);
		}
	}

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private List<PieceWidget> m_widgets = new();

	public void OnUpdateUI(Sprite sprite, int remain)
	{
		CachedGameObject.SetActive(remain > 0);
		
		for (int i = 0, widgetCount = m_widgets.Count; i < widgetCount; i++)
		{
			var widget = m_widgets[i];
			if (i >= remain)
			{
				widget.SetActive(false);
				continue;
			}

			widget.SetActive(true);
			widget.SetIcon(sprite);
		}
	}

	public void SetVisible(bool visible)
	{
		if (visible)
		{
			m_canvasGroup		
			.DOFade(1f, 0.1f)
			.ToUniTask(TweenCancelBehaviour.KillWithCompleteCallbackAndCancelAwait)
			.Forget();
			
			return;
		}

		m_canvasGroup.alpha = 0f;
	}
}
