using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleCallback : MonoBehaviour
{
    private Action OnParticleFinished;

    public void InitCallback(Action onParticleFinished)
    {
        OnParticleFinished = onParticleFinished;

        var main = GetComponent<ParticleSystem>().main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    private void OnParticleSystemStopped()
    {
        OnParticleFinished?.Invoke();
    }
}
