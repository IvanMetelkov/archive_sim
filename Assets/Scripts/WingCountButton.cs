using UnityEngine;
using UnityEngine.EventSystems;

public class WingCountButton : MonoBehaviour, IPointerClickHandler
{
    public int wingCount;

    public void OnPointerClick(PointerEventData eventData)
    {
        Settings.instance.wingCount = wingCount;
    }
}
