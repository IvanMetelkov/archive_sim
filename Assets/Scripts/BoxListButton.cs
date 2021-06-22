using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxListButton : MonoBehaviour
{
    public int buttonID;
    [SerializeField]
    private Sprite[] boxSprites;
    private void Awake()
    {
        PlanManager.instance.boxListContent.Add(this);
        gameObject.GetComponent<Image>().sprite = boxSprites[Random.Range(0, boxSprites.Length)];
        buttonID = PlanManager.instance.boxListContent.Count - 1;
    }

    public void ShowStats()
    {
        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        GameObject boxMenu = canvas.gameObject.transform.Find("BoxMenu").gameObject;

        if (PlanManager.instance.inspectedBoxes[buttonID].isBlank)
        {
            boxMenu.transform.Find("PosEmpty").gameObject.SetActive(true);
            HideUI(boxMenu);
        }
        else
        {
            boxMenu.transform.Find("PosEmpty").gameObject.SetActive(false);
            ShowUI(boxMenu);
            boxMenu.transform.Find("BoxID").gameObject.GetComponent<Text>().text = "Box ID: " + PlanManager.instance.inspectedBoxes[buttonID].boxID;
            boxMenu.transform.Find("Date").gameObject.GetComponent<Text>().text = "Arrival date: " + PlanManager.instance.inspectedBoxes[buttonID].arrivalDay;
            boxMenu.transform.Find("ShelfLife").gameObject.GetComponent<Text>().text = "ShelfLife: " + PlanManager.instance.inspectedBoxes[buttonID].shelfLifeType;
            boxMenu.transform.Find("Demand").gameObject.GetComponent<Text>().text = "Demand: " + PlanManager.instance.inspectedBoxes[buttonID].demandChanceType;
            boxMenu.transform.Find("LifeValue").gameObject.GetComponent<Text>().text = PlanManager.instance.inspectedBoxes[buttonID].lifeSpan.ToString();
            boxMenu.transform.Find("DemandValue").gameObject.GetComponent<Text>().text = PlanManager.instance.inspectedBoxes[buttonID].demandChance.ToString();
            Debug.Log(PlanManager.instance.inspectedBoxes[buttonID].CountBoxesToTake());
        }
    }

    void HideUI(GameObject menu)
    {
        menu.transform.Find("BoxID").gameObject.SetActive(false);
        menu.transform.Find("Date").gameObject.SetActive(false);
        menu.transform.Find("ShelfLife").gameObject.SetActive(false);
        menu.transform.Find("Demand").gameObject.SetActive(false);
        menu.transform.Find("LifeValue").gameObject.SetActive(false);
        menu.transform.Find("DemandValue").gameObject.SetActive(false);
    }

    void ShowUI(GameObject menu)
    {
        menu.transform.Find("BoxID").gameObject.SetActive(true);
        menu.transform.Find("Date").gameObject.SetActive(true);
        menu.transform.Find("ShelfLife").gameObject.SetActive(true);
        menu.transform.Find("Demand").gameObject.SetActive(true);
        menu.transform.Find("LifeValue").gameObject.SetActive(true);
        menu.transform.Find("DemandValue").gameObject.SetActive(true);
    }
}
