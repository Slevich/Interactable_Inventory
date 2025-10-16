using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class GridItem : MonoBehaviour, IInteractableItem /*, IDependenciesInjection<UnitDependenciesContainer>*/
{
    #region Fields
    private IAnimation _animation;
    private static readonly float _destroyTime = 0.5f;
    private CancellationTokenSource _cancellationTokenSource;
    #endregion

    #region Properties
    [field: Header("References.")]
    [field: SerializeField]
    public Transform CellsParent { get; set; }

    [field: Header("Container.")]
    [field: SerializeField]
    public List<ItemCell> Cells { get; set; } = new ();

    [field: Header("Settings.")]
    [field: SerializeField, ReadOnly]
    public ProductionSettings.ResourceType ResourceType { get; set; } = ProductionSettings.ResourceType.Wheat;

    public Transform Parent { get; set; }
    [field: SerializeField]
    public bool Active { get; set; } = true;
    public GridItemResourceSpriteUpdater ResourceSpriteUpdater { get; set; }
    public IInteractableZone StartZone { get; set; }
    public IInteractableZone EndZone { get; set; }
    public Vector3 StartPosition { get; set; } = Vector3.zero;
    public UnityEvent OnDestroyEvent { get; set; } = new UnityEvent(); 
    
    private ItemCell _primaryCell = null;
    #endregion
    
    #region Methods

    #region Unity methods
    private void Awake()
    {
        Parent = transform;
        Active = true;
    }
    
    private void OnDisable()
    {
        if(_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            _cancellationTokenSource.Cancel();
    }
    #endregion

    #region Lifetime
    public void Initialize()
    {
        Component animationComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(gameObject, typeof(IAnimation));
        
        if(animationComponent == null)
            return;
        
        _animation = (IAnimation)animationComponent;
        
        if(_animation == null)
            return;

        RandomizePrimaryCell();

        if (ResourceSpriteUpdater != null)
        {
            ResourceSpriteUpdater.UpdateSprite(ResourceType);
            ResourceSpriteUpdater.PlaceSpriteOnCell(_primaryCell);
        }
        
        _animation.PlayForward();
    }

    public async void Destroy()
    {
        Active = false;
        
        if(_animation != null)
            _animation.PlayBackward();
        
        if(_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            _cancellationTokenSource = new CancellationTokenSource();

        OnDestroyEvent?.Invoke();
        
        try
        {
            await UniTask.WaitForSeconds(_destroyTime / TimeScaler.TimeScale, cancellationToken: _cancellationTokenSource.Token);
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
        
        OnDestroyEvent?.RemoveAllListeners();
        
        if(gameObject != null)
            Destroy(gameObject);
    }
    #endregion

    #region Setters
    public void SetCellsSizesWithModifier(float SizeModifier)
    {
        if(Cells == null || Cells.Count == 0)
            return;
        
        foreach (ItemCell itemCell in Cells)
        {
            itemCell.SetNewSizeWithModifier(SizeModifier);
        }
    }

    public void SetNewCellsColors(Color NewColor)
    {
        if(Cells == null || Cells.Count == 0)
            return;
        
        foreach (ItemCell itemCell in Cells)
        {
            itemCell.SetColor(NewColor);
        }
    }
    #endregion

    #region IInteractableItem
    public string GetName() => transform != null ? transform.name : string.Empty;

    public ItemCell[] GetCells() => Cells.ToArray();
    
    public Bounds[] GetCellsBounds()
    {
        if(CellsParent == null || Cells.Count == 0)
            return Array.Empty<Bounds>();

        List<Bounds> bounds = new ();
        
        foreach (ItemCell cell in Cells)
        {
            Component cellSpriteRendererComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(cell.gameObject, typeof(SpriteRenderer));
            
            if(cellSpriteRendererComponent == null)
                continue;
            
            SpriteRenderer spriteRenderer = (SpriteRenderer)cellSpriteRendererComponent;
            Vector3 center = spriteRenderer.bounds.center;
            Vector3 size = new Vector3(cell.OriginalSize.x, cell.OriginalSize.y, spriteRenderer.bounds.size.z);
            Bounds cellBounds = new Bounds(center, size);
            bounds.Add(cellBounds);
        }
        
        return bounds.ToArray();
    }

    public void RandomizePrimaryCell()
    {
        if(Cells == null || Cells.Count == 0)
            return;
        
        int primaryCellRandomIndex = Random.Range(0, Cells.Count);

        for (int i = 0; i < Cells.Count; i++)
        {
            Cells[i].IsPrimary = i == primaryCellRandomIndex;
            
            if(Cells[i].IsPrimary)
                _primaryCell = Cells[i];
        }
    }

    public void OnDrag()
    {
        if(Cells.Count == 0)
            return;

        foreach (ItemCell cell in Cells)
        {
            bool hasRenderer = cell.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer);

            if (hasRenderer)
            {
                spriteRenderer.sortingOrder = 10;
            }
        }
    }

    public void OnDrop()
    {
        if(Cells.Count == 0)
            return;

        foreach (ItemCell cell in Cells)
        {
            bool hasRenderer = cell.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer);

            if (hasRenderer)
            {
                spriteRenderer.sortingOrder = 1;
            }
        }
    }

    public void PreparePopups(ProductionSettings.ResourceType[] Resources)
    {
        if(ResourceSpriteUpdater == null)
            return;
        
        ResourceSpriteUpdater.PreparePopups(Resources);
    }
    
    public void UpdateResourceSprite()
    {
        if(ResourceSpriteUpdater == null)
            return;
        
        ResourceSpriteUpdater.PlaceSpriteOnCell(_primaryCell);
    }

    public void PlayPopUpAnimation()
    {
        if(ResourceSpriteUpdater == null)
            return;
        
        ResourceSpriteUpdater.PlayPopupAnimation();
    }

    public void UpdateResourceAmount(ProductionSettings.ResourceType Resource, int Amount)
    {
        if(ResourceSpriteUpdater == null)
            return;
        
        ResourceSpriteUpdater.UpdateResourceAmount(Resource, Amount);
    }

    public void StartPlayingWorkAnimation()
    {
        if(Cells == null || Cells.Count == 0)
            return;

        foreach (ItemCell cell in Cells)
        {
            Component animationComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(cell.gameObject, typeof(IAnimation));
            if(animationComponent == null)
                continue;
            IAnimation animation = animationComponent as IAnimation;
            animation.PlayForward();
        }
    }

    public void StopPlayingWorkAnimation()
    {
        if(Cells == null || Cells.Count == 0)
            return;

        foreach (ItemCell cell in Cells)
        {
            if(cell == null)
                return;
            
            Component animationComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(cell.gameObject, typeof(IAnimation));
            if(animationComponent == null)
                continue;
            IAnimation animation = animationComponent as IAnimation;
            animation.StopAnimation();
        }
    }
    #endregion
    #endregion
}
