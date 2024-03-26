//#define USE_GOLD_TILE_MISSION

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using NineTap.Common;
using System.Linq;

public class TileItemParameter
{
    public Action<TileItem> OnClick;
}

[ResourcePath("UI/Widgets/TileObject")]
public partial class TileItem : CachedBehaviour
{
    [Header("★ [Live] Status")]
    [SerializeField]	private LocationType currentLocation;
    [SerializeField]	private bool m_interactable = false;
    [SerializeField]	private bool m_movable = false;
    [SerializeField]	private bool isMoving;
    public bool IsInteractable => m_interactable;
    public bool IsMovable => m_movable;
    public bool IsMoving => isMoving;

    [Header("★ [Live] Tile Info")]
    [SerializeField]    private Transform _parentLayer;
    public int layerIndex;
    public int siblingIndex;
    public int basketIndex;
    public string tileName;
    public int tileIcon;
    public List<int> iconList = new List<int>();
    public BlockerType blockerType;
    public int blockerICD;
    public int oldICD;
    
    [Header("★ [Reference] TileItem")]
	[SerializeField]	private RectTransform m_view;
	[SerializeField]	private Image m_icon;
	[SerializeField]	private EventTrigger m_trigger;
    public RectTransform m_triggerArea;

#if USE_GOLD_TILE_MISSION    
	[SerializeField]	private MissionPiece m_missionTile;
#endif
	[SerializeField]	private CanvasGroup m_dim;
    [SerializeField]	private Text m_tmpText; //이미지가 없을 경우 텍스트로
	
	private TileItemModel m_current;
	public TileItemModel Current => m_current;
    private TileDataTable m_tileDataTable;	
	private Vector3 m_originWorldPosition;
    private GlobalData globalData { get { return GlobalData.Instance; } }

#region Initialize

	public void OnSetup(TileItemParameter parameter)
	{
        blockerICD = 0;
        isMoving = false;
        m_tileDataTable = Game.Inst.Get<TableManager>().TileDataTable;
		SoundManager soundManager = Game.Inst.Get<SoundManager>();

        m_positionTween = new TweenContext(
			tweener: ObjectUtility.GetRawObject(CachedTransform)?
                .DOMove(Vector2.zero, Constant.Game.TWEENTIME_TILE_DEFAULT)
                .OnPlay(() => {
                    SetMoving(true);
                })
                .OnComplete(() => {
                    SetMoving(false);
                    if (Current.Location == LocationType.BOARD)
                        globalData.playScene?.mainView?.CurrentBoard?.SortLayerTiles();
                })
                .Pause()
				.SetAutoKill(false)
		);

		m_scaleTween = new TweenContext(
			tweener: ObjectUtility.GetRawObject(m_view)?
				.DOScale(Vector3.one, Constant.Game.TWEENTIME_TILE_DEFAULT)
                .OnComplete(() => {
                    if(Current.Location == LocationType.POOL)
                    {
                        // [Event] Sweet Holic
                        if (GlobalDefine.IsOpen_Event_SweetHolic())
                        {
                            // 매칭된 타일이 수집 이벤트 대상이면.
                            if(Current.Icon.Equals(globalData.eventSweetHolic_TargetIndex))
                            {
                                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Matching [{0}] {1}</color>", Current.Icon, tileName));
                                globalData.playScene.gameManager.EventCollect_SweetHolic(transform);
                            }
                        }

                        globalData.playScene.LoadFX(GlobalDefine.FX_Prefab_Sparkle, CachedTransform.position);

                        m_view.SetLocalScale(0);
                        Reset();
                    }
                })
                .SetAutoKill(false)
		);

		m_dimTween = new TweenContext(
			tweener: ObjectUtility.GetRawObject(m_dim)?
				.DOFade(0f, Constant.Game.TWEENTIME_TILE_DEFAULT)
				.SetAutoKill(false)
		);

		m_iconAlphaTween = new TweenContext(
			tweener: ObjectUtility.GetRawObject(m_icon)?
				.DOFade(1f, -1f)
				.SetAutoKill(false)
		);

        m_jumpSequence = null;

		List<EventTrigger.Entry> entries = new List<EventTrigger.Entry> {
			new EventTrigger.Entry { eventID = EventTriggerType.PointerDown }
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

#if USE_GOLD_TILE_MISSION  
		ObjectUtility.GetRawObject(m_missionTile)?.SetVisible(false);
#endif

        _shuffleCenter = globalData.playScene.mainView.transform;
        
        UniTask OnTriggerCallback(EventTriggerType type, BaseEventData eventData)
		{
			if (!m_interactable || isMoving)
			{
				return UniTask.CompletedTask;
			}

            // Press 단계에서 바로 이동.
            if (type is EventTriggerType.PointerDown)
			{
				if (IsReallyMovable(m_movable))
                {   
                    soundManager?.PlayFx(Constant.Sound.SFX_TILE_SELECT);

                    //SetMoving(true);
                    SetDim(0);

                    if (IsNeedEffectBeforeMove(blockerType))
                    {
                        globalData.SetTouchLock_PlayScene(true);
                        
                        switch(blockerType)
                        {
                            case BlockerType.Glue_Left:
                                if (globalData.playScene.bottomView.BasketView.IsBasketEnable())
                                {
                                    var(existRight, rightTile) = this.FindRightTile();
                                    return WaitGlueAnimation(existRight, rightTile, () => {
                                        parameter.OnClick?.Invoke(this);
                                    });
                                }
                                else
                                {
                                    globalData.SetTouchLock_PlayScene(false);
                                }
                                break;
                            case BlockerType.Glue_Right:
                                if(globalData.playScene.bottomView.BasketView.IsBasketEnable())
                                {
                                    var(existLeft, leftTile) = this.FindLeftTile();
                                    return WaitGlueAnimation(existLeft, leftTile, () => {
                                        parameter.OnClick?.Invoke(this);
                                    });
                                }
                                else
                                {
                                    globalData.SetTouchLock_PlayScene(false);
                                }
                                break;
                            default:
                                //SetScaleTween();
                                parameter.OnClick?.Invoke(this);
                                break;
                        }
                    }
                    else
                    {
                        //SetScaleTween();
                        parameter.OnClick?.Invoke(this);
                    }
                }
            }

            return UniTask.CompletedTask;
		}
	}

#endregion Initialize


#region OnUpdateUI

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

        oldICD = blockerICD;

        layerIndex = item.LayerIndex;
        siblingIndex = item.SiblingIndex;
        currentLocation = changeLocation;
        blockerType = item.BlockerType;
        blockerICD = item.BlockerICD;
        iconList = item.IconList;
        
        RefreshIcon(blockerType, blockerICD);
        RefreshBlockerState(blockerType, blockerICD);
        SetInteractable(item.Location, item.Overlapped, item.InvisibleIcon, ignoreInvisible).Forget();

        (m_tmpText.text, m_icon.enabled) = m_icon.sprite switch {
			null => (tileName, false),
			_ => (string.Empty, true)
		};

#if USE_GOLD_TILE_MISSION
		m_missionTile.OnUpdateUI(sprite, item.Location is LocationType.BOARD? item.GoldPuzzleCount : -1);
#endif

        gameObject.name = string.Format("[{0}][{1}]{2}", layerIndex, siblingIndex, tileName);
        
        ShuffleMove = false;
        //Debug.Log(CodeManager.GetMethodName() + gameObject.name);

		return currentType != item.Location;
	}

    private void RefreshIcon(BlockerType type, int currentICD)
    {
        int index;
        switch(type)
        {
            case BlockerType.Suitcase:
                index = Mathf.Max(0, currentICD - 1);
                break;
            
            default:
                index = Mathf.Max(0, iconList.Count - 1);
                break;
        }

        if(iconList.Count > index)
        {
            tileIcon = iconList[index];

            string path = m_tileDataTable.TryGetValue(tileIcon, out var rowData)? rowData.Path : string.Empty;
            Sprite sprite = SpriteManager.GetSprite(path);
            m_icon.sprite = sprite;

            tileName = path.Replace("UI_Img_", string.Empty);
        }
    }

    private async UniTask SetInteractable(LocationType location, bool overlapped, bool invisibleIcon = false, bool ignoreInvisible = false)
	{
		switch (location)
		{
			case LocationType.BOARD:
				m_interactable = !overlapped;

#if USE_GOLD_TILE_MISSION
				ObjectUtility.GetRawObject(m_missionTile)?.SetVisible(!invisibleIcon);
#endif

				if (overlapped || !m_movable)
				{
                    Color color = Color.white.WithA(invisibleIcon && !ignoreInvisible? 0f : 1f);
					if (m_iconAlphaTween.HasValue)
					{
						await m_iconAlphaTween.Value.OnChangeValue(color, -1f);
					}

                    //if (blockerType == BlockerType.None)
					await UniTask.Defer(() => m_dimTween?.OnChangeValue(IsReallyMovable(m_movable) ? 0 : 1f, 0f) ?? UniTask.CompletedTask);
				}
				else
				{
					if (!invisibleIcon && m_iconAlphaTween.HasValue)
					{
						await m_iconAlphaTween.Value.OnChangeValue(Color.white, -1f);	
					}

					await UniTask.Defer(() => m_dimTween?.OnChangeValue(IsReallyMovable(m_movable) ? 0 : 1f, Constant.Game.TWEENTIME_TILE_DEFAULT) ?? UniTask.CompletedTask);
				}

                MovableCheck();

				break;

            case LocationType.STASH:
                m_interactable = globalData.playScene.bottomView.StashView.TopTiles.Contains(transform);
                m_movable = true;
                if (m_iconAlphaTween.HasValue)
				{
					await m_iconAlphaTween.Value.OnChangeValue(Color.white, -1f);	
				}
                await UniTask.Defer(() => m_dimTween?.OnChangeValue(0f, 0f) ?? UniTask.CompletedTask);
                break;

			default:
				m_interactable = false;
                m_movable = true;
				if (m_iconAlphaTween.HasValue)
				{
					await m_iconAlphaTween.Value.OnChangeValue(Color.white, -1f);	
				}
                await UniTask.Defer(() => m_dimTween?.OnChangeValue(0f, 0f) ?? UniTask.CompletedTask);
				break;
		}
	}

#endregion OnUpdateUI


#region OnChangeLocation

    public UniTask OnChangeLocation(LocationType location, Vector2? moveAt = null, float duration = Constant.Game.TWEENTIME_TILE_DEFAULT, Action onComplete = null)
	{
		if (!moveAt.HasValue && Current == null)
		{
            Reset();
			return UniTask.CompletedTask;
		}

        //duration = 0.5f;

        currentLocation = location;
        Vector2 direction = moveAt ?? Current.Position;

        if (location == LocationType.POOL || Current == null)
        {
            Reset();
        }

        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} -> {1}", oldLocation, location));
        //Debug.Log(CodeManager.GetMethodName() + string.Format("direction : {0}", direction));

        return (location, Current != null) switch {
			(LocationType.BASKET, _) => // Board or Stash -> Basket || // Basket -> Basket (After Matching Move)
                TileJump(location, direction, duration, onComplete: () => {
                    globalData.playScene?.mainView?.CurrentBoard?.SortLayerTiles();
                }) ?? UniTask.CompletedTask,
            (LocationType.STASH, _) => // Basket -> Stash (Item: Return)
                TileJump(location, direction, duration, onComplete: onComplete) ?? UniTask.CompletedTask, 
			(LocationType.BOARD, true) => // Basket -> Board (Item: Undo)
                TileJump(location, m_originWorldPosition, duration, onComplete: () => {
                    globalData.playScene?.mainView?.CurrentBoard?.SetParentLayer(this, layerIndex);
                }) ?? UniTask.CompletedTask,
			(LocationType.POOL, _) => // Mathing Disappear
                m_scaleTween?.OnChangeValue(Vector3.zero, duration, duration) ?? UniTask.CompletedTask,
			_ => DoNothing(location, Current != null)
		};
	}

    private void StopJump()
    {
        if (m_jumpSequence?.IsActive() ?? false)
        {
            //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=white>[{0}] Stop : {1}</color>", gameObject.name, blockerType));
            m_jumpSequence?.Kill();
        }
    }

    private UniTask? TileJump(LocationType location, Vector3 value, float duration, float jumpPower = 0.5f, Action onComplete = null)
    {
        StopJump();

        m_jumpSequence = DOTween.Sequence();
        m_jumpSequence.Append(ObjectUtility.GetRawObject(CachedTransform)?
            .DOJump(value, jumpPower, 1, duration)
            .OnPlay(() => {
                //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=white>[{0}] Play : {1}</color>", gameObject.name, blockerType));
                SetMoving(true);
            })
            .OnComplete(() => {
                //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=white>[{0}] Complete : {1}</color>", gameObject.name, blockerType));
                SetMoving(false);
                onComplete?.Invoke();
            })
            .OnKill(() => m_jumpSequence = null)
            .SetAutoKill()
        );

        return m_jumpSequence
            .Play()
            .AsyncWaitForCompletion()
            .AsUniTask();
    }

    private UniTask DoNothing(LocationType location, bool existModel)
    {
        Debug.LogWarning(CodeManager.GetMethodName() + string.Format("<color=white>location: {0} / existModel: {1}</color>", location, existModel));

        Reset();

        return UniTask.CompletedTask;
    }

#endregion OnChangeLocation


#region ETC

    public void SetActive(bool enabled)
	{
        m_view.SetLocalScale(Vector2.one);
		CachedGameObject.SetActive(enabled);
	}

    public void InitParentLayer()
    {
        _parentLayer = CachedRectTransform.parent;
    }

    private void SetBasketParent()
    {
        CachedRectTransform.SetParent(globalData.playScene.bottomView.BasketView.BasketParent, true);
    }

    private void RollBackParent()
    {
        CachedRectTransform.SetParent(_parentLayer, true);
    }

    private void SetDim(float value)
    {
        m_dimTween?.OnChangeValue(value, 0f);
    }

    private void SetMoving(bool value)
    {
        isMoving = value;
    }

    /// <summary>상태 리셋.</summary>
    public void Reset()
    {
        SetMoving(false);
    }

    /// <summary>완전히 삭제.</summary>
	public void Release()
	{
        //Debug.Log(CodeManager.GetMethodName() + gameObject.name);
        
        Reset();

        m_scaleTween?.OnChangeValue(Vector3.one, -1f).Forget();
		m_iconAlphaTween?.OnChangeValue(Color.white, -1f).Forget();
        m_dimTween?.OnChangeValue(0, -1f).Forget();
        m_jumpSequence?.Kill();

        m_view.SetLocalScale(Vector2.one);
        m_icon.color = Color.white;
        m_dim.alpha	= 0f;

        currentLocation = LocationType.POOL;
        blockerICD = 0;
        m_interactable = false;
        m_movable = false;
        m_current = null;
        
        CachedGameObject.SetActive(false);
	}

#endregion ETC


#region Behaviour

	private void OnDestroy()
	{
		m_positionTween?.Dispose();
		m_scaleTween?.Dispose();
		m_iconAlphaTween?.Dispose();
		m_dimTween?.Dispose();
        m_jumpSequence?.Kill();
	}

#endregion Behaviour

}
