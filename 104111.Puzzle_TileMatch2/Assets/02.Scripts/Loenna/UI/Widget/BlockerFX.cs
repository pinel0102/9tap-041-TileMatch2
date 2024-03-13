using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BlockerFX : MonoBehaviour
{
    public List<ParticleCallback> particleCallback;
    private Action<GameObject> OnAllParticleFinished;

    private int particleCount => particleCallback.Count;
    private int finishedCount;

    public void InitCallback(Action<GameObject> onAllParticleFinished)
    {   
        finishedCount = 0;

        OnAllParticleFinished += onAllParticleFinished;
        particleCallback.ForEach(item => {
            item.InitCallback(ParticleFinished);
        });
    }

    private void ParticleFinished(GameObject particleObject)
    {
        finishedCount++;
        if (finishedCount >= particleCount)
            AllParticleFinished();
    }

    private void AllParticleFinished()
    {
        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0}", gameObject.name));
        OnAllParticleFinished?.Invoke(gameObject);
    }
}
