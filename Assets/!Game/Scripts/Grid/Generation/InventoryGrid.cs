using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class InventoryGrid : MonoBehaviour, IInteractableZone
{
    #region Properties
    [field: Header("References.")]
    [field: SerializeField] 
    public Transform CellsParent { get; set; }
    [field: SerializeField] 
    public Transform ItemsParent { get; set; }
    
    [field: Header("Settings.")]
    [field: SerializeField]
    public Vector2Int Size { get; set; } = Vector2Int.zero;
    [field: SerializeField]
    public Vector2 CellSize { get; set; } = Vector2.one;

    public Subject<List<GridCell>> BusyCellsChanged { get; private set; } = new Subject<List<GridCell>>(); 
    public List<GridCell> Cells => _cells;
    public List<GridItem> Items => _items;
    #endregion

    #region Fields
    [field: SerializeField, Range(0f, 100f)]
    public float _itemMovementSpeed = 5f;
    [Header("Container.")]
    [SerializeField]
    private List<GridCell> _cells = new ();
    [SerializeField, ReadOnly]
    private List<GridItem> _items = new ();
    
    private List<GridCell> _busyCells = new ();
    private IInteractableItem _itemInZone = null;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    #endregion
    
    #region Methods
    private void Awake()
    {
        if(CellsParent == null)
            CellsParent = transform;
    }

    #region Cells operations
    public void AddCell(GridCell cell)
    {
        if(_cells.Contains(cell))
            return;
        
        _cells.Add(cell);
    }

    public void RemoveCell(GridCell cell)
    {
        if(!_cells.Contains(cell))
            return;
        
        _cells.Remove(cell);
    }
    #endregion
    
    #region IInteractableZone
    public Bounds GetBounds()
    {
        Vector3 center = CellsParent.position;
        Vector2 size = new Vector2(Size.x * CellSize.x, Size.y * CellSize.y);
        return new Bounds(center, size);
    }

    public string GetName() => "Inventory Grid";
    
    public bool TryToDragItemFromZone(Vector3 PointerPosition, out IInteractableItem DraggedItem)
    {
        DraggedItem = null;
        return false;
    }

    public async void TryToDropItemIntoZone(IInteractableItem DroppedItem)
    {
        if (DroppedItem == null)
            return;
        
        ItemCell[] itemCells = DroppedItem.GetCells();
        if (itemCells == null || itemCells.Length == 0)
            return;

        List<GridCell> matchedCells = new();
        
        foreach (ItemCell itemCell in itemCells)
        {
            GridCell closestCell = null;
            float closestDistance = 100f;
            bool intersectsAny = false;
            Bounds itemCellBounds = itemCell.GetOriginalBounds();

            foreach (GridCell gridCell in _cells)
            {
                if (gridCell == null)
                    continue;

                Bounds cellBounds = gridCell.GetBounds();

                if (itemCellBounds.Intersects(cellBounds))
                {
                    intersectsAny = true;

                    float distance = Vector3.Distance(itemCellBounds.center, cellBounds.center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestCell = gridCell;
                    }
                }
            }
            
            if (!intersectsAny)
            {
                DroppedItem.StartZone?.TryToDropItemIntoZone(DroppedItem);
                return;
            }

            if (closestCell != null && !matchedCells.Contains(closestCell))
            {
                closestCell.SetProductiveCellState(itemCell.IsPrimary);
                matchedCells.Add(closestCell);
            }
        }
        
        if (matchedCells.Count == 0)
        {
            DroppedItem.StartZone?.TryToDropItemIntoZone(DroppedItem);
            return;
        }
        
        Vector3 averagePosition = Vector3.zero;

        List<GridItem> removedItems = new();
        
        foreach (GridCell matchedCell in matchedCells)
        {
            averagePosition += matchedCell.transform.position;

            if (matchedCell.Busy)
            {
                GridItem matchedCellItem = matchedCell.GridItem;
                
                if(!removedItems.Contains(matchedCellItem))
                    removedItems.Add(matchedCellItem);
            }

            GridItem item = (GridItem)DroppedItem;
            matchedCell.SetItem(item);
            _items.Add(item);
            
            if(!_busyCells.Contains(matchedCell))
                _busyCells.Add(matchedCell);
        }

        if (removedItems.Count > 0)
        {
            foreach (GridItem item in removedItems)
            {
                IEnumerable<GridCell> cellsBusyForItem = _cells.Where(cell => cell.Busy && cell.GridItem == item && !(matchedCells.Contains(cell)));

                if (cellsBusyForItem.Count() > 0)
                {
                    foreach (GridCell cell in cellsBusyForItem)
                    {
                        cell.SetItem(null);
                        cell.SetProductiveCellState(false);
                        _busyCells.Remove(cell);
                    }
                }
                
                item.StopPlayingWorkAnimation();
                item.transform.SetParent(null);
                item.StartZone.TryToDropItemIntoZone(item);
                _items.Remove(item);
            }
        }
        
        averagePosition /= matchedCells.Count;
        
        DroppedItem.Active = false;
        await LerpItemIntoZone(DroppedItem, averagePosition);
        DroppedItem.Active = true;
        DroppedItem.Parent.SetParent(ItemsParent);
        _items.Add((GridItem)DroppedItem);
        DroppedItem.StartPlayingWorkAnimation();
        BusyCellsChanged.OnNext(_busyCells);
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
                interactableItemTransform.position += direction * Time.deltaTime * _itemMovementSpeed * TimeScaler.TimeScale;
                
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

    public void OnInteractableItemEnter(IInteractableItem Item) => _itemInZone = Item;

    public void OnInteractableItemExit(IInteractableItem Item) => _itemInZone = null;
    #endregion

    #region Getters
    public bool TryGetCellAt(Vector2Int GridPosition, out GridCell Cell)
    {
        if (_cells == null || _cells.Count == 0)
        {
            Cell = null;
            return false;
        }
        
        IEnumerable<GridCell> cells = _cells.Where(cell => cell.GridPosition == GridPosition);

        if (cells.Count() == 0)
        {
            Cell = null;
            return false;
        }

        Cell = cells.Single();
        return true;
    }
    #endregion

    #region Items operations

    public void DestroyItem(GridItem ItemToDestroy)
    {
        if(ItemToDestroy == null)
            return;
        
        if(!_items.Contains(ItemToDestroy))
            return;

        _items.Remove(ItemToDestroy);
        ItemToDestroy.StopPlayingWorkAnimation();
        ItemToDestroy.Destroy();
        Debug.Log("Item destroyed!");
    }
    #endregion
    #if UNITY_EDITOR
    #region Gizmos
    private void OnDrawGizmosSelected() => DrawGridGizmos();
    
    public void DrawGridGizmos()
    {
        Bounds bounds = GetBounds();
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
    #endregion

    private void OnDisable()
    {
        if(_cancellationTokenSource != null &&  !_cancellationTokenSource.IsCancellationRequested)
            _cancellationTokenSource.Cancel();
    }
#endif
    #endregion
}