using System.Collections.Generic;
using UnityEngine;

public static class ProductionSettings
{
    public static readonly Dictionary<TileType, Color> TileColors = new Dictionary<TileType, Color>()
    {
        { TileType.Red, Color.red },
        { TileType.Yellow, Color.yellow },
        { TileType.Green, Color.green }
    };
    
    public enum TileType
    {
        Red,
        Yellow,
        Green,
    }
    
    public enum ResourceType
    {
        Wheat,
        Wood,
        Iron
    }

    public static float CalculateTotalTimeToProduct(int EffectivityPercents)
    {
        if (EffectivityPercents <= 0)
            return Mathf.Infinity;

        float totalTime = 100f / EffectivityPercents;
        return totalTime;
    }
}