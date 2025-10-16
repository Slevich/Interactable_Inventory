using System;
using DG.Tweening;
using UnityEngine;

public class ResourcesPopup : MonoBehaviour
{
    #region Fields
    [Header("References.")] 
    [SerializeField]
    private Transform _container;
    [SerializeField]
    private SpriteRenderer _background;
    [SerializeField]
    private SpriteRenderer _resourceIcon;
    [SerializeField] 
    private TextMesh _plusSignText;
    [SerializeField]
    private TextMesh _amountText;

    [Header("Settings.")]
    [SerializeField]
    private Vector3 _startScale = Vector3.one;
    [SerializeField]
    private Vector3 _endScale = Vector3.one;
    [SerializeField] 
    private Color _spritesStartColor;
    [SerializeField] 
    private Color _spritesEndColor;
    [SerializeField]
    private Color _textColor;
    [SerializeField]
    private float _duration = 0.25f;
    [SerializeField]
    private Ease _ease = Ease.Linear;
    
    private Sequence _sequence;
    private float _currentTimeScale = 0f;
    private float _startDuration = 0f;
    #endregion

    #region Properties
    public float Height => _background != null ? _background.bounds.size.y : 0.5f;
    public float Width => _background != null ? _background.bounds.size.x : 0.5f;
    public ProductionSettings.ResourceType ResourceType { get; set; }
    #endregion

    #region Public Methods
    private void Start()
    {
        if(_container != null)
            _container.gameObject.SetActive(false);

        _currentTimeScale = TimeScaler.TimeScale;
        _startDuration = _duration;
    }

    public void UpdateResourceImage(Sprite NewSprite)
    {
        if(_resourceIcon == null)
            return;
        
        _resourceIcon.sprite = NewSprite;
    }

    public void UpdateResourceAmount(int Amount)
    {
        if (_amountText == null)
            return;
        
        _amountText.text = Amount.ToString();
    }

    public void PlayAnimation()
    {
        if(_container == null || _background == null || _resourceIcon == null || _plusSignText == null || _amountText == null)
            return;
        
        if(_sequence != null && _sequence.IsPlaying())
            _sequence.Kill();
        
        _duration = _startDuration / _currentTimeScale;
        _container.gameObject.SetActive(true);
        _container.localScale = _startScale;
        Tween scaleTween = _container.DOScale(_endScale, _duration).SetEase(_ease);

        _background.color = _spritesStartColor;
        Tween backgroundColorTween = _background.DOColor(_spritesEndColor, _duration).SetEase(_ease);
        
        _resourceIcon.color = _spritesStartColor;
        Tween resourceImageColorTween = _resourceIcon.DOColor(_spritesEndColor, _duration).SetEase(_ease);
        
        _plusSignText.color = _textColor;
        
        _amountText.color = _textColor;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Join(scaleTween);
        sequence.Join(backgroundColorTween);
        sequence.Join(resourceImageColorTween);
        sequence.OnComplete(() => OnComplete());
        sequence.Play();
    }

    private void OnComplete()
    {
        _container.gameObject.SetActive(false);

        if (TimeScaler.TimeScale != _currentTimeScale)
        {
            _currentTimeScale = TimeScaler.TimeScale;
            _duration = _startDuration / _currentTimeScale;
            PlayAnimation();
        }
    }
    
    private void OnDisable()
    {
        if(_sequence != null && _sequence.IsPlaying())
            _sequence.Kill();
    }
    #endregion
}
