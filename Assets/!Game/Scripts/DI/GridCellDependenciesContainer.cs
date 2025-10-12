using UnityEngine;

public class GridCellDependenciesContainer : DependenciesContainerBase
{
    #region Fields
    [field: Header("Unity components.")]
    [field: SerializeField]
    public SpriteRenderer Graphic { get; set; }
    
    // [field: Header("Custom components.")]
    // [field: SerializeField]
    // public GridCell _gridCell;
    #endregion

    #region Methods
    public override void InjectDependencies() => Execute<GridCellDependenciesContainer>();
    #endregion
}
