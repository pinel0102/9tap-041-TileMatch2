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

public record PlaySceneParameterCustom
(
    int Level
) : PlaySceneParameter;

[ResourcePath("UI/Scene/PlayScene")]
public partial class PlayScene : UIScene
{
	[SerializeField]	private CanvasGroup m_canvasGroup;
	[SerializeField]	private PlaySceneTopUIView m_topView;
	[SerializeField]	private PlaySceneBoardView m_mainView;
	[SerializeField]	private PlaySceneBottomUIView m_bottomView;
	[SerializeField]	public GameObject m_block;
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
    public PlaySceneBottomUIView bottomView => m_bottomView;

	private void OnDestroy()
	{
		m_gameManager?.Dispose();	
	}

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

        //Debug.Log(CodeManager.GetMethodName());

		if (uiParameter is not PlaySceneParameter parameter)
		{
			Debug.LogError(new InvalidCastException(nameof(PlaySceneParameter)));
			return;
		}

        GlobalData.Instance.playScene = this;

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

        int Level = uiParameter is PlaySceneParameterCustom paramCustom ? paramCustom.Level : m_userManager.Current.Level;

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
                                GlobalDefine.RequestAD_HideBanner();
                                
                                GlobalData.Instance.ShowStorePopup(() => {
                                    GlobalDefine.RequestAD_ShowBanner();
                                    GlobalData.Instance.HUD_Hide();
                                });
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

        m_gameManager.LoadLevel(Level, m_mainView.CachedRectTransform);
	}

	public void OnPause()
	{
        SDKManager.SendAnalytics_C_Scene(Text.PAUSE);

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
                        if (GlobalData.Instance.isLevelEditor)
                        {
                            m_gameManager.LoadLevel(m_gameManager.CurrentLevel, m_mainView.CachedRectTransform);
                        }
                        else
                        {
                            SDKManager.SendAnalytics_C_Scene(Text.Button.REPLAY);
                            ShowRetryPopup();

                            /*ShowGiveUpPopup(
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
                            );*/
                        }
					}
				},
				BottomButtonParameter: new UITextButtonParameter
				{
					ButtonText = Text.Button.HOME,
					OnClick = () =>
					{
                        if (GlobalData.Instance.isLevelEditor)
                        {
                            OnExit(false);
                        }
                        else
                        {
                            SDKManager.SendAnalytics_C_Scene(Text.Button.HOME);

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
				}
			)
		);
	}

    private void ShowRetryPopup()
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

	private void ShowGiveUpPopup(UITextButtonParameter buttonParameter, ExitBaseParameter exitBaseParameter)
	{
        //[PlayScene:Pause] PausePopup > Replay : 하트 소모 알림. (x 누를시 Replay & 광고)
        //[PlayScene:Pause] PausePopup > Home : 하트 소모 알림. (x 누를시 Home & 광고)
        //[PlayScene:Fail] PlayEndPopup -> GiveUp/Home : 하트 소모 알림. (x 누를시 Home & 광고)
		UIManager.ShowPopupUI<GiveupPopup>(
			new GiveupPopupParameter(
				Title: Text.Popup.Title.GIVE_UP,
				Message: Text.Popup.Message.GIVE_UP,
                ignoreBackKey: true,
				ExitParameter: exitBaseParameter,
				BaseButtonParameter: buttonParameter
			)
		);
	}

    // 하트 0인 상태에서 플레이 시도시 상점 열기.
	private void ShowReadyPopup(int level)
	{
        var (_, valid, _) = m_userManager.Current.Valid();

        if (!valid)
        {
            //[PlayScene:Replay] 하트 부족시 상점 열기.
            SDKManager.SendAnalytics_C_Scene(Text.Button.STORE);

            GlobalDefine.RequestAD_HideBanner();

            GlobalData.Instance.ShowStorePopup(() => {
                GlobalDefine.RequestAD_ShowBanner();
                ShowRetryPopup();
            });
            
            return;
        }
        m_gameManager.LoadLevel(level, m_mainView.CachedRectTransform);
	}

    public override void Show()
    {
        base.Show();

        UIManager.ClosePopupUI_ForceAll();
        SDKManager.SendAnalytics_I_Scene();
        GlobalDefine.RequestAD_ShowBanner();
    }

	private void OnExit(bool onJumpStore)
	{
		UIManager.ClosePopupUI();
        GlobalDefine.RequestAD_Interstitial();
        
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
