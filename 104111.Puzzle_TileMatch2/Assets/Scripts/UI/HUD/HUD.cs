using UnityEngine;

using System;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using NineTap.Common;

[Flags]
public enum HUDType : byte
{
	NONE = 0,

	STAR = 1,
	LIFE = 2,
	COIN = 4,

	ALL = STAR | LIFE | COIN
}

public class HUD
{
	private readonly HUDBehaviour m_behaviour;

	private readonly AsyncMessageBroker<User> m_messageBroker;

	public HUD(UserManager userManager)
	{
		HUDBehaviour prefab = ResourcePathAttribute.GetResource<HUDBehaviour>();
		var instance = UnityEngine.Object.Instantiate(prefab);
		
		instance.transform.Reset();

		m_messageBroker = new AsyncMessageBroker<User>();
		userManager.OnUpdated += m_messageBroker.Publish;

		instance.OnSetup(
			new HUDFieldParameter {
				Type = HUDType.STAR,
				FieldBinder = m_messageBroker.Subscribe().Select(user => user.Puzzle.ToString()),
				OnClick = null
			},
			new HUDFieldParameter {
				Type = HUDType.LIFE,
				FieldBinder = m_messageBroker.Subscribe().Select(user => user.GetLifeString()),
				OnClick = null
			},
			new HUDFieldParameter {
				Type = HUDType.COIN,
				FieldBinder = m_messageBroker.Subscribe().Select(user => user.Coin.ToString()),
				OnClick = null
			}
		);

		m_behaviour = instance;

		userManager.Update();
	}

	public Transform GetAttractorTarget(HUDType hudType) => m_behaviour.GetAttractorTarget(hudType);

	public void Show(params HUDType[] types)
	{
		m_behaviour.CachedGameObject.SetActive(true);
		m_behaviour.OnShow(types);
	}

	public void Hide()
	{
		m_behaviour.CachedGameObject.SetActive(false);
	}
}
