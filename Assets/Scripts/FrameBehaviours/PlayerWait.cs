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
                AnimatorChangeAnimation(waitAnim);
                break;
            case 12: //end
                lastFrame = true;
                break;
        }

        AnimatorSetFrame();
    }
}

