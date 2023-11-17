using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoll : PlayerFrameBehaviour
{
    public bool goLeft = false;

    [SerializeField] float rollForce;
    [SerializeField] string rollAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                rb.velocity = Vector2.zero;

                currentAnimName = rollAnim;
                AnimatorChangeAnimation(currentAnimName);

                Vector2 rollDir = Vector2.right * (goLeft ? -1 : 1);
                rb.AddForce(rollDir * rollForce, ForceMode2D.Impulse);
                break;
            case 23: //end
                EndAnimation();

                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }

        AnimatorSetFrame();
    }
}

