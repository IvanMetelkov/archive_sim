using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusComponent : MonoBehaviour
{
    public void SetFollowPosition()
    {
        PlanManager.instance.SetFollowtarget(PlanManager.instance.robots[int.Parse(transform.Find("IDText").gameObject.GetComponent<Text>().text)].gameObject);
    }
}
