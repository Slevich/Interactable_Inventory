using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class InventoryGrid : MonoBehaviour
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

    public List<GridCell> Cells => _cells;
    #endregion

    #region Fields
    [Header("Container.")]
    [SerializeField]
    private List<GridCell> _cells = new ();
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
    
    #region Setters
    
    #endregion
    
    #if UNITY_EDITOR
    #region Gizmos
    private void OnDrawGizmosSelected() => DrawGridGizmos();
    
    public void DrawGridGizmos()
    {
        // Gizmos.color = Color.blue;
        // Vector3 gridCenter = BodyCenterWorld;
        // Gizmos.DrawSphere(gridCenter, 0.15f);
        //
        // if (IsNoCells)
        //     return;
        //
        // Vector2 cellSize = _cells.First().WorldCellSize;
        //
        // foreach (GridCell cell in _cells)
        // {
        //     Gizmos.color = Color.magenta;
        //     Gizmos.DrawSphere(cell.WorldPosition, 0.08f);
        //     
        //     Vector3 center = cell.WorldPosition;
        //     Vector3 top = center + Vector3.up * (cellSize.y / 2);
        //     Vector3 bottom = center - Vector3.up * (cellSize.y / 2);
        //     Vector3 left = center - Vector3.right * (cellSize.x / 2);
        //     Vector3 right = center + Vector3.right * (cellSize.x / 2);
        //
        //     Gizmos.color = Color.green;
        //     Gizmos.DrawLine(bottom, top);
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawLine(left, right);
        // }
    }
    #endregion
    #endif
    #endregion
}

[Serializable]
public struct GridLine
{
    [field: SerializeField] public int CellsAmount { get; private set; }
}