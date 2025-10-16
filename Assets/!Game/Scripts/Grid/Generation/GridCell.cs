using System;using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour, IDependenciesInjection<GridCellDependenciesContainer>
{
    #region Fields

    [Header("References.")]
    [SerializeField]
    private SpriteRenderer _graphic;
    [SerializeField]
    private InventoryGrid _grid = null;
    [SerializeField, ReadOnly]
    private GridItem _gridItem = null;
    #endregion
    
    #region Properties
    [Header("Settings.")]
    [SerializeField, ReadOnly]
    private TileProductionSettingsScriptable _productionSettings;
    [SerializeField, ReadOnly]
    private bool _isProductiveCell = false;
    [SerializeField] 
    private Vector2Int _gridPosition = Vector2Int.zero;
    [SerializeField]
    private Vector2 _cellSize = Vector2.zero;
    
    [field: SerializeField, HideInInspector]
    public bool Initialized { get; set; } = false;
    
    public Vector2Int GridPosition => _gridPosition;
    public GridItem GridItem => _gridItem;
    public Vector2 CellSize => _cellSize;
    public bool Busy => _gridItem != null;
    public bool IsProductive => _isProductiveCell;
    public TileProductionSettingsScriptable ProductionSettings => _productionSettings;
    #endregion
    
    #region Methods
    public void Inject(GridCellDependenciesContainer Container)
    {
        if(Container == null)
            return;

        _graphic = Container.Graphic;
    }

    private void UpdateVisual()
    {
        if(_productionSettings == null)
            return;
        
        if (_graphic != null)
            _graphic.color = global::ProductionSettings.TileColors[_productionSettings.TileType];
    }

    #region Getters
    public Bounds GetBounds() => _graphic.bounds;
    #endregion


    #region Setters
    public void SetProductionSettings(TileProductionSettingsScriptable NewSettings)
    {
        _productionSettings = NewSettings;
        UpdateVisual();
    }
    
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

        SpriteScaler.ScaleSpriteFromRenderer(_graphic, _cellSize);
    }
    
    public void SetGridPosition(Vector2Int NewPosition) => _gridPosition = NewPosition;
    public void SetItem(GridItem NewItem) => _gridItem = NewItem;
    
    public void SetProductiveCellState(bool NewState) => _isProductiveCell = NewState;
    #endregion
    #endregion
}