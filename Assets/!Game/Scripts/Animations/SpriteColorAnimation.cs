using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SpriteColorAnimation : MonoBehaviour, IAnimation
{
    #region Fields
    [Header("References.")]
    [SerializeField]
    private List<SpriteRenderer> _renderers = new ();

    [Header("Settings.")] 
    [SerializeField] 
    private Color _forwardColor = Color.white;
    [SerializeField] 
    private float _forwardDuration = 0.25f;
    [SerializeField]
    private Ease _forwardEase = Ease.Linear;
    [Space(15f)]
    [SerializeField] 
    private Color _backwardColor = Color.black;
    [SerializeField] 
    private float _backwardDuration = 0.25f;
    [SerializeField]
    private Ease _backwardEase = Ease.Linear;
    
    private Sequence _sequence;
    #endregion
    
    #region Methods
    public void PlayForward() => PlayAnimation(_backwardColor, _forwardColor, _forwardDuration, _forwardEase);

    public void PlayBackward() => PlayAnimation( _forwardColor, _backwardColor, _backwardDuration, _backwardEase);

    private void PlayAnimation(Color startColor, Color targetColor, float duration, Ease ease)
    { 
        if(_renderers == null || _renderers.Count == 0)
            return;
        
        StopAnimation();
        
        _sequence = DOTween.Sequence();

        foreach (SpriteRenderer renderer in _renderers)
        {
            if(renderer == null)
                continue;
            
            renderer.color = startColor;
            Tween scaledTween = renderer.DOColor(targetColor, duration / TimeScaler.TimeScale);
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
