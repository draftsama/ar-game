using System;
using System.Collections;
using System.Collections.Generic;
using Modules.Utilities;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [SerializeField] private Transform m_Target;
    [SerializeField] private float m_RotateSpeed = 3f;
    [SerializeField] private float m_MoveSpeed = 3f;
    [SerializeField] private float m_MinFollowDistance = 10f;
    [SerializeField] private float m_MaxFollowDistance = 100f;
    [SerializeField] private float m_Health = 100f;
    [SerializeField] private GameObject m_HitFXPrefab;
    [SerializeField] private Animator m_Animator;
   
    private Transform _Transform;
    private Vector3 _SpectifyAxis = new Vector3(1, 0, 1);
    private Vector3 _MovePostion;
    private Renderer[] _Renderers;
    private float _DissolveProgress = 1;
    private Collider _Collider;
    public enum AnimationState
    {
        Idle,
        Run,
        Attack,
        Die
    }

    [SerializeField] private AnimationState _AnimationState;

    private IDisposable _UpdateDisposable;
    private IDisposable _AnimatorDisposable;

    void Awake()
    {
        _Transform = transform;
        _Renderers = _Transform.GetComponentsInChildren<Renderer>();
        _Collider = GetComponent<Collider>();
    }

    private void Start()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (m_Health <= 0) return;
        ContactPoint contact = collision.contacts[0];
        var fireball = collision.gameObject.GetComponent<Fireball>();
        if (fireball != null)
        {
            ObjectPoolingManager.CreateObject("HitFx", m_HitFXPrefab, contact.point, Quaternion.identity,
                null);
            ObjectPoolingManager.KillObject(fireball.gameObject);
            m_Health -= 20;
        }
    }

    public void Init(Transform _target)
    {
        Debug.Log("init");

        m_Target = _target;
        _Collider.enabled = true;
        _AnimationState = AnimationState.Idle;
        m_Health = 100;
        _DissolveProgress = 1;
        m_Animator.Rebind();
        m_Animator.Update(0);
        _MovePostion = _Transform.position;
        for (int i = 0; i < _Renderers.Length; i++)
        {
            _Renderers[i].material.SetFloat("_Progress", _DissolveProgress);
        }

        var observer = m_Animator.GetBehaviour<ObservableStateMachineTrigger>();
        _AnimatorDisposable?.Dispose();
        _AnimatorDisposable = observer.OnStateUpdateAsObservable().Subscribe(_ =>
        {
            if (_.StateInfo.normalizedTime >= _.StateInfo.length && _DissolveProgress >= 0)
            {
                _DissolveProgress -= Time.deltaTime * 0.5f;
                for (int i = 0; i < _Renderers.Length; i++)
                {
                    _Renderers[i].material.SetFloat("_Progress", _DissolveProgress);
                }

                if (_DissolveProgress <= 0)
                {
                    _AnimatorDisposable?.Dispose();

                    ObjectPoolingManager.KillObject(gameObject);
                }
            }
        }).AddTo(this);

        _UpdateDisposable?.Dispose();

        _UpdateDisposable = Observable.EveryUpdate().Subscribe(_ => Running()).AddTo(this);
    }

    void Running()
    {
        if (m_Health <= 0)
        {
            if (_AnimationState != AnimationState.Die)
            {
                _AnimationState = AnimationState.Die;
                m_Animator.SetTrigger("Die");
            }

            _Collider.enabled = false;
            _UpdateDisposable?.Dispose();
            return;
        }

        var position = m_Target.position.ToSpectifyAxis(_SpectifyAxis);
        var targetPosition = _Transform.position.ToSpectifyAxis(_SpectifyAxis);


        var distance = (position - targetPosition).sqrMagnitude;

        if (distance >= m_MinFollowDistance && distance <= m_MaxFollowDistance)
        {
            _MovePostion -= (targetPosition - position).normalized * m_MoveSpeed * Time.deltaTime;
            _Transform.position = _MovePostion;
            var rot = Quaternion.LookRotation(position -
                                              targetPosition);

            _Transform.localRotation = Quaternion.Slerp(_Transform.localRotation, rot, m_RotateSpeed * Time.deltaTime);
            if (_AnimationState != AnimationState.Run)
            {
                _AnimationState = AnimationState.Run;
                m_Animator.SetTrigger("Run");
            }
        }
        else if (distance < m_MinFollowDistance)
        {
            if (_AnimationState != AnimationState.Attack)
            {
                _AnimationState = AnimationState.Attack;
                m_Animator.SetTrigger("Attack");
            }
        }
        else
        {
            if (_AnimationState != AnimationState.Idle)
            {
                _AnimationState = AnimationState.Idle;
                m_Animator.SetTrigger("Idle");
            }
        }
    }
}