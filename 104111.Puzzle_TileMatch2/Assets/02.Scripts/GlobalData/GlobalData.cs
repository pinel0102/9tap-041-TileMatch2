using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using NineTap.Common;

public partial class GlobalData : SingletonMono<GlobalData>
{
    [HideInInspector] public MainScene mainScene = default;
    [HideInInspector] public PlayScene playScene = default;
    [HideInInspector] public MainSceneFragmentContent_Home fragmentHome = default;
    [HideInInspector] public MainSceneFragmentContent_Collection fragmentCollection = default;
    [HideInInspector] public MainSceneFragmentContent_Puzzle fragmentPuzzle = default;
    [HideInInspector] public MainSceneFragmentContent_Store fragmentStore = default;
    [HideInInspector] public MainSceneFragmentContent_Settings fragmentSettings = default;

    public UserManager userManager => Game.Inst?.Get<UserManager>();
    public SoundManager soundManager => Game.Inst?.Get<SoundManager>();
    public TableManager tableManager => Game.Inst?.Get<TableManager>();
    public HUD HUD => Game.Inst?.Get<HUD>();
    public bool isLevelEditor => PlayerPrefs.GetString(Constant.Editor.DEVELOP_MODE_SCENE_KEY, Constant.Scene.CLIENT) == Constant.Scene.EDITOR;

    public MainMenuType currentTab;
    public long oldCoin = 0;
    public int oldPuzzlePiece = 0;
    public int oldGoldPiece = 0;
    public int missionCollected = 0;
    
    private IObjectPool<MissionCollectedFx> m_particlePool;

    public void Initialize()
    {
        Debug.Log(CodeManager.GetMethodName());

        missionCollected = 0;
        oldCoin = 0;
        oldPuzzlePiece = 0;
        oldGoldPiece = 0;

        m_particlePool = new ObjectPool<MissionCollectedFx>(
			createFunc: () => {
				var item = Instantiate(ResourcePathAttribute.GetResource<MissionCollectedFx>());
				item.OnSetup();
				return item;
			},
			actionOnRelease: item => item.OnRelease()
		);
    }

    public void SetTouchLock_MainScene(bool active)
    {
        mainScene.m_block.SetActive(active);
    }

    public bool IsTouchLockNow()
    {
        return mainScene.m_block.activeInHierarchy;
    }

    public void SetOldItems(long _coin, int _puzzlePiece, int _goldPiece)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1} / {2}", _coin, _puzzlePiece, _goldPiece));

        oldCoin = _coin;
        oldPuzzlePiece = _puzzlePiece;
        oldGoldPiece = _goldPiece;
    }

    public int GetEnableLevel(int level)
    {
        if (tableManager != null)
            return Mathf.Clamp(level, 1, tableManager.LastLevel + 1);
        
        return Mathf.Min(1, level);
    }

    public int GetGoldPiece_NextLevel()
    {
        return 999;
    }
}
