using UnityEngine;
using UnityEngine.UI;

public class RandSettings : MonoBehaviour
{
    [SerializeField]
    private GameObject agentCount;
    [SerializeField]
    private GameObject timeSim;
    [SerializeField]
    private GameObject taskIntensity;
    [SerializeField]
    private GameObject inventoryCap;
    public Text errorMessage;

    public GameObject boxMenu;

    public void Next()
    {
        bool operationError = false;
        HideErrorMessage();

        Settings.instance.agentCount = Settings.GetIntInput(agentCount, 1, 100, ref operationError);
        Settings.instance.timeSim = Settings.GetFloatInput(timeSim, 0, 100, ref operationError);
        Settings.instance.taskIntencity = Settings.GetFloatInput(taskIntensity, 0, 60000, ref operationError);
        Settings.instance.inventoryCap = Settings.GetIntInput(inventoryCap, 1, 100, ref operationError);

        if (!operationError || Settings.instance.simType == SimType.NotSet)
        {
            gameObject.SetActive(false);
            boxMenu.SetActive(true);
        }
        else
        {
            ShowErrorMessage();
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
