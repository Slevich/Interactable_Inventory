using UnityEngine;

public interface IInteractableZone
{
    public string GetName();
    public Bounds GetBounds();
    
    public bool TryToDragItemFromZone(Vector3 PointerPosition, out IInteractableItem DraggedItem);
    public void TryToDropItemIntoZone(IInteractableItem DroppedItem);

    public void OnPointerEnter();
    public void OnPointerExit();

    public void OnInteractableItemEnter(IInteractableItem Item);
    public void OnInteractableItemExit(IInteractableItem Item);
}