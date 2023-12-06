using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : PlayerFrameBehaviour
{
    public bool isWalking, isRolling = false;
    public Vector2 forwardDir = Vector2.zero;

    [SerializeField] float jumpForce, walkJumpForce, rollJumpForce;
    [SerializeField] string jumpAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                rb.velocity = Vector2.zero;

                currentAnimName = jumpAnim;
                AnimatorChangeAnimation(currentAnimName);

                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

                if (isWalking)
                {
                    rb.AddForce(forwardDir * walkJumpForce, ForceMode2D.Impulse);
                }
                else if (isRolling)
                {
                    rb.AddForce(forwardDir * rollJumpForce, ForceMode2D.Impulse);
                }

                break;
            case 35: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}

