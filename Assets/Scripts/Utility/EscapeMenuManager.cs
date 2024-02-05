using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeMenuManager : MonoBehaviour
{
    bool menuShown = false;

    [SerializeField] GameObject escapeMenuObj;

    [SerializeField] bool inGame = true;

    private void Awake()
    {
        ShowEscapeMenu(menuShown = false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowEscapeMenu(menuShown = !menuShown);
        }
    }

    public void ShowEscapeMenu(bool showMenu)
    {
        menuShown = showMenu;
        escapeMenuObj.SetActive(menuShown);
    }

    public void Quit()
    {
        if (inGame)
            ShooterGameManager.Instance.QuitGame();
        else
            Application.Quit();
    }
}
