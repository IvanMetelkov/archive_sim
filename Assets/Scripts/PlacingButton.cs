using UnityEngine;
using UnityEngine.EventSystems;

public class PlacingButton : MonoBehaviour, IPointerClickHandler
{
    public BoxPlacement boxPlacement;

    public void OnPointerClick(PointerEventData eventData)
    {
        Settings.instance.boxPlacement = boxPlacement;
    }
}
