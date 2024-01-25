using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleIconSwitcher : MonoBehaviour
{
    Toggle toggle;

    [SerializeField] GameObject onIcon, offIcon;


    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    public void OnToggled()
    {
        onIcon.SetActive(toggle.isOn);
        offIcon.SetActive(!toggle.isOn);
    }
}
