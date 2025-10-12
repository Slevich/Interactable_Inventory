using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHandler
{
    #region Fields
    private static InputSystem_Actions _inputActions;
    private static ActionUpdate _update;
    #endregion
    
    #region Properties
    public static Subject<Vector2> PointerPositionUpdate { get; private set; }
    public static Subject<bool> DragIsInProgress { get; private set; }
    public static Subject<bool> DeselectIsInProgress{ get; private set; }
    #endregion

    #region Constructor
    static InputHandler() => Initialize();
    #endregion
    
    #region Methods
    private static void Initialize()
    {
        _inputActions = new InputSystem_Actions();
        PointerPositionUpdate = new Subject<Vector2>();
        DragIsInProgress = new Subject<bool>();
        DeselectIsInProgress = new Subject<bool>();
        _update = new ActionUpdate();
    }

    private static void UpdateData()
    {
        PointerPositionUpdate.OnNext(_inputActions.Player.Pointer.ReadValue<Vector2>());
        DragIsInProgress.OnNext(_inputActions.Player.Drag.IsInProgress());
    }
    
    public static void Enable()
    {
        _inputActions.Enable();
        _update.StartUpdate(UpdateData);
    }

    public static void Disable()
    {
        _inputActions.Disable();
        _update.StopUpdate();
    }
    #endregion
}
