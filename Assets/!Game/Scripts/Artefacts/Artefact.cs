using UnityEngine;

[SerializeField]
public class Artefact
{
    #region Fields
    
    #endregion
    
    #region Properties
    [field: Header("Settings.")]
    [field: SerializeField]
    public ArtefactType Type { get; private set; } = ArtefactType.Wheat;
    #endregion

    #region Methods

    #endregion
}

public enum ArtefactType
{
    Wheat,
    Wood,
    Iron,
    General,
    GreenEffectivity
}