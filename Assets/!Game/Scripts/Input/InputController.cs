using System;
using UnityEngine;

public class InputController : MonoBehaviour
{
    #region Methods
    private void OnEnable() => InputHandler.Enable();
    private void OnDisable() => InputHandler.Disable();
    #endregion
}
