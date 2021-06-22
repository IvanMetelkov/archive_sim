using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BoxSettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject shelfLifeObj;
    [SerializeField] private GameObject chanceObj;
    [SerializeField] private Text errorMessage;
    [SerializeField] private GameObject lifeMenu;

    public void Next()
    {
        Settings.instance.demandChance.Clear();
        Settings.instance.shelfLife.Clear();

        bool operationErr = false;
        if (!CheckLifeInput() && !CheckDemandInput())
        {
            foreach (string s in shelfLifeObj.GetComponent<InputField>().text.Split(';'))
            {
                Settings.instance.shelfLife.Add(float.Parse(s));
            }

            foreach (string s in chanceObj.GetComponent<InputField>().text.Split(';'))
            {
                Settings.instance.demandChance.Add(float.Parse(s));
            }
        }
        else
        {
            ShowErrorMessage("Wrong numeric imput!");
            operationErr = true;
        }

        if (Settings.instance.boxDisposal == BoxDisposal.NotSet || Settings.instance.boxPlacement == BoxPlacement.NotSet)
        {
            operationErr = true;
            ShowErrorMessage("No set box treatment options!");
        }

        if (!operationErr)
        {
            lifeMenu.SetActive(true);
            gameObject.SetActive(false);
            HideErrMessage();
        }
    }

    public bool CheckLifeInput()
    {
        bool err = false;
        if (shelfLifeObj.GetComponent<InputField>().text.Split(';').Length != 0 && shelfLifeObj.GetComponent<InputField>().text.Length != 0)
        {
            foreach (string s in shelfLifeObj.GetComponent<InputField>().text.Split(';'))
            {
                if (float.TryParse(s, out float ans) && (s != string.Empty))
                {
                    if (ans < 0)
                    {
                        err = true;

                    }
                }
                else
                {
                    err = true;
                }
            }
        }
        else
        {
            err = true;
        }


        return err;

    }

    public bool CheckDemandInput()
    {
        bool err = false;
        List<float> demandList = new List<float>();
        if (chanceObj.GetComponent<InputField>().text.Split(';').Length != 0 && chanceObj.GetComponent<InputField>().text.Length != 0)
        {
            foreach (string s in chanceObj.GetComponent<InputField>().text.Split(';'))
            {
                if (float.TryParse(s, out float ans) && (s != string.Empty))
                {
                    if (ans <= 0 || ans >= 100)
                    {
                        err = true;
                    }
                    else
                    {
                        demandList.Add(ans);
                    }
                }
                else
                {
                    err = true;
                }
            }
        }
        else
        {
            err = true;
        }
        float check = 0.0f;
        foreach (float tmp in demandList)
        {
            check += tmp;
        }

        if (check != 100.0f)
        {
            err = true;
        }

        return err;

    }

    public void ShowErrorMessage(string s)
    {
        errorMessage.gameObject.SetActive(true);
        errorMessage.text = s;
    }

    public void HideErrMessage()
    {
        errorMessage.gameObject.SetActive(false);
    }
}
