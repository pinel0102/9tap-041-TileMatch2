using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlay : MonoBehaviour
{
    public ParticleSystem m_particle;

    public void Play()
    {
        m_particle.gameObject.SetActive(true);
        
        if (m_particle.isPlaying)
            m_particle.Stop();

        m_particle.Play();
    }

    public void Stop()
    {
        if (m_particle.isPlaying)        
            m_particle.Stop();

        m_particle.gameObject.SetActive(false);
    }
}
