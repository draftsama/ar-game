using System;
using System.Collections;
using System.Collections.Generic;
using Modules.Utilities;
using UniRx;
using UnityEngine;

public class SpawnMonsterManager : Singleton<SpawnMonsterManager>
{
    [SerializeField] private Transform m_SpawnTransform;
    [SerializeField] private Transform m_TargetTransform;
    [SerializeField] private GameObject m_MonsterPrefab;

    void Start()
    {
    }

    public void StartSpawn()
    {
        Observable.Timer(TimeSpan.FromMilliseconds(2000)).Subscribe(_ =>
        {
            CrateMonstor();
            Observable.Interval(TimeSpan.FromMilliseconds(7000)).Subscribe(_ =>
            {
                CrateMonstor();
            }).AddTo(this);
        }).AddTo(this);
    }

    void CrateMonstor()
    {
        var go = ObjectPoolingManager.CreateObject("Monster", m_MonsterPrefab, m_SpawnTransform.position,
            Quaternion.identity, null);
        go.transform.position = m_SpawnTransform.position;
        go.GetComponent<MonsterController>().Init(m_TargetTransform);
    }
}