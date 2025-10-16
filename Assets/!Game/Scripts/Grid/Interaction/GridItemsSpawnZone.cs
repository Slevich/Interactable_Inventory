using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridItemsSpawnZone : MonoBehaviour, IInteractableZone
{
    #region Fields
    [Header("References.")] 
    [SerializeField]
    private Transform _itemsParent;
    [SerializeField]
    private GridItemResourceSpriteUpdater _resourceSpriteUpdaterPrefab;
    [Space(15f)]
    [SerializeField]
    private List<GridItemsSpawnSetting> _startInstances = new();
    [SerializeField] 
    private List<GridItem> _itemsInstances = new();

    [Header("Zone settings.")] 
    [SerializeField, Range(0f, 10f)]
    private float _width = 1f;
    [SerializeField, Range(0f, 10f)]
    private float _height = 1f;
    [SerializeField, Range(0f, 100f)] 
    private float _itemReturnSpeed = 10f;
    
    [Header("Spawn settings.")]
    [SerializeField, Range(0f, 1f)]
    private float _waitTimeBetweenSpawns = 0.05f;
    [SerializeField, Range(1, 100)] 
    private int _maxAttemptsToPlaceItem = 25;
    

    private GridItem[] _prefabs = Array.Empty<GridItem>();

    private float _minX = 0f;
    private float _maxX = 0f;
    private float _minY = 0f;
    private float _maxY = 0f;
    private Vector3 _itemPosition = Vector3.zero;
    
    private static readonly int maxAttempts = 20;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    #endregion
    
    #region Methods

    #region Unity methods
    private void Awake()
    {
        GetBorderValues();
        PrepareInstances();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(_width, _height, 0f));
    }

    private void OnDisable()
    {
        if(_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            _cancellationTokenSource.Cancel();
    }
    #endregion
    
    #region Spawn instances
    private async void PrepareInstances()
    {
        if(_startInstances == null ||  _startInstances.Count == 0)
            return;
        
        foreach (GridItemsSpawnSetting spawnSetting in _startInstances)
        {
            GridItem prefab = spawnSetting.Prefab;
            
            if(prefab == null)
                continue;
            
            int amount = spawnSetting.Amount;
            
            if(amount <= 0)
                continue;

            for (int i = 0; i < amount; i++)
            {
                await CreateItemInstanceAsync(prefab);
            }
        }
        
        GetPrefabs();
    }
    
    private async UniTask CreateItemInstanceAsync(GridItem prefab)
    {
        if(_cancellationTokenSource == null || (_cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested))
            _cancellationTokenSource = new CancellationTokenSource();
        
        if(this == null)
            return;
        
        GridItem instance = Instantiate(prefab, transform);
        instance.transform.localPosition = Vector3.zero;

        try
        {
            bool placed = await TryToFindPlaceForItem(instance);

            if (placed)
            {
                instance.transform.position = _itemPosition;
                instance.StartPosition = _itemPosition;
                instance.transform.SetParent(_itemsParent);
                instance.OnDestroyEvent.AddListener(SpawnRandomItem);
                
                Array resources = Enum.GetValues(typeof(ProductionSettings.ResourceType));
                int randomResourceIndex = Random.Range(0, resources.Length);
                instance.ResourceType = (ProductionSettings.ResourceType)resources.GetValue(randomResourceIndex);

                if (_resourceSpriteUpdaterPrefab != null)
                {
                    GridItemResourceSpriteUpdater resourceSpriteUpdater = Instantiate(_resourceSpriteUpdaterPrefab);
                    resourceSpriteUpdater.transform.SetParent(instance.transform);
                    resourceSpriteUpdater.transform.localPosition = Vector3.zero;
                    instance.ResourceSpriteUpdater = resourceSpriteUpdater;
                }
                
                instance.Initialize();
                instance.StartZone = (IInteractableZone)this;
                _itemsInstances.Add(instance);
            }
            else
            {
                Destroy(instance.gameObject);
            }
            
            await UniTask.WaitForSeconds(_waitTimeBetweenSpawns, cancellationToken: _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException exception)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }
    
    public async void SpawnRandomItem()
    {
        if(_prefabs == null || _prefabs.Length == 0)
            return;
        
        int randomIndex = Random.Range(0, _prefabs.Length);
        await CreateItemInstanceAsync(_prefabs[randomIndex]);
    }
    #endregion
    
    #region Getters
    private void GetBorderValues()
    {
        _minX = transform.position.x - (_width / 2f);
        _maxX = transform.position.x + (_width / 2f);
        _minY = transform.position.y - (_height / 2f);
        _maxY = transform.position.y + (_height / 2f);
    }
    
    private void GetPrefabs()
    {
        if(_startInstances == null ||  _startInstances.Count == 0)
            return;
        
        _prefabs = _startInstances.Select(instance => instance.Prefab).ToArray();
    }
    #endregion
    
    #region Placement
    private async UniTask<bool> TryToFindPlaceForItem(GridItem itemInstance)
    {
        if (itemInstance == null)
            return false;
        
        if(_cancellationTokenSource == null || (_cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested))
            _cancellationTokenSource = new CancellationTokenSource();

        Bounds[] itemBounds = itemInstance.GetCellsBounds();
        if (itemBounds == null || itemBounds.Length == 0)
            return false;
        
        for (int attempt = 0; attempt < _maxAttemptsToPlaceItem; attempt++)
        {
            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
            
            float randomX = Random.Range(_minX, _maxX);
            float randomY = Random.Range(_minY, _maxY);
            Vector3 newPosition = new Vector3(randomX, randomY, itemInstance.transform.position.z);
            
            Bounds[] movedBounds = new Bounds[itemBounds.Length];
            Vector3 offset = newPosition - itemInstance.transform.position;

            for (int i = 0; i < itemBounds.Length; i++)
            {
                movedBounds[i] = itemBounds[i];
                movedBounds[i].center += offset;
            }
            
            bool outsideZone = false;
            foreach (Bounds moved in movedBounds)
            {
                float minX = moved.min.x;
                float maxX = moved.max.x;
                float minY = moved.min.y;
                float maxY = moved.max.y;

                if (minX < _minX || maxX > _maxX || minY < _minY || maxY > _maxY)
                {
                    outsideZone = true;
                    break;
                }
            }

            if (outsideZone)
                continue;

            bool intersects = false;
            
            foreach (GridItem other in _itemsInstances)
            {
                if (other == itemInstance || other == null)
                    continue;

                Bounds[] otherBounds = other.GetCellsBounds();
                if (otherBounds == null || otherBounds.Length == 0)
                    continue;

                foreach (Bounds moved in movedBounds)
                {
                    foreach (Bounds existing in otherBounds)
                    {
                        if (moved.Intersects(existing))
                        {
                            intersects = true;
                            break;
                        }
                    }
                    if (intersects)
                        break;
                }

                if (intersects)
                    break;
            }
            
            if (!intersects)
            {
                _itemPosition = newPosition;
                return true;
            }
        }

        Debug.LogWarning($"Could not place item '{itemInstance.name}' within {maxAttempts} attempts inside the zone.");
        return false;
    }
    #endregion
    
    #region IInteractableZone
    public Bounds GetBounds()
    {
        Vector3 center = transform.position;
        Vector2 size = new Vector2(_width, _height);
        return new Bounds(center, size);
    }

    public string GetName() => "Items zone.";
    
    public bool TryToDragItemFromZone(Vector3 PointerPosition, out IInteractableItem DraggedItem)
    {
        if (_itemsInstances.Count > 0)
        {
            foreach (GridItem itemInstance in _itemsInstances)
            {
                Bounds[] itemCellsBounds = itemInstance.GetCellsBounds();
                bool hitCell = false;

                foreach (Bounds bounds in itemCellsBounds)
                {
                    PointerPosition.z = bounds.center.z;
                    hitCell = bounds.Contains(PointerPosition);
                    
                    if(hitCell)
                        break;
                }

                if (hitCell)
                {
                    DraggedItem = (IInteractableItem)itemInstance;
                    _itemsInstances.Remove(itemInstance);
                    itemInstance.transform.SetParent(null);
                    return true;
                }
            }
        }

        DraggedItem = null;
        return false;
    }

    public async void TryToDropItemIntoZone(IInteractableItem DroppedItem)
    {
        if(DroppedItem == null)
            return;
        
        GridItem item = (GridItem)DroppedItem;
        item.Active = false;
        
        await ReturnItemIntoZone(item);
        
        item.Active = true;
        item.transform.SetParent(_itemsParent);
        _itemsInstances.Add(item);
    }

    private async UniTask ReturnItemIntoZone(GridItem Item)
    {
        if(_cancellationTokenSource == null || (_cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested))
            _cancellationTokenSource = new CancellationTokenSource();

        Vector3 targetPosition = Item.StartPosition;
        
        while (Vector3.Distance(Item.transform.position, targetPosition) > 0.1f)
        {
            try
            {
                Vector3 currentPosition = Item.transform.position;
                Vector3 direction = (targetPosition - currentPosition).normalized;
                Item.transform.position += direction * Time.deltaTime * _itemReturnSpeed * TimeScaler.TimeScale;
                
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
        
        Item.transform.position = targetPosition;
    }
    
    public void OnPointerEnter() { }
    public void OnPointerExit() { }
    public void OnInteractableItemEnter(IInteractableItem Item) { }
    public void OnInteractableItemExit(IInteractableItem Item) { }
    #endregion
    
    #endregion
}

[Serializable]
public class GridItemsSpawnSetting
{
    [field: SerializeField]
    public GridItem Prefab { get; private set; }
    [field: SerializeField, Range(1, 10)]
    public int Amount { get; private set; } = 1;
}