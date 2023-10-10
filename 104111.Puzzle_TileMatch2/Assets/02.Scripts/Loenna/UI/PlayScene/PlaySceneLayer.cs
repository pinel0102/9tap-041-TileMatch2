using UnityEngine;

using NineTap.Common;

[ResourcePath("UI/Widgets/Layer")]
public class PlaySceneLayer : CachedBehaviour
{
	private CanvasGroup m_canvasGroup;

	public CanvasGroup CanvasGroup
	{
		get 
		{
			if (m_canvasGroup == null)
			{
				m_canvasGroup = CachedGameObject.AddComponent<CanvasGroup>();
			}
			return m_canvasGroup;
		}
	}
}
