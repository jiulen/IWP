using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerAction : MonoBehaviour
{
    [SerializeField] PlayerController.PlayerActions toggleAction;
    [SerializeField] ControllerUI controllerUI;

    Toggle thisToggle;

    private void Awake()
    {
        thisToggle = GetComponent<Toggle>();
    }

    public void OnActionToggled()
    {
        if (!thisToggle.isOn)
            return;

        controllerUI.SetAction(toggleAction);
    }
}