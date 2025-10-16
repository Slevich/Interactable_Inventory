using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArtefactsCollector : MonoBehaviour
{
    #region Fields
    [Header("References.")]
    [SerializeField]
    private ResourceProduction _resourceProduction;
    
    [Header("Container.")]
    [SerializeField, ReadOnly]
    private List<ArtefactType> _currentArtefacts = new List<ArtefactType>();
    
    private Queue<ArtefactType> _randomArtefactQueue = new Queue<ArtefactType>();
    private ArtefactsEffector _effector;
    #endregion

    #region Properties
    public UnityEvent<ArtefactType> OnArtefactAddedEvent { get; set; } = new UnityEvent<ArtefactType>();
    public UnityEvent OnNewArtefactEvent { get; set; } = new UnityEvent();
    public UnityEvent<bool> HasNextArtefactEvent { get; set; } = new UnityEvent<bool>();
    public List<ArtefactType> CurrentArtefacts => _currentArtefacts;
    #endregion
    
    #region Methods
    
    #region Unity methods
    private void Awake()
    {
        GenerateArtefactsQueue();
        _effector = new ArtefactsEffector(_resourceProduction, this);
    }

    private void OnEnable()
    {
        _effector.Subscribe();
    }

    private void OnDisable()
    {
        _effector.Dispose();
    }
    #endregion
    
    private void GenerateArtefactsQueue()
    {
        _randomArtefactQueue.Clear();
        Array artefactTypes = Enum.GetValues(typeof(ArtefactType));
        
        List<ArtefactType> shuffledList = new List<ArtefactType>(artefactTypes.Length);
        
        foreach (ArtefactType type in artefactTypes)
            shuffledList.Add(type);
        
        for (int i = shuffledList.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (shuffledList[i], shuffledList[randomIndex]) = (shuffledList[randomIndex], shuffledList[i]);
        }
        
        foreach (ArtefactType type in shuffledList)
            _randomArtefactQueue.Enqueue(type);
    }
    
    public void AddNewArtefact()
    {
        if(_randomArtefactQueue.Count == 0)
            return;
        
        ArtefactType newArtefact = _randomArtefactQueue.Dequeue();
        Debug.Log("New artefact: "  + newArtefact);
        OnArtefactAddedEvent?.Invoke(newArtefact);
        OnNewArtefactEvent?.Invoke();
        _effector.ApplyArtefactEffectsForAll(newArtefact);
        _currentArtefacts.Add(newArtefact);
        
        bool hasNextArtefact = _randomArtefactQueue.Count > 0;
        HasNextArtefactEvent?.Invoke(hasNextArtefact);
    }
    #endregion
}
