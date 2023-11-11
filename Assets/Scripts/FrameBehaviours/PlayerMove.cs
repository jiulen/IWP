using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : FrameBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                break;
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                break;
        }

        AnimatorSetFrame();
    }
}
