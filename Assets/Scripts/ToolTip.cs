using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string text;
    public Vector2 pos;
    public GameObject toolTip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlanManager.ShowToolTip(toolTip, text, pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlanManager.HideToolTip(toolTip);
    }
}
