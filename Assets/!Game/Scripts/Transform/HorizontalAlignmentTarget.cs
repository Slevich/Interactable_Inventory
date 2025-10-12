using System;
using UnityEngine;

public class HorizontalAlignmentTarget : MonoBehaviour
{
    #region Properties
    [field: Header("Settings.")]
    [field: SerializeField] 
    public bool IsPreview { get; set; } = false;
    #endregion
}