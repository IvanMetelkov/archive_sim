using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphClass : MonoBehaviour
{
    public RectTransform graphContainer;
    public int graphID;
    private void Start()
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-700, 0);
    }
}
