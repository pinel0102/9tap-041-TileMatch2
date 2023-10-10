using UnityEngine;

using System.Collections.Generic;

using NineTap.Common;

public record MainSceneParameter(MainMenuType ShowMenuType = MainMenuType.HOME, PuzzleManager PuzzleManager = null) : DefaultParameter();

[ResourcePath("UI/Scene/MainScene")]
public class MainScene : UIScene
{
	[SerializeField]
	private MainSceneScrollView m_scrollView;

	[SerializeField]
	private MainSceneNavigationView m_navigationView;

	private LobbyManager m_lobbyManager;
	private	UserManager m_userManager;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not MainSceneParameter parameter)
		{
			return;
		}

		m_userManager = Game.Inst.Get<UserManager>();
		m_lobbyManager = new LobbyManager(
			m_userManager,
			tableManager: Game.Inst.Get<TableManager>()
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
									AwakeOn = m_userManager.Current.Settings[SettingsType.Fx],
									IconBuilder = isOn => SettingsType.Fx.GetIconPath(isOn),
									OnToggle = value => m_userManager.UpdateSettings(SettingsType.Fx, value),
								},
								new UIToggleButtonParameter{
									Text = SettingsType.Bgm.GetName(),
									AwakeOn = m_userManager.Current.Settings[SettingsType.Bgm],
									IconBuilder = isOn => SettingsType.Bgm.GetIconPath(isOn),
									OnToggle = value => m_userManager.UpdateSettings(SettingsType.Bgm, value),
								},
								new UIToggleButtonParameter{
									Text = SettingsType.Vibration.GetName(),
									AwakeOn = m_userManager.Current.Settings[SettingsType.Vibration],
									IconBuilder = isOn => SettingsType.Vibration.GetIconPath(isOn),
									OnToggle = value => m_userManager.UpdateSettings(SettingsType.Bgm, value),
								},
								new UIToggleButtonParameter{
									Text = SettingsType.Notification.GetName(),
									AwakeOn = true,
									IconBuilder = isOn => SettingsType.Notification.GetIconPath(isOn),
									OnToggle = value => {},
								}
							},
							AchievementButtonParameter = new UITextButtonParameter{
								ButtonText = "Achievement",
								OnClick = Stub.Nothing
							},
							PrivacyButtonParameter = new UITextButtonParameter{
								ButtonText = "Privacy Policy",
								OnClick = Stub.Nothing
							},
							ContactButtonParameter = new UITextButtonParameter{
								ButtonText = "Contact",
								OnClick = Stub.Nothing
							}
						}
					)
				}
			)
		);

		m_navigationView.OnSetup(
			new MainSceneNavigationViewParameter{
				OnClickTab = type => m_scrollView.MoveTo((int)type)
			}
		);

		m_userManager.OnUpdated += OnUpdateUI;
	}

	public override void Show()
	{
		base.Show();

		OnUpdateUI(m_userManager.Current);

		if (CachedParameter is not MainSceneParameter parameter)
		{
			return;
		}

		m_scrollView.MoveTo((int)parameter.ShowMenuType);
	}

	private void OnDestroy()
	{
		m_userManager.OnUpdated -= OnUpdateUI;
	}

	private void OnUpdateUI(User user)
	{
		m_scrollView.OnUpdateUI(user);
	}
}
