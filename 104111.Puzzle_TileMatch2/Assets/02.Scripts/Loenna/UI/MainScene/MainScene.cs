using UnityEngine;
using System.Collections.Generic;
using NineTap.Common;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Rendering;

public record MainSceneParameter(
    MainMenuType ShowMenuType = MainMenuType.HOME, 
    PuzzleManager PuzzleManager = null
    ) : DefaultParameter();

public record MainSceneRewardParameter(
    int clearedLevel = 0,
    int openPuzzleIndex = 0,
    int rewardCoin = 0,
    int rewardPuzzlePiece = 0,
    int rewardSweetHolic = 0
    ) : MainSceneParameter();

[ResourcePath("UI/Scene/MainScene")]
public class MainScene : UIScene
{
    [SerializeField]
	private MainSceneScrollView m_scrollView;

	[SerializeField]
	private MainSceneNavigationView m_navigationView;

    private LobbyManager m_lobbyManager;
	private	UserManager m_userManager;
    private ItemDataTable m_itemDataTable;
	private ProductDataTable m_productDataTable;

    public LobbyManager lobbyManager => m_lobbyManager;
    public MainSceneScrollView scrollView => m_scrollView;
    public GameObject m_block;
    public GameObject m_purchasing;

    private GlobalData globalData { get { return GlobalData.Instance; } }

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not MainSceneParameter parameter)
		{
			return;
		}

        globalData.mainScene = this;
        globalData.fragmentHome = m_scrollView.Contents[(int)MainMenuType.HOME] as MainSceneFragmentContent_Home;
        globalData.fragmentCollection = m_scrollView.Contents[(int)MainMenuType.COLLECTION] as MainSceneFragmentContent_Collection;
        globalData.fragmentPuzzle = m_scrollView.Contents[(int)MainMenuType.JIGSAW_PUZZLE] as MainSceneFragmentContent_Puzzle;
        globalData.fragmentStore = m_scrollView.Contents[(int)MainMenuType.STORE] as MainSceneFragmentContent_Store;
        globalData.fragmentSettings = m_scrollView.Contents[(int)MainMenuType.SETTINGS] as MainSceneFragmentContent_Settings;

        m_userManager = Game.Inst.Get<UserManager>();
		m_lobbyManager = new LobbyManager(
			m_userManager,
			tableManager: Game.Inst.Get<TableManager>()
		);

        TableManager tableManager = Game.Inst.Get<TableManager>();
        m_itemDataTable = tableManager.ItemDataTable;
        m_productDataTable = tableManager.ProductDataTable;

        SoundManager soundManager = Game.Inst?.Get<SoundManager>();
        HUD m_hud = Game.Inst.Get<HUD>();
        m_hud.behaviour.AddListener(
            new KeyValuePair<HUDType, System.Action>(
                HUDType.STAR, () => { 
                    if (IsEnableShowPopup())
                    {
                        soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
                        m_lobbyManager.OnCheckShowPopup(() => globalData.ShowBuyHeartPopup());
                    } 
                }
            ),
            new KeyValuePair<HUDType, System.Action>(
                HUDType.LIFE, () => { 
                    if (IsEnableShowPopup() && !m_userManager.Current.IsBoosterTime()
                                            && !m_userManager.Current.IsFullLife())
                    {
                        soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
                        globalData.ShowBuyHeartPopup();
                    }
                }
            ),
            new KeyValuePair<HUDType, System.Action>(
                HUDType.COIN, () => { 
                    if (IsEnableShowPopup()) 
                    {
                        soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
                        SDKManager.SendAnalytics_C_Scene(NineTap.Constant.Text.Button.STORE);
                        m_scrollView.MoveTo((int)MainMenuType.STORE); 
                    }
                }
            )
        );

		m_scrollView.OnSetup(
			new MainSceneScrollViewParameter(
				OnSelectMenu: m_navigationView.SetIsOnWithoutNotify,
				FragmentParams: new List<ScrollViewFragmentParameter> {
					new ScrollViewFragmentParameter(
						MainMenuType.COLLECTION,
						typeof(MainSceneFragmentContent_Collection),
						new MainSceneFragmentContentParameter_Collection{
							MoveToPuzzle = (puzzleData, placedPieces, unlockedPieces) => {
                                m_lobbyManager.OnSelectPuzzle(puzzleData, placedPieces, unlockedPieces);
								m_scrollView.MoveTo((int)MainMenuType.JIGSAW_PUZZLE);
							}
						}
					),
					new ScrollViewFragmentParameter(
						MainMenuType.JIGSAW_PUZZLE,
						typeof(MainSceneFragmentContent_Puzzle),
						new MainSceneFragmentContentParameter_Puzzle{
							PuzzleManager = parameter.PuzzleManager,
							OnUpdated = m_lobbyManager.OnUpdatePuzzle,
							OnClick = () => m_lobbyManager.OnCheckShowPopup(() => globalData.ShowBuyHeartPopup())
						}
					),
					new ScrollViewFragmentParameter(
						MainMenuType.HOME,
						typeof(MainSceneFragmentContent_Home),
						new MainSceneFragmentContentParameter_Home{
							PuzzleButtonParam = new HomeFragmentLargeButtonParameter{
								ButtonText = string.Empty,
								OnClick = () => {
                                    globalData.MoveToLatestPuzzle();
                                    m_scrollView.MoveTo((int)MainMenuType.JIGSAW_PUZZLE);
                                },
								GaugeBarParameter = new UIProgressBarParameter {
									Type = UIProgressBar.Type.STATIC,
									VisibleText = true
								}
							},
							PlayButtonParam = new UITextButtonParameter{
								ButtonText = string.Empty,
								OnClick = () => m_lobbyManager.OnCheckShowPopup(() => globalData.ShowBuyHeartPopup()),
								ButtonTextBinder = m_lobbyManager.CurrentLevel
							}
						}
					),
					new ScrollViewFragmentParameter(
						MainMenuType.STORE,
						typeof(MainSceneFragmentContent_Store),
						new MainSceneFragmentContentParameter_Store{
							TitleText = "Store",
							CloseButtonParameter = new UIImageButtonParameter {
								OnClick = () => m_scrollView.MoveTo((int)MainMenuType.HOME)
							}
						}
					),
					new ScrollViewFragmentParameter(
						MainMenuType.SETTINGS,
						typeof(MainSceneFragmentContent_Settings),
						new MainSceneFragmentContentParameter_Settings{
							TitleText = "Settings",
							CloseButtonParameter = new UIImageButtonParameter{
								OnClick = () => m_scrollView.MoveTo((int)MainMenuType.HOME)
							},
							ToggleButtonParameters = new List<UIToggleButtonParameter> {
								new UIToggleButtonParameter{
									Text = SettingsType.Fx.GetName(),
									AwakeOn = m_userManager.Current.Settings.GetValueOrDefault(SettingsType.Fx, true),
									IconBuilder = isOn => SettingsType.Fx.GetIconPath(isOn),
									OnToggle = value => m_userManager.UpdateSettings(SettingsType.Fx, value),
								},
								new UIToggleButtonParameter{
									Text = SettingsType.Bgm.GetName(),
									AwakeOn = m_userManager.Current.Settings.GetValueOrDefault(SettingsType.Bgm, true),
									IconBuilder = isOn => SettingsType.Bgm.GetIconPath(isOn),
									OnToggle = value => m_userManager.UpdateSettings(SettingsType.Bgm, value),
								},
								new UIToggleButtonParameter{
									Text = SettingsType.Vibration.GetName(),
									AwakeOn = m_userManager.Current.Settings.GetValueOrDefault(SettingsType.Vibration, true),
									IconBuilder = isOn => SettingsType.Vibration.GetIconPath(isOn),
									OnToggle = value => m_userManager.UpdateSettings(SettingsType.Vibration, value),
								},
								new UIToggleButtonParameter{
									Text = SettingsType.Notification.GetName(),
									AwakeOn = m_userManager.Current.Settings.GetValueOrDefault(SettingsType.Notification, true),
									IconBuilder = isOn => SettingsType.Notification.GetIconPath(isOn),
									OnToggle = value => {
                                        m_userManager.UpdateSettings(SettingsType.Notification, value);
                                        PushManager.PushAgree(value);
                                    },
								}
							},
							ServiceButtonParameter = new UITextButtonParameter{
								ButtonText = "Terms of Service",
								OnClick = GlobalSettings.OpenURL_TermsOfService
							},
							PrivacyButtonParameter = new UITextButtonParameter{
								ButtonText = "Privacy Policy",
								OnClick = GlobalSettings.OpenURL_PrivacyPolicy
							},
							ContactButtonParameter = new UITextButtonParameter{
								ButtonText = "Contact",
								OnClick = GlobalSettings.OpenMail_Contacts
							}
						}
					)
				}
			)
		);

		m_navigationView.OnSetup(
			new MainSceneNavigationViewParameter{
				OnClickTab = type => {  soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
                                        if (type == MainMenuType.COLLECTION)
                                            globalData.fragmentCollection.RefreshLockState();
                                        else if (type == MainMenuType.JIGSAW_PUZZLE)
                                        {
                                            if (globalData.CURRENT_SCENE != GlobalDefine.SCENE_PUZZLE)
                                                globalData.MoveToLatestPuzzle();
                                        }

                                        m_scrollView.MoveTo((int)type);}
			}
		);

		m_userManager.OnUpdated += OnUpdateUI;

        SDKManager.Instance.SDKStart();
        PushManager.PushAgree(m_userManager.Current.Settings.GetValueOrDefault(SettingsType.Notification, true));
	}

    public void MoveTo(MainMenuType type)
    {
        m_scrollView.MoveTo((int)type);
    }

    public override void Hide()
    {
        base.Hide();

        globalData.ResetParticlePool();
    }

    public override void Show()
	{
		base.Show();

        OnUpdateUI(m_userManager.Current);

        if (CachedParameter is not MainSceneParameter parameter)
		{
			return;
		}

        globalData.CURRENT_LEVEL = m_userManager.Current.Level;
        globalData.CURRENT_DIFFICULTY = 0;
        globalData.eventSweetHolic_IsBoosterTime = false;
        
        m_scrollView.MoveTo((int)parameter.ShowMenuType);

        SDKManager.SendAnalytics_I_Scene();
        GlobalDefine.RequestAD_HideBanner();

        CheckAutoPopups();
	}
    
    private async void CheckAutoPopups()
    {
        GlobalDefine.CheckEventActivate();

        globalData.SetTouchLock_MainScene(true);
        globalData.isAutoPopupPending = true;
        
        globalData.fragmentHome?.SideContainers.ForEach(item => { item.RefreshIcons(); });

        if (CachedParameter is MainSceneRewardParameter rewardParameter)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("[Get Reward] {0} / {1} / {2}", rewardParameter.rewardCoin, rewardParameter.rewardPuzzlePiece, rewardParameter.rewardSweetHolic));
            bool increaseFinished = await globalData.HUD_LateUpdate_MainSceneReward(
                rewardParameter.clearedLevel, rewardParameter.openPuzzleIndex, 
                rewardParameter.rewardCoin, rewardParameter.rewardPuzzlePiece, 
                rewardParameter.rewardSweetHolic);

            await UniTask.WaitUntil(() => increaseFinished);

            GlobalDefine.CheckEventRefresh();
            globalData.fragmentHome?.Refresh(m_userManager.Current);

            await globalData.CheckPuzzleOpen(rewardParameter.openPuzzleIndex);

            if (GlobalDefine.IsEnable_Review(rewardParameter.clearedLevel))
            {
                await globalData.ShowPopup_Review();
            }
        }
        else
        {
            GlobalDefine.CheckEventRefresh();
            globalData.fragmentHome?.Refresh(m_userManager.Current);
        }

        // [Level >= 15] && [not claimed] && [RewardVideo Ready]
        if (GlobalDefine.IsEnable_DailyRewards() && GlobalDefine.IsRewardVideoReady())
        {
            await globalData.ShowPopup_DailyRewards();
        }

        // [Level >= 10] && [not purchased]
        if (GlobalDefine.IsEnable_BeginnerBundle())
        {
            await globalData.ShowPopup_Beginner(() => globalData.fragmentHome?.SideContainers.ForEach(item => { item.RefreshIcons(); }));
        }

        // [Level >= 1] && [Weekend]
        if (GlobalDefine.IsEnable_Weekend1Bundle())
        {
            // [TODO] ShowPopup_Weekend1
            await globalData.ShowPopup_Weekend1(() => globalData.fragmentHome?.SideContainers.ForEach(item => { item.RefreshIcons(); }));
        }

        // [Level >= 1] && [Weekend]
        if (GlobalDefine.IsEnable_Weekend2Bundle())
        {
            // [TODO] ShowPopup_Weekend2
            await globalData.ShowPopup_Weekend2(() => globalData.fragmentHome?.SideContainers.ForEach(item => { item.RefreshIcons(); }));
        }

        // [Level >= 20] && [Interstitial AD]
        if (GlobalDefine.IsEnable_RemoveAdsPopup())
        {
            await globalData.ShowPopup_RemoveAds();
        }

        // [Level >= 21]
        if (GlobalDefine.IsEnable_EventPopup_SweetHolic())
        {
            Debug.Log(CodeManager.GetMethodName() + "<color=yellow>Show New Event Popup : Sweet Holic</color>");
            //
        }

        globalData.SetTouchLock_MainScene(false);
        globalData.isAutoPopupPending = false;
    }

	private void OnDestroy()
	{
		m_userManager.OnUpdated -= OnUpdateUI;
	}

	private void OnUpdateUI(User user)
	{
		m_scrollView.OnUpdateUI(user);
	}

    private bool IsEnableShowPopup()
    {
        return globalData.CURRENT_SCENE != GlobalDefine.SCENE_PLAY &&
            transform.root.childCount < 2 && 
            !globalData.IsTouchLockNow_MainScene() &&
            !GlobalDefine.IsPurchasePending();
    }
}
