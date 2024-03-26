using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public partial class TileItem
{
    private TweenContext? m_positionTween;
	private TweenContext? m_scaleTween;
	private TweenContext? m_dimTween;
	private TweenContext? m_iconAlphaTween;
    private Sequence m_jumpSequence;
    
#region Tween Context

	public struct TweenContext : IDisposable
	{
		private readonly Tweener m_tweener;
        private CancellationTokenSource m_tokenSource;

		public TweenContext(Tweener tweener)
		{
			m_tweener = tweener;
            m_tokenSource = new();
		}

		public UniTask OnChangeValue(Vector3 value, float duration)
		{
            m_tweener
				.ChangeEndValue(value, duration, true)
				.Restart();

			return m_tweener
			.AsyncWaitForCompletion()
			.AsUniTask()
			.AttachExternalCancellation(m_tokenSource.Token);
		}

		public UniTask OnChangeValue(float value, float duration)
		{
            m_tweener
				.ChangeEndValue(value, duration, true)
				.Restart();

			return m_tweener
			.AsyncWaitForCompletion()
			.AsUniTask()
			.AttachExternalCancellation(m_tokenSource.Token);
		}

		public UniTask OnChangeValue(Color value, float duration)
		{
			m_tweener
				.ChangeEndValue(value, duration, true)
				.Restart();

			return m_tweener
			.AsyncWaitForCompletion()
			.AsUniTask()
			.AttachExternalCancellation(m_tokenSource.Token);
		}

        public UniTask OnChangeValue(Vector3 value, float delay, float duration)
		{
			m_tweener
                .ChangeEndValue(value, duration, true)
                .Restart(true, delay);

			return m_tweener
			.AsyncWaitForCompletion()
			.AsUniTask()
			.AttachExternalCancellation(m_tokenSource.Token);
		}

		/*public UniTask OnChangeValue(Vector3 value, float duration, TweenCallback onComplete)
		{
			m_tweener
                .ChangeEndValue(value, duration, true)
                .Restart();

			return m_tweener
			.AsyncWaitForCompletion()
			.AsUniTask()
			.ContinueWith(() => onComplete?.Invoke())
			.AttachExternalCancellation(m_tokenSource.Token);
		}

        public UniTask OnChangeValue(Vector3 value, float delay, float duration, TweenCallback onComplete)
		{
			m_tweener
                .ChangeEndValue(value, duration, true)
                .Restart(true, delay);

			return m_tweener
			.AsyncWaitForCompletion()
			.AsUniTask()
			.ContinueWith(() => onComplete?.Invoke())
			.AttachExternalCancellation(m_tokenSource.Token);
		}*/

		public void Dispose()
		{
			m_tokenSource?.Dispose();
		}
	}

#endregion Tween Context
}
