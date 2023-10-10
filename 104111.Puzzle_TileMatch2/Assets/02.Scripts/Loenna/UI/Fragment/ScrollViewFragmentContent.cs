using NineTap.Common;

public class ScrollViewFragmentContentParameter
{
}

public abstract class ScrollViewFragmentContent : CachedBehaviour
{
	public abstract void OnSetup(ScrollViewFragmentContentParameter contentParameter);

	public virtual void OnUpdateUI(User user)
	{ 
	}
}
