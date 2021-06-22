using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadIn : MonoBehaviour
{

    [SerializeField]
    private GameObject averageIn;
    [SerializeField]
    private GameObject averageDev;
    [SerializeField]
    private GameObject yearsIn;
    [SerializeField]
    private GameObject disposeCap;


    public Text errorMessage;
    public GameObject loadPlaceholder;
    public void Next()
    {
        bool operationError = false;
        HideErrorMessage();

        Settings.instance.averageLoad = Settings.GetIntInput(averageIn, 1, 2000, ref operationError);
        Settings.instance.avgDeviation = Settings.GetIntInput(averageDev, 0, 2000, ref operationError);
        Settings.instance.yearsSim = Settings.GetIntInput(yearsIn, 1, 2000, ref operationError);
        Settings.instance.boxDisposeCap = Settings.GetIntInput(disposeCap, 0, 2000, ref operationError);

        if (Settings.instance.averageLoad - Settings.instance.avgDeviation <= 0 || operationError)
        {
            ShowErrorMessage();
        }
        else
        if (!operationError)
        {
            gameObject.SetActive(false);
            loadPlaceholder.SetActive(true);
            SceneManager.LoadScene("SampleScene");
        }


    }

    public void ShowErrorMessage()
    {
        errorMessage.gameObject.SetActive(true);
        errorMessage.text = "Values must be greater than 0 and lesser than 100";
    }
    public void HideErrorMessage()
    {
        errorMessage.gameObject.SetActive(false);
    }

}
