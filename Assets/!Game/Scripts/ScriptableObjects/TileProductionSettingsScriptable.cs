using UnityEngine;

[CreateAssetMenu(fileName = "TileProductionSettingsScriptable", menuName = "Scriptable Objects/TileProductionSettingsScriptable")]
public class TileProductionSettingsScriptable : ScriptableObject
{
    [field: SerializeField] 
    public ProductionSettings.TileType TileType { get; set; } = ProductionSettings.TileType.Red;
    [field: SerializeField, Range(0, 100)]
    public int EffectivePercentage { get; set; } = 100;
}
