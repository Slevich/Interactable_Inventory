using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridItemResourceSpriteUpdater : MonoBehaviour
{
    #region Fields
    [Header("References.")]
    [SerializeField]
    private ResourceSprite[] _resourceSprites = Array.Empty<ResourceSprite>();
    [SerializeField]
    private SpriteRenderer _backgroundSpriteRenderer = null;
    [SerializeField]
    private SpriteRenderer _iconSpriteRenderer = null;
    [SerializeField]
    private ResourcesPopup _popupPrefab = null;
    
    private List<ResourcesPopup> _popups = new ();
    #endregion

    #region Methods
    public void UpdateSprite(ProductionSettings.ResourceType Resource)
    {
        ResourceSprite resourceSprite = _resourceSprites.FirstOrDefault(resource => resource.Resource == Resource);
        
        if(resourceSprite == null)
            return;
        
        if (_iconSpriteRenderer != null)
            _iconSpriteRenderer.sprite = resourceSprite.Sprite;
    }

    public void PlaceSpriteOnCell(ItemCell Cell)
    {
        if(Cell == null)
            return;

        Bounds cellBounds = Cell.GetSpriteBounds();
        Vector3 resourceSpritePosition = cellBounds.center + (Vector3.right * cellBounds.extents.x * 0.8f) + (Vector3.up * cellBounds.extents.y * 0.8f);
        transform.position = cellBounds.center;
        transform.rotation = Quaternion.identity;

        if (_backgroundSpriteRenderer != null)
        {
            SpriteScaler.ScaleSpriteFromRenderer(_backgroundSpriteRenderer, cellBounds.size * 0.75f);
            _backgroundSpriteRenderer.transform.position = resourceSpritePosition;
        }

        if (_iconSpriteRenderer != null)
        {
            SpriteScaler.ScaleSpriteFromRenderer(_iconSpriteRenderer, cellBounds.size * 0.55f);
            _iconSpriteRenderer.transform.position = resourceSpritePosition;
        }
    }

    public void PreparePopups(ProductionSettings.ResourceType[] Resources)
    {
        if(Resources == null || Resources.Length == 0)
            return;
        
        if(Resources.Length == _popups.Count)
            return;

        if (_popupPrefab == null)
            return;
        
        int difference = Resources.Length - _popups.Count;

        for (int i = 0; i < difference; i++)
        {
            ResourcesPopup popup = Instantiate(_popupPrefab, transform);
            popup.transform.localPosition = Vector3.zero;
            _popups.Add(popup);
        }

        float popupBackgroundYExtents = _popups.First().Height / 2;
        bool direction = false;
        
        for (int i = 0; i < _popups.Count; i++)
        {
            ResourcesPopup popup = _popups[i];
            ResourceSprite resourceSprite = _resourceSprites.FirstOrDefault(resource => resource.Resource == Resources[i]);
            popup.ResourceType = Resources[i];
            popup.UpdateResourceImage(resourceSprite.Sprite);
            
            if(_popups.Count <= 1)
                return;

            if (direction)
            {
                popup.transform.position = transform.position + Vector3.up * popupBackgroundYExtents;
            }
            else
            {
                popup.transform.position = transform.position - Vector3.up * popupBackgroundYExtents;
            }
            
            direction = !direction;
        }
    }
    
    public void PlayPopupAnimation()
    {
        if(_popups.Count == 0)
            return;

        foreach (ResourcesPopup popup in _popups)
        {
            popup.PlayAnimation();
        }
    }

    public void UpdateResourceAmount(ProductionSettings.ResourceType Resource, int Amount)
    {
        if(_popups.Count == 0)
            return;
        
        ResourcesPopup popup = _popups.First(popup => popup.ResourceType == Resource);
        
        if(popup == null)
            return;
        
        popup.UpdateResourceAmount(Amount);
    }
    #endregion
}

[Serializable]
public class ResourceSprite
{
    [field: SerializeField]
    public ProductionSettings.ResourceType Resource { get; private set; } = ProductionSettings.ResourceType.Wheat;
    [field: SerializeField]
    public Sprite Sprite { get; private set; }
}