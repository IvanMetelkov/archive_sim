using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject settings;
    public GameObject structureMenu;
    public GameObject roomMenu;
    public Text structureErr;

    public void DeleteSettings()
    {
        if (Settings.instance != null)
        {
            structureErr.gameObject.SetActive(false);
            Destroy(Settings.instance.gameObject);
            Settings.instance = null;
        }
    }

    public void SimSettingsStart()
    {
        if (Settings.instance == null)
        {
            Instantiate(settings);
        }
    }

    public void StructureNext()
    {
        if (Settings.instance.roomType == RoomType.NotSet)
        {
            structureErr.gameObject.SetActive(true);
        }
        else
        {
            structureErr.gameObject.SetActive(false);
            structureMenu.SetActive(false);
            roomMenu.SetActive(true);
        }
    }

    public void SetDoDispose()
    {
        Settings.instance.boxDisposal = BoxDisposal.DoDispose;
    }

    public void SetDoNotDispose()
    {
        Settings.instance.boxDisposal = BoxDisposal.DoNotDispose;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
