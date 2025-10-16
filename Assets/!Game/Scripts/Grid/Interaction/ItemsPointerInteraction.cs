using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class ItemsPointerInteraction : MonoBehaviour
{
    #region Fields

    [Header("References.")]
    [SerializeField]
    private Transform _interactableZonesParent;
    [Header("Settings."), Range(0f, 100f)] 
    [SerializeField]
    private float _dragSpeed = 25f;
    
    private List<IInteractableZone> _zones = new ();
    private IInteractableZone _pointerZone = null;
    private IInteractableZone _itemZone = null;
    
    private IInteractableItem _draggedItem = null;
    private Transform _draggedItemTransform = null;
    
    private Vector3 _pointerWorldPosition = Vector3.zero;
    private Vector3 _itemStartOffset = Vector3.zero;
    private bool _isDragging = false;
    private bool _isRotating = false;
    #endregion
    
    #region Methods
    #region Unity methods
    private void Awake()
    {
        GetInteractableZones();
        
        if(_zones.Count == 0)
            return;

        InputHandler.PointerPositionUpdate.Subscribe(position => PointerMovement(position)).AddTo(this);
        InputHandler.DragIsInProgress.Subscribe(state => DragItem(state)).AddTo(this);
        InputHandler.RotateIsInProgress.Subscribe(state => RotateItem(state)).AddTo(this);
    }
    #endregion

    #region Getters
    private void GetInteractableZones()
    {
        if(_interactableZonesParent == null)
            return;
        
        int childsCount = _interactableZonesParent.childCount;
        
        if(childsCount == 0)
            return;

        for (int i = 0; i < childsCount; i++)
        {
            Transform child = _interactableZonesParent.GetChild(i);
            Component interactableZoneComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(child.gameObject, typeof(IInteractableZone));
            
            if(interactableZoneComponent == null)
                continue;
            
            _zones.Add((IInteractableZone)interactableZoneComponent);
        }
    }
    #endregion

    #region Interaction
    private void PointerMovement(Vector2 screenPointerPosition)
    {
        _pointerWorldPosition = Camera.main.ScreenToWorldPoint(screenPointerPosition);
        Vector3 pointerPosition = _pointerWorldPosition;

        if(_zones.Count == 0)
            return;
        
        IEnumerable<IInteractableZone> pointerZones = _zones.Where(zone => IsPointerInZoneBounds(pointerPosition, zone));
        bool pointerInZone = pointerZones.Count() > 0;

        if (pointerInZone)
        {
            IInteractableZone currentPointerZone = pointerZones.First();

            if (currentPointerZone != _pointerZone)
                currentPointerZone.OnPointerEnter();
            
            _pointerZone = currentPointerZone;
        }
        else
        {
            if(_pointerZone != null)
                _pointerZone.OnPointerExit();
            
            _pointerZone = null;
        }

        if (_draggedItem == null)
        {
            if (_itemZone != null)
            {
                _itemZone.OnInteractableItemExit(null);
            }
            
            _itemZone = null;
            return;
        }
        
        IEnumerable<IInteractableZone> itemZones = _zones.Where(zone => IsItemInZoneBounds(_draggedItem, zone));
        bool itemInZone = itemZones.Count() > 0;

        if (itemInZone)
        {
            IInteractableZone currentItemZone = itemZones.First();

            if (currentItemZone != _itemZone)
            {
                if(_itemZone != null)
                    _itemZone.OnInteractableItemExit(_draggedItem);
                
                currentItemZone.OnInteractableItemEnter(_draggedItem);
            }
            
            _itemZone = currentItemZone;
            _draggedItem.EndZone = _itemZone;
        }
        else
        {
            if (_itemZone != null)
            {
                _itemZone.OnInteractableItemExit(_draggedItem);
                _draggedItem.EndZone = null;
            }
            
            _itemZone = null;
        }
    }

    private bool IsPointerInZoneBounds(Vector3 pointerPosition, IInteractableZone zone)
    {
        if(zone == null)
            return false;
        
        Bounds interactableZoneBounds = zone.GetBounds();
        pointerPosition.z = interactableZoneBounds.center.z;
        return interactableZoneBounds.Contains(pointerPosition);
    }

    private bool IsItemInZoneBounds(IInteractableItem item, IInteractableZone zone)
    {
        if(item == null || zone == null)
            return false;
        
        Bounds interactableZoneBounds = zone.GetBounds();
        Bounds[] itemBounds = item.GetCellsBounds();
        
        if(itemBounds == null || itemBounds.Length == 0)
            return false;

        bool intersects = false;
        
        foreach (Bounds bounds in itemBounds)
        {
            Bounds itemCellBounds = bounds;
            Vector3 itemCellBoundsCenter = itemCellBounds.center;
            itemCellBoundsCenter.z = interactableZoneBounds.center.z;
            itemCellBounds.center = itemCellBoundsCenter;
            
            intersects = interactableZoneBounds.Intersects(itemCellBounds);
            
            if(intersects)
                break;
        }
        
        return intersects;
    }

    private void DragItem(bool dragInProgress)
    {
        if (!dragInProgress)
        {
            if (_draggedItem != null)
            {
                if (_draggedItem.EndZone != null && _draggedItem.EndZone != _draggedItem.StartZone)
                {
                    _draggedItem.EndZone.TryToDropItemIntoZone(_draggedItem);
                }
                else if(_draggedItem.EndZone == null || _draggedItem.EndZone == _draggedItem.StartZone)
                {
                    _draggedItem.StartZone.TryToDropItemIntoZone(_draggedItem);
                }
                
                _draggedItem.OnDrop();
                _draggedItemTransform = null;
                _itemStartOffset = Vector3.zero;
                _draggedItem = null;
            }
            
            _isDragging = false;
            return;
        }
        
        if (!_isDragging)
        {
            if(_pointerZone == null)
                return;
            
            bool itemDragged = _pointerZone.TryToDragItemFromZone(_pointerWorldPosition, out IInteractableItem item);

            if (itemDragged)
            {
                if (item != null)
                {
                    _draggedItem = item;
                    _draggedItem.OnDrag();
                }
                else
                {
                    Debug.Log("Item null");
                    return;
                }

                if (!item.Active)
                {
                    Debug.Log("Item not active");
                    return;
                }
                
                _draggedItemTransform = _draggedItem.Parent;
                Vector3 startItemPosition = _draggedItemTransform.position;
                Vector3 pointerStartPosition = _pointerWorldPosition;
                pointerStartPosition.z = startItemPosition.z;
                _itemStartOffset = startItemPosition - pointerStartPosition;
            }
            
            _isDragging = true;
        }
        
        if(_draggedItemTransform == null)
            return;
        
        Vector3 currentItemPosition = _draggedItemTransform.position;
        Vector3 currentPointerPosition = _pointerWorldPosition;
        currentPointerPosition.z = currentItemPosition.z;
        Vector3 newPosition = currentPointerPosition + _itemStartOffset;
        _draggedItemTransform.position = Vector3.Lerp(currentItemPosition, newPosition, Time.deltaTime * _dragSpeed * TimeScaler.TimeScale);
    }

    private void RotateItem(bool rotateInProgress)
    {
        if (!rotateInProgress)
        {
            _isRotating = false;
            return;
        }

        if(_draggedItem == null || _draggedItemTransform == null)
            return;
        
        if (!_isRotating)
        {
            Transform itemTransform = _draggedItemTransform;
            Vector3 mouseWorld = _pointerWorldPosition;
            Vector3 itemPos = itemTransform.position;
            float angle = 90f;
            Vector3 offset = itemPos - mouseWorld;
            
            float radians = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            Vector3 rotatedOffset = new Vector3(
                offset.x * cos - offset.y * sin,
                offset.x * sin + offset.y * cos,
                offset.z
            );
            
            Vector3 newPosition = mouseWorld + rotatedOffset;
            itemTransform.position = newPosition;
            itemTransform.Rotate(Vector3.forward, angle);
            
            _draggedItem.UpdateResourceSprite();
            _isRotating = true;
        }
    }
    #endregion
    #endregion
}
