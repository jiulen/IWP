using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : PlayerFrameBehaviour
{
    [SerializeField] float jumpForce;
    [SerializeField] string jumpAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = jumpAnim;
                AnimatorChangeAnimation(currentAnimName);

                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                break;
            case 35: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}

