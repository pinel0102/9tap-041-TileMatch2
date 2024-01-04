using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using NineTap.Payment;
using NineTap.Common;
using Text = NineTap.Constant.Text;

using static UnityEngine.SceneManagement.SceneManager;

public record PlaySceneParameter : DefaultParameterWithoutHUD;

[ResourcePath("UI/Scene/PlayScene")]
public partial class PlayScene : UIScene
{
	[SerializeField]	private CanvasGroup m_canvasGroup;
	[SerializeField]	private PlaySceneTopUIView m_topView;
	[SerializeField]	private PlaySceneBoardView m_mainView;
	[SerializeField]	private PlaySceneBottomUIView m_bottomView;
	[SerializeField]	private GameObject m_block;
	[SerializeField]	private Transform m_particleParent;
    [SerializeField]	private GameObject bg_default;
    [SerializeField]	private GameObject bg_puzzle;
    [SerializeField]    private Image backgroundImage;

	private GameManager m_gameManager;
	private UserManager m_userManager;
	private PaymentService m_paymentService;
	private ItemDataTable m_itemDataTable;
	private ProductDataTable m_productDataTable;
    private PuzzleDataTable m_puzzleDataTable;

    public GameManager gameManager => m_gameManager;
    public PlaySceneBoardView mainView => m_mainView;

	private void OnDestroy()
	{
		m_gameManager?.Dispose();	
	}

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

        Debug.Log(CodeManager.GetMethodName());

		if (uiParameter is not PlaySceneParameter parameter)
		{
			Debug.LogError(new InvalidCastException(nameof(PlaySceneParameter)));
			return;
		}

		TableManager tableManager = Game.Inst.Get<TableManager>();
		m_userManager = Game.Inst.Get<UserManager>();
		m_gameManager = new GameManager(m_userManager, tableManager);
		m_paymentService = Game.Inst.Get<PaymentService>();
		m_itemDataTable = tableManager.ItemDataTable;
		m_productDataTable = tableManager.ProductDataTable;
        m_puzzleDataTable = tableManager.PuzzleDataTable;
		m_tileItems = new List<TileItem>();

		Dictionary<SkillItemType, AsyncReactiveProperty<int>> skillDic = new Dictionary<SkillItemType, AsyncReactiveProperty<int>>();

		foreach (var skill in m_userManager.Current.OwnSkillItems)
		{
			AsyncReactiveProperty<int> eachCount = new(skill.Value);
			m_userManager.OnUpdated += user => {
				if (user.OwnSkillItems.TryGetValue(skill.Key, out int count))
				{
					eachCount.Value = count;
				}
			};
			skillDic.TryAdd(skill.Key, eachCount);
		}

		m_bottomView.OnSetup(
			new PlaySceneBottomUIViewParameter
			{
				Stash = new UIImageButtonParameter
				{
					Binder = m_gameManager.BasketNotEmpty,
					OnClick = () => m_gameManager.UseSkillItem(SkillItemType.Stash, true, OpenBuyItemPopup),
					SubWidgetBuilder = () => CreateButtonSubWidget(SkillItemType.Stash)
				},
				Undo = new UIImageButtonParameter
				{
					Binder = m_gameManager.Invoker.NotEmpty,
					OnClick = () => m_gameManager.UseSkillItem(SkillItemType.Undo, true, OpenBuyItemPopup),
					SubWidgetBuilder = () => CreateButtonSubWidget(SkillItemType.Undo)
				},
				Shuffle = new UIImageButtonParameter
				{
					OnClick = () => m_gameManager.UseSkillItem(SkillItemType.Shuffle, true, OpenBuyItemPopup),
					SubWidgetBuilder = () => CreateButtonSubWidget(SkillItemType.Shuffle)
				},
				Pause = new UIImageButtonParameter { OnClick = OnPause }
			}
		);

		SetupInternal();

		GameObject CreateButtonSubWidget(SkillItemType itemType)
		{
            if (!m_itemDataTable.TryGetValue((int)itemType, out var itemData))
            {
                return null;
            }

            if (!m_productDataTable.TryGetValue(itemData.ProductIndex, out var product))
            {
                return null;
            }

			var widget = Instantiate(ResourcePathAttribute.GetResource<ButtonCountWidget>());
			if (skillDic.TryGetValue(itemType, out var binder))
			{
                widget.BindTo(binder, product);
			}
			return widget.gameObject;
		}

		void OpenBuyItemPopup(int itemIndex)
		{
            if (!m_itemDataTable.TryGetValue(itemIndex, out var itemData))
			{
				return;
			}

			if (!m_productDataTable.TryGetValue(itemData.ProductIndex, out var product))
			{
				return;
			}

			var (valid, _, _) = m_userManager.Current.Valid(requireCoin: (long)product.Price);
			
			/*if (!valid)
			{
				UIManager.ShowSceneUI<StoreScene>(
					new StoreSceneParameter(
						StoreParam: new MainSceneFragmentContentParameter_Store {
							TitleText = "Store",
							CloseButtonParameter = new UIImageButtonParameter {
								OnClick = UIManager.ReturnBackUI
							}
						}
					)
				);
				return;
			}*/

			UIManager.ShowPopupUI<BuyItemPopup>(
				new BuyItemPopupParameter(
					Title: $"{itemData.Name}",
					Message: $"{itemData.Description}",
					ExitParameter: ExitBaseParameter.CancelParam,
					BaseButtonParameter: new UITextButtonParameter {
						OnClick = () => {
                            if (valid)
                            {
                                m_paymentService.Request(
                                    product.Index, 
                                    onSuccess: (result) => {
                                        var ownSkillItems = m_userManager.Current.OwnSkillItems;
                                        ownSkillItems[(SkillItemType)itemIndex] += 3;
                                        m_userManager.Update(ownSkillItems: ownSkillItems);
                                        m_gameManager.UseSkillItem((SkillItemType)itemIndex, true);
                                    },
                                    onError: (error) => { }
                                );
                            }
                            else
                            {
                                UIManager.ShowSceneUI<StoreScene>(
                                    new StoreSceneParameter(
                                        StoreParam: new MainSceneFragmentContentParameter_Store {
                                            TitleText = "Store",
                                            CloseButtonParameter = new UIImageButtonParameter {
                                                OnClick = () => { 
                                                    UIManager.ReturnBackUI();
                                                }
                                            }
                                        }
                                    )
                                );
                            }
						},
						ButtonText = "Buy",
						SubWidgetBuilder = () => {
							var widget = Instantiate(ResourcePathAttribute.GetResource<IconWidget>());
							widget.OnSetup("UI_Icon_Coin", $"{Mathf.FloorToInt(product.Price)}");
							return widget.CachedGameObject;
						}
					},
					Product: product
				)
			);
		}

		m_gameManager.LoadLevel(m_userManager.Current.Level, m_mainView.CachedRectTransform);
	}

	public void OnPause()
	{
		UIManager.ShowPopupUI<PausePopup>(
			new PausePopupParameter(
				Title: Text.PAUSE,
				ToggleButtonParameters: new List<UIToggleButtonParameter> {
					new UIToggleButtonParameter {
						Text = SettingsType.Fx.GetName(),
						AwakeOn = m_userManager.Current.Settings.GetValueOrDefault(SettingsType.Fx, true),
						IconBuilder = isOn => SettingsType.Fx.GetIconPath(isOn),
						OnToggle = value => m_userManager.UpdateSettings(SettingsType.Fx, value),
					},
					new UIToggleButtonParameter {
						Text = SettingsType.Bgm.GetName(),
						AwakeOn = m_userManager.Current.Settings.GetValueOrDefault(SettingsType.Bgm, true),
						IconBuilder = isOn => SettingsType.Bgm.GetIconPath(isOn),
						OnToggle = value => m_userManager.UpdateSettings(SettingsType.Bgm, value),
					},
					new UIToggleButtonParameter {
						Text = SettingsType.Vibration.GetName(),
						AwakeOn = m_userManager.Current.Settings.GetValueOrDefault(SettingsType.Vibration, true),
						IconBuilder = isOn => SettingsType.Vibration.GetIconPath(isOn),
						OnToggle = value => m_userManager.UpdateSettings(SettingsType.Vibration, value),
					}
				},
				TopButtonParameter: new UITextButtonParameter
				{
					ButtonText = Text.Button.REPLAY,
					OnClick = () =>
					{
						ShowGiveUpPopup(
							new UITextButtonParameter
							{
								ButtonText = Text.Button.PLAY_ON
							},
							new ExitBaseParameter(
								includeBackground: false,
								onExit: () =>
								{
									m_userManager.TryUpdate(requireLife: true);
									ShowReadyPopup(m_gameManager.CurrentLevel);
								}
							)
						);
					}
				},
				BottomButtonParameter: new UITextButtonParameter
				{
					ButtonText = Text.Button.HOME,
					OnClick = () =>
					{
						ShowGiveUpPopup(
							new UITextButtonParameter
							{
								ButtonText = Text.Button.PLAY_ON
							},
							new ExitBaseParameter(
								includeBackground: false,
								onExit: () =>
								{
									m_userManager.TryUpdate(requireLife: true);
									OnExit(false);
								}
							)
						);
					}
				}
			)
		);
	}

	private void ShowNext(CurrentPlayState.Finished.State stateType, int coinAmount, Action onContinue)
	{
		if (stateType is CurrentPlayState.Finished.State.OVER)
		{
			ShowGiveUpPopup(
				new UITextButtonParameter {
					ButtonText = Text.Button.PLAY_ON,
					OnClick = onContinue,
					SubWidgetBuilder = () => {
						var widget = Instantiate(ResourcePathAttribute.GetResource<IconWidget>());
						widget.OnSetup("UI_Icon_Coin", $"{coinAmount}");
						return widget.CachedGameObject;
					}
				},
				new ExitBaseParameter (
					includeBackground: false,
					onExit: () => {
						m_userManager.TryUpdate(requireLife: true);
						OnExit(false);
					}
				)
			);
			//return;
		}

		//UIManager.ClosePopupUI();

		//m_canvasGroup.alpha = 0f;
		//UIManager.ShowPopupUI<GameClearPopup>(
		//	new GameClearPopupParameter(
		//		m_gameManager.CurrentLevel, 
		//		OnContinue: level => m_gameManager.LoadLevel(level, m_mainView.CachedRectTransform)
		//	)
		//);
	}

	private void ShowGiveUpPopup(UITextButtonParameter buttonParameter, ExitBaseParameter exitBaseParameter)
	{
		UIManager.ShowPopupUI<GiveupPopup>(
			new DefaultPopupParameter(
				Title: Text.Popup.Title.GIVE_UP,
				Message: Text.Popup.Message.GIVE_UP,
				ExitParameter: exitBaseParameter,
				BaseButtonParameter: buttonParameter
			)
		);
	}

	private void ShowReadyPopup(int level)
	{
        var (_, valid, _) = m_userManager.Current.Valid();

        if (!valid)
        {
            //하트 구매 요구 (TBD)
            UIManager.ShowPopupUI<GiveupPopup>(
                new DefaultPopupParameter(
                    Title: "Purchase",
                    Message: "Purchase Life",
                    ExitParameter: new ExitBaseParameter(
                        onExit: () => OnExit(false)
                    ),
                    BaseButtonParameter: new UITextButtonParameter {
                        ButtonText = "Go to Shop",
                        OnClick = () =>OnExit(true)
                    }
                )
            );
            return;
        }
        m_gameManager.LoadLevel(level, m_mainView.CachedRectTransform);

		/*UIManager.ShowPopupUI<ReadyPopup>(
			new ReadyPopupParameter(
				Level: level,
				BaseButtonParameter: new UITextButtonParameter
				{
					ButtonText = Text.Button.REPLAY,
					OnClick = () => {
						var (_, valid, _) = m_userManager.Current.Valid();

						if (!valid)
						{
							//하트 구매 요구 (TBD)
							UIManager.ShowPopupUI<GiveupPopup>(
								new DefaultPopupParameter(
									Title: "Purchase",
									Message: "Purchase Life",
									ExitParameter: new ExitBaseParameter(
										onExit: () => OnExit(false)
									),
									BaseButtonParameter: new UITextButtonParameter {
										ButtonText = "Go to Shop",
										OnClick = () =>OnExit(true)
									}
								)
							);
							return;
						}
						m_gameManager.LoadLevel(level, m_mainView.CachedRectTransform);
					}
				},
				ExitParameter: new ExitBaseParameter(() => OnExit(false), false),
				AllPressToClose: true
			)
		);*/		
	}

	private void OnExit(bool onJumpStore)
	{
		UIManager.ClosePopupUI();
		string mode = PlayerPrefs.GetString(Constant.Editor.DEVELOP_MODE_SCENE_KEY, Constant.Scene.CLIENT);

		if (mode == Constant.Scene.CLIENT)
		{
			UIManager.ShowSceneUI<MainScene>(new MainSceneParameter { ShowMenuType = onJumpStore? MainMenuType.STORE : MainMenuType.HOME });
		}
		else
		{
			LoadScene(Constant.Scene.EDITOR);
		}
	}

}
