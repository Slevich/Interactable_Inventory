using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ArtefactCostCalculator : MonoBehaviour
{
    #region Fields
    [Header("References.")]
    [SerializeField]
    private ArtefactsCollector artefactsCollector;
    [SerializeField]
    private ResourceProduction _resourceProduction;
    [Space(15f)]
    [Header("Settings.")]
    [SerializeField]
    private ResourceAmount[] _currentCost = Array.Empty<ResourceAmount>();
    [SerializeField, Range(0, 100)] 
    private int _costIncreasePercentage = 20;
    #endregion

    #region Properties
    public UnityEvent<ResourceAmount[]> OnCostChangedEvent { get; set; } = new UnityEvent<ResourceAmount[]>();
    public UnityEvent<bool> EnoughResourcesEvent { get; set; } = new UnityEvent<bool>();
    #endregion
    
    #region Methods
    private void OnEnable()
    {
        if(artefactsCollector != null)
            artefactsCollector.OnNewArtefactEvent.AddListener(IncreaseCost);
        
        if(_resourceProduction != null)
            _resourceProduction.OnResourcesChangedEvent.AddListener(CheckResourcesAvailability);
    }

    private void OnDisable()
    {
        if(artefactsCollector != null)
            artefactsCollector.OnNewArtefactEvent.RemoveListener(IncreaseCost);
        
        if(_resourceProduction != null)
            _resourceProduction.OnResourcesChangedEvent.RemoveListener(CheckResourcesAvailability);
    }

    private void Start()
    {
        OnCostChangedEvent?.Invoke(_currentCost);
        EnoughResourcesEvent?.Invoke(false);
    }

    private void CheckResourcesAvailability(ResourceAmount[] CurrentResources)
    {
        if(_currentCost == null || _currentCost.Length == 0)
            return;
        
        if(CurrentResources == null || CurrentResources.Length == 0)
            return;

        int matches = 0;

        foreach (ResourceAmount resourceAmount in _currentCost)
        {
            int amount = resourceAmount.CurrentAmount;
            ProductionSettings.ResourceType resourceType = resourceAmount.ResourceType;
            
            if(CurrentResources.First(resource => resource.ResourceType == resourceType).CurrentAmount >= amount)
                matches++;
        }
        
        EnoughResourcesEvent?.Invoke(matches == _currentCost.Length);
    }
    
    public void IncreaseCost()
    {
        if(_currentCost ==  null || _currentCost.Length == 0)
            return;
        
        foreach (ResourceAmount resourceAmount in _currentCost)
        {
            if (_resourceProduction)
            {
                _resourceProduction.CurrentResources.First(resource => resource.ResourceType == resourceAmount.ResourceType).DecrementAmount(resourceAmount.CurrentAmount);
            }
            
            int currentResourceAmount = resourceAmount.CurrentAmount;
            float multiplier = 1f + (_costIncreasePercentage / 100f);
            int newCost = Mathf.CeilToInt((float)currentResourceAmount * multiplier);
            resourceAmount.SetAmount(newCost);
        }
        
        OnCostChangedEvent?.Invoke(_currentCost);
    }
    #endregion
}
