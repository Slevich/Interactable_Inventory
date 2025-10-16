using UnityEngine;

public interface IInteractableItem
{
    public Transform Parent { get; set; }
    public bool Active { get; set; }
    public IInteractableZone StartZone { get; set; }
    public IInteractableZone EndZone { get; set; }
    

    public string GetName();

    public Bounds[] GetCellsBounds();
    public ItemCell[] GetCells();
    
    public void RandomizePrimaryCell();
    
    public void OnDrag();
    
    public void OnDrop();
    
    public void Initialize();
    public void Destroy();

    public void UpdateResourceSprite();
    public void PreparePopups(ProductionSettings.ResourceType[] Resources);
    public void UpdateResourceAmount(ProductionSettings.ResourceType Resource, int Amount);
    public void StartPlayingWorkAnimation();
    public void StopPlayingWorkAnimation();
}
