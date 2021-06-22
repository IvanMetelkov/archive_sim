using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChanceListContent : MonoBehaviour
{
    public GameObject chanceObj;
    public Text idField;
    public Text chanceField;
    public GameObject cancelButton;
    public GameObject editButton;
    public GameObject applyButton;
    public Text entryValue;

    private List<ChanceListContent> chances = new List<ChanceListContent>();
    private List<float> values = new List<float>();
    private void Awake()
    {
        if (Settings.instance.lifeSpan.Count == 0)
        {
            values = Settings.instance.shelfLife;
            chances = Settings.instance.lifeChances;
        }
        else
        {
            values = Settings.instance.demandChance;
            chances = Settings.instance.demandChances;
        }

        chances.Add(this);
        idField.text = chances.Count.ToString();
        entryValue.text = values[chances.Count - 1].ToString();
    }
    public void ApplyChanges()
    {
        if (!CheckChanceInput())
        {
            chanceField.text = chanceObj.GetComponent<InputField>().text;
            idField.gameObject.SetActive(true);
            cancelButton.SetActive(false);
            applyButton.SetActive(false);
            editButton.SetActive(true);
            chanceObj.SetActive(false);
            chanceField.gameObject.SetActive(true);
        }
    }

    private bool CheckChanceInput()
    {
        bool err = false;
        if (float.TryParse(chanceObj.GetComponent<InputField>().text, out float ans) 
            && (chanceObj.GetComponent<InputField>().text != string.Empty))
        {
            if (ans <= 0 || ans >= 100)
            {
                err = true;
            }
        }
        else
        {
            err = true;
        }

        return err;
    }
}
