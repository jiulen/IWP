using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStun : PlayerFrameBehaviour
{
    public int stunDuration;

    [SerializeField] string stunAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = stunAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
        }

        if (frameNum >= stunDuration)
        {
            EndAnimation();
        }

        AnimatorSetFrame();
    }
}
