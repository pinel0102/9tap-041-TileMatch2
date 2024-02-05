using UnityEngine;

using NineTap.Common;

public record StoreSceneParameter(MainSceneFragmentContentParameter_Store StoreParam) : UIParameter(HUDType.COIN);

[ResourcePath("UI/Scene/StoreScene")]
public class StoreScene : UIScene
{
	[SerializeField]
	private MainSceneFragmentContent_Store m_storeFragment;

    public GameObject m_purchasing;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is StoreSceneParameter parameter)
		{
			m_storeFragment.OnSetup(parameter.StoreParam);
		}

        GlobalData.Instance.storeScene = this;
	}
}
