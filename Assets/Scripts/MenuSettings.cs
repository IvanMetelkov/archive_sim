using UnityEngine;
using UnityEngine.UI;

public class MenuSettings : MonoBehaviour
{

    [SerializeField]
    private GameObject rowsInput;
    [SerializeField]
    private GameObject columnsInput;
    [SerializeField]
    private GameObject lengthInput;
    [SerializeField]
    private GameObject floorCountInput;
    [SerializeField]
    private GameObject officeWidthInput;
    [SerializeField]
    private Text errorMessage;
    [SerializeField]
    private GameObject taskMenu;
    public void Next()
    {
        bool operationError = false;

        HideErrorMessage();

        Settings.instance.rowCount = Settings.GetIntInput(rowsInput, 1, 100, ref operationError);
        Settings.instance.columnCount = Settings.GetIntInput(columnsInput, 1, 100, ref operationError);
        Settings.instance.shelfLength = Settings.GetIntInput(lengthInput, 1, 100, ref operationError);
        Settings.instance.floorCount = Settings.GetIntInput(floorCountInput, 1, 100, ref operationError);
        Settings.instance.officeWidth = Settings.GetIntInput(officeWidthInput, 1, 100, ref operationError);

        if (Settings.instance.wingCount == 0 || operationError)
        {
            ShowErrorMessage();
        }
        else
        if (!operationError)
        {
            taskMenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void ShowErrorMessage()
    {
        errorMessage.gameObject.SetActive(true);

        if (Settings.instance.wingCount == 0)
        {
            errorMessage.text = "You have to choose the number of building's wings";
        }
        else
        {
            errorMessage.text = "Values must be greater than 0 and lesser than 100";
        }
    }
    public void HideErrorMessage()
    {
        errorMessage.gameObject.SetActive(false);
    }

}
