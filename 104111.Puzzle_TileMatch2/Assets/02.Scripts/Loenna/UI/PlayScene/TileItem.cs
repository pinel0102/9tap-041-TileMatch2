//#define USE_GOLD_TILE_MISSION

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using NineTap.Common;
using TMPro;

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
		}

		public void Dispose()
		{
			m_tokenSource?.Dispose();
		}
	}

    [Header("★ [Live] Tile Info")]
    public int layerIndex;
    public int siblingIndex;
    public int basketIndex;
    public string tileName;
    public int tileIcon;
    public List<int> iconList = new List<int>();
    public BlockerType blockerType;
    public int blockerICD;
    public int oldICD;

    [Header("★ [Live] Status")]
    [SerializeField]	private bool m_interactable = false;
    [SerializeField]	private bool m_movable = true;
    [SerializeField]	private bool isMoving;
    [SerializeField]	private bool isScaling;
    public LocationType currentLocation;
    public bool IsInteractable => m_interactable;
    public bool IsMovable => m_movable;
    public bool IsMoving => isMoving;

    [Header("★ [Reference] Tile")]
	[SerializeField]	private RectTransform m_view;
	[SerializeField]	private Image m_icon;
	[SerializeField]	private EventTrigger m_trigger;
    public RectTransform m_triggerArea;

#if USE_GOLD_TILE_MISSION    
	[SerializeField]	private MissionPiece m_missionTile;
#endif

	[SerializeField]	private CanvasGroup m_dim;
    [SerializeField]	private Text m_tmpText; //이미지가 없을 경우 텍스트로
	[SerializeField]	private GameObject m_disappearEffect;

    [Header("★ [Reference] Blocker")]
    [SerializeField]	private RectTransform m_blockerRect;
    [SerializeField]	private Image m_blockerImage;
    [SerializeField]	private Image m_blockerImageSub;
    [SerializeField]	private TMP_Text m_blockerText;

	private TileItemModel m_current;
	public TileItemModel Current => m_current;
    private TileDataTable m_tileDataTable;
	private TweenContext? m_positionTween;
	private TweenContext? m_scaleTween;
	private TweenContext? m_dimTween;
	private TweenContext? m_iconAlphaTween;
	private Vector3 m_originWorldPosition;

    private bool ShuffleMove;
    private float _shuffleAngle;
    private float _shuffleRadius;
    private float _shuffleSpeed;
    private Transform _shuffleCenter;
    private Vector3 _myPosition;

    private float jumpDelay;

    private GlobalData globalData { get { return GlobalData.Instance; } }

    private void Update()
    {
        if (ShuffleMove)
            CircleMove();
    }

    public void ShuffleStart(float radiusMin, float radiusMax, float speed)
    {
        _myPosition = transform.position;
        _shuffleAngle = UnityEngine.Random.Range(0, 360);
        _shuffleRadius = UnityEngine.Random.Range(radiusMin, radiusMax);
        _shuffleSpeed = speed;
        ShuffleMove = true;
    }

    public void ShuffleStop()
    {
        ShuffleMove = false;

        UniTask.Void(
            async () =>
            {
                await transform
                    .DOMove(_myPosition, 0.25f)
                    .OnComplete(() => {
                        transform.position = _myPosition;
                    })
                    .ToUniTask()
                    .AttachExternalCancellation(this.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();
            }
        );
    }

    private void CircleMove()
    {    
        _shuffleAngle -= _shuffleSpeed * Time.deltaTime;
    
        var offset = new Vector3(Mathf.Sin(_shuffleAngle), Mathf.Cos(_shuffleAngle)) * _shuffleRadius;
        transform.position = _shuffleCenter.position + offset;
    }

	private void OnDestroy()
	{
		m_positionTween?.Dispose();
		m_scaleTween?.Dispose();
		m_iconAlphaTween?.Dispose();
		m_dimTween?.Dispose();
	}

	public void Release()
	{
        //Debug.Log(CodeManager.GetMethodName() + gameObject.name);

        currentLocation = LocationType.POOL;
        blockerICD = 0;
        isScaling = false;
        isMoving = false;
        m_disappearEffect.SetActive(false);
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
        blockerICD = 0;
        isScaling = false;
        isMoving = false;
		m_tileDataTable = Game.Inst.Get<TableManager>().TileDataTable;
		SoundManager soundManager = Game.Inst.Get<SoundManager>();

        m_positionTween = new TweenContext(
			tweener: ObjectUtility.GetRawObject(CachedTransform)?
                .DOMove(Vector2.zero, Constant.Game.TWEENTIME_TILE_DEFAULT)
                .OnComplete(() => {
                    isMoving = false;
                })
                .Pause()
				.SetAutoKill(false)
		);

		m_scaleTween = new TweenContext(
			tweener: ObjectUtility.GetRawObject(m_view)?
				.DOScale(Vector3.one, Constant.Game.TWEENTIME_TILE_DEFAULT)
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

		List<EventTrigger.Entry> entries = new List<EventTrigger.Entry> {
			new EventTrigger.Entry { eventID = EventTriggerType.PointerDown }
			//new EventTrigger.Entry { eventID = EventTriggerType.PointerUp }
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
			if (!m_interactable)
			{
				return UniTask.CompletedTask;
			}

            // Press 단계에서 바로 이동.
            if (type is EventTriggerType.PointerDown)
			{
				if (IsReallyMovable(m_movable))
                {   
                    soundManager?.PlayFx(Constant.Sound.SFX_TILE_SELECT);

                    m_interactable = false;
                    isMoving = true;
                    jumpDelay = 0;

                    if (IsNeedEffectBeforeMove(blockerType))
                    {
                        globalData.SetTouchLock_PlayScene(true);
                        
                        switch(blockerType)
                        {
                            case BlockerType.Glue_Left:
                                var(existRight, rightTile) = this.FindRightTile();
                                return WaitGlueAnimation(existRight, rightTile, () => {
                                    parameter.OnClick?.Invoke(this);
                                });
                            case BlockerType.Glue_Right:
                                var(existLeft, leftTile) = this.FindLeftTile();
                                return WaitGlueAnimation(existLeft, leftTile, () => {
                                    parameter.OnClick?.Invoke(this);
                                });
                            default:
                                SetScaleTween();
                                parameter.OnClick?.Invoke(this);
                                break;
                        }
                    }
                    else
                    {
                        SetScaleTween();
                        parameter.OnClick?.Invoke(this);
                    }
                }
            }

            return UniTask.CompletedTask;
		}
	}

#region Glue FX

    public async UniTask WaitGlueAnimation(bool existPair, TileItem pairTile, Action onFinished)
    {
        bool aniFinished = false;

        if (existPair)
        {
            float duration = 1f;

            RefreshBlockerState(blockerType, blockerICD);
            pairTile.RefreshBlockerState(pairTile.blockerType, pairTile.blockerICD);

            switch(blockerType)
            {
                case BlockerType.Glue_Left:
                    pairTile.m_blockerImage.gameObject.SetActive(false);
                    pairTile.PlayBlockerEffect(pairTile.blockerType, pairTile.blockerICD, pairTile.m_blockerRect.position);
                    await DoGlueMove(transform, pairTile.transform, duration, () => aniFinished = true);
                    break;
                case BlockerType.Glue_Right:
                    m_blockerImage.gameObject.SetActive(false);
                    PlayBlockerEffect(blockerType, blockerICD, m_blockerRect.position);
                    await DoGlueMove(pairTile.transform, transform, duration, () => aniFinished = true);
                    break;
                default:
                    aniFinished = true;
                    break;
            }
        }
        else
        {
            aniFinished = true;
        }

        await UniTask.WaitUntil(() => aniFinished);

        onFinished?.Invoke();
    }

    private UniTask DoGlueMove(Transform left, Transform right, float duration, Action onComplete = null)
    {
        Sequence seqGlue = DOTween.Sequence();

        if(globalData.glueArray)
        {
            var (leftPath, rightPath) = CreateGluePath(left.transform.localPosition, right.transform.localPosition);
            var (leftRotation, rightRotation) = CreateGlueRotation();

            float delay = duration / 8;

            for(int i = 0; i < 8; i++)
            {
                seqGlue
                    .Append(left.DOLocalMove(leftPath[i], delay))
                    .Join(left.DOLocalRotate(leftRotation[i], delay))
                    .Join(right.DOLocalMove(rightPath[i], delay))
                    .Join(right.DOLocalRotate(rightRotation[i], delay));
            }
        }
        else
        {
            var (leftPathLast, rightPathLast) = GluePathLast(left.transform.localPosition, right.transform.localPosition);
            var (leftRotationLast, rightRotationLast) = GlueRotationLast();

            seqGlue
                .Append(left.DOLocalMove(leftPathLast, duration))
                .Join(left.DOLocalRotate(leftRotationLast, duration))
                .Join(right.DOLocalMove(rightPathLast, duration))
                .Join(right.DOLocalRotate(rightRotationLast, duration));
        }

        return seqGlue
            .SetEase(globalData.glueEase)
            .SetAutoKill()
            .OnComplete(() => {
                left.DOLocalRotate(Vector3.zero, Constant.Game.TWEENTIME_TILE_DEFAULT)
                    .OnComplete(() => left.transform.localRotation = Quaternion.identity);
                right.DOLocalRotate(Vector3.zero, Constant.Game.TWEENTIME_TILE_DEFAULT)
                    .OnComplete(() => right.transform.localRotation = Quaternion.identity);
                onComplete?.Invoke();
            })
            .Play()
            .ToUniTask();

        (Vector3, Vector3) GluePathLast(Vector3 originLeft, Vector3 originRight)
        {
            return (originLeft + new Vector3(-50, -50, 0), originRight + new Vector3(50, -50, 0));
        }

        (Vector3, Vector3) GlueRotationLast()
        {
            return (new Vector3(0, 0, 30), new Vector3(0, 0, -30));
        }

        (Vector3[], Vector3[]) CreateGluePath(Vector3 originLeft, Vector3 originRight)
        {
            Vector3[] glueLeftPath = new Vector3[8] 
            {
                originLeft + new Vector3(0, 0, 0),      //0
                originLeft + new Vector3(-2, 0, 0),     //1
                originLeft + new Vector3(-4, 0, 0),     //2
                originLeft + new Vector3(-6, 0, 0),     //3
                originLeft + new Vector3(-20, -20, 0),  //4
                originLeft + new Vector3(-30, -30, 0),  //5
                originLeft + new Vector3(-40, -40, 0),  //6
                originLeft + new Vector3(-50, -50, 0),  //7
            };

            Vector3[] glueRightPath = new Vector3[8] 
            {
                originRight + new Vector3(0, 0, 0),     //0
                originRight + new Vector3(2, 0, 0),     //1
                originRight + new Vector3(4, 0, 0),     //2
                originRight + new Vector3(6, 0, 0),     //3
                originRight + new Vector3(20, -20, 0),  //4
                originRight + new Vector3(30, -30, 0),  //5
                originRight + new Vector3(40, -40, 0),  //6
                originRight + new Vector3(50, -50, 0),  //7
            };

            return (glueLeftPath, glueRightPath);
        }

        (Vector3[], Vector3[]) CreateGlueRotation()
        {
            Vector3[] glueLeftPath = new Vector3[8] 
            {
                new Vector3(0, 0, 0),   //0
                new Vector3(0, 0, 2),   //1
                new Vector3(0, 0, 4),   //2
                new Vector3(0, 0, 6),   //3
                new Vector3(0, 0, 16),  //4
                new Vector3(0, 0, 16),  //5
                new Vector3(0, 0, 30),  //6
                new Vector3(0, 0, 30),  //7
            };

            Vector3[] glueRightPath = new Vector3[8] 
            {
                new Vector3(0, 0, 0),   //0
                new Vector3(0, 0, -2),  //1
                new Vector3(0, 0, -4),  //2
                new Vector3(0, 0, -6),  //3
                new Vector3(0, 0, -16), //4
                new Vector3(0, 0, -16), //5
                new Vector3(0, 0, -25), //6
                new Vector3(0, 0, -30), //7
            };

            return (glueLeftPath, glueRightPath);
        }
    }

#endregion Glue FX

    public void SetScaleTween(bool fromTrigger = true)
    {
        isScaling = true;
        m_scaleTween?.OnChangeValue(Vector3.one * 1.3f, Constant.Game.TWEENTIME_TILE_SCALE, () => {
            if (isScaling)
                m_scaleTween?.OnChangeValue(Vector3.one, Constant.Game.TWEENTIME_TILE_SCALE, () => isScaling = false);
        });

        /*if(fromTrigger)
        {
            switch (blockerType)
            {
                case BlockerType.Glue_Left:
                    var (_, rightTile) = this.FindRightTile();
                    rightTile?.SetScaleTween(false);
                    break;
                case BlockerType.Glue_Right:
                    var (_, leftTile) = this.FindLeftTile();
                    leftTile?.SetScaleTween(false);
                    break;
            }
        }*/
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

        oldICD = blockerICD;

        layerIndex = item.LayerIndex;
        siblingIndex = item.SiblingIndex;
        currentLocation = changeLocation;
        blockerType = item.BlockerType;
        blockerICD = item.BlockerICD;//blockerICD > 0 ? blockerICD : item.BlockerICD;
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

    public void RefreshIcon(BlockerType type, int currentICD)
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

    public void RefreshBlockerState(BlockerType type, int currentICD)
    {
        if (currentLocation != LocationType.BOARD)
        {
            SetBlockerObject(Vector3.zero, null, activeMain:false);
            return;
        }

        switch(type)
        {
            case BlockerType.Glue_Right:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD));
                break;
            case BlockerType.Bush:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD), activeMain:currentICD > 0);
                break;
            case BlockerType.Suitcase:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD), GlobalDefine.GetBlockerSubSprite(type, currentICD), currentICD.ToString(), true, true, true);
                break;
            case BlockerType.Jelly:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD), activeMain:currentICD > 0);
                break;
            case BlockerType.Chain:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD), activeMain:currentICD > 0);
                break;
            case BlockerType.Glue_Left:
            case BlockerType.None:
            default:
                SetBlockerObject(GetBlockerRectPosition(type), null, activeMain:false);
                break;
        }

        CheckBlockerEffect(type, currentICD);
    }

    private Vector3 GetBlockerRectPosition(BlockerType type)
    {
        switch(type)
        {
            case BlockerType.Glue_Right:
                return -Constant.Game.TILE_WIDTH_HALF_POSITION;
        }

        return Vector3.zero;
    }

    private void CheckBlockerEffect(BlockerType type, int newICD)
    {
        bool isDecreased = newICD < oldICD;
        if(!isDecreased)
            return;
        
        switch(type)
        {
            case BlockerType.Bush:
                PlayBlockerEffect(type, newICD, m_blockerRect.position);
                break;
            case BlockerType.Jelly:
                PlayBlockerEffect(type, newICD, m_blockerRect.position);
                break;
            case BlockerType.Chain:
                PlayBlockerEffect(type, newICD, m_blockerRect.position);
                break;
            case BlockerType.Glue_Left:
            case BlockerType.Glue_Right:
            case BlockerType.Suitcase:
            case BlockerType.None:
            default:
                break;
        }
    }

    private bool IsNeedEffectBeforeMove(BlockerType type)
    {
        switch(type)
        {
            case BlockerType.Glue_Left:
            case BlockerType.Glue_Right:
                return true;
        }

        return false;
    }

    private void SetBlockerObject(Vector3 rectPosition, Sprite mainSprite, Sprite subSprite = null, string text = null, bool activeMain = true, bool activeSub = false, bool activeText = false)
    {
        m_blockerRect.localPosition = rectPosition;
        m_blockerImage.sprite = mainSprite;
        m_blockerImageSub.sprite = subSprite;
        m_blockerText.SetText(text);
        m_blockerImage.gameObject.SetActive(activeMain);
        m_blockerImageSub.gameObject.SetActive(activeSub);
        m_blockerText.gameObject.SetActive(activeText);
    }

    private void PlayBlockerEffect(BlockerType type, int newICD, Vector3 worldPosition)
    {
        globalData.playScene.LoadFX(type, newICD, worldPosition);
    }

    public UniTask OnChangeLocation(LocationType location, Vector2? moveAt = null, float duration = Constant.Game.TWEENTIME_TILE_DEFAULT)
	{
		if (!moveAt.HasValue && Current == null)
		{
			return UniTask.CompletedTask;
		}

        //duration = 1f; // Test
        
        //float currentDelay = jumpDelay;
        //jumpDelay = 0;

        currentLocation = location;
        Vector2 direction = moveAt ?? Current.Position;

        if (location == LocationType.POOL)
        {
            //Debug.Log(CodeManager.GetMethodName() + string.Format("[m_disappearEffect] {0}", CachedGameObject.name));
            isScaling = false;
            isMoving = false;
            jumpDelay = 0;
        }

        return (location, Current != null) switch {
			(LocationType.STASH or LocationType.BASKET, _) => 
                TileJump(location, direction, duration, jumpDelay, () => {
                    if (location == LocationType.BASKET)
                    {
                        globalData.playScene?.mainView?.CurrentBoard?.SortLayerTiles();
                    }
                }) ?? UniTask.CompletedTask,
			(LocationType.BOARD, true) => 
                m_positionTween?.OnChangeValue(m_originWorldPosition, duration, () => {
                    globalData.playScene?.mainView?.CurrentBoard?.SortLayerTiles();
                }) 
                ?? UniTask.CompletedTask,
			(LocationType.POOL, _) => m_scaleTween?.OnChangeValue(Vector3.zero, duration, duration, () => {
                    
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

                    m_disappearEffect.SetActive(true);
                    m_view.SetLocalScale(0);

                    UniTask.Void(
                        async () => {
                            await UniTask.Delay(TimeSpan.FromSeconds(Constant.Game.EFFECTTIME_TILE_MATCH));
                            m_disappearEffect.SetActive(false);
                        }
                    );
                }) ?? UniTask.CompletedTask,
			_ => UniTask.CompletedTask
		};
	}

    private UniTask? TileJump(LocationType location, Vector3 value, float duration, float delay, Action onComplete = null)
    {
        //Debug.Log(CodeManager.GetMethodName() + string.Format("m_movable: {0}", m_movable));
        //Debug.Log(location);

        jumpDelay = 0;

        return ObjectUtility.GetRawObject(CachedTransform)?
            .DOJump(value, 0.5f, 1, duration)
            .SetDelay(jumpDelay)
            .OnComplete(() => {
                isMoving = false;
                onComplete?.Invoke();
            })
            .AsyncWaitForCompletion()
            .AsUniTask();
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
			default:
				m_interactable = location is LocationType.STASH;
                m_movable = true;
				if (m_iconAlphaTween.HasValue)
				{
					await m_iconAlphaTween.Value.OnChangeValue(Color.white, -1f);	
				}
                await UniTask.Defer(() => m_dimTween?.OnChangeValue(0f, 0f) ?? UniTask.CompletedTask);
				break;
		}
	}

#region Blocker Check

    /// <summary>
    /// 자신의 이동 가능 여부를 체크.
    /// </summary>
    public void MovableCheck()
    {
        bool oldValue = m_movable;

        if (m_interactable)
        {
            m_movable = BlockerMoveCheck(blockerICD);

            // 가려진 Blocker에 상관 없이 인접 타일 이동 가능하게 변경.
            /*if (GetHiddenAroundBlockers().Count > 0)
            {
                m_movable = false;
            }
            else
            {
                m_movable = BlockerMoveCheck(blockerICD);
            }*/
        }
        else
        {
            m_movable = false;
        }

        if (oldValue != m_movable)
        {
            //Debug.Log(CodeManager.GetMethodName() + string.Format("Movable Changed : {0} ({1})", m_movable, tileName));
            
            if (currentLocation == LocationType.BOARD)
            {
                //if (blockerType == BlockerType.None)
                m_dimTween?.OnChangeValue(IsReallyMovable(m_movable) ? 0 : 1f, 0f);
            }
            
            AroundMovableCheck();
        }
    }

    public void SetDim(float value)
    {
        m_dimTween?.OnChangeValue(value, 0f);
    }

    /// <summary>
    /// 자신이 Blocker인 경우 인접 타일의 이동 가능 여부를 체크.
    /// </summary>
    private void AroundMovableCheck()
    {
        switch(blockerType)
        {
            // 가려진 Blocker에 상관 없이 인접 타일 이동 가능하게 변경.
            /*case BlockerType.Bush:
                if(m_interactable)
                {
                    this.FindAroundTiles().ForEach(tile => {
                        tile.MovableCheck();
                    });
                }
                break;
            case BlockerType.Chain:
                if(m_interactable)
                {
                    this.FindLeftRightTiles().ForEach(tile => {
                        tile.MovableCheck();
                    });
                }
                break;*/
            case BlockerType.Glue_Left:
                var (_, rightTile) = this.FindRightTile();
                rightTile?.SetDim(IsReallyMovable(m_movable) ? 0 : 1f);
                break;
            case BlockerType.Glue_Right:
                var (_, leftTile) = this.FindLeftTile();
                leftTile?.SetDim(IsReallyMovable(m_movable) ? 0 : 1f);
                break;
        }
    }

    /// <summary>
    /// 자신이 Blocker인 경우 본인의 이동 가능 여부를 체크.
    /// </summary>
    private bool BlockerMoveCheck(int currentICD)
    {
        switch(blockerType)
        {
            case BlockerType.Glue_Left:
                var(existRight, rightTile) = this.FindRightTile();
                return existRight && rightTile.IsInteractable;
            case BlockerType.Glue_Right:
                var(existLeft, leftTile) = this.FindLeftTile();
                return existLeft && leftTile.IsInteractable;
            case BlockerType.Bush:
            case BlockerType.Chain:
            case BlockerType.Jelly:
            case BlockerType.Suitcase:
                return currentICD <= 0;
        }
        return true;
    }

    /// <summary>
    /// 실제 이동 가능 여부를 체크.
    /// </summary>
    /// <param name="tileMoveable"></param>
    /// <returns></returns>
    private bool IsReallyMovable(bool tileMoveable)
    {
        switch(currentLocation, blockerType)
        {
            case (LocationType.BOARD, BlockerType.Glue_Left):
                var(existRight, rightTile) = this.FindRightTile();
                return tileMoveable && existRight && rightTile.IsInteractable && rightTile.IsMovable;
            case (LocationType.BOARD, BlockerType.Glue_Right):
                var(existLeft, leftTile) = this.FindLeftTile();
                return tileMoveable && existLeft && leftTile.IsInteractable && leftTile.IsMovable;
        }

        return tileMoveable;
    }

    /*private bool BasketHasSpace(int count)
    {
        return globalData.playScene.gameManager.BasketRemainCount.Value >= count;
    }*/

    /// <summary>
    /// 인접 타일에 영향을 주는 숨겨진 Blocker 존재 여부.
    /// </summary>
    /// <returns>Bush / Chain</returns>
    private List<TileItem> GetHiddenAroundBlockers()
    {
        List<TileItem> result = new List<TileItem>();

        var topBottomTiles = this.FindTopBottomTiles();
        for(int i=0; i < topBottomTiles.Count; i++)
        {
            var targetTile = topBottomTiles[i];
            if (targetTile.blockerType is BlockerType.Bush)
            {
                if (!targetTile.IsInteractable)
                {
                    //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} ({1} : {2})", targetTile.blockerType, targetTile.tileName, targetTile.Current.Position));
                    result.Add(targetTile);
                }
            }
        }

        var leftRightTiles = this.FindLeftRightTiles();
        for(int i=0; i < leftRightTiles.Count; i++)
        {
            var targetTile = leftRightTiles[i];
            if (targetTile.blockerType is BlockerType.Bush or BlockerType.Chain)
            {
                if (!targetTile.IsInteractable)
                {
                    //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} ({1} : {2})", targetTile.blockerType, targetTile.tileName, targetTile.Current.Position));
                    result.Add(targetTile);
                }
            }
        }

        return result;
    }

    

#endregion Blocker Check

}
