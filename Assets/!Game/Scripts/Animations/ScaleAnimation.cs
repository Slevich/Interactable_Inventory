using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ScaleAnimation : MonoBehaviour, IAnimation
{
    #region Fields
    [Header("References.")]
    [SerializeField]
    private List<Transform> _scaledTransforms = new ();

    [Header("Settings.")] 
    [SerializeField] 
    private Vector3 _forwardTarget = Vector3.one;
    [SerializeField] 
    private float _forwardDuration = 0.25f;
    [SerializeField]
    private Ease _forwardEase = Ease.Linear;
    [Space(15f)]
    [SerializeField] 
    private Vector3 _backwardTarget = Vector3.zero;
    [SerializeField] 
    private float _backwardDuration = 0.25f;
    [SerializeField]
    private Ease _backwardEase = Ease.Linear;
    [Space(15f)]
    [SerializeField]
    private bool _affectChildren = true;
    
    private Sequence _sequence;
    #endregion
    
    #region Properties
    public bool AffectChildren {get => _affectChildren; set => _affectChildren = value; }
    #endregion
    
    #region Methods
    public void PlayForward() => PlayAnimation(_backwardTarget, _forwardTarget, _forwardDuration, _forwardEase);

    public void PlayBackward() => PlayAnimation( _forwardTarget, _backwardTarget, _backwardDuration, _backwardEase);

    private void PlayAnimation(Vector3 startScale, Vector3 targetScale, float duration, Ease ease)
    { 
        if(_scaledTransforms == null)
            return;

        if (_scaledTransforms.Count == 0)
        {
            if(!_affectChildren)
                return;
            
            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                _scaledTransforms.Add(child);
            }
        }
        
        StopAnimation();
        
        _sequence = DOTween.Sequence();

        foreach (Transform scaledTransform in _scaledTransforms)
        {
            if(scaledTransform == null)
                continue;
            
            scaledTransform.localScale = startScale;
            Tween scaledTween = scaledTransform.DOScale(targetScale, duration / TimeScaler.TimeScale);
            scaledTween.SetEase(ease);
            _sequence.Join(scaledTween);
        }
        
        _sequence.Play();
    }

    public void StopAnimation()
    {
        if(_sequence == null || !_sequence.IsPlaying())
            return;
        
        _sequence.Kill();
    }
    
    public void ModifySpeed(float Modifier){}
    public void ResetSpeed(){}
    #endregion
}
