using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChanceMenu : MonoBehaviour
{
    public ScrollRect scrollView;
    public GameObject scrollContent;
    public GameObject scrollItem;
    public GameObject nextStepMenu;
    public Text errorMessage;

    private List<float> valueList = new List<float>();
    private List<ChanceListContent> chancesList = new List<ChanceListContent>();
    private List<ValueAndWeight> weightsList = new List<ValueAndWeight>();

    private void OnEnable()
    {
        if (!Settings.instance.stepOneFinished)
        {
            valueList = Settings.instance.shelfLife;
            chancesList = Settings.instance.lifeChances;
            weightsList = Settings.instance.lifeSpan;
        }
        else
        {
            valueList = Settings.instance.demandChance;
            chancesList = Settings.instance.demandChances;
            weightsList = Settings.instance.chances;
        }
        HideErrorMessage();
        ClearLists();
        for (int i = 0; i < valueList.Count; i++)
        {
            GameObject scrollItem = Instantiate(this.scrollItem);
            scrollItem.transform.SetParent(scrollContent.transform, false);
        }
    }

    public void ClearLists()
    {
        weightsList.Clear();
        HideErrorMessage();
        chancesList.Clear();
        foreach (Transform child in scrollContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ClearPrevStep()
    {
        Settings.instance.stepOneFinished = false;
    }
    public void NextStep()
    {
        float checkChance = 0.0f;
        foreach (Transform child in scrollContent.transform)
        {
            float.TryParse(child.Find("EntryChance").gameObject.GetComponent<Text>().text, out float ans);
            checkChance += ans;
        }

        if (checkChance != 100)
        {
            ShowErrorMessage("Chances should add up to 100%!");
        }
        else
        {
            weightsList.Clear();
            foreach (Transform child in scrollContent.transform)
            {
                float.TryParse(child.Find("EntryChance").gameObject.GetComponent<Text>().text, out float chance);
                float.TryParse(child.Find("EntryValue").gameObject.GetComponent<Text>().text, out float value);

                ValueAndWeight tmp = new ValueAndWeight(value, chance);
                weightsList.Add(tmp);
            }
            HideErrorMessage();
            Settings.instance.stepOneFinished = true;
            nextStepMenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void HideErrorMessage()
    {
        errorMessage.gameObject.SetActive(false);
    }

    public void ShowErrorMessage(string s)
    {
        errorMessage.gameObject.SetActive(true);
        errorMessage.text = s;
    }
}
