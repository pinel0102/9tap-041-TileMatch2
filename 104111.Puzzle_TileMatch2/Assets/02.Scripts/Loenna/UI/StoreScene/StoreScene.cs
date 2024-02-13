using UnityEngine;

using NineTap.Common;

public record StoreSceneParameter(MainSceneFragmentContentParameter_Store StoreParam) : UIParameter(HUDType.COIN);

[ResourcePath("UI/Scene/StoreScene")]
public class StoreScene : UIScene
{
	[SerializeField]
	private MainSceneFragmentContent_Store m_storeFragment;

    public GameObject m_purchasing;

    private GlobalData globalData { get { return GlobalData.Instance; } }

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is StoreSceneParameter parameter)
		{
			m_storeFragment.OnSetup(parameter.StoreParam);
            
            if (globalData?.CURRENT_SCENE == GlobalDefine.SCENE_PLAY)
            {
                globalData.fragmentStore_popup = m_storeFragment;
            }
		}

        globalData.storeScene = this;
	}

    public override void OnShow()
    {
        base.OnShow();

        globalData.storeScene = this;
    }

    public override void OnHide()
    {
        base.OnHide();

        globalData.storeScene = null;

        if (globalData?.CURRENT_SCENE == GlobalDefine.SCENE_PLAY)
        {
            globalData.fragmentStore_popup = null;
        }
    }
}
