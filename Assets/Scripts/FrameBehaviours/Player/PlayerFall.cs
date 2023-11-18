using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFall : PlayerFrameBehaviour
{
    [SerializeField] string fallAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = fallAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 5: //end
                EndAnimation();
                break;
        }

        if (playerController.isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);

            EndAnimation();
        }

        AnimatorSetFrame();
    }
}
