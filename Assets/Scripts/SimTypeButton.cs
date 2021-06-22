using UnityEngine;
using UnityEngine.EventSystems;

public class SimTypeButton : MonoBehaviour, IPointerClickHandler
{
    public SimType simType;
    public void OnPointerClick(PointerEventData eventData)
    {
        Settings.instance.simType = simType;
    }
}
