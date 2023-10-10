using UnityEngine;
using UnityEngine.LowLevel;

public static class InitUniTaskLoopFix
{
    // AfterAssembliesLoaded is called before BeforeSceneLoad
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void InitUniTaskLoop()
	{
		var loop = PlayerLoop.GetCurrentPlayerLoop();
		Cysharp.Threading.Tasks.PlayerLoopHelper.Initialize(ref loop);
	}
}
