using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxMenu : MonoBehaviour
{
    [SerializeField] private GameObject topScrollView;
    [SerializeField] private GameObject topScrollViewPort;
    [SerializeField] private GameObject topListContent;

    [SerializeField] private GameObject midScrollView;
    [SerializeField] private GameObject midScrollViewPort;
    [SerializeField] private GameObject midListContent;

    [SerializeField] private GameObject bottomScrollView;
    [SerializeField] private GameObject bottomScrollViewPort;
    [SerializeField] private GameObject bottomListContent;

    [SerializeField]
    private GameObject emptyBox;
    [SerializeField]
    private GameObject boxListButton;

    public Text posID;
    public Text boxCount;
    public Text floorWing;
    public Text accessPoint;
    public Text standEmpty;
    public Text posEmpty;
    [SerializeField]
    private Text boxID;
    [SerializeField]
    private Text shelfLife;
    [SerializeField]
    private Text lifeValue;
    [SerializeField]
    private Text date;
    [SerializeField]
    private Text demand;
    [SerializeField]
    private Text demandValue;
    public void ClearLists()
    {
        foreach (Transform child in topListContent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in midListContent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in bottomListContent.transform)
        {
            Destroy(child.gameObject);
        }

        PlanManager.instance.boxListContent.Clear();
    }

    public void SetUIText(Storage storage)
    {
        posID.text = "Pos. ID: " + storage.storageID;
        boxCount.text = "Box count: " + storage.boxCount;
        floorWing.text = "Floor & wing: " + storage.rootFloor.floorNumber + "-" + storage.wing;
        accessPoint.text = "Access: " + storage.GetLocalPosition();
    }

    public void PopulateLists()
    {
        for (int i = 0; i < 9; ++i)
        {
            GameObject scrollItem;
            if (!PlanManager.instance.inspectedBoxes[i].isBlank)
            {
               scrollItem = Instantiate(boxListButton);
               scrollItem.transform.SetParent(topListContent.transform, false);

            }
            else
            {
                scrollItem = Instantiate(emptyBox);
                scrollItem.transform.SetParent(topListContent.transform, false);

            }
        }

        for (int i = 9; i < 18; ++i)
        {
            GameObject scrollItem;
            if (!PlanManager.instance.inspectedBoxes[i].isBlank)
            {
                scrollItem = Instantiate(boxListButton);
                scrollItem.transform.SetParent(midListContent.transform, false);
            }
            else
            {
                scrollItem = Instantiate(emptyBox) as GameObject;
                scrollItem.transform.SetParent(midListContent.transform, false);
            }
        }

        for (int i = 18; i < 27; ++i)
        {
            GameObject scrollItem;
            if (!PlanManager.instance.inspectedBoxes[i].isBlank)
            {
                scrollItem = Instantiate(boxListButton);
                scrollItem.transform.SetParent(bottomListContent.transform, false);

            }
            else
            {
                scrollItem = Instantiate(emptyBox);
                scrollItem.transform.SetParent(bottomListContent.transform, false);
            }
        }
    }

    public void HideUI()
    {
        boxID.gameObject.SetActive(false);
        shelfLife.gameObject.SetActive(false);
        lifeValue.gameObject.SetActive(false);
        date.gameObject.SetActive(false);
        demand.gameObject.SetActive(false);
        demandValue.gameObject.SetActive(false);
        standEmpty.gameObject.SetActive(false);
        posEmpty.gameObject.SetActive(false);
    }
}
