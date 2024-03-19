using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coffee.UIExtensions;

public partial class MainScene
{
    public UIParticle FXLayer;

    public void ClearFXLayer()
    {
        GlobalDefine.ClearChild(FXLayer.transform);
        FXLayer.RefreshParticles();
    }

#region WorldPosition

    public void LoadFX(string prefabPath, Vector3 worldPosition, UIParticle parentLayer)
    {
        GameObject ga = Instantiate(Resources.Load<GameObject>(prefabPath), parentLayer.transform);
        ga.transform.position = worldPosition;
        ga.GetComponent<ParticleCallbackManager>().InitCallback();
        parentLayer.RefreshParticles();
    }

    public void LoadFX(string prefabPath, Vector3 worldPosition, float localScale)
    {
        GameObject ga = Instantiate(Resources.Load<GameObject>(prefabPath), FXLayer.transform);
        ga.transform.position = worldPosition;
        ga.transform.SetLocalScale(localScale);
        ga.GetComponent<ParticleCallbackManager>().InitCallback();
        FXLayer.RefreshParticles();
    }

    public void LoadFX(string prefabPath, Vector3 worldPosition)
    {
        GameObject ga = Instantiate(Resources.Load<GameObject>(prefabPath), FXLayer.transform);
        ga.transform.position = worldPosition;
        ga.GetComponent<ParticleCallbackManager>().InitCallback();
        FXLayer.RefreshParticles();
    }

    public void LoadFX(BlockerType blockerType, int blockerICD, Vector3 worldPosition)
    {
        var(prefabExist, prefabPath) = GlobalDefine.GetBlockerFXPrefabPath(blockerType, blockerICD);
        if (prefabExist)
        {
            LoadFX(prefabPath, worldPosition);
        }
    }

#endregion WorldPosition


#region LocalPosition

    public void LoadFXLocal(string prefabPath, Vector3 localPosition, UIParticle parentLayer)
    {
        GameObject ga = Instantiate(Resources.Load<GameObject>(prefabPath), parentLayer.transform);
        ga.transform.localPosition = localPosition;
        ga.GetComponent<ParticleCallbackManager>().InitCallback();
        parentLayer.RefreshParticles();
    }

    public void LoadFXLocal(string prefabPath, Vector3 localPosition, float localScale)
    {
        GameObject ga = Instantiate(Resources.Load<GameObject>(prefabPath), FXLayer.transform);
        ga.transform.localPosition = localPosition;
        ga.transform.SetLocalScale(localScale);
        ga.GetComponent<ParticleCallbackManager>().InitCallback();
        FXLayer.RefreshParticles();
    }

    public void LoadFXLocal(string prefabPath, Vector3 localPosition)
    {
        GameObject ga = Instantiate(Resources.Load<GameObject>(prefabPath), FXLayer.transform);
        ga.transform.localPosition = localPosition;
        ga.GetComponent<ParticleCallbackManager>().InitCallback();
        FXLayer.RefreshParticles();
    }

    public void LoadFXLocal(BlockerType blockerType, int blockerICD, Vector3 localPosition)
    {
        var(prefabExist, prefabPath) = GlobalDefine.GetBlockerFXPrefabPath(blockerType, blockerICD);
        if (prefabExist)
        {
            LoadFXLocal(prefabPath, localPosition);
        }
    }

#endregion LocalPosition
}
