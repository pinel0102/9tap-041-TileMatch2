using UnityEngine;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;

using DG.Tweening;

namespace NineTap.Common
{
	public class UIPopup : UIBase
	{
		public enum TransitionEffect
		{
			NONE,
			FADE,
			SCALE,
			POSITION
		}

		[SerializeField]
		private TransitionEffect m_transitionEffect;
		public TransitionEffect Effect => m_transitionEffect;

		private Canvas m_cachedCanvas = null;
		public Canvas CachedCanvas
		{
			get
			{
				if (m_cachedCanvas == null)
				{
					if (!ObjectUtility.IsNullOrDestroyed(CachedGameObject))
					{
						m_cachedCanvas = CachedGameObject.GetComponent<Canvas>();
					}
				}
				return m_cachedCanvas;
			}
		}

		private GraphicRaycaster m_cachedRaycaster = null;
		public GraphicRaycaster CachedRaycaster
		{
			get
			{
				if (m_cachedRaycaster == null)
				{
					if (!ObjectUtility.IsNullOrDestroyed(CachedGameObject))
					{
						m_cachedRaycaster = CachedGameObject.GetComponent<GraphicRaycaster>();
					}
				}
				return m_cachedRaycaster;
			}
		}

		private RectTransform m_view = null;
		public RectTransform View
		{
			get
			{
				if (m_view == null)
				{
					if (!CachedTransform.TryGetComponentAtPath("View", out RectTransform result))
					{
						result = ObjectUtility.GetRawObject(CachedRectTransform);
					}

					m_view = result;
				}

				return m_view;
			}
		}

		private CanvasGroup m_background = null;
		public CanvasGroup Background
		{
			get
			{
				if (m_background == null)
				{
					if (View.TryGetComponentAtPath("UI_Back", out Transform target))
					{
						if (!target.TryGetComponent<CanvasGroup>(out var result))
						{
							result = target.gameObject.AddComponent<CanvasGroup>();
						}

						m_background = result;
					}
				}

				return m_background;
			}
		}

		private CanvasGroup m_canvasGroup = null;
		public CanvasGroup CanvasGroup
		{
			get
			{
				if (m_canvasGroup == null)
				{
					if (View.TryGetComponent<CanvasGroup>(out var result))
					{
						m_canvasGroup = result;
					}
					else
					{
						m_canvasGroup = View.gameObject.AddComponent<CanvasGroup>();
					}
				}

				return m_canvasGroup;
			}
		}

		public virtual void OnClickClose()
		{
			UIManager.ClosePopupUI(this);
		}

		public override void OnSetup(UIParameter uiParameter)
		{
			
		}

		public override void Show()
		{
			if (Effect is TransitionEffect.NONE)
			{
				base.Show();
				return;
			}

			CachedRaycaster.enabled = false;
			CachedGameObject.SetActive(true);

			switch (Effect)
			{
				case TransitionEffect.FADE:
					CanvasGroup.alpha = 0f;
					break;
				case TransitionEffect.SCALE:
					if(Background != null)
					{
						Background.transform.localScale = Vector3.one * 10f;
					}
					View.localScale = Vector2.one * 0.1f;
					break;
			}

			UniTask.Void(
				async () =>
				{
					await UniTask.WaitUntil(
						() => CachedGameObject.activeSelf
					);

					switch (Effect)
					{
						case TransitionEffect.FADE:
							await CanvasGroup
								.DOFade(1f, 0.25f)
								.ToUniTask()
								.AttachExternalCancellation(this.GetCancellationTokenOnDestroy())
								.SuppressCancellationThrow();
							break;
						case TransitionEffect.SCALE:
							bool canceled = await View
								.DOScale(Vector3.one, 0.25f)
								.SetEase(Ease.OutBack)
								.ToUniTask()
								.AttachExternalCancellation(this.GetCancellationTokenOnDestroy())
								.SuppressCancellationThrow();
							

							if (!canceled && Background != null)
							{
								Background.transform.localScale = Vector3.one;
							}
							break;
						//case TransitionEffect.POSITION:
						//	View.pivot = new Vector2(0.5f, 1f);;
					}

					await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
					if(!ObjectUtility.IsNullOrDestroyed(CachedRaycaster))
					{
						CachedRaycaster.enabled = true;
					}
					OnShow();
				}
			);
		}

		public override void OnShow()
		{
			
		}

		public override void OnHide()
		{
			
		}
	}
}
