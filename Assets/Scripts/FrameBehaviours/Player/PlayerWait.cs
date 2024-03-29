using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWait : PlayerFrameBehaviour
{
    [SerializeField] string waitAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = waitAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 11: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}

