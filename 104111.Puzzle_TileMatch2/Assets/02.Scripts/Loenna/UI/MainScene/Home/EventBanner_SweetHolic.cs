using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;
using TMPro;
using DG.Tweening;
using System;

public class EventBanner_SweetHolic : MonoBehaviour
{
    private GameEventType eventType = GameEventType.SweetHolic;

    [Header("★ [Reference] Sweet Holic")]
    public EventSlider eventSlider;
    public RectTransform targetItemPosition;

    [Header("★ [Reference] Reward Item")]
    public Canvas rewardCanvas;
    public CanvasGroup rewardCanvasGroup;
    public RectTransform rewardItemRect;
    public CanvasGroup rewardHalo;
    public RectTransform rewardIconRect;

    [Header("★ [Reference] Privates")]
    [SerializeField]	private GameObject sweetHolicLock;
    [SerializeField]	private GameObject sweetHolicUnlock;
    [SerializeField]	private TMP_Text m_lockedText;
    [SerializeField]	private Button m_bannerButton;
    [SerializeField]	private GameObject m_boosterTimeBadge;
    [SerializeField]	private Image m_boosterTimeImage;
    [SerializeField]	private TMP_Text m_boosterTimeText;
    
    [Header("★ [Live] Sweet Holic")]
    public bool isUnlocked;
    public string targetName;
    public int targetIndex;
    public int currentLevel;
    public int currentExp;
    public int requiredExp;
    public int totalExp;
    public string targetImagePath;
    public string boxImagePath;
    
    [Header("★ [Parameter] Privates")]
    private bool enableHalo;
    private CancellationTokenSource tokenSource = new CancellationTokenSource();
    private EventDataTable m_eventDataTable;
    private ExpTable m_expTable;
    private IUniTaskAsyncEnumerable<(bool, string)> BoosterStatus;
    private GlobalData globalData { get { return GlobalData.Instance; } }
    private const string textFormatLocked = "Unlock at Level {0}!";
    
    public void Initialize(User user, TableManager tableManager)
    {
        boxImagePath = GlobalDefine.BoxName_SweetHolic;

        m_eventDataTable = tableManager.EventDataTable;
        m_expTable = globalData.eventSweetHolic_ExpTable;
        RewardIconReset();

        m_lockedText.SetText(string.Format(textFormatLocked, Constant.User.MIN_OPENLEVEL_EVENT_SWEETHOLIC));
        eventSlider.Initialize(eventType, m_eventDataTable, m_expTable, boxImagePath);
        RefreshEventState(user);

        BoosterStatus = globalData.HUD.MessageBroker.Subscribe().Select(user => user.GetEventBoosterStatus(eventType));
        BoosterStatus.BindTo(m_boosterTimeText, (component, status) => {
            m_boosterTimeBadge.SetActive(status.Item1);
            component.text = status.Item2;
            //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Booster Time] {0}</color>", user.ExpiredSweetHolicBoosterAt));
        });

        m_bannerButton.onClick.RemoveAllListeners();
        m_bannerButton.onClick.AddListener(() => {
            if (GlobalDefine.IsOpen_Event_SweetHolic() && globalData.IsEnableShowPopup_MainScene())
            {
                globalData.soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
                globalData.SetTouchLock_MainScene();

                Debug.Log(CodeManager.GetMethodName() + "<color=yellow>Show Event Popup : Sweet Holic</color>");
                
                UIManager.ShowPopupUI<EventPopupSweetHolic>(
                    new EventPopupSweetHolicParameter(
                        Title: GlobalDefine.EventName_SweetHolic,
                        PopupCloseCallback: null,
                        EventBanner: this,
                        EventDataTable: m_eventDataTable,
                        ExpTable: m_expTable
                    )
                );
            }
        });
    }

    /// <summary>
    /// 자동 반복.
    /// </summary>
    /// <param name="user"></param>
    public void OnUpdateUI(User user)
    {
        eventSlider.RefreshRealTotalExp(user.Event_SweetHolic_TotalExp);
        eventSlider.RefreshTimeText(user.Event_SweetHolic_EndDate);
    }

    /// <summary>
    /// 1회 새로 고침.
    /// </summary>
    /// <param name="user"></param>
    public void Refresh(User user)
    {
        RefreshEventState(user);
    }

    private void RefreshEventState(User user)
    {
        isUnlocked = GlobalDefine.IsOpen_Event_SweetHolic();
        sweetHolicUnlock.SetActive(isUnlocked);
        sweetHolicLock.SetActive(!isUnlocked);

        RefreshBoosterIcon(GlobalDefine.GetSweetHolic_ItemImagePath());
        
        if (!isUnlocked)
            return;

        Debug.Log(CodeManager.GetMethodName());

        targetName = globalData.eventSweetHolic_TargetName;
        targetIndex = globalData.eventSweetHolic_TargetIndex;

        totalExp = user.Event_SweetHolic_TotalExp;
        (currentLevel, currentExp, requiredExp) = ExpManager.CalculateLevel(totalExp, m_expTable);

        eventSlider.RefreshEventState(targetImagePath, user.Event_SweetHolic_EndDate, currentLevel, currentExp, requiredExp, totalExp);
    }

    private void RefreshBoosterIcon(string newTargetImagePath)
    {
        if(string.IsNullOrEmpty(newTargetImagePath))
            return;
        
        targetImagePath = newTargetImagePath;
        m_boosterTimeImage.sprite = SpriteManager.GetSprite(targetImagePath);
    }

    public void RewardIconReset()
    {
        if (tokenSource.Token.CanBeCanceled)
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
        
        rewardCanvas.sortingOrder = 1;
        rewardCanvasGroup.alpha = 1;

        rewardItemRect.anchoredPosition = Vector2.zero;
        rewardIconRect.SetLocalScale(1f);

        rewardHalo.alpha = 0;
        rewardHalo.gameObject.SetActive(false);
        enableHalo = false;
    }

    public async UniTask RewardIconCenter(ChestType chestType, Action onComplete, float duration = 0.3f)
    {
        tokenSource = new CancellationTokenSource();

        rewardCanvas.sortingOrder = 2000;
        rewardCanvasGroup.alpha = 1;

        rewardItemRect.anchoredPosition = Vector2.zero;
        rewardIconRect.SetLocalScale(1f);

        rewardHalo.alpha = 0;
        rewardHalo.gameObject.SetActive(true);
        
        if (chestType == ChestType.Chest)
        {
            enableHalo = false;

            await rewardItemRect.DOMove(globalData.fragmentHome.CachedTransform.position, duration)
            .OnStart(() => {
                rewardIconRect.DOScale(2f, duration);
                rewardCanvasGroup.DOFade(0, duration);
            })
            .OnComplete(() => { 
                onComplete?.Invoke();
            })
            .ToUniTask(cancellationToken:tokenSource.Token);
        }
        else
        {
            enableHalo = true;

            await rewardItemRect.DOMove(globalData.fragmentHome.CachedTransform.position, duration)
            .OnStart(() => {
                rewardIconRect.DOScale(2f, duration);
            })
            .OnComplete(async () => { 
                onComplete?.Invoke();
                await PlayHalo();
            })
            .ToUniTask(cancellationToken:tokenSource.Token);
        }
    }

    private async UniTask PlayHalo(float delay = 0.2f)
	{
		await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken:tokenSource.Token);

		await rewardHalo
			.DOFade(1f, 0.25f)
			.ToUniTask(cancellationToken:tokenSource.Token)
			.SuppressCancellationThrow();

		UniTaskAsyncEnumerable
			.EveryUpdate(PlayerLoopTiming.LastPostLateUpdate) 
			.ForEachAsync(
				_ => {
					if (!enableHalo)
					{
						return;
					}

					ObjectUtility.GetRawObject(rewardHalo)?.transform.Rotate(Vector3.forward * 0.1f);
				},
                cancellationToken:tokenSource.Token
			).Forget();
	}
}
