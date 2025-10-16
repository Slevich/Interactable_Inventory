using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtefactCostUI : MonoBehaviour
{
    #region Properties
    [field: Header("References.")]
    [field: SerializeField]
    public Image ResourceImage {get; private set;}
    [field: SerializeField]
    public TextMeshProUGUI CostText {get; private set;}
    #endregion
}
