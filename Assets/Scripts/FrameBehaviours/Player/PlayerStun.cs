using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStun : PlayerFrameBehaviour
{
    [SerializeField] string stunAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = stunAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
