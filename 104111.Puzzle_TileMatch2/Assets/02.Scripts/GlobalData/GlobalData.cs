using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public partial class GlobalData : SingletonMono<GlobalData>
{
    [Header("★ [Settings] Activated Events")]
    public List<GameEventType> activatedEvents = new List<GameEventType>()
    {
        GameEventType.SweetHolic
    };

    [HideInInspector] public MainScene mainScene = default;
    [HideInInspector] public PlayScene playScene = default;
    [HideInInspector] public StoreScene storeScene = default;
    [HideInInspector] public MainSceneFragmentContent_Home fragmentHome = default;
    [HideInInspector] public MainSceneFragmentContent_Collection fragmentCollection = default;
    [HideInInspector] public MainSceneFragmentContent_Puzzle fragmentPuzzle = default;
    [HideInInspector] public MainSceneFragmentContent_Settings fragmentSettings = default;
    [HideInInspector] public MainSceneFragmentContent_Store fragmentStore = default;
    [HideInInspector] public MainSceneFragmentContent_Store fragmentStore_popup = default;

    public UserManager userManager => Game.Inst?.Get<UserManager>();
    public SoundManager soundManager => Game.Inst?.Get<SoundManager>();
    public TableManager tableManager => Game.Inst?.Get<TableManager>();
    public HUD HUD => Game.Inst?.Get<HUD>();
    public bool isLevelEditor => PlayerPrefs.GetString(Constant.Editor.DEVELOP_MODE_SCENE_KEY, Constant.Scene.CLIENT) == Constant.Scene.EDITOR;
    public bool isAutoPopupPending;

    [Header("★ [Live] Old Items")]
    public long oldCoin = 0;
    public int oldPuzzlePiece = 0;
    //public int oldGoldPiece = 0;
    public int oldSweetHolicExp = 0;

    [Header("★ [Settings] Shuffle")]
    public float shuffleRadiusMin = 200;
    public float shuffleRadiusMax = 350;
    public float shuffleSpeed = 15;
    public float shuffleTime = 1;
    
    private WaitForSecondsRealtime wTimeDelay = new WaitForSecondsRealtime(1.0f);

    public void Initialize()
    {
        Debug.Log(CodeManager.GetMethodName());

        oldCoin = 0;
        oldPuzzlePiece = 0;
        oldSweetHolicExp = 0;

        eventSweetHolic_GetCount = 0;

        ResetParticlePool();

        StartCoroutine(Co_RealTime());
    }

    private IEnumerator Co_RealTime()
    {
        while(true)
        {
            if(GlobalDefine.IsUserLoaded)
            {
                userManager?.UpdateLog(totalPlayTime: userManager.Current.TotalPlayTime + 1);
                fragmentHome?.SideContainers.ForEach(item => { item.RefreshIcons(); });
                GlobalDefine.CheckEventExpired();
            }
            
            yield return wTimeDelay;
        }
    }

    public void CreateExpTable()
    {
        var eventDataTable = tableManager.EventDataTable;
        activatedEvents.ForEach(eventType => {
            int MinLevel = eventDataTable.GetMinLevel(eventType);
            int MaxLevel = eventDataTable.GetMaxLevel(eventType);
            List<int> ExpList = eventDataTable.GetExpList(eventType);

            switch(eventType)
            {
                case GameEventType.SweetHolic:  eventSweetHolic_ExpTable = new ExpTable(MinLevel, MaxLevel, ExpList); break;
            }
        });
    }

    public void SetTouchLock_MainScene(float activeTimeSeconds = 0.5f)
    {
        SetTouchLock_MainScene(true);
        UniTask.Void(
            async () => {
                await UniTask.Delay(TimeSpan.FromSeconds(activeTimeSeconds));
                SetTouchLock_MainScene(false);
            }
        );
    }

    public void SetTouchLock_PlayScene(float activeTimeSeconds = 0.5f)
    {
        SetTouchLock_PlayScene(true);
        UniTask.Void(
            async () => {
                await UniTask.Delay(TimeSpan.FromSeconds(activeTimeSeconds));
                SetTouchLock_PlayScene(false);
            }
        );
    }

    public void SetTouchLock_MainScene(bool active)
    {
        mainScene?.m_block.SetActive(active);
    }

    public void SetTouchLock_PlayScene(bool active)
    {
        playScene?.m_block.SetActive(active);
    }

    public bool IsTouchLockNow_MainScene()
    {
        return (mainScene != null) && mainScene.m_block.activeInHierarchy;
    }

    public bool IsTouchLockNow_PlayScene()
    {
        return (playScene != null) && playScene.m_block.activeInHierarchy;
    }

    public void SetOldItems(long _coin, int _puzzlePiece, int _sweetHolicExp)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", _coin, _puzzlePiece));

        oldCoin = _coin;
        oldPuzzlePiece = _puzzlePiece;
        oldSweetHolicExp = _sweetHolicExp;
    }

    public int GetEnableLevel(int level)
    {
        if (tableManager != null)
            return Mathf.Clamp(level, 1, tableManager.LastLevel + 1);
        
        return Mathf.Min(1, level);
    }
}
