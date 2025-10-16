using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GridConstructor : MonoBehaviour
{
    #region Fields
    [Header("References.")]
    [SerializeField, ReadOnly] 
    private InventoryGrid _currentGrid;
    [SerializeField] 
    private GridCell _cellPrefab;
    [SerializeField]
    private TileProductionSettingsScriptable[] _tilesProductionsSettings;
    
    [Header("Grid settings.")] 
    [SerializeField, Range(1, 25)]
    private int _rows = 1;
    [SerializeField, Range(1, 25)]
    private int _columns = 1;
    
    [Space(5)]
    [SerializeField, Range(-1f, 1f)]
    private float _rowsOffset = 0.5f;

    [SerializeField, Range(-1f, 1f)]
    private float _columnsOffset = 0.5f;

    [Space(15)]
    [Header("Cell settings.")]
    [SerializeField] 
    private Vector2 _cellSize = Vector2.one;
    [SerializeField, Range(0f, 1f)]
    private float _cellSpacing = 1f;

    [Header("Draw settings.")] 
    [SerializeField]
    private bool _drawModeEnabled = false;
    [SerializeField]
    private ProductionSettings.TileType _selectedTileType = global::ProductionSettings.TileType.Red;
    
    private GridCell _activeCell;
    #endregion

    #region Properties
    public ProductionSettings.TileType SelectedTileType
    {
        get { return _selectedTileType; }
        set { _selectedTileType = value; }
    }
    
    public TileProductionSettingsScriptable[] ProductionSettings => _tilesProductionsSettings;
    #endregion
    
    #region Methods
    #if UNITY_EDITOR
    [ExecuteInEditMode]
    public void ReleaseCurrentGrid()
    {
        if (_currentGrid == null)
        {
            Debug.LogWarning("You can't release current grid, because you have no current grid!");
            return;
        }
        
        _currentGrid.transform.SetParent(null);
        _currentGrid = null;
    }

    [ExecuteInEditMode]
    public void DestroyCurrentGrid()
    {
        if (_currentGrid == null)
        {
            Debug.LogWarning("You can't destroy current grid, because you have no current grid!");
            return;
        }
            
        
        DestroyImmediate(_currentGrid.gameObject);
        _currentGrid = null;
    }
    
    [ExecuteInEditMode]
    public void CreateGrid()
    {
        if(Application.isPlaying)
            return;

        if (_currentGrid != null)
        {
            Debug.LogWarning("You can't create new grid, because you have one already!");
            return;
        }
        
        if (_cellPrefab == null)
        {
            Debug.LogError("You can't create grid cells, because cell prefab is null!");
            return;
        }
        
        GameObject gridObject = new GameObject("Grid");
        gridObject.transform.SetParent(transform);
        gridObject.transform.localPosition = Vector3.zero;
        
        InventoryGrid grid = (InventoryGrid)gridObject.AddComponent(typeof(InventoryGrid));
        _currentGrid = grid;
        _currentGrid.Size = new Vector2Int(_columns, _rows);

        ResourceProduction production = (ResourceProduction)gridObject.AddComponent(typeof(ResourceProduction));
        production.SetGrid(_currentGrid);
        
        HorizontalAlignmentTarget alignmentTarget = (HorizontalAlignmentTarget)gridObject.AddComponent(typeof(HorizontalAlignmentTarget));
        
        GameObject cellsParent = new GameObject("CellsParent");
        cellsParent.transform.SetParent(grid.transform);
        cellsParent.transform.localPosition = Vector3.zero;
        grid.CellsParent = cellsParent.transform;
        
        GameObject itemsParent = new GameObject("ItemsParent");
        itemsParent.transform.SetParent(grid.transform);
        itemsParent.transform.localPosition = Vector3.zero;
        grid.ItemsParent = itemsParent.transform;
        
        CreateGridCells();
        UpdateCells();
        AutoAssignTileTypes();
    }

    [ExecuteInEditMode]
    public void CreateGridCells()
    {
        if(Application.isPlaying)
            return;

        if (_currentGrid == null)
        {
            Debug.LogError("You can't create grid cells, because current grid is null!");
            return;
        }

        if (_cellPrefab == null)
        {
            Debug.LogError("You can't create grid cells, because cell prefab is null!");
            DestroyCurrentGrid();
            return;
        }

        int totalCellsAmount = _rows * _columns;

        for (int i = 0; i < totalCellsAmount; i++)
        {
            GridCell newGridCell = Instantiate(_cellPrefab);
            newGridCell.name = "GridCell";
            newGridCell.transform.SetParent(_currentGrid.CellsParent);
            newGridCell.SetGrid(_currentGrid);
            _currentGrid.AddCell(newGridCell);
            
            Component dependencyInjectionComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(newGridCell.gameObject, typeof(GridCellDependenciesContainer));
            
            if(dependencyInjectionComponent == null)
                continue;
            
            ((GridCellDependenciesContainer)dependencyInjectionComponent).InjectDependencies();
        }
    }

    [ExecuteInEditMode]
    public void UpdateCells()
    {
        if (_currentGrid == null)
        {
            Debug.LogError("You can't update grid cells, because current grid is null!");
            return;
        }

        if (_currentGrid.Cells.Count == 0)
        {
            Debug.LogError("You can't update grid cells, because current grid hase no cells!");
            return;
        }

        _currentGrid.CellSize = _cellSize;
        Vector3 gridCenter = _currentGrid.CellsParent.transform.position;
        float startYPosition = gridCenter.y - ((_rows - 1) * _rowsOffset * 0.5f);
        int cellPointer = 0;
        
        for (int r = 0; r < _rows; r++)
        {
            float startXPosition = gridCenter.x - ((_columns - 1) * _columnsOffset * 0.5f);
        
            for (int c = 0; c < _columns; c++)
            {
                float xWorld = startXPosition + (c * _columnsOffset);
                float yWorld = startYPosition + (r * _rowsOffset);
                
                GridCell cell =  _currentGrid.Cells[cellPointer];
                cell.transform.position = new Vector3(xWorld, yWorld, gridCenter.z);
                cell.SetCellSize(_cellSize * _cellSpacing);
                cell.SetGridPosition(new Vector2Int(r, c));
                cell.name = $"GridCell({r}, {c})";
                cellPointer++;
            }
        }
    }

    [ExecuteInEditMode]
    public void ResizeCells()
    {
        if (_currentGrid == null)
        {
            Debug.LogWarning("Can't sync grid cells — no current grid!");
            return;
        }

        if (_cellPrefab == null)
        {
            Debug.LogError("Can't sync grid cells — cell prefab is null!");
            return;
        }

        int targetCount = _rows * _columns;
        int currentCount = _currentGrid.Cells.Count;
        
        if(targetCount == currentCount)
            return;
        
        if (currentCount > targetCount)
        {
            int cellsAmountToRemove = currentCount - targetCount;
            for (int i = 0; i < cellsAmountToRemove; i++)
            {
                GridCell lastCell = _currentGrid.Cells.Last();
                _currentGrid.RemoveCell(lastCell);
                
                if (lastCell != null)
                    DestroyImmediate(lastCell.gameObject);
            }
        }
        else if (currentCount < targetCount)
        {
            int cellsAmountToAdd = targetCount - currentCount;
            for (int i = 0; i < cellsAmountToAdd; i++)
            {
                GridCell newCell = Instantiate(_cellPrefab, _currentGrid.CellsParent);
                newCell.SetGrid(_currentGrid);
                _currentGrid.AddCell(newCell);
                
                Component dependencyInjectionComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(newCell.gameObject, typeof(GridCellDependenciesContainer));
                
                if (dependencyInjectionComponent != null)
                    ((GridCellDependenciesContainer)dependencyInjectionComponent).InjectDependencies();
            }
        }
    }
    
    [ExecuteInEditMode]
    public void AutoAssignTileTypes()
    {
        if (_currentGrid == null || _currentGrid.Cells.Count == 0)
            return;
        
        if(_tilesProductionsSettings.Length == 0)
            return;
        
        int width = _columns;
        int height = _rows;
        
        foreach (GridCell cell in _currentGrid.Cells)
        {
            Vector2Int gridPosition = cell.GridPosition;
            int xPosition = gridPosition.x;
            int yPosition = gridPosition.y;

            ProductionSettings.TileType tileType = global::ProductionSettings.TileType.Red;
            
            if (xPosition == 0 || yPosition == 0 || xPosition == width - 1 || yPosition == height - 1)
            {
                tileType = global::ProductionSettings.TileType.Red;
            }
            else if (xPosition == 1 || yPosition == 1 || xPosition == width - 2 || yPosition == height - 2)
            {
                tileType = global::ProductionSettings.TileType.Yellow;
            }
            else
            {
                tileType = global::ProductionSettings.TileType.Green;
            }

            TileProductionSettingsScriptable settings = _tilesProductionsSettings.Where(settings => settings.TileType == tileType).Single();
            cell.SetProductionSettings(settings);
        }
    }
    #endif
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridConstructor))]
public class GridConstructorEditor : Editor
{
    private static readonly string currentGridPropertyName = "_currentGrid";
    private static readonly string cellPrefabPropertyName = "_cellPrefab";
    private static readonly string tilesProductionsSettingsPropertyName = "_tilesProductionsSettings";
    private static readonly string rowsPropertyName = "_rows";
    private static readonly string columnsPropertyName = "_columns";
    private static readonly string rowsOffsetPropertyName = "_rowsOffset";
    private static readonly string columnsOffsetPropertyName = "_columnsOffset";
    private static readonly string cellSizePropertyName = "_cellSize";
    private static readonly string cellSpacingPropertyName = "_cellSpacing";
    private static readonly string selectedTileTypePropertyName = "_selectedTileType";
    private static readonly string drawModeEnabledPropertyName = "_drawModeEnabled";
    
    private SerializedProperty _currentGridProperty;
    private SerializedProperty _cellPrefabProperty;
    private SerializedProperty _tilesProductionsSettingsProperty;
    private SerializedProperty _rowsProperty;
    private SerializedProperty _columnsProperty;
    private SerializedProperty _rowsOffsetProperty;
    private SerializedProperty _columnsOffsetProperty;
    private SerializedProperty _cellSizeProperty;
    private SerializedProperty _cellSpacingProperty;
    private SerializedProperty _drawModeEnabledProperty;
    
    private GridConstructor _constructor;

    private void OnEnable()
    {
        _currentGridProperty = serializedObject.FindProperty(currentGridPropertyName);
        _cellPrefabProperty = serializedObject.FindProperty(cellPrefabPropertyName);
        _tilesProductionsSettingsProperty = serializedObject.FindProperty(tilesProductionsSettingsPropertyName);
        _rowsProperty = serializedObject.FindProperty(rowsPropertyName);
        _columnsProperty = serializedObject.FindProperty(columnsPropertyName);
        _rowsOffsetProperty = serializedObject.FindProperty(rowsOffsetPropertyName);
        _columnsOffsetProperty = serializedObject.FindProperty(columnsOffsetPropertyName);
        _cellSizeProperty = serializedObject.FindProperty(cellSizePropertyName);
        _cellSpacingProperty = serializedObject.FindProperty(cellSpacingPropertyName);
        _drawModeEnabledProperty = serializedObject.FindProperty(drawModeEnabledPropertyName);
        _constructor = (GridConstructor)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Color defaultColor = GUI.backgroundColor;
        //constructor.UpdateGrid();
        
        EditorGUILayout.PropertyField(_currentGridProperty);
        EditorGUILayout.PropertyField(_cellPrefabProperty);
        EditorGUILayout.PropertyField(_tilesProductionsSettingsProperty);


        if (_cellPrefabProperty.objectReferenceValue == null || _tilesProductionsSettingsProperty.arraySize == 0)
        {
            EditorGUILayout.HelpBox("Please, assign Cell Prefab and Tiles Productions Settings scriptables to continue.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_rowsProperty);
        EditorGUILayout.PropertyField(_columnsProperty);
        bool gridSettingsChanged = EditorGUI.EndChangeCheck();

        EditorGUILayout.Space(20f);
        GUIStyle buttonStyle = new(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 13,
            fixedHeight = 35
        };
        
        if (_currentGridProperty.boxedValue != null)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_rowsOffsetProperty);
            EditorGUILayout.PropertyField(_columnsOffsetProperty);
            EditorGUILayout.PropertyField(_cellSizeProperty);
            EditorGUILayout.PropertyField(_cellSpacingProperty);
            
            bool cellsSettingsChanged = EditorGUI.EndChangeCheck();

            if (gridSettingsChanged || cellsSettingsChanged)
            {
                serializedObject.ApplyModifiedProperties();

                if (gridSettingsChanged)
                {
                    _constructor.ResizeCells();
                }
                
                _constructor.UpdateCells();
                _constructor.AutoAssignTileTypes();
                
                EditorUtility.SetDirty(_constructor);
                Repaint();
            }

            if (cellsSettingsChanged)
            {
                serializedObject.ApplyModifiedProperties();
                _constructor.UpdateCells();
            }
            
            EditorGUILayout.Space(10f);
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Release grid.", buttonStyle, GUILayout.Height(35)))
            {
                _constructor.ReleaseCurrentGrid();
            }
            GUI.backgroundColor = defaultColor;
            
            EditorGUILayout.Space(10f);
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Destroy grid...", buttonStyle, GUILayout.Height(35)))
            {
                _constructor.DestroyCurrentGrid();
            }
            GUI.backgroundColor = defaultColor;

            EditorGUILayout.Space(20f);
            EditorGUILayout.LabelField("Tile Painting", EditorStyles.boldLabel);
            _constructor.SelectedTileType = (ProductionSettings.TileType)EditorGUILayout.EnumPopup("Selected Tile Type", _constructor.SelectedTileType);

            bool drawModeEnabled = _drawModeEnabledProperty.boolValue;
            EditorGUILayout.Space(10f);
            if (GUILayout.Button(drawModeEnabled ? "Disable Tile Painting" : "Enable Tile Painting", GUILayout.Height(30)))
                _drawModeEnabledProperty.boolValue = !drawModeEnabled;

            if (drawModeEnabled)
            {
                EditorGUILayout.Space(10f);
                DrawGridPaintingArea(_constructor, (InventoryGrid)_currentGridProperty.objectReferenceValue);
            }
        }
        else
        {
            EditorGUILayout.Space(10f);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Create new grid!", buttonStyle, GUILayout.Height(35)))
            {
                _constructor.CreateGrid();
            }
            GUI.backgroundColor = defaultColor;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawGridPaintingArea(GridConstructor constructor, InventoryGrid grid)
    {
        if(grid == null || constructor == null)
            return;
        
        int rows = grid.Size.y;
        int columns = grid.Size.x;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Paint Tiles:", EditorStyles.boldLabel);

        Color defaultColor = GUI.backgroundColor;
        for (int r = rows - 1; r >= 0; r--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < columns; c++)
            {
                Vector2Int cellCoord = new Vector2Int(r, c);

                if (!grid.TryGetCellAt(cellCoord, out GridCell cell))
                {
                    GUI.backgroundColor = Color.grey;
                    GUILayout.Button("", GUILayout.Width(25), GUILayout.Height(25));
                    continue;
                }

                ProductionSettings.TileType type = cell.ProductionSettings.TileType;
                GUI.backgroundColor = ProductionSettings.TileColors[type];

                if (GUILayout.Button("", GUILayout.Width(25), GUILayout.Height(25)))
                {
                    Undo.RecordObject(cell, "Paint Tile");
                    cell.SetProductionSettings(constructor.ProductionSettings.Where(settings => settings.TileType == type).Single());
                    EditorUtility.SetDirty(cell);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        GUI.backgroundColor = defaultColor;
    }
}
#endif
