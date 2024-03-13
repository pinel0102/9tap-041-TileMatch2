using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ParticleCallback : MonoBehaviour
{
    private Action<GameObject> OnParticleFinished;

    private void Start()
    {   
        var main = GetComponent<ParticleSystem>().main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    public void InitCallback(Action<GameObject> onParticleFinished)
    {
        OnParticleFinished += onParticleFinished;
    }

    private void OnParticleSystemStopped()
    {
        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0}", gameObject.name));
        OnParticleFinished?.Invoke(gameObject);
    }
}
