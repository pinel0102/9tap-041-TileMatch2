using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ParticleCallbackManager : MonoBehaviour
{
    [SerializeField] private ParticleCallback m_particleCallback;
    [SerializeField] private CallbackType m_callbackType;
    [SerializeField] private bool showLog;
    private Action<GameObject> m_onFinished;

#region Initialize

    public void InitCallback()
    {
        m_onFinished = null;

        if(!m_particleCallback)
            m_particleCallback = GetSubComponent();
        
        m_particleCallback?.InitCallback(ParticleFinished);
    }

    public void InitCallback(CallbackType callbackType = CallbackType.DESTROY, Action<GameObject> onFinished = null)
    {
        m_callbackType = callbackType;
        m_onFinished = onFinished;

        if(!m_particleCallback)
            m_particleCallback = GetSubComponent();
        
        m_particleCallback?.InitCallback(ParticleFinished);
    }

#endregion Initialize


#region Callback

    private void ParticleFinished()
    {
        if (showLog)
            Debug.Log(CodeManager.GetMethodName() + gameObject.name);

        switch(m_callbackType)
        {
            case CallbackType.DESTROY:
                Destroy(gameObject);
                break;
            case CallbackType.DISABLE:
                gameObject.SetActive(false);
                break;
            case CallbackType.CUSTOM:
                m_onFinished?.Invoke(gameObject);
                break;
        }
    }

#endregion Callback


#region SubComponent

    private ParticleCallback GetSubComponent()
    {
        var childList = GetComponentsInChildren<ParticleSystem>();
        
        if (childList.Length > 0)
        {
            for(int i=0; i < childList.Length; i++)
            {
                if (childList[i].TryGetComponent<ParticleCallback>(out var subComponent))
                    return subComponent;
            }

            return childList[0].gameObject.AddComponent<ParticleCallback>();
        }
        
        return null;
    }

#endregion SubComponent


#region Enum

    public enum CallbackType
    {
        DESTROY,
        DISABLE,
        NONE,
        CUSTOM
    }

#endregion Enum
}

