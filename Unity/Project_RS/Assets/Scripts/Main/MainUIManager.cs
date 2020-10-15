using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    public GameObject[] UI;

    public void ViewPanel(int type)
    {
        UI[type].SetActive(true);
    }
    public void ResetAndViewPanel(int type)
    {
        for(int i = 0; i < UI.Length; i++)
        {
            UI[i].SetActive(false);
        }
        UI[type].SetActive(true);
    }
}
