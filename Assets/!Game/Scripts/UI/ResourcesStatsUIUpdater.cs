using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResourcesStatsUIUpdater : MonoBehaviour
{
    #region Fields
    [Header("References")] 
    [SerializeField]
    private ResourceProduction _production;
    [SerializeField]
    private ResourceTextBinding[] _bindings = Array.Empty<ResourceTextBinding>();
    #endregion

    #region Methods
    private void OnEnable()
    {
        if (_production != null)
            _production.OnResourceAmountChangedEvent.AddListener(UpdateTextForResource);
    }

    private void OnDisable()
    {
        if (_production != null)
            _production.OnResourceAmountChangedEvent.RemoveListener(UpdateTextForResource);
    }

    private void UpdateTextForResource(ProductionSettings.ResourceType ResourceType, int Amount)
    {
        if (_bindings == null || _bindings.Length == 0)
            return;

        ResourceTextBinding matchedResourceTypeBinding = _bindings.First(binding => binding.Resource == ResourceType);
        
        if(matchedResourceTypeBinding == null)
            return;
        
        if(matchedResourceTypeBinding.Text == null)
            return;
        
        matchedResourceTypeBinding.Text.text = Amount.ToString();
    }
    #endregion
}

[Serializable]
public class ResourceTextBinding
{
    [field: SerializeField]
    public ProductionSettings.ResourceType Resource { get; private set; } = ProductionSettings.ResourceType.Wheat;
    [field: SerializeField]
    public TextMeshProUGUI Text {get; private set;}
}