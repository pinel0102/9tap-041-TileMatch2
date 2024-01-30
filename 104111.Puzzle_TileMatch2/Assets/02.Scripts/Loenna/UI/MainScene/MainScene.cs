using UnityEngine;
using System.Collections.Generic;
using NineTap.Common;
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
    int rewardGoldPiece = 0
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

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not MainSceneParameter parameter)
		{
			return;
		}

        GlobalData.Instance.mainScene = this;
        GlobalData.Instance.fragmentHome = m_scrollView.Contents[(int)MainMenuType.HOME] as MainSceneFragmentContent_Home;
        GlobalData.Instance.fragmentCollection = m_scrollView.Contents[(int)MainMenuType.COLLECTION] as MainSceneFragmentContent_Collection;
        GlobalData.Instance.fragmentPuzzle = m_scrollView.Contents[(int)MainMenuType.JIGSAW_PUZZLE] as MainSceneFragmentContent_Puzzle;
        GlobalData.Instance.fragmentStore = m_scrollView.Contents[(int)MainMenuType.STORE] as MainSceneFragmentContent_Store;
        GlobalData.Instance.fragmentSettings = m_scrollView.Contents[(int)MainMenuType.SETTINGS] as MainSceneFragmentContent_Settings;

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
                        m_lobbyManager.OnCheckShowPopup(() => m_scrollView.MoveTo((int)MainMenuType.STORE));
                    } 
                }
            ),
            new KeyValuePair<HUDType, System.Action>(
                HUDType.LIFE, () => { 
                    if (IsEnableShowPopup() && !m_userManager.Current.IsBoosterTime()
                                            && !m_userManager.Current.IsFullLife())
                    {
                        soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
                        GlobalData.Instance.ShowBuyHeartPopup();
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
							OnClick = () => m_lobbyManager.OnCheckShowPopup(() => m_scrollView.MoveTo((int)MainMenuType.STORE))
						}
					),
					new ScrollViewFragmentParameter(
						MainMenuType.HOME,
						typeof(MainSceneFragmentContent_Home),
						new MainSceneFragmentContentParameter_Home{
							PuzzleButtonParam = new HomeFragmentLargeButtonParameter{
								ButtonText = string.Empty,
								OnClick = () => m_scrollView.MoveTo((int)MainMenuType.JIGSAW_PUZZLE),
								GaugeBarParameter = new UIProgressBarParameter {
									Type = UIProgressBar.Type.STATIC,
									VisibleText = true
								}
							},
							PlayButtonParam = new UITextButtonParameter{
								ButtonText = string.Empty,
								OnClick = () => m_lobbyManager.OnCheckShowPopup(() => m_scrollView.MoveTo((int)MainMenuType.STORE)),
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
                                            GlobalData.Instance.fragmentCollection.RefreshLockState();
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

	public override void Show()
	{
		base.Show();

		OnUpdateUI(m_userManager.Current);

        if (CachedParameter is not MainSceneParameter parameter)
		{
			return;
		}

        GlobalData.Instance.CURRENT_LEVEL = m_userManager.Current.Level;

        if (CachedParameter is MainSceneRewardParameter rewardParameter)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("[Get Reward] {0} / {1} / {2}", rewardParameter.rewardCoin, rewardParameter.rewardPuzzlePiece, rewardParameter.rewardGoldPiece));
            GlobalData.Instance.HUD_LateUpdate_MainSceneReward(rewardParameter.clearedLevel, rewardParameter.openPuzzleIndex, rewardParameter.rewardCoin, rewardParameter.rewardPuzzlePiece, rewardParameter.rewardGoldPiece);
        }

        if (GlobalDefine.IsEnableDailyRewards() && GlobalDefine.IsRewardVideoReady())
            GlobalData.Instance.ShowDailyRewardPopup();

        m_scrollView.MoveTo((int)parameter.ShowMenuType);

        SDKManager.SendAnalytics_I_Scene();
        GlobalDefine.RequestAD_HideBanner();
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
        return GlobalData.Instance.CURRENT_SCENE != GlobalDefine.SCENE_PLAY &&
            transform.root.childCount < 2 && 
            !GlobalData.Instance.IsTouchLockNow_MainScene();
    }
}
