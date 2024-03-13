using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayScene
{
    public void ClearFXLayer()
    {
        GlobalDefine.ClearChild(FXLayer.transform);
        FXLayer.RefreshParticles();
    }

    public void LoadFX(BlockerType blockerType, int blockerICD, Vector3 worldPosition)
    {
        var(prefabExist, prefabPath) = GlobalDefine.GetBlockerFXPrefabPath(blockerType, blockerICD);
        
        if (prefabExist)
        {
            //Debug.Log(CodeManager.GetMethodName() + string.Format("prefabPath : {0}", prefabPath));

            GameObject ga = Instantiate(Resources.Load<GameObject>(prefabPath), FXLayer.transform);
            ga.transform.position = worldPosition;
            ga.GetComponent<BlockerFX>().InitCallback(OnFXComplete);
            FXLayer.RefreshParticles();
        }

        //string soundPath = GlobalDefine.GetBlockerFXSoundPath(blockerType, blockerICD);
        //m_soundManager?.PlayFx(soundPath);
    }

   
    private void OnFXComplete(GameObject effectObject)
    {
        //Debug.Log(CodeManager.GetMethodName());
        Destroy(effectObject);
    }
}
