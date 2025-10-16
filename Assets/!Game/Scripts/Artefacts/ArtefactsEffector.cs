using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ArtefactsEffector
{
    #region Fields
    private ResourceProduction _production;
    private ArtefactsCollector _artefactsCollector;
    #endregion

    #region Constructors
    public ArtefactsEffector(ResourceProduction Production, ArtefactsCollector ArtefactsCollector)
    {
        _production = Production;
        _artefactsCollector = ArtefactsCollector;
    }
    #endregion
    
    #region Methods
    public void Subscribe()
    {
        if(_production != null)
            _production.OnNewProductionCellEvent?.AddListener(ApplyArtefactEffectForSingle);
    }

    public void Dispose()
    {
        if(_production != null)
            _production.OnNewProductionCellEvent?.RemoveListener(ApplyArtefactEffectForSingle);
    }
    
    public void ApplyArtefactEffectsForAll(ArtefactType ArtefactType)
    {
        if(_production == null)
            return;
        
        if(_production.ProductionCells == null || _production.ProductionCells.Length == 0)
            return;
        
        switch (ArtefactType)
        {
            case ArtefactType.Wheat:
                IEnumerable<ProductionCell> wheatCells = _production.ProductionCells.Where(cell => cell.PrimaryResourceType == ProductionSettings.ResourceType.Wheat);
        
                if(wheatCells.Count() == 0)
                    return;

                foreach (ProductionCell effectedCell in wheatCells)
                {
                    ApplyWheatArtefactEffectToCell(effectedCell);
                }
                break;
            
            case ArtefactType.Wood:
                IEnumerable<ProductionCell> woodCells = _production.ProductionCells.Where(cell => cell.PrimaryResourceType == ProductionSettings.ResourceType.Wood);
        
                if(woodCells.Count() == 0)
                    return;
        
                foreach (ProductionCell effectedCell in woodCells)
                {
                    ApplyWoodArtefactEffectForCell(effectedCell);
                }
                break;
            
            case ArtefactType.Iron:
                
                IEnumerable<ProductionCell> ironCells = _production.ProductionCells.Where(cell => cell.PrimaryResourceType == ProductionSettings.ResourceType.Iron);
        
                if(ironCells.Count() == 0)
                    return;
                
                foreach (ProductionCell effectedCell in ironCells)
                {
                    ApplyIronArtefactEffectToCell(effectedCell);
                }
                
                break;
            
            case ArtefactType.General:
                bool moreThan3Items = _production.Grid.Items.Count >= 3;
                
                if(moreThan3Items)
                    ApplyGeneralArtefactEffect();
                break;
            
            case ArtefactType.GreenEffectivity:
                IEnumerable<ProductionCell> greenCells = _production.ProductionCells.Where(cell => cell.ConnectedGridCell.ProductionSettings.TileType == ProductionSettings.TileType.Green);
                
                if(greenCells.Count() == 0)
                    return;

                foreach (ProductionCell effectedCell in greenCells)
                {
                    ApplyGreenEffectivityArtefactEffect(effectedCell);
                }
                break;
        }
    }

    public void ApplyArtefactEffectForSingle(ProductionCell EffectedCell)
    {
        if(EffectedCell == null)
            return;
        
        switch (EffectedCell.PrimaryResourceType)
        {
            case ProductionSettings.ResourceType.Wheat:
                if(_artefactsCollector.CurrentArtefacts.Contains(ArtefactType.Wheat))
                    ApplyWheatArtefactEffectToCell(EffectedCell);
                break;
            
            case ProductionSettings.ResourceType.Wood:
                if(_artefactsCollector.CurrentArtefacts.Contains(ArtefactType.Wood))
                    ApplyWoodArtefactEffectForCell(EffectedCell);
                break;
            
            case ProductionSettings.ResourceType.Iron:
                if(_artefactsCollector.CurrentArtefacts.Contains(ArtefactType.Iron))
                    ApplyIronArtefactEffectToCell(EffectedCell);
                break;
        }
        
        if(_artefactsCollector.CurrentArtefacts.Contains(ArtefactType.GreenEffectivity) && EffectedCell.ConnectedGridCell.ProductionSettings.TileType == ProductionSettings.TileType.Green)
            ApplyGreenEffectivityArtefactEffect(EffectedCell);
    }

    #region Effects methods
    private void ApplyWheatArtefactEffectToCell(ProductionCell effectedCell)
    {
        if(effectedCell == null)
            return;
        
        int currentAmountPerTime = effectedCell.PrimaryResource.AmountPerTime;
        int itemCellsAmount = effectedCell.ConnectedGridCell.GridItem.Cells.Count;
        currentAmountPerTime += itemCellsAmount;
        effectedCell.PrimaryResource.AmountPerTime = currentAmountPerTime;
        effectedCell.ConnectedGridCell.GridItem.UpdateResourceAmount(ProductionSettings.ResourceType.Wheat, currentAmountPerTime);
    }

    private void ApplyWoodArtefactEffectForCell(ProductionCell effectedCell)
    {
        if(effectedCell == null)
            return;
        
        PossibleResource newResource = effectedCell.AddNewPossibleResource(ProductionSettings.ResourceType.Wheat, 0.5f, effectedCell.PrimaryResource.AmountPerTime);
        effectedCell.ConnectedGridCell.GridItem.PreparePopups(effectedCell.AllResourceTypes);
        effectedCell.ConnectedGridCell.GridItem.UpdateResourceAmount(ProductionSettings.ResourceType.Wheat, newResource.AmountPerTime);
    }
    
    private void ApplyIronArtefactEffectToCell(ProductionCell effectedCell)
    {
        if(effectedCell == null)
            return;

        effectedCell.PrimaryResource.AmountPerTime *= 10;
        effectedCell.DestroyPossibility = 0.1f;
        effectedCell.ConnectedGridCell.GridItem.UpdateResourceAmount(ProductionSettings.ResourceType.Iron, effectedCell.PrimaryResource.AmountPerTime);
    }
    
    private void ApplyGeneralArtefactEffect()
    {
        if(_production == null)
            return;

        _production.PlusResource = true;
    }

    private void ApplyGreenEffectivityArtefactEffect(ProductionCell effectedCell)
    {
        if(effectedCell == null)
            return;
        
        effectedCell.RecalculateEffectivity(effectedCell.EffectivityPercentage + 25);
    }
    #endregion
    #endregion
}
