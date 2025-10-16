using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GridItemsDestroyZone : MonoBehaviour, IInteractableZone
{
    #region Fields
    [Header("References.")] 
    [SerializeField]
    private Transform _animationsParent;
    [SerializeField] 
    private ResourceProduction _production;
    [Header("Settings.")] 
    [SerializeField, Range(0f, 10f)]
    private float _width = 1f;
    [SerializeField, Range(0f, 10f)]
    private float _height = 1f;
    [SerializeField, Range(0f, 100f)] 
    private float _itemDropSpeed = 20f;
    [SerializeField] 
    private List<ResourceAmount> _destroyCost = new();
    
    
    private List<IAnimation> _animations = new ();
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private bool _hasResources = false;
    private bool _enter = false;
    #endregion
    
    #region Properties
    #endregion
    
    #region Methods

    #region Unity methods

    private void OnEnable()
    {
        if(_production != null)
            _production.OnResourcesChangedEvent.AddListener(HasResources);
    }

    private void OnDisable()
    {
        if(_production != null)
            _production.OnResourcesChangedEvent.RemoveListener(HasResources);
    }

    #endregion
    
    public string GetName() => "GridItemsDestroyZone";

    private void HasResources(ResourceAmount[] CurrentResources)
    {
        if(_destroyCost == null || _destroyCost.Count == 0)
            return;
        
        if(CurrentResources == null || CurrentResources.Length == 0)
            return;

        int matches = 0;

        foreach (ResourceAmount resourceAmount in _destroyCost)
        {
            int amount = resourceAmount.CurrentAmount;
            ProductionSettings.ResourceType resourceType = resourceAmount.ResourceType;
            
            if(CurrentResources.First(resource => resource.ResourceType == resourceType).CurrentAmount >= amount)
                matches++;
        }
        
        _hasResources = matches == _destroyCost.Count;
    }

    public Bounds GetBounds()
    {
        Vector3 center = transform.position;
        return new Bounds(center, new Vector3(_width, _height, 0));
    }

    public bool TryToDragItemFromZone(Vector3 PointerPosition, out IInteractableItem DraggedItem)
    {
        DraggedItem = null;
        return false;
    }

    public async void TryToDropItemIntoZone(IInteractableItem DroppedItem)
    {
        if(DroppedItem == null)
            return;

        if (_hasResources)
        {
            if (_production != null && _destroyCost != null && _destroyCost.Count > 0)
            {
                foreach(ResourceAmount resourceAmount in _destroyCost)
                    _production.CurrentResources.First(resource => resource.ResourceType == resourceAmount.ResourceType).DecrementAmount(resourceAmount.CurrentAmount);
            }
            
            OnInteractableItemExit(DroppedItem);
            Vector3 targetPosition = transform.position;
            await LerpItemIntoZone(DroppedItem, targetPosition);
            DroppedItem.Destroy();
        }
        else
        {
            DroppedItem.StartZone.TryToDropItemIntoZone(DroppedItem);
        }
    }

    private async UniTask LerpItemIntoZone(IInteractableItem Item, Vector3 TargetPosition)
    {
        if(_cancellationTokenSource == null || (_cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested))
            _cancellationTokenSource = new CancellationTokenSource();

        Transform interactableItemTransform = Item.Parent;
        
        if(interactableItemTransform == null)
            return;
        
        while (Vector3.Distance(interactableItemTransform.position, TargetPosition) > 0.1f)
        {
            try
            {
                Vector3 currentPosition = interactableItemTransform.position;
                Vector3 direction = (TargetPosition - currentPosition).normalized;
                interactableItemTransform.position += direction * Time.deltaTime * _itemDropSpeed * TimeScaler.TimeScale;
                
                await UniTask.WaitForEndOfFrame(cancellationToken: _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
                
                return;
            }
        }
        
        interactableItemTransform.position = TargetPosition;
    }
    
    public void OnPointerEnter() { }

    public void OnPointerExit() { }

    public void OnInteractableItemEnter(IInteractableItem Item)
    {
        if(!_hasResources)
            return;
        
        _enter = true;
        
        if (_animations.Count == 0)
        {
            if(_animationsParent == null)
                return;
            
            Component[] animationComponents = ComponentsSearcher.GetComponentsOfTypeFromObjectAndAllChildren(_animationsParent.gameObject, typeof(IAnimation));

            if (animationComponents.Length > 0)
            {
                foreach (Component animationComponent in animationComponents)
                {
                    _animations.Add((IAnimation)animationComponent);
                }
            }
        }

        if (_animations.Count > 0)
        {
            foreach (IAnimation animation in _animations)
            {
                animation.PlayForward();
            }
        }
    }

    public void OnInteractableItemExit(IInteractableItem Item)
    {
        if(!_enter)
            return;
        
        if (_animations.Count > 0)
        {
            foreach (IAnimation animation in _animations)
            {
                animation.PlayBackward();
            }
        }
        
        _enter = false;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Bounds bounds = GetBounds();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
