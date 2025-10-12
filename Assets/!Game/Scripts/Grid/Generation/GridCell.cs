using System;
using UnityEngine;

public class GridCell : MonoBehaviour, IDependenciesInjection<GridCellDependenciesContainer>
{
    #region Fields
    [Header("References.")]
    [SerializeField]
    private SpriteRenderer _graphic;
    [SerializeField]
    private GridItem _gridItem = null;
    [SerializeField]
    private InventoryGrid _grid = null;
    #endregion
    
    #region Properties
    [Header("Settings.")]
    [SerializeField] 
    private Vector2Int _gridPosition = Vector2Int.zero;
    [SerializeField]
    private Vector2 _cellSize = Vector2.zero;
    
    [field: SerializeField, HideInInspector]
    public bool Initialized { get; set; } = false;
    
    public Vector2Int GridPosition => _gridPosition;
    public Transform Parent => transform.parent;
    public GridItem GridItem => _gridItem;
    public bool Busy => _gridItem != null;

    #endregion
    
    #region Methods
    public void Inject(GridCellDependenciesContainer Container)
    {
        if(Container == null)
            return;

        _graphic = Container.Graphic;
        Initialize();
    }

    public void Initialize()
    {
        
    }
    
    // public bool PositionIsInCell(Vector2 WorldPosition)
    // {
    //     Vector2 worldPosition = WorldPosition - (Vector2)this.WorldPosition;
    //     
    //     float halfWidth = WorldCellSize.x / 2f;
    //     float halfHeight = WorldCellSize.y / 2f;
    //     
    //     float dx = Mathf.Abs(worldPosition.x) / halfWidth;
    //     float dy = Mathf.Abs(worldPosition.y) / halfHeight;
    //     
    //     return (dx + dy) <= 1f;
    // }

    public void SetValues()
    {
        
    }
    
    public void AddItem(GridItem Item)
    {
        if(Item == null)
            return;
        
        _gridItem = Item;
    }
    
    public void RemoveItem()
    {
        if(!Busy)
            return;
        
        _gridItem = null;
    }

    #region Setters
    public void SetGrid(InventoryGrid NewGrid)
    {
        if(_grid !=  null)
            return;
        
        _grid = NewGrid;
    }
    
    public void SetCellSize(Vector2 NewCellSize)
    {
        if(_cellSize == NewCellSize)
            return;
        
        _cellSize = NewCellSize;
        
        if(_graphic == null)
            return;

        switch (_graphic.drawMode)
        {
            case SpriteDrawMode.Simple:
            {
                if (_graphic.sprite == null)
                    return;

                Vector2 spriteSize = _graphic.sprite.bounds.size;
                if (spriteSize.x <= 0 || spriteSize.y <= 0)
                    return;

                Vector3 newScale = new Vector3(
                    _cellSize.x / spriteSize.x,
                    _cellSize.y / spriteSize.y,
                    1f
                );

                _graphic.transform.localScale = newScale;
                break;
            }

            case SpriteDrawMode.Sliced:
            case SpriteDrawMode.Tiled:
            {
                _graphic.size = _cellSize;
                _graphic.transform.localScale = Vector3.one;
                break;
            }

            default:
            {
                Debug.LogWarning($"Unsupported SpriteDrawMode: {_graphic.drawMode}");
                break;
            }
        }
    }
    
    public void SetGridPosition(Vector2Int NewPosition) => _gridPosition = NewPosition;
    #endregion
    #endregion
}
