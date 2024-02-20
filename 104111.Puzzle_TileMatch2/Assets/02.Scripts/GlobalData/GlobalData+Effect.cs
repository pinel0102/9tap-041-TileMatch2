using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using System;
using NineTap.Common;

public partial class GlobalData
{
    private IObjectPool<MissionCollectedFx> m_particlePool;

    public void ResetParticlePool()
    {
        for(int i = fragmentHome?.objectPool.childCount - 1 ?? -1; i >= 0; i--)
        {
            Destroy(fragmentHome.objectPool.GetChild(i).gameObject);
        }

        m_particlePool = new ObjectPool<MissionCollectedFx>(
			createFunc: () => {
				var item = Instantiate(ResourcePathAttribute.GetResource<MissionCollectedFx>());
				item.OnSetup();
				return item;
			},
			actionOnRelease: item => item.OnRelease()
		);
    }

    /// <summary>MainScene Only</summary>
    public void CreateEffect(string spriteName, string soundClip, Transform from, Transform to, float duration = 1f, Action onComplete = null, float sizeFrom = 70f, float sizeTo = 82f)
    {
        CreateEffect(m_particlePool, fragmentHome.objectPool, spriteName, soundClip, from.position, to.position, duration, onComplete, sizeFrom, sizeTo);
    }

    public void CreateEffect(IObjectPool<MissionCollectedFx> particlePool, Transform parent, string spriteName, string soundClip, Transform from, Transform to, float duration = 1f, Action onComplete = null, float sizeFrom = 70f, float sizeTo = 82f)
    {
        CreateEffect(particlePool, parent, spriteName, soundClip, from.position, to.position, duration, onComplete, sizeFrom, sizeTo);
    }

    public void CreateEffect(IObjectPool<MissionCollectedFx> particlePool, Transform parent, 
                            string spriteName, string soundClip, Vector2 from, Vector2 to, 
                            float duration = 1f, Action onComplete = null, float sizeFrom = 70f, float sizeTo = 82f)
    {
        MissionCollectedFx fx = particlePool.Get();
        fx.SetImage(spriteName);
        fx.CachedRectTransform.SetParentReset(parent, true);        
        fx.Play(from, to, duration, () => {
                soundManager?.PlayFx(soundClip);
                particlePool.Release(fx);
                onComplete?.Invoke();
            }, sizeFrom, sizeTo
        );
    }
}
