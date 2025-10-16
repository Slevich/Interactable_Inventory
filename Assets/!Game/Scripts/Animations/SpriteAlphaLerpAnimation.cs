using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SpriteAlphaLerpAnimation : MonoBehaviour, IAnimation
{
    #region Fields

    [Header("References.")]
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [Header("Settings.")]
    [SerializeField]
    private float _speed = 0.2f;
    [SerializeField]
    private float _targetAlpha = 0.5f;

    private float _startAlpha = 0f;
    private CancellationTokenSource _tokenSource = new CancellationTokenSource();
    private bool _isPlaying = false;
    private float _startSpeed = 0f;
    #endregion

    #region Methods
    private void Awake()
    {
        if(_spriteRenderer != null)
            _startAlpha = _spriteRenderer.color.a;
        
        _startSpeed = _speed;
    }

    public void ModifySpeed(float Modifier)
    {
        _speed *= Modifier;
    }

    public void ResetSpeed()
    {
        _speed = _startSpeed;
    }
    
    public async void PlayForward()
    {
        if(_spriteRenderer == null)
            return;
        
        if(_isPlaying)
            return;
        
        if(_speed == 0)
            return;
        
        _isPlaying = true;
        
        if(_tokenSource == null || _tokenSource.IsCancellationRequested)
            _tokenSource =  new CancellationTokenSource();

        bool reverse = _startAlpha > _targetAlpha;
        float targetAlpha = _targetAlpha;

        while (_isPlaying && (_tokenSource != null && !_tokenSource.IsCancellationRequested))
        {
            try
            {
                Color currentColor = _spriteRenderer.color;
                float currentAlpha = currentColor.a;

                float step = Time.deltaTime * _speed * TimeScaler.TimeScale;

                if (!reverse)
                {
                    currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, step);

                    if (Mathf.Approximately(currentAlpha, targetAlpha))
                    {
                        reverse = true;
                        targetAlpha = _startAlpha;
                    }
                }
                else
                {
                    currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, step);

                    if (Mathf.Approximately(currentAlpha, targetAlpha))
                    {
                        reverse = false;
                        targetAlpha = _targetAlpha;
                    }
                }

                currentColor.a = currentAlpha;
                _spriteRenderer.color = currentColor;
                
                await UniTask.WaitForEndOfFrame(cancellationToken: _tokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Dispose();
                    _tokenSource = null;
                }
                
                StopAnimation();
            }
        }
    }

    public void PlayBackward(){}
    
    public void StopAnimation()
    {
        if(!_isPlaying)
            return;
        
        _isPlaying = false;
        
        if(_tokenSource != null && !_tokenSource.IsCancellationRequested)
            _tokenSource.Cancel();
        
        if(_spriteRenderer == null)
            return;

        Color currentColor = _spriteRenderer.color;
        currentColor.a = _startAlpha;
        _spriteRenderer.color = currentColor;
    }

    private void OnDisable()
    {
        if(_tokenSource != null && !_tokenSource.IsCancellationRequested)
            _tokenSource.Cancel();
        
        _isPlaying = false;
    }
    #endregion
}
