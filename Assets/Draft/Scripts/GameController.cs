using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UniRx;
public class GameController : MonoBehaviour
{

    [SerializeField] private ARPlace m_ARPlace;
    [SerializeField] private FireballProjectile m_FireballProjectile;

    private enum GameState
    {
        FindPlane, GameRunning, End
    }
    void Start()
    {
        Application.targetFrameRate = 60;
        
        m_ARPlace.OnStateUpdateAsObservable().Subscribe(_state =>
        {

            Debug.Log(_state);
            switch (_state)
            {
                case ARPlace.State.FindPlane:
                    m_FireballProjectile.gameObject.SetActive(false);

                    break;

                case ARPlace.State.PlaneDetect:

                    break;
                case ARPlace.State.Completed:
                    m_FireballProjectile.gameObject.SetActive(true);
                    break;

            }
        }).AddTo(this);
        m_ARPlace.Init();

    }



}
