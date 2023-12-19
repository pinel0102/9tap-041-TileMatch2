using UnityEngine;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

public abstract class UIButton : Button
{
	[SerializeField]
	protected CanvasGroup m_canvasGroup;

	public float Alpha { set => m_canvasGroup.alpha = value; }
	
	private TweenerCore<Vector3, Vector3, VectorOptions> m_tweenCore;

	protected override void OnDestroy()
	{
		m_tweenCore?.Kill();
		base.OnDestroy();
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		//base.DoStateTransition(state, instant);

		if (ObjectUtility.IsNullOrDestroyed(transform))
		{
			return;
		}

		if (m_tweenCore != null && m_tweenCore.IsActive() && m_tweenCore.IsPlaying())
		{
			m_tweenCore.Kill(true);
		}

		m_tweenCore = state switch {
			SelectionState.Pressed => ObjectUtility.GetRawObject(transform)?.DOScale(0.9f, 0.1f),
			_ => ObjectUtility.GetRawObject(transform)?.DOScale(1f, 0.1f)
		};
	}

	public virtual void OnSetup(UIButtonParameter buttonParameter)
	{
		SoundManager soundManager = Game.Inst?.Get<SoundManager>();
		transition = Transition.None;
		onClick.AddListener(() => {
				soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
				buttonParameter?.OnClick?.Invoke();
			}
		);
		buttonParameter?.Binder?.BindTo(this, SetInteractable);
	}

	protected virtual void SetInteractable(Button button, bool interactable)
	{
		button.interactable = interactable;
	}
}
