using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class ParticleDestroyManager: MonoBehaviour
{

    private ParticleSystem m_ParticleSystem;
    void Start()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();

        Observable.EveryUpdate().Subscribe(_ =>
        {
            if (!m_ParticleSystem.IsAlive(true))
            {
                ObjectPoolingManager.KillObject(gameObject);
            }
        }).AddTo(this);
    }

  
}
