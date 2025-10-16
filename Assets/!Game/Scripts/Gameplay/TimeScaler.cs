using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class TimeScaler : MonoBehaviour
{
    #region Fields
    private bool _timeScaleChanged = false;
    [SerializeField, ReadOnly]
    private float _timeScale = 1f;
    #endregion

    #region Properties
    public static float TimeScale { get; private set; } = 1f;
    public UnityEvent<float> TimeScaleChangedEvent { get; set; } = new UnityEvent<float>();
    #endregion

    #region Methods

    #region Unity methods
    private void Awake()
    {
        TimeScale = 1f;
    }

    private void OnEnable()
    {
        InputHandler.TimeScaleX1AccelerationAction.started += delegate { SetTimeScaleByInput(1f);};
        InputHandler.TimeScaleX2AccelerationAction.started += delegate { SetTimeScaleByInput(2f);};
        InputHandler.TimeScaleX3AccelerationAction.started += delegate { SetTimeScaleByInput(3f);};
    }

    private void OnDisable()
    {
        InputHandler.TimeScaleX1AccelerationAction.started -= delegate { SetTimeScaleByInput(1f);};
        InputHandler.TimeScaleX2AccelerationAction.started -= delegate { SetTimeScaleByInput(2f);};
        InputHandler.TimeScaleX3AccelerationAction.started -= delegate { SetTimeScaleByInput(3f);};
    }
    #endregion

    private void SetTimeScaleByInput(float timeScale)
    {
        TimeScale = timeScale;
        _timeScaleChanged = true;
        _timeScale = timeScale;
        TimeScaleChangedEvent?.Invoke(timeScale);
    }
    #endregion
}