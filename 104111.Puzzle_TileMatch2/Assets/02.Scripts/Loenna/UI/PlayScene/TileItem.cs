using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using DG.Tweening;

using AssetKits.ParticleImage;

using NineTap.Common;

public class TileItemParameter
{
	public Action<TileItem> OnClick;
}

[ResourcePath("UI/Widgets/TileObject")]
public class TileItem : CachedBehaviour
{
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

		public UniTask OnChangeValue(Vector3 value, float duration, TweenCallback onComplete)
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

		public void Dispose()
		{
			m_tokenSource?.Dispose();
		}
	}

	private TileDataTable m_tileDataTable;

	[SerializeField]
	private RectTransform m_view;

	[SerializeField]
	private Image m_icon;

	[SerializeField]
	private EventTrigger m_trigger;

	[SerializeField]
	private MissionPiece m_missionTile;

	[SerializeField]
	private CanvasGroup m_dim;

	[SerializeField]
	private Text m_tmpText; //이미지가 없을 경우 텍스트로

	[SerializeField]
	private ParticleImage m_disappearEffect;

	private TileItemModel m_current;
	public TileItemModel Current => m_current;

	private bool m_interactable = false;

	private TweenContext? m_positionTween;
	private TweenContext? m_scaleTween;
	private TweenContext? m_dimTween;
	private TweenContext? m_iconAlphaTween;

	private Vector3 m_originWorldPosition;

	private void OnDestroy()
	{
		m_positionTween?.Dispose();
		m_scaleTween?.Dispose();
		m_iconAlphaTween?.Dispose();
		m_dimTween?.Dispose();
	}

	public void Release()
	{
        m_disappearEffect.gameObject.SetActive(false);
		m_scaleTween?.OnChangeValue(Vector3.one, 0f).Forget();
		m_iconAlphaTween?.OnChangeValue(Color.white, -1f).Forget();
		CachedGameObject.SetActive(false);
		m_interactable = false;
		m_icon.color = Color.white;
		m_dim.alpha	= 0f;
		m_current = null;
	}

	public void SetActive(bool enabled)
	{
		m_view.SetLocalScale(Vector2.one);
		CachedGameObject.SetActive(enabled);
	}

	public void OnSetup(TileItemParameter parameter)
	{
		m_tileDataTable = Game.Inst.Get<TableManager>().TileDataTable;
		SoundManager soundManager = Game.Inst.Get<SoundManager>();

		m_positionTween = new TweenContext(
			tweener: ObjectUtility.GetRawObject(CachedTransform)?
				.DOMove(Vector2.zero, Constant.Game.TWEEN_DURATION_SECONDS)
				.Pause()
				.SetAutoKill(false)
		);

		m_scaleTween = new TweenContext(
			tweener: ObjectUtility.GetRawObject(m_view)?
				.DOScale(Vector3.one, Constant.Game.DEFAULT_DURATION_SECONDS)
				.SetAutoKill(false)
		);

		m_dimTween = new TweenContext(
			tweener: ObjectUtility.GetRawObject(m_dim)?
				.DOFade(0f, Constant.Game.DEFAULT_DURATION_SECONDS)
				.SetAutoKill(false)
		);

		m_iconAlphaTween = new TweenContext(
			tweener: ObjectUtility.GetRawObject(m_icon)?
				.DOFade(1f, -1f)
				.SetAutoKill(false)
		);

		List<EventTrigger.Entry> entries = new List<EventTrigger.Entry> {
			new EventTrigger.Entry { eventID = EventTriggerType.PointerDown },
			new EventTrigger.Entry { eventID = EventTriggerType.PointerUp }
		};

		foreach (var entry in entries)
		{
			entry.callback
			.OnInvokeAsAsyncEnumerable(this.GetCancellationTokenOnDestroy())
			.SubscribeAwait(
				eventData => OnTriggerCallback(entry.eventID, eventData)
			);
		}

		m_trigger.triggers.AddRange(entries);

		ObjectUtility.GetRawObject(m_missionTile)?.SetVisible(false);

		UniTask OnTriggerCallback(EventTriggerType type, BaseEventData eventData)
		{
			if (!m_interactable)
			{
				return UniTask.CompletedTask;
			}

            // Press 단계에서 바로 이동.
            if (type is EventTriggerType.PointerDown)
			{
				soundManager.PlayFx(Constant.UI.BUTTON_CLICK_FX_NAME);
                m_interactable = false;
				//parameter.OnClick?.Invoke(this);
                Debug.Log(CodeManager.GetMethodName() + string.Format("Scale 1.25f : {0}", m_current.Guid));

                return m_scaleTween?.OnChangeValue(Vector3.one * 1.25f, Constant.Game.DEFAULT_DURATION_SECONDS * 0.25f, () => {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("Scale 1.00f : {0}", m_current.Guid));
                    m_scaleTween?.OnChangeValue(Vector3.one, Constant.Game.DEFAULT_DURATION_SECONDS * 0.25f, () => {
                        parameter.OnClick?.Invoke(this);
                    });                    
                }) ?? UniTask.CompletedTask;
            }

			/*if (type is EventTriggerType.PointerDown)
			{
				soundManager.PlayFx(Constant.UI.BUTTON_CLICK_FX_NAME);
				return m_scaleTween?.OnChangeValue(Vector3.one * 1.25f, Constant.Game.DEFAULT_DURATION_SECONDS) ?? UniTask.CompletedTask;
			}

			if (type is EventTriggerType.PointerUp && eventData is PointerEventData pointerEventData)
			{
				#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
				var sqrMagnitude = Vector2.SqrMagnitude((Vector2)CachedTransform.position - pointerEventData.position);

				if (sqrMagnitude < 4500f)
				{
				#endif
					m_interactable = false;
					parameter.OnClick?.Invoke(this);
				
				#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
				}
				#endif
			}

			return m_scaleTween?.OnChangeValue(Vector3.one, Constant.Game.DEFAULT_DURATION_SECONDS) ?? UniTask.CompletedTask;*/

            return UniTask.CompletedTask;
		}
	}

	public bool OnUpdateUI(TileItemModel item, bool ignoreInvisible, out LocationType changeLocation)
	{
		changeLocation = item?.Location ?? LocationType.POOL;
		
		if (m_current == null)
		{
			CachedRectTransform.SetLocalPosition(item.Position);
			if (item?.Location == LocationType.POOL)
			{
                CachedRectTransform.SetParentReset(UIManager.SceneCanvas.transform);
				SetActive(false);
				CachedTransform.Reset();
				return false;
			}
		}

		LocationType currentType = m_current?.Location ?? LocationType.POOL;

		if ((changeLocation, currentType) is (LocationType.BOARD, LocationType.BOARD))
		{
			m_originWorldPosition = CachedTransform.parent.TransformPoint(item.Position);
			CachedRectTransform.SetLocalPosition(item.Position);
		}

		m_current = item;
		SetInteractable(item.Location, item.Overlapped, item.InvisibleIcon, ignoreInvisible).Forget();

		string path = m_tileDataTable.TryGetValue(item.Icon, out var rowData)? rowData.Path : string.Empty;
		Sprite sprite = SpriteManager.GetSprite(path);
		m_icon.sprite = sprite;
		
		(m_tmpText.text, m_icon.enabled) = m_icon.sprite switch {
			null => (path.Replace("Sprites/Tile/UI_Img_", string.Empty), false),
			_ => (string.Empty, true)
		};
		m_missionTile.OnUpdateUI(sprite, item.Location is LocationType.BOARD? item.GoldPuzzleCount : -1);

		return currentType != item.Location;
	}

	public UniTask OnChangeLocation(LocationType location, Vector2? moveAt = null, float duration = Constant.Game.TWEEN_DURATION_SECONDS)
	{
		if (!moveAt.HasValue && Current == null)
		{
			return UniTask.CompletedTask;
		}

		Vector2 direction = moveAt ?? Current.Position;

        if (location == LocationType.POOL)
            Debug.Log(CodeManager.GetMethodName() + string.Format("Scale 0 : {0}", m_current.Guid));

		return (location, Current != null) switch {
			(LocationType.STASH or LocationType.BASKET, _) => m_positionTween?.OnChangeValue(direction, duration) ?? UniTask.CompletedTask,
			(LocationType.BOARD, true) => m_positionTween?.OnChangeValue(m_originWorldPosition, duration) ?? UniTask.CompletedTask,
			(LocationType.POOL, _) => m_scaleTween?.OnChangeValue(Vector3.zero, 0.15f, () => m_disappearEffect.gameObject.SetActive(true)) ?? UniTask.CompletedTask,
			_ => UniTask.CompletedTask
		};
	}

	private async UniTask SetInteractable(LocationType location, bool overlapped, bool invisibleIcon = false, bool ignoreInvisible = false)
	{
		switch (location)
		{
			case LocationType.BOARD:
				m_interactable = !overlapped;

				ObjectUtility.GetRawObject(m_missionTile)?.SetVisible(!invisibleIcon);
				
				if (overlapped)
				{
					Color color = Color.white.WithA(invisibleIcon && !ignoreInvisible? 0f : 1f);
					if (m_iconAlphaTween.HasValue)
					{
						await m_iconAlphaTween.Value.OnChangeValue(color, -1f);
					}
					await UniTask.Defer(() => m_dimTween?.OnChangeValue(1f, 0f) ?? UniTask.CompletedTask);
				}
				else
				{
					if (!invisibleIcon && m_iconAlphaTween.HasValue)
					{
						await m_iconAlphaTween.Value.OnChangeValue(Color.white, -1f);	
					}

					await UniTask.Defer(() => m_dimTween?.OnChangeValue(0f, Constant.Game.DEFAULT_DURATION_SECONDS) ?? UniTask.CompletedTask);
				}
				break;
			default:
				m_interactable = location is LocationType.STASH;
				if (m_iconAlphaTween.HasValue)
				{
					await m_iconAlphaTween.Value.OnChangeValue(Color.white, -1f);	
				}
				await UniTask.Defer(() => m_dimTween?.OnChangeValue(0f, 0f) ?? UniTask.CompletedTask);
				break;
		}
	}
}
