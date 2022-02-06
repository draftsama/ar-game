using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UniRx;
using System;

[RequireComponent(typeof(ARRaycastManager))]
public class ARPlace : MonoBehaviour
{

    [SerializeField] private TrackableType m_TrackableTypeRequest = TrackableType.Planes;
    [SerializeField] private GameObject m_PlaneObj;
    [SerializeField] private GameObject m_IndicatorObj;
    [SerializeField] private TextMeshProUGUI m_ARPlaceText;


    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    private ARRaycastManager _RaycastManager;
    private Vector2 _ScreenCenterPosition;
    private Transform _CameraTransform;
    private Pose _CurrentPose;

    public enum State
    {
        None, FindPlane, PlaneDetect, Completed
    }
    private State _State;
    private IDisposable _DisposableUpdate;
    private Action<State> _StateUpdate;
    void Awake()
    {
        _RaycastManager = GetComponent<ARRaycastManager>();
        _CameraTransform = Camera.main.transform;
    }



    void Start()
    {

    }

    private void SetState(State _state)
    {
        _State = _state;
        _StateUpdate?.Invoke(_state);

    }
    private void Processing()
    {

        if (_RaycastManager.Raycast(_ScreenCenterPosition, s_Hits, m_TrackableTypeRequest))
        {
            _CurrentPose = s_Hits[0].pose;
            m_IndicatorObj.SetActive(true);

            // Ray ray = Camera.main.ScreenPointToRay(_ScreenCenterPosition);
            // float drawPlaneHeight = _CurrentPose.position.y;
            // float dstToDrawPlane = (drawPlaneHeight - ray.origin.y) / ray.direction.y;
            // m_PlaneTransform.position = new Vector3(0, ray.GetPoint(dstToDrawPlane).y, 0);
            SetState(State.PlaneDetect);
        }
        if (_State == State.FindPlane)
        {
            m_ARPlaceText.text = "Look around floor.";
        }
        else if (_State == State.PlaneDetect)
        {
            m_ARPlaceText.text = "Tap to screen for select this place";

            Ray ray = Camera.main.ScreenPointToRay(_ScreenCenterPosition);
            float drawPlaneHeight = _CurrentPose.position.y;
            float dstToDrawPlane = (drawPlaneHeight - ray.origin.y) / ray.direction.y;
            m_IndicatorObj.transform.position = ray.GetPoint(dstToDrawPlane);


            var selfPos = new Vector3(m_IndicatorObj.transform.position.x, 0, m_IndicatorObj.transform.position.z);
            var targetPos = new Vector3(_CameraTransform.position.x, 0, _CameraTransform.position.z);
            m_IndicatorObj.transform.rotation = Quaternion.LookRotation(targetPos - selfPos);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                SelectPlanePosition();
            }

        }


    }

    public void SelectPlanePosition()
    {
        if (_State == State.PlaneDetect)
        {
            m_PlaneObj.SetActive(true);
            m_IndicatorObj.SetActive(false);

            m_PlaneObj.transform.position = m_IndicatorObj.transform.position;
            m_PlaneObj.transform.rotation = m_IndicatorObj.transform.rotation;
            SetState(State.Completed);
            _DisposableUpdate?.Dispose();
            m_ARPlaceText.text = string.Empty;

            SpawnMonsterManager.Instance.StartSpawn();
        }

    }

    public void Init()
    {
        _ScreenCenterPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        m_PlaneObj.SetActive(false);
        m_IndicatorObj.SetActive(false);
        _DisposableUpdate?.Dispose();
        SetState(State.FindPlane);

        _DisposableUpdate = Observable.EveryUpdate().Subscribe(_ => Processing()).AddTo(this);

    }

    public IObservable<State> OnStateUpdateAsObservable()
    {
        return Observable.FromEvent<State>(
            _event => _StateUpdate += _event,
        _event => _StateUpdate -= _event);
    }

}
