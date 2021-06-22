using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public static Settings instance = null;
    public SimType simType;
    public RoomType roomType;
    public BoxPlacement boxPlacement;
    public int officeWidth;
    public int wingCount;
    public int agentCount;
    public float taskIntencity;
    public int rowCount;
    public int columnCount;
    public int shelfLength;
    public int floorCount;
    public List<float> shelfLife = new List<float>();
    public List<float> demandChance = new List<float>();
    public BoxDisposal boxDisposal;
    public int averageLoad;
    public int avgDeviation;
    public int yearsSim;
    public List<ChanceListContent> lifeChances = new List<ChanceListContent>();
    public List<ChanceListContent> demandChances = new List<ChanceListContent>();
    public List<ValueAndWeight> lifeSpan = new List<ValueAndWeight>();
    public List<ValueAndWeight> chances = new List<ValueAndWeight>();
    public bool stepOneFinished = false;
    public int boxDisposeCap;
    public float timeSim;
    public int inventoryCap;

    public float minProcessingDelay = 1.0f;
    public float maxProcessingDelay = 2.0f;

    public float minTimeToMoveFloors = 10.0f;
    public float maxTimeToMoveFloors = 15.0f;

    public float minTimeToLeave = 3.0f;
    public float maxTimeToLeave = 5.0f;

    public float minTimeToCross = 5.0f;
    public float maxTimeToCross = 10.0f;

    public float minTimeToPlace = 2.0f;
    public float maxTimeToPlace = 4.0f;

    public float minTimeToTake = 3.0f;
    public float maxTimeToTake = 5.0f;

    public float minTimeToUnpack = 10.0f;
    public float maxTimeToUnpack = 15.0f;

    public float minTimeToScan = 30.0f;
    public float maxTimeToScan = 60.0f;


    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);

        DontDestroyOnLoad(gameObject);
    }

    public static int GetIntInput(GameObject inputField, int minInclusive, int maxInclusive, ref bool err)
    {
        Text placeHolder = inputField.transform.Find("Placeholder").GetComponent<Text>();
        int ans;
        if (inputField.GetComponent<InputField>().text == string.Empty)
        {
            if (int.TryParse(placeHolder.text, out ans))
            {
                if (ans < minInclusive || ans > maxInclusive)
                {
                    err = true;
                }
            }
        }
        else
        if (int.TryParse(inputField.GetComponent<InputField>().text, out ans))
        {
            if (ans < minInclusive || ans > maxInclusive)
            {
                err = true;
            }
        }
        else
        {
            err = true;
        }

        return ans;
    }

    public static float GetFloatInput(GameObject inputField, float minExclusive, float maxInclusive, ref bool err)
    {
        Text placeHolder = inputField.transform.Find("Placeholder").GetComponent<Text>();
        float ans;
        if (inputField.GetComponent<InputField>().text == string.Empty)
        {
            if (float.TryParse(placeHolder.text, out ans))
            {
                if (ans <= minExclusive || ans > maxInclusive)
                {
                    err = true;
                }
            }
        }
        else
        if (float.TryParse(inputField.GetComponent<InputField>().text, out ans))
        {
            if (ans <= minExclusive || ans > maxInclusive)
            {
                err = true;
            }
        }
        else
        {
            err = true;
        }

        return ans;
    }
}

public enum SimType
{
    NotSet,
    Closest,
    FirstIdle,
}

public enum BoxDisposal
{
    NotSet,
    DoDispose,
    DoNotDispose
}

public enum BoxPlacement
{
    NotSet,
    Simple,
    ByDemand,
    Corners
}

public enum MenuType
{
    Default,
    ShelfLife,
    Chances
}

public class ValueAndWeight
{
    public float value;
    public float weight;
    public ShelfLife shelfLifeType;
    public DemandChance demandType;

    public ValueAndWeight(float value, float weight)
    {
        this.value = value;
        this.weight = weight;
    }

    public void PrintPair()
    {

        Debug.Log(value + " " + weight + " " + shelfLifeType + " " + demandType);
    }
}
