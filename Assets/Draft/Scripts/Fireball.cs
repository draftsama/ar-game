using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class Fireball : MonoBehaviour, IPoolObjectEvent
{

    public float m_Speed = 5f;
    public int m_TimeLife = 5000;

    private Rigidbody m_Rigidbody;

    public void OnEndObject()
    {
    }

    public void OnStartObject()
    {
        m_Rigidbody.velocity = transform.forward * m_Speed;

        Observable.Timer(TimeSpan.FromMilliseconds(m_TimeLife)).Subscribe(_ => ObjectPoolingManager.KillObject(gameObject)).AddTo(this);
    }

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    void Start()
    {

    }

}
