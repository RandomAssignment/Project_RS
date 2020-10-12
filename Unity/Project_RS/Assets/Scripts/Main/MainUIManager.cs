using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] UI_List;


    public void ViewPanel(int type)
    {
        UI_List[type].SetActive(true);
    }
    public void ResetAndViewPanel(int type)
    {
        for(int i = 0; i < UI_List.Length; i++)
        {
            UI_List[i].SetActive(false);
        }
        UI_List[type].SetActive(true);
    }
}
