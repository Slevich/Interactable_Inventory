using System;
using UnityEngine;

public class GlobalSpritesContainer : MonoBehaviour
{
    #region Fields
    private static GlobalSpritesContainer _instance;
    #endregion
    
    #region Properties
    public static GlobalSpritesContainer Instance
    {
        get
        {
            if(_instance == null)
                _instance = FindFirstObjectByType<GlobalSpritesContainer>();
            
            return _instance;
        }
    }
    
    [field: Header("References.")]
    [field: SerializeField]
    public Sprite WheatSprite { get; private set; }
    [field: SerializeField]
    public Sprite WoodSprite { get; private set; }
    [field: SerializeField]
    public Sprite IronSprite { get; private set; }
    [field: SerializeField]
    public Sprite WheatArtefactSprite { get; private set; }
    [field: SerializeField]
    public Sprite WoodArtefactSprite { get; private set; }
    [field: SerializeField]
    public Sprite IronArtefactSprite { get; private set; }
    [field: SerializeField]
    public Sprite GeneralArtefactSprite { get; private set; }
    [field: SerializeField]
    public Sprite GreenEffectivityArtefactSprite { get; private set; }
    #endregion
    
    #region Methods
    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this.gameObject);
    }
    #endregion
}
