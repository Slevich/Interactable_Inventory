using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtefactCostUIManager : MonoBehaviour
{
    #region Fields
    [Header("References.")]
    [SerializeField]
    private ArtefactCostCalculator _artefactCostCalculator;
    [SerializeField]
    private ArtefactsCollector _artefactsCollector;
    [SerializeField] 
    private GridItemsSpawnZone _spawnZone;
    [SerializeField] 
    private ArtefactCostUI _artefactCostUIPrefab;
    [SerializeField]
    private Transform _artefactCostUIParent;
    [SerializeField] 
    private Button _artefactBuyingButton;

    private List<ArtefactCostUI> _currentCost = new();
    private bool _artefactsEnded = false;
    #endregion

    #region Properties
    
    #endregion

    #region Methods
    private void OnEnable()
    {
        if (_artefactCostCalculator != null)
        {
            _artefactCostCalculator.OnCostChangedEvent?.AddListener(UpdateUI);
            _artefactCostCalculator.EnoughResourcesEvent?.AddListener(IsEnoughResourcesForBuying);
        }
        
        if(_artefactBuyingButton  != null && _artefactsCollector != null)
            _artefactBuyingButton.onClick.AddListener(_artefactsCollector.AddNewArtefact);
        
        if(_artefactsCollector != null)
            _artefactsCollector.HasNextArtefactEvent.AddListener(ArtefactsEnded);
    }

    private void OnDisable()
    {
        if (_artefactCostCalculator != null)
        {
            _artefactCostCalculator.OnCostChangedEvent?.RemoveListener(UpdateUI);
            _artefactCostCalculator.EnoughResourcesEvent?.RemoveListener(IsEnoughResourcesForBuying);
        }
        
        if(_artefactBuyingButton  != null && _artefactsCollector != null)
            _artefactBuyingButton.onClick.RemoveListener(_artefactsCollector.AddNewArtefact);
        
        if(_artefactsCollector != null)
            _artefactsCollector.HasNextArtefactEvent.RemoveListener(ArtefactsEnded);
    }

    private void ArtefactsEnded(bool state)
    {
        _artefactsEnded = state;

        if (!_artefactsEnded)
        {
            Debug.Log("ArtefactsEnded!");
            
            if (_artefactCostCalculator != null)
            {
                _artefactCostCalculator.OnCostChangedEvent?.RemoveListener(UpdateUI);
                _artefactCostCalculator.EnoughResourcesEvent?.RemoveListener(IsEnoughResourcesForBuying);
            }
            
            if(_artefactsCollector != null)
                _artefactsCollector.HasNextArtefactEvent.RemoveListener(ArtefactsEnded);
            
            if (_artefactBuyingButton != null && _artefactsCollector != null)
            {
                _artefactBuyingButton.onClick.RemoveListener(_artefactsCollector.AddNewArtefact);
                TextMeshProUGUI buttonText = (TextMeshProUGUI)ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(_artefactBuyingButton.gameObject, typeof(TextMeshProUGUI));
                buttonText.text = "Create new item!";
                
                if(_spawnZone != null)
                    _artefactBuyingButton.onClick.AddListener(_spawnZone.SpawnRandomItem);
            }
            
            if(_artefactCostUIParent != null)
                _artefactCostUIParent.gameObject.SetActive(false);
        }
    }
    
    public void IsEnoughResourcesForBuying(bool ResourcesEnough)
    {
        if(_artefactBuyingButton != null)
            _artefactBuyingButton.interactable = ResourcesEnough;
    }
    
    public void UpdateUI(ResourceAmount[] NewCost)
    {
        if (_currentCost != null && _currentCost.Count > 0)
        {
            for (int i = _currentCost.Count - 1; i >= 0; i--)
            {
                ArtefactCostUI resourceCost = _currentCost[i];
                Destroy(resourceCost.gameObject);
                _currentCost.RemoveAt(i);
            }
        }

        foreach (ResourceAmount resourceCost in NewCost)
        {
            if(resourceCost.CurrentAmount == 0)
                continue;
            
            ArtefactCostUI newResourceCost = Instantiate(_artefactCostUIPrefab, _artefactCostUIParent != null ? _artefactCostUIParent : transform);
            newResourceCost.CostText.text = resourceCost.CurrentAmount.ToString();
            _currentCost.Add(newResourceCost);
            Sprite resourceSprite = null;
            
            switch (resourceCost.ResourceType)
            {
                case ProductionSettings.ResourceType.Wheat:
                    resourceSprite = GlobalSpritesContainer.Instance.WheatSprite;
                    break;
                
                case ProductionSettings.ResourceType.Wood:
                    resourceSprite = GlobalSpritesContainer.Instance.WoodSprite;
                    break;
                
                case ProductionSettings.ResourceType.Iron:
                    resourceSprite = GlobalSpritesContainer.Instance.IronSprite;
                    break;
            }
            
            newResourceCost.ResourceImage.sprite = resourceSprite;
        }
    }
    #endregion
}
