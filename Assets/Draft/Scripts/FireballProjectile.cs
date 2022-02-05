using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modules.Utilities;
using UniRx;

public class FireballProjectile : MonoBehaviour
{

    public float m_AimDistance = 2f;
    [SerializeField] private GameObject m_BulletPrefab;

    [SerializeField] private Transform m_SpawnTransform;
    [SerializeField] private CanvasGroup m_AimCanvas;
    private Vector3 m_AimPosition;

    void Start()
    {

    }

    private void OnEnable()
    {
        m_AimCanvas.LerpAlpha(300, 1).AddTo(this);
    }

    private void OnDisable()
    {
        m_AimCanvas.LerpAlpha(300, 0).AddTo(this);

    }
    void Update()
    {
        var centerScreenPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

        Ray ray = Camera.main.ScreenPointToRay(centerScreenPoint);
        m_AimPosition = ray.GetPoint(m_AimDistance);

        m_SpawnTransform.rotation = Quaternion.LookRotation((m_AimPosition - m_SpawnTransform.position).normalized);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            ObjectPoolingManager.CreateObject("Bullet", m_BulletPrefab, m_SpawnTransform.position, m_SpawnTransform.rotation);
        }
    }
}

