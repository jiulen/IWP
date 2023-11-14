using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWait : FrameBehaviour
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
                lastFrame = true;
                break;
        }

        AnimatorSetFrame();
    }
}

