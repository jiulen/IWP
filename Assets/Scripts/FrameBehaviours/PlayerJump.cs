using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : PlayerFrameBehaviour
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
            case 5: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}

