using System;

using Cysharp.Threading.Tasks;

public class UIButtonParameter
{
	public bool FadeEffect = false;
	public Action OnClick;
	public IUniTaskAsyncEnumerable<bool> Binder;
}