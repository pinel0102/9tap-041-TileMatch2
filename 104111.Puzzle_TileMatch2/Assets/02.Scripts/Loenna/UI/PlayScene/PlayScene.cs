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
    [SerializeField]	private GameObject bg_ads;

    private GameManager m_gameManager;
	private UserManager m_userManager;
    private SoundManager m_soundManager;
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

        ClearFXLayer();

        TableManager tableManager = Game.Inst.Get<TableManager>();
		m_userManager = Game.Inst.Get<UserManager>();
		m_gameManager = new GameManager(m_userManager, tableManager);
		m_soundManager = Game.Inst.Get<SoundManager>();
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
					OnClick = () => m_gameManager.UseSkillItem(SkillItemType.Stash, true, (itemIndex) => {
                        OpenBuyItemPopup(itemIndex);
                    }),
					SubWidgetBuilder = () => CreateButtonSubWidget(SkillItemType.Stash)
				},
				Undo = new UIImageButtonParameter
				{
					Binder = m_gameManager.Invoker.NotEmpty,
					OnClick = () => m_gameManager.UseSkillItem(SkillItemType.Undo, true, (itemIndex) => {
                        OpenBuyItemPopup(itemIndex);
                    }),
					SubWidgetBuilder = () => CreateButtonSubWidget(SkillItemType.Undo)
				},
				Shuffle = new UIImageButtonParameter
				{
					OnClick = () => m_gameManager.UseSkillItem(SkillItemType.Shuffle, true, (itemIndex) => {
                        OpenBuyItemPopup(itemIndex);
                    }),
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

        m_gameManager.LoadLevel(Level, m_mainView.CachedRectTransform);
	}

    private void OpenBuyItemPopup(int itemIndex, Action onOpened = null, Action<bool> onClosed = null, Action onStoreClosed = null, Action onPurchased = null, bool ignoreBackKey = false)
    {
        if (!m_itemDataTable.TryGetValue(itemIndex, out var itemData))
        {
            return;
        }

        if (!m_productDataTable.TryGetValue(itemData.ProductIndex, out var product))
        {
            return;
        }

        GlobalData.Instance.SetTouchLock_PlayScene(true);

        var (valid, _, _) = m_userManager.Current.Valid(requireCoin: (long)product.Price);

        UIManager.ShowPopupUI<BuyItemPopup>(
            new BuyItemPopupParameter(
                Title: $"{itemData.Name}",
                Message: $"{itemData.Description}",
                ExitParameter: new ExitBaseParameter(
                    onExit: null,
                    includeBackground: !ignoreBackKey
                ),
                BaseButtonParameter: new UITextButtonParameter {
                    OnClick = () => {
                        if (valid)
                        {
                            m_paymentService.Request(
                                product.Index, 
                                onSuccess: (_, result) => {
                                    
                                    Dictionary<SkillItemType, int> addSkillItems = new()
                                    {
                                        { (SkillItemType)itemIndex, 3}
                                    };

                                    GlobalDefine.GetItems(addSkillItems: addSkillItems);

                                    m_gameManager.UseSkillItem((SkillItemType)itemIndex, true, onSuccess:onPurchased);
                                },
                                onError: (_, error) => { }
                            );
                        }
                        else
                        {
                            GlobalDefine.RequestAD_HideBanner();
                            GlobalData.Instance.ShowStorePopup(() => {
                                bg_ads.SetActive(!GlobalData.Instance.userManager.Current.NoAD);
                                GlobalDefine.RequestAD_ShowBanner();
                                GlobalData.Instance.HUD_Hide();
                                onStoreClosed?.Invoke();
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
                Product: product,
                OnOpened: onOpened,
                OnClosed: onClosed,
                IgnoreBackKey: ignoreBackKey
            )
        );
    }

    public void OnPause()
	{
        SDKManager.SendAnalytics_C_Scene(Text.PAUSE);
        GlobalData.Instance.SetTouchLock_PlayScene(true);

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
                        if (GlobalDefine.IsLevelEditor())
                        {
                            m_gameManager.LoadLevel(m_gameManager.CurrentLevel, m_mainView.CachedRectTransform);
                        }
                        else
                        {
                            SDKManager.SendAnalytics_C_Scene(Text.Button.REPLAY);
                            ShowRetryPopup(Text.Button.PLAY_ON);
                        }
					}
				},
				BottomButtonParameter: new UITextButtonParameter
				{
					ButtonText = Text.Button.HOME,
					OnClick = () =>
					{
                        if (GlobalDefine.IsLevelEditor())
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
                                ),
                                Text.Popup.Title.ARE_YOU_SURE
                            );
                        }
					}
				}
			)
		);
	}

    private void ShowRetryPopup(string buttonText)
    {
        ShowGiveUpPopup(
            new UITextButtonParameter
            {
                ButtonText = buttonText
            },
            new ExitBaseParameter(
                includeBackground: false,
                onExit: () =>
                {
                    m_userManager.TryUpdate(requireLife: true);
                    ShowReadyPopup(m_gameManager.CurrentLevel, buttonText);
                }
            ),
            Text.Popup.Title.ARE_YOU_SURE
        );
    }

	private void ShowGiveUpPopup(UITextButtonParameter buttonParameter, ExitBaseParameter exitBaseParameter, string titleText = Text.Popup.Title.LEVEL_FAILED)
	{
        //[PlayScene:Pause] PausePopup -> Replay & Home : 하트 소모 예정 알림. (x 누를시 실행 전 광고)
        //[PlayScene:Fail] PlayEndPopup & BlockerFailedPopup -> GiveUp : 선 하트 소모. (x 누를시 실행 전 광고)
		UIManager.ShowPopupUI<GiveupPopup>(
			new GiveupPopupParameter(
				Title: titleText,
				Message: Text.Popup.Message.GIVE_UP,
                ignoreBackKey: true,
				ExitParameter: exitBaseParameter,
				BaseButtonParameter: buttonParameter
			)
		);
	}

    // 하트 0인 상태에서 플레이 시도시 상점 열기.
	private void ShowReadyPopup(int level, string buttonText)
	{
        var (_, valid, _) = m_userManager.Current.Valid();

        if (!valid)
        {
            //[PlayScene:Replay] 하트 부족시 상점 열기.
            SDKManager.SendAnalytics_C_Scene(Text.Button.STORE);

            GlobalDefine.RequestAD_HideBanner();

            GlobalData.Instance.ShowStorePopup(() => {
                bg_ads.SetActive(!GlobalData.Instance.userManager.Current.NoAD);
                GlobalDefine.RequestAD_ShowBanner();
                ShowRetryPopup(buttonText);
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

        bg_ads.SetActive(!GlobalData.Instance.userManager.Current.NoAD);
        GlobalDefine.RequestAD_ShowBanner();
    }

	private void OnExit(bool onJumpStore)
	{
		UIManager.ClosePopupUI();
        GlobalDefine.RequestAD_Interstitial(onADComplete:OnADComplete);

        void OnADComplete()
        {
            if (GlobalDefine.IsLevelEditor())
            {
                LoadScene(Constant.Scene.EDITOR);
            }
            else
            {
                UIManager.ShowSceneUI<MainScene>(new MainSceneParameter { ShowMenuType = onJumpStore? MainMenuType.STORE : MainMenuType.HOME });
            }
        }
	}
}
