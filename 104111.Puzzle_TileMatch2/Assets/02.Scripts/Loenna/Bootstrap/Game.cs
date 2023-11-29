using DG.Tweening;

using UnityEngine;
using UnityEngine.InputSystem;

using NineTap.Payment;
using NineTap.Common;

// 처음 구동
public class Game : MonoBehaviour
{
    private static Game s_inst;
	public static Game Inst => s_inst;

	private ServiceRegistry<object> m_serviceRegistry = new();
	private UserManager m_userManager;

	private void Awake()
	{
		Application.runInBackground = true;
		
		if (s_inst != null)
		{
			return;
		}
		s_inst = this;
	}

	private void Start()
	{
		Initialize();
	}

    private void Initialize()
    {
        Debug.Log(CodeManager.GetMethodName());

        Application.targetFrameRate = 120;
		DOTween.SetTweensCapacity(500, 50);
		SpriteManager.Initialize();
		TableManager tableManager = new TableManager();
		TimeManager timeManager = new TimeManager();
		m_userManager = new UserManager(timeManager);
		SoundManager soundManager = new SoundManager(gameObject, m_userManager);

		PaymentService paymentService = new PaymentService(new CoinService(m_userManager), new IAPService());

		UIManager.Initialize(m_serviceRegistry, m_userManager);

		// 필요한 서비스들을 초기화 후 등록
		m_serviceRegistry.Register(m_userManager);
		m_serviceRegistry.Register(paymentService);
		m_serviceRegistry.Register(tableManager);
		m_serviceRegistry.Register(timeManager);
		m_serviceRegistry.Register(soundManager);
		
		UIManager.ShowSceneUI<InitScene>(new DefaultParameterWithoutHUD());
    }

	public TService Get<TService>()
	{
		return m_serviceRegistry.Get<TService>();
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			m_userManager.Save();
		}
	}

	private void OnApplicationQuit()
	{
		m_userManager.Save();
		m_serviceRegistry.Dispose();
	}

	private void OnDestroy()
	{
		m_serviceRegistry.Dispose();
	}

	private void FixedUpdate()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
			Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            UIManager.ReturnBackUI();
        }
    }
}
