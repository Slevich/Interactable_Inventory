using System;
using UnityEngine;
using UnityEngine.UI;

public class CurrentArtefactsImagesManager : MonoBehaviour
{
    #region Fields
    [Header("References.")]
    [SerializeField]
    private ArtefactsCollector artefactsCollector;
    [SerializeField]
    private Image _currentArtefactImagePrefab;
    [SerializeField]
    private Transform _currentArtefactImagesParent;
    #endregion

    #region Methods
    private void OnEnable()
    {
        if (artefactsCollector != null)
            artefactsCollector.OnArtefactAddedEvent.AddListener(AddNewCurrentArtefactImage);
    }

    private void OnDisable()
    {
        if (artefactsCollector != null)
            artefactsCollector.OnArtefactAddedEvent.RemoveListener(AddNewCurrentArtefactImage);
    }

    private void AddNewCurrentArtefactImage(ArtefactType newArtefact)
    {
        if(_currentArtefactImagePrefab == null)
            return;
        
        Image currentArtefactImage = Instantiate(_currentArtefactImagePrefab, _currentArtefactImagesParent != null ? _currentArtefactImagesParent : transform);
        
        Sprite currentArtefactSprite = null;

        switch (newArtefact)
        {
            case ArtefactType.Wheat:
                currentArtefactSprite = GlobalSpritesContainer.Instance.WheatArtefactSprite;
                break;
            
            case ArtefactType.Wood:
                currentArtefactSprite = GlobalSpritesContainer.Instance.WoodArtefactSprite;
                break;
            
            case ArtefactType.Iron:
                currentArtefactSprite = GlobalSpritesContainer.Instance.IronArtefactSprite;
                break;
            
            case ArtefactType.General:
                currentArtefactSprite = GlobalSpritesContainer.Instance.GeneralArtefactSprite;
                break;
            
            case ArtefactType.GreenEffectivity:
                currentArtefactSprite = GlobalSpritesContainer.Instance.GreenEffectivityArtefactSprite;
                break;
        }
        
        currentArtefactImage.sprite = currentArtefactSprite;
    }
    #endregion
}
