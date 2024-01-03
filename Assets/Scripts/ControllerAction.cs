using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerAction : MonoBehaviour
{
    public PlayerController.PlayerActions toggleAction;
    [SerializeField] ControllerUI controllerUI;

    Toggle thisToggle;

    public void InitToggle()
    {
        thisToggle = GetComponent<Toggle>();
        /*if (thisToggle == null)
        {
            Debug.Log(gameObject.name + " not set");
        }
        else
        {
            Debug.Log(gameObject.name + " set");
        }*/
    }

    public void OnActionToggled()
    {
        if (!thisToggle.isOn)
            return;

        controllerUI.SetAction(toggleAction);
    }
}