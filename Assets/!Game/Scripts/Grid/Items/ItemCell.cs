using System;
using UnityEngine;

public class ItemCell : MonoBehaviour
{
    #region Properties
    [field: Header("Settings.")]
    [field: SerializeField]
    public bool IsPrimary { get; set; } = false;
    [field: SerializeField]
    public Vector2 OriginalSize { get; set; } = Vector2.one;
    
    public IAnimation Animation { get; set; }
    #endregion

    #region Fields
    [SerializeField]
    private SpriteRenderer _spriteRenderer = null;
    #endregion
    
    #region Methods

    private void Awake()
    {
        Animation = (IAnimation)ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(gameObject, typeof(IAnimation));
    }

    #region Setters
    public void SetNewSizeWithModifier(float SizeModifier)
    {
        if (_spriteRenderer == null)
        {
            Component spriteRendererComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(gameObject, typeof(SpriteRenderer));
            
            if(spriteRendererComponent == null)
                return;
            
            _spriteRenderer = (SpriteRenderer)spriteRendererComponent;
        }
        
        float modifier = Mathf.Clamp01(SizeModifier);
        SpriteScaler.ScaleSpriteFromRenderer(_spriteRenderer, OriginalSize * modifier);
    }

    public void SetColor(Color NewColor)
    {
        if (_spriteRenderer == null)
        {
            Component spriteRendererComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(gameObject, typeof(SpriteRenderer));
            
            if(spriteRendererComponent == null)
                return;
            
            _spriteRenderer = (SpriteRenderer)spriteRendererComponent;
        }
        
        _spriteRenderer.color = NewColor;
    }
    #endregion

    #region Getters
    public Bounds GetOriginalBounds()
    {
        if (_spriteRenderer == null)
        {
            Component spriteRendererComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(gameObject, typeof(SpriteRenderer));
            
            if(spriteRendererComponent == null)
                return new Bounds();
            
            _spriteRenderer = (SpriteRenderer)spriteRendererComponent;
        }
        
        Vector3 center = _spriteRenderer.bounds.center;
        Vector3 size = new Vector3(OriginalSize.x, OriginalSize.y, _spriteRenderer.bounds.size.z);
        return new Bounds(center, size);
    }

    public Bounds GetSpriteBounds()
    {
        if (_spriteRenderer == null)
        {
            Component spriteRendererComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(gameObject, typeof(SpriteRenderer));
            
            if(spriteRendererComponent == null)
                return new Bounds();
            
            _spriteRenderer = (SpriteRenderer)spriteRendererComponent;
        }
        
        return _spriteRenderer.bounds;
    }
    #endregion
    #endregion
}
