using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DailyRewardItem : MonoBehaviour
{
    public GameObject Default;
    public GameObject Today;
    public GameObject Box;
    public GameObject AlreadyGet;
    public Transform IconParent;
    public TMP_Text dayText;

    public void Initialize(int day)
    {
        dayText.SetText($"Day {day}");
    }
}
