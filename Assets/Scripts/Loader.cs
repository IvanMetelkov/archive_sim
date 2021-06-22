using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject planManager;
    void Awake()
    {
        if (PlanManager.instance == null)
        {
            Instantiate(planManager);
        }
    }
}
