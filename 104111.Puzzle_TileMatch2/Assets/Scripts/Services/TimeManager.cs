using UnityEngine;

using System;
using System.Threading;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class TimeManager : IDisposable
{
	public readonly CancellationTokenSource m_cancellationTokenSource;
	// 프레임 마다 업데이트
	public event Action<float> OnUpdateEveryFrame;
	// 초 마다 업데이트
	public event Action<TimeSpan> OnUpdateEverySecond;

	public TimeManager()
	{
		m_cancellationTokenSource = new();

		UniTask.Void(
			async token => {
				await UniTask.WhenAll(
					UniTaskAsyncEnumerable
					.EveryUpdate(PlayerLoopTiming.LastPostLateUpdate)
					.ForEachAsync(
						_ => {
							if (token.IsCancellationRequested)
							{
								return;
							}
							OnUpdateEveryFrame?.Invoke(Time.deltaTime);
						}
					),
					UniTaskAsyncEnumerable
					.Interval(TimeSpan.FromSeconds(1f))
					.ForEachAsync(
						_ => {
							if (token.IsCancellationRequested)
							{
								return;
							}
							OnUpdateEverySecond?.Invoke(TimeSpan.FromSeconds(1f));
						}
					)
				);
			},
			m_cancellationTokenSource.Token
		);
	}

    public void Dispose()
    {
		OnUpdateEveryFrame = null;
		OnUpdateEverySecond = null;
        m_cancellationTokenSource?.Dispose();
    }
}
