using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coffee.UIExtensions;

public class UIParticleWidget : MonoBehaviour
{
    //public UIParticle m_UIParticle;
    public List<GameObject> FXObjectList = new List<GameObject>();
    //public bool isPaused => m_UIParticle.isPaused;

    public void Initialize()
    {
        StopAllEffects();
    }

    public void StopAllEffects()
    {
        //m_UIParticle.Stop();

        for(int i=0; i < FXObjectList.Count; i++)
        {
            FXObjectList[i].SetActive(false);
        }
    }

    public void PlayEffect(BlockerType type, int newICD)
    {
        StopAllEffects();

        int index = GetEffectIndex(type, newICD);
        if (index >= 0 && index < FXObjectList.Count)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", type, newICD));
            FXObjectList[index].SetActive(true);
        }
    }

    private int GetEffectIndex(BlockerType type, int newICD)
    {
        switch(type)
        {
            case BlockerType.Glue_Right: return 0;
            case BlockerType.Bush: return 1;
            case BlockerType.Chain: return 2;
            case BlockerType.Jelly: 
                switch(newICD)
                {
                    case 2: return 3;
                    case 1: return 4;
                    case 0: return 5;
                    default: return -1;
                }
            case BlockerType.Suitcase: //return 6;
            default: return -1;
        }
    }
}
