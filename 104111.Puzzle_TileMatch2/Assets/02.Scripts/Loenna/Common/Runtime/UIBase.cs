
namespace NineTap.Common
{
	public abstract class UIBase : CachedBehaviour
	{
		public UIParameter CachedParameter { get; set; }

		public virtual void OnSetup(UIParameter uiParameter)
		{
            CachedParameter = uiParameter;
		}

		public virtual void Show()
		{
			CachedGameObject.SetActive(true);
			OnShow();
		}

		public abstract void OnShow();

		public virtual void Hide()
		{
			CachedGameObject.SetActive(false);
			OnHide();
		}

		public abstract void OnHide();
	}
}
