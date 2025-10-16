using System;
using TMPro;
using UnityEngine;

public class CurrentTimeScaleUIUpdater : MonoBehaviour
{
    #region Fields
    [Header("References.")] 
    [SerializeField]
    private TimeScaler _timeScaler;
    [SerializeField]
    private TextMeshProUGUI _timeScaleText;
    #endregion
    
    #region Methods
    private void OnEnable()
    {
        if(_timeScaler != null)
            _timeScaler.TimeScaleChangedEvent.AddListener(UpdateTimeScaleText);
    }

    private void OnDisable()
    {
        if(_timeScaler != null)
            _timeScaler.TimeScaleChangedEvent.RemoveListener(UpdateTimeScaleText);
    }

    private void UpdateTimeScaleText(float currentTimeScale)
    {
        if(_timeScaleText == null)
            return;
        
        _timeScaleText.text = currentTimeScale.ToString();
    }
    #endregion
}
