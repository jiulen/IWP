using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIcicle : PlayerFrameBehaviour
{
    [SerializeField] string attackAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 23: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
