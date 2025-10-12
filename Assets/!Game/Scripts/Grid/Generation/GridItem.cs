using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GridItem : MonoBehaviour /*, IDependenciesInjection<UnitDependenciesContainer>*/
{
    #region Properties
    [field: Header("Settings.")]
    [field: SerializeField]
    public Vector2 CellCenterOffset { get; private set; } = Vector2.zero;
    [field: SerializeField] 
    public Vector2 CellScaleModifier { get; private set; } = Vector2.one;

    public SpriteRenderer Renderer { get; private set; }
    public GridCell ParentCell { get; set; }

    public Vector3 RandomPoint { get; set; } = Vector3.zero;
    public bool IsActive { get; set; } = false;
    public bool Initialized { get; set; } = false;
    #endregion

    #region Fields

    [Header("Events.")] 
    [SerializeField] 
    private UnityEvent OnUnitInitializedEvent = new UnityEvent();
    [SerializeField]
    private UnityEvent OnUnitDestroyedEvent = new UnityEvent();

    private ActionTimer _actionTimer;
    #endregion

    #region Methods
    // public void Inject(UnitDependenciesContainer Container)
    // {
    //     if(Container == null)
    //         return;
    //
    //     Renderer = Container.SpriteRenderer;
    //     Initialize();
    // }

    public void Initialize()
    {
        OnUnitInitializedEvent?.Invoke();
        _actionTimer = new ActionTimer();
        Initialized = true;
    }
    
    public void DestroyUnit(float Delay = 0f)
    {
        OnUnitDestroyedEvent?.Invoke();

        if (Delay > 0f)
        {
            if (_actionTimer != null)
            {
                _actionTimer.StartTimerAndAction(Delay, DestroyObject);
            }
        }
        else
        {
            DestroyObject();
        }
    }

    private void OnDisable() => _actionTimer.StopTimer();
    private void DestroyObject() => Destroy(gameObject);
    #endregion
}
