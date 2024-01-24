#nullable enable

using UnityEngine;
using UnityEngine.UI;

using NineTap.Common;

public static partial class UIManager
{
	private static GameObject? m_root;
	private static UIManager_Imp? s_implementation;
    private static UserManager? m_userManager;

	public static GameObject Root
	{
		get
		{
			if (m_root == null)
			{
				m_root = new GameObject { name = "Root"};
			}

			return m_root;
		}
	}

	public static Canvas? SceneCanvas { get; private set; }
	public static HUD? HUD => s_implementation?.HUD;
    public static UIScene? CurrentScene => s_implementation?.SceneManager?.CurrentScene; 

	public static void Initialize(ServiceRegistry<object> serviceRegistry, UserManager userManager)
	{
        Debug.Log(CodeManager.GetMethodName());
        
        m_userManager = userManager;

		HUD hud = new HUD(userManager);
		SceneManager sceneManager = CreateSceneManager();
		PopupManager popupManager = new PopupManager(Root);

		s_implementation = new UIManager_Imp(hud, sceneManager, popupManager);

		serviceRegistry.Register(hud);
		serviceRegistry.Register(sceneManager);
		serviceRegistry.Register(popupManager);
	}

	public static void ShowSceneUI<T>(UIParameter uiParameter, bool clearStack = false) where T: UIScene
	{
        Debug.Log(CodeManager.GetMethodName() + uiParameter.ToString());

		s_implementation?.AttachHUD(uiParameter.VisibleHUD)?.SceneManager?.ShowSceneUI<T>(uiParameter, clearStack);
	}

	public static T? ShowPopupUI<T>(UIParameter uiParameter) where T: UIPopup
	{
		return s_implementation?.AttachHUD(uiParameter.VisibleHUD)?.PopupManager?.ShowPopup<T>(uiParameter);
	}

	public static void ReturnBackUI()
	{
        if (GlobalData.Instance.IsTouchLockNow_MainScene() || GlobalData.Instance.IsTouchLockNow_PlayScene()) 
            return;
        
		// 먼저 팝업 닫기 시도
		bool closedPopup = s_implementation?.PopupManager?.ClosePopupUI() ?? false;

        if(closedPopup)
        {
            Debug.Log(CodeManager.GetMethodName() + "Close Popup");
            return;
        }    
		else
		{
            if (s_implementation?.PopupManager?.Last != null)
            {
                UIParameter parameter = s_implementation.PopupManager.Last.CachedParameter!;
                s_implementation.AttachHUD(parameter.VisibleHUD);
            }
            else
            {
                UIScene scene = s_implementation?.SceneManager?.CurrentScene!;
                if (scene != null)
                {
                    s_implementation.AttachHUD(scene.CachedParameter.VisibleHUD);
                }

                switch (s_implementation?.SceneManager?.CurrentScene)
                {
                    case MainScene mainScene:
                        Debug.Log(CodeManager.GetMethodName() + mainScene.scrollView.PagedRect.currentTab);
                        if(mainScene.scrollView.PagedRect.currentTab != MainMenuType.HOME)
                        {
                            mainScene.MoveTo(MainMenuType.HOME);
                        }
                        else
                        {
                            // Quit 팝업 띄우기
                            ShowPopupUI<SimplePopup>(
                                new SimplePopupParameter(
                                    Title: NineTap.Constant.Text.Popup.Title.QUIT,
                                    Message: NineTap.Constant.Text.Popup.Message.Quit,
                                    ExitParameter: ExitBaseParameter.CancelParam,
                                    BaseButtonParameter: new UITextButtonParameter {
                                        ButtonText = NineTap.Constant.Text.Button.PLAY_ON,
                                        OnClick = ClosePopupUI
                                    },
                                    LeftButtonParameter: new UITextButtonParameter {
                                        ButtonText = NineTap.Constant.Text.Button.QUIT,
                                        OnClick = Application.Quit
                                    },						
                                    HUDTypes: mainScene.CachedParameter.VisibleHUD
                                )
                            );
                        }
                        break;
                    case PlayScene playScene:
                        // Pause 팝업 띄우기
                        playScene.OnPause();
                        break;
                    case StoreScene storeScene:
                        s_implementation?.SceneManager?.BackSceneUI();
                        break;
                }
            }
		}
	}

    public static void ClosePopupUI_ForceAll()
    {
        s_implementation?.PopupManager?.ClosePopupUI_ForceAll();
    }

    public static void ClosePopupUI()
	{
		s_implementation?.PopupManager?.ClosePopupUI();
	}

	public static void ClosePopupUI(UIPopup popup)
	{
		s_implementation?.PopupManager?.ClosePopupUI(popup);
	}

    public static void ClosePopupUI(params HUDType[] types)
    {
        s_implementation?.PopupManager?.ClosePopupUI();
        GlobalData.Instance.HUD_Show(types);
    }

    public static void ClosePopupUI(UIPopup popup, params HUDType[] types)
	{
		s_implementation?.PopupManager?.ClosePopupUI(popup);
        GlobalData.Instance.HUD_Show(types);
	}

	private static SceneManager CreateSceneManager()
	{
		GameObject canvasObj = new GameObject { name = "Canvas" };
		canvasObj.transform.SetParentReset(Root.transform, true);

		Canvas canvas = canvasObj.AddComponent<Canvas>();
		SceneCanvas = canvas;
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 0;
		canvas.transform.SetLayer(LayerMask.NameToLayer("UI"));

		CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();
		canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		canvasScaler.referenceResolution = new Vector2(Constant.UI.REFERENCE_RESOLUTION_X, Constant.UI.REFERENCE_RESOLUTION_Y);
		canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
		
		return new SceneManager(canvasObj);
	}

	public static UIManager_Imp? AttachHUD(this UIManager_Imp? imp, params HUDType[] types)
	{
		imp?.HUD?.Show(types);
		return imp;
	}

	public static void DetachAllHUD() => s_implementation?.HUD?.Hide();

	public static void ShowLoading(LoadingPopup.Options option = LoadingPopup.Options.Default) => s_implementation?.ShowLoading(option);

	public static void HideLoading() => s_implementation?.HideLoading();
}

public class UIManager_Imp
{
	private readonly HUD? m_hud;
	private readonly SceneManager? m_sceneManager;
	private readonly PopupManager? m_popupManager;

	private LoadingPopup? m_loadingPopup;

	public SceneManager? SceneManager => m_sceneManager;
	public PopupManager? PopupManager => m_popupManager;
	public HUD? HUD => m_hud;

    public UIManager_Imp(HUD? hud, SceneManager? sceneManager, PopupManager? popupManager)
	{
		m_hud = hud;
		m_sceneManager = sceneManager;
		m_popupManager = popupManager;
	}
	
	public void ShowLoading(LoadingPopup.Options options)
	{
		if (m_loadingPopup == null)
		{
			m_loadingPopup = m_popupManager?.ShowPopup<LoadingPopup>(new DefaultParameterWithoutHUD());
		}

		m_loadingPopup?.SetOption(options);
		m_loadingPopup?.Show();
	}

	public void HideLoading()
	{
		m_loadingPopup?.Hide();
	}

}
