using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class ResourceProduction : MonoBehaviour
{
    #region Fields
    [Header("References")]
    [SerializeField, ReadOnly]
    private InventoryGrid _grid = null;
    [Header("Display.")]
    [SerializeField, ReadOnly]
    private List<ResourceAmount> _productionStats = new ();
    
    private List<ProductionCell> _productionCells = new ();
    private ActionUpdate _update;
    private float _additionResourceTimer = 0f;
    #endregion

    #region Properties
    public UnityEvent<ProductionSettings.ResourceType, int> OnResourceAmountChangedEvent { get; set; } = new ();
    public UnityEvent<ResourceAmount[]> OnResourcesChangedEvent { get; set; } = new ();
    public UnityEvent<ProductionCell> OnNewProductionCellEvent { get; set; } = new ();
    public ProductionCell[] ProductionCells => _productionCells.ToArray();
    public InventoryGrid Grid => _grid;
    public List<ResourceAmount> CurrentResources => _productionStats;
    public bool PlusResource { get; set; }
    #endregion
    
    #region Methods
    #region Unity methods
    private void Awake()
    {
        if(_grid == null)
            return;

        CreateProductionStats();
        _grid.BusyCellsChanged.Subscribe(cells => UpdateProductionCells(cells)).AddTo(this);
        _update = new ActionUpdate();
    }

    private void Start()
    {
        OnResourcesChangedEvent?.Invoke(_productionStats.ToArray());
    }

    private void OnDisable() => _update.StopUpdate();
    #endregion

    #region Updaters
    private void UpdateProductionCells(List<GridCell> BusyCells)
    {
        if (BusyCells == null || BusyCells.Count == 0)
        {
            if(_productionStats.Count > 0)
                _productionCells.Clear();
            return;
        }
        
        List<GridCell> productiveCells = BusyCells.Where(cell => cell.IsProductive).ToList();
        
        if (productiveCells.Count == 0)
        {
            _productionCells.Clear();
            return;
        }
        else
        {
            _productionCells.RemoveAll(prodCell => prodCell == null || prodCell.ConnectedGridCell == null || !productiveCells.Contains(prodCell.ConnectedGridCell));   
        }
        
        foreach (GridCell productiveCell in productiveCells)
        {
            bool alreadyExists = _productionCells.Any(prodCell => prodCell.ConnectedGridCell == productiveCell);

            if (!alreadyExists)
            {
                ProductionCell newProdCell = new ProductionCell(productiveCell, productiveCell.ProductionSettings.EffectivePercentage, productiveCell.GridItem.ResourceType);
                _productionCells.Add(newProdCell);
                productiveCell.GridItem.PreparePopups(new [] {productiveCell.GridItem.ResourceType});
                productiveCell.GridItem.UpdateResourceAmount(productiveCell.GridItem.ResourceType, newProdCell.PrimaryResource.AmountPerTime);
                productiveCell.GridItem.Cells.ForEach(cell => cell.Animation.ModifySpeed((float)productiveCell.ProductionSettings.EffectivePercentage / 100f));
                OnNewProductionCellEvent?.Invoke(newProdCell);
            }
            else
            {
                ProductionSettings.ResourceType productiveCellItemResourceType = productiveCell.GridItem.ResourceType;
                ProductionCell productionCell = _productionCells.First(prodCell => prodCell.ConnectedGridCell == productiveCell);
                productiveCell.GridItem.UpdateResourceAmount(productiveCell.GridItem.ResourceType, productionCell.PrimaryResource.AmountPerTime);

                if (productiveCellItemResourceType != productionCell.PrimaryResourceType)
                {
                    productionCell = new ProductionCell(productiveCell, productiveCell.ProductionSettings.EffectivePercentage, productiveCell.GridItem.ResourceType);
                    OnNewProductionCellEvent?.Invoke(productionCell);
                }
            }
        }

        if(!_update.Busy && _productionCells.Count > 0)
            _update.StartUpdate(ResourceProductionExecution);
        else if(_update.Busy && _productionCells.Count == 0)
            _update.StopUpdate();
    }

    private void CreateProductionStats()
    {
        _productionStats.Clear();

        Array resourceTypeEnumValues = Enum.GetValues(typeof(ProductionSettings.ResourceType));

        foreach (ProductionSettings.ResourceType resourceType in resourceTypeEnumValues)
        {
            ResourceAmount newStat = new ResourceAmount(resourceType);
            _productionStats.Add(newStat);
        }
    }
    #endregion

    private void ResourceProductionExecution()
    {
        if(_productionCells.Count == 0)
            return;

        for (int i = _productionCells.Count - 1; i >= 0; i--)
        {
            ProductionCell productionCell = _productionCells[i];
            if(productionCell.EffectivityPercentage == 0)
                continue;
            
            productionCell.CurrentTime += (Time.deltaTime * TimeScaler.TimeScale);
            bool productCreated = productionCell.CheckForProduct();
            
            if (productCreated)
            {
                productionCell.CurrentTime = 0f;
                ResourceAmount[] products = productionCell.ProductNewResources();
                
                if(products == null || products.Length == 0)
                    continue;

                foreach (ResourceAmount product in products)
                {
                    ProductionSettings.ResourceType productionCellResourceType = product.ResourceType;
                    ResourceAmount stat = _productionStats.First(stat => stat.ResourceType == productionCellResourceType);
                    stat.IncrementAmount(product.CurrentAmount);
                    OnResourceAmountChangedEvent?.Invoke(productionCellResourceType, stat.CurrentAmount);
                    productionCell.ConnectedGridCell.GridItem.PlayPopUpAnimation();
                }
                
                OnResourcesChangedEvent?.Invoke(_productionStats.ToArray());
                
                if (productionCell.IsDestroyed)
                {
                    _productionCells.RemoveAt(i);
                    _grid.DestroyItem(productionCell.ConnectedGridCell.GridItem);
                }
            }
        }

        if (PlusResource)
        {
            _additionResourceTimer += Time.deltaTime;

            if (_additionResourceTimer >= 1f)
            {
                _additionResourceTimer = 0f;
                
                Array resources = Enum.GetValues(typeof(ProductionSettings.ResourceType));
                int randomResourceIndex = Random.Range(0, resources.Length);
                ProductionSettings.ResourceType selectedResource = (ProductionSettings.ResourceType)resources.GetValue(randomResourceIndex);
                ResourceAmount stat = _productionStats.First(stat => stat.ResourceType == selectedResource);
                stat.IncrementAmount(1);
            }
        }
    }
    
    #region Setters
    public void SetGrid(InventoryGrid Grid) => _grid = Grid;
    #endregion
    #endregion
}

public class ProductionCell
{
    #region Fields
    private GridCell _connectedGridCell;
    private List<PossibleResource> _resourcesProducts = new ();
    #endregion

    #region Properties

    public float TimeToProduct { get; set; } = 0f;
    public float CurrentTime { get; set; } = 0f;
    public int EffectivityPercentage { get; private set; } = 0;
    public float DestroyPossibility { get; set; } = 0f;
    public bool IsDestroyed { get; private set; } = false;

    public ProductionSettings.ResourceType PrimaryResourceType => _resourcesProducts.First().ResourceType;
    public ProductionSettings.ResourceType[] AllResourceTypes => _resourcesProducts.Select(product => product.ResourceType).ToArray();
    public PossibleResource PrimaryResource => _resourcesProducts.First();

    public GridCell ConnectedGridCell => _connectedGridCell;
    #endregion

    #region Constructor
    public ProductionCell(GridCell ConnectedGridCell, int NewEffectivityPercentage, ProductionSettings.ResourceType newPrimaryResourceType)
    {
        _connectedGridCell = ConnectedGridCell;
        EffectivityPercentage = NewEffectivityPercentage;
        _resourcesProducts.Add(new PossibleResource(newPrimaryResourceType, 1f, 1));
        TimeToProduct = ProductionSettings.CalculateTotalTimeToProduct(EffectivityPercentage);
    }
    #endregion
    
    #region Methods
    public bool CheckForProduct()
    {
        return CurrentTime >= TimeToProduct;
    }

    public void RecalculateEffectivity(int NewEffectivityPercentage)
    {
        EffectivityPercentage = NewEffectivityPercentage;
        TimeToProduct = ProductionSettings.CalculateTotalTimeToProduct(EffectivityPercentage);
    }

    public void UpdatePrimaryResourceType(ProductionSettings.ResourceType NewResourceType)
    {
        if(_resourcesProducts == null || _resourcesProducts.Count == 0)
            return;
        
        PossibleResource primaryResource = _resourcesProducts.First();
        primaryResource.ResourceType = NewResourceType;
        _resourcesProducts[0] = primaryResource;
    }
    
    public PossibleResource AddNewPossibleResource(ProductionSettings.ResourceType NewResourceType, float Possibility, int AmountPerTime)
    {
        if(_resourcesProducts.Any(resource => resource.ResourceType == NewResourceType))
            return null;
        
        PossibleResource newPossibleResource = new PossibleResource(NewResourceType, Possibility, AmountPerTime);
        _resourcesProducts.Add(newPossibleResource);
        List<ProductionSettings.ResourceType> resources = new();
        resources.Add(PrimaryResourceType);
        resources.AddRange(_resourcesProducts.Select(resource => resource.ResourceType));
        return newPossibleResource;
    }

    public ResourceAmount[] ProductNewResources()
    {
        List<ResourceAmount> newProducts = new List<ResourceAmount>();
        if(_resourcesProducts ==  null || _resourcesProducts.Count == 0)
            return newProducts.ToArray();

        foreach (PossibleResource possibleResource in _resourcesProducts)
        {
            if (possibleResource.Probability < 1f)
            {
                float probability = possibleResource.Probability;
                float randomValue = Random.Range(0f, 1f);
            
                if(randomValue > probability)
                    continue;
            }
            
            ResourceAmount possibleProduct = new ResourceAmount(possibleResource.ResourceType);
            possibleProduct.SetAmount(possibleResource.AmountPerTime);
            newProducts.Add(possibleProduct);
        }

        if (DestroyPossibility > 0f)
        {
            float randomValue = Random.Range(0f, 1f);

            if (randomValue <= DestroyPossibility)
            {
                IsDestroyed = true;
            }
        }
        
        return newProducts.ToArray();
    }
    #endregion
}

[Serializable]
public class ResourceAmount
{
    #region Fields
    [SerializeField]
    private ProductionSettings.ResourceType _resourceType = ProductionSettings.ResourceType.Wheat;
    [SerializeField, Range(0, 1000)]
    private int _currentAmount = 0;
    #endregion
    
    #region Properties
    public ProductionSettings.ResourceType ResourceType => _resourceType;
    public int CurrentAmount => _currentAmount;
    #endregion

    #region Constructor
    public ResourceAmount(ProductionSettings.ResourceType ResourceType)
    {
        _resourceType = ResourceType;
        _currentAmount = 0;
    }
    #endregion
    
    #region Methods
    public void SetAmount(int Amount)
    {
        int plusAmount = Mathf.Abs(Amount);
        _currentAmount = Amount;
    }

    public void IncrementAmount(int Amount)
    {
        int incrementedAmount = Mathf.Abs(Amount);
        _currentAmount += incrementedAmount;
    }

    public void DecrementAmount(int Amount)
    {
        int decrementedAmount = Mathf.Abs(Amount);
        
        if(_currentAmount == 0)
            return;
        
        int currentAmount = _currentAmount - decrementedAmount;
        
        if(currentAmount < 0)
            currentAmount = 0;
        
        _currentAmount = currentAmount;
    }
    #endregion
}

public class PossibleResource
{
    #region Properties
    public ProductionSettings.ResourceType ResourceType { get; set; }
    public float Probability { get; private set; }
    public int AmountPerTime { get; set; }
    #endregion
    
    #region Constructor

    public PossibleResource(ProductionSettings.ResourceType NewResourceType, float NewProbability, int NewAmountPerTime)
    {
        ResourceType = NewResourceType;
        Probability = NewProbability;
        AmountPerTime = NewAmountPerTime;
    }
    #endregion
}