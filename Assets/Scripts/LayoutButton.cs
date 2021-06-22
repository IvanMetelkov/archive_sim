using UnityEngine;
using UnityEngine.EventSystems;

public class LayoutButton : MonoBehaviour, IPointerClickHandler
{
    public RoomType roomType;

    public void OnPointerClick(PointerEventData eventData)
    {
        Settings.instance.roomType = roomType;
    }
}
