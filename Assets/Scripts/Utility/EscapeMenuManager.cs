using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeMenuManager : MonoBehaviour
{
    bool menuShown = false;

    [SerializeField] GameObject escapeMenuObj;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowEscapeMenu(menuShown = !menuShown);
        }
    }

    public void ShowEscapeMenu(bool showMenu)
    {
        escapeMenuObj.SetActive(showMenu);
    }
}
