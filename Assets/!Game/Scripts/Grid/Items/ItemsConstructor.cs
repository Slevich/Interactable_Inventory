using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ItemsConstructor : MonoBehaviour
{
    #region Fields
    [Header("References.")]
    [SerializeField]
    private InventoryGrid _inventoryGridOrigin;

    [SerializeField]
    private GameObject _itemCellPrefab;
    
    [Header("Settings.")]
    [SerializeField]
    private Color _itemColor = Color.white;
    [SerializeField, Range(0f, 1f)] 
    private float _itemCellsSizeModifier = 1f;

    [SerializeField, HideInInspector]
    private GridItem _currentGridItem;
    
    private Color _lastColor;
    #endregion

    #region Properties
    [field: SerializeField, HideInInspector]
    public List<Vector2Int> SelectedCells { get; set; } = new ();
    #endregion
    
    #region Methods
#if UNITY_EDITOR
    [ExecuteInEditMode]
    public void ClearSelection(bool UpdateItem = true)
    {
        SelectedCells.Clear();
        
        if (UpdateItem)
            UpdateItemFromSelection();
    }
    
    [ExecuteInEditMode]
    public void ToggleCell(Vector2Int СellCoordinate)
    {
        bool updated = TryToggleCell(СellCoordinate);

        if (updated)
            UpdateItemFromSelection();
    }
    
    private bool TryToggleCell(Vector2Int toggleCell)
    {
        if (!SelectedCells.Contains(toggleCell))
        {
            Debug.Log("Add new cell!");
            
            if (SelectedCells.Count == 0)
            {
                SelectedCells.Add(toggleCell);
                return true;
            }
            
            bool hasNeighbor = SelectedCells.Any(cell => (cell.x == toggleCell.x && (cell.y == (toggleCell.y + 1) || (cell.y == (toggleCell.y - 1))))
                                                                  || (cell.y == toggleCell.y && (cell.x == (toggleCell.x + 1) || (cell.x == (toggleCell.x - 1)))));
            
            if(hasNeighbor)
                SelectedCells.Add(toggleCell);
            
            return hasNeighbor;
        }
        else
        {
            if (SelectedCells.Count <= 1)
            {
                SelectedCells.Remove(toggleCell);
                return true;
            }
            
            var tempSet = new HashSet<Vector2Int>(SelectedCells);
            tempSet.Remove(toggleCell);

            if (!IsSelectionConnected(tempSet))
            {
                return false;
            }
            
            SelectedCells.Remove(toggleCell);
            return true;
        }
    }

    private bool IsSelectionConnected(HashSet<Vector2Int> cells)
    {
        if (cells == null || cells.Count <= 1)
            return true;
        
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        Vector2Int startCell = cells.First();
        
        queue.Enqueue(startCell);
        visited.Add(startCell);

        Vector2Int[] directions = new[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbor = current + direction;
                if (cells.Contains(neighbor) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        return visited.Count == cells.Count;
    }
    
    [ExecuteInEditMode]
    public void UpdateItemFromSelection()
    {
        if (_inventoryGridOrigin == null || _itemCellPrefab == null)
            return;

        if (SelectedCells.Count == 0)
        {
            DeleteCurrentItem();
            return;
        }

        if (_currentGridItem == null)
            CreateNewItem();

        RebuildItemCells();
    }
    
    private void CreateNewItem()
    {
        GameObject itemObject = new GameObject("GridItem");
        itemObject.transform.SetParent(transform);
        itemObject.transform.localPosition = Vector3.zero;

        GridItem gridItem = itemObject.AddComponent<GridItem>();
        _currentGridItem = gridItem;

        GameObject cellsParent = new GameObject("CellsParent");
        cellsParent.transform.SetParent(itemObject.transform);
        cellsParent.transform.localPosition = Vector3.zero;
        _currentGridItem.CellsParent = cellsParent.transform;

        ScaleAnimation scaleAnimation = (ScaleAnimation)cellsParent.AddComponent(typeof(ScaleAnimation));
        scaleAnimation.AffectChildren = true;
    }

    private void RebuildItemCells()
    {
        if (_currentGridItem == null)
            return;

        Transform cellsParent = _currentGridItem.CellsParent;
        if (cellsParent == null)
            return;

        for (int i = cellsParent.childCount - 1; i >= 0; i--)
        {
            _currentGridItem.Cells.Remove(cellsParent.GetChild(i).GetComponent<ItemCell>());
            DestroyImmediate(cellsParent.GetChild(i).gameObject);
        }
        
        foreach (Vector2Int cellPosition in SelectedCells)
        {
            if (!_inventoryGridOrigin.TryGetCellAt(cellPosition, out GridCell gridCell))
                continue;

            GameObject itemCellObject = Instantiate(_itemCellPrefab, cellsParent);
            itemCellObject.name = $"ItemCell";
            itemCellObject.transform.position = gridCell.transform.position;
            ItemCell itemCell = itemCellObject.AddComponent<ItemCell>();
            _currentGridItem.Cells.Add(itemCell);
            itemCell.OriginalSize = gridCell.CellSize;
        }

        _currentGridItem.SetCellsSizesWithModifier(_itemCellsSizeModifier);
        _currentGridItem.SetNewCellsColors(_itemColor);
        
        _lastColor = _itemColor;
    }
    
    [ExecuteInEditMode]
    public void DeleteCurrentItem()
    {
        if (_currentGridItem != null)
        {
            DestroyImmediate(_currentGridItem.gameObject);
            _currentGridItem = null;
        }
        
        ClearSelection(false);
    }
    
    [ExecuteInEditMode]
    public void RecolorItem()
    {
        if (_currentGridItem == null)
            return;

        _currentGridItem.SetNewCellsColors(_itemColor);
    }

    [ExecuteInEditMode]
    public void ResizeItemCellsSizes()
    {
        if (_currentGridItem == null)
            return;
        
        _currentGridItem.SetCellsSizesWithModifier(_itemCellsSizeModifier);
    }
    
    [ExecuteInEditMode]
    public void ReleaseCurrentItem()
    {
        if (_currentGridItem == null)
        {
            Debug.LogWarning("No current item to release!");
            return;
        }

        Transform cellsParent = _currentGridItem.CellsParent;
        if (cellsParent == null || cellsParent.childCount == 0)
        {
            Debug.LogWarning("Current item has no cells to release!");
            _currentGridItem.transform.SetParent(null);
            _currentGridItem = null;
            ClearSelection();
            return;
        }
        
        Vector3 totalPosition = Vector3.zero;
        foreach (Transform cell in cellsParent)
            totalPosition += cell.position;

        Vector3 center = totalPosition / cellsParent.childCount;
        Vector3 offset = cellsParent.position - center;

        foreach (Transform cell in cellsParent)
            cell.position += offset;

        int randomIndex = Random.Range(0, _currentGridItem.Cells.Count);
        _currentGridItem.Cells[randomIndex].IsPrimary = true;

        _currentGridItem.transform.SetParent(null);
        _currentGridItem = null;
        ClearSelection();
    }
#endif
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(ItemsConstructor))]
public class ItemsConstructorEditor : Editor
{
    private static readonly string _inventoryGridPropertyName = "_inventoryGridOrigin";
    private static readonly string _itemCellPrefabPropertyName = "_itemCellPrefab";
    private static readonly string _itemColorPropertyName = "_itemColor";
    private static readonly string _currentGridItemPropertyName = "_currentGridItem";
    private static readonly string _itemCellsSizeModifierPropertyName = "_itemCellsSizeModifier";
    
    private SerializedProperty _inventoryGridProperty;
    private SerializedProperty _itemCellPrefabProperty;
    private SerializedProperty _itemColorProperty;
    private SerializedProperty _currentGridItemProperty;
    private SerializedProperty _itemCellsSizeModifierProperty;

    private ItemsConstructor _constructor;
    private bool _gridFoldout;

    private void OnEnable()
    {
        _inventoryGridProperty = serializedObject.FindProperty(_inventoryGridPropertyName);
        _itemCellPrefabProperty = serializedObject.FindProperty(_itemCellPrefabPropertyName);
        _itemColorProperty = serializedObject.FindProperty(_itemColorPropertyName);
        _currentGridItemProperty = serializedObject.FindProperty(_currentGridItemPropertyName);
        _itemCellsSizeModifierProperty = serializedObject.FindProperty(_itemCellsSizeModifierPropertyName);
        _constructor = (ItemsConstructor)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_inventoryGridProperty);
        EditorGUILayout.PropertyField(_itemCellPrefabProperty);
        
        InventoryGrid grid = (InventoryGrid)_inventoryGridProperty.objectReferenceValue;
        GameObject prefab = (GameObject)_itemCellPrefabProperty.objectReferenceValue;

        if (grid != null && prefab != null)
        {
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(_itemColorProperty);
            EditorGUILayout.PropertyField(_itemCellsSizeModifierProperty);
            
            bool settingsChanged = EditorGUI.EndChangeCheck();

            if (settingsChanged)
            {
                serializedObject.ApplyModifiedProperties();
                _constructor.RecolorItem();
                _constructor.ResizeItemCellsSizes();
                EditorUtility.SetDirty(_constructor);
            }
            
            DrawGridButtons(grid, _constructor);

            if (_currentGridItemProperty.objectReferenceValue != null)
            {
                Color defaultInspectorColor = GUI.backgroundColor;
                
                EditorGUILayout.Space(10f);
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Release Item", GUILayout.Height(30)))
                {
                    _constructor.ReleaseCurrentItem();
                    EditorUtility.SetDirty(_constructor);
                }
                GUI.backgroundColor = defaultInspectorColor;
                
                EditorGUILayout.Space(10f);
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Clear Item", GUILayout.Height(30)))
                {
                    _constructor.DeleteCurrentItem();
                    EditorUtility.SetDirty(_constructor);
                }
                GUI.backgroundColor = defaultInspectorColor;
            }
        }
        else
        {
            if (_constructor.SelectedCells.Count > 0 || _currentGridItemProperty.objectReferenceValue != null)
            {
                _constructor.DeleteCurrentItem();
                _constructor.ClearSelection();
                EditorUtility.SetDirty(_constructor);
            }
            
            EditorGUILayout.HelpBox("Assign InventoryGrid and ItemCell Prefab to start constructing items.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGridButtons(InventoryGrid grid, ItemsConstructor constructor)
    {
        int rows = grid.Size.y;
        int columns = grid.Size.x;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Draw Item Shape:", EditorStyles.boldLabel);

        Color defaultInspectorColor = GUI.backgroundColor;
        Color baseColor = _itemColorProperty.colorValue;

        for (int r = rows - 1; r >= 0; r--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < columns; c++)
            {
                Vector2Int cellCoordinate = new Vector2Int(r, c);
                bool selected = constructor.SelectedCells.Contains(cellCoordinate);

                GUI.backgroundColor = selected ? baseColor : new Color(0.3f, 0.3f, 0.3f);

                if (GUILayout.Button("", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    Debug.Log($"Selected cell {cellCoordinate}");
                    constructor.ToggleCell(cellCoordinate);
                    EditorUtility.SetDirty(constructor);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = defaultInspectorColor;
    }
}
#endif