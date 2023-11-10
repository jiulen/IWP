using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerUI : MonoBehaviour
{
    public PlayerController.PlayerActions selectedAction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAction(PlayerController.PlayerActions toggleAction)
    {
        selectedAction = toggleAction;
    }
}
