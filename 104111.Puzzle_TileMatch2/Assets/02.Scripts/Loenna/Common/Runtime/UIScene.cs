using UnityEngine;

namespace NineTap.Common
{
	public class UIScene : UIBase
	{
		public enum HideType
		{
			Hide,
			Destroy
		}

		[SerializeField]
		private HideType m_hideOption = HideType.Hide;
		public HideType HideOption => m_hideOption;


		public override void OnSetup(UIParameter uiParameter)
		{
			base.OnSetup(uiParameter);
		}

		public override void OnShow()
		{
			
		}

		public override void OnHide()
		{
			
		}

	}
}
